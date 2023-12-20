using Microsoft.AspNetCore.Mvc;

namespace WebProject.Controllers;

public class Template : Controller
{
    // GET
    public IActionResult CreateTemplate()
    {
        return View();
    }
    public IActionResult EditTemplate()
    {
        return View();
    }
}