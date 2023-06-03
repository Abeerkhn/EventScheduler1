using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EventScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configurtion)
        {
            _configuration = configurtion;
            
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            CreatePasswordHash(request.Password, out byte[] Passwordhash, out byte[] Passwordsalt);
            user.Username = request.Username;
            user.Passwordhash = Passwordhash;
            user.Passwordsalt = Passwordsalt;
            return Ok(user);

        }
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            if (user.Username != request.Username)
            {
                return BadRequest("User Not Found");
            }
            if (!VerifyPasswordHash(request.Password, user.Passwordhash, user.Passwordsalt))
            {
                return BadRequest("Wrong Password");
            }
            string token = Createtoken( user);
            return Ok(token);
        }
        private string Createtoken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name , user.Username)
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials:cred);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;

              
                
            
            return string.Empty;
        }

        public static void CreatePasswordHash(string password, out byte[] Passwordhash, out byte[] Passwordsalt)
        {
            using (var hmac = new HMACSHA512())
            {
                Passwordsalt = hmac.Key;
                Passwordhash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            };
        }
        private bool VerifyPasswordHash(string password, byte[] Passwordhash, byte[] Passwordsalt)
        {
            using (var hmac = new HMACSHA512(user.Passwordsalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(Passwordhash);
            }
        }
    }
}
