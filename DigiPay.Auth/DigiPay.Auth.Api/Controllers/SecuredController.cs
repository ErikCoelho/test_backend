using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigiPay.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SecuredController : ControllerBase
    {
        [HttpGet("userinfo")]
        public IActionResult GetUserInfo()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Unauthorized("Not authenticated");
            }

            var userClaims = identity.Claims;
            
            return Ok(new
            {
                Username = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                Email = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                UserId = userClaims.FirstOrDefault(c => c.Type == "UserId")?.Value,
                Message = "This is a secured endpoint that requires authentication"
            });
        }

        [HttpGet("test")]
        public IActionResult TestSecuredEndpoint()
        {
            return Ok(new { Message = "You have successfully accessed a secured endpoint!" });
        }
    }
} 