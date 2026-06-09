using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace LanguageCenter.Services
{
    public static class EmailService
    {
        public static bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SmtpUser"]) &&
                       !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SmtpPass"]);
            }
        }

        public static bool TrySendClassRegistration(
            string toEmail,
            string fullName,
            string className,
            DateTime? startDate,
            string room,
            out string error)
        {
            error = null;

            if (!IsConfigured || string.IsNullOrWhiteSpace(toEmail))
                return false;

            string centerName = ConfigurationManager.AppSettings["CenterName"] ?? "Language Center";
            string body = string.Format(
                "<h2>Class Registration Received</h2>" +
                "<p>Dear <strong>{0}</strong>,</p>" +
                "<p>Your registration has been saved and is waiting for confirmation.</p>" +
                "<table style='width:100%;border-collapse:collapse'>" +
                "<tr><td style='padding:8px;font-weight:bold'>Class</td><td style='padding:8px'>{1}</td></tr>" +
                "<tr><td style='padding:8px;font-weight:bold'>Start date</td><td style='padding:8px'>{2}</td></tr>" +
                "<tr><td style='padding:8px;font-weight:bold'>Room</td><td style='padding:8px'>{3}</td></tr>" +
                "<tr><td style='padding:8px;font-weight:bold'>Status</td><td style='padding:8px'>Pending</td></tr>" +
                "</table>",
                HttpUtility.HtmlEncode(fullName),
                HttpUtility.HtmlEncode(className),
                startDate.HasValue ? startDate.Value.ToString("dd/MM/yyyy") : "Updating",
                HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(room) ? "Updating" : room));

            return TrySend(
                toEmail,
                "[" + centerName + "] Class Registration Confirmation",
                body,
                out error);
        }

        private static bool TrySend(string toEmail, string subject, string bodyHtml, out string error)
        {
            error = null;

            try
            {
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                int smtpPort;
                if (!int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out smtpPort))
                    smtpPort = 587;

                string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
                string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string centerName = ConfigurationManager.AppSettings["CenterName"] ?? "Language Center";

                if (string.IsNullOrWhiteSpace(fromEmail))
                    fromEmail = smtpUser;

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(fromEmail, centerName);
                    message.To.Add(toEmail);
                    message.Subject = subject;
                    message.IsBodyHtml = true;
                    message.Body =
                        "<div style='font-family:Arial,sans-serif;max-width:640px;margin:auto'>" +
                        bodyHtml +
                        "<hr><p style='color:#6b7280;font-size:12px'>" +
                        HttpUtility.HtmlEncode(centerName) + " - " + DateTime.Now.Year +
                        "</p></div>";

                    using (var smtp = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtp.EnableSsl = true;
                        smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                        smtp.Send(message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
