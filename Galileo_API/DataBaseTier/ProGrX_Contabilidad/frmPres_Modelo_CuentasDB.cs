using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.PRES;
using PgxAPI.Models.ProGrX.Bancos;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmPres_Modelo_CuentasDB
    {
        private readonly IConfiguration _config;

        public frmPres_Modelo_CuentasDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene las cuentas del catálogo de contabilidad para el modelo, unidad y centro de costo especificados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodContab">Código de la contabilidad.</param>
        /// <param name="CodModelo">Código del modelo.</param>
        /// <param name="CodUnidad">Código de la unidad.</param>
        /// <param name="CodCentroCosto">Código del centro de costo.</param>
        /// <returns>Un objeto ErrorDto que contiene una lista de CuentasCatalogoData. O mensaje de error</returns>
        public ErrorDto<List<CuentasCatalogoData>> spPres_CuentasCatalogo_Obtener(int CodEmpresa, int CodContab, string CodModelo, string CodUnidad, string CodCentroCosto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<CuentasCatalogoData>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_CuentasCatalogo {CodContab},'{CodModelo}','{CodUnidad}','{CodCentroCosto}'";
                    resp.Result = connection.Query<CuentasCatalogoData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene los modelos disponibles para un usuario específico en una contabilidad dada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodContab">Código de la contabilidad.</param>
        /// <param name="Usuario">Nombre de usuario.</param>
        /// <returns>Un objeto ErrorDto que contiene una lista de ModeloGenericList. O mensaje de error</returns>
        public ErrorDto<List<ModeloGenericList>> Pres_Modelos_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<ModeloGenericList>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select P.cod_modelo as 'IdX' , P.DESCRIPCION as 'ItmX'
                        From PRES_MODELOS P INNER JOIN PRES_MODELOS_USUARIOS Pmu on P.cod_Contabilidad = Pmu.cod_contabilidad
                        and P.cod_Modelo = Pmu.cod_Modelo and Pmu.Usuario = '{Usuario}'
                        inner join CNTX_CIERRES Cc on P.cod_Contabilidad = Cc.cod_Contabilidad and P.ID_CIERRE = Cc.ID_CIERRE 
                        Where P.COD_CONTABILIDAD = {CodContab}
                        order by Cc.Inicio_Anio desc";
                    resp.Result = connection.Query<ModeloGenericList>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Obtiene las unidades disponibles para un usuario específico en una contabilidad dada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodContab">Código de la contabilidad.</param>
        /// <param name="Usuario">Nombre de usuario.</param>
        /// <returns>Un objeto ErrorDto que contiene una lista de ModeloGenericList. O mensaje de error</returns>
        public ErrorDto<List<ModeloGenericList>> Pres_Unidades_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<ModeloGenericList>>
            {
                Code = 0
            }; try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Cu.Cod_Unidad as 'IdX' , Cu.DESCRIPCION as 'ItmX'
                    From CNTX_UNIDADES Cu INNER JOIN PRES_USUARIOS_NIVEL Pun on Cu.cod_Contabilidad = Pun.cod_contabilidad
                    and Cu.cod_Unidad = Pun.Cod_Unidad and Pun.Usuario = '{Usuario}'
                    Where Cu.COD_CONTABILIDAD = {CodContab}";
                    resp.Result = connection.Query<ModeloGenericList>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Obtiene los centros de costo disponibles para una contabilidad y unidad específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodContab">Código de la contabilidad.</param>
        /// <param name="CodUnidad">Código de la unidad.</param>
        /// <returns>Un objeto ErrorDto que contiene una lista de ModeloGenericList. O mensaje de error</returns>   
        public ErrorDto<List<ModeloGenericList>> Pres_CentroCosto_Obtener(int CodEmpresa, int CodContab, string CodUnidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<ModeloGenericList>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Cc.COD_CENTRO_COSTO as 'IdX', Cc.DESCRIPCION as 'ItmX'
                    from CNTX_CENTRO_COSTOS Cc left join CNTX_UNIDADES_CC Uc on Cc.COD_CONTABILIDAD = Uc.COD_CONTABILIDAD 
                    and Uc.COD_UNIDAD = '{CodUnidad}' 
                    Where Cc.COD_CONTABILIDAD = {CodContab}";
                    resp.Result = connection.Query<ModeloGenericList>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Carga datos de cuentas del modelo de presupuesto en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Lista de datos de cuentas del modelo a cargar.</param>    
        /// /// <returns>Un objeto ErrorDto que indica el resultado de la operación.</returns>
        public ErrorDto spPres_Modelo_Cuentas_CargaDatos(int CodEmpresa, List<PresModeloCuentasImportData> request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int Inicializa = 1;
                    foreach (PresModeloCuentasImportData row in request)
                    {
                        var fechaFormateada = row.Corte.Split(' ')[0] + " 23:59:59";

                        var procedure = "[spPres_Presupuesto_Import_Load]";
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
                            Inicializa = Inicializa++
                        };
                        connection.Execute(procedure, values, commandTimeout: 150);
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }


        /// <summary>
        /// Revisa los datos de importación de un modelo de cuentas en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodContab">Código de la contabilidad.</param>
        /// <param name="CodModelo">Código del modelo a revisar.</param>
        /// <param name="Usuario">Nombre de usuario que realiza la revisión.</param>
        /// <returns>Un objeto ErrorDto que contiene una lista de PresModeloCuentasImportData. O mensaje de error</returns>
        public ErrorDto<List<PresModeloCuentasImportData>> spPres_Modelo_Cuentas_RevisaImport(int CodEmpresa, int CodContab, string CodModelo, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PresModeloCuentasImportData>>
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Presupuesto_Import_Revisa '{CodModelo}', {CodContab}, '{Usuario}'";

                    resp.Result = connection.Query<PresModeloCuentasImportData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }


        /// <summary>
        /// Importa un modelo de cuentas en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodContab">Código de la contabilidad.</param>
        /// <param name="CodModelo">Código del modelo a importar.</param>
        /// <param name="Usuario">Nombre de usuario que realiza la importación.</param>
        /// <returns>Un objeto ErrorDto que indica el resultado de la operación.</returns>
        public ErrorDto spPres_Modelo_Cuentas_Import(int CodEmpresa, int CodContab, string CodModelo, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0
            };
            int pendientes = 0;
            int procesados = 0;
            string vMensaje = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query1 = $@"exec spPres_Presupuesto_Import_Mapeo @Modelo, @Contabilidad, @Usuario";
                    connection.Execute(query1, 
                        new
                        {
                            Modelo = CodModelo,
                            Contabilidad = CodContab,
                            Usuario = Usuario
                        }, commandTimeout: 150);

                    var query2 = $@"exec spPres_Presupuesto_Import_Procesa @Modelo, @Contabilidad, @Usuario";
                    var result = connection.QueryFirstOrDefault(query2, new
                    {
                        Modelo = CodModelo,
                        Contabilidad = CodContab,
                        Usuario = Usuario
                    }, commandTimeout: 150);

                    pendientes = result.Pendientes ?? 0;
                    procesados = result.Procesados ?? 0;

                    int i = 0;
                    int ciclo = (int)Math.Ceiling((double)pendientes / 50);
                    while (pendientes > 0)
                    {
                        if (i == ciclo)
                        {
                            vMensaje = "<br> Pendientes: " + pendientes + ", Procesados: " + procesados;
                            break;
                        }
                        var result2 = connection.QueryFirstOrDefault(query2, new
                        {
                            Modelo = CodModelo,
                            Contabilidad = CodContab,
                            Usuario = Usuario
                        }, commandTimeout: 150);

                        pendientes = result2.Pendientes ?? 0;
                        procesados = result2.Procesados ?? 0;

                        i++;
                    }

                    resp.Description = "Importaci&oacute;n del Presupuesto realizado satisfactoriamente!" + vMensaje;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Obtiene los meses del período fiscal para un modelo de cuentas específico.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodContab">Código de la contabilidad.</param>
        /// <param name="CodModelo">Código del modelo de cuentas.</param>
        /// <param name="Usuario">Nombre de usuario que realiza la consulta.</param>
        /// <param name="request">Lista de datos de cuentas del modelo a procesar.</param>
        /// <returns>Un objeto ErrorDto que contiene una lista de PresModeloCuentasImportData. O mensaje de error</returns>
        public ErrorDto<List<PresModeloCuentasImportData>> spCntXPeriodoFiscalMeses(int CodEmpresa, int CodContab, string CodModelo, string Usuario, List<PresModeloCuentasHorizontal> request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PresModeloCuentasImportData>>
            {
                Code = 0,
                Result = new List<PresModeloCuentasImportData>()
            };
            List<CntXPeriodoFiscalMeses> periodos = new List<CntXPeriodoFiscalMeses>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spCntXPeriodoFiscalMeses {CodContab}, 0, '{CodModelo}'";

                    periodos = connection.Query<CntXPeriodoFiscalMeses>(query).ToList();

                    foreach (var item in request)
                    {
                        foreach (var periodo in periodos)
                        {
                            float valor = 0;
                            switch (periodo.Mes)
                            {
                                case 1:
                                    valor = item.Enero;
                                    break;
                                case 2:
                                    valor = item.Febrero;
                                    break;
                                case 3:
                                    valor = item.Marzo;
                                    break;
                                case 4:
                                    valor = item.Abril;
                                    break;
                                case 5:
                                    valor = item.Mayo;
                                    break;
                                case 6:
                                    valor = item.Junio;
                                    break;
                                case 7:
                                    valor = item.Julio;
                                    break;
                                case 8:
                                    valor = item.Agosto;
                                    break;
                                case 9:
                                    valor = item.Septiembre;
                                    break;
                                case 10:
                                    valor = item.Octubre;
                                    break;
                                case 11:
                                    valor = item.Noviembre;
                                    break;
                                case 12:
                                    valor = item.Diciembre;
                                    break;
                            }

                            if (valor >= 0)
                            {
                                resp.Result.Add(new PresModeloCuentasImportData
                                {
                                    Cod_Contabilidad = CodContab,
                                    Cod_Modelo = CodModelo,
                                    Cod_Cuenta = item.Cuenta,
                                    Descripcion = item.Descripcion,
                                    Cod_Unidad = item.Unidad,
                                    Cod_Centro_Costo = item.Centro,
                                    Monto = valor,
                                    Corte = periodo.Corte.ToString("yyyy-MM-dd HH:mm:ss"),
                                    Usuario = Usuario,
                                    Inicializa = 1
                                });
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

    }
}