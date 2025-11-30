using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmPresDefinicionDb
    {
        private readonly IConfiguration _config;
        private const string Consolidado = "CONSOLIDADO";

        public FrmPresDefinicionDb(IConfiguration config)
        {
            _config = config;
        }

        #region Helpers

        private SqlConnection CreateConnection(int codEmpresa)
        {
            var connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(connString);
        }

        #endregion

        public ErrorDto<List<ModeloGenericList>> Pres_Modelos_Obtener(int codEmpresa, string usuario, int codContab)
        {
            var resp = new ErrorDto<List<ModeloGenericList>>
            {
                Code = 0,
                Result = new List<ModeloGenericList>()
            };

            const string sql = @"
                SELECT 
                    P.cod_modelo AS IdX, 
                    P.DESCRIPCION AS ItmX, 
                    Cc.Inicio_Anio
                FROM PRES_MODELOS P 
                INNER JOIN PRES_MODELOS_USUARIOS Pmu 
                    ON P.cod_Contabilidad = Pmu.cod_contabilidad 
                    AND P.cod_Modelo = Pmu.cod_Modelo 
                    AND Pmu.Usuario = @Usuario
                INNER JOIN CNTX_CIERRES Cc 
                    ON P.cod_Contabilidad = Cc.cod_Contabilidad 
                    AND P.ID_CIERRE = Cc.ID_CIERRE 
                WHERE P.COD_CONTABILIDAD = @CodContab
                GROUP BY P.cod_Modelo, P.Descripcion, Cc.Inicio_Anio 
                ORDER BY Cc.INICIO_ANIO DESC, P.Cod_Modelo;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection
                    .Query<ModeloGenericList>(sql, new { Usuario = usuario, CodContab = codContab })
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelos_Obtener: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Modelo_Unidades_Obtener(
            int codEmpresa,
            string codModelo,
            int codContab,
            string usuario)
        {
            var resp = new ErrorDto<List<ModeloGenericList>>
            {
                Code = 0,
                Result = new List<ModeloGenericList>()
            };

            const string sql = @"
                EXEC spPres_Modelo_Unidades 
                    @CodContab,
                    @CodModelo,
                    @Usuario;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodContab = codContab,
                    CodModelo = codModelo,
                    Usuario = usuario
                };

                resp.Result = connection.Query<ModeloGenericList>(sql, parameters).ToList();

                resp.Result.RemoveAll(x => string.IsNullOrWhiteSpace(x.IdX));
                resp.Result.Add(new ModeloGenericList { IdX = Consolidado, ItmX = Consolidado });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Unidades_Obtener: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Modelo_Unidades_CC_Obtener(
            int codEmpresa,
            string codModelo,
            int codContab,
            string codUnidad)
        {
            var resp = new ErrorDto<List<ModeloGenericList>>
            {
                Code = 0,
                Result = new List<ModeloGenericList>()
            };

            const string sql = @"
                EXEC spPres_Modelo_Unidades_CC 
                    @CodContab,
                    @CodModelo,
                    @CodUnidad;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var unidadParametro = codUnidad == Consolidado ? "CONS" : codUnidad;

                var parameters = new
                {
                    CodContab = codContab,
                    CodModelo = codModelo,
                    CodUnidad = unidadParametro
                };

                resp.Result = connection.Query<ModeloGenericList>(sql, parameters).ToList();

                resp.Result.RemoveAll(x => string.IsNullOrWhiteSpace(x.IdX));
                resp.Result.Add(new ModeloGenericList { IdX = "TODOS", ItmX = "TODOS" });
                resp.Result.Add(new ModeloGenericList { IdX = Consolidado, ItmX = Consolidado });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Unidades_CC_Obtener: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<CntxCuentasData> Pres_Definicion_scroll(
            int codEmpresa, int scrollValue, string? codCtaMask, int codContab)
        {
            var resp = new ErrorDto<CntxCuentasData> { Code = 0 };

            const string sqlNext = @"
            SELECT TOP 1 Cod_Cuenta_Mask, Descripcion
            FROM CntX_cuentas
            WHERE COD_CONTABILIDAD = @CodContab
            AND Acepta_Movimientos = 1
            AND Cod_Cuenta_Mask > @CodCtaMask
            ORDER BY Cod_Cuenta_Mask ASC;";

            const string sqlPrev = @"
            SELECT TOP 1 Cod_Cuenta_Mask, Descripcion
            FROM CntX_cuentas
            WHERE COD_CONTABILIDAD = @CodContab
            AND Acepta_Movimientos = 1
            AND Cod_Cuenta_Mask < @CodCtaMask
            ORDER BY Cod_Cuenta_Mask DESC;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection.QueryFirstOrDefault<CntxCuentasData>(
                    scrollValue == 1 ? sqlNext : sqlPrev,
                    new { CodContab = codContab, CodCtaMask = codCtaMask ?? string.Empty }
                );
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Definicion_scroll: " + ex.Message;
            }

            return resp;
        }



        public ErrorDto<List<VistaPresCuentaData>> Pres_VistaPresupuesto_Cuenta_SP(
            int codEmpresa,
            PresCuenta request)
        {
            var resp = new ErrorDto<List<VistaPresCuentaData>>
            {
                Code = 0,
                Result = new List<VistaPresCuentaData>()
            };

            const string sql = @"
                EXEC spPres_VistaPresupuesto_Cuenta 
                    @CodContabilidad,
                    @CodModelo,
                    @CodUnidad,
                    @CodCentroCosto,
                    @CodCuenta,
                    @Vista;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodContabilidad = request.Cod_Contabilidad,
                    CodModelo = request.Cod_Modelo,
                    CodUnidad = request.Cod_Unidad,
                    CodCentroCosto = request.Cod_Centro_Costo,
                    CodCuenta = request.Cod_Cuenta,
                    Vista = request.Vista
                };

                resp.Result = connection.Query<VistaPresCuentaData>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_VistaPresupuesto_Cuenta_SP: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<CuentasLista> Pres_Cuentas_Obtener(
            int codEmpresa,
            string cod_contabilidad,
            int? pagina,
            int? paginacion,
            string? filtro)
        {
            var resp = new ErrorDto<CuentasLista>
            {
                Code = 0,
                Result = new CuentasLista()
            };
            resp.Result.total = 0;

            const string countSql = @"
                SELECT COUNT(cod_cuenta) 
                FROM CntX_cuentas 
                WHERE cod_contabilidad = @CodContabilidad 
                  AND acepta_movimientos = 1;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("CodContabilidad", cod_contabilidad);

                // Total (mantengo el comportamiento original: sin filtro)
                resp.Result.total = connection.ExecuteScalar<int>(countSql, parameters);

                // Query de datos con filtro y paginación
                var sb = new StringBuilder();
                sb.Append(@"
                    SELECT Cod_Cuenta_Mask, descripcion
                    FROM CntX_cuentas
                    WHERE cod_contabilidad = @CodContabilidad
                      AND acepta_movimientos = 1");

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    sb.Append(" AND (Cod_Cuenta_Mask LIKE @Filtro OR descripcion LIKE @Filtro)");
                    parameters.Add("Filtro", "%" + filtro + "%");
                }

                sb.Append(" ORDER BY Cod_Cuenta_Mask");

                if (pagina.HasValue && paginacion.HasValue)
                {
                    var offset = (pagina.Value) * paginacion.Value;
                    var pageSize = paginacion.Value;

                    sb.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                    parameters.Add("Offset", offset, DbType.Int32);
                    parameters.Add("PageSize", pageSize, DbType.Int32);
                }

                var finalSql = sb.ToString();

                resp.Result.lista = connection
                    .Query<CntxCuentasData>(finalSql, parameters)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Cuentas_Obtener: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }
    }
}