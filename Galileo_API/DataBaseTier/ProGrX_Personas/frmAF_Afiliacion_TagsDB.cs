using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFAfiliacionTagsDB
    {
        private readonly IConfiguration _config;

        private const string SpAfiliacionesControlConsulta = "spAFI_Afiliaciones_Control_Consulta";
        private const string SpAfiliacionRecepcionAplica = "spAFI_Afiliacion_Recepcion_Aplica";
        private const string SpAfiliacionRevisionAplica = "spAFI_Afiliacion_Revision_Aplica";
        private const string SpAfiliacionesEtiquetasConsulta = "spAFI_Afiliaciones_Etiquetas_Consulta";
        private const string SpAfiliacionRevisionReversar = "spAFI_Afiliacion_Revision_Reversar";

        private const string SqlBoletasPendientesRecibir = @"
                select CONSEC, CEDULA, NOMBRE, Tipo_Desc
                from vAFI_Afiliaciones_Pendientes_Recibir";

        private const string SqlBoletasAfiliacionesList = @"
                select CONSEC, CEDULA, NOMBRE, tipo_desc
                from vAFI_Afiliaciones_List";

        private const string SqlRevisionReversarValida = @"
                select dbo.fxAFI_Afiliacion_Revision_Reversar_Valida(@boleta)";

        public FrmAFAfiliacionTagsDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene las afiliaciones pendientes de recepción.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="estado">Estado usado por el procedimiento.</param>
        /// <param name="filtro">Filtro textual aplicado a la consulta.</param>
        /// <returns>Lista de afiliaciones pendientes de recepción.</returns>
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Recepcion(int CodEmpresa, string estado, string filtro)
        {
            return ConsultarAfiliacionesControl(CodEmpresa, estado, filtro);
        }

        /// <summary>
        /// Obtiene las afiliaciones recibidas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="estado">Estado usado por el procedimiento.</param>
        /// <param name="filtro">Filtro textual aplicado a la consulta.</param>
        /// <returns>Lista de afiliaciones recibidas.</returns>
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Recibidas(int CodEmpresa, string estado, string filtro)
        {
            return ConsultarAfiliacionesControl(CodEmpresa, estado, filtro);
        }

        /// <summary>
        /// Obtiene las afiliaciones pendientes de revisión.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="estado">Estado usado por el procedimiento.</param>
        /// <param name="filtro">Filtro textual aplicado a la consulta.</param>
        /// <returns>Lista de afiliaciones pendientes de revisión.</returns>
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Pendientes(int CodEmpresa, string estado, string filtro)
        {
            return ConsultarAfiliacionesControl(CodEmpresa, estado, filtro);
        }

        /// <summary>
        /// Obtiene boletas de afiliación pendientes de recibir.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Lista de boletas pendientes de recibir.</returns>
        public ErrorDto<List<AfBoletasAfiliacion>> AF_CR_BoletasAfiliacion_Obtener(int CodEmpresa)
        {
            return ConsultarBoletasAfiliacion(CodEmpresa, SqlBoletasPendientesRecibir);
        }

        /// <summary>
        /// Aplica recepción a una boleta de afiliación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="boleta">Consecutivo de boleta.</param>
        /// <param name="usuario">Usuario que aplica la recepción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AFI_Afiliacion_Recepcion_Aplica(int codEmpresa, int boleta, string usuario)
        {
            return EjecutarStoredProcedure(
                codEmpresa,
                SpAfiliacionRecepcionAplica,
                new
                {
                    BoletaId = boleta,
                    Usuario = usuario,
                    Notas = $"Recibe Afiliacion No. {boleta}",
                    Equipo = "Web",
                    Version = "Galileo Web"
                });
        }

        /// <summary>
        /// Aplica revisión satisfactoria o pendiente a una afiliación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="consec">Consecutivo de boleta.</param>
        /// <param name="estado">Estado a aplicar: P para revisado, E para pendiente.</param>
        /// <param name="usuario">Usuario que aplica la revisión.</param>
        /// <param name="nota">Nota de pendiente.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AFI_Afiliacion_Revision_Aplica(int codEmpresa, int consec, string estado, string usuario, string nota)
        {
            return EjecutarStoredProcedure(
                codEmpresa,
                SpAfiliacionRevisionAplica,
                new
                {
                    BoletaId = consec,
                    Estado = estado,
                    Usuario = usuario,
                    Notas = nota ?? string.Empty,
                    Equipo = "Web",
                    Version = "Galileo Web"
                });
        }

        /// <summary>
        /// Consulta la bitácora de etiquetas de una afiliación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="boleta">Consecutivo de boleta.</param>
        /// <returns>Lista de etiquetas registradas.</returns>
        public ErrorDto<List<AfiEtiquetaDto>> AFI_Afiliaciones_Etiquetas_Consulta(int CodEmpresa, int boleta)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfiEtiquetaDbRow>(
                    $"EXEC {SpAfiliacionesEtiquetasConsulta} @BoletaId",
                    new { BoletaId = boleta })
                    .Select(MapearEtiqueta)
                    .ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfiEtiquetaDto>())
                : DbHelper.CreateErrorResponse<List<AfiEtiquetaDto>>(
                    result.Description ?? "Error al consultar bitácora de etiquetas.");
        }

        /// <summary>
        /// Reversa una revisión satisfactoria cuando la afiliación no ha sido remesada.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="boleta">Consecutivo de boleta.</param>
        /// <param name="usuario">Usuario que aplica la reversión.</param>
        /// <param name="nota">Nota de reversión.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AFI_Afiliacion_Revision_Reversar(int CodEmpresa, int boleta, string usuario, string nota)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var valida = connection.QuerySingle<int>(SqlRevisionReversarValida, new { boleta });
                if (valida == 0)
                {
                    return DbHelper.ErrorResponse("No procede la reversión, la afiliación ya fue remesada", -2);
                }

                connection.Execute(
                    $"exec {SpAfiliacionRevisionReversar} @BoletaId, @Usuario, @NotasReversa, @Equipo, @Version",
                    new
                    {
                        BoletaId = boleta,
                        Usuario = usuario,
                        NotasReversa = nota,
                        Equipo = "Web",
                        Version = "Galileo Web"
                    });

                return DbHelper.OkResponse("Ok");
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al reversar revisión de afiliación.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Agrega la recepción de una afiliación desde el campo de boleta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="boleta">Consecutivo de boleta.</param>
        /// <param name="usuario">Usuario que aplica la recepción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AFI_Afiliacion_Recepcion_Agregar(int CodEmpresa, int boleta, string usuario)
        {
            return AFI_Afiliacion_Recepcion_Aplica(CodEmpresa, boleta, usuario);
        }

        /// <summary>
        /// Obtiene lista general de boletas de afiliación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Lista de boletas de afiliación.</returns>
        public ErrorDto<List<AfBoletasAfiliacion>> AF_BoletasAfiliacionLista_Obtener(int CodEmpresa)
        {
            return ConsultarBoletasAfiliacion(CodEmpresa, SqlBoletasAfiliacionesList);
        }

        private ErrorDto<List<AfiAfiliacionControlDto>> ConsultarAfiliacionesControl(int codEmpresa, string estado, string filtro)
        {
            return DbHelper.ExecuteListQuery<AfiAfiliacionControlDto>(
                CreatePortalDb(),
                codEmpresa,
                $"EXEC {SpAfiliacionesControlConsulta} @Estado",
                new
                {
                    Estado = estado,
                });
        }

        private ErrorDto<List<AfBoletasAfiliacion>> ConsultarBoletasAfiliacion(int codEmpresa, string sql)
        {
            return DbHelper.ExecuteListQuery<AfBoletasAfiliacion>(
                CreatePortalDb(),
                codEmpresa,
                sql);
        }

        private ErrorDto EjecutarStoredProcedure(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(
                    storedProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? $"Error al ejecutar {storedProcedure}.", result.Code.GetValueOrDefault(-1));
        }

        private PortalDB CreatePortalDb() => new(_config);

        private static AfiEtiquetaDto MapearEtiqueta(AfiEtiquetaDbRow row)
        {
            return new AfiEtiquetaDto
            {
                id = row.ID,
                tag_desc = row.TAG_DESC,
                fecha_etiqueta = row.Fecha_Format,
                usuario_etiqueta = row.REGISTRO_USUARIO,
                observacion = row.OBSERVACION,
                tipo_desc = null,
                cedula = row.CEDULA,
                nombre = row.NOMBRE
            };
        }

        private sealed record AfiEtiquetaDbRow(
            int CODIGO,
            int ID,
            string? CEDULA,
            string? NOMBRE,
            string? TAG_DESC,
            string? COD_ETIQUETA,
            DateTime FECHA_ETIQUETA,
            string? USUARIO_ETIQUETA,
            string? OBSERVACION,
            string? REGISTRO_USUARIO,
            DateTime REGISTRO_FECHA,
            string? Fecha_Format);
    }
}
