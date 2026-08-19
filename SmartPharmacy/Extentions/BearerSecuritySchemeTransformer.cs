using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace SmartPharmacy.PL.Extentions
{
    /// <summary>
    /// AddOpenApi does not read the authentication setup, so without this the generated document
    /// has no Authorize button and every secured endpoint appears untestable from the docs page.
    /// </summary>
    public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        public BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider schemeProvider)
        {
            _schemeProvider = schemeProvider;
        }

        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var schemes = await _schemeProvider.GetAllSchemesAsync();

            if (!schemes.Any(scheme => scheme.Name == "Bearer"))
                return;

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JSON Web Token",
                In = ParameterLocation.Header,
                Description = "Paste the token returned by /api/Authentications/Login. The Bearer prefix is added for you."
            };

            var requirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                }] = Array.Empty<string>()
            };

            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security.Add(requirement);
            }
        }
    }
}
