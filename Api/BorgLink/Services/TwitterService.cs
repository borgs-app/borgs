using BorgLink.Models.Constants;
using BorgLink.Models.Options;
using BorgLink.Utils;
using CoreTweet;
using CoreTweet.Rest;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Parameters;

namespace BorgLink.Services
{
    /// <summary>
    /// For communication between borgs and Twitter
    /// </summary>
    public class TwitterService
    {
        private readonly TwitterServiceOptions _twitterOptions;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="twitterOptions"></param>
        public TwitterService(IOptions<TwitterServiceOptions> twitterOptions)
        {
            _twitterOptions = twitterOptions.Value;
        }

        /// <summary>
        /// Post content
        /// </summary>
        /// <param name="statusMessage">The message to post</param>
        /// <returns>An asyn task</returns>
        public async Task PostAsync(string statusMessage, string cardUri)
        {
            // Stop here if not enabled
            if (!_twitterOptions.Enabled)
                return;

            // Build client
            var userClient = new TwitterClient(_twitterOptions.ApiKey, _twitterOptions.ApiSecret, _twitterOptions.AccessToken, _twitterOptions.AccessTokenSecret);

            // Get user
            var user = await userClient.Users.GetAuthenticatedUserAsync();

            // Get image
            var borgImage = ImageUtils.GetImageFromUri(cardUri, BorgsConstants.BorgBackgroundColor);
            var borgBytes = ImageUtils.CopyImageToByteArray(borgImage);
        
            // Upload photo
            var uploadedPhoto = await userClient.Upload.UploadTweetImageAsync(borgBytes);

            // Post tweet
            await userClient.Tweets.PublishTweetAsync(new PublishTweetParameters(statusMessage)
            {
                Medias = { uploadedPhoto }
            });

            // Return task
            return;
        }
    }
}
