using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebClient.Infrastructure;
using WebClient.Models;

namespace WebClient.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    IArticleRepository _articleRepository;

    public HomeController(ILogger<HomeController> logger, IArticleRepository articleRepository)
    {
        _logger = logger;
        _articleRepository = articleRepository;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var list = _articleRepository.GetAll();
        return View(list.Value);
    }

    [HttpPost]
    public IActionResult Index(Article article)
    {
        var result = _articleRepository.Insert(article);
        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}