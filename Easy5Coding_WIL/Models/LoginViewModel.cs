using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Firebase.Auth;
using Microsoft.Extensions.Configuration;

namespace Easy5Coding_WIL.Models
{
    public class LoginViewModel
    {
        // Email is required and should follow the correct email format
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        // Password is required and should be entered as a password field
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // RememberMe is optional; it's used to decide whether the user stays signed in
        public bool RememberMe { get; set; }

    }

}
