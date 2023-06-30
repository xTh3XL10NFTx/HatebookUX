using HatebookUX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace HatebookUX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly HubConnection _hubConnection;
        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7288/chathub")
                .Build();
        }



        public async Task<IActionResult> Index(Message chatMessage)
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                // Start the connection if it's not already started
                await _hubConnection.StartAsync();
            }

            // Send the message to the API endpoint
            using (var httpClient = new HttpClient())
            {
                var apiUrl = "https://localhost:7288/api/Chat/allChat";

                var json = JsonConvert.SerializeObject(new { user = chatMessage.email, message = chatMessage.message });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Capture the request payload
                var requestPayload = await content.ReadAsStringAsync();
                Console.WriteLine(requestPayload); // Output the payload to the console for inspection

                var response = await httpClient.PostAsync(apiUrl, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Redirect back to the index page
                    return View();
                }
                else
                {
                    // Handle the error case
                    // You can customize this based on your requirements
                    return View();
                }
            }

        }

        public async Task<IActionResult> SendMessage(Message chatMessage)
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                // Start the connection if it's not already started
                await _hubConnection.StartAsync();
            }

            // Send the message to the API endpoint
            using (var httpClient = new HttpClient())
            {
                var apiUrl = "https://localhost:7288/api/Chat/allChat";

                var json = JsonConvert.SerializeObject(new { user = chatMessage.email, message = chatMessage.message });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Capture the request payload
                var requestPayload = await content.ReadAsStringAsync();
                Console.WriteLine(requestPayload); // Output the payload to the console for inspection

                var response = await httpClient.PostAsync(apiUrl, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Redirect back to the index page
                    return View();
                }
                else
                {
                    // Handle the error case
                    // You can customize this based on your requirements
                    return View();
                }
            }

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Contacts()
        {
            return View();
        }


        public async Task<IActionResult> SendMessage(string username, string message)
        {
            using (var httpClient = new HttpClient())
            {
                var apiUrl = "https://localhost:7288/api/Chat/allChat";

                var formData = new Dictionary<string, string>
        {
            { "user", username },
            { "message", message }
        };

                var content = new FormUrlEncodedContent(formData);
                var response = await httpClient.PostAsync(apiUrl, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Redirect back to the index page
                    return RedirectToAction("Index");
                }
                else
                {
                    // Handle the error case
                    // You can customize this based on your requirements
                    return RedirectToAction("Error");
                }
            }
        }


    }
}