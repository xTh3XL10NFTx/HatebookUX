using System.ComponentModel.DataAnnotations;

namespace HatebookUX.Models
{
    public class LogIn
    {
        [Required]
        public string? email { get; set; }
        [Required]
        public string? password { get; set; }
    }
}
