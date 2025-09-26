using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace InvoicecCorreos
{
    public class EnviarMail
    {
        SmtpClient mSmtpClient = new SmtpClient();
        MailMessage mMailMessage = new MailMessage();

        /// <summary> 
        /// Envia un Email
        /// </summary>
        /// <param name="from">Remitente</param>
        /// <param name="to">Receptor</param>
        /// <param name="bcc">Bcc Receptor</param>
        /// <param name="cc">Cc Receptor</param>
        /// <param name="subject">Cabecero del mensaje</param>
        /// <param name="body">Cuerpo del mensaje</param>
        public void servidorSTMP(String servidor, Int32 puerto, Boolean ssl, String emailCredencial, String passCredencial)
        {
            mSmtpClient.Timeout = 5000;
            mSmtpClient.Host = servidor; //aqui poner el smtp de mexexpress
            mSmtpClient.Port = puerto;
            mSmtpClient.EnableSsl = ssl;
            mSmtpClient.Credentials = new System.Net.NetworkCredential(emailCredencial, passCredencial);
        }

        public void adjuntar(String ruta)
        {
            if (File.Exists(ruta))
            {
                mMailMessage.Attachments.Add(new Attachment(ruta));
            }
        }

        public void adjuntar_xml(MemoryStream doc, string file)
        {
            mMailMessage.Attachments.Add(new Attachment(doc, file));
        }

        public void adjuntar_pdf(MemoryStream doc, string file)
        {
            //mMailMessage.Attachments.Add(new Attachment(doc, file));
            // Asegurarse de que la Stream esté posicionada al principio del archivo
            doc.Seek(0, SeekOrigin.Begin);

            // Crear un objeto Attachment utilizando la Stream
            Attachment attachment = new Attachment(doc, file);

            // Agregar el Attachment al objeto MailMessage
            mMailMessage.Attachments.Add(attachment);
            // mMailMessage.Attachments.Add(new Attachment(doc, file));
        }

        public void llenarEmail(string from, string to, string bcc, string cc, string subject, string body)
        {
            mMailMessage.From = new MailAddress(from);
            to = to.Replace(';', ',');
            bcc = bcc.Replace(';', ',');
            cc = cc.Replace(';', ',');
            String[] destinatarios = to.Split(',');
            foreach (String email in destinatarios)
            {
                if (CheckEmail(email.Trim()))
                {
                    mMailMessage.To.Add(new MailAddress(email.Trim()));
                }
            }


            if ((bcc != null) && (bcc != string.Empty)) mMailMessage.Bcc.Add(new MailAddress(bcc));
            if ((cc != null) && (cc != string.Empty)) mMailMessage.CC.Add(new MailAddress(cc));

            mMailMessage.Subject = subject;
            mMailMessage.Body = body;
            mMailMessage.IsBodyHtml = true;
            mMailMessage.Priority = MailPriority.Normal;
        }

        public void llenarEmailHTML(string from, string to, string bcc, string cc, string subject, AlternateView body, string compania)
        {
            to = to.Replace(';', ',');
            bcc = bcc.Replace(';', ',');
            cc = cc.Replace(';', ',');
            //-----------------------PLAIN TEXT VIEW FOR EMAIL--------------------------------
            string plainTextBody = "Documento electr&oacute;nico de " + compania;
            System.Net.Mail.AlternateView plainTextView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(plainTextBody, null, System.Net.Mime.MediaTypeNames.Text.Plain);
            mMailMessage.AlternateViews.Add(plainTextView);

            mMailMessage.AlternateViews.Add(body);

            mMailMessage.From = new MailAddress(from);
            String[] destinatarios = to.Split(',');
            foreach (String email in destinatarios)
            {
                mMailMessage.To.Add(new MailAddress(email));
            }

            if ((bcc != null) && (bcc != string.Empty))
            {
                String[] copiaoculta = bcc.Split(',');
                foreach (String email2 in copiaoculta)
                {
                    mMailMessage.Bcc.Add(new MailAddress(email2));
                    //mMailMessage.To.Add(new MailAddress(email));
                }
            }


            //  if ((bcc != null) && (bcc != string.Empty)) mMailMessage.Bcc.Add(new MailAddress(bcc));
            if ((cc != null) && (cc != string.Empty)) mMailMessage.CC.Add(new MailAddress(cc));

            mMailMessage.Subject = subject;
            //mMailMessage.Body = body;
            mMailMessage.IsBodyHtml = true;
            mMailMessage.Priority = MailPriority.Normal;

        }
        public void enviarEmail()
        {

            if (mMailMessage.To.Count > 0)
            {
                mSmtpClient.Send(mMailMessage);
            }

        }

        public bool CheckEmail(string EmailAddress)
        {
            string strPattern = "^([0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";

            if (System.Text.RegularExpressions.Regex.IsMatch(EmailAddress, strPattern))
            {
                return true;
            }

            return false;
        }

        public string CheckGrupoEmail(string GrupoEmailAddress)
        {
            string strPattern = "^([0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";
            string mails = "";

            String[] destinatarios = GrupoEmailAddress.Split(',');
            foreach (String email in destinatarios)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(email, strPattern))
                {
                    mails += email + ",";
                }
            }

            mails = mails.Trim();

            return mails;
        }
    }
}
