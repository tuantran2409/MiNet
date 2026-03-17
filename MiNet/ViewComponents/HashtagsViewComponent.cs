using Microsoft.AspNetCore.Mvc;

namespace MiNet.ViewComponents
{
    public class HashtagsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
