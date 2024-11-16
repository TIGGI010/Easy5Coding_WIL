using System.ComponentModel.DataAnnotations;

namespace Easy5Coding_WIL.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Employee|HRManager|Manager)$", ErrorMessage = "Invalid role. Please select a valid role.")]
        public string Role { get; set; } // Expected values: "Employee", "HRManager", or "Manager"
    }
}
