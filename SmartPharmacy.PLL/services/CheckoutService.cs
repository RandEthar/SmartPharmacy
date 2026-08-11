using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public class CheckoutService : ICheckoutService
    {

        private readonly ICartService _cartService;
        private readonly IProductRepository _productRepository;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailSender _emailSender;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(

          IOrderRepository orderRepository,
        ICartService cartService,
            IProductRepository productRepository,
            UserManager<ApplicationUser> userManager,
        IFileService fileService,
        IHttpContextAccessor httpContextAccessor,
        IEmailSender emailSender,
        INotificationService notificationService,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        ILogger<CheckoutService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;

            _userManager = userManager;
            _cartService = cartService;
            _productRepository = productRepository;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
            _orderRepository = orderRepository;
            _emailSender = emailSender;
            _notificationService = notificationService;
            _configuration = configuration;
        }

        public async Task<CheckoutResponse> Checkout(string userId, CheckoutRequest request)
        {
           var cartItems =await _cartService.GetCart(userId);
            if (cartItems == null || cartItems.Items.Count == 0)
            {
               return new CheckoutResponse
               {
                   Success = false,
                   ErrorMessage = "Cart is empty."
               };
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = "User not found."
                };
            }
            var city = request.City ?? user.City;
            if (string.IsNullOrEmpty(city))
            {
                return new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = "City is required."
                };
            }
            var street = request.Street ?? user.Street;
            if (string.IsNullOrEmpty(street))
            {
                return new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = "Street is required."
                };
            }
            var phoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = "Phone number is required."
                };
            }
            bool requiresPrescription = false;
            foreach (var item in cartItems.Items)
            {
                var product = await _productRepository.GetOne(p=>p.Id== item.ProductId,
                    new string[] { nameof(Product.ProductTranslations) }
                    );

                if (product == null)
                {
                    return new CheckoutResponse
                    {
                        Success = false,
                        ErrorMessage = $"Product #{item.ProductId} is no longer available."
                    };
                }

                var productResponse = product.Adapt<ProductResponse>();
                // Early check purely so the customer gets the offending product name. The
                // binding check is the reservation below - this value can be stale by then.
                if (item.Quantity>product.StockQuantity)
                {
                    return new CheckoutResponse
                    {
                        Success = false,
                        ErrorMessage = $"Insufficient stock for product {productResponse.Name}."
                    };
                }
                if (product.NeedsPrescription)
                {
                    requiresPrescription = true;
                }
            }
            Order order = new Order
            {
                UserId = userId,
                City = city,
                Street = street,
                PhoneNumber = phoneNumber,
                OrderDate = DateTime.UtcNow,
              OrderStatus = requiresPrescription ? OrderStatusEnum.AwaitingPrescription : OrderStatusEnum.Pending,
                PaymentMethod = request.PaymentMethod,

                OrderItems = cartItems.Items.Select(item => new OrderItem
                {

                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                }).ToList()
            };

            // Stock is taken here, when the order is created, rather than after payment. That
            // closes the window where a customer could sit on the Stripe page (or wait days for
            // a prescription review) while the last box was sold to somebody else.
            await using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                if (!await _productRepository.TryReserveStock(order.OrderItems))
                {
                    await transaction.RollbackAsync();
                    return new CheckoutResponse
                    {
                        Success = false,
                        ErrorMessage = "One of the products in your cart just went out of stock. Please review your cart."
                    };
                }

                await _orderRepository.CreateAsync(order);
                await transaction.CommitAsync();
            }

            if (requiresPrescription)
            {
                return new CheckoutResponse
                {
                    Success = true,
                    OrderId = order.Id,
                    RequiresPrescription = true,
                    CheckoutUrl = null
                };
            }

            string checkoutUrl = null;
            if (request.PaymentMethod == PaymentMethod.Visa)
            {
                checkoutUrl = await CreateStripeSession(order);
            }
            else
            {
                await FinalizeOrder(order);
            }

            return new CheckoutResponse
            {
                Success = true,
                OrderId = order.Id,
                CheckoutUrl = checkoutUrl
            };
        }

        public async Task<CheckoutResponse> PayOrder(string userId, int orderId)
        {
            var order = await _orderRepository.GetOne(
                o => o.Id == orderId && o.UserId == userId,
                new string[] { nameof(Order.OrderItems) });

            if (order == null)
            {
                return new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = "Order not found."
                };
            }

            if (order.OrderStatus != OrderStatusEnum.Pending)
            {
                return new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = "Order is not ready for payment."
                };
            }

            string checkoutUrl = null;
            if (order.PaymentMethod == PaymentMethod.Visa)
            {
                checkoutUrl = await CreateStripeSession(order);
            }
            else
            {
                await FinalizeOrder(order);
            }

            return new CheckoutResponse
            {
                Success = true,
                OrderId = order.Id,
                CheckoutUrl = checkoutUrl
            };
        }

        private async Task<string> CreateStripeSession(Order order)
        {
            var sessionService = new Stripe.Checkout.SessionService();
            var sessionOptions = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                LineItems = order.OrderItems.Select(oi => new Stripe.Checkout.SessionLineItemOptions
                {
                    Quantity = oi.Quantity,
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(oi.UnitPrice * 100),
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Product #{oi.ProductId}"
                        }
                    }
                }).ToList(),
                SuccessUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/api/Checkout/success?sessionId={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/api/Checkout/cancel",
            };
            var session = await sessionService.CreateAsync(sessionOptions);

            order.StripeSessionId = session.Id;
            await _orderRepository.UpdateAsync(order);

            return session.Url;
        }

        public async Task<CheckoutResponse> ConfirmPayment(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = "Session id is required."
                };
            }

            var order = await _orderRepository.GetOne(o => o.StripeSessionId == sessionId,
                new string[] { nameof(Order.OrderItems),
                    $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}"
                 , $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}.{nameof(Product.ProductTranslations)}" }
                );

            if (order == null)
            {
                return new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = "Order not found."
                };
            }

            // Idempotency guard: this endpoint is hit by a browser redirect *and* by the Stripe
            // webhook, and the user can refresh it. Finalizing twice would decrement stock twice.
            if (order.OrderStatus == OrderStatusEnum.Paid)
            {
                return new CheckoutResponse
                {
                    Success = true,
                    OrderId = order.Id
                };
            }

            if (order.OrderStatus != OrderStatusEnum.Pending)
            {
                return new CheckoutResponse
                {
                    Success = false,
                    OrderId = order.Id,
                    ErrorMessage = "Order is not awaiting payment."
                };
            }

            // Stripe is the only party that knows whether money actually moved. Trusting the
            // redirect alone let anyone mark their own order as paid just by calling the URL.
            Stripe.Checkout.Session session;
            try
            {
                session = await new Stripe.Checkout.SessionService().GetAsync(sessionId);
            }
            catch (Stripe.StripeException)
            {
                return new CheckoutResponse
                {
                    Success = false,
                    OrderId = order.Id,
                    ErrorMessage = "Could not verify the payment with Stripe."
                };
            }

            if (session == null || session.PaymentStatus != "paid")
            {
                return new CheckoutResponse
                {
                    Success = false,
                    OrderId = order.Id,
                    ErrorMessage = "Payment has not been completed."
                };
            }

            await FinalizeOrder(order);

            return new CheckoutResponse
            {
                Success = true,
                OrderId = order.Id
            };
        }

        public async Task<CheckoutResponse> HandleStripeWebhook(string requestBody, string? stripeSignature)
        {
            var webhookSecret = _configuration["StripeSettings:WebhookSecret"];

            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                return new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = "StripeSettings:WebhookSecret is not configured."
                };
            }

            Stripe.Event stripeEvent;
            try
            {
                // Verifies the payload was signed by Stripe; without it anyone could POST here.
                stripeEvent = Stripe.EventUtility.ConstructEvent(requestBody, stripeSignature, webhookSecret);
            }
            catch (Stripe.StripeException)
            {
                return new CheckoutResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid Stripe signature."
                };
            }

            if (stripeEvent.Type != "checkout.session.completed")
            {
                // Acknowledged so Stripe stops retrying events this app does not act on.
                return new CheckoutResponse { Success = true };
            }

            if (stripeEvent.Data.Object is not Stripe.Checkout.Session session)
            {
                return new CheckoutResponse { Success = true };
            }

            return await ConfirmPayment(session.Id);
        }

        private async Task FinalizeOrder(Order order)
        {
            // Stock was already taken when the order was created, so paying only flips the status.
            order.OrderStatus = OrderStatusEnum.Paid;
            await _orderRepository.UpdateAsync(order);
            await _cartService.ClearCart(order.UserId);

            var lowStockProducts = await _productRepository.GetProductsBelowMinimumStock(
                order.OrderItems.Select(oi => oi.ProductId));

            // Low stock is the pharmacy's problem, not the customer's - this used to email the
            // customer a restocking notice for every product their own order pushed under the limit.
            if (lowStockProducts.Any())
            {
                await _notificationService.NotifyPharmacists(
                    NotificationTypeEnum.LowStock,
                    lowStockProducts.Select(p => p.Id).ToList());
            }

            // Best effort: the confirmation mail used to sit in the middle of this method, so a
            // failing SMTP server threw and left the order paid with the stock never adjusted.
            try
            {
                var user = await _userManager.FindByIdAsync(order.UserId);
                await _emailSender.SendEmailAsync(user.Email, "Order Confirmation",
                    $"Your order #{order.Id} has been successfully placed and paid.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not send the confirmation email for order {OrderId}.", order.Id);
            }
        }
    }
}
