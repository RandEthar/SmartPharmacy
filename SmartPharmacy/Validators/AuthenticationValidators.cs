using FluentValidation;
using SmartPharmacy.DAL.DTO.Request;

namespace SmartPharmacy.PL.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MinimumLength(3).WithMessage("Full name must be at least 3 characters.")
                .MaximumLength(100);

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("User name is required.")
                .MinimumLength(3)
                .MaximumLength(50)
                .Matches("^[a-zA-Z0-9._-]+$")
                .WithMessage("User name may only contain letters, digits, dots, dashes and underscores.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is not a valid email address.");

            // Identity enforces the same policy again; repeating it here means the caller gets
            // the rule up front instead of a generic "User creation failed".
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^07[7-9]\d{7}$")
                .WithMessage("Phone number must be a valid Jordanian mobile number, e.g. 0791234567.");
        }
    }


    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            // Deliberately only "not empty": rejecting a badly formed password here would tell
            // an attacker something about the stored password policy.
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is not a valid email address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }


    public class ForgetPasswordRequestValidator : AbstractValidator<ForgetPasswordRequest>
    {
        public ForgetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is not a valid email address.");
        }
    }


    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is not a valid email address.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Reset code is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
        }
    }


    public class ChangeRoleRequestValidator : AbstractValidator<ChangeRoleRequest>
    {
        public ChangeRoleRequestValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(role => DAL.Models.Roles.All.Contains(role))
                .WithMessage($"Role must be one of: {string.Join(", ", DAL.Models.Roles.All)}.");
        }
    }
}
