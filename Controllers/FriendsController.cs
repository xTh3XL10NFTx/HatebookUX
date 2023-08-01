using HatebookUX.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace HatebookUX.Controllers
{
    public class FriendsController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _accountEndpoint;
        private readonly string _groupsEndpoint;
        private readonly string _friendsEndpoint;
        private readonly string _otherEndpoint;
        private readonly ILogger<UserAccountController> _logger;
        //
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FriendsController(IConfiguration configuration, ILogger<UserAccountController> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _baseUrl = _configuration["ApiEndpoints:BaseUrl"];
            _accountEndpoint = _configuration["ApiEndpoints:AccountEndpoint"];
            _groupsEndpoint = _configuration["ApiEndpoints:GroupsEndpoint"];
            _friendsEndpoint = _configuration["ApiEndpoints:FriendsEndpoint"];
            _otherEndpoint = _configuration["ApiEndpoints:OtherEndpoint"];
            _logger = logger;

            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }


        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> GetFriends()
        {
            IList<Friends> friend = new List<Friends>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl + "api/Friend/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Retrieve the token from the HttpContext session
                string token = HttpContext.Session.GetString("AuthToken");

                // Add the bearer token to the request's Authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage getData = await client.GetAsync("friends");

                if (getData.IsSuccessStatusCode)
                {
                    string results = getData.Content.ReadAsStringAsync().Result;
                    friend = JsonConvert.DeserializeObject<IList<Friends>>(results);
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid email or password. Please try again.";
                }

                ViewData.Model = friend;
            }

            return View(friend);
        }

        public async Task<IActionResult> GetFriendRequests()
        {
            IList<Friends> friend = new List<Friends>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl + "api/Friend/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Retrieve the token from the HttpContext session
                string token = HttpContext.Session.GetString("AuthToken");

                // Add the bearer token to the request's Authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage getData = await client.GetAsync("friend-requests");

                if (getData.IsSuccessStatusCode)
                {
                    string results = getData.Content.ReadAsStringAsync().Result;
                    friend = JsonConvert.DeserializeObject<IList<Friends>>(results);
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid email or password. Please try again.";
                }

                ViewData.Model = friend;
            }

            return View();
        }

        public async Task<IActionResult> AddFriend(Friends friend)
        {
            TempData["ErrorMessage"] = "";
            TempData["SuccessMessage"] = "";
            // Get the ClaimsPrincipal object representing the current user
            ClaimsPrincipal currentUser = HttpContext.User;

            // Retrieve the email claim
            Claim creatorClaim = currentUser.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            if (creatorClaim != null)
            {
                friend.creatorId = creatorClaim.Value;
                friend.sender = creatorClaim.Value;
                friend.status = "Pending"; // Set the status to "Pending"
            }
            else
            {
                return View(friend);
            }

            if (friend.reciver != null)
            {
                using (var client = _httpClientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri(_baseUrl + "api/Friend/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string token = HttpContext.Session.GetString("AuthToken");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.PostAsJsonAsync("addFriend", friend);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["ErrorMessage"] = "";
                        TempData["SuccessMessage"] = "Friend added!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to add friend. Please try again.";
                    }
                }
            }

            return View(friend);
        }

        public async Task<IActionResult> AcceptFriendRequest(string inputEmail)
        {
            if(inputEmail != null) {
            using (var client = _httpClientFactory.CreateClient())
            {
                client.BaseAddress = new Uri(_baseUrl + "api/Friend/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Retrieve the token from the HttpContext session
                string token = HttpContext.Session.GetString("AuthToken");

                // Add the bearer token to the request's Authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Fetch the user details of the person who sent the friend request
                HttpResponseMessage response = await client.PostAsJsonAsync("accept", inputEmail);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Friend request accepted!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to accept friend request. Please try again.";
                }
                }
            }
            return View();
        }

        public async Task<IActionResult> DeclineFriendRequest(string inputEmail)
        {
            if (inputEmail != null)
            {
                using (var client = _httpClientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri(_baseUrl + "api/Friend/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string token = HttpContext.Session.GetString("AuthToken");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.PostAsJsonAsync("decline", inputEmail);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Friend request declined!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to decline friend request. Please try again.";
                    }
                }
            }
            return View();
        }

        public async Task<IActionResult> RemoveFriend(string inputEmail)
        {

            if (inputEmail != null)
            {
                using (var client = _httpClientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri(_baseUrl + "api/Friend/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string token = HttpContext.Session.GetString("AuthToken");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.PostAsJsonAsync("removeFriend", inputEmail);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Friend removed!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to remove friend. Please try again.";
                    }
                }
            }

            return View();
        }

    }
}
