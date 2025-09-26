using Datos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GeneraAutorizacion
{
    public class Proceso
    {
        BasesDatos DB;
        enviarSRI enviarSRI;
        public Proceso()
        {
            DB = new BasesDatos();
        }

        public void inicio()
        {
            Logs.Log log = new Logs.Log();
            try
            {
                DB.Conectar();
                DataSet dataSet = new DataSet();
                XDocument CRE = new XDocument(
                        new XElement("INSTRUCCION",
                            new XElement("FILTROS",
                                new XElement("OPCION", "1"))));
                dataSet = DB.TraerDataset("sp_consultaInfoProcesoAu", CRE.ToString());
                if (dataSet.Tables.Count > 0)
                {
                    enviarSRI = new enviarSRI();
                    foreach (DataRow dr in dataSet.Tables[0].Rows)
                    {
                        log.guardar_Log("procesando idDocumento " + dr["idComprobante"].ToString());
                        enviarSRI.consultarDocumentoOffline(
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
            catch (Exception ex)
            {
                log.guardar_Log("error " + ex.ToString());
            }

        }

    }
}
