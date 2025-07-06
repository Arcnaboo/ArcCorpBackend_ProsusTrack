using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ArcCorpBackend.Services
{
    /// <summary>
    /// Provides JWT token generation and validation for user authentication.
    /// Uses your decrypted JWT key from ConstantSecretKeyService.
    /// </summary>
    public static class AuthService
    {
        private static readonly string JwtKey = ConstantSecretKeyService.Instance.GetJWT();
        private static readonly string Issuer = "ArcCorp";
        private static readonly string Audience = "ArcCorpClients";
        private static readonly int ExpiresMinutes = 60 * 24 * 7; // 7 days


        /// <summary>
        /// Generates a signed JWT token for the given user ID.
        /// </summary>
        public static string GenerateToken(string userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(ExpiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates the given JWT token. Returns true if valid; sets userId to the token's subject.
        /// </summary>
        public static bool ValidateToken(string token, out string userId)
        {
            userId = null;
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(JwtKey);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            try
            {
                var principal = handler.ValidateToken(token, parameters, out _);
                userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
