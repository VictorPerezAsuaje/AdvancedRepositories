using AdvancedRepositories.Core.Repositories;
using AdvancedRepositories.Core.Repositories.Fluent;
using Microsoft.AspNetCore.Mvc;
using WebClient.Infrastructure;
using WebClient.Models;

namespace WebClient.Controllers;

public class HomeController : Controller
{
    IArticleRepository _articleRepository;
    IArticleTypedRepository _articleTypedRepository;
    FluentRepository _fluentRepo;

    public HomeController(IArticleRepository articleRepository, FluentRepository fluentRepository, IArticleTypedRepository articleTypedRepository)
    {
        _articleRepository = articleRepository;
        _fluentRepo = fluentRepository;
        _articleTypedRepository = articleTypedRepository;
    }

    [HttpGet]
    public IActionResult Index(int searchType = 1, string? title = null)
    {
        IndexVM vm = new IndexVM();
        
        vm.Title = title;
        vm.SearchType = searchType;

        DbResult<List<Article>> articleResult = searchType switch
        {
            1 => _articleRepository.FindClassic(title),
            2 => _articleRepository.FindAdvanced(x => x.ColumnName("Titulo").Like(title)),
            3 => _fluentRepo.Select<Article>()
                        .From("Articulos")
                        .Where(x => x.ColumnName("Titulo").Like(title))
                        .GetList(x =>
                        {
                            x.Add("Id", "Id");
                            x.Add("Title", "Titulo");
                            x.Add("Slug", "Slug");
                            x.Add("CreatedOn", "FechaCreacion");
                        }),
            4 => _articleTypedRepository.FindAdvanced(x => x.ColumnName("Titulo").Like(title))
        };

        if(articleResult.IsSuccess)
            vm.Articles = articleResult.Value;

        return View(vm);
    }

    [HttpPost]
    public IActionResult Index(Article article)
    {
        DbResult result = null;
        bool insert = false;

        if(insert)
            result = _articleRepository.Insert(article);
        else
            result = _articleRepository.Update(article, 10);

        return RedirectToAction("Index");
    }
}

public class IndexVM
{
    public string Title { get; set; }
    public int SearchType { get; set; }
    public List<Article> Articles { get; set; } = new();
}