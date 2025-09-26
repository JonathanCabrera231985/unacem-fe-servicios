using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using Control;
using System.Xml;
using Datos;
using System.Data.Common;
using System.Net;
using System.Data.SqlClient;
using Control;

namespace Interfaz
{
				public partial class Pruebawebservices : Form
				{
								public Pruebawebservices()
								{
												InitializeComponent();
								}
								Log log = new Log();
								string ambiente, codigoControl, claveAcceso, idComprobante;
								string edo = "", msjSRI = "", msjT = "", numeroAutorizacion = "", fechaAutorizacion = "", msj = "";
								string codDoc; string estab; string ptoEmi; string secuencial; string fechaEmision;

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
								public Control.TestUnacemSap.ZECSRIFM01 webunasapsTEST = new Control.TestUnacemSap.ZECSRIFM01();

								private Control.autoWeb.AutorizacionComprobantesService autorizacion;
								BasesDatos DB;
								private DbDataReader DR;
								private EnviarMail EM;

								private void button1_Click(object sender, EventArgs e)
								{

                                    

												autorizacion = new Control.autoWeb.AutorizacionComprobantesService();
												validarAutorizacion("1810201706179023686200120120240001562830015628315");
											//	RespuestaWebapp("001-001-000000002", "3110201401099129543700110010011230000012300000119");
								}

								private string obtener_codigo(string a_parametro)
								{
												string retorna = ConfigurationManager.AppSettings.Get(a_parametro);

												return retorna;
								}
								//public void RespuestaWebapp(string p_numDoc, string p_Accesskey)
								//{
								//    string respuesta = "",estab="012",ptoEmi="999";
												
								//    try
								//    {

								//        if ((estab.Equals(obtener_codigo("estabprod")) && ptoEmi.Equals(obtener_codigo("ptoemiprod"))) || (estab.Equals(obtener_codigo("estabprue")) && ptoEmi.Equals(obtener_codigo("ptoemiprue"))))
								//        {


								//            System.Threading.Thread.Sleep(2000);

								//            DateTime d_fAut = DateTime.Now;
								//            string fecha = d_fAut.ToString("yyyy-MM-ddTHH:mm:ss");
								//            Control.ec.com.unacem.aplicaciones.WebServiceUNACEM wsapp = new Control.ec.com.unacem.aplicaciones.WebServiceUNACEM();
								//            wsapp.Url = obtener_codigo("webserviceapp");
								//            respuesta = wsapp.of_autorizaGuiaRemision(p_numDoc, p_Accesskey, Convert.ToDateTime(fecha));
								//            log.mensajesLog("US001", "Respuesta of_autorizaGuiaRemision: " + "Mensaje: " + respuesta, respuesta, "", p_Accesskey, "");
																
								//        }

								//    }
								//    catch (Exception ex)
								//    {
								//        respuesta="";
								//        //log.mensajesLog("US001", "Error web service n0:of_autorizaGuiaRemision: No se controlo parametro envio " + p_numDoc, ex.Message, "", codigoControl, "");
								//    }

								//}

								private Boolean validarAutorizacion(string clave)
								{
												XmlDocument xmlAutorizacion = new XmlDocument();
												ambiente = "2";
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
												intentos_autorizacion = int.Parse(ejecuta_query1(@"select top 1 case when intentosautorizacion > 0 then intentosautorizacion else 1 end  from dbo.ParametrosSistema with(nolock)"));
												Boolean b_respuesta = false, b_no_autorizado = false;

												if (ambiente.Equals("1"))
												{
																autorizacion.Url = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";
												}
												else
												{
																autorizacion.Url = "https://cel.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";
												}

												try
												{
																while (i_contador < intentos_autorizacion && b_respuesta == false && b_no_autorizado == false)
																{
																				i_contador++;

																				using (var autorizacionTrace = autorizacion)
																				{

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
																								xmlAutorizacion.LoadXml(soapResponse);
																								XmlElement EnvelopeNodo = (XmlElement)xmlAutorizacion.GetElementsByTagName("soap:Envelope")[0];
																								XmlElement BodyNodo = (XmlElement)EnvelopeNodo.GetElementsByTagName("soap:Body")[0];
																								XmlElement autorizacionComprobanteNodo = (XmlElement)BodyNodo.GetElementsByTagName("ns2:autorizacionComprobanteResponse")[0];
																								XmlElement respuestaNodo = (XmlElement)autorizacionComprobanteNodo.GetElementsByTagName("RespuestaAutorizacionComprobante")[0];
																								XmlElement autorizacionesNodo = (XmlElement)respuestaNodo.GetElementsByTagName("autorizaciones")[0];
																								foreach (XmlElement autorizacionNodo in autorizacionesNodo)
																								{
																												//XmlElement autorizacionNodo = (XmlElement)autorizacionesNodo.GetElementsByTagName("autorizacion")[0];
																												msjSRI = autorizacionNodo.GetElementsByTagName("mensajes")[0].OuterXml;
																												edo = obtener_tag_Element(autorizacionNodo, "estado");
																												if (edo.Equals("AUTORIZADO"))
																												{
																																numeroAutorizacion = obtener_tag_Element(autorizacionNodo, "numeroAutorizacion");
																																fechaAutorizacion = obtener_tag_Element(autorizacionNodo, "fechaAutorizacion");
																																DateTime dt_aut = DateTime.Parse(fechaAutorizacion);
																																aux = dt_aut.ToString("yyyy-MM-ddTHH:mm:ss");


																																DB.Conectar();
																																DB.CrearComando(@"UPDATE GENERAL SET numeroAutorizacion= @numeroAutorizacion,fechaAutorizacion=@fechaAutorizacion , estado ='1' , tipo='E'  WHERE codigoControl = @codigoControl");
																																DB.AsignarParametroCadena("@fechaAutorizacion", aux);
																																DB.AsignarParametroCadena("@numeroAutorizacion", numeroAutorizacion);
																																DB.AsignarParametroCadena("@codigoControl", codigoControl);
																																DB.EjecutarConsulta1();
																																DB.Desconectar();

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

																																																DB.Conectar();
																																																DB.CrearComando(@"UPDATE GENERAL SET creado= @creado, estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
																																																DB.AsignarParametroCadena("@creado", s_creado); // el estado y tipo son iguales como si fuera contingencia...lo que diferencia es creado=0
																																																DB.AsignarParametroCadena("@estado", s_estado);
																																																DB.AsignarParametroCadena("@tipo", s_tipo);
																																																DB.AsignarParametroCadena("@codigoControl", codigoControl);
																																																DB.EjecutarConsulta1();
																																																DB.Desconectar();

																																																enviar_notificacion_correo_punto(estab, codDoc + estab + ptoEmi + secuencial, fechaEmision, mensaje_sri);

																																																// RespuestaLFWS(codDoc, estab + ptoEmi + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "RJ", mensaje_sri);

																																																try
																																																{

																																																				DataTable tb_infoA = obtener_infoAdicional(idComprobante);
																																																				if (tb_infoA.Rows.Count > 0)
																																																				{
																																																								RespuestaWebUNASAPTEST(codDoc, estab + "-" + ptoEmi + "-" + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "RJ", mensaje_sri, tb_infoA.Rows[0]["sociedad"].ToString(), tb_infoA.Rows[0]["numeroAsientoContable"].ToString(), tb_infoA.Rows[0]["anioAsientoContable"].ToString());
																																																				}
																																																}
																																																catch (Exception ex)
																																																{
																																																				log.mensajesLog("EM016", claveAcceso, ex.Message, "", codigoControl, " Validación de Comprobantes, WebService validación1: No se encontro informacion adicional");
																																																}

																																												}
																																								}
																																				}
																																				else
																																				{
																																								if (i_contador == intentos_autorizacion)
																																								{
																																												// emite_doc_prov();
																																												if (!actualiza_estado_factura("1", "1", "E", codigoControl))
																																												{
																																																msj = msj + " No se pudo actualizar estado de documento. ";
																																																//log.mensajesLog("EM016", claveAcceso, msjT, "", codigoControl, " Validación de Comprobantes, WebService validación1 no se pudo actualizar estado de documento. ");
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

																																				// mensajes_error_usuario((XmlElement)autorizacionesNodo.GetElementsByTagName("mensajes")[0]);
																																				//log.mensajesLog("EM016", claveAcceso, soapResponse, "", estab + ptoEmi + secuencial, " Validación de Comprobantes, WebService validación2 ");
																																				//return false;


																																}
																																b_respuesta = false;
																												}
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

																return b_respuesta;
												}
												catch (Exception ex)
												{
																msjT = ex.Message;
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
																msjT = ex.Message;
																log.mensajesLog("BD001", claveAcceso, msjT, "", codigoControl, " Error al actualizar estado de documento.");
																msjT = "";
																return false;
												}

								}


								public void RespuestaWebUNASAPTEST(string p_codDoc, string p_numDoc, string p_Accesskey, string p_Autorizacion, string p_Authdate, string p_Authtime, string p_Contingency, string p_Contdate, string p_Conttime, string p_status, string p_Message, string Bukrs, string Belnr, string Gjahr)
								{

												try
												{
																System.Threading.Thread.Sleep(2000);

																Control.TestUnacemSap.Zecsrifm01 wsapTEST = new Control.TestUnacemSap.Zecsrifm01();

																wsapTEST.IDoc = new Control.TestUnacemSap.Zecsrist005();

																if (codDoc.Equals("05"))
																{
																				p_codDoc = "05A";
																}

																string msj1 = "";

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
																				wsapTEST.IDoc.Dauth = Convert.ToDateTime(null); //Convert.ToDateTime((string)null).ToShortTimeString();
																}

																wsapTEST.IDoc.Status = p_status;
																wsapTEST.IDoc.Msgid = "-";//Truncate(p_Message, 299);

																webunasapsTEST.Url = obtener_codigo("webserviceUNACEMSAPTEST");
																webunasapsTEST.UseDefaultCredentials = true;
																ICredentials credential = new NetworkCredential(obtener_codigo("webserviceUSERTEST"), obtener_codigo("webservicePASSTEST"));
																webunasapsTEST.Credentials = credential;

																XmlDocument xmlEnvio = new XmlDocument();

																string soapResponse = "";
																using (var recepcionTrace = webunasapsTEST)
																{
																				//log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " EMPIEZA ENVIO DOCUMENTO");
																				//Se llama a un metodo del servicio.
																				recepcionTrace.Timeout = 20000;

																				var result = recepcionTrace.Zecsrifm01(wsapTEST);  // recepcionTrace.ntfyElectronicVouchers(ias1, "", out msj1);
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
																				log.mensajesLog("US001", "Respuesta Exitosa: " + "Estatus: " + o_status + "Mensaje: " + o_message, o_message, "", codigoControl, "");
																}
												}
												catch (Exception ex)
												{
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
																log.mensajesLog("US001", "Error al convertir fecha: " + ex.Message, ex.Message, "", p_fecha, "");
												}
												return rpt;
								}
								private String ejecuta_query1(String query)
								{
												String retorno = "";
												DB = new BasesDatos();
												DB.Conectar();
												DB.CrearComando(query);
												DR = DB.EjecutarConsulta();
												while (DR.Read())
												{
																retorno = DR[0].ToString();
												}

												DB.Desconectar();
												DR.Dispose();
												return retorno;
								}

								private void enviar_notificacion_correo_punto(string pr_estab, string pr_folio, string pr_fechaEmision, string pr_mensaje)
								{
												String correos = "", asunto = "", mensaje = "";
												string nomDoc = "";
												DB.Conectar();
												DB.CrearComando(@"select a.correo from  dbo.Sucursales a with(nolock) where a.clave = '" + pr_estab + "'");
												DbDataReader DR3 = DB.EjecutarConsulta();
												while (DR3.Read())
												{
																correos = correos.Trim(',') + "," + DR3[0].ToString().Trim(',') + "";
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
																				log.mensajesLog("EM001", " ", msjT, "", codigoControl, "");
																				msjT = "";
																}
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
												DB.Conectar();
												DB.CrearComando(@"select servidorSMTP,puertoSMTP,sslSMTP,userSMTP,passSMTP,
							  dirdocs,dirtxt,dirrespaldo,dircertificados,dirllaves,emailEnvio,
							  dirp12,passP12,dirXMLbase 
							  from ParametrosSistema with(nolock)");
												DR = DB.EjecutarConsulta();
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
												DB.Desconectar();

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

				}
}
