using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreWithAuth.Models;
using AspNetCoreWithAuth.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace AspNetCoreWithAuth.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsersContext _usersContext;
        private readonly AuthSettings _authSettings;

        public AccountController(
            UsersContext usersContext, 
            AuthSettings authSettings)
        {
            _usersContext = usersContext;
            _authSettings = authSettings;
        }

        [HttpPost("/token")]
        public async Task Token()
        {
            var email = Request.Form["email"];
            var password = Request.Form["password"];
 
            var identity = GetIdentity(email, password);
            if (identity == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Invalid email or password.");
                return;
            }
 
            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: _authSettings.Issuer,
                audience: _authSettings.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(_authSettings.Lifetime)),
                signingCredentials: new SigningCredentials(
                    _authSettings.GetSymmetricSecurityKey(), 
                    SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
             
            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };
 
            Response.ContentType = "application/json";
            await Response.WriteAsync(
                JsonConvert.SerializeObject(response, new JsonSerializerSettings {Formatting = Formatting.Indented}));
        }
 
        private ClaimsIdentity GetIdentity(string email, string password)
        {
            var user = _usersContext.Users.FirstOrDefault(x => x.Email == email && x.Password == password);
            
            if (user == null) return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            };
            var claimsIdentity =
                new ClaimsIdentity(
                    claims, "Token", 
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }
    }
}