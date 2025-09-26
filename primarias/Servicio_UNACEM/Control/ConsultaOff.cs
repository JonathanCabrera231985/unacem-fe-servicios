using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Datos;
using System.Data.Common;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using CriptoSimetrica;
using clibLogger;

namespace Control
{
    public class ConsultaOff
    {

        string ambiente, codigoControl, claveAcceso, idComprobante;
        string edo = "", msjSRI = "", msjT = "", numeroAutorizacion = "", fechaAutorizacion = "", msj = "";
        string codDoc; string estab; string ptoEmi; string secuencial; string fechaEmision;
        private receWeb.RecepcionComprobantesService recepcion;
        private string compania = "UNACEM ECUADOR S.A.";
        private string xsd = "";
        private string servidor = "";
        private int puerto = 587;
        private Boolean ssl = false;
        private string emailCredencial = "";
        private string passCredencial = "";
        private string RutaDOC = "";
        private string RutaTXT = "";
        private string RutaBCK = "";
        private string RutaCER = "";
        private string RutaKEY = "";
        private string emailEnviar = "";
        private string RutaP12 = "";
        private string PassP12 = "";
        private string RutaXMLbase = "";
        FirmarXML firmaXADES;
        private AES Cs;
        Key_Electronica.Key_Electronica FirmaBCE;
        public WebUNASAP.ZECSRIFM01 webunasaps = new WebUNASAP.ZECSRIFM01();
        public TestUnacemSap.ZECSRIFM01 webunasapsTEST = new TestUnacemSap.ZECSRIFM01();

        private autoWeb.AutorizacionComprobantesService autorizacion;
        //BasesDatos DB;
        //BasesDatos DB2;
        //private DbDataReader DR;
        private Log log;
        private EnviarMail EM;
        private CrearPDF cPDF;
        private string emails = "";

        public ConsultaOff()
        {
            autorizacion = new autoWeb.AutorizacionComprobantesService();
            recepcion = new receWeb.RecepcionComprobantesService();
            //DB = new BasesDatos();
            //DB2 = new BasesDatos();
            firmaXADES = new FirmarXML();
            Cs = new AES();
            log = new Log();
            cPDF = new CrearPDF();
            FirmaBCE = new Key_Electronica.Key_Electronica();
            parametrosSistema();
        }

        public void consultarDocumentoOffline(string idComprobante, string claveAcceso, string ambiente, string codigoControl,
            string codDoc, string estab, string ptoEmi, string secuencial, string fechaEmision, string INrazonSocialComprador)
        {
            this.ambiente = ambiente;
            this.codigoControl = codigoControl;
            this.claveAcceso = claveAcceso;
            this.idComprobante = idComprobante;
            this.codDoc = codDoc; this.estab = estab; this.ptoEmi = ptoEmi;
            this.secuencial = secuencial;
            this.fechaEmision = fechaEmision;


            XmlDocument xDocF2 = new XmlDocument();
            clsLogger.Graba_Log_Info("consulta_archivo_xml_2  " + codigoControl);
            string xDocF = consulta_archivo_xml_2(codigoControl, 5);
            if (!string.IsNullOrEmpty(xDocF))
            {
                xDocF2.LoadXml(xDocF);
                xDocF = "";
            }
            clsLogger.Graba_Log_Info("firmando documento  " + codigoControl);
            if (firmaXADES.Firmar(RutaP12, PassP12, xDocF2, out xDocF))
            {
                byte[] bytesXML = Encoding.Default.GetBytes(xDocF);
                msjSRI = "";
                xDocF2.LoadXml(xDocF);
                procesa_archivo_xml(xDocF2, codigoControl, "0", 2);

                clsLogger.Graba_Log_Info("enviando comprobante al sri  " + codigoControl);
                log.mensajesLog("US001", "", "enviando comprobante al sri ", "", codigoControl, "leerReenvioSRI");
                bool genero = enviarComprobante(bytesXML);
                if (genero)
                {
                    clsLogger.Graba_Log_Info("Validando autorizacion  " + codigoControl);
                    log.mensajesLog("US001", "", "Validando autorizacion ", "", codigoControl, "leerReenvioSRI");
                    bool validaauto = validarAutorizacion(claveAcceso);
                    if (validaauto)
                    {
                        clsLogger.Graba_Log_Info("documento autorizado  " + codigoControl);
                        if (codDoc.Equals("01"))
                        {
                            clsLogger.Graba_Log_Info("enviando correo " + codigoControl);
                            log.mensajesLog("US001", "", "enviando fcturas3 ", "", codigoControl, "leerReenvioSRI");
                            //enviar_correo(INrazonSocialComprador);
                            clsLogger.Graba_Log_Info("enviando correo ok" + codigoControl);
                        }

                        log.mensajesLog("EM010", msjSRI, msjT, "", codigoControl, "Final Autorizado OffLine");
                    }
                }

            }


        }



        private XmlDocument Archivoxml(string p_codigoControl, string p_opcion)
        {
            XmlDocument xDocF2 = new XmlDocument();

            BasesDatos DB = new BasesDatos();
            try
            {
                string doc = "";
                DB.Conectar();
                DB.CrearComandoProcedimiento("PA_ARCHIVO_XML");
                DB.AsignarParametroProcedimiento("@documentoXML", System.Data.DbType.Xml, "0");
                DB.AsignarParametroProcedimiento("@codigoControl", System.Data.DbType.String, p_codigoControl);
                DB.AsignarParametroProcedimiento("@idComprobante", System.Data.DbType.Int32, "0");
                DB.AsignarParametroProcedimiento("@opcion", System.Data.DbType.Int32, p_opcion);
                using (DbDataReader dr = DB.EjecutarConsulta())
                {
                    if (dr.Read())
                    {
                        doc = @"<?xml version=""1.0"" encoding=""UTF-8""?>" + dr[0].ToString();
                        xDocF2.LoadXml(doc);
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

            return xDocF2;



        }

        public string consulta_archivo_xml_2(string p_codigoControl, int p_opcion)
        {
            string rpt = "";
            string valorXML = "";

            BasesDatos DB = new BasesDatos();
           

            try
            {
                DB.Conectar();
                DB.CrearComandoProcedimiento("PA_ARCHIVO_XML");
                DB.AsignarParametroProcedimiento("@documentoXML", System.Data.DbType.Xml, "");
                DB.AsignarParametroProcedimiento("@codigoControl", System.Data.DbType.String, p_codigoControl);
                //DB.AsignarParametroProcedimiento("@idComprobante", System.Data.DbType.Int32, "");
                DB.AsignarParametroProcedimiento("@idComprobante", System.Data.DbType.String, "");
                DB.AsignarParametroProcedimiento("@opcion", System.Data.DbType.Int32, p_opcion);
                using (DbDataReader dr = DB.EjecutarConsulta())
                {
                    if (dr.Read())
                    {
                        rpt = @"<?xml version=""1.0"" encoding=""UTF-8""?>" + dr[0].ToString();
                        valorXML = dr[0].ToString();
                    }
                }

                DB.Desconectar();
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error("US001" + "Error en proceso consulta_archivo_xml. " + ex.Message);
            }

            if (String.IsNullOrEmpty(valorXML))
            {
                regularizarXML(p_codigoControl);
            }

            return rpt;
        }


        private void regularizarXML(String p_codigoControl)
        {
            string idComprobante = "";
            string numeroDocumento = "";
            string idLog = "";
            BasesDatos DB = new BasesDatos();

            string paso = "";
            try
            {
                paso = "1";
                DB.Conectar();
                DB.CrearComando(@"select idComprobante, codDoc+estab+ptoEmi+secuencial as numeroDocumento
                                    from GENERAL with(nolock) where codigoControl =@p_codigoControl");
                DB.AsignarParametroCadena("@p_codigoControl", @p_codigoControl);
                using (DbDataReader DR1 = DB.EjecutarConsulta())
                {
                    if (DR1.Read())
                    {
                        idComprobante = DR1["idComprobante"].ToString();
                        numeroDocumento = DR1["numeroDocumento"].ToString();
                    }
                }

                DB.Desconectar();

                paso = "2";
                DB.Conectar();
                DB.CrearComando("update GENERAL set estado='0', tipo='N' where idComprobante=@idComprobante ");
                DB.AsignarParametroCadena("@idComprobante", idComprobante);
                using (DbDataReader DR2 = DB.EjecutarConsulta())
                {
                }

                DB.Desconectar();

                paso = "3";
                DB.Conectar();
                DB.CrearComando(@"select top 1 idLog from LogWebService with(nolock) where tipo like '%" + numeroDocumento + "%' order by 1 desc");
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    if (DR3.Read())
                    {
                        idLog = DR3["idLog"].ToString();
                    }
                }

                DB.Desconectar();

                paso = "4";
                DB.Conectar();
                DB.CrearComando("update LogWebService set estado='P' where idLog=@idLog ");
                DB.AsignarParametroCadena("@idLog", idLog);
                using (DbDataReader DR4 = DB.EjecutarConsulta())
                {
                }

                DB.Desconectar();
                paso = "5";
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error("error metodo  regularizarXML paso  " + paso + " error--> " + ex.ToString());
            }

        }




        private void enviar_correo(string razonSocialComprador)
        {
            //BasesDatos DB = new BasesDatos();

            string nomDoc = "", asunto = "";
            try
            {
                log.mensajesLog("US001", "", "recogiendo email a enviar2  ", "", codigoControl, "leerReenvioSRI");
                emails = recogerValorEmail(idComprobante);
                log.mensajesLog("US001", "", "email a enviar2  " + emails, "", codigoControl, "leerReenvioSRI");
                nomDoc = CodigoDocumento(codDoc);
                emails = emails.Trim(',');
                EM = new EnviarMail();

                EM.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);

                if (emails.Length > 10)
                {
                    log.mensajesLog("US001", "", "adjuntando xml2  " + emails + " ruta doc " + RutaDOC, "", codigoControl, "leerReenvioSRI");
                    //EM.adjuntar(RutaDOC + codigoControl + ".xml");
                    EM.adjuntar_xml(consulta_archivo_xml(codigoControl, 7), codigoControl + ".xml");
                    log.mensajesLog("US001", "", "adjuntando pdf2  " + emails, "", codigoControl, "leerReenvioSRI");
                    EM.adjuntar_xml(cPDF.msPDF(codigoControl), codigoControl + ".pdf");
                    log.mensajesLog("US001", "", "docuemntos ok  " + emails, "", codigoControl, "leerReenvioSRI");

                    asunto = nomDoc + " ELECTRONICA No: " + estab + "-" + ptoEmi + "-" + secuencial + " de " + compania;
                    clsLogger.Graba_Log_Info($"asunto: {asunto}");

                    //-----------------------HTML VIEW FOR EMAIL--------------------------------
                    //Build up Linked resources

                    string rutaImg = RutaDOC.Replace("docus\\", "imagenes\\logo_cima.png");
                    System.Net.Mail.LinkedResource image001 = new System.Net.Mail.LinkedResource(rutaImg, "image/png");
                    image001.ContentId = "image001";
                    image001.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;

                    // Structure HTML email text (IN BACKGROUND-IMAGE WE HAVE SPECIFIED THE LINKED RESOURCE ID)
                    System.Text.StringBuilder htmlBody = new System.Text.StringBuilder();
                    htmlBody.Append("<html>");
                    htmlBody.Append("<body>");
                    htmlBody.Append("<table style=\"width:100%;\">");
                    htmlBody.Append("<tr>");
                    //htmlBody.Append("<td colspan=\"3\"><img align=\"middle\" src=\"cid:image001\" /></td>");
                    htmlBody.Append("<td colspan=\"3\"></td>");
                    htmlBody.Append("</tr>");

                    htmlBody.Append("<tr>");
                    htmlBody.Append("<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td><td><br/>Estimado(a) " + razonSocialComprador +
                        "<br/><br/>Adjunto s&iacute;rvase encontrar su " + nomDoc + " ELECTRONICA No: " + estab + "-" + ptoEmi + "-" + secuencial + @"&sup1; y el archivo PDF&sup2; de dicho 
							comprobante que hemos emitido en nuestra empresa.<br/> Gracias por preferirnos.<br/><br/> Atentamente, " + " <br/> <img height=\"50\" width=\"50\" align=\"middle\" src=\"cid:image001\" /><br/>" + compania +
@"<br/>--------------------------------------------------------------------------------" +
@"<br/>&sup1; El comprobante electr&oacute;nico es el archivo XML adjunto, le socilitamos que lo almacene de manera segura puesto que tiene validez tributaria." +
@"<br/>&sup2; La representaci&oacute;n impresa del comprobante electr&oacute;nico es el archivo PDF adjunto, y no es necesario que la imprima." +
@"</td><td> </td>");
                    htmlBody.Append("</tr>");

                    //htmlBody.Append("<tr>");
                    //htmlBody.Append("<td colspan=\"3\"><img align=\"middle\" src=\"cid:image001\" /></td>");
                    //htmlBody.Append("</tr>");

                    htmlBody.Append("</table>");

                    //                        htmlBody.Append("<br/><br/><br/><h1>Facturaci&#243n Electr&#243nica</h1><br /><p>Estimado(a) cliente; </p><br /><p>Acaba de recibir su documento electrónico generado el " + l_Label1.Text + @"<br>
                    //							con " + leeCodDoc(l_Label10.Text) + " No: " + l_Label5.Text + "." + "</p><br /><br /><p>Saludos Cordiales,</p> <br/> <p>" + compania + ", </p><br/><br/><p>Servicio proporcionado por CIMA IT</p> <br/><br/><p>Tel. 593 04 2280217</p>");
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


                    clsLogger.Graba_Log_Info($"llenarEmailHTML: emailEnviar: {emailEnviar}, emails: {emails.Trim(',')}, mailCCo: {mailCCo}, asunto: {asunto}, compania: {compania}");

                    EM.llenarEmailHTML(emailEnviar, emails.Trim(','), mailCCo, "", asunto, htmlView, compania);

                    try
                    {
                        clsLogger.Graba_Log_Info("Enviando mensaje");

                        EM.enviarEmail();
                        log.mensajesLog("US001", "", "envio ok2  " + emails, "", codigoControl, "leerReenvioSRI");

                        clsLogger.Graba_Log_Info("Mensaje enviado");

                    }
                    catch (Exception ex)
                    {
                        clsLogger.Graba_Log_Error(ex.Message);

                        msjT = ex.Message;
                        //DB.Desconectar();
                        log.mensajesLog("EM001", emails + "2 ", msjT, "", codigoControl, "");
                    }
                }
                //else
                //{
                //    if (codDoc.Equals("06"))
                //    {
                //        cPDF.msPDF(codigoControl);
                //    }
                //}
            }
            catch (Exception mex)
            {
                msjT = mex.Message;
                //DB.Desconectar();
                clsLogger.Graba_Log_Error(mex.Message);
                log.mensajesLog("EM001", emails + "ERROR ENVIAR MAIL2", msjT, "", codigoControl, "LeerReenvioSRI");

            }
        }



        private String recogerValorEmail(string idComprobante2)
        {
            BasesDatos DB = new BasesDatos();

            try
            {
                String emails = "", destinatarioLF = "";
                DB.Conectar();
                DB.CrearComando(@" SELECT valor  FROM InfoAdicional with(nolock)  where nombre ='E-MAIL' and id_Comprobante =@id_Comprobante ");
                DB.AsignarParametroCadena("@id_Comprobante", idComprobante2);
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    if (DR3.Read())
                    {
                        emails = DR3[0].ToString().Trim(',');
                    }
                }

                DB.Desconectar();

                DB.Conectar();
                DB.CrearComando(@" SELECT valor  FROM InfoAdicional  with(nolock) where nombre ='destinatario' and id_Comprobante =@id_Comprobante ");
                DB.AsignarParametroCadena("@id_Comprobante", idComprobante2);
                using (DbDataReader DR4 = DB.EjecutarConsulta())
                {
                    if (DR4.Read())
                    {
                        destinatarioLF = DR4[0].ToString().Trim(',');
                    }
                }

                DB.Desconectar();

                DB.Conectar();
                DB.CrearComando("SELECT top 1 emailsRegla FROM EmailsReglas with(nolock)  WHERE SUBSTRING(nombreRegla,1,6)=SUBSTRING(@rfcrec,1,6) AND estadoRegla=1 and eliminado=0");
                DB.AsignarParametroCadena("@rfcrec", destinatarioLF);
                using (DbDataReader DR5 = DB.EjecutarConsulta())
                {
                    if (DR5.Read())
                    {
                        emails = emails.Trim(',') + "," + DR5[0].ToString().Trim(',') + "";
                    }
                }

                DB.Desconectar();


                return emails;
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("US001", "Error al cargar el detalle adicional de los correos email2  " + emails, "Mensaje Usuario", ex.Message, codigoControl, "");
                return "";
            }
        }


        public MemoryStream consulta_archivo_xml(string p_codigoControl, int p_opcion)
        {
            MemoryStream rpt = new MemoryStream();
            string doc = "";
            BasesDatos DB = new BasesDatos();

            try
            {
                DB.Conectar();
                DB.CrearComandoProcedimiento("PA_ARCHIVO_XML");
                DB.AsignarParametroProcedimiento("@documentoXML", System.Data.DbType.Xml, "0");
                DB.AsignarParametroProcedimiento("@codigoControl", System.Data.DbType.String, p_codigoControl);
                DB.AsignarParametroProcedimiento("@idComprobante", System.Data.DbType.Int32, "0");
                DB.AsignarParametroProcedimiento("@opcion", System.Data.DbType.Int32, p_opcion);
                using (DbDataReader dr = DB.EjecutarConsulta())
                {
                    if (dr.Read())
                    {
                        doc = @"<?xml version=""1.0"" encoding=""UTF-8""?>" + dr[0].ToString();
                        rpt = GenerateStreamFromString(doc);
                    }
                }

                DB.Desconectar();
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("US001" + "Error en proceso consulta_archivo_xml. " + ex.Message, ex.Message, ex.Message, "", codigoControl, "");
            }

            return rpt;
        }


        private MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        public Boolean enviarComprobante(Byte[] xml1)
        {
            bool retorno = false;
            string soapResponse = "";
            BasesDatos DB = new BasesDatos();

            edo = "";
            XmlDocument xmlEnvio = new XmlDocument();
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate(object sender1,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

            if (ambiente.Equals("1"))
            {
                recepcion.Url = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline";
            }
            else
            {
                recepcion.Url = "https://cel.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline";
            }

            try
            {

                using (var recepcionTrace = recepcion)
                {
                    recepcionTrace.Timeout = 20000;
                    var result = recepcionTrace.validarComprobante(xml1);
                    var soapRequest = TraceExtension.XmlRequest.OuterXml;
                    soapResponse = TraceExtension.XmlResponse.OuterXml;
                }
                clsLogger.Graba_Log_Info("enviarComprobante --> " + codigoControl + " --- " + soapResponse.ToString());
                xmlEnvio.LoadXml(soapResponse);
                XmlElement EnvelopeNodo = (XmlElement)xmlEnvio.GetElementsByTagName("soap:Envelope")[0];
                XmlElement BodyNodo = (XmlElement)EnvelopeNodo.GetElementsByTagName("soap:Body")[0];
                XmlElement validarComprobanteNodo = (XmlElement)BodyNodo.GetElementsByTagName("ns2:validarComprobanteResponse")[0];
                XmlElement respuestaNodo = (XmlElement)validarComprobanteNodo.GetElementsByTagName("RespuestaRecepcionComprobante")[0];
                //edo = obtener_tag_Element(respuestaNodo, "estado");
                edo = respuestaNodo.GetElementsByTagName("estado")[0].InnerText;
                if (edo.Equals("RECIBIDA"))
                {
                    retorno = true;
                }
                else
                {
                    XmlElement comprobantes = (XmlElement)respuestaNodo.GetElementsByTagName("comprobantes")[0];
                    XmlElement comprobante = (XmlElement)comprobantes.GetElementsByTagName("comprobante")[0];
                    XmlElement mensajes = (XmlElement)comprobante.GetElementsByTagName("mensajes")[0];
                    XmlElement mensaje = (XmlElement)comprobante.GetElementsByTagName("mensaje")[0];

                    string mensaje_us = "Error en generar documento: " + obtener_tag_Element(mensaje, "mensaje") + Environment.NewLine + ": " + obtener_tag_Element(mensaje, "informacionAdicional");
                    log.mensajesLog("US001", mensaje_us, soapResponse, "", codigoControl, " Recepción de Comprobantes, WebService envio ");

                    enviar_notificacion_correo_punto(estab, codDoc + estab + ptoEmi + secuencial, fechaEmision, mensaje_us);

                    if (obtener_tag_Element(mensaje, "identificador").Equals("43") || obtener_tag_Element(mensaje, "identificador").Equals("45") || obtener_tag_Element(mensaje, "identificador").Equals("70")) //CLAVE ACCESO REGISTRADA
                        retorno = true;

                }

                if (edo.Equals("DEVUELTA"))
                {
                    DB.Conectar();
                    DB.CrearComando(@"UPDATE GENERAL SET estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                    DB.AsignarParametroCadena("@estado", "0");
                    DB.AsignarParametroCadena("@tipo", "N");
                    DB.AsignarParametroCadena("@codigoControl", codigoControl);
                    DB.EjecutarConsulta1();
                    DB.Desconectar();
                }

            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                msj = "";
                msjT = ex.Message;
                return false;
            }

            return retorno;

        }



        public Boolean validarAutorizacion(string clave)
        {
            XmlDocument xmlAutorizacion = new XmlDocument();

            edo = "";
            String aux = "";
            string soapResponse = "";
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate(object sender1,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

            int intentos_autorizacion = 0, i_contador = 0;
            intentos_autorizacion = int.Parse(ejecuta_query1(@"select top 1 case when intentosautorizacion > 0 then intentosautorizacion else 1 end  from dbo.ParametrosSistema  with(nolock)"));
            Boolean b_respuesta = false, b_no_autorizado = false;

            if (ambiente.Equals("1"))
            {
                autorizacion.Url = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";
            }
            else
            {
                autorizacion.Url = "https://cel.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";
            }
            BasesDatos DB = new BasesDatos();

            try
            {
                while (i_contador < intentos_autorizacion && b_respuesta == false && b_no_autorizado == false)
                {
                    i_contador++;

                    using (var autorizacionTrace = autorizacion)
                    {
                        autorizacionTrace.Timeout = 10000;
                        var result = autorizacionTrace.autorizacionComprobante(clave);
                        var soapRequest = TraceExtension.XmlRequest.OuterXml;
                        soapResponse = TraceExtension.XmlResponse.OuterXml;

                    }
                    if (soapResponse.Length > 10)
                    {
                        string SnumeroComprobantes;
                        int InumeroComprobantes = 0;
                        clsLogger.Graba_Log_Info("validar autorizacion --> " + codigoControl + " --- " + soapResponse.ToString());

                        xmlAutorizacion.LoadXml(soapResponse);
                        XmlElement EnvelopeNodo = (XmlElement)xmlAutorizacion.GetElementsByTagName("soap:Envelope")[0];
                        XmlElement BodyNodo = (XmlElement)EnvelopeNodo.GetElementsByTagName("soap:Body")[0];
                        XmlElement autorizacionComprobanteNodo = (XmlElement)BodyNodo.GetElementsByTagName("ns2:autorizacionComprobanteResponse")[0];
                        XmlElement respuestaNodo = (XmlElement)autorizacionComprobanteNodo.GetElementsByTagName("RespuestaAutorizacionComprobante")[0];

                        SnumeroComprobantes = lee_nodo_xml(respuestaNodo, "numeroComprobantes");
                        if (!string.IsNullOrEmpty(SnumeroComprobantes))
                        {
                            InumeroComprobantes = int.Parse(SnumeroComprobantes);
                        }

                        if (InumeroComprobantes > 0)
                        {
                            XmlElement autorizacionesNodo = (XmlElement)respuestaNodo.GetElementsByTagName("autorizaciones")[0];
                            //XmlElement autorizacionesNodo = (XmlElement)respuestaNodo.GetElementsByTagName("autorizaciones")[0];
                            foreach (XmlElement autorizacionNodo in autorizacionesNodo)
                            {
                                msjSRI = autorizacionNodo.GetElementsByTagName("mensajes")[0].OuterXml;
                                edo = obtener_tag_Element(autorizacionNodo, "estado");
                                if (edo.Equals("AUTORIZADO"))
                                {
                                    numeroAutorizacion = obtener_tag_Element(autorizacionNodo, "numeroAutorizacion");
                                    fechaAutorizacion = obtener_tag_Element(autorizacionNodo, "fechaAutorizacion");
                                    DateTime dt_aut = DateTime.Parse(fechaAutorizacion);
                                    aux = dt_aut.ToString("yyyy-MM-ddTHH:mm:ss");

                                    DB.Desconectar();
                                    DB.Conectar();
                                    DB.CrearComando(@"UPDATE GENERAL SET numeroAutorizacion= @numeroAutorizacion,fechaAutorizacion=@fechaAutorizacion , estado ='1' , tipo='E'  WHERE codigoControl = @codigoControl");
                                    DB.AsignarParametroCadena("@fechaAutorizacion", aux);
                                    DB.AsignarParametroCadena("@numeroAutorizacion", numeroAutorizacion);
                                    DB.AsignarParametroCadena("@codigoControl", codigoControl);
                                    DB.EjecutarConsulta1();
                                    DB.Desconectar();


                                    XmlDocument docA = new XmlDocument();
                                    docA.LoadXml(autorizacionNodo.OuterXml);

                                    procesa_archivo_xml(docA, codigoControl, idComprobante, 3);

                                    b_respuesta = true;

                                    try
                                    {
                                        DataTable tb_infoA = obtener_infoAdicional(idComprobante);
                                        if (tb_infoA.Rows.Count > 0)
                                        {
                                            log.mensajesLog("US001", claveAcceso, "datos a enviar sociedad", "", codigoControl, "informacion  numero documento " + estab + "-" + ptoEmi + "-" + secuencial + "  sociedad " + tb_infoA.Rows[0]["sociedad"].ToString() + " numeroAsientoContable " + tb_infoA.Rows[0]["numeroAsientoContable"].ToString() + " anioAsientoContable " + tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                            RespuestaWebUNASAP(codDoc, estab + "-" + ptoEmi + "-" + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "AT", "AT", tb_infoA.Rows[0]["sociedad"].ToString(), tb_infoA.Rows[0]["numeroAsientoContable"].ToString(), tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                        }

                                        if (codDoc.Equals("06"))
                                        {

                                            if ((estab.Equals(obtener_codigo("estabprod")) && ptoEmi.Equals(obtener_codigo("ptoemiprod"))) || (estab.Equals(obtener_codigo("estabprue")) && ptoEmi.Equals(obtener_codigo("ptoemiprue"))))
                                            {
                                                log.mensajesLog("US001", "A", "", "", "entrando a webservices mina -->", "clase de error ConsultaOffLine.cs");
                                                RespuestaWebapp(estab + "-" + ptoEmi + "-" + secuencial, claveAcceso, fechaAutorizacion);
                                                log.mensajesLog("US001", "A", "", "", "saliendo a webservices mina -->", "clase de error ConsultaOffLine.cs");
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        DB.Desconectar();
                                        clsLogger.Graba_Log_Error(ex.Message);
                                        log.mensajesLog("EM050", claveAcceso, ex.Message, "", codigoControl, " Validación de Comprobantes, WebService validación1: No se encontro informacion adicional");
                                    }
                                }
                            }

                            if (!b_respuesta)
                            {
                                XmlNodeList existe = autorizacionesNodo.GetElementsByTagName("autorizacion");
                                if (existe.Count != 0)
                                {
                                    XmlElement autorizacionNodo = (XmlElement)autorizacionesNodo.GetElementsByTagName("autorizacion")[0];
                                    msjSRI = autorizacionNodo.GetElementsByTagName("mensajes")[0].OuterXml;
                                    edo = obtener_tag_Element(autorizacionNodo, "estado");
                                    //if(!b_respuesta)
                                    {
                                        if (edo.Equals("NO AUTORIZADO"))
                                        {
                                            b_no_autorizado = true;
                                            string identificador = "";
                                            string mensaje_sri = "";
                                            string s_estado = "0";
                                            string s_tipo = "N";
                                            string s_creado = "0";
                                            msj = " Validación de Comprobantes, WebService validación1.";

                                            XmlElement mensajes = (XmlElement)autorizacionNodo.GetElementsByTagName("mensajes")[0];
                                            foreach (XmlElement tmensaje in mensajes)
                                            {
                                                if (!tmensaje.GetElementsByTagName("identificador")[0].InnerText.Equals("68") & !tmensaje.GetElementsByTagName("identificador")[0].InnerText.Equals("60"))
                                                {
                                                    identificador = obtener_tag_Element(tmensaje, "identificador");
                                                    mensaje_sri = obtener_tag_Element(tmensaje, "mensaje"); // +Environment.NewLine + ": " + mensaje.GetElementsByTagName("informacionAdicional")[0].InnerText;

                                                    if (tmensaje.GetElementsByTagName("informacionAdicional").Count > 0)
                                                    {
                                                        mensaje_sri = mensaje_sri + Environment.NewLine + ": " + obtener_tag_Element(tmensaje, "informacionAdicional");
                                                    }

                                                    if (identificador.Equals("40")) //Cuando hay problemas de conexion entre el SRI y el BCE hay que reenviarlo
                                                    {
                                                        //emite_doc_prov();
                                                        s_estado = "2";
                                                        s_tipo = "E";
                                                        s_creado = "1";
                                                        //log.mensajesLog("EM016", "No autorizado" + claveAcceso, soapResponse, "", codigoControl, " Validación de Comprobantes, WebService validación1. Error de comunicación SRI-BCE se reenvía el documento ");
                                                        msj = msj + "Error de comunicación SRI-BCE se reenvía el documento. ";

                                                    }
                                                    if (identificador.Equals("35"))
                                                    {
                                                        s_estado = "2";
                                                        s_tipo = "E";
                                                        s_creado = "1";
                                                        msj = msj + "Error del SRI. ";

                                                    }
                                                    DB.Conectar();
                                                    DB.CrearComando(@"UPDATE GENERAL SET creado= @creado, estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                                                    DB.AsignarParametroCadena("@creado", s_creado); // el estado y tipo son iguales como si fuera contingencia...lo que diferencia es creado=0
                                                    DB.AsignarParametroCadena("@estado", s_estado);
                                                    DB.AsignarParametroCadena("@tipo", s_tipo);
                                                    DB.AsignarParametroCadena("@codigoControl", codigoControl);
                                                    DB.EjecutarConsulta1();
                                                    DB.Desconectar();

                                                    enviar_notificacion_correo_punto(estab, codDoc + estab + ptoEmi + secuencial, fechaEmision, mensaje_sri);

                                                    try
                                                    {

                                                        DataTable tb_infoA = obtener_infoAdicional(idComprobante);
                                                        if (tb_infoA.Rows.Count > 0)
                                                        {
                                                            RespuestaWebUNASAP(codDoc, estab + "-" + ptoEmi + "-" + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "RJ", mensaje_sri, tb_infoA.Rows[0]["sociedad"].ToString(), tb_infoA.Rows[0]["numeroAsientoContable"].ToString(), tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        DB.Desconectar();
                                                        clsLogger.Graba_Log_Error(ex.Message);
                                                        log.mensajesLog("EM050", claveAcceso, ex.Message, "", codigoControl, " Validación de Comprobantes, WebService validación1: No se encontro informacion adicional");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (i_contador == intentos_autorizacion)
                                            {                                                
                                                if (!actualiza_estado_factura("1", "1", "E", codigoControl))
                                                {
                                                    msj = msj + " No se pudo actualizar estado de documento. ";
                                                }
                                            }
                                        }

                                        mensajes_error_usuario((XmlElement)autorizacionNodo.GetElementsByTagName("mensajes")[0]);
                                        log.mensajesLog("EM017", ((XmlElement)autorizacionNodo.GetElementsByTagName("mensajes")[0]).InnerXml.ToString(), soapResponse, claveAcceso, codigoControl, msj);
                                        //return false;

                                        b_respuesta = false;

                                    }
                                }
                                else
                                {
                                    if (i_contador == intentos_autorizacion)
                                    {
                                        // emite_doc_prov();
                                        if (actualiza_estado_factura("4", "0", "P", codigoControl))
                                        {
                                            log.mensajesLog("EM016", claveAcceso, soapResponse, "", codigoControl, " Validación de Comprobantes, WebService validación2 ");
                                        }
                                        else
                                        {
                                            log.mensajesLog("EM016", claveAcceso, soapResponse, "", codigoControl, " Validación de Comprobantes, WebService validación2, no se pudo actualizar estado de documento. ");
                                        }



                                    }
                                    b_respuesta = false;
                                }
                            }

                        }
                        else
                        {
                            if (i_contador == intentos_autorizacion)
                            {
                                if (actualiza_estado_factura("2", "0", "E", codigoControl))
                                {
                                    log.mensajesLog("EM016", "", soapResponse, "", "", " Validación de Comprobantes, WebService validación2 en Reproceso. LeerReenvio.cs");
                                }
                                else
                                {
                                    log.mensajesLog("EM016", "", soapResponse, "", "", " Validación de Comprobantes, WebService validación2, no se pudo actualizar estado de documento en reproceso. LeerReenvio.cs");
                                }

                            }
                            b_respuesta = false;
                        }



                    }
                    else
                    {
                        if (i_contador == intentos_autorizacion)
                        {
                            log.mensajesLog("EM016", claveAcceso, soapResponse, "", codigoControl, " Validación de Comprobantes, WebService validación3 ");
                            //return false;
                        }
                        b_respuesta = false;
                    }

                    if (b_respuesta == false && b_no_autorizado == false)
                    {
                        // log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " entra al BUCLE DE ESPERA 999999");
                        int i = 0, j = 0;
                        while (i <= 10000)
                        {
                            i++;
                            j = 0;
                            while (j <= 10000)
                            {
                                j++;
                            }
                            i++;
                        }
                        //log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " sale BUCLE DE ESPERA 999999");
                    }

                }
                DB.Desconectar();

                return b_respuesta;
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                msjT = ex.Message;
                //log.mensajesLog("EM016", claveAcceso, msjT, "", estab + ptoEmi + secuencial, " Validación de Comprobantes, WebService validación4 ");

                if (actualiza_estado_factura("4", "0", "P", codigoControl))
                {
                    log.mensajesLog("EM016", claveAcceso, msjT, "", codigoControl, " Validación de Comprobantes, WebService validación offline ");
                }
                else
                {
                    log.mensajesLog("EM016", claveAcceso, msjT, "", codigoControl, " Validación de Comprobantes, WebService validación4: No se actualizó estado de factura offline");
                }
                return false;
            }
        }

        public void RespuestaWebUNASAP(string p_codDoc, string p_numDoc, string p_Accesskey, string p_Autorizacion, string p_Authdate, string p_Authtime, string p_Contingency, string p_Contdate, string p_Conttime, string p_status, string p_Message, string Bukrs, string Belnr, string Gjahr)
        {

            try
            {
                System.Threading.Thread.Sleep(2000);


                //WebUNASAP.Zecsrifm01 wsap = new WebUNASAP.Zecsrifm01();
                WebSapUnacem.SvcUNACEMECSRI svc = new WebSapUnacem.SvcUNACEMECSRI();
                
                WebSapUnacem.IDoc IDoc = new WebSapUnacem.IDoc();
                //wsap.IDoc = new WebUNASAP.Zecsrist005();
                if (p_codDoc.Equals("05"))
                {
                    p_codDoc = "05A";
                }
                string msj1 = "";

                string claveAcceso_ = p_Accesskey;

                IDoc.Bukrs = Bukrs.ToString();
                IDoc.Belnr = Belnr.ToString();
                IDoc.Gjahr = Convert.ToInt32(Gjahr.ToString());
                IDoc.GjahrSpecified = true;
                DateTime d_fAut = DateTime.Today;
                if (!String.IsNullOrEmpty(p_Authdate)) d_fAut = Convert.ToDateTime(p_Authdate);


                IDoc.Blart = obtener_codigo(p_codDoc);
                IDoc.NroSri = p_numDoc;
                IDoc.Ackey = p_Accesskey;
                IDoc.Authn = p_Autorizacion;
                IDoc.Xblnr = "-";
                IDoc.Stcd1 = "-";

                if (!string.IsNullOrEmpty(p_Autorizacion))
                {
                    IDoc.Fauth = this.formato_fecha(p_Authdate, "yyyy-MM-dd");
                    IDoc.Dauth = this.formato_fecha(p_Authdate, "HH:mm:ss");
                }
                else
                {
                    IDoc.Fauth = "";
                    IDoc.Dauth = "";
                }

                IDoc.Status = p_status;
                IDoc.Msgid = "-";

                svc.Url = obtener_codigo("webserviceUNACEMSAP");
                svc.UseDefaultCredentials = true;
                ICredentials credential = new NetworkCredential(obtener_codigo("webserviceUSER"), obtener_codigo("webservicePASS"));
                svc.Credentials = credential;


                XmlDocument xmlEnvio = new XmlDocument();
                string soapResponse = "";

                clsLogger.Graba_Log_Info("Bukrs: " + IDoc.Bukrs + 
                    " Belnr: " + IDoc.Belnr + 
                    " Gjahr: " + IDoc.Gjahr.ToString() +
                    " Blart: " + IDoc.Blart +
                    " NroSri: " + IDoc.NroSri +
                    " IDoc: " + IDoc.Ackey +
                    " Authn: " + IDoc.Authn +
                    " Xblnr: " + IDoc.Xblnr +
                    " Stcd1: " + IDoc.Stcd1 +
                    " Fauth: " + IDoc.Fauth +
                    " Dauth: " + IDoc.Dauth +
                    " Status: " + IDoc.Status +
                    " Msgid: " + IDoc.Msgid +
                    " url: " + obtener_codigo("webserviceUNACEMSAP") + 
                    obtener_codigo("webserviceUSER") + obtener_codigo("webservicePASS"));

                

                try 
                {
                    using (var recepcionTrace = svc)
                    {
                        recepcionTrace.Timeout = 20000;
                        var result = recepcionTrace.Zecsrifm01(IDoc);
                        var soapRequest = TraceExtension.XmlRequest.OuterXml;
                        soapResponse = TraceExtension.XmlResponse.OuterXml;
                    }
                    clsLogger.Graba_Log_Info(soapResponse.ToString());
                    xmlEnvio.LoadXml(soapResponse);
                    XmlElement EnvelopeNodo = (XmlElement)xmlEnvio.GetElementsByTagName("s:Envelope")[0];
                    XmlElement BodyNodo = (XmlElement)EnvelopeNodo.GetElementsByTagName("s:Body")[0];
                    XmlElement respuestaNodo = (XmlElement)BodyNodo.GetElementsByTagName("Zecsrifm01Response")[0];
                    XmlElement respuestaNodo2 = (XmlElement)respuestaNodo.GetElementsByTagName("Zecsrifm01Result")[0];
                    XmlElement respuestaMensaje = (XmlElement)respuestaNodo2.GetElementsByTagName("a:EMessage")[0];
                    
                    if (respuestaMensaje != null)
                    {
                        string o_status = "", o_message = "";
                        //o_status = lee_nodo_xml(respuestaMensaje, "Type");
                        XmlElement status = (XmlElement)respuestaMensaje.GetElementsByTagName("b:Type")[0];
                        o_status = status.InnerText;
                        clsLogger.Graba_Log_Info("o_status "  + status.InnerText);
                        XmlElement Message = (XmlElement)respuestaMensaje.GetElementsByTagName("b:Message")[0];
                        o_message = Message.InnerText;
                        //o_message = lee_nodo_xml(respuestaMensaje, "b:Message");
                        clsLogger.Graba_Log_Info("o_message " + o_message);
                        clsLogger.Graba_Log_Info("Respuesta Exitosa: " + "Estatus: " + o_status + " Mensaje: " + o_message);
                        //log.mensajesLog("US001", "Respuesta Exitosa: " + "Estatus: " + o_status + "Mensaje: " + o_message, o_message, p_status, codigoControl, p_status);
                        actualizaEstadosWLF(o_status, p_status, claveAcceso_);
                    }
                }
                catch (Exception exlf)
                {
                    clsLogger.Graba_Log_Error(exlf.Message);

                    this.log.mensajesLog("US001", "Error web service n0:Zecsrifm01Response: No se controlo parametro envio primer cath " + p_numDoc, exlf.Message, "", p_numDoc, "ConsultaOff");
                    actualizaEstadosWLF("N", p_status, claveAcceso_);
                }   

            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error("error metodo RespuestaWebUNASAP: " + ex.ToString()); 
                log.mensajesLog("US001", "Error web service n0:Zecsrifm01Response: No se controlo parametro envio " + p_numDoc, ex.Message, "", codigoControl, "");
            }

        }

        public void actualizaEstadosWLF(string A_estadoWLF, string A_archivoWLF, string A_claveAcceso)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando(@"UPDATE GENERAL SET  estadoWLF=@estadoWLF,archivoWLF=@archivoWLF WHERE claveAcceso = @claveAcceso");
                DB.AsignarParametroCadena("@estadoWLF", A_estadoWLF);
                DB.AsignarParametroCadena("@archivoWLF", A_archivoWLF);
                DB.AsignarParametroCadena("@claveAcceso", A_claveAcceso);
                DB.EjecutarConsulta1();
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

        public void RespuestaWebUNASAPTEST(string p_codDoc, string p_numDoc, string p_Accesskey, string p_Autorizacion, string p_Authdate, string p_Authtime, string p_Contingency, string p_Contdate, string p_Conttime, string p_status, string p_Message, string Bukrs, string Belnr, string Gjahr)
        {

            try
            {
                System.Threading.Thread.Sleep(2000);

                TestUnacemSap.Zecsrifm01 wsapTEST = new TestUnacemSap.Zecsrifm01();

                wsapTEST.IDoc = new TestUnacemSap.Zecsrist005();

                if (codDoc.Equals("05"))
                {
                    p_codDoc = "05A";
                }

                string claveAcceso_ = p_Accesskey;
                wsapTEST.IDoc.Bukrs = Bukrs.ToString();
                wsapTEST.IDoc.Belnr = Belnr.ToString();
                wsapTEST.IDoc.Gjahr = Gjahr.ToString();

                DateTime d_fAut = DateTime.Today;
                if (!String.IsNullOrEmpty(p_Authdate)) d_fAut = Convert.ToDateTime(p_Authdate);


                wsapTEST.IDoc.Blart = obtener_codigo(p_codDoc);
                wsapTEST.IDoc.NroSri = p_numDoc;
                wsapTEST.IDoc.Ackey = p_Accesskey;
                wsapTEST.IDoc.Authn = p_Autorizacion;
                wsapTEST.IDoc.Xblnr = "-";
                wsapTEST.IDoc.Stcd1 = "-";

                if (!string.IsNullOrEmpty(p_Authdate))
                {
                    wsapTEST.IDoc.Fauth = this.formato_fecha(p_Authdate, "yyyy-MM-dd");
                    wsapTEST.IDoc.Dauth = Convert.ToDateTime(this.formato_fecha(p_Authdate, "HH:mm:ss"));
                }
                else
                {
                    wsapTEST.IDoc.Fauth = "";
                    wsapTEST.IDoc.Dauth = Convert.ToDateTime((string)null);//.ToShortTimeString();
                }

                wsapTEST.IDoc.Status = p_status;
                wsapTEST.IDoc.Msgid = "-";//Truncate(p_Message, 299);

                webunasapsTEST.Url = obtener_codigo("webserviceUNACEMSAPTEST");
                webunasapsTEST.UseDefaultCredentials = true;
                ICredentials credential = new NetworkCredential(obtener_codigo("webserviceUSERTEST"), obtener_codigo("webservicePASSTEST"));
                webunasapsTEST.Credentials = credential;

                XmlDocument xmlEnvio = new XmlDocument();

                string soapResponse = "";
                try 
                {

                    using (var recepcionTrace = webunasapsTEST)
                    {
                        recepcionTrace.Timeout = 20000;
                        var result = recepcionTrace.Zecsrifm01(wsapTEST);
                        var soapRequest = TraceExtension.XmlRequest.OuterXml;
                        soapResponse = TraceExtension.XmlResponse.OuterXml;
                    }
                    xmlEnvio.LoadXml(soapResponse);
                    XmlElement EnvelopeNodo = (XmlElement)xmlEnvio.GetElementsByTagName("soap-env:Envelope")[0];
                    XmlElement BodyNodo = (XmlElement)EnvelopeNodo.GetElementsByTagName("soap-env:Body")[0];
                    XmlElement respuestaNodo = (XmlElement)BodyNodo.GetElementsByTagName("n0:Zecsrifm01Response")[0];
                    XmlElement respuestaMensaje = (XmlElement)BodyNodo.GetElementsByTagName("EMessage")[0];

                    if (respuestaMensaje != null)
                    {
                        string o_status = "", o_message = "";
                        o_status = lee_nodo_xml(respuestaMensaje, "Type");
                        o_message = lee_nodo_xml(respuestaMensaje, "Message");
                        log.mensajesLog("US001", "Respuesta Exitosa: " + "Estatus: " + p_status + "Mensaje: " + o_message, o_message, "", codigoControl, p_status);
                        actualizaEstadosWLF(o_status, p_status, claveAcceso_);
                    }
                }
                catch (Exception exlf)
                {   
                    clsLogger.Graba_Log_Error(exlf.Message);
                    this.log.mensajesLog("US001", "Error web service n0:Zecsrifm01Response: No se controlo parametro envio primer cath " + p_numDoc, exlf.Message, "", p_numDoc, "ConsultaOff");
                    actualizaEstadosWLF("N", p_status, claveAcceso_);
                }

            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error(ex.Message);

                log.mensajesLog("US001", "Error web service n0:Zecsrifm01Response: No se controlo parametro envio " + p_numDoc, ex.Message, "", codigoControl, "");
                //logControl.mensajesLog("US001", "Error consulta Web Service GRABARESULTADOFACTURACION()", ex.Message + " soapResponse: " + soapResponse, "", "");
            }

        }

        private string lee_nodo_xml(XmlNode p_root, string p_tag)
        {
            string retorno = "";
            if (p_root.SelectSingleNode("descendant::" + p_tag) != null)
            {
                retorno = p_root.SelectSingleNode("descendant::" + p_tag).InnerText;//.InnerText;
                // retorno = System.Net.WebUtility.HtmlEncode(retorno);
            }
            return retorno;
        }

        private string formato_fecha(string p_fecha, string formato)
        {
            string rpt = "";
            DateTime v_fecha;
            try
            {
                if (DateTime.TryParse(p_fecha, out v_fecha))
                    rpt = v_fecha.ToString(formato);
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error(ex.Message);

                log.mensajesLog("US001", "Error al convertir fecha: " + ex.Message, ex.Message, "", p_fecha, "");
            }
            return rpt;
        }
        private string obtener_codigo(string a_parametro)
        {
            string retorna = ConfigurationManager.AppSettings.Get(a_parametro);

            return retorna;
        }
        private String ejecuta_query1(String query)
        {
            String retorno = "";
            BasesDatos DB = new BasesDatos();
            try
            {

                DB = new BasesDatos();
                DB.Conectar();
                DB.CrearComando(query);
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        retorno = DR[0].ToString();
                    }

                    DB.Desconectar();
                    DR.Dispose();
                }
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
            
            return retorno;
        }

        private void enviar_notificacion_correo_punto(string pr_estab, string pr_folio, string pr_fechaEmision, string pr_mensaje)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                String correos = "", asunto = "", mensaje = "";
                string nomDoc = "";
                DB.Conectar();
                DB.CrearComando(@"select a.correo from  dbo.Sucursales a with(nolock) where a.clave = '" + pr_estab + "'");
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    while (DR3.Read())
                    {
                        correos = correos.Trim(',') + "," + DR3[0].ToString().Trim(',') + "";
                    }
                }

                DB.Desconectar();

                correos = correos.Trim(',');
                EM = new EnviarMail();
                EM.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);

                nomDoc = CodigoDocumento(pr_folio.Substring(0, 2));


                if (correos.Length > 10)
                {
                    asunto = nomDoc + " " + pr_folio + " con observaciones";
                    mensaje = @"Estimado(a);  <br>
							Hubo inconvenientes con " + nomDoc + " generada el " + pr_fechaEmision + @"<br>
							No: " + pr_folio + ".";
                    mensaje += "<br><br>Mensaje: " + pr_mensaje;
                    mensaje += "<br><br>" + compania;
                    mensaje += "<br><br>Cualquier novedad comunicarse con: ";
                    mensaje += "<br>helpdesk@cimait.com.ec";


                    EM.llenarEmail(emailEnviar, correos.Trim(','), "", "", asunto, mensaje);
                    try
                    {
                        EM.enviarEmail();
                    }
                    catch (System.Net.Mail.SmtpException ex)
                    {
                        msjT = ex.Message;
                        DB.Desconectar();
                        clsLogger.Graba_Log_Error(ex.Message);

                        log.mensajesLog("EM001", " ", msjT, "", codigoControl, "");
                        msjT = "";
                    }
                }

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

        private string CodigoDocumento(string p_codigo)
        {
            string desc = "";
            try
            {
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
            }
            catch (Exception ex)
            {
                log.mensajesLog("US001", "No se pudo interpretar código documento: " + p_codigo, "Mensaje Usuario", "", p_codigo, "");
            }

            return desc;

        }

        public void parametrosSistema()
        {
            BasesDatos DB2 = new BasesDatos();

            try
            {
                DB2.Desconectar();
                DB2.Conectar();
                DB2.CrearComando(@"select servidorSMTP,puertoSMTP,sslSMTP,userSMTP,passSMTP,
							         dirdocs,dirtxt,dirrespaldo,dircertificados,dirllaves,emailEnvio,
							         dirp12,passP12,dirXMLbase 
							         from ParametrosSistema with(nolock)");
                using (DbDataReader DR = DB2.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        servidor = DR[0].ToString().Trim();
                        puerto = Convert.ToInt32(DR[1]);
                        ssl = Convert.ToBoolean(DR[2]);
                        emailCredencial = DR[3].ToString().Trim();
                        passCredencial = DR[4].ToString().Trim();
                        RutaDOC = DR[5].ToString().Trim();
                        RutaTXT = DR[6].ToString().Trim();
                        RutaBCK = DR[7].ToString().Trim();
                        RutaCER = DR[8].ToString().Trim();
                        RutaKEY = DR[9].ToString().Trim();
                        emailEnviar = DR[10].ToString().Trim();
                        RutaP12 = DR[11].ToString().Trim();
                        PassP12 = DR[12].ToString().Trim();
                        RutaXMLbase = DR[13].ToString().Trim();

                        if (String.IsNullOrEmpty(PassP12))
                            PassP12 = FirmaBCE.clavep12();
                        else
                            PassP12 = Cs.desencriptar(PassP12, "CIMAIT");
                    }
                }

                DB2.Desconectar();
            }
            catch (Exception ed)
            {
                DB2.Desconectar();
                clsLogger.Graba_Log_Error(ed.Message);

            }

        }

        private string obtener_tag_Element(XmlElement p_element, string tag)
        {
            string rpt = "";
            try
            {
                var matches = p_element.GetElementsByTagName(tag);
                if (matches.Count > 0)
                    rpt = matches[0].InnerText;
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error(ex.Message);

                log.mensajesLog("EM001", ex.Message, ex.StackTrace, "", codigoControl, "Método obtener_tag_Element");
            }
            return rpt;
        }

        public Boolean actualiza_estado_factura(string p_estado, string p_creado, string p_tipo, string p_codigoControl)
        {
            BasesDatos DB = new BasesDatos();

            try
            {
                DB.Conectar();
                DB.CrearComando(@"UPDATE GENERAL SET creado= @creado, estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                DB.AsignarParametroCadena("@creado", p_creado);
                DB.AsignarParametroCadena("@estado", p_estado);
                DB.AsignarParametroCadena("@tipo", p_tipo);
                DB.AsignarParametroCadena("@codigoControl", p_codigoControl);
                DB.EjecutarConsulta1();
                DB.Desconectar();

                return true;
            }
            catch (Exception ex)
            {
                msjT = ex.Message;
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("BD001", claveAcceso, msjT, "", codigoControl, " Error al actualizar estado de documento.");
                msjT = "";
                return false;
            }

        }

        public void procesa_archivo_xml(XmlDocument p_documentoXML, string p_codigoControl, string p_idComprobante, int p_opcion)
        {
            XmlDocument fRec4 = new XmlDocument();
            XmlElement facturaXML;
            string sXml = "";
            BasesDatos DB = new BasesDatos();
            try
            {
                sXml = p_documentoXML.InnerXml;
                sXml = sXml.Replace(@"<?xml version=""1.0"" encoding=""UTF-8""?>", "");
                fRec4.LoadXml(sXml);
                facturaXML = fRec4.DocumentElement;

                int idc = 0;
                int.TryParse(p_idComprobante, out idc);

                DB.Desconectar();
                DB.Conectar();
                DB.CrearComandoProcedimiento("PA_ARCHIVO_XML");
                DB.AsignarParametroProcedimiento("@documentoXML", System.Data.DbType.Xml, facturaXML.OuterXml);
                DB.AsignarParametroProcedimiento("@codigoControl", System.Data.DbType.String, p_codigoControl);
                DB.AsignarParametroProcedimiento("@idComprobante", System.Data.DbType.Int32, idc);
                DB.AsignarParametroProcedimiento("@opcion", System.Data.DbType.Int32, p_opcion);
                DB.EjecutarConsulta1();
                DB.Desconectar();
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);

                log.mensajesLog("US001" + "Error en proceso procesa_archivo_xml. " + ex.Message, ex.Message, ex.Message, "", codigoControl, "");
            }
        }

        public DataTable obtener_infoAdicional(string id_comprobante)
        {
            DataTable dt = new DataTable();
            try
            {

                String StrInfoAdicional1 = @"DECLARE @cols AS NVARCHAR(MAX),
    @query  AS NVARCHAR(MAX)

select @cols = STUFF((SELECT ',' + QUOTENAME(nombre) 
                    from InfoAdicional with(nolock)
                    where id_Comprobante = " + id_comprobante + @"
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,1,'')

set @query = 'SELECT ' + @cols + '," + id_comprobante + @" idComprobante 
             from 
             (
                select ia.valor, ia.nombre
                from InfoAdicional ia with(nolock)
                where id_Comprobante = " + id_comprobante + @"               
            ) x
            pivot 
            (
                max(valor)
                for nombre in (' + @cols + ')
            ) p '

execute sp_executesql @query";


                string strConn;
                strConn = ConfigurationManager.ConnectionStrings["dataexpressConnectionString"].ConnectionString;

                SqlConnection conexion = new SqlConnection(strConn);
                SqlCommand comando = new SqlCommand(StrInfoAdicional1, conexion);
                SqlDataAdapter adap = new SqlDataAdapter(comando);
                adap.Fill(dt);

                return dt;

            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error(ex.Message);

                log.mensajesLog("EM016", claveAcceso, "error en procedimiento info adicional: ", "", codigoControl, "error " + ex.Message);
                return dt;
            }


        }

        private void mensajes_error_usuario(XmlElement mensajes)
        {
            XmlElement msg = (XmlElement)mensajes.GetElementsByTagName("mensaje")[0];
            string respuesta = "";


            respuesta = respuesta + obtener_tag_Element(msg, "tipo") + ": " + obtener_tag_Element(msg, "mensaje") + Environment.NewLine;

            if (msg.GetElementsByTagName("informacionAdicional").Count > 0)
            {
                respuesta = respuesta + Environment.NewLine + ": " + obtener_tag_Element(msg, "informacionAdicional");
            }


            log.mensajesLog("US001", respuesta, "Mensaje Usuario", "", codigoControl, "");


        }

        public void RespuestaWebapp(string p_numDoc, string p_Accesskey, string fechaAut)
        {
            string respuesta = "";
            try
            {
                System.Threading.Thread.Sleep(2000);
                DateTime dt_aut = DateTime.Parse(fechaAut);
                string fecha = dt_aut.ToString("yyyy-MM-ddTHH:mm:ss");
                log.mensajesLog("US001", "", "", "", "entrando a webservices mina 2-->", "clase de error ConsultaOffLine.cs");

                mina.WebServiceUNACEM wsapp = new mina.WebServiceUNACEM();
                wsapp.Url = obtener_codigo("webserviceapp");
                respuesta = wsapp.of_autorizaGuiaRemision(p_numDoc, p_Accesskey, Convert.ToDateTime(fecha));
                log.mensajesLog("US001", "Respuesta of_autorizaGuiaRemision: " + "Mensaje: " + respuesta, respuesta, "", p_Accesskey, "");

            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error(ex.Message);

                log.mensajesLog("US001", "Error web service n0:of_autorizaGuiaRemision: No se controlo parametro envio " + p_numDoc, ex.Message, "", codigoControl, "");
            }

        }


    }
}
