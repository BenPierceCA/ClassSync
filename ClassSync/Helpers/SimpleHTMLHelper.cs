using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;

namespace ClassSync.Helpers
{
    public class SimpleHTMLHelper
    {
        public static string Post(string _url, Dictionary<string, string> _formParams)
        {
            RestClient client = new RestClient { BaseUrl = new Uri(_url) };
            RestRequest request = new RestRequest { Method = Method.POST, RequestFormat = DataFormat.Json };

            request.AddHeader("accept", "application/json, text/javascript, */*; q=0.01");

            foreach (KeyValuePair<string, string> param in _formParams)
            {
                request.AddParameter(param.Key, param.Value);
            }            

            var response = client.Execute(request);

            return response.Content;
        }
    }
}
