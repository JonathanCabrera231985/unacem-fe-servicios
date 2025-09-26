using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class BasesDatos
    {
        private DbConnection conexion = null;
        public DbCommand comando = null;
        private DbTransaction transaccion = null;
        private string cadenaConexion;
        private static DbProviderFactory factory = null;
        private static object sync;
        /// <summary>
        /// Crea una instancia del acceso a la base de datos.
        /// </summary>
        public BasesDatos()
        {
            Configurar();
            BasesDatos.sync = new object();
        }
        /// <summary>
        /// Configura el acceso a la base de datos para su utilización.
        /// </summary>
        /// <exception cref="BaseDatosException">Si existe un error al cargar la configuración.</exception>
        public void Configurar()
        {
            try
            {
                string proveedor = ConfigurationManager.AppSettings.Get("PROVEEDOR_ADONET");
                this.cadenaConexion = ConfigurationManager.AppSettings.Get("CADENA_CONEXION");
                BasesDatos.factory = DbProviderFactories.GetFactory(proveedor);
            }
            catch (ConfigurationException ex)
            {
                throw new BaseDatosException("Error al cargar la configuración del acceso a datos.", ex);
            }
        }
        public void Configurar_pg()
        {
            try
            {
                string proveedor = ConfigurationManager.AppSettings.Get("PROVEEDOR_POSTGRES");
                this.cadenaConexion = ConfigurationManager.AppSettings.Get("CADENA_POSTGRES");
                BasesDatos.factory = DbProviderFactories.GetFactory(proveedor);

            }
            catch (ConfigurationException ex)
            {
                throw new BaseDatosException("Error al cargar la configuración del acceso a datos.", ex);
            }
        }

        public void Configurar(string p_proveedor, string p_cadena)
        {
            try
            {
                string proveedor = ConfigurationManager.AppSettings.Get(p_proveedor);
                this.cadenaConexion = ConfigurationManager.AppSettings.Get(p_cadena);
                BasesDatos.factory = DbProviderFactories.GetFactory(proveedor);

            }
            catch (ConfigurationException ex)
            {
                throw new BaseDatosException("Error al cargar la configuración del acceso a datos.", ex);
            }
        }
        /// <summary>
        /// Permite desconectarse de la base de datos.
        /// </summary>
        public void Desconectar()
        {
            if (this.conexion != null)
            {
                if (this.conexion.State.Equals(ConnectionState.Open))
                {
                    this.conexion.Close();
                    this.conexion = null;
                }
            }
        }


        /// <summary>
        /// Se concecta con la base de datos.
        /// </summary>
        /// <exception cref="BaseDatosException">Si existe un error al conectarse.</exception>
        public void Conectar()
        {
            if (this.conexion != null && !this.conexion.State.Equals(ConnectionState.Closed))
            {
                throw new BaseDatosException("La conexión ya se encuentra abierta.");
            }
            try
            {
                if (this.conexion == null)
                {
                    this.conexion = factory.CreateConnection();
                    this.conexion.ConnectionString = cadenaConexion;
                }
                this.conexion.Open();
            }
            catch (DataException ex)
            {
                throw new BaseDatosException("Error al conectarse a la base de datos.", ex);
            }
        }
        /// <summary>
        /// Crea un comando en base a una sentencia SQL.
        /// Ejemplo:
        /// <code>SELECT * FROM Tabla WHERE campo1=@campo1, campo2=@campo2</code>
        /// Guarda el comando para el seteo de parámetros y la posterior ejecución.
        /// </summary>
        /// <param name="sentenciaSQL">La sentencia SQL con el formato: SENTENCIA [param = @param,]</param>
        public void CrearComando(string sentenciaSQL)
        {
            this.comando = factory.CreateCommand();
            this.comando.Connection = this.conexion;
            this.comando.CommandType = CommandType.Text;
            this.comando.CommandText = sentenciaSQL;
            if (this.transaccion != null)
            {
                this.comando.Transaction = this.transaccion;
            }
        }

        public void CrearComandoProcedimiento(string sentenciaSQL)
        {
            this.comando = factory.CreateCommand();
            this.comando.Connection = this.conexion;
            this.comando.CommandType = System.Data.CommandType.StoredProcedure;
            this.comando.CommandText = sentenciaSQL;
            if (this.transaccion != null)
            {
                this.comando.Transaction = this.transaccion;
            }
        }


        public void AsignarParametroProcedimiento(string nombre, DbType tipo, object valor)
        {
            DbParameter param = comando.CreateParameter();
            param.ParameterName = nombre;
            param.DbType = tipo;
            param.Value = valor;
            this.comando.Parameters.Add(param);


        }        /// <summary>
        public void AsignarParametroProcedimiento(string nombre, DbType tipo, int size, Boolean salida)
        {
            DbParameter param = comando.CreateParameter();
            param.ParameterName = nombre;
            param.DbType = tipo;
            //param.Value = valor;
            param.Size = size;
            if (salida) param.Direction = ParameterDirection.Output;
            else param.Direction = ParameterDirection.Input;
            this.comando.Parameters.Add(param);

        }        /// <summary>

        public object devolverParametroProcedimiento(string nombre)
        {
            Object value;
            value = this.comando.Parameters[nombre].Value;
            return value;
        }

        /// Asigna un parámetro de tipo entero al comando creado.
        /// </summary>
        /// <param name="nombre">El nombre del parámetro.</param>
        /// <param name="valor">El valor del parámetro.</param>
        public void AsignarParametroFlotante(string nombre, string valor)
        {
            if (valor == " ") { valor = "0"; }
            AsignarParametro(nombre, "", valor.ToString());
        }
        /// <summary>
        /// Asigna un parámetro de tipo entero al comando creado.
        /// </summary>
        /// <param name="nombre">El nombre del parámetro.</param>
        /// <param name="valor">El valor del parámetro.</param>
        public void AsignarParametroEntero(string nombre, int valor)
        {

            AsignarParametro(nombre, "", valor.ToString());
        }

        public void AsignarParametroCadenaEntero(string nombre, string valor)
        {

            AsignarParametro(nombre, "", valor);
        }
        /// <summary>
        /// Asigna un parámetro de tipo cadena al comando creado.
        /// </summary>
        /// <param name="nombre">El nombre del parámetro.</param>
        /// <param name="valor">El valor del parámetro.</param>
        public void AsignarParametroCadena(string nombre, string valor)
        {
            AsignarParametro(nombre, "'", valor);
        }
        /// <summary>
        /// Asigna un parámetro de tipo fecha al comando creado.
        /// </summary>
        /// <param name="nombre">El nombre del parámetro.</param>
        /// <param name="valor">El valor del parámetro.</param>
        public void AsignarParametroFecha(string nombre, string valor)
        {
            AsignarParametro(nombre, "'", valor.ToString());
        }
        /// <summary>
        /// Asigna un parámetro al comando creado.
        /// </summary>
        /// <param name="nombre">El nombre del parámetro.</param>
        /// <param name="separador">El separador que será agregado al valor del parámetro.</param>
        /// <param name="valor">El valor del parámetro.</param>
        private void AsignarParametro(string nombre, string separador, string valor)
        {
            int indice = this.comando.CommandText.IndexOf(nombre);
            string prefijo = this.comando.CommandText.Substring(0, indice);
            string sufijo = this.comando.CommandText.Substring(indice + nombre.Length);
            this.comando.CommandText = prefijo + separador + valor + separador + sufijo;
        }
        //        Dim da As New SqlDataAdapter("Select * From Compras ", cnMySql)
        //Dim ds As New DataSet
        //da.Fill(ds)
        //DataGridView1.DataSource = ds.Tables(0) DataTable4
        public DataSet DS()
        {
            DbCommand command = factory.CreateCommand();
            command.CommandText = "select * from ventatmp";
            Conectar();
            command.Connection = conexion;
            DbDataAdapter da = factory.CreateDataAdapter();
            da.SelectCommand = command;
            DataSet table = new DataSet();
            da.Fill(table, "ventatmp");
            return table;
        }
        /// <summary>
        /// Ejecuta el comando creado y retorna el resultado de la consulta.
        /// </summary>
        /// <returns>El resultado de la consulta.</returns>
        /// <exception cref="BaseDatosException">Si ocurre un error al ejecutar el comando.</exception>
        public DbDataReader EjecutarConsulta()
        {
            lock (BasesDatos.sync)
            {
                return this.comando.ExecuteReader();
            }
        }

        public void EjecutarConsulta2(ref string men)
        {
            lock (BasesDatos.sync)
            {
                try
                {
                    this.comando.ExecuteReader();
                    //this.comando.ExecuteNonQuery();
                    men = "";
                }
                //catch (Exception)
                catch (SqlException ex)
                {
                    SqlError err = ex.Errors[0];
                    string mensaje = string.Empty;
                    mensaje = err.ToString();
                    men = mensaje;
                }
            }
        }

        public DbDataReader EjecutarConsulta3(ref string men)
        {
            lock (BasesDatos.sync)
            {
                try
                {
                    return this.comando.ExecuteReader();
                    //this.comando.ExecuteNonQuery();
                    men = "";
                }
                //catch (Exception)
                catch (SqlException ex)
                {
                    SqlError err = ex.Errors[0];
                    string mensaje = string.Empty;
                    mensaje = err.ToString();
                    men = mensaje;

                    return null;
                }
            }
        }
        /*  public DataTable EjecutarConsultaA()
          {
             return this.comando.ExecuteReader();
          }*/
        /// <summary>
        /// Ejecuta el comando creado .
        /// </summary>
        /// <exception cref="BaseDatosException">Si ocurre un error al ejecutar el comando.</exception>
        public void EjecutarConsulta1()
        {
            lock (BasesDatos.sync)
            {
                this.comando.ExecuteReader();
            }
        }

        public int ActualizarOracle(String Query)
        {

            return 1;
        }

        #region "Coneccion retornando datatable"
        protected System.Data.IDbTransaction mTransaccion;

        /// <summary>
        /// retorna un select de una tabla especificado
        /// </summary>
        /// <param name="ComandoSelect"></param>
        /// <param name="Args"></param>
        /// <returns></returns>
        public DataSet TraerDataSetConsulta(String ComandoSelect, params System.Object[] Args)
        {
            lock (BasesDatos.sync)
            {
                DataSet mDataASet = new DataSet();
                mTransaccion = this.conexion.BeginTransaction(System.Data.IsolationLevel.Serializable);
                System.Data.IDataAdapter ida = this.CrearDataAdapterSelect(ComandoSelect, Args);
                ida.Fill(mDataASet);
                mTransaccion.Commit();
                Desconectar();
                return mDataASet;
            }
        }

        /// <summary>
        /// Obtiene un DataSet a partir de un Procedimiento Almacenado y sus parámetros.
        /// </summary>
        public System.Data.DataSet TraerDataset(string ProcedimientoAlmacenado, params System.Object[] Args)
        {
            lock (BasesDatos.sync)
            {
                System.Data.DataSet mDataSet = new System.Data.DataSet();
                mTransaccion = this.conexion.BeginTransaction(System.Data.IsolationLevel.Serializable);
                System.Data.IDataAdapter ida = this.CrearDataAdapter(ProcedimientoAlmacenado, Args);
                ida.Fill(mDataSet);
                mTransaccion.Commit();
                Desconectar();
                return mDataSet;
            }
        }

        /// <summary>
        /// Obtiene un IDataAdapter
        /// ProcedimientoAlmacenado = nombre del procedimeinto almacenado que se encuentra en la base de datos
        /// Args = los parametros que se encuentra en la base de datos
        /// </summary>
        protected System.Data.IDataAdapter CrearDataAdapter(string ProcedimientoAlmacenado, params System.Object[] Args)
        {
            System.Data.SqlClient.SqlDataAdapter Da = new System.Data.SqlClient.SqlDataAdapter((System.Data.SqlClient.SqlCommand)Comandos(ProcedimientoAlmacenado));
            if (Args.Length != 0)
                CargarParametros(Da.SelectCommand, Args);
            return (System.Data.IDataAdapter)Da;
        }

        protected System.Data.IDataAdapter CrearDataAdapterSelect(string SecuenciaSelect, params System.Object[] Args)
        {
            System.Data.SqlClient.SqlDataAdapter Da = new System.Data.SqlClient.SqlDataAdapter((System.Data.SqlClient.SqlCommand)Comandosselect(SecuenciaSelect));
            if (Args.Length != 0)
                AsignarParametroSelect("'", Da.SelectCommand, Args);
            return (System.Data.IDataAdapter)Da;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ProcedimientoAlmacenado">aqui se envia el nombre del procedimiento almacenado</param>
        /// <returns></returns>
        protected System.Data.IDbCommand Comandos(string ProcedimientoAlmacenado)
        {
            System.Data.SqlClient.SqlCommand Com;
            Com = new System.Data.SqlClient.SqlCommand(ProcedimientoAlmacenado, (System.Data.SqlClient.SqlConnection)this.conexion);
            //Com.CommandTimeout = 0;
            Com.Transaction = (System.Data.SqlClient.SqlTransaction)this.mTransaccion;

            Com.CommandType = System.Data.CommandType.StoredProcedure;
            System.Data.SqlClient.SqlCommandBuilder.DeriveParameters(Com);

            return (System.Data.IDbCommand)Com;
        }

        protected System.Data.IDbCommand Comandosselect(string SecuenciaSelect)
        {
            System.Data.SqlClient.SqlCommand Com;
            Com = new System.Data.SqlClient.SqlCommand(SecuenciaSelect, (System.Data.SqlClient.SqlConnection)this.conexion);
            Com.Transaction = (System.Data.SqlClient.SqlTransaction)this.mTransaccion;
            Com.CommandType = System.Data.CommandType.Text;
            return (System.Data.IDbCommand)Com;
        }

        /// <summary>
        /// metodo que recoge los parametro que estan asignados al store procedure
        /// </summary>
        /// <param name="Com">variable comando que se a realizado la conexion a la base </param>
        /// <param name="Args">objeto de parametros</param>
        protected void CargarParametros(System.Data.IDbCommand Com, System.Object[] Args)
        {
            int Limite = Com.Parameters.Count;
            for (int i = 1; i < Com.Parameters.Count; i++)
            {
                System.Data.SqlClient.SqlParameter P = (System.Data.SqlClient.SqlParameter)Com.Parameters[i];
                if (i <= Args.Length)
                    P.Value = Args[i - 1];
                else
                    P.Value = null;
            }
        }

        /// <summary>
        /// Asigna parametros al select siempre y cuando el parametro seleccionado tiene que ir en la misma posicion del arreglo de parametros
        /// </summary>
        /// <param name="separador"></param>Separador para el comando select de cadena
        /// <param name="Com"></param>es la varaiable Comand
        /// <param name="Args"></param>es el arreglo de parametros que se asignara al comando de texto
        private void AsignarParametroSelect(String separador, System.Data.IDbCommand Com, System.Object[] Args)
        {
            for (int i = 1; i < Args.Length + 1; i++)
            {
                int indice = Com.CommandText.IndexOf("@p" + i.ToString());
                string prefijo = Com.CommandText.Substring(0, indice);
                string sufijo = Com.CommandText.Substring(indice + ("@p" + i).ToString().Length);
                Com.CommandText = prefijo + separador + Args[i - 1].ToString() + separador + sufijo;
            }
        }
        #endregion
    }
}
