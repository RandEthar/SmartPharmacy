using FluentValidation;
using SmartPharmacy.DAL.DTO.Request;

namespace SmartPharmacy.PL.Validators
{
    public class ProductTranslationRequestValidator : AbstractValidator<ProductTranslationRequest>
    {
        public ProductTranslationRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description is required.")
                .MaximumLength(2000);

            RuleFor(x => x.Language)
                .NotEmpty().WithMessage("Language is required.")
                .Must(BeSupported).WithMessage("Language must be either 'en' or 'ar'.");
        }

        private static bool BeSupported(string language) =>
            language is "en" or "ar";
    }


    public class CategoryTranslationRequestValidator : AbstractValidator<CategoryTranslationRequest>
    {
        public CategoryTranslationRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(200);

            RuleFor(x => x.Language)
                .NotEmpty().WithMessage("Language is required.")
                .Must(language => language is "en" or "ar")
                .WithMessage("Language must be either 'en' or 'ar'.");
        }
    }


    public class ProductRequestValidator : AbstractValidator<ProductRequest>
    {
        public ProductRequestValidator()
        {
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

            RuleFor(x => x.MinimumStock)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stock cannot be negative.");

            // A pharmacy has no reason to add stock that is already unsellable.
            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Expiry date must be in the future.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("A valid category is required.");

            RuleFor(x => x.Image)
                .NotNull().WithMessage("A product image is required.");

            RuleFor(x => x.ProductTranslations)
                .NotEmpty().WithMessage("At least one translation is required.");

            RuleForEach(x => x.ProductTranslations)
                .SetValidator(new ProductTranslationRequestValidator());
        }
    }


    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            // Every field is optional on a PATCH, so each rule only applies when a value was sent.
            RuleFor(x => x.Price!.Value)
                .GreaterThan(0).WithMessage("Price must be greater than zero.")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.StockQuantity!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
                .When(x => x.StockQuantity.HasValue);

            RuleFor(x => x.MinimumStock!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stock cannot be negative.")
                .When(x => x.MinimumStock.HasValue);

            RuleFor(x => x.CategoryId!.Value)
                .GreaterThan(0).WithMessage("A valid category is required.")
                .When(x => x.CategoryId.HasValue);

            RuleForEach(x => x.ProductTranslations)
                .SetValidator(new ProductTranslationRequestValidator())
                .When(x => x.ProductTranslations != null);
        }
    }


    public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
    {
        public CategoryRequestValidator()
        {
            RuleFor(x => x.Image)
                .NotNull().WithMessage("A category image is required.");

            RuleFor(x => x.CategoryTranslations)
                .NotEmpty().WithMessage("At least one translation is required.");

            RuleForEach(x => x.CategoryTranslations)
                .SetValidator(new CategoryTranslationRequestValidator());
        }
    }


    public class CategoryUpdateRequestValidator : AbstractValidator<CategoryUpdateRequest>
    {
        public CategoryUpdateRequestValidator()
        {
            RuleForEach(x => x.CategoryTranslations)
                .SetValidator(new CategoryTranslationRequestValidator())
                .When(x => x.CategoryTranslations != null);
        }
    }


    public class PagenationRequestValidator : AbstractValidator<PagenationRequest>
    {
        public PagenationRequestValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be 1 or greater.");

            // Capped so a single request cannot be used to pull the whole table.
            RuleFor(x => x.Limit)
                .InclusiveBetween(1, 100)
                .WithMessage("Limit must be between 1 and 100.");

            RuleFor(x => x.Search)
                .MaximumLength(100)
                .When(x => x.Search != null);
        }
    }


    public class ProductFilterRequestValidator : AbstractValidator<ProductFilterRequest>
    {
        public ProductFilterRequestValidator()
        {
            Include(new PagenationRequestValidator());

            RuleFor(x => x.MinPrice!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum price cannot be negative.")
                .When(x => x.MinPrice.HasValue);

            RuleFor(x => x.MaxPrice!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Maximum price cannot be negative.")
                .When(x => x.MaxPrice.HasValue);

            RuleFor(x => x)
                .Must(x => x.MinPrice <= x.MaxPrice)
                .WithMessage("Minimum price cannot be greater than maximum price.")
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);
        }
    }
}
