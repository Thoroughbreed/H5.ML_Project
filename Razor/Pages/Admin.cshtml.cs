using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor.Pages;

public class Admin : PageModel
{
    [BindProperty(SupportsGet = true)] public string Password { get; set; }
    [BindProperty(SupportsGet = true)] public string Response { get; set; }

    private HttpClient _client;

    public Admin(HttpClient client)
    {
        _client = client;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (Password != "potato")
        {
            Response = "Wrong password Nicolai!";
            return Page();
        }

        Response = "Password accepted ...";
        var res = await _client.GetAsync("/retrain");
        Response = res.StatusCode.ToString();
        var resCode = await res.Content.ReadAsStringAsync();
        Response = resCode;
        return Page();
    }
}