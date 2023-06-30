using System.ComponentModel.DataAnnotations;

namespace HatebookUX.Models
{
    public class Message
    {
        [Required]
        public string? email { get; set; }
        [Required]
        public string? message { get; set; }
    }
}
