using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

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
            var resp = new ErrorDto<List<CuentasCatalogoData>> { Code = 0 };

            const string proc = "[spPres_CuentasCatalogo]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodContab = codContab,
                    CodModelo = codModelo,
                    CodUnidad = codUnidad,
                    CodCentroCosto = codCentroCosto
                };

                resp.Result = connection
                    .Query<CuentasCatalogoData>(
                        proc,
                        parameters,
                        commandType: System.Data.CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "spPres_CuentasCatalogo_Obtener: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene los modelos disponibles para un usuario específico en una contabilidad dada.
        /// </summary>
        public ErrorDto<List<ModeloGenericList>> Pres_Modelos_Obtener(
            int codEmpresa,
            int codContab,
            string usuario)
        {
            var resp = new ErrorDto<List<ModeloGenericList>> { Code = 0 };

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

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection
                    .Query<ModeloGenericList>(
                        sql,
                        new { CodContab = codContab, Usuario = usuario })
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

        /// <summary>
        /// Obtiene las unidades disponibles para un usuario específico en una contabilidad dada.
        /// </summary>
        public ErrorDto<List<ModeloGenericList>> Pres_Unidades_Obtener(
            int codEmpresa,
            int codContab,
            string usuario)
        {
            var resp = new ErrorDto<List<ModeloGenericList>> { Code = 0 };

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

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection
                    .Query<ModeloGenericList>(
                        sql,
                        new { CodContab = codContab, Usuario = usuario })
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Unidades_Obtener: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene los centros de costo disponibles para una contabilidad y unidad específica.
        /// </summary>
        public ErrorDto<List<ModeloGenericList>> Pres_CentroCosto_Obtener(
            int codEmpresa,
            int codContab,
            string codUnidad)
        {
            var resp = new ErrorDto<List<ModeloGenericList>> { Code = 0 };

            const string sql = @"
                SELECT 
                    Cc.COD_CENTRO_COSTO AS IdX,
                    Cc.DESCRIPCION     AS ItmX
                FROM CNTX_CENTRO_COSTOS Cc
                LEFT JOIN CNTX_UNIDADES_CC Uc
                    ON Cc.COD_CONTABILIDAD = Uc.COD_CONTABILIDAD
                    AND Uc.COD_UNIDAD      = @CodUnidad
                WHERE Cc.COD_CONTABILIDAD = @CodContab;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection
                    .Query<ModeloGenericList>(
                        sql,
                        new { CodContab = codContab, CodUnidad = codUnidad })
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_CentroCosto_Obtener: " + ex.Message;
                resp.Result = null;
            }

            return resp;
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
                    // Si quieres, podrías convertir Corte a DateTime en lugar de string
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
                        commandType: System.Data.CommandType.StoredProcedure,
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
            var resp = new ErrorDto<List<PresModeloCuentasImportData>>
            {
                Code = 0,
                Description = "Ok"
            };

            const string proc = "[spPres_Presupuesto_Import_Revisa]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    Modelo = codModelo,
                    Contabilidad = codContab,
                    Usuario = usuario
                };

                resp.Result = connection
                    .Query<PresModeloCuentasImportData>(
                        proc,
                        parameters,
                        commandType: System.Data.CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "spPres_Modelo_Cuentas_RevisaImport: " + ex.Message;
                resp.Result = null;
            }

            return resp;
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
                    commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: 150);

                // 2) Primer proceso
                var result = connection.QueryFirstOrDefault(
                    procProcesa,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure,
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
                        commandType: System.Data.CommandType.StoredProcedure,
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
                        commandType: System.Data.CommandType.StoredProcedure)
                    .ToList();

                foreach (var item in request)
                {
                    foreach (var periodo in periodos)
                    {
                        float valor = 0;
                        switch (periodo.Mes)
                        {
                            case 1:  valor = item.Enero; break;
                            case 2:  valor = item.Febrero; break;
                            case 3:  valor = item.Marzo; break;
                            case 4:  valor = item.Abril; break;
                            case 5:  valor = item.Mayo; break;
                            case 6:  valor = item.Junio; break;
                            case 7:  valor = item.Julio; break;
                            case 8:  valor = item.Agosto; break;
                            case 9:  valor = item.Septiembre; break;
                            case 10: valor = item.Octubre; break;
                            case 11: valor = item.Noviembre; break;
                            case 12: valor = item.Diciembre; break;
                        }

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