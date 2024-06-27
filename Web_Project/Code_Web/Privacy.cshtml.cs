using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Natsukasiy_Web.Pages
{
    public class PrivacyModel : PageModel
    {
        public readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        
        public void OnGet()
        {
            
        }
    }

}
