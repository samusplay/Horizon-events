using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Horizon.Events.Security;

public static class JwtExtensions
{
    // Valida los tokens emitidos por Identity usando solo la llave pública (RS256)
    public static IServiceCollection AddJwtSecurity(this IServiceCollection services)
    {
        var rsa = RSA.Create();
        var publicKeyPath = Path.Combine(AppContext.BaseDirectory, "Security", "Keys", "public.pem");
        rsa.ImportFromPem(File.ReadAllText(publicKeyPath));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false; // mantiene los claims con el nombre que vienen en el token
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidIssuer = "Horizon.Identity",
                    ValidateAudience = true,
                    ValidAudience = "Horizon.Platform",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    // Identity escribe el rol con este nombre largo
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                };
            });

        // Solo los creadores pueden gestionar eventos
        services.AddAuthorizationBuilder()
            .AddPolicy("CreatorOnly", policy => policy.RequireRole("Creador"));

        return services;
    }
}