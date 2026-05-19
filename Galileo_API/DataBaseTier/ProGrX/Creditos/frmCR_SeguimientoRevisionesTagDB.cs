using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito.Galileo_API.Models.ProGrX.Credito;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSeguimientoRevisionesTagDB
    {
        private readonly PortalDB _portalDB;
        private const string validaSolicitud = "Debe indicar una operación válida.";

        public FrmCrSeguimientoRevisionesTagDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de bancos disponibles para el filtro del formulario.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de bancos activos.</returns>
        public ErrorDto<List<CrSeguimientoRevisionesTagBancoRow>> Cr_SeguimientoRevisionesTag_Bancos_Obtener(
            int codEmpresa)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sql = """
            select
                rtrim(ID_BANCO) as id_banco,
                rtrim(DESCRIPCION) as descripcion
            from BANCOS
            where ESTADO = 'A'
            order by DESCRIPCION
            """;

                var lista = conn.Query<CrSeguimientoRevisionesTagBancoRow>(sql).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagBancoRow>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las etiquetas disponibles para el usuario en revisión.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario logueado.</param>
        /// <returns>Lista de etiquetas habilitadas para el usuario.</returns>
        public ErrorDto<List<CrSeguimientoRevisionesTagEtiquetaRow>> Cr_SeguimientoRevisionesTag_Etiquetas_Obtener(
            int codEmpresa,
            string usuario)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sql = """
            select
                rtrim(CT.TAG_CODIGO) as idx,
                '[' + rtrim(CT.TAG_CODIGO) + '] ' + rtrim(CT.DESCRIPCION) as descripcion
            from CRD_TAGS CT
            inner join CRD_TAGS_GRUPOS CTG on CT.TAG_CODIGO = CTG.TAG_CODIGO
            inner join CRD_GRPUSERS CGU on CTG.COD_GRUPO = CGU.COD_GRUPO
            where CT.ACTIVO = 1
              and CGU.USUARIO = @usuario
            order by CT.TAG_CODIGO
            """;

                var lista = conn.Query<CrSeguimientoRevisionesTagEtiquetaRow>(
                    sql,
                    new { usuario = (usuario ?? string.Empty).Trim() }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagEtiquetaRow>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista principal de operaciones pendientes de revisión.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtros de consulta del formulario.</param>
        /// <returns>Total y lista de operaciones.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagOperacionesResponse> Cr_SeguimientoRevisionesTag_Operaciones_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagOperacionesFiltrosRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagOperacionesResponse>(
                    "La solicitud es requerida.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var etiquetaFiltro = (request.etiqueta_filtro ?? string.Empty).Trim();
                var bancos = request.bancos ?? new List<string>();
                var bancosNormalizados = bancos
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                if (request.id_solicitud.HasValue && request.id_solicitud.Value > 0)
                {
                    const string sqlOperacion = """
                select
                    R.id_solicitud,
                    rtrim(R.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    rtrim(R.codigo) as codigo,
                    isnull(R.MONTOSOL, 0) as montosol,
                    isnull(R.CUOTA, 0) as cuota,
                    isnull(R.PLAZO, 0) as plazo,
                    isnull(R.INT, 0) as [int],
                    case R.ESTADOSOL
                        when 'R' then 'Recibido'
                        when 'P' then 'Pendiente'
                        else rtrim(R.ESTADOSOL)
                    end as estadosol,
                    R.FECHASOL as fechasol,
                    isnull(cast(RA.remesa as varchar(50)), '') as remesa,
                    isnull(rtrim(RE.USUARIO), '') as usuario_remesa
                from REG_CREDITOS R
                inner join SOCIOS S on S.cedula = R.cedula
                left join CRD_REMESA_ASG RA on R.id_solicitud = RA.id_solicitud
                left join CRD_REMESAS RE on RE.REMESA = RA.REMESA
                where R.ESTADOSOL = 'F'
                  and R.ID_SOLICITUD = @id_solicitud
                order by R.id_solicitud
                """;

                    var listaOperacion = conn.Query<CrSeguimientoRevisionesTagOperacionRow>(
                        sqlOperacion,
                        new { id_solicitud = request.id_solicitud.Value }).ToList();

                    return DbHelper.CreateOkResponse(new CrSeguimientoRevisionesTagOperacionesResponse
                    {
                        total = listaOperacion.Count,
                        lista = listaOperacion
                    });
                }

                var sql = """
            select top 3000
                R.id_solicitud,
                rtrim(R.cedula) as cedula,
                rtrim(S.nombre) as nombre,
                rtrim(R.codigo) as codigo,
                isnull(R.MONTOSOL, 0) as montosol,
                isnull(R.CUOTA, 0) as cuota,
                isnull(R.PLAZO, 0) as plazo,
                isnull(R.INT, 0) as [int],
                case R.ESTADOSOL
                    when 'R' then 'Recibido'
                    when 'P' then 'Pendiente'
                    else rtrim(R.ESTADOSOL)
                end as estadosol,
                R.FECHASOL as fechasol,
                isnull(cast(RA.remesa as varchar(50)), '') as remesa,
                isnull(rtrim(RE.USUARIO), '') as usuario_remesa
            from REG_CREDITOS R
            inner join CATALOGO C
                on R.codigo = C.codigo
               and C.poliza = 'N'
               and C.retencion = 'N'
            inner join SOCIOS S on S.cedula = R.cedula
            left join CRD_REMESA_ASG RA on R.id_solicitud = RA.id_solicitud
            left join CRD_REMESAS RE on RE.REMESA = RA.REMESA
            where isnull(R.ANALISTAS_REVISION, 0) = 0
              and R.ESTADOSOL = 'F'
              and R.REFERENCIA is null
            """;

                var parametros = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(etiquetaFiltro))
                {
                    sql += """
                 and dbo.fxCRDValidaTag(@etiqueta_filtro, R.id_solicitud) > 0
                """;
                    parametros.Add("@etiqueta_filtro", etiquetaFiltro);
                }

                if (request.solo_creditos_espera)
                {
                    sql += """
                 and R.EN_ESPERA_FECHA is not null
                """;
                }

                if (bancosNormalizados.Count > 0)
                {
                    sql += """
                 and R.COD_BANCO in @bancos
                """;
                    parametros.Add("@bancos", bancosNormalizados);
                }

                sql += """
             order by R.id_solicitud
            """;

                var lista = conn.Query<CrSeguimientoRevisionesTagOperacionRow>(sql, parametros).ToList();

                return DbHelper.CreateOkResponse(new CrSeguimientoRevisionesTagOperacionesResponse
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagOperacionesResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el encabezado y montos principales del detalle de la operación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Detalle principal del crédito.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagDetalleCreditoResponse> Cr_SeguimientoRevisionesTag_DetalleCredito_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagDetalleCreditoResponse>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sql = """
            select
                rtrim(R.cedula) as cedula,
                rtrim(S.nombre) as nombre,
                R.ID_SOLICITUD as id_solicitud,
                rtrim(G.DESCRIPCION) as garantia,
                isnull(R.MONTOAPR, 0) as montoapr,
                isnull(R.CUOTA, 0) as cuota,
                isnull(R.MONTO_GIRADO, 0) as monto_girado,
                isnull(dbo.fxCrdDesembolsosOperacion(R.ID_SOLICITUD), 0) as montodesembolsos,
                isnull(dbo.fxCrdRefundicionesOperacion(R.ID_SOLICITUD), 0) as montorefundicion,
                isnull(dbo.fxCrdRefundicionesCuotaOperacion(R.ID_SOLICITUD), 0) as refundicionescuota,
                cast(0 as decimal(18, 2)) as desembolsoscuota
            from REG_CREDITOS R
            inner join SOCIOS S on S.cedula = R.cedula
            inner join AFI_ESTADOS_PERSONA E on S.ESTADOACTUAL = E.COD_ESTADO
            inner join CRD_GARANTIA_TIPOS G on R.GARANTIA = G.GARANTIA
            where R.ID_SOLICITUD = @id_solicitud
            """;

                var result = conn.QueryFirstOrDefault<CrSeguimientoRevisionesTagDetalleCreditoResponse>(
                    sql,
                    new { id_solicitud = request.id_solicitud });

                if (result == null)
                {
                    return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagDetalleCreditoResponse>(
                        "No se encontró información para la operación indicada.");
                }

                result.total_cuotas = result.refundicionescuota + result.desembolsoscuota;
                result.diferencia_cuota = result.cuota - result.total_cuotas;

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagDetalleCreditoResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el historial de seguimiento por etiquetas aplicado a la operación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Historial de seguimiento registrado.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagSeguimientoResponse> Cr_SeguimientoRevisionesTag_Seguimiento_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagSeguimientoRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagSeguimientoResponse>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sql = """
            select
                rtrim(T.DESCRIPCION) as descripcion,
                isnull(rtrim(OT.NOTAS), '') as notas,
                OT.REGISTRO_FECHA as registro_fecha,
                isnull(rtrim(OT.REGISTRO_USUARIO), '') as registro_usuario
            from CRD_OPERACION_TAGS OT
            inner join CRD_TAGS T on OT.TAG_CODIGO = T.TAG_CODIGO
            where OT.ID_SOLICITUD = @id_solicitud
            order by OT.LINEA
            """;

                var lista = conn.Query<CrSeguimientoRevisionesTagSeguimientoRow>(
                    sql,
                    new { id_solicitud = request.id_solicitud }).ToList();

                return DbHelper.CreateOkResponse(new CrSeguimientoRevisionesTagSeguimientoResponse
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagSeguimientoResponse>(ex.Message);
            }
        }

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
                row_number() over(order by A.LINEA, A.ERROR_CODIGO) as linea,
                cast(0 as bit) as seleccionado,
                isnull(rtrim(A.ERROR_CODIGO), '') as error_codigo,
                isnull(rtrim(A.DESCRIPCION), '') as error_descripcion,
                case when isnull(A.APLICADO, 0) = 1 then 'S' else 'N' end as aplicado,
                isnull(rtrim(A.NOTA_DEFAULT), '') as nota_default
            from CRD_OPERACION_TAGS_REVISION A
            where A.ID_SOLICITUD = @id_solicitud
            order by A.LINEA, A.ERROR_CODIGO
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
                update A
                   set A.APLICADO = 1
                from CRD_OPERACION_TAGS_REVISION A
                where A.ID_SOLICITUD = @id_solicitud
                  and A.LINEA in @lineas
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
