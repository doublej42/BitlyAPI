using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitlyAPI
{
 
    /// <summary>
    /// Implements Most of the bitly API as a hard typed c# library
    /// Provide an access token when you initialize the class or in your web config under the name bitlyAccess_Token 
    /// See: https://bitly.com/a/oauth_apps
    /// TODO: metrics http://dev.bitly.com/link_metrics.html
    /// TODO: network_history and tracking domain_list http://dev.bitly.com/user_info.html
    /// The framework is in place to add the rest of the API but I have not until I found a use for it.
    /// See http://dev.bitly.com/links.html for more information
    /// 
    /// </summary>
    public class Bitly
    {
        private string _accessToken;
        private const string BaseUrl = "https://api-ssl.bitly.com/v3/";

        
        /// <summary>
        /// Initialize the Bitly api with an access token
        /// Create your token at https://bitly.com/a/oauth_apps
        /// for more information http://dev.bitly.com/authentication.html
        /// </summary>
        /// <param name="accessToken"></param>
        public Bitly(string accessToken = null)
        {
            if (accessToken != null)
            {
                AccessToken = accessToken;
            }
        }

        /// <summary>
        /// Helper function that takes the parameters as a dictionary[string,string]
        /// This makes it easy to to just and and forget parameters and let the software build teh request
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected BitlyResponse GetResponseDict(string command, Dictionary<string, string> parameters)
        {
            var parms = new StringBuilder();
            var itemCount = 0;
            foreach (var item in parameters)
            {
                parms.Append(item.Key);
                parms.Append("=");
                parms.Append(HttpUtility.UrlEncode(item.Value));
                itemCount++;
                if (itemCount != parameters.Count())
                {
                    parms.Append("&");
                }
            }
            return GetResponse(command, parms.ToString());
        }

        /// <summary>
        /// Takes the parameters as an anonymous object
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected BitlyResponse GetResponseObj(string command, object parameters)
        {
            var parms = new StringBuilder();
            var itemCount = 0;
            var objType = parameters.GetType();

            foreach (var property in objType.GetProperties())
            {
                parms.Append(property.Name);
                parms.Append("=");
                parms.Append(HttpUtility.UrlEncode(property.GetValue(parameters).ToString()));
                itemCount++;
                if (itemCount != objType.GetProperties().Count())
                {
                    parms.Append("&");
                }
            }
            return GetResponse(command, parms.ToString());
        }


        /// <summary>
        /// Access token is the best form of authentication you can get your access token at https://bitly.com/a/oauth_apps
        /// </summary>
        protected string AccessToken
        {
            get { return _accessToken ?? (_accessToken = WebConfigurationManager.AppSettings["bitlyAccess_Token"]); }
            set { _accessToken = value; }
        }


        
        /// <summary>
        /// Helper function for getting a response wants the parameters as a pre formed URL string
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected BitlyResponse GetResponse(string command, string parameters)
        {

            var targetUri = new StringBuilder(BaseUrl + command + "?");
            targetUri.Append("access_token=" + AccessToken);
            
            if (!string.IsNullOrWhiteSpace(parameters))
            {
                targetUri.Append("&");
                targetUri.Append(parameters);
            }

            var webRequest = (HttpWebRequest)WebRequest.Create(targetUri.ToString());
            webRequest.KeepAlive = false;
            webRequest.Method = "GET";
            var webResponse = (HttpWebResponse)webRequest.GetResponse();
            if (webResponse.StatusCode == HttpStatusCode.OK)
            {
                var responseStream = webResponse.GetResponseStream();
                if (responseStream != null)
                {
                    using (var sr = new StreamReader(responseStream))
                    {
                        var response = sr.ReadToEnd();
                        BitlyResponse ret;
                        try
                        {
                            ret = JsonConvert.DeserializeObject<BitlyResponse>(response);
                        }
                        catch 
                        {
                            var o = JObject.Parse(response);
                            ret = new BitlyResponse
                                {
                                    status_code = o["status_code"].Value<int>(),
                                    status_txt = o["status_txt"].Value<string>()
                                };
                        }
                        return ret;
                    }
                }
            }
            return new BitlyResponse();
        }

        /// <summary>
        /// Get the long Url and other information given a short url
        /// </summary>
        /// <param name="shortUrl">Short Url : example http://bit.ly/XIu3w or http://cnan.ca/Zv2BBM </param>
        /// <returns></returns>
        public BitlyResponse Expand(string shortUrl)
        {
            return GetResponse("expand", "shortUrl=" + HttpUtility.UrlEncode(shortUrl));
        }

        /// <summary>
        /// This is used to return the page title for a given bitly link.
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public BitlyResponse Info(string shortUrl = null, string hash = null)
        {
            if (shortUrl == null && hash == null)
            {
                return null;
            }
            var parameters = new Dictionary<string, string>();

            if (shortUrl != null)
            {
                parameters.Add("shortUrl", shortUrl);
            }
            if (hash != null)
            {
                parameters.Add("hash", hash);
            }
            return GetResponseDict("info", parameters);
        }

        /// <summary>
        /// http://dev.bitly.com/links.html#v3_link_lookup
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public BitlyResponse LinkLookup(string url)
        {
            return GetResponseObj("link/lookup", new {url});
        }

        /// <summary>
        /// http://dev.bitly.com/links.html#v3_user_link_lookup
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public BitlyResponse UserLinkLookup(string url)
        {
            return GetResponseObj("user/link_lookup", new {url });
        }

        /// <summary>
        /// Link Save gives you more options and should likely be used but this will also shorted a link.
        /// http://dev.bitly.com/links.html#v3_shorten
        /// </summary>
        /// <param name="longUrl"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public BitlyResponse Shorten(string longUrl, string domain = null)
        {
            var parameters = new Dictionary<string, string> {{"longUrl", longUrl}};
            if (domain != null)
            {
                parameters.Add("domain", domain);
            }
    
            return GetResponseDict("shorten", parameters);
        }

        /// <summary>
        /// http://dev.bitly.com/links.html#v3_user_link_edit
        /// </summary>
        /// <param name="link"></param>
        /// <param name="title"></param>
        /// <param name="note"></param>
        /// <param name="isPrivate"></param>
        /// <param name="userTs"></param>
        /// <param name="archived"></param>
        /// <param name="editList"></param>
        /// <returns></returns>
        public BitlyResponse LinkEdit(string link, string title = null, string note = null, bool isPrivate = false,
                                      double userTs = 0, bool archived = false, List<string> editList = null)
        {
            var parameters = new Dictionary<string, string> { { "link", link } };
            if (editList != null)
            {
                if (editList.Contains("title") && title != null)
                {
                    parameters.Add("title", title);
                }
                if (editList.Contains("note ") && note  != null)
                {
                    parameters.Add("note ", title);
                }
                 if (editList.Contains("private"))
                {
                    parameters.Add("private", isPrivate ? "true" : "false");
                }
// ReSharper disable CompareOfFloatsByEqualityOperator
                 if (editList.Contains("user_ts") && userTs != 0)
// ReSharper restore CompareOfFloatsByEqualityOperator
                {
                    parameters.Add("user_ts", userTs.ToString(CultureInfo.InvariantCulture));
                }
                if (editList.Contains("archived"))
                {
                    parameters.Add("archived", archived ? "true" : "false");
                }
                parameters.Add("edit",string.Join(",",editList));
               
            }
            return GetResponseDict("user/link_edit", parameters);
        }


        /// <summary>
        /// http://dev.bitly.com/links.html#v3_user_link_save
        /// </summary>
        /// <param name="longUrl"></param>
        /// <param name="title"></param>
        /// <param name="note"></param>
        /// <param name="isPrivate"></param>
        /// <returns></returns>
        public BitlyResponse UserLinkSave(string longUrl, string title = null, string note= null, bool isPrivate = false )
        {
            var parameters = new Dictionary<string, string> { { "longUrl", longUrl } };
            
                if (!string.IsNullOrWhiteSpace(title))
                {
                    parameters.Add("title", title);
                }
                if (!string.IsNullOrWhiteSpace(note))
                {
                    parameters.Add("note", note);
                }
                if (isPrivate)
                {
                    parameters.Add("private", "true");
                }
            return GetResponseDict("user/link_save", parameters);
        }

        /// <summary>
        /// http://dev.bitly.com/links.html#v3_user_save_custom_domain_keyword
        /// Not tested as I don't have a paid account if someone has a paid account and can send me a copy of the json response. It's not documented on bitlys site.
        /// </summary>
        /// <param name="keywordLink"></param>
        /// <param name="targetLink"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public BitlyResponse UserSaveCustomDomainKeyword(string keywordLink, string targetLink, bool overwrite = false)
        {
            var parameters = new Dictionary<string, string>
            {
                {"keyword_link", keywordLink},
                {"target_link", targetLink}
            };
            if (overwrite)
            {
                parameters.Add("overwrite", "true");
            }
            return GetResponseDict("user/save_custom_domain_keyword", parameters);
        }

        /// <summary>
        /// http://dev.bitly.com/user_info.html#v3_user_info
        /// </summary>
        /// <param name="login"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public BitlyResponse UserInfo(string login = null, string fullName = null)
        {
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(login))
            {
                parameters.Add("login",login);
            }
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                parameters.Add("full_name", fullName);
            }
            return GetResponseDict("user/info", parameters);
        }


        /// <summary>
        /// Gets a users link history.
        /// ExpandClientID is not supported because it changes the type of a client ID and this conflicts with a static typed language and is ignored for now
        /// http://dev.bitly.com/user_info.html#v3_user_link_history
        /// </summary>
        /// <param name="link"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="expandClientID">Ignored</param>
        /// <param name="isPrivate"></param>
        /// <param name="user"></param>
        /// <param name="created_after"></param>
        /// <param name="modified_after"></param>
        /// <param name="created_before"></param>
        /// <returns></returns>
        public BitlyResponse UserLinkHistory(string link = null, int limit = 50, int offset = 0,
// ReSharper disable InconsistentNaming
                                             bool expandClientID = false, bool isPrivate = false, string user = null, long created_after = 0, long modified_after = 0, long created_before = 0)
// ReSharper restore InconsistentNaming
        {
            var parameters = new Dictionary<string, string> { { "limit", limit.ToString(CultureInfo.InvariantCulture) }};

            if (offset > 0)
            {
                parameters.Add("offset", offset.ToString(CultureInfo.InvariantCulture));
            }
            if (created_after > 0)
            {
                parameters.Add("created_after", created_after.ToString(CultureInfo.InvariantCulture));
            }

            if (modified_after > 0)
            {
                parameters.Add("modified_after", modified_after.ToString(CultureInfo.InvariantCulture));
            }

            if (created_before > 0)
            {
                parameters.Add("created_before", created_before.ToString(CultureInfo.InvariantCulture));
            }

            //if (expandClientID)
            //{
            //    parameters.Add("expand_client_id ", "true");
            //}
            if (!string.IsNullOrWhiteSpace(user))
            {
                parameters.Add("user", user);
            }
            if (isPrivate)
            {
                parameters.Add("private", "true");
            }

            return GetResponseDict("user/link_history", parameters);
        }

        /// <summary>
        /// Retrieves Click Metrics for a shortened Link
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public BitlyResponse LinkMetrics(string link = null)
        {
            var parameters = new Dictionary<string, string> { { "link", link } };
            return GetResponseDict("link/clicks", parameters);
        }

    }


    // ReSharper disable InconsistentNaming
    // ReSharper disable CSharpWarnings::CS1591
    /// <summary>
    /// DataType for all returned Classes
    /// </summary>
    public class BitlyResponse
    {
        public int status_code { get; set; }
        public BitlyResponseData data { get; set; }
        public string status_txt { get; set; }
        
    }

    /// <summary>
    /// This represents the data part of the Bitly response
    /// </summary>
    public class BitlyResponseData
    {

        public List<BitlyExpandData> expand { get; set; }
        public List<BitlyInfoData> info { get; set; }
        public List<BitlyLinkLookupData> link_lookup { get; set; }
        public BitlyLinkSaveData link_save { get; set; }
        public List<BitlyLinkHistoryItem> link_history { get; set; }
        public BitlyLinkEdit link_edit { get; set; }

        //shorted
        public int new_hash { get; set; }
        public string url { get; set; }
        public string hash { get; set; }
        public string global_hash { get; set; }
        public string long_url { get; set; }
        
        //userinfo
        public string apiKey { get; set; }
        public string custom_short_domain { get; set; }
        public string display_name { get; set; }
        public string full_name { get; set; }
        public bool is_enterprise { get; set; }
        public string login { get; set; }
        public double member_since { get; set; }
        public string profile_image { get; set; }
        public string profile_url { get; set; }
        public List<BityShareAccount> share_accounts { get; set; }
        public List<string> tracking_domains { get; set; }
        public string default_link_privacy { get; set; }

        //User Link_history
        public long result_count { get; set; }
        /// <summary>
        /// domain_options is domain_preference_options in the documentation, the official documentation is wrong
        /// </summary>
        public List<string> domain_options { get; set; }

        //LinkMetrics
        public int link_clicks { get; set; }
        public string tz_offset { get; set; }
        public string unit { get; set; }
        public long? unit_reference_ts { get; set; }
        public int units { get; set; }
    }

    #region BityShareAccount
    public class BityShareAccount
    {
        public string account_id { get; set; }
        public string account_login { get; set; }
        public string account_name { get; set; }
        public string account_type { get; set; }
        public bool auto_import_links { get; set; }
        public string full_name { get; set; }
        public long numeric_id { get; set; }
        public bool primary { get; set; }
        public bool visible { get; set; }
    }

    #endregion
    #region expand

    public class BitlyExpandData
    {
        public string global_hash { get; set; }
        public string long_url { get; set; }
        public string short_url { get; set; }
        public string user_hash { get; set; }
    }
    #endregion

    #region info

    
    public class BitlyInfoData
    {
        public string short_url { get; set; }
        public string hash { get; set; }
        public string user_hash { get; set; }
        public string global_hash { get; set; }
        public string error { get; set; }
        public string title { get; set; }
        public string created_by { get; set; }
        public string created_at { get; set; }
    }

    #endregion



    #region LinkLookup

    public class BitlyLinkLookupData
    {
        public string url { get; set; }
        public string link { get; set; }
        public string aggregate_link { get; set; }
    }

    #endregion

    //#region Shorten

    //public class BitlyShortenData
    //{
    //    public int new_hash { get; set; }
    //    public string url { get; set; }
    //    public string hash { get; set; }
    //    public string global_hash { get; set; }
    //    public string long_url { get; set; }
    //}

    //#endregion

    #region LinkSave

    public class BitlyLinkSaveData
    {
        public int new_link { get; set; }
        public string aggregate_link { get; set; }
        public string link { get; set; }
        public string long_url { get; set; }
    }

    #endregion

    #region LinkHistoryItem

    /// <summary>
    /// http://dev.bitly.com/user_info.html#v3_user_link_history
    /// </summary>
    public class BitlyLinkHistoryItem
    {
        public string keyword_link { get; set; }//undocumented
        public string link { get; set; }
        public string aggregate_link { get; set; }
        public string long_url { get; set; }
        public bool archived { get; set; }
        [JsonProperty("Private")]
        public bool isPrivate  { get; set; }
        public long created_at { get; set; }
        public long user_ts { get; set; }
        public long? modified_at { get; set; }
        public string title { get; set; }
        public List<string> shares { get; set; }
        public string client_id { get; set; }
        public string note { get; set; }
    }
#endregion

#region LinkEdit
    public class BitlyLinkEdit
    {
        public string link { get; set; }
    }
#endregion
// ReSharper restore CSharpWarnings::CS1591
    // ReSharper restore InconsistentNaming
}