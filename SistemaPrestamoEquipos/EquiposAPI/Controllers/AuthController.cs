using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using EquiposAPI.Data;
using EquiposAPI.DTOs;
using EquiposAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EquiposAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IMapper mapper, IConfiguration config)
        {
            _context = context;
            _mapper = mapper;
            _config = config;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<UsuarioDto>> Register(RegisterUserDto registerDto)
        {
            // Caso 5: Username duplicado -> 409 Conflict
            if (await _context.Usuarios.AnyAsync(u => u.Username.ToLower() == registerDto.Username.ToLower()))
            {
                return Conflict(new { mensaje = "El nombre de usuario ya está registrado." });
            }

            var usuario = _mapper.Map<Usuario>(registerDto);
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<UsuarioDto>(usuario));
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username.ToLower() == loginDto.Username.ToLower());

            // Caso 2: Login fallido -> 401 Unauthorized
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.PasswordHash))
            {
                return Unauthorized(new { mensaje = "Credenciales incorrectas." });
            }

            // Caso 1: Generar Token JWT con Claims de Rol e Id
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSettings = _config.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "ClavePredeterminadaParaDesarrolloLocalDe32Bits!");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var expiration = DateTime.UtcNow.AddHours(8);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new AuthResponseDto
            {
                Token = tokenHandler.WriteToken(token),
                Username = usuario.Username,
                Rol = usuario.Rol,
                Expiration = expiration
            });
        }
    }
}
