using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Control;
using System.Threading;
using Datos;
using System.Data.Common;
using System.Collections;
using AE.Net.Mail;
using Ionic.Zip;
using System.Xml;
using CriptoSimetrica;
using System.Configuration;
using clibLogger;

namespace Interfaz
{
				public partial class Inicio : Form
				{

                    private AES Cs = new AES();

								#region "Creacion de variables generales static"
								private static Leer LecturaTxT;
								private static BasesDatos DB = new BasesDatos();
								private static BasesDatos DB3 = new BasesDatos();
								private static String txt;
								private static Thread ThreadFacturaElectronica;
								private static String pdf;
								private static String bck;
								private static Log logErrores = new Log();
								private static int contador = 0;
								private string men;
								private static String servidor_correo, user_recepcion, clave_correo_recep;
								#endregion

								//Reproceso
								private static LeerReenvioSRI leer_reproceso;
								private static ConsultaOff off;
								private static LeerOffline lloff;

								private static Recepcion rece_doc;
        private bool monitoreando;

        public Inicio()
								{
												InitializeComponent();
                                                string valor = "";
                                                valor = Cs.encriptar("Unacem.2019", "CIMAIT");
                                }
								private void Form1_Load(object sender, EventArgs e)
								{

												//consumeWSprueba();
												//    Leer k = new Leer();
												//    k.RespuestaWebUNASAP("07", "001030000000245", "3011201507099138193700120010300000002450000024515", "0312201520335809913819370011824670710", "2015-12-03", "20:33:58-05:00", "", "", "", "AT", "AT", "", "", "");
												//Leer k = new Leer();
												//  k.RespuestaWebUNASAP("07", "001030000000245", "3011201507099138193700120010300000002450000024515", "0312201520335809913819370011824670710", "2015/12/03 20:33:58", "2015/12/03 20:33:58", "", "", "", "AT", "AT", "ECO2", "0600000017", "2015");

												//LecturaTxT = new Leer();
												//LecturaTxT.RespuestaWebUNASAP("07", "001-030-000000245", "3011201507099138193700120010300000002450000024515", "0312201520335809913819370011824670710", "2015-12-03", "20:33:58", "", "", "", "AT", "AT", "ECO2", "0600000017", "2015");

												//lloff = new LeerOffline();
												//lloff.RespuestaWebUNASAPTEST("07", "001" + "-" + "001" + "-" + "000000027", "0405201707179023686200110010010000000270000002710", "0405201707179023686200110010010000000270000002710", "", "", "", "", "", "AT", "AT", "EC01", "0600000048", "2017");

                                                GenerarProcesoThread();
                                                //ReprocesoOff();

                                    //validarCorreo();


								}



                                private void validarCorreo() 
                                {
                                    if (obtener_codigo("ActivarValidacionesCorreo").Equals("SI"))
                                    {
                                        DateTime fechaSistema = DateTime.Today;

                                        DateTime FechaMayor = Convert.ToDateTime(obtener_codigo("IntervaloFechaMayor"));
                                        DateTime FechaMenor = Convert.ToDateTime(obtener_codigo("IntervaloFechaMenor"));

                                        if (obtener_codigo("tipoDocumentoEnvioCorreo").Contains("01") && fechaSistema >= FechaMayor && fechaSistema <= FechaMenor) //
                                        {

                                            //enviar_correo();

                                        }
                                    }
                                    else
                                    {
                                        if (!"01".Contains(obtener_codigo("tipoDocumentoEnvioCorreo"))) //!codDoc.Equals("01")
                                        {
                                            //log.mensajesLog("US001", "", "enviando documentos que  sean diferentes de facturas ", "", codigoControl, "leerOffLine.cs");
                                            //log.guardar_Log("inicia envio correo cliente");
                                            //enviar_correo();
                                            //log.guardar_Log("fin envio correo cliente");

                                            //if (codDoc.Equals("06")) 
                                            //{
                                            //    log.guardar_Log("inicia EnviarPDFGuia");
                                            //    EnviarPDFGuia epdf = new EnviarPDFGuia();
                                            //    epdf.envia_guia(cPDF.msPDF(codigoControl), codigoControl, estab, ptoEmi);
                                            //    log.guardar_Log("fin EnviarPDFGuia");
                                            //}
                                        }
                                    }
                                }


                                private string obtener_codigo(string a_parametro)
                                {
                                    string retorna = ConfigurationManager.AppSettings.Get(a_parametro);

                                    return retorna;
                                }

		/// <summary>
		/// llamado hilo que lo instancia de la clase Golbal.asax
		/// </summary>
		//public void GenerarProcesoThread()
		//{



		//				try
		//				{

		//								conectarParametrosdelSistema();
		//								//procesoHilos();
		//								ReprocesoOff();
		//								//if (ThreadFacturaElectronica == null)
		//								//{
		//								//    ThreadFacturaElectronica = new Thread(new ThreadStart(procesoHilos));
		//								//    ThreadFacturaElectronica.CurrentCulture = new System.Globalization.CultureInfo("es-MX");
		//								//    ThreadFacturaElectronica.Name = "tProcesoFacturaElectronica";
		//								//    ThreadFacturaElectronica.Priority = ThreadPriority.Highest;
		//								//    ThreadFacturaElectronica.Start();

		//								//}
		//								//else
		//								//{
		//								//    logErrores.mensajesLog("No se ha iniciado el servicio Invoicec: El proceso " + ThreadFacturaElectronica.Name + ": Se encuentra " + ThreadFacturaElectronica.ThreadState, "", "Proceso Thread", ThreadFacturaElectronica.Name, "Metodo GenerarProcesoThread");
		//								//}

		//				}
		//				catch (Exception ex)
		//				{
		//								logErrores.mensajesLog("ES003", "", ex.Message, "", "Problema con el proceso Thread  ", "clase de error Invoicec.cs");
		//				}
		//}

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
				//if (ThreadFacturaElectronicaOffLine == null)
				//{
				//                ThreadFacturaElectronicaOffLine = new System.Threading.Thread(new System.Threading.ThreadStart(ReprocesoOff));
				//                ThreadFacturaElectronicaOffLine.CurrentCulture = new System.Globalization.CultureInfo("es-MX");
				//                ThreadFacturaElectronicaOffLine.Name = "ThreadFacturaElectronicaOffLine";
				//                ThreadFacturaElectronicaOffLine.Priority = System.Threading.ThreadPriority.Highest;
				//                ThreadFacturaElectronicaOffLine.Start();
				//}
			}
			catch (System.Exception ex)
			{
				logErrores.mensajesLog("ES003", "", ex.Message, "", "Problema con el proceso Thread  ", "clase de error Invoicec.cs");
			}
		}


		private void ReprocesoOff()
								{
												off = new ConsultaOff();
												//t.Enabled = false;
												ArrayList arrayLFacturas = new ArrayList();
												String[] datos = new String[6];
												try
            {
                DB.Conectar();
                DB.CrearComando(@"select idComprobante , claveAcceso , ambiente , codigoControl ,
                                                                                codDoc , estab , ptoEmi , secuencial , fecha, RECEPTOR.NOMREC from GENERAL  with(nolock), RECEPTOR  with(nolock)
                                                                                where GENERAL.id_Receptor = RECEPTOR.IDEREC AND 
                                                                                ((estado ='2' AND tipo ='E') OR (estado ='1' AND creado = '1' AND tipo ='E' and numeroAutorizacion is null) OR (estado ='4')) 
                                                                                AND (fecha < dateadd(MINUTE,-2,GETDATE()))
                                                                                order by 1 desc ");
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        datos = new String[10];
                        datos[0] = DR[0].ToString().Trim();
                        datos[1] = DR[1].ToString().Trim();
                        datos[2] = DR[2].ToString().Trim();
                        datos[3] = DR[3].ToString().Trim();
                        datos[4] = DR[4].ToString().Trim();
                        datos[5] = DR[5].ToString().Trim();
                        datos[6] = DR[6].ToString().Trim();
                        datos[7] = DR[7].ToString().Trim();
                        datos[8] = DR[8].ToString().Trim();
                        datos[9] = DR[9].ToString().Trim();

                        arrayLFacturas.Add(datos);
                    }
                }

                DB.Desconectar();

                foreach (String[] codigo in arrayLFacturas)
                {
                    //momentaneo(codigo[0]);
                    //leer_reproceso.procesar(codigo[0], codigo[1], codigo[2], codigo[3], codigo[4], codigo[5], codigo[6], codigo[7]);
                    off.consultarDocumentoOffline(codigo[0], codigo[1], codigo[2], codigo[3], codigo[4], codigo[5], codigo[6], codigo[7], codigo[8], codigo[9]);
                }
            }
            catch (Exception ex)
												{
																logErrores.mensajesLog("ES003" + "Error en reproceso off line: " + ex.Message, "", ex.Message, "", "", "clase de error Invoice.cs");
																//System.Diagnostics.EventLog.WriteEntry("Application SRI", "Exception: " + ex.Message);
												}
												//t.Enabled = true;
								}










								/// <summary>
								/// Metodo que extrae los parametros del sistema
								/// </summary>
								private static void conectarParametrosdelSistema()
								{
												//Leer k = new Leer();
												//k.RespuestaWebUNASAP("07", "001-030-000000245", "3011201507099138193700120010300000002450000024515", "0312201520335809913819370011824670710", "2015-12-03", "20:33:58", "", "", "", "AT", "AT", "ECO2", "0600000017", "2015");

												try
            {
                //LecturaTxT = new Leer();
                //logErrores = new Log();
                //leer_reproceso = new LeerReenvioSRI();
                //rece_doc = new Recepcion();
                DB.Conectar();
                DB.CrearComando(@"select dirtxt,dirdocs,dirrespaldo,servidorSMTP/*,correoRecepcion,passRecepcion,*/dirRecepcion from ParametrosSistema  with(nolock)");
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    DR.Read();
                    txt = DR[0].ToString();
                    pdf = DR[1].ToString();
                    bck = DR[2].ToString();
                    //servidor_correo = DR[6].ToString();
                    //user_recepcion = DR[4].ToString();
                    clave_correo_recep = DR[3].ToString();
                }
                //DB.Desconectar();
            }
            catch (Exception ex)
												{
																logErrores.mensajesLog("ES003", "", ex.Message, "", "Problema con la conexion a la base de datos metodo conectarParametrosdelSistema", "clase de error Invoice.cs");
												}
												finally
												{
																DB.Desconectar();
												}
								}


								private void timer1_Tick(object sender, EventArgs e)
								{
												timer1.Enabled = false;

												//consumeWSprueba();
												//try
												//{
												//    String[] files = Directory.GetFiles(txt);
												//    foreach (String file in files)
												//    {
												//        LecturaTxT.procesarArchivoXML(file);
												//    }
												//}
												//catch (UnauthorizedAccessException) { };

												//if (contador > 30)
												//{
												//    contador = 0;

												//    //Reproceso();
												//}
												//else
												//{
												//    contador++;
												//}

												timer1.Enabled = true;
								}


								private void Reproceso()
								{
												leer_reproceso = new LeerReenvioSRI();
												//t.Enabled = false;
												ArrayList arrayLFacturas = new ArrayList();
												String[] datos = new String[6];
												try
            {
                DB.Conectar();
                DB.CrearComando(@"Select codigoControl,codDoc,idComprobante,tipoEmision as tipoEnvio,case when fecha < dateadd(dd,-2,GETDATE()) then '2' else estado end as estado,tipo,claveAcceso, ambiente from GENERAL with(nolock) WHERE (estado ='2' AND tipo ='E') OR (estado ='1' AND creado = '1' AND tipo ='E' and numeroAutorizacion is null)");
                using (DbDataReader DR = DB.EjecutarConsulta())
                {
                    while (DR.Read())
                    {
                        datos = new String[8];
                        datos[0] = DR[0].ToString().Trim();
                        datos[1] = DR[1].ToString().Trim();
                        datos[2] = DR[2].ToString().Trim();
                        datos[3] = DR[3].ToString().Trim();
                        datos[4] = DR[4].ToString().Trim();
                        datos[5] = DR[5].ToString().Trim();
                        datos[6] = DR[6].ToString().Trim();
                        datos[7] = DR[7].ToString().Trim();
                        datos[8] = DR[8].ToString().Trim();
                        arrayLFacturas.Add(datos);
                    }
                }

                DB.Desconectar();

                foreach (String[] codigo in arrayLFacturas)
                {
                    //momentaneo(codigo[0]);
                    leer_reproceso.procesar(codigo[0], codigo[1], codigo[2], codigo[3], codigo[4], codigo[5], codigo[6], codigo[7], codigo[8]);
                }
            }
            catch (Exception ex)
												{
																logErrores.mensajesLog("ES003" + "Error en reproceso: " + ex.Message, "", ex.Message, "", "", "clase de error Invoice.cs");
																//System.Diagnostics.EventLog.WriteEntry("Application SRI", "Exception: " + ex.Message);
												}
												//t.Enabled = true;
								}


								/// <summary>
								/// metodo que consulta los documentos que estan pendiente para la facturacion electronica
								/// </summary>
								/// <returns></returns>
								private static DataSet ContenidoXML()
								{
												DataSet dsConsulta = new DataSet();
												try
												{
																try { DB.Conectar(); }
																catch (Exception ex) { }
																dsConsulta = DB.TraerDataSetConsulta(@"select idLog, infoAdicional, tipo from LogWebService with(nolock) where isnull(estado,'') = @p1", new Object[] { "P" });
												}
												catch (Exception ex)
												{
																logErrores.mensajesLog("ES003", "Problema con la conexion a la base de datos metodo ContenidoXML", ex.Message, "", "", "clase de error Invoice.cs");
												}
												return dsConsulta;
								}

								/// <summary>
								/// metodo que es proceso de hilos que es llamado del metodo principal
								/// </summary>
								//private void procesoHilos()
								//{


								//    while (true)
								//    {
								//        try
								//        {
								//            //Reproceso();

								//            LecturaTxT = new Leer();
																				
								//            String[] files = Directory.GetFiles(txt);
								//            foreach (String file in files)
								//            {
								//                string extension = string.Empty;
								//                extension = Path.GetExtension(file);

								//                if (extension.Equals(".xml") || extension.Equals(".XML"))
								//                {
								//                    LecturaTxT.procesarArchivoXML(true, file, "", "");
								//                }
								//                else
								//                {
								//                    LecturaTxT.procesarArchivo(file);
								//                }

								//            }

								//            DataSet dsConsultaxml = ContenidoXML();
								//            string proceso = consultaMetodoOffOn();
								//            if (dsConsultaxml.Tables.Count > 0)
								//                foreach (DataRow dr in dsConsultaxml.Tables[0].Rows)
								//                {
								//                    LecturaTxT.procesarArchivoXML(false, dr["infoAdicional"].ToString(), dr["idLog"].ToString(), proceso);
								//                    if (!proceso.Equals("OFF"))
								//                    {
								//                        DB.Conectar();
								//                        DB.TraerDataSetConsulta(@"update LogWebService set estado = @p1 where idLog = @p2 ", new Object[] { "A", dr["idLog"].ToString() });
								//                    }
								//                }



								//            //if (contador > 1)
								//            //{
								//            //    contador = 0;

								//            //    // Reproceso();

								//            //    // RevisarCorreo();
								//            //}
								//            //else
								//            //{
								//            //    contador++;
								//            //}
								//            //// procesando tabla de LogWebservice para verificar que comprobantes estan pendientes
								//            //// durmiendo el hilo
								//            Thread.Sleep(10000);
								//        }
								//        catch (UnauthorizedAccessException ex)
								//        {
								//            logErrores.mensajesLog("ES003", "", ex.Message, "", "Problema con el proceso Thread en el metodo  procesoHilos()", "clase de error Invoice.cs");
								//            Thread.Sleep(10000);
								//            ThreadFacturaElectronica.Start();
								//        }
								//    }
								//}

    private void facturaToolStripMenuItem1_Click(object sender, EventArgs e)
    {
            procesoHilos();
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
                                        //webServiceWLF();
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
        //								private void procesoHilos()
        //								{
        //												while (true)
        //												{
        //																try
        //                {
        //                    LecturaTxT = new Leer();
        //                    using (DataSet dataSet = ContenidoXML())
        //                    {
        //                        string text = this.consultaMetodoOffOn();
        //                        if (dataSet.Tables.Count > 0)
        //                        {
        //                            foreach (DataRow dataRow in dataSet.Tables[0].Rows)
        //                            {
        //                                LecturaTxT.procesarArchivoXML(false, dataRow["infoAdicional"].ToString(), dataRow["idLog"].ToString(), text);


        //                                if (!text.Equals("OFF"))
        //                                {
        //                                    DB3.Conectar();
        //                                    DB3.TraerDataSetConsulta(@"update LogWebService set estado = @p1 where idLog = @p2 ", new object[]
        //{
        //                                                                            "A",
        //                                                                            dataRow["idLog"].ToString()
        //});
        //                                }
        //                            }
        //                        }
        //                        if (!text.Equals("OFF"))
        //                        {
        //                            if (contador > 30)
        //                            {
        //                                contador = 0;
        //                                this.Reproceso();
        //                            }
        //                            else
        //                            {
        //                                contador++;
        //                            }
        //                        }
        //                    }

        //                    System.Threading.Thread.Sleep(30000);

        //                }
        //                catch (System.Exception ex)
        //																{

        //																				DB3.Desconectar();
        //																				logErrores.mensajesLog("ES003", "Catch Inicion Primer Timer ThreadFacturaElectronica. ", ex.Message, "", "", "Servicio ThreadFacturaElectronica");
        //																}
        //												}
        //								}

        private string consultaMetodoOffOn()
        {
            string result = "";
            DB.Conectar();
            DB.CrearComando(@"select Proceso from ParametrosSistema  with(nolock)");
            using (DbDataReader DR = DB.EjecutarConsulta())
            {
                while (DR.Read())
                {
                    result = DR["Proceso"].ToString();
                }
            }

            DB.Desconectar();
            return result;
        }

								private void regPdf_Click(object sender, EventArgs e)
								{
												//Detener_Hilo_Invoicec();
												try
												{
																CrearPDF cPDF = new CrearPDF();
				var memoryStream = new MemoryStream();
																cPDF.PoblarReporte(out memoryStream, txtCodControl.Text, txtIdComprobante.Text, txtCodDoc.Text);
				File.WriteAllBytes("Prueba.pdf", memoryStream.ToArray());
			}
												catch (Exception ex)
												{
												}

								}

        private void generarFacturaToolStripMenuItem_Click(object sender, EventArgs e)
        {
			String[] files = Directory.GetFiles(@"C:\DataExpress\txt\procesar");
			foreach (String file in files)
			{

				if (file.ToLower().Contains(".txt"))
				{
					//LecturaTxT.procesarArchivo(file);
				}
				else if (file.ToLower().Contains(".xml"))
				{
					Control.Leer leer = new Control.Leer();
					leer.procesarArchivoXML(true, file, "", "OFF");
				}
			}
		}

        public void Detener_Hilo_Invoicec()
								{
												string men;
												try
												{
																if (ThreadFacturaElectronica != null)
																{
																				ThreadFacturaElectronica.Abort();
																				men = "Se detuvo el servicio InvoicecContado: El proceso " + ThreadFacturaElectronica.Name + ": Se encuentra " + ThreadFacturaElectronica.ThreadState;
																				//log_Invoicec(men, ThreadFacturaElectronica.ThreadState.ToString(), 1);
																				logErrores.mensajesLog("ES003", men, "", "", ThreadFacturaElectronica.ThreadState.ToString(), "clase de error Invoice.cs");
																				ThreadFacturaElectronica = null;
																}
																else
																{
																				men = "No se puede detener el servicio InvoicecContado: El proceso " + ThreadFacturaElectronica.Name + ": Se encuentra " + ThreadFacturaElectronica.ThreadState;
																				//log_Invoicec(men, ThreadFacturaElectronica.ThreadState.ToString(), 1);
																				logErrores.mensajesLog("ES003", men, "", "", ThreadFacturaElectronica.ThreadState.ToString(), "clase de error Invoice.cs");

																}
												}
												catch (Exception ex)
												{
																men = "Error en ejecución del Hilo tContado Abort: " + DateTime.Now.ToString() + ":" + ex.Message;
																//log_Invoicec(men, ex.Message, 1);
																logErrores.mensajesLog("ES003", men, ex.Message, "", ThreadFacturaElectronica.ThreadState.ToString(), "clase de error Invoice.cs");
												}
								}


								public void RevisarCorreo()
								{
												try
												{

																//using (ImapClient ic = new ImapClient(servidor_correo, user_recepcion, clave_correo_recep, ImapClient.AuthMethods.Login, 143, false, false))
																using (ImapClient ic = new ImapClient(servidor_correo, user_recepcion, clave_correo_recep, ImapClient.AuthMethods.Login, 993, true, false))
																{
																				// Open a mailbox, case-insensitive
																				ic.SelectMailbox("INBOX");

																				// Get messages based on the flag "Undeleted()".
																				Lazy<MailMessage>[] messages = ic.SearchMessages(SearchCondition.Unseen(), false);

																				// Process each message
																				foreach (Lazy<MailMessage> message in messages)
																				{
																								MailMessage m = new MailMessage();
																								m = message.Value;

																								string sender = m.From.Address;

																								foreach (Attachment attachment in m.Attachments)
																								{
																												string fileName = string.Empty;
																												string extension = string.Empty;
																												fileName = attachment.Filename;// item.Attachments[i].FileName;
																												extension = Path.GetExtension(fileName);

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
																																																XmlDocument doc = new XmlDocument();
																																																doc.Load(ms);

																																																rece_doc.procesarRecepcion(doc, sender);

																																												}

																																								}
																																								//e.Extract(TargetDirectory, true);  // overwrite == true
																																				}
																																}

																																//ZipFile.OpenRead(fileName);
																												}
																												else
																												{

																																//if (extension.Equals(".pdf") || extension.Equals(".PDF") || extension.Equals(".xml") || extension.Equals(".XML"))
																																if (extension.Equals(".xml") || extension.Equals(".XML"))
																																{
																																				//attachment.Save(txt + fileName);
																																				using (var ms = new MemoryStream())
																																				{
																																								attachment.Save(ms);
																																								ms.Position = 0;
																																								XmlDocument doc = new XmlDocument();
																																								doc.Load(ms);

																																								rece_doc.procesarRecepcion(doc, sender);

																																				}

																																}
																												}
																												//attachment.Save(@"C:\Demo\" + fileName + Path.GetExtension(attachment.Filename));
																								}

																								// Setear como leido
																								ic.SetFlags(Flags.Seen, m);
																				}
																}
																//Reproceso de docuementos que no se pudieron validar
																rece_doc.ProcesoDocsPendientes();
												}
												catch (Exception ex)
												{
																logErrores.mensajesLog("ES003", "Error en revisar correo recepción: " + ex.Message, ex.Message, "", ThreadFacturaElectronica.ThreadState.ToString(), "clase de error Invoice.cs");
												}
								}

				}
}
