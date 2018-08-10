using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WilderMinds.MetaWeblog;

namespace BlogEngine.Services
{
    public class MetaWeblogService : IMetaWeblogProvider
    {
        private readonly IBlogService _blog;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _context;

        public MetaWeblogService(IBlogService blog, IConfiguration config, IHttpContextAccessor context,
            IUserService userService)
        {
            _blog = blog;
            _config = config;
            _context = context;
            _userService = userService;
        }

        public async Task<UserInfo> GetUserInfoAsync(string key, string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<BlogInfo[]> GetUsersBlogsAsync(string key, string username, string password)
        {
            ValidateUser(username, password);

            var request = _context.HttpContext.Request;
            var url = $"{request.Scheme}://{request.Host}";
        }

        public async Task<Post> GetPostAsync(string postid, string username, string password)
        {
            ValidateUser(username, password);

            var post = _blog.GetPostById(postid).GetAwaiter().GetResult();

            if (post != null)
                return await Task.FromResult(ToMetaWeblogPost(post));

            return null;
        }

        public async Task<Post[]> GetRecentPostsAsync(string blogid, string username, string password, int numberOfPosts)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> AddPostAsync(string blogid, string username, string password, Post post, bool publish)
        {
            ValidateUser(username, password);

            var newPost = new Models.Post
            {
                Title = post.title,
                Slug = !string.IsNullOrWhiteSpace(post.wp_slug) ? post.wp_slug : Models.Post.CreateSlug(post.title),
                Content = post.description,
                IsPublished = publish,
                Categories = post.categories
            };

            if (post.dateCreated != DateTime.MinValue)
                newPost.PubDate = post.dateCreated;

            _blog.SavePost(newPost).GetAwaiter().GetResult();

            return await Task.FromResult(newPost.Id);
        }

        public async Task<bool> DeletePostAsync(string key, string postid, string username, string password, bool publish)
        {
            ValidateUser(username, password);
            var post = _blog.GetPostById(postid).GetAwaiter().GetResult();

            if (post != null)
            {
                _blog.DeletePost(post).GetAwaiter().GetResult();
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        public async Task<bool> EditPostAsync(string postid, string username, string password, Post post, bool publish)
        {
            ValidateUser(username, password);

            var existing = _blog.GetPostById(postid).GetAwaiter().GetResult();
            if (existing != null)
            {
                existing.Title = post.title;
                existing.Slug = post.wp_slug;
                existing.Content = post.description;
                existing.IsPublished = publish;
                existing.Categories = post.categories;

                if (post.dateCreated != DateTime.MinValue)
                    existing.PubDate = post.dateCreated;

                _blog.SavePost(existing).GetAwaiter().GetResult();

                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        public async Task<CategoryInfo[]> GetCategoriesAsync(string blogid, string username, string password)
        {
            ValidateUser(username, password);
            var categories = _blog.GetCategories().GetAwaiter().GetResult()
                .Select(cat =>
                    new CategoryInfo
                    {
                        categoryid = cat,
                        title = cat
                    })
                .ToArray();

            return await Task.FromResult(categories);
        }

        public async Task<int> AddCategoryAsync(string key, string username, string password, NewCategory category)
        {
            ValidateUser(username, password);
            throw new NotImplementedException();
        }

        public async Task<MediaObjectInfo> NewMediaObjectAsync(string blogid, string username, string password, MediaObject mediaObject)
        {
            ValidateUser(username, password);
            byte[] bytes = Convert.FromBase64String(mediaObject.bits);
            string path = _blog.SaveFile(bytes, mediaObject.name).GetAwaiter().GetResult();

            return await Task.FromResult(new MediaObjectInfo {url = path});
        }

        public async Task<Page> GetPageAsync(string blogid, string pageid, string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Page[]> GetPagesAsync(string blogid, string username, string password, int numPages)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Author[]> GetAuthorsAsync(string blogid, string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> AddPageAsync(string blogid, string username, string password, Page page, bool publish)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> EditPageAsync(string blogid, string pageid, string username, string password, Page page, bool publish)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> DeletePageAsync(string blogid, string username, string password, string pageid)
        {
            throw new System.NotImplementedException();
        }

        public void ValidateUser(string username, string password)
        {
            if (_userService.ValidateUser(username, password) == false)
            {
                throw new MetaWeblogException("Unauthorized");
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, username));

            _context.HttpContext.User = new ClaimsPrincipal(identity);
        }

        private Post ToMetaWeblogPost(Models.Post post)
        {
            var request = _context.HttpContext.Request;
            string url = request.Scheme + "://" + request.Host;

            return new Post
            {
                postid = post.Id,
                title = post.Title,
                wp_slug = post.Slug,
                permalink = url + post.GetLink(),
                dateCreated = post.PubDate,
                description = post.Content,
                categories = post.Categories.ToArray()
            };
        }
    }
}