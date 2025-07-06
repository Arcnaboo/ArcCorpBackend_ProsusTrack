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
        /// Generates a signed JWT token for the given user GUID ID (as string).
        /// </summary>
        public static string GenerateToken(string userGuidId)
        {
            if (!Guid.TryParse(userGuidId, out _))
                throw new ArgumentException("Invalid user GUID for token generation.", nameof(userGuidId));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userGuidId),                 // User GUID in sub
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())   // Random token GUID in jti
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
        /// Validates the given JWT token. Returns true if valid; sets userId to sub and tokenId to jti.
        /// </summary>
        public static bool ValidateToken(string token, out string userId)
        {
            userId = null;
            
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(JwtKey);

            var parameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
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
                var identity = principal?.Identity as ClaimsIdentity;

                if (identity == null || !identity.IsAuthenticated)
                    return false;

                var subClaim = identity.FindFirst(ClaimTypes.NameIdentifier); // sub → user GUID
                userId = subClaim?.Value;

                var jtiClaim = identity.FindFirst(JwtRegisteredClaimNames.Jti);
                //tokenId = jtiClaim?.Value;

                return !string.IsNullOrEmpty(userId); //&& !string.IsNullOrEmpty(tokenId);
            }
            catch
            {
                return false;
            }
        }
    }
}
