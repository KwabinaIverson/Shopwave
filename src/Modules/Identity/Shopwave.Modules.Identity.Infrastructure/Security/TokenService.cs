using Shopwave.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shopwave.Modules.Identity.Infrastructure.Security;

public class TokenService : ITokenService
{
   private readonly IConfiguration _configuration;

   public TokenService(IConfiguration configuration)
   {
      _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
   }
   public string GenerateToken(Guid userId, string role)
   {
      var key = new SymmetricSecurityKey(
         Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var claims = new[]
      {
         new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
         new Claim(ClaimTypes.Role, role)
      };

      var token = new JwtSecurityToken(
         issuer: _configuration["Jwt:Issuer"],
         audience: _configuration["Jwt:Audience"],
         claims: claims,
         expires: DateTime.UtcNow.AddMinutes(
            double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
         signingCredentials: credentials
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
   }
}