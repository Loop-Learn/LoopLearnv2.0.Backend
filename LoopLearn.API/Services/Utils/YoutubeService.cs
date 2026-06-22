using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using LoopLearn.Entities.Helpers.Models;
using Microsoft.Extensions.Options;
using System.Xml;

namespace LoopLearn.API.Services.Utils
{
    public class YoutubeService
    {
        private readonly string _token;

        public YoutubeService(IOptions<Youtube> token)
        {
            _token = token?.Value.token ?? throw new ArgumentNullException(nameof(token));
        }

        public async Task<TimeSpan?> GetYouTubeVideoDurationAsync(string videoUrl)
        {
            var videoId = ExtractVideoId(videoUrl);
            if (string.IsNullOrEmpty(videoId))
            {
                return null;
            }

            try
            {
                using var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = _token,
                    ApplicationName = "LoopLearn"
                });

                var listRequest = youtubeService.Videos.List("contentDetails");
                listRequest.Id = videoId;

                var response = await listRequest.ExecuteAsync();
                var video = response.Items.FirstOrDefault();

                if (video != null)
                {
                    string isoDuration = video.ContentDetails.Duration;
                    return XmlConvert.ToTimeSpan(isoDuration);
                }
                else
                {
                    return null;
                }
            }
            catch (Google.GoogleApiException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // More reliable video ID extractor (no regex magic)
        private string ExtractVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            try
            {
                var uri = new Uri(url);
                if (uri.Host.Contains("youtu.be"))
                {
                    // e.g. youtu.be/f3oLnr4PWUc?si=...
                    return uri.Segments.Last().TrimEnd('/').Split('?')[0];
                }

                if (uri.Host.Contains("youtube.com") || uri.Host.Contains("youtube-nocookie.com"))
                {
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    return query["v"];  // returns null if not present
                }
            }
            catch (UriFormatException ex)
            {
                throw;
            }
            return null;
        }
    }
}