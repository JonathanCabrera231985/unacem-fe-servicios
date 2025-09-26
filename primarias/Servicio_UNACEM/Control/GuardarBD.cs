using System;
using Datos;
using System.Data.Common;
using System.Collections;
using System.Drawing;
using BarcodeLib.Barcode;
using System.IO;
using System.Globalization;
using System.Data;
using System.Xml.Linq;
using clibLogger;
using CriptoSimetrica;

namespace Control
{
    class GuardarBD
    {
        private EnviarMail em;
        private NumerosALetras numA;
        private Log log;
        private string msj = "";
        private string msjT = "";
        private string msjAux = "";
        private string RutaTXT = "";
        private string RutaBCK = "";
        private string RutaDOC = "";
        private string RutaERR = "";
        private string RutaCER = "";
        private string RutaKEY = "";
        private string codigoControl = "";
        Code.BarcodeGenerator bgCode128 = new Code.BarcodeGenerator();
        Code39.Code39 bgCode39;
        Code.Convertir cCode = new Code.Convertir();
        byte[] imgBar;
        private System.Collections.ArrayList arraylPagos;
        private System.Collections.ArrayList arraylRubros;
        private System.Collections.ArrayList arraylCompensacion;
        private AES cs = new AES();

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
        string fechaEmision, dirEstablecimiento, dirEstablecimientoGuia, contribuyenteEspecial, obligadoContabilidad, tipoIdentificacionComprador;
        string guiaRemision, razonSocialComprador, identificacionComprador, totalSinImpuestos, totalDescuento, propina, importeTotal, moneda;
        string dirPartida, razonSocialTransportista, tipoIdentificacionTransportista, rucTransportista, rise, fechaIniTransporte, fechaFinTransporte, placa;//Guia de Remision
        string codDocModificado, numDocModificado, fechaEmisionDocSustentoNota, valorModificacion, motivo;//Nota de Credito
        string valorTotal;
        //comprobante de Retencion
        string tipoIdentificacionSujetoRetenido, razonSocialSujetoRetenido, identificacionSujetoRetenido, periodoFiscal;
        //Destinatario Para Guia de Remision
        string identificacionDestinatario, razonSocialDestinatario, dirDestinatario, motivoTraslado, docAduaneroUnico, codEstabDestino, ruta, codDocSustentoDestinatario, numDocSustentoDestinatario, numAutDocSustento, fechaEmisionDocSustentoDestinatario;
        //Total de Impuestos
        string codigo, codigoPorcentaje, baseImponible, tarifa, valor;
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
        string direccionComprador;
        //ret 2.0
        string tipoSujetoRetenido, parteRel;
        #endregion

        //variables en comun.
        string identificacionRec;
        string tipoIdentificacionRec;
        string razonSocialRec;
        string direccionRec = "";

        //Necesarias
        string firmaSRI;

        //IdTablas

        string id_Config = "1";
        string id_Empleado = "1";
        string id_Receptor = "1";
        string id_ReceptorDest = "1";
        string id_Emisor = "1";
        string id_EmisorExp = "1";
        string id_ReceptorCon = "1";
        public string id_Comprobante { get; set; }
        string id_Destinatario = "1";
        string id_Detalles = "1";

        string empleado = "";
        #region totales
        string subtotal12 = "";
        string subtotal0 = "";
        string subtotalNoSujeto = "";
        string ICE = "";
        string IVA12 = "";
        string importeAPagar = "";

        //PMONCAYO 20200814 (Etiquetas de exportacion)
        string comercioExterior, incoTermFactura,
         lugarIncoTerm,
         paisOrigen,
         puertoEmbarque,
         puertoDestino,
         paisDestino,
         paisAdquisicion,
         incoTermTotalSinImpuestos,
         fleteInternacional,
         seguroInternacional,
         gastosAduaneros,
         gastosTransporteOtros;

        #endregion
        private string totalComprobantesReembolso = "";
        private string totalBaseImponibleReembolso = "";
        private string totalImpuestoReembolso = "";

        ArrayList arraylDetalles;
        ArrayList arraylImpuestosDetalles;
        ArrayList arraylDetallesAdicionales;
        ArrayList arraylInfoAdicionales;
        ArrayList arraylTotalImpuestos;
        //IVA 15
        ArrayList arraylTotalConImpuestos;
        ArrayList arraylMotivos;
        ArrayList arraylTotalImpuestosRetenciones;
        ArrayList arraylDestinatarios;
        ArrayList arraylDoscsSutentos;
        ArrayList arraylReembolsosRetenciones;
        ArrayList arraylImpuestosReembolsosRet;

        //Información Adicional CIMA
        string termino, proforma, domicilio, telefono, pedido;

        public GuardarBD()
        {
            arraylDetalles = new ArrayList();
            arraylImpuestosDetalles = new ArrayList();
            arraylDetallesAdicionales = new ArrayList();
            arraylInfoAdicionales = new ArrayList();
            arraylTotalImpuestos = new ArrayList();
            //IVA 15
            arraylTotalConImpuestos = new ArrayList();
            arraylMotivos = new ArrayList();
            arraylTotalImpuestosRetenciones = new ArrayList();
            arraylDestinatarios = new ArrayList();
            arraylPagos = new ArrayList();
            arraylDoscsSutentos = new ArrayList();
            arraylReembolsosRetenciones = new ArrayList();
            arraylImpuestosReembolsosRet = new ArrayList();
            arraylRubros = new ArrayList();
            arraylCompensacion = new ArrayList();
            BasesDatos DB = new BasesDatos();
            try
            {
                DB = new BasesDatos();
                log = new Log();
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
                //Fin de Parametros Generales.

                version = ""; idComprobante = "";
                id_Comprobante = "";
                this.ambiente = ""; this.tipoEmision = ""; this.razonSocial = "";
                this.nombreComercial = ""; this.ruc = ""; this.claveAcceso = "";
                this.codDoc = ""; this.estab = ""; this.ptoEmi = ""; this.secuencial = "";
                this.dirMatriz = "";
                this.fechaEmision = ""; this.dirEstablecimiento = ""; this.dirEstablecimientoGuia = ""; this.contribuyenteEspecial = "";
                this.obligadoContabilidad = ""; this.tipoIdentificacionComprador = "";
                this.guiaRemision = ""; this.razonSocialComprador = ""; this.identificacionComprador = ""; this.direccionRec = "";
                this.moneda = "";
                this.dirPartida = ""; this.razonSocialTransportista = ""; this.tipoIdentificacionTransportista = "";
                this.rucTransportista = ""; this.rise = ""; this.fechaIniTransporte = ""; this.fechaFinTransporte = "";
                this.placa = ""; this.codDocModificado = ""; this.numDocModificado = "";
                this.fechaEmisionDocSustentoNota = ""; this.valorModificacion = ""; this.motivo = ""; this.direccionComprador = "";

                //Info Adicional CIMA
                termino = ""; proforma = ""; domicilio = ""; telefono = ""; pedido = ""; firmaSRI = "";
                msjAux = "";
                msjT = "";
                msj = "";
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

        public void xmlComprobante(string version, string idComprobante, string guiaServ)
        {
            this.version = version;
            this.idComprobante = idComprobante;
        }

        public void InformacionTributaria(string ambiente, string tipoEmision, string razonSocial, string nombreComercial, string ruc,
            string claveAcceso, string codDoc, string estab, string ptoEmi, string secuencial, string dirMatriz, string emails)
        {
            this.ambiente = ambiente; this.tipoEmision = tipoEmision; this.razonSocial = razonSocial;
            this.nombreComercial = nombreComercial; this.ruc = ruc; this.claveAcceso = claveAcceso;
            this.codDoc = codDoc; this.estab = estab; this.ptoEmi = ptoEmi; this.secuencial = secuencial;
            this.dirMatriz = dirMatriz; this.emails = emails;
        }

        public void infromacionDocumento(string fechaEmision, string dirEstablecimiento, string dirEstablecimientoGuia, string contribuyenteEspecial, string obligadoContabilidad, string tipoIdentificacionComprador,
            string guiaRemision, string razonSocialComprador, string identificacionComprador, string moneda,
            string dirPartida, string razonSocialTransportista, string tipoIdentificacionTransportista, string rucTransportista, string rise, string fechaIniTransporte, string fechaFinTransporte, string placa,//Guia de Remision
                                                string codDocModificado, string numDocModificado, string fechaEmisionDocSustentoNota, string valorModificacion, string motivo, string direccionComprador)
        {
            this.fechaEmision = fechaEmision; this.dirEstablecimiento = dirEstablecimiento; this.dirEstablecimientoGuia = dirEstablecimientoGuia; this.contribuyenteEspecial = contribuyenteEspecial;
            this.obligadoContabilidad = obligadoContabilidad; this.tipoIdentificacionComprador = tipoIdentificacionComprador;
            this.guiaRemision = guiaRemision; this.razonSocialComprador = razonSocialComprador; this.identificacionComprador = identificacionComprador; this.direccionComprador = direccionComprador;
            this.moneda = moneda;
            this.dirPartida = dirPartida; this.razonSocialTransportista = razonSocialTransportista; this.tipoIdentificacionTransportista = tipoIdentificacionTransportista;
            this.rucTransportista = rucTransportista; this.rise = rise; this.fechaIniTransporte = fechaIniTransporte; this.fechaFinTransporte = fechaFinTransporte;
            this.placa = placa; this.codDocModificado = codDocModificado; this.numDocModificado = numDocModificado;
            this.fechaEmisionDocSustentoNota = fechaEmisionDocSustentoNota; this.valorModificacion = valorModificacion; this.motivo = motivo;

            if (!String.IsNullOrEmpty(identificacionComprador))
            {
                identificacionRec = identificacionComprador; tipoIdentificacionRec = tipoIdentificacionComprador; razonSocialRec = razonSocialComprador; direccionRec = direccionComprador;
            }
            if (!String.IsNullOrEmpty(rucTransportista))
            {
                this.motivo = tipoIdentificacionTransportista + "|" + rucTransportista + "|" + razonSocialTransportista;
            }
        }
        public void comprobanteRetencion(string periodoFiscal, string tipoIdentificacionSujetoRetenido, string razonSocialSujetoRetenido, string identificacionSujetoRetenido, string tipoSujetoRetenido = null, string parteRel = null)
        {
            this.periodoFiscal = periodoFiscal; this.tipoIdentificacionSujetoRetenido = tipoIdentificacionSujetoRetenido;
            this.razonSocialSujetoRetenido = razonSocialSujetoRetenido; this.identificacionSujetoRetenido = identificacionSujetoRetenido;
            identificacionRec = this.identificacionSujetoRetenido; tipoIdentificacionRec = this.tipoIdentificacionSujetoRetenido; razonSocialRec = this.razonSocialSujetoRetenido;
            this.tipoSujetoRetenido = tipoSujetoRetenido; this.parteRel = parteRel;
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
        public void motivoND(string motivo)
        {
            this.motivo = motivo;
        }

        public void Destinatarios(ArrayList arraylDestinatarios)
        {
            this.arraylDestinatarios = arraylDestinatarios;
        }

        public void guarda_destinatario_receptor(string ruc, string razonSocial)
        {
            string tipoid;
            if (ruc.Length == 13)
            {
                tipoid = "04";
            }
            else
            {
                if (ruc.Length == 10)
                {
                    tipoid = "05";
                }
                else
                {
                    tipoid = "06";
                }
            }
            identificacionRec = ruc; tipoIdentificacionRec = tipoid; razonSocialRec = razonSocial;
        }

        public void impuestos(ArrayList arraylImpuestosDetalles)
        {
            this.arraylImpuestosDetalles = arraylImpuestosDetalles;
        }

        public void totalImpuestos(ArrayList arraylTotalImpuestos)
        {
            this.arraylTotalImpuestos = arraylTotalImpuestos;
        }

        public void totalConImpuestos(ArrayList arraylTotalImpuestos)
        {
            this.arraylTotalConImpuestos = arraylTotalImpuestos;
        }

        public void totalImpuestosRetenciones(ArrayList arraylTotalImpuestosRetenciones)
        {
            this.arraylTotalImpuestosRetenciones = arraylTotalImpuestosRetenciones;
        }

        public void Motivos(ArrayList arraylMotivos)
        {
            this.arraylMotivos = arraylMotivos;
        }

        public void detallesAdicionales(ArrayList arraylDetallesAdicionales)
        {
            this.arraylDetallesAdicionales = arraylDetallesAdicionales;
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
        public void informacionAdicional(ArrayList arraylInfoAdicionales)
        {
            this.arraylInfoAdicionales = arraylInfoAdicionales;
        }
        public void otrosCampos(string claveAcceso, string secuencial, string guiaRemision, string codigoControl)
        {
            this.claveAcceso = claveAcceso;
            this.secuencial = secuencial;
            this.guiaRemision = guiaRemision;
            this.codigoControl = codigoControl;
        }
        public Boolean guardarBD()
        {
            BasesDatos DB = new BasesDatos();

            Log lg2 = new Log();
            int banderaBD = 0;
            string sql = "";
            try
            {
                if (codDoc.Equals("07"))
                {
                    double total_retenciones = 0;
                    //TotalImpuestosRetenidos
                    banderaBD = 7;
                    foreach (String[] tir_cima in arraylTotalImpuestosRetenciones)
                    {

                        total_retenciones += double.Parse(valida_texto_a_numero(tir_cima[4]), CultureInfo.InvariantCulture);
                    }

                    importeAPagar = total_retenciones.ToString("F2", CultureInfo.InvariantCulture);
                    importeAPagar = importeAPagar.Replace(",", "");
                }

                if (String.IsNullOrEmpty(consultarIDE(secuencial, codDoc, ptoEmi, estab, "secuencial", "codDoc", "ptoEmi", "estab", "select idComprobante from General where tipo='E' and ")))
                {
                    //Emisor
                    banderaBD = 1;
                    id_Emisor = Emisor(ruc);
                    #region "Ingresando datos del emisor"
                    if (String.IsNullOrEmpty(id_Emisor))
                    {
                        sql = @"INSERT INTO EMISOR
                    (RFCEMI,NOMEMI,nombreComercial,dirMatriz)
                    VALUES
                   (@RFCEMI,@NOMEMI,@nombreComercial,@dirMatriz)";
                        DB.Conectar();
                        DB.CrearComando(sql);
                        DB.AsignarParametroCadena("@RFCEMI", ruc);
                        DB.AsignarParametroCadena("@NOMEMI", razonSocial);
                        DB.AsignarParametroCadena("@nombreComercial", nombreComercial);
                        DB.AsignarParametroCadena("@dirMatriz", dirMatriz);
                        DB.EjecutarConsulta1();
                        DB.Desconectar();
                        id_Emisor = Emisor(ruc);
                    }
                    #endregion
                    //Receptor
                    banderaBD = 2;
                    id_Receptor = Receptor(identificacionRec);
                    #region "Ingresando o Actualizando datos del Receptor"
                    if (String.IsNullOrEmpty(id_Receptor))
                    {
                        clsLogger.Graba_Log_Info("ingresando a guardar DB paso 4");
                        XDocument XdocumentoXML = new XDocument(
                        new XElement("INSTRUCCION",
                                        new XElement("FILTRO",
                                            new XElement("opcion", "3")),
                                            new XElement("RECEPTOR",
                                                            new XElement("RFCREC", this.identificacionRec),
                                                            new XElement("NOMREC", this.razonSocialRec),
                                                            new XElement("contribuyenteEspecial", this.contribuyenteEspecial),
                                                            new XElement("obligadoContabilidad", this.obligadoContabilidad),
                                                            new XElement("tipoIdentificacionComprador", this.tipoIdentificacionRec),
                                                            new XElement("domicilio", this.domicilio),
                                                            new XElement("telefono", this.telefono),
                                                            new XElement("email", this.emails),
                                                            new XElement("direccionComprador", this.direccionRec))));
                        DB.Conectar();
                        using (var x = DB.TraerDataset("PA_Comprobantes_A_Recep", XdocumentoXML.ToString()))
                        {
                        }

                        id_Receptor = Receptor(identificacionRec);
                    }
                    else
                    {
                        String tipoIde = "", dir = "", tel = "", ema = "", CPREM = "", v_razon = "", dircomp = "";
                        DB.Conectar();
                        DB.CrearComando(@"SELECT top 1 * FROM Receptor with(nolock) WHERE IDEREC=@rfc");
                        DB.AsignarParametroCadena("@rfc", id_Receptor);
                        using (DbDataReader DR = DB.EjecutarConsulta())
                        {
                            if (DR.Read())
                            {
                                v_razon = DR[2].ToString();
                                tipoIde = DR[5].ToString();
                                ema = DR[6].ToString();
                                dir = DR[7].ToString();
                                tel = DR[8].ToString();
                                dircomp = DR[9].ToString();
                            }
                        }

                        DB.Desconectar();
                        if (v_razon == razonSocialRec && tipoIde == tipoIdentificacionComprador && ema == emails &&
                            dir == domicilio && tel == telefono && dircomp == direccionRec)
                        {
                            id_Receptor = Receptor(identificacionRec);
                        }
                        else
                        {
                            XDocument XdocumentoXML = new XDocument(
                            new XElement("INSTRUCCION",
                                            new XElement("FILTRO",
                                                            new XElement("opcion", "4")),
                                                new XElement("RECEPTOR",
                                                                new XElement("NOMREC", this.razonSocialRec),
                                                                new XElement("contribuyenteEspecial", this.contribuyenteEspecial),
                                                                new XElement("obligadoContabilidad", this.obligadoContabilidad),
                                                                new XElement("tipoIdentificacionComprador", this.tipoIdentificacionRec),
                                                                new XElement("domicilio", this.domicilio),
                                                                new XElement("telefono", this.telefono),
                                                                new XElement("email", this.emails),
                                                                new XElement("direccionComprador", this.direccionRec),
                                                                new XElement("IDEREC", this.id_Receptor))));
                            DB.Conectar();
                            using (var u = DB.TraerDataset("PA_Comprobantes_UP_Recep", XdocumentoXML.ToString()))
                            {
                            }

                            id_Receptor = Receptor(identificacionRec);
                        }
                    }
                    #endregion
                    banderaBD = 3;
                    id_EmisorExp = consultarIDE(dirEstablecimiento, "dirEstablecimientos", "select IDEDOMEMIEXP from DOMEMIEXP with(nolock) where ");
                    banderaBD = 4;
                    id_ReceptorCon = "72";
                    if (String.IsNullOrEmpty(id_EmisorExp))
                    {
                        sql = @"INSERT INTO DOMEMIEXP
                    (dirEstablecimientos)
                    VALUES
                    (@dirEstablecimientos)";
                        DB.Conectar();
                        DB.CrearComando(sql);
                        DB.AsignarParametroCadena("@dirEstablecimientos", dirEstablecimiento);
                        DB.EjecutarConsulta1();
                        DB.Desconectar();

                        id_EmisorExp = consultarIDE(dirEstablecimiento, "dirEstablecimientos", "select IDEDOMEMIEXP from DOMEMIEXP with(nolock) where ");
                    }
                    banderaBD = 5;
                    System.Collections.IEnumerator enumerator;
                    System.Collections.IEnumerator enumeratorC;
                    System.Collections.IEnumerator enumeratorRubros;
                    //Cambiar numeros a letras
                    NumerosALetras nletras = new NumerosALetras();
                    //Insertar en tabla General
                    XDocument CRE = default(XDocument);
                    DateTime FfechaEmision = DateTime.ParseExact(fechaEmision, "dd/MM/yyyy", null);
                    DateTime FfechaIniTransporte = new DateTime();
                    DateTime FfechaFinTransporte = new DateTime();
                    DateTime FfechaEmisionDocSustentoNota = new DateTime();
                    if (!fechaIniTransporte.ToString().Equals(""))
                        FfechaIniTransporte = DateTime.ParseExact(fechaIniTransporte, "dd/MM/yyyy", null);
                    if (!fechaFinTransporte.ToString().Equals(""))
                        FfechaFinTransporte = DateTime.ParseExact(fechaFinTransporte, "dd/MM/yyyy", null);
                    if (!fechaEmisionDocSustentoNota.ToString().Equals(""))
                        FfechaEmisionDocSustentoNota = DateTime.ParseExact(fechaEmisionDocSustentoNota, "dd/MM/yyyy", null);
                    XElement INSTRUCCION = new XElement("INSTRUCCION");
                    INSTRUCCION.Add(new XElement("FILTRO",
                        new XElement("opcion", (valida_duplicidad(codDoc, estab, ptoEmi, secuencial, ambiente, ruc) ? "2" : "1"))));
                    #region "Ingresando Tabla General"
                    INSTRUCCION.Add(new XElement("GENERAL",
                        new XElement("id", idComprobante),
                        new XElement("version", version),
                        new XElement("serie", guiaRemision),
                        new XElement("folio", ""),
                        new XElement("fecha", String.Format("{0:yyyyMMdd H:mm:ss}", Convert.ToDateTime(FfechaEmision).Add(DateTime.Now.TimeOfDay))),
                        new XElement("sello", ""),
                        new XElement("noCertificado", ""),
                        new XElement("subTotal", valida_texto_a_numero(totalSinImpuestos).Replace(',', '.')),
                        new XElement("total", valida_texto_a_numero(importeAPagar).Replace(',', '.')),
                        new XElement("tipoDeComprobante", codDoc),
                        new XElement("firmaSRI", firmaSRI),
                        new XElement("id_Config", id_Config),
                        new XElement("id_Empleado", id_Empleado),
                        new XElement("id_Receptor", id_Receptor),
                        new XElement("id_Emisor", id_Emisor),
                        new XElement("id_EmisorExp", id_EmisorExp),
                        new XElement("id_ReceptorCon", id_ReceptorCon),
                        new XElement("ambiente", ambiente),
                        new XElement("tipoEmision", tipoEmision),
                        new XElement("claveAcceso", claveAcceso),
                        new XElement("codDoc", codDoc),
                        new XElement("estab", estab),
                        new XElement("ptoEmi", ptoEmi),
                        new XElement("secuencial", secuencial),
                        new XElement("totalSinImpuestos", valida_texto_a_numero(totalSinImpuestos).Replace(',', '.')),
                        new XElement("totalDescuento", valida_texto_a_numero(totalDescuento).Replace(',', '.')),
                        new XElement("periodoFiscal", periodoFiscal),
                        new XElement("fechaIniTransporte", (fechaIniTransporte.ToString().Equals("") ? "" : String.Format("{0:yyyyMMdd H:mm:ss}", Convert.ToDateTime(FfechaIniTransporte)))),
                        new XElement("fechaFinTransporte", (fechaFinTransporte.ToString().Equals("") ? "" : String.Format("{0:yyyyMMdd H:mm:ss}", Convert.ToDateTime(FfechaFinTransporte)))),
                        new XElement("placa", placa),
                        new XElement("codDocModificado", codDocModificado),
                        new XElement("numDocModificado", numDocModificado),
                        new XElement("fechaEmisionDocSustento", (fechaEmisionDocSustentoNota.ToString().Equals("") ? "" : String.Format("{0:yyyyMMdd H:mm:ss}", Convert.ToDateTime(FfechaEmisionDocSustentoNota)))),
                        new XElement("valorModificacion", valida_texto_a_numero(valorModificacion.Replace(',', '.'))),
                        new XElement("moneda", moneda),
                        new XElement("propina", valida_texto_a_numero(propina).Replace(',', '.')),
                        new XElement("importeTotal", valida_texto_a_numero(importeTotal).Replace(',', '.')),
                        new XElement("motivo", motivo),
                        new XElement("subtotal12", valida_texto_a_numero(subtotal12).Replace(',', '.')),
                        new XElement("subtotal0", valida_texto_a_numero(subtotal0).Replace(',', '.')),
                        new XElement("subtotalNoSujeto", valida_texto_a_numero(subtotalNoSujeto).Replace(',', '.')),
                        new XElement("ICE", valida_texto_a_numero(ICE).Replace(',', '.')),
                        new XElement("IVA12", valida_texto_a_numero(IVA12).Replace(',', '.')),
                        new XElement("importeAPagar", valida_texto_a_numero(importeAPagar).Replace(',', '.')),
                        new XElement("estado", "0"),
                        new XElement("rise", rise),
                        new XElement("dirPartida", dirPartida),
                        new XElement("creado", "0"),
                        new XElement("codigoControl", codigoControl),
                        new XElement("termino", termino),
                        new XElement("proforma", proforma),
                        new XElement("pedido", pedido),
                        new XElement("dirEstablecimientoGuia", dirEstablecimientoGuia),
                        new XElement("cantletras", nletras.ConvertirALetras(importeAPagar.ToString(), "USD")),
                        new XElement("tipo", "E"),
                        new XElement("tipo", "E")
                        ));
                    #endregion
                    #region "Ingresando Tabla TotalCodImpuestos de registros Total de Impuesto y Total de Impuestos Retenidos"
                    foreach (String[] ti in arraylTotalImpuestos)
                    {
                        DateTime FfechaEmisionDocSustento = new DateTime();
                        if (!String.IsNullOrEmpty(fechaEmisionDocSustento))
                            FfechaEmisionDocSustento = DateTime.ParseExact(fechaEmisionDocSustento, "dd/MM/yyyy", null);
                        INSTRUCCION.Add(new XElement("TOTAL_COD_IMPUESTOS",
                            new XElement("codigo", ti[0].ToString()),
                            new XElement("codigoPorcentaje", ti[1].ToString()),
                            new XElement("baseImponible", valida_texto_a_numero(ti[2].ToString().Replace(',', '.'))),
                            new XElement("tarifa", ((ti[0].Equals("2") && ti[1].Equals("6")) ? "0.00" : valida_texto_a_numero(ti[3].ToString().Replace(',', '.')))),
                            new XElement("valor", valida_texto_a_numero(ti[4].ToString().Replace(',', '.'))),
                            new XElement("porcentajeRetener", valida_texto_a_numero(ti[6].Replace(',', '.'))),
                            new XElement("codDocSustento", codDocSustento),
                            new XElement("numDocSustento", numDocSustento),
                            new XElement("fechaEmisionDocSustento", (String.IsNullOrEmpty(fechaEmisionDocSustento) ? "" : String.Format("{0:yyyyMMdd H:mm:ss}", Convert.ToDateTime(FfechaEmisionDocSustento))))
                            ));
                    }

                    foreach (String[] ti in arraylTotalConImpuestos)
                    {
                        INSTRUCCION.Add(new XElement("TOTAL_COD_IMPUESTOS_2",
                            new XElement("codigo", ti[2].ToString()),
                            new XElement("codigoPorcentaje", ti[3].ToString()),
                            new XElement("nombre", ti[0].ToString()),
                            new XElement("valor", ti[1].ToString())
                            ));
                    }

                    if (codDoc.Equals("07") & (version.Equals("2.0.0") || version.Equals("2.0")))
                    {
                        foreach (String[] tir in arraylTotalImpuestosRetenciones)
                        {
                            DateTime FfechaEmisionDocSustento = new DateTime();
                            if (!String.IsNullOrEmpty(tir[7]))
                                FfechaEmisionDocSustento = DateTime.ParseExact(tir[7], "dd/MM/yyyy", null);
                            INSTRUCCION.Add(new XElement("TOTAL_COD_IMPUESTOS",
                                new XElement("idDocSustento", tir[13].ToString()),
                                new XElement("codigo", tir[0].ToString()),
                                new XElement("codigoPorcentaje", tir[1].ToString()),
                                new XElement("baseImponible", valida_texto_a_numero(tir[2].ToString().Replace(',', '.'))),
                                new XElement("tarifa", "0.00"),
                                new XElement("valor", valida_texto_a_numero(tir[4].ToString().Replace(',', '.'))),
                                new XElement("porcentajeRetener", valida_texto_a_numero(tir[3].Replace(',', '.'))),
                                new XElement("codDocSustento", tir[5]),
                                new XElement("numDocSustento", tir[6]),
                                new XElement("fechaEmisionDocSustento", (String.IsNullOrEmpty(tir[7]) ? "" : String.Format("{0:yyyyMMdd H:mm:ss}", Convert.ToDateTime(FfechaEmisionDocSustento)))),
                                new XElement("fechaPagoDiv", codDoc == "07" && (version == "2.0.0" || version == "2.0") ? tir[8] : ""),
                                new XElement("imRentaSoc", codDoc == "07" && (version == "2.0.0" || version == "2.0") ? tir[9] : ""),
                                new XElement("ejerFisUtDiv", codDoc == "07" && (version == "2.0.0" || version == "2.0") ? tir[10] : ""),
                                new XElement("NumCajBan", codDoc == "07" && (version == "2.0.0" || version == "2.0") ? tir[11] : ""),
                                new XElement("PrecCajBan", codDoc == "07" && (version == "2.0.0" || version == "2.0") ? tir[12] : "")
                                ));
                        }
                    }

                    else
                    {
                        foreach (String[] tir in arraylTotalImpuestosRetenciones)
                        {
                            DateTime FfechaEmisionDocSustento = new DateTime();
                            if (!String.IsNullOrEmpty(tir[7]))
                                FfechaEmisionDocSustento = DateTime.ParseExact(tir[7], "dd/MM/yyyy", null);
                            INSTRUCCION.Add(new XElement("TOTAL_COD_IMPUESTOS",
                                new XElement("idDocSustento", ""),
                                new XElement("codigo", tir[0].ToString()),
                                new XElement("codigoPorcentaje", tir[1].ToString()),
                                new XElement("baseImponible", valida_texto_a_numero(tir[2].ToString())),
                                new XElement("tarifa", "0.00"),
                                new XElement("valor", valida_texto_a_numero(tir[4].ToString())),
                                new XElement("porcentajeRetener", valida_texto_a_numero(tir[3])),
                                new XElement("codDocSustento", tir[5]),
                                new XElement("numDocSustento", tir[6]),
                                new XElement("fechaEmisionDocSustento", (String.IsNullOrEmpty(tir[7]) ? "" : String.Format("{0:yyyyMMdd H:mm:ss}", Convert.ToDateTime(FfechaEmisionDocSustento)))),
                                new XElement("fechaPagoDiv", ""),
                                new XElement("imRentaSoc", ""),
                                new XElement("ejerFisUtDiv", ""),
                                new XElement("NumCajBan", ""),
                                new XElement("PrecCajBan", "")
                                ));
                        }
                    }
                    #endregion

                    #region "Ingresando Tablas DocsSustentos de Retenciones v2"
                    if (codDoc == "07" && (version == "2.0.0" || version == "2.0"))
                    {
                        foreach (String[] tir in arraylDoscsSutentos)
                        {
                            INSTRUCCION.Add(new XElement("DOC_SUSTENTO",
                                new XElement("idDocSustento", tir[0]),
                                new XElement("codSustento", tir[1]),
                                new XElement("codDocSustento", tir[2]),
                                new XElement("numDocSustento", tir[3]),
                                new XElement("fechaEmisionDocSustento", tir[4]),
                                new XElement("fechaRegistroContable", tir[5]),
                                new XElement("numAutDocSustento", tir[6]),
                                new XElement("pagoLocExt", tir[7]),
                                new XElement("tipoRegi", tir[8]),
                                new XElement("paisEfecPago", tir[9]),
                                new XElement("aplicConvDobTrib", tir[10]),
                                new XElement("pagExtSujRetNorLeg", tir[11]),
                                new XElement("pagoRegFis", tir[12]),
                                new XElement("totalComprobantesReembolso", valida_texto_a_numero(tir[13])),
                                new XElement("totalBaseImponibleReembolso", valida_texto_a_numero(tir[14])),
                                new XElement("totalImpuestoReembolso", valida_texto_a_numero(tir[15])),
                                new XElement("totalSinImpuestos", valida_texto_a_numero(tir[16])),
                                new XElement("importeTotal", valida_texto_a_numero(tir[17]))
                                ));
                        }

                        foreach (String[] tir in arraylImpuestosDetalles)
                        {
                            INSTRUCCION.Add(new XElement("DOC_SUSTENTO_IMPUESTO",
                                new XElement("idDocSustento", tir[0]),
                                new XElement("codImpuestoDocSustento", tir[1]),
                                new XElement("codigoPorcentaje", tir[2]),
                                new XElement("baseImponible", valida_texto_a_numero(tir[3])),
                                new XElement("tarifa", tir[4]),
                                new XElement("valorImpuesto", valida_texto_a_numero(tir[5])),
                                new XElement("impuestotipoImpuesto", tir[6])
                                ));
                        }

                        foreach (String[] tir in arraylReembolsosRetenciones)
                        {
                            INSTRUCCION.Add(new XElement("DOC_SUSTENTO_REEMBOLSO",
                                new XElement("idDocSustento", tir[0]),
                                new XElement("idReembolsos", tir[1]),
                                new XElement("tipoIdentificacionProveedorReembolso", tir[2]),
                                new XElement("identificacionProveedorReembolso", tir[3]),
                                new XElement("codPaisPagoProveedorReembolso", tir[4]),
                                new XElement("tipoProveedorReembolso", tir[5]),
                                new XElement("codDocReembolso", tir[6]),
                                new XElement("estabDocReembolso", tir[7]),
                                new XElement("ptoEmiDocReembolso", tir[8]),
                                new XElement("secuencialDocReembolso", tir[9]),
                                new XElement("fechaEmisionDocReembolso", tir[10]),
                                new XElement("numeroAutorizacionDocReemb", tir[11])
                                ));
                        }

                        foreach (String[] tir in arraylImpuestosReembolsosRet)
                        {
                            INSTRUCCION.Add(new XElement("DOC_SUSTENTO_REEMBOLSO_IMP",
                                new XElement("idDocSustento", tir[7]),
                                new XElement("idReembolsos", tir[0]),
                                new XElement("codigo", tir[1]),
                                new XElement("codigoPorcentaje", tir[2]),
                                new XElement("tarifa", tir[3]),
                                new XElement("baseImponibleReembolso", valida_texto_a_numero(tir[4])),
                                new XElement("impuestoReembolso", valida_texto_a_numero(tir[5])),
                                new XElement("impuestotipoImpuesto", tir[6])
                                ));
                        }

                        foreach (String[] tir in arraylPagos)
                        {
                            INSTRUCCION.Add(new XElement("DOC_SUSTENTO_PAGO",
                                new XElement("idDocSustento", tir[0]),
                                new XElement("formaPago", tir[1]),
                                new XElement("total", valida_texto_a_numero(tir[2]))
                                ));
                        }
                    }
                    #endregion
                    int contador_Destinatarios = 0;
                    int contador_Detalles = 0;
                    #region CompensacionSolidaria
                    if (this.arraylCompensacion != null)
                    {
                        enumeratorC = this.arraylCompensacion.GetEnumerator();
                        try
                        {
                            while (enumeratorC.MoveNext())
                            {
                                string[] arrayC = (string[])enumeratorC.Current;
                                INSTRUCCION.Add(new XElement("Compensacion", new object[]
                                {
                                    new XElement("codigo", arrayC[0].ToString()),
                                    new XElement("tarifa", arrayC[1].ToString()),
                                    new XElement("valor", this.valida_texto_a_numero(arrayC[2].ToString()))
                                }));
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
                    }
                    #endregion

                    //PMONCAYO 20200813 Etiqueta exportacion (INICIO)
                    #region "Exportacion"
                    if (!string.IsNullOrEmpty(this.comercioExterior))
                    {
                        if (!comercioExterior.Equals(""))
                        {
                            INSTRUCCION.Add(new XElement("GENERALEXPORTACION", new object[]
                        {
                                new XElement("comercioExterior", this.comercioExterior),
                                new XElement("incoTermFactura", this.incoTermFactura),
                                new XElement("lugarIncoTerm", this.lugarIncoTerm),
                                new XElement("paisOrigen", this.valida_texto_a_numero(this.paisOrigen)),
                                new XElement("puertoEmbarque", this.puertoEmbarque),
                                new XElement("puertoDestino", this.puertoDestino),
                                new XElement("paisDestino", this.valida_texto_a_numero(this.paisDestino)),
                                new XElement("paisAdquisicion", this.valida_texto_a_numero(this.paisAdquisicion)),
                                new XElement("dirComprador", this.direccionComprador),
                                new XElement("incoTermTotalSinImpuestos", this.incoTermTotalSinImpuestos),
                                new XElement("fleteInternacional", this.valida_texto_a_numero(this.fleteInternacional)),
                                new XElement("seguroInternacional", this.valida_texto_a_numero(this.seguroInternacional)),
                                new XElement("gastosAduaneros", this.valida_texto_a_numero(this.gastosAduaneros)),
                                new XElement("gastosTransporteOtros", this.valida_texto_a_numero(this.gastosTransporteOtros))
                        }));
                        }
                    }
                    #endregion
                    //PMONCAYO 20200813 Etiqueta exportacion (FIN)

                    #region pagos
                    if (!codDoc.Equals("07") & !version.Equals("2.0.0"))
                    {
                        if (this.arraylPagos != null)
                        {
                            enumerator = this.arraylPagos.GetEnumerator();
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    string[] array5 = (string[])enumerator.Current;
                                    INSTRUCCION.Add(new XElement("Pago", new object[]
                                    {
                                        new XElement("formaPago", array5[0]),
                                        new XElement("total", this.valida_texto_a_numero(array5[1])),
                                        new XElement("plazo", this.valida_texto_a_numero(array5[2])),
                                        new XElement("unidadTiempo", array5[3])
                                    }));
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
                        }
                    }

                    #endregion
                    #region otros rubros
                    if (this.arraylRubros != null)
                    {
                        enumeratorRubros = this.arraylRubros.GetEnumerator();
                        try
                        {
                            while (enumeratorRubros.MoveNext())
                            {
                                string[] array5 = (string[])enumeratorRubros.Current;
                                INSTRUCCION.Add(new XElement("rubro", new object[]
                                {
                                    new XElement("concepto", array5[0]),
                                    new XElement("total", this.valida_texto_a_numero(array5[1]))
                                }));
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
                    }
                    #endregion
                    #region "Ingresando tabla en la cual se ingresa los destinatarios que esta relacionada con el detalle y detalle adicional"
                    if (codDoc.Equals("06"))
                    {
                        foreach (String[] dest in arraylDestinatarios)
                        {
                            XElement DESTINATARIOSElement = new XElement("DESTINATARIOS");
                            contador_Destinatarios++;
                            DESTINATARIOSElement.Add(new XElement("identificacionDestinatario", dest[0]),
                                new XElement("razonSocialDestinatario", dest[1]),
                                new XElement("dirDestinatario", dest[2]),
                                new XElement("motivoTraslado", dest[3]),
                                new XElement("docAduaneroUnico", dest[4]),
                                new XElement("codEstabDestino", dest[5]),
                                new XElement("ruta", dest[6]),
                                new XElement("codDocSustento", dest[7]),
                                new XElement("numDocSustento", dest[8]),
                                new XElement("numAutDocSustento", dest[9]),
                                new XElement("fechaEmisionDocSustento", (verificarFecha(dest[10].ToString(), 1).ToString().Equals("") ? "" : String.Format("{0:yyyyMMdd H:mm:ss}", Convert.ToDateTime(verificarFecha(dest[10].ToString(), 1))))),
                                new XElement("contador_Destinatarios", contador_Destinatarios));

                            foreach (String[] d in arraylDetalles)
                            {
                                if (dest[11].ToString().Equals(d[8].ToString())) //Verifica el codigo del detalle
                                {
                                    XElement DETALLESElement = new XElement("DETALLES");
                                    contador_Detalles++;
                                    DETALLESElement.Add(
                                        new XElement("codigoPrincipal", d[0].ToString()),
                                        new XElement("codigoAuxiliar", d[1].ToString()),
                                        new XElement("descripcion", d[2].ToString()),
                                        new XElement("cantidad", d[3].ToString()),
                                        new XElement("precioUnitario", "0"),
                                        new XElement("descuento", "0"),
                                        new XElement("precioTotalSinImpuestos", "0"),
                                        new XElement("contador_Detalles", contador_Detalles),
                                        new XElement("contador_Destinatarios", contador_Destinatarios),
                                        new XElement("item", contador_Detalles));
                                    foreach (String[] da in arraylDetallesAdicionales)
                                    {
                                        if (d[7].ToString().Equals(da[3].ToString()))
                                        {
                                            DETALLESElement.Add(new XElement("DETALLES_ADICIONALES",
                                                    new XElement("nombre", da[0].ToString()),
                                                    new XElement("valor", da[1].ToString()),
                                                    new XElement("contador_Detalles", contador_Detalles)));
                                        }
                                    }
                                    DESTINATARIOSElement.Add(DETALLESElement);
                                }
                            }
                            INSTRUCCION.Add(DESTINATARIOSElement);
                        }
                        arraylDetalles = new ArrayList();
                    }
                    #endregion

                    #region "Ingresando tabla detalle en la cual se ingresa los detalles que estan relacionado con Impuestos Detalles y detalle adicional"
                    contador_Detalles = 0;
                    foreach (String[] d in arraylDetalles)
                    {
                        contador_Detalles++;
                        XElement DETALLESElement = new XElement("DETALLES");
                        DETALLESElement.Add(new XElement("codigoPrincipal", d[0].ToString()),
                            new XElement("codigoAuxiliar", d[1].ToString()),
                            new XElement("descripcion", d[2].ToString()),
                            new XElement("cantidad", d[3].ToString()),
                            new XElement("precioUnitario", valida_texto_a_numero(d[4].ToString().Replace(',', '.'))),
                            new XElement("descuento", valida_texto_a_numero(d[5].ToString().Replace(',', '.'))),
                            new XElement("precioTotalSinImpuestos", valida_texto_a_numero(d[6].ToString().Replace(',', '.'))),
                            new XElement("contador_Detalles", contador_Detalles),
                            new XElement("item", contador_Detalles));
                        foreach (String[] id in arraylImpuestosDetalles)
                        {
                            if (d[7].ToString().Equals(id[7].ToString())) //Verifica el codigo del detalle
                            {
                                DETALLESElement.Add(new XElement("IMPUESTOS_DETALLES",
                                    new XElement("codigo", id[0].ToString()),
                                    new XElement("codigoPorcentaje", id[1].ToString()),
                                    new XElement("baseImponible", valida_texto_a_numero(id[2].ToString().Replace(',', '.'))),
                                    new XElement("tarifa", valida_texto_a_numero(id[3].ToString().Replace(',', '.'))),
                                    new XElement("valor", valida_texto_a_numero(id[4].ToString().Replace(',', '.'))),
                                    new XElement("tipo", id[6].ToString()),
                                    new XElement("contador_Detalles", contador_Detalles)
                                    ));
                            }
                        }
                        foreach (String[] da in arraylDetallesAdicionales)
                        {
                            if (d[7].ToString().Equals(da[3].ToString())) //Verifica el codigo del detalle
                            {
                                DETALLESElement.Add(new XElement("DETALLES_ADICIONALES",
                                    new XElement("nombre", da[0].ToString()),
                                    new XElement("valor", da[1].ToString()),
                                    new XElement("contador_Detalles", contador_Detalles)));
                            }
                        }
                        INSTRUCCION.Add(DETALLESElement);
                    }
                    #endregion
                    #region "Ingresando tabla Informacion Adicional"
                    if (arraylInfoAdicionales != null)
                        foreach (String[] id in arraylInfoAdicionales)
                        {
                            INSTRUCCION.Add(new XElement("INFO_ADICIONAL",
                                new XElement("nombre", id[0].ToString().Trim()),
                                new XElement("valor", id[1].ToString().Trim())));
                        }
                    #endregion
                    #region "Ingresando tabla de Archivos"
                    INSTRUCCION.Add(new XElement("ARCHIVOS",
                                new XElement("PDFARC", "docus/" + codigoControl + ".pdf"),
                                new XElement("XMLARC", "docus/" + codigoControl + ".xml")));
                    #endregion
                    CRE = new XDocument(INSTRUCCION);
                    DB.Conectar();
                    DB.CrearComandoProcedimiento("PA_LOG_USER_2");
                    DB.AsignarParametroProcedimiento("@id_Receptor", System.Data.DbType.Int32, id_Receptor);
                    DB.AsignarParametroProcedimiento("@id_Emisor", System.Data.DbType.Int32, id_Emisor);
                    DB.AsignarParametroProcedimiento("@estab", System.Data.DbType.String, estab);
                    DB.AsignarParametroProcedimiento("@P_ESTADO", System.Data.DbType.String, "2");
                    DB.AsignarParametroProcedimiento("@P_clave", System.Data.DbType.String, cs.encriptar(identificacionRec, "CIMAIT"));
                    using (DbDataReader DR3_1 = DB.EjecutarConsulta())
                    {
                        if (DR3_1.Read())
                        {
                            //id_Comprobante = DR3_1[0].ToString();
                        }
                    }

                    DB.Desconectar();
                    DB.Conectar();
                    using (DataSet ds = DB.TraerDataset("PA_Comprobantes_AM", CRE.ToString()))
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            id_Comprobante = ds.Tables[0].Rows[0][0].ToString();
                        }
                    }
                    DB.Desconectar();
                    banderaBD = 13;

                    string imagenCodigoBarras;

                    if (codDoc.Equals("06"))
                    {
                        imagenCodigoBarras = estab + "-" + ptoEmi + "-" + secuencial;
                    }
                    else
                    {
                        imagenCodigoBarras = claveAcceso;
                    }

                    System.Drawing.Graphics g = Graphics.FromImage(new Bitmap(1, 1));
                    imgBar = ImageToByte2(GenCode128.Code128Rendering.MakeBarcodeImage(imagenCodigoBarras, 3, true));
                    DB.Conectar();
                    DB.CrearComandoProcedimiento("PA_CodigoBarras");
                    DB.AsignarParametroProcedimiento("@idComprobante", System.Data.DbType.String, id_Comprobante);
                    DB.AsignarParametroProcedimiento("@codigoBarras", System.Data.DbType.Binary, imgBar);
                    DB.EjecutarConsulta1();
                    DB.Desconectar();
                    return true;
                    #region  "Registros originales comentados"
                    //                    sql = @" set dateformat dmy INSERT INTO GENERAL
                    //                    (id,version,serie,folio,fecha,sello,noCertificado,subTotal,total
                    //                    ,tipoDeComprobante,firmaSRI,id_Config,id_Empleado,id_Receptor,id_Emisor,id_EmisorExp,
                    //                    id_ReceptorCon,ambiente,tipoEmision,claveAcceso,codDoc,estab,ptoEmi,secuencial,
                    //                    totalSinImpuestos,totalDescuento,periodoFiscal,fechaIniTransporte,fechaFinTransporte,
                    //                    placa,codDocModificado,numDocModificado,fechaEmisionDocSustento,valorModificacion,moneda,
                    //                    propina,importeTotal,motivo,subtotal12,subtotal0,subtotalNoSujeto,ICE,IVA12,importeAPagar,
                    //                    estado,rise,dirPartida,creado,codigoControl,termino, proforma, pedido, cantletras, tipo)
                    //                    VALUES
                    //                    (@id,@version,@serie,@folio,@fecha,@sello,@noCertificado,@subTotal,@total,
                    //                    @tipoDeComprobante,@firmaSRI,@id_Config,@id_Empleado,@id_Receptor,@id_Emisor,@id_EmisorExp,
                    //                    @id_ReceptorCon,@ambiente,@tipoEmision,@claveAcceso,@codDoc,@estab,@ptoEmi,@secuencial,
                    //                    @totalSinImpuestos,@totalDescuento,@periodoFiscal,@fechaIniTransporte,@fechaFinTransporte,
                    //                    @placa,@codDocModificado,@numDocModificado,@fechaEmisionDocSustento,@valorModificacion,@moneda,
                    //                    @propina,@importeTotal,@motivo,@subtotal12,@subtotal0,@subtotalNoSujeto,@ICE,@IVA12,@importeAPagar,
                    //                    @estado,@rise,@dirPartida,@creado,@codigoControl,@termino, @proforma, @pedido, @cantletras, @tipo)";
                    //                    DB.Conectar();
                    //                    DB.CrearComando(sql);
                    //                    DB.AsignarParametroCadena("@id", idComprobante);
                    //                    DB.AsignarParametroCadena("@version", version);
                    //                    DB.AsignarParametroCadena("@serie", "");
                    //                    DB.AsignarParametroCadena("@folio", "");
                    //                    DB.AsignarParametroCadena("@fecha", fechaEmision);
                    //                    DB.AsignarParametroCadena("@sello", "");
                    //                    DB.AsignarParametroCadena("@noCertificado", "");
                    //                    DB.AsignarParametroCadena("@subTotal", totalSinImpuestos);
                    //                    DB.AsignarParametroCadena("@total", importeAPagar);
                    //                    DB.AsignarParametroCadena("@tipoDeComprobante", codDoc);
                    //                    DB.AsignarParametroCadena("@firmaSRI", firmaSRI);
                    //                    DB.AsignarParametroCadena("@id_Config", id_Config);
                    //                    DB.AsignarParametroCadena("@id_Empleado", id_Empleado);
                    //                    DB.AsignarParametroCadena("@id_Receptor", id_Receptor);
                    //                    DB.AsignarParametroCadena("@id_Emisor", id_Emisor);
                    //                    DB.AsignarParametroCadena("@id_EmisorExp", id_EmisorExp);
                    //                    DB.AsignarParametroCadena("@id_ReceptorCon", id_ReceptorCon);
                    //                    DB.AsignarParametroCadena("@ambiente", ambiente);
                    //                    DB.AsignarParametroCadena("@tipoEmision", tipoEmision);
                    //                    DB.AsignarParametroCadena("@claveAcceso", claveAcceso);
                    //                    DB.AsignarParametroCadena("@codDoc", codDoc);
                    //                    DB.AsignarParametroCadena("@estab", estab);
                    //                    DB.AsignarParametroCadena("@ptoEmi", ptoEmi);
                    //                    DB.AsignarParametroCadena("@secuencial", secuencial);
                    //                    DB.AsignarParametroCadena("@totalSinImpuestos", totalSinImpuestos);
                    //                    DB.AsignarParametroCadena("@totalDescuento", totalDescuento);
                    //                    DB.AsignarParametroCadena("@periodoFiscal", periodoFiscal);
                    //                    DB.AsignarParametroCadena("@fechaIniTransporte", fechaIniTransporte);
                    //                    DB.AsignarParametroCadena("@fechaFinTransporte", fechaFinTransporte);
                    //                    DB.AsignarParametroCadena("@placa", placa);
                    //                    DB.AsignarParametroCadena("@codDocModificado", codDocModificado);
                    //                    DB.AsignarParametroCadena("@numDocModificado", numDocModificado);
                    //                    DB.AsignarParametroCadena("@fechaEmisionDocSustento", fechaEmisionDocSustentoNota);
                    //                    DB.AsignarParametroCadena("@valorModificacion", valorModificacion);
                    //                    DB.AsignarParametroCadena("@moneda", moneda);
                    //                    DB.AsignarParametroCadena("@propina", propina);
                    //                    DB.AsignarParametroCadena("@importeTotal", importeTotal);
                    //                    DB.AsignarParametroCadena("@motivo", motivo);
                    //                    DB.AsignarParametroCadena("@subtotal12", subtotal12);
                    //                    DB.AsignarParametroCadena("@subtotal0", subtotal0);
                    //                    DB.AsignarParametroCadena("@subtotalNoSujeto", subtotalNoSujeto);
                    //                    DB.AsignarParametroCadena("@ICE", ICE);
                    //                    DB.AsignarParametroCadena("@IVA12", IVA12);
                    //                    DB.AsignarParametroCadena("@importeAPagar", importeAPagar);
                    //                    DB.AsignarParametroCadena("@estado", "0");
                    //                    DB.AsignarParametroCadena("@rise", rise);
                    //                    DB.AsignarParametroCadena("@dirPartida", dirPartida);
                    //                    DB.AsignarParametroCadena("@creado", "0");
                    //                    DB.AsignarParametroCadena("@codigoControl", codigoControl);//codDoc + estab + ptoEmi + Convert.ToDateTime(fechaEmision).ToString("yyyyMMddHHmmss"));
                    //                    DB.AsignarParametroCadena("@termino", termino);
                    //                    DB.AsignarParametroCadena("@proforma", proforma);
                    //                    DB.AsignarParametroCadena("@pedido", pedido);
                    //                    DB.AsignarParametroCadena("@cantletras", nletras.ConvertirALetras(importeAPagar.ToString(), "USD"));
                    //                    DB.AsignarParametroCadena("@tipo", "E");
                    //                    DB.EjecutarConsulta1();
                    //                    DB.Desconectar();

                    //                    id_Comprobante = consultarIDE(secuencial, codDoc, ptoEmi, estab, "secuencial", "codDoc", "ptoEmi", "estab", "select idComprobante from General where tipo='E' and "); //consultarIDE(claveAcceso, "claveAcceso", "select idComprobante from General where ");

                    //                    //Total Con Impuestos
                    //                    banderaBD = 6;
                    //                    foreach (String[] ti in arraylTotalImpuestos)
                    //                    {
                    //                        //TIR | codigo | codigoRetencion | baseImponible | porcentajeRetener | valorRetenido | codDocSustento | numDocSustento | fechaEmisionDocSustento |
                    //                        sql = @" set dateformat dmy INSERT INTO TotalConImpuestos
                    //                    (codigo,codigoPorcentaje,baseImponible,tarifa,valor,porcentajeRetener,
                    //                    codDocSustento,numDocSustento,fechaEmisionDocSustento,id_Comprobante)
                    //                    VALUES
                    //                    (@codigo,@codigoPorcentaje,@baseImponible,@tarifa,@valor,@porcentajeRetener,
                    //                    @codDocSustento,@numDocSustento,@fechaEmisionDocSustento,@id_Comprobante)";
                    //                        DB.Conectar();
                    //                        DB.CrearComando(sql);
                    //                        DB.AsignarParametroCadena("@codigo", ti[0].ToString());
                    //                        DB.AsignarParametroCadena("@codigoPorcentaje", ti[1].ToString());
                    //                        DB.AsignarParametroCadena("@baseImponible", ti[2].ToString());
                    //                        if (ti[0].Equals("2") && ti[1].Equals("6"))
                    //                        {
                    //                            DB.AsignarParametroCadena("@tarifa", "0.00");
                    //                        }
                    //                        else
                    //                        {
                    //                            DB.AsignarParametroCadena("@tarifa", ti[3].ToString());
                    //                        }
                    //                        DB.AsignarParametroCadena("@valor", ti[4].ToString());
                    //                        DB.AsignarParametroCadena("@porcentajeRetener", ti[6]);
                    //                        DB.AsignarParametroCadena("@codDocSustento", codDocSustento);
                    //                        DB.AsignarParametroCadena("@numDocSustento", numDocSustento);
                    //                        DB.AsignarParametroCadena("@fechaEmisionDocSustento", fechaEmisionDocSustento);
                    //                        DB.AsignarParametroCadena("@id_Comprobante", id_Comprobante);
                    //                        DB.EjecutarConsulta1();
                    //                        DB.Desconectar();
                    //                    }

                    //                    //TotalImpuestosRetenidos
                    //                    banderaBD = 7;
                    //                    foreach (String[] tir in arraylTotalImpuestosRetenciones)
                    //                    {//TIR | codigo | codigoRetencion | baseImponible | porcentajeRetener | valorRetenido | codDocSustento | numDocSustento | fechaEmisionDocSustento |
                    //                        sql = @"set dateformat dmy INSERT INTO TotalConImpuestos
                    //                    (codigo,codigoPorcentaje,baseImponible,tarifa,valor,porcentajeRetener,
                    //                    codDocSustento,numDocSustento,fechaEmisionDocSustento,id_Comprobante)
                    //                    VALUES
                    //                    (@codigo,@codigoPorcentaje,@baseImponible,@tarifa,@valor,@porcentajeRetener,
                    //                    @codDocSustento,@numDocSustento,@fechaEmisionDocSustento,@id_Comprobante)";
                    //                        DB.Conectar();
                    //                        DB.CrearComando(sql);
                    //                        DB.AsignarParametroCadena("@codigo", tir[0].ToString());
                    //                        DB.AsignarParametroCadena("@codigoPorcentaje", tir[1].ToString());
                    //                        DB.AsignarParametroCadena("@baseImponible", tir[2].ToString());
                    //                        DB.AsignarParametroCadena("@tarifa", "0.00");
                    //                        DB.AsignarParametroCadena("@valor", tir[4].ToString());
                    //                        DB.AsignarParametroCadena("@porcentajeRetener", tir[3]);
                    //                        DB.AsignarParametroCadena("@codDocSustento", tir[5]);
                    //                        DB.AsignarParametroCadena("@numDocSustento", tir[6]);
                    //                        DB.AsignarParametroCadena("@fechaEmisionDocSustento", tir[7]); //Convert.ToDateTime(tir[7]).ToShortDateString());
                    //                        DB.AsignarParametroCadena("@id_Comprobante", id_Comprobante);
                    //                        DB.EjecutarConsulta1();
                    //                        DB.Desconectar();
                    //                    }

                    //                    //Destinatario
                    //                    banderaBD = 8;
                    //                    if (codDoc.Equals("06"))
                    //                    {
                    //                        foreach (String[] dest in arraylDestinatarios)
                    //                        {
                    //                            sql = @"set dateformat dmy INSERT INTO Destinatarios
                    //                    (identificacionDestinatario,razonSocialDestinatario,dirDestinatario
                    //                    ,motivoTraslado,docAduaneroUnico,codEstabDestino,ruta,codDocSustento,numDocSustento
                    //                    ,numAutDocSustento,fechaEmisionDocSustento,id_Comprobante)
                    //                    VALUES
                    //                    (@identificacionDestinatario,@razonSocialDestinatario,@dirDestinatario
                    //                    ,@motivoTraslado,@docAduaneroUnico,@codEstabDestino,@ruta,@codDocSustento,@numDocSustento
                    //                    ,@numAutDocSustento,@fechaEmisionDocSustento,@id_Comprobante)";
                    //                            //identificacionDestinatario|razonSocialDestinatario|dirDestinatario |motivoTraslado |docAduaneroUnico|codEstabDestino|
                    //                            //ruta|codDocSustento|numDocSustento|numAutDocSustento |fechaEmisionDocSustento |idDestinatario|
                    //                            DB.Conectar();
                    //                            DB.CrearComando(sql);
                    //                            DB.AsignarParametroCadena("@identificacionDestinatario", dest[0]);
                    //                            DB.AsignarParametroCadena("@razonSocialDestinatario", dest[1]);
                    //                            DB.AsignarParametroCadena("@dirDestinatario", dest[2]);
                    //                            DB.AsignarParametroCadena("@motivoTraslado", dest[3]);
                    //                            DB.AsignarParametroCadena("@docAduaneroUnico", dest[4]);
                    //                            DB.AsignarParametroCadena("@codEstabDestino", dest[5]);
                    //                            DB.AsignarParametroCadena("@ruta", dest[6]);
                    //                            DB.AsignarParametroCadena("@codDocSustento", dest[7]);
                    //                            DB.AsignarParametroCadena("@numDocSustento", dest[8]);
                    //                            DB.AsignarParametroCadena("@numAutDocSustento", dest[9]);
                    //                            DB.AsignarParametroCadena("@fechaEmisionDocSustento", verificarFecha(dest[10].ToString(), 1));
                    //                            DB.AsignarParametroCadena("@id_Comprobante", id_Comprobante);
                    //                            DB.EjecutarConsulta1();
                    //                            DB.Desconectar();
                    //                            id_Destinatario = consultarIDE(dest[0], "identificacionDestinatario", "select MAX(idDestinatario) from Destinatarios where ");
                    //                            //Detalles
                    //                            banderaBD = 9;
                    //                            foreach (String[] d in arraylDetalles)
                    //                            {
                    //                                if (dest[11].ToString().Equals(d[8].ToString())) //Verifica el codigo del detalle
                    //                                {
                    //                                    sql = @"INSERT INTO Detalles
                    //                    (codigoPrincipal,codigoAuxiliar,descripcion,cantidad,precioUnitario,
                    //                    descuento,precioTotalSinImpuestos,id_Comprobante,id_Destinatario)
                    //                    VALUES
                    //                    (@codigoPrincipal,@codigoAuxiliar,@descripcion,@cantidad,@precioUnitario,
                    //                    @descuento,@precioTotalSinImpuestos,@id_Comprobante,@id_Destinatario)";
                    //                                    DB.Conectar();
                    //                                    DB.CrearComando(sql);
                    //                                    DB.AsignarParametroCadena("@codigoPrincipal", d[0].ToString());
                    //                                    DB.AsignarParametroCadena("@codigoAuxiliar", d[1].ToString());
                    //                                    DB.AsignarParametroCadena("@descripcion", d[2].ToString());
                    //                                    DB.AsignarParametroCadena("@cantidad", d[3].ToString());
                    //                                    DB.AsignarParametroCadena("@precioUnitario", "0");
                    //                                    DB.AsignarParametroCadena("@descuento", "0");
                    //                                    DB.AsignarParametroCadena("@precioTotalSinImpuestos", "0");
                    //                                    DB.AsignarParametroCadena("@id_Comprobante", id_Comprobante);
                    //                                    DB.AsignarParametroCadena("@id_Destinatario", id_Destinatario);

                    //                                    DB.EjecutarConsulta1();
                    //                                    DB.Desconectar();
                    //                                    id_Detalles = consultarIDE(d[0].ToString(), id_Comprobante, "codigoPrincipal", "id_Comprobante", "select idDetalles from Detalles where ");

                    //                                    //Detalles Adicionales
                    //                                    banderaBD = 11;
                    //                                    foreach (String[] da in arraylDetallesAdicionales)
                    //                                    {
                    //                                        sql = @"INSERT INTO DetallesAdicionales
                    //                                          (nombre,valor,id_Detalles)
                    //                                            VALUES
                    //                                         (@nombre,@valor,@id_Detalles)";
                    //                                        if (d[7].ToString().Equals(da[3].ToString()))
                    //                                        {
                    //                                            DB.Conectar();
                    //                                            DB.CrearComando(sql);
                    //                                            DB.AsignarParametroCadena("@nombre", da[0].ToString());
                    //                                            DB.AsignarParametroCadena("@valor", da[1].ToString());
                    //                                            DB.AsignarParametroCadena("@id_Detalles", id_Detalles);
                    //                                            DB.EjecutarConsulta1();
                    //                                            DB.Desconectar();
                    //                                        }
                    //                                    }
                    //                                }
                    //                            }
                    //                        }
                    //                        arraylDetalles = new ArrayList();
                    //                    }
                    //                    Detalles
                    //                    banderaBD = 9;
                    //                    foreach (String[] d in arraylDetalles)
                    //                    {
                    //                        sql = @"INSERT INTO Detalles
                    //                    (codigoPrincipal,codigoAuxiliar,descripcion,cantidad,precioUnitario,
                    //                    descuento,precioTotalSinImpuestos,id_Comprobante,id_Destinatario, item)
                    //                    VALUES
                    //                    (@codigoPrincipal,@codigoAuxiliar,@descripcion,@cantidad,@precioUnitario,
                    //                    @descuento,@precioTotalSinImpuestos,@id_Comprobante,@id_Destinatario, @item)";
                    //                        DB.Conectar();
                    //                        DB.CrearComando(sql);
                    //                        DB.AsignarParametroCadena("@codigoPrincipal", d[0].ToString());
                    //                        DB.AsignarParametroCadena("@codigoAuxiliar", d[1].ToString());
                    //                        DB.AsignarParametroCadena("@descripcion", d[2].ToString());
                    //                        DB.AsignarParametroCadena("@cantidad", d[3].ToString());
                    //                        DB.AsignarParametroCadena("@precioUnitario", d[4].ToString());
                    //                        DB.AsignarParametroCadena("@descuento", d[5].ToString());
                    //                        DB.AsignarParametroCadena("@precioTotalSinImpuestos", d[6].ToString());
                    //                        DB.AsignarParametroCadena("@id_Comprobante", id_Comprobante);
                    //                        DB.AsignarParametroCadena("@id_Destinatario", "1");
                    //                        DB.AsignarParametroCadena("@item", d[7].ToString());
                    //                        DB.EjecutarConsulta1();
                    //                        DB.Desconectar();
                    //                        id_Detalles = consultarIDE(d[7].ToString(), id_Comprobante, "item", "id_Comprobante", "select idDetalles from Detalles where ");

                    //                        //Impuestos Detalles
                    //                        banderaBD = 10;
                    //                        foreach (String[] id in arraylImpuestosDetalles)
                    //                        {
                    //                            sql = @"INSERT INTO ImpuestosDetalles
                    //                    (codigo,codigoPorcentaje,baseImponible,tarifa,valor,id_Detalles,tipo)
                    //                    VALUES
                    //                    (@codigo,@codigoPorcentaje,@baseImponible,@tarifa,@valor,@id_Detalles,@tipo)";
                    //                            if (d[7].ToString().Equals(id[7].ToString())) //Verifica el codigo del detalle
                    //                            {
                    //                                DB.Conectar();
                    //                                DB.CrearComando(sql);
                    //                                DB.AsignarParametroCadena("@codigo", id[0].ToString());
                    //                                DB.AsignarParametroCadena("@codigoPorcentaje", id[1].ToString());
                    //                                DB.AsignarParametroCadena("@baseImponible", id[2].ToString());
                    //                                DB.AsignarParametroCadena("@tarifa", id[3].ToString());
                    //                                DB.AsignarParametroCadena("@valor", id[4].ToString());
                    //                                DB.AsignarParametroCadena("@id_Detalles", id_Detalles);
                    //                                DB.AsignarParametroCadena("@tipo", id[6].ToString());
                    //                                DB.EjecutarConsulta1();
                    //                                DB.Desconectar();
                    //                            }
                    //                        }

                    //                        //Detalles Adicionales
                    //                        banderaBD = 11;
                    //                        foreach (String[] da in arraylDetallesAdicionales)
                    //                        {
                    //                            sql = @"INSERT INTO DetallesAdicionales
                    //                    (nombre,valor,id_Detalles)
                    //                    VALUES
                    //                    (@nombre,@valor,@id_Detalles)";
                    //                            if (d[7].ToString().Equals(da[3].ToString())) //Verifica el codigo del detalle
                    //                            {
                    //                                DB.Conectar();
                    //                                DB.CrearComando(sql);
                    //                                DB.AsignarParametroCadena("@nombre", da[0].ToString());
                    //                                DB.AsignarParametroCadena("@valor", da[1].ToString());
                    //                                DB.AsignarParametroCadena("@id_Detalles", id_Detalles);
                    //                                DB.EjecutarConsulta1();
                    //                                DB.Desconectar();
                    //                            }
                    //                        }
                    //                    }
                    //                    //Informacion Adicional
                    //                    banderaBD = 12;
                    //                    foreach (String[] id in arraylInfoAdicionales)
                    //                    {
                    //                        sql = @"INSERT INTO InfoAdicional
                    //                    (nombre,valor,id_Comprobante)
                    //                    VALUES
                    //                    (@nombre,@valor,@id_Comprobante)";
                    //                        DB.Conectar();
                    //                        DB.CrearComando(sql);
                    //                        DB.AsignarParametroCadena("@nombre", id[0].ToString());
                    //                        DB.AsignarParametroCadena("@valor", id[1].ToString());
                    //                        DB.AsignarParametroCadena("@id_Comprobante", id_Comprobante);
                    //                        DB.EjecutarConsulta1();
                    //                        DB.Desconectar();
                    //                    }

                    //                    banderaBD = 13;
                    //                    System.Drawing.Graphics g = Graphics.FromImage(new Bitmap(1, 1));
                    //                    //imgBar = cCode.getBytCodigo(bgCode128.DrawCode128(g, claveAcceso, 0, 0));
                    //                    imgBar = ImageToByte2(GenCode128.Code128Rendering.MakeBarcodeImage(claveAcceso, 3, true));
                    //                    //bgCode39 = new Code39.Code39(claveAcceso);
                    //                    //imgBar = cCode.getBytCodigo((Image)bgCode39.Paint());

                    //                    // imgBar = generarCodigoBarrasGS1128(claveAcceso);

                    //                    DB.Conectar();
                    //                    DB.CrearComandoProcedimiento("PA_CodigoBarras");
                    //                    DB.AsignarParametroProcedimiento("@idComprobante", System.Data.DbType.String, id_Comprobante);
                    //                    DB.AsignarParametroProcedimiento("@codigoBarras", System.Data.DbType.Binary, imgBar);
                    //                    DB.EjecutarConsulta1();
                    //                    DB.Desconectar();

                    //                    banderaBD = 14;
                    //                    DB.Conectar();
                    //                    DB.CrearComando(@"insert into Archivos 
                    //                                (PDFARC,XMLARC,IDEFAC) 
                    //                                values
                    //                                (@PDFARC,@XMLARC,@IDEFAC)");
                    //                    DB.AsignarParametroCadena("@PDFARC", "docus/" + codigoControl + ".pdf");
                    //                    DB.AsignarParametroCadena("@XMLARC", "docus/" + codigoControl + ".xml");
                    //                    DB.AsignarParametroCadena("@IDEFAC", id_Comprobante);
                    //                    DB.EjecutarConsulta1();
                    //                    DB.Desconectar();
                    //                    return true;
                    #endregion
                }
                else
                {
                    msjAux = estab + "-" + ptoEmi + "-" + secuencial;
                    msj = "Folio: " + msjAux;
                    log.mensajesLog("EM003", msj, "", "", codigoControl, msjAux);
                    return false;
                }
            }
            catch (Exception sqlex)
            {
                msjAux = DB.comando.CommandText;
                DB.Desconectar();
                clsLogger.Graba_Log_Error(sqlex.Message);
                msj = "No se pudo Guardar. " + msjGuardarBD(banderaBD);
                msjT = sqlex.Message;
                log.mensajesLog("BD002", msj, msjT, "", codigoControl, msjAux);
                return false;
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
                p_valor.Trim();
            }
            return p_valor;
        }

        //Validar si el secuencial ya fue procesado
        private Boolean valida_duplicidad(string p_codDoc, string p_estab, string p_ptoEmi, string p_secuencial, string p_ambiente, string p_ruc)
        {
            Boolean rpt = false;
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando("select idComprobante ,ISNULL(tipo,'') tipo from GENERAL with(nolock) inner join EMISOR with(nolock) on GENERAL.id_Emisor = EMISOR.IDEEMI where codDoc = @p_codDoc and estab = @p_estab and ptoEmi = @p_ptoEmi and secuencial = @p_secuencial and RFCEMI = @p_ruc  and ambiente = @p_ambiente ");
                DB.AsignarParametroCadena("@p_codDoc", p_codDoc);
                DB.AsignarParametroCadena("@p_estab", p_estab);
                DB.AsignarParametroCadena("@p_ptoEmi", p_ptoEmi);
                DB.AsignarParametroCadena("@p_secuencial", p_secuencial);
                DB.AsignarParametroCadena("@p_ruc", p_ruc);
                DB.AsignarParametroCadena("@p_ambiente", p_ambiente);
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    if (DR3.Read())
                    {
                        rpt = true;
                    }
                }

                DB.Desconectar();

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

        public String Receptor(string ruc)
        {
            string ide;
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando("select max(IDEREC) from Receptor with(nolock) where RFCREC=@RUC ");
                DB.AsignarParametroCadena("@RUC", ruc);
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        ide = DR[0].ToString();
                        DB.Desconectar();
                        return ide.Trim();
                    }
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
            return "";
        }

        public String Emisor(string ruc)
        {
            string ide;
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando("select IDEEMI from Emisor with(nolock) where RFCEMI=@RUC");
                DB.AsignarParametroCadena("@RUC", ruc);
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        ide = DR[0].ToString();
                        DB.Desconectar();
                        return ide;
                    }
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
            return "";
        }

        private String consultarIDE(string valor1, string valor2, string valor3, string campo1, string campo2, string campo3, string consulta)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                String ide;
                DB.Conectar();
                DB.CrearComando(consulta + " " + campo1 + "=@a and " + campo2 + "=@b and " + campo3 + "=@c");
                DB.AsignarParametroCadena("@a", valor1);
                DB.AsignarParametroCadena("@b", valor2);
                DB.AsignarParametroCadena("@c", valor3);
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        ide = DR[0].ToString();
                        DB.Desconectar();
                        return ide;
                    }
                }
                DB.Desconectar();
                return "";
            }
            catch (Exception de)
            {
                msjT = de.Message;
                DB.Desconectar();
                clsLogger.Graba_Log_Error(de.Message);
                log.mensajesLog("BD001", "Error al consultar un ID ", msjT, "", codigoControl);
                return "";
            }
        }
        private String consultarIDE(string valor1, string valor2, string campo1, string campo2, string consulta)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                String ide;
                DB.Conectar();
                DB.CrearComando(consulta + " " + campo1 + "=@a and " + campo2 + "=@b");
                DB.AsignarParametroCadena("@a", valor1);
                DB.AsignarParametroCadena("@b", valor2);
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    if (DR.Read())
                    {
                        ide = DR[0].ToString();
                        DB.Desconectar();
                        return ide;
                    }
                }
                DB.Desconectar();
                return null;
            }
            catch (Exception de)
            {
                msjT = de.Message;
                DB.Desconectar();
                clsLogger.Graba_Log_Error(de.Message);
                log.mensajesLog("BD001", "Error al consultar un ID", msjT, "", codigoControl);
                return "";
            }
        }
        private String consultarIDE(string valor1, string campo1, string consulta)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                String ide;
                DB.Conectar();
                DB.CrearComando(consulta + " " + campo1 + "=@a");
                DB.AsignarParametroCadena("@a", valor1);
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    if (DR.Read())
                    {
                        ide = DR[0].ToString();
                        DB.Desconectar();
                        return ide;
                    }
                }
                DB.Desconectar();
                return null;
            }
            catch (Exception de)
            {
                msjT = de.Message;
                DB.Desconectar();
                clsLogger.Graba_Log_Error(de.Message);
                log.mensajesLog("BD001", "Error al consultar un ID", msjT, "", codigoControl);
                return null;
            }
        }
        private String cerosNull(string a)
        {
            if (a.Equals(""))
                return "0.00";
            else
                return a;
        }

        private byte[] generarCodigoBarrasGS1128(string valor)
        {
            BarcodeLib.Barcode.Linear barcode = new BarcodeLib.Barcode.Linear();
            barcode.Type = BarcodeType.EAN128;
            barcode.Data = valor;
            barcode.UOM = UnitOfMeasure.PIXEL;
            barcode.BarWidth = 1;
            barcode.BarHeight = 80;
            barcode.LeftMargin = 10;
            barcode.RightMargin = 10;
            barcode.TopMargin = 10;
            barcode.BottomMargin = 10;
            byte[] barcodeInBytes = barcode.drawBarcodeAsBytes();
            return barcodeInBytes;
        }
        private string msjGuardarBD(int bandera)
        {
            switch (bandera)
            {
                case 1: return "Error Emisor";
                case 2: return "Error Receptor";
                case 3: return "Error Expedición";
                case 4: return "Error Sucursal Receptor";
                case 5: return "Error General";
                case 6: return "Error Total de Impuestos";
                case 7: return "Error Total de Impuestos Retenciones";
                case 8: return "Error Destinatarios";
                case 9: return "Error Detalles";
                case 10: return "Error Impuestos Detalles";
                case 11: return "Error Detalles Adicional";
                case 12: return "Error Información Adicional";
                case 13: return "Error Código de Barras";
                case 14: return "Error Archivos";
                default: return "";
            }
        }

        public void infromacionAdicionalCima(string termino, string proforma, string pedido, string domicilio, string telefono, string emails, string firmaSRI)
        {
            this.termino = termino; this.proforma = proforma; this.pedido = pedido; this.domicilio = domicilio; this.telefono = telefono; this.emails = emails; this.firmaSRI = firmaSRI;
        }

        private string verificarFecha(string fecha, int tipo)
        {
            string strFecha = "";
            try
            {
                if (tipo == 1) strFecha = Convert.ToDateTime(fecha).ToString("yyyy-MM-ddTHH:mm:ss");
                if (tipo == 2) strFecha = Convert.ToDateTime(fecha).ToString("yyyy-MM-dd");
                if (tipo == 3) strFecha = Convert.ToDateTime(fecha).ToString("dd-MM-dd");
                return strFecha;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        private String consultarIDE(string valor1, string valor2, string valor3, string valor4, string campo1, string campo2, string campo3, string campo4, string consulta)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                String ide;
                DB.Conectar();
                DB.CrearComando(consulta + " " + campo1 + "=@a and " + campo2 + "=@b and " + campo3 + "=@c and " + campo4 + "=@d");
                DB.AsignarParametroCadena("@a", valor1);
                DB.AsignarParametroCadena("@b", valor2);
                DB.AsignarParametroCadena("@c", valor3);
                DB.AsignarParametroCadena("@d", valor4);
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        ide = DR[0].ToString();
                        DB.Desconectar();
                        return ide;
                    }
                }

                DB.Desconectar();
                return "";
            }
            catch (Exception de)
            {
                msjT = de.Message;
                DB.Desconectar();
                clsLogger.Graba_Log_Error(de.Message);
                log.mensajesLog("BD001", "Error al consultar un ID ", msjT, "", codigoControl);
                return "";
            }
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static byte[] ImageToByte2(Image img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
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
    }

}
