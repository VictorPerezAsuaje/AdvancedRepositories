using AdvancedRepositories.Core.Repositories;
using AdvancedRepositories.Core.Repositories.Fluent;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebClient.Infrastructure;
using WebClient.Models;

namespace WebClient.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    IArticleRepository _articleRepository;
    IFluentRepository _fluentRepo;

    public HomeController(ILogger<HomeController> logger, IArticleRepository articleRepository, IFluentRepository fluentRepository)
    {
        _logger = logger;
        _articleRepository = articleRepository;
        _fluentRepo = fluentRepository;
    }

    [HttpGet]
    public IActionResult Index()
    {
        IndexVM vm = new IndexVM();

        DbResult<List<Article>> articleResult = _articleRepository.GetAll();

        if(articleResult.IsSuccess)
            vm.Articles = articleResult.Value;

        vm.Tags = _fluentRepo.Select<TagDTO>()
            .FromDefaultTable()
            .OrderByDesc("Id")
            .GetList();

        return View(vm);
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

public class IndexVM
{
    public List<Article> Articles { get; set; } = new();
    public List<TagDTO> Tags { get; set; } = new();
}