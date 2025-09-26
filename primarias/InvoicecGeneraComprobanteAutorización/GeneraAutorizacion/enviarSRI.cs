using Datos;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Logs;
using CriptoSimetrica;

namespace GeneraAutorizacion
{
    class enviarSRI
    {

        BasesDatos DB;
        Logs.Log log = new Logs.Log();
        private recWeb.RecepcionComprobantesOfflineService recepcion;
        FirmarXML firmaXADES;
        private DbDataReader DR;
        private AES Cs;
        string ambiente ="", codigoControl = "", claveAcceso = "", idComprobante = "";
        string codDoc = "", estab = "", ptoEmi = "", secuencial = "", fechaEmision = "", msjSRI="";
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

        public enviarSRI()
        {
            DB = new BasesDatos();
            recepcion = new recWeb.RecepcionComprobantesOfflineService();
            firmaXADES = new FirmarXML();
            Cs = new AES();
        }


        public void consultarDocumentoOffline(string idComprobante, string claveAcceso, string ambiente, string codigoControl,
            string codDoc, string estab, string ptoEmi, string secuencial, string fechaEmision, string INrazonSocialComprador)
        {
            this.ambiente = ambiente;
            this.codigoControl = codigoControl;
            this.claveAcceso = claveAcceso;
            this.idComprobante = idComprobante;
            this.codDoc = codDoc; this.estab = estab; 
            this.ptoEmi = ptoEmi;
            this.secuencial = secuencial;
            this.fechaEmision = fechaEmision;
            parametrosSistema();

            XmlDocument xDocF2 = new XmlDocument();
            string xDocF = consulta_archivo_xml_2(codigoControl, 5);
            if (!string.IsNullOrEmpty(xDocF))
            {
                xDocF2.LoadXml(xDocF);
                xDocF = "";
            }
            log.guardar_Log("firmando documento  " + codigoControl);
            if (firmaXADES.Firmar(RutaP12, PassP12, xDocF2, out xDocF))
            {
                byte[] bytesXML = Encoding.Default.GetBytes(xDocF);
                msjSRI = "";
                xDocF2.LoadXml(xDocF);
                procesa_archivo_xml(xDocF2, codigoControl, "0", 2);

                log.guardar_Log("enviando comprobante al sri  " + codigoControl);
                log.mensajesLog("US001", "", "enviando comprobante al sri ", "", codigoControl, "leerReenvioSRI");
                if (enviarComprobante(bytesXML))
                {
                    DB.Conectar();
                    DB.CrearComando(@"UPDATE GENERAL SET estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                    DB.AsignarParametroCadena("@estado", "4");
                    DB.AsignarParametroCadena("@tipo", "P");
                    DB.AsignarParametroCadena("@codigoControl", codigoControl);
                    DB.EjecutarConsulta1();
                    DB.Desconectar();
                }

            }


        }

        public string consulta_archivo_xml_2(string p_codigoControl, int p_opcion)
        {
            string rpt = "";
            string valorXML = "";

            try
            {
                DB.Conectar();
                DB.CrearComandoProcedimiento("PA_ARCHIVO_XML");
                DB.AsignarParametroProcedimiento("@documentoXML", System.Data.DbType.Xml, "");
                DB.AsignarParametroProcedimiento("@codigoControl", System.Data.DbType.String, p_codigoControl);
                DB.AsignarParametroProcedimiento("@idComprobante", System.Data.DbType.String, "");
                DB.AsignarParametroProcedimiento("@opcion", System.Data.DbType.Int32, p_opcion);
                DbDataReader dr = DB.EjecutarConsulta();
                if (dr.Read())
                {
                    rpt = @"<?xml version=""1.0"" encoding=""UTF-8""?>" + dr[0].ToString();
                    valorXML = dr[0].ToString();
                }
                DB.Desconectar();
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                log.mensajesLog("US001" + "Error en proceso consulta_archivo_xml. " + ex.Message, ex.StackTrace, ex.Message, "", codigoControl, "LeerReenvio.cs");
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

            string paso = "";
            try
            {
                paso = "1";
                DB.Conectar();
                DB.CrearComando(@"select idComprobante, codDoc+estab+ptoEmi+secuencial as numeroDocumento
                                    from GENERAL where codigoControl =@p_codigoControl");
                DB.AsignarParametroCadena("@p_codigoControl", @p_codigoControl);
                DbDataReader DR1 = DB.EjecutarConsulta();
                if (DR1.Read())
                {
                    idComprobante = DR1["idComprobante"].ToString();
                    numeroDocumento = DR1["numeroDocumento"].ToString();
                }
                DB.Desconectar();

                paso = "2";
                DB.Conectar();
                DB.CrearComando("update GENERAL set estado='0', tipo='N' where idComprobante=@idComprobante ");
                DB.AsignarParametroCadena("@idComprobante", idComprobante);
                DbDataReader DR2 = DB.EjecutarConsulta();
                DB.Desconectar();

                paso = "3";
                DB.Conectar();
                DB.CrearComando(@"select top 1 idLog from LogWebService where tipo like '%" + numeroDocumento + "%' order by 1 desc");
                DbDataReader DR3 = DB.EjecutarConsulta();
                if (DR3.Read())
                {
                    idLog = DR3["idLog"].ToString();
                }
                DB.Desconectar();

                paso = "4";
                DB.Conectar();
                DB.CrearComando("update LogWebService set estado='P' where idLog=@idLog ");
                DB.AsignarParametroCadena("@idLog", idLog);
                DbDataReader DR4 = DB.EjecutarConsulta();
                DB.Desconectar();
                paso = "5";
            }
            catch (Exception ex)
            {
                log.guardar_Log("error metodo  regularizarXML paso  " + paso + " error--> " + ex.ToString());
            }

        }

        public void procesa_archivo_xml(XmlDocument p_documentoXML, string p_codigoControl, string p_idComprobante, int p_opcion)
        {
            XmlDocument fRec4 = new XmlDocument();
            XmlElement facturaXML;
            string sXml = "";
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
                log.mensajesLog("US001" + "Error en proceso procesa_archivo_xml. " + ex.Message, ex.Message, ex.Message, "", codigoControl, "");
            }
        }

        public void parametrosSistema()
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
                    passCredencial = Cs.desencriptar(DR[4].ToString().Trim(), "CIMAIT");
                    RutaDOC = DR[5].ToString().Trim();
                    RutaTXT = DR[6].ToString().Trim();
                    RutaBCK = DR[7].ToString().Trim();
                    RutaCER = DR[8].ToString().Trim();
                    RutaKEY = DR[9].ToString().Trim();
                    emailEnviar = DR[10].ToString().Trim();
                    RutaP12 = DR[11].ToString().Trim();
                    PassP12 = DR[12].ToString().Trim();
                    RutaXMLbase = DR[13].ToString().Trim();

                    PassP12 = Cs.desencriptar(PassP12, "CIMAIT");

                }
                DB.Desconectar();
            }
            catch (Exception ed)
            {
                DB.Desconectar();
            }

        }

        public Boolean enviarComprobante(Byte[] xml1)
        {
            bool retorno = false;
            string soapResponse = "";

            string edo = "";
            XmlDocument xmlEnvio = new XmlDocument();
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate (object sender1,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

            recepcion.Url = System.Configuration.ConfigurationManager.AppSettings.Get("Recepcion");

            try
            {

                using (var recepcionTrace = recepcion)
                {
                    recepcionTrace.Timeout = 20000;
                    var result = recepcionTrace.validarComprobante(xml1);
                    var soapRequest = TraceExtension.XmlRequest.OuterXml;
                    soapResponse = TraceExtension.XmlResponse.OuterXml;
                }
                log.guardar_Log("enviarComprobante --> " + codigoControl + " --- " + soapResponse.ToString());
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
                //msj = "";
                //msjT = ex.Message;
                return false;
            }

            return retorno;

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
