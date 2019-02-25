using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Commissions.CryptoCortex.Models;
using Newtonsoft.Json;
using WebSocketSharp;

namespace Commissions.Subscription
{
    public class StompWebSocketService : IStompWebSocketService
    {
        protected const string SubIdPrefix = "subscription:";
        protected const int SubIdPrefixLength = 13;
        protected const string MsgIdPrefix = "message-id:";
        protected const string CorrIdPrefix = "correlation-id:";
        protected const int CorrIdPrefixLength = 15;
        protected const string StatusPrefix = "status:";
        protected const int StatusPrefixLength = 7;
        protected const long PingIntervalMsec = 10000;

        protected readonly WebSocket socket;
        protected readonly string token;
        // id=>topics
        protected Dictionary<string, string> topics = new Dictionary<string, string>();
        // id => handler
        protected Dictionary<string, Action<string>> subscribers = new Dictionary<string, Action<string>>();
        protected Dictionary<Action<string>, HashSet<string>> actions = new Dictionary<Action<string>, HashSet<string>>();
        protected Dictionary<string, Action<string>> fastSubscribers = new Dictionary<string, Action<string>>();
        protected int subIdx = 1;
        protected Timer pingTimer;

        public StompWebSocketService(string url, string newToken)
        {
            socket = new WebSocket(url);
            socket.Log.Level = LogLevel.Fatal;
            socket.OnOpen += SocketOnOnOpen;
            socket.OnMessage += SocketOnOnMessage;
            //_socket.OnError += (obj, error) =>
            //Console.WriteLine(error.Message);
            this.token = newToken;
            socket.Connect();
        }

        protected void SocketOnOnOpen(object sender, EventArgs eventArgs)
        {
            pingTimer = new Timer(PingIntervalMsec);
            pingTimer.Elapsed += _pingTimer_Elapsed;

            socket.Send(GetConnectMessage(token));
            //Console.WriteLine(_socket.IsAlive);
        }

        public void Subscribe(string topic, Action<string> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            string subscriptionId = "sub-" + subIdx.ToString();
            subIdx++;

            socket.Send(GetSubscribeMessage(subscriptionId, topic));

            if (subscribers.Count == 0)
                pingTimer.Start();

            subscribers.Add(subscriptionId, action);
            topics.Add(subscriptionId, topic);
            HashSet<string> subIds;
            if (!actions.TryGetValue(action, out subIds))
            {
                subIds = new HashSet<string>();
                actions.Add(action, subIds);
            }
            subIds.Add(subscriptionId);
        }

        public void SendMessage<T>(string topic, T body, Action<string> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            string correlationId = GetRandomString(10);

            socket.Send(GetSendMessage(correlationId, topic, body));
            fastSubscribers.Add(correlationId, action);
        }

        protected void SocketOnOnMessage(object sender, MessageEventArgs e)
        {
            int corrIdStartIndex = e.Data.IndexOf(CorrIdPrefix);
            int bodyStartIndex = e.Data.IndexOf("\n\n");
            int subIdStartIndex = e.Data.IndexOf(SubIdPrefix);
            int msgIdStartIndex = e.Data.IndexOf(MsgIdPrefix);
            int statusStartIndex = e.Data.IndexOf(StatusPrefix);
            if (subIdStartIndex > 0 && msgIdStartIndex > 0 && statusStartIndex > 0)
            {
                int s = statusStartIndex + StatusPrefixLength;
                string status = e.Data.Substring(s, 3);

                s = subIdStartIndex + SubIdPrefixLength;
                string subId = e.Data.Substring(s, msgIdStartIndex - s - 1/*-1 for trailing \n*/).Replace("\\c", ":");

                string message = status + e.Data.Substring(bodyStartIndex);

                Action<string> action;

                if (corrIdStartIndex > 0)
                {
                    s = corrIdStartIndex + CorrIdPrefixLength;
                    string corrId = e.Data.Substring(s, 10);
                    if (subscribers.ContainsKey(subId) && fastSubscribers.TryGetValue(corrId, out action))
                    {
                        action(message);
                        fastSubscribers.Remove(corrId);
                    }    
                }
                else if (subscribers.TryGetValue(subId, out action))
                    action(message);
            }
        }

        public string GetSubscribeMessage(string subscriptionId, string topic)
	    {
            return $"SUBSCRIBE\r\nX-Deltix-Nonce:1550757750718\r\nid:{subscriptionId}\r\ndestination:{topic}\r\n\r\n\0";
        }

        public string GetSendMessage<T>(string correlationId, string topic, T body)
        {
            return String.Format("SEND\r\ncorrelation-id:{0}\r\nX-Deltix-Nonce:{2}\r\ndestination:{1}\r\n\r\n{3}\r\n\r\n\0",
                correlationId, topic,
                StompWebSocketService.ConvertToUnixTimestamp(DateTime.Now),
                JsonConvert.SerializeObject(body));
        }

        public string GetConnectMessage(string token)
	    {
            return $"CONNECT\r\nAuthorization:{token}\r\nheart-beat:{PingIntervalMsec},{PingIntervalMsec}\r\naccept-version:1.1,1.0\r\n\r\n\0";
        }

        protected void _pingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            pingTimer.Stop();

            try
            {
                socket.Send("\r\n");
            }
            finally
            {
                pingTimer.Start();
            }
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (long)diff.TotalMilliseconds;
        }

        public static string GetRandomString(int length)
        {
            Byte[] seedBuffer = new Byte[4];
            using (var rngCryptoServiceProvider = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(seedBuffer);
                string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
                Random random = new Random(System.BitConverter.ToInt32(seedBuffer, 0));
                return new String(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }

        public bool Unsubscribe(string topic, Action<string> action)
        {
            HashSet<string> subIds = null;
            if (!actions.TryGetValue(action, out subIds))
                return false;
            string removedSubId = null;

            foreach (var subId in subIds)
            {
                if (topics[subId] == topic)
                {
                    removedSubId = subId;
                    break;
                }
            }
            if (removedSubId == null)
            {
                Debug.Assert(false);
                return false;
            }

            string unsubscribeMessage = "UNSUBSCRIBE\r\nid:" + removedSubId + "\r\n\r\n\0";
            socket.Send(unsubscribeMessage);

            subIds.Remove(removedSubId);
            if (subIds.Count == 0)
                actions.Remove(action);
            topics.Remove(removedSubId);
            subscribers.Remove(removedSubId);

            if (subscribers.Count == 0)
                pingTimer.Stop();

            return true;
        }

        public void Close()
        {
            socket.Close();
        }
    }

    public interface IStompWebSocketService
    {
        void Subscribe(string topic, Action<string> handler);
        bool Unsubscribe(string topic, Action<string> handler);
    }
}
