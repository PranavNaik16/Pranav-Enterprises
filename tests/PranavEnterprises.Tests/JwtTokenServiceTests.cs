using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using PranavEnterprises.Domain.Constants;
using PranavEnterprises.Domain.Entities;
using PranavEnterprises.Infrastructure.Services;
using Xunit;

namespace PranavEnterprises.Tests;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateToken_ShouldReturnValidJwtWithCorrectClaims()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "PranavEnterprisesEnterpriseSecretKeyForJwtTokenSigning2026!WithMinimum256BitsLength" },
            { "Jwt:Issuer", "PranavEnterprises" },
            { "Jwt:Audience", "PranavEnterprisesClient" },
            { "Jwt:DurationInMinutes", "60" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var service = new JwtTokenService(configuration);

        var user = new ApplicationUser
        {
            Id = "usr-test-123",
            UserName = "superadmin@pranav.com",
            Email = "superadmin@pranav.com",
            FirstName = "Pranav",
            LastName = "Naik",
            JobTitle = "CTO",
            DepartmentName = "Executive"
        };

        var roles = new List<string> { AppRoles.SuperAdmin };

        // Act
        var (token, expiration) = service.GenerateToken(user, roles);

        // Assert
        Assert.NotNull(token);
        Assert.True(token.Length > 50);
        Assert.True(expiration > DateTime.UtcNow);

        // Parse token and inspect claims
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("PranavEnterprises", jwt.Issuer);
        Assert.Contains("PranavEnterprisesClient", jwt.Audiences);

        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");
        Assert.NotNull(roleClaim);
        Assert.Equal(AppRoles.SuperAdmin, roleClaim.Value);

        var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal("superadmin@pranav.com", emailClaim.Value);
    }
}
