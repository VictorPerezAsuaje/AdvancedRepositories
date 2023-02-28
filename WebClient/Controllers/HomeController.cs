using AdvancedRepositories.Core.Repositories;
using AdvancedRepositories.Core.Repositories.Fluent;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebClient.Infrastructure;
using WebClient.Models;

namespace WebClient.Controllers;

public class HomeController : Controller
{
    IArticleRepository _articleRepository;
    IFluentRepository _fluentRepo;

    public HomeController(IArticleRepository articleRepository, IFluentRepository fluentRepository)
    {
        _articleRepository = articleRepository;
        _fluentRepo = fluentRepository;
    }

    [HttpGet]
    public IActionResult Index(int searchType = 1, string? title = null)
    {
        IndexVM vm = new IndexVM();
        
        vm.Title = title;
        vm.SearchType = searchType;

        DbResult<List<Article>> articleResult = searchType switch
        {
            1 => _articleRepository.FindMultipleClassic(title),
            2 => _articleRepository.FindMultipleBasic(x => x.ColumnName("Titulo").Like(title)),
            3 => _articleRepository.FindMultipleAdvanced(x => x.ColumnName("Titulo").Like(title)),
            4 => _fluentRepo.Select<Article>()
                        .From("Articulos")
                        .AndWhere(x => x.ColumnName("Titulo").Like(title))
                        .GetList(x =>
                        {
                            x.Add("Id", "Id");
                            x.Add("Title", "Titulo");
                            x.Add("Slug", "Slug");
                            x.Add("CreatedOn", "FechaCreacion");
                        })
        };

        if(articleResult.IsSuccess)
            vm.Articles = articleResult.Value;

        vm.Tags = _fluentRepo.Select<TagDTO>()
            .FromDefaultTable()
            .OrderByDesc("Id")
            .GetList().Value;

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
    public string Title { get; set; }
    public int SearchType { get; set; }
    public List<Article> Articles { get; set; } = new();
    public List<TagDTO> Tags { get; set; } = new();
}