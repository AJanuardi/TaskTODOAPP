using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TODODATABASE.Models;
using TODODATABASE.Data;
using System.IO;

namespace TODODATABASE.Controllers
{
    [ApiController]
    [Route("/user")]
    public class UserControllers : ControllerBase
    {
        private IConfiguration Configuration;

        public AppDbContext AppDbContext;

        public UserControllers(IConfiguration configuration,AppDbContext appDbContext)
        {
            Configuration = configuration;
            AppDbContext = appDbContext;
        }

        [Authorize]
        [HttpGet("welcome")]
        public IActionResult Welcome()
        {
            var token = System.IO.File.ReadAllText("Token.txt");
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;

            if (securityToken == null)
            {
                return Unauthorized();
            }

            return Ok(new
            {
                message = "Welcome",
            });
        }

        [HttpPost("register")]
        public IActionResult Register ([FromBody] User register)
        {
            var data = from x in AppDbContext.Users select new {x.Nama,x.Password};
            foreach (var x in data)
            if (register.Nama == x.Nama)
            {
                return Ok("Name Already in Database");
            }
            AppDbContext.Users.Add(register);
            AppDbContext.SaveChanges();
            return Ok("Account has been Signed");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User login)
        {
            IActionResult response = Unauthorized();

           var user = AuthenticatedUser(login);

           if (user!= null)
           {
               var token = GenerateJwtToken(user);
               TextWriter tkn = new StreamWriter("Token.txt", true);
               tkn.WriteLine(token);
               tkn.Close(); 
               return Ok(new{token = token});
           }

            return Ok();
        }

        private User AuthenticatedUser(User input)
        {
            User user = null;
            var data = from x in AppDbContext.Users select new {x.UserId,x.Nama,x.Password};
            foreach(var x in data)
            {
                if (input.Nama == x.Nama && input.Password == x.Password)
            {
                user = new User
                {
                    UserId = x.UserId,
                    Nama = input.Nama,
                    Password = input.Password,
                };
                return user;
            }
            }
            
            return user;
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: Configuration["Jwt:Issuer"],
                audience: Configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(200),
                signingCredentials:credentials
            );
            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return encodedToken;
        }
}
}