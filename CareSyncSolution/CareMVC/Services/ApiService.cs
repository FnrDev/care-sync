using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CareMVC.Services
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint, string? token = null);
        Task<T?> PostAsync<T>(string endpoint, object body, string? token = null);
        Task<T?> PutAsync<T>(string endpoint, object body, string? token = null);
        Task<HttpResponseMessage> PostRawAsync(string endpoint, object body, string? token = null);
        Task<HttpResponseMessage> PutRawAsync(string endpoint, object body, string? token = null);
        Task<HttpResponseMessage> GetRawAsync(string endpoint, string? token = null);
    }

    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient(string? token)
        {
            var client = _httpClientFactory.CreateClient("CareSyncAPI");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
        {
            var client = CreateClient(token);
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
                return default;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object body, string? token = null)
        {
            var client = CreateClient(token);
            var content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
                return default;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        public async Task<T?> PutAsync<T>(string endpoint, object body, string? token = null)
        {
            var client = CreateClient(token);
            var content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
                return default;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        public async Task<HttpResponseMessage> PostRawAsync(string endpoint, object body, string? token = null)
        {
            var client = CreateClient(token);
            var content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");

            return await client.PostAsync(endpoint, content);
        }

        public async Task<HttpResponseMessage> PutRawAsync(string endpoint, object body, string? token = null)
        {
            var client = CreateClient(token);
            var content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");

            return await client.PutAsync(endpoint, content);
        }

        public async Task<HttpResponseMessage> GetRawAsync(string endpoint, string? token = null)
        {
            var client = CreateClient(token);
            return await client.GetAsync(endpoint);
        }
    }
}
