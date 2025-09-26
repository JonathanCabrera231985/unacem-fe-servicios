using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Datos;
using System.Data.Common;
using clibLogger;
namespace Control
{
    public class LogRecepcion
    {
        //private BasesDatos DB;
        public LogRecepcion()
        {
            //DB = new BasesDatos();
        }

        public void mensajesLog(string codigo, string mensaje, string mensajeTecnico, string nombreArchivo, string noFolio)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                string[] array = new string[2];
                array = PA_mensajes(codigo);
                if (String.IsNullOrEmpty(mensaje))
                {
                    mensaje = "";
                }
                DB.Conectar();
                DB.CrearComando(@"insert into LogErrorRecepcion
                                (detalle,fecha,archivo,linea,numeroDocumento,tipo,detalleTecnico) 
                                values 
                                (@detalle,@fecha,@archivo,@linea,@numeroDocumento,@tipo,@detalleTecnico)");
                DB.AsignarParametroCadena("@detalle", array[0].Replace("'", "''") + Environment.NewLine + mensaje.Replace("'", "''"));
                DB.AsignarParametroCadena("@fecha", System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                DB.AsignarParametroCadena("@archivo", nombreArchivo.Replace("'", "''"));
                DB.AsignarParametroCadena("@linea", "-");
                DB.AsignarParametroCadena("@tipo", array[1].Replace("'", "''"));
                DB.AsignarParametroCadena("@numeroDocumento", noFolio.Replace("'", "''"));
                DB.AsignarParametroCadena("@detalleTecnico", mensajeTecnico.Replace("'", "''"));
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
        public void mensajesLog(string codigo, string mensaje, string mensajeTecnico, string nombreArchivo, string noFolio,string infoAdicional)
        {
            BasesDatos DB = new BasesDatos();
            try
            {
                string[] array = new string[2];
                array = PA_mensajes(codigo);
                if (String.IsNullOrEmpty(mensaje))
                {
                    mensaje = "";
                }
                if (String.IsNullOrEmpty(infoAdicional))
                {
                    infoAdicional = "";
                }
                DB.Conectar();
                DB.CrearComando(@"insert into LogErrorRecepcion
                                (detalle,fecha,archivo,linea,numeroDocumento,tipo,detalleTecnico,infoAdicional) 
                                values 
                                (@detalle,@fecha,@archivo,@linea,@numeroDocumento,@tipo,@detalleTecnico,@infoAdicional)");
                DB.AsignarParametroCadena("@detalle", array[0].Replace("'", "''") + Environment.NewLine + mensaje.Replace("'", "''"));
                DB.AsignarParametroCadena("@fecha", System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                DB.AsignarParametroCadena("@archivo", nombreArchivo.Replace("'", "''"));
                DB.AsignarParametroCadena("@linea", "-");
                DB.AsignarParametroCadena("@tipo", array[1].Replace("'", "''"));
                DB.AsignarParametroCadena("@numeroDocumento", noFolio.Replace("'", "''"));
                DB.AsignarParametroCadena("@detalleTecnico", mensajeTecnico.Replace("'", "''"));
                DB.AsignarParametroCadena("@infoAdicional", infoAdicional.Replace("'", "''"));
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

        public void mensajesLog(string codigo, string mensaje, string mensajeTecnico, string rucProveedor, string noFolio, string claveAcceso, string codDoc )
        {
            BasesDatos DB = new BasesDatos();
            try
            {

                string[] array = new string[2];
                array = PA_mensajes(codigo);
                if (String.IsNullOrEmpty(mensaje))
                {
                    mensaje = "";
                }
                if (String.IsNullOrEmpty(claveAcceso))
                {
                    claveAcceso = "";
                }
                DB.Conectar();
                DB.CrearComando(@"insert into LogErrorRecepcion
                                (detalle,fecha,archivo,linea,numeroDocumento,tipo,detalleTecnico,infoAdicional) 
                                values 
                                (@detalle,@fecha,@archivo,@linea,@numeroDocumento,@tipo,@detalleTecnico,@infoAdicional)");
                DB.AsignarParametroCadena("@detalle", array[0].Replace("'", "''") + Environment.NewLine + mensaje.Replace("'", "''"));
                DB.AsignarParametroCadena("@fecha", System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                DB.AsignarParametroCadena("@archivo", rucProveedor.Replace("'", "''"));
                DB.AsignarParametroCadena("@linea", codDoc);
                DB.AsignarParametroCadena("@tipo", array[1].Replace("'", "''"));
                DB.AsignarParametroCadena("@numeroDocumento", noFolio.Replace("'", "''"));
                DB.AsignarParametroCadena("@detalleTecnico", mensajeTecnico.Replace("'", "''"));
                DB.AsignarParametroCadena("@infoAdicional", claveAcceso.Replace("'", "''"));
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

        public String[] PA_mensajes(string codigo)
        {
            BasesDatos DB = new BasesDatos();
            string[] array;
            array = new string[2];
            try
            {

                DB.Conectar();
                DB.CrearComandoProcedimiento("PA_Errores");
                DB.AsignarParametroProcedimiento("@CODIGO", System.Data.DbType.String, codigo);
                using (DbDataReader DRE = DB.EjecutarConsulta())
                {
                    if (DRE.Read())
                    {
                        array[0] = codigo + ": " + DRE[0].ToString();
                        array[1] = DRE[1].ToString();
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
            
            return array;
        }
    }
}
