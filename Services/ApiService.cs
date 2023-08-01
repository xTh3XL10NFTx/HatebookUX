using HatebookUX.Models;
using System.Net.Http.Headers;

namespace HatebookUX.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public static string RetrieveAuthToken(IHttpContextAccessor httpContextAccessor)
        {
            var session = httpContextAccessor.HttpContext.Session;
            string token = session.GetString("AuthToken");
            return token;
        }

        public async Task<HttpResponseMessage> MakeRequestAsync(HttpMethod method, string controllerType, string request, object data = null)
        {
            var url = "https://localhost:7288/api/" + controllerType + "/" + request;
            using (var client = new HttpClient())
            {
                var theRequest = new HttpRequestMessage(method, url);

                // Set default request headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var token = RetrieveAuthToken(_httpContextAccessor);
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                client.BaseAddress = new Uri(url);

                HttpResponseMessage response = await client.PostAsJsonAsync("request", data);

                return response;
            }
        }
    }
}