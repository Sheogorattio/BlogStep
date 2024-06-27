using Blog.Models;
using Blog.Models.Pages;

namespace Blog.Interfaces
{
    public interface IPublication
    {
        Task<IEnumerable<Publication>> GetFourRandomPublicationsAsync(string id);
        Task<PagedList<Publication>> GetAllPublicationsByCategoryWithCategories(QueryOptions options, string id);
        Task<IEnumerable<Publication>> GetAllPublicationsAsync();
        PagedList<Publication> GetAllPublicationsWithCategories(QueryOptions options);
        Task<Publication> GetPublicationAsync(string id);
        Task<Publication> GetPublicationWithCategoriesAsync(string id);

        Task UpdateViewsAsync(string id);

        Task AddPublicationAsync(Publication publication);
        Task UpdatePublicationAsync(Publication publication);
        Task DeletePublicationAsync(Publication publication);
    }
}
