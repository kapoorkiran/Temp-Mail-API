using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TempMail
{
    class Mail
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string To { get; set; }
        public string Date { get; set; }

        /// <summary>
        /// Return the Mail object of the mail that have the Id provided.
        /// </summary>
        /// <param name="session">The session which mail belongs to.</param>
        /// <param name="Id">The Id of the mail.</param>
        public static Mail FromID(Session session, string Id)
        {
            Mail mail = new Mail();

            string source_url = string.Format("https://temp-mail.org/en/source/{0}", Id);

            var source = session.GET(source_url);
            var result = Regex.Split(source, "\r\n|\n|\r");

            foreach (var line in result)
            {
                if (line.Length > 0 && line[0] != ' ' && line[0] != '\t' && line.Contains(":"))
                {
                    var index = line.IndexOf(':');

                    var name = line.Substring(0, index);
                    var value = line.Substring(index + 1).Trim();

                    if (name == "Subject")
                        mail.Subject = value;
                    else if (name == "From")
                        mail.From = value;
                    else if (name == "To")
                        mail.To = value;
                    else if (name == "Date")
                        mail.Date = value;
                }
            }

            mail.Content = new Regex("--.*\r\nContent-Type: text/plain; charset=UTF-8\r\n\r\n(?<text>.*?)\r\n\r\n--.*", RegexOptions.Singleline).Match(source).Groups["text"].Value.Trim();

            return mail;
        }

    }
}