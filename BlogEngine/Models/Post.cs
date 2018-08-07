using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BlogEngine.Models
{
    public class Post
    {
        [Required]
        public string Id { get; set; } = DateTime.UtcNow.Ticks.ToString();
        [Required]
        public string Title { get; set; }

        public string Slug { get; set; }
        [Required]
        public string Excerpt { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime PubDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public bool IsPublished { get; set; }
        public IList<string> Categories { get; set; }
        public IList<Comment> Comments { get; set; } = new List<Comment>();

        public string GetLink() => $"/blog/{Slug}";

        public bool AreCommentsOpen(int commentsCloseAfterDays) =>
            PubDate.AddDays(commentsCloseAfterDays) >= DateTime.UtcNow;

        private static string RemoveReservedUrlChars(string text)
        {
            var reservedChars = new List<string>
            {
                "!", "#", "$", "'", "(", ")", "*", ",", "/", ":", ";", "?", "@", "[",
                "]", "\"", "%", ".", "<", ">", "\\", "^", "_", "'", "{", "}", "|", "~",
                "`", "+"
            };

            foreach (var chr in reservedChars)
                text = text.Replace(chr, "");

            return text;
        }

        // TODO: Understand this code.
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var ch in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(ch);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        // TODO: Understand this code.
        public string RenderContent()
        {
            var result = Content;
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Replace(" src=\"",
                    " src=\"data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==\" data-src=\"");

                var video =
                    "<div class=\"video\"><iframe width=\"560\" height=\"315\" title=\"Youtube embed\" src=\"about:blank\" data-src=\"https://www.youtube-nocookie.com/embed/{0}?modestbranding=1&amp;hd=1&amp;rel=0&amp;theme=light\" allowfullscreen></iframe></div>";

                result = Regex.Replace(result, @"\[youtube:(.*?)\]", m => string.Format(video, m.Groups[1].Value));
            }

            return result;
        }

        public static string CreateSlug(string title)
        {
            title = title.ToLowerInvariant().Replace(" ", "-");
            title = RemoveDiacritics(title);
            title = RemoveReservedUrlChars(title);

            return title.ToLowerInvariant();
        }
    }
}