﻿// (c) 2012-2016 Nick Hodge mailto:nhodge@mungr.com & Brendan Forster
// License: MS-PL

using System.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Models;

// ReSharper disable CheckNamespace
namespace BoxKite.Twitter
// ReSharper enable CheckNamespace
{
    public static class ReportExtensions
    {
        /// <summary>
        /// Report the specified user as a spam account to Twitter. Additionally performs the equivalent of POST blocks/create on behalf of the authenticated user.
        /// </summary>
        /// <param name="screenName">The ID or screen_name of the user you want to report as a spammer. Helpful for disambiguating when a valid screen name is also a user ID.</param>
        /// <param name="userId">The ID of the user you want to report as a spammer. Helpful for disambiguating when a valid user ID is also a valid screen name.</param>
        /// <returns></returns>
        /// <remarks> ref: https://dev.twitter.com/docs/api/1.1/post/users/report_spam </remarks>
        public static async Task<User> ReportUserForSpam(this IUserSession session, string screenName="", int userId=0)
        {
            var parameters = new TwitterParametersCollection();
            parameters.Create(screen_name: screenName, user_id: userId);

            if (parameters.EnsureEitherOr("screen_name", "user_id").IsFalse())
            {
                return session.MapParameterError<User>(
                        "Either screen_name or user_id required");
            }

            return await session.PostAsync(TwitterApi.Resolve("/1.1/users/report_spam.json"), parameters)
                          .ContinueWith(c => c.MapToSingle<User>());
        }
    }
}
