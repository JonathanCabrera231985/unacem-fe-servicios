using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Control
{
    public class ValidaRUC
    {
        public bool ValidarDigitos(string numero)
        {
            int numeroProvincias = 24;
            int n1 = int.Parse(numero.Substring(0, 1)) * 10 + int.Parse(numero.Substring(1, 1)); //numero de provincvia 1 - 24

            if (n1 == 0 || n1 > numeroProvincias)                                         //si no coincide con un # de provincia retorna false
                return false;
            else if (numero.Length == 13)
            {
                if ((numero.Substring(10, 3) == "000") || (numero.Substring(9, 4) == "0000")) //el RUC para Juridicos no puenden ser 000
                    return false;                                                            //el RUC para Instituciones Públicas no puede ser 0000
            }
            else if ((int.Parse(numero.Substring(2, 1)) == 7) || (int.Parse(numero.Substring(2, 1)) == 8)) //el tercer dígito de la indentificación no puede ser 7 ni 8
                return false;

            return true;
        }

        public bool ValidarNumeroIdentificacion(string numero)
        {
            List<int> id = new List<int>();

            for (int i = 0; i < 10; i++)
                id.Add(int.Parse(numero.Substring(i, 1)));  //Llenando la lista de números en id

            #region "Validación del # de Identificación"
            if (ValidarDigitos(numero))      //Validación previa del número
            {
                if ((numero.Length == 10))  //Cédula
                {
                    return ValidarCedula(id);  //Validando el # número de cédula
                }
                else if (numero.Length == 13) //RUC
                {
                    if (int.Parse(numero.Substring(2, 1)) < 6)  //Validando el # de Ruc para Entidad Natural
                        return ValidarCedula(id);
                    else
                        return ValidarRuc(id);  //Validando el # de Ruc para Entidad Jurídica y Públicas
                }
            }
            else
                return false;
            #endregion

            return true;
        }

        #region "Validación del Ruc para Entidades Jurídicas y Públicas"
        private bool ValidarRuc(List<int> ident)
        {
            int suma = 0;
            int residuo;
            int digitoVerificador = 0;      //si el residuo del módulo es 0 el dígito verificador es 0

            #region "Coeficientes  4 3 2 7 6 5 4 3 2"
            List<int> coeficiente = new List<int>();
            coeficiente.Add(4);
            coeficiente.Add(3);
            coeficiente.Add(2);
            coeficiente.Add(7);
            coeficiente.Add(6);
            coeficiente.Add(5);
            coeficiente.Add(4);
            coeficiente.Add(3);
            coeficiente.Add(2);
            #endregion

            int modulo = 11; //Módulo 11 para RUC Jurídico y Público

            #region "Proceso del Algoritmo"
            if (ident[2] == 9)      //Entidad Jurídica
            {
                for (int i = 0; i < 9; i++)                   //se multiplica solo los 9 primeros dígitos
                    ident[i] = ident[i] * coeficiente[i];  //multiplicación de cada digito por su respectivo coeficiente
            }
            else if (ident[2] == 6)             //Entidad Pública    
            {
                for (int i = 0; i < 8; i++)        //se multiplica solo los 9 primeros dígitos
                {
                    ident[i] = ident[i] * coeficiente[i + 1];  //multiplicación de cada digito por su respectivo coeficiente
                    if (i == 9)                 //para este caso  solo se toman encuenta los 8 primeros 
                        ident[i] = 0;
                }
            }

            for (int i = 0; i < 9; i++)             //suma de los valores que resultaron de la multiplicación
                suma = suma + ident[i];

            residuo = suma % modulo;       //se calcula el módulo en este caso 11

            if (residuo != 0)        //si el residuo del módulo no es 0 se calcula el digito verificador
                digitoVerificador = modulo - residuo;
            #endregion

            #region "Verificación"
            if (digitoVerificador == ident[9])  //si el dígito verificador es igual al décimo dígito del 
                return true;                    //número de identificación, el # es correcto (true)
            else                                //caso contrario retornamos false
                return false;
            #endregion
        }
        #endregion

        #region "Validación de la Cédula y del RUC para Entidad Natural"
        private bool ValidarCedula(List<int> ident)
        {
            int suma = 0;
            int residuo;
            int digitoVerificador = 0; //si el residuo del módulo es 0 el dígito verificador es 0

            #region "Coeficientes  2 1 2 1 2 1 2 1 2"
            List<int> coeficiente = new List<int>();
            coeficiente.Add(2);
            coeficiente.Add(1);
            coeficiente.Add(2);
            coeficiente.Add(1);
            coeficiente.Add(2);
            coeficiente.Add(1);
            coeficiente.Add(2);
            coeficiente.Add(1);
            coeficiente.Add(2);
            #endregion

            int modulo = 10; //Módulo 10 para Cédula y RUC Natural

            #region "Proceso del Algoritmo"
            for (int i = 0; i < 9; i++)        //se multiplica solo los 9 primeros dígitos
            {
                ident[i] = ident[i] * coeficiente[i];

                if (ident[i] >= 10)         //Si el producto es >= 10 deben sumarse sus dígitos 
                    ident[i] = ident[i] - 9; //14 = 1 + 4 = 5 (14-9 = 5) 
            }

            for (int i = 0; i < 9; i++)      //suma de los valores que resultaron de la multiplicación
                suma = suma + ident[i];  //y descomposición

            residuo = suma % modulo; //se cálcula el módulo en este caso 10

            if (residuo != 0)        //si el residuo del módulo no es 0 se calcula el digito verificador
                digitoVerificador = modulo - residuo;
            #endregion

            #region "Verificación"
            if (digitoVerificador == ident[9]) //si el dígito verificador es igual al décimo dígito del
                return true;                   //número de identificación, el # es correcto (true)
            else                               //caso contrario retornamos false
                return false;
            #endregion
        }
        #endregion
    }
}
