using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace CriptoSimetrica
{
    public class AES
    {
        private RijndaelManaged rij = new RijndaelManaged();
        public string encriptar(string cadena, string clave)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(cadena);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(clave);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] saltBytes = new byte[] { 2, 1, 7, 3, 6, 4, 8, 5 };
            byte[] iv = rij.IV;
            rij.Mode = CipherMode.ECB;
            rij.KeySize = 256;
            rij.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
            rij.Key = key.GetBytes(rij.KeySize / 8);
            rij.IV = key.GetBytes(rij.BlockSize / 8);
            ICryptoTransform encriptador;
            encriptador = rij.CreateEncryptor(passwordBytes, iv);
            MemoryStream memStream = new MemoryStream();
            CryptoStream cifradoStream;
            cifradoStream = new CryptoStream(memStream, encriptador, CryptoStreamMode.Write);
            cifradoStream.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
            cifradoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memStream.ToArray();
            memStream.Close();
            cifradoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }
        public string desencriptar(string cadena, string clave)
        {
            byte[] bytesToBeDecrypted = Convert.FromBase64String(cadena);
            byte[] passwordBytesdecrypt = Encoding.UTF8.GetBytes(clave);
            passwordBytesdecrypt = SHA256.Create().ComputeHash(passwordBytesdecrypt);
            byte[] saltBytes = new byte[] { 2, 1, 7, 3, 6, 4, 8, 5 };
            byte[] iv = rij.IV;
            rij.Mode = CipherMode.ECB;
            rij.KeySize = 256;
            rij.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytesdecrypt, saltBytes, 1000);
            rij.Key = key.GetBytes(rij.KeySize / 8);
            rij.IV = key.GetBytes(rij.BlockSize / 8);
            ICryptoTransform desencriptador;
            desencriptador = rij.CreateDecryptor(passwordBytesdecrypt, iv);
            MemoryStream memStream = new MemoryStream(bytesToBeDecrypted);
            CryptoStream cifradoStream;
            cifradoStream = new CryptoStream(memStream, desencriptador, CryptoStreamMode.Read);
            StreamReader lectorStream = new StreamReader(cifradoStream);
            string resultado = Convert.ToString(lectorStream.ReadToEnd());
            memStream.Close();
            cifradoStream.Close();
            return resultado;
        }


    }
}