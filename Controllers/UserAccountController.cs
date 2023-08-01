using HatebookUX.Models;
using HatebookUX.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace HatebookUX.Controllers
{
    public class UserAccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _accountEndpoint;
        private readonly string _otherEndpoint;
        private readonly ILogger<UserAccountController> _logger;
        private readonly ApiService _apiService;

        public UserAccountController(IConfiguration configuration, ILogger<UserAccountController> logger, ApiService apiService)
        {
            _configuration = configuration;
            _baseUrl = _configuration["ApiEndpoints:BaseUrl"];
            _accountEndpoint = _configuration["ApiEndpoints:AccountEndpoint"];
            _otherEndpoint = _configuration["ApiEndpoints:OtherEndpoint"];
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<ActionResult<String>> LogIn(LogIn account)
        {
            if (account.email != null)
            {
                var response = await _apiService.MakeRequestAsync("post", "Account", "log-in", account);

                if (response.IsSuccessStatusCode)
                {

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var token = JObject.Parse(responseContent)["token"].ToString();

                    // Store the user claims in session
                    HttpContext.Session.SetString("AuthToken", token);

                    // Get the user claims from the token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var parsedToken = tokenHandler.ReadJwtToken(token);
                    var emailClaim = parsedToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                    // Get the profile picture path from the user model
                    var getUserDetails = await GetUserDetails(emailClaim);
                    var profilePicturePath = getUserDetails.profilePicture;
                    var defaultProfilePicturePath = getUserDetails.DefaultProfilePicture;

                    HttpContext.Session.SetString("Email", emailClaim);
                    if (profilePicturePath != null)
                    {
                        HttpContext.Session.SetString("ProfilePicture", profilePicturePath);
                    }
                    else
                    {
                        HttpContext.Session.SetString("ProfilePicture", defaultProfilePicturePath);
                    }

                    // Create the claims for the authenticated user
                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, emailClaim),
                            // Add additional claims if needed
                        };

                    // Create the identity for the authenticated user
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Sign in the user
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    ViewBag.Email = emailClaim;

                    TempData["SuccessMessage"] = "Login successful! Welcome.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid email or password. Please try again.";
                }
            }

            return View();
        }
        public IActionResult Register()
        {
            // Create a list of roles
            var roles = new List<Role>
            {
                new Role { Name = "User" },
                new Role { Name = "Administrator" }
            };

            // Create a new Register model and assign the roles
            var registerModel = new Register
            {
                Roles = roles
            };

            return View(registerModel);
        }

        [HttpPost]
        public async Task<IActionResult> Register(Register registerRequest, IFormFile profilePictureFile)
        {
            // Populate roles options again
            registerRequest.Roles = GetRoles();

            if (registerRequest.email != null)
            {
                // Set the roles based on the selected role
                registerRequest.Roles = new List<Role> { new Role { Name = registerRequest.SelectedRole } };

                // Profile picture handling
                if (registerRequest.ProfilePictureFile != null && registerRequest.ProfilePictureFile.Length > 0)
                {
                    // Save the uploaded profile picture file to a location (e.g., server folder or cloud storage)
                    var filePath = "path/to/save/profile/picture.jpg";
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await registerRequest.ProfilePictureFile.CopyToAsync(stream);
                    }

                    // Update the profilePicture property with the file path
                    registerRequest.profilePicture = filePath;
                }
                else if (!string.IsNullOrEmpty(registerRequest.ProfilePictureUrl))
                {
                    // Use the provided URL as the profile picture
                    registerRequest.profilePicture = registerRequest.ProfilePictureUrl;
                }

                // Map the selected gender string to the corresponding GenderType enum value
                if (registerRequest.gender == GenderType.Male)
                {
                    registerRequest.genderType = "Male";
                }
                else if (registerRequest.gender == GenderType.Female)
                {
                    registerRequest.genderType = "Female";
                }
                else
                {
                    registerRequest.genderType = "Unknown";
                }

                if (ValidateFields(registerRequest))
                {
                    ValidateFields(registerRequest);
                    return View(registerRequest);
                }

                var response = await _apiService.MakeRequestAsync("post", "Account", "Register", registerRequest);


                if (response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "";
                    TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to register. Please enter a valid email adderess.";
                }
            }
            return View(registerRequest);
        }

        [Authorize]
        public async Task<IActionResult> EditUser(Register registerRequest)
        {
            TempData["SuccessMessage"] = "";
            TempData["ErrorMessage"] = "";
            // Initialize the Roles property with an empty list if it is null
            registerRequest.Roles ??= new List<Role>();
            // Get the ClaimsPrincipal object representing the current user
            ClaimsPrincipal currentUser = HttpContext.User;

            // Retrieve the email claim
            Claim emailClaim = currentUser.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            if (emailClaim != null)
            {
                string email = emailClaim.Value;
                registerRequest.email = emailClaim.Value;
            }
            else
            {
                return View(registerRequest);
            }
            // Profile picture handling
            if (registerRequest.ProfilePictureFile != null && registerRequest.ProfilePictureFile.Length > 0)
            {
                // Save the uploaded profile picture file to a location (e.g., server folder or cloud storage)
                var filePath = "path/to/save/profile/picture.jpg";
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await registerRequest.ProfilePictureFile.CopyToAsync(stream);
                }

                // Update the profilePicture property with the file path
                registerRequest.profilePicture = filePath;
            }
            else if (!string.IsNullOrEmpty(registerRequest.ProfilePictureUrl))
            {
                // Use the provided URL as the profile picture
                registerRequest.profilePicture = registerRequest.ProfilePictureUrl;
            }

            // Map the selected gender string to the corresponding GenderType enum value
            if (registerRequest.gender == GenderType.Male)
            {
                registerRequest.genderType = "Male";
            }
            else if (registerRequest.gender == GenderType.Female)
            {
                registerRequest.genderType = "Female";
            }
            else if (registerRequest.gender == GenderType.Unknown)
            {
                registerRequest.genderType = "Unknown";
            }

            if (registerRequest.firstName != null || registerRequest.lastName != null ||
                registerRequest.birthday != null || registerRequest.profilePicture != null)
            {
                TempData["SuccessMessage"] = "Changes successful";
            }

            // Create a LogIn object with the email and password for password verification
            LogIn passwordVerification = new LogIn
            {
                email = emailClaim.Value,
                password = registerRequest.password
            };

            var response = await _apiService.MakeRequestAsync("put", "Account", "edit", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Changes successful!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Register userDetails = await GetUserDetails(emailClaim.Value);
                // Map the selected gender string to the corresponding GenderType enum value
                if (userDetails.genderType == "0")
                {
                    userDetails.genderType = "Male";
                    userDetails.gender = GenderType.Male;
                }
                else if (userDetails.genderType == "1")
                {
                    userDetails.genderType = "Female";
                    userDetails.gender = GenderType.Female;
                }
                else
                {
                    userDetails.genderType = "Unknown";
                    userDetails.gender = GenderType.Unknown;
                }
                if (userDetails != null)
                {
                    return View(userDetails);
                }
                TempData["ErrorMessage"] = "Changes failed!";
                ModelState.AddModelError("passwordVerification", "Incorrect password");
                return View(registerRequest);
            }
        }

        public async Task<IActionResult> Users()
        {
            var viewModel = new UserFriendsViewModel();
            IList<UserEntity> user = new List<UserEntity>();

            using (var client = new HttpClient())
            {
                // Retrieve users
                IList<UserEntity> users = new List<UserEntity>();
                client.BaseAddress = new Uri(_baseUrl + "api/Account/");
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
                    user = JsonConvert.DeserializeObject<IList<UserEntity>>(results);
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid email or password. Please try again.";
                }
                viewModel.Users = user;

                // Retrieve friends
                IList<Friends> friends = new List<Friends>();

                var response = await _apiService.MakeRequestAsync("get", "Account", "get", user);

                if (response.IsSuccessStatusCode)
                {
                    string results = response.Content.ReadAsStringAsync().Result;
                    friends = JsonConvert.DeserializeObject<IList<Friends>>(results);

                    viewModel.Friends = friends;
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid email or password. Please try again.";
                }
            }

            ViewData.Model = viewModel;
            return View();
        }

        private async Task<Register?> GetUserDetails(string email)
        {
            var response = await _apiService.MakeRequestAsync("get", "Account", "get/" + email);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize the response content to get the user details
                Register userDetails = await response.Content.ReadAsAsync<Register>();

                return userDetails;
            }
            else
            {
                return null;
            }
        }

        public async Task<ActionResult<PasswordVerificationResponse>> PasswordVerification(LogIn account)
        {
            var response = await _apiService.MakeRequestAsync("post", "Account", "log-in", account);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var token = JObject.Parse(responseContent)["token"].ToString();

                // Store the user claims in session
                HttpContext.Session.SetString("AuthToken", token);

                // Get the user claims from the token
                var tokenHandler = new JwtSecurityTokenHandler();
                var parsedToken = tokenHandler.ReadJwtToken(token);
                var emailClaim = parsedToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                HttpContext.Session.SetString("Email", emailClaim);

                // Create the claims for the authenticated user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, emailClaim),
                    // Add additional claims if needed
                };

                // Create the identity for the authenticated user
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Sign in the user
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                ViewBag.Email = emailClaim;

                return new PasswordVerificationResponse { Success = true, Password = account.password };
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid email or password. Please try again.";
            }

            return new PasswordVerificationResponse
            {
                Success = false,
                Password = account.password
            };
        }

        public class PasswordVerificationResponse
        {
            public bool Success { get; set; }
            public string Password { get; set; }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); // Perform sign out
            return RedirectToAction("Index", "Home"); // Redirect to the desired page after logout
        }

        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            // Get the ClaimsPrincipal object representing the current user
            ClaimsPrincipal currentUser = HttpContext.User;
            // Retrieve the email claim
            var emailClaim = currentUser.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");

            if (emailClaim != null)
            {
                var response = await _apiService.MakeRequestAsync("delete", "Account", "delete");

                if (response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Account deletion successful!";

                    Logout();

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Deletion unsuccessfull.";
                }
            }
            else TempData["ErrorMessage"] = "No assotiated account.";

            return RedirectToAction("Index", "Home");
        }

        private List<Role> GetRoles()
        {
            return new List<Role>
    {
        new Role { Name = "User" },
        new Role { Name = "Administrator" }
    };
        }

        private bool ValidateFields(Register registerRequest)
        {
            // Check for specific validation errors and set corresponding error messages
            List<string> errorMessages = new List<string>();

            if (string.IsNullOrEmpty(registerRequest.email))
            {
                errorMessages.Add("Please enter an email address.");
            }

            if (string.IsNullOrEmpty(registerRequest.password))
            {
                errorMessages.Add("Please enter a password.");
            }

            if (string.IsNullOrEmpty(registerRequest.firstName))
            {
                errorMessages.Add("Please enter your first name.");
            }

            if (string.IsNullOrEmpty(registerRequest.lastName))
            {
                errorMessages.Add("Please enter your last name.");
            }

            if (registerRequest.birthday == null)
            {
                errorMessages.Add("Please enter your birthday.");
            }

            // Check if the birthday is after today
            if (registerRequest.birthday >= DateTime.Today)
            {
                errorMessages.Add("Please enter a valid birthday date.");
            }

            if (string.IsNullOrEmpty(registerRequest.genderType))
            {
                errorMessages.Add("Please select a gender.");
            }

            if (errorMessages.Count > 0)
            {
                // Concatenate error messages into an HTML bulleted list
                StringBuilder errorMessageBuilder = new StringBuilder("<ul>");
                foreach (var errorMessage in errorMessages)
                {
                    errorMessageBuilder.AppendLine("<li>" + errorMessage + "</li>");
                }
                errorMessageBuilder.AppendLine("</ul>");

                TempData["ErrorMessage"] = errorMessageBuilder.ToString();

                return true;
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to register. Please enter a valid email adderess.";
                return false;
            }

        }
    }
}