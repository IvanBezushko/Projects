using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Natsukasiy_Web.Pages.Shared
{
    public class registrationModel : PageModel
    {
        [BindProperty]
        public RegisterInput Input { get; set; } = new RegisterInput();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var registrationData = Input;
            var json = JsonSerializer.Serialize(registrationData, new JsonSerializerOptions { WriteIndented = true });
            var existingData = new List<RegisterInput>();

            if (System.IO.File.Exists("registrationData.json"))
            {
                var existingJson = await System.IO.File.ReadAllTextAsync("registrationData.json");
                existingData = JsonSerializer.Deserialize<List<RegisterInput>>(existingJson);
            }

            existingData.Add(registrationData);
            var updatedJson = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync("registrationData.json", updatedJson);

            return RedirectToPage("/Privacy");
        }
    }

    public class RegisterInput
    {
        [Required]
        public string UserNick { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Has³a nie s¹ zgodne.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
