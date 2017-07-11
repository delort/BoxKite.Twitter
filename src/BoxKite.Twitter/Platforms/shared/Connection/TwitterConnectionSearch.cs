﻿// (c) 2012-2016 Nick Hodge mailto:nhodge@mungr.com & Brendan Forster
// License: MS-PL

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using BoxKite.Twitter.Models;

namespace BoxKite.Twitter
{
    public partial class TwitterConnection
    {
        // largestSeenIds
        public long _searchLargestSeenId { get; set; }
        //

        // status bits
        private readonly Subject<bool> _searchBackFillCompleted = new Subject<bool>();
        private readonly Subject<bool> _searchStreamDisconnected = new Subject<bool>();
        //

        private readonly Subject<Tweet> _searchtimeline = new Subject<Tweet>();
        public IObservable<Tweet> SearchTimeLine { get { return _searchtimeline; } }
        private string _currentSearchText = "";

        private CancellationTokenSource _twitterSearchCommunicationToken;

        /// <summary>
        /// Start a SearchStream
        /// </summary>
        /// <param name="textToSearch">text string to begin search</param>
        public void StartSearchStreaming(string textToSearch)
        {
            _twitterSearchCommunicationToken = new CancellationTokenSource();
            // when the searchstream gets disconnected, revert to polling (REST) searches
            _searchStreamDisconnected.Where(status => status.Equals(true)).Subscribe(StartPollingSearch);
            _currentSearchText = textToSearch;
            //
            if (UserSession.IsActive)
            {
                SearchStream = UserSession.StartSearchStream(track: textToSearch);
                SearchStream.FoundTweets.Subscribe(_searchtimeline.OnNext);
                SearchStream.Start();
                SearchStream.StreamActive.Subscribe(t => ManageSearchStreamDisconnect());
            }
            else
            {
                // cannot get usersession, fall back
                _searchStreamDisconnected.OnNext(true);
            }
            // and get "backfills"; tweets that pre-date the current stream/polling connection
            Task.Factory.StartNew(ProcessSearchBackFill_Pump);
        }

        // subscriber to the userstream disconnecting
        private void ManageSearchStreamDisconnect()
        {
            _searchStreamDisconnected.OnNext(true); // push message saying userStream is no longer connected
        }

        /// <summary>
        /// Stop SearchStreaming
        /// </summary>
        public void StopSearchStreaming()
        {
            _twitterSearchCommunicationToken.Cancel();
            SearchStream.Stop();
        }

        private async void ProcessSearchBackFill_Pump()
        {   
            _searchLargestSeenId = await GetSearchTimeLine_Backfill();
            _searchBackFillCompleted.OnNext(true);
        }

        private async Task<long> GetSearchTimeLine_Backfill()
        {
            int backofftimer = 30;
            long smallestid = 0;
            long largestid = 0;
            int backfillQuota = 50;

            do
            {
                //TODO: need to unhardcode SearchResultType just for backfills
                var searchtl = await TwitterSession.SearchFor(searchText:_currentSearchText, searchResponseType:SearchResultType.Recent, count: _pagingSize, maxId: smallestid);
                if (searchtl.OK)
                {
                    smallestid = long.MaxValue;
                    if (searchtl.Tweets.Count() < backfillQuota) backfillQuota = searchtl.Tweets.Count();
                    foreach (var tweet in searchtl.Tweets)
                    {
                        if (tweet.Id < smallestid) smallestid = tweet.Id;
                        if (tweet.Id > largestid) largestid = tweet.Id;
                        _searchtimeline.OnNext(tweet);
                        backfillQuota--;
                    }
                }
                else
                {
                    // The Backoff will trigger 7 times before just giving up
                    // once at 30s, 60s, 1m, 2m, 4m, 8m and then 16m
                    // note that the last call into this will be 1m above the 15 "rate limit reset" window 
                    await Task.Delay(TimeSpan.FromSeconds(backofftimer));
                    if (backofftimer > _maxbackoff)
                    backofftimer = backofftimer * 2;
                }
            } while (backfillQuota > 0);
            return largestid;
        }

        private void StartPollingSearch(bool status)
        {
            // firstly wait on the backfills to complete before firing off these
            _searchBackFillCompleted.Where(st => st).Subscribe(s =>
            {
                // this will fire once per minute for 24 hours from init
                var observable = Observable.Interval(TimeSpan.FromSeconds(20));
                observable.Subscribe(async t =>
                {
                    _searchLargestSeenId = await GetSearchTimeLine_Failover(_searchLargestSeenId);
                });
            });
        }

        private async Task<long> GetSearchTimeLine_Failover(long sinceid)
        {
            var largestseenid = sinceid;

            var searchtl = await TwitterSession.SearchFor(searchText: _currentSearchText, searchResponseType: SearchResultType.Recent, count: _pagingSize, sinceId: sinceid);
            if (!searchtl.OK) return largestseenid;

            foreach (var tweet in searchtl.Tweets.Where(tweet => tweet.Id > sinceid))
            {
                if (tweet.Id > sinceid) largestseenid = tweet.Id;
                _searchtimeline.OnNext(tweet);
            }
            return largestseenid;
        }

    }
}
