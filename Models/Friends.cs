namespace HatebookUX.Models
{
    public class Friends
    {
        public string? sender { get; set; }
        public string? reciver { get; set; }
        public string? status { get; set; } // Pending, Accepted, Declined
        public string? creatorId { get; set; }
    }
}
