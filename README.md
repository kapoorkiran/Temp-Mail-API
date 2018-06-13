# Temp-Mail-API
Unofficial API for [TempMail](https://temp-mail.org) in .NET

It can be used to generate temporary emails, it can help in making bots or anything else.

# Usage
```csharp
// Example

var session = new Session();

// To get Mailbox
var Mails = session.GetMails();
```

# Dependencies
* [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack)
* [MimeKit](https://www.nuget.org/packages/MimeKit)
