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
using ValSign;
using System.Net;
using System.Xml.Serialization;
using System.Globalization;
using System.Xml.Schema;
using System.Text.RegularExpressions;
using Key_Electronica;
using CriptoSimetrica;
using Control.WebUNASAP;
using System.Configuration;
using System.Data.SqlClient;
using ReportesDEI;
using System.Data;
using clibLogger;

namespace Control
{
    public class Leer
    {
        //private BasesDatos DB;
        //private DbDataReader DR;
        private EnviarMail EM;
        private NumerosALetras numA;
        private Log log;
        private GenerarXml gXml;
        private GuardarBD gBD;
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
        private string RutaP12 = "";
        private string PassP12 = "";
        string idcomprobante2 = "";
        private string RutaXMLbase = "";
        private string nombreArchivo = "";
        private string codigoControl = "";
        private string linea = "";
        private string edo = "";
        private string mensajeBitacora = "";
        private string compania = "UNACEM ECUADOR S.A.";
        private AES Cs;
        private string p_msj = "";
        private string p_msjT = "";
        private string xmlRegenerado;
        private Boolean b_respuesta;
        private string aux;
        private Boolean esNDFinLF = false;
        private LeerOffline offline;

        //pmoncayo 20200813 (se agregan aetiquetas al XML)
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
        String exp = "";

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

        #region General

        string tipoComprobante, idComprobante, version;
        //Informacion Tributaria
        string ambiente, tipoEmision, razonSocial, nombreComercial, ruc, claveAcceso, codDoc, estab, ptoEmi, secuencial, dirMatriz, codDocVersion;
        //Informacion del Documento(Factura,guia,notas,retenciones)
        string fechaEmision, dirEstablecimiento, dirEstablecimientoGuia, contribuyenteEspecial, obligadoContabilidad, tipoIdentificacionComprador;
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
        #endregion
        string direccionComprador;

        string tipoImpuesto;
        string impuestotipoImpuesto;
        string codigoTemp;
        string identificacion = "";
        string estado = "";
        string fechaAutorizacion = "";
        string numeroAutorizacion = "";
        int idDetallesTemp = 0;
        int idDestinatario = 0;
        string empleado = "";
        string destinatarioLF = "";
        #region totales
        string subtotal12;
        string subtotal0;
        string subtotalNoSujeto;
        string ICE;
        string IVA12;
        string importeAPagar;


        //Información Adicional CIMA
        string termino, proforma, domicilio, telefono, pedido;

        #endregion

        #region Retencion 2.0
        string tipoSujetoRetenido, parteRel;
        string fechaPagoDiv, imRentaSoc, ejerFisUtDiv, NumCajBan, PrecCajBan;
        string tipoIdentificacionProveedorReembolsoRet, identificacionProveedorReembolsoRet, codPaisPagoProveedorReembolsoRet, tipoProveedorReembolsoRet, codDocReembolsoRet,
            estabDocReembolsoRet, ptoEmiDocReembolsoRet, secuencialDocReembolsoRet, fechaEmisionDocReembolsoRet, numeroAutorizacionDocReembRet;
        #endregion


        private string totalBaseImponibleReembolso = "";
        private string totalImpuestoReembolso = "";
        private string totalComprobantesReembolso = "";

        ArrayList arraylDetalles;
        ArrayList arraylImpuestosDetalles;
        ArrayList arraylDetallesAdicionales;
        ArrayList arrayInfoAdicionales;
        ArrayList arraylTotalImpuestos;
        ArrayList arraylTotalConImpuestos;
        ArrayList arraylMotivos;
        ArrayList arraylTotalImpuestosRetenciones;
        ArrayList arraylDestinatarios;
        ArrayList arraylDocsSustentos;
        ArrayList arraylReembolsosRetenciones;
        ArrayList arraylImpuestosReembolsosRet;
        String[] asDetalles;
        String[] asImpuestosDetalles;
        String[] asDetallesAdicionales;
        String[] asInfoAdicionales;
        String[] asTotalImpuestos;
        String[] asMotivos;
        String[] asTotalImpuestosRetenciones;
        String[] asDestinatarios;
        String[] asDocsSustentos;
        String[] asReembolsosRetenciones;
        String[] asImpuestosReembolsosRet;
        string codigoContingenciaTemp = "";
        Boolean banErrorArchivo = false;
        Boolean banDetalles = false;
        FirmarXML firmaXADES;
        Key_Electronica.Key_Electronica FirmaBCE;
        ValidaRUC Valiruc = new ValidaRUC();
        private string[] asPagos;
        private string[] asRubros;
        private string[] asCompensacion;
        private System.Collections.ArrayList arraylPagos = new ArrayList();
        private System.Collections.ArrayList arraylRubros = new ArrayList();
        private System.Collections.ArrayList arraylCompensacion = new ArrayList();


        string secuencial_doc = "";

        public Leer()
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                firmaXADES = new FirmarXML();
                DB = new BasesDatos();
                numA = new NumerosALetras();
                log = new Log();
                gBD = new GuardarBD();
                gXml = new GenerarXml();
                cPDF = new CrearPDF();
                Cs = new AES();
                r = new Random(DateTime.Now.Millisecond);
                recepcion = new receWeb.RecepcionComprobantesService();
                autorizacion = new autoWeb.AutorizacionComprobantesService();
                FirmaBCE = new Key_Electronica.Key_Electronica();
                offline = new LeerOffline();
                //Parametros Generales
                DB.Desconectar();
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
            
               
              

            //Fin de Parametros Generales.
        }

        public void procesarArchivo(string archivo)
        {
            BasesDatos DB = new BasesDatos();

            #region Seteo de variables

            linea = "";
            int banderaArchivo = 0;
            int banderaComprobante = 0;
            asunto = "";
            mensaje = "";
            codigoControl = "";
            codigoContingenciaTemp = "";
            identificacion = "";
            tipoComprobante = ""; idComprobante = ""; version = "";
            //Informacion Tributaria
            ambiente = ""; tipoEmision = ""; razonSocial = ""; nombreComercial = ""; ruc = ""; claveAcceso = ""; codDoc = ""; estab = ""; ptoEmi = ""; secuencial = ""; dirMatriz = "";
            //Informacion del Documento(Factura="";guia="";notas="";retenciones)
            fechaEmision = ""; dirEstablecimiento = ""; dirEstablecimientoGuia = ""; contribuyenteEspecial = ""; obligadoContabilidad = ""; tipoIdentificacionComprador = "";
            guiaRemision = ""; razonSocialComprador = ""; identificacionComprador = ""; direccionComprador = ""; totalSinImpuestos = "0"; totalDescuento = ""; propina = "0"; importeTotal = "0"; moneda = "0";
            dirPartida = ""; razonSocialTransportista = ""; tipoIdentificacionTransportista = ""; rucTransportista = ""; rise = ""; fechaIniTransporte = ""; fechaFinTransporte = ""; placa = "";//Guia de Remision
            codDocModificado = ""; numDocModificado = ""; fechaEmisionDocSustentoNota = ""; valorModificacion = "0"; motivo = "";//Nota de Credito
            valorTotal = "";
            //Nota de Debito
            tipoIdentificacionSujetoRetenido = ""; razonSocialSujetoRetenido = ""; identificacionSujetoRetenido = ""; periodoFiscal = "";
            //Destinatario Para Guia de Remision
            identificacionDestinatario = ""; razonSocialDestinatario = ""; dirDestinatario = ""; motivoTraslado = ""; docAduaneroUnico = ""; codEstabDestino = ""; ruta = ""; codDocSustentoDestinatario = ""; numDocSustentoDestinatario = ""; numAutDocSustento = ""; fechaEmisionDocSustentoDestinatario = "";
            //Total de Impuestos
            codigo = ""; codigoPorcentaje = ""; baseImponible = "0"; tarifa = "0"; valor = "0";
            codigoRetencion = ""; porcentajeRetener = ""; valorRetenido = ""; codDocSustento = ""; numDocSustento = ""; fechaEmisionDocSustento = ""; //Retenciones
            //detalles
            codigoPrincipal = ""; codigoAuxiliar = ""; descripcion = ""; cantidad = ""; precioUnitario = ""; descuento = ""; precioTotalSinImpuesto = "";
            codigoInterno = ""; codigoAdicional = "";
            //detalles Adicionales
            detAdicionalNombre = ""; detAdicionalValor = "";
            //Impuestos Detalles
            impuestoCodigo = ""; impuestoCodigoPorcentaje = ""; impuestoTarifa = ""; impuestoBaseImponible = ""; impuestoValor = "";
            //infoAdicional
            infoAdicionalNombre = ""; infoAdicionalValor = "";
            //REEMBOLSO
            this.totalComprobantesReembolso = "";
            this.totalBaseImponibleReembolso = "";
            this.totalImpuestoReembolso = "";

            //Motivo (Nota de Debito)
            motivoRazon = ""; motivoValor = "";
            //variable de autorizacion
            fechaAutorizacion = "";
            numeroAutorizacion = "";
            //Totales
            subtotal12 = "0"; subtotal0 = "0"; subtotalNoSujeto = "0"; ICE = "0"; IVA12 = "0"; importeAPagar = "0"; totalSinImpuestos = "0"; totalDescuento = "0"; importeTotal = "0"; propina = "0";
            arraylDetalles = new ArrayList();
            arraylImpuestosDetalles = new ArrayList();
            arraylDetallesAdicionales = new ArrayList();
            arrayInfoAdicionales = new ArrayList();
            arraylTotalImpuestos = new ArrayList();
            arraylMotivos = new ArrayList();
            arraylTotalImpuestosRetenciones = new ArrayList();
            arraylDestinatarios = new ArrayList();
            arraylPagos = new ArrayList();
            arraylDocsSustentos = new ArrayList();
            arraylReembolsosRetenciones = new ArrayList();
            arraylImpuestosReembolsosRet = new ArrayList();
            arraylRubros = new ArrayList();
            arraylCompensacion = new ArrayList();
            secuencial_doc = ""; msjT = ""; msj = "";


            linea = "";
            String[] asLinea;

            #endregion

            #region Retencion 2.0
            tipoSujetoRetenido = ""; parteRel = "";
            fechaPagoDiv = ""; imRentaSoc = ""; ejerFisUtDiv = ""; PrecCajBan = ""; NumCajBan = "";
            tipoIdentificacionProveedorReembolsoRet = ""; identificacionProveedorReembolsoRet = ""; codPaisPagoProveedorReembolsoRet = ""; tipoProveedorReembolsoRet = "";
            codDocReembolsoRet = ""; estabDocReembolsoRet = ""; ptoEmiDocReembolsoRet = ""; secuencialDocReembolsoRet = ""; fechaEmisionDocReembolsoRet = ""; numeroAutorizacionDocReembRet = "";
            #endregion

            FileInfo fi = new FileInfo(archivo);
            System.IO.StreamReader sr = new System.IO.StreamReader(archivo);

            //Info Adicional CIMA
            termino = ""; proforma = ""; domicilio = ""; telefono = ""; pedido = "";

            nombreArchivo = fi.Name;

            try
            {
                linea = sr.ReadLine();
                linea = VerificaAcentos(linea);
                linea = VerificaEspacios(linea);
                if (linea != null)
                {
                    asLinea = linea.Split('|');
                    version = asLinea[3];
                    if (version == "2.0")
                        version = "2.0.0";

                }
                int countdocsSustento = 0, countReembolsos = 0;
                while (linea != null && !banErrorArchivo)
                {
                    linea = sr.ReadLine();
                    if (!String.IsNullOrEmpty(linea))
                    {
                        linea = VerificaAcentos(linea);
                        linea = VerificaEspacios(linea);
                        asLinea = linea.Split('|');

                        switch (asLinea[0].ToString().Trim())
                        {
                            case "IT":
                                if (banderaArchivo == 0)
                                {
                                    banderaArchivo = 1;
                                    ambiente = asLinea[1];
                                    razonSocial = asLinea[3].ToString().Trim();
                                    ruc = asLinea[5];
                                    codDoc = asLinea[7];
                                    estab = asLinea[8];
                                    ptoEmi = asLinea[9];
                                    secuencial_doc = asLinea[10];
                                    secuencial = obtenerSecuencial(secuencial_doc);
                                    if (valida_duplicidad(codDoc, estab, ptoEmi, secuencial, ambiente.Trim()))
                                    {
                                        String[] SapasInfoAdicionales;
                                        string sap_codigoControl = "", sap_idcomprobante = "", sap_claveAcceso = "", sap_numAutorizacion = "", sap_fechaAutorizacion = "", sapinfoAdicionalNombre = "", sapinfoAdicionalValor = "", sap_anioasiento = "", sap_num_asiento = "", sap_sociedad = "", sap_fechaemision = "";

                                        banErrorArchivo = true;
                                        msj = "No se pudo procesar el archivo. " + msjLectura(banderaArchivo);
                                        msjT = "Se intentó procesar un secuencial ya registrado";
                                        codigoControl = ruc + codDoc + estab + ptoEmi + secuencial + DateTime.Now.ToString("yyyyMMddHHmm");
                                        sap_codigoControl = ruc + codDoc + estab + ptoEmi + secuencial;
                                        DB.Conectar();
                                        DB.CrearComando("select idComprobante,claveAcceso,numeroAutorizacion,fechaAutorizacion,fecha  from GENERAL with(nolock) where codigoControl like '%" + sap_codigoControl + "%'");
                                        using (DbDataReader DR5 = DB.EjecutarConsulta())
                                        {
                                            if (DR5.Read())
                                            {
                                                sap_idcomprobante = DR5.GetInt32(0).ToString();
                                                sap_claveAcceso = DR5.GetString(1);
                                                sap_numAutorizacion = DR5.GetString(2);
                                                sap_fechaAutorizacion = DR5.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss");
                                                sap_fechaemision = DR5.GetDateTime(4).ToString("dd/MM/yyyy");
                                            }
                                        }

                                        DB.Desconectar();
                                        DB.Conectar();
                                        DB.CrearComando("select nombre,valor  from InfoAdicional with(nolock) where id_Comprobante = @id_comp");
                                        DB.AsignarParametroCadena("@id_comp", sap_idcomprobante);
                                        using (DbDataReader DR4 = DB.EjecutarConsulta())
                                        {
                                            while (DR4.Read())
                                            {

                                                sapinfoAdicionalNombre = DR4.GetString(0);
                                                sapinfoAdicionalValor = DR4.GetString(1);
                                                if (sapinfoAdicionalNombre.Equals("sociedad")) { sap_sociedad = DR4.GetString(1); }
                                                if (sapinfoAdicionalNombre.Equals("numeroAsientoContable")) { sap_num_asiento = DR4.GetString(1); }
                                                if (sapinfoAdicionalNombre.Equals("anioAsientoContable")) { sap_anioasiento = DR4.GetString(1); }

                                            }
                                        }

                                        DB.Desconectar();
                                        RespuestaWebUNASAP(codDoc, estab + "-" + ptoEmi + "-" + secuencial, sap_claveAcceso, sap_numAutorizacion, sap_fechaAutorizacion, sap_fechaAutorizacion, "", "", "", "AT", "AT", sap_sociedad, sap_num_asiento, sap_anioasiento);
                                        enviar_notificacion_correo_punto(estab, codDoc + estab + ptoEmi + secuencial, sap_fechaemision, msjT);
                                        //consulta datos a la base 
                                        // enviar respuesta 
                                        // correo notificacion 

                                        break;
                                    }

                                    tipoEmision = asLinea[2];
                                    emails = asLinea[12];

                                    ambiente = verifica_puntoEmision(codDoc, estab, ptoEmi);

                                    if (!ambiente.Equals("1") && !ambiente.Equals("2"))
                                    {
                                        banErrorArchivo = true;
                                        break;
                                    }

                                    gBD.InformacionTributaria(ambiente.Trim(), asLinea[2].ToString().Trim(), asLinea[3].ToString().Trim(), asLinea[4].ToString().Trim(), asLinea[5].ToString().Trim(), asLinea[6].ToString().Trim(), asLinea[7].ToString().Trim(), asLinea[8].ToString().Trim(), asLinea[9].ToString().Trim(), asLinea[10].ToString().Trim(), asLinea[11].ToString().Trim(), emails);
                                    gXml.InformacionTributaria(ambiente.Trim(), asLinea[2].ToString().Trim(), asLinea[3].ToString().Trim(), asLinea[4].ToString().Trim(), asLinea[5].ToString().Trim(), asLinea[6].ToString().Trim(), asLinea[7].ToString().Trim(), asLinea[8].ToString().Trim(), asLinea[9].ToString().Trim(), asLinea[10].ToString().Trim(), asLinea[11]);

                                    //}
                                }
                                break;

                            case "IC":
                                if (banderaArchivo == 1)
                                {
                                    //if (asLinea[0] == "IC" && banderaArchivo == 1)
                                    //{
                                    banderaArchivo = 2;
                                    fechaEmision = asLinea[1];
                                    identificacion = asLinea[8];
                                    numDocModificado = asLinea[12].ToString().Trim();
                                    identificacionComprador = asLinea[8].ToString().Trim();
                                    if (!String.IsNullOrEmpty(asLinea[26].ToString().Trim()))
                                    {
                                        direccionComprador = asLinea[26].ToString().Trim();
                                    }
                                    if (identificacionComprador.Equals("9999999999999"))
                                    {
                                        tipoIdentificacionComprador = "07";
                                        asLinea[5] = "07";
                                        asLinea[7] = "CONSUMIDOR FINAL";

                                    }
                                    else
                                    {
                                        if (identificacionComprador.Length == 13 && identificacionComprador.Substring(10).Equals("001"))
                                        {
                                            //es ruc
                                            tipoIdentificacionComprador = "04";
                                            asLinea[5] = "04";
                                        }
                                    }
                                    if (tipoIdentificacionComprador.Equals("04") || tipoIdentificacionComprador.Equals("05"))
                                    {
                                        if (!Valiruc.ValidarNumeroIdentificacion(identificacionComprador))
                                        {
                                            banErrorArchivo = true;
                                            msj = "Número de cédula o RUC incorrecto";
                                            msjT = "La cédula/RUC no superó la validación del dígito";
                                            codigoControl = ruc + codDoc + estab + ptoEmi + secuencial + DateTime.Now.ToString("yyyyMMddHHmm");

                                        }
                                    }

                                    //if (codDoc.Equals("04")) // NOTA DE CREDITO
                                    //{
                                    //    if (valida_duplicidad_NC(asLinea[12].ToString().Trim()))
                                    //    {
                                    //        banErrorArchivo = true;
                                    //        msj = "Ya existe una Nota de Crédito aplicada a esta factura: " + asLinea[12].ToString().Trim();
                                    //    }
                                    //}

                                    razonSocialComprador = asLinea[7];

                                    gBD.infromacionDocumento(asLinea[1].ToString().Trim(), asLinea[2].ToString().Trim(), asLinea[2].ToString().Trim(), asLinea[3].ToString().Trim(), asLinea[4].ToString().Trim(), asLinea[5].ToString().Trim(), asLinea[6].ToString().Trim(), razonSocialComprador, asLinea[8].ToString().Trim(), asLinea[9].ToString().Trim(),
                                       asLinea[17].ToString().Trim(), asLinea[18].ToString().Trim(), asLinea[19].ToString().Trim(), asLinea[20].ToString().Trim(), asLinea[10].ToString().Trim(), asLinea[23].ToString().Trim(), asLinea[24].ToString().Trim(), asLinea[25].ToString().Trim(), asLinea[11].ToString().Trim(), asLinea[12].ToString().Trim(), asLinea[13].ToString().Trim(), asLinea[14].ToString().Trim(), asLinea[15], direccionComprador);

                                    gXml.infromacionDocumento(asLinea[1].ToString().Trim(), asLinea[2].ToString().Trim(), asLinea[3].ToString().Trim(), asLinea[4].ToString().Trim(), asLinea[5].ToString().Trim(), asLinea[6].ToString().Trim(), razonSocialComprador, asLinea[8].ToString().Trim(), asLinea[9].ToString().Trim(),
                                                asLinea[17].ToString().Trim(), asLinea[18].ToString().Trim(), asLinea[19].ToString().Trim(), asLinea[20].ToString().Trim(), asLinea[10].ToString().Trim(), asLinea[23].ToString().Trim(), asLinea[24].ToString().Trim(), asLinea[25].ToString().Trim(), asLinea[11].ToString().Trim(), asLinea[12].ToString().Trim(), asLinea[13].ToString().Trim(), asLinea[14].ToString().Trim(), asLinea[15], direccionComprador);
                                    if (codDoc.Equals("07"))
                                    {
                                        if (version.Equals("2.0") || version.Equals("2.0.0"))
                                        {
                                       
                                            gXml.comprobanteRetencion(asLinea[16].ToString().Trim(), asLinea[5].ToString().Trim(), asLinea[7].ToString().Trim(), asLinea[8], asLinea[27].ToString(), asLinea[26].ToString());
                                            gBD.comprobanteRetencion(asLinea[16].ToString().Trim(), asLinea[5].ToString().Trim(), asLinea[7].ToString().Trim(), asLinea[8], asLinea[27].ToString(), asLinea[26].ToString());
                                        }

                                        else
                                        {
                                            gXml.comprobanteRetencion(asLinea[16].ToString().Trim(), asLinea[5].ToString().Trim(), asLinea[7].ToString().Trim(), asLinea[8]);
                                            gBD.comprobanteRetencion(asLinea[16].ToString().Trim(), asLinea[5].ToString().Trim(), asLinea[7].ToString().Trim(), asLinea[8]);
                                        }
                                    }
                                  
                                    //}
                                }
                                break;

                            case "T":
                                //if (asLinea[0] == "T" && banderaArchivo == 2)
                                //{
                                banderaArchivo = 3;
                                gBD.cantidades(asLinea[1], asLinea[2], asLinea[3], asLinea[4], asLinea[5], asLinea[6], asLinea[7], asLinea[8], asLinea[9], asLinea[10]);
                                gXml.cantidades(asLinea[1], asLinea[2], asLinea[3], asLinea[4], asLinea[5], asLinea[6], asLinea[7], asLinea[8], asLinea[9], asLinea[10]);

                                //}
                                break;

                            case "TI":
                                //if (asLinea[0] == "TI")
                                //{
                                banderaArchivo = 4;
                                asTotalImpuestos = new String[8];
                                codigo = asLinea[1].ToString().Trim();
                                codigoPorcentaje = asLinea[2].ToString().Trim();
                                baseImponible = asLinea[3].ToString().Trim();
                                tarifa = asLinea[4].ToString().Trim();
                                valor = asLinea[5].ToString().Trim();
                                tipoImpuesto = asLinea[6].ToString().Trim();
                                asTotalImpuestos[0] = codigo; asTotalImpuestos[1] = codigoPorcentaje; asTotalImpuestos[2] = baseImponible;
                                asTotalImpuestos[3] = tarifa; asTotalImpuestos[4] = valor; asTotalImpuestos[5] = tipoImpuesto;
                                asTotalImpuestos[6] = "0.00";
                                arraylTotalImpuestos.Add(asTotalImpuestos);
                                gBD.totalImpuestos(arraylTotalImpuestos);
                                gXml.totalImpuestos(arraylTotalImpuestos);

                                //}
                                break;
                            case "PA":
                                banderaArchivo = 5;
                                this.asPagos = new string[5];
                                this.asPagos[0] = asLinea[1].ToString().Trim();
                                this.asPagos[1] = asLinea[2].ToString().Trim();
                                this.asPagos[2] = asLinea[3].ToString().Trim();
                                this.asPagos[3] = asLinea[4].ToString().Trim();
                                this.arraylPagos.Add(this.asPagos);
                                this.gBD.DetallePagos(this.arraylPagos);
                                this.gXml.DetallePagos(this.arraylPagos);
                                break;
                            case "TIR":
                                //if (asLinea[0] == "TIR")
                                //{
                                banderaArchivo = 6;
                                if (codDoc.Equals("07") && (version.Equals("2.0") || version.Equals("2.0.0")))
                                {
                                    asTotalImpuestosRetenciones = new String[13];
                                }
                                else
                                {
                                    asTotalImpuestosRetenciones = new String[8];
                                }
                                codigo = asLinea[1].ToString().Trim();
                                codigoRetencion = asLinea[2].ToString().Trim();
                                baseImponible = asLinea[3].ToString().Trim();
                                porcentajeRetener = asLinea[4].ToString().Trim();
                                valorRetenido = asLinea[5].ToString().Trim();
                                if (codDoc != "07" && (version != "2.0" || version != "2.0.0")) 
                                {
                                    codDocSustento = asLinea[6].ToString().Trim();
                                    numDocSustento = asLinea[7].ToString().Trim();
                                    fechaEmisionDocSustento = Convert.ToDateTime(asLinea[8].ToString().Trim()).ToString("dd/MM/yyyy");
                                }
                               
                                asTotalImpuestosRetenciones[0] = codigo; asTotalImpuestosRetenciones[1] = codigoRetencion; asTotalImpuestosRetenciones[2] = baseImponible;
                                asTotalImpuestosRetenciones[3] = porcentajeRetener; asTotalImpuestosRetenciones[4] = valorRetenido; asTotalImpuestosRetenciones[5] = codDocSustento;
                                asTotalImpuestosRetenciones[6] = numDocSustento; asTotalImpuestosRetenciones[7] = fechaEmisionDocSustento;
                                if (codDoc.Equals("07") && (version.Equals("2.0") || version.Equals("2.0.0")))
                                {
                                    asTotalImpuestosRetenciones[8] = asLinea[9].ToString().Trim();
                                    asTotalImpuestosRetenciones[9] = asLinea[10].ToString().Trim();
                                    asTotalImpuestosRetenciones[10] = asLinea[11].ToString().Trim();
                                    asTotalImpuestosRetenciones[11] = asLinea[12].ToString().Trim();
                                    asTotalImpuestosRetenciones[12] = asLinea[13].ToString().Trim();
                                }
                                arraylTotalImpuestosRetenciones.Add(asTotalImpuestosRetenciones);
                                gBD.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);
                                gXml.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);
                                //}
                                break;

                            case "MO":
                                //if (asLinea[0] == "MO")
                                //{
                                banderaArchivo = 7;
                                asDetalles = new String[9];
                                codigoPrincipal = "";
                                codigoAuxiliar = "";
                                descripcion = asLinea[1];
                                cantidad = "0";
                                precioUnitario = "0";
                                descuento = "0";
                                precioTotalSinImpuesto = asLinea[2];
                                asDetalles[0] = codigoPrincipal; asDetalles[1] = codigoAuxiliar; asDetalles[2] = descripcion;
                                asDetalles[3] = cantidad; asDetalles[4] = precioUnitario; asDetalles[5] = descuento;
                                asDetalles[6] = precioTotalSinImpuesto; asDetalles[7] = "";
                                arraylDetalles.Add(asDetalles);
                                codigoTemp = codigoPrincipal;
                                gBD.detalles(arraylDetalles);
                                gXml.detalles(arraylDetalles);



                                asMotivos = new String[2];
                                motivoRazon = asLinea[1];
                                motivoValor = asLinea[2];
                                asMotivos[0] = motivoRazon; asMotivos[1] = motivoValor;
                                arraylMotivos.Add(asMotivos);
                                gBD.Motivos(arraylMotivos);
                                gXml.Motivos(arraylMotivos);

                                //}
                                break;

                            case "DEST":
                                //if (asLinea[0] == "DEST")
                                //{
                                banderaArchivo = 8;
                                asDestinatarios = new String[12];
                                identificacionDestinatario = asLinea[1].ToString().Trim();
                                razonSocialDestinatario = asLinea[2].ToString().Trim();
                                dirDestinatario = asLinea[3].ToString().Trim();
                                motivoTraslado = asLinea[4].ToString().Trim();
                                docAduaneroUnico = "";//asLinea[5].ToString().Trim();
                                codEstabDestino = asLinea[6].ToString().Trim();
                                ruta = asLinea[7].ToString().Trim();
                                codDocSustento = asLinea[8].ToString().Trim();
                                numDocSustento = asLinea[9].ToString().Trim();
                                numAutDocSustento = asLinea[10].ToString().Trim();
                                fechaEmisionDocSustento = Convert.ToDateTime(asLinea[11].ToString().Trim()).ToString("dd/MM/yyyy"); //asLinea[11].ToString().Trim();
                                idDestinatario++;
                                asDestinatarios[0] = identificacionDestinatario; asDestinatarios[1] = razonSocialDestinatario; asDestinatarios[2] = dirDestinatario;
                                asDestinatarios[3] = motivoTraslado; asDestinatarios[4] = docAduaneroUnico; asDestinatarios[5] = codEstabDestino;
                                asDestinatarios[6] = ruta; asDestinatarios[7] = codDocSustento; asDestinatarios[8] = numDocSustento;
                                asDestinatarios[9] = numAutDocSustento; asDestinatarios[10] = fechaEmisionDocSustento; asDestinatarios[11] = idDestinatario.ToString().Trim();
                                //asDestinatarios[12] = "";
                                arraylDestinatarios.Add(asDestinatarios);
                                gBD.Destinatarios(arraylDestinatarios);
                                gXml.Destinatarios(arraylDestinatarios);
                                //}
                                break;

                            case "DE":
                                //if (asLinea[0] == "DE")
                                //{
                                banderaArchivo = 9;
                                asDetalles = new String[11];
                                codigoPrincipal = asLinea[1].ToString().Trim();
                                codigoAuxiliar = asLinea[2].ToString().Trim();
                                descripcion = asLinea[3].ToString().Trim();
                                cantidad = asLinea[4].ToString().Trim();
                                precioUnitario = asLinea[5].ToString().Trim();
                                descuento = asLinea[6].ToString().Trim();
                                precioTotalSinImpuesto = asLinea[7].ToString().Trim();
                                idDetallesTemp++;
                                //idDestinatario = idDestinatario;
                                asDetalles[0] = codigoPrincipal; asDetalles[1] = codigoAuxiliar; asDetalles[2] = descripcion;
                                asDetalles[3] = cantidad; asDetalles[4] = precioUnitario; asDetalles[5] = descuento;
                                asDetalles[6] = precioTotalSinImpuesto; asDetalles[7] = idDetallesTemp.ToString().Trim();
                                asDetalles[8] = idDestinatario.ToString().Trim();
                                arraylDetalles.Add(asDetalles);
                                codigoTemp = codigoPrincipal;
                                gBD.detalles(arraylDetalles);
                                gXml.detalles(arraylDetalles);
                                //}
                                break;

                            case "IM":
                                //if (asLinea[0] == "IM")
                                //{
                                banderaArchivo = 10;
                                asImpuestosDetalles = new String[8];
                                impuestoCodigo = asLinea[1].ToString().Trim();
                                impuestoCodigoPorcentaje = asLinea[2].ToString().Trim();
                                impuestoBaseImponible = asLinea[3].ToString().Trim();
                                impuestoTarifa = asLinea[4].ToString().Trim();
                                impuestoValor = asLinea[5].ToString().Trim();
                                codigoTemp = codigoPrincipal;
                                impuestotipoImpuesto = asLinea[6].ToString().Trim();
                                asImpuestosDetalles[0] = impuestoCodigo; asImpuestosDetalles[1] = impuestoCodigoPorcentaje; asImpuestosDetalles[2] = impuestoBaseImponible;
                                asImpuestosDetalles[3] = impuestoTarifa; asImpuestosDetalles[4] = impuestoValor; asImpuestosDetalles[5] = codigoTemp;
                                asImpuestosDetalles[6] = impuestotipoImpuesto;
                                asImpuestosDetalles[7] = idDetallesTemp.ToString();
                                arraylImpuestosDetalles.Add(asImpuestosDetalles);
                                gBD.impuestos(arraylImpuestosDetalles);
                                gXml.impuestos(arraylImpuestosDetalles);
                                //}
                                break;

                            case "DA":
                                //if (asLinea[0] == "DA")
                                //{
                                banderaArchivo = 11;
                                asDetallesAdicionales = new String[4];
                                detAdicionalNombre = "DA";//asLinea[1].ToString().Trim();
                                detAdicionalValor = linea; //asLinea[2].ToString().Trim();
                                codigoTemp = codigoPrincipal;
                                //idDetallesTemp = idDetallesTemp;
                                asDetallesAdicionales[0] = detAdicionalNombre; asDetallesAdicionales[1] = detAdicionalValor;
                                asDetallesAdicionales[2] = codigoTemp; asDetallesAdicionales[3] = idDetallesTemp.ToString().Trim();
                                arraylDetallesAdicionales.Add(asDetallesAdicionales);
                                gBD.detallesAdicionales(arraylDetallesAdicionales);
                                gXml.detallesAdicionales(arraylDetallesAdicionales);
                                //}
                                break;

                            case "IA":
                                //if (asLinea[0] == "IA")
                                //{
                                banderaArchivo = 12;
                                asInfoAdicionales = new String[2];
                                infoAdicionalNombre = asLinea[1].ToString().Trim();
                                infoAdicionalValor = asLinea[2].ToString().Trim();
                                asInfoAdicionales[0] = infoAdicionalNombre; asInfoAdicionales[1] = infoAdicionalValor;
                                arrayInfoAdicionales.Add(asInfoAdicionales);
                                gBD.informacionAdicional(arrayInfoAdicionales);
                                gXml.informacionAdicional(arrayInfoAdicionales);
                                //}
                                break;

                            //CIMA

                            case "CIMAIT":

                                banderaArchivo = 13;
                                termino = asLinea[1].ToString();
                                proforma = asLinea[2].ToString();
                                pedido = asLinea[3].ToString();
                                domicilio = asLinea[4].ToString();
                                telefono = asLinea[5].ToString();

                                gBD.infromacionAdicionalCima(termino, proforma, pedido, domicilio, telefono, "", "");
                                break;

                            case "DS":
                                try
                                {
                                    if (codDoc.Equals("07") && (version.Equals("2.0") || version.Equals("2.0.0")))
                                    {
                                        banderaArchivo = 14;
                                        countdocsSustento++;
                                        countReembolsos = 0;
                                        asDocsSustentos = new String[18];
                                        asDocsSustentos[0] = countdocsSustento.ToString();
                                        asDocsSustentos[1] = asLinea[1].ToString().Trim();
                                        asDocsSustentos[2] = codDocSustento = asLinea[2].ToString().Trim();
                                        asDocsSustentos[3] = numDocSustento = asLinea[3].ToString().Trim();
                                        asDocsSustentos[4] = fechaEmisionDocSustento = asLinea[4].ToString().Trim();
                                        asDocsSustentos[5] = asLinea[5].ToString().Trim();
                                        asDocsSustentos[6] = asLinea[6].ToString().Trim();
                                        asDocsSustentos[7] = asLinea[7].ToString().Trim();
                                        asDocsSustentos[8] = asLinea[8].ToString().Trim();
                                        asDocsSustentos[9] = asLinea[9].ToString().Trim();
                                        asDocsSustentos[10] = asLinea[10].ToString().Trim();
                                        asDocsSustentos[11] = asLinea[11].ToString().Trim();
                                        asDocsSustentos[12] = asLinea[12].ToString().Trim();
                                        asDocsSustentos[13] = asLinea[13].ToString().Trim();
                                        asDocsSustentos[14] = asLinea[14].ToString().Trim();
                                        asDocsSustentos[15] = asLinea[15].ToString().Trim();
                                        asDocsSustentos[16] = asLinea[16].ToString().Trim();
                                        asDocsSustentos[17] = asLinea[17].ToString().Trim();
                                        arraylDocsSustentos.Add(asDocsSustentos);
                                        gBD.docsSustentos(arraylDocsSustentos);
                                        gXml.docsSustentos(arraylDocsSustentos);

                                        clsLogger.Graba_Log_Info("Fin linea DS bandera   " + banderaArchivo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DB.Desconectar();
                                    clsLogger.Graba_Log_Error("  Error DS Exception " + ex.ToString());
                                }

                                break;

                            case "IMS":
                                try
                                {
                                    if (codDoc.Equals("07") && (version.Equals("2.0") || version.Equals("2.0.0")))
                                    {
                                        banderaArchivo = 15;
                                        asImpuestosDetalles = new String[7];
                                        impuestoCodigo = asLinea[1].ToString().Trim();
                                        impuestoCodigoPorcentaje = asLinea[2].ToString().Trim();
                                        impuestoBaseImponible = asLinea[4].ToString().Trim();
                                        impuestoTarifa = asLinea[3].ToString().Trim();
                                        impuestoValor = asLinea[5].ToString().Trim();
                                        switch (impuestoCodigo) { case "2": impuestotipoImpuesto = "IVA"; break; case "3": impuestotipoImpuesto = "ICE"; break; }

                                        asImpuestosDetalles[0] = countdocsSustento.ToString();
                                        asImpuestosDetalles[1] = impuestoCodigo;
                                        asImpuestosDetalles[2] = impuestoCodigoPorcentaje;
                                        asImpuestosDetalles[3] = valida_texto_a_numero(impuestoBaseImponible);
                                        asImpuestosDetalles[4] = impuestoTarifa;
                                        asImpuestosDetalles[5] = valida_texto_a_numero(impuestoValor);
                                        asImpuestosDetalles[6] = impuestotipoImpuesto;
                                        arraylImpuestosDetalles.Add(asImpuestosDetalles);
                                        gBD.impuestos(arraylImpuestosDetalles);
                                        gXml.impuestos(arraylImpuestosDetalles);
                                        clsLogger.Graba_Log_Error("Fin linea IMS bandera   " + banderaArchivo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DB.Desconectar();
                                    clsLogger.Graba_Log_Error("  Error IMS Exception " + ex.ToString());
                                }

                                break;

                            case "TIRS":
                                try
                                {
                                    if (codDoc.Equals("07") && (version.Equals("2.0") || version.Equals("2.0.0")))
                                    {
                                        banderaArchivo = 16;
                                        asTotalImpuestosRetenciones = new String[14];
                                        codigo = asLinea[1].ToString().Trim();
                                        codigoRetencion = asLinea[2].ToString().Trim();
                                        baseImponible = asLinea[3].ToString().Trim();
                                        porcentajeRetener = asLinea[4].ToString().Trim();
                                        valorRetenido = asLinea[5].ToString().Trim();
                                        fechaPagoDiv = asLinea[9].ToString().Trim();
                                        imRentaSoc = asLinea[10].ToString().Trim();
                                        ejerFisUtDiv = asLinea[11].ToString().Trim();
                                        NumCajBan = asLinea[12].ToString().Trim();
                                        PrecCajBan = asLinea[13].ToString().Trim();

                                        asTotalImpuestosRetenciones[13] = countdocsSustento.ToString();
                                        asTotalImpuestosRetenciones[0] = codigo;
                                        asTotalImpuestosRetenciones[1] = codigoRetencion;
                                        asTotalImpuestosRetenciones[2] = valida_texto_a_numero(baseImponible);
                                        asTotalImpuestosRetenciones[3] = porcentajeRetener;
                                        asTotalImpuestosRetenciones[4] = valida_texto_a_numero(valorRetenido);
                                        asTotalImpuestosRetenciones[5] = codDocSustento;
                                        asTotalImpuestosRetenciones[6] = numDocSustento;
                                        asTotalImpuestosRetenciones[7] = fechaEmisionDocSustento;
                                        asTotalImpuestosRetenciones[8] = fechaPagoDiv;
                                        asTotalImpuestosRetenciones[9] = imRentaSoc;
                                        asTotalImpuestosRetenciones[10] = ejerFisUtDiv;
                                        asTotalImpuestosRetenciones[11] = NumCajBan;
                                        asTotalImpuestosRetenciones[12] = PrecCajBan;
                                        arraylTotalImpuestosRetenciones.Add(asTotalImpuestosRetenciones);
                                        gBD.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);
                                        gXml.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);
                                        clsLogger.Graba_Log_Info("Fin linea TIRS bandera   " + banderaArchivo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DB.Desconectar();
                                        clsLogger.Graba_Log_Error("  Error TIRS Exception " + ex.ToString());
                                }

                                break;
                            case "RBS":
                                try
                                {
                                    if (codDoc.Equals("07") && (version.Equals("2.0") || version.Equals("2.0.0")))
                                    {
                                        banderaArchivo = 17;
                                        countReembolsos++;
                                        asReembolsosRetenciones = new String[12];

                                        tipoIdentificacionProveedorReembolsoRet = asLinea[1].ToString().Trim();
                                        identificacionProveedorReembolsoRet = asLinea[2].ToString().Trim();
                                        codPaisPagoProveedorReembolsoRet = asLinea[3].ToString().Trim();
                                        tipoProveedorReembolsoRet = asLinea[4].ToString().Trim();
                                        codDocReembolsoRet = asLinea[5].ToString().Trim();
                                        estabDocReembolsoRet = asLinea[6].ToString().Trim();
                                        ptoEmiDocReembolsoRet = asLinea[7].ToString().Trim();
                                        secuencialDocReembolsoRet = asLinea[8].ToString().Trim();
                                        fechaEmisionDocReembolsoRet = asLinea[9].ToString().Trim();
                                        numeroAutorizacionDocReembRet = asLinea[10].ToString().Trim();

                                        asReembolsosRetenciones[0] = countdocsSustento.ToString();
                                        asReembolsosRetenciones[1] = countReembolsos.ToString();
                                        asReembolsosRetenciones[2] = tipoIdentificacionProveedorReembolsoRet;
                                        asReembolsosRetenciones[3] = identificacionProveedorReembolsoRet;
                                        asReembolsosRetenciones[4] = codPaisPagoProveedorReembolsoRet;
                                        asReembolsosRetenciones[5] = tipoProveedorReembolsoRet;
                                        asReembolsosRetenciones[6] = codDocReembolsoRet;
                                        asReembolsosRetenciones[7] = estabDocReembolsoRet;
                                        asReembolsosRetenciones[8] = ptoEmiDocReembolsoRet;
                                        asReembolsosRetenciones[9] = secuencialDocReembolsoRet;
                                        asReembolsosRetenciones[10] = fechaEmisionDocReembolsoRet;
                                        asReembolsosRetenciones[11] = numeroAutorizacionDocReembRet;
                                        arraylReembolsosRetenciones.Add(asReembolsosRetenciones);
                                        gBD.reembolsosSustentos(arraylReembolsosRetenciones);
                                        gXml.reembolsosSustentos(arraylReembolsosRetenciones);
                                        clsLogger.Graba_Log_Info("Fin linea RBS bandera   " + banderaArchivo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DB.Desconectar();
                                    clsLogger.Graba_Log_Error("  Error RBS Exception " + ex.ToString());
                                }
                                break;

                            case "IRB":
                                try
                                {
                                    if (codDoc.Equals("07") && (version.Equals("2.0") || version.Equals("2.0.0")))
                                    {
                                        banderaArchivo = 18;
                                        asImpuestosReembolsosRet = new String[8];
                                        impuestoCodigo = asLinea[1].ToString().Trim();
                                        impuestoCodigoPorcentaje = asLinea[2].ToString().Trim();
                                        impuestoTarifa = asLinea[3].ToString().Trim();
                                        impuestoBaseImponible = asLinea[4].ToString().Trim();
                                        impuestoValor = asLinea[5].ToString().Trim();
                                        switch (impuestoCodigo) { case "2": impuestotipoImpuesto = "IVA"; break; case "3": impuestotipoImpuesto = "ICE"; break; }

                                        asImpuestosReembolsosRet[0] = countReembolsos.ToString();
                                        asImpuestosReembolsosRet[1] = impuestoCodigo;
                                        asImpuestosReembolsosRet[2] = impuestoCodigoPorcentaje;
                                        asImpuestosReembolsosRet[3] = impuestoTarifa;
                                        asImpuestosReembolsosRet[4] = valida_texto_a_numero(impuestoBaseImponible);
                                        asImpuestosReembolsosRet[5] = valida_texto_a_numero(impuestoValor);
                                        asImpuestosReembolsosRet[6] = impuestotipoImpuesto;
                                        asImpuestosReembolsosRet[7] = countdocsSustento.ToString();
                                        arraylImpuestosReembolsosRet.Add(asImpuestosReembolsosRet);

                                        gBD.impuestosReembolsosSustentos(arraylImpuestosReembolsosRet);
                                        gXml.impuestosReembolsosSustentos(arraylImpuestosReembolsosRet);
                                        clsLogger.Graba_Log_Info("Fin linea IRB bandera   " + banderaArchivo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DB.Desconectar();
                                    clsLogger.Graba_Log_Error("  Error IRB Exception " + ex.ToString());
                                }

                                break;

                            case "PAS":
                                try
                                {
                                    if (codDoc.Equals("07") && (version.Equals("2.0") || version.Equals("2.0.0")))
                                    {
                                        banderaArchivo = 19;
                                        this.asPagos = new string[3];
                                        this.asPagos[0] = countdocsSustento.ToString();
                                        this.asPagos[1] = asLinea[1].ToString().Trim();
                                        this.asPagos[2] = asLinea[2].ToString().Trim();
                                        this.arraylPagos.Add(this.asPagos);
                                        this.gBD.DetallePagos(this.arraylPagos);
                                        this.gXml.DetallePagos(this.arraylPagos);
                                        clsLogger.Graba_Log_Info("Fin linea PAS bandera   " + banderaArchivo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DB.Desconectar();
                                    clsLogger.Graba_Log_Error("  Error PAS Exception " + ex.ToString());
                                }

                                break;
                        
                            case "CS":

                                banderaArchivo = 14;
                                this.asCompensacion = new string[4];
                                this.asCompensacion[0] = asLinea[1].ToString().Trim();
                                this.asCompensacion[1] = asLinea[2].ToString().Trim();
                                this.asCompensacion[2] = asLinea[3].ToString().Trim();
                                this.arraylCompensacion.Add(this.asCompensacion);
                                this.gBD.DetalleCompensacion(this.arraylCompensacion);
                                this.gXml.DetalleCompensacion(this.arraylCompensacion);
                                break;
                        }
                    }
                }

                if (!banErrorArchivo)
                {
                    if (banderaArchivo > 2)
                    {
                        //guiaRemision = estab + "-" + ptoEmi + "-" + secuencial;
                        if (tipoEmision.Equals("2"))
                        {
                            codigoContingenciaTemp = obtenerClaveContingencia(ambiente);
                            if (!String.IsNullOrEmpty(codigoContingenciaTemp))
                            {
                                claveAcceso = generarClaveAccesoContingencia(codigoContingenciaTemp);
                            }
                            else
                            {
                                banErrorArchivo = true;
                                msjT = "No se dispone de clave de contingencia";

                            }
                        }
                        else
                        {
                            claveAcceso = generarClaveAcceso();
                        }
                        codigoControl = ruc + codDoc + estab + ptoEmi + secuencial + Convert.ToDateTime(fechaEmision).ToString("yyyyMMddHHmm");
                        banErrorArchivo = false;
                    }
                    else
                    {
                        banErrorArchivo = true;
                    }
                }


            }
            catch (Exception arex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(arex.Message);
                banErrorArchivo = true;
                msj = "No se pudo procesar el archivo. " + msjLectura(banderaArchivo);
                msjT = arex.Message;
                log.mensajesLog("ES003", "", msjT, "", codigoControl, linea);
                //msjT = "";
            }
            finally
            {
                sr.Close();
            }

            if (!banErrorArchivo)
            {
                if (codDoc == "07" && version == "2.0.0")
                {
                    this.gBD.xmlComprobante(version, "comprobante", "");
                    this.gXml.xmlComprobante(version, "comprobante");
                    gBD.otrosCampos(claveAcceso, secuencial, guiaRemision, codigoControl);
                    gXml.otrosCampos(claveAcceso, secuencial, guiaRemision);
                }
                else
                {
                    gBD.xmlComprobante("1.1.0", "comprobante", "");
                    gXml.xmlComprobante("1.1.0", "comprobante");
                    gBD.otrosCampos(claveAcceso, secuencial, guiaRemision, codigoControl);
                    gXml.otrosCampos(claveAcceso, secuencial, guiaRemision);
                }
                procesar();
            }
            else
            {
                banErrorArchivo = false;
                log.mensajesLog("ES003", "El archivo tiene error o no está activada para facturación electrónica. " + msj + msjT, msjT, "", codigoControl, linea);
            }

            copiarArc(RutaTXT, RutaBCK, nombreArchivo, "");
        }

        public void procesarArchivoXML(Boolean esArchivo, string archivo, string p_idLog, string p_proceso)
        {
            #region Seteo de variables

            linea = "";
            int banderaArchivo = 0;
            int banderaComprobante = 0;
            asunto = "";
            mensaje = "";
            codigoControl = "";// p_codControl;
            codigoContingenciaTemp = "";
            identificacion = "";
            tipoComprobante = ""; idComprobante = ""; version = "";
            //Informacion Tributaria
            ambiente = ""; tipoEmision = ""; razonSocial = ""; nombreComercial = ""; ruc = ""; claveAcceso = ""; codDoc = ""; codDocVersion = ""; estab = ""; ptoEmi = ""; secuencial = ""; dirMatriz = "";
            //Informacion del Documento(Factura="";guia="";notas="";retenciones)
            fechaEmision = ""; dirEstablecimiento = ""; dirEstablecimientoGuia = ""; contribuyenteEspecial = ""; obligadoContabilidad = ""; tipoIdentificacionComprador = "";
            guiaRemision = ""; razonSocialComprador = ""; identificacionComprador = ""; direccionComprador = ""; totalSinImpuestos = "0"; totalDescuento = ""; propina = "0"; importeTotal = "0"; moneda = "0";
            dirPartida = ""; razonSocialTransportista = ""; tipoIdentificacionTransportista = ""; rucTransportista = ""; rise = ""; fechaIniTransporte = ""; fechaFinTransporte = ""; placa = "";//Guia de Remision
            codDocModificado = ""; numDocModificado = ""; fechaEmisionDocSustentoNota = ""; valorModificacion = "0"; motivo = "";//Nota de Credito
            valorTotal = "";
            //Nota de Debito
            tipoIdentificacionSujetoRetenido = ""; razonSocialSujetoRetenido = ""; identificacionSujetoRetenido = ""; periodoFiscal = "";
            //Destinatario Para Guia de Remision
            identificacionDestinatario = ""; razonSocialDestinatario = ""; dirDestinatario = ""; motivoTraslado = ""; docAduaneroUnico = ""; codEstabDestino = ""; ruta = ""; codDocSustentoDestinatario = ""; numDocSustentoDestinatario = ""; numAutDocSustento = ""; fechaEmisionDocSustentoDestinatario = "";
            //Total de Impuestos
            codigo = ""; codigoPorcentaje = ""; baseImponible = "0"; tarifa = "0"; valor = "0";
            codigoRetencion = ""; porcentajeRetener = ""; valorRetenido = ""; codDocSustento = ""; numDocSustento = ""; fechaEmisionDocSustento = ""; //Retenciones
            //detalles
            codigoPrincipal = ""; codigoAuxiliar = ""; descripcion = ""; cantidad = ""; precioUnitario = ""; descuento = ""; precioTotalSinImpuesto = "";
            codigoInterno = ""; codigoAdicional = "";
            //detalles Adicionales
            detAdicionalNombre = ""; detAdicionalValor = "";
            //Impuestos Detalles
            impuestoCodigo = ""; impuestoCodigoPorcentaje = ""; impuestoTarifa = ""; impuestoBaseImponible = ""; impuestoValor = "";
            //infoAdicional
            infoAdicionalNombre = ""; infoAdicionalValor = "";
            //Motivo (Nota de Debito)
            motivoRazon = ""; motivoValor = "";
            //Totales
            subtotal12 = "0"; subtotal0 = "0"; subtotalNoSujeto = "0"; ICE = "0"; IVA12 = "0"; importeAPagar = "0"; totalSinImpuestos = "0"; totalDescuento = "0"; importeTotal = "0"; propina = "0";
            //variable de autorizacion
            fechaAutorizacion = "";
            numeroAutorizacion = "";
            arraylDetalles = new ArrayList();
            arraylImpuestosDetalles = new ArrayList();
            arraylDetallesAdicionales = new ArrayList();
            arrayInfoAdicionales = new ArrayList();
            arraylTotalImpuestos = new ArrayList();
            arraylTotalConImpuestos = new ArrayList();
            arraylMotivos = new ArrayList();
            arraylTotalImpuestosRetenciones = new ArrayList();
            arraylDestinatarios = new ArrayList();
            arraylPagos = new ArrayList();
            arraylDocsSustentos = new ArrayList();
            arraylReembolsosRetenciones = new ArrayList();
            arraylImpuestosReembolsosRet = new ArrayList();
            arraylRubros = new ArrayList();
            arraylCompensacion = new ArrayList();
            secuencial_doc = "";
            mensajeBitacora = "";
            esNDFinLF = false;
            destinatarioLF = "";

            linea = "";
            String[] asLinea;

            #endregion

            #region Retencion 2.0
            tipoSujetoRetenido = ""; parteRel = "";
            fechaPagoDiv = ""; imRentaSoc = ""; ejerFisUtDiv = ""; PrecCajBan = ""; NumCajBan = "";
            tipoIdentificacionProveedorReembolsoRet = ""; identificacionProveedorReembolsoRet = ""; codPaisPagoProveedorReembolsoRet = ""; tipoProveedorReembolsoRet = "";
            codDocReembolsoRet = ""; estabDocReembolsoRet = ""; ptoEmiDocReembolsoRet = ""; secuencialDocReembolsoRet = ""; fechaEmisionDocReembolsoRet = ""; numeroAutorizacionDocReembRet = "";
            #endregion
            BasesDatos DB = new BasesDatos();

            //pmoncayo 20200813
            this.comercioExterior = "";
            this.incoTermFactura = "";
            this.lugarIncoTerm = "";
            this.paisOrigen = "";
            this.puertoEmbarque = "";
            this.puertoDestino = "";
            this.paisDestino = "";
            this.paisAdquisicion = "";
            this.incoTermTotalSinImpuestos = "";
            this.fleteInternacional = "";
            this.seguroInternacional = "";
            this.gastosAduaneros = "";
            this.gastosTransporteOtros = "";


            //Info Adicional CIMA
            termino = ""; proforma = ""; domicilio = ""; telefono = ""; pedido = "";

            Log lg1 = new Log();
            //lg1.guardar_Log("ingresando  a procesarArchivoXML paso 1 ");
            try
            {
                if (esArchivo)
                {
                    FileInfo fi = new FileInfo(archivo);
                    nombreArchivo = fi.Name;
                }
                //Leer XML
                XmlDocument xmlbase = new XmlDocument();
                if (esArchivo)
                    xmlbase.Load(archivo);
                else
                    xmlbase.LoadXml(archivo);

                xmlbase.InnerXml = Regex.Replace(xmlbase.InnerXml, @"\t|\n|\r", "");
                xmlbase.InnerXml = VerificaAcentos(xmlbase.InnerXml);

                string temp1 = xmlbase.OuterXml;
                byte[] bytes1 = Encoding.Default.GetBytes(temp1);
                temp1 = Encoding.UTF8.GetString(bytes1);
                xmlbase.LoadXml(temp1);

                //lg1.guardar_Log("ingresando  a procesarArchivoXML paso 2 ");

                XmlNode root = xmlbase.DocumentElement;
                //IT
                if (banderaArchivo == 0)
                {
                    banderaArchivo = 1;
                    version = lee_atributo_nodo_xml(root, "version");
                    ambiente = lee_nodo_xml(root, "ambiente");
                    tipoEmision = lee_nodo_xml(root, "tipoEmision");
                    razonSocial = lee_nodo_xml(root, "razonSocial");
                    nombreComercial = lee_nodo_xml(root, "nombreComercial");
                    ruc = lee_nodo_xml(root, "ruc");
                    codDoc = obtener_codigo(lee_nodo_xml(root, "codDoc"));
                    String DOC = obtener_codigo(lee_nodo_xml(root, "codDoc"));
                    codDoc = DOC == null ? lee_nodo_xml(root, "codDoc") : DOC;
                    estab = lee_nodo_xml(root, "estab");
                    ptoEmi = lee_nodo_xml(root, "ptoEmi");
                    secuencial_doc = lee_nodo_xml(root, "secuencial");

                    secuencial = obtenerSecuencial(secuencial_doc);
                    clsLogger.Graba_Log_Info("ingresando  a procesarArchivoXML paso 3 ");
                    if (codDoc.Equals("05A"))
                    {
                        termino = codDoc;
                        esNDFinLF = true;
                        codDoc = "05";
                    }
                    if (codDoc.Equals("EF"))
                    {
                        termino = codDoc;
                        esNDFinLF = true;
                        codDoc = "01";
                    }

                    if (valida_duplicidad(codDoc, estab, ptoEmi, secuencial, ambiente.Trim()))
                    {
                        //lg1.guardar_Log("ingresando  a procesarArchivoXML paso 4 ");

                        String[] SapasInfoAdicionales;
                        string sap_codigoControl = "", sap_idcomprobante = "", sap_claveAcceso = "", sap_numAutorizacion = "", sap_fechaAutorizacion = "", sapinfoAdicionalNombre = "", sapinfoAdicionalValor = "", sap_anioasiento = "", sap_num_asiento = "", sap_sociedad = "";
                        string sap_fechaemision = lee_nodo_xml(root, "fechaEmision");
                        if (sap_fechaemision.Equals("") || sap_fechaemision == null) { sap_fechaemision = DateTime.Now.ToString("dd/MM/yyyy"); }


                        banErrorArchivo = true;
                        msj = "No se pudo procesar el archivo. " + msjLectura(banderaArchivo);
                        msjT = "Se intentó procesar un secuencial ya registrado";
                        if (String.IsNullOrEmpty(codigoControl))
                            codigoControl = ruc + codDoc + estab + ptoEmi + secuencial + DateTime.Now.ToString("yyyyMMddHHmm");
                        sap_codigoControl = ruc + codDoc + estab + ptoEmi + secuencial + Convert.ToDateTime(sap_fechaemision).ToString("yyyyMMddHHmm");
                        //DB.Desconectar();
                        DB.Conectar();
                        DB.CrearComando("select idComprobante,claveAcceso,numeroAutorizacion,fechaAutorizacion  from GENERAL with(nolock) where codigoControl = @codigo_control");
                        DB.AsignarParametroCadena("@codigo_control", sap_codigoControl);
                        using (DbDataReader DR5 = DB.EjecutarConsulta())
                        {
                            if (DR5.Read())
                            {
                                sap_idcomprobante = DR5.GetInt32(0).ToString();
                                sap_claveAcceso = DR5.GetString(1);
                                sap_numAutorizacion = DR5.GetString(2);
                                sap_fechaAutorizacion = DR5.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                        }

                        DB.Desconectar();
                        DB.Conectar();
                        DB.CrearComando("select nombre,valor  from InfoAdicional with(nolock) where id_Comprobante = @id_comp");
                        DB.AsignarParametroCadena("@id_comp", sap_idcomprobante);
                        using (DbDataReader DR4 = DB.EjecutarConsulta())
                        {
                            while (DR4.Read())
                            {

                                sapinfoAdicionalNombre = DR4.GetString(0);
                                sapinfoAdicionalValor = DR4.GetString(1);
                                if (sapinfoAdicionalNombre.Equals("sociedad")) { sap_sociedad = DR4.GetString(1); }
                                if (sapinfoAdicionalNombre.Equals("numeroAsientoContable")) { sap_num_asiento = DR4.GetString(1); }
                                if (sapinfoAdicionalNombre.Equals("anioAsientoContable")) { sap_anioasiento = DR4.GetString(1); }

                            }
                        }

                        DB.Desconectar();
                        RespuestaWebUNASAP(codDoc, estab + "-" + ptoEmi + "-" + secuencial, sap_claveAcceso, sap_numAutorizacion, sap_fechaAutorizacion, sap_fechaAutorizacion, "", "", "", "AT", "AT", sap_sociedad, sap_num_asiento, sap_anioasiento);
                        enviar_notificacion_correo_punto(estab, codDoc + estab + ptoEmi + secuencial, sap_fechaemision, msjT);

                        //consulta datos a la base 
                        // enviar respuesta 
                        // correo notificacion 
                    }


                    //lg1.guardar_Log("ingresando  a procesarArchivoXML paso 5 ");
                    dirMatriz = lee_nodo_xml(root, "dirMatriz");
                    emails = "";

                    gBD.InformacionTributaria(ambiente, tipoEmision, razonSocial, nombreComercial, ruc, claveAcceso, codDoc, estab, ptoEmi, secuencial, dirMatriz, emails);
                    offline.InformacionTributaria(ambiente, tipoEmision, razonSocial, nombreComercial, ruc, claveAcceso, codDoc, estab, ptoEmi, secuencial, dirMatriz, emails, version);
                    gXml.InformacionTributaria(ambiente, tipoEmision, razonSocial, nombreComercial, ruc, claveAcceso, codDoc, estab, ptoEmi, secuencial, dirMatriz);
                    mensajeBitacora += "Se proceso InfoTributario";

                }

                //IC
                if (banderaArchivo == 1)
                {
                    banderaArchivo = 2;
                    fechaEmision = lee_nodo_xml(root, "fechaEmision"); if (fechaEmision.Equals("") || fechaEmision == null) fechaEmision = DateTime.Now.ToString("dd/MM/yyyy");
                    dirEstablecimiento = lee_nodo_xml(root, "dirEstablecimiento"); // obtener_dir_Sucursal(estab); 
                    if (codDoc.Equals("06"))
                    {
                        dirEstablecimientoGuia = lee_nodo_xml(root, "dirEstablecimiento");
                    }
                    contribuyenteEspecial = lee_nodo_xml(root, "contribuyenteEspecial");
                    obligadoContabilidad = lee_nodo_xml(root, "obligadoContabilidad");

                    if (codDoc.Equals("03"))
                    {
                        razonSocialComprador = lee_nodo_xml(root, "razonSocialProveedor");
                        identificacionComprador = lee_nodo_xml(root, "identificacionProveedor");
                        tipoIdentificacionComprador = obtener_codigo_tipoIdentificacion(lee_nodo_xml(root, "tipoIdentificacionProveedor"), identificacionComprador);
                        direccionComprador = lee_nodo_xml(root, "direccionProveedor");
                    }
                    else
                    {
                        razonSocialComprador = lee_nodo_xml(root, "razonSocialComprador");
                        identificacionComprador = lee_nodo_xml(root, "identificacionComprador");
                        tipoIdentificacionComprador = obtener_codigo_tipoIdentificacion(lee_nodo_xml(root, "tipoIdentificacionComprador"), identificacionComprador);
                        direccionComprador = lee_nodo_xml(root, "direccionComprador");
                    }


                    totalSinImpuestos = lee_nodo_xml(root, "totalSinImpuestos");
                    totalDescuento = lee_nodo_xml(root, "totalDescuento");
                    //lg1.guardar_Log("ingresando  a procesarArchivoXML paso 6 ");


                    #region "exportacion"
                    if (!lee_nodo_xml(root, "comercioExterior").Equals(""))
                    {
                        this.comercioExterior = this.lee_nodo_xml(root, "comercioExterior");
                        this.incoTermFactura = this.lee_nodo_xml(root, "incoTermFactura");
                        this.lugarIncoTerm = this.lee_nodo_xml(root, "lugarIncoTerm");
                        this.paisOrigen = this.lee_nodo_xml(root, "paisOrigen");
                        this.puertoEmbarque = this.lee_nodo_xml(root, "puertoEmbarque");
                        this.puertoDestino = this.lee_nodo_xml(root, "puertoDestino");
                        this.paisDestino = this.lee_nodo_xml(root, "paisDestino");
                        this.paisAdquisicion = this.lee_nodo_xml(root, "paisAdquisicion");
                        this.incoTermTotalSinImpuestos = this.lee_nodo_xml(root, "incoTermTotalSinImpuestos");
                        this.fleteInternacional = this.lee_nodo_xml(root, "fleteInternacional");
                        this.seguroInternacional = this.lee_nodo_xml(root, "seguroInternacional");
                        this.gastosAduaneros = this.lee_nodo_xml(root, "gastosAduaneros");
                        this.gastosTransporteOtros = this.lee_nodo_xml(root, "gastosTransporteOtros");
                        exp = "EXTERIOR";

                        this.gXml.informacionDocumentoExportacion(this.comercioExterior, this.incoTermFactura, this.lugarIncoTerm, this.paisOrigen, this.puertoEmbarque, this.puertoDestino, this.paisDestino, this.paisAdquisicion, this.incoTermTotalSinImpuestos, this.fleteInternacional, this.seguroInternacional, this.gastosAduaneros, this.gastosTransporteOtros);
                        this.gBD.informacionDocumentoExportacion(this.comercioExterior, this.incoTermFactura, this.lugarIncoTerm, this.paisOrigen, this.puertoEmbarque, this.puertoDestino, this.paisDestino, this.paisAdquisicion, this.incoTermTotalSinImpuestos, this.fleteInternacional, this.seguroInternacional, this.gastosAduaneros, this.gastosTransporteOtros);
                        this.offline.informacionDocumentoExportacion(this.comercioExterior, this.incoTermFactura, this.lugarIncoTerm, this.paisOrigen, this.puertoEmbarque, this.puertoDestino, this.paisDestino, this.paisAdquisicion, this.incoTermTotalSinImpuestos, this.fleteInternacional, this.seguroInternacional, this.gastosAduaneros, this.gastosTransporteOtros);

                    }
                    #endregion



                    #region pagos
                    if (codDoc != "07" && (version != "2.0.0" || version != "2.0"))
                    {
                        XmlElement xmlElement = (XmlElement)xmlbase.GetElementsByTagName("pagos")[0];
                        if (xmlElement != null)
                        {
                            foreach (XmlElement p_root in xmlElement)
                            {
                                this.asPagos = new string[5];
                                this.asPagos[0] = this.lee_nodo_xml(p_root, "formaPago");
                                this.asPagos[1] = this.lee_nodo_xml(p_root, "total");
                                this.asPagos[2] = this.lee_nodo_xml(p_root, "plazo");
                                this.asPagos[3] = this.lee_nodo_xml(p_root, "unidadTiempo");
                                this.arraylPagos.Add(this.asPagos);
                                this.gBD.DetallePagos(this.arraylPagos);
                                this.offline.DetallePagos(this.arraylPagos);
                                this.gXml.DetallePagos(this.arraylPagos);
                            }
                        }
                    }
                    #endregion

                    #region compensaciones
                    if (codDoc.Equals("01") || codDoc.Equals("04") || codDoc.Equals("05"))
                    {
                        XmlElement xmlElementc = (XmlElement)xmlbase.GetElementsByTagName("compensaciones")[0];
                        if (xmlElementc != null)
                        {
                            foreach (XmlElement p_rootc in xmlElementc)
                            {
                                this.asCompensacion = new string[3];
                                this.asCompensacion[0] = this.lee_nodo_xml(p_rootc, "codigo");
                                this.asCompensacion[1] = this.lee_nodo_xml(p_rootc, "tarifa");
                                this.asCompensacion[2] = this.lee_nodo_xml(p_rootc, "valor");
                                this.arraylCompensacion.Add(this.asCompensacion);
                                this.gBD.DetalleCompensacion(this.arraylCompensacion);
                                this.offline.DetalleCompensacion(this.arraylCompensacion);
                                this.gXml.DetalleCompensacion(this.arraylCompensacion);
                            }
                        }
                    }
                    #endregion
                    propina = lee_nodo_xml(root, "propina");
                    importeTotal = lee_nodo_xml(root, "importeTotal");
                    moneda = lee_nodo_xml(root, "moneda");

                    numDocModificado = lee_nodo_xml(root, "numDocModificado");

                    if (identificacionComprador.Equals("9999999999999"))
                    {
                        tipoIdentificacionComprador = "07";
                        razonSocialComprador = "CONSUMIDOR FINAL";
                    }
                    else
                    {
                        if (identificacionComprador.Length == 13 && identificacionComprador.Substring(10).Equals("001"))
                        {
                            //es ruc
                            tipoIdentificacionComprador = "04";
                        }
                    }
                    mensajeBitacora += "entra a validar id " + tipoIdentificacionComprador;
                    //if (tipoIdentificacionComprador.Equals("04") || tipoIdentificacionComprador.Equals("05"))
                    //{
                    //    if (!Valiruc.ValidarNumeroIdentificacion(identificacionComprador))
                    //    {
                    //        mensajeBitacora += "validó false tipoID";
                    //        banErrorArchivo = true;
                    //        msj = "Número de cédula o RUC incorrecto";
                    //        msjT = "La cédula/RUC no superó la validación del dígito";
                    //        if (String.IsNullOrEmpty(codigoControl))
                    //            codigoControl = ruc + codDoc + estab + ptoEmi + secuencial + DateTime.Now.ToString("yyyyMMddHHmm");

                    //    }
                    //    else
                    //        mensajeBitacora += "validó true tipoID";
                    //}

                    //N/C
                    rise = lee_nodo_xml(root, "rise");
                    codDocModificado = obtener_codigo(lee_nodo_xml(root, "codDocModificado"));
                    fechaEmisionDocSustentoNota = lee_nodo_xml(root, "fechaEmisionDocSustento");
                    valorModificacion = lee_nodo_xml(root, "valorModificacion");
                    if (string.IsNullOrEmpty(valorModificacion))
                    {
                        valorModificacion = "0";
                    }
                    motivo = lee_nodo_xml(root, "motivo").Trim();
                    guiaRemision = lee_nodo_xml(root, "guiaRemision");

                    //Guia Remision
                    placa = lee_nodo_xml(root, "placa");
                    fechaFinTransporte = lee_nodo_xml(root, "fechaFinTransporte");
                    fechaIniTransporte = lee_nodo_xml(root, "fechaIniTransporte");
                    rise = lee_nodo_xml(root, "rise");
                    rucTransportista = lee_nodo_xml(root, "rucTransportista");
                    tipoIdentificacionTransportista = obtener_codigo_tipoIdentificacion(lee_nodo_xml(root, "tipoIdentificacionTransportista"), rucTransportista);
                    razonSocialTransportista = lee_nodo_xml(root, "razonSocialTransportista");
                    dirPartida = lee_nodo_xml(root, "dirPartida");

                    mensajeBitacora += "validara nc codDoc = " + codDoc;
                    if (codDoc.Equals("07")) // NOTA DE CREDITO
                    {
                        try
                        {
                            codDocModificado = obtener_codigo(lee_nodo_xml(root, "codDocSustento"));
                            numDocModificado = lee_nodo_xml(root, "numDocSustento");
                        }
                        catch (Exception ex) { }
                    }
                    if (esNDFinLF)
                    {

                        try
                        {
                            DB.Conectar();
                            DB.CrearComando(@"select top 1 g.codDoc, g.estab + '-' + g.ptoEmi + '-' + g.secuencial as numDoc, convert(varchar(50),g.fecha,103) as fecha from GENERAL g with(nolock) inner join RECEPTOR r with(nolock) on g.id_Receptor = r.IDEREC where r.RFCREC = @p_ruc and codDoc = '01' and estab = @p_estab order by g.idComprobante desc");
                            DB.AsignarParametroCadena("@p_ruc", ruc);
                            DB.AsignarParametroCadena("@p_estab", estab);
                            using (DbDataReader DR5 = DB.EjecutarConsulta())
                            {
                                if (DR5.Read())
                                {
                                    codDocModificado = DR5["codDoc"].ToString();
                                    numDocModificado = DR5["numDoc"].ToString();
                                    fechaEmisionDocSustentoNota = DR5["fecha"].ToString();
                                }
                                else
                                {
                                    codDocModificado = "01";
                                    numDocModificado = "001-001-000000001";
                                    fechaEmisionDocSustentoNota = DateTime.Now.ToString("dd/MM/yyyy");
                                }
                            }

                            DB.Desconectar();

                        }
                        catch (Exception ex)
                        {
                            DB.Desconectar();
                            clsLogger.Graba_Log_Error(ex.Message);
                            log.mensajesLog("BD001", claveAcceso, ex.Message, "", codigoControl, " Error en consulta documento modificad ND Financiera.");

                        }

                    }

                    mensajeBitacora += "entra a guardar informacion doc";
                    gBD.infromacionDocumento(fechaEmision, dirEstablecimiento, dirEstablecimientoGuia, contribuyenteEspecial, obligadoContabilidad, tipoIdentificacionComprador, guiaRemision, razonSocialComprador, identificacionComprador, moneda, dirPartida, razonSocialTransportista, tipoIdentificacionTransportista, rucTransportista, rise, fechaIniTransporte, fechaFinTransporte, placa, codDocModificado, numDocModificado, fechaEmisionDocSustentoNota, valorModificacion, motivo, direccionComprador);
                    offline.infromacionDocumento(fechaEmision, dirEstablecimiento, dirEstablecimientoGuia, contribuyenteEspecial, obligadoContabilidad, tipoIdentificacionComprador, guiaRemision, razonSocialComprador, identificacionComprador, moneda, dirPartida, razonSocialTransportista, tipoIdentificacionTransportista, rucTransportista, rise, fechaIniTransporte, fechaFinTransporte, placa, codDocModificado, numDocModificado, fechaEmisionDocSustentoNota, valorModificacion, motivo, direccionComprador);
                    gXml.infromacionDocumento(fechaEmision, dirEstablecimiento, contribuyenteEspecial, obligadoContabilidad, tipoIdentificacionComprador, guiaRemision, razonSocialComprador, identificacionComprador, moneda, dirPartida, razonSocialTransportista, tipoIdentificacionTransportista, rucTransportista, rise, fechaIniTransporte, fechaFinTransporte, placa, codDocModificado, numDocModificado, fechaEmisionDocSustentoNota, valorModificacion, motivo, direccionComprador);

                    mensajeBitacora += " Se proceso InfoDocumento";
                }
                //lg1.guardar_Log("ingresando  a procesarArchivoXML paso 7 ");
                #region "Nota de Debito"
                if (codDoc.Equals("05"))
                {

                    XmlElement detalles05 = (XmlElement)xmlbase.GetElementsByTagName("detalles")[0];

                    if (detalles05 != null)
                    {
                        lee_Detalles(detalles05);
                    }
                    else
                    {
                        XmlElement impuestos = (XmlElement)xmlbase.GetElementsByTagName("impuestos")[0];
                        if (impuestos != null)
                        {
                            lee_Impuestos(impuestos);
                        }
                    }

                    if (banderaArchivo == 2 && !banErrorArchivo)
                    {
                        //banderaArchivo = 6;

                        XmlElement infoMotivos = (XmlElement)xmlbase.GetElementsByTagName("motivos")[0];
                        foreach (XmlElement tmotivos in infoMotivos)
                        {
                            asMotivos = new String[2];
                            motivoRazon = lee_nodo_xml(tmotivos, "razon").Trim();
                            motivoValor = lee_nodo_xml(tmotivos, "valor");
                            asMotivos[0] = motivoRazon; asMotivos[1] = motivoValor;
                            arraylMotivos.Add(asMotivos);
                            gBD.Motivos(arraylMotivos);
                            offline.Motivos(arraylMotivos);
                            gXml.Motivos(arraylMotivos);
                        }
                        gBD.detalles(arraylDetalles); gBD.motivoND(motivoRazon); offline.motivoND(motivoRazon);//LF motivo
                        offline.detalles(arraylDetalles);
                        gXml.detalles(arraylDetalles);
                    }
                    importeTotal = lee_nodo_xml(root, "valorTotal");
                    XmlElement totalConImpuestosiva = null;

                    double tsi = double.Parse(valida_texto_a_numero(totalSinImpuestos), CultureInfo.InvariantCulture);
                    double it = double.Parse(valida_texto_a_numero(importeTotal), CultureInfo.InvariantCulture);
                    double v_iva = it - tsi;

                    if (v_iva > 0)
                    {
                        totalConImpuestosiva = (XmlElement)xmlbase.GetElementsByTagName("impuestos")[0];

                        foreach (XmlElement tiIVA in totalConImpuestosiva)
                        {
                            codigoPorcentaje = obtener_codigo(lee_nodo_xml(tiIVA, "codigoPorcentaje"));
                            if (codigoPorcentaje.Equals("2") || codigoPorcentaje.Equals("3"))
                            {
                                double IvaPorcentaje = obtener_iva(codigoPorcentaje);

                                double s12 = (v_iva / IvaPorcentaje);
                                if (Math.Round(s12, 0) == Math.Round(tsi, 0))
                                {
                                    subtotal12 = tsi.ToString("F2", CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    subtotal12 = s12.ToString("F2", CultureInfo.InvariantCulture);
                                    subtotal0 = (tsi - s12).ToString("F2", CultureInfo.InvariantCulture);
                                }
                            }
                        }
                    }
                    else
                    {
                        subtotal12 = "0";
                        subtotal0 = valida_texto_a_numero(totalSinImpuestos);
                    }

                    subtotal12 = valida_texto_a_numero(subtotal12);
                    subtotal0 = valida_texto_a_numero(subtotal0);
                    totalSinImpuestos = valida_texto_a_numero(totalSinImpuestos);
                    IVA12 = valida_texto_a_numero(v_iva.ToString("F2", CultureInfo.InvariantCulture));
                    importeTotal = valida_texto_a_numero(importeTotal);

                    gBD.cantidades(subtotal12, subtotal0, "0", totalSinImpuestos, "0", "0", IVA12, importeTotal, "0", importeTotal);
                    offline.cantidades(subtotal12, subtotal0, "0", totalSinImpuestos, "0", "0", IVA12, importeTotal, "0", importeTotal);
                    gXml.cantidades(subtotal12, subtotal0, "0", totalSinImpuestos, "0", "0", IVA12, importeTotal, "0", importeTotal);
                }
                #endregion

                #region "Renteciones"
                if (codDoc.Equals("07"))
                {
                    if (banderaArchivo == 2 && !banErrorArchivo)
                    {
                        if (version == "2.0.0" || version == "2.0")
                        {
                            banderaArchivo = 3;
                            XmlElement infoCompRetencion = (XmlElement)xmlbase.GetElementsByTagName("infoCompRetencion")[0];
                            periodoFiscal = lee_nodo_xml(infoCompRetencion, "periodoFiscal");
                            tipoIdentificacionSujetoRetenido = lee_nodo_xml(infoCompRetencion, "tipoIdentificacionSujetoRetenido");
                            tipoIdentificacionSujetoRetenido = obtener_codigo_tipoIdentificacion(lee_nodo_xml(infoCompRetencion, "tipoIdentificacionSujetoRetenido"), identificacionSujetoRetenido);
                            if (tipoIdentificacionSujetoRetenido == "08")
                            {
                                tipoSujetoRetenido = lee_nodo_xml(infoCompRetencion, "tipoSujetoRetenido");
                            }
                            parteRel = lee_nodo_xml(infoCompRetencion, "parteRel");
                            razonSocialSujetoRetenido = lee_nodo_xml(infoCompRetencion, "razonSocialSujetoRetenido");
                            identificacionSujetoRetenido = lee_nodo_xml(infoCompRetencion, "identificacionSujetoRetenido");
                            gXml.comprobanteRetencion(periodoFiscal, tipoIdentificacionSujetoRetenido, razonSocialSujetoRetenido, identificacionSujetoRetenido, tipoSujetoRetenido, parteRel);
                            gBD.comprobanteRetencion(periodoFiscal, tipoIdentificacionSujetoRetenido, razonSocialSujetoRetenido, identificacionSujetoRetenido, tipoSujetoRetenido, parteRel);
                            offline.comprobanteRetencion(periodoFiscal, tipoIdentificacionSujetoRetenido, razonSocialSujetoRetenido, identificacionSujetoRetenido, tipoSujetoRetenido, parteRel);
                            XmlElement docsSustento = (XmlElement)xmlbase.GetElementsByTagName("docsSustento")[0];
                            int countdocsSustento = 0;
                            foreach (XmlElement docSustento in docsSustento)
                            {
                                countdocsSustento++;
                                asDocsSustentos = new String[18];
                                asDocsSustentos[0] = countdocsSustento.ToString();
                                asDocsSustentos[1] = lee_nodo_xml(docSustento, "codSustento");
                                asDocsSustentos[2] = codDocSustento = obtener_codigo(lee_nodo_xml(docSustento, "codDocSustento"));
                                asDocsSustentos[3] = numDocSustento = lee_nodo_xml(docSustento, "numDocSustento");
                                asDocsSustentos[4] = fechaEmisionDocSustento = lee_nodo_xml(docSustento, "fechaEmisionDocSustento");
                                asDocsSustentos[5] = lee_nodo_xml(docSustento, "fechaRegistroContable");
                                asDocsSustentos[6] = lee_nodo_xml(docSustento, "numAutDocSustento");
                                asDocsSustentos[7] = lee_nodo_xml(docSustento, "pagoLocExt");
                                asDocsSustentos[8] = lee_nodo_xml(docSustento, "tipoRegi");
                                asDocsSustentos[9] = lee_nodo_xml(docSustento, "paisEfecPago");
                                asDocsSustentos[10] = lee_nodo_xml(docSustento, "aplicConvDobTrib");
                                asDocsSustentos[11] = lee_nodo_xml(docSustento, "pagExtSujRetNorLeg");
                                asDocsSustentos[12] = lee_nodo_xml(docSustento, "pagoRegFis");
                                asDocsSustentos[13] = lee_nodo_xml(docSustento, "totalComprobantesReembolso");
                                asDocsSustentos[14] = lee_nodo_xml(docSustento, "totalBaseImponibleReembolso");
                                asDocsSustentos[15] = lee_nodo_xml(docSustento, "totalImpuestoReembolso");
                                asDocsSustentos[16] = lee_nodo_xml(docSustento, "totalSinImpuestos");
                                asDocsSustentos[17] = lee_nodo_xml(docSustento, "importeTotal");
                                arraylDocsSustentos.Add(asDocsSustentos);
                                gBD.docsSustentos(arraylDocsSustentos);
                                gXml.docsSustentos(arraylDocsSustentos);
                                offline.DocSustentos(arraylDocsSustentos);

                                XmlElement impuestosSustentos = (XmlElement)docSustento.GetElementsByTagName("impuestosDocSustento")[0];
                                if (impuestosSustentos != null)
                                    lee_ImpuestosRet2(impuestosSustentos, false, countdocsSustento.ToString());

                                XmlElement retencionesSustentos = (XmlElement)docSustento.GetElementsByTagName("retenciones")[0];
                                if (retencionesSustentos != null)
                                    lee_ImpuestosRet2(retencionesSustentos, true, countdocsSustento.ToString());

                                XmlElement reembolsosSustentos = (XmlElement)docSustento.GetElementsByTagName("reembolsos")[0];
                                if (reembolsosSustentos != null)
                                    lee_ReembolsosSustentos(reembolsosSustentos, countdocsSustento.ToString());

                                #region pagos
                                if (codDoc.Equals("07") & (version.Equals("2.0.0") || version.Equals("2.0")))
                                {
                                    XmlElement xmlElement = (XmlElement)docSustento.GetElementsByTagName("pagos")[0];
                                    if (xmlElement != null)
                                    {
                                        foreach (XmlElement p_root in xmlElement)
                                        {
                                            this.asPagos = new string[3];
                                            this.asPagos[0] = countdocsSustento.ToString();
                                            this.asPagos[1] = this.lee_nodo_xml(p_root, "formaPago");
                                            this.asPagos[2] = this.lee_nodo_xml(p_root, "total");
                                            this.arraylPagos.Add(this.asPagos);
                                            this.gBD.DetallePagos(this.arraylPagos);
                                            this.gXml.DetallePagos(this.arraylPagos);
                                            this.offline.DetallePagos(this.arraylPagos);
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            banderaArchivo = 3;
                            XmlElement infoCompRetencion = (XmlElement)xmlbase.GetElementsByTagName("infoCompRetencion")[0];
                            periodoFiscal = lee_nodo_xml(infoCompRetencion, "periodoFiscal");

                            razonSocialSujetoRetenido = lee_nodo_xml(infoCompRetencion, "razonSocialSujetoRetenido");
                            identificacionSujetoRetenido = lee_nodo_xml(infoCompRetencion, "identificacionSujetoRetenido");
                            tipoIdentificacionSujetoRetenido = obtener_codigo_tipoIdentificacion(lee_nodo_xml(infoCompRetencion, "tipoIdentificacionSujetoRetenido"), identificacionSujetoRetenido);


                            gXml.comprobanteRetencion(periodoFiscal, tipoIdentificacionSujetoRetenido, razonSocialSujetoRetenido, identificacionSujetoRetenido);
                            gBD.comprobanteRetencion(periodoFiscal, tipoIdentificacionSujetoRetenido, razonSocialSujetoRetenido, identificacionSujetoRetenido);
                            offline.comprobanteRetencion(periodoFiscal, tipoIdentificacionSujetoRetenido, razonSocialSujetoRetenido, identificacionSujetoRetenido);
                            mensajeBitacora += " Se proceso CompRetención";

                            //IM
                            XmlElement impuestos = (XmlElement)xmlbase.GetElementsByTagName("impuestos")[0];
                            lee_Impuestos(impuestos);


                        }
                    }
                    if (version == "2.0.0" || version == "2.0")
                    {
                        gBD.cantidades("0", "0", "0", valida_texto_a_numero(totalSinImpuestos), "0", "0", "0", valida_texto_a_numero(importeTotal), "0", "0");
                        offline.cantidades("0", "0", "0", valida_texto_a_numero(totalSinImpuestos), "0", "0", "0", valida_texto_a_numero(importeTotal), "0", "0");
                        gXml.cantidades("0", "0", "0", valida_texto_a_numero(totalSinImpuestos), "0", "0", "0", valida_texto_a_numero(importeTotal), "0", "0");

                    }
                    else
                    {
                        gBD.cantidades("0", "0", "0", "0", "0", "0", "0", "0", "0", "0");
                        offline.cantidades("0", "0", "0", "0", "0", "0", "0", "0", "0", "0");
                        gXml.cantidades("0", "0", "0", "0", "0", "0", "0", "0", "0", "0");
                    }


                    razonSocialComprador = razonSocialSujetoRetenido;

                }
                #endregion

                //Factura y Nota de Credito
                //lg1.guardar_Log("ingresando  a procesarArchivoXML paso 8 ");
                #region "Factura y Nota de Credito"
                if (codDoc.Equals("01") || codDoc.Equals("04") || codDoc.Equals("03"))
                {
                    //////T y TI
                    if (banderaArchivo == 2 && !banErrorArchivo)
                    {
                        banderaArchivo = 3;

                        XmlElement totalConImpuestos = (XmlElement)xmlbase.GetElementsByTagName("totalConImpuestos")[0];

                        if (totalConImpuestos == null)
                            totalConImpuestos = (XmlElement)xmlbase.GetElementsByTagName("impuestos")[0];

                        foreach (XmlElement ti in totalConImpuestos)
                        {
                            String descuentoAdicional = this.lee_nodo_xml(ti, "descuentoAdicional");
                            asTotalImpuestos = new String[9];
                            codigo = obtener_codigo(lee_nodo_xml(ti, "codigo"));
                            codigoPorcentaje = obtener_codigo(lee_nodo_xml(ti, "codigoPorcentaje"));

                            baseImponible = lee_nodo_xml(ti, "baseImponible");
                            if (!string.IsNullOrEmpty(baseImponible))
                            {
                                asInfoAdicionales = new String[4];
                                asInfoAdicionales[0] = "baseImponible";
                                asInfoAdicionales[1] = baseImponible;
                                asInfoAdicionales[2] = codigo;
                                asInfoAdicionales[3] = codigoPorcentaje;
                                arraylTotalConImpuestos.Add(asInfoAdicionales);
                                gBD.totalConImpuestos(arraylTotalConImpuestos);
                                offline.totalConImpuestos(arraylTotalConImpuestos);
                                gXml.totalImpuestos(arraylTotalConImpuestos);
                                mensajeBitacora += " Se proceso Totales 2";
                            }


                            tarifa = lee_nodo_xml(ti, "tarifa");
                            if (!string.IsNullOrEmpty(tarifa))
                            {
                                asInfoAdicionales = new String[4];
                                asInfoAdicionales[0] = "tarifa";
                                asInfoAdicionales[1] = tarifa;
                                asInfoAdicionales[2] = codigo;
                                asInfoAdicionales[3] = codigoPorcentaje;
                                arraylTotalConImpuestos.Add(asInfoAdicionales);
                                gBD.totalConImpuestos(arraylTotalConImpuestos);
                                offline.totalConImpuestos(arraylTotalConImpuestos);
                                gXml.totalImpuestos(arraylTotalConImpuestos);
                                mensajeBitacora += " Se proceso Totales 2";
                            }
                            else
                            {
                                DB.Conectar();
                                using (DataTable dtImpuesto = DB.TraerDataSetConsulta("select codigo , descripcion, valor from CatImpuestos_C with(nolock) where tipo = 'IVA' and codigo = " + codigoPorcentaje, new Object[] { }).Tables[0])
                                {
                                    if (dtImpuesto.Rows.Count > 0)
                                        tarifa = dtImpuesto.Rows[0]["valor"].ToString();
                                }

                                DB.Desconectar();

                                asInfoAdicionales = new String[4];
                                asInfoAdicionales[0] = "tarifa";
                                asInfoAdicionales[1] = tarifa;
                                asInfoAdicionales[2] = codigo;
                                asInfoAdicionales[3] = codigoPorcentaje;
                                arraylTotalConImpuestos.Add(asInfoAdicionales);
                                gBD.totalConImpuestos(arraylTotalConImpuestos);
                            }

                            valor = lee_nodo_xml(ti, "valor");
                            if (!string.IsNullOrEmpty(valor))
                            {
                                asInfoAdicionales = new String[4];
                                asInfoAdicionales[0] = "valor";
                                asInfoAdicionales[1] = valor;
                                asInfoAdicionales[2] = codigo;
                                asInfoAdicionales[3] = codigoPorcentaje;
                                arraylTotalConImpuestos.Add(asInfoAdicionales);
                                gBD.totalConImpuestos(arraylTotalConImpuestos);
                                offline.totalConImpuestos(arraylTotalConImpuestos);
                                gXml.totalImpuestos(arraylTotalConImpuestos);
                                mensajeBitacora += " Se proceso Totales 2";
                            }

                        }


                        foreach (XmlElement ti in totalConImpuestos)
                        {
                            asTotalImpuestos = new String[8];

                            if (codDoc.Equals("01") && !lee_nodo_xml(root, "comercioExterior").Equals(""))
                            {
                                codigo = obtener_codigo(lee_nodo_xml(ti, "codigo"));
                            }
                            else
                            {
                                //codigo = lee_nodo_xml(ti, "codigo");
                                codigo = obtener_codigo(lee_nodo_xml(ti, "codigo"));
                            }

                            switch (codigo)
                            {
                                case "2": // IVA
                                    if (codDoc.Equals("01") && !lee_nodo_xml(root, "comercioExterior").Equals(""))
                                    {
                                        codigoPorcentaje = obtener_codigo(lee_nodo_xml(ti, "codigoPorcentaje"));
                                    }
                                    else
                                    {
                                        //codigoPorcentaje = lee_nodo_xml(ti, "codigoPorcentaje");
                                        codigoPorcentaje = obtener_codigo(lee_nodo_xml(ti, "codigoPorcentaje"));
                                    }

                                    switch (codigoPorcentaje)
                                    {
                                        case "0": // 0%
                                            subtotal0 = lee_nodo_xml(ti, "baseImponible");
                                            baseImponible = subtotal0;
                                            tarifa = lee_nodo_xml(ti, "tarifa");
                                            valor = lee_nodo_xml(ti, "valor");
                                            tipoImpuesto = "IVA";
                                            break;
                                        //case "2":
                                        //case "3"://12% - 14%
                                        //    subtotal12 = lee_nodo_xml(ti, "baseImponible");
                                        //    baseImponible = subtotal12;
                                        //    tarifa = lee_nodo_xml(ti, "tarifa");
                                        //    valor = lee_nodo_xml(ti, "valor");
                                        //    tipoImpuesto = "IVA";
                                        //    IVA12 = valor;
                                        //    break;
                                        case "6": //No Objeto
                                            subtotalNoSujeto = lee_nodo_xml(ti, "baseImponible");
                                            baseImponible = subtotalNoSujeto;
                                            tarifa = lee_nodo_xml(ti, "tarifa");
                                            valor = lee_nodo_xml(ti, "valor");
                                            tipoImpuesto = "IVA";
                                            break;
                                        //case "7": // 0%
                                        //    subtotal0 = lee_nodo_xml(ti, "baseImponible");
                                        //    baseImponible = subtotal0;
                                        //    tarifa = lee_nodo_xml(ti, "tarifa");
                                        //    valor = lee_nodo_xml(ti, "valor");
                                        //    tipoImpuesto = "IVA";
                                        //    break;
                                        default:
                                            subtotal12 = lee_nodo_xml(ti, "baseImponible");
                                            baseImponible = subtotal12;
                                            tarifa = lee_nodo_xml(ti, "tarifa");
                                            if (string.IsNullOrEmpty(this.tarifa))
                                            {
                                                DB.Conectar();
                                                using (DataTable dtImpuesto = DB.TraerDataSetConsulta("select codigo , descripcion, valor from CatImpuestos_C with(nolock) where tipo = 'IVA' and codigo = " + codigoPorcentaje, new Object[] { }).Tables[0])
                                                {
                                                    if (dtImpuesto.Rows.Count > 0)
                                                        tarifa = dtImpuesto.Rows[0]["valor"].ToString();
                                                    else
                                                        tarifa = "0.00";
                                                }
                                            }
                                            valor = lee_nodo_xml(ti, "valor");
                                            tipoImpuesto = "IVA";
                                            IVA12 = valor;
                                            break;
                                    }

                                    break;
                                case "3": //ICE
                                    codigoPorcentaje = obtener_codigo(lee_nodo_xml(ti, "codigoPorcentaje"));
                                    baseImponible = lee_nodo_xml(ti, "baseImponible");
                                    tarifa = lee_nodo_xml(ti, "tarifa");
                                    valor = lee_nodo_xml(ti, "valor");
                                    ICE = valor;
                                    tipoImpuesto = "ICE";
                                    break;


                            }
                            if (codDoc.Equals("04"))
                            {
                                if (codigoPorcentaje.Equals("3"))
                                    tarifa = "14";
                                else if (codigoPorcentaje.Equals("2"))
                                    tarifa = "12";
                                else
                                    tarifa = "0";
                                //Solo LaFarge
                                //importeTotal = (double.Parse(valida_texto_a_numero(importeTotal), CultureInfo.InvariantCulture) + double.Parse(valida_texto_a_numero(baseImponible), CultureInfo.InvariantCulture) + double.Parse(valida_texto_a_numero(valor), CultureInfo.InvariantCulture)).ToString("F2", CultureInfo.InvariantCulture);
                                importeTotal = valorModificacion;
                            }

                            asTotalImpuestos[0] = codigo; asTotalImpuestos[1] = codigoPorcentaje; asTotalImpuestos[2] = baseImponible;
                            asTotalImpuestos[3] = tarifa; asTotalImpuestos[4] = valor; asTotalImpuestos[5] = tipoImpuesto;
                            asTotalImpuestos[6] = "0.00";
                            arraylTotalImpuestos.Add(asTotalImpuestos);
                            gBD.totalImpuestos(arraylTotalImpuestos);
                            offline.totalImpuestos(arraylTotalImpuestos);
                            gXml.totalImpuestos(arraylTotalImpuestos);
                            mensajeBitacora += " Se proceso Totales";
                        }


                        importeAPagar = (double.Parse(valida_texto_a_numero(importeTotal), CultureInfo.InvariantCulture) + double.Parse(valida_texto_a_numero(propina), CultureInfo.InvariantCulture)).ToString("F2", CultureInfo.InvariantCulture);
                        gBD.cantidades(valida_texto_a_numero(subtotal12), valida_texto_a_numero(subtotal0), valida_texto_a_numero(subtotalNoSujeto), valida_texto_a_numero(totalSinImpuestos), valida_texto_a_numero(totalDescuento), valida_texto_a_numero(ICE), valida_texto_a_numero(IVA12), valida_texto_a_numero(importeTotal), valida_texto_a_numero(propina), valida_texto_a_numero(importeAPagar));
                        offline.cantidades(valida_texto_a_numero(subtotal12), valida_texto_a_numero(subtotal0), valida_texto_a_numero(subtotalNoSujeto), valida_texto_a_numero(totalSinImpuestos), valida_texto_a_numero(totalDescuento), valida_texto_a_numero(ICE), valida_texto_a_numero(IVA12), valida_texto_a_numero(importeTotal), valida_texto_a_numero(propina), valida_texto_a_numero(importeAPagar));
                        gXml.cantidades(valida_texto_a_numero(subtotal12), valida_texto_a_numero(subtotal0), valida_texto_a_numero(subtotalNoSujeto), valida_texto_a_numero(totalSinImpuestos), valida_texto_a_numero(totalDescuento), valida_texto_a_numero(ICE), valida_texto_a_numero(IVA12), valida_texto_a_numero(importeTotal), valida_texto_a_numero(propina), valida_texto_a_numero(importeAPagar));
                        mensajeBitacora += " Se proceso Cantidades";
                    }


                    //DE
                    if (!banErrorArchivo)
                    {
                        banderaArchivo = 8;

                        XmlElement detalles = (XmlElement)xmlbase.GetElementsByTagName("detalles")[0];

                        if (detalles != null)
                        {
                            lee_Detalles(detalles);
                        }

                    }
                }
                #endregion

                #region otros rubros a terceros
                XmlElement otrosRubrosTerceros = (XmlElement)xmlbase.GetElementsByTagName("otrosRubrosTerceros")[0];

                if (otrosRubrosTerceros != null)
                {
                    codDocVersion = "012";
                    foreach (XmlElement p_root in otrosRubrosTerceros)
                    {
                        this.asRubros = new string[3];
                        this.asRubros[0] = this.lee_nodo_xml(p_root, "concepto");
                        this.asRubros[1] = this.lee_nodo_xml(p_root, "total");

                        this.arraylRubros.Add(this.asRubros);
                        this.gBD.DetalleRubros(this.arraylRubros);
                        this.offline.DetalleRubros(this.arraylRubros);
                        this.gXml.DetalleRubros(this.arraylRubros);
                    }
                }

                #endregion
                //GUIA DE REMISION
                //lg1.guardar_Log("ingresando  a procesarArchivoXML paso 9 ");
                #region "GUIA DE REMISION"
                if (codDoc.Equals("06"))
                {
                    //////T y TI
                    if (banderaArchivo == 2 && !banErrorArchivo)
                    {
                        banderaArchivo = 7;

                        fechaEmision = fechaIniTransporte;

                        XmlElement destinatarios = (XmlElement)xmlbase.GetElementsByTagName("destinatarios")[0];

                        foreach (XmlElement dest in destinatarios)
                        {
                            asDestinatarios = new String[12];
                            identificacionDestinatario = lee_nodo_xml(dest, "identificacionDestinatario");
                            razonSocialDestinatario = lee_nodo_xml(dest, "razonSocialDestinatario");
                            dirDestinatario = lee_nodo_xml(dest, "dirDestinatario"); destinatarioLF = dirDestinatario; //LF para reglas de correo
                            motivoTraslado = lee_nodo_xml(dest, "motivoTraslado");
                            docAduaneroUnico = lee_nodo_xml(dest, "docAduaneroUnico");
                            codEstabDestino = lee_nodo_xml(dest, "codEstabDestino");
                            ruta = lee_nodo_xml(dest, "ruta");
                            codDocSustento = obtener_codigo(lee_nodo_xml(dest, "codDocSustento"));
                            numDocSustento = lee_nodo_xml(dest, "numDocSustento");
                            numAutDocSustento = lee_nodo_xml(dest, "numAutDocSustento");
                            fechaEmisionDocSustento = lee_nodo_xml(dest, "fechaEmisionDocSustento");
                            //if (!fechaEmisionDocSustento.Equals("") && fechaEmisionDocSustento != null) fechaEmisionDocSustento = Convert.ToDateTime(fechaEmisionDocSustento).ToString("dd/MM/yyyy");
                            idDestinatario++;
                            asDestinatarios[0] = identificacionDestinatario; asDestinatarios[1] = razonSocialDestinatario; asDestinatarios[2] = dirDestinatario;
                            asDestinatarios[3] = motivoTraslado; asDestinatarios[4] = docAduaneroUnico; asDestinatarios[5] = codEstabDestino;
                            asDestinatarios[6] = ruta; asDestinatarios[7] = codDocSustento; asDestinatarios[8] = numDocSustento;
                            asDestinatarios[9] = numAutDocSustento; asDestinatarios[10] = fechaEmisionDocSustento; asDestinatarios[11] = idDestinatario.ToString().Trim();
                            //asDestinatarios[12] = "";
                            arraylDestinatarios.Add(asDestinatarios);

                            XmlElement detalles = (XmlElement)dest.GetElementsByTagName("detalles")[0];

                            if (detalles != null)
                            {
                                lee_Detalles(detalles);
                            }


                        }

                        razonSocialComprador = razonSocialDestinatario;

                        gBD.cantidades("0", "0", "0", "0", "0", "0", "0", "0", "0", "0");
                        offline.cantidades("0", "0", "0", "0", "0", "0", "0", "0", "0", "0");
                        gXml.cantidades("0", "0", "0", "0", "0", "0", "0", "0", "0", "0");

                        gBD.guarda_destinatario_receptor(identificacionDestinatario, razonSocialDestinatario);
                        offline.guarda_destinatario_receptor(identificacionDestinatario, razonSocialDestinatario);
                        gBD.Destinatarios(arraylDestinatarios);
                        offline.Destinatarios(arraylDestinatarios);
                        gXml.Destinatarios(arraylDestinatarios);

                    }
                }
                #endregion

                //INFORMACION ADICIONAL
                #region "INFORMACION ADICIONAL"

                XmlElement infoAdicional = (XmlElement)xmlbase.GetElementsByTagName("infoAdicional")[0];

                if (infoAdicional != null)
                {
                    banderaArchivo = 11;
                    foreach (XmlElement ca in infoAdicional)
                    {
                        asInfoAdicionales = new String[2];
                        infoAdicionalNombre = ca.Attributes["nombre"].Value;
                        infoAdicionalValor = ca.InnerText;// lee_nodo_xml(ca, "campoAdicional");
                        if (!string.IsNullOrEmpty(infoAdicionalNombre) && !string.IsNullOrEmpty(infoAdicionalValor))
                        {
                            if (infoAdicionalNombre.Equals("E-MAIL")) emails = infoAdicionalValor;
                            if (infoAdicionalNombre.Equals("Direccion")) domicilio = infoAdicionalValor;
                            if (infoAdicionalNombre.Equals("Sellos")) proforma = infoAdicionalValor;
                            if (infoAdicionalNombre.Equals("destinatario")) destinatarioLF = infoAdicionalValor;
                            asInfoAdicionales[0] = infoAdicionalNombre; asInfoAdicionales[1] = infoAdicionalValor;
                            arrayInfoAdicionales.Add(asInfoAdicionales);
                        }
                    }
                    gBD.informacionAdicional(arrayInfoAdicionales);
                    offline.informacionAdicional(arrayInfoAdicionales);
                    gXml.informacionAdicional(arrayInfoAdicionales);
                }

                DB.Conectar();
                DB.CrearComando("SELECT top 1 emailsRegla FROM EmailsReglas  with(nolock) WHERE SUBSTRING(nombreRegla,1,6)=SUBSTRING(@rfcrec,1,6) AND estadoRegla=1 and eliminado=0");
                DB.AsignarParametroCadena("@rfcrec", destinatarioLF);
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    if (DR3.Read())
                    {
                        emails = emails.Trim(',') + "," + DR3[0].ToString().Trim(',') + "";
                    }
                }

                DB.Desconectar();


                gBD.infromacionAdicionalCima(termino, proforma, "", domicilio, "", emails, emails);
                offline.infromacionAdicionalCima(termino, proforma, "", domicilio, "", emails, emails);
                #endregion

                if (!banErrorArchivo)
                {
                    if (banderaArchivo > 2)
                    {
                        //guiaRemision = estab + "-" + ptoEmi + "-" + secuencial;
                        if (tipoEmision.Equals("2"))
                        {
                            claveAcceso = generarClaveAcceso();
                            //codigoContingenciaTemp = obtenerClaveContingencia(ambiente);
                            //claveAcceso = generarClaveAccesoContingencia(codigoContingenciaTemp);
                        }
                        else
                        {
                            claveAcceso = generarClaveAcceso();
                        }
                        if (String.IsNullOrEmpty(codigoControl))
                            mensajeBitacora += " Se va a convertir fecha: " + fechaEmision + " a " + DateTime.Today.ToString("yyyyMMddHHmm");
                        codigoControl = ruc + codDoc + estab + ptoEmi + secuencial + Convert.ToDateTime(fechaEmision).ToString("yyyyMMddHHmm");

                        banErrorArchivo = false;
                    }
                    else
                    {
                        banErrorArchivo = true;
                    }
                }


            }
            catch (Exception arex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(arex.Message);
                banErrorArchivo = true;
                msj = "No se pudo procesar el archivo. " + msjLectura(banderaArchivo);
                msjT = arex.Message;
                log.mensajesLog("ES003", msj + "; " + msjT, msjT, "", codigoControl, linea);
                //ActualizaTablaLogWebService("E", p_idLog);
                //msjT = "";
            }
            finally
            {
                //sr.Close();
            }
            //lg1.guardar_Log("ingresando  a procesarArchivoXML paso 10 " + codDoc);
            if (!banErrorArchivo)
            {
                switch (codDoc)
                {
                    case "01":
                    case "03":
                    case "04":
                    case "06":
                    

                        if (codDocVersion.Equals("012"))
                        {
                            version = "2.1.0";
                        }
                        else
                        {
                            version = "1.1.0";
                        }

                        break;
                    case "07" :

                        version = version;

                        break;

                    default:
                        version = "1.0.0";
                        break;


                }
                //lg1.guardar_Log("ingresando  a procesarArchivoXML paso 11 ");
                gBD.xmlComprobante(version, "comprobante", "");
                
                offline.xmlComprobante(version, "comprobante", "");
                gXml.xmlComprobante(version, "comprobante");
                gBD.otrosCampos(claveAcceso, secuencial, guiaRemision, codigoControl);
                offline.otrosCampos(claveAcceso, secuencial, guiaRemision, codigoControl, p_idLog, esNDFinLF);
                gXml.otrosCampos(claveAcceso, secuencial, guiaRemision);

                log.mensajesLog("US001", "", "", "", "p_proceso -->" + p_proceso, "clase de error Invoice.cs");

                if (p_proceso.Equals("ON"))
                {
                    procesar();
                }
                else if (p_proceso.Equals("OFF"))
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc = generarXML();
                    offline.procesarOff(xDoc);
                }
            }
            else
            {
                banErrorArchivo = false;
                log.mensajesLog("ES003", "El archivo tiene error o no está activada para facturación electrónica. " + msj + "; " + msjT, msjT, "", codigoControl, "BanderaError:" + banErrorArchivo.ToString() + " Bandera:" + banderaArchivo.ToString() + " Bitácora: " + mensajeBitacora);
                //ActualizaTablaLogWebService("E", p_idLog);
            }
            if (esArchivo)
                copiarArc(RutaTXT, RutaBCK, nombreArchivo, "");
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

        private void lee_Detalles(XmlElement detalles)
        {
            foreach (XmlElement ti in detalles)
            {
                asDetalles = new String[11];
                if (codDoc.Equals("04") || codDoc.Equals("06") || codDoc.Equals("05"))
                {
                    codigoPrincipal = lee_nodo_xml(ti, "codigoInterno");
                    codigoAuxiliar = lee_nodo_xml(ti, "codigoAdicional");
                }

                else
                {
                    codigoPrincipal = lee_nodo_xml(ti, "codigoPrincipal");
                    codigoAuxiliar = lee_nodo_xml(ti, "codigoAuxiliar");
                }

                //codigoAuxiliar = lee_nodo_xml(ti, "codigoAuxiliar");
                descripcion = lee_nodo_xml(ti, "descripcion");
                cantidad = lee_nodo_xml(ti, "cantidad");
                precioUnitario = lee_nodo_xml(ti, "precioUnitario");
                descuento = valida_texto_a_numero(lee_nodo_xml(ti, "descuento"));
                precioTotalSinImpuesto = lee_nodo_xml(ti, "precioTotalSinImpuesto");

                idDetallesTemp++;

                asDetalles[0] = codigoPrincipal; asDetalles[1] = codigoAuxiliar; asDetalles[2] = descripcion;
                asDetalles[3] = cantidad; asDetalles[4] = precioUnitario; asDetalles[5] = valida_texto_a_numero(descuento);
                asDetalles[6] = valida_texto_a_numero(precioTotalSinImpuesto); asDetalles[7] = idDetallesTemp.ToString().Trim();
                asDetalles[8] = idDestinatario.ToString().Trim();
                arraylDetalles.Add(asDetalles);

                codigoTemp = codigoPrincipal;
                gBD.detalles(arraylDetalles);
                offline.detalles(arraylDetalles);
                gXml.detalles(arraylDetalles);
                mensajeBitacora += " Se proceso Detalles";

                //DA
                XmlElement detallesAdicionales2 = (XmlElement)ti.GetElementsByTagName("detallesAdicionales")[0];
                if (detallesAdicionales2 != null)
                {
                    foreach (XmlElement da in detallesAdicionales2)
                    {
                        //banderaArchivo = 10;
                        asDetallesAdicionales = new String[4];
                        detAdicionalNombre = da.Attributes["nombre"].Value;
                        detAdicionalValor = da.Attributes["valor"].Value;
                        codigoTemp = codigoPrincipal;
                        //idDetallesTemp = idDetallesTemp;
                        asDetallesAdicionales[0] = detAdicionalNombre;
                        asDetallesAdicionales[1] = detAdicionalValor;
                        asDetallesAdicionales[2] = codigoTemp;
                        asDetallesAdicionales[3] = idDetallesTemp.ToString().Trim();
                        arraylDetallesAdicionales.Add(asDetallesAdicionales);
                    }

                    gBD.detallesAdicionales(arraylDetallesAdicionales);
                    offline.detallesAdicionales(arraylDetallesAdicionales);
                    gXml.detallesAdicionales(arraylDetallesAdicionales);
                    mensajeBitacora += " Se procesó detallesAdicionales";
                }
                //IM
                XmlElement impuestos = (XmlElement)ti.GetElementsByTagName("impuestos")[0];
                if (impuestos != null)
                {
                    lee_Impuestos(impuestos);
                }
            }
        }

        private void lee_Impuestos(XmlElement impuestos)
        {
            foreach (XmlElement imp in impuestos)
            {
                if (codDoc.Equals("01") ||codDoc.Equals("03") || codDoc.Equals("04"))
                {
                    //banderaArchivo = 9;
                    asImpuestosDetalles = new String[8];
                    impuestoCodigo = obtener_codigo(lee_nodo_xml(imp, "codigo"));
                    String Cod = obtener_codigo(lee_nodo_xml(imp, "codigo"));
                    impuestoCodigo = Cod == null ? lee_nodo_xml(imp, "codigo") : Cod;
                    impuestoCodigoPorcentaje = obtener_codigo(lee_nodo_xml(imp, "codigoPorcentaje"));
                    String Por = obtener_codigo(lee_nodo_xml(imp, "codigoPorcentaje"));
                    impuestoCodigoPorcentaje = Por == null ? lee_nodo_xml(imp, "codigoPorcentaje") : Por;
                    if (impuestoCodigo == "04")
                    {
                        impuestoCodigo = "2";
                        impuestoCodigoPorcentaje = lee_nodo_xml(imp, "codigoPorcentaje");
                    }
                    
                    impuestoBaseImponible = lee_nodo_xml(imp, "baseImponible");
                    impuestoTarifa = lee_nodo_xml(imp, "tarifa");
                    impuestoValor = lee_nodo_xml(imp, "valor");
                    codigoTemp = codigoPrincipal;
                    switch (impuestoCodigo) { case "2": impuestotipoImpuesto = "IVA"; break; case "3": impuestotipoImpuesto = "ICE"; break; }
                    asImpuestosDetalles[0] = impuestoCodigo; asImpuestosDetalles[1] = impuestoCodigoPorcentaje; asImpuestosDetalles[2] = valida_texto_a_numero(impuestoBaseImponible);
                    asImpuestosDetalles[3] = impuestoTarifa; asImpuestosDetalles[4] = valida_texto_a_numero(impuestoValor); asImpuestosDetalles[5] = codigoTemp;
                    asImpuestosDetalles[6] = impuestotipoImpuesto;
                    asImpuestosDetalles[7] = idDetallesTemp.ToString();
                    arraylImpuestosDetalles.Add(asImpuestosDetalles);
                    gBD.impuestos(arraylImpuestosDetalles);
                    offline.impuestos(arraylImpuestosDetalles);
                    gXml.impuestos(arraylImpuestosDetalles);
                }
                else
                {
                    if (codDoc.Equals("07"))
                    {
                        //TIR
                        //banderaArchivo = 5;
                        asTotalImpuestosRetenciones = new String[8];
                        codigo = lee_nodo_xml(imp, "codigo"); // asLinea[1].ToString().Trim();
                        if (codigo.Equals("1")) codigoRetencion = obtener_codigo_renta(lee_nodo_xml(imp, "codigoRetencion"));
                        else codigoRetencion = lee_nodo_xml(imp, "codigoRetencion"); //asLinea[2].ToString().Trim();
                        baseImponible = lee_nodo_xml(imp, "baseImponible"); //asLinea[3].ToString().Trim();
                        porcentajeRetener = lee_nodo_xml(imp, "porcentajeRetener"); //asLinea[4].ToString().Trim();
                        valorRetenido = lee_nodo_xml(imp, "valorRetenido");  //asLinea[5].ToString().Trim();
                        codDocSustento = obtener_codigo(lee_nodo_xml(imp, "codDocSustento")); //asLinea[6].ToString().Trim();
                        numDocSustento = lee_nodo_xml(imp, "numDocSustento");  //asLinea[7].ToString().Trim();
                        fechaEmisionDocSustento = lee_nodo_xml(imp, "fechaEmisionDocSustento");  //Convert.ToDateTime(asLinea[8].ToString().Trim()).ToString("dd/MM/yyyy");
                        asTotalImpuestosRetenciones[0] = codigo; asTotalImpuestosRetenciones[1] = codigoRetencion; asTotalImpuestosRetenciones[2] = baseImponible;
                        asTotalImpuestosRetenciones[3] = porcentajeRetener; asTotalImpuestosRetenciones[4] = valorRetenido; asTotalImpuestosRetenciones[5] = codDocSustento;
                        asTotalImpuestosRetenciones[6] = numDocSustento; asTotalImpuestosRetenciones[7] = fechaEmisionDocSustento;
                        arraylTotalImpuestosRetenciones.Add(asTotalImpuestosRetenciones);
                        gBD.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);
                        offline.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);
                        gXml.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);

                    }
                    else
                    {
                        if (codDoc.Equals("05"))
                        {
                            asTotalImpuestos = new String[8];

                            codigo = obtener_codigo(lee_nodo_xml(imp, "codigo"));
                            switch (codigo)
                            {
                                case "2": // IVA
                                    codigoPorcentaje = obtener_codigo(lee_nodo_xml(imp, "codigoPorcentaje"));
                                    switch (codigoPorcentaje)
                                    {
                                        case "0": // 0%
                                            subtotal0 = lee_nodo_xml(imp, "baseImponible");
                                            baseImponible = subtotal0;
                                            tarifa = lee_nodo_xml(imp, "tarifa");
                                            valor = lee_nodo_xml(imp, "valor");
                                            tipoImpuesto = "IVA";
                                            break;
                                        case "2":
                                        case "3"://12% - 14%
                                            subtotal12 = lee_nodo_xml(imp, "baseImponible");
                                            baseImponible = subtotal12;
                                            tarifa = lee_nodo_xml(imp, "tarifa");
                                            valor = lee_nodo_xml(imp, "valor"); if (valor.Equals("0") || valor.Equals("0.0") || valor.Equals("0.00")) codigoPorcentaje = "0";
                                            tipoImpuesto = "IVA";
                                            IVA12 = valor;
                                            break;
                                        case "6": //No Objeto
                                            subtotalNoSujeto = lee_nodo_xml(imp, "baseImponible");
                                            baseImponible = subtotalNoSujeto;
                                            tarifa = lee_nodo_xml(imp, "tarifa");
                                            valor = lee_nodo_xml(imp, "valor");
                                            tipoImpuesto = "IVA";
                                            break;
                                        case "7": // 0%
                                            subtotal0 = lee_nodo_xml(imp, "baseImponible");
                                            baseImponible = subtotal0;
                                            tarifa = lee_nodo_xml(imp, "tarifa");
                                            valor = lee_nodo_xml(imp, "valor");
                                            tipoImpuesto = "IVA";
                                            break;

                                    }

                                    break;
                                case "3": //ICE
                                    codigoPorcentaje = obtener_codigo(lee_nodo_xml(imp, "codigoPorcentaje"));
                                    baseImponible = lee_nodo_xml(imp, "baseImponible");
                                    tarifa = lee_nodo_xml(imp, "tarifa");
                                    valor = lee_nodo_xml(imp, "valor");
                                    ICE = valor;
                                    tipoImpuesto = "ICE";
                                    break;



                            }
                            if (codDoc.Equals("04"))
                            {
                                if (codigoPorcentaje.Equals("3"))
                                    tarifa = "14";
                                else if (codigoPorcentaje.Equals("2"))
                                    tarifa = "12";
                                else
                                    tarifa = "0";
                            }

                            asTotalImpuestos[0] = codigo; asTotalImpuestos[1] = codigoPorcentaje; asTotalImpuestos[2] = baseImponible;
                            asTotalImpuestos[3] = tarifa; asTotalImpuestos[4] = valor; asTotalImpuestos[5] = tipoImpuesto;
                            asTotalImpuestos[6] = "0.00";
                            arraylTotalImpuestos.Add(asTotalImpuestos);
                            gBD.totalImpuestos(arraylTotalImpuestos);
                            offline.totalImpuestos(arraylTotalImpuestos);
                            gXml.totalImpuestos(arraylTotalImpuestos);
                            //mensajeBitacora += " Se proceso Totales";
                        }

                    }

                }
                mensajeBitacora += " Se proceso Impuestos y Totales";
            }


        }

        private void lee_ImpuestosRet2(XmlElement impuestos, bool retencion, string idCodSustento)
        {
            foreach (XmlElement imp in impuestos)
            {
                if (!retencion)
                {
                    asImpuestosDetalles = new String[7];
                    impuestoCodigo = lee_nodo_xml(imp, "codImpuestoDocSustento");
                    impuestoCodigoPorcentaje = obtener_codigo(lee_nodo_xml(imp, "codigoPorcentaje"));
                    impuestoBaseImponible = lee_nodo_xml(imp, "baseImponible");
                    impuestoTarifa = lee_nodo_xml(imp, "tarifa");
                    impuestoValor = lee_nodo_xml(imp, "valorImpuesto");
                    switch (impuestoCodigo) 
                    { 
                        case "2": 
                            impuestotipoImpuesto = "IVA"; 
                            break; 
                        case "3": 
                            impuestotipoImpuesto = "ICE"; 
                            break; 
                    }

                    asImpuestosDetalles[0] = idCodSustento;
                    asImpuestosDetalles[1] = impuestoCodigo;
                    asImpuestosDetalles[2] = impuestoCodigoPorcentaje;
                    asImpuestosDetalles[3] = valida_texto_a_numero(impuestoBaseImponible);
                    asImpuestosDetalles[4] = impuestoTarifa;
                    asImpuestosDetalles[5] = valida_texto_a_numero(impuestoValor);
                    asImpuestosDetalles[6] = impuestotipoImpuesto;
                    arraylImpuestosDetalles.Add(asImpuestosDetalles);
                    gBD.impuestos(arraylImpuestosDetalles);
                    gXml.impuestos(arraylImpuestosDetalles);
                    offline.impuestos(arraylImpuestosDetalles);
                }
                else
                {
                    asTotalImpuestosRetenciones = new String[14];
                    codigo = lee_nodo_xml(imp, "codigo");
                    codigoRetencion = lee_nodo_xml(imp, "codigoRetencion");
                    baseImponible = lee_nodo_xml(imp, "baseImponible");
                    porcentajeRetener = lee_nodo_xml(imp, "porcentajeRetener");
                    valorRetenido = lee_nodo_xml(imp, "valorRetenido");
                    XmlElement dividendos = (XmlElement)imp.GetElementsByTagName("dividendos")[0];
                    if (dividendos != null)
                    {
                        fechaPagoDiv = lee_nodo_xml(dividendos, "fechaPagoDiv");
                        imRentaSoc = lee_nodo_xml(dividendos, "imRentaSoc");
                        ejerFisUtDiv = lee_nodo_xml(dividendos, "ejerFisUtDiv");
                    }
                    XmlElement compraCajBanano = (XmlElement)imp.GetElementsByTagName("compraCajBanano")[0];
                    if (compraCajBanano != null)
                    {
                        NumCajBan = lee_nodo_xml(compraCajBanano, "numCajBan");
                        PrecCajBan = lee_nodo_xml(compraCajBanano, "precCajBan");
                    }

                    asTotalImpuestosRetenciones[13] = idCodSustento;
                    asTotalImpuestosRetenciones[0] = codigo;
                    asTotalImpuestosRetenciones[1] = codigoRetencion;
                    asTotalImpuestosRetenciones[2] = valida_texto_a_numero(baseImponible);
                    asTotalImpuestosRetenciones[3] = porcentajeRetener;
                    asTotalImpuestosRetenciones[4] = valida_texto_a_numero(valorRetenido);
                    asTotalImpuestosRetenciones[5] = codDocSustento;
                    asTotalImpuestosRetenciones[6] = numDocSustento;
                    asTotalImpuestosRetenciones[7] = fechaEmisionDocSustento;
                    asTotalImpuestosRetenciones[8] = fechaPagoDiv;
                    asTotalImpuestosRetenciones[9] = imRentaSoc;
                    asTotalImpuestosRetenciones[10] = ejerFisUtDiv;
                    asTotalImpuestosRetenciones[11] = NumCajBan;
                    asTotalImpuestosRetenciones[12] = PrecCajBan;
                    arraylTotalImpuestosRetenciones.Add(asTotalImpuestosRetenciones);
                    gBD.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);
                    gXml.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);
                    offline.totalImpuestosRetenciones(arraylTotalImpuestosRetenciones);


                }
            }
        }

        private void lee_ReembolsosSustentos(XmlElement reembolsos, string idCodSustento)
        {
            int countReembolsos = 0;
            foreach (XmlElement reembolso in reembolsos)
            {
                countReembolsos++;
                asReembolsosRetenciones = new String[12];

                tipoIdentificacionProveedorReembolsoRet = lee_nodo_xml(reembolso, "tipoIdentificacionProveedorReembolso");
                identificacionProveedorReembolsoRet = lee_nodo_xml(reembolso, "identificacionProveedorReembolso");
                codPaisPagoProveedorReembolsoRet = lee_nodo_xml(reembolso, "codPaisPagoProveedorReembolso");
                tipoProveedorReembolsoRet = lee_nodo_xml(reembolso, "tipoProveedorReembolso");
                codDocReembolsoRet = lee_nodo_xml(reembolso, "codDocReembolso");
                estabDocReembolsoRet = lee_nodo_xml(reembolso, "estabDocReembolso");
                ptoEmiDocReembolsoRet = lee_nodo_xml(reembolso, "ptoEmiDocReembolso");
                secuencialDocReembolsoRet = lee_nodo_xml(reembolso, "secuencialDocReembolso");
                fechaEmisionDocReembolsoRet = lee_nodo_xml(reembolso, "fechaEmisionDocReembolso");
                numeroAutorizacionDocReembRet = lee_nodo_xml(reembolso, "numeroAutorizacionDocReemb");

                asReembolsosRetenciones[0] = idCodSustento;
                asReembolsosRetenciones[1] = countReembolsos.ToString();
                asReembolsosRetenciones[2] = tipoIdentificacionProveedorReembolsoRet;
                asReembolsosRetenciones[3] = identificacionProveedorReembolsoRet;
                asReembolsosRetenciones[4] = codPaisPagoProveedorReembolsoRet;
                asReembolsosRetenciones[5] = tipoProveedorReembolsoRet;
                asReembolsosRetenciones[6] = codDocReembolsoRet;
                asReembolsosRetenciones[7] = estabDocReembolsoRet;
                asReembolsosRetenciones[8] = ptoEmiDocReembolsoRet;
                asReembolsosRetenciones[9] = secuencialDocReembolsoRet;
                asReembolsosRetenciones[10] = fechaEmisionDocReembolsoRet;
                asReembolsosRetenciones[11] = numeroAutorizacionDocReembRet;
                arraylReembolsosRetenciones.Add(asReembolsosRetenciones);
                gBD.reembolsosSustentos(arraylReembolsosRetenciones);
                gXml.reembolsosSustentos(arraylReembolsosRetenciones);
                offline.ReembolsosSustentos(arraylReembolsosRetenciones);

                XmlElement detalleImpuestos = (XmlElement)reembolso.GetElementsByTagName("detalleImpuestos")[0];
                if (detalleImpuestos != null)
                    lee_ImpuestosReembRet(detalleImpuestos, countReembolsos.ToString(), idCodSustento);
            }
        }

        private void lee_ImpuestosReembRet(XmlElement impuestos, string idReemb, string idCodSustento)
        {
            foreach (XmlElement imp in impuestos)
            {
                asImpuestosReembolsosRet = new String[8];
                impuestoCodigo = lee_nodo_xml(imp, "codigo");
                impuestoCodigoPorcentaje = lee_nodo_xml(imp, "codigoPorcentaje");
                impuestoTarifa = lee_nodo_xml(imp, "tarifa");
                impuestoBaseImponible = lee_nodo_xml(imp, "baseImponibleReembolso");
                impuestoValor = lee_nodo_xml(imp, "impuestoReembolso");
                switch (impuestoCodigo) { case "2": impuestotipoImpuesto = "IVA"; break; case "3": impuestotipoImpuesto = "ICE"; break; }

                asImpuestosReembolsosRet[0] = idReemb;
                asImpuestosReembolsosRet[1] = impuestoCodigo;
                asImpuestosReembolsosRet[2] = impuestoCodigoPorcentaje;
                asImpuestosReembolsosRet[3] = impuestoTarifa;
                asImpuestosReembolsosRet[4] = valida_texto_a_numero(impuestoBaseImponible);
                asImpuestosReembolsosRet[5] = valida_texto_a_numero(impuestoValor);
                asImpuestosReembolsosRet[6] = impuestotipoImpuesto;
                asImpuestosReembolsosRet[7] = idCodSustento;
                arraylImpuestosReembolsosRet.Add(asImpuestosReembolsosRet);

                gBD.impuestosReembolsosSustentos(arraylImpuestosReembolsosRet);
                gXml.impuestosReembolsosSustentos(arraylImpuestosReembolsosRet);
                offline.ImpuestosReembolsosSustentos(arraylImpuestosReembolsosRet);
            }
        }


        public void procesar()
        {
            BasesDatos DB = new BasesDatos();

            string temp = "";
            mensaje = "";
            asunto = "";
            msjSRI = "";
            msjT = "";

            try
            {
                XmlDocument xDoc = new XmlDocument();

                xDoc = generarXML();
                if (xDoc != null)
                {
                    temp = xDoc.OuterXml;
                    byte[] bytes = Encoding.Default.GetBytes(temp);
                    temp = Encoding.UTF8.GetString(bytes);

                    xDoc.LoadXml(temp);
                    xDoc.InnerXml = temp;
                    if (xDoc != null)
                    {
                        procesa_archivo_xml(xDoc, codigoControl, "0", 1);

                        xtrReader = new XmlTextReader(new StringReader(xDoc.OuterXml));

                        if (estructura(xtrReader, codDoc, xsd))
                        {
                            XmlDocument xDocF2 = new XmlDocument();
                            string xDocF = "";

                            if (firmaXADES.Firmar(RutaP12, PassP12, xDoc, out xDocF))
                            {
                                byte[] bytesXML = Encoding.Default.GetBytes(xDocF);
                                msjSRI = "";
                                xDocF2.LoadXml(xDocF);
                                procesa_archivo_xml(xDocF2, codigoControl, "0", 2);

                                if (enviarComprobante(bytesXML))
                                {

                                    if (gBD.guardarBD())
                                    {
                                        System.Threading.Thread.Sleep(3000); //3 segundos

                                        if (validarAutorizacion(claveAcceso))
                                        {
                                            try
                                            {
                                                log.mensajesLog("EM010", msjSRI, msjT, "", codigoControl, "Final Autorizado 1");
                                            }
                                            catch (Exception e)
                                            {
                                                msjT = e.Message;
                                                log.mensajesLog("EM011", "Excepcion al agregar nodo autorización", msjT, "", codigoControl, "");
                                                msjT = "";

                                            }
                                        }
                                        else
                                        {
                                            //log.mensajesLog("EM017", msjSRI, msjT, "", codigoControl, "");
                                        }
                                    }
                                }
                                else
                                {
                                    if (edo != "DEVUELTA")
                                    {
                                        gBD.guardarBD();

                                        if (!msjT.Equals("The operation has timed out"))
                                        {
                                            log.mensajesLog("EM016", "Se usará una clave de contingencia en lugar de: " + claveAcceso + "; Mensaje: " + msjT, msjT, "", codigoControl, " Recepción de Comprobantes, WebService envio.");

                                            codigoContingenciaTemp = obtenerClaveContingencia(ambiente);
                                            if (!String.IsNullOrEmpty(codigoContingenciaTemp))
                                            {
                                                claveAcceso = generarClaveAccesoContingencia(codigoContingenciaTemp);
                                                gBD.otrosCampos(claveAcceso, secuencial, guiaRemision, codigoControl);
                                                gXml.otrosCamposConti(claveAcceso, secuencial, guiaRemision, "2");
                                                xDoc = null;
                                                xDoc = generarXML();

                                                log.mensajesLog("EM013", claveAcceso, msjT, "", codigoControl, "");

                                                if (xDoc != null)
                                                {
                                                    temp = xDoc.OuterXml;

                                                    procesa_archivo_xml(xDoc, codigoControl, idComprobante, 3);
                                                    procesa_archivo_xml(xDoc, codigoControl, idComprobante, 4);

                                                    if (firmaXADES.Firmar(RutaP12, PassP12, xDoc, out xDocF))
                                                    {
                                                        DB.Conectar();
                                                        DB.CrearComando(@"UPDATE GENERAL SET claveAcceso = @claveAcceso,tipoEmision=@tipoEmision WHERE codigoControl = @codigoControl");
                                                        DB.AsignarParametroCadena("@claveAcceso", claveAcceso);
                                                        DB.AsignarParametroCadena("@codigoControl", codigoControl);
                                                        DB.AsignarParametroCadena("@tipoEmision", "2");
                                                        DB.EjecutarConsulta1();
                                                        DB.Desconectar();
                                                    }
                                                    //log.mensajesLog("EM017", "Aun no se manda a autorizar.", msjT, "", codigoControl, "");
                                                }

                                                //Actualizar Contingencias
                                                DB.Conectar();
                                                DB.CrearComando(@"UPDATE ClavesContignencia SET estado= @estado,uso=@uso WHERE clave = @codigoContingenciaTemp");
                                                DB.AsignarParametroCadena("@estado", "1");
                                                DB.AsignarParametroCadena("@uso", System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                                                DB.AsignarParametroCadena("@codigoContingenciaTemp", codigoContingenciaTemp);
                                                DB.EjecutarConsulta1();
                                                DB.Desconectar();

                                                //Actualizar facturas a estado de contingencia
                                                DB.Conectar();
                                                DB.CrearComando(@"UPDATE GENERAL SET creado= @creado, estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                                                DB.AsignarParametroCadena("@creado", "1");
                                                DB.AsignarParametroCadena("@estado", "2");
                                                DB.AsignarParametroCadena("@tipo", "E");
                                                DB.AsignarParametroCadena("@codigoControl", codigoControl);
                                                DB.EjecutarConsulta1();
                                                DB.Desconectar();

                                                //enviar_correo();
                                                try
                                                {
                                                    idcomprobante2 = obtenerid_comprobante(codigoControl);
                                                    DataTable tb_infoA = obtener_infoAdicional(idcomprobante2);
                                                    if (tb_infoA.Rows.Count > 0)
                                                    {
                                                        RespuestaWebUNASAP(codDoc, estab + "-" + ptoEmi + "-" + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, claveAcceso, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), "CN", msjT, tb_infoA.Rows[0]["sociedad"].ToString(), tb_infoA.Rows[0]["numeroAsientoContable"].ToString(), tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    DB.Desconectar();
                                                    clsLogger.Graba_Log_Error(ex.Message);
                                                    log.mensajesLog("EM016", claveAcceso, ex.Message, "", codigoControl, " Validación de Comprobantes, WebService validación1: No se encontro informacion adicional");
                                                }

                                            }
                                            else
                                            {
                                                msjT = "No se dispone de clave de contingencia";
                                                log.mensajesLog("US001", msjT, msjT, "", codigoControl, " Error en generar factura de contingencia. ");

                                            }

                                            string conti = obtener_codigo("LIMITE_CONTINGENCIA");
                                            if (string.IsNullOrEmpty(conti)) conti = "10000";
                                            enviar_notificacion_contingencia(ruc, ambiente, conti);
                                        }
                                        else
                                        {
                                            log.mensajesLog("EM016", "Se creará un documento provisional con clave acceso: " + claveAcceso + "; Mensaje: " + msjT, msjT, "", codigoControl, " Recepción de Comprobantes, WebService envio.");

                                            //Actualizar documentos a estado pendiente
                                            DB.Conectar();
                                            DB.CrearComando(@"UPDATE GENERAL SET creado= @creado, estado=@estado,tipo=@tipo WHERE codigoControl = @codigoControl");
                                            DB.AsignarParametroCadena("@creado", "1");
                                            DB.AsignarParametroCadena("@estado", "1");
                                            DB.AsignarParametroCadena("@tipo", "E");
                                            DB.AsignarParametroCadena("@codigoControl", codigoControl);
                                            DB.EjecutarConsulta1();
                                            DB.Desconectar();

                                            //enviar_correo();

                                            try
                                            {
                                                idcomprobante2 = obtenerid_comprobante(codigoControl);
                                                DataTable tb_infoA = obtener_infoAdicional(idcomprobante2);
                                                if (tb_infoA.Rows.Count > 0)
                                                {

                                                    RespuestaWebUNASAP(codDoc, estab + "-" + ptoEmi + "-" + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "PD", msjT, tb_infoA.Rows[0]["sociedad"].ToString(), tb_infoA.Rows[0]["numeroAsientoContable"].ToString(), tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                DB.Desconectar();
                                                clsLogger.Graba_Log_Error(ex.Message);
                                                log.mensajesLog("EM016", claveAcceso, ex.Message, "", codigoControl, " Validación de Comprobantes, WebService validación1: No se encontro informacion adicional");
                                            }

                                        }
                                    }

                                }

                                if (edo == "AUTORIZADO")
                                {
                                    try
                                    {
                                        DB.Conectar();
                                        DB.CrearComando(@"UPDATE GENERAL SET estado=@estado WHERE codigoControl = @codigoControl");
                                        DB.AsignarParametroCadena("@estado", "1");
                                        DB.AsignarParametroCadena("@codigoControl", codigoControl);
                                        DB.EjecutarConsulta1();
                                        DB.Desconectar();
                                        //enviar_correo();

                                        DB.Conectar();
                                        DB.CrearComando(@"UPDATE GENERAL SET creado= @creado WHERE codigoControl = @codigoControl");
                                        DB.AsignarParametroCadena("@creado", "1");
                                        DB.AsignarParametroCadena("@codigoControl", codigoControl);
                                        DB.EjecutarConsulta1();
                                        DB.Desconectar();
                                    }
                                    catch (Exception ex)
                                    {
                                        DB.Desconectar();
                                        clsLogger.Graba_Log_Error(ex.Message);
                                        log.mensajesLog("US001", "", ex.Message.ToString(), "", codigoControl, " Error update estados. ");
                                    }


                                }

                            }
                            else
                            {
                                log.mensajesLog("US001", "", msjT, "", codigoControl, " Comprobante no se puede firmar.");
                            }
                        }
                        else
                        {

                            log.mensajesLog("US001", "", msjT, "", codigoControl, " Comprobante no cumple el esquema de Rentas.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("US001", "Error en clase procesar " + ex.Message, ex.Message, "", codigoControl, " Error en clase procesar.");
            }
        }

        private XmlDocument generarXML()
        {
            XmlDocument xDocTemp = new XmlDocument();
            try
            {
                switch (codDoc)
                {
                    case "01":
                        xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\factura.xsd";
                        xDocTemp = gXml.xmlFactura();
                        break;
                    case "03":
                        xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\LiquidacionCompra.xsd";
                        xDocTemp = gXml.xmlLiquidacionCompras();
                        break;
                    case "04":
                        xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\notaCredito.xsd";
                        xDocTemp = gXml.xmlNotaCredito(); 
                        break;
                    case "05":
                        xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\notaDebito.xsd";
                        xDocTemp = gXml.xmlNotaDebito();
                        break;
                    case "06":
                        xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\guiaRemision.xsd";
                        xDocTemp = gXml.xmlGuiaRemision();
                        break;
                    case "07":
                        if (version == "2.0.0" || version == "2.0")
                        {
                            xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\ComprobanteRetencion_V2.0.0.xsd";
                            xDocTemp = gXml.xmlComprobanteRetencionV2();
                        }
                        else
                        {
                            xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsd\comprobanteRetencion.xsd";
                            xDocTemp = gXml.xmlComprobanteRetencion();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("EM011", "Excepcion al agregar nodo autorización", ex.Message, "Leer.cs", "", "metodo generarXML");
            }
            return xDocTemp;
        }

        private string generarClaveAcceso()
        {
            try
            {
                string clave, codigoNumerico;
                //int aleat3 = r.Next(1000000, 9999999);
                codigoNumerico = secuencial.Substring(1);//digitoVerificador(aleat3.ToString().Trim());
                clave = Convert.ToDateTime(fechaEmision).ToString("ddMMyyyy") + codDoc + ruc;
                clave += ambiente + (estab + ptoEmi) + secuencial + codigoNumerico + tipoEmision;
                clave = digitoVerificador(clave);
                return clave;
            }
            catch (Exception ex)
            {
                msj = "";
                msjT = ex.Message;
                log.mensajesLog("EM014", "", msjT, "", codigoControl, "");
                return "";
            }
        }

        private string generarClaveAccesoContingencia(string codigoNumerico)
        {
            try
            {
                string clave;
                clave = Convert.ToDateTime(fechaEmision).ToString("ddMMyyyy") + codDoc;
                clave += codigoNumerico + "2"; //emision por indisponibilidad
                clave = digitoVerificador(clave);
                return clave;
            }
            catch (Exception ex)
            {
                msj = "";
                msjT = ex.Message;
                log.mensajesLog("EM014", "", msjT, "", codigoControl, "");
                return "";
            }
        }

        private static string digitoVerificador(string number)
        {
            int Sum = 0;
            for (int i = number.Length - 1, Multiplier = 2; i >= 0; i--)
            {
                Sum += (int)char.GetNumericValue(number[i]) * Multiplier;

                if (++Multiplier == 8) Multiplier = 2;
            }
            string Validator = (11 - (Sum % 11)).ToString().Trim();

            if (Validator == "11") Validator = "0";
            else if (Validator == "10") Validator = "1";

            return number + Validator;
        }

        public string obtenerSecuencial(string folio)
        {
            string ultimoFolio = "";
            string code = "";
            BasesDatos DB = new BasesDatos();
            try
            {
                if (String.IsNullOrEmpty(folio))
                {
                    DB.Desconectar();
                    DB.Conectar();
                    DB.CrearComando(@"SELECT MAX(SEC) FROM (select ISNULL(MAX(CONVERT(int,secuencial)),0)+ 1 as sec from General with(nolock) where estab= @estab and ptoEmi= @ptoEmi and codDoc = @codDoc and ambiente = @ambiente
                UNION ALL
                select ISNULL(CONVERT(int,secuencial),0)+ 1 as sec from dbo.CajaSucursal with(nolock) where estab= @estab and ptoEmi= @ptoEmi and NumeroRentas = @codDoc and estadoPro = @ambiente) AS SEC");
                    DB.AsignarParametroCadena("@estab", estab);
                    DB.AsignarParametroCadena("@ptoEmi", ptoEmi);
                    DB.AsignarParametroCadena("@codDoc", codDoc);
                    DB.AsignarParametroCadena("@ambiente", ambiente);
                    DB.AsignarParametroCadena("@estab", estab);
                    DB.AsignarParametroCadena("@ptoEmi", ptoEmi);
                    DB.AsignarParametroCadena("@codDoc", codDoc);
                    DB.AsignarParametroCadena("@ambiente", ambiente);
                    using (DbDataReader DR = DB.EjecutarConsulta())
                    {
                        if (DR.Read())
                        {
                            ultimoFolio = DR[0].ToString().Trim();
                        }
                    }

                    DB.Desconectar();
                }
                else
                {
                    ultimoFolio = folio;

                }

                switch (ultimoFolio.ToString().Trim().Length)
                {
                    case 1:
                        code = "00000000" + ultimoFolio.ToString().Trim();
                        break;
                    case 2:
                        code = "0000000" + ultimoFolio.ToString().Trim();
                        break;
                    case 3:
                        code = "000000" + ultimoFolio.ToString().Trim();
                        break;
                    case 4:
                        code = "00000" + ultimoFolio.ToString().Trim();
                        break;
                    case 5:
                        code = "0000" + ultimoFolio.ToString().Trim();
                        break;
                    case 6:
                        code = "000" + ultimoFolio.ToString().Trim();
                        break;
                    case 7:
                        code = "00" + ultimoFolio.ToString().Trim();
                        break;
                    case 8:
                        code = "0" + ultimoFolio.ToString().Trim();
                        break;
                    case 9:
                        code = ultimoFolio.ToString().Trim();
                        break;
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
            

            return code;
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

        private Boolean enviarComprobante(Byte[] xml1)
        {
            bool retorno = false;
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
                    //log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " EMPIEZA ENVIO DOCUMENTO");
                    //Se llama a un metodo del servicio.
                    recepcionTrace.Timeout = 20000;
                    var result = recepcionTrace.validarComprobante(xml1);
                    //Se accede a la objeto "XmlRequest" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                    var soapRequest = TraceExtension.XmlRequest.OuterXml;
                    //Se accede a la objeto "XmlResponse" de la clase TraceExtension y llamamos a su propiedad "OuterXml".
                    soapResponse = TraceExtension.XmlResponse.OuterXml;
                    // log.mensajesLog("EM016", "CICLO   " + claveAcceso, msjT, "", secuencial, " TERMINA ENVIO DOCUMENTO");
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
                    //mensajes_error_usuario_envio(mensaje);
                    string mensaje_us = "Error en generar documento: " + obtener_tag_Element(mensaje, "mensaje") + Environment.NewLine + ": " + obtener_tag_Element(mensaje, "informacionAdicional");
                    log.mensajesLog("US001", mensaje_us, soapResponse, "", codigoControl, " Recepción de Comprobantes, WebService envio ");

                    enviar_notificacion_correo_punto(estab, codDoc + estab + ptoEmi + secuencial, fechaEmision, mensaje_us);

                    if (obtener_tag_Element(mensaje, "identificador").Equals("43")) //CLAVE ACCESO REGISTRADA
                        retorno = true;

                    //return false;
                }
            }
            catch (Exception ex)
            {
                msj = "";
                msjT = ex.Message;
                //log.mensajesLog("EM016", "Se usará una clave de contingencia en lugar de: " + claveAcceso + " " + msjT, Encoding.Default.GetString(xml1), "", codigoControl, " Recepción de Comprobantes, WebService envio. XML: " + Encoding.Default.GetString(xml1));
                return false;
            }

            return retorno;
            /*
            "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">
             * <soap:Body>
             * <ns2:validarComprobanteResponse xmlns:ns2=\"http://ec.gob.sri.ws.recepcion\">
             * <RespuestaRecepcionComprobante>
             * <estado>RECIBIDA</estado>
             * <comprobantes />
             * </RespuestaRecepcionComprobante></ns2:validarComprobanteResponse>
             * </soap:Body></soap:Envelope>"
             */
        }

        private Boolean validarAutorizacion(string clave)
        {
            XmlDocument xmlAutorizacion = new XmlDocument();
            //string edo = "";
            edo = "";
            BasesDatos DB = new BasesDatos();
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
                                //aux = fechaAutorizacion.Substring(0, fechaAutorizacion.IndexOf("."));
                                DateTime dt_aut = DateTime.Parse(fechaAutorizacion);
                                aux = dt_aut.ToString("yyyy-MM-ddTHH:mm:ss");
                                //if (fechaAutorizacion.Contains("."))
                                //{
                                //    aux = fechaAutorizacion.Substring(0, fechaAutorizacion.IndexOf("."));
                                //    fechaAutorizacion = aux;
                                //}
                                //else
                                //{
                                //    aux = fechaAutorizacion;
                                //}

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

                                // RespuestaLFWS(codDoc, estab + ptoEmi + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "AT", "AT");
                                try
                                {

                                    idcomprobante2 = obtenerid_comprobante(codigoControl);
                                    // log.mensajesLog("EM016", claveAcceso, "idcomprobante " + idComprobante, "", codigoControl, "enviando id comprobante para extraer informacion adicional");
                                    DataTable tb_infoA = obtener_infoAdicional(idcomprobante2);
                                    if (tb_infoA.Rows.Count > 0)
                                    {
                                        //log.mensajesLog("EM016", claveAcceso, "datos a enviar sociedad", "", codigoControl, "informacion  numero documento " + estab + "-" + ptoEmi + "-" + secuencial + "  sociedad " + tb_infoA.Rows[0]["sociedad"].ToString() + " numeroAsientoContable " + tb_infoA.Rows[0]["numeroAsientoContable"].ToString() + " anioAsientoContable " + tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                        RespuestaWebUNASAP(codDoc, estab + "-" + ptoEmi + "-" + secuencial, claveAcceso, numeroAutorizacion, fechaAutorizacion, fechaAutorizacion, "", "", "", "AT", "AT", tb_infoA.Rows[0]["sociedad"].ToString(), tb_infoA.Rows[0]["numeroAsientoContable"].ToString(), tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DB.Desconectar();
                                    clsLogger.Graba_Log_Error(ex.Message);
                                    log.mensajesLog("EM016", claveAcceso, ex.Message, "", codigoControl, " Validación de Comprobantes, WebService validación1: No se encontro informacion adicional");
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
                                        //XmlElement mensaje = (XmlElement)autorizacionNodo.GetElementsByTagName("mensaje")[0];
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
                                                    log.mensajesLog("EM016", claveAcceso, ex.Message, "", codigoControl, " Validación de Comprobantes, WebService validación1: No se encontro informacion adicional");
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (i_contador == intentos_autorizacion)
                                        {
                                            //emite_doc_prov();
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
                                    //emite_doc_prov();
                                    if (actualiza_estado_factura("1", "1", "E", codigoControl))
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
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                msjT = ex.Message;
                //log.mensajesLog("EM016", claveAcceso, msjT, "", estab + ptoEmi + secuencial, " Validación de Comprobantes, WebService validación4 ");

                if (actualiza_estado_factura("1", "1", "E", codigoControl))
                {
                    log.mensajesLog("EM016", claveAcceso, msjT, "", codigoControl, " Validación de Comprobantes, WebService validación4 ");
                }
                else
                {
                    log.mensajesLog("EM016", claveAcceso, msjT, "", codigoControl, " Validación de Comprobantes, WebService validación4: No se actualizó estado de factura ");
                }
                return false;

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

        private string obtenerClaveContingencia(string pr_ambiente)
        {
            BasesDatos DB = new BasesDatos();
            string codigoNumerico = "";
            try
            {
                DB.Conectar();
                DB.CrearComando(@"select top 1 clave FROM ClavesContignencia  with(nolock) where (estado is null or estado != '1') and tipo = '" + pr_ambiente + "' order by clave");
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    if (DR.Read()) { codigoNumerico = DR[0].ToString(); }
                    else { codigoNumerico = ""; }
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
            
            return codigoNumerico;
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
            }
            finally
            {
                DB.Desconectar();
            }
           
            return codigoidcomprobante;
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
                case 14: return "Error Compensacion (CS)";
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
                log.mensajesLog("ES001", "", e.Message, "", codigoControl);
            }
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
            strCadena = strCadena.Replace("Ý", "i");
            strCadena = strCadena.Replace("Ð", "n");

            strCadena = strCadena.Replace("Á", "A");
            strCadena = strCadena.Replace("É", "E");
            strCadena = strCadena.Replace("Í", "I");
            strCadena = strCadena.Replace("Ó", "O");
            strCadena = strCadena.Replace("Ú", "U");
            strCadena = strCadena.Replace("Ñ", "N");
            strCadena = strCadena.Replace("º", "o");
            strCadena = strCadena.Replace("°", "o");
            strCadena = strCadena.Replace("'", "");
            strCadena = strCadena.Replace("''", "");
            return strCadena;
        }

        public string VerificaEspacios(string strCadena)
        {
            strCadena = strCadena.Replace("     ", " ").Trim();
            strCadena = strCadena.Replace("    ", " ").Trim();
            strCadena = strCadena.Replace("   ", " ").Trim();
            strCadena = strCadena.Replace("  ", " ").Trim();
            return strCadena;
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
            }
            finally
            {
                DB.Desconectar();
            }
            
            return retorno;
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

        private void mensajes_error_usuario_envio(XmlElement msg)
        {
            string respuesta = "";
            respuesta = respuesta + obtener_tag_Element(msg, "tipo") + ": " + obtener_tag_Element(msg, "mensaje") + Environment.NewLine + ": " + obtener_tag_Element(msg, "informacionAdicional") + Environment.NewLine;
            if (!respuesta.Equals(""))
            {
                log.mensajesLog("US001", respuesta, "Mensaje Usuario", "", codigoControl, "");
            }
        }

        private void enviar_correo0()
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                string nomDoc = "";
                DB.Conectar();
                DB.CrearComando("SELECT emailsRegla FROM EmailsReglas  with(nolock) WHERE Receptor=@rfcrec AND estadoRegla=1");
                DB.AsignarParametroCadena("@rfcrec", identificacion);
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    if (DR3.Read())
                    {
                        emails = emails.Trim(',') + "," + DR3[0].ToString().Trim(',') + "";
                    }
                }

                DB.Desconectar();

                nomDoc = CodigoDocumento(codDoc);

                emails = emails.Trim(',');
                EM = new EnviarMail();
                EM.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);
                if (emails.Length > 10)
                {
                    EM.adjuntar(RutaDOC + codigoControl + ".pdf");
                    EM.adjuntar(RutaDOC + codigoControl + ".xml");

                    asunto = nomDoc + " electrónica No: " + estab + "-" + ptoEmi + "-" + secuencial + " de " + compania;
                    mensaje = @"Estimado(a) cliente;  <br><br>
							Acaba de recibir el documento electrónico: " + @"<br>
                            Tipo: " + nomDoc + @"<br>
                            N&#250;mero: " + estab + "-" + ptoEmi + "-" + secuencial + @". <br>
							Fecha: " + fechaEmision + ".";
                    mensaje += "<br><br>Saludos cordiales, ";
                    mensaje += "<br>" + compania + ", ";
                    mensaje += "<br><br>Servicio proporcionado por CimaIT";
                    mensaje += "<br>Tel. 593 04 2280217";

                    EM.llenarEmail(emailEnviar, emails.Trim(','), "", "", asunto, mensaje);
                    try
                    {
                        EM.enviarEmail();
                    }
                    catch (System.Net.Mail.SmtpException ex)
                    {
                        msjT = ex.Message;
                        DB.Desconectar();
                        log.mensajesLog("EM001", emails + " ", msjT, "", codigoControl, "");
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

        private void enviar_correo()
        {
            string nomDoc = "";
            try
            {
                //DB.Conectar();
                //DB.CrearComando("SELECT top 1 emailsRegla FROM EmailsReglas  WHERE SUBSTRING(nombreRegla,1,7)=SUBSTRING(@rfcrec,1,7) AND estadoRegla=1");
                //DB.AsignarParametroCadena("@rfcrec", destinatarioLF);
                //DbDataReader DR3 = DB.EjecutarConsulta();
                //if (DR3.Read())
                //{
                //    emails = emails.Trim(',') + "," + DR3[0].ToString().Trim(',') + "";
                //}
                //DB.Desconectar();

                //System.Net.Mail.MailAddress sender = new System.Net.Mail.MailAddress(emailEnviar, emailEnviar);

                nomDoc = CodigoDocumento(codDoc);
                emails = emails.Trim(',');
                EM = new EnviarMail();

                EM.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);

                if (emails.Length > 10)
                {
                    //EM.adjuntar(RutaDOC + codigoControl + ".pdf");
                    //EM.adjuntar(RutaDOC + codigoControl + ".xml");
                    EM.adjuntar_xml(consulta_archivo_xml(codigoControl, 7), codigoControl + ".xml");
                    EM.adjuntar_xml(cPDF.msPDF(codigoControl), codigoControl + ".pdf");

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

                    }
                    catch (System.Net.Mail.SmtpException ex)
                    {
                        msjT = ex.Message;
                        //DB.Desconectar();
                        log.mensajesLog("EM001", emails + " ", msjT, "", codigoControl, "");
                    }
                }
                else
                {
                    if (codDoc.Equals("06"))
                    {
                        cPDF.msPDF(codigoControl);
                    }
                }
            }
            catch (Exception mex)
            {
                msjT = mex.Message;
                clsLogger.Graba_Log_Error(mex.Message);
                log.mensajesLog("EM001", emails + "ERROR ENVIAR MAIL", msjT, "", codigoControl, "");

            }
        }

        private Boolean valida_duplicidad_NC(string valor)
        {
            Boolean rpt = false;
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando("select idComprobante from GENERAL with(nolock) where numDocModificado = @valor and codDoc='04'");
                DB.AsignarParametroCadena("@valor", valor);
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
            }
            finally
            {
                DB.Desconectar();
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

        private String verifica_puntoEmision(string pr_tipo, string pr_estab, string pr_ptoEmi)
        {
            string rpt = "N";
            BasesDatos DB = new BasesDatos();
            try
            {

                DB.Conectar();
                DB.CrearComando("select top 1 estadoPro from CajaSucursal with(nolock) where NumeroRentas = @tipo and  estab = @estab and ptoEmi = @ptoEmi and estado='A' and estadoFE ='A'");
                DB.AsignarParametroCadena("@tipo", pr_tipo);
                DB.AsignarParametroCadena("@estab", pr_estab);
                DB.AsignarParametroCadena("@ptoEmi", pr_ptoEmi);
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    if (DR3.Read())
                    {
                        rpt = DR3[0].ToString();

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
            

            return rpt;
        }
        
        private void enviar_notificacion_correo_punto(string pr_estab, string pr_folio, string pr_fechaEmision, string pr_mensaje)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                String correos = "";
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

        private void ModificaPDF(string p_tipoDoc, string p_doc_mod, string docNC)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                string v_codigoControl = "";

                DB.Conectar();
                DB.CrearComando(@"select top 1 codigoControl FROM [GENERAL] with(nolock) where codDoc = @tipoDoc and estab + ptoEmi + secuencial = @doc_mod");
                DB.AsignarParametroCadena("@tipoDoc", p_tipoDoc);
                DB.AsignarParametroCadena("@doc_mod", p_doc_mod);
                using (DbDataReader drDoc = DB.EjecutarConsulta())
                {
                    while (drDoc.Read())
                    {
                        v_codigoControl = drDoc[0].ToString().Trim();
                    }
                }

                DB.Desconectar();

                if (!String.IsNullOrEmpty(v_codigoControl))
                {
                    cPDF.modificaPDF(RutaDOC + v_codigoControl + ".pdf", RutaCER + v_codigoControl + ".pdf", "NOTA DE CRÉDITO: " + docNC, 180, 252);//Factura
                    //cPDF.modificaPDF(RutaDOC + "\\traza\\" + v_codigoControl + ".pdf", RutaCER + v_codigoControl + ".pdf", "NOTA DE CRÉDITO: " + docNC, 180, 95); //Trazabilidad
                }
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("EM001" + "Error en modificar N/C. " + ex.Message, ex.Message, ex.Message, "", codigoControl, "");
            }

        }

        //Validar si el secuencial ya fue procesado
        private Boolean valida_duplicidad(string p_codDoc, string p_estab, string p_ptoEmi, string p_secuencial, string p_ambiente)
        {
            Boolean rpt = false;
            BasesDatos DB = new BasesDatos();
            try
            {

                DB.Conectar();
                DB.CrearComando("select idComprobante ,ISNULL(tipo,'') tipo from GENERAL with(nolock) where codDoc = @p_codDoc and estab = @p_estab and ptoEmi = @p_ptoEmi and secuencial = @p_secuencial and ambiente = @p_ambiente");
                DB.AsignarParametroCadena("@p_codDoc", p_codDoc);
                DB.AsignarParametroCadena("@p_estab", p_estab);
                DB.AsignarParametroCadena("@p_ptoEmi", p_ptoEmi);
                DB.AsignarParametroCadena("@p_secuencial", p_secuencial);
                DB.AsignarParametroCadena("@p_ambiente", p_ambiente);
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    if (DR3.Read())
                    {
                        rpt = ((DR3["tipo"].ToString().Equals("N") || DR3["tipo"].ToString().Equals("")) ? false : true);
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
           
            return rpt;
        }

        //Emitir documento contingencia provicional
        //private void emite_doc_prov(string p_RutaDOC, string p_codigoControl, string p_id_Comprobante, string p_codDoc, string p_tipoEnvio)
        private void emite_doc_prov()
        {
            try
            {
                //enviar_correo();
                //cPDF.PoblarReporte(RutaDOC, codigoControl, gBD.id_Comprobante, codDoc);

            }
            catch (Exception ex)
            {

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

        private void validarXML(string p_CodDoc, string p_xml)
        {
            XmlSchema v_xsd = new XmlSchema();

            try
            {
                switch (p_CodDoc)
                {
                    case "01":
                        v_xsd.SourceUri = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\factura.xsd";
                        break;
                    case "04":
                        v_xsd.SourceUri = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\notaCredito.xsd";
                        break;
                    case "05":
                        v_xsd.SourceUri = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\notaDebito.xsd";
                        break;
                    case "06":
                        v_xsd.SourceUri = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\guiaRemision.xsd";
                        break;
                    case "07":
                        v_xsd.SourceUri = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\comprobanteRetencion.xsd";
                        break;
                }
                XmlReaderSettings xmlConfiguracion = new XmlReaderSettings();
                xmlConfiguracion.Schemas.Add(v_xsd);
                xmlConfiguracion.ValidationType = ValidationType.Schema;
                xmlConfiguracion.ValidationEventHandler += new ValidationEventHandler(booksSettingsValidationEventHandler);

                XmlReader books = XmlReader.Create(p_xml, xmlConfiguracion);

                while (books.Read()) { }
            }
            catch (Exception ex)
            {
            }
        }

        static void booksSettingsValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                Console.Write("WARNING: ");
                Console.WriteLine(e.Message);
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                Console.Write("ERROR: ");
                Console.WriteLine(e.Message);
            }
        }


        private string obtenerXSD(string p_codDoc, string p_version)
        {
            string archivo_xsd = "";

            switch (p_codDoc)
            {
                case "01":
                    archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\factura.xsd";
                    break;
                case "04":
                    archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\notaCredito.xsd";
                    break;
                case "05":
                    archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\notaDebito.xsd";
                    break;
                case "06":
                    archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\guiaRemision.xsd";
                    break;
                case "07":
                    archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\comprobanteRetencion.xsd";
                    break;
            }
            return archivo_xsd;
        }


        private Boolean ValidarEstructuraXSD(string p_doc, string p_codDoc, string p_version, out string p_msj, out string p_msjT)
        {
            XmlTextReader lector;
            Boolean b = false;
            p_msj = ""; p_msjT = "";

            try
            {
                lector = new XmlTextReader(new StringReader(p_doc));
                lector.Read();

                ValidacionEstructura VE = new ValidacionEstructura();

                VE.agregarSchemas(obtenerXSD(p_codDoc, p_version));
                if (VE.Validar(lector))
                {
                    p_msj += VE.msj;
                    p_msjT += VE.msjT;
                    b = true;
                }
                else
                {
                    p_msj += VE.msj;
                    p_msjT += VE.msjT;
                    //log.mensajesLog("EM015", " Los datos no son correctos. ", msjT, "", codigoControl, "");
                    b = false;
                }
            }
            catch (Exception ex)
            {
                p_msj += "Archivo no superó la validación";
                p_msjT += ex.Message;
            }
            return b;
        }

        private string obtener_codigo(string a_parametro)
        {
            string retorna = ConfigurationManager.AppSettings.Get(a_parametro);

            return retorna;
        }

        private double obtener_iva(string codigoPorcentaje)
        {
            double Ivap = 0.00;
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComandoProcedimiento("PA_obtener_IVA");
                DB.AsignarParametroProcedimiento("@p_codigo", System.Data.DbType.String, codigoPorcentaje);
                using (DbDataReader DR4 = DB.EjecutarConsulta())
                {
                    if (DR4.Read())
                    {
                        Ivap = Convert.ToDouble(DR4["valor"].ToString()) / 100;
                    }
                }

                DB.Desconectar();

                return Ivap;
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                msjT = ex.Message;
                log.mensajesLog("BD001", claveAcceso, msjT, "", codigoControl, " Error consulta metodo obtener_codigo_iva.");
                msjT = "";
            }
            return Ivap;
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

        private string obtener_codigo_tipoIdentificacion(string a_parametro, string p_numIde)
        {
            string retorna = "";
            int pint;
            try
            {
                if (!string.IsNullOrEmpty(a_parametro))
                {
                    switch (a_parametro)
                    {
                        case "1":
                        case "P":
                            retorna = "05";
                            break;
                        case "PP":
                            retorna = "06";
                            break;
                        case "IE":
                            retorna = "08";
                            break;
                        default:
                            retorna = "04";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                msjT += "Error en método: obtener_codigo_tipoIdentificacion. Error: " + ex.Message;
            }

            return retorna;
        }

        public string obtener_codigo_renta(string p_codigo)
        {
            string rpt = "";
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando(@"select top 1 codigo from [CatImpuestos_C] with(nolock) where CONVERT(varchar(20),comentarios) = @p_codigo and tipo = 'LFIRCODE'");
                DB.AsignarParametroCadena("@p_codigo", p_codigo);
                using (DbDataReader DR4 = DB.EjecutarConsulta())
                {
                    if (DR4.Read())
                    {
                        rpt = DR4["codigo"].ToString();
                    }
                }

                DB.Desconectar();

                return rpt;
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                msjT = ex.Message;
                log.mensajesLog("BD001", claveAcceso, msjT, "", codigoControl, " Error consulta metodo obtener_codigo_renta.");
                msjT = "";
                return rpt;
            }

        }

        public void RespuestaLFWS(string p_codDoc, string p_numDoc, string p_Accesskey, string p_Autorizacion, string p_Authdate, string p_Authtime, string p_Contingency, string p_Contdate, string p_Conttime, string p_status, string p_Message)
        {

            //log.mensajesLog("US001", "Respuesta LF: p_codDoc: " + obtener_codigo(p_codDoc) + "p_numDoc:" + p_numDoc + " p_Accesskey:" + p_Accesskey + " p_Autorizacion:" + p_Autorizacion + " p_Authdate:" + p_Authdate + " p_Authtime:" + p_Authtime + " p_Contingency:" + p_Contingency + " p_Contdate:" + p_Contdate + " p_Conttime:" + p_Conttime + " p_status:" + p_status + " p_Message:" + p_Message, "Mensaje Usuario", "", p_Accesskey, "");
            try
            {
                LFWSrpt.infoAutSRIString ias1 = new LFWSrpt.infoAutSRIString();
                LFWSrpt.infoAutSRIString2 ias2 = new LFWSrpt.infoAutSRIString2();


                //pdoc = p_codDoc;
                if (esNDFinLF)
                {
                    p_codDoc = "05A";
                }

                string msj1 = "";

                DateTime d_fAut = DateTime.Today;
                if (!String.IsNullOrEmpty(p_Authdate))
                {
                    d_fAut = Convert.ToDateTime(p_Authdate);
                }


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
                    ias2.p_Contdate = DateTime.Now.ToString("dd/MM/yyyy");// formato_fecha(p_Contdate, "dd/MM/yyyy");
                    ias2.p_Conttime = DateTime.Now.ToString("HH:mm:ss"); //formato_fecha(p_Contdate, "HH:mm:ss");
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
                    wsap.IDoc.Fauth = this.formato_fecha(p_Authdate, "yyyy-MM-dd");
                    wsap.IDoc.Dauth = this.formato_fecha(p_Authdate, "HH:mm:ss");
                }
                else
                {
                    wsap.IDoc.Fauth = "";
                    wsap.IDoc.Dauth = "";
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
                        log.mensajesLog("US001", "Respuesta Exitosa: " + "Estatus: " + o_status + "Mensaje: " + o_message, o_message, "", codigoControl, "");
                        actualizaEstadosWLF(o_status, p_status, codigoControl);
                    }
                }
                catch (Exception ex) 
                {
                    this.log.mensajesLog("US001", "Error web service n0:Zecsrifm01Response: No se controlo parametro envio primer cath " + p_numDoc, ex.Message, "", p_numDoc, "ConsultaOff");
                    actualizaEstadosWLF("N", p_status, codigoControl);
                }
            }
            catch (Exception ex)
            {
                log.mensajesLog("US001", "Error web service n0:Zecsrifm01Response: No se controlo parametro envio " + p_numDoc, ex.Message, "", codigoControl, "");
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

        public string obtener_dir_Sucursal(string p_codigo)
        {
            string rpt = "";
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Conectar();
                DB.CrearComando(@"select top 1 domicilio from [Sucursales] with(nolock) where clave = @p_codigo");
                DB.AsignarParametroCadena("@p_codigo", p_codigo);
                using (DbDataReader DR4 = DB.EjecutarConsulta())
                {
                    if (DR4.Read())
                    {
                        rpt = DR4["domicilio"].ToString();
                    }
                }

                DB.Desconectar();

                return rpt;
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                msjT = ex.Message;
                log.mensajesLog("BD001", claveAcceso, msjT, "", codigoControl, " Error consulta metodo obtener_dir_Sucursal.");
                msjT = "";
                return rpt;
            }

        }

        public void enviar_notificacion_contingencia(string pr_ruc, string pr_ambiente, string pr_limite)
        {
            BasesDatos DB = new BasesDatos();

            string claves = "";
            try
            {

                DB.Conectar();
                DB.CrearComando(@"select count(idClaveContingencia) contingencia FROM [ClavesContignencia] with(nolock) where ruc = '" + pr_ruc + "' and tipo = '" + pr_ambiente + "' and isnull(estado,'0') = '0' having count(idClaveContingencia) < " + pr_limite);
                using (DbDataReader DR3 = DB.EjecutarConsulta())
                {
                    if (DR3.Read())
                    {
                        claves = DR3[0].ToString();
                    }
                }

                DB.Desconectar();

                int id_int = 0;
                int.TryParse(claves, out id_int);

                if (id_int > 0)
                {
                    String correos = "";
                    DB.Conectar();
                    DB.CrearComando(@"select top 1 a.correo from  dbo.Sucursales a  with(nolock) where a.eliminado = '0'");
                    using (DbDataReader DR4 = DB.EjecutarConsulta())
                    {
                        while (DR4.Read())
                        {
                            correos = correos.Trim(',') + "," + DR4[0].ToString().Trim(',') + "";
                        }
                    }

                    DB.Desconectar();

                    correos = correos.Trim(',');
                    EM = new EnviarMail();
                    EM.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);

                    if (correos.Length > 10)
                    {
                        asunto = "Notificación de claves de contingencia " + compania;
                        mensaje = @"Estimado(a);  <br>
							Le informamos que tiene " + claves + " claves de contingencia disponibles. " + @"<br>
							Debe realizar las gestiones necesarias para obtener nuevas claves.";
                        mensaje += "<br><br>" + compania;
                        mensaje += "<br><br>Cualquier novedad comunicarse con su soporte a usuario";


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
                            log.mensajesLog("EM001", emails + " ", msjT, "", codigoControl, "Método enviar_notificacion_contingencia");
                            msjT = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("EM001", emails + " ", ex.Message, "", codigoControl, "Método enviar_notificacion_contingencia");
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

    }

}








