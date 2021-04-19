using System.Text;
using CharacterApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CharacterApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var result = new StringBuilder("<html>");
            result.Append("<head><title>HelloMoon Character API</title></head>");
            result.Append("<body><h1>HelloMoon Character API</h1><br /><table cellspacing='10'>");

            var characters = LocationService.GetCharacters();
            for (var i = 0; i < characters.Count; i++)
                result.Append($"<tr><td>Character #{i+1}</td><td>{characters[i].X},{characters[i].Y}</tr>");
            
            result.Append("</table></body></html>");

            return Content(result.ToString(), "text/html");
        }
    }
}
