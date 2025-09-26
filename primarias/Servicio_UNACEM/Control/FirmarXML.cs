using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using java.security;
using java.io;
using java.util;
using java.security.cert;
using javax.xml.parsers;
using es.mityc.javasign.pkstore;
using es.mityc.javasign.pkstore.keystore;
using es.mityc.javasign.trust;
using es.mityc.javasign.xml.xades.policy;
using es.mityc.firmaJava.libreria.xades;
using es.mityc.javasign.xml.refs;
using es.mityc.firmaJava.libreria.utilidades;
using org.w3c.dom;
using sviudes.blogspot.com; 
using System.IO;

namespace Control
{

    public class FirmarXML
    {

        //public Boolean Firmar(string rutaCER, string password, string rutaXML, string rutaXMLFirmado)
        public Boolean Firmar(string rutaCER, string password, XmlDocument rutaXML, out string rutaXMLFirmado)
        {
            rutaXMLFirmado = null;
            Boolean rpt = false;
            PrivateKey privateKey;
            Provider provider;
            X509Certificate certificate = LoadCertificate(rutaCER, password, out privateKey, out provider);

            //Si encontramos el certificado
            if (certificate != null)
            {
                //Política de firma (Con las librerías JAVA, esto se define en tiempo de ejecución)
                //TrustFactory.instance = es.mityc.javasign.trust.TrustExtendFactory.newInstance();
                //TrustFactory.truster = es.mityc.javasign.trust.MyPropsTruster.getInstance();
                // PoliciesManager.POLICY_SIGN = new es.mityc.javasign.xml.xades.policy.facturae.Facturae31Manager();
                //PoliciesManager.POLICY_VALIDATION = new es.mityc.javasign.xml.xades.policy.facturae.Facturae31Manager();

                //Crear datos a firmar

                ObjectToSign objtoSign = new ObjectToSign(new InternObjectToSign("comprobante"), "CIMA IT, Tel. +593 (4) 2280217", null, "text/xml", null);
                objtoSign.setId("#comprobante");
                DataToSign dataToSign = new DataToSign();
                dataToSign.setXadesFormat(EnumFormatoFirma.XAdES_BES); //XAdES-EPES  
                dataToSign.setEsquema(XAdESSchemas.XAdES_132);
                //dataToSign.setPolicyKey("FE Ecuador"); //Da igual lo que pongamos aquí, la política de firma se define arriba  
                dataToSign.setAddPolicy(false);
                dataToSign.setXMLEncoding("UTF-8");
                dataToSign.setBaseURI("#comprobante");
                dataToSign.setEnveloped(true);
                dataToSign.addObject(objtoSign);
                //dataToSign.setDocument(LoadXML(rutaXML));
                dataToSign.setDocument(LoadXML(rutaXML));

                //Firmar
                Object[] res = new FirmaXML().signFile(certificate, dataToSign, privateKey, provider);
                // Guardamos la firma a un fichero en el home del usuario
                //FileOutputStream fosXML;
                //UtilidadTratarNodo.saveDocumentToOutputStream((Document)res[0], fosXML = new FileOutputStream(rutaXMLFirmado), true);
                XmlDocument doc2 = new XmlDocument();

                ByteArrayOutputStream fosXML;// = new OutputStream();// new OutputStream();
                fosXML = new ByteArrayOutputStream();
                UtilidadTratarNodo.saveDocumentToOutputStream((Document)res[0], fosXML, true);
                rutaXMLFirmado = fosXML.toString();
                fosXML.close();

                rpt = true;
            }
            else
            {
                rpt  = false;
            }
            return rpt;
        }

        private X509Certificate LoadCertificate(string path, string password, out PrivateKey privateKey, out Provider provider)
        {
            X509Certificate certificate = null;
            X509Certificate certificate2 = null;
            provider = null;
            privateKey = null;
            try
            {
                //Cargar certificado de fichero PFX

                KeyStore ks = KeyStore.getInstance("PKCS12");
                //log1.guardar_Log("creando un PKCS12 Metodo LoadCertificado() FirmarXML.cs");
                ks.load(new BufferedInputStream(new FileInputStream(path)), password.ToCharArray());
                //log1.guardar_Log("cargando el contenido del archivo xml Metodo LoadCertificado() FirmarXML.cs");
                IPKStoreManager storeManager = new KSStore(ks, new PassStoreKS(password));
                //log1.guardar_Log("verificando la firma Metodo LoadCertificado() FirmarXML.cs");
                List certificates = storeManager.getSignCertificates();
                //log1.guardar_Log("creando una lista de ceritficado Metodo LoadCertificado() FirmarXML.cs");
                //Si encontramos el certificado...
                // if (certificates.size() == 1)
                // {
                //certificate = (X509Certificate)certificates.get(0);
                for (int indice = 0; indice < certificates.size(); indice++)
                {
                    certificate2 = (X509Certificate)certificates.get(indice);
                    var keyUsage = certificate2.getKeyUsage();
                    if (keyUsage[0].Equals(true)) //0 digital Signature      2 KeyEnchipermet
                    {
                        certificate = (X509Certificate)certificates.get(indice);
                    }
                }
                //log1.guardar_Log("recigiendo certificado Metodo LoadCertificado() FirmarXML.cs");
                // Obtención de la clave privada asociada al certificado
                //log1.guardar_Log("creando un privateKey Metodo LoadCertificado() FirmarXML.cs");
                privateKey = storeManager.getPrivateKey(certificate);
                // Obtención del provider encargado de las labores criptográficas
                //log1.guardar_Log("creando un provider Metodo LoadCertificado() FirmarXML.cs");
                provider = storeManager.getProvider(certificate);
                //}
            }
            catch (Exception ex)
            {
                //log1.mensajesLog("EM011", "Error en la firma electrónica", "Problemas con la contraseña de la firma electrónica", ex.Message, codigoControl, "");
            }
            return certificate;
        }

        private Document LoadXML(XmlDocument path)
        {            
            DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
            dbf.setNamespaceAware(true);
            //return dbf.newDocumentBuilder().parse(new BufferedInputStream(new ByteArrayInputStream(Encoding.UTF8.GetBytes(path))));
            //string s =""; s.
            MemoryStream ms = new MemoryStream();
            path.Save(ms);
            //byte[] bytes = ms.ToArray();

            return dbf.newDocumentBuilder().parse(new ByteArrayInputStream(ms.ToArray()));
            //return dbf.newDocumentBuilder().parse(new BufferedInputStream(new FileInputStream(xs.InnerXml)));
        }

    }
}
