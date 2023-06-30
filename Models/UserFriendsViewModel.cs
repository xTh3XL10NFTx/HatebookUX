namespace HatebookUX.Models
{
    public class UserFriendsViewModel
    {
        public IList<UserEntity> Users { get; set; }
        public IList<Friends> Friends { get; set; }
    }
}
