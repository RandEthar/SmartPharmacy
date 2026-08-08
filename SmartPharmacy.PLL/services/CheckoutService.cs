using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

        public CheckoutService(

          IOrderRepository orderRepository,
        ICartService cartService,
            IProductRepository productRepository,
            UserManager<ApplicationUser> userManager,
        IFileService fileService,
        IHttpContextAccessor httpContextAccessor,
        IEmailSender emailSender)
        {

            _userManager = userManager;
            _cartService = cartService;
            _productRepository = productRepository;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
            _orderRepository = orderRepository;
            _emailSender = emailSender;
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
                var productResponse = product.Adapt<ProductResponse>();
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

            await _orderRepository.CreateAsync(order);

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
            var order = await _orderRepository.GetOne(o => o.StripeSessionId == sessionId,
                new string[] { nameof(Order.OrderItems),
                    $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}"
                 , $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}.{nameof(Product.ProductTranslations)}" }
                );

            await FinalizeOrder(order);

            return new CheckoutResponse
            {
                Success = true,
                OrderId = order.Id
            };
        }

        private async Task FinalizeOrder(Order order)
        {
            order.OrderStatus = OrderStatusEnum.Paid;
            await _orderRepository.UpdateAsync(order);
            await _cartService.ClearCart(order.UserId);

            var user = await _userManager.FindByIdAsync(order.UserId);
            await _emailSender.SendEmailAsync(user.Email, "Order Confirmation",
                $"Your order #{order.Id} has been successfully placed and paid.");

            var products = await _productRepository.DecreaseQuantity(order.OrderItems.ToList());

            foreach (var item in products)
            {
                var productResponse = item.Adapt<ProductResponse>();

                await _emailSender.SendEmailAsync(user.Email, "Low Stock Alert",
                        $"The product {productResponse.Name} is low in stock. Please restock soon.");
            }
        }
    }
}
