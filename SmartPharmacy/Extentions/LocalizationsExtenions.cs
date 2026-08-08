using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace SmartPharmacy.PL.Extentions
{
    public static class LocalizationsExtenions
    {

        public static IServiceCollection AddLocaliztionsServices(this IServiceCollection Service)

        {

            Service.AddLocalization(options =>
            options.ResourcesPath = "");

            const string defaultCulture = "ar";

            var supportedCultures = new[]
            {
             new CultureInfo("en"),
             new CultureInfo("ar")
            };

            Service.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders.Clear();
                //options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider
                //{
                //    QueryStringKey = "lang"
                //})
                options.RequestCultureProviders.Add(
                    new AcceptLanguageHeaderRequestCultureProvider(
                    ));
            });
            return Service;
        }

    }
}
