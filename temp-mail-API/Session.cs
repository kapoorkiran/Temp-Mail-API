using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System;

namespace TempMail
{
    class Session
    {

        private readonly Dictionary<string, Regex> RegexObjects = new Dictionary<string, Regex>()
        {
            {"Lines", new Regex("(\n|\r|\r\n)")},
            {"SpacesBetweenTags", new Regex(@">\s*<")},
            {"Domains", new Regex("<select name=\"domain\" class=\"form-control\" id=\"domain\">(<option value=\"(?<domain>.*?)\">.*?</option>)+</select>")},
            {"MailsIds", new Regex("https://temp-mail.org/en/view/(?<id>.*?)\"")},
            {"Mail", new Regex("class=\"mail opentip\" value=\"(?<email>.*?)\"")}
        };

        public List<string> AvailableDomains { get { return this.GetAvailableDomains(); } }
        public List<Mail> Mails { get { return this.GetMails(); } }
        public string Email { get; set; }

        private CookieContainer _cookies;

        public Session()
        {
            CreateNewSession();
        }

        /// <summary>
        /// Gets the Mailbox of the temporary email.
        /// </summary>
        public List<Mail> GetMails()
        {
            List<Mail> Mails = new List<Mail>();

            var Client = CreateHttpClient();
            string Response = Client.DownloadString("https://temp-mail.org/en/option/check");

            Response = RegexObjects["Lines"].Replace(Response, "");
            Response = RegexObjects["SpacesBetweenTags"].Replace(Response, "><");

            List<string> ids = new List<string>();

            var Matches = RegexObjects["MailsIds"].Matches(Response);
            foreach (Match match in Matches)
            {
                var id = match.Groups["id"].Value;
                if (!ids.Contains(id))
                {
                    Mails.Add(Mail.FromID(this, id));
                    ids.Add(id);
                }
            }

            return Mails;
        }

        /// <summary>
        /// Changes the temporary email to ex: Login@Domain .
        /// </summary>
        /// <param name="Login">New temporary email login</param>
        /// <param name="Domain">New temporary email domain</param>
        public bool Change(string Login, string Domain)
        {
            if (!this.AvailableDomains.Contains(Domain))
                throw new Exception("The domain you entered isn't an available domain");

            var Client = CreateHttpClient();

            Client.Headers.Add("Referer", "https://temp-mail.org/en/option/change");
            Client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var csrf = Client.CookieContainer.GetCookies(new Uri("https://temp-mail.org/"))["csrf"];
            var data = string.Format("csrf={0}&mail={1}&domain={2}", csrf.Value, Login, "@" + Domain);

            var res = Client.UploadString("https://temp-mail.org/en/option/change", data);

            if (Client.StatusCode == HttpStatusCode.OK)
            {
                this.Email = string.Format("{0}@{1}", Login, Domain);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes the temporary email and gets a new one.
        /// </summary>
        public bool Delete()
        {
            var Client = CreateHttpClient();

            var res = Client.DownloadString("https://temp-mail.org/en/option/delete");

            if (Client.StatusCode == HttpStatusCode.OK)
            {
                var obj = (Dictionary<string, object>)new JavaScriptSerializer().Deserialize<object>(res);
                this.Email = obj["mail"].ToString();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Sends a get request to the Url provided using this session cookies and returns the string result.
        /// </summary>
        /// <param name="Url"></param>
        public string GET(string Url)
        {
            return CreateHttpClient().DownloadString(Url);
        }

        /// <summary>
        /// Gets the available domains.
        /// </summary>
        private List<string> GetAvailableDomains()
        {
            List<string> domains = new List<string>();

            var Client = CreateHttpClient();

            var res = Client.DownloadString("https://temp-mail.org/en/option/change");

            res = RegexObjects["Lines"].Replace(res, "");
            res = RegexObjects["SpacesBetweenTags"].Replace(res, "><");

            var Matches = RegexObjects["Domains"].Matches(res);
            if (Matches.Count > 0)
                foreach (Capture capture in Matches[0].Groups["domain"].Captures)
                    domains.Add(capture.Value.Substring(1));

            this._cookies = Client.CookieContainer;

            return domains;
        }

        private void CreateNewSession()
        {
            var Client = CreateHttpClient();

            var Response = Client.DownloadString("https://temp-mail.org/en/");

            var Matches = RegexObjects["Mail"].Matches(Response);
            if (Matches.Count > 0)
                this.Email = Matches[0].Groups["email"].Value;

            this._cookies = Client.CookieContainer;
        }

        /// <summary>
        /// Returns an HttpClient that have this session cookies.
        /// </summary>
        private HttpClient CreateHttpClient()
        {
            HttpClient Client = new HttpClient();
            Client.Headers.Add("Accept", "*/*");
            Client.Headers.Add("Accept-Language", "en-US,en");
            Client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
            Client.Headers.Add("Host", "temp-mail.org");
            Client.Headers.Add("Origin", "https://temp-mail.org");
            Client.Headers.Add("X-Requested-With", "XMLHttpRequest");
            Client.Headers.Add("Upgrade-Insecure-Requests", "1");
            Client.CookieContainer = this._cookies;
            return Client;
        }

    }
}
