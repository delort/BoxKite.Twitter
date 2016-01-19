﻿// (c) 2012-2016 Nick Hodge mailto:nhodge@mungr.com & Brendan Forster
// License: MS-PL

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoxKite.Twitter.Models;

namespace BoxKite.Twitter.Console
{
    public class ApplicationOnlyAuthFireTests
    {
        public async Task<bool> DoApplicationOnlyAuthFireTests(TwitterConnection twitterConnection, List<int> testSeq)
        {
            var successStatus = true;

            try
            {
                // 1
                if (testSeq.Contains(1))
                {
                    ConsoleOutput.PrintMessage("12.1 User Time Line//Application Auth Only", ConsoleColor.Gray);

                    var combo1 = await twitterConnection.TwitterSession.GetUserTimeline("KatyPerry");

                    if (combo1.OK)
                    {
                        foreach (var trnd in combo1)
                        {
                            ConsoleOutput.PrintMessage(
                                $"App Auth Timeline Test: {trnd.Text}");
                        }
                    }
                    else
                    {
                        successStatus = false;
                        throw new Exception("cannot user time line (app only auth)");
                    }
                } // end test 1

                // 2
                if (testSeq.Contains(2))
                {
                    ConsoleOutput.PrintMessage("12.2 API Management//Rate limits", ConsoleColor.Gray);

                    var combo2 = await twitterConnection.TwitterSession.GetCurrentApiStatus();

                    if (combo2.OK)
                    {
                        foreach (var apir in combo2.APIRateStatuses)
                        {
                            ConsoleOutput.PrintMessage(
                                $"API: {apir.Value.APIPath} Limit: {apir.Value.Limit} Remaining: {apir.Value.Remaining}");
                        }
                    }
                    else
                    {
                        successStatus = false;
                        throw new Exception("cannot api management (app only auth)");
                    }
                } // end test 2

                // 3 
                if (testSeq.Contains(3))
                {
                    ConsoleOutput.PrintMessage("12.3 Get a Tweet//", ConsoleColor.Gray);

                    var combo3 = await twitterConnection.TwitterSession.GetTweet(464717579861258242);

                    if (combo3.OK)
                    {
                            ConsoleOutput.PrintMessage(combo3.Text);
                    }
                    else
                    {
                        successStatus = false;
                        throw new Exception("cannot get tweet (app only auth)");
                    }
                } // end test 3


                // 4 
                if (testSeq.Contains(4))
                {
                    ConsoleOutput.PrintMessage("12.4 Get a Retweet//", ConsoleColor.Gray);

                    var oldtweet = new Tweet() {Id = 453310114796412928};

                    var combo4 = await twitterConnection.TwitterSession.GetRetweets(oldtweet);

                    if (combo4.OK)
                    {
                        ConsoleOutput.PrintMessage($"Retweeted {combo4.Count} times");
                    }
                    else
                    {
                        successStatus = false;
                        throw new Exception("cannot get retweet (app only auth)");
                    }
                } // end test 4


                // 5 
                if (testSeq.Contains(5))
                {
                    ConsoleOutput.PrintMessage("12.5 Get friendships//", ConsoleColor.Gray);

                    var combo5 = await twitterConnection.TwitterSession.GetFriendship(sourceScreenName:"shiftkey",targetScreenName:"nickhodgemsft");

                    if (combo5.OK)
                    {
                        
                    }
                    else
                    {
                        successStatus = false;
                        throw new Exception("cannot get friendships (app only auth)");
                    }
                } // end test 5

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