using System.ComponentModel.DataAnnotations;

namespace Entities.DataTransferObjects
{
    public class UserForAuthenticatingDto
    {
        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password name is required")]
        public string Password { get; set; }
    }
}
