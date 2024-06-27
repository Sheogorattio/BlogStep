using Microsoft.AspNetCore.Mvc;

namespace Blog.ViewComponents
{
    public class SubscriberViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("Subscriber");
        }
    }
}
