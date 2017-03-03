using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;

public class HttpClient : WebClient
{
    public CookieContainer CookieContainer { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool AllowAutoRedirect { get; set; }
    public bool KeepAlive { get; set; }

    public HttpClient()
    {
        CookieContainer = new CookieContainer();
        AllowAutoRedirect = true;
        KeepAlive = true;
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
        var request = (HttpWebRequest)base.GetWebRequest(address);
        request.CookieContainer = CookieContainer;
        request.KeepAlive = KeepAlive;
        request.AllowAutoRedirect = AllowAutoRedirect;
        return request;
    }

    protected override WebResponse GetWebResponse(WebRequest request)
    {
        WebResponse Response = null;
        try { Response = request.GetResponse(); }
        catch (WebException wb) { Response = ((HttpWebResponse)wb.Response); }

        StatusCode = ((HttpWebResponse)Response).StatusCode;

        if (this.CookieContainer == null)
            this.CookieContainer = ExtractCookies(Response);

        return Response;
    }

    private CookieContainer ExtractCookies(WebResponse Response)
    {
        var cookieContainer = new CookieContainer();
        List<string> list = new List<string>() { "expires", "path", "domain", "max-age" };

        var setCookie = Response.Headers["Set-Cookie"].Split(';').ToList();

        for (int i = 0; i < setCookie.Count; i++)
        {
            setCookie[i] = setCookie[i].Trim().ToLower().Replace("httponly,", "");
            var x = setCookie[i].Count(c => c == ',') > 1;
            if ((!setCookie[i].Contains("expires") && setCookie[i].Contains(",")) || (setCookie[i].Contains("expires") && setCookie[i].Count(c => c == ',') > 1))
            {
                var temp = setCookie[i].Split(',');
                setCookie[i] = temp[0];
                if (!temp[0].Contains("expires"))
                    setCookie.Insert(i + 1, temp[1]);
                else
                    setCookie.Insert(i + 1, temp[2]);
            }
        }

        Cookie tempCookie = new Cookie();
        for (int i = 0; i < setCookie.Count; i++)
        {
            if (!setCookie[i].Contains("="))
                continue;

            var temp = setCookie[i].Split('=');
            if (list.TrueForAll(k => !temp[0].Contains(k)))
            {
                if (tempCookie.Name != "")
                    cookieContainer.Add(tempCookie);

                tempCookie = new Cookie();
                tempCookie.Name = temp[0];
                tempCookie.Value = temp[1];
            }

            if (temp[0] == "path")
                tempCookie.Path = temp[1];
            else if (temp[0] == "domain")
                tempCookie.Domain = (temp[1].First() == '.') ? temp[1].Substring(1) : temp[1];

            if (i == setCookie.Count - 1 && tempCookie.Name != "")
                cookieContainer.Add(tempCookie);
        }

        return cookieContainer;
    }

}