using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Datos;
using System.IO;
using ValSign;
using System.Data.Common;
using CriptoSimetrica;
using System.Configuration;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using clibLogger;

namespace Control
{
    public class LeerOffline
    {
        //BasesDatos DB;
        Log log;
        XmlTextReader xtrReader;
        FirmarXML firmaXADES;
        //private DbDataReader DR;
        Key_Electronica.Key_Electronica FirmaBCE;
        private AES Cs;
        private receWeb.RecepcionComprobantesService recepcion;
        EnviarMail EM;
        GuardarBD gBD;
        CrearPDF cPDF;
        public TestUnacemSap.ZECSRIFM01 webunasapsTEST = new TestUnacemSap.ZECSRIFM01();
        public WebUNASAP.ZECSRIFM01 webunasaps = new WebUNASAP.ZECSRIFM01();

        private string compania = "UNACEM ECUADOR S.A.";
        private string msj = "";
        private string msjT = "";
        private string msjSRI = "";
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
        private string edo;

        private Boolean esNDFinLF;
        //PMONCAYO 20200814 (Etiquetas de exportacion)
        private string comercioExterior = "";
        private string incoTermFactura = "";
        private string lugarIncoTerm = "";
        private string paisOrigen = "";
        private string puertoEmbarque = "";
        private string puertoDestino = "";
        private string paisDestino = "";
        private string paisAdquisicion = "";
        private string incoTermTotalSinImpuestos = "";
        private string fleteInternacional = "";
        private string seguroInternacional = "";
        private string gastosAduaneros = "";
        private string gastosTransporteOtros = "";

        /******  region variables base datos ********/
        #region otrosCampos
        /*********** otrosCampos ****************/
        private string claveAcceso;
        private string secuencial;
        private string guiaRemision;
        private string codigoControl;
        private string idLog;

        #endregion

        #region InformacionTributaria
        /*********** InformacionTributaria ****************/
        private string ambiente; private string tipoEmision; private string razonSocial;
        private string nombreComercial; private string ruc;
        private string codDoc; private string estab; private string ptoEmi; private string version;
        private string dirMatriz; private string emails; private string firmaSRI;
        #endregion

        #region infromacionDocumento
        /*********** infromacionDocumento ****************/
        private string fechaEmision;
        private string razonSocialComprador;
        /**********motivos *****************/
        ArrayList arraylMotivos;
        #endregion
        /******  region variables base datos ********/

        public LeerOffline()
        {
            //DB = new BasesDatos();
            log = new Log();
            firmaXADES = new FirmarXML();
            FirmaBCE = new Key_Electronica.Key_Electronica();
            Cs = new AES();
            recepcion = new receWeb.RecepcionComprobantesService();
            gBD = new GuardarBD();
            cPDF = new CrearPDF();
        }

        public void procesarOff(XmlDocument xDoc)
        {
            log.mensajesLog("US001", "", "", "", "procesarOff paso 0-->", "clase de error Invoice.cs");
            parametrosSistema();
            string temp = "";
            if (xDoc != null)
            {
                log.mensajesLog("US001", "", "", "", "procesarOff paso 1-->", "clase de error Invoice.cs");
                temp = xDoc.OuterXml;
                byte[] bytes = Encoding.Default.GetBytes(temp);
                temp = Encoding.UTF8.GetString(bytes);
                xDoc.LoadXml(temp);
                xDoc.InnerXml = temp;
                log.mensajesLog("US001", "", "", "", "procesarOff paso 2-->", "clase de error Invoice.cs");
                if (xDoc != null)
                {
                    procesa_archivo_xml(xDoc, codigoControl, "0", 1);
                    xtrReader = new XmlTextReader(new StringReader(xDoc.OuterXml));
                    InformacionXSD();
                    log.mensajesLog("US001", "", "", "", "procesarOff paso 3-->", "clase de error Invoice.cs");
                    if (estructura(xtrReader, codDoc, xsd))
                    {
                        log.mensajesLog("US001", "", "", "", "procesarOff paso 4-->", "clase de error Invoice.cs");
                        XmlDocument xDocF2 = new XmlDocument();
                        string xDocF = "";
                        xDocF = xDoc.OuterXml;
                        //if (firmaXADES.Firmar(RutaP12, PassP12, xDoc, out xDocF))

                      if (true)
                        {
                            log.mensajesLog("US001", "", "", "", "procesarOff paso 5-->", "clase de error Invoice.cs");
                            byte[] bytesXML = Encoding.Default.GetBytes(xDocF);
                            msjSRI = "";
                            xDocF2.LoadXml(xDocF);
                            procesa_archivo_xml(xDocF2, codigoControl, "0", 2);
                            //if (enviarComprobante(bytesXML))
                            //{
                            log.mensajesLog("US001", "", "", "", "procesarOff paso 6-->", "clase de error Invoice.cs");
                            gBD.informacionDocumentoExportacion(this.comercioExterior, this.incoTermFactura, this.lugarIncoTerm, this.paisOrigen, this.puertoEmbarque, this.puertoDestino, this.paisDestino, this.paisAdquisicion, this.incoTermTotalSinImpuestos, this.fleteInternacional, this.seguroInternacional, this.gastosAduaneros, this.gastosTransporteOtros);
                            if (gBD.guardarBD())
                            {
                                log.mensajesLog("US001", "", "", "", "procesarOff paso 7-->", "clase de error Invoice.cs");
                                ActualizaTablaGeneral("2", "E", codigoControl);
                                ActualizaTablaLogWebService("A", idLog);
                                try
                                {
                                    DataTable tb_infoA = obtener_infoAdicional(codigoControl);
                                    if (tb_infoA.Rows.Count > 0)
                                    {
                                        log.mensajesLog("US001", claveAcceso, "datos a enviar sociedad", "", codigoControl, "informacion  numero documento " + estab + "-" + ptoEmi + "-" + secuencial + "  sociedad " + tb_infoA.Rows[0]["sociedad"].ToString() + " numeroAsientoContable " + tb_infoA.Rows[0]["numeroAsientoContable"].ToString() + " anioAsientoContable " + tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                        RespuestaWebUNASAP(codDoc, estab + "-" + ptoEmi + "-" + secuencial, claveAcceso, claveAcceso, "", "", "", "", "", "RC", "RC", tb_infoA.Rows[0]["sociedad"].ToString(), tb_infoA.Rows[0]["numeroAsientoContable"].ToString(), tb_infoA.Rows[0]["anioAsientoContable"].ToString());

                                    }
                                    if (codDoc.Equals("06"))
                                    {

                                        if ((estab.Equals(obtener_codigo("estabprod")) && ptoEmi.Equals(obtener_codigo("ptoemiprod"))) || (estab.Equals(obtener_codigo("estabprue")) && ptoEmi.Equals(obtener_codigo("ptoemiprue"))))
                                        {

                                            log.mensajesLog("US001", "E", "", "", "entrando a webservices mina -->", "clase de error leerOffLine.cs");
                                            RespuestaWebapp(estab + "-" + ptoEmi + "-" + secuencial, claveAcceso);
                                            log.mensajesLog("US001", "E", "", "", "saliendo a webservices mina -->", "clase de error leerOffLine.cs");

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.mensajesLog("EM050", claveAcceso, ex.Message, "", codigoControl, " Validación de Comprobantes, WebService validación1: No se encontro informacion adicional");
                                }
                            }
                        }
                    }
                }
            }
        }


        public DataTable obtener_infoAdicional(string codigoControl)
        {
            DataTable dt = new DataTable();
            BasesDatos DB = new BasesDatos();
            try
            {
                string id_comprobante = "";
                DB.Conectar();
                DB.CrearComando(@"select idComprobante  from GENERAL with(nolock) where codigoControl = '" + codigoControl + "'");
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    while (DR3.Read())
                    {
                        id_comprobante = DR3["idComprobante"].ToString();
                    }
                }

                DB.Desconectar();

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
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("EM016", claveAcceso, "error en procedimiento info adicional: ", "", codigoControl, "error " + ex.Message);
                return dt;
            }
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
                wsap.IDoc.Authn = "";
                wsap.IDoc.Xblnr = "-";
                wsap.IDoc.Stcd1 = "-";

                if (!string.IsNullOrEmpty(p_Autorizacion))
                {
                    wsap.IDoc.Fauth = this.formato_fecha(p_Authdate, "yyyy-MM-dd");
                    wsap.IDoc.Dauth = this.formato_fecha(p_Authdate, "HH:mm:ss");
                }
                else
                {
                    wsap.IDoc.Fauth = "";
                    wsap.IDoc.Dauth ="";
                }

                wsap.IDoc.Status = p_status;
                wsap.IDoc.Msgid = "-";

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
                        recepcionTrace.Timeout = 20000;
                        var result = recepcionTrace.Zecsrifm01(wsap);
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
                        log.mensajesLog("US001", "Respuesta Exitosa: " + "Estatus: " + o_status + "Mensaje: " + o_message, o_message, p_status, codigoControl, p_status);
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
                log.mensajesLog("US001", "Error web service n0:Zecsrifm01Response: No se controlo parametro envio " + p_numDoc, ex.Message, "", codigoControl, "");
            }

        }

        public void RespuestaWebapp(string p_numDoc, string p_Accesskey)
        {
            string respuesta = "";
            try
            {
                System.Threading.Thread.Sleep(2000);
                DateTime d_fAut = DateTime.Now;
                string fecha = d_fAut.ToString("yyyy-MM-ddTHH:mm:ss");
                log.mensajesLog("US001", "", "", "", "entrando a webservices mina2-->", "clase de error leerOffLine.cs");

                mina.WebServiceUNACEM wsapp = new mina.WebServiceUNACEM();


                wsapp.Url = obtener_codigo("webserviceapp");


                respuesta = wsapp.of_autorizaGuiaRemision(p_numDoc, p_Accesskey, Convert.ToDateTime(fecha));

                log.mensajesLog("US001", "Respuesta of_autorizaGuiaRemision: " + "Mensaje: " + respuesta, respuesta, "", p_Accesskey, "");

            }
            catch (Exception ex)
            {
                log.mensajesLog("US001", "Error web service n0:of_autorizaGuiaRemision: No se controlo parametro envio " + p_numDoc, ex.Message, "", codigoControl, "");
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

        private string lee_nodo_xml(XmlNode p_root, string p_tag)
        {
            string retorno = "";
            if (p_root.SelectSingleNode("descendant::" + p_tag) != null)
            {
                retorno = p_root.SelectSingleNode("descendant::" + p_tag).InnerText;//.InnerText;
            }
            return retorno;
        }

        public Boolean enviarComprobante(Byte[] xml1)
        {
            bool retorno = false;
            string soapResponse = "";

            edo = "";
            XmlDocument xmlEnvio = new XmlDocument();
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate (object sender1,
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
                xmlEnvio.LoadXml(soapResponse);
                XmlElement EnvelopeNodo = (XmlElement)xmlEnvio.GetElementsByTagName("soap:Envelope")[0];
                XmlElement BodyNodo = (XmlElement)EnvelopeNodo.GetElementsByTagName("soap:Body")[0];
                XmlElement validarComprobanteNodo = (XmlElement)BodyNodo.GetElementsByTagName("ns2:validarComprobanteResponse")[0];
                XmlElement respuestaNodo = (XmlElement)validarComprobanteNodo.GetElementsByTagName("RespuestaRecepcionComprobante")[0];
                edo = obtener_tag_Element(respuestaNodo, "estado");
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
            }
            catch (Exception ex)
            {
                msj = "";
                msjT = ex.Message;
                return false;
            }

            return retorno;

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
                log.mensajesLog("EM015", " Los datos no son correctos. ", msjT, "", codigoControl, "");
                b = false;
            }
            return b;
        }

        public void InformacionXSD()
        {
            switch (codDoc)
            {
                case "01":
                    xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\factura.xsd";
                    break;
                case "03":
                    xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\LiquidacionCompra.xsd";
                    break;
                case "04":
                    xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\notaCredito.xsd";
                    break;
                case "05":
                    xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\notaDebito.xsd";
                    break;
                case "06":
                    xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\guiaRemision.xsd";
                    break;
                case "07":
                    if (version == "2.0.0" || version == "2.0")
                    {
                        xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\ComprobanteRetencion_V2.0.0.xsd";
                    }
                    else
                    {
                        xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\comprobanteRetencion.xsd";
                    }

                    break;
            }


        }

        public void parametrosSistema()
        {
            BasesDatos DB = new BasesDatos();
            try
            {

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

        private void enviar_notificacion_correo_punto(string pr_estab, string pr_folio, string pr_fechaEmision, string pr_mensaje)
        {
            String correos = "", asunto = "", mensaje = "";
            string nomDoc = "";
            BasesDatos DB = new BasesDatos();
            try
            {
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
                        log.mensajesLog("EM001", emails + " ", msjT, "", codigoControl, "");
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

        private void ActualizaTablaLogWebService(string estado, string idLog)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                using (var x = DB.TraerDataSetConsulta(@"update LogWebService set estado = @p1 where idLog = @p2 ", new Object[] { estado, idLog }))
                {
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

        private void ActualizaTablaGeneral(string g_estado, string g_tipo, string g_codigoControl)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Desconectar();
                DB.Conectar();
                DB.CrearComando(@"UPDATE GENERAL SET  estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                DB.AsignarParametroCadena("@estado", g_estado);
                DB.AsignarParametroCadena("@tipo", g_tipo);
                DB.AsignarParametroCadena("@codigoControl", g_codigoControl);
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
        private void ActualizaTablaGeneral(string g_creado, string g_codigoControl)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Desconectar();
                DB.Conectar();
                DB.CrearComando(@"UPDATE GENERAL SET creado = @creado  WHERE codigoControl = @codigoControl");
                DB.AsignarParametroCadena("@creado", g_creado);
                DB.AsignarParametroCadena("@codigoControl", g_codigoControl);
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

        private void enviar_correo()
        {
            string nomDoc = "", asunto = "";
            try
            {
                if (consultaTablaGeneralCreado(codigoControl).Equals("0"))
                {
                    nomDoc = CodigoDocumento(codDoc);
                    emails = emails.Trim(',');
                    EM = new EnviarMail();

                    EM.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);

                    if (emails.Length > 10)
                    {

                        EM.adjuntar_xml(consulta_archivo_xml(codigoControl, 6), codigoControl + ".xml");
                        clsLogger.Graba_Log_Info("se adjunto xml");
                        EM.adjuntar_xml(cPDF.msPDF(codigoControl), codigoControl + ".pdf");
                        clsLogger.Graba_Log_Info("se adjunto pdf");
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
                        htmlBody.Append("<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td><td><br/>Estimado(a) " + razonSocialComprador + "<br/><br/>Adjunto s&iacute;rvase encontrar su " + nomDoc + " ELECTRONICA No: " + estab + "-" + ptoEmi + "-" + secuencial + @"&sup1; y el archivo PDF&sup2; de dicho 
						                 	comprobante que hemos emitido en nuestra empresa.<br/> Gracias por preferirnos.<br/><br/> Atentamente, " + " <br/> <img height=\"50\" width=\"50\" align=\"middle\" src=\"cid:image001\" /><br/>" + compania +
                        @"<br/>--------------------------------------------------------------------------------" +
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

                        EM.llenarEmailHTML(emailEnviar, emails.Trim(','), mailCCo, "", asunto, htmlView, compania);

                        try
                        {
                            EM.enviarEmail();
                            ActualizaTablaGeneral("1", codigoControl);//actualiza campo creado

                        }
                        catch (System.Net.Mail.SmtpException ex)
                        {
                            ActualizaTablaGeneral("2", codigoControl);//actualiza campo creado
                            msjT = ex.Message;
                            //DB.Desconectar();
                            clsLogger.Graba_Log_Error(ex.Message);
                            log.mensajesLog("EM001", emails + " ", msjT, "", codigoControl, "");
                        }
                    }
                    else
                    {
                        if (codDoc.Equals("06"))
                        {
                            cPDF.msPDF(codigoControl);
                            ActualizaTablaGeneral("2", codigoControl);
                        }
                    }
                }
            }
            catch (Exception mex)
            {

                msjT = mex.Message;
                //DB.Desconectar();
                clsLogger.Graba_Log_Error(mex.Message);
                log.mensajesLog("EM001", emails + "ERROR ENVIAR MAIL", msjT, "", codigoControl, "");
                ActualizaTablaGeneral("2", codigoControl);//actualiza campo creado

            }
        }

        public MemoryStream consulta_archivo_xml(string p_codigoControl, int p_opcion)
        {
            MemoryStream rpt = new MemoryStream();
            string doc = "";
            string repl1 = "";
            string repl2 = "";
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Desconectar();
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
                        repl1 = dr[0].ToString().Replace("&lt;", "<");
                        repl2 = repl1.ToString().Replace("&gt;", ">");
                        doc = @"<?xml version=""1.0"" encoding=""UTF-8""?>" + repl2.ToString();
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

        private string obtener_codigo(string a_parametro)
        {
            string retorna = ConfigurationManager.AppSettings.Get(a_parametro);

            return retorna;
        }

        private string consultaTablaGeneralCreado(string p_codigoControl)
        {
            string result = "";
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando("select creado from GENERAL with(nolock) where codigoControl ='" + p_codigoControl + "'");
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        result = DR["creado"].ToString();
                    }
                }
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
            
            return result;
        }

        /************************************* variables base datos *****************************/

        public void otrosCampos(string claveAcceso, string secuencial, string guiaRemision, string codigoControl, string idLog, Boolean esNDFinLF)
        {
            this.claveAcceso = claveAcceso;
            this.secuencial = secuencial;
            this.guiaRemision = guiaRemision;
            this.codigoControl = codigoControl;
            this.idLog = idLog;
            this.esNDFinLF = esNDFinLF;

            gBD.otrosCampos(claveAcceso, secuencial, guiaRemision, codigoControl);
        }

        public void InformacionTributaria(string ambiente, string tipoEmision, string razonSocial, string nombreComercial, string ruc,
           string claveAcceso, string codDoc, string estab, string ptoEmi, string secuencial, string dirMatriz, string emails, string version = null)
        {
            this.ambiente = ambiente; this.tipoEmision = tipoEmision; this.razonSocial = razonSocial;
            this.nombreComercial = nombreComercial; this.ruc = ruc; this.claveAcceso = claveAcceso;
            this.codDoc = codDoc; this.estab = estab; this.ptoEmi = ptoEmi; this.secuencial = secuencial;
            this.dirMatriz = dirMatriz; this.emails = emails; this.version = version;

            gBD.InformacionTributaria(ambiente, tipoEmision, razonSocial, nombreComercial, ruc, claveAcceso, codDoc, estab, ptoEmi, secuencial, dirMatriz, emails);
        }

        public void infromacionDocumento(string fechaEmision, string dirEstablecimiento, string dirEstablecimientoGuia, string contribuyenteEspecial, string obligadoContabilidad, string tipoIdentificacionComprador,
           string guiaRemision, string razonSocialComprador, string identificacionComprador, string moneda,
           string dirPartida, string razonSocialTransportista, string tipoIdentificacionTransportista, string rucTransportista, string rise, string fechaIniTransporte, string fechaFinTransporte, string placa,//Guia de Remision
                                               string codDocModificado, string numDocModificado, string fechaEmisionDocSustentoNota, string valorModificacion, string motivo, string direccionComprador)
        {
            this.fechaEmision = fechaEmision;
            this.razonSocialComprador = razonSocialComprador;
            gBD.infromacionDocumento(fechaEmision, dirEstablecimiento, dirEstablecimientoGuia, contribuyenteEspecial, obligadoContabilidad, tipoIdentificacionComprador, guiaRemision, razonSocialComprador, identificacionComprador, moneda, dirPartida, razonSocialTransportista, tipoIdentificacionTransportista, rucTransportista, rise, fechaIniTransporte, fechaFinTransporte, placa, codDocModificado, numDocModificado, fechaEmisionDocSustentoNota, valorModificacion, motivo, direccionComprador);
        }

        public void Motivos(ArrayList arraylMotivos)
        {
            this.arraylMotivos = arraylMotivos;
            gBD.Motivos(arraylMotivos);
        }

        public void detalles(ArrayList arraylDetalles)
        {
            gBD.detalles(arraylDetalles);
        }

        public void motivoND(string motivo)
        {
            gBD.motivoND(motivo);
        }

        public void cantidades(string subtotal12, string subtotal0, string subtotalNoSujeto, string totalSinImpuestos,
                             string totalDescuento, string ICE, string IVA12, string importeTotal, string propina, string importeAPagar)
        {
            gBD.cantidades(subtotal12, subtotal0, subtotalNoSujeto, totalSinImpuestos, totalDescuento, ICE, IVA12, importeTotal, propina, importeTotal);
        }

        public void comprobanteRetencion(string periodoFiscal, string tipoIdentificacionSujetoRetenido, string razonSocialSujetoRetenido, string identificacionSujetoRetenido, string tipoSujetoRetenido = null, string parteRel = null)
        {
            gBD.comprobanteRetencion(periodoFiscal, tipoIdentificacionSujetoRetenido, razonSocialSujetoRetenido, identificacionSujetoRetenido, tipoSujetoRetenido, parteRel);
        }


        public void totalImpuestos(ArrayList arraylTotalImpuestos)
        {
            gBD.totalImpuestos(arraylTotalImpuestos);
        }
        public void totalConImpuestos(ArrayList arraylTotalConImpuestos)
        {
            gBD.totalConImpuestos(arraylTotalConImpuestos);
        }
        public void Destinatarios(ArrayList arraylDestinatarios)
        {
            gBD.Destinatarios(arraylDestinatarios);
        }

        public void guarda_destinatario_receptor(string ruc, string razonSocial)
        {
            gBD.guarda_destinatario_receptor(ruc, razonSocial);
        }

        public void informacionAdicional(ArrayList arraylInfoAdicionales)
        {
            gBD.informacionAdicional(arraylInfoAdicionales);
        }

        public void infromacionAdicionalCima(string termino, string proforma, string pedido, string domicilio, string telefono, string emails, string firmaSRI)
        {
            this.emails = emails; this.firmaSRI = firmaSRI;
            gBD.infromacionAdicionalCima(termino, proforma, pedido, domicilio, telefono, emails, emails);
        }

        public void detallesAdicionales(ArrayList arraylDetallesAdicionales)
        {
            gBD.detallesAdicionales(arraylDetallesAdicionales);
        }

        public void impuestos(ArrayList arraylImpuestosDetalles)
        {
            gBD.impuestos(arraylImpuestosDetalles);
        }

        public void totalImpuestosRetenciones(ArrayList arraylTotalImpuestosRetenciones)
        {
            gBD.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);
        }

        public void xmlComprobante(string version, string idComprobante, string guiaServ)
        {
            gBD.xmlComprobante(version, "comprobante", "");
        }

        public void DetallePagos(System.Collections.ArrayList arraylPagos)
        {
            gBD.DetallePagos(arraylPagos);
        }


        public void DocSustentos(System.Collections.ArrayList arraylDoscsSutentos)
        {
            gBD.docsSustentos(arraylDoscsSutentos);
           
        }

        public void ReembolsosSustentos(System.Collections.ArrayList arraylReembolsosRetenciones)
        {
            gBD.reembolsosSustentos(arraylReembolsosRetenciones);
        }

        public void ImpuestosReembolsosSustentos(System.Collections.ArrayList arraylImpuestosReembolsosRet)
        {
            gBD.impuestosReembolsosSustentos(arraylImpuestosReembolsosRet);
        }

        public void DetalleRubros(System.Collections.ArrayList arraylRubros)
        {
            gBD.DetalleRubros(arraylRubros);
        }

        public void DetalleCompensacion(System.Collections.ArrayList arraylCompensacion)
        {
            gBD.DetalleCompensacion(arraylCompensacion);
        }


        //PMONCAYO 20200814 (Etiquetas de exportacion)
        public void informacionDocumentoExportacion(string comercioExterior, string incoTermFactura, string lugarIncoTerm, string paisOrigen, string puertoEmbarque, string puertoDestino, string paisDestino, string paisAdquisicion, string incoTermTotalSinImpuestos, string fleteInternacional, string seguroInternacional, string gastosAduaneros, string gastosTransporteOtros)
        {
            this.comercioExterior = comercioExterior;
            this.incoTermFactura = incoTermFactura;
            this.lugarIncoTerm = lugarIncoTerm;
            this.paisOrigen = paisOrigen;
            this.puertoEmbarque = puertoEmbarque;
            this.puertoDestino = puertoDestino;
            this.paisDestino = paisDestino;
            this.paisAdquisicion = paisAdquisicion;
            this.incoTermTotalSinImpuestos = incoTermTotalSinImpuestos;
            this.fleteInternacional = fleteInternacional;
            this.seguroInternacional = seguroInternacional;
            this.gastosAduaneros = gastosAduaneros;
            this.gastosTransporteOtros = gastosTransporteOtros;
        }
    }
}
