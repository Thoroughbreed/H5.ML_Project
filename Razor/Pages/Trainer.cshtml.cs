using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor.Pages;

public class Trainer : PageModel
{
    [BindProperty(SupportsGet = true)] public float score { get; set; }
    [BindProperty(SupportsGet = true)] public string image { get; set; }
    [BindProperty(SupportsGet = true)] public string classificationGuess { get; set; }
    [BindProperty(SupportsGet = true)] public string classificationHuman  { get; set; }
    private static string[] _classifications = { "moped", "car", "bike", "bus", "motorbike", "truck", "person" };
    private static string? _path;
    private HttpClient _client;

    public Trainer(HttpClient client)
    {
        _client = client;
    }
    public async Task<IActionResult> OnGet(int? id)
    {
        if (id != null)
        {
            if (id == 404)
            {
                await _client.PostAsJsonAsync("delete", _path);
            }
            classificationHuman = _classifications[id.Value];
            await _client.PostAsJsonAsync("captcha", new Tuple<string, string?>(classificationHuman,_path));
            return RedirectToPage("Trainer");
        }
        
        try
        {
            var reply = await _client.GetFromJsonAsync<Tuple<string, float, string, string>>("captcha");
            if (reply == null)
            {
                classificationGuess = "No images are waiting to be evaluated by a sentient being like you.";
                return Page();
            }
            classificationGuess = reply.Item1;
            score = reply.Item2;
            image = reply.Item3;
            _path = reply.Item4;
            return Page();
        }
        catch (Exception e)
        {
            classificationGuess = "It kinda looks like the API isn't running ... weird?!";
            return Page();
        }
    } 
}