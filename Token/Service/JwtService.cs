using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using apitienda.Data;
using apitienda.Models;
using Microsoft.IdentityModel.Tokens;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly DataContext _context;

    public int AccessTokenExpiryMinutes =>
        int.Parse(_configuration["Jwt:AccessTokenMinutes"]!);

    public int RefreshTokenExpiryDays =>
        int.Parse(_configuration["Jwt:RefreshTokenDays"]!);


    public JwtService(IConfiguration configuration, DataContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    // Creamos el token a partir de la llave en la palabra secreta
    private string CreateToken(IEnumerable<Claim> claims, DateTime expires)
    {
        // Toma tu palabra secreta (definida en el appsettings.json) y la convierte en una llave matemática.
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        // Es el algoritmo que "sella" el token. Si alguien cambia una sola letra
        // del token, el sello se rompe y la API lo detectará.
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        // Convierte ese objeto complejo en el string largo lleno de puntos que todos conocemos.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Generamos el refresh token
    public string GenerateRefreshToken(Guid userId)
    {
        var jti = Guid.NewGuid().ToString();

        // claim "type": "refresh". Esto es una medida de seguridad extra
        // para que nadie intente usar un Refresh Token como si fuera un Access
        // Token para entrar a ver datos.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim("type", "refresh")
        };

        return CreateToken(
            claims,
            DateTime.UtcNow.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenDays"]!)
            )
        );
    }

    public string GenerateAccessToken(Guid userId, string email)
    {
        var jti = Guid.NewGuid().ToString();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.Email, email)
        };

        return CreateToken(
            claims,
            DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:AccessTokenMinutes"]!)
            )
        );
    }

    // Validamos el token
    public ClaimsPrincipal ValidateToken(string token, bool validateLifetime = true)
    {
        var handler = new JwtSecurityTokenHandler();

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            ),
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.Zero
        };

        var principal = handler.ValidateToken(token, parameters, out _);

        var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
            ?? throw new SecurityTokenException("Token sin jti");

        // Si el token es valido verificamos que no este en la lista negra
        if (_context.token_blacklist.Any(t => t.jti == jti))
            throw new SecurityTokenException("Token revocado");

        return principal;
    }

    // toma el JTI (el número de serie único) y lo guarda en la base de datos.
    // A partir de ese milisegundo, ValidateToken lo rechazará.
    public async Task RevokeTokenAsync(string jti, Guid userId, DateTimeOffset expiresAt, string reason)
    {
        _context.token_blacklist.Add(new token_blacklist
        {
            jti = jti,
            user_id = userId,
            revoked_at = DateTimeOffset.UtcNow,
            expires_at = expiresAt,
            reason = reason
        });

        await _context.SaveChangesAsync();
    }
}
