using Blog.Interfaces;
using Blog.Models;
using Blog.Models.Pages;
using Blog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICategory _categories;
        private readonly IPublication _publications;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly ISubscriber _subscribers;


        public HomeController(ICategory categories, IPublication publications, IWebHostEnvironment appEnvironment, ISubscriber subscribers)
        {
            _categories = categories;
            _publications = publications;
            _appEnvironment = appEnvironment;
            _subscribers = subscribers;
        }


        public async Task<IActionResult> Index(QueryOptions? options, string? categoryId)
        {
            var allCategories = await _categories.GetAllCategoriesAsync();
            var allPublications = await _publications.GetAllPublicationsByCategoryWithCategories(options, categoryId);

            return View(new IndexViewModel
            {
                Categories = allCategories.ToList(),
                Publications = allPublications,
            });
        }


        [Route("publication")]
        public async Task<IActionResult> GetPublication(string? id, string? returnUrl)
        {
            var currentPublication = await _publications.GetPublicationWithCategoriesAsync(id);
            if (currentPublication != null)
            {
                await _publications.UpdateViewsAsync(currentPublication.Id.ToString());
                return View(new GetPublicationViewModel
                {
                    Publication = currentPublication,
                    ReturnUrl = returnUrl
                });
            }
            return NotFound();
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Subscribe(string email)
        {
            if (!await _subscribers.IsSubscribed(email))
            {
                await _subscribers.Subscribe(new Subscriber
                {
                    Email = email
                });

                return Content("Подписка успешно оформлена!");
            }
            else
            {
                return Content("Вы уже оформили подписку!");
            }
        }
    }
}
