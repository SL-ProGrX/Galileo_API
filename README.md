# 🌌 Galileo_API

[![.NET](https://img.shields.io/badge/.NET-9.0-blue?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
![Status](https://img.shields.io/badge/Status-Private%20Project-critical)
[![Swagger](https://img.shields.io/badge/API-Swagger-green?logo=swagger)](https://swagger.io/)

---

API desarrollada con **ASP.NET Core 9** que implementa autenticación segura mediante **JWT (JSON Web Tokens)**.  
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

- [.NET 9 SDK](https://dotnet.microsoft.com/)
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


2. **Configurar Secretos de Usuario según las indicaciones brindadas**

---

### 🔐 Configuración de Secretos de Usuario (.NET User-Secrets)

Para mantener seguras las credenciales y claves sensibles durante el desarrollo, este proyecto utiliza  
**.NET User Secrets**, evitando exponer información en el repositorio.

Cada desarrollador debe configurar sus secrets localmente siguiendo estos pasos:

---

## 1️⃣ Solicitar los secretos del proyecto
Pide al responsable técnico el archivo `secrets.json` o las claves necesarias.

Ejemplo:

```json
{
  "Jwt:Secret": "XXXXXXXXXXXXXXXXXXXX",
  "ConnectionStrings:DefaultConnString": "...",
  "ConnectionStrings:GAConnString": "...",
  "ConnectionStrings:BaseConnString": "..."
}

```  

```bash

# Crear los User Secrets del proyecto
dotnet user-secrets init
dotnet user-secrets set InitKey InitValue

# ----------------------------
# 📂 ABRIR CARPETA DE SECRETS
# ----------------------------

# 🍎 macOS (abrir carpeta de secrets)
open ~/.microsoft/usersecrets/<UserSecretsId>

# 🪟 Windows (abrir esta ruta en el Explorador)
# C:\Users\<TU_USUARIO>\AppData\Roaming\Microsoft\UserSecrets\<UserSecretsId>\

# ----------------------------
# ✏️ EDITAR secrets.json
# ----------------------------
# 1. Abrir secrets.json
# 2. Borrar todo su contenido
# 3. Pegar los secrets proporcionados por el equipo
# 4. Guardar el archivo

# ----------------------------
# ✅ VERIFICAR SECRETS
# ----------------------------
dotnet user-secrets list

# ----------------------------
# 🧼 (Opcional) borrar clave temporal
# ----------------------------
dotnet user-secrets remove InitKey

# -----------------------------------
# 🔄 Actualizar valores usando dotnet CLI
# (equivalente a lo que va dentro de secrets.json)
# -----------------------------------

# JWT Secret
dotnet user-secrets set "Jwt:Secret" "XXXXXXXXXXXXXXXXXXXX"

# Connection Strings
dotnet user-secrets set "ConnectionStrings:DefaultConnString" "<valor>"
dotnet user-secrets set "ConnectionStrings:GAConnString" "<valor>"
dotnet user-secrets set "ConnectionStrings:BaseConnString" "<valor>"



