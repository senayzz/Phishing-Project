using System.Net;
using System.Net.Mail;


namespace WebProject.Functions;

public class SMTP
{
    public string senderAddress { get; set; }
    public string password { get; set; }
    public string receiverAddress { get; set; }
    
    public SMTP(string ReceiverAddress)
    {
        this.senderAddress = "200706001@st.maltepe.edu.tr";
        this.password = "Maltepe36712";
        this.receiverAddress = ReceiverAddress;
    }
    
    public void SendMail(string message, string subject)
    {
        try
        {
            MailMessage mailMessage = new MailMessage(); 
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Port = 587; // Şifrelenmemiş Port : 587 , TLS,SSL Portu : 465
            smtpClient.Host = "smtp-mail.outlook.com"; // Kullandığınız her bir mail sunucusunun farklı hostları olabilir.
            smtpClient.EnableSsl = true; // SSL’in açılımı Secure Socket Layer’dır. Türkçe anlamıysa Güvenli Giriş Katmanı’dır.
            smtpClient.Credentials = new NetworkCredential(senderAddress, password); // Gönderen Kişinin Mail Adresi ve Şifresi 
            mailMessage.From = new MailAddress(senderAddress); // Gönderen Kişinin Mail Adresi
            mailMessage.To.Add(receiverAddress); // Alıcının Mail Adresi
            mailMessage.Subject = subject; // Mail'inizin Konusu
            mailMessage.Body = message; // Mail mesajı(içeriği)
            mailMessage.IsBodyHtml = true;
            smtpClient.Send(mailMessage); // SMTP İsteği
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.ToString());
        }

    }
    
    
}