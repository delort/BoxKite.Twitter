﻿// (c) 2012-2016 Nick Hodge mailto:nhodge@mungr.com & Brendan Forster
// License: MS-PL

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BoxKite.Twitter.Console.Helpers;
using BoxKite.Twitter.Models;

namespace BoxKite.Twitter.Console
{
    public class TweetsLiveFireTests
    {
        public async Task<bool> DoTweetTest(IUserSession session, List<int> testSeq)
        {
            var successStatus = true;
            try
            {
                // 1
                long tweetid = 0;
                if (testSeq.Contains(1))
                {
                    ConsoleOutput.PrintMessage("4.1 Tweets\\SendTweet", ConsoleColor.Gray);
                    var tweets1 = await session.SendTweet("Live Fire Test only, please ignore");

                    if (tweets1.OK)
                    {
                        tweetid = tweets1.Id;
                        ConsoleOutput.PrintMessage(
                            $"From: {tweets1.User.ScreenName} // Message: {tweets1.Text}");
                    }
                    else
                        successStatus = false;
                }

                var tweets2 = new Tweet();

                // 2
                if (testSeq.Contains(2))
                {
                    ConsoleOutput.PrintMessage("4.2 Tweets\\GetTweet", ConsoleColor.Gray);
                    tweets2 = await session.GetTweet(336377569098207233);

                    if (tweets2.OK)
                    {
                        ConsoleOutput.PrintMessage(
                            $"From: {tweets2.User.ScreenName} // Message: {tweets2.Text}");
                    }
                    else
                        successStatus = false;
                }

                // 3
                if (testSeq.Contains(3))
                {
                    ConsoleOutput.PrintMessage("4.3 Tweets\\GetRetweets", ConsoleColor.Gray);
                    var tweets3 = await session.GetRetweets(tweets2);

                    if (tweets3.OK)
                    {
                        foreach (var t in tweets3)
                        {
                            ConsoleOutput.PrintMessage(
                                $"From: {t.User.ScreenName} // Message: {t.Text}");
                        }
                    }
                    else
                        successStatus = false;
                }

                // 4
                if (testSeq.Contains(4))
                {
                    ConsoleOutput.PrintMessage("4.4 Tweets\\SendTweetWithImage", ConsoleColor.Gray);

                    var sr = FilesHelper.FromFile("sampleimage\\Boxkite-Logo-github.jpg");

                    using (var fs = new FileStream(sr, FileMode.Open, FileAccess.Read))
                    {
                        var tweets4 =
                            await
                                session.SendTweetWithImage("Live Fire Test only, please ignore", Path.GetFileName(sr), fs);

                        if (tweets4.OK)
                        {
                            tweetid = tweets4.Id;
                            ConsoleOutput.PrintMessage(
                                $"From: {tweets4.User.ScreenName} // Message: {tweets4.Text}");
                        }
                        else
                        {
                            TwitterLiveFireUserAuth.PrintTwitterErrors(tweets4.twitterControlMessage);
                            successStatus = false;
                        }
                    }
                }

                // 5
                if (testSeq.Contains(5))
                {
                    ConsoleOutput.PrintMessage("4.5 Tweets\\GetTweet - with Extended Entities", ConsoleColor.Gray);
                    var tweets5 = await session.GetTweet(560049149836808192);

                    if (tweets5.OK)
                    {
                        ConsoleOutput.PrintMessage(
                            $"From: {tweets5.User.ScreenName} // Message: {tweets5.Text}");
                        ConsoleOutput.PrintMessage(
                            $"Extended Entities Count: {tweets5.ExtendedEntities.Urls.Count()}");
                    }
                    else
                        successStatus = false;
                }
            }
            catch (Exception e)
            {
                ConsoleOutput.PrintError(e.ToString());
                return false;
            }
            return successStatus;
        }
    }
}