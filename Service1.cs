using System;
using System.ServiceProcess;
using System.Timers;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace MailMyService
{
    public partial class Service1 : ServiceBase
    {
        #region ATRIBUTOS
        private System.Timers.Timer _timer;
        private EmailCommand _emailCommand;

        //Hilos de comunicacion para el Socket
        private Thread _serverThread;
        private TcpListener _listener;
        private bool _isRunning = false;
        #endregion


        #region CONSTRUCTOR
        public Service1()
        {
            InitializeComponent();
            _emailCommand = new EmailCommand();
        }
        #endregion


        #region ONSTART
        /// <summary>
        /// Metodo que inicia el temporizador para la comprobacion y envio de emails
        /// </summary>
        protected override void OnStart(string[] args)
        {
            //Temporizador
            _timer = new System.Timers.Timer(30000);
            _timer.Elapsed += OnElapsedTime;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            //Hilo Socket
            _isRunning = true;
            _serverThread = new Thread(StartServer);
            _serverThread.Start();
        }

        #endregion

        #region TEMPORIZADOR EMAILS
        /// <summary>
        /// Metodo que ejecuta la funcion de comprobacion y envio para cada tipo de balance.
        /// </summary>
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            _emailCommand.ComprobarPendientes('D'); //Deudores
            _emailCommand.ComprobarPendientes('A'); //Acreedores
        }

       /// <summary>
       /// Metodo que detiene el temporizador
       /// </summary>
        protected override void OnStop()
        {
            _timer.Enabled = false;
        }
        #endregion


        #region SOCKET
        private void StartServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, 12345);
                _listener.Start();
                while (_isRunning)
                {
                    if (_listener.Pending())
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        Thread clientThread = new Thread(ClienteHandle);
                        clientThread.Start(client);
                    }
                    else
                    {
                        Thread.Sleep(100); // Evita consumo excesivo de CPU
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar excepciones y registrar errores
                System.IO.File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
            }
        }


        private void ClienteHandle(object clientObj)
        {
            TcpClient client = (TcpClient)clientObj;
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    // Leer datos del cliente
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Deserializar el mensaje
                    EmailRequest request = JsonConvert.DeserializeObject<EmailRequest>(requestJson);

                    // Procesar el mensaje
                    _emailCommand.EnviarCorreo(request.Titulo, request.FechaCreacion, request.Monto, request.Correo, request.Nombre, request.Asunto, request.Contenido);

                    // Enviar respuesta al cliente
                    string response = "OK";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception ex)
            {
                // Manejar excepciones y registrar errores
                System.IO.File.AppendAllText("C:\\EmailServiceLog.txt", ex.ToString());
            }
            finally
            {
                client.Close();
            }
        }
        #endregion

    }

}

