using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using Datos;
using System.Xml;
using System.Xml.Xsl;
using System.Data.OleDb;
using System.Data.Common;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using clibLogger;

namespace Control
{
    public class GenerarXml
    {
        //private BasesDatos DB;
        //private DbDataReader DR;
        private EnviarMail em;
        private NumerosALetras numA;
        private Log log;
        private string msj = "";
        private string msjT;
        private string RutaTXT = "";
        private string RutaBCK = "";
        private string RutaDOC = "";
        private string RutaERR = "";
        private string RutaCER = "";
        private string RutaKEY = "";

        //PMONCAYO 20200813 Etiquetas Exportacion
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

        #region Parametros
        private string servidor = "";
        private int puerto = 587;
        private Boolean ssl = false;
        private string emailCredencial = "";
        private string passCredencial = "";
        private string emailEnviar = "";
        private string emails = "";
        #endregion
        #region General

        string tipoComprobante, idComprobante, version;
        //Informacion Tributaria
        string ambiente, tipoEmision, razonSocial, nombreComercial, ruc, claveAcceso, codDoc, estab, ptoEmi, secuencial, dirMatriz;
        //Informacion del Documento(Factura,guia,notas,retenciones)
        string fechaEmision, dirEstablecimiento, contribuyenteEspecial, obligadoContabilidad, tipoIdentificacionComprador;
        string guiaRemision, razonSocialComprador, identificacionComprador, totalSinImpuestos, totalDescuento, propina, importeTotal, moneda;
        string dirPartida, razonSocialTransportista, tipoIdentificacionTransportista, rucTransportista, rise, fechaIniTransporte, fechaFinTransporte, placa;//Guia de Remision
        string codDocModificado, numDocModificado, fechaEmisionDocSustentoNota, valorModificacion, motivo;//Nota de Credito
        string valorTotal;
        //Nota de Debito
        string tipoIdentificacionSujetoRetenido, razonSocialSujetoRetenido, identificacionSujetoRetenido, periodoFiscal;
        //Destinatario Para Guia de Remision
        string identificacionDestinatario, razonSocialDestinatario, dirDestinatario, motivoTraslado, docAduaneroUnico, codEstabDestino, ruta, codDocSustentoDestinatario, numDocSustentoDestinatario, numAutDocSustento, fechaEmisionDocSustentoDestinatario;
        //Total de Impuestos
        string codigo, codigoPorcentaje, baseImponible, tarifa, valor;
        string fechaPagoDiv, imRentaSoc, ejerFisUtDiv, NumCajBan, PrecCajBan;
        string codigoRetencion, porcentajeRetener, valorRetenido, codDocSustento, numDocSustento, fechaEmisionDocSustento; //Retenciones
        //detalles
        string codigoPrincipal, codigoAuxiliar, descripcion, cantidad, precioUnitario, descuento, precioTotalSinImpuesto;
        string codigoInterno, codigoAdicional;
        //detalles Adicionales
        string detAdicionalNombre, detAdicionalValor;
        //Impuestos Detalles
        string impuestoCodigo, impuestoCodigoPorcentaje, impuestoTarifa, impuestoBaseImponible, impuestoValor;
        //infoAdicional
        string infoAdicionalNombre, infoAdicionalValor;
        //Motivo (Nota de Debito)
        string motivoRazon, motivoValor;
        //ret 2.0
        string tipoSujetoRetenido, parteRel;
        #endregion

        private string codDocReemb = "";
        private string totalComprobantesReembolso = "";
        private string totalBaseImponibleReembolso = "";
        private string totalImpuestoReembolso = "";

        //variables en comun.
        string identificacionRec;
        string tipoIdentificacionRec;
        string razonSocialRec;
								string direccionRec = "";
        //Necesarias
        string firmaSRI;
								string direccionComprador = "";
        //IdTablas

        #region totales
        string subtotal12;
        string subtotal0;
        string subtotalNoSujeto;
        string ICE;
        string IVA12;
        string importeAPagar;

        #endregion

        ArrayList arraylDetalles;
        ArrayList arraylImpuestosDetalles;
        ArrayList arraylDetallesAdicionales;
        ArrayList arraylInfoAdicionales;
        ArrayList arraylTotalImpuestos;
        ArrayList arraylTotalConImpuestos;
        ArrayList arraylMotivos;
        ArrayList arraylTotalImpuestosRetenciones;
        ArrayList arraylDestinatarios;
        ArrayList arraylDoscsSutentos;
        ArrayList arraylReembolsosRetenciones;
        ArrayList arraylImpuestosReembolsosRet;
        private System.Collections.ArrayList arraylPagos;
                                private System.Collections.ArrayList arraylRubros;
								private System.Collections.ArrayList arraylCompensacion;

        public GenerarXml()
        {
            arraylDetalles = new ArrayList();
            arraylImpuestosDetalles = new ArrayList();
            arraylDetallesAdicionales = new ArrayList();
            arraylInfoAdicionales = new ArrayList();
            arraylTotalImpuestos = new ArrayList();
            arraylTotalConImpuestos = new ArrayList();
            arraylMotivos = new ArrayList();
            arraylTotalImpuestosRetenciones = new ArrayList();
            arraylDestinatarios = new ArrayList(); arraylDoscsSutentos = new ArrayList();
            arraylReembolsosRetenciones = new ArrayList();
            arraylImpuestosReembolsosRet = new ArrayList();
            BasesDatos DB = new BasesDatos();
            try
            {
                log = new Log();
                arraylPagos = new ArrayList();
                arraylRubros = new ArrayList();
                arraylCompensacion = new ArrayList();
                msj = "";
                //Parametros Generales
                DB.Conectar();
                DB.CrearComando(@"select servidorSMTP,puertoSMTP,sslSMTP,userSMTP,passSMTP,
                              dirdocs,dirtxt,dirrespaldo,dircertificados,dirllaves,emailEnvio 
                              from ParametrosSistema with(nolock)");
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        servidor = DR[0].ToString();
                        puerto = Convert.ToInt32(DR[1]);
                        ssl = Convert.ToBoolean(DR[2]);
                        emailCredencial = DR[3].ToString();
                        passCredencial = DR[4].ToString();
                        RutaDOC = DR[5].ToString();
                        RutaTXT = DR[6].ToString();
                        RutaBCK = DR[7].ToString();
                        RutaCER = DR[8].ToString();
                        RutaKEY = DR[9].ToString();
                        emailEnviar = DR[10].ToString();
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
            
            //Fin de Parametros Generales.
        }

        public void xmlComprobante(string version, string idComprobante)
        {
            this.version = version;
            this.idComprobante = idComprobante;
        }

        public void InformacionTributaria(string ambiente, string tipoEmision, string razonSocial, string nombreComercial, string ruc,
            string claveAcceso, string codDoc, string estab, string ptoEmi, string secuencial, string dirMatriz)
        {
            this.ambiente = ambiente; this.tipoEmision = tipoEmision; this.razonSocial = razonSocial;
            this.nombreComercial = nombreComercial; this.ruc = ruc; this.claveAcceso = claveAcceso;
            this.codDoc = codDoc; this.estab = estab; this.ptoEmi = ptoEmi; this.secuencial = secuencial;
            this.dirMatriz = dirMatriz;
        }
        public void infromacionDocumento(string fechaEmision, string dirEstablecimiento, string contribuyenteEspecial, string obligadoContabilidad, string tipoIdentificacionComprador,
            string guiaRemision, string razonSocialComprador, string identificacionComprador, string moneda,
            string dirPartida, string razonSocialTransportista, string tipoIdentificacionTransportista, string rucTransportista, string rise, string fechaIniTransporte, string fechaFinTransporte, string placa,//Guia de Remision
            string codDocModificado, string numDocModificado, string fechaEmisionDocSustentoNota, string valorModificacion, string motivo,string direccionComprador)
        {
            this.fechaEmision = fechaEmision; this.dirEstablecimiento = dirEstablecimiento; this.contribuyenteEspecial = contribuyenteEspecial;
            this.obligadoContabilidad = obligadoContabilidad; this.tipoIdentificacionComprador = tipoIdentificacionComprador;
            this.guiaRemision = guiaRemision; this.razonSocialComprador = razonSocialComprador; this.identificacionComprador = identificacionComprador;
            this.moneda = moneda;
            this.dirPartida = dirPartida; this.razonSocialTransportista = razonSocialTransportista; this.tipoIdentificacionTransportista = tipoIdentificacionTransportista;
            this.rucTransportista = rucTransportista; this.rise = rise; this.fechaIniTransporte = fechaIniTransporte; this.fechaFinTransporte = fechaFinTransporte;
            this.placa = placa; this.codDocModificado = codDocModificado; this.numDocModificado = numDocModificado;
												this.fechaEmisionDocSustentoNota = fechaEmisionDocSustentoNota; this.valorModificacion = valorModificacion; this.motivo = motivo; this.direccionComprador = direccionComprador;

            if (!String.IsNullOrEmpty(identificacionComprador))
            {
																identificacionRec = identificacionComprador; tipoIdentificacionRec = tipoIdentificacionComprador; razonSocialRec = razonSocialComprador; direccionRec = direccionComprador;
            }
            if (!String.IsNullOrEmpty(rucTransportista))
            {
                identificacionRec = rucTransportista; tipoIdentificacionRec = tipoIdentificacionTransportista; razonSocialRec = razonSocialTransportista;
            }
            if (!String.IsNullOrEmpty(identificacionSujetoRetenido))
            {
                identificacionRec = identificacionSujetoRetenido; tipoIdentificacionRec = tipoIdentificacionSujetoRetenido; razonSocialRec = razonSocialSujetoRetenido;
            }
        }

        public void cantidades(string subtotal12, string subtotal0, string subtotalNoSujeto, string totalSinImpuestos,
                              string totalDescuento, string ICE, string IVA12, string importeTotal, string propina, string importeAPagar)
        {
            this.totalSinImpuestos = totalSinImpuestos;
            this.totalDescuento = totalDescuento;
            this.propina = propina;
            this.importeTotal = importeTotal;
            this.subtotal12 = subtotal12;
            this.subtotal0 = subtotal0;
            this.subtotalNoSujeto = subtotalNoSujeto;
            this.ICE = ICE;
            this.IVA12 = IVA12;
            this.importeAPagar = importeAPagar;
            this.valorTotal = importeTotal;
            if (codDoc.Equals("04"))
                valorModificacion = importeTotal;
        }
        public void detalles(ArrayList arraylDetalles)
        {
            this.arraylDetalles = arraylDetalles;
        }

        public void impuestos(ArrayList arraylImpuestosDetalles)
        {
            this.arraylImpuestosDetalles = arraylImpuestosDetalles;
        }

        public void totalImpuestos(ArrayList arraylTotalImpuestos)
        {
            this.arraylTotalImpuestos = arraylTotalImpuestos;
        }

        public void totalConImpuestos(ArrayList arraylTotalConImpuestos)
        {
            this.arraylTotalConImpuestos = arraylTotalConImpuestos;
        }

        public void totalImpuestosRetenciones(ArrayList arraylTotalImpuestosRetenciones)
        {
            this.arraylTotalImpuestosRetenciones = arraylTotalImpuestosRetenciones;
        }
        public void comprobanteRetencion(string periodoFiscal, string tipoIdentificacionSujetoRetenido, string razonSocialSujetoRetenido, string identificacionSujetoRetenido, string tipoSujetoRetenido = null, string parteRel = null)
        {
            this.periodoFiscal = periodoFiscal; this.tipoIdentificacionSujetoRetenido = tipoIdentificacionSujetoRetenido;
            this.razonSocialSujetoRetenido = razonSocialSujetoRetenido; this.identificacionSujetoRetenido = identificacionSujetoRetenido;
            this.tipoSujetoRetenido = tipoSujetoRetenido; this.parteRel = parteRel;
        }

        public void Destinatarios(ArrayList arraylDestinatarios) {

            this.arraylDestinatarios = arraylDestinatarios;
        }

								public void DetallePagos(System.Collections.ArrayList arraylPagos)
								{
												this.arraylPagos = arraylPagos;
								}

                                public void DetalleRubros(System.Collections.ArrayList arraylRubros)
                                {
                                    this.arraylRubros = arraylRubros;
                                }
								public void DetalleCompensacion(System.Collections.ArrayList arraylCompensacion)
								{
												this.arraylCompensacion = arraylCompensacion; 
								}
        public void Motivos(ArrayList arraylMotivos) {
            this.arraylMotivos = arraylMotivos;
        }
        
        public void detallesAdicionales(ArrayList arraylDetallesAdicionales)
        {
            this.arraylDetallesAdicionales = arraylDetallesAdicionales;
        }

        public void informacionAdicional(ArrayList arraylInfoAdicionales)
        {
            this.arraylInfoAdicionales = arraylInfoAdicionales;
        }
        public void otrosCampos(string claveAcceso, string secuencial, string guiaRemision)
        {
            this.claveAcceso = claveAcceso;
            this.secuencial = secuencial;
            this.guiaRemision = guiaRemision;
        }
        public void otrosCamposConti(string claveAcceso, string secuencial, string guiaRemision, string tipoEmi)
        {
            this.claveAcceso = claveAcceso;
            this.secuencial = secuencial;
            this.guiaRemision = guiaRemision;
            this.tipoEmision = tipoEmi;
        }
        public XmlDocument xmlFactura()
        {
            XmlDocument xDoc = new XmlDocument();//retorna un xml como cadena (la que se timbra)
            StringWriter sw = new StringWriter();//convierte el xml como una cadena
            XmlTextWriter oXML = new XmlTextWriter(sw);
            try
            {
                
                oXML.Formatting = Formatting.Indented; //para darle formato, se puede comentar para algunas adendas 
                oXML.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                oXML.WriteStartElement("factura");
           //     oXML.WriteAttributeString("xmlns:ds", @"http://www.w3.org/2000/09/xmldsig#");
             //   oXML.WriteAttributeString("xmlns:xsi", @"http://www.w3.org/2001/XMLSchema-instance");
                oXML.WriteAttributeString("id", idComprobante);
                oXML.WriteAttributeString("version", version);

                /*********************************************infoTributaria*/
                oXML.WriteStartElement("infoTributaria");
                oXML.WriteStartElement("ambiente"); oXML.WriteString(ambiente); oXML.WriteEndElement();
                oXML.WriteStartElement("tipoEmision"); oXML.WriteString(tipoEmision); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocial"); oXML.WriteString(razonSocial); oXML.WriteEndElement();
                if (nombreComercial.Length>0) { oXML.WriteStartElement("nombreComercial"); oXML.WriteString(nombreComercial); oXML.WriteEndElement(); }
                oXML.WriteStartElement("ruc"); oXML.WriteString(ruc); oXML.WriteEndElement();
                oXML.WriteStartElement("claveAcceso"); oXML.WriteString(claveAcceso); oXML.WriteEndElement();
                oXML.WriteStartElement("codDoc"); oXML.WriteString(codDoc); oXML.WriteEndElement();
                oXML.WriteStartElement("estab"); oXML.WriteString(estab); oXML.WriteEndElement();
                oXML.WriteStartElement("ptoEmi"); oXML.WriteString(ptoEmi); oXML.WriteEndElement();
                oXML.WriteStartElement("secuencial"); oXML.WriteString(secuencial); oXML.WriteEndElement();
                oXML.WriteStartElement("dirMatriz"); oXML.WriteString(dirMatriz); oXML.WriteEndElement();
                /****Fin****************************************infoTributaria*/
                oXML.WriteEndElement();
                /**************************************************infoFactura*/
                oXML.WriteStartElement("infoFactura");
                oXML.WriteStartElement("fechaEmision"); oXML.WriteString(Convert.ToDateTime(fechaEmision).ToString("dd/MM/yyyy")); oXML.WriteEndElement();
                if (dirEstablecimiento.Length > 0) { oXML.WriteStartElement("dirEstablecimiento"); oXML.WriteString(dirEstablecimiento); oXML.WriteEndElement(); }
                if (!String.IsNullOrEmpty(contribuyenteEspecial))
                {
                    oXML.WriteStartElement("contribuyenteEspecial"); oXML.WriteString(contribuyenteEspecial); oXML.WriteEndElement();
                }
                if (obligadoContabilidad.Length > 0) { oXML.WriteStartElement("obligadoContabilidad"); oXML.WriteString(obligadoContabilidad); oXML.WriteEndElement(); }

                //PMONCAYO 20200814 (Etiquetas Exportacion)
                {
                    if (this.comercioExterior.Length > 0)
                    {
                        oXML.WriteStartElement("comercioExterior");
                        oXML.WriteString(this.comercioExterior);
                        oXML.WriteEndElement();
                    }
                    if (this.incoTermFactura.Length > 0)
                    {
                        oXML.WriteStartElement("incoTermFactura");
                        oXML.WriteString(this.incoTermFactura);
                        oXML.WriteEndElement();
                    }

                    if (this.lugarIncoTerm.Length > 0)
                    {
                        oXML.WriteStartElement("lugarIncoTerm");
                        oXML.WriteString(this.lugarIncoTerm);
                        oXML.WriteEndElement();
                    }
                    if (this.paisOrigen.Length > 0)
                    {
                        oXML.WriteStartElement("paisOrigen");
                        oXML.WriteString(this.paisOrigen);
                        oXML.WriteEndElement();
                    }
                    if (this.puertoEmbarque.Length > 0)
                    {
                        oXML.WriteStartElement("puertoEmbarque");
                        oXML.WriteString(this.puertoEmbarque);
                        oXML.WriteEndElement();
                    }
                    if (this.puertoDestino.Length > 0)
                    {
                        oXML.WriteStartElement("puertoDestino");
                        oXML.WriteString(this.puertoDestino);
                        oXML.WriteEndElement();
                    }
                    if (this.paisDestino.Length > 0)
                    {
                        oXML.WriteStartElement("paisDestino");
                        oXML.WriteString(this.paisDestino);
                        oXML.WriteEndElement();
                    }
                    if (this.paisAdquisicion.Length > 0)
                    {
                        oXML.WriteStartElement("paisAdquisicion");
                        oXML.WriteString(this.paisAdquisicion);
                        oXML.WriteEndElement();
                    }
                }
                //PMONCAYO 20200814 (Etiquetas Exportacion)

                oXML.WriteStartElement("tipoIdentificacionComprador"); oXML.WriteString(tipoIdentificacionComprador); oXML.WriteEndElement();
                if (guiaRemision.Length > 0) { oXML.WriteStartElement("guiaRemision"); oXML.WriteString(guiaRemision); oXML.WriteEndElement(); }
                oXML.WriteStartElement("razonSocialComprador"); oXML.WriteString(razonSocialComprador); oXML.WriteEndElement();
                oXML.WriteStartElement("identificacionComprador"); oXML.WriteString(identificacionComprador); oXML.WriteEndElement();
																if (!String.IsNullOrEmpty(direccionComprador))
																{
																				oXML.WriteStartElement("direccionComprador"); oXML.WriteString(direccionComprador); oXML.WriteEndElement();
																}
																oXML.WriteStartElement("totalSinImpuestos"); oXML.WriteString(totalSinImpuestos); oXML.WriteEndElement();

                //PMONCAYO 20200814 
                if (this.incoTermTotalSinImpuestos.Length > 0)
                {
                    oXML.WriteStartElement("incoTermTotalSinImpuestos");
                    oXML.WriteString(this.incoTermTotalSinImpuestos);
                    oXML.WriteEndElement();
                }

                oXML.WriteStartElement("totalDescuento"); oXML.WriteString(totalDescuento); oXML.WriteEndElement();
                /**********************************************totalConImpuestos*/
                oXML.WriteStartElement("totalConImpuestos");
                /***Ciclo**************************************totalImpuesto*/
                foreach (String[] ti in arraylTotalImpuestos)
                {         //"IT |" + Codigo + "|" + CodigoPorcentaje + "|" + Tarifa + "|" + BaseImponible + "|" + Valor + "|"+Impuestos +"|";
                    oXML.WriteStartElement("totalImpuesto");
                    oXML.WriteStartElement("codigo"); oXML.WriteString(ti[0]); oXML.WriteEndElement();
                    oXML.WriteStartElement("codigoPorcentaje"); oXML.WriteString(ti[1]); oXML.WriteEndElement();
                    oXML.WriteStartElement("baseImponible"); oXML.WriteString(ti[2]); oXML.WriteEndElement();
                    oXML.WriteStartElement("tarifa"); oXML.WriteString(ti[3]); oXML.WriteEndElement();
                    oXML.WriteStartElement("valor"); oXML.WriteString(ti[4]); oXML.WriteEndElement();
                    oXML.WriteEndElement();
                }
                /***Fin*****************************************totalImpuesto*/
                oXML.WriteEndElement();

                /***Fin*****************************************totalConImpuestos*/
																/***Inicio Compesaciones*****************************************Compensaciones*/
																#region Compensaciones
																System.Collections.IEnumerator enumeratorC;
																if (this.arraylCompensacion != null && this.arraylCompensacion.Count > 0)
																{
																				oXML.WriteStartElement("compensaciones");
																				enumeratorC = this.arraylCompensacion.GetEnumerator();
																				try
																				{
																								while (enumeratorC.MoveNext())
																								{
																												string[] arrayC = (string[])enumeratorC.Current;
																												oXML.WriteStartElement("compensacion");
																												if (arrayC[0].Length > 0)
																												{
																																oXML.WriteStartElement("codigo");
																																oXML.WriteString(arrayC[0]);
																																oXML.WriteEndElement();
																												}
																												if (arrayC[1].Length > 0)
																												{
																																oXML.WriteStartElement("tarifa");
																																oXML.WriteString(arrayC[1]);
																																oXML.WriteEndElement();
																												}
																												if (arrayC[2].Length > 0)
																												{
																																oXML.WriteStartElement("valor");
																																oXML.WriteString(valida_texto_a_numero(arrayC[2].ToString()).Replace(",", "."));
																																oXML.WriteEndElement();
																												}
																												oXML.WriteEndElement();
																								}
																				}
																				finally
																				{
																								System.IDisposable disposable = enumeratorC as System.IDisposable;
																								if (disposable != null)
																								{
																												disposable.Dispose();
																								}
																				}
																				oXML.WriteEndElement();
																}
																#endregion
																/***Fin Compesaciones*****************************************Compensaciones*/
                oXML.WriteStartElement("propina"); oXML.WriteString(propina); oXML.WriteEndElement();

                //PMONCAYO (etiquetas de exportacion)
                {
                    if (this.fleteInternacional.Length > 0)
                    {
                        oXML.WriteStartElement("fleteInternacional");
                        oXML.WriteString(this.fleteInternacional);
                        oXML.WriteEndElement();
                    }
                    if (this.seguroInternacional.Length > 0)
                    {
                        oXML.WriteStartElement("seguroInternacional");
                        oXML.WriteString(this.seguroInternacional);
                        oXML.WriteEndElement();
                    }
                    if (this.gastosAduaneros.Length > 0)
                    {
                        oXML.WriteStartElement("gastosAduaneros");
                        oXML.WriteString(this.gastosAduaneros);
                        oXML.WriteEndElement();
                    }
                    if (this.gastosTransporteOtros.Length > 0)
                    {
                        oXML.WriteStartElement("gastosTransporteOtros");
                        oXML.WriteString(this.gastosTransporteOtros);
                        oXML.WriteEndElement();
                    }
                }
                //PMONCAYO (etiquetas de exportacion) (FIN)


                oXML.WriteStartElement("importeTotal"); oXML.WriteString(importeTotal); oXML.WriteEndElement();
                if (moneda.Length > 0) { oXML.WriteStartElement("moneda"); oXML.WriteString(moneda); oXML.WriteEndElement(); }

																//formaspagos
																System.Collections.IEnumerator enumerator;
																if (this.arraylPagos != null && this.arraylPagos.Count > 0)
																{
																				oXML.WriteStartElement("pagos");
																				enumerator = this.arraylPagos.GetEnumerator();
																				try
																				{
																								while (enumerator.MoveNext())
																								{
																												string[] array2 = (string[])enumerator.Current;
																												oXML.WriteStartElement("pago");
																												if (array2[0].Length > 0)
																												{
																																oXML.WriteStartElement("formaPago");
																																oXML.WriteString(array2[0]);
																																oXML.WriteEndElement();
																												}
																												if (array2[1].Length > 0)
																												{
																																oXML.WriteStartElement("total");
																																oXML.WriteString(valida_texto_a_numero(array2[1].ToString()).Replace(",", "."));
																																oXML.WriteEndElement();
																												}
																												if (array2[2].Length > 0)
																												{
																																oXML.WriteStartElement("plazo");
																																oXML.WriteString(array2[2]);
																																oXML.WriteEndElement();
																												}
																												if (array2[3].Length > 0)
																												{
																																oXML.WriteStartElement("unidadTiempo");
																																oXML.WriteString(array2[3]);
																																oXML.WriteEndElement();
																												}
																												oXML.WriteEndElement();
																								}
																				}
																				finally
																				{
																								System.IDisposable disposable = enumerator as System.IDisposable;
																								if (disposable != null)
																								{
																												disposable.Dispose();
																								}
																				}
																				oXML.WriteEndElement();
																}
															
                /****Fin****************************************infoFactura*/

                oXML.WriteEndElement();
                /************************************************Detalles*/
                oXML.WriteStartElement("detalles");
                /**Ciclo***************************************Detalle*/
                foreach (String[] d in arraylDetalles)
                {         //"DE|" + codigoPrincipal + "|" + codigoAuxiliar + "|" + descripcion + "|" + cantidad + "|" + precioUnitario + "|" + descuento + "|" + precioTotalSinImpuesto + "|";
                    oXML.WriteStartElement("detalle");
                    oXML.WriteStartElement("codigoPrincipal"); oXML.WriteString(d[0]); oXML.WriteEndElement();
                    if (d[1].Length > 0)
                    {
                        oXML.WriteStartElement("codigoAuxiliar"); oXML.WriteString(d[1]); oXML.WriteEndElement();
                    }
                    oXML.WriteStartElement("descripcion"); oXML.WriteString(d[2]); oXML.WriteEndElement();
                    oXML.WriteStartElement("cantidad"); oXML.WriteString(d[3]); oXML.WriteEndElement();
                    oXML.WriteStartElement("precioUnitario"); oXML.WriteString(d[4]); oXML.WriteEndElement();
                    oXML.WriteStartElement("descuento"); oXML.WriteString(d[5]); oXML.WriteEndElement();
                    oXML.WriteStartElement("precioTotalSinImpuesto"); oXML.WriteString(d[6]); oXML.WriteEndElement();
                    /*******************************detallesAdicionales*/
                    if (arraylDetallesAdicionales.Count >0)
                    {
                        oXML.WriteStartElement("detallesAdicionales");
                        /**Ciclo********************detAdicional*/
                        foreach (String[] da in arraylDetallesAdicionales)
                        {         //"DA|" + detAdicionalNombre + "|" + detAdicionalValor;
                            if (d[7].Equals(da[3]))
                            {
                                oXML.WriteStartElement("detAdicional");
                                oXML.WriteAttributeString("valor", da[1]);
                                oXML.WriteAttributeString("nombre", da[0]);
                                
                                //oXML.WriteString(da[1]);
                                /**Fin*********************detAdicional*/
                                oXML.WriteEndElement();
                            }
                        }
                        /**Fin**************************detallesAdicionales*/
                        oXML.WriteEndElement();
                    }
                    /*******************************impuestos*/
                    oXML.WriteStartElement("impuestos");
                    /***Ciclo*******************impuesto*/
                    foreach (String[] id in arraylImpuestosDetalles)
                    {         //"IM |" + impuestoCodigo + "|" + impuestoCodigoPorcentaje + "|" + impuestoTarifa + "|" + impuestoBaseImponible + "|" + impuestoValor + "|"+tipoImpuestos +"|";
                        if (d[7].Equals(id[7]))
                        {
                            oXML.WriteStartElement("impuesto");
                            oXML.WriteStartElement("codigo"); oXML.WriteString(id[0]); oXML.WriteEndElement();
                            oXML.WriteStartElement("codigoPorcentaje"); oXML.WriteString(id[1]); oXML.WriteEndElement();
                            oXML.WriteStartElement("tarifa"); oXML.WriteString(id[3]); oXML.WriteEndElement();
                            oXML.WriteStartElement("baseImponible"); oXML.WriteString(id[2]); oXML.WriteEndElement();
                            oXML.WriteStartElement("valor"); oXML.WriteString(id[4]); oXML.WriteEndElement();
                            /***Fin*********************impuesto*/
                            oXML.WriteEndElement();
                        }
                    }
                    /*******************************impuestos*/
                    oXML.WriteEndElement();

                    /***Fin****************************************Detalle*/
                    oXML.WriteEndElement();
                }
                /***Fin******************************************Detalles*/
                oXML.WriteEndElement();

                /*********** inicio *************** otros rubros*/
                #region rubros
                System.Collections.IEnumerator enumeratorRubros;
                if (this.arraylRubros != null && this.arraylRubros.Count > 0)
                {
                    oXML.WriteStartElement("otrosRubrosTerceros");
                    enumeratorRubros = this.arraylRubros.GetEnumerator();
                    try
                    {
                        while (enumeratorRubros.MoveNext())
                        {
                            string[] array2 = (string[])enumeratorRubros.Current;
                            oXML.WriteStartElement("rubro");
                            if (array2[0].Length > 0)
                            {
                                oXML.WriteStartElement("concepto");
                                oXML.WriteString(array2[0]);
                                oXML.WriteEndElement();
                            }
                            if (array2[1].Length > 0)
                            {
                                oXML.WriteStartElement("total");
                                oXML.WriteString(valida_texto_a_numero(array2[1].ToString()).Replace(",", "."));
                                oXML.WriteEndElement();
                            }
                            oXML.WriteEndElement();
                        }
                    }
                    finally
                    {
                        System.IDisposable disposable = enumeratorRubros as System.IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    oXML.WriteEndElement();
                }
                #endregion
                /***********  fin  *************** otros rubros*/

                /***Inicio**********************************infoAdicional*/
																int contador = 0;
																if (this.arraylInfoAdicionales.Count > 0)
																{
                    oXML.WriteStartElement("infoAdicional");
                    /***Ciclo*******************************campoAdicional*/
                    //Información Adicional
                    foreach (String[] ia in arraylInfoAdicionales)
                    {       //IA |" + infoAdicionalNombre + "|" + infoAdicionalValor + "|";
																								if (contador < 15)
                        {
                          oXML.WriteStartElement("campoAdicional");
                          oXML.WriteAttributeString("nombre", ia[0]);
                          oXML.WriteString(ia[1]);
                          /***Fin*********************************campoAdicional*/
                          oXML.WriteEndElement();
																								}
																								else
																												break;
																								contador++;
                    }
                    /***Fin*************************************infoAdicional*/
                    oXML.WriteEndElement();
                }
                oXML.WriteEndElement();
                oXML.Flush();
                xDoc.InnerXml = sw.ToString(); //convertir el xml a cadena
                
                return xDoc; //es la cadena que se va a mandar a timbrar
            }catch (Exception ex){
                msjT = ex.Message;
                log.mensajesLog("XM002", "", msjT, "", ruc + codDoc + estab + ptoEmi + secuencial, "");
                return null;
            }
        }

        public XmlDocument xmlLiquidacionCompras()
        {
            XmlDocument xDoc = new XmlDocument();//retorna un xml como cadena (la que se timbra)
            StringWriter sw = new StringWriter();//convierte el xml como una cadena
            XmlTextWriter oXML = new XmlTextWriter(sw);
            try
            {

                oXML.Formatting = Formatting.Indented; //para darle formato, se puede comentar para algunas adendas 
                oXML.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");
                oXML.WriteStartElement("liquidacionCompra");
                //     oXML.WriteAttributeString("xmlns:ds", @"http://www.w3.org/2000/09/xmldsig#");
                //   oXML.WriteAttributeString("xmlns:xsi", @"http://www.w3.org/2001/XMLSchema-instance");
                oXML.WriteAttributeString("id", idComprobante);
                oXML.WriteAttributeString("version", version);

                /*********************************************infoTributaria*/
                oXML.WriteStartElement("infoTributaria");
                oXML.WriteStartElement("ambiente"); oXML.WriteString(ambiente); oXML.WriteEndElement();
                oXML.WriteStartElement("tipoEmision"); oXML.WriteString(tipoEmision); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocial"); oXML.WriteString(razonSocial); oXML.WriteEndElement();
                if (nombreComercial.Length > 0) { oXML.WriteStartElement("nombreComercial"); oXML.WriteString(nombreComercial); oXML.WriteEndElement(); }
                oXML.WriteStartElement("ruc"); oXML.WriteString(ruc); oXML.WriteEndElement();
                oXML.WriteStartElement("claveAcceso"); oXML.WriteString(claveAcceso); oXML.WriteEndElement();
                oXML.WriteStartElement("codDoc"); oXML.WriteString(codDoc); oXML.WriteEndElement();
                oXML.WriteStartElement("estab"); oXML.WriteString(estab); oXML.WriteEndElement();
                oXML.WriteStartElement("ptoEmi"); oXML.WriteString(ptoEmi); oXML.WriteEndElement();
                oXML.WriteStartElement("secuencial"); oXML.WriteString(secuencial); oXML.WriteEndElement();
                oXML.WriteStartElement("dirMatriz"); oXML.WriteString(dirMatriz); oXML.WriteEndElement();
                /****Fin****************************************infoTributaria*/
                oXML.WriteEndElement();
                /**************************************************infoFactura*/
                oXML.WriteStartElement("infoLiquidacionCompra");
                oXML.WriteStartElement("fechaEmision"); oXML.WriteString(Convert.ToDateTime(fechaEmision).ToString("dd/MM/yyyy")); oXML.WriteEndElement();
                if (dirEstablecimiento.Length > 0) { oXML.WriteStartElement("dirEstablecimiento"); oXML.WriteString(dirEstablecimiento); oXML.WriteEndElement(); }
                if (!String.IsNullOrEmpty(contribuyenteEspecial))
                {
                    oXML.WriteStartElement("contribuyenteEspecial"); oXML.WriteString(contribuyenteEspecial); oXML.WriteEndElement();
                }
                if (obligadoContabilidad.Length > 0) { oXML.WriteStartElement("obligadoContabilidad"); oXML.WriteString(obligadoContabilidad); oXML.WriteEndElement(); }
                oXML.WriteStartElement("tipoIdentificacionProveedor"); oXML.WriteString(tipoIdentificacionComprador); oXML.WriteEndElement();
                if (guiaRemision.Length > 0) { oXML.WriteStartElement("guiaRemision"); oXML.WriteString(guiaRemision); oXML.WriteEndElement(); }
                oXML.WriteStartElement("razonSocialProveedor"); oXML.WriteString(razonSocialComprador); oXML.WriteEndElement();
                oXML.WriteStartElement("identificacionProveedor"); oXML.WriteString(identificacionComprador); oXML.WriteEndElement();
                if (!String.IsNullOrEmpty(direccionComprador))
                {
                    oXML.WriteStartElement("direccionProveedor"); oXML.WriteString(direccionComprador); oXML.WriteEndElement();
                }
                oXML.WriteStartElement("totalSinImpuestos"); oXML.WriteString(totalSinImpuestos); oXML.WriteEndElement();
                oXML.WriteStartElement("totalDescuento"); oXML.WriteString(totalDescuento); oXML.WriteEndElement();
                /**********************************************totalConImpuestos*/
                oXML.WriteStartElement("totalConImpuestos");
                /***Ciclo**************************************totalImpuesto*/
                foreach (String[] ti in arraylTotalImpuestos)
                {         //"IT |" + Codigo + "|" + CodigoPorcentaje + "|" + Tarifa + "|" + BaseImponible + "|" + Valor + "|"+Impuestos +"|";
                    oXML.WriteStartElement("totalImpuesto");
                    oXML.WriteStartElement("codigo"); oXML.WriteString(ti[0]); oXML.WriteEndElement();
                    oXML.WriteStartElement("codigoPorcentaje"); oXML.WriteString(ti[1]); oXML.WriteEndElement();
                    oXML.WriteStartElement("baseImponible"); oXML.WriteString(ti[2]); oXML.WriteEndElement();
                    oXML.WriteStartElement("tarifa"); oXML.WriteString(ti[3]); oXML.WriteEndElement();
                    oXML.WriteStartElement("valor"); oXML.WriteString(ti[4]); oXML.WriteEndElement();
                    oXML.WriteEndElement();
                }
                /***Fin*****************************************totalImpuesto*/
                oXML.WriteEndElement();

                /***Fin*****************************************totalConImpuestos*/
                /***Inicio Compesaciones*****************************************Compensaciones*/
                #region Compensaciones
                System.Collections.IEnumerator enumeratorC;
                if (this.arraylCompensacion != null && this.arraylCompensacion.Count > 0)
                {
                    oXML.WriteStartElement("compensaciones");
                    enumeratorC = this.arraylCompensacion.GetEnumerator();
                    try
                    {
                        while (enumeratorC.MoveNext())
                        {
                            string[] arrayC = (string[])enumeratorC.Current;
                            oXML.WriteStartElement("compensacion");
                            if (arrayC[0].Length > 0)
                            {
                                oXML.WriteStartElement("codigo");
                                oXML.WriteString(arrayC[0]);
                                oXML.WriteEndElement();
                            }
                            if (arrayC[1].Length > 0)
                            {
                                oXML.WriteStartElement("tarifa");
                                oXML.WriteString(arrayC[1]);
                                oXML.WriteEndElement();
                            }
                            if (arrayC[2].Length > 0)
                            {
                                oXML.WriteStartElement("valor");
                                oXML.WriteString(valida_texto_a_numero(arrayC[2].ToString()).Replace(",", "."));
                                oXML.WriteEndElement();
                            }
                            oXML.WriteEndElement();
                        }
                    }
                    finally
                    {
                        System.IDisposable disposable = enumeratorC as System.IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    oXML.WriteEndElement();
                }
                #endregion
                /***Fin Compesaciones*****************************************Compensaciones*/
              
                oXML.WriteStartElement("importeTotal"); oXML.WriteString(importeTotal); oXML.WriteEndElement();
                if (moneda.Length > 0) { oXML.WriteStartElement("moneda"); oXML.WriteString(moneda); oXML.WriteEndElement(); }

                //formaspagos
                System.Collections.IEnumerator enumerator;
                if (this.arraylPagos != null && this.arraylPagos.Count > 0)
                {
                    oXML.WriteStartElement("pagos");
                    enumerator = this.arraylPagos.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            string[] array2 = (string[])enumerator.Current;
                            oXML.WriteStartElement("pago");
                            if (array2[0].Length > 0)
                            {
                                oXML.WriteStartElement("formaPago");
                                oXML.WriteString(array2[0]);
                                oXML.WriteEndElement();
                            }
                            if (array2[1].Length > 0)
                            {
                                oXML.WriteStartElement("total");
                                oXML.WriteString(valida_texto_a_numero(array2[1].ToString()).Replace(",", "."));
                                oXML.WriteEndElement();
                            }
                            if (array2[2].Length > 0)
                            {
                                oXML.WriteStartElement("plazo");
                                oXML.WriteString(array2[2]);
                                oXML.WriteEndElement();
                            }
                            if (array2[3].Length > 0)
                            {
                                oXML.WriteStartElement("unidadTiempo");
                                oXML.WriteString(array2[3]);
                                oXML.WriteEndElement();
                            }
                            oXML.WriteEndElement();
                        }
                    }
                    finally
                    {
                        System.IDisposable disposable = enumerator as System.IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    oXML.WriteEndElement();
                }

                /****Fin****************************************infoFactura*/

                oXML.WriteEndElement();
                /************************************************Detalles*/
                oXML.WriteStartElement("detalles");
                /**Ciclo***************************************Detalle*/
                foreach (String[] d in arraylDetalles)
                {         //"DE|" + codigoPrincipal + "|" + codigoAuxiliar + "|" + descripcion + "|" + cantidad + "|" + precioUnitario + "|" + descuento + "|" + precioTotalSinImpuesto + "|";
                    oXML.WriteStartElement("detalle");
                    oXML.WriteStartElement("codigoPrincipal"); oXML.WriteString(d[0]); oXML.WriteEndElement();
                    if (d[1].Length > 0)
                    {
                        oXML.WriteStartElement("codigoAuxiliar"); oXML.WriteString(d[1]); oXML.WriteEndElement();
                    }
                    oXML.WriteStartElement("descripcion"); oXML.WriteString(d[2]); oXML.WriteEndElement();
                    oXML.WriteStartElement("cantidad"); oXML.WriteString(d[3]); oXML.WriteEndElement();
                    oXML.WriteStartElement("precioUnitario"); oXML.WriteString(d[4]); oXML.WriteEndElement();
                    oXML.WriteStartElement("descuento"); oXML.WriteString(d[5]); oXML.WriteEndElement();
                    oXML.WriteStartElement("precioTotalSinImpuesto"); oXML.WriteString(d[6]); oXML.WriteEndElement();
                    /*******************************detallesAdicionales*/
                    if (arraylDetallesAdicionales.Count > 0)
                    {
                        oXML.WriteStartElement("detallesAdicionales");
                        /**Ciclo********************detAdicional*/
                        foreach (String[] da in arraylDetallesAdicionales)
                        {         //"DA|" + detAdicionalNombre + "|" + detAdicionalValor;
                            if (d[7].Equals(da[3]))
                            {
                                oXML.WriteStartElement("detAdicional");
                                oXML.WriteAttributeString("valor", da[1]);
                                oXML.WriteAttributeString("nombre", da[0]);

                                //oXML.WriteString(da[1]);
                                /**Fin*********************detAdicional*/
                                oXML.WriteEndElement();
                            }
                        }
                        /**Fin**************************detallesAdicionales*/
                        oXML.WriteEndElement();
                    }
                    /*******************************impuestos*/
                    oXML.WriteStartElement("impuestos");
                    /***Ciclo*******************impuesto*/
                    foreach (String[] id in arraylImpuestosDetalles)
                    {         //"IM |" + impuestoCodigo + "|" + impuestoCodigoPorcentaje + "|" + impuestoTarifa + "|" + impuestoBaseImponible + "|" + impuestoValor + "|"+tipoImpuestos +"|";
                        if (d[7].Equals(id[7]))
                        {
                            oXML.WriteStartElement("impuesto");
                            oXML.WriteStartElement("codigo"); oXML.WriteString(id[0]); oXML.WriteEndElement();
                            oXML.WriteStartElement("codigoPorcentaje"); oXML.WriteString(id[1]); oXML.WriteEndElement();
                            oXML.WriteStartElement("tarifa"); oXML.WriteString(id[3]); oXML.WriteEndElement();
                            oXML.WriteStartElement("baseImponible"); oXML.WriteString(id[2]); oXML.WriteEndElement();
                            oXML.WriteStartElement("valor"); oXML.WriteString(id[4]); oXML.WriteEndElement();
                            /***Fin*********************impuesto*/
                            oXML.WriteEndElement();
                        }
                    }
                    /*******************************impuestos*/
                    oXML.WriteEndElement();

                    /***Fin****************************************Detalle*/
                    oXML.WriteEndElement();
                }
                /***Fin******************************************Detalles*/
                oXML.WriteEndElement();

                /*********** inicio *************** otros rubros*/
                #region rubros
                System.Collections.IEnumerator enumeratorRubros;
                if (this.arraylRubros != null && this.arraylRubros.Count > 0)
                {
                    oXML.WriteStartElement("otrosRubrosTerceros");
                    enumeratorRubros = this.arraylRubros.GetEnumerator();
                    try
                    {
                        while (enumeratorRubros.MoveNext())
                        {
                            string[] array2 = (string[])enumeratorRubros.Current;
                            oXML.WriteStartElement("rubro");
                            if (array2[0].Length > 0)
                            {
                                oXML.WriteStartElement("concepto");
                                oXML.WriteString(array2[0]);
                                oXML.WriteEndElement();
                            }
                            if (array2[1].Length > 0)
                            {
                                oXML.WriteStartElement("total");
                                oXML.WriteString(valida_texto_a_numero(array2[1].ToString()).Replace(",", "."));
                                oXML.WriteEndElement();
                            }
                            oXML.WriteEndElement();
                        }
                    }
                    finally
                    {
                        System.IDisposable disposable = enumeratorRubros as System.IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    oXML.WriteEndElement();
                }
                #endregion
                /***********  fin  *************** otros rubros*/

                /***Inicio**********************************infoAdicional*/
                int contador = 0;
                if (this.arraylInfoAdicionales.Count > 0)
                {
                    oXML.WriteStartElement("infoAdicional");
                    /***Ciclo*******************************campoAdicional*/
                    //Información Adicional
                    foreach (String[] ia in arraylInfoAdicionales)
                    {       //IA |" + infoAdicionalNombre + "|" + infoAdicionalValor + "|";
                        if (contador < 15)
                        {
                            oXML.WriteStartElement("campoAdicional");
                            oXML.WriteAttributeString("nombre", ia[0]);
                            oXML.WriteString(ia[1]);
                            /***Fin*********************************campoAdicional*/
                            oXML.WriteEndElement();
                        }
                        else
                            break;
                        contador++;
                    }
                    /***Fin*************************************infoAdicional*/
                    oXML.WriteEndElement();
                }
                oXML.WriteEndElement();
                oXML.Flush();
                xDoc.InnerXml = sw.ToString(); //convertir el xml a cadena

                return xDoc; //es la cadena que se va a mandar a timbrar
            }
            catch (Exception ex)
            {
                msjT = ex.Message;
                log.mensajesLog("XM002", "", msjT, "", ruc + codDoc + estab + ptoEmi + secuencial, "");
                return null;
            }
        }

        public XmlDocument xmlNotaCredito()
        {
            XmlDocument xDoc = new XmlDocument();//retorna un xml como cadena (la que se timbra)
            StringWriter sw = new StringWriter();//convierte el xml como una cadena
            XmlTextWriter oXML = new XmlTextWriter(sw);
            try{
            oXML.Formatting = Formatting.Indented; //para darle formato, se puede comentar para algunas adendas 
            oXML.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");

                oXML.WriteStartElement("notaCredito");
                //oXML.WriteAttributeString("xmlns:ds", @"http://www.w3.org/2000/09/xmldsig#");
                //            oXML.WriteAttributeString("xsi:schemaLocation", @"D:\server\Ecuador\Documentacion\Documentacion Iicial\Esquemas xsd\notaCredito1.xsd");
                //oXML.WriteAttributeString("xmlns:xsi", @"http://www.w3.org/2001/XMLSchema-instance");

                oXML.WriteAttributeString("id", idComprobante);
                oXML.WriteAttributeString("version", version);

                /*********************************************infoTributaria*/
                oXML.WriteStartElement("infoTributaria");
                oXML.WriteStartElement("ambiente"); oXML.WriteString(ambiente); oXML.WriteEndElement();
                oXML.WriteStartElement("tipoEmision"); oXML.WriteString(tipoEmision); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocial"); oXML.WriteString(razonSocial); oXML.WriteEndElement();
                if (nombreComercial.Length > 0) { oXML.WriteStartElement("nombreComercial"); oXML.WriteString(nombreComercial); oXML.WriteEndElement(); }
                oXML.WriteStartElement("ruc"); oXML.WriteString(ruc); oXML.WriteEndElement();
                oXML.WriteStartElement("claveAcceso"); oXML.WriteString(claveAcceso); oXML.WriteEndElement();
                oXML.WriteStartElement("codDoc"); oXML.WriteString(codDoc); oXML.WriteEndElement();
                oXML.WriteStartElement("estab"); oXML.WriteString(estab); oXML.WriteEndElement();
                oXML.WriteStartElement("ptoEmi"); oXML.WriteString(ptoEmi); oXML.WriteEndElement();
                oXML.WriteStartElement("secuencial"); oXML.WriteString(secuencial); oXML.WriteEndElement();
                oXML.WriteStartElement("dirMatriz"); oXML.WriteString(dirMatriz); oXML.WriteEndElement();
                /****Fin****************************************infoTributaria*/
                oXML.WriteEndElement();
                /**************************************************infoNotaCredito*/
                oXML.WriteStartElement("infoNotaCredito");
                oXML.WriteStartElement("fechaEmision"); oXML.WriteString(Convert.ToDateTime(fechaEmision).ToString("dd/MM/yyyy")); oXML.WriteEndElement();
                if (dirEstablecimiento.Length > 0) { oXML.WriteStartElement("dirEstablecimiento"); oXML.WriteString(dirEstablecimiento); oXML.WriteEndElement(); }
                oXML.WriteStartElement("tipoIdentificacionComprador"); oXML.WriteString(tipoIdentificacionComprador); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocialComprador"); oXML.WriteString(razonSocialComprador); oXML.WriteEndElement();
                oXML.WriteStartElement("identificacionComprador"); oXML.WriteString(identificacionComprador); oXML.WriteEndElement();
                if (!String.IsNullOrEmpty(contribuyenteEspecial))
                {
                    oXML.WriteStartElement("contribuyenteEspecial"); oXML.WriteString(contribuyenteEspecial); oXML.WriteEndElement();
                }
                if (obligadoContabilidad.Length > 0) { oXML.WriteStartElement("obligadoContabilidad"); oXML.WriteString(obligadoContabilidad); oXML.WriteEndElement(); }
                if (rise.Length > 0)
                {
                    oXML.WriteStartElement("rise"); oXML.WriteString(rise); oXML.WriteEndElement();
                }
                oXML.WriteStartElement("codDocModificado"); oXML.WriteString(codDocModificado); oXML.WriteEndElement();
                oXML.WriteStartElement("numDocModificado"); oXML.WriteString(numDocModificado); oXML.WriteEndElement();
                oXML.WriteStartElement("fechaEmisionDocSustento"); oXML.WriteString(Convert.ToDateTime(fechaEmisionDocSustentoNota).ToString("dd/MM/yyyy")); oXML.WriteEndElement();
                oXML.WriteStartElement("totalSinImpuestos"); oXML.WriteString(totalSinImpuestos); oXML.WriteEndElement();
																/***Inicio Compesaciones*****************************************Compensaciones*/
																#region Compensaciones
																System.Collections.IEnumerator enumeratorC;
																if (this.arraylCompensacion != null && this.arraylCompensacion.Count > 0)
																{
																				oXML.WriteStartElement("compensaciones");
																				enumeratorC = this.arraylCompensacion.GetEnumerator();
																				try
																				{
																								while (enumeratorC.MoveNext())
																								{
																												string[] arrayC = (string[])enumeratorC.Current;
																												oXML.WriteStartElement("compensacion");
																												if (arrayC[0].Length > 0)
																												{
																																oXML.WriteStartElement("codigo");
																																oXML.WriteString(arrayC[0]);
																																oXML.WriteEndElement();
																												}
																												if (arrayC[1].Length > 0)
																												{
																																oXML.WriteStartElement("tarifa");
																																oXML.WriteString(arrayC[1]);
																																oXML.WriteEndElement();
																												}
																												if (arrayC[2].Length > 0)
																												{
																																oXML.WriteStartElement("valor");
																																oXML.WriteString(valida_texto_a_numero(arrayC[2].ToString()).Replace(",", "."));
																																oXML.WriteEndElement();
																												}
																												oXML.WriteEndElement();
																								}
																				}
																				finally
																				{
																								System.IDisposable disposable = enumeratorC as System.IDisposable;
																								if (disposable != null)
																								{
																												disposable.Dispose();
																								}
																				}
																				oXML.WriteEndElement();
																}
																#endregion
																/***Fin Compesaciones*****************************************Compensaciones*/
																oXML.WriteStartElement("valorModificacion"); oXML.WriteString(valorModificacion); oXML.WriteEndElement();
                if (moneda.Length > 0) { oXML.WriteStartElement("moneda"); oXML.WriteString(moneda); oXML.WriteEndElement(); }
                /**********************************************totalConImpuestos*/
                oXML.WriteStartElement("totalConImpuestos");
                /***Ciclo**************************************totalImpuesto*/
                foreach (String[] ti in arraylTotalImpuestos)
                {         //"IT |" + Codigo + "|" + CodigoPorcentaje + "|" + Tarifa + "|" + BaseImponible + "|" + Valor + "|"+Impuestos +"|";
                    oXML.WriteStartElement("totalImpuesto");
                    oXML.WriteStartElement("codigo"); oXML.WriteString(ti[0]); oXML.WriteEndElement();
                    oXML.WriteStartElement("codigoPorcentaje"); oXML.WriteString(ti[1]); oXML.WriteEndElement();
                    oXML.WriteStartElement("baseImponible"); oXML.WriteString(ti[2]); oXML.WriteEndElement();
                    oXML.WriteStartElement("valor"); oXML.WriteString(ti[4]); oXML.WriteEndElement();
                    /***Fin*****************************************totalImpuesto*/
                    oXML.WriteEndElement();
                }
                /***Fin*****************************************totalConImpuestos*/
                oXML.WriteEndElement();
                oXML.WriteStartElement("motivo"); oXML.WriteString(motivo); oXML.WriteEndElement();
                /****Fin****************************************infoNotaCredito*/
                oXML.WriteEndElement();
                /************************************************Detalles*/
                oXML.WriteStartElement("detalles");
                /**Ciclo***************************************Detalle*/
                foreach (String[] d in arraylDetalles)
                {

                    oXML.WriteStartElement("detalle");
                    oXML.WriteStartElement("codigoInterno"); oXML.WriteString(d[0]); oXML.WriteEndElement();
                    if (d[1].Length > 0)
                    {
                        oXML.WriteStartElement("codigoAdicional"); oXML.WriteString(d[1]); oXML.WriteEndElement();
                    }
                    oXML.WriteStartElement("descripcion"); oXML.WriteString(d[2]); oXML.WriteEndElement();
                    oXML.WriteStartElement("cantidad"); oXML.WriteString(d[3]); oXML.WriteEndElement();
                    oXML.WriteStartElement("precioUnitario"); oXML.WriteString(d[4]); oXML.WriteEndElement();
                    if (d[5].Length > 0) { oXML.WriteStartElement("descuento"); oXML.WriteString(d[5]); oXML.WriteEndElement(); }
                    oXML.WriteStartElement("precioTotalSinImpuesto"); oXML.WriteString(d[6]); oXML.WriteEndElement();
                    /*******************************detallesAdicionales*/
                    if (arraylDetallesAdicionales.Count > 0)
                    {
                        oXML.WriteStartElement("detallesAdicionales");
                        /**Ciclo********************detAdicional*/
                        foreach (String[] da in arraylDetallesAdicionales)
                        {         //"DA|" + detAdicionalNombre + "|" + detAdicionalValor;
                            if (d[7].Equals(da[3]))
                            {
                                oXML.WriteStartElement("detAdicional");
                                oXML.WriteAttributeString("valor", da[1]);
                                oXML.WriteAttributeString("nombre", da[0]);
                                
                                /**Fin*********************detAdicional*/
                                oXML.WriteEndElement();
                            }
                        }
                        /**Fin**************************detallesAdicionales*/
                        oXML.WriteEndElement();
                    }
                    /*******************************impuestos*/
                    oXML.WriteStartElement("impuestos");
                    /***Ciclo*******************impuesto*/
                    foreach (String[] id in arraylImpuestosDetalles)
                    {         //"IM |" + impuestoCodigo + "|" + impuestoCodigoPorcentaje + "|" + impuestoTarifa + "|" + impuestoBaseImponible + "|" + impuestoValor + "|"+tipoImpuestos +"|";
                        if (d[7].Equals(id[7]))
                        {
                            oXML.WriteStartElement("impuesto");
                            oXML.WriteStartElement("codigo"); oXML.WriteString(id[0]); oXML.WriteEndElement();
                            oXML.WriteStartElement("codigoPorcentaje"); oXML.WriteString(id[1]); oXML.WriteEndElement();
                            if (id[3].Length > 0) { oXML.WriteStartElement("tarifa"); oXML.WriteString(id[3]); oXML.WriteEndElement(); }
                            oXML.WriteStartElement("baseImponible"); oXML.WriteString(id[2]); oXML.WriteEndElement();
                            oXML.WriteStartElement("valor"); oXML.WriteString(id[4]); oXML.WriteEndElement();
                            /***Fin*********************impuesto*/
                            oXML.WriteEndElement();
                        }
                    }
                    /*******************************impuestos*/
                    oXML.WriteEndElement();
                    /***Fin****************************************Detalle*/
                    oXML.WriteEndElement();
                }
                /***Fin******************************************Detalles*/
                oXML.WriteEndElement();
                /***Inicio**********************************infoAdicional*/
                if (arraylInfoAdicionales.Count > 0) // && arraylInfoAdicionales.Count < 15
                {
                    int cuentaIA = 0;
                    oXML.WriteStartElement("infoAdicional");
                    /***Ciclo*******************************campoAdicional*/
                    foreach (String[] ia in arraylInfoAdicionales)
                    {       //IA |" + infoAdicionalNombre + "|" + infoAdicionalValor + "|";
                        cuentaIA++;
                        oXML.WriteStartElement("campoAdicional");
                        oXML.WriteAttributeString("nombre", ia[0]);
                        oXML.WriteString(ia[1]);
                        /***Fin*********************************campoAdicional*/
                        oXML.WriteEndElement();
                        if (cuentaIA == 15)
                            break;
                    }
                    /***Fin*************************************infoAdicional*/
                    oXML.WriteEndElement();
                }
                oXML.WriteEndElement();
                oXML.Flush();
                xDoc.InnerXml = sw.ToString(); //convertir el xml a cadena
                return xDoc; //es la cadena que se va a mandar a timbrar
            }
            catch (Exception ex)
            {
                msjT = ex.Message;
                log.mensajesLog("XM003", "", msjT, "", ruc + codDoc + estab + ptoEmi + secuencial, "");
                return null;
            }
        }

        public XmlDocument xmlComprobanteRetencion()
        {
            XmlDocument xDoc = new XmlDocument();//retorna un xml como cadena (la que se timbra)
            StringWriter sw = new StringWriter();//convierte el xml como una cadena
            XmlTextWriter oXML = new XmlTextWriter(sw);
            try
            {
                oXML.Formatting = Formatting.Indented; //para darle formato, se puede comentar para algunas adendas 
                oXML.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");

                oXML.WriteStartElement("comprobanteRetencion");
                //  oXML.WriteAttributeString("xmlns:ds", @"http://www.w3.org/2000/09/xmldsig#");
                //                oXML.WriteAttributeString("xsi:schemaLocation", @"C:\Documents and Settings\mfsalazar\Escritorio\facturación electronica\formato xsd xml 07-03\comprobanteRetencion1.xsd");
                // oXML.WriteAttributeString("xmlns:xsi", @"http://www.w3.org/2001/XMLSchema-instance");

                oXML.WriteAttributeString("id", idComprobante);
                oXML.WriteAttributeString("version", version);

                /*********************************************infoTributaria*/
                oXML.WriteStartElement("infoTributaria");
                oXML.WriteStartElement("ambiente"); oXML.WriteString(ambiente); oXML.WriteEndElement();
                oXML.WriteStartElement("tipoEmision"); oXML.WriteString(tipoEmision); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocial"); oXML.WriteString(razonSocial); oXML.WriteEndElement();
                if (nombreComercial.Length > 0) { oXML.WriteStartElement("nombreComercial"); oXML.WriteString(nombreComercial); oXML.WriteEndElement(); }
                oXML.WriteStartElement("ruc"); oXML.WriteString(ruc); oXML.WriteEndElement();
                oXML.WriteStartElement("claveAcceso"); oXML.WriteString(claveAcceso); oXML.WriteEndElement();
                oXML.WriteStartElement("codDoc"); oXML.WriteString(codDoc); oXML.WriteEndElement();
                oXML.WriteStartElement("estab"); oXML.WriteString(estab); oXML.WriteEndElement();
                oXML.WriteStartElement("ptoEmi"); oXML.WriteString(ptoEmi); oXML.WriteEndElement();
                oXML.WriteStartElement("secuencial"); oXML.WriteString(secuencial); oXML.WriteEndElement();
                oXML.WriteStartElement("dirMatriz"); oXML.WriteString(dirMatriz); oXML.WriteEndElement();
                /****Fin****************************************infoTributaria*/
                oXML.WriteEndElement();
                /**************************************************infoCompRetencion*/
                oXML.WriteStartElement("infoCompRetencion");
                oXML.WriteStartElement("fechaEmision"); oXML.WriteString(Convert.ToDateTime(fechaEmision).ToString("dd/MM/yyyy")); oXML.WriteEndElement();
                if (dirEstablecimiento.Length > 0) { oXML.WriteStartElement("dirEstablecimiento"); oXML.WriteString(dirEstablecimiento); oXML.WriteEndElement(); }
                if (!String.IsNullOrEmpty(contribuyenteEspecial))
                {
                    oXML.WriteStartElement("contribuyenteEspecial"); oXML.WriteString(contribuyenteEspecial); oXML.WriteEndElement();
                }
                if (obligadoContabilidad.Length > 0) { oXML.WriteStartElement("obligadoContabilidad"); oXML.WriteString(obligadoContabilidad); oXML.WriteEndElement(); }
                oXML.WriteStartElement("tipoIdentificacionSujetoRetenido"); oXML.WriteString(tipoIdentificacionSujetoRetenido); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocialSujetoRetenido"); oXML.WriteString(razonSocialSujetoRetenido); oXML.WriteEndElement();
                oXML.WriteStartElement("identificacionSujetoRetenido"); oXML.WriteString(identificacionSujetoRetenido); oXML.WriteEndElement();
                oXML.WriteStartElement("periodoFiscal"); oXML.WriteString(periodoFiscal); oXML.WriteEndElement();
                /****Fin****************************************infoCompRetencion*/
                oXML.WriteEndElement();
                /**********************************************impuestos*/
                oXML.WriteStartElement("impuestos");
                /***Ciclo*************************************impuesto*/
                foreach (String[] tir in arraylTotalImpuestosRetenciones)
                {
                    oXML.WriteStartElement("impuesto");
                    oXML.WriteStartElement("codigo"); oXML.WriteString(tir[0]); oXML.WriteEndElement();
                    oXML.WriteStartElement("codigoRetencion"); oXML.WriteString(tir[1]); oXML.WriteEndElement();
                    oXML.WriteStartElement("baseImponible"); oXML.WriteString(tir[2]); oXML.WriteEndElement();
                    oXML.WriteStartElement("porcentajeRetener"); oXML.WriteString(tir[3]); oXML.WriteEndElement();
                    oXML.WriteStartElement("valorRetenido"); oXML.WriteString(tir[4]); oXML.WriteEndElement();
                    oXML.WriteStartElement("codDocSustento"); oXML.WriteString(tir[5]); oXML.WriteEndElement();
                    if (tir[6].Length > 0) { oXML.WriteStartElement("numDocSustento"); oXML.WriteString(tir[6].Replace("-", "")); oXML.WriteEndElement(); }
                    if (tir[7].Length > 0) { oXML.WriteStartElement("fechaEmisionDocSustento"); oXML.WriteString(tir[7]); oXML.WriteEndElement(); }
                    /***Fin****************************************impuesto*/
                    oXML.WriteEndElement();
                }
                /***Fin*****************************************impuestos*/
                oXML.WriteEndElement();
                if (arraylInfoAdicionales.Count > 0)
                {
                    /***Inicio**********************************infoAdicional*/
                    oXML.WriteStartElement("infoAdicional");
                    /***Ciclo*******************************campoAdicional*/
                    foreach (String[] ia in arraylInfoAdicionales)
                    {       //IA |" + infoAdicionalNombre + "|" + infoAdicionalValor + "|";
                        oXML.WriteStartElement("campoAdicional");
                        oXML.WriteAttributeString("nombre", ia[0]);
                        oXML.WriteString(ia[1]);
                        /***Fin*********************************campoAdicional*/
                        oXML.WriteEndElement();
                    }
                    /***Fin*************************************infoAdicional*/
                    oXML.WriteEndElement();
                }
                oXML.WriteEndElement();
                oXML.Flush();
                xDoc.InnerXml = sw.ToString(); //convertir el xml a cadena
                return xDoc; //es la cadena que se va a mandar a timbrar
            }
            catch (Exception ex)
            {
                msjT = ex.Message;
                log.mensajesLog("XM006", "", msjT, "", ruc + codDoc + estab + ptoEmi + secuencial, "");
                return null;
            }
        }

        public XmlDocument xmlComprobanteRetencionV2()
        {
            XmlDocument xDoc = new XmlDocument();//retorna un xml como cadena (la que se timbra)
            StringWriter sw = new StringWriter();//convierte el xml como una cadena
            XmlTextWriter oXML = new XmlTextWriter(sw);
            try
            {
                oXML.Formatting = Formatting.Indented; //para darle formato, se puede comentar para algunas adendas 
                oXML.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");

                oXML.WriteStartElement("comprobanteRetencion");
                oXML.WriteAttributeString("id", idComprobante);
                oXML.WriteAttributeString("version", version);

                /*********************************************infoTributaria*/
                oXML.WriteStartElement("infoTributaria");
                oXML.WriteStartElement("ambiente"); oXML.WriteString(ambiente); oXML.WriteEndElement();
                oXML.WriteStartElement("tipoEmision"); oXML.WriteString(tipoEmision); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocial"); oXML.WriteString(razonSocial); oXML.WriteEndElement();
                if (nombreComercial.Length > 0) { oXML.WriteStartElement("nombreComercial"); oXML.WriteString(nombreComercial); oXML.WriteEndElement(); }
                oXML.WriteStartElement("ruc"); oXML.WriteString(ruc); oXML.WriteEndElement();
                oXML.WriteStartElement("claveAcceso"); oXML.WriteString(claveAcceso); oXML.WriteEndElement();
                oXML.WriteStartElement("codDoc"); oXML.WriteString(codDoc); oXML.WriteEndElement();
                oXML.WriteStartElement("estab"); oXML.WriteString(estab); oXML.WriteEndElement();
                oXML.WriteStartElement("ptoEmi"); oXML.WriteString(ptoEmi); oXML.WriteEndElement();
                oXML.WriteStartElement("secuencial"); oXML.WriteString(secuencial); oXML.WriteEndElement();
                oXML.WriteStartElement("dirMatriz"); oXML.WriteString(dirMatriz); oXML.WriteEndElement();
                /****Fin****************************************infoTributaria*/
                oXML.WriteEndElement();
                /**************************************************infoCompRetencion*/
                oXML.WriteStartElement("infoCompRetencion");
                DateTime tiempo = DateTime.ParseExact(fechaEmision, "dd/MM/yyyy", null);
                oXML.WriteStartElement("fechaEmision"); oXML.WriteString(Convert.ToDateTime(tiempo).ToString("dd/MM/yyyy")); oXML.WriteEndElement();
                if (dirEstablecimiento.Length > 0) { oXML.WriteStartElement("dirEstablecimiento"); oXML.WriteString(dirEstablecimiento); oXML.WriteEndElement(); }
                if (!String.IsNullOrEmpty(contribuyenteEspecial))
                {
                    oXML.WriteStartElement("contribuyenteEspecial"); oXML.WriteString(contribuyenteEspecial); oXML.WriteEndElement();
                }
                if (obligadoContabilidad.Length > 0) { oXML.WriteStartElement("obligadoContabilidad"); oXML.WriteString(obligadoContabilidad); oXML.WriteEndElement(); }
                oXML.WriteStartElement("tipoIdentificacionSujetoRetenido"); oXML.WriteString(tipoIdentificacionSujetoRetenido); oXML.WriteEndElement();
                if (tipoIdentificacionSujetoRetenido == "08")
                {
                    if (!String.IsNullOrEmpty(tipoSujetoRetenido))
                    {
                        oXML.WriteStartElement("tipoSujetoRetenido"); oXML.WriteString(tipoSujetoRetenido); oXML.WriteEndElement();
                    }
                }
                oXML.WriteStartElement("parteRel"); oXML.WriteString(parteRel); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocialSujetoRetenido"); oXML.WriteString(razonSocialSujetoRetenido); oXML.WriteEndElement();
                oXML.WriteStartElement("identificacionSujetoRetenido"); oXML.WriteString(identificacionSujetoRetenido); oXML.WriteEndElement();
                oXML.WriteStartElement("periodoFiscal"); oXML.WriteString(periodoFiscal); oXML.WriteEndElement();
                /****Fin****************************************infoCompRetencion*/
                oXML.WriteEndElement();
                /**********************************************docsSustento*/
                oXML.WriteStartElement("docsSustento");
                /***Ciclo*************************************docSustento*/
                foreach (String[] docSust in arraylDoscsSutentos)
                {
                    oXML.WriteStartElement("docSustento");
                    oXML.WriteStartElement("codSustento"); oXML.WriteString(docSust[1]); oXML.WriteEndElement();
                    oXML.WriteStartElement("codDocSustento"); oXML.WriteString(docSust[2]); oXML.WriteEndElement();
                    if (docSust[3].Length > 0)
                    {
                        oXML.WriteStartElement("numDocSustento"); oXML.WriteString(docSust[3].Replace("-", "")); oXML.WriteEndElement();
                    }
                    oXML.WriteStartElement("fechaEmisionDocSustento"); oXML.WriteString(docSust[4]); oXML.WriteEndElement();
                    if (docSust[5].Length > 0)
                    {
                        oXML.WriteStartElement("fechaRegistroContable"); oXML.WriteString(docSust[5]); oXML.WriteEndElement();
                    }
                    if (docSust[6].Length > 0)
                    {
                        oXML.WriteStartElement("numAutDocSustento"); oXML.WriteString(docSust[6]); oXML.WriteEndElement();
                    }
                    oXML.WriteStartElement("pagoLocExt"); oXML.WriteString(docSust[7]); oXML.WriteEndElement();
                    if (docSust[7] == "02")
                    {
                        oXML.WriteStartElement("tipoRegi"); oXML.WriteString(docSust[8]); oXML.WriteEndElement();
                        oXML.WriteStartElement("paisEfecPago"); oXML.WriteString(docSust[9]); oXML.WriteEndElement();
                        oXML.WriteStartElement("aplicConvDobTrib"); oXML.WriteString(docSust[10]); oXML.WriteEndElement();
                        if (docSust[10] == "NO")
                        {
                            oXML.WriteStartElement("pagExtSujRetNorLeg"); oXML.WriteString(docSust[11]); oXML.WriteEndElement();
                        }
                        oXML.WriteStartElement("pagoRegFis"); oXML.WriteString(docSust[12]); oXML.WriteEndElement();
                    }
                    if (docSust[2] == "41")
                    {
                        oXML.WriteStartElement("totalComprobantesReembolso"); oXML.WriteString(docSust[13]); oXML.WriteEndElement();
                        oXML.WriteStartElement("totalBaseImponibleReembolso"); oXML.WriteString(docSust[14]); oXML.WriteEndElement();
                        oXML.WriteStartElement("totalImpuestoReembolso"); oXML.WriteString(docSust[15]); oXML.WriteEndElement();

                    }
                    oXML.WriteStartElement("totalSinImpuestos"); oXML.WriteString(docSust[16]); oXML.WriteEndElement();
                    oXML.WriteStartElement("importeTotal"); oXML.WriteString(docSust[17]); oXML.WriteEndElement();

                    bool crearNodo = devolverCrearNodo(docSust[0], arraylImpuestosDetalles, 0);
                    if (crearNodo)
                    {
                        /**********************************************impuestosDocSustento*/
                        oXML.WriteStartElement("impuestosDocSustento");
                        /***Ciclo*************************************impuesto*/
                        foreach (String[] tir in arraylImpuestosDetalles)
                        {
                            if (docSust[0] == tir[0])
                            {
                                oXML.WriteStartElement("impuestoDocSustento");
                                oXML.WriteStartElement("codImpuestoDocSustento"); oXML.WriteString(tir[1]); oXML.WriteEndElement();
                                oXML.WriteStartElement("codigoPorcentaje"); oXML.WriteString(tir[2]); oXML.WriteEndElement();
                                oXML.WriteStartElement("baseImponible"); oXML.WriteString(tir[3]); oXML.WriteEndElement();
                                oXML.WriteStartElement("tarifa"); oXML.WriteString(tir[4]); oXML.WriteEndElement();
                                oXML.WriteStartElement("valorImpuesto"); oXML.WriteString(tir[5]); oXML.WriteEndElement();
                                /***Fin****************************************impuesto*/
                                oXML.WriteEndElement();
                            }
                        }
                        /***Fin*****************************************impuestosDocSustento*/
                        oXML.WriteEndElement();
                    }
                    crearNodo = false;
                    crearNodo = devolverCrearNodo(docSust[0], arraylTotalImpuestosRetenciones, 13);
                    if (crearNodo)
                    {
                        /**********************************************retenciones*/
                        oXML.WriteStartElement("retenciones");
                        /***Ciclo*************************************retencion*/
                        foreach (String[] tir in arraylTotalImpuestosRetenciones)
                        {
                            if (docSust[0] == tir[13])
                            {
                                oXML.WriteStartElement("retencion");
                                oXML.WriteStartElement("codigo"); oXML.WriteString(tir[0]); oXML.WriteEndElement();
                                oXML.WriteStartElement("codigoRetencion"); oXML.WriteString(tir[1]); oXML.WriteEndElement();
                                oXML.WriteStartElement("baseImponible"); oXML.WriteString(tir[2]); oXML.WriteEndElement();
                                oXML.WriteStartElement("porcentajeRetener"); oXML.WriteString(tir[3]); oXML.WriteEndElement();
                                oXML.WriteStartElement("valorRetenido"); oXML.WriteString(tir[4]); oXML.WriteEndElement();
                                if (docSust[1] == "10")
                                {
                                    /***Ciclo*************************************dividendos*/
                                    oXML.WriteStartElement("dividendos");
                                    oXML.WriteStartElement("fechaPagoDiv"); oXML.WriteString(tir[8]); oXML.WriteEndElement();
                                    oXML.WriteStartElement("imRentaSoc"); oXML.WriteString(tir[9]); oXML.WriteEndElement();
                                    oXML.WriteStartElement("ejerFisUtDiv"); oXML.WriteString(tir[10]); oXML.WriteEndElement();
                                    oXML.WriteEndElement();
                                    /***Fin****************************************dividendos*/
                                }
                                if (tir[1] == "338" || tir[1] == "340" || tir[1] == "341" || tir[1] == "342" || tir[1] == "342A" || tir[1] == "342B")
                                {
                                    /***Ciclo*************************************compraCajBanano*/
                                    oXML.WriteStartElement("compraCajBanano");
                                    oXML.WriteStartElement("numCajBan"); oXML.WriteString(tir[11]); oXML.WriteEndElement();
                                    oXML.WriteStartElement("precCajBan"); oXML.WriteString(tir[12]); oXML.WriteEndElement();
                                    oXML.WriteEndElement();
                                    /***Fin****************************************compraCajBanano*/
                                }
                                /***Fin****************************************retencion*/
                                oXML.WriteEndElement();
                            }
                        }
                        /***Fin*****************************************retenciones*/
                        oXML.WriteEndElement();
                    }
                    crearNodo = false;
                    crearNodo = devolverCrearNodo(docSust[0], arraylReembolsosRetenciones, 0);
                    if (crearNodo)
                    {
                        /**********************************************reembolsos*/
                        oXML.WriteStartElement("reembolsos");
                        /***Ciclo*************************************reembolsoDetalle*/
                        foreach (String[] tir in arraylReembolsosRetenciones)
                        {
                            if (docSust[0] == tir[0])
                            {
                                oXML.WriteStartElement("reembolsoDetalle");
                                oXML.WriteStartElement("tipoIdentificacionProveedorReembolso"); oXML.WriteString(tir[2]); oXML.WriteEndElement();
                                oXML.WriteStartElement("identificacionProveedorReembolso"); oXML.WriteString(tir[3]); oXML.WriteEndElement();
                                oXML.WriteStartElement("codPaisPagoProveedorReembolso"); oXML.WriteString(tir[4]); oXML.WriteEndElement();
                                oXML.WriteStartElement("tipoProveedorReembolso"); oXML.WriteString(tir[5]); oXML.WriteEndElement();
                                oXML.WriteStartElement("codDocReembolso"); oXML.WriteString(tir[6]); oXML.WriteEndElement();
                                oXML.WriteStartElement("estabDocReembolso"); oXML.WriteString(tir[7]); oXML.WriteEndElement();
                                oXML.WriteStartElement("ptoEmiDocReembolso"); oXML.WriteString(tir[8]); oXML.WriteEndElement();
                                oXML.WriteStartElement("secuencialDocReembolso"); oXML.WriteString(tir[9]); oXML.WriteEndElement();
                                oXML.WriteStartElement("fechaEmisionDocReembolso"); oXML.WriteString(tir[10]); oXML.WriteEndElement();
                                oXML.WriteStartElement("numeroAutorizacionDocReemb"); oXML.WriteString(tir[11]); oXML.WriteEndElement();
                                bool crearNodoImpReemb = devolverCrearNodo(tir[1], arraylImpuestosReembolsosRet, 0);
                                if (crearNodoImpReemb)
                                {
                                    /**********************************************detalleImpuestos*/
                                    oXML.WriteStartElement("detalleImpuestos");
                                    /***Ciclo*************************************detalleImpuesto*/
                                    foreach (String[] tirImp in arraylImpuestosReembolsosRet)
                                    {
                                        if (tir[0] == tirImp[7] & tir[1] == tirImp[0])
                                        {
                                            oXML.WriteStartElement("detalleImpuesto");
                                            oXML.WriteStartElement("codigo"); oXML.WriteString(tirImp[1]); oXML.WriteEndElement();
                                            oXML.WriteStartElement("codigoPorcentaje"); oXML.WriteString(tirImp[2]); oXML.WriteEndElement();
                                            oXML.WriteStartElement("tarifa"); oXML.WriteString(tirImp[3].Replace(".00", "")); oXML.WriteEndElement();
                                            oXML.WriteStartElement("baseImponibleReembolso"); oXML.WriteString(tirImp[4]); oXML.WriteEndElement();
                                            oXML.WriteStartElement("impuestoReembolso"); oXML.WriteString(tirImp[5]); oXML.WriteEndElement();
                                            /***Fin****************************************detalleImpuesto*/
                                            oXML.WriteEndElement();
                                        }
                                    }
                                    /***Fin****************************************detalleImpuestos*/
                                    oXML.WriteEndElement();
                                }
                                /***Fin****************************************reembolsoDetalle*/
                                oXML.WriteEndElement();
                            }
                        }
                        /***Fin*****************************************reembolsos*/
                        oXML.WriteEndElement();
                    }
                    crearNodo = false;
                    crearNodo = devolverCrearNodo(docSust[0], arraylPagos, 0);
                    if (crearNodo)
                    {
                        /***Inicio*****************************************Forma Pago*/
                        oXML.WriteStartElement("pagos");
                        /***Ciclo*************************************Pago*/
                        foreach (String[] tir in arraylPagos)
                        {
                            if (docSust[0] == tir[0])
                            {
                                oXML.WriteStartElement("pago");
                                if (tir[1].Length > 0)
                                {
                                    oXML.WriteStartElement("formaPago"); oXML.WriteString(tir[1]); oXML.WriteEndElement();
                                }
                                if (tir[2].Length > 0)
                                {
                                    oXML.WriteStartElement("total"); oXML.WriteString(tir[2]); oXML.WriteEndElement();
                                }
                                /***Fin****************************************pago*/
                                oXML.WriteEndElement();
                            }
                        }
                        /***Fin****************************************Forma Pago*/
                        oXML.WriteEndElement();
                    }
                    /***Fin****************************************docSustento*/
                    oXML.WriteEndElement();
                }
                /***Fin*****************************************docsSustento*/
                oXML.WriteEndElement();
                if (arraylInfoAdicionales.Count > 0) // && arraylInfoAdicionales.Count < 15
																{
																				int cuentaIA = 0;
                    /***Inicio**********************************infoAdicional*/
                    oXML.WriteStartElement("infoAdicional");
                    /***Ciclo*******************************campoAdicional*/
                    foreach (String[] ia in arraylInfoAdicionales)
                    {       //IA |" + infoAdicionalNombre + "|" + infoAdicionalValor + "|";
																								cuentaIA++;
                        oXML.WriteStartElement("campoAdicional");
                        oXML.WriteAttributeString("nombre", ia[0]);
                        oXML.WriteString(ia[1]);
                        /***Fin*********************************campoAdicional*/
                        oXML.WriteEndElement();
																								if (cuentaIA == 15)
																												break;
                    }
                    /***Fin*************************************infoAdicional*/
                    oXML.WriteEndElement();
                }
                oXML.WriteEndElement();
                oXML.Flush();
                xDoc.InnerXml = sw.ToString(); //convertir el xml a cadena
                return xDoc; //es la cadena que se va a mandar a timbrar
            }
            catch (Exception ex)
            {
                msjT = ex.Message;
                log.mensajesLog("XM006", "", msjT, "", ruc + codDoc + estab + ptoEmi + secuencial, "");
                return null;
            }
        }

   

        public XmlDocument xmlNotaDebito()
        {
            XmlDocument xDoc = new XmlDocument();//retorna un xml como cadena (la que se timbra)
            StringWriter sw = new StringWriter();//convierte el xml como una cadena
            XmlTextWriter oXML = new XmlTextWriter(sw);
            try
            {
                oXML.Formatting = Formatting.Indented; //para darle formato, se puede comentar para algunas adendas 
                oXML.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");

                oXML.WriteStartElement("notaDebito");
                // oXML.WriteAttributeString("xmlns:ds", @"http://www.w3.org/2000/09/xmldsig#");
                //oXML.WriteAttributeString("xsi:schemaLocation", @"C:\Documents and Settings\mfsalazar\Escritorio\facturación electronica\formato xsd xml 07-03\notaDebito1.xsd");
                // oXML.WriteAttributeString("xmlns:xsi", @"http://www.w3.org/2001/XMLSchema-instance");

                oXML.WriteAttributeString("id", idComprobante);
                oXML.WriteAttributeString("version", version);

                /*********************************************infoTributaria*/
                oXML.WriteStartElement("infoTributaria");
                oXML.WriteStartElement("ambiente"); oXML.WriteString(ambiente); oXML.WriteEndElement();
                oXML.WriteStartElement("tipoEmision"); oXML.WriteString(tipoEmision); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocial"); oXML.WriteString(razonSocial); oXML.WriteEndElement();
                oXML.WriteStartElement("nombreComercial"); oXML.WriteString(nombreComercial); oXML.WriteEndElement();
                oXML.WriteStartElement("ruc"); oXML.WriteString(ruc); oXML.WriteEndElement();
                oXML.WriteStartElement("claveAcceso"); oXML.WriteString(claveAcceso); oXML.WriteEndElement();
                oXML.WriteStartElement("codDoc"); oXML.WriteString(codDoc); oXML.WriteEndElement();
                oXML.WriteStartElement("estab"); oXML.WriteString(estab); oXML.WriteEndElement();
                oXML.WriteStartElement("ptoEmi"); oXML.WriteString(ptoEmi); oXML.WriteEndElement();
                oXML.WriteStartElement("secuencial"); oXML.WriteString(secuencial); oXML.WriteEndElement();
                oXML.WriteStartElement("dirMatriz"); oXML.WriteString(dirMatriz); oXML.WriteEndElement();
                oXML.WriteEndElement();
                /****Fin****************************************infoTributaria*/
                /**************************************************infoNotaDebito*/
                oXML.WriteStartElement("infoNotaDebito");
                oXML.WriteStartElement("fechaEmision"); oXML.WriteString(Convert.ToDateTime(fechaEmision).ToString("dd/MM/yyyy")); oXML.WriteEndElement();
                if (dirEstablecimiento.Length > 0) { oXML.WriteStartElement("dirEstablecimiento"); oXML.WriteString(dirEstablecimiento); oXML.WriteEndElement(); }
                oXML.WriteStartElement("tipoIdentificacionComprador"); oXML.WriteString(tipoIdentificacionComprador); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocialComprador"); oXML.WriteString(razonSocialComprador); oXML.WriteEndElement();
                oXML.WriteStartElement("identificacionComprador"); oXML.WriteString(identificacionComprador); oXML.WriteEndElement();
                if (!String.IsNullOrEmpty(contribuyenteEspecial))
                {
                    oXML.WriteStartElement("contribuyenteEspecial"); oXML.WriteString(contribuyenteEspecial); oXML.WriteEndElement();
                }
                if (obligadoContabilidad.Length > 0) { oXML.WriteStartElement("obligadoContabilidad"); oXML.WriteString(obligadoContabilidad); oXML.WriteEndElement(); }
                if (rise.Length > 0)
                {
                    oXML.WriteStartElement("rise"); oXML.WriteString(rise); oXML.WriteEndElement();
                }
                oXML.WriteStartElement("codDocModificado"); oXML.WriteString(codDocModificado); oXML.WriteEndElement();
                oXML.WriteStartElement("numDocModificado"); oXML.WriteString(numDocModificado); oXML.WriteEndElement();
                oXML.WriteStartElement("fechaEmisionDocSustento"); oXML.WriteString(Convert.ToDateTime(fechaEmisionDocSustentoNota).ToString("dd/MM/yyyy")); oXML.WriteEndElement();
                oXML.WriteStartElement("totalSinImpuestos"); oXML.WriteString(totalSinImpuestos); oXML.WriteEndElement();
                /**********************************************impuestos*/
                oXML.WriteStartElement("impuestos");
                /***Ciclo**************************************impuesto*/
                foreach (String[] ti in arraylTotalImpuestos)
                {         //"IT |" + Codigo + "|" + CodigoPorcentaje + "|" + Tarifa + "|" + BaseImponible + "|" + Valor + "|"+Impuestos +"|";
                    oXML.WriteStartElement("impuesto");
                    oXML.WriteStartElement("codigo"); oXML.WriteString(ti[0]); oXML.WriteEndElement();
                    oXML.WriteStartElement("codigoPorcentaje"); oXML.WriteString(ti[1]); oXML.WriteEndElement();
                    oXML.WriteStartElement("tarifa"); oXML.WriteString(ti[3]); oXML.WriteEndElement();
                    oXML.WriteStartElement("baseImponible"); oXML.WriteString(ti[2]); oXML.WriteEndElement();
                    oXML.WriteStartElement("valor"); oXML.WriteString(ti[4]); oXML.WriteEndElement();
                    /***Fin*****************************************totalImpuesto*/
                    oXML.WriteEndElement();
                }
                /***Fin*****************************************impuestos*/
                oXML.WriteEndElement();
																/***Inicio Compesaciones*****************************************Compensaciones*/
																#region Compensaciones
																System.Collections.IEnumerator enumeratorC;
																if (this.arraylCompensacion != null && this.arraylCompensacion.Count > 0)
																{
																				oXML.WriteStartElement("compensaciones");
																				enumeratorC = this.arraylCompensacion.GetEnumerator();
																				try
																				{
																								while (enumeratorC.MoveNext())
																								{
																												string[] arrayC = (string[])enumeratorC.Current;
																												oXML.WriteStartElement("compensacion");
																												if (arrayC[0].Length > 0)
																												{
																																oXML.WriteStartElement("codigo");
																																oXML.WriteString(arrayC[0]);
																																oXML.WriteEndElement();
																												}
																												if (arrayC[1].Length > 0)
																												{
																																oXML.WriteStartElement("tarifa");
																																oXML.WriteString(arrayC[1]);
																																oXML.WriteEndElement();
																												}
																												if (arrayC[2].Length > 0)
																												{
																																oXML.WriteStartElement("valor");
																																oXML.WriteString(valida_texto_a_numero(arrayC[2].ToString()).Replace(",", "."));
																																oXML.WriteEndElement();
																												}
																												oXML.WriteEndElement();
																								}
																				}
																				finally
																				{
																								System.IDisposable disposable = enumeratorC as System.IDisposable;
																								if (disposable != null)
																								{
																												disposable.Dispose();
																								}
																				}
																				oXML.WriteEndElement();
																}
																#endregion
																/***Fin Compesaciones*****************************************Compensaciones*/
                oXML.WriteStartElement("valorTotal"); oXML.WriteString(valorTotal); oXML.WriteEndElement();
																/***Inicio FormaPago*****************************************pagos*/
																#region FormaPago
																System.Collections.IEnumerator enumerator;
																if (this.arraylPagos != null && this.arraylPagos.Count > 0)
																{
																				oXML.WriteStartElement("pagos");
																				enumerator = this.arraylPagos.GetEnumerator();
																				try
																				{
																								while (enumerator.MoveNext())
																								{
																												string[] array2 = (string[])enumerator.Current;
																												oXML.WriteStartElement("pago");
																												if (array2[0].Length > 0)
																												{
																																oXML.WriteStartElement("formaPago");
																																oXML.WriteString(array2[0]);
																																oXML.WriteEndElement();
																												}
																												if (array2[1].Length > 0)
																												{
																																oXML.WriteStartElement("total");
																																oXML.WriteString(valida_texto_a_numero(array2[1].ToString()).Replace(",", "."));
																																oXML.WriteEndElement();
																												}
																												if (array2[2].Length > 0)
																												{
																																oXML.WriteStartElement("plazo");
																																oXML.WriteString(array2[2]);
																																oXML.WriteEndElement();
																												}
																												if (array2[3].Length > 0)
																												{
																																oXML.WriteStartElement("unidadTiempo");
																																oXML.WriteString(array2[3]);
																																oXML.WriteEndElement();
																												}
																												oXML.WriteEndElement();
																								}
																				}
																				finally
																				{
																								System.IDisposable disposable = enumerator as System.IDisposable;
																								if (disposable != null)
																								{
																												disposable.Dispose();
																								}
																				}
																				oXML.WriteEndElement();
																}
																#endregion
																/***Fin FormaPago*****************************************pagos*/
                /****Fin****************************************infoNotaDebito*/
                oXML.WriteEndElement();
                /************************************************motivos*/
                oXML.WriteStartElement("motivos");
                /**Ciclo***************************************motivo*/
                foreach (String[] d in arraylMotivos)
                {
                    oXML.WriteStartElement("motivo");
                    oXML.WriteStartElement("razon"); oXML.WriteString(d[0]); oXML.WriteEndElement();
                    oXML.WriteStartElement("valor"); oXML.WriteString(totalSinImpuestos); oXML.WriteEndElement();
                    /***Fin****************************************motivo*/
                    oXML.WriteEndElement();
                }
                /***Fin******************************************motivos*/
                oXML.WriteEndElement();
                /***Inicio**********************************infoAdicional*/

                if (arraylInfoAdicionales.Count > 0) //&& arraylInfoAdicionales.Count < 15
                {
                    int cuentaIA = 0;
                    oXML.WriteStartElement("infoAdicional");
                    /***Ciclo*******************************campoAdicional*/
                    foreach (String[] ia in arraylInfoAdicionales)
                    {       //IA |" + infoAdicionalNombre + "|" + infoAdicionalValor + "|";
                        cuentaIA++;
                        oXML.WriteStartElement("campoAdicional");
                        oXML.WriteAttributeString("nombre", ia[0]);
                        oXML.WriteString(ia[1]);
                        /***Fin*********************************campoAdicional*/
                        oXML.WriteEndElement();
                        if (cuentaIA == 15)
                            break;
                    }
                    /***Fin*************************************infoAdicional*/
                    oXML.WriteEndElement();
                }
                oXML.WriteEndElement();
                oXML.Flush();
                xDoc.InnerXml = sw.ToString(); //convertir el xml a cadena
                return xDoc; //es la cadena que se va a mandar a timbrar
            }
            catch (Exception ex)
            {
                msjT = ex.Message;
                log.mensajesLog("XM004", "", msjT, "", ruc + codDoc + estab + ptoEmi + secuencial, "");
                return null;
            }
        }

        public XmlDocument xmlGuiaRemision()
        {
            XmlDocument xDoc = new XmlDocument();//retorna un xml como cadena (la que se timbra)
            StringWriter sw = new StringWriter();//convierte el xml como una cadena
            XmlTextWriter oXML = new XmlTextWriter(sw);
            try
            {
                oXML.Formatting = Formatting.Indented; //para darle formato, se puede comentar para algunas adendas 
                oXML.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""UTF-8""");

                oXML.WriteStartElement("guiaRemision");
                //oXML.WriteAttributeString("xmlns:ds", @"http://www.w3.org/2000/09/xmldsig#");
                // oXML.WriteAttributeString("xsi:schemaLocation", @"C:\Documents and Settings\mfsalazar\Escritorio\facturación electronica\formato xsd xml 07-03\guiaRemision1.xsd");
                // oXML.WriteAttributeString("xmlns:xsi", @"http://www.w3.org/2001/XMLSchema-instance");

                oXML.WriteAttributeString("id", idComprobante);
                oXML.WriteAttributeString("version", version);

                /*********************************************infoTributaria*/
                oXML.WriteStartElement("infoTributaria");
                oXML.WriteStartElement("ambiente"); oXML.WriteString(ambiente); oXML.WriteEndElement();
                oXML.WriteStartElement("tipoEmision"); oXML.WriteString(tipoEmision); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocial"); oXML.WriteString(razonSocial); oXML.WriteEndElement();
                if (nombreComercial.Length > 0) { oXML.WriteStartElement("nombreComercial"); oXML.WriteString(nombreComercial); oXML.WriteEndElement(); }
                oXML.WriteStartElement("ruc"); oXML.WriteString(ruc); oXML.WriteEndElement();
                oXML.WriteStartElement("claveAcceso"); oXML.WriteString(claveAcceso); oXML.WriteEndElement();
                oXML.WriteStartElement("codDoc"); oXML.WriteString(codDoc); oXML.WriteEndElement();
                oXML.WriteStartElement("estab"); oXML.WriteString(estab); oXML.WriteEndElement();
                oXML.WriteStartElement("ptoEmi"); oXML.WriteString(ptoEmi); oXML.WriteEndElement();
                oXML.WriteStartElement("secuencial"); oXML.WriteString(secuencial); oXML.WriteEndElement();
                oXML.WriteStartElement("dirMatriz"); oXML.WriteString(dirMatriz); oXML.WriteEndElement();
                /****Fin****************************************infoTributaria*/
                oXML.WriteEndElement();
                /*********************************************infoGuiaRemision*/
                oXML.WriteStartElement("infoGuiaRemision");
                if (dirEstablecimiento.Length > 0) { oXML.WriteStartElement("dirEstablecimiento"); oXML.WriteString(dirEstablecimiento); oXML.WriteEndElement(); }
                oXML.WriteStartElement("dirPartida"); oXML.WriteString(dirPartida); oXML.WriteEndElement();
                oXML.WriteStartElement("razonSocialTransportista"); oXML.WriteString(razonSocialTransportista); oXML.WriteEndElement();
                oXML.WriteStartElement("tipoIdentificacionTransportista"); oXML.WriteString(tipoIdentificacionTransportista); oXML.WriteEndElement();
                oXML.WriteStartElement("rucTransportista"); oXML.WriteString(rucTransportista); oXML.WriteEndElement();
                if (rise.Length > 0)
                {
                    oXML.WriteStartElement("rise"); oXML.WriteString(rise); oXML.WriteEndElement();
                }
                if (obligadoContabilidad.Length > 0) { oXML.WriteStartElement("obligadoContabilidad"); oXML.WriteString(obligadoContabilidad); oXML.WriteEndElement(); }
                if (!String.IsNullOrEmpty(contribuyenteEspecial))
                {
                    oXML.WriteStartElement("contribuyenteEspecial"); oXML.WriteString(contribuyenteEspecial); oXML.WriteEndElement();
                }
                oXML.WriteStartElement("fechaIniTransporte"); oXML.WriteString(fechaIniTransporte); oXML.WriteEndElement();
                oXML.WriteStartElement("fechaFinTransporte"); oXML.WriteString(fechaFinTransporte); oXML.WriteEndElement();
                oXML.WriteStartElement("placa"); oXML.WriteString(placa); oXML.WriteEndElement();
                /****Fin****************************************infoGuiaRemision*/
                oXML.WriteEndElement();
                /**********************************************destinatarios*/
                oXML.WriteStartElement("destinatarios");
                /***Ciclo**************************************destinatario*/
                foreach (String[] dest in arraylDestinatarios)
                {
                    oXML.WriteStartElement("destinatario");
                    oXML.WriteStartElement("identificacionDestinatario"); oXML.WriteString(dest[0]); oXML.WriteEndElement();
                    oXML.WriteStartElement("razonSocialDestinatario"); oXML.WriteString(dest[1]); oXML.WriteEndElement();
                    oXML.WriteStartElement("dirDestinatario"); oXML.WriteString(dest[2]); oXML.WriteEndElement();
                    oXML.WriteStartElement("motivoTraslado"); oXML.WriteString(dest[3]); oXML.WriteEndElement();
                    if (dest[4].Length > 0) { oXML.WriteStartElement("docAduaneroUnico"); oXML.WriteString(dest[4]); oXML.WriteEndElement(); }
                    if (dest[5].Length == 3) { oXML.WriteStartElement("codEstabDestino"); oXML.WriteString(dest[5]); oXML.WriteEndElement(); }
                    if (dest[6].Length > 0) { oXML.WriteStartElement("ruta"); oXML.WriteString(dest[6]); oXML.WriteEndElement(); }
                    if (!String.IsNullOrEmpty(dest[7])) { oXML.WriteStartElement("codDocSustento"); oXML.WriteString(dest[7]); oXML.WriteEndElement(); }
																				if (!String.IsNullOrEmpty(dest[8])) { oXML.WriteStartElement("numDocSustento"); oXML.WriteString(dest[8].Trim()); oXML.WriteEndElement(); }
                    if (!String.IsNullOrEmpty(dest[9])) { oXML.WriteStartElement("numAutDocSustento"); oXML.WriteString(dest[9]); oXML.WriteEndElement(); }
																				if (!String.IsNullOrEmpty(dest[10]))
																				{
																								DateTime tiempo = DateTime.ParseExact(dest[10], "dd/MM/yyyy", null);
																								oXML.WriteStartElement("fechaEmisionDocSustento"); oXML.WriteString(Convert.ToDateTime(tiempo).ToString("dd/MM/yyyy")); oXML.WriteEndElement();
																				}
                    /************************************************Detalles*/
                    oXML.WriteStartElement("detalles");
                    /**Ciclo***************************************Detalle*/
                    foreach (String[] d in arraylDetalles)
                    {
                        if (dest[11].ToString().Equals(d[8].ToString())) //Verifica el codigo del detalle
                        {
                            oXML.WriteStartElement("detalle");
                            if (d[0].Length > 0) { oXML.WriteStartElement("codigoInterno"); oXML.WriteString(d[0]); oXML.WriteEndElement(); }
                            if (d[1].Length > 0)
                            {
                                oXML.WriteStartElement("codigoAdicional"); oXML.WriteString(d[1]); oXML.WriteEndElement();
                            }
                            oXML.WriteStartElement("descripcion"); oXML.WriteString(d[2]); oXML.WriteEndElement();
                            oXML.WriteStartElement("cantidad"); oXML.WriteString(d[3]); oXML.WriteEndElement();
                            /*******************************detallesAdicionales*/
                            if (arraylDetallesAdicionales.Count > 0)
                            {
                                oXML.WriteStartElement("detallesAdicionales");
                                /**Ciclo********************detAdicional*/
                               
                                foreach (String[] da in arraylDetallesAdicionales)
                                {
                                    if (d[7].ToString().Equals(da[3].ToString()))
                                    {
                                        oXML.WriteStartElement("detAdicional");
                                        oXML.WriteAttributeString("valor", da[1]);
                                        oXML.WriteAttributeString("nombre", da[0]);
                                        /***Fin*********************detAdicional*/
                                        oXML.WriteEndElement();
                                        
                                    }
                                   
                                }
                                /***Fin**************************detallesAdicionales*/
                                oXML.WriteEndElement();
                            }
                            /***Fin****************************************Detalle*/
                            oXML.WriteEndElement();
                        }
                    }
                    /***Fin******************************************Detalles*/
                    oXML.WriteEndElement();
                    /***Fin*****************************************destinatario*/
                    oXML.WriteEndElement();
                }
                /***Fin*****************************************destinatarios*/
                oXML.WriteEndElement();
                /***Inicio**********************************infoAdicional*/
                
                if (arraylInfoAdicionales.Count > 0)
                {
                    int cuentaIA = 0;
                    
                    oXML.WriteStartElement("infoAdicional");
                    /***Ciclo*******************************campoAdicional*/
                    foreach (String[] ia in arraylInfoAdicionales)
                    {       //IA |" + infoAdicionalNombre + "|" + infoAdicionalValor + "|";
                        cuentaIA++;
                        oXML.WriteStartElement("campoAdicional");
                        oXML.WriteAttributeString("nombre", ia[0]);
                        oXML.WriteString(ia[1]);
                        /***Fin*********************************campoAdicional*/
                        oXML.WriteEndElement();
                        if (cuentaIA == 15)
                            break;
                    }
                    /***Fin*************************************infoAdicional*/
                    oXML.WriteEndElement();
                }
                oXML.WriteEndElement();
                oXML.Flush();
                xDoc.InnerXml = sw.ToString(); //convertir el xml a cadena
                return xDoc; //es la cadena que se va a mandar a timbrar
            }
            catch (Exception ex)
            {
                msjT = ex.Message;
                log.mensajesLog("XM005", "", msjT, "", ruc + codDoc + estab + ptoEmi + secuencial, "");
                return null;
            }
        }

								private string valida_texto_a_numero(string p_valor)
								{
												if (string.IsNullOrEmpty(p_valor))
												{
																p_valor = "0";
												}
												else
												{
																p_valor.Replace(",", string.Empty);
																p_valor.Trim();
												}
												return p_valor;
								}

        //PMONCAYO 20200813 (Se agrega etiquetas de exportacion )
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


        public void docsSustentos(ArrayList arraylDoscsSutentos)
        {
            this.arraylDoscsSutentos = arraylDoscsSutentos;
        }

        public void reembolsosSustentos(ArrayList arraylReembolsosRetenciones)
        {
            this.arraylReembolsosRetenciones = arraylReembolsosRetenciones;
        }

        public void impuestosReembolsosSustentos(ArrayList arraylImpuestosReembolsosRet)
        {
            this.arraylImpuestosReembolsosRet = arraylImpuestosReembolsosRet;
        }

        public bool devolverCrearNodo(string id, ArrayList arrayl, int posicion)
        {
            bool insertar = false;
            foreach (String[] current in arrayl)
            {
                if (id == current[posicion])
                {
                    insertar = true;
                    break;
                }
            }
            return insertar;
        }
    }
}