using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Datos;
using System.Data.Common;
using System.IO;
using clibLogger;
namespace Control
{

    public class Log
    {

        //private BasesDatos DB;
        //private BasesDatos DB2;
        //private BasesDatos DB3;

        public Log()
        {
            //DB = new BasesDatos();
            //DB2 = new BasesDatos();
            //DB3 = new BasesDatos();
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
                DB.CrearComando(@"insert into LogErrorFacturas
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
                //this.guardar_Log("mensajesLog:" + ex.ToString());		
            }
        }

        public void mensajesLog(string codigo, string mensaje, string mensajeTecnico, string nombreArchivo, string noFolio, string infoAdicional)
        {
            BasesDatos DB2 = new BasesDatos();
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
                DB2.Conectar();
                DB2.CrearComando(@"insert into LogErrorFacturas
                                (detalle,fecha,archivo,linea,numeroDocumento,tipo,detalleTecnico,infoAdicional) 
                                values 
                                (@detalle,@fecha,@archivo,@linea,@numeroDocumento,@tipo,@detalleTecnico,@infoAdicional)");
                DB2.AsignarParametroCadena("@detalle", array[0].Replace("'", "''") + Environment.NewLine + mensaje.Replace("'", "''"));
                DB2.AsignarParametroCadena("@fecha", System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                DB2.AsignarParametroCadena("@archivo", nombreArchivo.Replace("'", "''"));
                DB2.AsignarParametroCadena("@linea", "-");
                DB2.AsignarParametroCadena("@tipo", array[1].Replace("'", "''"));
                DB2.AsignarParametroCadena("@numeroDocumento", noFolio.Replace("'", "''"));
                DB2.AsignarParametroCadena("@detalleTecnico", mensajeTecnico.Replace("'", "''"));
                DB2.AsignarParametroCadena("@infoAdicional", infoAdicional.Replace("'", "''"));
                DB2.EjecutarConsulta1();
                DB2.Desconectar();
            }
            catch (Exception ex)
            {
                DB2.Desconectar();
                clsLogger.Graba_Log_Error(ex.Message);

                //this.guardar_Log("mensajesLog1:" + ex.ToString());									
            }
        }

        public String[] PA_mensajes(string codigo)
        {
            string[] array;
            array = new string[2];
            BasesDatos DB3 = new BasesDatos();
            try
            {

                DB3.Conectar();
                DB3.CrearComandoProcedimiento("PA_Errores");
                DB3.AsignarParametroProcedimiento("@CODIGO", System.Data.DbType.String, codigo);
                using (DbDataReader DRE = DB3.EjecutarConsulta())
                {
                    if (DRE.Read())
                    {
                        array[0] = codigo + ": " + DRE[0].ToString();
                        array[1] = DRE[1].ToString();
                    }
                }

                DB3.Desconectar();
            }
            catch (Exception ex)
            {
                clsLogger.Graba_Log_Error(ex.Message);

                DB3.Desconectar();
                //this.guardar_Log("PA_mensajes:" + ex.ToString());

            }
            return array;
        }

       

      
    }
}
