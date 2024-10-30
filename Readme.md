# PROYECTO MISCUENTAS - Servicio de Correo

Servicio de Windows para el proyecto MisCuentas, desarrollado en C#, que gestiona el env�o de correos electr�nicos automatizados para notificaciones y recuperaci�n de contrase�as.

[![C#](https://img.shields.io/badge/Code-C%23-green)](https://docs.microsoft.com/es-es/dotnet/csharp/) [![LICENSE](https://img.shields.io/badge/Licencia-CC-%23e64545)](https://creativecommons.org/licenses/by-nc-sa/4.0/) ![GitHub](https://img.shields.io/github/last-commit/LeoCare/MailMyService)

![Imagen del Servicio](https://media.licdn.com/dms/image/v2/C4E12AQEOW8HLRbGzeg/article-cover_image-shrink_720_1280/article-cover_image-shrink_720_1280/0/1520636373448?e=2147483647&v=beta&t=cTpgSevOQAnMgmYjtuZkPIkYLwYXVu2Fwtye6pePv_I)

- [Acerca de](#acerca-de)
  - [Notas Sobre el Desarrollo](#notas-sobre-el-desarrollo)
  - [Tecnolog�as Utilizadas](#tecnologias-utilizadas)
  - [Caracter�sticas](#caracteristicas)
  - [Librer�as](#librerias)
  - [Aplicaciones que forman parte de este proyecto](#aplicaciones-que-forman-parte-de-este-proyecto)
- [Arquitectura de la Aplicaci�n](#arquitectura-de-la-aplicacion)
- [Ejemplos de C�digo](#ejemplos-de-codigo)
- [Instalaci�n](#instalacion) 
- [Instalaci�n rapida](#instalacion-rapida)
- [Uso](#uso)
- [Autor](#autor)
  - [Contacto](#contacto)
- [Contribuci�n](#contribucion)
- [Licencia](#licencia)

---

## Acerca de

Bienvenido al **Servicio de Correo MisCuentas**, una aplicaci�n desarrollada en C# que funciona como un servicio de Windows para automatizar el env�o de correos electr�nicos relacionados con el proyecto MisCuentas. Este servicio se encarga de:

- Enviar notificaciones a usuarios sobre deudas pendientes o cr�ditos en las hojas de c�lculo de gastos compartidos.
- Gestionar la recuperaci�n de contrase�as mediante el env�o de c�digos de verificaci�n.
- Actualizar estados en la base de datos tras el env�o exitoso de correos.

---

## Notas Sobre el Desarrollo

- El servicio est� dise�ado para ejecutarse en segundo plano en sistemas Windows, inici�ndose autom�ticamente y comprobando peri�dicamente si hay correos pendientes de enviar.
- Utiliza **MySQL** como base de datos para almacenar y consultar la informaci�n necesaria.
- Implementa consultas parametrizadas y manejo de excepciones para asegurar la integridad y seguridad de los datos.
- Se incluyen funcionalidades para el manejo de plantillas de correo en HTML, incorporando logotipos e im�genes embebidas.

### Tecnolog�as Utilizadas

- **C# (.NET Framework):** Lenguaje principal de desarrollo para el servicio.
- **MySQL Connector/NET:** Para la conexi�n y operaciones con la base de datos MySQL.
- **SMTP (System.Net.Mail):** Para el env�o de correos electr�nicos.
- **Windows Service Components:** Para crear y gestionar el servicio de Windows.

### Caracter�sticas

1. **Env�o Automatizado de Correos:**
   - Notifica a los usuarios sobre deudas o cr�ditos pendientes.
   - Env�a c�digos de recuperaci�n de contrase�a.

2. **Plantillas de Correo en HTML:**
   - Correos electr�nicos con formato profesional.
   - Incluye logotipos e im�genes embebidas.
   - Soporte para contenido din�mico en el cuerpo del correo.

3. **Manejo de Excepciones y Logging:**
   - Registra errores y excepciones en un archivo de log para facilitar la depuraci�n.
   - Manejo de excepciones espec�ficas de MySQL y generales.

4. **Consultas Parametrizadas:**
   - Uso de consultas SQL parametrizadas para prevenir inyecciones SQL.
   - Manejo eficiente y seguro de datos en la base de datos.

5. **Integraci�n con el Proyecto MisCuentas:**
   - Interact�a con la base de datos compartida del proyecto.
   - Complementa las funcionalidades de la aplicaci�n m�vil y de escritorio.

### Librer�as

- **MySql.Data:** Conector oficial para MySQL.
- **System.Net.Mail:** Para el env�o de correos electr�nicos SMTP.
- **System.ServiceProcess:** Para crear servicios de Windows.
- **Newtonsoft.Json:** (Opcional) Para manejar datos en formato JSON si es necesario.

### Aplicaciones que forman parte de este proyecto

- #### [API REST con Ktor](https://github.com/LeoCare/ktor-Api-MisCuentas)
- #### [APP Escritorio MisCuentas_desk](https://github.com/LeoCare/MisCuentas_desk)
- #### [App Android con Kotlin](https://github.com/LeoCare/MisCuentas)

---

## Arquitectura de la Aplicaci�n

El servicio sigue una arquitectura modular que incluye las siguientes capas:

- **Capa de Datos:** Maneja las conexiones y operaciones con la base de datos MySQL.
- **Capa de Negocio:** Contiene la l�gica para comprobar correos pendientes, generar c�digos, actualizar estados y enviar correos.
- **Capa de Servicio:** Implementa el servicio de Windows, gestionando el ciclo de vida (inicio, pausa, continuaci�n y parada).

---

## Ejemplos de C�digo

A continuaci�n, se muestran algunos fragmentos de c�digo representativos:

**M�todo para Comprobar Correos Pendientes:**

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

**M�todo para Enviar Correo:**

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
            Credentials = new System.Net.NetworkCredential("correo@example.com", "contrase�a"),
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

## Instalaci�n

Para instalar y configurar el servicio:

1. **Clonar el repositorio:**

   ```bash
   git clone https://github.com/LeoCare/MailMyService.git
   ```

2. **Abrir el proyecto en Visual Studio.**

3. **Configurar la cadena de conexi�n:**

   - Modifica la variable `_cadenaConexion` con los detalles de tu base de datos MySQL.

4. **Configurar las credenciales SMTP:**

   - Actualiza el correo electr�nico y la contrase�a en el m�todo `EnviarCorreo`.

5. **Compilar el proyecto.**

6. **Instalar el servicio:**

   - Utiliza `InstallUtil.exe` para instalar el servicio de Windows.
   - Ejecuta en el s�mbolo del sistema:

     ```bash
     InstallUtil.exe /i Ruta\Al\Ejecutable.exe
     ```

7. **Iniciar el servicio desde el Administrador de Servicios de Windows.**

---
## Instalaci�n

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

1. **Automatizaci�n de Correos:**

   - El servicio comprobar� peri�dicamente si hay correos pendientes y los enviar� autom�ticamente.

2. **Recuperaci�n de Contrase�as:**

   - Genera c�digos de recuperaci�n y los env�a a los usuarios que lo soliciten.

3. **Actualizaci�n de Estados:**

   - Tras el env�o exitoso de un correo, el servicio actualiza el estado en la base de datos para evitar reenv�os.

---

## Autor
Mi nombre es <b>Leonardo David Care Prado</b>, soy tecnico en sistemas y desarrollador de aplicaciones multiplataforma, o eso espero con este proyecto...jjjjj.<br>
A fecha de este a�o (2024) llevo 4 a�os realizando trabajos de desarrollo para la misma empresa, ademas de soporte y sistemas.<br>
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