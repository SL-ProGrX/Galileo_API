using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Nucleo;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_Nucleo
{
    /// <summary>
    /// Acceso a datos de frmSYS_APL_Integrador. Usa la conexión de empresa por defecto (PortalDB).
    /// Los parámetros de los SP se envían por posición, igual que el EXEC del formulario VB6.
    /// </summary>
    public class FrmSysAplIntegradorDB
    {
        private const string SqlDominio = "SELECT ISNULL(APL_Dominio, '') FROM SIF_EMPRESA";
        private const string SqlLineas = "EXEC spProGrX_APL_Lineas @dominio";
        private const string SqlTramiteLista =
            "EXEC spProGrX_APL_OPERACIONES_Lista @dominio, NULL, @estado, @usuario, @usuario_atiende, @linea_id, NULL, NULL, @atendidas";
        private const string SqlSolicitudLista =
            "EXEC spProGrX_APL_SOLICITUDES_Lista @dominio, NULL, @estado, @usuario, @usuario_atiende, @linea_id, NULL, NULL, @atendidas";
        private const string SqlTramiteAtiende =
            "EXEC spProGrX_APL_OPERACIONES_Atiende @dominio, @operacion, @usuario";
        private const string SqlSolicitudAtiende =
            "EXEC spProGrX_APL_SOLICITUDES_Atiende @dominio, @operacion, @usuario";
        private const string SqlMonitor = "EXEC spProGrX_APL_Monitor @dominio";

        private readonly PortalDB _portalDB;

        public FrmSysAplIntegradorDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el dominio APL de la empresa (select APL_Dominio from SIF_EMPRESA en Form_Load).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        private ErrorDto<string?> SYS_APL_Integrador_Dominio_Obtiene(int codEmpresa)
        {
            return DbHelper.ExecuteSingleQuery<string>(_portalDB, codEmpresa, SqlDominio, string.Empty);
        }

        /// <summary>
        /// Arma los parámetros de las listas. V6 envía el usuario en dos posiciones del SP.
        /// </summary>
        private static object ArmarParametrosLista(string? dominio, AplListaRequest request)
        {
            return new
            {
                dominio,
                request.estado,
                request.usuario,
                usuario_atiende = request.usuario,
                request.linea_id,
                atendidas = request.atendidas ? 1 : 0
            };
        }

        /// <summary>
        /// Lista las líneas del combo Línea (spProGrX_APL_Lineas).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        public ErrorDto<List<AplLineaDto>> SYS_APL_Integrador_Lineas_Lista(int codEmpresa)
        {
            var dominio = SYS_APL_Integrador_Dominio_Obtiene(codEmpresa);
            if (dominio.Code == -1)
            {
                return DbHelper.CreateErrorResponse<List<AplLineaDto>>(dominio.Description ?? string.Empty);
            }

            var filas = DbHelper.ExecuteListQuery<AplLineaRaw>(
                _portalDB, codEmpresa, SqlLineas, new { dominio = dominio.Result });
            if (filas.Code == -1)
            {
                return DbHelper.CreateErrorResponse<List<AplLineaDto>>(filas.Description ?? string.Empty);
            }

            var lineas = (filas.Result ?? new List<AplLineaRaw>())
                .Select(f => new AplLineaDto
                {
                    item_id = Convert.ToString(f.item_id, CultureInfo.InvariantCulture),
                    item_desc = f.item_desc
                })
                .ToList();

            return DbHelper.CreateOkResponse(lineas);
        }

        /// <summary>
        /// Lista "Solicitudes en Trámite" (sbLista_Tramite, spProGrX_APL_OPERACIONES_Lista).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Estado, línea, atendidas y usuario.</param>
        public ErrorDto<List<AplTramiteDto>> SYS_APL_Integrador_Tramite_Lista(int codEmpresa, AplListaRequest request)
        {
            var dominio = SYS_APL_Integrador_Dominio_Obtiene(codEmpresa);
            if (dominio.Code == -1)
            {
                return DbHelper.CreateErrorResponse<List<AplTramiteDto>>(dominio.Description ?? string.Empty);
            }

            return DbHelper.ExecuteListQuery<AplTramiteDto>(
                _portalDB, codEmpresa, SqlTramiteLista, ArmarParametrosLista(dominio.Result, request));
        }

        /// <summary>
        /// Lista "Consultas" (sbLista_Solicitudes, spProGrX_APL_SOLICITUDES_Lista).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Estado, línea, atendidas y usuario.</param>
        public ErrorDto<List<AplSolicitudDto>> SYS_APL_Integrador_Solicitud_Lista(int codEmpresa, AplListaRequest request)
        {
            var dominio = SYS_APL_Integrador_Dominio_Obtiene(codEmpresa);
            if (dominio.Code == -1)
            {
                return DbHelper.CreateErrorResponse<List<AplSolicitudDto>>(dominio.Description ?? string.Empty);
            }

            return DbHelper.ExecuteListQuery<AplSolicitudDto>(
                _portalDB, codEmpresa, SqlSolicitudLista, ArmarParametrosLista(dominio.Result, request));
        }

        /// <summary>
        /// Registra una operación en trámite como atendiendo (spProGrX_APL_OPERACIONES_Atiende).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="apl_operacion">Número de operación APL.</param>
        /// <param name="usuario">Usuario que atiende.</param>
        public ErrorDto SYS_APL_Integrador_Tramite_Atiende(int codEmpresa, long apl_operacion, string usuario)
        {
            return SYS_APL_Integrador_Atiende(codEmpresa, SqlTramiteAtiende, apl_operacion, usuario);
        }

        /// <summary>
        /// Registra una solicitud de consulta como atendiendo (spProGrX_APL_SOLICITUDES_Atiende).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="num_solicitud">Número de solicitud.</param>
        /// <param name="usuario">Usuario que atiende.</param>
        public ErrorDto SYS_APL_Integrador_Solicitud_Atiende(int codEmpresa, long num_solicitud, string usuario)
        {
            return SYS_APL_Integrador_Atiende(codEmpresa, SqlSolicitudAtiende, num_solicitud, usuario);
        }

        /// <summary>
        /// Ejecuta el SP "Atiende" indicado con dominio, número de caso y usuario.
        /// </summary>
        private ErrorDto SYS_APL_Integrador_Atiende(int codEmpresa, string sql, long operacion, string usuario)
        {
            var dominio = SYS_APL_Integrador_Dominio_Obtiene(codEmpresa);
            if (dominio.Code == -1)
            {
                return DbHelper.ErrorResponse(dominio.Description ?? string.Empty);
            }

            return DbHelper.ExecuteNonQuery(
                _portalDB, codEmpresa, sql, new { dominio = dominio.Result, operacion, usuario });
        }

        /// <summary>
        /// Consulta los casos pendientes por resolver (TimerX_Timer, spProGrX_APL_Monitor).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        public ErrorDto<AplMonitorDto?> SYS_APL_Integrador_Monitor(int codEmpresa)
        {
            var dominio = SYS_APL_Integrador_Dominio_Obtiene(codEmpresa);
            if (dominio.Code == -1)
            {
                return DbHelper.CreateErrorResponse<AplMonitorDto?>(dominio.Description ?? string.Empty);
            }

            return DbHelper.ExecuteSingleQuery<AplMonitorDto>(
                _portalDB, codEmpresa, SqlMonitor, new AplMonitorDto(), new { dominio = dominio.Result });
        }
    }
}
