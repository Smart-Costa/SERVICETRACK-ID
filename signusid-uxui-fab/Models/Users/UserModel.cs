namespace AspnetCoreMvcFull.Models.Users
{
  public class UserModel
  {
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreationDate { get; set; }
    public string Password { get; set; } = "";
    public string RolName { get; set; } = "";
    public Guid RolId { get; set; }
    public Guid UserSysId { get; set; }
    public bool isActive { get; set; }
  }
}
