using FluentValidation;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.PL.Validators
{
    public class CartItemRequestValidator : AbstractValidator<CartItemRequest>
    {
        public CartItemRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("A valid product is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 per item.");
        }
    }


    public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
    {
        public UpdateCartItemRequestValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 per item.");
        }
    }


    public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
    {
        public CheckoutRequestValidator()
        {
            RuleFor(x => x.PaymentMethod)
                .IsInEnum().WithMessage("Payment method must be either 'Cash' or 'Visa'.");

            // All three are optional here: the service falls back to the address saved on the
            // user profile. They are only checked for shape when the caller does send them.
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^07[7-9]\d{7}$")
                .WithMessage("Phone number must be a valid Jordanian mobile number, e.g. 0791234567.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.City)
                .MaximumLength(100)
                .When(x => x.City != null);

            RuleFor(x => x.Street)
                .MaximumLength(200)
                .When(x => x.Street != null);
        }
    }


    public class PrescriptionRequestValidator : AbstractValidator<PrescriptionRequest>
    {
        public PrescriptionRequestValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("A valid order is required.");

            RuleFor(x => x.Image)
                .NotNull().WithMessage("A prescription image is required.");
        }
    }


    public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
    {
        public UpdateOrderStatusRequestValidator()
        {
            RuleFor(x => x.OrderStatus)
                .IsInEnum().WithMessage("Order status is not a recognised value.");

            // Paid is reached by paying, never by an admin flipping a dropdown - allowing it here
            // would mark an order as paid without any money having moved.
            RuleFor(x => x.OrderStatus)
                .NotEqual(OrderStatusEnum.Paid)
                .WithMessage("An order can only become Paid through the checkout flow.");
        }
    }


    public class UpdatePrescriptionStatusRequestValidator : AbstractValidator<UpdatePrescriptionStatusRequest>
    {
        public UpdatePrescriptionStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Prescription status is not a recognised value.")
                .Must(status => status is PrescriptionStatusEnum.Approved or PrescriptionStatusEnum.Rejected)
                .WithMessage("A review must set the status to either Approved or Rejected.");
        }
    }
}
