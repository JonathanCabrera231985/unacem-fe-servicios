using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Datos;
using System.Data.Common;
using clibLogger;

namespace Control
{
    public class EnviarPDFGuia
    {
        EnviarMail EM;
        //BasesDatos DB;
        //private DbDataReader DR;
        private string servidor = "", emailCredencial = "", passCredencial = "", emailEnviar = "", asunto="", correocliente="";
        int puerto;
        Boolean ssl;
        private string compania = "UNACEM ECUADOR S.A.";
        

        public EnviarPDFGuia()
        {
            EM = new EnviarMail();
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando(@"select servidorSMTP,puertoSMTP,sslSMTP,userSMTP,passSMTP,emailEnvio
							  from ParametrosSistema with(nolock)");
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        servidor = DR[0].ToString().Trim();
                        puerto = Convert.ToInt32(DR[1]);
                        ssl = Convert.ToBoolean(DR[2]);
                        emailCredencial = DR[3].ToString().Trim();
                        passCredencial = DR[4].ToString().Trim();
                        emailEnviar = DR[5].ToString().Trim();
                    }
                }

                DB.Desconectar();

            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                throw;
            }
            finally
            {
                DB.Desconectar();
            }
           
        }


         public void envia_guia(MemoryStream pdf ,string  codigoControl, string estab, string ptemi) 
         {
             Log lg1 = new Log();

                clsLogger.Graba_Log_Info("inicio envia_guia");
                clsLogger.Graba_Log_Info("servidor " + servidor + " puerto " + puerto + " ssl " + ssl + " emailCredencial " + emailCredencial + " passCredencial " + passCredencial);
             EM.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);
                clsLogger.Graba_Log_Info("adjuntando PDF");
             EM.adjuntar_xml(pdf, codigoControl + ".pdf");

             if ((estab.Equals(obtener_codigo("estabprod")) && ptemi.Equals(obtener_codigo("ptoemiprod"))) || (estab.Equals(obtener_codigo("estabprue")) && ptemi.Equals(obtener_codigo("ptoemiprue"))))
             {
                 asunto = "Envío de guia mina para imprimir documento "; 
             }
             else 
             {
                 asunto = "Envío de guia para imprimir documento ";
             }

             

             System.Text.StringBuilder htmlBody = new System.Text.StringBuilder();
             htmlBody.Append("<html>");
             htmlBody.Append("<body>");
             htmlBody.Append("<table style=\"width:100%;\">");
             htmlBody.Append("<tr>");
             htmlBody.Append("<td colspan=\"3\"></td>");
             htmlBody.Append("</tr>");

             htmlBody.Append("<tr>");
             htmlBody.Append("<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td><td><br/>Estimado(a) <br/><br/><br/><br/><br/>" +
             @"<br/>--------------------------------------------------------------------------------" +
             @"</td><td> </td>");
             htmlBody.Append("</tr>");
             htmlBody.Append("</table>");
             htmlBody.Append("</body>");
             htmlBody.Append("</html>");

             System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(htmlBody.ToString(), null, "text/html");

             correocliente = System.Configuration.ConfigurationManager.AppSettings.Get("correoclientePDFguias");
            clsLogger.Graba_Log_Info("llenarEmailHTML ");
            clsLogger.Graba_Log_Info("emailEnviar " + emailEnviar + " correocliente " + correocliente );
             EM.llenarEmailHTML(emailEnviar, correocliente, "", "", asunto, htmlView, compania);
             try
             {
                 EM.enviarEmail();
                clsLogger.Graba_Log_Info("guía enviada..");
             }
             catch (Exception ex) 
             {
                clsLogger.Graba_Log_Error("error al enviar pdf guia a imprimir ");
                clsLogger.Graba_Log_Error("error -- > "+ ex.ToString());
             }
             

         }


         private string obtener_codigo(string a_parametro)
         {
             string retorna = System.Configuration.ConfigurationManager.AppSettings.Get(a_parametro);

             return retorna;
         }

    }
}
