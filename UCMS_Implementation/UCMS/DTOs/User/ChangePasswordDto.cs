namespace UCMS.DTOs.User
{
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPasswrod { get; set; }
    }
}
