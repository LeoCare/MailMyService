# PROYECTO MISCUENTAS - Servicio de Correo

Servicio de Windows para el proyecto MisCuentas, desarrollado en C#, que gestiona el envío de correos electrónicos automatizados para notificaciones y recuperación de contraseñas.

[![C#](https://img.shields.io/badge/Code-C%23-green)](https://docs.microsoft.com/es-es/dotnet/csharp/) [![LICENSE](https://img.shields.io/badge/Licencia-CC-%23e64545)](https://creativecommons.org/licenses/by-nc-sa/4.0/) ![GitHub](https://img.shields.io/github/last-commit/LeoCare/MailMyService)

![Imagen del Servicio](https://media.licdn.com/dms/image/v2/C4E12AQEOW8HLRbGzeg/article-cover_image-shrink_720_1280/article-cover_image-shrink_720_1280/0/1520636373448?e=2147483647&v=beta&t=cTpgSevOQAnMgmYjtuZkPIkYLwYXVu2Fwtye6pePv_I)

- [Acerca de](#acerca-de)
  - [Notas Sobre el Desarrollo](#notas-sobre-el-desarrollo)
  - [Tecnologías Utilizadas](#tecnologias-utilizadas)
  - [Características](#caracteristicas)
  - [Librerías](#librerias)
  - [Aplicaciones que forman parte de este proyecto](#aplicaciones-que-forman-parte-de-este-proyecto)
- [Arquitectura de la Aplicación](#arquitectura-de-la-aplicacion)
- [Ejemplos de Código](#ejemplos-de-codigo)
- [Instalación](#instalacion) 
- [Instalación rapida](#instalacion-rapida)
- [Uso](#uso)
- [Autor](#autor)
  - [Contacto](#contacto)
- [Contribución](#contribucion)
- [Licencia](#licencia)

---

## Acerca de

Bienvenido al **Servicio de Correo MisCuentas**, una aplicación desarrollada en C# que funciona como un servicio de Windows para automatizar el envío de correos electrónicos relacionados con el proyecto MisCuentas. Este servicio se encarga de:

- Enviar notificaciones a usuarios sobre deudas pendientes o créditos en las hojas de cálculo de gastos compartidos.
- Gestionar la recuperación de contraseñas mediante el envío de códigos de verificación.
- Actualizar estados en la base de datos tras el envío exitoso de correos.

---

## Notas Sobre el Desarrollo

- El servicio está diseñado para ejecutarse en segundo plano en sistemas Windows, iniciándose automáticamente y comprobando periódicamente si hay correos pendientes de enviar.
- Utiliza **MySQL** como base de datos para almacenar y consultar la información necesaria.
- Implementa consultas parametrizadas y manejo de excepciones para asegurar la integridad y seguridad de los datos.
- Se incluyen funcionalidades para el manejo de plantillas de correo en HTML, incorporando logotipos e imágenes embebidas.

### Tecnologías Utilizadas

- **C# (.NET Framework):** Lenguaje principal de desarrollo para el servicio.
- **MySQL Connector/NET:** Para la conexión y operaciones con la base de datos MySQL.
- **SMTP (System.Net.Mail):** Para el envío de correos electrónicos.
- **Windows Service Components:** Para crear y gestionar el servicio de Windows.

### Características

1. **Envío Automatizado de Correos:**
   - Notifica a los usuarios sobre deudas o créditos pendientes.
   - Envía códigos de recuperación de contraseña.

2. **Plantillas de Correo en HTML:**
   - Correos electrónicos con formato profesional.
   - Incluye logotipos e imágenes embebidas.
   - Soporte para contenido dinámico en el cuerpo del correo.

3. **Manejo de Excepciones y Logging:**
   - Registra errores y excepciones en un archivo de log para facilitar la depuración.
   - Manejo de excepciones específicas de MySQL y generales.

4. **Consultas Parametrizadas:**
   - Uso de consultas SQL parametrizadas para prevenir inyecciones SQL.
   - Manejo eficiente y seguro de datos en la base de datos.

5. **Integración con el Proyecto MisCuentas:**
   - Interactúa con la base de datos compartida del proyecto.
   - Complementa las funcionalidades de la aplicación móvil y de escritorio.

### Librerías

- **MySql.Data:** Conector oficial para MySQL.
- **System.Net.Mail:** Para el envío de correos electrónicos SMTP.
- **System.ServiceProcess:** Para crear servicios de Windows.
- **Newtonsoft.Json:** (Opcional) Para manejar datos en formato JSON si es necesario.

### Aplicaciones que forman parte de este proyecto

- #### [API REST con Ktor](https://github.com/LeoCare/ktor-Api-MisCuentas)
- #### [APP Escritorio MisCuentas_desk](https://github.com/LeoCare/MisCuentas_desk)
- #### [App Android con Kotlin](https://github.com/LeoCare/MisCuentas)

---

## Arquitectura de la Aplicación

El servicio sigue una arquitectura modular que incluye las siguientes capas:

- **Capa de Datos:** Maneja las conexiones y operaciones con la base de datos MySQL.
- **Capa de Negocio:** Contiene la lógica para comprobar correos pendientes, generar códigos, actualizar estados y enviar correos.
- **Capa de Servicio:** Implementa el servicio de Windows, gestionando el ciclo de vida (inicio, pausa, continuación y parada).

---

## Ejemplos de Código

A continuación, se muestran algunos fragmentos de código representativos:

**Método para Comprobar Correos Pendientes:**

```csharp
public void ComprobarPendientes()
{
    try
    {
        using (MySqlConnection db = new MySqlConnection(_cadenaConexion))
        {
            db.Open();
            string datosEmail = "SELECT el.id_email, h.titulo, h.fecha_creacion, b.monto, p.correo, p.nombre, te.asunto, te.contenido " +
                                "FROM PARTICIPANTES p " +
                                "JOIN BALANCES b ON b.id_participante = p.id_participante AND b.id_hoja = p.id_hoja " +
                                "JOIN HOJAS h ON h.id_hoja = p.id_hoja " +
                                "JOIN EMAIL_LOG el ON el.id_balance = b.id_balance " +
                                "JOIN TIPO_EMAIL te ON el.tipo = te.tipo " +
                                "WHERE el.status = @status " +
                                "AND el.fecha_envio IS NULL " +
                                "AND p.correo IS NOT NULL;";

            using (MySqlCommand cmd = new MySqlCommand(datosEmail, db))
            {
                cmd.Parameters.AddWithValue("@status", 'P');

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Lectura y procesamiento de datos
                        EnviarCorreo(...);
                        ActualizarStatus(...);
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Manejo de excepciones
    }
}
```

**Método para Enviar Correo:**

```csharp
private void EnviarCorreo(...)
{
    try
    {
        MailMessage mail = new MailMessage
        {
            From = new MailAddress("info_miscuentas_app@leondev.es"),
            Subject = asunto,
            IsBodyHtml = true
        };
        mail.To.Add(correo);
        mail.Body = htmlBody;

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new System.Net.NetworkCredential("correo@example.com", "contraseña"),
            EnableSsl = true,
        };

        smtpServer.Send(mail);
    }
    catch (Exception ex)
    {
        // Manejo de excepciones
    }
}
```

---

## Instalación

Para instalar y configurar el servicio:

1. **Clonar el repositorio:**

   ```bash
   git clone https://github.com/LeoCare/MailMyService.git
   ```

2. **Abrir el proyecto en Visual Studio.**

3. **Configurar la cadena de conexión:**

   - Modifica la variable `_cadenaConexion` con los detalles de tu base de datos MySQL.

4. **Configurar las credenciales SMTP:**

   - Actualiza el correo electrónico y la contraseña en el método `EnviarCorreo`.

5. **Compilar el proyecto.**

6. **Instalar el servicio:**

   - Utiliza `InstallUtil.exe` para instalar el servicio de Windows.
   - Ejecuta en el símbolo del sistema:

     ```bash
     InstallUtil.exe /i Ruta\Al\Ejecutable.exe
     ```

7. **Iniciar el servicio desde el Administrador de Servicios de Windows.**

---
## Instalación

Seguir los pasos hasta el puntos 5 del apartado **`Instalacion`**, luego:
 1. Abri el CMD de Windows como administrador
 2. Crear el servicio:
 ```bash
     sc create MailMyService binPath= "C:\Users\leon1\Documents\Repo_Github\MailMyService\bin\Release\MailMyService.exe"
 ```
 3. Arancar el servicio:
 ```bash
     sc start MailMyService
 ```
 4. Listo

 Si fuera necesario eliminarlo:
 ```bash
     sc stop MailMyService
 ```
 ```bash
     sc delete MailMyService
 ```

---
## Uso

1. **Automatización de Correos:**

   - El servicio comprobará periódicamente si hay correos pendientes y los enviará automáticamente.

2. **Recuperación de Contraseñas:**

   - Genera códigos de recuperación y los envía a los usuarios que lo soliciten.

3. **Actualización de Estados:**

   - Tras el envío exitoso de un correo, el servicio actualiza el estado en la base de datos para evitar reenvíos.

---

## Autor
Mi nombre es <b>Leonardo David Care Prado</b>, soy tecnico en sistemas y desarrollador de aplicaciones multiplataforma, o eso espero con este proyecto...jjjjj.<br>
A fecha de este año (2024) llevo 4 años realizando trabajos de desarrollo para la misma empresa, ademas de soporte y sistemas.<br>
Estos desarrollos incluyen lenguajes como Html, C#, Xamarin, Oracle, Java y Kotlin.

[![Html](https://img.shields.io/badge/Code-Htmnl-blue)](https://www.w3schools.com/html/) [![C#](https://img.shields.io/badge/Code-C_SHARP-green)](https://dotnet.microsoft.com/es-es/languages/csharp) [![Xamarin](https://img.shields.io/badge/Code-Xamarin-red)](https://dotnet.microsoft.com/es-es/apps/xamarin) [![Oracle](https://img.shields.io/badge/Code-Oracle-white)](https://www.oracle.com/es/) [![Java](https://img.shields.io/badge/Code-Java-orange)](https://www.java.com/es/) [![Kotlin](https://img.shields.io/badge/Code-Kotlin-blueviolet)](https://kotlinlang.org/)

### Contacto
Para cualquier consulta o aporte puedes comunicarte conmigo por correo<br>
[leon1982care@gmail.com](https://mail.google.com/mail/u/0/?pli=1#inbox)
<p><a href="https://mail.google.com/mail/u/0/?pli=1#inbox" target="_blank">
        <img src="https://ams3.digitaloceanspaces.com/graffica/2021/06/logogmailgrafica-1-1024x576.png" 
    height="30" alt="correo_electronico">
</a></p> 

## Contribucion
Gracias a todos los que aporten nuevas ideas de como mejorar mi proyecto. Sientance libres de participar, cambiar u opinar sobre el mismo.</br>
Solo pido que al crear la rama, esta comience por 'contribucion/lo_que_aporteis'. Y, el commit sea claro y descriptivo.</br>
En caso de necesitar documentar los nuevos cambios, seguir con el uso de las libreria mensionada en el apartado [Documentaciones](#documentaciones).</br>
Muchisimas gracias a todos!

## Licencia
Este repositorio y todo su contenido estan bajo la licencia de **Creative Commons**. Solo pido que si haces uso de ella, me cites como el autor.</br>
<a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/"><img alt="Creative Commons License" style="border-width:0" src="https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png" /></a>

<a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/">Creative Commons
Attribution-NonCommercial-ShareAlike 4.0 International License</a>.

---