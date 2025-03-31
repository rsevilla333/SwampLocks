using System;
using System.Net;
using System.Net.Mail;


namespace SwampLocks.AlphaVantage.Email
{
    public class EmailNotificationService
    {
        private readonly string _smtpServer;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailNotificationService(string server, string user, string password)
        {
            _smtpServer = server;
            _smtpUsername = user;
            _smtpPassword = password;

            Console.WriteLine($"{_smtpServer} {_smtpUsername}  {_smtpPassword}");
        }

        public Task SendEmailNotification(string recipient, string subject, string body)
        {
            var client = new SmtpClient(_smtpServer, 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            return client.SendMailAsync(
                new MailMessage(from: _smtpUsername,
                    to: recipient,
                    subject,
                    body));
            
        }
    }
}