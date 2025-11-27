using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPresModeloCuentasDb
    {
        private readonly IConfiguration _config;

        public FrmPresModeloCuentasDb(IConfiguration config)
        {
            _config = config;
        }

        #region Helpers

        private SqlConnection CreateConnection(int codEmpresa)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(stringConn);
        }

        private ErrorDto<List<T>> ExecuteStoredProcList<T>(
            int codEmpresa,
            string procedureName,
            object? parameters,
            string metodoContexto)
        {
            var resp = new ErrorDto<List<T>>
            {
                Code = 0,
                Result = new List<T>()
            };

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection.Query<T>(
                        procedureName,
                        parameters,
                        commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = $"{metodoContexto}: {ex.Message}";
                resp.Result = null;
            }

            return resp;
        }

        private ErrorDto<List<T>> ExecuteSqlList<T>(
            int codEmpresa,
            string sql,
            object? parameters,
            string metodoContexto)
        {
            var resp = new ErrorDto<List<T>>
            {
                Code = 0,
                Result = new List<T>()
            };

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection.Query<T>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = $"{metodoContexto}: {ex.Message}";
                resp.Result = null;
            }

            return resp;
        }

        #endregion

        /// <summary>
        /// Obtiene las cuentas del catálogo de contabilidad para el modelo, unidad y centro de costo especificados.
        /// </summary>
        public ErrorDto<List<CuentasCatalogoData>> spPres_CuentasCatalogo_Obtener(
            int codEmpresa,
            int codContab,
            string codModelo,
            string codUnidad,
            string codCentroCosto)
        {
            const string proc = "[spPres_CuentasCatalogo]";

            var parameters = new
            {
                CodContab = codContab,
                CodModelo = codModelo,
                CodUnidad = codUnidad,
                CodCentroCosto = codCentroCosto
            };

            return ExecuteStoredProcList<CuentasCatalogoData>(
                codEmpresa,
                proc,
                parameters,
                "spPres_CuentasCatalogo_Obtener");
        }

        /// <summary>
        /// Obtiene los modelos disponibles para un usuario específico en una contabilidad dada.
        /// </summary>
        public ErrorDto<List<ModeloGenericList>> Pres_Modelos_Obtener(
            int codEmpresa,
            int codContab,
            string usuario)
        {
            const string sql = @"
                SELECT 
                    P.cod_modelo AS IdX,
                    P.DESCRIPCION AS ItmX
                FROM PRES_MODELOS P
                INNER JOIN PRES_MODELOS_USUARIOS Pmu
                    ON P.cod_Contabilidad = Pmu.cod_contabilidad
                    AND P.cod_Modelo      = Pmu.cod_Modelo
                    AND Pmu.Usuario       = @Usuario
                INNER JOIN CNTX_CIERRES Cc
                    ON P.cod_Contabilidad = Cc.cod_Contabilidad
                    AND P.ID_CIERRE       = Cc.ID_CIERRE
                WHERE P.COD_CONTABILIDAD = @CodContab
                ORDER BY Cc.Inicio_Anio DESC;";

            var parameters = new
            {
                CodContab = codContab,
                Usuario = usuario
            };

            return ExecuteSqlList<ModeloGenericList>(
                codEmpresa,
                sql,
                parameters,
                "Pres_Modelos_Obtener");
        }

        /// <summary>
        /// Obtiene las unidades disponibles para un usuario específico en una contabilidad dada.
        /// </summary>
        public ErrorDto<List<ModeloGenericList>> Pres_Unidades_Obtener(
            int codEmpresa,
            int codContab,
            string usuario)
        {
            const string sql = @"
                SELECT 
                    Cu.Cod_Unidad AS IdX,
                    Cu.DESCRIPCION AS ItmX
                FROM CNTX_UNIDADES Cu
                INNER JOIN PRES_USUARIOS_NIVEL Pun
                    ON Cu.cod_Contabilidad = Pun.cod_contabilidad
                    AND Cu.cod_Unidad      = Pun.Cod_Unidad
                    AND Pun.Usuario        = @Usuario
                WHERE Cu.COD_CONTABILIDAD = @CodContab;";

            var parameters = new
            {
                CodContab = codContab,
                Usuario = usuario
            };

            return ExecuteSqlList<ModeloGenericList>(
                codEmpresa,
                sql,
                parameters,
                "Pres_Unidades_Obtener");
        }

        /// <summary>
        /// Obtiene los centros de costo disponibles para una contabilidad y unidad específica.
        /// </summary>
        public ErrorDto<List<ModeloGenericList>> Pres_CentroCosto_Obtener(
            int codEmpresa,
            int codContab,
            string codUnidad)
        {
            const string sql = @"
                SELECT 
                    Cc.COD_CENTRO_COSTO AS IdX,
                    Cc.DESCRIPCION     AS ItmX
                FROM CNTX_CENTRO_COSTOS Cc
                LEFT JOIN CNTX_UNIDADES_CC Uc
                    ON Cc.COD_CONTABILIDAD = Uc.COD_CONTABILIDAD
                    AND Uc.COD_UNIDAD      = @CodUnidad
                WHERE Cc.COD_CONTABILIDAD = @CodContab;";

            var parameters = new
            {
                CodContab = codContab,
                CodUnidad = codUnidad
            };

            return ExecuteSqlList<ModeloGenericList>(
                codEmpresa,
                sql,
                parameters,
                "Pres_CentroCosto_Obtener");
        }

        /// <summary>
        /// Carga datos de cuentas del modelo de presupuesto en la base de datos.
        /// </summary>
        public ErrorDto spPres_Modelo_Cuentas_CargaDatos(
            int codEmpresa,
            List<PresModeloCuentasImportData> request)
        {
            var resp = new ErrorDto { Code = 0 };

            const string proc = "[spPres_Presupuesto_Import_Load]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                int inicializa = 1;

                foreach (PresModeloCuentasImportData row in request)
                {
                    var fechaFormateada = row.Corte.Split(' ')[0] + " 23:59:59";

                    var values = new
                    {
                        Modelo = row.Cod_Modelo,
                        Contabilidad = row.Cod_Contabilidad,
                        Cuenta = row.Cod_Cuenta,
                        Unidad = row.Cod_Unidad,
                        Centro = row.Cod_Centro_Costo,
                        Corte = fechaFormateada,
                        Monto = row.Monto,
                        Usuario = row.Usuario,
                        Inicializa = inicializa++
                    };

                    connection.Execute(
                        proc,
                        values,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 150);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "spPres_Modelo_Cuentas_CargaDatos: " + ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Revisa los datos de importación de un modelo de cuentas en la base de datos.
        /// </summary>
        public ErrorDto<List<PresModeloCuentasImportData>> spPres_Modelo_Cuentas_RevisaImport(
            int codEmpresa,
            int codContab,
            string codModelo,
            string usuario)
        {
            const string proc = "[spPres_Presupuesto_Import_Revisa]";

            var parameters = new
            {
                Modelo = codModelo,
                Contabilidad = codContab,
                Usuario = usuario
            };

            return ExecuteStoredProcList<PresModeloCuentasImportData>(
                codEmpresa,
                proc,
                parameters,
                "spPres_Modelo_Cuentas_RevisaImport");
        }

        /// <summary>
        /// Importa un modelo de cuentas en la base de datos.
        /// </summary>
        public ErrorDto spPres_Modelo_Cuentas_Import(
            int codEmpresa,
            int codContab,
            string codModelo,
            string usuario)
        {
            var resp = new ErrorDto { Code = 0 };

            const string procMapeo = "[spPres_Presupuesto_Import_Mapeo]";
            const string procProcesa = "[spPres_Presupuesto_Import_Procesa]";

            int pendientes = 0;
            int procesados = 0;
            string vMensaje = "";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    Modelo = codModelo,
                    Contabilidad = codContab,
                    Usuario = usuario
                };

                // 1) Mapeo
                connection.Execute(
                    procMapeo,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 150);

                // 2) Primer proceso
                var result = connection.QueryFirstOrDefault(
                    procProcesa,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 150);

                if (result != null)
                {
                    pendientes = result.Pendientes ?? 0;
                    procesados = result.Procesados ?? 0;
                }

                int i = 0;
                int ciclo = pendientes > 0
                    ? (int)Math.Ceiling(pendientes / 50.0)
                    : 0;

                while (pendientes > 0)
                {
                    if (i == ciclo)
                    {
                        vMensaje = "<br> Pendientes: " + pendientes + ", Procesados: " + procesados;
                        break;
                    }

                    var result2 = connection.QueryFirstOrDefault(
                        procProcesa,
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 150);

                    if (result2 != null)
                    {
                        pendientes = result2.Pendientes ?? 0;
                        procesados = result2.Procesados ?? 0;
                    }
                    else
                    {
                        pendientes = 0;
                        procesados = 0;
                    }

                    i++;
                }

                resp.Description = "Importaci&oacute;n del Presupuesto realizado satisfactoriamente!" + vMensaje;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "spPres_Modelo_Cuentas_Import: " + ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene los meses del período fiscal para un modelo de cuentas específico.
        /// </summary>
        public ErrorDto<List<PresModeloCuentasImportData>> spCntX_Periodo_Fiscal_Meses(
            int codEmpresa,
            int codContab,
            string codModelo,
            string usuario,
            List<PresModeloCuentasHorizontal> request)
        {
            var resp = new ErrorDto<List<PresModeloCuentasImportData>>
            {
                Code = 0,
                Result = new List<PresModeloCuentasImportData>()
            };

            const string proc = "[spCntX_Periodo_Fiscal_Meses]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var periodos = connection
                    .Query<CntXPeriodoFiscalMeses>(
                        proc,
                        new
                        {
                            Contabilidad = codContab,
                            Tipo = 0,
                            Modelo = codModelo
                        },
                        commandType: CommandType.StoredProcedure)
                    .ToList();

                foreach (var item in request)
                {
                    foreach (var periodo in periodos)
                    {
                        float valor = periodo.Mes switch
                        {
                            1 => item.Enero ?? 0,
                            2 => item.Febrero ?? 0,
                            3 => item.Marzo ?? 0,
                            4 => item.Abril ?? 0,
                            5 => item.Mayo ?? 0,
                            6 => item.Junio ?? 0,
                            7 => item.Julio ?? 0,
                            8 => item.Agosto ?? 0,
                            9 => item.Septiembre ?? 0,
                            10 => item.Octubre ?? 0,
                            11 => item.Noviembre ?? 0,
                            12 => item.Diciembre ?? 0,
                            _ => 0
                        };

                        if (valor >= 0)
                        {
                            resp.Result.Add(new PresModeloCuentasImportData
                            {
                                Cod_Contabilidad = codContab,
                                Cod_Modelo = codModelo,
                                Cod_Cuenta = item.Cuenta,
                                Descripcion = item.Descripcion,
                                Cod_Unidad = item.Unidad,
                                Cod_Centro_Costo = item.Centro,
                                Monto = valor,
                                Corte = periodo.Corte.ToString("yyyy-MM-dd HH:mm:ss"),
                                Usuario = usuario,
                                Inicializa = 1
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "spCntX_Periodo_Fiscal_Meses: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }
    }
}