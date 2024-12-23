namespace Zcrypta.Entities.Models
{
    public class UserModel
    {
        public UserModel()
        {
            UserRoles = new List<UserRoleModel>();
        }
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public virtual ICollection<UserRoleModel> UserRoles { get; set; }
    }
}
