using System.Threading.Tasks;
using WilderMinds.MetaWeblog;

namespace BlogEngine.Services
{
    public class MetaWeblogService : IMetaWeblogProvider
    {
        public async Task<UserInfo> GetUserInfoAsync(string key, string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<BlogInfo[]> GetUsersBlogsAsync(string key, string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Post> GetPostAsync(string postid, string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Post[]> GetRecentPostsAsync(string blogid, string username, string password, int numberOfPosts)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> AddPostAsync(string blogid, string username, string password, Post post, bool publish)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> DeletePostAsync(string key, string postid, string username, string password, bool publish)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> EditPostAsync(string postid, string username, string password, Post post, bool publish)
        {
            throw new System.NotImplementedException();
        }

        public async Task<CategoryInfo[]> GetCategoriesAsync(string blogid, string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<int> AddCategoryAsync(string key, string username, string password, NewCategory category)
        {
            throw new System.NotImplementedException();
        }

        public async Task<MediaObjectInfo> NewMediaObjectAsync(string blogid, string username, string password, MediaObject mediaObject)
        {
            throw new System.NotImplementedException();
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
    }
}