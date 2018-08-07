using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogEngine.Models;
using Microsoft.AspNetCore.Http;

namespace BlogEngine.Services
{
    public interface IBlogService
    {
        Task<IEnumerable<Post>> GetPosts(int count, int skip = 0);
        Task<IEnumerable<Post>> GetPostByCategory(string category);
        Task<Post> GetPostBySlug(string slug);
        Task<Post> GetPostById(string id);
        Task<IEnumerable<string>> GetCategories();
        Task SavePost(Post post);
        Task DeletePost(Post post);
        Task<string> SaveFile(byte[] bytes, string fileName, string suffic = null);
    }

    public abstract class InMemoryBlogServiceBase : IBlogService
    {
        protected InMemoryBlogServiceBase(IHttpContextAccessor contextAccessor)
        {
            ContextAccessor = contextAccessor;
        }
        protected List<Post> Cache { get; set; }
        protected IHttpContextAccessor ContextAccessor { get; }

        protected bool IsAdmin() => ContextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;

        protected void SortCache() => Cache.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));

        public virtual async Task<IEnumerable<Post>> GetPosts(int count, int skip = 0)
        {
            bool isAdmin = IsAdmin();

            var posts = Cache
                .Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin)) // TODO: What is this??
                .Skip(skip)
                .Take(count);

            return await Task.FromResult(posts);
        }

        public async Task<IEnumerable<Post>> GetPostByCategory(string category)
        {
            bool isAdmin = IsAdmin();

            var posts = Cache
                .Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
                .Where(p => p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));

            return await Task.FromResult(posts);
        }

        public async Task<Post> GetPostBySlug(string slug)
        {
            var post = Cache.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
            bool isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
                return await Task.FromResult(post);

            return await Task.FromResult<Post>(null);
        }

        public async Task<Post> GetPostById(string id)
        {
            var post = Cache.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            bool isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
                return await Task.FromResult(post);

            return await Task.FromResult(post);
        }

        public async Task<IEnumerable<string>> GetCategories()
        {
            bool isAdmin = IsAdmin();

            var categories = Cache
                .Where(p => p.IsPublished || isAdmin)
                .SelectMany(post => post.Categories)
                .Select(cat => cat.ToLowerInvariant())
                .Distinct();

            return await Task.FromResult(categories);
        }

        public abstract Task SavePost(Post post);

        public abstract Task DeletePost(Post post);

        public abstract Task<string> SaveFile(byte[] bytes, string fileName, string suffic = null);
    }
}