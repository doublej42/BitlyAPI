using System;
using System.Collections.Generic;
using System.Globalization;
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

            if (parameters != null && httpMethod == HttpMethod.Get)
            {
                var parms = new StringBuilder();
                parms.Append("?");
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

            using (var request = new HttpRequestMessage(httpMethod, commandUrl))
            {
                if (parameters != null && httpMethod != HttpMethod.Get)
                {
                        request.Content = new StringContent(JsonConvert.SerializeObject(parameters),Encoding.UTF8, "application/json");
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

        public async Task<BitlyLink> PostShorten(string longUrl, string groupGuid = null, string domain = null)
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
            
            
            var response = await GetResponse<BitlyLink>("shorten", parameters,HttpMethod.Post);
            return response;
        }

        


        public async Task<BitlyBitlinksResponse> GetBitlinksByGroup(
            string groupGuid = null, 
            int size = 50, 
            int page = 1, 
            string keyword = null,
            string query = null,
            DateTime? createdBefore = null,
            DateTime? createdAfter = null,
            DateTime? modifiedAfter = null,
            string archived = "off",
            string deeplinks = "both",
            string domainDeeplinks = "both",
            string campaignGuid = null,
            string channelGuid = null,
            string customBitlink = "both",
            List<string> tags = null,
            List<string> encodingLogin = null
        )
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
            }
            var parameters = new Dictionary<string, string>
            {
                { "size", size.ToString() }, 
                { "page", page.ToString() },
                { "archived", archived.ToString() },
                { "deeplinks", deeplinks.ToString() },
                { "domain_deeplinks", domainDeeplinks.ToString() },
                { "custom_bitlink", customBitlink.ToString() }
            };
            if (keyword != null)
            {
                parameters.Add("keyword", keyword);
            }
            if (query != null)
            {
                parameters.Add("query", query);
            }
            if (campaignGuid != null)
            {
                parameters.Add("campaignGuid", campaignGuid);
            }
            if (channelGuid != null)
            {
                parameters.Add("channelGuid", channelGuid);
            }
            if (tags != null)
            {
                parameters.Add("tags", JsonConvert.SerializeObject(tags));
            }
            if (encodingLogin != null)
            {
                parameters.Add("encodingLogin", JsonConvert.SerializeObject(encodingLogin));
            }

            if (createdBefore != null)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var unixDatetime = (((DateTime) createdBefore).ToUniversalTime() - epoch).TotalSeconds;
                parameters.Add("created_before", unixDatetime.ToString(CultureInfo.InvariantCulture));
            }
            if (createdAfter != null)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var unixDatetime = (((DateTime)createdAfter).ToUniversalTime() - epoch).TotalSeconds;
                parameters.Add("created_after", Convert.ToInt64(unixDatetime).ToString(CultureInfo.InvariantCulture));
            }
            if (modifiedAfter != null)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var unixDatetime = (((DateTime)modifiedAfter).ToUniversalTime() - epoch).TotalSeconds;
                parameters.Add("modified_after", unixDatetime.ToString(CultureInfo.InvariantCulture));
            }
            var response = await GetResponse<BitlyBitlinksResponse>("groups/"+groupGuid+"/bitlinks", parameters, HttpMethod.Get);
            return response;
        }

        //TODO:
        //  https://dev.bitly.com/v4/#operation/getBitlinksByGroup
    }
}

