using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor.Pages;

public class IndexModel : PageModel
{
    public string responseString { get; set; }
    public string responseScore { get; set; }
    public string b64Img { get; set; }
    public bool errorInApi { get; set; }
    [BindProperty(SupportsGet = true)] public IFormFile Image { get; set; }
    
    
    private HttpClient _client;

    public IndexModel(HttpClient client)
    {
        _client = client;
    }

    public async Task<IActionResult> OnGet()
    {
        responseString = await _client.GetStringAsync("/ctor");
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        using (MemoryStream m = new MemoryStream())
        {
            await Image.OpenReadStream().CopyToAsync(m);
            b64Img = Convert.ToBase64String(m.ToArray());
        }
        return Page();
    }
}