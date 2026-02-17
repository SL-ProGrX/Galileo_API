using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRastreoMovimientosDb
    {
        private readonly PortalDB _portalDB;

        public FrmCntXRastreoMovimientosDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Busca los movimientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<RastreoMovimientosTablaDto>> Buscar(int codEmpresa,RastreoMovimientosFiltroDto filtros)
        {
            var response = new ErrorDto<List<RastreoMovimientosTablaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var sql = new StringBuilder();
                var where = new StringBuilder();

                ConstruirSelect(sql);
                ConstruirFiltros(where, filtros);

                if (where.Length > 0)
                    sql.Append(" WHERE ").Append(where);

                sql.Append(" ORDER BY D.fecha_asiento, D.cod_cuenta ");

                response.Result = cn.Query<RastreoMovimientosTablaDto>(
                    sql.ToString(),
                    ObtenerParametros(filtros)
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        private static void ConstruirSelect(StringBuilder sql)
        {
            sql.Append(@"
                SELECT
                    C.cod_cuenta_mask AS cuenta,
                    C.descripcion,
                    D.fecha_asiento AS fecha,
                    D.tipo_asiento,
                    D.num_asiento,
                    D.monto_debito AS debitos,
                    D.monto_credito AS creditos,
                    E.nombre AS empresa,
                    D.documento,
                    D.detalle
                FROM CntX_Asientos A
                INNER JOIN CntX_Asientos_detalle D
                    ON A.COD_CONTABILIDAD = D.COD_CONTABILIDAD
                    AND A.tipo_asiento = D.tipo_asiento
                    AND A.num_asiento = D.num_asiento
                INNER JOIN CNTX_CONTABILIDADES E
                    ON D.COD_CONTABILIDAD = E.COD_CONTABILIDAD
                INNER JOIN CntX_Cuentas C
                    ON D.COD_CONTABILIDAD = C.COD_CONTABILIDAD
                    AND D.cod_cuenta = C.cod_cuenta
            ");
        }

        private static void ConstruirFiltros(StringBuilder where, RastreoMovimientosFiltroDto filtros)
        {
            AgregarFiltro(where,
                "D.fecha_asiento BETWEEN @fechaInicio AND @fechaCorte",
                filtros.fechaInicio.HasValue && filtros.fechaCorte.HasValue);

            AgregarFiltro(where,
                "C.cod_cuenta_mask BETWEEN @cuentaInicio AND @cuentaCorte",
                !string.IsNullOrEmpty(filtros.cuentaInicio) &&
                !string.IsNullOrEmpty(filtros.cuentaCorte));

            ConstruirFiltroParametro(where, filtros);
        }

        private static void ConstruirFiltroParametro(StringBuilder where,RastreoMovimientosFiltroDto filtros)
        {
            if (!filtros.parametro.HasValue)
                return;

            var operador = ObtenerOperador(filtros.signo!);

            if (filtros.movimiento == "Ambos")
            {
                AppendAnd(where);
                where.Append($@"
                            (
                                D.monto_debito {operador} @parametro
                                OR D.monto_credito {operador} @parametro
                            )");
                                            return;
            }

            var campo = ObtenerCampoMovimiento(filtros.movimiento!);

            AppendAnd(where);where.Append($"{campo} {operador} @parametro");
        }


        private static string ObtenerCampoMovimiento(string movimiento) => movimiento switch
                {
                    "Creditos" => "D.monto_credito",
                    "Debitos" => "D.monto_debito",
                    _ => "D.monto_debito"
                };

        private static string ObtenerOperador(string signo) =>
            signo switch
            {
                "=" => "=",
                ">" => ">",
                "<" => "<",
                _ => "="
            };

        private static void AppendAnd(StringBuilder where)
        {
            if (where.Length > 0)where.Append(" AND ");
        }


        private static void AgregarFiltro(StringBuilder where, string condicion, bool aplicar)
        {
            if (!aplicar) return;

            if (where.Length > 0)
                where.Append(" AND ");

            where.Append(condicion);
        }

        private object ObtenerParametros(RastreoMovimientosFiltroDto filtros)
        {
            return new
            {
                filtros.codigo,
                fechaInicio = filtros.fechaInicio?.Date,
                fechaCorte = filtros.fechaCorte?.Date.AddDays(1).AddSeconds(-1),
                filtros.cuentaInicio,
                filtros.cuentaCorte,
                parametro = filtros.parametro,
                documento = $"%{filtros.documento}%",
                detalle = $"%{filtros.detalle}%"
            };
        }


        /// <summary>
        /// Busca las contabilidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Contabilidades_Buscar(int codEmpresa,string tipo)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var sql = new StringBuilder();

                if (tipo == "Individual")
                {
                    sql.Append(@"
                        SELECT 
                            CAST(COD_CONTABILIDAD AS VARCHAR) AS item,
                            RTRIM(NOMBRE) AS descripcion
                        FROM CNTX_CONTABILIDADES
                        ORDER BY COD_CONTABILIDAD
                    ");
                }
                else
                {
                    sql.Append(@"
                        SELECT 
                            CAST(cod_consolida AS VARCHAR) AS item,
                            RTRIM(descripcion) AS descripcion
                        FROM CNTX_CONSOLIDA_DEFINICION
                        ORDER BY cod_consolida
                    ");
                }

                response.Result = cn
                    .Query<DropDownListaGenericaModel>(sql.ToString())
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}

