using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXExploradorContableDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXExploradorContableDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXExploradorContableDb(
            PortalDB portalDb,
            MSecurityMainDb securityDb)
        {
            _portalDb = portalDb;
        }

        #region CARGA TREE

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_Cuentas_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT cod_cuenta AS Item,
                                   descripcion AS Descripcion
                            FROM CntX_Cuentas
                            WHERE cod_contabilidad = @codEmpresa
                            ORDER BY cod_cuenta";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql,
                    new { codEmpresa }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsiento_Obtener(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT tipo_asiento AS Item,
                                   descripcion AS Descripcion
                            FROM CntX_Tipos_Asientos
                            WHERE cod_contabilidad = @cod_contabilidad";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql,
                    new { cod_contabilidad }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        public ErrorDto<List<CntxPeriodoDto>> Cntx_Periodos_Obtener(int codEmpresa, string estado)
        {
            var response = new ErrorDto<List<CntxPeriodoDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT 
                                CONCAT(anio,'-',FORMAT(mes,'00')) AS Periodo,
                                CAST(CONCAT(anio,'-',FORMAT(mes,'00'),'-01') AS DATE) AS Fecha_Periodo
                            FROM CntX_Periodos
                            WHERE cod_contabilidad = @codEmpresa
                              AND estado = @estado
                            ORDER BY anio DESC, mes DESC";

                response.Result = cn.Query<CntxPeriodoDto>(sql,
                    new { codEmpresa, estado }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion

        #region LISTADO ASIENTOS

        public ErrorDto<List<CntxAsientoRsmDto>> Cntx_Asientos_Listar(int codEmpresa, int cod_contabilidad, CntxExploradorFiltrosDto filtros)
        {
            var response = new ErrorDto<List<CntxAsientoRsmDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sb = new StringBuilder();

                sb.Append(@"
                    SELECT 
                        A.num_asiento,
                        A.tipo_asiento,
                        A.fecha_asiento,
                        A.descripcion,
                        ISNULL(SUM(D.monto_debito),0) AS Debe,
                        ISNULL(SUM(D.monto_credito),0) AS Haber,
                        CASE WHEN A.fecha_aplicado IS NULL THEN 'NO' ELSE 'SI' END AS Aplicado
                    FROM CntX_Asientos A
                    LEFT JOIN CntX_Asientos_Detalle D 
                        ON A.cod_contabilidad = D.cod_contabilidad
                        AND A.tipo_asiento = D.tipo_asiento
                        AND A.num_asiento = D.num_asiento
                    WHERE A.cod_contabilidad = @cod_contabilidad
                ");

                // ==== FILTROS DINÁMICOS ====

                if (!string.IsNullOrWhiteSpace(filtros.cod_tipo_asiento))
                    sb.Append(" AND A.tipo_asiento = @tipo ");

                if (!string.IsNullOrWhiteSpace(filtros.num_asiento))
                    sb.Append(" AND A.num_asiento LIKE '%' + @num + '%' ");

                if (filtros.todas != true && filtros.fecha_desde != null && filtros.fecha_hasta != null)
                {
                    sb.Append(" AND A.fecha_asiento BETWEEN @desde AND @hasta ");
                }

                sb.Append(@"
                    GROUP BY A.num_asiento, A.tipo_asiento, A.fecha_asiento, 
                             A.descripcion, A.fecha_aplicado
                    ORDER BY A.fecha_asiento DESC
                ");

                response.Result = cn.Query<CntxAsientoRsmDto>(
                    sb.ToString(),
                    new
                    {
                        codEmpresa,
                        cod_contabilidad,
                        tipo = filtros.cod_tipo_asiento,
                        num = filtros.num_asiento,
                        desde = filtros.fecha_desde,
                        hasta = filtros.fecha_hasta
                    }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        public ErrorDto<List<CntxAsientoDetDto>> AsientoDetalle_Listar(int codEmpresa, CntxExploradorFiltrosDto filtros)
        {
            var response = new ErrorDto<List<CntxAsientoDetDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT 
                D.cod_cuenta,
                C.descripcion AS cuenta_descripcion,
                D.monto_debito,
                D.monto_credito,
                D.detalle,
                D.centro_costo
            FROM CntX_Asientos_Detalle D
            INNER JOIN CntX_Cuentas C
                ON D.cod_contabilidad = C.cod_contabilidad
                AND D.cod_cuenta = C.cod_cuenta
            WHERE D.cod_contabilidad = @codEmpresa
              AND D.tipo_asiento = @tipo
              AND D.num_asiento = @numero
            ORDER BY D.linea";

                response.Result = cn.Query<CntxAsientoDetDto>(sql, new
                {
                    codEmpresa,
                    tipo = filtros.cod_tipo_asiento,
                    numero = filtros.num_asiento
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        public ErrorDto<string> FechaServidor_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<string>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = "SELECT dbo.MyGetdate()";

                var fechaServidor = cn.QuerySingle<DateTime>(sql);

                response.Description =
                    fechaServidor.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion


        #region CATALOGO CUENTAS

        public ErrorDto<List<CntxCuentaDto>> CuentasPorPadre(int codEmpresa, string? codCuentaPadre)
        {
            var response = new ErrorDto<List<CntxCuentaDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var lista = cn.Query<CntxCuentaDto>(
                    "spCntX_Cuentas_PorPadre",
                    new { codCuentaPadre },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                response.Result = lista;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion

        #region GENERICOS

        public ErrorDto<List<CntxAsientoTreeDto>>Cntx_Asientos_TreePorTipo(int codEmpresa,int cod_contabilidad,string tipo,int anio,int mes)
        {
            var response = new ErrorDto<List<CntxAsientoTreeDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT 
                num_asiento,
                fecha_asiento
            FROM CntX_Asientos
            WHERE cod_contabilidad = @cod_contabilidad
              AND tipo_asiento = @tipo
              AND anio = @anio
              AND mes = @mes
            ORDER BY num_asiento";

                response.Result = cn.Query<CntxAsientoTreeDto>(sql, new
                {
                    cod_contabilidad,
                    tipo,
                    anio,
                    mes
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<List<CntxTipoCuentaDto>> Cntx_TiposCuenta_Obtener(int codEmpresa,int codContabilidad)
        {
            var response = new ErrorDto<List<CntxTipoCuentaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT tipo_cuenta     AS item,
                   descripcion
            FROM CntX_Tipos_Cuentas
            WHERE cod_contabilidad = @codContabilidad
            ORDER BY prioridad";

                response.Result = cn.Query<CntxTipoCuentaDto>(
                    sql,
                    new { codContabilidad }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<List<CntxCuentaDto>> Cntx_CuentasRaizPorTipo_Obtener(int codEmpresa,int codContabilidad,string tipoCuenta)
        {
            var response = new ErrorDto<List<CntxCuentaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT cod_cuenta,
                   descripcion,
                   CASE 
                       WHEN acepta_movimientos = 0 THEN 1
                       ELSE 0
                   END AS es_mayor
            FROM CntX_Cuentas
            WHERE cod_contabilidad = @codContabilidad
              AND tipo_cuenta = @tipoCuenta
              AND ISNULL(cuenta_madre,'') = ''
            ORDER BY cod_cuenta";

                response.Result = cn.Query<CntxCuentaDto>(
                    sql,
                    new { codContabilidad, tipoCuenta }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion

        public ErrorDto<List<DropDownListaGenericaModel>> Diferidos_Obtener(int codEmpresa, int codContabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT cod_diferido AS Item,
                   descripcion AS Descripcion
            FROM CntX_Diferidos
            WHERE cod_contabilidad = @codContabilidad
            ORDER BY cod_diferido";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { codContabilidad }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }



        public ErrorDto<List<DropDownListaGenericaModel>> DiferidoPlantillas_Obtener(int codEmpresa,int codContabilidad,int codDiferido)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT cod_difPlantilla AS Item,
                   descripcion AS Descripcion
            FROM CntX_diferido_plantilla
            WHERE cod_contabilidad = @codContabilidad
              AND cod_diferido = @codDiferido
            ORDER BY cod_difPlantilla";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { codContabilidad, codDiferido }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        public ErrorDto<List<CntxDiferidoHistoricoDto>> DiferidoHistorico_Obtener(int codEmpresa,int codContabilidad,int codDiferido,int codPlantilla)
        {
            var response = new ErrorDto<List<CntxDiferidoHistoricoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT num_asiento,
                   tipo_asiento,
                   fecha,
                   anio,
                   mes,
                   usuario
            FROM CntX_Diferido_Historico
            WHERE cod_contabilidad = @codContabilidad
              AND cod_diferido = @codDiferido
              AND cod_difPlantilla = @codPlantilla
            ORDER BY anio, mes";

                response.Result = cn.Query<CntxDiferidoHistoricoDto>(
                    sql,
                    new { codContabilidad, codDiferido, codPlantilla }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var result = cn.Query<DropDownListaGenericaModel>(
                    @"SELECT tipo_asiento AS item,
                             descripcion   AS descripcion
                      FROM CntX_Tipos_Asientos
                      WHERE cod_contabilidad = @cod_contabilidad
                      ORDER BY tipo_asiento",
                    new { cod_contabilidad }).ToList();

                response.Result = result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }




        #region F4 Unidad

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_Unidades_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var result = cn.Query<DropDownListaGenericaModel>(
                    @"SELECT cod_unidad AS item,
                             descripcion AS descripcion
                      FROM CntX_Unidades
                      WHERE cod_contabilidad = @cod_contabilidad
                      ORDER BY cod_unidad",
                    new { cod_contabilidad }).ToList();

                response.Result = result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion


        #region F4 Centro Costo

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_CentroCosto_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var result = cn.Query<DropDownListaGenericaModel>(
                    @"SELECT cod_centro_costo AS item,
                             descripcion       AS descripcion
                      FROM CntX_Centro_Costos
                      WHERE cod_contabilidad = @cod_contabilidad
                      ORDER BY cod_centro_costo",
                    new { cod_contabilidad }).ToList();

                response.Result = result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion


        #region F4 Divisa

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_Divisas_Buscar(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var result = cn.Query<DropDownListaGenericaModel>(
                    @"SELECT cod_divisa AS item,
                             descripcion AS descripcion
                      FROM CntX_Divisas
                      WHERE cod_contabilidad = @cod_contabilidad
                      ORDER BY cod_divisa",
                    new { cod_contabilidad }).ToList();

                response.Result = result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion

        public ErrorDto<List<CntxAsientoResumenDto>> Asientos_Resumen(int codEmpresa,int cod_contabilidad,int anio,int mes)
        {
            var response = new ErrorDto<List<CntxAsientoResumenDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var result = cn.Query<CntxAsientoResumenDto>(
                    "spCntx_Consulta_Asientos_Rsm",
                    new
                    {
                        Contabilidad = cod_contabilidad,
                        Anio = anio,
                        Mes = mes
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                response.Result = result;
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