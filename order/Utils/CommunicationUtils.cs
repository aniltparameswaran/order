using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using order.Models;
using System.Text;

namespace order.Utils
{
    public class CommunicationUtils
    {
        public bool SendMail(EmailModel emailModel)
        {
            try
            {

                // Build email body
                StringBuilder emailBody = new("<html><body>");
                emailBody.Append(emailModel.email_html_body);

                if (emailModel.signature_content != null)
                {
                    emailBody.Append(emailModel.signature_content);
                }

                // Create a new MimeMessage
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(MailboxAddress.Parse(emailModel.from_email.ToString()));
                mimeMessage.To.Add(MailboxAddress.Parse(emailModel.to_email.ToString()));

                /*  // Add CC recipients
                  if (!string.IsNullOrEmpty(emailModel.email_cc.ToString()))
                  {
                      foreach (var ccEmail in emailModel.email_cc.ToString().Split(','))
                      {
                          mimeMessage.Cc.Add(MailboxAddress.Parse(ccEmail.Trim()));
                      }
                  }*/

                mimeMessage.Subject = emailModel.subject.ToString();
                mimeMessage.Body = new TextPart(TextFormat.Html) { Text = emailBody.ToString() };

                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(emailModel.from_email.ToString(), emailModel.from_email_password.ToString());
                smtp.Send(mimeMessage);
                smtp.Disconnect(true);

                return true; // Success

            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false; // Failure
            }
        }
    }
}
