using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.Xml.Linq;
using Datos;
using System.Data;

namespace consultaAutorizacion
{
    public class Validar
    {
        BasesDatos DB;
        private autoWeb.AutorizacionComprobantesOfflineService autorizacion;

        public Validar()
        {
            DB = new BasesDatos();
            autorizacion = new autoWeb.AutorizacionComprobantesOfflineService();
        }

        public Boolean validarAutorizacion(string clave, string ambiente, string codigoControl, string RutaXMLbase, string RutaDOC)
        {
            bool resp = false;
            string soapResponse = "", xmlRegenerado = "", edo = "", msjSRI = "", numeroAutorizacion = "", fechaAutorizacion = "",
                aux = "";
            XmlDocument xmlAutorizacion = new XmlDocument();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate(object sender1,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

            String urlAutorizacion = "";
            
            autorizacion.Url = System.Configuration.ConfigurationManager.AppSettings.Get("Autorizacion");

            using (var autorizacionTrace = autorizacion)
            {
                var result = autorizacionTrace.autorizacionComprobante(clave);
                autorizacion.Timeout = 20000;
                //Se accede a la objeto "XmlRequest" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                var soapRequest = TraceExtension.XmlRequest.OuterXml;
                //Se accede a la objeto "XmlResponse" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                soapResponse = TraceExtension.XmlResponse.OuterXml;

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
                    foreach (XmlElement autorizacionNodo in autorizacionesNodo)
                    {
                        xmlRegenerado = autorizacionNodo.GetElementsByTagName("comprobante")[0].InnerText;
                        msjSRI = autorizacionNodo.GetElementsByTagName("mensajes")[0].OuterXml;
                        edo = lee_nodo_xml(autorizacionNodo, "estado");

                        if (edo.Equals("AUTORIZADO"))
                        {
                            numeroAutorizacion = autorizacionNodo.GetElementsByTagName("numeroAutorizacion")[0].InnerText;
                            fechaAutorizacion = autorizacionNodo.GetElementsByTagName("fechaAutorizacion")[0].InnerText;
                            try
                            {
                                DateTime dt_aut = DateTime.Parse(fechaAutorizacion);
                                aux = dt_aut.ToString("yyyy-MM-ddTHH:mm:ss");//2013-04-01T13:36:25 
                            }
                            catch (Exception ex)
                            {
                                edo = "";
                                actualiza_estado_factura("4", "1", "P", codigoControl);
                                return false;
                            }

                            DB.Conectar();
                            DB.CrearComando(@"UPDATE GENERAL SET numeroAutorizacion= @numeroAutorizacion,fechaAutorizacion=@fechaAutorizacion , estado=1, tipo='E'  WHERE codigoControl = @codigoControl");
                            DB.AsignarParametroCadena("@fechaAutorizacion", aux);
                            DB.AsignarParametroCadena("@numeroAutorizacion", numeroAutorizacion);
                            DB.AsignarParametroCadena("@codigoControl", codigoControl);
                            DB.EjecutarConsulta1();
                            DB.Desconectar();

                            if (System.IO.File.Exists(RutaXMLbase + codigoControl + "_Firmado.xml"))
                            {
                                System.IO.File.Copy(RutaXMLbase + codigoControl + "_Firmado.xml", RutaDOC + codigoControl + ".xml", true);
                            }
                            System.IO.File.Delete(RutaDOC + codigoControl + ".xml");
                            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(RutaDOC + codigoControl + ".xml");
                            streamWriter.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?><autorizacion>" + autorizacionNodo.InnerXml + "</autorizacion>");
                            streamWriter.Close();
                            this.DB.Conectar();
                            this.DB.TraerDataSetConsulta(string.Concat(new string[]
									{
                                        "update ArchivoXml set xmlSRI = '<autorizacion>",
										autorizacionNodo.InnerXml,
                                        "</autorizacion>' where codigoControl = '",
										codigoControl,
										"' "
									}), new object[0]);

                            return true;
                        }

                    }

                    XmlElement autorizacionNodo1 = (XmlElement)autorizacionesNodo.GetElementsByTagName("autorizacion")[0];
                    edo = lee_nodo_xml(autorizacionNodo1, "estado");
                    if (edo.Equals("NO AUTORIZADO"))
                    {
                        string identificador = "";
                        string mensaje_sri = "";
                        string s_estado = "0";
                        string s_tipo = "N";
                        string s_creado = "0";
                        XmlElement mensajes = (XmlElement)autorizacionNodo1.GetElementsByTagName("mensajes")[0];
                        foreach (XmlElement xmlElement9 in mensajes)
                        {
                            if (!xmlElement9.GetElementsByTagName("identificador")[0].InnerText.Equals("68") & !xmlElement9.GetElementsByTagName("identificador")[0].InnerText.Equals("60"))
                            {
                                identificador = xmlElement9.GetElementsByTagName("identificador")[0].InnerText;
                                mensaje_sri = xmlElement9.GetElementsByTagName("mensaje")[0].InnerText; // +Environment.NewLine + ": " + mensaje.GetElementsByTagName("informacionAdicional")[0].InnerText;

                                if (xmlElement9.GetElementsByTagName("informacionAdicional").Count > 0)
                                {
                                    mensaje_sri = mensaje_sri + Environment.NewLine + ": " + xmlElement9.GetElementsByTagName("informacionAdicional")[0].InnerText;
                                }

                                if (identificador.Equals("40")) //Cuando hay problemas de conexion entre el SRI y el BCE hay que reenviarlo
                                {
                                    s_estado = "2";
                                    s_tipo = "E";
                                    s_creado = "1";
                                    //log.mensajesLog("EM016", "No autorizado", soapResponse, "", "", " Error de comunicación SRI-BCE en Reproceso se reenvía el documento ");
                                }

                                DB.Conectar();
                                DB.CrearComando(@"UPDATE GENERAL SET creado= @creado, estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                                DB.AsignarParametroCadena("@creado", s_creado); // el estado y tipo son iguales como si fuera contingencia...lo que diferencia es creado=0
                                DB.AsignarParametroCadena("@estado", s_estado);
                                DB.AsignarParametroCadena("@tipo", s_tipo);
                                DB.AsignarParametroCadena("@codigoControl", codigoControl);
                                DB.EjecutarConsulta1();
                                DB.Desconectar();

                            }
                        }
                    }
                }
            }

            return resp;
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
                return false;
            }

        }
    }
}
