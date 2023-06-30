using HatebookUX.Models;
using Hatebook.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Net.Http;
using System.Net.Http.Json;

namespace HatebookUX.Controllers
{
    public class GroupController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _accountEndpoint;
        private readonly string _groupsEndpoint;
        private readonly string _otherEndpoint;
        private readonly ILogger<UserAccountController> _logger;
        //
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GroupController(IConfiguration configuration, ILogger<UserAccountController> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _baseUrl = _configuration["ApiEndpoints:BaseUrl"];
            _accountEndpoint = _configuration["ApiEndpoints:AccountEndpoint"];
            _groupsEndpoint = _configuration["ApiEndpoints:GroupsEndpoint"];
            _otherEndpoint = _configuration["ApiEndpoints:OtherEndpoint"];
            _logger = logger;

            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        //2 get-> all and by name
        //1 post -> create a group
        //1 put -> update group info
        //1 delet -> delete a group


        public async Task<IActionResult> GetGroup()
        {
            IList<Groups> user = new List<Groups>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl + "api/Groups/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Retrieve the token from the HttpContext session
                string token = HttpContext.Session.GetString("AuthToken");

                // Add the bearer token to the request's Authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage getData = await client.GetAsync("get");

                if (getData.IsSuccessStatusCode)
                {
                    string results = getData.Content.ReadAsStringAsync().Result;
                    user = JsonConvert.DeserializeObject<IList<Groups>>(results);
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid email or password. Please try again.";
                }

                ViewData.Model = user;
            }

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public IActionResult CreateGroup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(Groups groups)
        {
            // Get the ClaimsPrincipal object representing the current user
            ClaimsPrincipal currentUser = HttpContext.User;

            // Retrieve the email claim
            Claim creatorClaim = currentUser.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            if (creatorClaim != null)
            {
                string CreatorId = creatorClaim.Value;
                groups.CreatorId = creatorClaim.Value;
            }
            else
            {
                return View(groups);
            }


            using (var client = _httpClientFactory.CreateClient())
            {
                client.BaseAddress = new Uri(_baseUrl + "api/Groups/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Retrieve the token from the HttpContext session
                string token = HttpContext.Session.GetString("AuthToken");

                // Add the bearer token to the request's Authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await client.PostAsJsonAsync("CreateGroup", groups);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Group created!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create group. Please try again.";
                }
            }

            return View(groups);
        }

        public async Task<IActionResult> EditGroup(Groups group)
        {

            // Get the ClaimsPrincipal object representing the current user
            ClaimsPrincipal currentUser = HttpContext.User;

            // Retrieve the email claim
            Claim creatorClaim = currentUser.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            if (creatorClaim != null)
            {
                string CreatorId = creatorClaim.Value;
                group.CreatorId = creatorClaim.Value;
            }
            else
            {
                return View(group);
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl + "api/Groups/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Retrieve the token from the HttpContext session
                string token = HttpContext.Session.GetString("AuthToken");

                // Add the bearer token to the request's Authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage edit = await client.PutAsJsonAsync("editGroup/" + group.name, group);

                if (edit.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Changes successful!";
                    return RedirectToAction("GetGroup", "Group");
                }
                else
                {
                    TempData["ErrorMessage"] = "Changes failed!";
                }
            }

            return View(group);
        }

    }
}
