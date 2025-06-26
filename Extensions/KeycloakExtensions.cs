using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace TrueDetectWebAPI.Extensions
{
    public static class KeycloakExtensions
    {
        public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["Keycloak:Authority"];
                    options.Audience = configuration["Keycloak:Audience"];
                    options.RequireHttpsMetadata = configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtRegisteredClaimNames.PreferredUsername
                    };
                });

            return services;
        }
    }
} 