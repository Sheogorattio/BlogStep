﻿using Blog.Data;
using Blog.Interfaces;
using Blog.Models;
using Blog.Models.Pages;
using Microsoft.EntityFrameworkCore;

namespace Blog.Repository
{
    public class PublicationRepository : IPublication
    {
        private readonly ApplicationContext _applicationContext;


        public PublicationRepository(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }


        public async Task AddPublicationAsync(Publication publication)
        {
            if (publication.Categories.Count > 0)
            {
                var categoriesId = publication.Categories.Select(e => e.Id).ToArray();
                var allCategories = _applicationContext.Categories.Where(e => categoriesId.Contains(e.Id)).ToList();    

                publication.Categories = allCategories;
            }

            _applicationContext.Publications.Add(publication);
            await _applicationContext.SaveChangesAsync();
        }

        public async Task DeletePublicationAsync(Publication publication)
        {
            _applicationContext.Publications.Remove(publication);
            await _applicationContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Publication>> GetAllPublicationsAsync()
        {
            return await _applicationContext.Publications.ToListAsync();
        }

        public async Task<PagedList<Publication>> GetAllPublicationsByCategoryWithCategories(QueryOptions options, string id)
        {
            var category = await _applicationContext.Categories.FirstOrDefaultAsync(e => e.Id.ToString() == id);
            if (category is not null)
            {
                return new PagedList<Publication>(_applicationContext.Publications.Include(e => e.Categories).Where(e => e.Categories.Contains(category)), options);
            }
            else
            {
                return new PagedList<Publication>(_applicationContext.Publications.Include(e => e.Categories), options);
            }
        }

        public PagedList<Publication> GetAllPublicationsWithCategories(QueryOptions options)
        {
            return new PagedList<Publication>(_applicationContext.Publications.Include(e => e.Categories), options);
        }

        public async Task<IEnumerable<Publication>> GetFourRandomPublicationsAsync(string id)
        {
            return await _applicationContext.Publications.Where(e => e.Id.ToString() != id).OrderBy(e => Guid.NewGuid()).Take(4).ToListAsync();
        }

        public async Task<Publication> GetPublicationAsync(string id)
        {
            return await _applicationContext.Publications.FirstOrDefaultAsync(e => e.Id.ToString() == id);
        }

        public async Task<Publication> GetPublicationWithCategoriesAsync(string id)
        {
            return await _applicationContext.Publications.Include(e => e.Categories).FirstOrDefaultAsync(e => e.Id.ToString() == id);
        }

        public async Task UpdatePublicationAsync(Publication publication)
        {
            var categoriesId = publication.Categories.Select(e => e.Id).ToArray();
            var allCategories = _applicationContext.Categories.Where(e => categoriesId.Contains(e.Id)).ToList();

            var currentPublication = await GetPublicationWithCategoriesAsync(publication.Id.ToString());
            currentPublication.Title = publication.Title;
            currentPublication.Description = publication.Description;
            currentPublication.Categories = allCategories;
            currentPublication.SeoDescription = publication.SeoDescription;
            currentPublication.Keywords = publication.Keywords;
            currentPublication.FullImageName = publication.FullImageName;
            currentPublication.Image = publication.Image;

            _applicationContext.Publications.Update(currentPublication);
            await _applicationContext.SaveChangesAsync();
        }

        public async Task UpdateViewsAsync(string id)
        {
            await _applicationContext.Database.ExecuteSqlRawAsync($"UPDATE [Publications] SET [TotalViews] += 1 WHERE Id = '{id}'");
        }
    }
}
