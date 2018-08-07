using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlogEngine.Models
{
    public class Comment
    {
        [Required]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string Author { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime PubDate { get; set; }
        public bool IsAdmin { get; set; }

        public string GetGravatar()
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(Email.Trim().ToLowerInvariant());
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var t in hashBytes)
                {
                    sb.Append(t.ToString("X2"));
                }

                return $"https://www.gravatar.com/avatar/{sb.ToString().ToLowerInvariant()}?s=60&d=blank";
            }
        }

        public string RenderContent => Content;

    }
}