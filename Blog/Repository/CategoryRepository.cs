using Blog.Data;
using Blog.Interfaces;
using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Repository
{
    public class CategoryRepository : ICategory
    {
        private readonly ApplicationContext _applicationContext;


        public CategoryRepository(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }


        public async Task AddCategoryAsync(Category category)
        {
            _applicationContext.Categories.Add(category);
            await _applicationContext.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            _applicationContext.Categories.Remove(category);
            await _applicationContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _applicationContext.Categories.ToListAsync();
        }

        public async Task<Category> GetCategoryAsync(string id)
        {
            return await _applicationContext.Categories.FirstOrDefaultAsync(e => e.Id.ToString() == id);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _applicationContext.Categories.Update(category);
            await _applicationContext.SaveChangesAsync();
        }
    }
}
