using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRSeguimientoRegTagDB
    {
        private readonly PortalDB _portalDB;

        public FrmCRSeguimientoRegTagDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las etiquetas activas con requisito asignado disponibles para el usuario.
        /// </summary>
        /// <param name="codEmpresa">Empresa de la sesión.</param>
        /// <param name="usuario">Usuario autenticado.</param>
        /// <returns>Etiquetas disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_SeguimientoRegTag_Etiquetas_Obtener(
            int codEmpresa,
            string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                var result = conn.Query<DropDownListaGenericaModel>(SqlEtiquetas, new
                {
                    usuario = (usuario ?? string.Empty).Trim().ToUpperInvariant()
                }).ToList();
                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message, -1, new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Consulta las solicitudes pendientes del requisito asociado a una etiqueta.
        /// </summary>
        /// <param name="codEmpresa">Empresa de la sesión.</param>
        /// <param name="request">Filtros de la consulta.</param>
        /// <returns>Solicitudes encontradas.</returns>
        public ErrorDto<List<CrSeguimientoRegTagOperacionDto>> CR_SeguimientoRegTag_Operaciones_Obtener(
            int codEmpresa,
            CrSeguimientoRegTagConsultaRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.tag_codigo))
            {
                return ErrorLista("Debe seleccionar una etiqueta.");
            }

            if (request.fecha_fin.Date < request.fecha_inicio.Date)
            {
                return ErrorLista("La fecha final no puede ser menor que la fecha inicial.");
            }

            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                var result = conn.Query<CrSeguimientoRegTagOperacionDto>(SqlOperaciones, new
                {
                    tagCodigo = request.tag_codigo.Trim().ToUpperInvariant(),
                    fechaInicio = request.fecha_inicio.Date,
                    fechaFin = request.fecha_fin.Date.AddDays(1),
                    estado = NormalizarEstado(request.estado)
                }).ToList();
                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return ErrorLista(ex.Message);
            }
        }

        /// <summary>
        /// Aplica la etiqueta a las operaciones seleccionadas mediante el proceso legado.
        /// </summary>
        /// <param name="codEmpresa">Empresa de la sesión.</param>
        /// <param name="request">Etiqueta, observación y operaciones seleccionadas.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto CR_SeguimientoRegTag_Aplicar(
            int codEmpresa,
            CrSeguimientoRegTagAplicarRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.tag_codigo))
            {
                return DbHelper.ErrorResponse("Debe seleccionar una etiqueta.");
            }

            if (request.operaciones == null || request.operaciones.Count == 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar al menos una operación.");
            }

            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using var transaction = conn.BeginTransaction();
            try
            {
                foreach (var operacion in request.operaciones
                    .Where(item => item.id_solicitud > 0)
                    .GroupBy(item => item.id_solicitud)
                    .Select(group => group.First()))
                {
                    conn.Execute(
                        @"exec spCrdOperacionTagRegistra
                            @idSolicitud, @linea, @tag, @usuario, @asignado, @notas;",
                        new
                        {
                            operacion.id_solicitud,
                            linea = (operacion.codigo ?? string.Empty).Trim(),
                            tag = request.tag_codigo.Trim().ToUpperInvariant(),
                            usuario = (request.usuario ?? string.Empty).Trim().ToUpperInvariant(),
                            asignado = string.Empty,
                            notas = (request.observacion ?? string.Empty).Trim()
                        },
                        transaction,
                        commandType: CommandType.Text);
                }

                transaction.Commit();
                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                transaction.Rollback();
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static ErrorDto<List<CrSeguimientoRegTagOperacionDto>> ErrorLista(string mensaje)
        {
            return DbHelper.CreateErrorResponse<List<CrSeguimientoRegTagOperacionDto>>(
                mensaje, -1, new List<CrSeguimientoRegTagOperacionDto>());
        }

        private static string NormalizarEstado(string? estado)
        {
            return estado switch
            {
                "Recibida" => "R",
                "Pendiente" => "P",
                _ => "Todos"
            };
        }

        private const string SqlEtiquetas = @"
            select distinct
                rtrim(CT.TAG_CODIGO) as item,
                rtrim(CT.TAG_CODIGO) + ' - ' + rtrim(CT.DESCRIPCION) as descripcion
            from CRD_TAGS CT
            inner join CRD_TAGS_GRUPOS CTG on CT.TAG_CODIGO = CTG.TAG_CODIGO
            inner join CRD_GRPUSERS CGU on CTG.COD_GRUPO = CGU.COD_GRUPO
            where CT.ACTIVO = 1
              and isnull(CT.COD_REQUISITO, '') <> ''
              and CGU.USUARIO = @usuario
            order by item;";

        private const string SqlOperaciones = @"
            select distinct
                RC.ID_SOLICITUD as id_solicitud,
                rtrim(RC.CEDULA) as cedula,
                rtrim(S.NOMBRE) as nombre,
                rtrim(RC.CODIGO) as codigo,
                isnull(RC.MONTOSOL, 0) as montosol,
                isnull(RC.CUOTA, 0) as cuota,
                isnull(RC.PLAZO, 0) as plazo,
                isnull(RC.INT, 0) as tasa,
                case RC.ESTADOSOL
                    when 'R' then 'Recibido'
                    when 'P' then 'Pendiente'
                    else rtrim(RC.ESTADOSOL)
                end as estado,
                RC.FECHASOL as fechasol,
                rtrim(Ofi.DESCRIPCION) as oficina
            from REG_CREDITOS RC
            inner join SOCIOS S on RC.CEDULA = S.CEDULA
            inner join OPERACION_REQUISITOS ORE on RC.ID_SOLICITUD = ORE.ID_SOLICITUD
            inner join CRD_TAGS CT on CT.COD_REQUISITO = ORE.COD_REQUISITO
            inner join SIF_OFICINAS Ofi on RC.COD_OFICINA_R = Ofi.COD_OFICINA
            where ORE.ESTADO = 0
              and CT.TAG_CODIGO = @tagCodigo
              and RC.FECHASOL >= @fechaInicio
              and RC.FECHASOL < @fechaFin
              and RC.ESTADOSOL in ('P', 'R')
              and (@estado = 'Todos' or RC.ESTADOSOL = @estado)
            order by RC.ID_SOLICITUD;";
    }
}
