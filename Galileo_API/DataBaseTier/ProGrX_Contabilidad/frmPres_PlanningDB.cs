using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPresPlanningDb
    {
        private readonly IConfiguration _config;
        public FrmPresPlanningDb(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Busca el presupuesto según filtros puestos por el usuario
        /// </summary>
        public ErrorDto<List<PresVistaPresupuestoData>> PresPlanning_Obtener(int CodCliente, string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            PresVistaPresupuestoBuscar? filtros = JsonConvert.DeserializeObject<PresVistaPresupuestoBuscar>(datos);
            var info = new ErrorDto<List<PresVistaPresupuestoData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresVistaPresupuestoData>()
            };

            if (filtros == null)
            {
                info.Code = -1;
                info.Description = "No se pudieron deserializar los filtros de presupuesto.";
                return info;
            }

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string procedure = "[spPres_VistaPresupuesto]";
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

                info.Result = connection
                    .Query<PresVistaPresupuestoData>(procedure, values,
                        commandType: CommandType.StoredProcedure, commandTimeout: 600)
                    .ToList();
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<PresVistaPresupuestoData>();
            }

            return info;
        }

        /// <summary>
        /// Método que obtiene la información de las cuentas del presupuesto según los filtros puestos por el usuario
        /// </summary>
        public ErrorDto<List<PreVistaPresupuestoCuentaData>> PresPlanningCuenta_Obtener(int CodCliente, string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            PresVistaPresupuestoCuentaBuscar? filtros = JsonConvert.DeserializeObject<PresVistaPresupuestoCuentaBuscar>(datos);

            var info = new ErrorDto<List<PreVistaPresupuestoCuentaData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PreVistaPresupuestoCuentaData>()
            };

            if (filtros == null)
            {
                info.Code = -1;
                info.Description = "No se pudieron deserializar los filtros de presupuesto de cuenta.";
                return info;
            }

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                string? vFecha = null;
                const string procedure = "[spPres_VistaPresupuesto_Cuenta]";

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

                info.Result = connection
                    .Query<PreVistaPresupuestoCuentaData>(procedure, values, commandType: CommandType.StoredProcedure)
                    .ToList();

                return info;
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<PreVistaPresupuestoCuentaData>();
            }

            return info;
        }

        /// <summary>
        /// Obtiene la información de las cuentas del presupuesto real histórico según los filtros puestos por el usuario
        /// </summary>
        public ErrorDto<List<PresVistaPresCuentaRealHistoricoData>> PresPlanningCuentaReal_Obtener(int CodCliente, string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            PresPresCuentaRealBuscar? filtros = JsonConvert.DeserializeObject<PresPresCuentaRealBuscar>(datos);

            var info = new ErrorDto<List<PresVistaPresCuentaRealHistoricoData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresVistaPresCuentaRealHistoricoData>()
            };

            if (filtros == null)
            {
                info.Code = -1;
                info.Description = "No se pudieron deserializar los filtros de cuenta real histórico.";
                return info;
            }

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string procedure = "[spPres_Cuenta_Real_Historico]";
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

                info.Result = connection
                    .Query<PresVistaPresCuentaRealHistoricoData>(procedure, values, commandType: CommandType.StoredProcedure)
                    .ToList();

                return info;
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<PresVistaPresCuentaRealHistoricoData>();
            }

            return info;
        }

        /// <summary>
        /// Guarda los ajustes del presupuesto según los parámetros enviados por el usuario
        /// </summary>
        public ErrorDto PresAjustes_Guardar(int CodCliente, PresAjustesGuarda request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto { Code = 0 };

            const string estadoPeriodoQuery = @"
                SELECT p.ESTADO 
                FROM CntX_Cierres c
                LEFT JOIN CntX_Periodos p 
                    ON c.CORTE_ANIO = p.ANIO AND p.COD_CONTABILIDAD = c.COD_CONTABILIDAD  
                WHERE c.COD_CONTABILIDAD = @contabilidad 
                  AND c.ID_CIERRE = (SELECT ID_CIERRE FROM PRES_MODELOS WHERE COD_MODELO = @modelo)
                  AND c.ACTIVO = 1
                  AND p.ESTADO = 'P' AND p.MES = @mes";

            const string tipoAjusteSql = @"
                SELECT *
                FROM pres_tipos_ajustes
                WHERE cod_ajuste = @CodAjuste";

            try
            {
                if (request.cod_unidad == "CONSOLIDADO" || request.centro_costo == "CONSOLIDADO")
                {
                    resp.Description = "No se permiten ajustes sobre Consolidados!";
                    resp.Code = -1;
                    return resp;
                }

                using var connection = new SqlConnection(stringConn);

                var periodoAct = connection.Query<string>(
                    estadoPeriodoQuery,
                    new
                    {
                        contabilidad = request.cod_conta,
                        modelo = request.cod_modelo,
                        mes = request.mes
                    }).FirstOrDefault();

                if (periodoAct == null)
                {
                    resp.Description = "No se permiten ajustes a periodos cerrados!";
                    resp.Code = -1;
                    return resp;
                }

                var tipoAjuste = connection.QueryFirstOrDefault<PresTiposAjustes>(
                    tipoAjusteSql,
                    new { CodAjuste = request.ajuste_id });

                if (tipoAjuste == null)
                {
                    resp.Description = "No se encontró el tipo de ajuste especificado.";
                    resp.Code = -1;
                    return resp;
                }

                if (request.mnt_ajuste > 0 && tipoAjuste.ajuste_libre_positivo == 0)
                {
                    resp.Description = $"El tipo de Ajuste: {tipoAjuste.descripcion} ,no concuerda con el valor del cambio!";
                    resp.Code = -1;
                    return resp;
                }

                if (request.mnt_ajuste < 0 && tipoAjuste.ajuste_libre_negativo == 0)
                {
                    resp.Description = $"El tipo de Ajuste: {tipoAjuste.descripcion} ,no concuerda con el valor del cambio!";
                    resp.Code = -1;
                    return resp;
                }

                const string procedure = "spPres_PresupuestoAjustesGuarda";

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

                resp.Code = connection
                    .ExecuteAsync(procedure, parameters, commandType: CommandType.StoredProcedure)
                    .Result;

                resp.Description = "Ajustes aplicados satisfactoriamente!";
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
        public ErrorDto<CntxCierres> Pres_Cierre_Obtener(int CodEmpresa, string codModelo, int codContab, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<CntxCierres>();

            const string sql = @"
                SELECT 
                    Cc.INICIO_ANIO,
                    Cc.INICIO_MES,
                    Cc.CORTE_ANIO,
                    Cc.CORTE_MES,
                    Pm.Estado
                FROM CNTX_CIERRES Cc 
                INNER JOIN PRES_MODELOS Pm 
                    ON Cc.COD_CONTABILIDAD = Pm.COD_CONTABILIDAD 
                    AND Cc.ID_CIERRE       = Pm.ID_CIERRE
                WHERE Pm.COD_CONTABILIDAD = @CodContab
                  AND Pm.COD_MODELO       = @CodModelo
                ORDER BY Cc.INICIO_ANIO DESC";

            try
            {
                using var connection = new SqlConnection(stringConn);

                resp.Result = connection.Query<CntxCierres>(
                    sql,
                    new { CodContab = codContab, CodModelo = codModelo })
                    .FirstOrDefault();
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
        public ErrorDto<List<PreVistaPresupuestoCuentaData>> Pres_Ajustes_Obtener(
            int CodCliente,
            int consulta,
            string datos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var filtros = JsonConvert.DeserializeObject<PresVistaPresupuestoBuscar>(datos);

            var info = new ErrorDto<List<PreVistaPresupuestoCuentaData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PreVistaPresupuestoCuentaData>()
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                if (filtros == null)
                {
                    return CrearRespuestaError(
                        "Los filtros no pueden ser nulos para la consulta de ajustes.");
                }

                NormalizarCentroCosto(filtros);

                switch (consulta)
                {
                    case 0:
                        EjecutarConsultaPresupuestoPeriodo(connection, filtros, info);
                        break;

                    case 1:
                        EjecutarConsultaAjustesConPeriodo(connection, filtros, info);
                        break;

                    case 2:
                        EjecutarConsultaAjustesSinPeriodo(connection, filtros, info);
                        break;

                    default:
                        info.Code = -1;
                        info.Description = "Tipo de consulta no válido.";
                        info.Result = new List<PreVistaPresupuestoCuentaData>();
                        break;
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<PreVistaPresupuestoCuentaData>();
            }

            return info;
        }

        #region Métodos privados para reducir complejidad

        private static ErrorDto<List<PreVistaPresupuestoCuentaData>> CrearRespuestaError(string mensaje)
        {
            return new ErrorDto<List<PreVistaPresupuestoCuentaData>>
            {
                Code = -1,
                Description = mensaje,
                Result = new List<PreVistaPresupuestoCuentaData>()
            };
        }

        private static void NormalizarCentroCosto(PresVistaPresupuestoBuscar filtros)
        {
            if (!string.IsNullOrEmpty(filtros.centro_costo) &&
                filtros.centro_costo == "TODOS")
            {
                filtros.centro_costo = null;
            }
        }

        private static void EjecutarConsultaPresupuestoPeriodo(
            SqlConnection connection,
            PresVistaPresupuestoBuscar filtros,
            ErrorDto<List<PreVistaPresupuestoCuentaData>> info)
        {
            filtros.periodo = null;

            info.Result = connection.Query<PreVistaPresupuestoCuentaData>(
                "[spPres_VistaPresupuesto_Cuenta]",
                new
                {
                    COD_CONTA = filtros.cod_conta,
                    COD_MODELO = filtros.cod_modelo,
                    COD_UNIDAD = filtros.cod_unidad,
                    CENTRO_COSTO = filtros.centro_costo,
                    CUENTA = filtros.cuenta,
                    TIPO_VISTA = filtros.tipo_vista,
                    Periodo = filtros.periodo
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 600).ToList();
        }

        private static void EjecutarConsultaAjustesConPeriodo(
            SqlConnection connection,
            PresVistaPresupuestoBuscar filtros,
            ErrorDto<List<PreVistaPresupuestoCuentaData>> info)
        {
            if (string.IsNullOrEmpty(filtros.periodo))
            {
                info.Code = -1;
                info.Description = "Los filtros o el periodo no pueden ser nulos para la consulta de ajustes.";
                info.Result = new List<PreVistaPresupuestoCuentaData>();
                return;
            }

            // Si quieres ser más defensiva aún puedes usar DateTime.TryParse
            var vFecha = Convert.ToDateTime(filtros.periodo);

            var fechaFinal = new DateTime(
                vFecha.Year,
                vFecha.Month,
                DateTime.DaysInMonth(vFecha.Year, vFecha.Month),
                23, 59, 0, 0, DateTimeKind.Local);

            string vStringFecha = fechaFinal.ToString("yyyy-MM-dd HH:mm:ss.fff");

            info.Result = connection.Query<PreVistaPresupuestoCuentaData>(
                "[spPres_PresupuestoAjustesConsulta]",
                new
                {
                    Contabilidad = filtros.cod_conta,
                    Modelo = filtros.cod_modelo,
                    Unidad = filtros.cod_unidad,
                    CentroCosto = filtros.centro_costo,
                    Cuenta = filtros.cuenta,
                    Periodo = vStringFecha
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 600).ToList();
        }

        private static void EjecutarConsultaAjustesSinPeriodo(
            SqlConnection connection,
            PresVistaPresupuestoBuscar filtros,
            ErrorDto<List<PreVistaPresupuestoCuentaData>> info)
        {
            filtros.periodo = null;

            info.Result = connection.Query<PreVistaPresupuestoCuentaData>(
                "[spPres_PresupuestoAjustesConsulta]",
                new
                {
                    Contabilidad = filtros.cod_conta,
                    Modelo = filtros.cod_modelo,
                    Unidad = filtros.cod_unidad,
                    CentroCosto = filtros.centro_costo,
                    Cuenta = filtros.cuenta,
                    Periodo = filtros.periodo
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 600).ToList();
        }

        #endregion


        /// <summary>
        /// Obtiene los modelos de presupuesto según la contabilidad y el usuario
        /// </summary>
        public ErrorDto<List<PresModelisLista>> Pres_Modelos_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<PresModelisLista>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresModelisLista>()
            };

            const string query = @"
                select P.cod_modelo as 'IdX' , P.DESCRIPCION as 'ItmX', P.ESTADO ,Cc.Inicio_Anio
                From PRES_MODELOS P INNER JOIN PRES_MODELOS_USUARIOS Pmu on P.cod_Contabilidad = Pmu.cod_contabilidad
                 and P.cod_Modelo = Pmu.cod_Modelo and Pmu.Usuario = @usuario
                INNER JOIN CNTX_CIERRES Cc on P.cod_Contabilidad = Cc.cod_Contabilidad and P.ID_CIERRE = Cc.ID_CIERRE 
                Where P.COD_CONTABILIDAD = @contabilidad
                group by P.cod_Modelo, P.Descripcion,P.ESTADO ,Cc.Inicio_Anio 
                order by Cc.INICIO_ANIO desc, P.Cod_Modelo";

            try
            {
                using var connection = new SqlConnection(stringConn);

                resp.Result = connection.Query<PresModelisLista>(
                    query,
                    new
                    {
                        contabilidad = CodContab,
                        usuario = Usuario
                    }).ToList();
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
        public ErrorDto<List<ModeloGenericList>> Pres_Ajustes_Permitidos_Obtener(int CodEmpresa, int codContab, string codModelo, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<ModeloGenericList>>();

            const string procedure = "[spPres_Modelo_Ajustes_Permitidos]";

            try
            {
                using var connection = new SqlConnection(stringConn);

                resp.Result = connection.Query<ModeloGenericList>(
                    procedure,
                    new
                    {
                        CodContab = codContab,
                        CodModelo = codModelo,
                        Usuario
                    },
                    commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto Pres_AjusteMasivo_Guardar(
            int CodEmpresa,
            int codContab,
            string codModelo,
            string usuario,
            DateTime periodo,
            List<PresCargaMasivaModel> datos)
        {
            var resp = new ErrorDto();

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                int vMes = periodo.Month;
                int vAnio = periodo.Year;

                using var connection = new SqlConnection(stringConn);

                if (!PeriodoEstaAbierto(connection, codContab, codModelo, vMes))
                {
                    resp.Description = "No se permiten ajustes a periodos cerrados!";
                    resp.Code = -1;
                    return resp;
                }

                var ajustesPermitidos = Pres_Ajustes_Permitidos_Obtener(CodEmpresa, codContab, codModelo, usuario).Result;

                string movimientosNoPermitidosMensaje =
                    ObtenerMovimientosNoPermitidosMensaje(datos, ajustesPermitidos ?? new List<ModeloGenericList>(), codModelo);

                if (!string.IsNullOrEmpty(movimientosNoPermitidosMensaje))
                {
                    resp.Code = -1;
                    resp.Description = movimientosNoPermitidosMensaje;
                    return resp;
                }

                var msjError = new System.Text.StringBuilder();
                int row = 0;

                var ctx = new ProcesarLineaAjusteContext
                {
                    Connection = connection,
                    CodEmpresa = CodEmpresa,
                    CodContab = codContab,
                    CodModelo = codModelo,
                    Usuario = usuario,
                    Mes = vMes,
                    Anio = vAnio
                };

                foreach (var linea in datos)
                {
                    row++;
                    var mensajeLinea = ProcesarLineaAjuste(ctx, linea, row);
                    if (!string.IsNullOrEmpty(mensajeLinea))
                        msjError.AppendLine(mensajeLinea);
                }

                if (msjError.Length > 0)
                {
                    resp.Code = -1;
                    resp.Description = msjError +
                                       "  \n\n**Los demas movimientos fueron aplicados satisfactoriamente** \n";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        #region Métodos privados de refactor

        private static string ObtenerMovimientosNoPermitidosMensaje(
            IEnumerable<PresCargaMasivaModel> datos,
            List<ModeloGenericList> ajustesPermitidos,
            string codModelo)
        {
            var movimientosNoPermitidos = datos
                .Select(linea => linea.movimiento)
                .Where(movimiento =>
                    ajustesPermitidos == null ||
                    !ajustesPermitidos.Any(a => a.IdX == movimiento))
                .Distinct()
                .ToList();

            if (!movimientosNoPermitidos.Any())
            {
                return string.Empty;
            }

            return string.Join(
                "\n",
                movimientosNoPermitidos.Select(
                    m => $"El tipo de ajuste {m} no está permitido para el modelo {codModelo}"));
        }

        private bool PeriodoEstaAbierto(
            SqlConnection connection,
            int codContab,
            string codModelo,
            int mes)
        {
            const string EstadoPeriodoQuery = @"
                SELECT p.ESTADO 
                FROM CntX_Cierres c
                LEFT JOIN CntX_Periodos p 
                    ON c.CORTE_ANIO = p.ANIO 
                    AND p.COD_CONTABILIDAD = c.COD_CONTABILIDAD  
                WHERE   c.COD_CONTABILIDAD = @contabilidad 
                    AND c.ID_CIERRE = (SELECT ID_CIERRE FROM PRES_MODELOS WHERE COD_MODELO = @modelo)
                    AND c.ACTIVO = 1
                    AND p.ESTADO = 'P' 
                    AND p.MES = @mes";

            var periodoAct = connection.Query<string>(
                EstadoPeriodoQuery,
                new
                {
                    contabilidad = codContab,
                    modelo = codModelo,
                    mes
                }).FirstOrDefault();

            return periodoAct != null;
        }

        public class ProcesarLineaAjusteContext
        {
            public required SqlConnection Connection { get; set; }
            public int CodEmpresa { get; set; }
            public int CodContab { get; set; }
            public required string CodModelo { get; set; }
            public required string Usuario { get; set; }
            public int Mes { get; set; }
            public int Anio { get; set; }
        }

        private string ProcesarLineaAjuste(ProcesarLineaAjusteContext ctx, PresCargaMasivaModel linea, int row)
        {
            var sb = new System.Text.StringBuilder();

            if (!UnidadExiste(ctx.Connection, ctx.CodContab, linea.unidad))
                sb.AppendLine($"La linea {row} contine una unidad incorrecta {linea.unidad}");

            if (!CentroCostoExiste(ctx.Connection, ctx.CodContab, linea.unidad))
                sb.AppendLine($"La linea {row} contine un centro de costo incorrecto {linea.cc}");

            if (sb.Length > 0)
                return sb.ToString();

            var cuentaSinGuiones = (linea.cuenta ?? string.Empty).Replace("-", "");

            var presupuestoCuenta = ObtenerPresupuestoCuenta(
                ctx.Connection,
                ctx.CodContab,
                ctx.CodModelo,
                linea.unidad,
                linea.cc,
                cuentaSinGuiones);

            if (presupuestoCuenta == null)
            {
                sb.AppendLine($"No se encontró información para la cuenta {linea.cuenta} en la línea {row}");
                return sb.ToString();
            }

            var rowData = presupuestoCuenta.FirstOrDefault(x => x.mes == ctx.Mes);

            if (rowData == null)
            {
                sb.AppendLine($"No se encontró información para el mes {ctx.Mes} en la línea {row}");
                return sb.ToString();
            }

            var guarda = new PresAjustesGuarda
            {
                anio = ctx.Anio,
                mes = ctx.Mes,
                cod_conta = ctx.CodContab,
                cod_modelo = ctx.CodModelo,
                cuenta = cuentaSinGuiones,
                mensual_nuevo = rowData.mensual,
                mnt_ajuste = linea.valor,
                cod_unidad = linea.unidad,
                centro_costo = linea.cc,
                notas = $"Ajuste Masivo {ctx.Usuario}",
                usuario = ctx.Usuario,
                ajuste_id = linea.movimiento
            };

            ErrorDto saveResp = PresAjustes_Guardar(ctx.CodEmpresa, guarda);

            if (saveResp.Code != 0 &&
                (string.IsNullOrEmpty(saveResp.Description) ||
                 !saveResp.Description.Contains("Ajustes aplicados satisfactoriamente!")))
            {
                sb.AppendLine($"Error en la linea {row} : {saveResp.Description}");
            }

            return sb.ToString();
        }

        private bool UnidadExiste(SqlConnection connection, int codContab, string unidad)
        {
            const string UnidadExisteQuery = @"
                SELECT COUNT(*) 
                FROM CNTX_UNIDADES 
                WHERE COD_UNIDAD = @unidad 
                  AND COD_CONTABILIDAD = @contabilidad";

            int count = connection.Query<int>(
                UnidadExisteQuery,
                new
                {
                    unidad,
                    contabilidad = codContab
                }).FirstOrDefault();

            return count > 0;
        }

        private bool CentroCostoExiste(SqlConnection connection, int codContab, string unidad)
        {
            const string CentroCostoExisteQuery = @"
                SELECT COUNT(*) 
                FROM CNTX_UNIDADES_CC 
                WHERE COD_UNIDAD = @unidad 
                  AND COD_CONTABILIDAD = @contabilidad";

            int count = connection.Query<int>(
                CentroCostoExisteQuery,
                new
                {
                    unidad,
                    contabilidad = codContab
                }).FirstOrDefault();

            return count > 0;
        }

        private List<PreVistaPresupuestoCuentaData> ObtenerPresupuestoCuenta(
            SqlConnection connection,
            int codContab,
            string codModelo,
            string codUnidad,
            string centroCosto,
            string cuenta)
        {
            const string Procedure = "[spPres_VistaPresupuesto_Cuenta]";

            var values = new
            {
                COD_CONTA = codContab,
                COD_MODELO = codModelo,
                COD_UNIDAD = codUnidad,
                CENTRO_COSTO = centroCosto,
                CUENTA = cuenta,
                TIPO_VISTA = "C",
                Periodo = (string?)null
            };

            return connection
                .Query<PreVistaPresupuestoCuentaData>(Procedure, values, commandType: CommandType.StoredProcedure)
                .ToList();
        }

        #endregion
    }
}