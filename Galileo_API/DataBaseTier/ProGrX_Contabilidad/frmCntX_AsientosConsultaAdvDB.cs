using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXAsientosConsultaAdvDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXAsientosConsultaAdvDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }


        /// <summary>
        /// Helper ejecutar
        /// </summary>
        private ErrorDto<T> Ejecutar<T>(int codEmpresa, Func<SqlConnection, T> accion)
        {
            var response = new ErrorDto<T>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = accion(cn);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Busca movimientos de asiento según filtros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxMovimientoConsultaDto>> CntX_Movimientos_Consulta(int codEmpresa, int codContabilidad,CntxMovimientosFiltroDto f)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                var sql = SqlBase;

                sql += ConstruirFiltros(f);

                sql += " ORDER BY Asi.fecha_asiento DESC";

                return cn.Query<CntxMovimientoConsultaDto>(
                    sql,
                    new
                    {
                        codContabilidad,
                        f.tipo,
                        f.num_asiento,
                        f.unidad,
                        f.cc,
                        f.divisa,
                        f.documento,
                        f.detalle,
                        f.referencia,
                        f.cuenta,
                        f.fecha_desde,
                        f.fecha_hasta,
                        lineas = f.lineas
                    }).ToList();
            });
        }

                            private const string SqlBase = @"
                        SELECT TOP (@lineas)

                            Asi.tipo_asiento,
                            Asi.num_asiento,
                            Asi.fecha_asiento,

                            Det.cod_cuenta,

                            Det.cod_unidad,
                            Det.cod_centro_costo,

                            Det.cod_divisa,
                            Det.tipo_cambio,

                            Det.documento,
                            Det.detalle,

                            Det.monto_credito,
                            Det.monto_debito

                        FROM CntX_Cuentas Cta

                        INNER JOIN CntX_Asientos_Detalle Det
                            ON Cta.cod_contabilidad = Det.cod_contabilidad
                            AND Cta.cod_cuenta = Det.cod_cuenta

                        INNER JOIN CntX_Asientos Asi
                            ON Det.cod_contabilidad = Asi.cod_contabilidad
                            AND Det.tipo_asiento = Asi.tipo_asiento
                            AND Det.num_asiento = Asi.num_asiento

                        WHERE Cta.cod_contabilidad = @codContabilidad
                    ";

        private static string ConstruirFiltros(CntxMovimientosFiltroDto f)
        {
            var sql = "";

            if (!string.IsNullOrWhiteSpace(f.referencia))
                sql += " AND Asi.referencia LIKE '%' + @referencia + '%'";

            if (!string.IsNullOrWhiteSpace(f.tipo))
                sql += " AND Det.tipo_asiento = @tipo";

            if (!string.IsNullOrWhiteSpace(f.num_asiento))
                sql += " AND Det.num_asiento LIKE '%' + @num_asiento + '%'";

            if (!string.IsNullOrWhiteSpace(f.unidad))
                sql += " AND Det.cod_unidad = @unidad";

            if (!string.IsNullOrWhiteSpace(f.cc))
                sql += " AND Det.cod_centro_costo = @cc";

            if (!string.IsNullOrWhiteSpace(f.divisa))
                sql += " AND Det.cod_divisa = @divisa";

            if (!string.IsNullOrWhiteSpace(f.documento))
                sql += " AND Det.documento LIKE '%' + @documento + '%'";

            if (!string.IsNullOrWhiteSpace(f.detalle))
                sql += " AND Det.detalle LIKE '%' + @detalle + '%'";

            if (!string.IsNullOrWhiteSpace(f.cuenta))
                sql += " AND Det.cod_cuenta = @cuenta";

            if (f.todas != true && f.fecha_desde.HasValue && f.fecha_hasta.HasValue)
                sql += " AND Asi.fecha_asiento BETWEEN @fecha_desde AND @fecha_hasta";

            return sql;
        }


        /// <summary>
        /// Lista tipo de asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_TiposAsiento_Listar(int codEmpresa, int codContabilidad)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                        tipo_asiento AS item,
                        descripcion
                      FROM CntX_Tipos_Asientos
                      WHERE cod_contabilidad = @codContabilidad
                      ORDER BY tipo_asiento",
                    new { codContabilidad }
                ).ToList();
            });
        }



        /// <summary>
        /// Busca unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(int codEmpresa, int codContabilidad)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                        cod_unidad AS item,
                        descripcion
                      FROM CntX_Unidades
                      WHERE cod_contabilidad = @codContabilidad
                      ORDER BY cod_unidad",
                    new { codContabilidad }
                ).ToList();
            });
        }



        /// <summary>
        /// Lista centros de costos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCostos_Listar(int codEmpresa, int codContabilidad)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                        cod_centro_costo AS item,
                        descripcion
                      FROM CntX_Centro_Costos
                      WHERE cod_contabilidad = @codContabilidad
                      ORDER BY cod_centro_costo",
                    new { codContabilidad }
                ).ToList();
            });
        }


        /// <summary>
        /// Lista de Divisas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Divisas_Listar(int codEmpresa, int codContabilidad)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                        cod_divisa AS item,
                        descripcion
                      FROM CntX_Divisas
                      WHERE cod_contabilidad = @codContabilidad
                      ORDER BY cod_divisa",
                    new { codContabilidad }
                ).ToList();
            });
        }

        /// <summary>
        /// Lista de tipos de asiento para filtros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(int codEmpresa, int cod_contabilidad)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                       tipo_asiento AS item,
                       descripcion
                     FROM CntX_Tipos_Asientos
                     WHERE cod_contabilidad = @cod_contabilidad
                     ORDER BY tipo_asiento",
                    new { cod_contabilidad }
                ).ToList();
            });
        }

    }
}