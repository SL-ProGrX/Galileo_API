# Control global de sesiones de SSecurity

## Acceso

Los endpoints globales requieren un JWT válido con `app=ssecurity`. En cada solicitud, el API vuelve a comprobar en la base de datos que el usuario del JWT conserve el perfil global `Admin_Portal`. Los roles administrativos por empresa no conceden acceso a este control. La pantalla se agrega al menú de Bitácoras y solo se muestra cuando la sesión indica ese perfil; esta visibilidad es una ayuda de navegación, no un control de seguridad.

## API

- `GET /api/Auth/Admin/Sessions`: devuelve una página de sesiones de Galileo y SSecurity. Acepta `page`, `pageSize` (10, 25, 50 o 100), `application` (`all`, `galileo`, `ssecurity`), `status` (`all`, `active`, `revoked`, `expired`), `search`, `fromUtc` y `toUtc`. La búsqueda cubre usuario, nombre, IP observada, navegador y dispositivo. Las fechas filtran por fecha de inicio. La respuesta incluye contadores globales de activas, inicios recientes y revocaciones recientes.
- `DELETE /api/Auth/Admin/Sessions/{sessionId}`: recibe `{ "application": "galileo|ssecurity", "reason": "..." }`. El motivo es obligatorio y debe tener de 5 a 300 caracteres. No permite cerrar la sesión desde la cual se ejecuta la operación.

El API no expone el hash del refresh token. Un cierre administrativo actualiza la sesión con el motivo técnico `admin-revoked` y, dentro de la misma transacción, inserta el actor del JWT, la sesión y usuario afectados, aplicación, explicación, fecha UTC y resultado. Si la auditoría no se puede guardar, también se revierte el cierre. Los intentos de una sesión inexistente o ya cerrada quedan registrados como resultado. Un cierre remoto se hace efectivo para la otra aplicación en su siguiente petición protegida.

El control requiere `AuthSessions:Store=Sql`; en memoria responde 503 para no presentar una lista parcial ni permitir una revocación que no se compartiría entre instancias. La pantalla muestra únicamente datos de sesiones persistidas: no representa los intentos de inicio fallidos ni una bitácora de cada petición.

## Base de datos y despliegue

Antes de desplegar el API, aplicar `database/2026-10-16-auth-session-admin-audit.sql` en la base configurada como `DefaultConnString`, después de las migraciones existentes de `PGX_AuthSessions` y sus metadatos. El script es idempotente y crea la tabla e índices de auditoría; no ejecuta cambios en la tabla de sesiones ni elimina datos. Confirmar que producción mantiene `AuthSessions:Store=Sql`, publicar el API y luego compilar/publicar SSecurity.

La tabla de auditoría no configura purga automática. Definir y aplicar una política de retención de acuerdo con las necesidades operativas y regulatorias antes de acumular historial por periodos prolongados.
