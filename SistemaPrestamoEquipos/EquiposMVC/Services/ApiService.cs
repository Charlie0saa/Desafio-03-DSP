using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EquiposMVC.Models;

namespace EquiposMVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient();
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7038/";
            _httpClient.BaseAddress = new Uri(baseUrl);

            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private void AddJwtTokenHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<AuthResponseModel?> LoginAsync(LoginViewModel loginDto)
        {
            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AuthResponseModel>(responseBody, _jsonOptions);
            }
            return null;
        }

        public async Task<(T? Data, string? ErrorMessage, HttpStatusCode StatusCode)> GetAsync<T>(string endpoint)
        {
            AddJwtTokenHeader();
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                return (data, null, response.StatusCode);
            }

            var error = await ExtractErrorMessage(response);
            return (default, error, response.StatusCode);
        }

        public async Task<(bool Success, string? ErrorMessage, HttpStatusCode StatusCode)> PostAsync<T>(string endpoint, T data)
        {
            AddJwtTokenHeader();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                return (true, null, response.StatusCode);
            }

            var error = await ExtractErrorMessage(response);
            return (false, error, response.StatusCode);
        }

        public async Task<(bool Success, string? ErrorMessage, HttpStatusCode StatusCode)> PutAsync<T>(string endpoint, T data)
        {
            AddJwtTokenHeader();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                return (true, null, response.StatusCode);
            }

            var error = await ExtractErrorMessage(response);
            return (false, error, response.StatusCode);
        }

        public async Task<(bool Success, string? ErrorMessage, HttpStatusCode StatusCode)> DeleteAsync(string endpoint)
        {
            AddJwtTokenHeader();
            var response = await _httpClient.DeleteAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                return (true, null, response.StatusCode);
            }

            var error = await ExtractErrorMessage(response);
            return (false, error, response.StatusCode);
        }

        private async Task<string> ExtractErrorMessage(HttpResponseMessage response)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("mensaje", out var mensaje))
                {
                    return mensaje.GetString() ?? "Ocurrió un error inesperado.";
                }
            }
            catch { }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Sesión expirada o no autorizada.",
                HttpStatusCode.Forbidden => "No posee permisos para realizar esta acción.",
                HttpStatusCode.Conflict => "Conflicto de negocio o de datos.",
                _ => $"Error en el servidor ({response.StatusCode})."
            };
        }
    }
}