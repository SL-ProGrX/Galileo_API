using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos
{
    public class FrmCcProcesoMensualPlanillaDB
    {
        private readonly PortalDB _portalDb;

        public FrmCcProcesoMensualPlanillaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de instituciones disponibles para el usuario en el proceso mensual de planilla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CC_ProcesoMensualPlanilla_Lista_Obtener(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                          ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            var criterios = ObtenerCriterios(filtros);

            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<CcProcesoMensualPlanillaListaDto>()
                }
            };

            try
            {
                if (string.IsNullOrWhiteSpace(criterios.usuario))
                {
                    response.Result ??= new TablasListaGenericaModel
                    {
                        total = 0,
                        lista = new List<CcProcesoMensualPlanillaListaDto>()
                    };

                    response.Result.total = 0;
                    response.Result.lista = new List<CcProcesoMensualPlanillaListaDto>();
                    return response;
                }

                var sortField = NormalizarSortField(filtros.sortField);
                var sortOrder = filtros.sortOrder == 1 ? 1 : 0;

                var pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
                var paginacion = filtros.paginacion < 0 ? 0 : filtros.paginacion;
                var usarPaginacion = paginacion > 0;
                var offset = usarPaginacion ? pagina * paginacion : 0;

                var likeDescripcion = string.IsNullOrWhiteSpace(criterios.descripcion)
                    ? null
                    : $"%{criterios.descripcion.Trim()}%";

                const string sqlCount = @"
                    select count(1)
                    from INSTITUCIONES I
                    inner join PRM_USUARIOS U
                        on U.COD_INSTITUCION = I.COD_INSTITUCION
                       and U.USUARIO = @usuario
                    where I.ACTIVA = @activa
                      and (@codigo is null or I.COD_INSTITUCION = @codigo)
                      and (
                            @descripcion is null
                            or I.DESCRIPCION like @likeDescripcion
                            or isnull(I.DESC_CORTA,'') like @likeDescripcion
                          );";

                response.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    usuario = criterios.usuario,
                    activa = criterios.activa,
                    codigo = criterios.codigo,
                    descripcion = string.IsNullOrWhiteSpace(criterios.descripcion) ? null : criterios.descripcion.Trim(),
                    likeDescripcion
                });

                var sql = @"
                    select
                        I.COD_INSTITUCION as cod_institucion,
                        isnull(I.DESC_CORTA,'') as desc_corta,
                        isnull(I.DESCRIPCION,'') as descripcion,
                        case isnull(I.FRECUENCIA,'M')
                            when 'M' then
                                concat(
                                    year(I.PR_FECHA_CORTE),
                                    right('00' + cast(month(I.PR_FECHA_CORTE) as varchar(2)), 2)
                                )
                            when 'Q' then
                                concat(
                                    year(I.PR_FECHA_CORTE),
                                    right('00' + cast(month(I.PR_FECHA_CORTE) as varchar(2)), 2),
                                    case when day(I.PR_FECHA_CORTE) > 15 then '.2' else '.1' end
                                )
                            else ''
                        end as proceso,
                        case dbo.fxPrm_Deduccion_Aplicada_Fecha(I.PR_FECHA_CORTE, I.COD_INSTITUCION)
                            when 0 then 'Pendiente'
                            when 1 then 'Enviada'
                            when 2 then 'Aplicada'
                            else ''
                        end as estado,
                        cast(dbo.fxPrm_Deduccion_Aplicada_Fecha(I.PR_FECHA_CORTE, I.COD_INSTITUCION) as smallint) as aplicada,
                        isnull(I.FRECUENCIA,'M') as frecuencia_id,
                        I.PR_FECHA_CORTE as pr_fecha_corte,
                        cast(isnull(I.ACTIVA,0) as smallint) as activa
                    from INSTITUCIONES I
                    inner join PRM_USUARIOS U
                        on U.COD_INSTITUCION = I.COD_INSTITUCION
                       and U.USUARIO = @usuario
                    where I.ACTIVA = @activa
                      and (@codigo is null or I.COD_INSTITUCION = @codigo)
                      and (
                            @descripcion is null
                            or I.DESCRIPCION like @likeDescripcion
                            or isnull(I.DESC_CORTA,'') like @likeDescripcion
                          )
                    order by
                        case when @sortField = 'cod_institucion' and @sortOrder = 1 then I.COD_INSTITUCION end asc,
                        case when @sortField = 'cod_institucion' and @sortOrder = 0 then I.COD_INSTITUCION end desc,

                        case when @sortField = 'desc_corta' and @sortOrder = 1 then isnull(I.DESC_CORTA,'') end asc,
                        case when @sortField = 'desc_corta' and @sortOrder = 0 then isnull(I.DESC_CORTA,'') end desc,

                        case when @sortField = 'descripcion' and @sortOrder = 1 then isnull(I.DESCRIPCION,'') end asc,
                        case when @sortField = 'descripcion' and @sortOrder = 0 then isnull(I.DESCRIPCION,'') end desc,

                        case when @sortField = 'proceso' and @sortOrder = 1 then
                            case isnull(I.FRECUENCIA,'M')
                                when 'M' then concat(
                                    year(I.PR_FECHA_CORTE),
                                    right('00' + cast(month(I.PR_FECHA_CORTE) as varchar(2)), 2)
                                )
                                when 'Q' then concat(
                                    year(I.PR_FECHA_CORTE),
                                    right('00' + cast(month(I.PR_FECHA_CORTE) as varchar(2)), 2),
                                    case when day(I.PR_FECHA_CORTE) > 15 then '.2' else '.1' end
                                )
                                else ''
                            end
                        end asc,

                        case when @sortField = 'proceso' and @sortOrder = 0 then
                            case isnull(I.FRECUENCIA,'M')
                                when 'M' then concat(
                                    year(I.PR_FECHA_CORTE),
                                    right('00' + cast(month(I.PR_FECHA_CORTE) as varchar(2)), 2)
                                )
                                when 'Q' then concat(
                                    year(I.PR_FECHA_CORTE),
                                    right('00' + cast(month(I.PR_FECHA_CORTE) as varchar(2)), 2),
                                    case when day(I.PR_FECHA_CORTE) > 15 then '.2' else '.1' end
                                )
                                else ''
                            end
                        end desc,

                        case when @sortField = 'estado' and @sortOrder = 1 then dbo.fxPrm_Deduccion_Aplicada_Fecha(I.PR_FECHA_CORTE, I.COD_INSTITUCION) end asc,
                        case when @sortField = 'estado' and @sortOrder = 0 then dbo.fxPrm_Deduccion_Aplicada_Fecha(I.PR_FECHA_CORTE, I.COD_INSTITUCION) end desc,

                        I.COD_INSTITUCION asc";

                if (usarPaginacion)
                {
                    sql += " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";
                }

                var lista = conn.Query<CcProcesoMensualPlanillaListaDto>(sql, new
                {
                    usuario = criterios.usuario,
                    activa = criterios.activa,
                    codigo = criterios.codigo,
                    descripcion = string.IsNullOrWhiteSpace(criterios.descripcion) ? null : criterios.descripcion.Trim(),
                    likeDescripcion,
                    sortField,
                    sortOrder,
                    offset,
                    fetch = paginacion
                }).ToList();

                response.Result ??= new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<CcProcesoMensualPlanillaListaDto>()
                };

                response.Result.total = response.Result.total;
                response.Result.lista = lista;

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
        }
        /// <summary>
        /// Exporta la lista completa de instituciones disponibles para el usuario en el proceso mensual de planilla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CC_ProcesoMensualPlanilla_Lista_Export(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                          ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CC_ProcesoMensualPlanilla_Lista_Obtener(
                CodEmpresa,
                JsonConvert.SerializeObject(filtros));
        }
        /// <summary>
        /// Obtiene los criterios de búsqueda para la consulta.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static CcProcesoMensualPlanillaFiltrosDto ObtenerCriterios(FiltrosLazyLoadData filtros)
        {
            var criterios = new CcProcesoMensualPlanillaFiltrosDto();

            if (filtros == null)
            {
                return criterios;
            }

            var filtro = (filtros.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return criterios;
            }

            if (filtro.StartsWith("{", StringComparison.Ordinal))
            {
                try
                {
                    var dto = JsonConvert.DeserializeObject<CcProcesoMensualPlanillaFiltrosDto>(filtro);
                    if (dto != null)
                    {
                        dto.usuario = (dto.usuario ?? string.Empty).Trim().ToUpperInvariant();
                        dto.descripcion = (dto.descripcion ?? string.Empty).Trim();
                        return dto;
                    }
                }
                catch (JsonException)
                {
                    return criterios;
                }
            }

            criterios.descripcion = filtro;
            return criterios;
        }
        /// <summary>
        /// Normaliza el campo de ordenamiento permitido.
        /// </summary>
        /// <param name="sortField"></param>
        /// <returns></returns>
        private static string NormalizarSortField(string? sortField)
        {
            return (sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "cod_institucion" => "cod_institucion",
                "desc_corta" => "desc_corta",
                "descripcion" => "descripcion",
                "proceso" => "proceso",
                "estado" => "estado",
                _ => "cod_institucion"
            };
        }
    }
}