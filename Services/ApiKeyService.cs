using GymManagerApi.Data;
using GymManagerApi.Models;
using Microsoft.AspNetCore.Identity;

namespace GymManagerApi.Services
{
    public class ApiKeyService
    {
        private readonly GymManagerApiContext _context;

        public ApiKeyService(GymManagerApiContext context)
        {
            _context = context;
        }

        public UserApiKey CreateApiKey(IdentityUser user)
        {
            var newApiKey = new UserApiKey
            {
                User = user,
                Value = GenerateApiKeyValue()
            };

            _context.UserApiKeys.Add(newApiKey);

            _context.SaveChanges();

            return newApiKey;
        }

        private string GenerateApiKeyValue() =>
            $"{Guid.NewGuid().ToString()}-{Guid.NewGuid().ToString()}";
    }

}
