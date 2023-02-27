using Blazored.LocalStorage;
using JWTDemo.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using JWTDemo.Client.Utility;

namespace JWTDemo.Client.Service
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient,
                           AuthenticationStateProvider authenticationStateProvider,
                           ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }

        public async Task<RegisterResult> Register(RegisterModel registerModel)
        {
            var result = await _httpClient.PostAsJsonAsync("api/accounts/CreateAccount", registerModel);
            if (!result.IsSuccessStatusCode)
            {
                return new RegisterResult { Successful = false, Errors = new List<string> { "Error occured" } };
            }  
            return new RegisterResult { Successful = true };
        }


        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            //var loginAsJson = JsonSerializer.Serialize(loginModel);
            //var response = await _httpClient.PostAsync("api/Login",
            //    new StringContent(loginAsJson, Encoding.UTF8, "application/json"));

            //var loginResult = JsonSerializer.Deserialize<LoginResult>(await response.Content.ReadAsStringAsync(),
                //new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var response = await _httpClient.PostAsJsonAsync("api/Login",loginModel);
            var ResponseContent = await response.Content.ReadFromJsonAsync<LoginResult>();

            if (ResponseContent!.Successful)
            {
                await _localStorage.SetItemAsync("authToken", ResponseContent.Token);
                ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(ResponseContent.Token!);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", ResponseContent.Token);

                return ResponseContent;
            }
            return ResponseContent;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
