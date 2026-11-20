using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TradeRestAPI.Models;
using TradeRestAPI.Models.Services;

namespace TradeRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutorizeController : ControllerBase
    {
        private readonly AuthService service;

        public AutorizeController(AuthService _service)
        {
            service = _service;
        }

        public AutorizeController()
        {
            service = new AuthService();
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(string login, string password)
        {
            var client = service.Login(login, password);
            if (client != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, client.Login),
                    new Claim(ClaimTypes.Role, client.Type ?? "User")
                };

                var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromHours(2)), // Время действия токена 2 часа
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

                var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);
                return Ok(new
                {
                    token = tokenString,
                    login = client.Login,
                    firstname = client.Firstname,
                    lastname = client.Lastname,
                    role = client.Type
                });
            }
            return BadRequest(new { message = "Неверный логин или пароль" });
        }

        [HttpPost("register")]
        public async Task<ActionResult<Client>> Register([FromBody] Client client)
        {
            if (service.Register(client))
                return Ok(client);
            return BadRequest(new { message = "Пользователь с таким логином уже существует" });
        }
    }
}
