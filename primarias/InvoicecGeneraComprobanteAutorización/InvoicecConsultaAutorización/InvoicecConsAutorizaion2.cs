using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Datos;
using System.Globalization;
using System.Timers;


namespace InvoicecConsultaAutorización
{
    public partial class InvoicecConsAutorizaion2 : ServiceBase
    {

        private System.Threading.Thread ThreadFacturaElectronica;
        System.Timers.Timer _timerProceso = new System.Timers.Timer();
        System.Timers.Timer _timerEnvia = new System.Timers.Timer();
        private Logs.Log logErrores = new Logs.Log();
        private Logs.Log log = new Logs.Log();
        private consultaAutorizacion.Proceso proceso;
        private GeneraAutorizacion.Proceso proceso2;

        private string men;

        public InvoicecConsAutorizaion2()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.GenerarProcesoThread();
        }

        protected override void OnStop()
        {
            this.Detener_Hilo_Invoicec();
        }

        public void GenerarProcesoThread()
        {
            try
            {
                if (ThreadFacturaElectronica == null)
                {
                    CultureInfo forceDotCulture = new System.Globalization.CultureInfo("es-MX");
                    //forceDotCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                    forceDotCulture.NumberFormat.CurrencySymbol = "$";
                    forceDotCulture.NumberFormat.NumberDecimalSeparator = ".";
                    forceDotCulture.NumberFormat.CurrencyDecimalSeparator = ".";
                    System.Threading.Thread.CurrentThread.CurrentCulture = forceDotCulture;

                    ThreadFacturaElectronica = new System.Threading.Thread(new System.Threading.ThreadStart(this.procesoHilos));
                    ThreadFacturaElectronica.CurrentCulture = forceDotCulture;//new System.Globalization.CultureInfo("es-MX");
                    ThreadFacturaElectronica.Name = "tProcesoFacturaElectronica2";
                    ThreadFacturaElectronica.Priority = System.Threading.ThreadPriority.Highest;
                    ThreadFacturaElectronica.Start();
                }
                else
                {
                    //logErrores.mensajesLog(string.Concat(new object[]
                    //{
                    //    "No se ha iniciado el servicio Invoicec: El proceso ",
                    //    ThreadFacturaElectronica.Name,
                    //    ": Se encuentra ",
                    //    ThreadFacturaElectronica.ThreadState
                    //}), "", "Proceso Thread", ThreadFacturaElectronica.Name, "Metodo GenerarProcesoThread");
                }
            }
            catch (System.Exception ex)
            {
                //logErrores.mensajesLog("ES003", "", ex.Message, "", "Problema con el proceso Thread  ", "clase de error cs");
            }
        }

        public void Detener_Hilo_Invoicec()
        {
            try
            {
                if (ThreadFacturaElectronica != null)
                {
                    ThreadFacturaElectronica.Abort();
                    this.men = string.Concat(new object[]
					{
						"Se detuvo el servicio InvoicecContado: El proceso ",
						ThreadFacturaElectronica.Name,
						": Se encuentra ",
						ThreadFacturaElectronica.ThreadState
					});
                    //logErrores.mensajesLog("ES003", this.men, "", "", ThreadFacturaElectronica.ThreadState.ToString(), "clase de error Invoice.cs");
                    ThreadFacturaElectronica = null;
                }
                else
                {
                    this.men = string.Concat(new object[]
					{
						"No se puede detener el servicio InvoicecContado: El proceso ",
						ThreadFacturaElectronica.Name,
						": Se encuentra ",
						ThreadFacturaElectronica.ThreadState
					});
                   // logErrores.mensajesLog("ES003", this.men, "", "", ThreadFacturaElectronica.ThreadState.ToString(), "clase de error Invoice.cs");
                }
            }
            catch (System.Exception ex)
            {
                this.men = "Error en ejecución del Hilo tContado Abort: " + System.DateTime.Now.ToString() + ":" + ex.Message;
               // logErrores.mensajesLog("ES003", this.men, ex.Message, "", ThreadFacturaElectronica.ThreadState.ToString(), "clase de error Invoice.cs");
            }
        }




        private void procesoHilos()
        {
            CultureInfo forceDotCulture = new System.Globalization.CultureInfo("es-MX");
            forceDotCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            forceDotCulture.NumberFormat.CurrencySymbol = "$";
            forceDotCulture.NumberFormat.NumberDecimalSeparator = ".";
            forceDotCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = forceDotCulture;
            _timerProceso.Elapsed += new ElapsedEventHandler(_timerProceso_Elapsed);
            _timerProceso.Start();

            _timerEnvia.Elapsed += new ElapsedEventHandler(_timerProceso_Elapsed2);
            _timerEnvia.Start();

        }

        private void _timerProceso_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CultureInfo forceDotCulture = new System.Globalization.CultureInfo("es-MX");
            forceDotCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            forceDotCulture.NumberFormat.CurrencySymbol = "$";
            forceDotCulture.NumberFormat.NumberDecimalSeparator = ".";
            forceDotCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = forceDotCulture;

            _timerProceso.Stop();
            // dormir el thread porun tiempo 
            _timerProceso.Interval = Convert.ToInt32(3000);
            
            try
            {
                proceso = new consultaAutorizacion.Proceso();
                proceso.inicio();
            }
            catch (Exception ex)
            {
                log.guardar_Log(" _timerProceso_Elapsed " + ex.ToString());
                //logErrores.mensajesLog("ES003", "", ex.Message, "", "Problema con el proceso Thread en el metodo  procesoHilos()", "clase de error Invoice.cs");
            }
            _timerProceso.Start();
        }

        private void _timerProceso_Elapsed2(object sender, System.Timers.ElapsedEventArgs e)
        {
            CultureInfo forceDotCulture = new System.Globalization.CultureInfo("es-MX");
            forceDotCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            forceDotCulture.NumberFormat.CurrencySymbol = "$";
            forceDotCulture.NumberFormat.NumberDecimalSeparator = ".";
            forceDotCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = forceDotCulture;

            _timerEnvia.Stop();
            // dormir el thread porun tiempo 
            _timerEnvia.Interval = Convert.ToInt32(3000);

            try
            {
                proceso2 = new GeneraAutorizacion.Proceso();
                proceso2.inicio();
            }
            catch (Exception ex)
            {
                log.guardar_Log(" _timerProceso_Elapsed " + ex.ToString());
                //logErrores.mensajesLog("ES003", "", ex.Message, "", "Problema con el proceso Thread en el metodo  procesoHilos()", "clase de error Invoice.cs");
            }
            _timerEnvia.Start();
        }


    }
}
