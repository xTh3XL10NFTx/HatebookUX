using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;

namespace HatebookUX.Services
{
    public static class ApiCallService
    {

        private static string RetrieveAuthToken(IServiceProvider services)
        {
            var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
            var session = httpContextAccessor.HttpContext.Session;
            string token = session.GetString("AuthToken");

            return token;
        }

        private static void ConfigureHttpClient(HttpClient client, string baseAddress, string authToken)
        {
            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        public static async void MakeRequest(this IServiceCollection service)
        {
            // Create the retry policy
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));



            var tokenRetrievalPolicy = Policy
                .Handle<HttpRequestException>()
                .RetryAsync(3);


            // service.AddHttpClient("GetUsers", client =>
            // {
            //     client.BaseAddress = new Uri("https://localhost:7288/api/Account/get");
            //     client.DefaultRequestHeaders.Accept.Clear();
            //     client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // })
            //.ConfigureHttpClient(async (services, client) =>
            //{
            //    // Retrieve the token from the HttpContext session
            //    var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
            //    var session = httpContextAccessor.HttpContext.Session;
            //    string token = session.GetString("AuthToken");

            //    // Add the bearer token to the request's Authorization header
            //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            //});

            service.AddHttpClient();

            service.AddScoped<ApiService>();

        }
    }
}