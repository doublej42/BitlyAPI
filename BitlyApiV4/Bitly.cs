using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;

/// Update of bitlyAPI to work with V4 of the bitly API.
/// Special thanks to https://github.com/ransagy/BitlyDotNet for providing example
/// </summary>
namespace BitlyAPI
{
    public class Bitly
    {
        private string _accessToken;
        private const string BaseUrl = "https://api-ssl.bitly.com/v4/";

        private const string BearerAuthScheme = "Bearer";
        /// <summary>
        /// Initialize the Bitly api with an access token
        /// Create your token at https://bitly.is/accesstoken
        /// for more information https://dev.bitly.com/v4/#section/Application-using-a-single-account
        /// </summary>
        /// <param name="genericAccessToken"></param>
        public Bitly(string genericAccessToken = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _accessToken = genericAccessToken;
        }

        protected async Task<T> GetResponse<T>(string commandUrl, Dictionary<string, string> parameters = null, HttpMethod httpMethod = null)
        {
            commandUrl = BaseUrl + commandUrl;
            if (httpMethod == null)
            {
                httpMethod = HttpMethod.Get;
            }

            

            using (var request = new HttpRequestMessage(httpMethod, commandUrl))
            {
                if (parameters != null)
                {
                    if (httpMethod == HttpMethod.Get)
                    {
                        var parms = new StringBuilder();
                        var itemCount = 0;
                        foreach (var item in parameters)
                        {
                            parms.Append(item.Key);
                            parms.Append("=");
                            parms.Append(WebUtility.UrlEncode(item.Value));
                            itemCount++;
                            if (itemCount != parameters.Count)
                            {
                                parms.Append("&");
                            }
                        }

                        commandUrl += parms;
                    }
                    else
                    {
                        //request.Content = new FormUrlEncodedContent(parameters);
                        request.Content = new StringContent(JsonConvert.SerializeObject(parameters),Encoding.UTF8, "application/json");
                    }
                }
                request.Headers.Authorization = new AuthenticationHeaderValue(BearerAuthScheme, _accessToken);
                using (var httpClient = new HttpClient())
                {
                    var res = await httpClient.SendAsync(request).ConfigureAwait(false);
                    res.EnsureSuccessStatusCode();
                    var resultJson = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<T>(resultJson);
                }
            }
        }

        public async Task<IEnumerable<BitlyGroup>> GetGroups()
        {
            var response = await GetResponse<BitlyGroupResponse>("groups");
            return response.Groups;
        }

        public async Task<String> PostShortenLink(string longUrl, string groupGuid = null, string domain = null)
        {
            var result =  await PostShorten(longUrl, groupGuid,   domain);
            return result.Link;
        }

        public async Task<BitlyShortenResponse> PostShorten(string longUrl, string groupGuid = null, string domain = null)
        {
            if (groupGuid == null)
            {
                var groups = await GetGroups();
                var groupsArray = groups as BitlyGroup[] ?? groups.ToArray();
                if (!groupsArray.Any())
                {
                    throw new Exception("Unable to find groups for user");
                }

                var group = groupsArray.First();
                groupGuid = group.Guid;
                if (domain == null && group.Bsds.Any())
                {
                    domain = group.Bsds.First();
                }
            }

            var parameters = new Dictionary<string, string> {{"group_guid", groupGuid}, {"long_url", longUrl}};
            if (domain != null)
            {
                parameters.Add("domain", domain);
            }
            
            
            var response = await GetResponse<BitlyShortenResponse>("shorten", parameters,HttpMethod.Post);
            return response;
        }

        //TODO:
        //  https://dev.bitly.com/v4/#operation/getBitlinksByGroup
    }
}

