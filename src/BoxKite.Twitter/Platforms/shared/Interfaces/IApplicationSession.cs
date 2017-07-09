﻿// (c) 2012-2016 Nick Hodge mailto:nhodge@mungr.com & Brendan Forster
// License: MS-PL

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BoxKite.Twitter
{
    public interface IApplicationSession : ITwitterSession
    {
        bool IsActive { get; set; }
        string clientID { get; set; }
        string clientSecret { get; set; }
        string bearerToken { get; set; }
        int waitTimeoutSeconds { get; set; }

        new Task<HttpResponseMessage> GetAsync(string relativeUrl, SortedDictionary<string, string> parameters);

       Task<HttpResponseMessage> PostAsync(string url, SortedDictionary<string, string> parameters, bool forInitialAuth);

    }
}
