using AdvancedRepositories.Core.Repositories;
using AdvancedRepositories.Core.Repositories.Fluent;
using Microsoft.AspNetCore.Mvc;
using WebClient.Models;

namespace WebClient.Views.Home.Components.TagSelect;

public class TagSelectViewComponent : ViewComponent
{
    FluentRepository _fluentRepository;
	public TagSelectViewComponent(FluentRepository fluentRepository)
	{
		_fluentRepository= fluentRepository;
	}

    public async Task<IViewComponentResult> InvokeAsync()
    {
        //DbResult<List<TagDTO>> result = _fluentRepository.Select<TagDTO>()
        //    .FromDefaultTable()
        //    .OrderByDesc("Name")
        //    .GetList(x => x.Add("Name", "Nombre"));

        DbResult<List<TagDTO>> result = _fluentRepository.AutoList<TagDTO>();

        if (result.IsFailure) return View(new List<TagDTO>());

        return View(result.Value);
    }
}
