using Firebase.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Services
{
    public class FirebaseAuthService
    {
        private readonly FirebaseAuthProvider _authProvider;

        public FirebaseAuthService(IConfiguration configuration)
        {
            string apiKey = configuration["Firebase:ApiKey"];
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
        }

        public async Task<FirebaseAuthLink> RegisterWithEmailAndPasswordAsync(string email, string password, string displayName)
        {
            return await _authProvider.CreateUserWithEmailAndPasswordAsync(email, password, displayName);
        }

        public async Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            return await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
        }
    }
}
