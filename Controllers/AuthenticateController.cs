using GymManagerApi.Models;
using GymManagerApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GymManagerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApiKeyService _apiKeyService;

        public AuthenticateController(UserManager<IdentityUser> userManager, ApiKeyService apiKeyService)
        {
            _userManager = userManager;
            _apiKeyService = apiKeyService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if(!ModelState.IsValid) { 
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var token = _apiKeyService.CreateApiKey(user);


                return Ok(token);
            }
            return Unauthorized();
        }
    }
}
