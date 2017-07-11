// (c) 2012-2016 Nick Hodge mailto:nhodge@mungr.com & Brendan Forster
// License: MS-PL

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using BoxKite.Twitter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BoxKite.Twitter
{
    public class UserStream : TwitterStream, IUserStream
    {
        // Public properties with private backingstores
        readonly Subject<Tweet> _tweets = new Subject<Tweet>();
        readonly Subject<DirectMessage> _directmessages = new Subject<DirectMessage>();
        readonly Subject<IStreamEvent> _events = new Subject<IStreamEvent>();
        readonly Subject<DeleteEvent> _deleteevents = new Subject<DeleteEvent>();
        readonly Subject<StreamScrubGeo> _scrubgeorequests = new Subject<StreamScrubGeo>();
        readonly Subject<StreamLimitNotice> _limitnotices = new Subject<StreamLimitNotice>();
        readonly Subject<StreamStatusWithheld> _statuswithheld = new Subject<StreamStatusWithheld>();
        readonly Subject<StreamUserWithheld> _userwithheld = new Subject<StreamUserWithheld>();
        readonly Subject<IEnumerable<long>> _friends = new Subject<IEnumerable<long>>();
        public IObservable<Tweet> Tweets { get { return _tweets; } }
        public IObservable<DirectMessage> DirectMessages { get { return _directmessages; } }
        public IObservable<IStreamEvent> Events { get { return _events; } }
        public IObservable<DeleteEvent> DeleteEvents { get { return _deleteevents; } }
        public IObservable<StreamScrubGeo> ScrubGeoRequests { get { return _scrubgeorequests; } }
        public IObservable<StreamLimitNotice> LimitNotices { get { return _limitnotices; } }
        public IObservable<StreamStatusWithheld> StatusWithheld { get { return _statuswithheld; } }
        public IObservable<StreamUserWithheld> UserWithheld { get { return _userwithheld; } }
        public IObservable<IEnumerable<long>> Friends { get { return _friends; } }

        public UserStream(Func<Task<HttpResponseMessage>> createOpenConnection)
        {
            CreateOpenConnection = createOpenConnection;
            TimeoutDelay = TimeSpan.FromMinutes(2);
        }

        public void Start()
        {
            CancelStream = new CancellationTokenSource();
            Task.Factory.StartNew(ProcessMessages, CancelStream.Token);
            _streamActive.OnNext(StreamSignal.Started);
        }

        public void Dispose()
        {
            Stop();
        }

        private void ProcessMessages()
        {
            Task.Factory.StartNew(ReadLines, CancelStream.Token);
            readLinesObservable.Subscribe(line =>
            {
#region Main Observer work here
#if (DEBUG)
                Debug.WriteLine(line);
#endif
#if (TRACE)
                if (line == "ENDBOXKITEUSERSTREAMTEST")
                {
                    Stop();
                }
#endif
                if (string.IsNullOrWhiteSpace(line)) return;
                var obj = JsonConvert.DeserializeObject<JObject>(line);

                //https://dev.twitter.com/docs/streaming-apis/messages

                if (obj["direct_message"] != null)
                {
                    _directmessages.OnNext(MapFromStreamTo<DirectMessage>(obj["direct_message"].ToString()));
                    return;
                }

                if (obj["in_reply_to_user_id"] != null)
                {
                    _tweets.OnNext(MapFromStreamTo<Tweet>(obj.ToString()));
                    return;
                }

                if (obj["friends"] != null)
                {
                    SendFriendsMessage(obj["friends"].Values<long>());
                    return;
                }

                // source: https://dev.twitter.com/docs/streaming-apis/messages#Events_event
                if (obj["event"] != null)
                {
                    _events.OnNext(MapFromEventInStream(obj));
                    return;
                }

                if (obj["scrub_geo"] != null)
                {
                    _scrubgeorequests.OnNext(MapFromStreamTo<StreamScrubGeo>(obj["scrub_geo"].ToString()));
                    return;
                }

                if (obj["limit"] != null)
                {
                    _limitnotices.OnNext(MapFromStreamTo<StreamLimitNotice>(obj["limit"].ToString()));
                    return;
                }

                if (obj["delete"] != null)
                {
                    _deleteevents.OnNext(MapFromStreamTo<DeleteEvent>(obj["delete"].ToString()));
                    return;
                }

                if (obj["status_withheld"] != null)
                {
                    _statuswithheld.OnNext(
                        MapFromStreamTo<StreamStatusWithheld>(obj["status_withheld"].ToString()));
                    return;
                }

                if (obj["user_withheld"] != null)
                {
                    _userwithheld.OnNext(MapFromStreamTo<StreamUserWithheld>(obj["user_withheld"].ToString()));
                    return;
                }

                if (obj["disconnect"] != null)
                {
                    MapFromStreamTo<StreamDisconnect>(obj["disconnect"].ToString());
                    Stop();
                    // check for non-hard disconnects & attempt reconnect
                }

                if (obj["warning"] == null) return; // no warnings, so start loop from beginning again

                if (obj["warning"]["percent_full"] != null)
                {
                    MapFromStreamTo<StreamStallWarning>(obj["warning"].ToString());
                    // do something something stall warning.
                }
                if (obj["warning"]["user_id"] != null)
                {
                    MapFromStreamTo<StreamToManyFollowsWarning>(obj["warning"].ToString());
                    // do something something user follows warning this is pretty final, actually.
                    Stop();
                }
#endregion           
            });
        }

        private void SendFriendsMessage(IEnumerable<long> obj)
        {
            _friends.OnNext(obj);
        }

    }
}
