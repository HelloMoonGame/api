using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Character.Api.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var result = new StringBuilder("<html>");
            result.Append("<head><title>HelloMoon Character API</title></head>");
            result.Append("<body><h1>HelloMoon Character API</h1>");
            result.Append("</body></html>");

            return Content(result.ToString(), "text/html");
        }
    }
}
