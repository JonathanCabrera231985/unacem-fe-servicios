using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Linq;
using Datos;

namespace consultaAutorizacion
{
    public class Proceso
    {
        BasesDatos DB;
        Validar validar;
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
                                new XElement("OPCION", "2"))));
                dataSet = DB.TraerDataset("sp_consultaInfoProcesoAu", CRE.ToString());
                if (dataSet.Tables.Count > 0)
                {
                    validar = new Validar();
                    foreach (DataRow dataRow in dataSet.Tables[0].Rows)
                    {
                        log.guardar_Log("procesando idDocumento " + dataRow["idComprobante"].ToString());
                        validar.validarAutorizacion(dataRow["claveAcceso"].ToString(), dataRow["ambiente"].ToString(), dataRow["codigoControl"].ToString(), dataRow["RutaXMLbase"].ToString(), dataRow["RutaDoc"].ToString());
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
