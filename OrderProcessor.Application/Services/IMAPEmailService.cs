using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderProcessor.Application.Services;

public class IMAPEmailService
{
  private readonly string _imapServer;
  private readonly int _imapPort;
  private readonly string _username;
  private readonly string _password;

  public IMAPEmailService(string imapServer, int imapPort, string username, string password)
  {
    _imapServer = imapServer;
    _imapPort = imapPort;
    _username = username;
    _password = password;
  }

  public async Task<List<MimeMessage>> FetchUnreadEmailsAsync()
  {
    var messages = new List<MimeMessage>();

    using (var client = new ImapClient())
    {
      client.ServerCertificateValidationCallback = (s, c, h, e) => true;
      await client.ConnectAsync(_imapServer, _imapPort, SecureSocketOptions.SslOnConnect);
      await client.AuthenticateAsync(_username, _password);

      var inbox = client.Inbox;
      await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

      foreach (var uid in await inbox.SearchAsync(MailKit.Search.SearchQuery.NotSeen))
      {
        var message = await inbox.GetMessageAsync(uid);
        messages.Add(message);
      }

      await client.DisconnectAsync(true);
    }

    return messages;
  }
}