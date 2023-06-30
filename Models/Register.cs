using System.ComponentModel.DataAnnotations;

namespace HatebookUX.Models
{
    public class Register
    {
        public string? email { get; set; }
        public string? password { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public DateTime? birthday { get; set; }
        public GenderType gender { get; set; }
        public string? genderType { get; set; }
        public IFormFile ProfilePictureFile { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string? profilePicture { get; set; }
        public string DefaultProfilePicture { get; set; } = "https://1fid.com/wp-content/uploads/2022/06/no-profile-picture-13-1024x1024.jpg";
        public List<Role> Roles { get; set; }
        public string SelectedRole { get; set; }

        public Register()
        {
            Roles = new List<Role>();
        }
    }

    public class Role
    {
        public string Name { get; set; }
    }

    public enum GenderType
    {
        Unknown,
        Male,
        Female
    }

}
