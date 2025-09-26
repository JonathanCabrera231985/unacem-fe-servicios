using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Datos;
using System.Collections;
using System.Xml;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Data;
using ReportesDEI;
using System.Globalization;
using clibLogger;

namespace Control
{ 
    public class Recepcion
    {
        #region Variables

        //private BasesDatos DB;
        private LogRecepcion log;
        private autoWeb.AutorizacionComprobantesService autorizacion;
        //private DbDataReader DR;
        private EnviarMail EM;

        //
        private string rucEmisor = "";

        //Version
        string tipoComprobante, idComprobante, version;
        //Informacion Tributaria
        string ambiente, tipoEmision, razonSocial, nombreComercial, ruc, claveAcceso, codDoc, estab, ptoEmi, secuencial, dirMatriz;
        //Informacion del Documento(Factura,guia,notas,retenciones)
        string fechaEmision, dirEstablecimiento, contribuyenteEspecial, obligadoContabilidad, tipoIdentificacionComprador;
        string guiaRemision, razonSocialComprador, identificacionComprador, totalSinImpuestos, totalDescuento, propina, importeTotal, moneda;

        string emails;
        string fechaAutorizacion = "";
        string numeroAutorizacion = "";
        string msjSRI = "", msj = "", msjT = "", mensaje_sri = "";
        XmlElement facturaXML, facturaXML_recibida, facturaXMLAut;
        string doc_version = "";

        //XML 2_00
        string rucCedulaComprador = "";

        //boolean doc procesado
        bool v_docProcesado = false;

        XmlDocument fRec2;

        //correo
        private string servidor = "", emailCredencial = "", passCredencial = "";
        private int puerto;
        private bool ssl;
        private string compania = "UNACEM ECUADOR S.A.";
        private string RutaDOC = "";
        private string emailEnviar = "";

        #endregion

        public Recepcion()
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                //firmaXADES = new FirmarXML();

                //numA = new NumerosALetras();
                log = new LogRecepcion();

                //gBD = new GuardarBD();
                //gXml = new GenerarXml();
                //cPDF = new CrearPDF();
                //r = new Random(DateTime.Now.Millisecond);
                //recepcion = new receWeb.RecepcionComprobantesService();
                autorizacion = new autoWeb.AutorizacionComprobantesService();


                //Parametros Generales
                DB.Conectar();
                DB.CrearComando(@"select rfcEmisor,servidorSMTP,puertoSMTP,sslSMTP,userSMTP,passSMTP,
                              dirdocs,dirRecepcion,dirrespaldo,dircertificados,dirllaves,emailEnvio,
                              dirp12,passP12,dirXMLbase 
                              from ParametrosSistema with(nolock)");
                try
                {
                    using (DbDataReader DR = DB.EjecutarConsulta())
                    {
                        while (DR.Read())
                        {
                            rucEmisor = DR[0].ToString(); rucEmisor.Trim();
                            servidor = DR[1].ToString();
                            puerto = Convert.ToInt32(DR[2]);
                            ssl = Convert.ToBoolean(DR[3]);
                            emailCredencial = DR[4].ToString();
                            passCredencial = DR[5].ToString();
                            RutaDOC = DR[6].ToString();
                            //RutaTXT = DR[6].ToString();
                            //RutaBCK = DR[7].ToString();
                            //RutaCER = DR[8].ToString();
                            //RutaKEY = DR[9].ToString();
                            emailEnviar = DR[11].ToString();
                            //RutaP12 = DR[11].ToString();
                            //PassP12 = DR[12].ToString();
                            //RutaXMLbase = DR[13].ToString();
                        }
                    }

                    DB.Desconectar();
                    //Fin de Parametros Generales.
                }
                catch (Exception e)
                {
                    DB.Desconectar();
                    clsLogger.Graba_Log_Error(e.Message);
                    log.mensajesLog("ES001", "", e.Message, "", "");
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

        public void procesarRecepcion(XmlDocument archivo, string email)
        {
            #region Seteo Variables

            seteo_variables();


            #endregion

            emails = email;

            archivo.InnerXml = Regex.Replace(archivo.InnerXml, @"\t|\n|\r", "");
            archivo.InnerXml = VerificaAcentos(archivo.InnerXml);

            XmlNode nodoArchivo = archivo.DocumentElement;
            claveAcceso = lee_nodo_xml(nodoArchivo, "claveAcceso");
            ambiente = lee_nodo_xml(nodoArchivo, "ambiente");
            doc_version = lee_atributo_nodo_xml(nodoArchivo, "version");
            numeroAutorizacion = lee_nodo_xml(nodoArchivo, "numeroAutorizacion"); 
            fechaAutorizacion = lee_nodo_xml(nodoArchivo, "fechaAutorizacion"); 

            //doc_version = lee_nodo_xml(nodoArchivo, "version");
            if (claveAcceso.Equals(""))
            {
                if (doc_version.Equals("2_00")) //factura CNT
                {
                    procesarArchivoXML_2_00(nodoArchivo, "R", emails);
                    v_docProcesado = true;

                    //DataSet dsFactura_1_0_0 = new DataSet();
                    //dsFactura_1_0_0.ReadXml(@"DATA\xml1_0_0.xml");
                    //factura1_0_0 report = new factura1_0_0();
                    //report.SetDataSource(dsFactura_1_0_0);
                }
                else
                {
                    facturaXML_recibida = (XmlElement)archivo.GetElementsByTagName("comprobante")[0];
                    fRec2 = new XmlDocument();
                    fRec2.LoadXml(facturaXML_recibida.InnerText);
                    if (fRec2 != null)
                    {
                        facturaXML_recibida = fRec2.DocumentElement;
                        claveAcceso = lee_nodo_xml(facturaXML_recibida, "claveAcceso");
                        doc_version = lee_atributo_nodo_xml(facturaXML_recibida, "version");

                    }
                }
            }

            
            if (!claveAcceso.Equals("") && !v_docProcesado)
            {
                if (string.IsNullOrEmpty(ambiente)) ambiente = "2";
                if (validarAutorizacion(claveAcceso, ambiente))
                {
                    XmlNode nodoFact = facturaXML;
                    doc_version = lee_atributo_nodo_xml(nodoFact, "version");

                    if (doc_version.Equals("1.0.0") || doc_version.Equals("1.1.0"))
                    {
                        procesarArchivoXML_1_0_0(nodoFact, "R", emails);
                    }
                }
                else
                {
                    facturaXML_recibida = archivo.DocumentElement;
                    if (!facturaXML_recibida.IsEmpty)
                    {
                        doc_version = lee_atributo_nodo_xml(facturaXML_recibida, "version"); //facturaXML_recibida.Attributes["version"].Value;

                        if (doc_version.Equals("1.0.0") || doc_version.Equals("1.1.0"))
                        {
                            facturaXMLAut = facturaXML_recibida;
                            procesarArchivoXML_1_0_0(facturaXML_recibida, "P", emails);
                        }
                    }
                }
            }
            else
            {
                log.mensajesLog("US001", "No se encuentra la clave de acceso. MailEmisor:" + emails, archivo.OuterXml, identificacionComprador, estab + ptoEmi + secuencial, claveAcceso, codDoc);
            }
            
            //Reproceso de docuementos que no se pudieron validar
            //ProcesoDocsPendientes();

        }




        //XmlElement detallesAdicionales = (XmlElement)xmlbase.GetElementsByTagName("detallesAdicionales")[0];
        //                    foreach (XmlElement da in detallesAdicionales)
        //                    {
        //                        banderaArchivo = 10;
        //                        asDetallesAdicionales = new String[4];
        //                        detAdicionalNombre = da.Attributes["nombre"].Value;
        //                        detAdicionalValor = da.Attributes["valor"].Value;
        //                        codigoTemp = codigoPrincipal;
        //                        //idDetallesTemp = idDetallesTemp;
        //                        asDetallesAdicionales[0] = detAdicionalNombre; asDetallesAdicionales[1] = detAdicionalValor;
        //                        asDetallesAdicionales[2] = codigoTemp; asDetallesAdicionales[3] = idDetallesTemp.ToString().Trim();
        //                        arraylDetallesAdicionales.Add(asDetallesAdicionales);
        //                        gBD.detallesAdicionales(arraylDetallesAdicionales);
        //                        gXml.detallesAdicionales(arraylDetallesAdicionales);
        //                    }


        public void seteo_variables()
        {
            #region Seteo variables
            tipoComprobante = ""; idComprobante = ""; version = "";
            //Informacion Tributaria
            ambiente = ""; tipoEmision = ""; razonSocial = ""; nombreComercial = ""; ruc = ""; codDoc = ""; estab = ""; ptoEmi = ""; secuencial = ""; dirMatriz = "";
            //Informacion del Documento(Factura="";guia="";notas="";retenciones)
            fechaEmision = ""; dirEstablecimiento = ""; contribuyenteEspecial = ""; obligadoContabilidad = ""; tipoIdentificacionComprador = "";
            guiaRemision = ""; razonSocialComprador = ""; identificacionComprador = ""; totalSinImpuestos = "0"; totalDescuento = ""; propina = "0"; importeTotal = "0"; moneda = "0";
            fechaAutorizacion = "";
            numeroAutorizacion = "";

            // xml 2_00
            rucCedulaComprador = ""; claveAcceso = ""; idComprobante = "";

            v_docProcesado = false;

            emails = "";
            claveAcceso = "";
            fechaAutorizacion = "";
            numeroAutorizacion = "";
            facturaXML_recibida = null;
            facturaXML = null;
            facturaXMLAut = null;
            fRec2 = null;
            doc_version = "";

            msjSRI = ""; msj = ""; msjT = ""; mensaje_sri = "";

            #endregion

        }

        public void procesarArchivoXML_1_0_0(XmlNode xmlNodo, string p_estado, string p_mail)
        {
            XmlNode root = xmlNodo;

            codDoc = lee_nodo_xml(root, "codDoc");
            codDoc = codDoc.PadLeft(2, '0');

            switch (codDoc)
            {
                case "01":
                case "04":
                case "05":
                    identificacionComprador = lee_nodo_xml(root, "identificacionComprador");
                    identificacionComprador.Trim();
                    break;

                case "06":
                    identificacionComprador = lee_nodo_xml(root, "identificacionDestinatario");
                    identificacionComprador.Trim();
                    break;

                case "07":
                    identificacionComprador = lee_nodo_xml(root, "identificacionSujetoRetenido");
                    identificacionComprador.Trim();
                    break;

                default:
                    //log.mensajesLog("US001", "No se detecta código del documento" + ": Clave Acceso: " + claveAcceso, root.OuterXml, " CodDoc: " + codDoc, "Recepción", claveAcceso);
                    log.mensajesLog("US001", "No se detecta código del documento. MailEmisor:" + emails, root.OuterXml, identificacionComprador + emails, estab + ptoEmi + secuencial, claveAcceso, codDoc);
                    break;
            }

            if (identificacionComprador.Equals(rucEmisor))
            {
                if (!valida_duplicidad_claveAcceso(claveAcceso))
                {
                    ruc = lee_nodo_xml(root, "ruc");
                    idComprobante = "";
                    fechaEmision = lee_nodo_xml(root, "fechaEmision");
                    razonSocial = lee_nodo_xml(root, "razonSocial");
                    importeTotal = lee_nodo_xml(root, "importeTotal");
                    ambiente = lee_nodo_xml(root, "ambiente");
                    tipoEmision = lee_nodo_xml(root, "tipoEmision");
                    estab = lee_nodo_xml(root, "estab");
                    ptoEmi = lee_nodo_xml(root, "ptoEmi");
                    secuencial = lee_nodo_xml(root, "secuencial");
                    if (String.IsNullOrEmpty(numeroAutorizacion))
                    {
                        numeroAutorizacion = lee_nodo_xml(root, "autorizacion");
                    }

                    if (String.IsNullOrEmpty(fechaAutorizacion))
                    {
                        fechaAutorizacion = lee_nodo_xml(root, "fechaAutorizacion");
                    }

                    DateTime fe = System.DateTime.Now;
                    DateTime fa = System.DateTime.Now;
                    Double valor = 0;

                    if (!string.IsNullOrEmpty(fechaEmision))
                    {
                        fe = DateTime.Parse(fechaEmision);
                    }

                    if (!string.IsNullOrEmpty(fechaAutorizacion))
                    {
                        fa = DateTime.Parse(fechaAutorizacion);
                    }

                    switch (codDoc)
                    {
                        case "04":
                            importeTotal = lee_nodo_xml(root, "valorModificacion");
                            break;
                        case "05":
                            importeTotal = lee_nodo_xml(root, "valorTotal");
                            break;
                        case "07":
                            XmlDocument xmlbase2 = new XmlDocument();
                            xmlbase2.LoadXml(root.OuterXml);
                            XmlElement impuestos = (XmlElement)xmlbase2.GetElementsByTagName("impuestos")[0];
                            if (impuestos != null)
                            {
                                importeTotal = lee_Impuestos(impuestos);
                            }
                            //importeTotal = lee_nodo_xml(root, "valorRetenido");
                            break;
                    }

                    if (!string.IsNullOrEmpty(importeTotal))
                    {
                        importeTotal = importeTotal.Replace(",", ".");
                        valor = Double.Parse(importeTotal);

                        //CultureInfo cs2 = new CultureInfo("es-US");
                        //valor = Double.Parse(valor.ToString(), cs2.NumberFormat);
                    }
                    else
                    {
                        importeTotal = "0";
                    }

                    codDoc = codDoc.PadLeft(2, '0');
                    estab = estab.PadLeft(3, '0');
                    ptoEmi = ptoEmi.PadLeft(3, '0');
                    secuencial = secuencial.PadLeft(9, '0');

                    inserta_doc(identificacionComprador, ruc, claveAcceso, idComprobante, doc_version, fe, razonSocial, importeTotal, ambiente, tipoEmision, codDoc, estab, ptoEmi, secuencial, numeroAutorizacion, fa, root, p_estado, p_mail);
                    
                    enviar_correo(p_mail, codDoc, estab + ptoEmi + secuencial);
                }
                else
                {
                    //log.mensajesLog("US001", "Documento ya se encuentra registrado" + ": Clave Acceso: " + claveAcceso, root.OuterXml, rucCedulaComprador, "Recepción");
                    log.mensajesLog("US001", "Documento ya se encuentra registrado. MailEmisor:" + emails, root.OuterXml, identificacionComprador, estab + ptoEmi + secuencial, claveAcceso, codDoc);

                }

            }
            else
            {
                //log.mensajesLog("US001", "Documento no pertence a la empresa", rucCedulaComprador + "es diferente al de la empresa", root.OuterXml, "Recepción");
                log.mensajesLog("US001", "Documento no pertence a la empresa. MailEmisor:" + emails, root.OuterXml, identificacionComprador, estab + ptoEmi + secuencial, claveAcceso, codDoc);
            }

        }


        public void procesarArchivoXML_2_00(XmlNode xmlNodo, string p_estado, string p_mail)
        {
            XmlNode root = xmlNodo;

            //IT
            rucCedulaComprador = lee_nodo_xml(root, "rucCedulaComprador");
            rucCedulaComprador.Trim();

            if (rucCedulaComprador.Equals(rucEmisor))
            {
                ruc = lee_nodo_xml(root, "ruc");
                idComprobante = "";
                fechaEmision = lee_nodo_xml(root, "fechaEmision");
                razonSocial = lee_nodo_xml(root, "razonSocial");
                importeTotal = lee_nodo_xml(root, "totalConImpuestos");
                ambiente = "2";
                tipoEmision = "No proporcionado";
                codDoc = lee_nodo_xml(root, "codDoc");
                estab = lee_nodo_xml(root, "estab");
                ptoEmi = lee_nodo_xml(root, "ptoEmi");
                secuencial = lee_nodo_xml(root, "secuencial");
                numeroAutorizacion = lee_nodo_xml(root, "numAut");
                fechaAutorizacion = lee_nodo_xml(root, "fechaAutorizacion");

                DateTime fe = System.DateTime.Now;
                DateTime fa = System.DateTime.Now;
                Double valor = 0;

                if (!string.IsNullOrEmpty(fechaEmision))
                {
                    fe = DateTime.Parse(fechaEmision);
                }

                if (!string.IsNullOrEmpty(fechaAutorizacion))
                {
                    fa = DateTime.Parse(fechaAutorizacion);
                }

                if (!string.IsNullOrEmpty(importeTotal))
                {
                    importeTotal = importeTotal.Replace(",", ".");
                    valor = Double.Parse(importeTotal);

                    //CultureInfo cs2 = new CultureInfo("es-US");
                    //valor = Double.Parse(valor.ToString(), cs2.NumberFormat);
                }

                codDoc = codDoc.PadLeft(2, '0');
                estab = estab.PadLeft(3, '0');
                ptoEmi = ptoEmi.PadLeft(3, '0');
                secuencial = secuencial.PadLeft(9, '0');

                claveAcceso = ruc + ambiente + codDoc + estab + ptoEmi + secuencial;

                if (!valida_duplicidad_claveAcceso(claveAcceso))
                {
                    inserta_doc(rucCedulaComprador, ruc, claveAcceso, idComprobante, doc_version, fe, razonSocial, importeTotal, ambiente, tipoEmision, codDoc, estab, ptoEmi, secuencial, numeroAutorizacion, fa, root, p_estado, p_mail);
                }
                else
                {
                    //log.mensajesLog("US001", "Documento ya se encuentra registrado" + ": Clave Acceso: " + claveAcceso, root.OuterXml, rucCedulaComprador, "Recepción");
                    log.mensajesLog("US001", "Documento ya se encuentra registrado. MailEmisor:" + emails, root.OuterXml, rucCedulaComprador, estab + ptoEmi + secuencial, claveAcceso, codDoc);
                }

            }
            else
            {
                //log.mensajesLog("US001", "Documento no pertence a la empresa", rucCedulaComprador + "es diferente al de la empresa", root.OuterXml, "Recepción");
                log.mensajesLog("US001", "Documento no pertence a la empresa. MailEmisor:" + emails, root.OuterXml, rucCedulaComprador, estab + ptoEmi + secuencial, claveAcceso, codDoc);
            }

        }


        private Boolean validarAutorizacion(string p_clave, string p_ambiente)
        {
            XmlDocument xmlAutorizacion = new XmlDocument();
            string edo = "";
            //edo = "";
            String aux = "";
            string soapResponse = "";
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate(object sender1,
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

            if (p_ambiente.Equals("1"))
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
                        var result = autorizacionTrace.autorizacionComprobante(p_clave);

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

                        XmlNodeList existe = autorizacionesNodo.GetElementsByTagName("autorizacion");
                        if (existe.Count != 0)
                        {

                            foreach (XmlElement autorizacionNodo in autorizacionesNodo)
                            {
                                edo = lee_nodo_xml(autorizacionNodo, "estado");
                                //XmlElement autorizacionNodo = (XmlElement)autorizacionesNodo.GetElementsByTagName("autorizacion")[0];

                                msjSRI = autorizacionNodo.GetElementsByTagName("mensajes")[0].OuterXml;

                                if (edo.Equals("AUTORIZADO"))
                                {
                                    XmlElement fRec3 = (XmlElement)autorizacionNodo.GetElementsByTagName("comprobante")[0];
                                    XmlDocument fRec4 = new XmlDocument();
                                    //fRec4 = null;
                                    fRec4.LoadXml(fRec3.InnerText);
                                    if (fRec4 != null)
                                    {
                                        facturaXML = fRec4.DocumentElement;
                                        facturaXMLAut = autorizacionNodo;

                                    }

                                    //facturaXML = (XmlElement)autorizacionNodo.GetElementsByTagName("comprobante")[0];
                                    numeroAutorizacion = autorizacionNodo.GetElementsByTagName("numeroAutorizacion")[0].InnerText;
                                    fechaAutorizacion = autorizacionNodo.GetElementsByTagName("fechaAutorizacion")[0].InnerText;
                                    //aux = fechaAutorizacion.Substring(0, fechaAutorizacion.IndexOf("."));
                                    if (fechaAutorizacion.Contains("."))
                                    {
                                        aux = fechaAutorizacion.Substring(0, fechaAutorizacion.IndexOf("."));
                                        fechaAutorizacion = aux;
                                    }
                                    else
                                    {
                                        aux = fechaAutorizacion;
                                    }

                                    b_respuesta = true;
									break;
                                }
                                else
                                {
                                    if (edo.Equals("NO AUTORIZADO"))
                                    {
                                        b_no_autorizado = true;
                                        string identificador = "";

                                        XmlElement mensajes = (XmlElement)autorizacionNodo.GetElementsByTagName("mensajes")[0];
                                        XmlElement mensaje = (XmlElement)autorizacionNodo.GetElementsByTagName("mensaje")[0];
                                        identificador = mensaje.GetElementsByTagName("identificador")[0].InnerText;
                                        mensaje_sri = mensaje.GetElementsByTagName("mensaje")[0].InnerText; // +Environment.NewLine + ": " + mensaje.GetElementsByTagName("informacionAdicional")[0].InnerText;

                                        if (mensaje.GetElementsByTagName("informacionAdicional").Count > 0)
                                        {
                                            mensaje_sri = mensaje_sri + Environment.NewLine + ": " + mensaje.GetElementsByTagName("informacionAdicional")[0].InnerText;
                                        }

                                        //enviar_notificacion_correo_punto(estab, estab + ptoEmi + secuencial, fechaEmision, mensaje_sri);

                                    }
                                    else
                                    {
                                        if (i_contador == intentos_autorizacion)
                                        {


                                        }

                                    }

                                    //log.mensajesLog("EM016", "No autorizado: " + claveAcceso, soapResponse, "", codigoControl, msj);

                                    b_respuesta = false;

                                }
                            }
                        }
                        else
                        {
                            if (i_contador == intentos_autorizacion)
                            {


                            }
                            b_respuesta = false;
                        }
                    }
                    else
                    {
                        if (i_contador == intentos_autorizacion)
                        {
                            //log.mensajesLog("EM016", claveAcceso, soapResponse, "", codigoControl, " Validación de Comprobantes, WebService validación3 ");

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

                return false;

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


        private string lee_nodo_xml(XmlNode p_root, string p_tag)
        {
            string retorno = "";
            if (p_root.SelectSingleNode("descendant::" + p_tag) != null)
            {
                retorno = p_root.SelectSingleNode("descendant::" + p_tag).InnerText;
            }
            return retorno;
        }

        private string lee_atributo_nodo_xml(XmlNode p_root, string p_tag)
        {
            string retorno = "";
            if (p_root.Attributes != null)
            {
                var nameAttribute = p_root.Attributes[p_tag];
                if (nameAttribute != null)
                    retorno = nameAttribute.Value;
            }

            return retorno;
        }


        public string VerificaAcentos(string strCadena)
        {
            strCadena = strCadena.Replace("á", "a");
            strCadena = strCadena.Replace("é", "e");
            strCadena = strCadena.Replace("í", "i");
            strCadena = strCadena.Replace("ó", "o");
            strCadena = strCadena.Replace("ú", "u");
            strCadena = strCadena.Replace("ñ", "n");
            strCadena = strCadena.Replace("Ï­-­­­", "ni");
            strCadena = strCadena.Replace("Ï­", "ni");

            strCadena = strCadena.Replace("Á", "A");
            strCadena = strCadena.Replace("É", "E");
            strCadena = strCadena.Replace("Í", "I");
            strCadena = strCadena.Replace("Ó", "O");
            strCadena = strCadena.Replace("Ú", "U");
            strCadena = strCadena.Replace("Ñ", "N");

            return strCadena;
        }

        private void inserta_doc(string p_rucReceptor, string p_rucProveedor, string p_claveAcceso, string p_idComp, string p_version, DateTime p_fecha, string p_razonSocialProv, string p_total, string p_ambiente, string p_tipoEmision, string p_codDoc, string p_estab, string p_ptoEmi, string p_secuencial, string p_numeroAutorizacion, DateTime p_fechaAutorizacion, XmlNode p_xmlDoc, string p_estado, string p_mail)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComandoProcedimiento("SP_INS_DOCRECEPCION");
                DB.AsignarParametroProcedimiento("@p_rucReceptor", System.Data.DbType.String, p_rucReceptor);
                DB.AsignarParametroProcedimiento("@p_rucProveedor", System.Data.DbType.String, p_rucProveedor);
                DB.AsignarParametroProcedimiento("@p_claveAcceso", System.Data.DbType.String, p_claveAcceso);
                DB.AsignarParametroProcedimiento("@p_idComp", System.Data.DbType.String, p_idComp);
                DB.AsignarParametroProcedimiento("@p_version", System.Data.DbType.String, p_version);
                DB.AsignarParametroProcedimiento("@p_fecha", System.Data.DbType.DateTime, p_fecha);
                DB.AsignarParametroProcedimiento("@p_razonSocialProv", System.Data.DbType.String, p_razonSocialProv);
                DB.AsignarParametroProcedimiento("@p_total", System.Data.DbType.String, p_total);
                DB.AsignarParametroProcedimiento("@p_ambiente", System.Data.DbType.String, p_ambiente);
                DB.AsignarParametroProcedimiento("@p_tipoEmision", System.Data.DbType.String, p_tipoEmision);
                DB.AsignarParametroProcedimiento("@p_codDoc", System.Data.DbType.String, p_codDoc);
                DB.AsignarParametroProcedimiento("@p_estab", System.Data.DbType.String, p_estab);
                DB.AsignarParametroProcedimiento("@p_ptoEmi", System.Data.DbType.String, p_ptoEmi);
                DB.AsignarParametroProcedimiento("@p_secuencial", System.Data.DbType.String, p_secuencial);
                DB.AsignarParametroProcedimiento("@p_numeroAutorizacion", System.Data.DbType.String, p_numeroAutorizacion);
                DB.AsignarParametroProcedimiento("@p_fechaAutorizacion", System.Data.DbType.DateTime, p_fechaAutorizacion);
                DB.AsignarParametroProcedimiento("@p_xmlDoc", System.Data.DbType.Xml, p_xmlDoc.OuterXml);
                DB.AsignarParametroProcedimiento("@p_mail", System.Data.DbType.String, p_mail);
                DB.AsignarParametroProcedimiento("@p_estado", System.Data.DbType.String, p_estado);
                using (DbDataReader dbDataReader = DB.EjecutarConsulta())
                {
                    using (DbDataReader DR = dbDataReader)
                    {
                    }
                }

                DB.Desconectar();

            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                //log.mensajesLog("ES001", "", e.Message, "", "");
                log.mensajesLog("ES001", "Error al grabar documento. MailEmisor:" + p_mail, p_xmlDoc.OuterXml, p_rucReceptor, p_estab + p_ptoEmi + p_secuencial, p_claveAcceso, p_codDoc);
            }

        }

        //Validar duplicidad
        private Boolean valida_duplicidad_claveAcceso(string p_claveAcceso)
        {
            Boolean rpt = false;
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando("select top 1 claveAcceso from docRecepcion with(nolock) where claveAcceso = @p_claveAcceso");
                DB.AsignarParametroCadena("@p_claveAcceso", p_claveAcceso);
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    if (DR3.Read())
                    {
                        rpt = true;
                    }
                    DB.Desconectar();
                    DR3.Dispose();
                }
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                //throw;
            }
            finally
            {
                DB.Desconectar();
            }
            

            return rpt;
        }

        //Reproceso de docuementos que no se pudieron validar
        public void ProcesoDocsPendientes()
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                seteo_variables();

                DB.Conectar();
                DB.CrearComando("select claveAcceso, [version], ambiente, correo, codDoc, razonSocialProv, estab + ptoEmi + secuencial as num  from [docRecepcion] with(nolock) where estado = 'P'");
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    while (DR3.Read())
                    {
                        string amb = "", ca = "", ver = "", ver2 = "", ma = "", cd = "", prov = "", num = "";
                        ca = DR3[0].ToString();
                        ver = DR3[1].ToString();
                        amb = DR3[2].ToString();
                        ma = DR3[3].ToString();
                        cd = DR3[4].ToString();
                        prov = DR3[5].ToString();
                        num = DR3[6].ToString();

                        if (string.IsNullOrEmpty(amb)) amb = "2";
                        if (validarAutorizacion(ca, amb))
                        {
                            XmlNode nodoFact = facturaXML;
                            ver2 = lee_atributo_nodo_xml(nodoFact, "version");

                            if (ver2.Equals("1.0.0") || ver2.Equals("1.1.0"))
                            {
                                claveAcceso = ca;
                                doc_version = ver2;
                                procesarArchivoXML_1_0_0(nodoFact, "R", ma);
                            }
                        }
                        else
                        {
                            log.mensajesLog("US001", "La " + CodigoDocumento(cd) + " No. " + num + " del proveedor " + prov + " no ha podido ser validada. Se intentará procesar nuevamente, o en su defecto puede rechazarla.", num, prov, num, ca, cd);
                        }

                    }
                    DR3.Dispose();
                }
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("US001", "No se pudo reprocesar documentos pendientes: " + ex.Message, ex.Message, "", "", "Mensaje Usuario");
            }
            finally
            {
                DB.Desconectar();
            }

           /// return rpt;
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

        private void enviar_correo(string pr_emails, string p_codDoc, string p_numDoc)
        {
            string nomDoc = "", asunto="";
            

            //System.Net.Mail.MailAddress sender = new System.Net.Mail.MailAddress(emailEnviar, emailEnviar);

            nomDoc = CodigoDocumento(p_codDoc);
            pr_emails = pr_emails.Trim(',');
            EM = new EnviarMail();
            EM.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);
            if (pr_emails.Length > 10)
            {
                asunto = nomDoc + " ELECTRONICA RECIBIDA No: " + p_numDoc + " DE " + compania;

                //-----------------------HTML VIEW FOR EMAIL--------------------------------
                //Build up Linked resources
                string rutaImg = RutaDOC.Replace("docus\\", "imagenes\\logo_cima.PNG");
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
                htmlBody.Append("<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td><td><br/>Estimado(a) proveedor:<br/><br/>Se recibi&oacute; la " + nomDoc + " electr&oacute;nica N&#250;mero: " + p_numDoc +  @". <br>
							<br/>Este mail no significa la aceptaci&oacute;n del documento, simplemente es una recepci&oacute;n del mismo previo al proceso de aceptaci&oacute;n." + "<br><br> Saludos, <br> <img height=\"50\" width=\"90\" align=\"middle\" src=\"cid:image001\" /><br>Web Site: <a href=\"www.ipac-acero.com\">www.ipac-acero.com</a><br>" +
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

                EM.llenarEmailHTML(emailEnviar, pr_emails.Trim(','), "", "", asunto, htmlView, compania);

                try
                {
                    EM.enviarEmail();
                }
                catch (System.Net.Mail.SmtpException ex)
                {
                    msjT = ex.Message;
                    //DB.Desconectar();
                    log.mensajesLog("US001", "No se pudo enviar correo de recepción: " + ex.Message, ex.Message, "","", "Mensaje Usuario");           
                }
            }

        }

        private string lee_Impuestos(XmlElement impuestos)
        {
            double total_retenciones = 0;
            foreach (XmlElement imp in impuestos)
            {
                total_retenciones += double.Parse(valida_texto_a_numero(lee_nodo_xml(imp, "valorRetenido")), CultureInfo.InvariantCulture);

            }

            return total_retenciones.ToString();
        }


        private string valida_texto_a_numero(string p_valor)
        {
            if (string.IsNullOrEmpty(p_valor))
            {
                p_valor = "0";
            }
            else
            {
                p_valor.Trim();
            }
            return p_valor;
        }
    }
}
