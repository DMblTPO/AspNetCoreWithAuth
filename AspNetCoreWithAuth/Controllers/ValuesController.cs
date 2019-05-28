using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreWithAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : Controller
    {
        [Authorize]
        [Route("getlogin")]
        public IActionResult GetLogin()
        {
            return Ok($"Ваш логин: {User.Identity.Name}");
        }

        [Authorize(Roles = "admin,user")]
        [Route("getrole")]
        public IActionResult GetRole()
        {
            return Ok($"Ваша роль: {User.Claims.First(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value}");
        }
    }
}
