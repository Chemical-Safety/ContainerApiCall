using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace ContainerApiCall
{
    internal class Connect
    {
        public delegate void LogEventHandler(object sender, string message);

        public event LogEventHandler MessageLogged = new LogEventHandler((e, a) => { });

        public enum CallMethods
        {
            GetSessionId = 0,
            AddToSession,
            ProcessSession,
            SessionResults,
            GetData
        }

        public class HttpReturn
        {
            public string url { get; set; }
            public string responseBody { get; set; }
            public string errorMessage { get; set; }
            public override string ToString()
            {
                return url + Environment.NewLine + responseBody + Environment.NewLine + errorMessage;
            }
        }
        string _baseUrl; 
        string _auth; 
        string _token; 
        public Connect(string authKey, string baseUrl,string token)
        {
            _token = token;
            _auth = authKey;
            _baseUrl = baseUrl;
            if (!_baseUrl.EndsWith("/")) _baseUrl += "/";
            _baseUrl = _baseUrl + "externalinterface/";
        }

        public Connect(string authKey, string baseUrl)
        {
            _auth = authKey;
            _baseUrl = baseUrl;
            if (!_baseUrl.EndsWith("/")) _baseUrl += "/";
            _baseUrl = _baseUrl + "externalinterface/";
        }
        public string GetEndpoint(CallMethods action)
        {
            string[] endPoints = {"getsessionid", "addtosession","processsession","sessionresults","getdata"};
            return _baseUrl + "wrappers/inventory.ashx?method=" + endPoints[(int)action];
        }

        public async Task<string> GetToken()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("auth", _auth);
            string url = _baseUrl + "keygetter.ashx?auth=" + _auth;
            return await client.GetStringAsync(url);
        }

        public async Task<HttpReturn> GetData(CallMethods action, string json)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("auth",_auth);
            client.DefaultRequestHeaders.Add("key", _token);
            var url = GetEndpoint(action);
            if (_auth.Length == 0)
                MessageLogged(this, "No auth set");
            if (_token.Length == 0)
                MessageLogged(this, "No token set");
            MessageLogged(this, "Calling " + url);
            if (json.Length > 0)
                MessageLogged(this, "Post body: " + json);
            try
            {
                using (var response = await client.PostAsync(url, new StringContent(json,Encoding.UTF8,"application/json")))
                {
                    using (var content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        return new HttpReturn {url=url,responseBody = result };
                    }
                }

            }
            catch (HttpRequestException e)
            {
                return new HttpReturn { url = url, errorMessage = e.Message };
            }
        }

    }

}
