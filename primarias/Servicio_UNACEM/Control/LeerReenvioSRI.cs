using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Common;
using Datos;
using System.Collections;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Net;
//using System.Xml.Serialization;
using ValSign;
using Key_Electronica;
using CriptoSimetrica;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using clibLogger;
namespace Control 
{
    public class LeerReenvioSRI
    {
        private Leer lee_normal;
        //private BasesDatos DB;
        //private DbDataReader DR;
        private EnviarMail EM;
        private NumerosALetras numA;
        private Log log;
        private GenerarXml gXml;
        private CrearPDF cPDF;
        XmlTextReader xtrReader;
        private Random r;
        private receWeb.RecepcionComprobantesService recepcion;
        private autoWeb.AutorizacionComprobantesService autorizacion;
        private string asunto = "";
        private string mensaje = "";
        private string msj = "";
        private string msjT = "";
        private string msjSRI = "";
        private string RutaTXT = "";
        private string RutaBCK = "";
        private string RutaDOC = "";
        private string RutaERR = "";
        private string RutaCER = "";
        private string RutaKEY = "";
        string idcomprobante2 = "";
        private string RutaP12 = "";
        private string PassP12 = "";
        private string RutaXMLbase = "";
        private string nombreArchivo = "";
        private string codigoControl = "";
        private string linea = "";
        private string edo = "";
        private string xmlRegenerado = "";
        private string compania = "UNACEM ECUADOR S.A.";
        Key_Electronica.Key_Electronica FirmaBCE;
        private AES Cs;
        private Boolean esNDFinLF = false;

        private LFWSrpt.lafwebServicesinvoicecinfoAutSRIStringinfoAutSRIString wsLF = new LFWSrpt.lafwebServicesinvoicecinfoAutSRIStringinfoAutSRIString();

        public WebUNASAP.ZECSRIFM01 webunasaps = new WebUNASAP.ZECSRIFM01();

        #region Parametros
        private string servidor = "";
        private int puerto = 587;
        private Boolean ssl = false;
        private string emailCredencial = "";
        private string passCredencial = "";
        private string emailEnviar = "";
        private string emails = "";
        private string xsd = "";
        #endregion

        Boolean banErrorArchivo = false;
        FirmarXML firmaXADES;
        private string numeroAutorizacion = "";
        private string fechaAutorizacion = "";
        private string ambiente = "", idComprobante = "";
        private string codDoc = "", estab = "", ptoEmi = "", secuencial = "", claveAcceso = "";

        public LeerReenvioSRI()
        {
            BasesDatos DB = new BasesDatos();
            try
            {

                FirmaBCE = new Key_Electronica.Key_Electronica();
                firmaXADES = new FirmarXML();
                DB = new BasesDatos();
                numA = new NumerosALetras();
                log = new Log();
                gXml = new GenerarXml();
                lee_normal = new Leer();
                Cs = new AES();
                //cPDF = new CrearPDFCredito();
                cPDF = new CrearPDF();// CrearPDFCredito();
                r = new Random(DateTime.Now.Millisecond);
                recepcion = new receWeb.RecepcionComprobantesService();
                autorizacion = new autoWeb.AutorizacionComprobantesService();
                //Parametros Generales
                DB.Conectar();
                DB.CrearComando(@"select servidorSMTP,puertoSMTP,sslSMTP,userSMTP,passSMTP,
                              dirdocs,dirtxt,dirrespaldo,dircertificados,dirllaves,emailEnvio,
                              dirp12,passP12,dirXMLbase 
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
                        RutaDOC = DR[5].ToString().Trim();
                        RutaTXT = DR[6].ToString().Trim();
                        RutaBCK = DR[7].ToString().Trim();
                        RutaCER = DR[8].ToString().Trim();
                        RutaKEY = DR[9].ToString().Trim();
                        emailEnviar = DR[10].ToString().Trim();
                        RutaP12 = DR[11].ToString().Trim();
                        PassP12 = DR[12].ToString().Trim();
                        RutaXMLbase = DR[13].ToString().Trim();
                    }
                }

                DB.Desconectar();

                if (String.IsNullOrEmpty(PassP12))
                    PassP12 = FirmaBCE.clavep12();
                else
                    PassP12 = Cs.desencriptar(PassP12, "CIMAIT");
                //Fin de Parametros Generales.
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

        public void procesarArchivo(string archivo)
        {
            //DB = new BasesDatos();
            linea = "";
            asunto = "";
            mensaje = "";
            codigoControl = "";
            FileInfo fi = new FileInfo(archivo);
            numA = new NumerosALetras();
            log = new Log();
            gXml = new GenerarXml();
            cPDF = new CrearPDF();

            System.IO.StreamReader sr = new System.IO.StreamReader(archivo);

            nombreArchivo = fi.Name;
            linea = "";
        }

        public void procesar(string INcodigoControl, string INcodDoc, string INidComprobante, string INtipoEnvio, string INestado, string INtipo, string INclaveAcceso, string INambiente ,string INrazonSocialComprador)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                esNDFinLF = false;
                if (INcodDoc.Equals("05"))
                    if (ConsultaNDFin(INcodigoControl))
                        esNDFinLF = true;
                //099253194000106001001000000010201208190000
                codigoControl = INcodigoControl;
                ambiente = INambiente;
                idComprobante = INidComprobante;
                if (!string.IsNullOrEmpty(INcodigoControl))
                {
                    estab = INcodigoControl.Substring(15, 3);
                    ptoEmi = INcodigoControl.Substring(18, 3);
                    secuencial = INcodigoControl.Substring(21, 9);
                }
                claveAcceso = INclaveAcceso;
                codDoc = INcodDoc;
                msjT = ""; msjSRI = "";
                if (!INtipoEnvio.Equals("FacturaCreditoA"))
                {
                    numeroAutorizacion = "";
                    fechaAutorizacion = "";
                    int opcion = 0;
                    if (INtipoEnvio.Equals("1"))
                        opcion = 5;
                    else
                        opcion = 8;

                    if (INestado.Equals("2"))
                    {
                        // Se verifica si la comunicación está activa
                        if (!ejecuta_query1("Select top 1 isnull(estado,2) from GENERAL with(nolock) order by idComprobante desc").Equals("2"))
                        {
                            XmlDocument xDocF2 = new XmlDocument();
                            string xDocF = consulta_archivo_xml(codigoControl, opcion);
                            if (!string.IsNullOrEmpty(xDocF))
                            {
                                xDocF2.LoadXml(xDocF);
                                xDocF = "";
                            }
                            // if (estructura(xtrReader, codDoc, xsd)){
                            //if (firmaXADES.Firmar(RutaP12, PassP12, RutaXMLbase + INcodigoControl + ".xml", RutaXMLbase + INcodigoControl + "_Firmado" + ".xml"))
                            if (firmaXADES.Firmar(RutaP12, PassP12, xDocF2, out xDocF))
                            {
                                //StreamReader srXMLFirmado = new StreamReader(RutaXMLbase + INcodigoControl + "_Firmado" + ".xml");
                                //StreamReader srXMLFirmado = new StreamReader(RutaDOC + INcodigoControl + ".xml");
                                byte[] bytesXML = Encoding.Default.GetBytes(xDocF);// Encoding.Default.GetBytes(srXMLFirmado.ReadToEnd());
                                                                                   //srXMLFirmado.Close();
                                msjSRI = "";

                                xDocF2.LoadXml(xDocF);
                                procesa_archivo_xml(xDocF2, codigoControl, INidComprobante, 2);

                                if (enviarComprobante(bytesXML))
                                {
                                    System.Threading.Thread.Sleep(4000); //4 segundos
                                    if (validarAutorizacion(INclaveAcceso))
                                    {
                                        try
                                        {


                                            //Crear el nuevo nodo.

                                            //Actualiza a estado 1....
                                            DB.Conectar();
                                            DB.CrearComando(@"UPDATE GENERAL SET estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                                            DB.AsignarParametroCadena("@estado", "1");
                                            DB.AsignarParametroCadena("@tipo", "E");
                                            DB.AsignarParametroCadena("@codigoControl", INcodigoControl);
                                            DB.EjecutarConsulta1();
                                            DB.Desconectar();

                                            log.mensajesLog("EM010", msjSRI, msjT, "", INcodigoControl, "Final Autorizado 2a");

                                            if (codDoc.Equals("01"))
                                            {
                                                log.mensajesLog("US001", "", "enviando fcturas1 ", "", codigoControl, "leerReenvioSRI");
                                                enviar_correo(INrazonSocialComprador);
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            msjT = e.Message;
                                            DB.Desconectar();
                                            clsLogger.Graba_Log_Error(e.Message); 
                                            log.mensajesLog("EM017", "Excepcion al agregar los nodos Autorización en reproceso", msjT, "", INcodigoControl, "");

                                        }

                                        //cPDF.PoblarReporte(RutaDOC, INcodigoControl, INidComprobante, INcodDoc);
                                        DB.Conectar();
                                        DB.CrearComando(@"UPDATE GENERAL SET creado= @creado, estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                                        DB.AsignarParametroCadena("@creado", "1");
                                        DB.AsignarParametroCadena("@estado", "1");
                                        DB.AsignarParametroCadena("@tipo", "E");
                                        DB.AsignarParametroCadena("@codigoControl", INcodigoControl);
                                        DB.EjecutarConsulta1();
                                        DB.Desconectar();

                                        //Actualizar estado 0 a Factura anulada a la que se aplica nota de credito
                                        if (INcodDoc.Equals("04"))
                                        {
                                            DB.Conectar();
                                            DB.CrearComando(@"UPDATE GENERAL SET estado='0' WHERE estab +'-' + ptoEmi +'-'+ secuencial = (select top 1 numDocModificado from GENERAL with(nolock) where codigoControl = @codigoControl)");
                                            DB.AsignarParametroCadena("@codigoControl", INcodigoControl);
                                            DB.EjecutarConsulta1();
                                            DB.Desconectar();
                                        }
                                    }

                                }
                            }

                        }

                    }
                    else
                    {
                        if (validarAutorizacion(INclaveAcceso))
                        {
                            try
                            {
                                log.mensajesLog("EM010", msjSRI, "", "", INcodigoControl, "Final Autorizado 2b");
                            }
                            catch (Exception e)
                            {
                                msjT = e.Message;
                                log.mensajesLog("EM017", "Excepcion al agregar los nodos Autorización Reenvio", msjT, "", INcodigoControl, "");

                            }
                            //cPDF.PoblarReporte(RutaDOC, INcodigoControl, INidComprobante, INcodDoc);
                            DB.Conectar();
                            DB.CrearComando(@"UPDATE GENERAL SET creado= @creado, estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                            DB.AsignarParametroCadena("@creado", "1");
                            DB.AsignarParametroCadena("@estado", "1");
                            DB.AsignarParametroCadena("@tipo", "E");
                            DB.AsignarParametroCadena("@codigoControl", INcodigoControl);
                            DB.EjecutarConsulta1();
                            DB.Desconectar();


                            if (codDoc.Equals("01"))
                            {
                                log.mensajesLog("US001", "", "enviando fcturas2 ", "", codigoControl, "leerReenvioSRI");
                                enviar_correo(INrazonSocialComprador);
                            }

                            //Actualizar estado 0 a Factura anulada a la que se aplica nota de credito
                            if (INcodDoc.Equals("04"))
                            {
                                DB.Conectar();
                                DB.CrearComando(@"UPDATE GENERAL SET estado='0' WHERE estab +'-' + ptoEmi +'-'+ secuencial = (select top 1 numDocModificado from GENERAL with(nolock) where codigoControl = @codigoControl)");
                                DB.AsignarParametroCadena("@codigoControl", INcodigoControl);
                                DB.EjecutarConsulta1();
                                DB.Desconectar();
                            }
                        }
                        else
                        {
                            log.mensajesLog("EM017", msjSRI, msjT, "", INcodigoControl, INcodigoControl);

                        }
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

        private Boolean estructura(XmlTextReader xmlTR, string doc, string xsd)
        {
            Boolean b = false;
            ValidacionEstructura VE = new ValidacionEstructura();

            VE.agregarSchemas(xsd);
            if (VE.Validar(xmlTR))
            {
                msj += VE.msj;
                msjT += VE.msjT;
                b = true;
            }
            else
            {
                msj += VE.msj;
                msjT += VE.msjT;
                log.mensajesLog("EM015", "", msjT, "", "", "");
                b = false;
            }
            return b;
        }


        private void enviar_correo(string razonSocialComprador )
        {
            string nomDoc = "";
            try
            {
                log.mensajesLog("US001", "", "recogiendo email a enviar  ", "", codigoControl, "leerReenvioSRI");
                emails = recogerValorEmail(idComprobante);
                log.mensajesLog("US001", "", "email a enviar  " + emails, "", codigoControl, "leerReenvioSRI");
                nomDoc = CodigoDocumento(codDoc);
                emails = emails.Trim(',');
                EM = new EnviarMail();

                EM.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);

                if (emails.Length > 10)
                {
                    log.mensajesLog("US001", "", "adjuntando xml  " + emails + " ruta doc " + RutaDOC, "", codigoControl, "leerReenvioSRI");
                    EM.adjuntar(RutaDOC + codigoControl + ".xml");
                    //EM.adjuntar_xml(consulta_archivo_xml(codigoControl, 7), codigoControl + ".xml");
                    log.mensajesLog("US001", "", "adjuntando pdf  " + emails, "", codigoControl, "leerReenvioSRI");
                    EM.adjuntar_xml(cPDF.msPDF(codigoControl), codigoControl + ".pdf");
                    log.mensajesLog("US001", "", "docuemntos ok  " + emails, "", codigoControl, "leerReenvioSRI");

                    asunto = nomDoc + " ELECTRONICA No: " + estab + "-" + ptoEmi + "-" + secuencial + " de " + compania;

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



                    EM.llenarEmailHTML(emailEnviar, emails.Trim(','), mailCCo, "", asunto, htmlView, compania);

                    try
                    {
                        EM.enviarEmail();
                        log.mensajesLog("US001", "", "envio ok  " + emails, "", codigoControl, "leerReenvioSRI");
                    }
                    catch (System.Net.Mail.SmtpException ex)
                    {
                        msjT = ex.Message;
                        //DB.Desconectar();
                        log.mensajesLog("EM001", emails + " ", msjT, "", codigoControl, "");
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
                log.mensajesLog("EM001", emails + "ERROR ENVIAR MAIL", msjT, "", codigoControl, "LeerReenvioSRI");

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
                DB.CrearComando(@" SELECT valor  FROM InfoAdicional with(nolock)  where nombre ='destinatario' and id_Comprobante =@id_Comprobante ");
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
                DB.CrearComando("SELECT top 1 emailsRegla FROM EmailsReglas  with(nolock) WHERE SUBSTRING(nombreRegla,1,6)=SUBSTRING(@rfcrec,1,6) AND estadoRegla=1 and eliminado=0");
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
                log.mensajesLog("US001", "Error al cargar el detalle adicional de los correos email  " + emails, "Mensaje Usuario", ex.Message, codigoControl, "");
                return "";
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
        private Boolean enviarComprobante(Byte[] xml1)
        {
            BasesDatos DB = new BasesDatos();

            Boolean retorno = false;
            string soapResponse = "";
            //string edo = "";
            edo = "";
            XmlDocument xmlEnvio = new XmlDocument();
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate(object sender1,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            }; // éstas líneas son para realizar la transacción con protocolo https

            //Creamos un nuevo Objeto con la referencia que hemos agregado previamente del servicio ASMX.
            //Para este caso la referencia del servicio se llama: "Service".

            if (ambiente.Equals("1"))
            {
                recepcion.Url = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantes";
            }
            else
            {
                recepcion.Url = "https://cel.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantes";
            }

            try
            {
                using (var recepcionTrace = recepcion)
                {
                    //log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " EMPIEZA ENVIO DOCUMENTO");
                    //Se llama a un metodo del servicio.
                    recepcionTrace.Timeout = 15000;
                    var result = recepcionTrace.validarComprobante(xml1);
                    //Se accede a la objeto "XmlRequest" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                    var soapRequest = TraceExtension.XmlRequest.OuterXml;
                    //Se accede a la objeto "XmlResponse" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                    soapResponse = TraceExtension.XmlResponse.OuterXml;
                    //log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " TERMINA ENVIO DOCUMENTO");
                }
                xmlEnvio.LoadXml(soapResponse);
                XmlElement EnvelopeNodo = (XmlElement)xmlEnvio.GetElementsByTagName("soap:Envelope")[0];
                XmlElement BodyNodo = (XmlElement)EnvelopeNodo.GetElementsByTagName("soap:Body")[0];
                XmlElement validarComprobanteNodo = (XmlElement)BodyNodo.GetElementsByTagName("ns2:validarComprobanteResponse")[0];
                XmlElement respuestaNodo = (XmlElement)validarComprobanteNodo.GetElementsByTagName("RespuestaRecepcionComprobante")[0];
                edo = obtener_tag_Element(respuestaNodo,"estado");
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
                    mensajes_error_usuario_envio(mensaje);
                    string mensaje_us = "Error en generar documento: " + obtener_tag_Element(mensaje,"mensaje") + Environment.NewLine + ": " + obtener_tag_Element(mensaje,"informacionAdicional");
                    log.mensajesLog("US001", mensaje_us, soapResponse, "", codigoControl, " Reenvio del Comprobante al SRI.");

                    String estado = "", tipo = "";
                    string indent = obtener_tag_Element(mensaje,"identificador");
                    if (indent.Equals("70") || indent.Equals("43"))
                    {// Clave de acceso en procesamiento
                        //CLAVE ACCESO REGISTRADA
                       
                        estado = "1"; tipo = "E";
                        retorno = true;
                    }
                    else
                    {
                        estado = "0"; tipo = "N";
                    }

                    DB.Conectar();
                    DB.CrearComando(@"UPDATE GENERAL SET estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                    DB.AsignarParametroCadena("@estado", estado);
                    DB.AsignarParametroCadena("@tipo", tipo);
                    DB.AsignarParametroCadena("@codigoControl", codigoControl);
                    DB.EjecutarConsulta1();

                    //return false;
                }
                DB.Desconectar();

            }
            catch (Exception ex)
            {
                msj = "";
                msjT = ex.Message;
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("EM016", msjT, msjT, "", codigoControl, " Reenvio del Comprobante al SRI. ");
                //return false;
            }
            return retorno;
        }

        private Boolean validarAutorizacion(string clave)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                XmlDocument xmlAutorizacion = new XmlDocument();
                xmlRegenerado = "";
                edo = "";
                String aux = "";
                string soapResponse = "";
                System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate (object sender1,
                System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                System.Security.Cryptography.X509Certificates.X509Chain chain,
                System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                }; // éstas líneas son para realizar la transacción con protocolo https
                   //Creamos un nuevo Objeto con la referencia que hemos agregado previamente del servicio ASMX.
                   //Para este caso la referencia del servicio se llama: "Service".

                int intentos_autorizacion = 0, i_contador = 0;
                intentos_autorizacion = int.Parse(ejecuta_query1(@"select top 1 case when intentosautorizacion > 0 then intentosautorizacion else 1 end  from dbo.ParametrosSistema with(nolock)"));
                Boolean b_respuesta = false, b_no_autorizado = false;

                if (ambiente.Equals("1"))
                {
                    autorizacion.Url = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantes";
                }
                else
                {
                    autorizacion.Url = "https://cel.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantes";
                }

                try
                {
                    while (i_contador < intentos_autorizacion && b_respuesta == false && b_no_autorizado == false)
                    {
                        i_contador++;

                        using (var autorizacionTrace = autorizacion)
                        {
                            // log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " EMPIEZA consulta DOCUMENTO");
                            //Se llama a un metodo del servicio.
                            autorizacionTrace.Timeout = 10000;
                            var result = autorizacionTrace.autorizacionComprobante(clave);

                            //Se accede a la objeto "XmlRequest" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                            var soapRequest = TraceExtension.XmlRequest.OuterXml;

                            //Se accede a la objeto "XmlResponse" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                            soapResponse = TraceExtension.XmlResponse.OuterXml;
                            // log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " TERMINA consulta DOCUMENTO");

                        }
                        if (soapResponse.Length > 10)
                        {
                            string SnumeroComprobantes;
                            int InumeroComprobantes = 0;

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
                                //XmlNodeList existe = autorizacionesNodo.GetElementsByTagName("autorizacion");
                                //XmlElement autorizacionNodo = (XmlElement)autorizacionesNodo.GetElementsByTagName("autorizacion")[0];

                                foreach (XmlElement autorizacionNodo in autorizacionesNodo)
                                {
                                    xmlRegenerado = autorizacionNodo.GetElementsByTagName("comprobante")[0].InnerText;
                                    msjSRI = autorizacionNodo.GetElementsByTagName("mensajes")[0].OuterXml;
                                    //edo = autorizacionNodo.GetElementsByTagName("estado")[0].InnerText;

                                    edo = lee_nodo_xml(autorizacionNodo, "estado");

                                    if (edo.Equals("AUTORIZADO"))
                                    {
                                        numeroAutorizacion = lee_nodo_xml(autorizacionNodo, "numeroAutorizacion");  //autorizacionNodo.GetElementsByTagName("numeroAutorizacion")[0].InnerText;
                                        fechaAutorizacion = lee_nodo_xml(autorizacionNodo, "fechaAutorizacion"); //autorizacionNodo.GetElementsByTagName("fechaAutorizacion")[0].InnerText;
                                        DateTime dt_aut = DateTime.Parse(fechaAutorizacion);
                                        aux = dt_aut.ToString("yyyy-MM-ddTHH:mm:ss");//2013-04-01T13:36:25 

                                        DB.Conectar();
                                        DB.CrearComando(@"UPDATE GENERAL SET numeroAutorizacion= @numeroAutorizacion,fechaAutorizacion=@fechaAutorizacion  WHERE codigoControl = @codigoControl");
                                        DB.AsignarParametroCadena("@fechaAutorizacion", aux);
                                        DB.AsignarParametroCadena("@numeroAutorizacion", numeroAutorizacion);
                                        DB.AsignarParametroCadena("@codigoControl", codigoControl);
                                        DB.EjecutarConsulta1();
                                        DB.Desconectar();

                                        //File.Delete(RutaDOC + codigoControl + ".xml");
                                        //StreamWriter sw = new StreamWriter(RutaDOC + codigoControl + ".xml");
                                        //sw.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?>" + autorizacionesNodo.InnerXml);
                                        //sw.Close();

                                        XmlDocument docA = new XmlDocument();
                                        docA.LoadXml(autorizacionNodo.OuterXml);

                                        procesa_archivo_xml(docA, codigoControl, idComprobante, 3);

                                        b_respuesta = true;

                                        //RespuestaLFWS(codDoc, estab + ptoEmi + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "AT", "AT");

                                        try
                                        {
                                            idcomprobante2 = obtenerid_comprobante(codigoControl);
                                            DataTable tb_infoA = obtener_infoAdicional(idcomprobante2);
                                            if (tb_infoA.Rows.Count > 0)
                                            {

                                                RespuestaWebUNASAP(codDoc, estab + "-" + ptoEmi + "-" + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "AT", "AT", tb_infoA.Rows[0]["sociedad"].ToString(), tb_infoA.Rows[0]["numeroAsientoContable"].ToString(), tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            DB.Desconectar();
                                            clsLogger.Graba_Log_Error(ex.Message);
                                            log.mensajesLog("EM016", claveAcceso, ex.Message, "", codigoControl, " Validación de Comprobantes, WebService validación1.2 REPROCESO: No se encontro informacion adicional");
                                        }
                                    }

                                }

                                if (!b_respuesta)
                                {
                                    string v_identificador = "";
                                    string v_mensaje = "";
                                    string v_infoAdicional = "";
                                    string v_tipo = "";
                                    string v_mensaje_historico = "";

                                    if (InumeroComprobantes > 1)
                                    {
                                        foreach (XmlElement autorizacionNodo in autorizacionesNodo)
                                        {
                                            edo = lee_nodo_xml(autorizacionNodo, "estado");
                                            fechaAutorizacion = lee_nodo_xml(autorizacionNodo, "fechaAutorizacion");

                                            v_mensaje_historico = edo + "-" + fechaAutorizacion;


                                            XmlElement mensajes = (XmlElement)autorizacionNodo.GetElementsByTagName("mensajes")[0];

                                            foreach (XmlElement mensaje in mensajes)
                                            {
                                                v_identificador = lee_nodo_xml(mensaje, "identificador");
                                                v_mensaje = lee_nodo_xml(mensaje, "mensaje");
                                                v_infoAdicional = lee_nodo_xml(mensaje, "informacionAdicional");
                                                v_tipo = lee_nodo_xml(mensaje, "tipo");

                                                v_mensaje_historico += "-" + v_tipo + "-" + v_identificador + "-" + v_mensaje + "-" + v_infoAdicional;

                                            }

                                            //log.mensajesLog("EM016", "Mensaje Historico:", v_mensaje_historico, "", codigoControl, " Revalidación del Comprobante al SRI.1");

                                        }
                                    }

                                    XmlElement autorizacionNodo1 = (XmlElement)autorizacionesNodo.GetElementsByTagName("autorizacion")[0];
                                    edo = lee_nodo_xml(autorizacionNodo1, "estado");

                                    if (edo.Equals("NO AUTORIZADO"))
                                    {
                                        b_no_autorizado = true;
                                        string identificador = "";
                                        string mensaje_sri = "";
                                        string s_estado = "0";
                                        string s_tipo = "N";
                                        string s_creado = "0";
                                        msj = " Validación de Comprobantes, WebService validación1.";

                                        XmlElement mensajes = (XmlElement)autorizacionNodo1.GetElementsByTagName("mensajes")[0];
                                        XmlElement mensaje = (XmlElement)autorizacionNodo1.GetElementsByTagName("mensaje")[0];
                                        identificador = obtener_tag_Element(mensaje, "identificador");
                                        mensaje_sri = obtener_tag_Element(mensaje, "mensaje"); // +Environment.NewLine + ": " + mensaje.GetElementsByTagName("informacionAdicional")[0].InnerText;

                                        if (mensaje.GetElementsByTagName("informacionAdicional").Count > 0)
                                        {
                                            mensaje_sri = mensaje_sri + Environment.NewLine + ": " + obtener_tag_Element(mensaje, "informacionAdicional");
                                        }

                                        if (identificador.Equals("40")) //Cuando hay problemas de conexion entre el SRI y el BCE hay que reenviarlo
                                        {
                                            s_estado = "2";
                                            s_tipo = "E";
                                            s_creado = "1";
                                            //log.mensajesLog("EM016", "No autorizado", soapResponse, "", codigoControl, " Error de comunicación SRI-BCE en Reproceso se reenvía el documento ");
                                            msj = msj + "Error de comunicación SRI-BCE se reenvía el documento en Reproceso. ";

                                        }

                                        DB.Conectar();
                                        DB.CrearComando(@"UPDATE GENERAL SET creado= @creado, estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                                        DB.AsignarParametroCadena("@creado", s_creado); // el estado y tipo son iguales como si fuera contingencia...lo que diferencia es creado=0
                                        DB.AsignarParametroCadena("@estado", s_estado);
                                        DB.AsignarParametroCadena("@tipo", s_tipo);
                                        DB.AsignarParametroCadena("@codigoControl", codigoControl);
                                        DB.EjecutarConsulta1();
                                        DB.Desconectar();

                                        //RespuestaLFWS(codDoc, estab + ptoEmi + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "RJ", mensaje_sri);

                                        try
                                        {
                                            idcomprobante2 = obtenerid_comprobante(codigoControl);
                                            DataTable tb_infoA = obtener_infoAdicional(idcomprobante2);
                                            if (tb_infoA.Rows.Count > 0)
                                            {

                                                RespuestaWebUNASAP(codDoc, estab + "-" + ptoEmi + "-" + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "RJ", mensaje_sri, tb_infoA.Rows[0]["sociedad"].ToString(), tb_infoA.Rows[0]["numeroAsientoContable"].ToString(), tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            DB.Desconectar();
                                            clsLogger.Graba_Log_Error(ex.Message);
                                            log.mensajesLog("EM016", claveAcceso, ex.Message, "", codigoControl, " Validación de Comprobantes, WebService validación1.2 REPROCESO: No se encontro informacion adicional");
                                        }

                                    }
                                    else
                                    {
                                        if (i_contador == intentos_autorizacion)
                                        {
                                            if (!actualiza_estado_factura("1", "1", "E", codigoControl))
                                            {
                                                msj = msj + " No se pudo actualizar estado de documento. ";
                                                //log.mensajesLog("EM016", "", msjT, "", codigoControl, " Validación de Comprobantes, WebService validación1 no se pudo actualizar estado de documento en reproceso. ");
                                            }

                                        }
                                    }
                                    mensajes_error_usuario((XmlElement)autorizacionNodo1.GetElementsByTagName("mensajes")[0]);
                                    log.mensajesLog("US001", "Comprobante no autorizado" + clave + " Histórico: " + v_mensaje_historico, msjSRI, "", codigoControl, " Revalidación del Comprobante al SRI.1");
                                    //return false;
                                    b_respuesta = false;

                                }
                            }
                            else
                            {
                                if (i_contador == intentos_autorizacion)
                                {
                                    if (actualiza_estado_factura("1", "1", "E", codigoControl))
                                    {
                                        log.mensajesLog("EM016", clave, soapResponse, "", codigoControl, " Validación de Comprobantes, WebService validación2 en Reproceso ");
                                    }
                                    else
                                    {
                                        log.mensajesLog("EM016", clave, soapResponse, "", codigoControl, " Validación de Comprobantes, WebService validación2, no se pudo actualizar estado de documento en reproceso. ");
                                    }

                                    // mensajes_error_usuario((XmlElement)autorizacionesNodo.GetElementsByTagName("mensajes")[0]);
                                    //log.mensajesLog("EM016", clave, soapResponse, "", "", " Revalidación del Comprobante al SRI.2 ");
                                    //return false;
                                }
                                b_respuesta = false;
                            }
                        }
                        else
                        {
                            if (i_contador == intentos_autorizacion)
                            {
                                log.mensajesLog("EM016", clave, soapResponse, "", codigoControl, " Revalidación del Comprobante al SRI.3 ");
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
                    return b_respuesta;
                }
                catch (Exception ex)
                {
                    DB.Desconectar();
                    clsLogger.Graba_Log_Error(ex.Message);
                    msjT = ex.Message;
                    if (actualiza_estado_factura("1", "1", "E", codigoControl))
                    {
                        log.mensajesLog("EM016", clave, msjT, "", codigoControl, "Revalidación del Comprobante al SRI.4");
                    }
                    else
                    {
                        log.mensajesLog("EM016", clave, msjT, "", codigoControl, " Validación de Comprobantes, WebService validación4: No se actualizó estado de factura en reproceso ");
                    }
                    return false;
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
            return false;
            
        }

        private string obtenerid_comprobante(string codigocontrol)
        {
            string codigoidcomprobante = "";
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando(@"select idComprobante from GENERAL with(nolock) where codigoControl ='" + codigocontrol + "'");
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    if (DR.Read()) { codigoidcomprobante = DR[0].ToString(); }
                    else { codigoidcomprobante = ""; }
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
           
            return codigoidcomprobante;
        }

        public DataTable obtener_infoAdicional(string id_comprobante)
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
            DataTable dt = new DataTable();
            SqlConnection conexion = new SqlConnection(strConn);
            SqlCommand comando = new SqlCommand(StrInfoAdicional1, conexion);
            SqlDataAdapter adap = new SqlDataAdapter(comando);
            adap.Fill(dt);

            return dt;


        }


        public void RespuestaWebUNASAP(string p_codDoc, string p_numDoc, string p_Accesskey, string p_Autorizacion, string p_Authdate, string p_Authtime, string p_Contingency, string p_Contdate, string p_Conttime, string p_status, string p_Message, string Bukrs, string Belnr, string Gjahr)
        {

            try
            {
                System.Threading.Thread.Sleep(2000);

                WebUNASAP.Zecsrifm01 wsap = new WebUNASAP.Zecsrifm01();

                wsap.IDoc = new WebUNASAP.Zecsrist005();

                if (esNDFinLF)
                {
                    p_codDoc = "05A";
                }

                string msj1 = "";

                wsap.IDoc.Bukrs = Bukrs.ToString();
                wsap.IDoc.Belnr = Belnr.ToString();
                wsap.IDoc.Gjahr = Gjahr.ToString();

                DateTime d_fAut = DateTime.Today;
                if (!String.IsNullOrEmpty(p_Authdate)) d_fAut = Convert.ToDateTime(p_Authdate);


                wsap.IDoc.Blart = obtener_codigo(p_codDoc);
                wsap.IDoc.NroSri = p_numDoc;
                wsap.IDoc.Ackey = p_Accesskey;
                wsap.IDoc.Authn = p_Autorizacion;
                wsap.IDoc.Xblnr = "-";
                wsap.IDoc.Stcd1 = "-";

                if (!string.IsNullOrEmpty(p_Autorizacion))
                {
                    wsap.IDoc.Fauth = formato_fecha(p_Authdate, "yyyy-MM-dd");
                    //wsap.IDoc.Dauth = formato_fecha(p_Authdate, "HH:mm:ss");
                }
                else
                {
                    wsap.IDoc.Fauth = "";
                    //wsap.IDoc.Dauth = Convert.ToDateTime(null).ToShortTimeString();
                }

                wsap.IDoc.Status = p_status;
                wsap.IDoc.Msgid = "-";//Truncate(p_Message, 299);

                webunasaps.Url = obtener_codigo("webserviceUNACEMSAP");
                webunasaps.UseDefaultCredentials = true;
                ICredentials credential = new NetworkCredential(obtener_codigo("webserviceUSER"), obtener_codigo("webservicePASS"));
                webunasaps.Credentials = credential;

                XmlDocument xmlEnvio = new XmlDocument();

                string soapResponse = "";

                try 
                {
                    using (var recepcionTrace = webunasaps)
                    {
                        //log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " EMPIEZA ENVIO DOCUMENTO");
                        //Se llama a un metodo del servicio.
                        recepcionTrace.Timeout = 20000;

                        var result = recepcionTrace.Zecsrifm01(wsap);  // recepcionTrace.ntfyElectronicVouchers(ias1, "", out msj1);
                        //Se accede a la objeto "XmlRequest" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                        var soapRequest = TraceExtension.XmlRequest.OuterXml;
                        //Se accede a la objeto "XmlResponse" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                        soapResponse = TraceExtension.XmlResponse.OuterXml;
                        // log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " TERMINA ENVIO DOCUMENTO");
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
                        log.mensajesLog("US001", "Respuesta Exitosa: " + "Estatus: " + o_status + "Mensaje: " + o_message, o_message, "", p_numDoc, "");
                        actualizaEstadosWLF(o_status, p_status, codigoControl);
                    }
                }
                catch (Exception exlf)
                {
                    this.log.mensajesLog("US001", "Error web service n0:Zecsrifm01Response: No se controlo parametro envio primer cath " + p_numDoc, exlf.Message, "", p_numDoc, "ConsultaOff");
                    actualizaEstadosWLF("N", p_status, codigoControl);
                }

                

            }
            catch (Exception ex)
            {
                log.mensajesLog("US001", "Error web service n0:Zecsrifm01Response: No se controlo parametro envio " + p_numDoc, ex.Message, "", p_numDoc, "");
                //logControl.mensajesLog("US001", "Error consulta Web Service GRABARESULTADOFACTURACION()", ex.Message + " soapResponse: " + soapResponse, "", "");
            }

        }

        public void actualizaEstadosWLF(string A_estadoWLF, string A_archivoWLF, string A_codigoControl)
        {
            BasesDatos DB = new BasesDatos();
            try
            {

                DB.Conectar();
                DB.CrearComando(@"UPDATE GENERAL SET  estadoWLF=@estadoWLF,archivoWLF=@archivoWLF WHERE codigoControl = @codigoControl");
                DB.AsignarParametroCadena("@estadoWLF", A_estadoWLF);
                DB.AsignarParametroCadena("@archivoWLF", A_archivoWLF);
                DB.AsignarParametroCadena("@codigoControl", A_codigoControl);
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

        private string msjLectura(int bandera)
        {
            switch (bandera)
            {
                case 1: return "Error Información Tributaria (IF)";
                case 2: return "Error Información Comprobante (IC)";
                case 3: return "Error Totales(T)";
                case 4: return "Error Impuestos Totales (TI)";
                case 5: return "Error Impuestos Retenidos (TIR)";
                case 6: return "Error Motivos (MO)";
                case 7: return "Error Destinatarios (DEST)";
                case 8: return "Error Detalles (DE)";
                case 9: return "Error Impuestos Detalles (IM)";
                case 10: return "Error Detalles Adicionales (DA)";
                case 11: return "Error Información Adicional (IA)";
                case 12: return "Error en los datos de Factura Credito (SERVIENTREGACREDITO)";
                default: return "";
            }
        }

        private void copiarArc(string rutaOrigen, string rutaDestino, string nombre, string cliente)
        {
            DirectoryInfo DIR = new DirectoryInfo(rutaDestino);

            if (!DIR.Exists)
            {
                DIR.Create();
            }
            try
            {
                if (!System.IO.File.Exists(rutaDestino + nombre))
                { System.IO.File.Copy(rutaOrigen + nombre, rutaDestino + nombre.Replace(" ", "")); }
                System.IO.File.Delete(rutaOrigen + nombre);
            }
            catch (Exception e)
            {
                log.mensajesLog("ES001", "", e.Message, codigoControl, "");
            }
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

        private string[] obtenerInfoFactura(string controlInterno)
        {
            string[] info = new string[3];
            BasesDatos DB = new BasesDatos();
            try
            {

                DB.Conectar();
                DB.CrearComando(@"select estab,ptoEmi,secuencial,fecha
                                FROM GENERAL  with(nolock)
                                WHERE  controlInterno=@controlInterno");
                DB.AsignarParametroCadena("@controlInterno", controlInterno);
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    if (DR.Read())
                    {
                        info[0] = DR[0].ToString().Trim() + "-" + DR[1].ToString().Trim() + "-" + DR[2].ToString().Trim();
                        info[1] = DR[3].ToString().Trim();
                        info[2] = "true";
                    }
                    else { info[2] = "false"; }
                }

                DB.Desconectar();
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
            }
            finally
            {
                DB.Desconectar();
            }
            
            return info;
        }

        private void mensajes_error_usuario(XmlElement mensajes)
        {
            XmlElement msg = (XmlElement)mensajes.GetElementsByTagName("mensaje")[0];
            string respuesta = "";
            respuesta = respuesta + obtener_tag_Element(msg,"tipo") + ": " + obtener_tag_Element(msg,"mensaje") + Environment.NewLine;
            if (!respuesta.Equals(""))
            {
                log.mensajesLog("US001", respuesta, "Mensaje Usuario", "", codigoControl, "");
                msjSRI = respuesta;
            }
        }

        private void mensajes_error_usuario_envio(XmlElement msg)
        {
            string respuesta = "";
            //XmlNodeList Listmsg = mensajes.GetElementsByTagName("mensaje");
            //if (Listmsg.Count > 0)
            //{
            //    

            //    foreach (XmlElement msg in Listmsg)
            //    {
            // msg =msg.GetElementsByTagName("mensaje");
            respuesta = respuesta + obtener_tag_Element(msg,"tipo") + ": " + obtener_tag_Element(msg,"mensaje") + Environment.NewLine + ": " + obtener_tag_Element(msg,"informacionAdicional") + Environment.NewLine;
            //}
            log.mensajesLog("US001", respuesta, "Mensaje Usuario", "", codigoControl, "");
        }

        private void rehacer_xml(string INcodDoc, string INcodigoControl, string RutaDOC)
        {

            try
            {
                //System.IO.File.WriteAllText(RutaDOC + INcodigoControl + ".xml", xmlRegenerado);
                //log.mensajesLog("EM010", INcodigoControl, "Reproceso Exitoso", "", codigoControl, "Regeneración del XML.");
            }
            catch (Exception ex)
            {
                log.mensajesLog("EM006", INcodigoControl, "Reproceso Fallido: " + ex.Message, "", codigoControl, "Regeneración del XML.");
            }
        }

        private void recrearPDF()
        {

        }

        private string lee_nodo_xml(XmlNode p_root, string p_tag)
        {
            string retorno = "";
            if (p_root.SelectSingleNode("descendant::" + p_tag) != null)
            {
                retorno = p_root.SelectSingleNode("descendant::" + p_tag).InnerText;
            }
            return retorno;
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
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                msjT = ex.Message;
                log.mensajesLog("BD001", msjT, msjT, "", p_codigoControl, " Error al actualizar estado de documento. Reproceso");
                msjT = "";
                return false;
            }

        }

        public void RespuestaLFWS(string p_codDoc, string p_numDoc, string p_Accesskey, string p_Autorizacion, string p_Authdate, string p_Authtime, string p_Contingency, string p_Contdate, string p_Conttime, string p_status, string p_Message)
        {

            //log.mensajesLog("US001", "Respuesta LF: p_codDoc" + obtener_codigo(p_codDoc) + "p_numDoc:" + p_numDoc + " p_Accesskey:" + p_Accesskey + " p_Autorizacion:" + p_Autorizacion + " p_Authdate:" + p_Authdate + " p_Authtime:" + p_Authtime + " p_Contingency:" + p_Contingency + " p_Contdate:" + p_Contdate + " p_Conttime:" + p_Conttime + " p_status:" + p_status + " p_Message:" + p_Message, "Mensaje Usuario", "", p_Accesskey, "");
            try
            {
                LFWSrpt.infoAutSRIString ias1 = new LFWSrpt.infoAutSRIString();
                LFWSrpt.infoAutSRIString2 ias2 = new LFWSrpt.infoAutSRIString2();
                //LFWSrpt.p_codDoc pdoc = new LFWSrpt.p_codDoc();

                //switch (p_codDoc)
                //{
                //    case "07":
                //        pdoc = LFWSrpt.p_codDoc.E1;
                //        break;
                //    case "05A":
                //        pdoc = LFWSrpt.p_codDoc.E8;
                //        break;
                //    case "05":
                //        pdoc = LFWSrpt.p_codDoc.EB;
                //        break;
                //    case "04":
                //        pdoc = LFWSrpt.p_codDoc.EC;
                //        break;
                //    case "01":
                //        pdoc = LFWSrpt.p_codDoc.EF;
                //        break;
                //    case "06":
                //        pdoc = LFWSrpt.p_codDoc.EG;
                //        break;
                //}


                //pdoc = p_codDoc;
                if (esNDFinLF)
                {
                    p_codDoc = "05A";
                }

                string msj1 = "";

                DateTime d_fAut = DateTime.Today;
                if (!String.IsNullOrEmpty(p_Authdate)) d_fAut = Convert.ToDateTime(p_Authdate);

                ias2.p_codDoc = obtener_codigo(p_codDoc);
                ias2.p_numDoc = p_numDoc;
                ias2.p_Accesskey = p_Accesskey;
                ias2.p_Autorizacion = p_Autorizacion;
                if (!string.IsNullOrEmpty(p_Autorizacion))
                {
                    ias2.p_Authdate = formato_fecha(p_Authdate, "dd/MM/yyyy");
                    ias2.p_Authtime = formato_fecha(p_Authdate, "HH:mm:ss");
                }
                else
                {
                    ias2.p_Authdate = "";
                    ias2.p_Authtime = "";
                }

                ias2.p_Contingency = p_Contingency;
                if (!String.IsNullOrEmpty(p_Contingency))
                {
                    ias2.p_Contdate = formato_fecha(p_Contdate, "dd/MM/yyyy");
                    ias2.p_Conttime = formato_fecha(p_Authdate, "HH:mm:ss");
                }
                else
                {
                    ias2.p_Contdate = "";
                    ias2.p_Conttime = "";
                }

                ias2.p_status = p_status;
                ias2.p_Message = Truncate(p_Message, 299);

                ias1.infoAutSRIString1 = ias2;

                wsLF.Url = obtener_codigo("webserviceLF");

                XmlDocument xmlEnvio = new XmlDocument();
                string soapResponse = "";

                using (var recepcionTrace = wsLF)
                {
                    //log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " EMPIEZA ENVIO DOCUMENTO");
                    //Se llama a un metodo del servicio.
                    recepcionTrace.Timeout = 20000;
                    var result = recepcionTrace.ntfyElectronicVouchers(ias1, "", out msj1);
                    //Se accede a la objeto "XmlRequest" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                    var soapRequest = TraceExtension.XmlRequest.OuterXml;
                    //Se accede a la objeto "XmlResponse" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                    soapResponse = TraceExtension.XmlResponse.OuterXml;
                    // log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " TERMINA ENVIO DOCUMENTO");
                }
                xmlEnvio.LoadXml(soapResponse);
                XmlElement EnvelopeNodo = (XmlElement)xmlEnvio.GetElementsByTagName("soapenv:Envelope")[0];
                XmlElement BodyNodo = (XmlElement)EnvelopeNodo.GetElementsByTagName("soapenv:Body")[0];
                XmlElement respuestaNodo = (XmlElement)BodyNodo.GetElementsByTagName("ser-root:ntfyElectronicVouchersResponse")[0];

                if (respuestaNodo != null)
                {
                    string o_status = "", o_message = "";
                    o_status = lee_nodo_xml(respuestaNodo, "o_status");
                    o_message = lee_nodo_xml(respuestaNodo, "o_message");
                    //log.mensajesLog("US001", "Respuesta Exitosa: " + "Estatus: " + o_status + "Mensaje: " + o_message, o_message, "", p_numDoc, "");
                }

            }
            catch (Exception ex)
            {
                log.mensajesLog("US001", "Error web service infoAutSRIString: " + p_numDoc, ex.Message, "", p_numDoc, "");
                //logControl.mensajesLog("US001", "Error consulta Web Service GRABARESULTADOFACTURACION()", ex.Message + " soapResponse: " + soapResponse, "", "");
            }

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
                log.mensajesLog("US001", "Error al convertir fecha: " + ex.Message, ex.Message, "", p_fecha, "");
            }
            return rpt;
        }

        private string obtener_codigo(string a_parametro)
        {
            string retorna = System.Configuration.ConfigurationManager.AppSettings.Get(a_parametro);

            return retorna;
        }

        private Boolean ConsultaNDFin(string p_CodControl)
        {
            Boolean rpt = false;
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando(@"select top 1 g.termino from GENERAL g with(nolock) where g.codigoControl = @p_codControl");
                DB.AsignarParametroCadena("@p_codControl", p_CodControl);
                using (DbDataReader DR5 = DB.EjecutarConsulta())
                {
                    if (DR5.Read())
                    {
                        if (DR5["termino"].ToString().Equals("05A"))
                            rpt = true;
                    }
                }

                DB.Desconectar();
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("BD001", claveAcceso, ex.Message, "", codigoControl, " Error en metodo de reproceso ConsultaNDFin.");

            }
            return rpt;
        }

        public string Truncate(string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                return value.Substring(0, maxLength);
            }

            return value;
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

        public string consulta_archivo_xml(string p_codigoControl, int p_opcion)
        {
            string rpt = "";
            BasesDatos DB = new BasesDatos();

            try
            {
                DB.Conectar();
                DB.CrearComandoProcedimiento("PA_ARCHIVO_XML");
                DB.AsignarParametroProcedimiento("@documentoXML", System.Data.DbType.Xml, "");
                DB.AsignarParametroProcedimiento("@codigoControl", System.Data.DbType.String, p_codigoControl);
                DB.AsignarParametroProcedimiento("@idComprobante", System.Data.DbType.Int32, 0);
                DB.AsignarParametroProcedimiento("@opcion", System.Data.DbType.Int32, p_opcion);
                using (DbDataReader dr = DB.EjecutarConsulta())
                {
                    if (dr.Read())
                    {
                        rpt = @"<?xml version=""1.0"" encoding=""UTF-8""?>" + dr[0].ToString();
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
                log.mensajesLog("EM001", ex.Message, ex.StackTrace, "", codigoControl, "Método obtener_tag_Element");
            }
            return rpt;
        }

    }
}