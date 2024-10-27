using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MailMyService
{
    public class EmailCommand
    {
        #region ATRIBUTOS
        private MisCuentasConnect _conn = new MisCuentasConnect();
        protected readonly string _cadenaConexion;
        #endregion

        #region CONSTRUCTOR
        public EmailCommand() 
        {
            _cadenaConexion = _conn.Conexion();
        }
        #endregion


        #region METODOS A BBDD
        /// <summary>
        /// Metodo que comprueba los email pendientes de ser enviados
        /// </summary>
        public void ComprobarPendientes()
        {        
            try
            {
                using (MySqlConnection db = new MySqlConnection(_cadenaConexion))
                {
                    db.Open();
                    string datosEmail = "SELECT el.id_email, h.titulo, h.fecha_creacion, pg.monto, p.correo, p.nombre, te.asunto, te.contenido " +
                                            "FROM PARTICIPANTES p, TIPO_EMAIL te, BALANCES b, HOJAS h, EMAIL_LOG el, PAGOS pg " +
                                            "WHERE el.status = @status " +
                                            "AND el.fecha_envio IS NULL " +
                                            "AND el.id_balance = b.id_balance " +
                                            "AND el.tipo = te.tipo " +
                                            "AND b.id_participante = p.id_participante " +
                                           "AND b.id_hoja = p.id_hoja " +
                                            "AND b.id_balance = pg.id_balance_pagado " +
                                            "AND h.id_hoja = p.id_hoja " +
                                            "AND te.tipo = @tipoEnvioAcreedor " +
                                            "AND p.correo IS NOT NULL " +
                                        "UNION " +
                                        "SELECT el.id_email, h.titulo, h.fecha_creacion, b.monto, p.correo, p.nombre, te.asunto, te.contenido " +
                                           "FROM PARTICIPANTES p, TIPO_EMAIL te, BALANCES b, HOJAS h, EMAIL_LOG el, PAGOS pg " +
                                            "WHERE el.status = @status " +
                                            "AND el.fecha_envio IS NULL " +
                                            "AND el.id_balance = b.id_balance " +
                                            "AND el.tipo = te.tipo " +
                                            "AND b.id_participante = p.id_participante " +
                                            "AND b.id_hoja = p.id_hoja " +
                                            "AND h.id_hoja = p.id_hoja " +
                                            "AND te.tipo = @tipoEnvioDeudor " +
                                            "AND p.correo IS NOT NULL;";

                    using (MySqlCommand cmd = new MySqlCommand(datosEmail, db))
                    {
                        cmd.Parameters.AddWithValue("@status", 'P');
                        cmd.Parameters.AddWithValue("@tipoEnvioDeudor", 'S');
                        cmd.Parameters.AddWithValue("@tipoEnvioAcreedor", 'E');

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id_email = reader.GetInt32("id_email");
                                string titulo = reader.IsDBNull(reader.GetOrdinal("titulo")) ? null : reader.GetString("titulo");
                                DateTime? fecha_creacion = reader.IsDBNull(reader.GetOrdinal("fecha_creacion")) ? (DateTime?)null : reader.GetDateTime("fecha_creacion");
                                double? monto = reader.IsDBNull(reader.GetOrdinal("monto")) ? (double?)null : Math.Abs(reader.GetDouble("monto"));
                                string correo = reader.IsDBNull(reader.GetOrdinal("correo")) ? null : reader.GetString("correo");
                                string nombre = reader.IsDBNull(reader.GetOrdinal("nombre")) ? null : reader.GetString("nombre");
                                string asunto = reader.IsDBNull(reader.GetOrdinal("asunto")) ? null : reader.GetString("asunto");
                                string contenido = reader.IsDBNull(reader.GetOrdinal("contenido")) ? null : reader.GetString("contenido");

                                // Enviar correo electrónico
                                EnviarCorreo(titulo, fecha_creacion, monto, correo, nombre, asunto, contenido);

                                // Actualizar el estado a 'ENVIADO' después de enviar el correo
                                ActualizarStatus(id_email);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
                throw ManejarExcepcionMySql(ex);
            }
            catch (Exception ex)
            {
                File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
                throw;
            }

        }

     
        /// <summary>
        /// Metodo que actualiza el estado del email una vez enviado
        /// </summary>
        /// <param name="id_email">identificador del email</param>
        private void ActualizarStatus(int id_email)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_cadenaConexion))
                {
                    conn.Open();
                    string updateQuery = "UPDATE EMAIL_LOG SET status = @status, fecha_envio = @fechaEnvio WHERE id_email = @idEmail;";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", "E");
                        cmd.Parameters.AddWithValue("@fechaEnvio", DateTime.Now);
                        cmd.Parameters.AddWithValue("@idEmail", id_email);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
                throw ManejarExcepcionMySql(ex);
            }
            catch (Exception ex)
            {
                File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Metodo que comprueba los usuarios que solicitaron un codigo de recuperacion.
        /// </summary>
        public void GenerarCodigoRecuperacion()
        {
            try
            {
                using (MySqlConnection db = new MySqlConnection(_cadenaConexion))
                {
                    db.Open();

                    string selectQuery = "SELECT id_usuario, nombre, correo, asunto, contenido " +
                                            "FROM USUARIOS, TIPO_EMAIL " +
                                            "WHERE codigo_recup_pass = @codigoRecupPass " +
                                            "AND tipo = @tipo;";

                    using (MySqlCommand selectCmd = new MySqlCommand(selectQuery, db))
                    {
                        selectCmd.Parameters.AddWithValue("@codigoRecupPass", 0);
                        selectCmd.Parameters.AddWithValue("@tipo", "C");

                        using (MySqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id_usuario = reader.GetInt32("id_usuario");
                                string correo = reader.IsDBNull(reader.GetOrdinal("correo")) ? null : reader.GetString("correo");
                                string nombre = reader.IsDBNull(reader.GetOrdinal("nombre")) ? null : reader.GetString("nombre");
                                string asunto = reader.IsDBNull(reader.GetOrdinal("asunto")) ? null : reader.GetString("asunto");
                                string contenido = reader.IsDBNull(reader.GetOrdinal("contenido")) ? null : reader.GetString("contenido");

                                // Generar un código de 4 dígitos al azar
                                int codigo = GenerarCodigoAleatorio();

                                // Enviar correo electrónico
                                EnviarCodigo(codigo.ToString(), correo, nombre, asunto, contenido);

                                // Actualizar el campo codigo_recup_pass en la base de datos
                                ActualizarCodigoRecuperacion(id_usuario, codigo);

                               
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Manejar excepciones de MySQL y registrar errores
                File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
                throw ManejarExcepcionMySql(ex);
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones y registrar errores
                File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Metodo que genera un codigo de 4 digitos aleatorios
        /// </summary>
        /// <returns>codigo generado</returns>
        private int GenerarCodigoAleatorio()
        {
            Random random = new Random();
            return random.Next(1000, 10000); // Genera un número entre 1000 y 9999
        }

        /// <summary>
        /// Metodo para insertar el codigo generado.
        /// </summary>
        /// <param name="id_usuario">identificador del usuario</param>
        /// <param name="codigo">codigo generado automaticamente</param>
        private void ActualizarCodigoRecuperacion(int id_usuario, int codigo)
        {
            try
            {
                using (MySqlConnection db = new MySqlConnection(_cadenaConexion))
                {
                    db.Open();

                    string updateQuery = "UPDATE USUARIOS SET codigo_recup_pass = @codigo WHERE id_usuario = @idUsuario;";

                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, db))
                    {
                        updateCmd.Parameters.AddWithValue("@codigo", codigo);
                        updateCmd.Parameters.AddWithValue("@idUsuario", id_usuario);

                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Manejar excepciones de MySQL y registrar errores
                File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
                throw ManejarExcepcionMySql(ex);
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones y registrar errores
                File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
                throw;
            }
        }
        #endregion


        #region METODOS ENVIO EMAIL
        /// <summary>
        /// Metodo de envio de correo informando de los balances.
        /// </summary>
        /// <param name="titulo">titulo de la hoja</param>
        /// <param name="fecha_creacion">fecha de creacion de la hoja</param>
        /// <param name="monto">monto del balance</param>
        /// <param name="correo">correo del usuario</param>
        /// <param name="nombre">nombre del usuario</param>
        /// <param name="asunto">asunto del email</param>
        /// <param name="contenido">contenido del email</param>
        public void EnviarCorreo(
           string titulo,
           DateTime? fecha_creacion,
           double? monto,
           string correo,
           string nombre,
           string asunto,
           string contenido)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("info_miscuentas_app@leondev.es");
                mail.To.Add(correo);
                mail.Subject = asunto;
                mail.IsBodyHtml = true;


                string htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <img src='cid:logo' alt='Mis Cuentas App' style='width: 100px;'/>
                    <p>Hola {nombre},</p>
                    <p>{contenido}</p>
                    <h3>Datos de la hoja:</h3>
                    <table style='border-collapse: collapse;'>
                        <tr>
                            <td style='padding: 8px; font-weight: bold;'>Hoja:</td>
                            <td style='padding: 8px;'>{titulo}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; font-weight: bold;'>Fecha Creación:</td>
                            <td style='padding: 8px;'>{fecha_creacion?.ToString("dd/MM/yyyy")}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; font-weight: bold;'>Importe:</td>
                            <td style='padding: 8px;'>{monto}€</td>
                        </tr>
                    </table>
                    <p>Gracias por usar nuestra aplicación.</p>
                    <p>Saludos cordiales,<br/>Equipo de Mis Cuentas App</p>
                    <img src='cid:presentacion' alt='Mis Cuentas App' style='width: 120px;'/>
                </body>
                </html>";
                mail.Body = htmlBody;

                // Agregar imagenes embebidas
                string tituloPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "titulo.png");
                LinkedResource presentacion = new LinkedResource(tituloPath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "presentacion"
                };
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
                LinkedResource logo = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "logo"
                };

                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                avHtml.LinkedResources.Add(presentacion);
                avHtml.LinkedResources.Add(logo);
                mail.AlternateViews.Add(avHtml);

                smtpServer.Port = 587;
                smtpServer.Credentials = new System.Net.NetworkCredential("contacto.miscuentas@gmail.com", "cmhy xnxr griw hjqa");
                smtpServer.EnableSsl = true;

                smtpServer.Send(mail);

            }
            catch (Exception ex)
            {
                // Manejar excepciones y registrar errores
                File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
                throw;
            }
        }


        /// <summary>
        /// Metodo de envio de codigo para la recuperacion de la contraseña.
        /// </summary>
        /// <param name="codigo">codigo para la recuperacion</param>
        /// <param name="correo">correo del usuario</param>
        /// <param name="nombre">nombre del usuario</param>
        /// <param name="asunto">asunto del email</param>
        /// <param name="contenido">contenido del email</param>
        public void EnviarCodigo(
           string codigo,        
           string correo,
           string nombre,
           string asunto,
           string contenido)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("info_miscuentas_app@leondev.es");
                mail.To.Add(correo);
                mail.Subject = asunto;
                mail.IsBodyHtml = true;


                string htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <img src='cid:logo' alt='Mis Cuentas App' style='width: 100px;'/>
                    <p>Hola {nombre},</p>
                    <p>{contenido}</p>
                    <h3>Datos de la hoja:</h3>
                    <table style='border-collapse: collapse;'>
                        <tr>
                            <td style='padding: 8px; font-weight: bold;'>Hoja:</td>
                            <td style='padding: 8px;'>{codigo}</td>
                        </tr>                 
                    </table>
                    <p>Gracias por usar nuestra aplicación.</p>
                    <p>Saludos cordiales,<br/>Equipo de Mis Cuentas App</p>
                    <img src='cid:presentacion' alt='Mis Cuentas App' style='width: 120px;'/>
                </body>
                </html>";
                mail.Body = htmlBody;

                // Agregar imagenes embebidas
                string tituloPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "titulo.png");
                LinkedResource presentacion = new LinkedResource(tituloPath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "presentacion"
                };
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
                LinkedResource logo = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "logo"
                };

                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                avHtml.LinkedResources.Add(presentacion);
                avHtml.LinkedResources.Add(logo);
                mail.AlternateViews.Add(avHtml);

                smtpServer.Port = 587;
                smtpServer.Credentials = new System.Net.NetworkCredential("contacto.miscuentas@gmail.com", "cmhy xnxr griw hjqa");
                smtpServer.EnableSsl = true;

                smtpServer.Send(mail);

            }
            catch (Exception ex)
            {
                // Manejar excepciones y registrar errores
                File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
                throw;
            }
        }
        #endregion

        #region METODOS
        public Exception ManejarExcepcionMySql(MySqlException ex)
        {
            switch (ex.Number)
            {
                case 1045:
                    return new Exception("Acceso denegado a la base de datos. Verifica las credenciales de acceso.", ex);
                case 1049:
                    return new Exception("Base de datos desconocida. Verifica el nombre de la base de datos.", ex);
                case 1051:
                    return new Exception("La tabla especificada no existe en la base de datos.", ex);
                case 1054:
                    return new Exception("Columna desconocida en la tabla. Verifica los nombres de las columnas.", ex);
                case 1062:
                    return new Exception("El registro ya existe en la base de datos. Violación de la clave única.", ex);
                case 1064:
                    return new Exception("Error de sintaxis en la consulta SQL. Verifica la sintaxis de la consulta.", ex);
                case 1136:
                    return new Exception("La cantidad de columnas no coincide con los valores proporcionados.", ex);
                case 1142:
                    return new Exception("Permisos insuficientes para ejecutar la consulta. Verifica los privilegios del usuario.", ex);
                case 1146:
                    return new Exception("La tabla especificada no existe en la base de datos.", ex);
                case 1149:
                    return new Exception("Sintaxis incorrecta en la consulta SQL.", ex);
                case 1216:
                    return new Exception("Error de clave foránea. Asegúrate de que las claves foráneas sean correctas.", ex);
                case 1217:
                    return new Exception("Error de eliminación de clave foránea. No se puede eliminar o actualizar una fila padre.", ex);
                case 1292:
                    return new Exception("Valor no válido para un campo. Verifica los valores que se están insertando.", ex);
                case 1364:
                    return new Exception("El campo no tiene un valor predeterminado. Proporcione un valor para el campo.", ex);
                case 1366:
                    return new Exception("Valor de campo incorrecto. El valor proporcionado no es compatible con el tipo de datos.", ex);
                case 1451:
                    return new Exception("Restricción de clave foránea falló en eliminación. No se puede eliminar una fila que está referenciada.", ex);
                case 1452:
                    return new Exception("Restricción de clave foránea falló en inserción. El valor de la clave foránea no coincide con ninguna fila en la tabla de referencia.", ex);
                case 1040:
                    return new Exception("Límite de conexiones a la base de datos alcanzado. No se pueden establecer más conexiones en este momento.", ex);
                case 1042:
                    return new Exception("No se puede conectar a la base de datos. Verifique el servidor o el nombre de host.", ex);
                default:
                    return new Exception("Ocurrió un error en la base de datos. Código de error: " + ex.Number + ". Mensaje: " + ex.Message, ex);
            }

        }
        #endregion
    }
}
