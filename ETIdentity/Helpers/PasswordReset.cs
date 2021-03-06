using System.Net.Mail;

namespace ETIdentity.Helpers
{
    public static class PasswordReset
    {
        public static void PasswordResetSendEmail(string link)
        {
            MailMessage mail = new MailMessage();

            SmtpClient smtpClient = new SmtpClient("mail.teknohub.net");

            mail.From = new MailAddress("fcakiroglu@teknohub.net");
            mail.To.Add("f-cakiroglu@outlook.com");

            mail.Subject = $"Şifre sıfırlama";
            mail.Body = "<h2>Şifrenizi yenilemek için lütfen aşağıdaki linke tıklayınız.</h2></hr>";
            mail.Body += $"<a href='{link}'>şifre yenileme linki</a>";
            mail.IsBodyHtml = true;

            smtpClient.Port = 587;
            smtpClient.Credentials = new System.Net.NetworkCredential("fcakiroglu@teknohub.net", "FatihFatih30");

            smtpClient.Send(mail);
        }
    }
}
