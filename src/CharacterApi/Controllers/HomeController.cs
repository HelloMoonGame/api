using CharacterApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CharacterApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var result = "<html>" +
                "<head><title>HelloMoon Character API</title></head>" +
                "<body><h1>HelloMoon Character API</h1><br /><table cellspacing='10'>";
            for (var i = 0; i < LocationService.Characters.Count; i++)
                result += $"<tr><td>Character #{i+1}</td><td>{LocationService.Characters[i].X},{LocationService.Characters[i].Y}</tr>";
            result += "</table></body></html>";

            return Content(result, "text/html");
        }
    }
}
