using System;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;

namespace BlogEngine.Services
{
    public class BlogUserService : IUserService
    {
        private readonly IConfiguration _config;

        public BlogUserService(IConfiguration config)
        {
            _config = config;
        }

        public bool ValidateUser(string username, string password) =>
            username == _config["user:username"] && VerifyHashedPassword(password, _config);

        // TODO: Understand this code.
        private bool VerifyHashedPassword(string password, IConfiguration config)
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(config["user:salt"]);
            byte[] hashBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            );

            string hashText = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            return hashText == config["user:password"];
        }
    }
}