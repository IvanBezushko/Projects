using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Natsukasiy_Web.Pages.Shared
{
    public class loginModel : PageModel
    {
        [BindProperty]
        public LoginInput Input { get; set; } = new LoginInput();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var registrationData = new List<RegisterInput>();
            if (System.IO.File.Exists("registrationData.json"))
            {
                var existingJson = await System.IO.File.ReadAllTextAsync("registrationData.json");
                registrationData = JsonSerializer.Deserialize<List<RegisterInput>>(existingJson);
            }

            var user = registrationData.Find(u => u.Email == Input.Email && u.Password == Input.Password);
            if (user != null)
            {
                return RedirectToPage("/Privacy");
            }
            else
            {
                return RedirectToPage("/Shared/registration");
            }
        }
    }

    public class LoginInput
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
