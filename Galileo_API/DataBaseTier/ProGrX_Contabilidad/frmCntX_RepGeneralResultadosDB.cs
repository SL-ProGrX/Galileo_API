using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepGeneralResultadosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCntXCalculosDb _mCntXCalculosDb;

        public FrmCntXRepGeneralResultadosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mCntXCalculosDb = new MCntXCalculosDb(config);
        }

        /// <summary>
        /// Obtiene unidades para el dropdown, agregando [CONSOLIDADO].
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Dropdown_Obtener(int CodEmpresa, int codContabilidad)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(cod_unidad) as item,
                        rtrim(descripcion) as descripcion
                    from CntX_Unidades
                    where cod_contabilidad = @codContabilidad
                    order by descripcion;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new { codContabilidad }).ToList();

                lista.Insert(0, new DropDownListaGenericaModel
                {
                    item = "[CONSOLIDADO]",
                    descripcion = "[CONSOLIDADO]"
                });

                return lista;
            });
        }

        /// <summary>
        /// Obtiene centros de costo según unidad seleccionada, agregando TODOS.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCosto_Dropdown_Obtener(int CodEmpresa, int codContabilidad, string? codUnidad)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                codUnidad = (codUnidad ?? string.Empty).Trim();

                const string sqlConsolidado = @"
                    select
                        rtrim(cod_centro_costo) as item,
                        rtrim(descripcion) as descripcion
                    from CntX_Centro_Costos
                    where cod_contabilidad = @codContabilidad
                    order by descripcion;";

                const string sqlUnidad = @"
                    select
                        rtrim(cc.cod_centro_costo) as item,
                        rtrim(cc.descripcion) as descripcion
                    from CntX_Centro_Costos cc
                    where cc.cod_contabilidad = @codContabilidad
                      and cc.cod_centro_costo in (
                            select ucc.cod_centro_costo
                            from CntX_Unidades_CC ucc
                            where ucc.cod_contabilidad = @codContabilidad
                              and ucc.cod_unidad = @codUnidad
                      )
                    order by cc.descripcion;";

                List<DropDownListaGenericaModel> lista;

                if (string.IsNullOrWhiteSpace(codUnidad) || codUnidad == "[CONSOLIDADO]")
                {
                    lista = conn.Query<DropDownListaGenericaModel>(sqlConsolidado, new
                    {
                        codContabilidad
                    }).ToList();
                }
                else
                {
                    lista = conn.Query<DropDownListaGenericaModel>(sqlUnidad, new
                    {
                        codContabilidad,
                        codUnidad
                    }).ToList();
                }

                lista.Insert(0, new DropDownListaGenericaModel
                {
                    item = "TODOS",
                    descripcion = "TODOS"
                });

                return lista;
            });
        }

        /// <summary>
        /// Valida contexto del reporte y devuelve valores calculados para fórmulas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto<CntXRepGeneralResultadosValidarResponseDto> CntX_RepGeneralResultados_ValidarReporte(int CodEmpresa,CntXRepGeneralResultadosValidarRequestDto data)
        {
            var response = DbHelper.CreateOkResponse(new CntXRepGeneralResultadosValidarResponseDto());

            response.Result ??= new CntXRepGeneralResultadosValidarResponseDto();

            try
            {
                if (data.cod_contabilidad <= 0)
                {
                    return new ErrorDto<CntXRepGeneralResultadosValidarResponseDto>
                    {
                        Code = -2,
                        Description = "La contabilidad es requerida.",
                        Result = new CntXRepGeneralResultadosValidarResponseDto()
                    };
                }

                var unidad = string.IsNullOrWhiteSpace(data.unidad)
                    ? "[CONSOLIDADO]"
                    : data.unidad.Trim();

                var centroCosto = string.IsNullOrWhiteSpace(data.centro_costo)
                    ? "TODOS"
                    : data.centro_costo.Trim();

                var nivel = string.IsNullOrWhiteSpace(data.nivel)
                    ? "2"
                    : data.nivel.Trim();

                var periodoAbierto = _mCntXCalculosDb.FxCntX_PeriodoVerifica(
                    CodEmpresa,
                    data.cod_contabilidad,
                    data.periodo_anio,
                    data.periodo_mes);

                if (data.chk_preliminar == 1 && !periodoAbierto)
                {
                    return new ErrorDto<CntXRepGeneralResultadosValidarResponseDto>
                    {
                        Code = -2,
                        Description = "El período está cerrado y no permite preliminares.",
                        Result = new CntXRepGeneralResultadosValidarResponseDto()
                    };
                }

                var periodoDesc = MCntXCalculosDb.FxCntX_PeriodoDesc(
                    data.periodo_anio,
                    data.periodo_mes);

                var esMesFiscal = _mCntXCalculosDb.FxCntX_MesFiscal(
                    CodEmpresa,
                    data.cod_contabilidad,
                    data.periodo_anio,
                    data.periodo_mes);

                var subtitulo = $"PERIODO: {periodoDesc} {(periodoAbierto ? "[PENDIENTE]" : "[CERRADO]")} [Nivel {nivel}]";
                subtitulo += $"  Unidad: {unidad}   Centro Costo: {centroCosto}";

                var fxUnidad = string.Empty;

                if (unidad != "[CONSOLIDADO]" && centroCosto == "TODOS")
                {
                    fxUnidad = unidad;
                }
                else if (centroCosto != "TODOS")
                {
                    fxUnidad = $"{unidad}     Centro de Costos: {centroCosto}";
                }

                response.Result.periodo_desc = periodoDesc;
                response.Result.periodo_abierto = periodoAbierto ? 1 : 0;
                response.Result.es_mes_fiscal = esMesFiscal ? 1 : 0;
                response.Result.fx_asiento_cierre = esMesFiscal ? 1 : 0;
                response.Result.fx_muestra_titulo = data.chk_titulos == 1 ? 1 : 0;
                response.Result.fx_subtitulo = subtitulo;
                response.Result.fx_unidad = fxUnidad;

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CntXRepGeneralResultadosValidarResponseDto>(
                    ex.Message,
                    -1,
                    new CntXRepGeneralResultadosValidarResponseDto());
            }
        }
    }
}