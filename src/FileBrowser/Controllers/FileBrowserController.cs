using Microsoft.AspNetCore.Mvc;

namespace FileBrowser.Controllers
{
    public class FileBrowserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
