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
        //List<TagDTO> list = _fluentRepository.Select<TagDTO>()
        //    .FromDefaultTable()
        //    .OrderByDesc("Name")
        //    .GetList().Value;

        List<TagDTO> unorderedList = _fluentRepository.AutoList<TagDTO>().Value;

        return View(unorderedList);
    }
}
