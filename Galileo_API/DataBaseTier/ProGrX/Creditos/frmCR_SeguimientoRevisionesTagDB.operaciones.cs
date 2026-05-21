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

    }
}