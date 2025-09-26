using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Datos;

namespace Logs
{
    public class Log
    {
        private BasesDatos DB;

        public Log()
        {
            this.DB = new BasesDatos();
        }
        public void mensajesLog(string codigo, string mensaje, string mensajeTecnico, string nombreArchivo, string noFolio)
        {
            try
            {
                string[] array = new string[2];
                array = this.PA_mensajes(codigo);
                if (string.IsNullOrEmpty(mensaje))
                {
                    mensaje = "Error";
                }
                if (string.IsNullOrEmpty(nombreArchivo))
                {
                    nombreArchivo = "Error";
                }
                this.DB.Conectar();
                this.DB.CrearComando("insert into LogErrorFacturas\r\n                                (detalle,fecha,archivo,linea,numeroDocumento,tipo,detalleTecnico) \r\n                                values \r\n                                (@detalle,getdate(),@archivo,@linea,@numeroDocumento,@tipo,@detalleTecnico)");
                this.DB.AsignarParametroCadena("@detalle", array[0].Replace("'", "''") + System.Environment.NewLine + mensaje.Replace("'", "''"));
                this.DB.AsignarParametroCadena("@fecha", System.DateTime.Now.ToString());
                this.DB.AsignarParametroCadena("@archivo", nombreArchivo.Replace("'", "''"));
                this.DB.AsignarParametroCadena("@linea", "-");
                this.DB.AsignarParametroCadena("@numeroDocumento", noFolio.Replace("'", "''"));
                this.DB.AsignarParametroCadena("@tipo", array[1].Replace("'", "''"));
                this.DB.AsignarParametroCadena("@detalleTecnico", mensajeTecnico.Replace("'", "''"));
                this.DB.EjecutarConsulta1();
                this.DB.Desconectar();
            }
            catch (System.Exception ex)
            {
                this.DB.Desconectar();
                this.guardar_Log2("Error al logInvoicec:" + ex.Message + " |" + codigo + "|" + mensaje + "|" + mensajeTecnico + "|" + nombreArchivo + "|" + noFolio);
            }
        }

        public void mensajesLog(string codigo, string mensaje, string mensajeTecnico, string nombreArchivo, string noFolio, string infoAdicional)
        {
            try
            {
                string[] array = new string[2];
                array = this.PA_mensajes(codigo);
                if (string.IsNullOrEmpty(mensaje))
                {
                    mensaje = "Error";
                }
                if (string.IsNullOrEmpty(nombreArchivo))
                {
                    nombreArchivo = "Error";
                }
                if (string.IsNullOrEmpty(infoAdicional))
                {
                    infoAdicional = "Error";
                }
                if (string.IsNullOrEmpty(noFolio))
                {
                    noFolio = "Error";
                }
                this.DB.Conectar();
                this.DB.CrearComando("insert into LogErrorFacturas\r\n                                (detalle,fecha,archivo,linea,numeroDocumento,tipo,detalleTecnico,infoAdicional) \r\n                                values \r\n                                (@detalle,getdate(),@archivo,@linea,@numeroDocumento,@tipo,@detalleTecnico,@infoAdicional)");
                this.DB.AsignarParametroCadena("@detalle", array[0].Replace("'", "''") + System.Environment.NewLine + mensaje.Replace("'", "''"));
                this.DB.AsignarParametroCadena("@fecha", System.DateTime.Now.ToString());
                this.DB.AsignarParametroCadena("@archivo", nombreArchivo.Replace("'", "''"));
                this.DB.AsignarParametroCadena("@linea", "-");
                this.DB.AsignarParametroCadena("@tipo", array[1].Replace("'", "''"));
                this.DB.AsignarParametroCadena("@numeroDocumento", noFolio.Replace("'", "''"));
                this.DB.AsignarParametroCadena("@detalleTecnico", mensajeTecnico.Replace("'", "''"));
                this.DB.AsignarParametroCadena("@infoAdicional", infoAdicional.Replace("'", "''"));
                this.DB.EjecutarConsulta1();
                this.DB.Desconectar();
            }
            catch (System.Exception ex)
            {
                this.DB.Desconectar();
                this.guardar_Log2("Error al logInvoicec:" + ex.Message + " |" + codigo + "|" + mensaje + "|" + mensajeTecnico + "|" + nombreArchivo + "|" + noFolio + "|" + infoAdicional);
            }
        }

        public string[] PA_mensajes(string codigo)
        {
            string[] array = new string[2];
            try
            {
                this.DB.Conectar();
                this.DB.CrearComandoProcedimiento("PA_Errores");
                this.DB.AsignarParametroProcedimiento("@CODIGO", System.Data.DbType.String, codigo);
                System.Data.Common.DbDataReader dbDataReader = this.DB.EjecutarConsulta();
                if (dbDataReader.Read())
                {
                    array[0] = codigo + ": " + dbDataReader[0].ToString();
                    array[1] = dbDataReader[1].ToString();
                }
                this.DB.Desconectar();
            }
            catch (System.Exception ex)
            {
                this.DB.Desconectar();
                this.guardar_Log2("Error al logInvoicec:" + ex.Message);
            }
            return array;
        }

        public void guardar_Log(string datos)
        {
            if (ConfigurationManager.AppSettings.Get("LogErrorTXT").Equals("SI"))
            {
                string str = "temp\\Log_" + System.DateTime.Now.ToString("ddMMyyyy") + ".txt";
                string path = System.AppDomain.CurrentDomain.BaseDirectory + str;
                if (System.IO.File.Exists(path))
                {
                    using (System.IO.StreamWriter streamWriter = System.IO.File.AppendText(path))
                    {
                        this.contenidoLog(datos, streamWriter);
                        streamWriter.Close();
                    }
                }
                else
                {
                    System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.CreateNew);
                    System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(fileStream);
                    binaryWriter.Close();
                    fileStream.Close();
                    using (System.IO.StreamWriter streamWriter = System.IO.File.AppendText(path))
                    {
                        this.contenidoLog(datos, streamWriter);
                        streamWriter.Close();
                    }
                }
            }
        }

        public void guardar_Log2(string datos)
        {
            string str = "temp\\Log_" + System.DateTime.Now.ToString("ddMMyyyy") + ".txt";
            string path = System.AppDomain.CurrentDomain.BaseDirectory + str;
            if (System.IO.File.Exists(path))
            {
                using (System.IO.StreamWriter streamWriter = System.IO.File.AppendText(path))
                {
                    this.contenidoLog(datos, streamWriter);
                    streamWriter.Close();
                }
            }
            else
            {
                System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.CreateNew);
                System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(fileStream);
                binaryWriter.Close();
                fileStream.Close();
                using (System.IO.StreamWriter streamWriter = System.IO.File.AppendText(path))
                {
                    this.contenidoLog(datos, streamWriter);
                    streamWriter.Close();
                }
            }
        }

        public void contenidoLog(string logMessage, System.IO.TextWriter w)
        {
            w.WriteLine("{0}", System.DateTime.Now.ToString("dd-MM-yyyy  HH:mm:ss") + "  " + logMessage);
            w.WriteLine(" ");
            w.Flush();
        }
    }
}
