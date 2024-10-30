using System;
using System.Data;
using MySqlConnector;

namespace MailMyService
{
    public class MisCuentasConnect
    {
        #region ATRIBUTOS
        private string _strConn;
        #endregion


        #region CONTRUCTOR
        /// <summary>
        /// Metodo que entrega la cadena de conexion.
        /// </summary>
        /// <returns>Cadena de conexion a la BBDD</returns>
        public String Conexion()
        {
           // return this._strConn = "Server=192.168.7.3;Port=3306;Database=DBMisCuentas;Uid=leo;Pwd=111nonamaEM";
            return this._strConn = "Server=176.222.66.18;Port=3306;Database=DBMisCuentas;Uid=leo;Pwd=111nonamaEM";
        }
        #endregion

        #region METODOS
        /// <summary>
        /// Metodo que realiza la conexion con la BBDD.
        /// </summary>
        /// <returns>True si logra conectarse, o mensaje de aviso si falla.</returns>
        public Boolean PruebaConexion()
        {
            try
            {
                using (IDbConnection db = new MySqlConnection(_strConn))
                {
                    db.Open(); // Intentamos abrir la conexión

                    if (db.State == ConnectionState.Open)
                    {
                        return true;
                    }
                    else
                    {                      
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {              
                return false;
            }
        }
        #endregion
    }
}
