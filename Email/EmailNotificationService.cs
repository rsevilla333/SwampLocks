using System;
using System.Net;
using System.Net.Mail;

namespace SwampLocks.EmailSevice
{
    // Email Service class 
    public class EmailNotificationService
    {
        private readonly string _smtpServer;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly int _serverPort;

        // Email Service Construtor
        public EmailNotificationService(string server, string user, string password, int port = 465)
        {
            _smtpServer = server;
            _smtpUsername = user;
            _smtpPassword = password;
            _serverPort = port;

            Console.WriteLine($"{_smtpServer} {_smtpUsername}  {_smtpPassword}");
        }

        // Sends Email
        public Task SendEmailNotification(string recipient, string subject, string body)
        {
            var client = new SmtpClient(_smtpServer, 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            };

            Console.WriteLine($"Sending email to {recipient}");

            return client.SendMailAsync(
                new MailMessage(from: _smtpUsername,
                    to: recipient,
                    subject,
                    body));
        }
    }
}
