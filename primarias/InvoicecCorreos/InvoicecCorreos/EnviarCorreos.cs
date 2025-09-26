using clibLogger;
using Control;
using Datos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CriptoSimetrica;

namespace InvoicecCorreos
{
    public class EnviarCorreos
    {
        private AES cs = new AES();
        BasesDatos DB;
        private DbDataReader DR;
        private EnviarMail EM;
        private CrearPDF cPDF;
        private string compania = "UNACEM ECUADOR S.A.";

        #region Parametros
        private string servidor = "";
        private int puerto = 587;
        private Boolean ssl = false;
        private string emailCredencial = "";
        private string passCredencial = "";
        private string emailEnviar = "";
        private string emails = "";
        private string RutaDOC = "";
        #endregion

        public EnviarCorreos()
        {
            DB = new BasesDatos();
            cPDF = new CrearPDF();
            parametrosCorreo();
        }

        public void inicio()
        {
            try
            {
                DB.Conectar();
                DataSet dataSet = new DataSet();
                XDocument CRE = new XDocument(
                        new XElement("INSTRUCCION",
                            new XElement("FILTROS",
                                new XElement("OPCION", "3"))));
                dataSet = DB.TraerDataset("sp_consultaInfoProcesoAu", CRE.ToString());
                if (dataSet.Tables.Count > 0)
                {
                    foreach (DataRow dr in dataSet.Tables[0].Rows)
                    {
                        clsLogger.Graba_Log_Info("procesando idDocumento " + dr["idComprobante"].ToString());
                        enviaNotificacion(dr["idComprobante"].ToString(),
                            dr["ambiente"].ToString(),
                            dr["codigoControl"].ToString(),
                            dr["codDoc"].ToString(),
                            dr["estab"].ToString(),
                            dr["ptoEmi"].ToString(),
                            dr["secuencial"].ToString(),
                            dr["fecha"].ToString(),
                            dr["NOMREC"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error("error inicio " + ex.ToString());
            }
        }

        private void enviaNotificacion(string idComprobante, string ambiente, string codigoControl, string codDoc,
            string estab, string ptoEmi, string secuencial, string fechaEmision, string INrazonSocialComprador)
        {
            bool bandenv = false;
            String[] estabptoemi = obtener_codigo("estabptoemienv").Split(',');
            foreach (String resp in estabptoemi)
            {
                if (resp.Equals(estab + ptoEmi))
                {
                    bandenv = true;
                }
            }

            if (codDoc.Equals("06") && !bandenv)
            {
                clsLogger.Graba_Log_Info("Correo no enviado para idcomprobante " + idComprobante + ", guia que no se debe enviar.");
                actualizaTablaGeneral(idComprobante, "2");
                actualizaEnviocorreo(idComprobante, "2");
            }
            else
            {
                string nomDoc = "", asunto = "";
                try
                {
                    emails = recogerValorEmail(idComprobante);
                    if (emails == "")
                    {
                        clsLogger.Graba_Log_Info("Correo no enviado para idcomprobante " + idComprobante + ", no existen correos registrados");
                        actualizaTablaGeneral(idComprobante, "2");
                        actualizaEnviocorreo(idComprobante, "2");
                    }
                    else
                    {
                        nomDoc = CodigoDocumento(codDoc);
                        emails = emails.Trim(',');
                        EM = new EnviarMail();
                        clsLogger.Graba_Log_Info("recuperando correos  " + emails);
                        EM.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);

                        if (emails.Length > 10)
                        {
                            EM.adjuntar_xml(consulta_archivo_xml(codigoControl, 7), codigoControl + ".xml");
                            clsLogger.Graba_Log_Info("Se adjunto el xml ");
                            EM.adjuntar_pdf(cPDF.msPDF(codigoControl), codigoControl + ".pdf");
                            clsLogger.Graba_Log_Info("Se adjunto el PDF ");

                            asunto = nomDoc + " ELECTRONICA No: " + estab + "-" + ptoEmi + "-" + secuencial + " de " + compania;
                            string rutaImg = RutaDOC.Replace("docus\\", "imagenes\\logo_cima.png");
                            System.Net.Mail.LinkedResource image001 = new System.Net.Mail.LinkedResource(rutaImg, "image/png");
                            image001.ContentId = "image001";
                            image001.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                            System.Text.StringBuilder htmlBody = new System.Text.StringBuilder();
                            htmlBody.Append("<html>");
                            htmlBody.Append("<body>");
                            htmlBody.Append("<table style=\"width:100%;\">");
                            htmlBody.Append("<tr>");
                            htmlBody.Append("<td colspan=\"3\"></td>");
                            htmlBody.Append("</tr>");

                            htmlBody.Append("<tr>");
                            htmlBody.Append("<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td><td><br/>Estimado(a) " + INrazonSocialComprador +
                                "<br/><br/>Adjunto s&iacute;rvase encontrar su " + nomDoc + " ELECTRONICA No: " + estab + "-" + ptoEmi + "-" + secuencial +
                                @"&sup1; y el archivo PDF&sup2; de dicho 
                                comprobante que hemos emitido en nuestra empresa.<br/> Gracias por preferirnos.<br/><br/> Atentamente, " + " <br/> <img height=\"50\" width=\"50\" align=\"middle\" src=\"cid:image001\" /><br/>" + compania +                                
                                @"<br/>&sup1; El comprobante electr&oacute;nico es el archivo XML adjunto, le socilitamos que lo almacene de manera segura puesto que tiene validez tributaria." +
                                @"<br/>&sup2; La representaci&oacute;n impresa del comprobante electr&oacute;nico es el archivo PDF adjunto, y no es necesario que la imprima." +
                                @"</td><td> </td>");
                            htmlBody.Append("</tr>");
                            htmlBody.Append("</table>");
                            htmlBody.Append("</body>");
                            htmlBody.Append("</html>");

                            System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(htmlBody.ToString(), null, "text/html");
                            htmlView.LinkedResources.Add(image001);

                            string mailCCo = "";
                            switch (codDoc)
                            {
                                case "01":
                                    mailCCo = obtener_codigo("MAIL_COMERCIAL");
                                    break;
                                case "04":
                                    mailCCo = obtener_codigo("MAIL_COMERCIAL");
                                    break;
                                case "06":
                                    mailCCo = obtener_codigo("MAIL_COMERCIAL") + obtener_codigo("MAIL_GuiasRemision");

                                    break;
                                default:
                                    mailCCo = obtener_codigo("MAIL_FINANCIERO");
                                    break;
                            }
                            clsLogger.Graba_Log_Info("emailEnviar " + emailEnviar + " emails " + emails.Trim(',') + " mailCCo " + mailCCo);
                            EM.llenarEmailHTML(emailEnviar, emails.Trim(','), mailCCo, "", asunto, htmlView, compania);
                            try
                            {
                                clsLogger.Graba_Log_Info("Eviando correo..");
                                EM.enviarEmail();
                                actualizaTablaGeneral(idComprobante, "1");
                                actualizaEnviocorreo(idComprobante, "1");
                                clsLogger.Graba_Log_Info("Correo enviado.");
                            }
                            catch (System.Net.Mail.SmtpException ex)
                            {
                                clsLogger.Graba_Log_Info("Error envio. " + ex.ToString());
                                actualizaTablaGeneral(idComprobante, "3");
                                actualizaEnviocorreo(idComprobante, "3");
                            }
                        }
                    }
                }
                catch (Exception mex)
                {
                    DB.Desconectar();
                    clsLogger.Graba_Log_Info("ERROR ENVIAR MAIL2. Codigocontorl " + codigoControl + "  " + mex.Message);
                    actualizaEnviocorreo(idComprobante, "2");
                }
            }
        }

        private String recogerValorEmail(string idComprobante2)
        {
            try
            {
                String emails = "", destinatarioLF = "";
                DB.Conectar();
                DB.CrearComando(@" SELECT valor  FROM InfoAdicional  where nombre ='E-MAIL' and id_Comprobante =@id_Comprobante ");
                DB.AsignarParametroCadena("@id_Comprobante", idComprobante2);
                DbDataReader DR3 = DB.EjecutarConsulta();
                if (DR3.Read())
                {
                    emails = DR3[0].ToString().Trim(',');
                }
                DB.Desconectar();

                DB.Conectar();
                DB.CrearComando(@" SELECT valor  FROM InfoAdicional  where nombre ='destinatario' and id_Comprobante =@id_Comprobante ");
                DB.AsignarParametroCadena("@id_Comprobante", idComprobante2);
                DbDataReader DR4 = DB.EjecutarConsulta();
                if (DR4.Read())
                {
                    destinatarioLF = DR4[0].ToString().Trim(',');
                }
                DB.Desconectar();

                DB.Conectar();
                DB.CrearComando("SELECT top 1 emailsRegla FROM EmailsReglas  WHERE SUBSTRING(nombreRegla,1,6)=SUBSTRING(@rfcrec,1,6) AND estadoRegla=1 and eliminado=0");
                DB.AsignarParametroCadena("@rfcrec", destinatarioLF);
                DbDataReader DR5 = DB.EjecutarConsulta();
                if (DR5.Read())
                {
                    emails = emails.Trim(',') + "," + DR5[0].ToString().Trim(',') + "";
                }
                DB.Desconectar();
                return emails;
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error("recogerValorEmail " + ex.ToString());
                return "";
            }
        }

        private string CodigoDocumento(string p_codigo)
        {
            string desc = "";
            switch (p_codigo)
            {
                case "01":
                    desc = "FACTURA";
                    break;
                case "04":
                    desc = "NOTA DE CREDITO";
                    break;
                case "05":
                    desc = "NOTA DE DEBITO";
                    break;
                case "06":
                    desc = "GUIA DE REMISION";
                    break;
                case "07":
                    desc = "COMPROBANTE DE RETENCION";
                    break;
                default:
                    desc = "DOCUMENTO ELECTRONICO";
                    break;
            }
            return desc;
        }

        private void parametrosCorreo()
        {
            try
            {
                DB.Conectar();
                DB.CrearComando(@"select servidorSMTP,puertoSMTP,sslSMTP,userSMTP,passSMTP,
                                dirdocs,dirtxt,dirrespaldo,dircertificados,dirllaves,emailEnvio,
                                dirp12,passP12,dirXMLbase 
                                from ParametrosSistema");
                DR = DB.EjecutarConsulta();
                while (DR.Read())
                {
                    servidor = DR[0].ToString().Trim();
                    puerto = Convert.ToInt32(DR[1]);
                    ssl = Convert.ToBoolean(DR[2]);
                    emailCredencial = DR[3].ToString().Trim();
                    passCredencial = cs.desencriptar(DR[4].ToString().Trim(), "CIMAIT");
                    emailEnviar = DR[10].ToString().Trim();
                    RutaDOC = DR[5].ToString().Trim();
                }
                DB.Desconectar();
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error("error parametrosCorreo " + ex.ToString());
                DB.Desconectar();
            }
        }

        public MemoryStream consulta_archivo_xml(string p_codigoControl, int p_opcion)
        {
            MemoryStream rpt = new MemoryStream();
            string doc = "";
            try
            {
                DB.Conectar();
                DB.CrearComandoProcedimiento("PA_ARCHIVO_XML");
                DB.AsignarParametroProcedimiento("@documentoXML", System.Data.DbType.Xml, "0");
                DB.AsignarParametroProcedimiento("@codigoControl", System.Data.DbType.String, p_codigoControl);
                DB.AsignarParametroProcedimiento("@idComprobante", System.Data.DbType.Int32, "0");
                DB.AsignarParametroProcedimiento("@opcion", System.Data.DbType.Int32, p_opcion);
                DbDataReader dr = DB.EjecutarConsulta();
                if (dr.Read())
                {
                    doc = @"<?xml version=""1.0"" encoding=""UTF-8""?>" + dr[0].ToString();
                    rpt = GenerateStreamFromString(doc);
                }
                DB.Desconectar();
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error("error consulta_archivo_xml " + ex.ToString());
            }

            return rpt;
        }

        private MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        private string obtener_codigo(string a_parametro)
        {
            string retorna = ConfigurationManager.AppSettings.Get(a_parametro);
            return retorna;
        }

        private void actualizaTablaGeneral(string idComprobante, string creado)
        {
            DB.Desconectar();
            DB.Conectar();
            DB.CrearComando(@"UPDATE GENERAL SET creado = @creado  WHERE idComprobante = @idComprobante");
            DB.AsignarParametroCadena("@creado", creado);
            DB.AsignarParametroCadena("@idComprobante", idComprobante);
            DB.EjecutarConsulta1();
            DB.Desconectar();
        }

        private void actualizaEnviocorreo(string idComprobante, string enviado)
        {
            DB.Desconectar();
            DB.Conectar();
            DB.CrearComando(@"UPDATE GENERAL SET enviado = @enviado  WHERE idComprobante = @idComprobante");
            DB.AsignarParametroCadena("@enviado", enviado);
            DB.AsignarParametroCadena("@idComprobante", idComprobante);
            DB.EjecutarConsulta1();
            DB.Desconectar();
        }
    }
}
