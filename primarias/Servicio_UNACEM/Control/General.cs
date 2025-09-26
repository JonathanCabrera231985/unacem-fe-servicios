using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using ValSign;

namespace Control
{
    public class General
    {
        public string obtenerXSD(string p_codDoc, string p_version)
        {
            string archivo_xsd = "";

            switch (p_version)
            {
                case "1.0.0":
                case "1.0":
                    switch (p_codDoc)
                    { 
                        case "01":
                            archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\1_0_0\factura.xsd";
                            break;
                        case "04":
                            archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\1_0_0\notaCredito.xsd";
                            break;
                        case "05":
                            archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\1_0_0\notaDebito.xsd";
                            break;
                        case "06":
                            archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\1_0_0\guiaRemision.xsd";
                            break;
                        case "07":
                            archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\1_0_0\comprobanteRetencion.xsd";
                            break;
                    }
                    break;

                case "1.1.0":
                    switch (p_codDoc)
                    {
                        case "01":
                            archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\1_1_0\factura.xsd";
                            break;
                        case "04":
                            archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\1_1_0\notaCredito.xsd";
                            break;
                        case "05":
                            archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\1_0_0\notaDebito.xsd";
                            break;
                        case "06":
                            archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\1_1_0\guiaRemision.xsd";
                            break;
                        case "07":
                            archivo_xsd = AppDomain.CurrentDomain.BaseDirectory + @"xsdCima\1_0_0\comprobanteRetencion.xsd";
                            break;
                    }
                    break;
            }
            return archivo_xsd;
        }

        public Boolean ValidarEstructuraXSD(string p_doc, string p_codDoc, string p_version, out string p_msj, out string p_msjT)
        {
            XmlTextReader lector;
            Boolean b = false;
            p_msj = ""; p_msjT = "";
            string xsd;

            try
            {
                lector = new XmlTextReader(new StringReader(p_doc));
                lector.Read();

                ValidacionEstructura VE = new ValidacionEstructura();
                xsd = obtenerXSD(p_codDoc, p_version);

                if (!String.IsNullOrEmpty(xsd))
                {
                    VE.agregarSchemas(xsd);
                    if (VE.Validar(lector))
                    {
                        p_msj += VE.msj;
                        p_msjT += VE.msjT;
                        b = true;
                    }
                    else
                    {
                        p_msj += VE.msj;
                        p_msjT += VE.msjT;
                    }
                }
                else
                {
                    p_msj += "Parámetro versión o codDoc incorrectos";
                }
            }
            catch (Exception ex)
            {
                p_msj += "Archivo no superó la validación";
                p_msjT += ex.Message;
            }
            return b;
        }

        public string VerificaAcentos(string strCadena)
        {

            strCadena = strCadena.Replace("á", "a");
            strCadena = strCadena.Replace("é", "e");
            strCadena = strCadena.Replace("í", "i");
            strCadena = strCadena.Replace("ó", "o");
            strCadena = strCadena.Replace("ú", "u");
            strCadena = strCadena.Replace("ñ", "n");
            strCadena = strCadena.Replace("Ï­-­­­", "ni");
            strCadena = strCadena.Replace("Ï­", "ni");
            strCadena = strCadena.Replace("Ý", "i");
            strCadena = strCadena.Replace("Ð", "n");

            strCadena = strCadena.Replace("Á", "A");
            strCadena = strCadena.Replace("É", "E");
            strCadena = strCadena.Replace("Í", "I");
            strCadena = strCadena.Replace("Ó", "O");
            strCadena = strCadena.Replace("Ú", "U");
            strCadena = strCadena.Replace("Ñ", "N");


            return strCadena;
        }

        public string lee_nodo_xml(XmlNode p_root, string p_tag)
        {
            string retorno = "";
            if (p_root.SelectSingleNode("descendant::" + p_tag) != null)
            {
                retorno = p_root.SelectSingleNode("descendant::" + p_tag).InnerText;
            }
            return retorno;
        }

        public string lee_atributo_nodo_xml(XmlNode p_root, string p_tag)
        {
            string retorno = "";
            if (p_root.Attributes != null)
            {
                var nameAttribute = p_root.Attributes[p_tag];
                if (nameAttribute != null)
                    retorno = nameAttribute.Value;
            }

            return retorno;
        }

        public string completaSecuencial(string folio)
        {
            string ultimoFolio = "";
            if (String.IsNullOrEmpty(folio))
            {
                ultimoFolio = "1";
            }
            else
            {
                ultimoFolio = folio;
            }

            string code = "";

            switch (ultimoFolio.ToString().Trim().Length)
            {
                case 1:
                    code = "00000000" + ultimoFolio.ToString().Trim();
                    break;
                case 2:
                    code = "0000000" + ultimoFolio.ToString().Trim();
                    break;
                case 3:
                    code = "000000" + ultimoFolio.ToString().Trim();
                    break;
                case 4:
                    code = "00000" + ultimoFolio.ToString().Trim();
                    break;
                case 5:
                    code = "0000" + ultimoFolio.ToString().Trim();
                    break;
                case 6:
                    code = "000" + ultimoFolio.ToString().Trim();
                    break;
                case 7:
                    code = "00" + ultimoFolio.ToString().Trim();
                    break;
                case 8:
                    code = "0" + ultimoFolio.ToString().Trim();
                    break;
                case 9:
                    code = ultimoFolio.ToString().Trim();
                    break;
            }

            return code;
        }

    }
}
