using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Datos;
using System.Configuration;
using ReportesDEI;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;

namespace Control
{
    public class CrearPDF1
    {
        string msj = "";
        string msjT = "";
        Log log;
        public void PoblarReporte(string ruta, string nombre, string idComprobante, string comprobante)
        {

            log = new Log();
            SqlConnection sqlConn;
            SqlDataAdapter sqlDaComprobante, sqlDaDetalles, sqlDaInfoAdicional, sqlDaImpuestosRetenciones;
            SqlDataAdapter sqlDaDetalleDest, sqlDaDestinatarios;
            SqlDataReader sqlDrDetalles;
            CrystalDecisions.CrystalReports.Engine.ReportObjects crReportObjects;
            CrystalDecisions.CrystalReports.Engine.SubreportObject crSubreportObject;
            CrystalDecisions.CrystalReports.Engine.ReportDocument crSubreportDocument;
            CrystalDecisions.CrystalReports.Engine.Database crDatabase;
            CrystalDecisions.CrystalReports.Engine.Tables crTables;

            DataSet1 dsPc = new DataSet1();
            String strConn;
            strConn = ConfigurationManager.ConnectionStrings["dataexpressConnectionString"].ConnectionString;
            ConnectionInfo connectionInfo = new ConnectionInfo();
            connectionInfo.ServerName = ConfigurationManager.AppSettings.Get("SERVER");
            connectionInfo.DatabaseName = ConfigurationManager.AppSettings.Get("DATABASE");
            connectionInfo.UserID = ConfigurationManager.AppSettings.Get("USER");
            connectionInfo.Password = ConfigurationManager.AppSettings.Get("PASSWORD");
            //   strConn = @"Data Source=CIMA02\SQLEXPRESS;Initial Catalog=DataCimaIT;User ID=sa;Password=data_cima; Persist Security info=false;";

            String StrComprobante = @"SELECT        GENERAL.idComprobante, GENERAL.id, GENERAL.version, GENERAL.serie, GENERAL.folio, GENERAL.fecha, GENERAL.sello, GENERAL.noCertificado, 
                         GENERAL.subTotal, GENERAL.total, GENERAL.tipoDeComprobante, GENERAL.firmaSRI, GENERAL.id_Config, GENERAL.id_Empleado, GENERAL.id_Receptor, 
                         GENERAL.id_Emisor, GENERAL.id_EmisorExp, GENERAL.id_ReceptorCon,
                             (SELECT        descripcion
                               FROM            Catalogo1_C
                               WHERE        (codigo = GENERAL.ambiente) AND (tipo = 'Ambiente')) AS ambienteDesc,
                             (SELECT        descripcion
                               FROM            Catalogo1_C AS Catalogo1_C_2
                               WHERE        (codigo = GENERAL.tipoEmision) AND (tipo = 'Emision')) AS tipoEmision, GENERAL.claveAcceso,
                             (SELECT        descripcion
                               FROM            Catalogo1_C AS Catalogo1_C_1
                               WHERE        (codigo = GENERAL.codDoc) AND (tipo = 'Comprobante')) AS codDocDesc, GENERAL.estab, GENERAL.ptoEmi, GENERAL.secuencial, 
                         GENERAL.totalSinImpuestos, GENERAL.totalDescuento, GENERAL.periodoFiscal, GENERAL.fechaIniTransporte, GENERAL.fechaFinTransporte, GENERAL.placa,
                             ISNULL((SELECT        descripcion AS Expr1
                               FROM            Catalogo1_C AS Catalogo1_C_1
                               WHERE        (codigo = GENERAL.codDocModificado) AND (tipo = 'Comprobante')),'') AS codDocModificadoDesc,GENERAL.codDocModificado, GENERAL.numDocModificado, 
                         GENERAL.fechaEmisionDocSustento, GENERAL.valorModificacion, GENERAL.moneda, GENERAL.propina, GENERAL.importeTotal, GENERAL.motivo, 
                         GENERAL.subtotal12, GENERAL.subtotal0, GENERAL.subtotalNoSujeto, GENERAL.ICE, GENERAL.IVA12, GENERAL.importeAPagar, EMISOR.IDEEMI, 
                         EMISOR.RFCEMI, EMISOR.NOMEMI, EMISOR.nombreComercial, EMISOR.dirMatriz, RECEPTOR.IDEREC, RECEPTOR.RFCREC, RECEPTOR.NOMREC, 
                         RECEPTOR.contribuyenteEspecial, RECEPTOR.obligadoContabilidad, RECEPTOR.tipoIdentificacionComprador, DOMEMIEXP.IDEDOMEMIEXP, 
                         DOMEMIEXP.dirEstablecimientos, GENERAL.codigoBarras, GENERAL.numeroAutorizacion, GENERAL.estado, GENERAL.fechaAutorizacion,
                        GENERAL.rise, GENERAL.dirPartida
FROM            GENERAL INNER JOIN
                         EMISOR ON GENERAL.id_Emisor = EMISOR.IDEEMI INNER JOIN
                         RECEPTOR ON GENERAL.id_Receptor = RECEPTOR.IDEREC INNER JOIN
                         DOMEMIEXP ON GENERAL.id_EmisorExp = DOMEMIEXP.IDEDOMEMIEXP
                                  WHERE 
                         General.idComprobante='" + idComprobante + "'";
            String StrInfoAdicional = @"SELECT * FROM InfoAdicional ";

            String StrDetalles = @"SELECT        idDetalles, codigoPrincipal, codigoAuxiliar, descripcion + CHAR(10) + CHAR(13) + ISNULL
                             ((SELECT         ISNULL(nombre, '') + ': ' + ISNULL(valor, '') AS Descripcion
                                 FROM            DetallesAdicionales
                                 WHERE        (id_Detalles = Detalles.idDetalles)), '') AS Descripcion, cantidad, precioUnitario, descuento, precioTotalSinImpuestos, id_Comprobante
FROM            Detalles";

            String StrImpuestosRetenciones = @"SELECT tci.codigo,ISNULL((SELECT descripcion FROM CatImpuestos_C where tipo = 'Retencion' AND codigo =tci.codigo ),'') as descripcionImpuesto,
                                                tci.codigoPorcentaje, tci.baseImponible, tci.tarifa, tci.valor,tci.porcentajeRetener, 
                                                ISNULL((SELECT descripcion FROM Catalogo1_C where tipo = 'Comprobante' AND codigo =tci.codDocSustento ),'') as descripcionComprobante,
                                                tci.numDocSustento,tci.fechaEmisionDocSustento, tci.id_Comprobante
                                                FROM TotalConImpuestos AS tci";

            String StrDetallesDest = @"SELECT idDetalles, codigoPrincipal, codigoAuxiliar, descripcion, cantidad,
                                              precioUnitario, descuento, precioTotalSinImpuestos, id_Comprobante, id_Destinatario
                                       FROM            Detalles";
            String StrDestinatarios = @"SELECT        idDestinatario, identificacionDestinatario, razonSocialDestinatario, dirDestinatario, motivoTraslado, docAduaneroUnico, codEstabDestino, ruta, ISNULL
                             ((SELECT        descripcion
                                 FROM            Catalogo1_C AS Catalogo1_C_1
                                 WHERE        (codigo = Destinatarios.codDocSustento) AND (tipo = 'Comprobante')), '') AS codDocDesc, numDocSustento, numAutDocSustento, 
                         fechaEmisionDocSustento, id_Comprobante
FROM            Destinatarios  ORDER BY razonSocialDestinatario DESC";
            DirectoryInfo DIR = new DirectoryInfo(ruta);

            if (!DIR.Exists)
            {
                DIR.Create();
            }
            try
            {
                sqlConn = new SqlConnection(strConn);
                /*    sqlDaDetalles = new SqlDataAdapter("PA_Det_Facturas", sqlConn);
                    sqlDaDetalles.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sqlDaDetalles.SelectCommand.Parameters.Add("@id_Comprobante", SqlDbType.VarChar).Value = Convert.ToInt32(idComprobante);
                    sqlConn.Open();
                    sqlDrDetalles = sqlDaDetalles.SelectCommand.ExecuteReader();
                    sqlConn.Close();
                
                    DataSet1 miDataSet1 = new DataSet1();
                    // este es tu dataset tipado
                    sqlDaDetalles.Fill(dsPc, "PA_Det_Facturas");*/
                // aqui pones el nombre del datatable
                //Crear los DataAdapters
                //Comprobante
                sqlDaComprobante = new SqlDataAdapter(StrComprobante, sqlConn);
                //Detalles
                sqlDaDetalles = new SqlDataAdapter(StrDetalles, sqlConn);
                //info Adicional
                sqlDaInfoAdicional = new SqlDataAdapter(StrInfoAdicional, sqlConn);
                //Impuestos Retenciones
                sqlDaImpuestosRetenciones = new SqlDataAdapter(StrImpuestosRetenciones, sqlConn);
                //Destinatarios
                sqlDaDestinatarios = new SqlDataAdapter(StrDestinatarios, sqlConn);
                //DetallesDestinatarios
                sqlDaDetalleDest = new SqlDataAdapter(StrDetallesDest, sqlConn);
                //Poblar las tablas del dataset desde los dataAdaperts
                dsPc.EnforceConstraints = false;
                sqlDaComprobante.Fill(dsPc, "Comprobante");
                sqlDaDetalles.Fill(dsPc, "DetallesConDetalleAdicionales");
                sqlDaInfoAdicional.Fill(dsPc, "InfoAdicional");
                sqlDaImpuestosRetenciones.Fill(dsPc, "TotalConImpuestos");
                sqlDaDestinatarios.Fill(dsPc, "Destinatarios");
                sqlDaDetalleDest.Fill(dsPc, "Detalles");
                //Poblar el informe con el dataSet y mostrarlo
                if (comprobante == "01")
                {
                    // strConn = @"Data Source=CIMA02\SQLEXPRESS;Initial Catalog=DataCimaIT;User ID=sa;Password=data_cima; Persist Security info=false;";
                    Factura rpt = new Factura();
                    Tables tables = rpt.Database.Tables;

                    foreach (CrystalDecisions.CrystalReports.Engine.Section crSection in rpt.ReportDefinition.Sections)
                    {
                        //loop through all the report objects to find all the subreports
                        foreach (CrystalDecisions.CrystalReports.Engine.ReportObject crReportObject in crSection.ReportObjects)
                        {
                            if (crReportObject.Kind == ReportObjectKind.SubreportObject)
                            {
                                //you will need to typecast the reportobject to a subreport 
                                //object once you find it
                                crSubreportObject = (CrystalDecisions.CrystalReports.Engine.SubreportObject)crReportObject;

                                //open the subreport object
                                crSubreportDocument = crSubreportObject.OpenSubreport(crSubreportObject.SubreportName);

                                //set the database and tables objects to work with the subreport
                                crDatabase = crSubreportDocument.Database;
                                crTables = crDatabase.Tables;

                                //loop through all the tables in the subreport and 
                                //set up the connection info and apply it to the tables
                                foreach (CrystalDecisions.CrystalReports.Engine.Table crTable in crTables)
                                {
                                    TableLogOnInfo crTableLogOnInfo = crTable.LogOnInfo; ;
                                    crTableLogOnInfo.ConnectionInfo = connectionInfo;
                                    crTable.ApplyLogOnInfo(crTableLogOnInfo);
                                }
                            }
                        }
                    }
                   /* foreach (CrystalDecisions.CrystalReports.Engine.Table table in tables)
                    {
                        TableLogOnInfo tableLogonInfo = table.LogOnInfo;
                        tableLogonInfo.ConnectionInfo = connectionInfo;
                        table.ApplyLogOnInfo(tableLogonInfo);
                    } */
                    //rpt.DataSourceConnections[0].SetConnection(@"SAMF41\SQLEXPRESS", "dataEcuador", "sa", "123456");
                    rpt.SetDataSource(dsPc);
                    //crystalReportViewer1.ReportSource = rpt; 
                    MessageBox.Show("setadata");
                    rpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, ruta + nombre + ".pdf");
                    MessageBox.Show("exportar");
                    //rpt.PrintOptions.PrinterName = @"\\Factura-graciel\HP LaserJet P1005 (Copiar 1)";
                    //rpt.PrintToPrinter(2, false, 0, 0);
                    rpt.Close();
                }
                if (comprobante.Equals("04"))
                {
                    NC rpt = new NC();
                    rpt.SetDataSource(dsPc);
                    //crystalReportViewer1.ReportSource = rpt; 
                    rpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, ruta + nombre + ".pdf");
                    //rpt.PrintOptions.PrinterName = @"\\Factura-graciel\HP LaserJet P1005 (Copiar 1)";
                    //rpt.PrintToPrinter(2, false, 0, 0);
                    rpt.Close();
                }
                if (comprobante.Equals("05"))
                {
                    ND rpt = new ND();
                    //Factura rpt = new Factura();
                    rpt.SetDataSource(dsPc);
                    //crystalReportViewer1.ReportSource = rpt; 
                    rpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, ruta + nombre + ".pdf");
                    //rpt.PrintOptions.PrinterName = @"\\Factura-graciel\HP LaserJet P1005 (Copiar 1)";
                    //rpt.PrintToPrinter(2, false, 0, 0);
                    rpt.Close();
                }
                if (comprobante.Equals("06"))
                {
                    GuiaRemision rpt = new GuiaRemision();
                    rpt.SetDataSource(dsPc);
                    //crystalReportViewer1.ReportSource = rpt; 
                    rpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, ruta + nombre + ".pdf");
                    //rpt.PrintOptions.PrinterName = @"\\Factura-graciel\HP LaserJet P1005 (Copiar 1)";
                    //rpt.PrintToPrinter(2, false, 0, 0);
                    rpt.Close();
                }
                if (comprobante.Equals("07"))
                {
                    CRetencion rpt = new CRetencion();
                    rpt.SetDataSource(dsPc);
                    //crystalReportViewer1.ReportSource = rpt; 
                    rpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, ruta + nombre + ".pdf");
                    //rpt.PrintOptions.PrinterName = @"\\Factura-graciel\HP LaserJet P1005 (Copiar 1)";
                    //rpt.PrintToPrinter(2, false, 0, 0);
                    rpt.Close();
                }
            }
            catch (Exception ex)
            {
                msj = "";
                msjT = ex.Message;
                log.mensajesLog("EM009", "", msjT, "", nombre, "");
            }
        }

    }
}


