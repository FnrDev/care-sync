using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ReportingApplication.Models;

namespace ReportingApplication.Services
{
    public class CareSyncApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CareSyncApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var client = _httpClientFactory.CreateClient("CareSyncApi");

            var loginData = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/auth/login", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var body = await response.Content.ReadAsStringAsync();

            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(
                body,
                _jsonOptions
            );

            return loginResponse?.Token;
        }

        public async Task<AppointmentStatsDto?> GetAppointmentStatsAsync(string token)
        {
            var client = _httpClientFactory.CreateClient("CareSyncApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/reports/appointment-stats");

            if (!response.IsSuccessStatusCode)
                return null;

            var body = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<AppointmentStatsDto>(
                body,
                _jsonOptions
            );
        }

        public async Task<List<DoctorUtilizationDto>?> GetDoctorUtilizationAsync(string token)
        {
            var client = _httpClientFactory.CreateClient("CareSyncApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/reports/doctor-utilization");

            if (!response.IsSuccessStatusCode)
                return null;

            var body = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<DoctorUtilizationDto>>(
                body,
                _jsonOptions
            );
        }
    }
}