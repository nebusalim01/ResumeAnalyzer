using ResumeAnalzer.Web.Models;
using System.Net.Http.Json;

namespace ResumeAnalzer.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private AuthResponse? _currentUser;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public AuthResponse? CurrentUser => _currentUser;
        public bool IsLoggedIn => _currentUser != null;

        public async Task<bool> RegisterAsync(RegisterModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/auth/register", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginAsync(LoginModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/auth/login", model);

            if (!response.IsSuccessStatusCode)
                return false;

            _currentUser = await response.Content
                .ReadFromJsonAsync<AuthResponse>();

            // ✅ Save token to AppState
            AppState.Token = _currentUser!.Token;

            // ✅ Add token to all future requests
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", _currentUser!.Token);

            return true;
        }

        public void Logout()
        {
            _currentUser = null;
            AppState.Token = string.Empty;
            AppState.CurrentResumeId = 0;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}