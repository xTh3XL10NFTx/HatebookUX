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

        private HttpClient CreateHttpClient(string controllerType, string request)
        {
            var client = _httpClientFactory.CreateClient();

            // Set default request headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var token = RetrieveAuthToken(_httpContextAccessor);
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            client.BaseAddress = new Uri("https://localhost:7288/api/" + controllerType + "/" + request);

            return client;
        }

        public async Task<HttpResponseMessage> MakeRequestAsync(string method, string controllerType, string request, object data = null)
        {
            using (var client = CreateHttpClient(controllerType, request))
            {
                HttpResponseMessage response;

                switch (method.ToLower())
                {
                    case "get":
                        response = await client.GetAsync(request);
                        break;
                    case "post":
                        response = await client.PostAsJsonAsync(request, data);
                        break;
                    case "put":
                        response = await client.PutAsJsonAsync(request, data);
                        break;
                    case "delete":
                        response = await client.DeleteAsync(request);
                        break;
                    default:
                        throw new ArgumentException("Invalid HTTP method.");
                }

                return response;
            }
        }

    }
}