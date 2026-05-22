using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;
using Galileo_API.Models.ProGrX.Credito.Galileo_API.Models.ProGrX.Credito;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoRevisionesTagDB
    {
        /// <summary>
        /// Obtiene la longitud mínima requerida para la nota de la etiqueta seleccionada.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="tagCodigo">Código de la etiqueta.</param>
        /// <returns>Longitud mínima requerida.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagNotaLargoResponse> Cr_SeguimientoRevisionesTag_NotaLargo_Obtener(
            int codEmpresa,
            string tagCodigo)
        {
            if (string.IsNullOrWhiteSpace(tagCodigo))
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagNotaLargoResponse>(
                    "Debe indicar una etiqueta válida.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sql = """
                    select
                        isnull(Nota_Largo, 0) as nota_largo
                    from CRD_TAGS
                    where TAG_CODIGO = @tagCodigo
                    """;

                var result = conn.QueryFirstOrDefault<CrSeguimientoRevisionesTagNotaLargoResponse>(
                    sql,
                    new { tagCodigo = tagCodigo.Trim() }) ?? new CrSeguimientoRevisionesTagNotaLargoResponse();

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagNotaLargoResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el mensaje o aviso por defecto asociado a la etiqueta seleccionada.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="tagCodigo">Código de la etiqueta.</param>
        /// <returns>Mensaje asociado a la etiqueta.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagAvisoResponse> Cr_SeguimientoRevisionesTag_Aviso_Obtener(
            int codEmpresa,
            string tagCodigo)
        {
            if (string.IsNullOrWhiteSpace(tagCodigo))
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagAvisoResponse>(
                    "Debe indicar una etiqueta válida.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sql = """
                    select
                        isnull(MENSAJE, '') as mensaje
                    from CRD_TAGS_AVISOS
                    where TAG_CODIGO = @tagCodigo
                    """;

                var result = conn.QueryFirstOrDefault<CrSeguimientoRevisionesTagAvisoResponse>(
                    sql,
                    new { tagCodigo = tagCodigo.Trim() }) ?? new CrSeguimientoRevisionesTagAvisoResponse();

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagAvisoResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la información necesaria para el tab de revisión.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Usuario, tag de revisión, estado de revisión y lista de errores.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagRevisionResponse> Cr_SeguimientoRevisionesTag_Revision_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagRevisionResponse>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var response = new CrSeguimientoRevisionesTagRevisionResponse();

                const string sqlRevision = """
                    select
                        cast(isnull(valor, '') as varchar(50)) as tag_revision
                    from CRD_PARAMETROS
                    where cod_parametro = '26'
                    """;

                response.tag_revision = conn.QueryFirstOrDefault<string>(sqlRevision) ?? string.Empty;

                const string sqlOperacion = """
                    select
                        case when isnull(ANALISTAS_REVISION, 0) = 1 then cast(1 as bit) else cast(0 as bit) end as operacion_revisada
                    from REG_CREDITOS
                    where ID_SOLICITUD = @id_solicitud
                    """;

                response.operacion_revisada = conn.QueryFirstOrDefault<bool>(
                    sqlOperacion,
                    new { id_solicitud = request.id_solicitud });

                const string sqlErrores = """
                        select
                            row_number() over(order by E.ID_ERROR) as linea,
                            cast(0 as bit) as seleccionado,
                            E.ID_ERROR as id_error,
                            isnull(rtrim(E.DESCRIPCION), '') as descripcion,
                            isnull(ER.APLICADO, 'N') as aplicado,
                            isnull(rtrim(E.MENSAJE), '') as mensaje
                        from CRD_ANALISIS_ERRORES E
                        left join CRD_ANALISIS_ERRORESREG ER
                            on E.ID_ERROR = ER.ID_ERROR
                           and ER.ID_SOLICITUD = @id_solicitud
                        where E.ACTIVO = '1'
                        order by E.ID_ERROR
                        """;

                response.errores = conn.Query<CrSeguimientoRevisionesTagErrorRow>(
                    sqlErrores,
                    new { id_solicitud = request.id_solicitud }).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagRevisionResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Aplica la revisión de la operación, registra la etiqueta y marca el crédito como revisado.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario logueado.</param>
        /// <param name="request">Datos de la revisión a aplicar.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagAplicarResponse> Cr_SeguimientoRevisionesTag_Aplicar(
            int codEmpresa,
            string usuario,
            CrSeguimientoRevisionesTagAplicarRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagAplicarResponse>(
                    "La solicitud es requerida.");
            }

            if (request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagAplicarResponse>(
                    validaSolicitud);
            }

            if (string.IsNullOrWhiteSpace(request.tag_codigo))
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagAplicarResponse>(
                    "Debe indicar una etiqueta válida.");
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagAplicarResponse>(
                    "Debe indicar un usuario válido.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();

                var usuarioNormalizado = usuario.Trim();
                var tagCodigo = request.tag_codigo.Trim();
                var observacion = request.observacion?.Trim() ?? string.Empty;

                const string sqlNotaLargo = """
                    select isnull(Nota_Largo, 0)
                    from CRD_TAGS
                    where TAG_CODIGO = @tagCodigo
                    """;

                var notaLargo = conn.QueryFirstOrDefault<int>(
                    sqlNotaLargo,
                    new { tagCodigo },
                    tx);

                if (notaLargo > observacion.Length)
                {
                    return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagAplicarResponse>(
                        $"Este tipo de etiqueta requiere que la nota sea de al menos {notaLargo} caracteres.");
                }

                const string sqlLinea = """
                    select isnull(max(LINEA), 0) + 1
                    from CRD_OPERACION_TAGS
                    where ID_SOLICITUD = @id_solicitud
                    """;

                var linea = conn.QueryFirstOrDefault<int>(
                    sqlLinea,
                    new { id_solicitud = request.id_solicitud },
                    tx);

                const string sqlInsertTag = """
                    insert into CRD_OPERACION_TAGS
                    (
                        ID_SOLICITUD,
                        LINEA,
                        TAG_CODIGO,
                        NOTAS,
                        REGISTRO_FECHA,
                        REGISTRO_USUARIO
                    )
                    values
                    (
                        @id_solicitud,
                        @linea,
                        @tag_codigo,
                        @notas,
                        getdate(),
                        @registro_usuario
                    )
                    """;

                conn.Execute(
                    sqlInsertTag,
                    new
                    {
                        id_solicitud = request.id_solicitud,
                        linea,
                        tag_codigo = tagCodigo,
                        notas = observacion,
                        registro_usuario = usuarioNormalizado
                    },
                    tx);

                if (request.errores_seleccionados?.Count > 0)
                {
                    const string sqlAplicarErrores = """
                        update CRD_ANALISIS_ERRORESREG
                           set APLICADO = 'S'
                        where ID_SOLICITUD = @id_solicitud
                        """;

                    conn.Execute(
                        sqlAplicarErrores,
                        new
                        {
                            id_solicitud = request.id_solicitud,
                            lineas = request.errores_seleccionados.Distinct().ToList()
                        },
                        tx);
                }

                const string sqlUpdateCredito = """
                    update REG_CREDITOS
                       set ANALISTAS_REVISION = 1
                    where ID_SOLICITUD = @id_solicitud
                    """;

                conn.Execute(
                    sqlUpdateCredito,
                    new { id_solicitud = request.id_solicitud },
                    tx);

                tx.Commit();

                return DbHelper.CreateOkResponse(new CrSeguimientoRevisionesTagAplicarResponse
                {
                    aplicado = true,
                    analistas_revision = true,
                    mensaje = "Revisión aplicada correctamente."
                });
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagAplicarResponse>(ex.Message);
            }
        }
    }
}