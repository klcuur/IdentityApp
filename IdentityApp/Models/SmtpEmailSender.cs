using System.Net;
using System.Net.Mail;

namespace IdentityApp.Models
{
    public class SmtpEmailSender : IEMailSender
    {

        private string? _host;

        private int _port;

        private bool _enableSSl;

        private string? _username;
        private string? _password;

        public SmtpEmailSender(string? host, int port, bool enableSSL, string? username, string? password)
        {
            _host = host;
            _port = port;
            _enableSSl = enableSSL;
            _username = username;
            _password = password;

        }
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_username, _password),
                EnableSsl = _enableSSl
            };

            return client.SendMailAsync(new MailMessage(_username ?? "", email, subject, message) { IsBodyHtml = true });

        }
    }
}