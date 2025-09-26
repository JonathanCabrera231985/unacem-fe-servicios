using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using clibLogger;

namespace InvoicecCorreos
{
    public partial class InvoicecCorreos : ServiceBase
    {
        private System.Threading.Thread ThreadFacturaElectronica;
        System.Timers.Timer _timerProcesoNotificacion = new System.Timers.Timer();
        //private Logs.Log log = new Logs.Log();
        private EnviarCorreos Enviar;
        
        private string men;

        public InvoicecCorreos()
        {
            InitializeComponent();
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {

            this.GenerarProcesoThread();
        }

        protected override void OnStop()
        {
            this.Detener_Hilo_Invoicec();
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
                }
            }
            catch (System.Exception ex)
            {
                this.men = "Error en ejecución del Hilo tContado Abort: " + System.DateTime.Now.ToString() + ":" + ex.Message;
                // logErrores.mensajesLog("ES003", this.men, ex.Message, "", ThreadFacturaElectronica.ThreadState.ToString(), "clase de error Invoice.cs");
            }
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
                
            }
            catch (System.Exception ex)
            {
                //log.guardar_Log("Error GenerarProcesoThread " + ex.ToString());
                //logErrores.mensajesLog("ES003", "", ex.Message, "", "Problema con el proceso Thread  ", "clase de error cs");
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
            _timerProcesoNotificacion.Elapsed += new ElapsedEventHandler(_timerProceso_Elapsed);
            _timerProcesoNotificacion.Start();


        }

        private void _timerProceso_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CultureInfo forceDotCulture = new System.Globalization.CultureInfo("es-MX");
            forceDotCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            forceDotCulture.NumberFormat.CurrencySymbol = "$";
            forceDotCulture.NumberFormat.NumberDecimalSeparator = ".";
            forceDotCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = forceDotCulture;

            _timerProcesoNotificacion.Stop();
            // dormir el thread porun tiempo 
            _timerProcesoNotificacion.Interval = Convert.ToInt32(3000);

            try
            {
                Enviar = new EnviarCorreos();
                Enviar.inicio();
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error("Error  _timerProceso_Elapsed " + ex.ToString());
            }
            _timerProcesoNotificacion.Start();
        }

    }
}
