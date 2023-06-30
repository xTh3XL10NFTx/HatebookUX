using HatebookUX.Models;

namespace HatebookUX.Services
{
    public class ApiService
    {

        private readonly HttpClient httpClient;

        public ApiService()
        {
            httpClient = new HttpClient();
        }

        public async Task<string> CallApiEndpoint(string endpointUrl, LogIn account)
        {
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", account.email),
                new KeyValuePair<string, string>("password", account.password)
            });

            HttpResponseMessage response = await httpClient.PostAsync(endpointUrl, requestContent);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }

            return null; // or throw an exception
        }
    }
}
