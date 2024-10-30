using System;


namespace MailMyService
{
    public class EmailRequest
    {
        public string Titulo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public double? Monto { get; set; }
        public string Correo { get; set; }
        public string Nombre { get; set; }
        public string Asunto { get; set; }
        public string Contenido { get; set; }
    }
}
