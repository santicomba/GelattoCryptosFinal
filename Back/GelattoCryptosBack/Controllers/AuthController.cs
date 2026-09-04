using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
    }

    public class RegisterDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = "usuario"; // "usuario" o "admin"
    }

    public class LoginDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // POST /auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // Identity pide un "email" internamente, usamos el nombre de usuario + un dominio falso
        var user = new IdentityUser { UserName = dto.Usuario, Email = dto.Usuario + "@gelatto.local" };
        var resultado = await _userManager.CreateAsync(user, dto.Password);

        if (!resultado.Succeeded)
            return BadRequest(resultado.Errors.Select(e => e.Description));

        // si el rol no existe todavía en la base, lo creamos
        if (!await _roleManager.RoleExistsAsync(dto.Rol))
            await _roleManager.CreateAsync(new IdentityRole(dto.Rol));

        await _userManager.AddToRoleAsync(user, dto.Rol);

        return Ok(new { mensaje = "Usuario creado con éxito" });
    }

    // POST /auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Usuario);
        if (user == null) return Unauthorized("Usuario o contraseña incorrectos.");

        var passwordOk = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordOk) return Unauthorized("Usuario o contraseña incorrectos.");

        var roles = await _userManager.GetRolesAsync(user);
        var rol = roles.FirstOrDefault() ?? "usuario";

        var token = GenerarToken(user, rol);

        return Ok(new
        {
            token,
            usuario = user.UserName,
            rol
        });
    }

    private string GenerarToken(IdentityUser user, string rol)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Role, rol)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}