using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Datos;
using System.Configuration;
using ReportesDEI;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data.Common;
using clibLogger;
using System.Xml.Linq;

namespace InvoicecImpresionUnacem
{
    class CrearPDF
    {
        private BasesDatos DB;
        private BasesDatos DBPI;
        string msj = "";
        string msjT = "";
        DatosComprobantes dsPc;
        string compen = "";
        string estab2 = "";
        string ptoemi2 = "";
        DateTime fecha;
        int cont = 0;

        public void PoblarReporte(out MemoryStream ruta, string nombre, string idComprobante, string comprobante, string categoriaNegocio = null)
        {
            ruta = null;
            DB = new BasesDatos();
            SqlConnection sqlConn;
            sucursal(idComprobante);
            dsPc = new DatosComprobantes();
            String strConn;
            strConn = ConfigurationManager.ConnectionStrings["dataexpressConnectionString"].ConnectionString;

            String StrComprobante = @"
                DECLARE @strXML XML = ''
	                ,@IdDoc INT = 0
	                ,@strXML2 XML = ''
	                ,@COD_DOC VARCHAR(2) = ''
	                ,@nameOpenXML VARCHAR(50) = ''
	                ,@INFODOCUMENTO VARCHAR(50) = ''
	                ,@NOMBRECOMPROBANTE VARCHAR(50) = ''
	                ,@nameOpenXML2 VARCHAR(50) = ''
	                ,@strXMLNormal XML = ''
	                ,@strXMLAUT XML = ''
	                ,@TIPO VARCHAR(1) = ''
	                ,@ESTADO VARCHAR(1) = ''
                SELECT @strXMLNormal = AX.xmlEnviado
	                ,@strXMLAUT = AX.xmlSRI
	                ,@COD_DOC = G.codDoc
	                ,@TIPO = G.tipo
	                ,@ESTADO = G.estado
                FROM dbo.GENERAL G
                INNER JOIN dbo.ArchivoXml AX ON AX.codigoControl = G.codigoControl
                WHERE G.idComprobante ='" + idComprobante + @"'
                IF @strXMLNormal IS NULL
                BEGIN
	                SET @strXML = @strXMLAUT
                END
                ELSE
                BEGIN
	                SET @strXML = @strXMLNormal
                END
                EXEC sp_xml_preparedocument @IdDoc OUTPUT
	                ,@strXML
                IF @strXMLNormal IS NULL
	                AND @TIPO = 'E'
	                AND @ESTADO = '1'
                BEGIN
	                SELECT @strXML2 = replace(comprobante, '<?xml version=""1.0"" encoding=""UTF-8""?>', '')
	                FROM OPENXML(@IdDoc, '/autorizacion', 2) WITH (comprobante VARCHAR(max))
                END
                ELSE
                BEGIN
	                SET @strXML2 = replace(CONVERT(VARCHAR(MAX), @strXML), '<?xml version=""1.0"" encoding=""UTF-8""?>', '')
                END
                IF ISNULL(CAST(@strXML2 AS VARCHAR(max)), '') = ''
	                SET @strXML2 = @strXML
                EXEC sp_xml_removedocument @IdDoc
                EXEC sp_xml_preparedocument @IdDoc OUTPUT
	                ,@strXML2
                SELECT @INFODOCUMENTO = (
		                SELECT CASE @COD_DOC
				                WHEN '01'
					                THEN 'infoFactura'
				                WHEN '04'
					                THEN 'infoNotaCredito'
				                WHEN '03'
					                THEN 'infoLiquidacionCompra'
				                WHEN '05'
					                THEN 'infoNotaDebito'
				                WHEN '06'
					                THEN 'infoGuiaRemision'
				                WHEN '07'
					                THEN 'infoCompRetencion'
				                ELSE ''
				                END
		                )
	                ,@NOMBRECOMPROBANTE = (
		                SELECT CASE @COD_DOC
				                WHEN '01'
					                THEN 'factura'
				                WHEN '03'
					                THEN 'liquidacionCompra'
				                WHEN '04'
					                THEN 'notaCredito'
				                WHEN '05'
					                THEN 'notaDebito'
				                WHEN '06'
					                THEN 'guiaRemision'
				                WHEN '07'
					                THEN 'comprobanteRetencion'
				                END
		                )
                SET @nameOpenXML = '/' + @NOMBRECOMPROBANTE + '/' + @INFODOCUMENTO
                SET @nameOpenXML2 = '/' + @NOMBRECOMPROBANTE + '/infoTributaria'
                SELECT razonSocial
	                ,nombreComercial
	                ,dirMatriz
	                ,agenteRetencion
                INTO #tmpXmlaRet
                FROM OPENXML(@IdDoc, @nameOpenXML2, 2) WITH (
		                dirMatriz VARCHAR(300)
		                ,razonSocial VARCHAR(300)
		                ,nombreComercial VARCHAR(300)
		                ,agenteRetencion VARCHAR(300)
		                )
                SELECT direccionComprador
	                ,contribuyenteEspecial
	                ,obligadoContabilidad
	                ,dirEstablecimiento
	                ,dirPartida
	                ,razonSocialComprador
	                ,razonSocialSujetoRetenido
	                ,identificacionComprador
	                ,identificacionSujetoRetenido
                INTO #tmpXmla2Ret
                FROM OPENXML(@IdDoc, @nameOpenXML, 2) WITH (
		                dirEstablecimiento VARCHAR(300)
		                ,razonSocialComprador VARCHAR(300)
		                ,direccionComprador VARCHAR(300)
		                ,razonSocialSujetoRetenido VARCHAR(300)
		                ,identificacionComprador VARCHAR(300)
		                ,identificacionSujetoRetenido VARCHAR(300)
		                ,dirPartida VARCHAR(300)
		                ,contribuyenteEspecial VARCHAR(30)
		                ,obligadoContabilidad VARCHAR(30)
		                )
                SELECT GENERAL.idComprobante
	                ,GENERAL.id
	                ,GENERAL.version
	                ,GENERAL.serie
	                ,GENERAL.folio
	                ,GENERAL.fecha
	                ,GENERAL.sello
	                ,GENERAL.noCertificado
	                ,GENERAL.subTotal
	                ,GENERAL.total
	                ,GENERAL.tipoDeComprobante
	                ,GENERAL.firmaSRI
	                ,GENERAL.id_Config
	                ,GENERAL.id_Empleado
	                ,GENERAL.id_Receptor
	                ,GENERAL.id_Emisor
	                ,GENERAL.id_EmisorExp
	                ,GENERAL.id_ReceptorCon
	                ,(
		                SELECT descripcion
		                FROM Catalogo1_C
		                WHERE (codigo = GENERAL.ambiente)
			                AND (tipo = 'Ambiente')
		                ) AS ambienteDesc
	                ,(
		                SELECT descripcion
		                FROM Catalogo1_C AS Catalogo1_C_2
		                WHERE (codigo = GENERAL.tipoEmision)
			                AND (tipo = 'Emision')
		                ) AS tipoEmision
	                ,GENERAL.claveAcceso
	                ,(
		                SELECT descripcion
		                FROM Catalogo1_C AS Catalogo1_C_1
		                WHERE (codigo = GENERAL.codDoc)
			                AND (tipo = 'Comprobante')
		                ) AS codDocDesc
	                ,GENERAL.estab
	                ,GENERAL.ptoEmi
	                ,GENERAL.secuencial
	                ,GENERAL.totalSinImpuestos
	                ,GENERAL.totalDescuento
	                ,GENERAL.periodoFiscal
	                ,GENERAL.fechaIniTransporte
	                ,GENERAL.fechaFinTransporte
	                ,GENERAL.placa
	                ,ISNULL((
			                SELECT descripcion AS Expr1
			                FROM Catalogo1_C AS Catalogo1_C_1
			                WHERE (codigo = GENERAL.codDocModificado)
				                AND (tipo = 'Comprobante')
			                ), '') AS codDocModificadoDesc
	                ,GENERAL.codDocModificado
	                ,GENERAL.numDocModificado
	                ,GENERAL.fechaEmisionDocSustento
	                ,GENERAL.valorModificacion
	                ,GENERAL.moneda
	                ,GENERAL.propina
	                ,GENERAL.importeTotal
	                ,GENERAL.motivo
	                ,GENERAL.subtotal12
	                ,GENERAL.subtotal0
	                ,GENERAL.subtotalNoSujeto
	                ,GENERAL.ICE
	                ,GENERAL.IVA12
	                ,GENERAL.importeAPagar
	                ,EMISOR.IDEEMI
	                ,EMISOR.RFCEMI
	                ,EMISOR.NOMEMI
	                ,EMISOR.nombreComercial
	                ,isnull(XMLA.dirMatriz, EMISOR.dirMatriz) dirMatriz
	                ,EMISOR.telefonoE
	                ,RECEPTOR.IDEREC
	                ,RECEPTOR.RFCREC
	                ,ISNULL(RECEPTOR.direccionComprador,'') as direccionComprador
	                ,RECEPTOR.NOMREC
	                ,RECEPTOR.contribuyenteEspecial
	                ,RECEPTOR.obligadoContabilidad
	                ,RECEPTOR.tipoIdentificacionComprador
	                ,DOMEMIEXP.IDEDOMEMIEXP
	                ,isnull(XMLA2.dirEstablecimiento, DOMEMIEXP.dirEstablecimientos) dirEstablecimientos
	                ,GENERAL.codigoBarras
	                ,GENERAL.numeroAutorizacion
	                ,GENERAL.estado
	                ,GENERAL.fechaAutorizacion
	                ,GENERAL.rise
	                ,GENERAL.dirPartida
	                ,GENERAL.termino
	                ,GENERAL.proforma
	                ,GENERAL.pedido
	                ,RECEPTOR.domicilio
	                ,RECEPTOR.telefono
	                ,GENERAL.cantletras
                FROM GENERAL
                INNER JOIN EMISOR ON GENERAL.id_Emisor = EMISOR.IDEEMI
                INNER JOIN RECEPTOR ON GENERAL.id_Receptor = RECEPTOR.IDEREC
                INNER JOIN DOMEMIEXP ON GENERAL.id_EmisorExp = DOMEMIEXP.IDEDOMEMIEXP
                LEFT JOIN #tmpXmlaRet XMLA ON 1 = 1
                LEFT JOIN #tmpXmla2Ret XMLA2 ON 1 = 1
                WHERE General.idComprobante = '" + idComprobante + @"'
                drop table #tmpXmlaRet
				drop table #tmpXmla2Ret";

            String StrDetallesDest = @"SELECT idDetalles, 
                                            codigoPrincipal, 
                                            codigoAuxiliar, 
                                            descripcion, 
                                            cantidad,
                                            precioUnitario, 
                                            descuento, 
                                            precioTotalSinImpuestos, 
                                            id_Comprobante, 
                                            id_Destinatario
                                        FROM  Detalles WHERE id_Comprobante='" + idComprobante + "'";
            String StrDestinatarios = @"DECLARE @FECHA VARCHAR(MAX) ;
                                        SELECT idDestinatario, 
                                                identificacionDestinatario, 
                                                razonSocialDestinatario, 
                                                dirDestinatario, 
                                                motivoTraslado, 
                                                docAduaneroUnico, 
                                                codEstabDestino, 
                                                ruta, 
                                                ISNULL((SELECT descripcion
                                                        FROM Catalogo1_C AS Catalogo1_C_1
                                                        WHERE (codigo = Destinatarios.codDocSustento) AND (tipo = 'Comprobante')), '') AS codDocDesc,           
                                                numDocSustento, 
                                                numAutDocSustento, 
                                                case when  fechaEmisionDocSustento = '' then @FECHA
                                                    when  fechaEmisionDocSustento  IS NOT null then fechaEmisionDocSustento end as fechaEmisionDocSustento , 
                                                id_Comprobante
                                        FROM Destinatarios  WHERE id_Comprobante='" + idComprobante + "' ORDER BY razonSocialDestinatario DESC";
                        
            //Info Adicional1
            String StrInfoAdicional1 = @"DECLARE @cols AS NVARCHAR(MAX)
	                                        ,@query AS NVARCHAR(MAX)

                                        SELECT @cols = STUFF((
			                                        SELECT ',' + QUOTENAME(nombre)
			                                        FROM InfoAdicional
			                                        WHERE id_Comprobante = " + idComprobante + @"
			                                        FOR XML PATH('')
				                                        ,TYPE
			                                        ).value('.', 'NVARCHAR(MAX)'), 1, 1, '')

                                        SET @query = 'SELECT ' + @cols + '," + idComprobante + @" idComprobante 
                                                            from 
                                                             (
                                                                select ia.valor, ia.nombre
                                                                from InfoAdicional ia
                                                                where id_Comprobante = " + idComprobante + @"               
                                                            ) x
                                                            pivot 
                                                            (
                                                                max(valor)
                                                                for nombre in (' + @cols + ')
                                                            ) p '

                                        EXECUTE sp_executesql @query";
                        
            //Detalles AdicionalesGR
            String StrDetAdicionalGR1 = @"SELECT Unidad,Saldo_Anterior,Saldo_Actual,id_Detalles as idDetalles, Despachado, Programado, Viaje
                                         from 
                                         (
                                            select da.valor, da.nombre,da.id_Detalles, iaD.valor as Despachado, iaP.valor as Programado, iaV.valor as Viaje
                                            from DetallesAdicionales da
                                            inner join Detalles d on da.id_Detalles = d.idDetalles
                                            left join InfoAdicional iaD on iaD.id_Comprobante = d.id_Comprobante and iaD.nombre = 'despachadoM3'
				                            left join InfoAdicional iaP on iaP.id_Comprobante = d.id_Comprobante and iaP.nombre = 'programadoM3'
				                            left join InfoAdicional iaV on iaV.id_Comprobante = d.id_Comprobante and iaV.nombre = 'viaje'
                                            where d.id_Comprobante = " + idComprobante + @"               
                                        ) x
                                        pivot 
                                        (
                                            max(valor)
                                            for nombre in (Unidad,Saldo_Anterior,Saldo_Actual,idDetalles)
                                        ) p";

            String infoAdicionalGuiaPesoNeto = "select top 1 valor as Peso_Neto from InfoAdicional where nombre ='Peso_Neto' and id_Comprobante = " + idComprobante;

            sqlConn = new SqlConnection(strConn);
            try
            {
                switch (comprobante)
                {
                    case "06":
                        dsPc.EnforceConstraints = false;
                        using (SqlDataAdapter sqlDaComprobante = new SqlDataAdapter(StrComprobante, sqlConn))
                        {
                            sqlDaComprobante.Fill(dsPc, "Comprobante");
                        }

                        using (SqlDataAdapter sqlDaDestinatarios = new SqlDataAdapter(StrDestinatarios, sqlConn))
                        {
                            sqlDaDestinatarios.Fill(dsPc, "Destinatarios");
                        }

                        using (SqlDataAdapter sqlDaDetalleDest = new SqlDataAdapter(StrDetallesDest, sqlConn))
                        {
                            sqlDaDetalleDest.Fill(dsPc, "Detalles");
                        }

                        using (SqlDataAdapter sqlDaInfoAdicional1 = new SqlDataAdapter(StrInfoAdicional1, sqlConn))
                        {
                            sqlDaInfoAdicional1.Fill(dsPc, "InfoAdicionalGR");
                        }

                        using (SqlDataAdapter sqlDaDetAdicionales2 = new SqlDataAdapter(StrDetAdicionalGR1, sqlConn))
                        {
                            sqlDaDetAdicionales2.Fill(dsPc, "detAdicionalGR");
                        }

                        using (SqlDataAdapter sqlDaDetAdicionalGuia = new SqlDataAdapter(infoAdicionalGuiaPesoNeto, sqlConn))
                        {
                            dsPc.EnforceConstraints = false;
                            sqlDaDetAdicionalGuia.Fill(dsPc, "InfoAdicionalGRDetalle");
                        }

                        clsLogger.Graba_Log_Info("inicio envía guía");

                        if (estab2.Equals(obtener_codigo("estabEliminarCampos")) && ptoemi2.Equals(obtener_codigo("ptoemiEliminarCampos")))
                        {
                            clsLogger.Graba_Log_Info("Ingresa por condicion 014 - 024");

                            using (GuiaRemision014024 rptGR = new GuiaRemision014024())
                            {
                                rptGR.SetDataSource(dsPc);
                                try
                                {
                                    ruta = (MemoryStream)rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                }
                                catch (Exception)
                                {
                                    rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat).CopyTo(ruta = new MemoryStream());
                                }
                                rptGR.PrintOptions.PaperSize = PaperSize.PaperA4;
                                System.Drawing.Printing.PrinterSettings printersettings = new System.Drawing.Printing.PrinterSettings();
                                if ((estab2.Equals(obtener_codigo("estabprod")) && ptoemi2.Equals(obtener_codigo("ptoemiprod"))) || (estab2.Equals(obtener_codigo("estabprue")) && ptoemi2.Equals(obtener_codigo("ptoemiprue"))))
                                {
                                    clsLogger.Graba_Log_Info("Ingresa por segunda condicion 014 - 024");
                                    if (ConsultaDirEstablecimientoGuia(idComprobante))
                                    {
                                        clsLogger.Graba_Log_Info("enviando a impresora.. " + obtener_codigo("Impresora2"));
                                        printersettings.PrinterName = obtener_codigo("Impresora2");
                                    }
                                }
                                else
                                {
                                    clsLogger.Graba_Log_Info("enviando a impresora.. " + obtener_codigo("Impresora"));
                                    printersettings.PrinterName = obtener_codigo("Impresora");
                                }
                                printersettings.Copies = 1;
                                printersettings.Collate = false;
                                if ((estab2.Equals(obtener_codigo("estabprod")) && ptoemi2.Equals(obtener_codigo("ptoemiprod"))) || (estab2.Equals(obtener_codigo("estabprue")) && ptoemi2.Equals(obtener_codigo("ptoemiprue"))))
                                {
                                    rptGR.PrintToPrinter(printersettings, new System.Drawing.Printing.PageSettings(), false);
                                    cambioEstado("1", idComprobante);
                                    clsLogger.Graba_Log_Info("OK Impreso...: " + idComprobante);
                                }
                                else
                                {
                                    cambioEstado("2", idComprobante);
                                    clsLogger.Graba_Log_Info("NO Impreso...: " + idComprobante);
                                }
                                
                                rptGR.Dispose();
                                rptGR.Close();
                            }                            
                            
                        }
                        else
                        {
                            try
                            {
                                string TipoHoja = "", NombreImpresora = "", FormatoGuia = "", categoriaNegocio_ = "";
                                int Copias = 0;
                                bool Collate = false;
                                clsLogger.Graba_Log_Info("Consultando datos para impresion Parametros entrada");
                                clsLogger.Graba_Log_Info("Establecimiento " + estab2 + " PuntoEmision " + ptoemi2 + " CategoriaNegocio " + categoriaNegocio);
                                consultaDatosImpresion(estab2, ptoemi2, categoriaNegocio,
                                    out TipoHoja, out Copias, out Collate, out NombreImpresora, out FormatoGuia, out categoriaNegocio_);
                                clsLogger.Graba_Log_Info("Parametros Salida ");
                                clsLogger.Graba_Log_Info("TipoHoja " + TipoHoja + " Copias " + Copias.ToString() +
                                                            " Collate " + Collate.ToString() + " NombreImpresora " + NombreImpresora +
                                                            " FormatoGuia " + FormatoGuia + " categoriaNegocio  " + categoriaNegocio_);

                                string configCN = ConfigurationManager.AppSettings.Get("categoria_negocio");
                                string configCNT = ConfigurationManager.AppSettings.Get("categoria_negocioT");

                                clsLogger.Graba_Log_Info("Inicio Imprime guia " + idComprobante);

                                if (configCN.Contains(categoriaNegocio) && !String.IsNullOrEmpty(categoriaNegocio))
                                {
                                    using (ReportDocument rptGR = new GuiaRemisionConcreto())
                                    {
                                        rptGR.SetDataSource(dsPc);
                                        //ruta = (MemoryStream)rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                        try
                                        {
                                            ruta = (MemoryStream)rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                        }
                                        catch (Exception)
                                        {
                                            rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat).CopyTo(ruta = new MemoryStream());
                                        }
                                        rptGR.PrintOptions.PaperSize = PaperSize.PaperA4;
                                        System.Drawing.Printing.PrinterSettings printersettings = new System.Drawing.Printing.PrinterSettings();

                                        if (ConsultaDirEstablecimientoGuia(idComprobante))
                                        {
                                            clsLogger.Graba_Log_Info("ConsultaDirEstablecimientoGuia " + idComprobante);
                                            printersettings.PrinterName = obtener_codigo("Impresora2");
                                        }
                                        printersettings.PrinterName = NombreImpresora;
                                        printersettings.Copies = (short)Copias;
                                        printersettings.Collate = Collate;
                                        rptGR.PrintToPrinter(printersettings, new System.Drawing.Printing.PageSettings(), false);
                                        cambioEstado("1", idComprobante);
                                        rptGR.Dispose();
                                        rptGR.Close();
                                    }
                                }
                                else if (configCNT.Contains(categoriaNegocio) && !String.IsNullOrEmpty(configCNT))
                                {
                                    using (ReportDocument rptGR = new GuiaRemisionTraslado())
                                    {
                                        rptGR.SetDataSource(dsPc);                                        
                                        try
                                        {
                                            ruta = (MemoryStream)rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                        }
                                        catch (Exception)
                                        {
                                            rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat).CopyTo(ruta = new MemoryStream());
                                        }
                                        rptGR.PrintOptions.PaperSize = PaperSize.PaperA4;
                                        System.Drawing.Printing.PrinterSettings printersettings = new System.Drawing.Printing.PrinterSettings();

                                        if (ConsultaDirEstablecimientoGuia(idComprobante))
                                        {
                                            clsLogger.Graba_Log_Info("ConsultaDirEstablecimientoGuia " + idComprobante);
                                            printersettings.PrinterName = obtener_codigo("Impresora2");
                                        }
                                        printersettings.PrinterName = NombreImpresora;
                                        printersettings.Copies = (short)Copias;
                                        printersettings.Collate = Collate;
                                        rptGR.PrintToPrinter(printersettings, new System.Drawing.Printing.PageSettings(), false);
                                        cambioEstado("1", idComprobante);
                                        rptGR.Dispose();
                                        rptGR.Close();
                                    }
                                }
                                else
                                {
                                    if (FormatoGuia.Equals("Concreto"))
                                    {
                                        using (ReportDocument rptGR = new GuiaRemisionConcreto())
                                        {
                                            rptGR.SetDataSource(dsPc);
                                            try
                                            {
                                                ruta = (MemoryStream)rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                            }
                                            catch (Exception)
                                            {
                                                rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat).CopyTo(ruta = new MemoryStream());
                                            }

                                            rptGR.PrintOptions.PaperSize = PaperSize.PaperA4;
                                            System.Drawing.Printing.PrinterSettings printersettings = new System.Drawing.Printing.PrinterSettings();

                                            if (ConsultaDirEstablecimientoGuia(idComprobante))
                                            {
                                                clsLogger.Graba_Log_Info("ConsultaDirEstablecimientoGuia " + idComprobante);
                                                printersettings.PrinterName = obtener_codigo("Impresora2");
                                            }
                                            printersettings.PrinterName = NombreImpresora;
                                            printersettings.Copies = (short)Copias;
                                            printersettings.Collate = Collate;
                                            rptGR.PrintToPrinter(printersettings, new System.Drawing.Printing.PageSettings(), false);
                                            cambioEstado("1", idComprobante);
                                            rptGR.Dispose();
                                            rptGR.Close();
                                        }
                                    }
                                    else if (FormatoGuia.Equals("Traslado"))
                                    {
                                        using (ReportDocument rptGR = new GuiaRemisionTraslado())
                                        {
                                            rptGR.SetDataSource(dsPc);
                                            try
                                            {
                                                ruta = (MemoryStream)rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                            }
                                            catch (Exception)
                                            {
                                                rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat).CopyTo(ruta = new MemoryStream());
                                            }
                                            rptGR.PrintOptions.PaperSize = PaperSize.PaperA4;
                                            System.Drawing.Printing.PrinterSettings printersettings = new System.Drawing.Printing.PrinterSettings();

                                            if (ConsultaDirEstablecimientoGuia(idComprobante))
                                            {
                                                clsLogger.Graba_Log_Info("ConsultaDirEstablecimientoGuia " + idComprobante);
                                                printersettings.PrinterName = obtener_codigo("Impresora2");
                                            }
                                            printersettings.PrinterName = NombreImpresora;
                                            printersettings.Copies = (short)Copias;
                                            printersettings.Collate = Collate;
                                            rptGR.PrintToPrinter(printersettings, new System.Drawing.Printing.PageSettings(), false);
                                            cambioEstado("1", idComprobante);
                                            rptGR.Dispose();
                                            rptGR.Close();
                                        }
                                    }
                                    else
                                    {
                                        using (ReportDocument rptGR = new GuiaRemision())
                                        {
                                            rptGR.SetDataSource(dsPc);
                                            try
                                            {
                                                ruta = (MemoryStream)rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                            }
                                            catch (Exception)
                                            {
                                                rptGR.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat).CopyTo(ruta = new MemoryStream());
                                            }
                                            rptGR.PrintOptions.PaperSize = PaperSize.PaperA4;
                                            System.Drawing.Printing.PrinterSettings printersettings = new System.Drawing.Printing.PrinterSettings();

                                            if (ConsultaDirEstablecimientoGuia(idComprobante))
                                            {
                                                clsLogger.Graba_Log_Info("ConsultaDirEstablecimientoGuia " + idComprobante);
                                                printersettings.PrinterName = obtener_codigo("Impresora2");
                                            }
                                            printersettings.PrinterName = NombreImpresora;
                                            printersettings.Copies = (short)Copias;
                                            printersettings.Collate = Collate;
                                            rptGR.PrintToPrinter(printersettings, new System.Drawing.Printing.PageSettings(), false);
                                            cambioEstado("1", idComprobante);
                                            rptGR.Dispose();
                                            rptGR.Close();
                                        }
                                    }

                                }

                                clsLogger.Graba_Log_Info("FIN Imprime guia " + idComprobante);
                            }
                            catch (Exception ex)
                            {
                                cambioEstado("3", idComprobante);
                                clsLogger.Graba_Log_Error("error imprimir guia " + ex.ToString());
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                msj = "";
                msjT = ex.ToString();
                clsLogger.Graba_Log_Error(ex.Message);
                msjT = "";
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                    dsPc.Dispose();
                }
            }
        }

        private void sucursal(string idComprobante)
        {
            DB.Conectar();
            DB.CrearComando(@"select estab,ptoEmi from general WITH (NOLOCK) where idComprobante = @idComprobante");
            DB.AsignarParametroCadena("@idComprobante", idComprobante);
            DbDataReader DR6 = DB.EjecutarConsulta();
            if (DR6.Read())
            {
                estab2 = DR6["estab"].ToString();
                ptoemi2 = DR6["ptoEmi"].ToString();

            }
            DB.Desconectar();
        }
        private string obtener_codigo(string a_parametro)
        {
            string retorna = ConfigurationManager.AppSettings.Get(a_parametro);

            return retorna;
        }
        private void cambioEstado(String estado, String idComprobante)
        {
            DBPI = new BasesDatos();
            DataSet listPendientes = new DataSet();
            XDocument CRE = new XDocument(
            new XElement("INSTRUCCION",
                new XElement("FILTROS",
                    new XElement("OPCION", "2"),
                    new XElement("idComprobante", idComprobante),
                    new XElement("estadoImpresion", estado))));
            DBPI.Conectar();
            listPendientes = DBPI.TraerDataset("sp_pendientesImpresion", CRE.ToString());
            DBPI.Desconectar();
        }
        private Boolean ConsultaDirEstablecimientoGuia(string pr_id_Comprobante)
        {
            bool respuesta = false;
            string rpt = "";
            DB.Conectar();
            DB.CrearComando("select top 1 dirEstablecimientoGuia from general with(nolock) where idComprobante =@id_Comprobante");
            DB.AsignarParametroCadena("@id_Comprobante", pr_id_Comprobante);
            DbDataReader DR3 = DB.EjecutarConsulta();
            if (DR3.Read())
            {
                rpt = DR3[0].ToString();
            }
            DB.Desconectar();

            if (!String.IsNullOrEmpty(rpt))
            {
                respuesta = rpt.Contains("(M)");
            }


            return respuesta;
        }        
        private void consultaDatosImpresion(string Establecimiento, string PtoEmision, string CategoriaNegocio,
            out string TipoHoja, out int Copias, out bool Collate, out string NombreImpresora, out string FormatoGuia, out string categoriaNegocio_)
        {

            TipoHoja = ""; Copias = 0; Collate = false; NombreImpresora = ""; FormatoGuia = ""; categoriaNegocio_ = "";
            try
            {
                DB.Conectar();
                DB.CrearComandoProcedimiento("SP_Consulta_ConfigImpresoras");
                DB.AsignarParametroProcedimiento("@Establecimiento", System.Data.DbType.String, Establecimiento);
                DB.AsignarParametroProcedimiento("@PtoEmision", System.Data.DbType.String, PtoEmision);
                DB.AsignarParametroProcedimiento("@CategoriaNegocio", System.Data.DbType.String, CategoriaNegocio);
                DbDataReader dr = DB.EjecutarConsulta();
                if (dr.Read())
                {
                    TipoHoja = dr["TipoHoja"].ToString();
                    Copias = Convert.ToInt32(dr["Copias"].ToString());
                    Collate = Convert.ToBoolean(dr["Collate_"].ToString());
                    NombreImpresora = dr["NombreImpresora"].ToString();
                    FormatoGuia = dr["FormatoGuia"].ToString();
                    categoriaNegocio_ = dr["categoriaNegocio"].ToString();
                }
                DB.Desconectar();
            }
            catch (Exception ex)
            {
                //log.guardar_Log("Error ConsultaDatosImpresion " + ex.ToString());
                DB.Desconectar();
            }

        }
    }
}
