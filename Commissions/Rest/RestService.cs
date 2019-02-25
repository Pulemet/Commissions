using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Commissions.Rest
{
    public class RestService : IRestService
    {
        private string _token;

        public string Token
        {
            get
            {
                if (_token == null)
                    throw new Exception("Unauthorized"); // TODO: better exception
                return _token;
            }
            private set { _token = value; }
        }

        public string _refreshToken;
        public readonly string _url;
        public readonly string _oauthPath;
        private readonly string _authorization;
        private System.Timers.Timer timer;
        private volatile bool needToRefreshToken;
        private long expiresInSec;

        public RestService(string url, string oauthPath, string authorization)
        {
            _url = url;
            _oauthPath = oauthPath;
            _authorization = authorization;
        }

        public void Authorize(string user, string password)
        {
            var httpContent = new HttpRequestMessage(HttpMethod.Post, _url + _oauthPath);
            if(_authorization != "")
                httpContent.Headers.Add("Authorization", _authorization);

            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("grant_type", "password"));
            values.Add(new KeyValuePair<string, string>("username", user));
            values.Add(new KeyValuePair<string, string>("password", password));
            values.Add(new KeyValuePair<string, string>("scope", "public"));
            httpContent.Content = new FormUrlEncodedContent(values);

            dynamic responseData = RequestJsonImpl(httpContent);
            Token = responseData.access_token;
            _refreshToken = responseData.refresh_token;
            expiresInSec = int.Parse(responseData.expires_in.ToString());

            timer = new System.Timers.Timer(expiresInSec * 1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            needToRefreshToken = true;
        }

        public void RefreshToken()
        {
            var httpContent = new HttpRequestMessage(HttpMethod.Post, _url + _oauthPath);
            httpContent.Headers.Add("Authorization", _authorization);
            //var byteArray = Encoding.ASCII.GetBytes($"{user}:{password}");
            //httpContent.Headers.Authorization  = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            values.Add(new KeyValuePair<string, string>("refresh_token", _refreshToken));
            httpContent.Content = new FormUrlEncodedContent(values);

            dynamic responseData = RequestJsonImpl(httpContent);
            Token = responseData.access_token;
            _refreshToken = responseData.refresh_token;
            Console.WriteLine("RefreshToken");
            needToRefreshToken = false;
            expiresInSec = int.Parse(responseData.expires_in.ToString());

            timer.Interval = expiresInSec * 1000;
            timer.Enabled = true;
        }

        private dynamic RequestJsonImpl(HttpRequestMessage httpContent)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Debug.WriteLine("Perform " + httpContent.Method + " " + httpContent.RequestUri);

            HttpResponseMessage response = client.SendAsync(httpContent).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(httpContent.Method + " error: " + response.ToString());
            }

            Debug.WriteLine("OK " + httpContent.Method + " " + httpContent.RequestUri);

            string responseJson = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject(responseJson);
        }

        public T RequestJson<T>(HttpRequestMessage httpContent)
        {
            if (needToRefreshToken)
                RefreshToken();

            httpContent.Headers.Add("Authorization", "bearer " + Token);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Debug.WriteLine("Perform " + httpContent.Method + " " + httpContent.RequestUri);

            HttpResponseMessage response = client.SendAsync(httpContent).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(httpContent.Method + " error: " + response.ToString());
            }

            Debug.WriteLine("OK " + httpContent.Method + " " + httpContent.RequestUri);

            string responseJson = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(responseJson);
        }

        public HttpRequestMessage CreateUnauthorizedHttpRequest(HttpMethod method, string request, dynamic body)
        {
            var httpRequest = new HttpRequestMessage(method, _url + request);
            if (body != null)
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            return httpRequest;
        }

        public T Get<T>(string requestUrl)
        {
            return RequestJson<T>(CreateUnauthorizedHttpRequest(HttpMethod.Get, requestUrl, null));
        }

        public T Delete<T>(string requestUrl)
        {
            return RequestJson<T>(CreateUnauthorizedHttpRequest(HttpMethod.Delete, requestUrl, null));
        }

        public T Post<T>(string requestUrl)
        {
            return RequestJson<T>(CreateUnauthorizedHttpRequest(HttpMethod.Post, requestUrl, null));
        }

        public Tout Post<Tin, Tout>(string requestUrl, Tin body)
        {
            return RequestJson<Tout>(CreateUnauthorizedHttpRequest(HttpMethod.Post, requestUrl, body));
        }

        public Tout Put<Tin, Tout>(string requestUrl, Tin body)
        {
            return RequestJson<Tout>(CreateUnauthorizedHttpRequest(HttpMethod.Put, requestUrl, body));
        }
    }
}
