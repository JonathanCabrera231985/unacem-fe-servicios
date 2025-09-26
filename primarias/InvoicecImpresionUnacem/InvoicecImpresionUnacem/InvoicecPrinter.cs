using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Xml.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Datos;
using System.IO;
using clibLogger;

namespace InvoicecImpresionUnacem
{
    public partial class InvoicecPrinter : ServiceBase
    {
        private static System.Threading.Thread ThreadImpresion;
        private static bool monitoreando = false;
        private static BasesDatos DB = new BasesDatos();
        private static BasesDatos DBPI = new BasesDatos();
        private static BasesDatos DB2 = new BasesDatos();

        public InvoicecPrinter()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.GenerarProcesoThread();
        }

        protected override void OnStop()
        {
            clsLogger.Graba_Log_Info("DETENER HILOS");
            ThreadImpresion.Abort();
            ThreadImpresion = null;
        }

        public void GenerarProcesoThread()
        {
            try
            {
                clsLogger.Graba_Log_Info("ACTIVAR HILOS");
                if (ThreadImpresion == null)
                {
                    ThreadImpresion = new System.Threading.Thread(new System.Threading.ThreadStart(procesoHilos));
                    ThreadImpresion.CurrentCulture = new System.Globalization.CultureInfo("es-MX");
                    ThreadImpresion.Name = "tProcesoImpresion";
                    ThreadImpresion.Priority = System.Threading.ThreadPriority.Highest;
                    ThreadImpresion.Start();
                }
            }
            catch (System.Exception ex)
            {
                clsLogger.Graba_Log_Error("ES003 Problema con el proceso Thread. Clase de error Invoicec.cs " + ex.Message);
            }
        }

        public void procesoHilos()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (!monitoreando)
                        {
                            monitoreando = true;
                            clsLogger.Graba_Log_Info(" procesoHilos:");
                            MemoryStream mrpt = new MemoryStream();
                            DataSet listPendientes = new DataSet();
                            XDocument CRE = new XDocument(
                            new XElement("INSTRUCCION",
                                new XElement("FILTROS",
                                    new XElement("OPCION", "1"))));
                            DBPI.Conectar();                            
                            listPendientes = DBPI.TraerDataset("sp_pendientesImpresion", CRE.ToString());
                            DBPI.Desconectar();
                            String p_codigoControl = "", idComprobante = "", codDoc = "", categoriaNegocio = "";
                            if (listPendientes.Tables.Count > 0)
                            {
                                foreach (DataRow dr in listPendientes.Tables[0].Rows)
                                {
                                    CrearPDF cdpsf = new CrearPDF();
                                    p_codigoControl = dr["codigoControl"].ToString();
                                    idComprobante = dr["idComprobante"].ToString();
                                    codDoc = dr["codDoc"].ToString();
                                    categoriaNegocio = dr["categoriaNegocio"].ToString();
                                    cdpsf.PoblarReporte(out mrpt, p_codigoControl, idComprobante, codDoc, categoriaNegocio);
                                    if(mrpt == null)
                                    {
                                        clsLogger.Graba_Log_Info("Documento no imprimible. Idcomprobante: " + idComprobante);
                                        cambioEstado("2", idComprobante);
                                    }
                                }
                            }                           

                            monitoreando = false;
                            System.Threading.Thread.Sleep(10000); //10 segundos
                        }
                    }
                    catch (System.Exception ex)
                    {
                        DB.Desconectar();
                        clsLogger.Graba_Log_Error("error procesoHilos:" + ex.ToString());
                    }
                    finally
                    {
                        monitoreando = false;
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                monitoreando = false;
                DB2.Desconectar();
                ThreadImpresion.Abort();
                ThreadImpresion = null;
                GenerarProcesoThread();
            }
            monitoreando = false;

        }

        private void cambioEstado(String estado, String idComprobante)
        {
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
    }
}
