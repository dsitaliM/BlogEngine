using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using BlogEngine.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BlogEngine.Services
{
    public class FileBlogService : IBlogService
    {
        private readonly List<Post> _cache = new List<Post>();
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string _folder;

        public FileBlogService(IHostingEnvironment env, IHttpContextAccessor contextAccessor)
        {
            _folder = Path.Combine(env.WebRootPath, "Posts");
            _contextAccessor = contextAccessor;

            Initialize();
        }

        private void Initialize()
        {
            LoadPosts();
            SortCache();
        }
        public async Task<IEnumerable<Post>> GetPosts(int count, int skip = 0)
        {
            bool isAdmin = IsAdmin();

            var posts = _cache
                .Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
                .Skip(skip)
                .Take(count);

            return await Task.FromResult(posts);
        }

        public async Task<IEnumerable<Post>> GetPostByCategory(string category)
        {
            bool isAdmin = IsAdmin();
            var posts = _cache
                .Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
                .Where(p => p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));

            return await Task.FromResult(posts);
        }

        public async Task<Post> GetPostBySlug(string slug)
        {
            var post = _cache.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
            bool isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
                return await Task.FromResult(post);

            return await Task.FromResult<Post>(null);
        }

        public async Task<Post> GetPostById(string id)
        {
            var post = _cache.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            bool isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
                return await Task.FromResult(post);

            return await Task.FromResult<Post>(null);
        }

        public async Task<IEnumerable<string>> GetCategories()
        {
            bool isAdmin = IsAdmin();

            var categories = _cache
                .Where(p => p.IsPublished || isAdmin)
                .SelectMany(post => post.Categories)
                .Select(cat => cat.ToLowerInvariant())
                .Distinct();

            return await Task.FromResult(categories);
        }

        public async Task SavePost(Post post)
        {
            string filepath = GetFilePath(post);
            post.LastModified = DateTime.UtcNow;
            
            XDocument doc = new XDocument(
                new XElement("post", 
                    new XElement("title", post.Title),
                    new XElement("slug", post.Slug),
                    new XElement("pubDate", post.PubDate.ToString("yyyy-MM-dd HH:mm:ss")),
                    new XElement("lastModified",  post.LastModified.ToString("yyyy-MM-dd HH:mm:ss")),
                    new XElement("excerpt", post.Excerpt),
                    new XElement("content", post.Content),
                    new XElement("isPublished", post.IsPublished),
                    new XElement("categories", string.Empty),
                    new XElement("comments", string.Empty)));

            XElement categories = doc.XPathSelectElement("post/categories");
            foreach(string category in post.Categories)
                categories.Add(new XElement("category", category));

            XElement comments = doc.XPathSelectElement("post/comments");
            foreach (Comment comment in post.Comments)
            {
                comments.Add(
                    new XElement("comment",
                        new XElement("author", comment.Author),
                        new XElement("email", comment.Email),
                        new XElement("date", comment.PubDate.ToString("yyyy-MM-dd HH:mm:ss")),
                        new XAttribute("isAdmin", comment.IsAdmin),
                        new XAttribute("id", comment.Id)));
            }

            using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite))
            {
                await doc.SaveAsync(fs, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);
            }

            if (!_cache.Contains(post))
            {
                _cache.Add(post);
                SortCache();
            }
        }

        public async Task DeletePost(Post post)
        {
            string filePath = GetFilePath(post);

            if (File.Exists(filePath))
                File.Delete(filePath);

            if (_cache.Contains(post))
                _cache.Remove(post);

            await Task.CompletedTask;
        }

        // TODO: Understand this piece.
        public async Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null)
        {
            suffix = suffix ?? DateTime.UtcNow.Ticks.ToString();
            string ext = Path.GetExtension(fileName);
            string name = Path.GetFileNameWithoutExtension(fileName);

            string relative = $"files/{name}_{suffix}{ext}";
            string absolute = Path.Combine(_folder, relative);
            string dir = Path.GetDirectoryName(absolute);

            Directory.CreateDirectory(dir);
            using (var writer = new FileStream(absolute, FileMode.CreateNew))
                await writer.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

            return $"/Posts/{relative}";
        }

        private void LoadPosts()
        {
            if (!Directory.Exists(_folder))
                Directory.CreateDirectory(_folder);

            foreach (string file in Directory.EnumerateFiles(_folder, "*.xml", SearchOption.TopDirectoryOnly))
            {
                XElement doc = XElement.Load(file);

                Post post = new Post
                {
                    Id = Path.GetFileNameWithoutExtension(file),
                    Title = ReadValue(doc, "title"),
                    Excerpt = ReadValue(doc, "content"),
                    Content = ReadValue(doc, "content"),
                    Slug = ReadValue(doc, "slug").ToLowerInvariant(),
                    PubDate = DateTime.Parse(ReadValue(doc, "pubDate")),
                    LastModified = DateTime.Parse(ReadValue(doc, "lastModified",
                        DateTime.Now.ToString(CultureInfo.InvariantCulture))),
                    IsPublished = bool.Parse(ReadValue(doc, "isPublished", "true"))
                };

                LoadCategories(post, doc);
                LoadComments(post, doc);
                _cache.Add(post);
            }
        }

        private void LoadComments(Post post, XElement doc)
        {
            var comments = doc.Element("comments");
            if (comments == null)
                return;

            foreach (var node in comments.Elements("comment"))
            {
                Comment comment = new Comment
                {
                    Id = ReadAttribute(node, "id"),
                    Author = ReadValue(node, "author"),
                    Email = ReadValue(node, "email"),
                    IsAdmin = bool.Parse(ReadAttribute(node, "isAdmin", "false")),
                    Content = ReadValue(node, "content"),
                    PubDate = DateTime.Parse(ReadValue(node, "date", "2000-01-01"))
                };

                post.Comments.Add(comment);
            }

        }

        private static void LoadCategories(Post post, XElement doc)
        {
            XElement categories = doc.Element("categories");
            if (categories == null)
                return;

            post.Categories = categories.Elements("category").Select(node => node.Value).ToArray();
        }

        private static string ReadValue(XElement doc, XName name, string defaultValue = "")
        {
            return doc.Element(name) != null ? doc.Element(name)?.Value : defaultValue;
        }

        private static string ReadAttribute(XElement element, XName name, string defaultValue = "")
        {
            return element.Attribute(name) != null ? element.Attribute(name)?.Value : defaultValue;
        }


        private string GetFilePath(Post post) => Path.Combine(_folder, $"{post.Id}.xml");

        protected void SortCache()
        {
            _cache.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
        }

        protected bool IsAdmin() => _contextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;
    }
}