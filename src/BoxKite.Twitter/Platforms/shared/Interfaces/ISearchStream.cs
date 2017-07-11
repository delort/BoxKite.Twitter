// (c) 2012-2016 Nick Hodge mailto:nhodge@mungr.com & Brendan Forster
// License: MS-PL

using System;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using BoxKite.Twitter.Models;

namespace BoxKite.Twitter
{
    public interface ISearchStream : IDisposable
    {
        IObservable<Tweet> FoundTweets { get; }
        Subject<StreamSearchRequest> SearchRequests { get; }
        TwitterParametersCollection SearchParameters { get; set; }
        CancellationTokenSource CancelStream { get; set; }
        IObservable<StreamSignal> StreamActive { get; }
        TimeSpan TimeoutDelay { get; set; }
        Func<Task<HttpResponseMessage>> CreateOpenConnection { get; set; }
        void Start();
        void Stop();
    }
}