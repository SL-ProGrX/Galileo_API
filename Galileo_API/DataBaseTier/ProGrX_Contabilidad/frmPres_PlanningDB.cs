using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.PRES;
using System.Data;
using System.Globalization;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace PgxAPI.DataBaseTier
{
    public class frmPres_PlanningDB
    {
        private readonly IConfiguration _config;
        private int vModulo = 12;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmPres_PlanningDB(IConfiguration config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Busca el presupuesto según filtros puestos por el usuario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<List<presVistaPresupuestoData>> PresPlanning_Obtener(int CodCliente, string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            presVistaPresupuestoBuscar filtros = JsonConvert.DeserializeObject<presVistaPresupuestoBuscar>(datos);
            var info = new ErrorDto<List<presVistaPresupuestoData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<presVistaPresupuestoData>()
            };
            try
            {


                using var connection = new SqlConnection(clienteConnString);
                {

                    

                    var procedure = "[spPres_VistaPresupuesto]";
                    var values = new
                    {
                        COD_CONTA = filtros.cod_conta,
                        COD_MODELO = filtros.cod_modelo,
                        COD_UNIDAD = filtros.cod_unidad,
                        CENTRO_COSTO = filtros.centro_costo,
                        ANIO = filtros.anio,
                        MES = filtros.mes,
                        TIPO_VISTA = filtros.tipo_vista,
                        CtaMov = filtros.ctaMov ? (bool?)true : null 
                    };

                    info.Result = connection.Query<presVistaPresupuestoData>(procedure, values, commandType: CommandType.StoredProcedure, commandTimeout: 600).ToList();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<presVistaPresupuestoData>();
            }

            return info;
        }

        /// <summary>
        /// Metodo que obtiene la información de las cuentas del presupuesto según los filtros puestos por el usuario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<List<preVistaPresupuestoCuentaData>> PresPlanningCuenta_Obtener(int CodCliente, string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            presVistaPresupuestoCuentaBuscar filtros = JsonConvert.DeserializeObject<presVistaPresupuestoCuentaBuscar>(datos);
            var info = new ErrorDto<List<preVistaPresupuestoCuentaData>>
            { 
                Code = 0,
                Description = "OK",
                Result = new List<preVistaPresupuestoCuentaData>()
            };
            try
            {


                using var connection = new SqlConnection(clienteConnString);
                {
                    string vFecha = null;

                    var procedure = "[spPres_VistaPresupuesto_Cuenta]";
                    var values = new
                    {
                        COD_CONTA = filtros.cod_conta,
                        COD_MODELO = filtros.cod_modelo,
                        COD_UNIDAD = filtros.cod_unidad,
                        CENTRO_COSTO = filtros.centro_costo,
                        CUENTA = filtros.cuenta,
                        TIPO_VISTA = filtros.tipo_vista,
                        Periodo = vFecha

                    };

                    info.Result = connection.Query<preVistaPresupuestoCuentaData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }

                return info;
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<preVistaPresupuestoCuentaData>();
            }
            return info;
        }

        /// <summary>
        /// Obtiene la información de las cuentas del presupuesto real histórico según los filtros puestos por el usuario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<List<presVistaPresCuentaRealHistoricoData>> PresPlanningCuentaReal_Obtener(int CodCliente, string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            presPresCuentaRealBuscar filtros = JsonConvert.DeserializeObject<presPresCuentaRealBuscar>(datos);
            var info = new ErrorDto<List<presVistaPresCuentaRealHistoricoData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<presVistaPresCuentaRealHistoricoData>()
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "[spPres_Cuenta_Real_Historico]";
                    var values = new
                    {
                        COD_CONTA = filtros.cod_conta,
                        COD_MODELO = filtros.cod_modelo,
                        MES = filtros.mes,
                        COD_UNIDAD = filtros.cod_unidad,
                        CENTRO_COSTO = filtros.centro_costo,
                        CUENTA = filtros.cuenta,
                        TIPO_VISTA = filtros.tipo_vista,
                    };

                    info.Result = connection.Query<presVistaPresCuentaRealHistoricoData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }

                return info;
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<presVistaPresCuentaRealHistoricoData>();
            }
            return info;
        }

        /// <summary>
        /// Guarda los ajustes del presupuesto según los parámetros enviados por el usuario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PresAjustes_Guardar(int CodCliente, presAjustesGuarda request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            presTiposAjustes tipoAjuste = new presTiposAjustes();
            resp.Code = 0;
            try
            {
                //Validar Movimientos sobre Consolidados
                if (request.cod_unidad == "CONSOLIDADO" || request.centro_costo == "CONSOLIDADO")
                {
                    resp.Description = "No se permiten ajustes sobre Consolidados!";
                    resp.Code = -1;
                    return resp;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    //Valida que el periodo no este cerrado
                    var estadoPeriodo = $@"select p.ESTADO 
                            from CntX_Cierres c
                            left join CntX_Periodos p on c.CORTE_ANIO = p.ANIO and p.COD_CONTABILIDAD = c.COD_CONTABILIDAD  
                            WHERE c.COD_CONTABILIDAD = @contabilidad and c.ID_CIERRE = 
                            (select ID_CIERRE from PRES_MODELOS where COD_MODELO = @modelo)
                            AND c.ACTIVO = 1
                            AND p.ESTADO = 'P' AND p.MES = @mes";
                    var periodoAct = connection.Query<string>(estadoPeriodo,
                        new {
                            contabilidad = request.cod_conta,
                            modelo = request.cod_modelo,
                            mes = request.mes
                        }).FirstOrDefault();
                    if(periodoAct == null)
                    {
                        resp.Description = "No se permiten ajustes a periodos cerrados!";
                        resp.Code = -1;
                        return resp;
                    }



                    //Validar el Tipo de Ajustes +/- y que la cuenta reciba movimientos
                    var validaTipoAjust = $@"SELECT * FROM pres_tipos_ajustes where cod_ajuste = '{request.ajuste_id}'";
                    tipoAjuste = connection.Query<presTiposAjustes>(validaTipoAjust).FirstOrDefault();
                    if (request.mnt_ajuste > 0 && tipoAjuste.ajuste_libre_positivo == 0)
                    {
                        resp.Description = "El tipo de Ajuste: " + tipoAjuste.descripcion + " ,no concuerda con el valor del cambio!";
                        resp.Code = -1;
                        return resp;
                    }
                    if (request.mnt_ajuste < 0 && tipoAjuste.ajuste_libre_negativo == 0)
                    {
                        resp.Description = "El tipo de Ajuste: " + tipoAjuste.descripcion + " ,no concuerda con el valor del cambio!";
                        resp.Code = -1;
                        return resp;
                    }

                    //Ejecutar procedimiento almacenado spPres_PresupuestoAjustesGuarda
                    var query = "exec spPres_PresupuestoAjustesGuarda @Contabilidad,@Modelo,@Anio,@Mes,@Cuenta," +
                        "@Mnt_MensualNuevo,@Mnt_Ajuste,@Unidad,@CentroCosto,@Notas,@Usuario,@AjusteId";

                    var parameters = new DynamicParameters();
                    parameters.Add("Contabilidad", request.cod_conta, DbType.Int32);
                    parameters.Add("Modelo", request.cod_modelo, DbType.String);
                    parameters.Add("Anio", request.anio, DbType.Int32);
                    parameters.Add("Mes", request.mes, DbType.Int32);
                    parameters.Add("Cuenta", request.cuenta, DbType.String);
                    parameters.Add("Mnt_MensualNuevo", request.mensual_nuevo, DbType.Decimal);
                    parameters.Add("Mnt_Ajuste", request.mnt_ajuste, DbType.Decimal);
                    parameters.Add("Unidad", request.cod_unidad, DbType.String);
                    parameters.Add("CentroCosto", request.centro_costo, DbType.String);
                    parameters.Add("Notas", request.notas, DbType.String);
                    parameters.Add("Usuario", request.usuario, DbType.String);
                    parameters.Add("AjusteId", request.ajuste_id, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ajustes aplicados satisfactoriamente!";
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
        /// Obtiene el cierre del presupuesto según el modelo y la contabilidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codModelo"></param>
        /// <param name="codContab"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CntxCierres> Pres_Cierre_Obtener(int CodEmpresa, string codModelo, int codContab, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<CntxCierres> resp = new ErrorDto<CntxCierres>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Cc.INICIO_ANIO,Cc.INICIO_MES, Cc.CORTE_ANIO, Cc.CORTE_MES, Pm.Estado
                    from CNTX_CIERRES Cc inner join PRES_MODELOS Pm on Cc.COD_CONTABILIDAD = Pm.COD_CONTABILIDAD and Cc.ID_CIERRE = Pm.ID_CIERRE
                    where Pm.COD_CONTABILIDAD = {codContab}
                    and Pm.COD_MODELO = '{codModelo}' order by Cc.INICIO_ANIO desc";
                    resp.Result = connection.Query<CntxCierres>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Cierre_Obtener - " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene los ajustes del presupuesto según los filtros puestos por el usuario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="consulta"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<List<preVistaPresupuestoCuentaData>> Pres_Ajustes_Obtener(int CodCliente, int consulta ,string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            presVistaPresupuestoBuscar filtros = JsonConvert.DeserializeObject<presVistaPresupuestoBuscar>(datos);
            var info = new ErrorDto<List<preVistaPresupuestoCuentaData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<preVistaPresupuestoCuentaData>()
            };
            try
            {


                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "";

                    filtros.centro_costo = (filtros.centro_costo == "TODOS") ? null : filtros.centro_costo;

                    switch (consulta)
                    {
                        case 0: //Presupuesto del Periodo
                            filtros.periodo = null;

                            procedure = "[spPres_VistaPresupuesto_Cuenta]";
                            var values = new
                            {
                                COD_CONTA = filtros.cod_conta,
                                COD_MODELO = filtros.cod_modelo,
                                COD_UNIDAD = filtros.cod_unidad,
                                CENTRO_COSTO = filtros.centro_costo,
                                CUENTA = filtros.cuenta,
                                TIPO_VISTA = filtros.tipo_vista,
                                Periodo = filtros.periodo
                            };

                            info.Result = connection.Query<preVistaPresupuestoCuentaData>(procedure, values, commandType: CommandType.StoredProcedure, commandTimeout: 600).ToList();
                            break;
                        case 1://Ajustes
                        case 2:

                            

                            if (consulta == 1)
                            {
                                //convertir a DateTime 
                                //formateo la hora a 23:59:00.000
                                DateTime vFecha = Convert.ToDateTime(filtros.periodo);

                                // Ajustar la fecha al último día del mes a las 23:59:00.000
                                DateTime fechaFinal = new DateTime(vFecha.Year, vFecha.Month,
                                                                   DateTime.DaysInMonth(vFecha.Year, vFecha.Month),
                                                                   23, 59, 0, 0);

                                // Convertir al formato deseado
                                string vStringFecha = fechaFinal.ToString("yyyy-MM-dd HH:mm:ss.fff");

                              
                               
                                procedure = "[spPres_PresupuestoAjustesConsulta]";
                                var valuesAjustes = new
                                {
                                    Contabilidad = filtros.cod_conta,
                                    Modelo = filtros.cod_modelo,
                                    Unidad = filtros.cod_unidad,
                                    CentroCosto = filtros.centro_costo,
                                    Cuenta = filtros.cuenta,
                                    Periodo = vStringFecha
                                };
                                info.Result = connection.Query<preVistaPresupuestoCuentaData>(procedure, valuesAjustes, commandType: CommandType.StoredProcedure, commandTimeout: 600).ToList();
                            }
                            else
                            {
                                filtros.periodo = null;

                                procedure = "[spPres_PresupuestoAjustesConsulta]";
                                var valuesAjuste = new
                                {
                                    Contabilidad = filtros.cod_conta,
                                    Modelo = filtros.cod_modelo,
                                    Unidad = filtros.cod_unidad,
                                    CentroCosto = filtros.centro_costo,
                                    Cuenta = filtros.cuenta,
                                    Periodo = filtros.periodo
                                };
                                info.Result = connection.Query<preVistaPresupuestoCuentaData>(procedure, valuesAjuste, commandType: CommandType.StoredProcedure, commandTimeout: 600).ToList();
                            }

                                
                            break;
                        default:
                            
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<preVistaPresupuestoCuentaData>();
            }

            return info;
        }

        /// <summary>
        /// Obtiene los modelos de presupuesto según la contabilidad y el usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContab"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<presModelisLista>> Pres_Modelos_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<presModelisLista>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<presModelisLista>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select P.cod_modelo as 'IdX' , P.DESCRIPCION as 'ItmX', P.ESTADO ,Cc.Inicio_Anio
                    From PRES_MODELOS P INNER JOIN PRES_MODELOS_USUARIOS Pmu on P.cod_Contabilidad = Pmu.cod_contabilidad
                     and P.cod_Modelo = Pmu.cod_Modelo and Pmu.Usuario = @usuario
                    INNER JOIN CNTX_CIERRES Cc on P.cod_Contabilidad = Cc.cod_Contabilidad and P.ID_CIERRE = Cc.ID_CIERRE 
                    Where P.COD_CONTABILIDAD = @contabilidad
                    group by P.cod_Modelo, P.Descripcion,P.ESTADO ,Cc.Inicio_Anio 
                    order by Cc.INICIO_ANIO desc, P.Cod_Modelo";
                    resp.Result = connection.Query<presModelisLista>(query, 
                        new {
                            contabilidad = CodContab,
                            usuario = Usuario
                        }
                        ).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelos_Obtener - " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene los ajustes permitidos para un modelo de presupuesto según la contabilidad y el usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContab"></param>
        /// <param name="codModelo"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<ModeloGenericList>> Pres_Ajustes_Permitidos_Obtener(int CodEmpresa, int codContab, string codModelo, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<ModeloGenericList>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Modelo_Ajustes_Permitidos {codContab},'{codModelo}', '{Usuario}'";
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
    
        public ErrorDto Pres_AjusteMasivo_Guardar(int CodEmpresa, int codContab, string codModelo, string usuario, DateTime periodo, List<presCargaMasivaModel> datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto();
            try
            {
                int vMes = periodo.Month;
                int vAnio = periodo.Year;

                using var connection = new SqlConnection(stringConn);
                {
                    //Valida que el periodo no este cerrado
                    var estadoPeriodo = $@"select p.ESTADO 
                            from CntX_Cierres c
                            left join CntX_Periodos p on c.CORTE_ANIO = p.ANIO and p.COD_CONTABILIDAD = c.COD_CONTABILIDAD  
                            WHERE c.COD_CONTABILIDAD = @contabilidad and c.ID_CIERRE = 
                            (select ID_CIERRE from PRES_MODELOS where COD_MODELO = @modelo)
                            AND c.ACTIVO = 1
                            AND p.ESTADO = 'P' AND p.MES = @mes";
                    var periodoAct = connection.Query<string>(estadoPeriodo,
                        new
                        {
                            contabilidad = codContab,
                            modelo = codModelo,
                            mes = vMes
                        }).FirstOrDefault();
                    if (periodoAct == null)
                    {
                        resp.Description = "No se permiten ajustes a periodos cerrados!";
                        resp.Code = -1;
                        return resp;
                    }

                    //Traigo los tipos de ajustes permitidos para el modelo
                    var ajustesPermitidos = Pres_Ajustes_Permitidos_Obtener(CodEmpresa, codContab, codModelo, usuario).Result;
                    int row = 0;
                    //recorro la lista de datos a insertar y valido que el tipo de ajuste este permitido
                    foreach (var linea in datos)
                    {
                        row++;
                        var ajusteValido = ajustesPermitidos.Where(a => a.IdX == linea.movimiento).FirstOrDefault();
                        if(ajusteValido == null)
                        {
                            resp.Description += $"El tipo de ajuste {linea.movimiento} no está permitido para el modelo {codModelo}  \n";
                            resp.Code = -1;
                            
                        }
                    }

                    if(resp.Code == -1)
                    {
                        return resp;
                    }

                    string msjErrpr = "";
                    row = 0;
                    bool hayError = false;
                    foreach (var linea in datos)
                    {
                        row++;
                        hayError = false;
                        //valido que la unidad exista.
                        var unidadExiste = $@" select COUNT(*) from CNTX_UNIDADES WHERE COD_UNIDAD = @unidad AND COD_CONTABILIDAD = @contabilidad";
                        var unidad = connection.Query<int>(unidadExiste,
                            new
                            {
                                unidad = linea.unidad,
                                contabilidad = codContab
                            }).FirstOrDefault();
                        if (unidad == 0)
                        {
                            hayError = true;
                            msjErrpr += $"La linea {row} contine una unidad incorrecta { linea.unidad }  \n";
                        }

                        //valido que el centro de costo exista.
                        var centroCostoExiste = $@" select COUNT(*) from CNTX_UNIDADES_CC WHERE COD_UNIDAD = @unidad AND COD_CONTABILIDAD = @contabilidad";
                        var centroCosto = connection.Query<int>(centroCostoExiste,
                            new
                            {
                                unidad = linea.unidad,
                                contabilidad = codContab
                            }).FirstOrDefault();
                        if (centroCosto == 0)
                        {
                            hayError = true;
                            msjErrpr += $"La linea {row} contine un centro de costo incorrecto { linea.cc }  \n";
                        }

                        if (hayError)
                        {
                            continue;
                        }
                        else
                        {
                            string vFecha = null;
                            string cuenta = linea.cuenta.Replace("-", "");
                            var procedure = "[spPres_VistaPresupuesto_Cuenta]";
                            var values = new
                            {
                                COD_CONTA = codContab,
                                COD_MODELO = codModelo,
                                COD_UNIDAD = linea.unidad,
                                CENTRO_COSTO = linea.cc,
                                CUENTA = cuenta,
                                TIPO_VISTA = "C",
                                Periodo = vFecha

                            };

                            var lista = connection.Query<preVistaPresupuestoCuentaData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                            //busco el mes en la lista
                            if(lista.Count > 0)
                            {
                                var rowData = lista.Find(x => x.mes == vMes).mensual;


                                presAjustesGuarda guarda = new presAjustesGuarda();
                                guarda.anio = vAnio;
                                guarda.mes = vMes;
                                guarda.cod_conta = codContab;
                                guarda.cod_modelo = codModelo;
                                guarda.cuenta = linea.cuenta.Replace("-", "");
                                guarda.mensual_nuevo = rowData;
                                guarda.mnt_ajuste = linea.valor;
                                guarda.cod_unidad = linea.unidad;
                                guarda.centro_costo = linea.cc;
                                guarda.notas = $"Ajuste Masivo {usuario}";
                                guarda.usuario = usuario;
                                guarda.ajuste_id = linea.movimiento;
                                //Guardo el ajuste

                                ErrorDto saveResp = PresAjustes_Guardar(CodEmpresa, guarda);
                                if (saveResp.Code != 0)
                                {
                                    if(saveResp.Description.Contains("Ajustes aplicados satisfactoriamente!")){

                                    }
                                    else
                                    {
                                        msjErrpr += $"Error en la linea {row} : {saveResp.Description} \n";
                                    }

                                        
                                }
                            }
                            else
                            {
                                msjErrpr += $"No se encontró información para la cuenta {linea.cuenta} en la línea {row} \n";
                            }
                           
                        }
                        
                    }
                
                    if (msjErrpr.Length > 0)
                    {
                        resp.Code = -1;
                        resp.Description = msjErrpr + "  \n\n**Los demas movimientos fueron aplicados satisfactoriamente** \n";
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
    
    }
}