using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SpotifyAPI.Web;

namespace tracksByPopularity.models;

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly string _jwtKey =
        configuration["Jwt:Key"] ?? throw new ArgumentNullException("JWT Key is missing");

    private readonly string _jwtIssuer =
        configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("JWT Issuer is missing");

    private readonly string _jwtAudience =
        configuration["Jwt:Audience"] ?? throw new ArgumentNullException("JWT Audience is missing");

    public string GenerateJwtToken(string userId, string spotifyAccessToken, DateTime expiresAt)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtKey);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("spotify_token", spotifyAccessToken),
            new Claim("expires_at", expiresAt.ToString("o")),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1), // JWT token expires in 1 hour
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public UserToken? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtKey);

        try
        {
            tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAudience,
                    ClockSkew = TimeSpan.Zero,
                },
                out var validatedToken
            );

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
            var spotifyToken = jwtToken.Claims.First(x => x.Type == "spotify_token").Value;
            var expiresAtString = jwtToken.Claims.First(x => x.Type == "expires_at").Value;

            return new UserToken
            {
                UserId = userId,
                AccessToken = spotifyToken,
                ExpiresAt = DateTime.Parse(expiresAtString),
            };
        }
        catch
        {
            return null;
        }
    }

    public SpotifyClient CreateSpotifyClient(string accessToken)
    {
        var config = SpotifyClientConfig.CreateDefault().WithToken(accessToken);
        return new SpotifyClient(config);
    }
}
