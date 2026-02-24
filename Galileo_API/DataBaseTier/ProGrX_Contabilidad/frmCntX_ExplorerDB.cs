using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
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


        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsiento_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT tipo_asiento AS Item,
                                   descripcion AS Descripcion
                            FROM CntX_Tipos_Asientos
                            WHERE cod_contabilidad = @codEmpresa
                            ORDER BY tipo_asiento";

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

        public ErrorDto<List<CntxAsientoRsmDto>> Cntx_Asientos_Listar(int codEmpresa, CntxExploradorFiltrosDto filtros)
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
                    WHERE A.cod_contabilidad = @codEmpresa
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


        public ErrorDto<List<CntxAsientoDetDto>> AsientoDetalle_Listar(int codEmpresa,CntxExploradorFiltrosDto filtros)
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
    }
}