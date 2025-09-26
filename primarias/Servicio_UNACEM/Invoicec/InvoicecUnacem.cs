using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Datos;
using Control;
using System.Data.Common;
using System.Collections;
using System.Threading;
using System.IO;
using AE.Net.Mail;
using Ionic.Zip;
using System.Xml;
using System.Threading.Tasks;
using System.Xml.Linq;
using clibLogger;
namespace Invoicec
{
    partial class InvoicecUnacem : ServiceBase
    {
        private static Leer LecturaTxT;
        private static ConsultaOff off;
        private static string txt = "";
        private static System.Threading.Thread ThreadFacturaElectronica;
        private static string pdf = "";
        private static string bck = "";
        private static Log logErrores = new Log();
        private static int contador = 0;
        private string men;
        private static string servidor_correo;
        private static string user_recepcion;
        private static string clave_correo_recep;
        private static LeerReenvioSRI leer_reproceso;
        private static Recepcion rece_doc;
        private static bool monitoreando = false;
        public InvoicecUnacem()
        {
            this.InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            this.GenerarProcesoThread();
        }
        protected override void OnStop()
        {
            clsLogger.Graba_Log_Info("DETENER HILOS");
            ThreadFacturaElectronica.Abort();
            ThreadFacturaElectronica = null;
        }
        public void GenerarProcesoThread()
        {
            try
            {
                clsLogger.Graba_Log_Info("ACTIVAR HILOS");
                conectarParametrosdelSistema();
                if (ThreadFacturaElectronica == null)
                {
                    ThreadFacturaElectronica = new System.Threading.Thread(new System.Threading.ThreadStart(procesoHilos));
                    ThreadFacturaElectronica.CurrentCulture = new System.Globalization.CultureInfo("es-MX");
                    ThreadFacturaElectronica.Name = "tProcesoFacturaElectronica";
                    ThreadFacturaElectronica.Priority = System.Threading.ThreadPriority.Highest;
                    ThreadFacturaElectronica.Start();
                }
            }
            catch (System.Exception ex)
            {
                logErrores.mensajesLog("ES003", "", ex.Message, "", "Problema con el proceso Thread  ", "clase de error Invoicec.cs");
            }
        }

        private void conectarParametrosdelSistema()
        {
            BasesDatos DB = new BasesDatos();

            try
            {
                rece_doc = new Recepcion();
                DB.Desconectar();
                DB.Conectar();
                DB.CrearComando(@"select dirtxt,dirdocs,dirrespaldo,servidorSMTP,correoRecepcion,passRecepcion,dirRecepcion from ParametrosSistema with(nolock)");
                using (DbDataReader dbDataReader = DB.EjecutarConsulta())
                {
                    dbDataReader.Read();
                    txt = dbDataReader[0].ToString();
                    pdf = dbDataReader[1].ToString();
                    bck = dbDataReader[2].ToString();
                    servidor_correo = dbDataReader[6].ToString();
                    user_recepcion = dbDataReader[4].ToString();
                    clave_correo_recep = dbDataReader[5].ToString();
                }

                DB.Desconectar();
            }
            catch (System.Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                logErrores.mensajesLog("ES003", "", ex.Message, "", "Problema con la conexion a la base de datos metodo", "clase de error Invoice.cs");
            }
        }

        private DataSet ContenidoXML()
        {
            Control.Log log = new Control.Log();
            DataSet dsConsulta = new DataSet();
            BasesDatos DB = new BasesDatos();
            try
            {

                DB.Conectar();
                dsConsulta = DB.TraerDataSetConsulta(@"select idLog, infoAdicional from LogWebService with(nolock) where isnull(estado,'') = @p1", new Object[] { "P" });
                DB.Desconectar();
            }
            catch (Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                log.mensajesLog("ES003", "", ex.Message, "", "", "Problema con la conexion a la base de datos metodo ContenidoXML clase de error Invoice.cs");
            }
            return dsConsulta;
        }

        private void procesoHilos()
        {
            BasesDatos DB = new BasesDatos();

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
                            string Esquema = consultaMetodoOffOn();
                            if (Esquema.Equals("OFF"))
                            {
                                using (DataSet dsConsultaxml = ContenidoXML())
                                {
                                    if (dsConsultaxml.Tables.Count > 0)
                                    {
                                        foreach (DataRow dr in dsConsultaxml.Tables[0].Rows)
                                        {
                                            LecturaTxT = new Leer();
                                            LecturaTxT.procesarArchivoXML(false, dr["infoAdicional"].ToString(), dr["idLog"].ToString(), Esquema);
                                            DB.Conectar();
                                            using (var x = DB.TraerDataSetConsulta(@"update LogWebService set estado = @p1 where idLog = @p2 ", new Object[] { "A", dr["idLog"].ToString() }))
                                            {
                                            }

                                            DB.Desconectar();
                                        }

                                    }
                                }

                                try
                                {
                                    if (contador > 1)
                                    {
                                        contador = 0;
                                        webServiceWLF();
                                        ReprocesoOff();

                                    }
                                    else
                                    {
                                        System.Threading.Thread.Sleep(10000); //10 segundos
                                        contador++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DB.Desconectar();
                                    clsLogger.Graba_Log_Error("error  ReprocesoOff:" + ex.Message);
                                }

                            }

                            monitoreando = false;
                            
                        }


                    }
                    catch (System.Exception ex)
                    {
                            DB.Desconectar();
                            clsLogger.Graba_Log_Error("error procesoHilos:" + ex.ToString());
                        logErrores.mensajesLog("ES003", "Catch Inicion Primer Timer ThreadFacturaElectronica.", ex.Message, "", "", "Servicio ThreadFacturaElectronica");
                    }
                    finally
                    {
                        monitoreando = false;
                    }

                    System.Threading.Thread.Sleep(10000); //10 segundos
                }
            }
            catch (UnauthorizedAccessException ex)
            {

                monitoreando = false;
                DB.Desconectar();
                //log1.mensajesLog("ES003", "Catch Inicion Primer Timer InvoicecComprobantes. ", ex.Message, "", "", "Servicio InvoicecComprobantes");
                ThreadFacturaElectronica.Abort();
                ThreadFacturaElectronica = null;
                GenerarProcesoThread();
            }
            monitoreando = false;

        }

        private void ReprocesoOff()
        {
            //while (true)
            //{
            clsLogger.Graba_Log_Info("ingresando a ReprocesoOff");
            BasesDatos DB = new BasesDatos();
            try
            {

                try
                {
                    DB.Desconectar();
                }
                catch (Exception ex) { clsLogger.Graba_Log_Error("error 2.3 " + ex.Message); }
                DB.Conectar();
                using (DataSet ds = DB.TraerDataSetConsulta(@"select top 50 idComprobante , claveAcceso , ambiente , codigoControl ,
                                                                                codDoc , estab , ptoEmi , secuencial , fecha, RECEPTOR.NOMREC from GENERAL  with(nolock), RECEPTOR with(nolock)
                                                                                where GENERAL.id_Receptor = RECEPTOR.IDEREC AND 
                                                                                ((estado ='2' AND tipo ='E') OR (estado ='1' AND creado = '1' AND tipo ='E' and numeroAutorizacion is null) OR (estado ='4')) 
                                                                                AND (fecha < dateadd(MINUTE,-2,GETDATE()))
                                                                                order by 1 desc", new Object[] { }))
                {
                    if (ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            off = new ConsultaOff();
                            off.consultarDocumentoOffline(
                                                                    dr["idComprobante"].ToString(),
                                                                    dr["claveAcceso"].ToString(),
                                                                    dr["ambiente"].ToString(),
                                                                    dr["codigoControl"].ToString(),
                                                                    dr["codDoc"].ToString(),
                                                                    dr["estab"].ToString(),
                                                                    dr["ptoEmi"].ToString(),
                                                                    dr["secuencial"].ToString(),
                                                                    dr["fecha"].ToString(),
                                                                    dr["NOMREC"].ToString());
                        }
                    }
                }
                //System.Threading.Thread.Sleep(30000);
                clsLogger.Graba_Log_Info("fin ReprocesoOff");
            }
            catch (System.Exception ex)
            {
                clsLogger.Graba_Log_Error(" error ReprocesoOff:" + ex.ToString());
                DB.Desconectar();
                logErrores.mensajesLog("ES003", "Catch Inicion Primer Timer ThreadFacturaElectronicaOffLine. ", ex.Message, "", "", "Servicio ReprocesoOFF");
            }
            //}
        }


        private static void webServiceWLF()
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                clsLogger.Graba_Log_Info("ingresando a webServiceWLF");
                ConsultaOff coff = new ConsultaOff();
                //DataTable TwebServiceWLF = new DataTable();
                //XDocument CRE = new XDocument(
                //    new XElement("INSTRUCCION",
                //        new XElement("FILTROS",
                //            new XElement("OPCION", "1"))));
                //DB7.Conectar();
                //TwebServiceWLF = DB7.TraerDataset("SP_CONSULTAS_HILOS", CRE.ToString()).Tables[0];
                //DB7.Desconectar();

                XDocument CRE = new XDocument(
                        new XElement("INSTRUCCION",
                            new XElement("FILTROS",
                                new XElement("OPCION", "1"))));
                DB.Conectar();
                using (DataSet TwebServiceWLF = DB.TraerDataset("SP_CONSULTAS_HILOS", CRE.ToString()))
                {
                    DB.Desconectar();
                    clsLogger.Graba_Log_Info("total registros " + TwebServiceWLF.Tables.Count.ToString());
                    if (TwebServiceWLF.Tables.Count > 0)
                    {
                        clsLogger.Graba_Log_Info("paso1");
                        foreach (DataRow dr in TwebServiceWLF.Tables[0].Rows)
                        {
                            clsLogger.Graba_Log_Info("paso2");
                            DataTable tb_infoA = coff.obtener_infoAdicional(dr["idComprobante"].ToString());
                            if (tb_infoA.Rows.Count > 0)
                            {
                                clsLogger.Graba_Log_Info("paso3");
                                coff.RespuestaWebUNASAP(dr["codDoc"].ToString(), dr["numElect"].ToString(), dr["claveAcceso"].ToString(), dr["numeroAutorizacion"].ToString(), dr["fechaAutorizacion"].ToString(), dr["fechaAutorizacion"].ToString(), "", "", "", "AT", "AT", tb_infoA.Rows[0]["sociedad"].ToString(), tb_infoA.Rows[0]["numeroAsientoContable"].ToString(), tb_infoA.Rows[0]["anioAsientoContable"].ToString());
                                clsLogger.Graba_Log_Info("paso4 FIN");
                            }
                        }
                    }
                }

                System.Threading.Thread.Sleep(30000); //10 segundos
                clsLogger.Graba_Log_Info("fin webServiceWLF");
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error(" Catch Error webServiceWLF() - " + ex.Message + " TRAZA: " + ex.StackTrace);
                DB.Desconectar();
                logErrores.mensajesLog("ES003", " Catch Error webServiceWLF()", ex.Message, "", "", "Servicio webServiceWLF");
            }


        }

        private string consultaMetodoOffOn()
        {
            BasesDatos DB = new BasesDatos();
            string result = "";
            try
            {
                DB.Desconectar();
                DB.Conectar();
                DB.CrearComando(@"select Proceso from ParametrosSistema with(nolock) ");
                using (DbDataReader dbDataReader4 = DB.EjecutarConsulta())
                {
                    if (dbDataReader4.Read())
                    {
                        result = dbDataReader4["Proceso"].ToString();
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
            
            return result;
        }

        private void Reproceso()
        {
            leer_reproceso = new LeerReenvioSRI();
            System.Collections.ArrayList arrayList = new System.Collections.ArrayList();
            string[] array = new string[7];
            BasesDatos DB = new BasesDatos();
            try
            {
                DB.Desconectar();
                DB.Conectar();
                DB.CrearComando(@"Select codigoControl,codDoc,idComprobante,tipoEmision as tipoEnvio,
                                                                case when estado = '4' then '1' when fecha < dateadd(MINUTE,-30,GETDATE()) then '2' else estado end as estado,
                                                                tipo,claveAcceso, ambiente, RECEPTOR.NOMREC
                                                                from GENERAL with(nolock), RECEPTOR with(nolock)
                                                                WHERE GENERAL.id_Receptor = RECEPTOR.IDEREC AND ((estado ='2' AND tipo ='E') OR (estado ='1' AND creado = '1' AND tipo ='E' and numeroAutorizacion is null) OR (estado ='4')) AND (fecha < dateadd(MINUTE,-2,GETDATE()))");
                using (DbDataReader dbDataReader = DB.EjecutarConsulta())
                {
                    while (dbDataReader.Read())
                    {
                        arrayList.Add(new string[]
                                       {
                                        dbDataReader[0].ToString().Trim(),
                                        dbDataReader[1].ToString().Trim(),
                                        dbDataReader[2].ToString().Trim(),
                                        dbDataReader[3].ToString().Trim(),
                                                                      dbDataReader[4].ToString().Trim(),
                                                                      dbDataReader[5].ToString().Trim(),
                                                                      dbDataReader[6].ToString().Trim(),
                                                                      dbDataReader[7].ToString().Trim(),
                                                                      dbDataReader[8].ToString().Trim()
                                       });
                    }
                }

                DB.Desconectar();
                foreach (string[] array2 in arrayList)
                {
                    leer_reproceso.procesar(array2[0], array2[1], array2[2], array2[3], array2[4], array2[5], array2[6], array2[7], array2[8]);
                }
            }
            catch (System.Exception ex)
            {
                DB.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);
                logErrores.mensajesLog("ES003Error en reproceso: " + ex.Message, "", ex.Message, "", "", "clase de error Invoice.cs");
            }
        }

        public void RevisarCorreo()
        {
            try
            {
                using (ImapClient ic = new ImapClient(servidor_correo, user_recepcion, clave_correo_recep, ImapClient.AuthMethods.Login, 993, true, false))
                {
                    ic.SelectMailbox("INBOX");
                    Lazy<MailMessage>[] messages = ic.SearchMessages(SearchCondition.Unseen(), false);
                    string sender = "", NombreArchivo = "";
                    AE.Net.Mail.MailMessage m = null;
                    try
                    {
                        foreach (Lazy<MailMessage> message in messages)
                        {
                            m = message.Value;
                            sender = m.From.Address;
                            foreach (Attachment attachment in m.Attachments)
                            {
                                string fileName = string.Empty;
                                string extension = string.Empty;
                                fileName = attachment.Filename;
                                NombreArchivo = fileName + " Asunto: " + m.Subject;
                                extension = Path.GetExtension(fileName);
                                try
                                {
                                    if (extension.Equals(".zip") || extension.Equals(".ZIP"))
                                    {
                                        MemoryStream inStream = new MemoryStream();
                                        attachment.Save(inStream);
                                        inStream.Position = 0;

                                        using (ZipFile zip = ZipFile.Read(inStream))
                                        {
                                            foreach (ZipEntry e in zip)
                                            {
                                                string extension2 = string.Empty;
                                                extension2 = Path.GetExtension(e.FileName);
                                                if (extension2.Equals(".xml") || extension2.Equals(".XML"))
                                                {
                                                    using (var ms = new MemoryStream())
                                                    {
                                                        e.Extract(ms);
                                                        ms.Position = 0;
                                                        var sr = new StreamReader(ms);
                                                        string mystr = sr.ReadToEnd();
                                                        mystr = encodingUTF8String(mystr);
                                                        //
                                                        XmlDocument doc = new XmlDocument();
                                                        doc.LoadXml(mystr);
                                                        rece_doc.procesarRecepcion(doc, sender);

                                                    }

                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (extension.Equals(".xml") || extension.Equals(".XML"))
                                        {
                                            using (var ms = new MemoryStream())
                                            {
                                                attachment.Save(ms);
                                                ms.Position = 0;
                                                //
                                                var sr = new StreamReader(ms);
                                                string mystr = sr.ReadToEnd();
                                                mystr = encodingUTF8String(mystr);
                                                //
                                                XmlDocument doc = new XmlDocument();
                                                doc.LoadXml(mystr);
                                                rece_doc.procesarRecepcion(doc, sender);

                                            }

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //DB.Desconectar();
                                    clsLogger.Graba_Log_Error(ex.Message);
                                    enviar_notificacion_correo_punto("Error en el archivo de recepción correo: " + sender + " archivo:" + NombreArchivo + " Error: " + ex.Message);
                                }
                            }

                            // Setear como leido
                            ic.SetFlags(Flags.Seen, m);
                        }
                    }
                    catch (Exception ex)
                    {
                        //DB.Desconectar();
                        clsLogger.Graba_Log_Error(ex.Message);
                        enviar_notificacion_correo_punto("Error en el archivo de recepción correo: " + sender + " archivo:" + NombreArchivo + " Error: " + ex.Message);
                        ic.SetFlags(AE.Net.Mail.Flags.Seen, m);
                    }
                }
                rece_doc.ProcesoDocsPendientes();
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error(ex.Message);

                //DB.Desconectar();
                logErrores.mensajesLog("ES003", "Error en revisar correo recepción: " + ex.Message, ex.Message, "", ThreadFacturaElectronica.ThreadState.ToString(), "clase de error Invoice.cs");
            }
        }

        private void enviar_notificacion_correo_punto(string pr_mensaje)
        {
            BasesDatos DB = new BasesDatos();
            try
            {

                string text = "";
                string servidor = "";
                string emailCredencial = "";
                string passCredencial = "";
                string from = "";
                bool ssl = true;
                int puerto = 0;
                DB.Conectar();
                using (DataSet dataSet = DB.TraerDataSetConsulta(@"select distinct a.correo from  dbo.Sucursales a with(nolock) select * from dbo.ParametrosSistema with(nolock)", new object[0]))
                {
                    foreach (DataRow dataRow in dataSet.Tables[0].Rows)
                    {
                        text = text.Trim(new char[]
                        {
                    ','
                        }) + "," + dataRow[0].ToString().Trim(new char[]
                        {
                    ','
                        });
                    }
                    if (dataSet.Tables[1].Rows.Count > 0)
                    {
                        servidor = dataSet.Tables[1].Rows[0]["servidorSMTP"].ToString();
                        emailCredencial = dataSet.Tables[1].Rows[0]["userSMTP"].ToString();
                        passCredencial = dataSet.Tables[1].Rows[0]["passSMTP"].ToString();
                        from = dataSet.Tables[1].Rows[0]["emailEnvio"].ToString();
                        ssl = System.Convert.ToBoolean(dataSet.Tables[1].Rows[0]["sslSMTP"]);
                        puerto = System.Convert.ToInt32(dataSet.Tables[1].Rows[0]["puertoSMTP"]);
                    }
                }

                text = text.Trim(new char[]
                                {
                ','
                                });
                EnviarMail enviarMail = new EnviarMail();
                enviarMail.servidorSTMP(servidor, puerto, ssl, emailCredencial, passCredencial);
                if (text.Length > 10)
                {
                    string subject = "Documeto Recepción con observaciones";
                    string text2 = "Estimado(a);  <br>\r\n\t\t\t\t\t\t\tHubo inconvenientes con documento electrónico.";
                    text2 = text2 + "<br><br>Mensaje: " + pr_mensaje;
                    enviarMail.llenarEmail(from, text.Trim(new char[]
                    {
                    ','
                    }), "", "", subject, text2);
                    try
                    {
                        enviarMail.enviarEmail();
                    }
                    catch (System.Net.Mail.SmtpException ex)
                    {
                        DB.Desconectar();
                        logErrores.mensajesLog("ES003", "Error en enviar correo error de recepción: " + text, ex.Message, "", "", "clase de error Invoice.cs");
                    }
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
            
        }

        private string encodingUTF8String(string p_valor)
        {
            string text = "";
            try
            {
                byte[] bytes = System.Text.Encoding.Default.GetBytes(p_valor);
                text = System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch (System.Exception ex)
            {
                text = "<error>Error método encodingUTF8String: " + ex.Message + "</error>";
            }
            return text.Trim();
        }

    }
}
