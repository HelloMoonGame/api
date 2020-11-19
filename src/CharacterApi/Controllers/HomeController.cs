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
                "<body><h1>HelloMoon Character API</h1><br /><table>";
            foreach (var character in LocationService.Characters)
                result += $"<tr><td>{character.Guid}</td><td>{character.X},{character.Y}</tr>";
            result += "</table></body></html>";

            return Content(result, "text/html");
        }
    }
}
