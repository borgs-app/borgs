using BorgLink.Models.Options;
using CoreTweet;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;

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
        public async Task Post(string statusMessage)
        {
            // Build client
            var userClient = new TwitterClient(_twitterOptions.ApiKey, _twitterOptions.ApiSecret, _twitterOptions.AccessToken, _twitterOptions.AccessTokenSecret);

            // Get user
            var user = await userClient.Users.GetAuthenticatedUserAsync();

            // Make tweet
            var tweet = await userClient.Tweets.PublishTweetAsync(statusMessage);

            // Return task
            return;
        }
    }
}
