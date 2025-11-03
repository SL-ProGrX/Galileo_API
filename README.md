# 🌌 Galileo_API

[![.NET](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
![Status](https://img.shields.io/badge/Status-Private%20Project-critical)
[![Swagger](https://img.shields.io/badge/API-Swagger-green?logo=swagger)](https://swagger.io/)

---

API desarrollada con **ASP.NET Core 8** que implementa autenticación segura mediante **JWT (JSON Web Tokens)**.  
Diseñada para entornos privados y uso interno, con prácticas seguras para manejo de claves y configuración.

---

## 🚀 Características principales

- 🔐 **Autenticación JWT segura**
  - Clave privada almacenada con *user-secrets* en desarrollo.
  - Compatible con variables de entorno en producción.
- 🧱 **Estructura limpia y extensible**
  - Controladores y endpoints minimalistas.
- 🧪 **Swagger UI integrado**
  - Documentación y pruebas interactivas desde el navegador.
- ⚙️ **Configuración segura**
  - Sin llaves ni credenciales en el código fuente.
  - Compatible con auditorías de seguridad (Checkmarx, SonarQube, etc.).

---

## 🧰 Tecnologías utilizadas

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- C# 12
- ASP.NET Core Web API
- Swagger / Swashbuckle
- JWT (System.IdentityModel.Tokens.Jwt)

---

## 🧩 Configuración para desarrollo

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/<tu_usuario>/Galileo_API.git
   cd Galileo_API/Galileo_API


2. Configurar Secretos de Usuario
  dotnet user-secrets init
  dotnet user-secrets set "Jwt:Key" "tu_clave_larga_y_segura"

3.Ejecutar API
  dotnet run


