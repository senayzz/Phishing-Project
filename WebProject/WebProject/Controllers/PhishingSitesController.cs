using Microsoft.AspNetCore.Mvc;
using WebProject.Models;

namespace WebProject.Controllers;

public class PhishingSitesController : Controller
{
    
    public IActionResult Google()
    {
        return View();
    }
    public IActionResult GooglePassword()
    {
        return View();
    }
    public IActionResult GoogleCard()
    {
        return View();
    }
    public IActionResult Netflix()
    {
        return View();
    }
    public IActionResult NetflixCard()
    {
        return View();
    }
    public IActionResult Epic()
    {
        
        return View();
    }
    public IActionResult EpicCard()
    {
        return View();
    }
    public IActionResult Error()
    {
        return View();
    }
    
}