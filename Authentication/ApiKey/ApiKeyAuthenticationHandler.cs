using GymManagerApi.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace GymManagerApi.Authentication.ApiKey
{
    class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string API_KEY_HEADER = "Api-Key";

        private readonly GymManagerApiContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            GymManagerApiContext context,
            UserManager<IdentityUser> userManager
        ) : base(options, logger, encoder, clock)
        {
            _context = context;
            _userManager = userManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey(API_KEY_HEADER))
            {
                return AuthenticateResult.Fail("Header Not Found.");
            }

            string apiKeyToValidate = Request.Headers[API_KEY_HEADER];

            var apiKey = await _context.UserApiKeys
                .Include(uak => uak.User)
                .SingleOrDefaultAsync(uak => uak.Value == apiKeyToValidate);

            if (apiKey == null)
            {
                return AuthenticateResult.Fail("Invalid key.");
            }
            var userRoles = await _userManager.GetRolesAsync(apiKey.User);

            return AuthenticateResult.Success(CreateTicket(apiKey.User, userRoles));
        }

        private AuthenticationTicket CreateTicket(IdentityUser user, IList<string> userRoles)
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return ticket;
        }
    }
}
