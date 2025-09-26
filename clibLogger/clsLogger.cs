using System;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml;
using System.Collections.Generic;
using System.Data.Common;
using System.Web;

namespace clibLogger
{
    public class clsLogger
    {
        
        public static void Graba_Log_Warn(string PI_Texto, dynamic Objeto = null,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string filePath = "",
                               [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                if (System.Configuration.ConfigurationManager.AppSettings["LogInfo"].Equals("S"))
                    Graba_LogInterno(PI_Texto, "WARN", Objeto, memberName, filePath, lineNumber);
            }
            catch (Exception)
            {
            }
        }

        public static void Graba_Log_Fatal(string PI_Texto, dynamic Objeto = null,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string filePath = "",
                               [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                if (System.Configuration.ConfigurationManager.AppSettings["LogError"].Equals("S"))
                    Graba_LogInterno(PI_Texto, "FATAL", Objeto, memberName, filePath, lineNumber);
            }
            catch (Exception)
            {
            }
        }

        public static void Graba_Log_Debug(string PI_Texto, dynamic Objeto = null, string PI_Identificador = "0",
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string filePath = "",
                               [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                if (System.Configuration.ConfigurationManager.AppSettings["LogInfo"].Equals("S"))
                    Graba_LogInterno(PI_Texto, "DEBUG", Objeto, memberName, filePath, lineNumber);
            }
            catch (Exception)
            {
            }
        }

        public static void Graba_Log_Info(string PI_Texto, dynamic Objeto = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                if (System.Configuration.ConfigurationManager.AppSettings["LogInfo"].Equals("S"))
                    Graba_LogInterno(PI_Texto, "INFO", Objeto, memberName, filePath, lineNumber);
            }
            catch (Exception)
            {
            }
        }


        public static void Graba_Log_Error(string PI_Texto, dynamic Objeto = null,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string filePath = "",
                               [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                if (System.Configuration.ConfigurationManager.AppSettings["LogError"].Equals("S"))
                    Graba_LogInterno(PI_Texto, "ERROR", Objeto, memberName, filePath, lineNumber);
            }
            catch (Exception)
            {
            }
        }

        public static void Graba_Log(string Datos, string Tipo, dynamic Objeto = null,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string filePath = "",
                               [CallerLineNumber] int lineNumber = 0)
        {
            bool GrabaLog = true;
            try
            {
                string config = System.Configuration.ConfigurationManager.AppSettings[$"Log{Tipo}"];
                if (!(config is null))
                {
                    GrabaLog = config.Equals("S");
                }
            }
            catch (Exception)
            {
            }

            if (GrabaLog)
            {
                Graba_LogInterno(Datos, Tipo.ToUpper(), Objeto, memberName, filePath, lineNumber);
            }
        }

        public static XNode ConvertJsonToXml(string json)
        {
            string rootElementName = "Objeto";
            json = json.Trim();

            if (json.StartsWith("{"))
            {
                return JsonConvert.DeserializeXNode(json, rootElementName);
            }
            else if (json.StartsWith("["))
            {
                string adjustedJson = $"{{\"{rootElementName}\": {json}}}";
                return JsonConvert.DeserializeXNode(adjustedJson, "Objetos");
            }
            else
            {
                throw new ArgumentException("El JSON proporcionado no es válido. Debe comenzar con un objeto o un arreglo.");
            }
        }
        private static string SerializaObjeto(dynamic Objeto)
        {



            string ObjetoTexto = "";

            if (Objeto is DbParameterCollection)
            {
                var parametrosSimples = new List<object>();

                try
                {

                    foreach (DbParameter parametro in Objeto)
                    {
                        var parametroSimple = new
                        {
                            parametro.ParameterName,
                            parametro.Value,
                            Tipo = parametro.DbType.ToString()
                        };

                        parametrosSimples.Add(parametroSimple);
                    }
                    Objeto = parametrosSimples;
                }
                catch (Exception)
                {

                }
            }
            



            if (Objeto != null)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(Objeto, Newtonsoft.Json.Formatting.None);

                    //XNode nodoXml = JsonConvert.DeserializeXNode(json, "Objeto");
                    XNode nodoXml = ConvertJsonToXml(json);
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = false, // No indentar
                        NewLineHandling = NewLineHandling.None, // No agregar nuevas líneas
                        OmitXmlDeclaration = true // Omitir la declaración de XML si es necesario
                    };
                    using (StringWriter textWriter = new StringWriter())
                    {
                        using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                        {
                            nodoXml.WriteTo(xmlWriter);
                        }
                        ObjetoTexto = HttpUtility.HtmlDecode(textWriter.ToString());
                    }
                }
                catch (Exception)
                {
                   
                    ObjetoTexto = "Logger: Error al serializar el Objeto para su presentación en el log.";
                }
            }

            return ObjetoTexto;
        }

        private static void Graba_LogInterno(string Datos, string Tipo, dynamic Objeto = null, string memberName = "", string filePath = "", int lineNumber = 0)
        {
            
            string ObjetoTexto = SerializaObjeto(Objeto);
            string LogInfoAdicional = "N";
            try
            {

                if (ConfigurationManager.AppSettings["Auditar"].Equals("S"))
                {
                    string NombreArchivo = ConfigurationManager.AppSettings["ArchivoLog"];
                    try
                    {
                        LogInfoAdicional = ConfigurationManager.AppSettings["LogInfoAdicional"];
                    }
                    catch (Exception) { }

                    string logAdicional = "";
                    string fileName = "";
                    string fileNameNE = "";
                    if (LogInfoAdicional == "S")
                    {
                        fileName = Path.GetFileName(filePath);
                        fileNameNE = Path.GetFileNameWithoutExtension(fileName);
                        logAdicional = $"<InfoAdicional>Archivo: {fileName} | Metodo: {memberName} | Linea: {lineNumber}</InfoAdicional>";
                    }


                    NombreArchivo = NombreArchivo.Replace("|dd", DateTime.Now.ToString("dd"));
                    NombreArchivo = NombreArchivo.Replace("|MM", DateTime.Now.ToString("MM"));
                    NombreArchivo = NombreArchivo.Replace("|yyyy", DateTime.Now.ToString("yyyy"));
                    NombreArchivo = NombreArchivo.Replace("|HH", DateTime.Now.ToString("HH"));
                    NombreArchivo = NombreArchivo.Replace("|IA", fileNameNE);
                    NombreArchivo = NombreArchivo.Replace("|TP", Tipo);


                    DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(NombreArchivo));
                    string Archivo = Path.Combine(dir.FullName, NombreArchivo);

                    if (!(dir.Exists))
                    {
                        dir.Create();
                    }
                    using (Process procesoActual = Process.GetCurrentProcess())
                    {
                        using (FileStream objStream = new FileStream(Archivo, FileMode.Append, FileAccess.Write))
                        {
                            using (TextWriterTraceListener objTraceListener = new TextWriterTraceListener(objStream))
                            {
                                Trace.Listeners.Add(objTraceListener);
                                Trace.WriteLine($"{DateTime.Now:yyyy-MM-dd-HH:mm:ss:fff} {Tipo} |Proceso: {procesoActual.Id}| {Datos} {logAdicional} {ObjetoTexto}");
                                Trace.Flush();
                                Trace.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception){}
        }
    }
}
