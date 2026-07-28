using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo_API.DataBaseTier;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndPlanillaFondosDb
    {
        private readonly IConfiguration _config;

        private const string SpPlanillaDirectaSube = "spFndPlanillaDirecta_Sube";
        private const string SpPlanillaDirectaConsulta = "spFndPlanillaDirecta_Consulta";
        private const string SpPlanillaDirectaProcesa = "spFndPlanillaDirecta_Procesa";
        private const string SpPlanillaDirectaAsiento = "spFndPlanillaDirectaAsiento";

        private const string SqlInstituciones = @"
                    SELECT
                        cod_institucion AS item,
                        descripcion
                    FROM dbo.instituciones
                    WHERE activa = 1
                    ORDER BY descripcion;";

        private const string SqlOperadoras = @"
                    SELECT
                        cod_operadora AS item,
                        descripcion
                    FROM dbo.FND_Operadoras
                    ORDER BY descripcion;";

        private const string SqlPlanes = @"
                    SELECT
                        RTRIM(cod_plan) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM dbo.fnd_planes
                    WHERE deduce_independiente = 1
                      AND cod_operadora = @CodOperadora
                    ORDER BY descripcion;";

        private const string SqlComprobante = @"
                    SELECT dbo.fxFnd_Planillas_Comprobante(@CodInstitucion, @Proceso);";

        private const string SqlCuentaInstitucion = @"
                    SELECT cta_fondos
                    FROM dbo.instituciones
                    WHERE cod_institucion = @CodInstitucion;";

        private const string SqlCuentaPlan = @"
                    SELECT cuenta_gasto
                    FROM dbo.fnd_planes
                    WHERE cod_operadora = @CodOperadora
                      AND cod_plan = @CodPlan;";

        private const string SqlBitacoraConsecutivo = @"
                    SELECT ISNULL(MAX(id_seq), 0) + 1
                    FROM dbo.fnd_prm_bitacora
                    WHERE cod_institucion = @Institucion
                      AND cod_plan = @Plan
                      AND proceso = @Proceso;";

        private const string SqlBitacoraInsert = @"
                    INSERT INTO dbo.fnd_prm_bitacora
                    (
                        id_seq,
                        cod_institucion,
                        proceso,
                        cod_plan,
                        gestion,
                        transaccion,
                        documento,
                        usuario,
                        fecha,
                        casos,
                        monto
                    )
                    VALUES
                    (
                        @IdSeq,
                        @Institucion,
                        @Proceso,
                        @Plan,
                        @Gestion,
                        @Transaccion,
                        @Documento,
                        @Usuario,
                        GETDATE(),
                        @Casos,
                        @Monto
                    );";

        private const string SqlPlanillaAplicada = @"
                    SELECT ISNULL(COUNT(id_seq), 0)
                    FROM dbo.fnd_prm_bitacora
                    WHERE cod_institucion = @CodInstitucion
                      AND cod_plan = @CodPlan
                      AND proceso = @Proceso
                      AND documento = @Documento;";

        private const string SqlContratoActivo = @"
                    SELECT TOP 1 cod_contrato
                    FROM dbo.fnd_contratos
                    WHERE cedula = @Cedula
                      AND cod_operadora = @CodOperadora
                      AND cod_plan = @CodPlan
                      AND estado = 'A';";

        public FrmFndPlanillaFondosDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtener Instituciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaFondos_Instituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlInstituciones);
        }

        /// <summary>
        /// Obtener Operadoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaFondos_Operadoras_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlOperadoras);
        }

        /// <summary>
        /// Obtener Planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOperadora"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaFondos_Planes_Obtener(int CodEmpresa, int CodOperadora)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanes,
                new { CodOperadora });
        }

        /// <summary>
        /// Obtener Comprobante
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="Proceso"></param>
        /// <returns></returns>
        public ErrorDto<string> FND_PlanillaFondos_Comprobante_Obtener(int CodEmpresa, int CodInstitucion, int Proceso)
        {
            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlComprobante,
                string.Empty,
                new
                {
                    CodInstitucion,
                    Proceso
                });

            return new ErrorDto<string>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? string.Empty
            };
        }

        /// <summary>
        /// Obtener Cuentas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Tipo"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CodOperadora"></param>
        /// <param name="CodPlan"></param>
        /// <param name="CodConta"></param>
        /// <returns></returns>
        public ErrorDto<DropDownListaGenericaModel> FND_PlanillaFondos_Cuenta_Obtener(int CodEmpresa, string Tipo, int CodInstitucion, int CodOperadora, string CodPlan, int CodConta)
        {
            var tipo = NormalizarTexto(Tipo).ToUpperInvariant();
            if (tipo != "A" && tipo != "R")
            {
                return DbHelper.CreateErrorResponse(
                    "Tipo de proceso no identificado (solo se permite 'A' o 'R').",
                    -2,
                    new DropDownListaGenericaModel());
            }

            var cuentaResult = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                tipo == "A" ? SqlCuentaInstitucion : SqlCuentaPlan,
                string.Empty,
                new
                {
                    CodInstitucion,
                    CodOperadora,
                    CodPlan = NormalizarTexto(CodPlan)
                });

            if (cuentaResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    cuentaResult.Description ?? "Error al obtener la cuenta.",
                    cuentaResult.Code.GetValueOrDefault(-1),
                    new DropDownListaGenericaModel());
            }

            return DbHelper.CreateOkResponse(CrearCuentaModel(CodEmpresa, CodConta, cuentaResult.Result));
        }

        /// <summary>
        /// Obtener Rango de Procesos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Proceso"></param>
        /// <returns></returns>
        public ErrorDto<List<int>> FND_PlanillaFondos_Procesos_ObtenerRango(int CodEmpresa, int Proceso)
        {
            var mcobro = new MCobroDb(_config);
            var response = new ErrorDto<List<int>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<int>()
            };
            int vProceso = Proceso;

            // Retroceder 2 procesos
            for (int i = 0; i < 2; i++)
            {
                var res = mcobro.fxFechaProcesoAnterior(CodEmpresa, vProceso);
                vProceso = (int)res;
            }
            // Agregar el proceso dos pasos atrás
            response.Result.Add(vProceso);

            // Avanzar 6 procesos
            for (int i = 0; i < 6; i++)
            {
                var res = mcobro.fxFechaProcesoSiguiente(CodEmpresa, vProceso);
                vProceso = (int)res;
                response.Result.Add(vProceso);
            }

            return response;
        }

        /// <summary>
        /// Cargar deducciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FndPlanillaFondosData> FND_PlanillaFondos_Deducciones_Cargar(int CodEmpresa, CargarDeduccionesRequest request)
        {
            var response = DbHelper.CreateOkResponse(new FndPlanillaFondosData());

            if (request?.registros == null || request.registros.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No hay registros para procesar.",
                    -2,
                    new FndPlanillaFondosData());
            }

            if (FxAplicada(CodEmpresa, request.cod_institucion, request.plan ?? string.Empty, request.proceso, request.comprobante ?? string.Empty))
            {
                return DbHelper.CreateErrorResponse(
                    "Ya se aplico una planilla con esta fecha de proceso para la institucion y el plan elegidos",
                    -2,
                    new FndPlanillaFondosData());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                SubirDeducciones(connection, request);
                var detalles = ConsultarDeducciones(connection, request);
                return CrearResumenDeducciones(detalles);
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al cargar deducciones.",
                    result.Code.GetValueOrDefault(-1),
                    new FndPlanillaFondosData());
            }

            response.Result = result.Result ?? new FndPlanillaFondosData();
            return response;
        }

        /// <summary>
        /// Procesar deducciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<object> FND_PlanillaFondos_Procesar(int CodEmpresa, FndPlanillaDirectaProcesaDto request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<object>("Los datos del proceso son requeridos.", -2, null);
            }

            var vNumDoc = NormalizarTexto(request.comprobante);
            const string vTipoDoc = "PLA";

            var cuenta = new MCntLinkDB(_config).fxgCntCuentaFormato(CodEmpresa, true, request.cuenta, 0);
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    SpPlanillaDirectaProcesa,
                    new
                    {
                        Institucion = request.cod_institucion,
                        Operadora = request.cod_operadora,
                        Plan = NormalizarTexto(request.plan),
                        Proceso = request.proceso,
                        Documento = vNumDoc,
                        Usuario = NormalizarTexto(request.usuario),
                        Cuenta = cuenta,
                        Tipo = NormalizarTexto(request.tipo)
                    },
                    commandType: CommandType.StoredProcedure, commandTimeout: 0));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<object>(
                    result.Description ?? "Error al procesar deducciones.",
                    result.Code.GetValueOrDefault(-1),
                    null);
            }

            var recibo = new MRecibos(_config).sbImprimeRecibo(CodEmpresa, vNumDoc, vTipoDoc, request.usuario);
            if (recibo.Code != -1)
            {
                recibo.Description = "Proceso Aplicado Satisfactoriamente...";
            }

            return recibo;
        }

        /// <summary>
        /// Parámetros para registrar bitácora de planilla.
        /// </summary>
        public sealed class BitacoraPlanillaParams
        {
            public string Transaccion { get; init; } = string.Empty;
            public int Institucion { get; init; }
            public int Proceso { get; init; }
            public string Gestion { get; init; } = string.Empty;
            public decimal Monto { get; init; }
            public string Plan { get; init; } = string.Empty;
            public int Casos { get; init; }
            public string Usuario { get; init; } = string.Empty;
            public string Documento { get; init; } = string.Empty;
        }

        /// <summary>
        /// Parámetros para generar asiento de planilla.
        /// </summary>
        public sealed class PlanillaAsientoParams
        {
            public int CodInstitucion { get; init; }
            public int Proceso { get; init; }
            public int Operadora { get; init; }
            public string Plan { get; init; } = string.Empty;
            public string CuentaPlanilla { get; init; } = string.Empty;
            public string Comprobante { get; init; } = string.Empty;
            public string Usuario { get; init; } = string.Empty;
        }

        /// <summary>
        /// Agregar a la Bitacora Planilla
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto sbBitacoraPlanilla(int CodEmpresa, BitacoraPlanillaParams request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los parámetros de bitácora son requeridos.", -2);
            }
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                var consecutivo = connection.QueryFirstOrDefault<int>(SqlBitacoraConsecutivo, new
                {
                    Institucion = request.Institucion,
                    Plan = NormalizarTexto(request.Plan),
                    Proceso = request.Proceso
                });

                connection.Execute(SqlBitacoraInsert, new
                {
                    IdSeq = consecutivo,
                    Institucion = request.Institucion,
                    Proceso = request.Proceso,
                    Plan = NormalizarTexto(request.Plan),
                    Gestion = NormalizarTexto(request.Gestion),
                    Transaccion = NormalizarTexto(request.Transaccion),
                    Documento = NormalizarTexto(request.Documento),
                    Usuario = NormalizarTexto(request.Usuario),
                    Casos = request.Casos,
                    Monto = request.Monto
                });

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar bitácora de planilla.", result.Code.GetValueOrDefault(-1));
        }

        public bool FxAplicada(int CodEmpresa, int CodInstitucion, string CodPlan, int Proceso, string Documento)
        {
            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanillaAplicada,
                0,
                new
                {
                    CodInstitucion,
                    CodPlan = NormalizarTexto(CodPlan),
                    Proceso,
                    Documento = NormalizarTexto(Documento)
                });

            return result.Code == 0 && result.Result > 0;
        }

        public ErrorDto SbFndAsiento(int CodEmpresa, PlanillaAsientoParams request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los parámetros del asiento son requeridos.", -2);
            }
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    SpPlanillaDirectaAsiento,
                    new
                    {
                        Proceso = request.Proceso,
                        Institucion = request.CodInstitucion,
                        Operadora = request.Operadora,
                        Plan = NormalizarTexto(request.Plan),
                        CtaConta = NormalizarTexto(request.CuentaPlanilla),
                        Comprobante = NormalizarTexto(request.Comprobante),
                        Usuario = NormalizarTexto(request.Usuario)
                    },
                    commandType: CommandType.StoredProcedure));

            return result.Code == 0
                ? DbHelper.OkResponse("Asiento generado correctamente.")
                : DbHelper.ErrorResponse(result.Description ?? "Error al generar asiento.", result.Code.GetValueOrDefault(-1));
        }

        public bool FxExisteContrato(int CodEmpresa, string Cedula, int CodOperadora, string CodPlan)
        {
            var result = DbHelper.ExecuteSingleQuery<int?>(
                new PortalDB(_config),
                CodEmpresa,
                SqlContratoActivo,
                null,
                new
                {
                    Cedula = NormalizarTexto(Cedula),
                    CodOperadora,
                    CodPlan = NormalizarTexto(CodPlan)
                });

            return result.Code == 0 && result.Result.HasValue;
        }

        private DropDownListaGenericaModel CrearCuentaModel(int codEmpresa, int codConta, string? cuenta)
        {
            var cuentaNormalizada = NormalizarTexto(cuenta);
            if (string.IsNullOrWhiteSpace(cuentaNormalizada))
            {
                return new DropDownListaGenericaModel
                {
                    item = string.Empty,
                    descripcion = string.Empty
                };
            }

            var cntLinkDb = new MCntLinkDB(_config);
            return new DropDownListaGenericaModel
            {
                item = cntLinkDb.fxgCntCuentaFormato(codEmpresa, true, cuentaNormalizada, 0),
                descripcion = cntLinkDb.fxgCntCuentaDesc(codEmpresa, cuentaNormalizada, codConta) ?? string.Empty
            };
        }

        private static void SubirDeducciones(IDbConnection connection, CargarDeduccionesRequest request)
        {
            var linea = 0;
            foreach (var item in request.registros.Where(x => !string.IsNullOrWhiteSpace(x.cedula)))
            {
                linea++;
                connection.Execute(
                    SpPlanillaDirectaSube,
                    new
                    {
                        Institucion = request.cod_institucion,
                        Operadora = request.cod_operadora,
                        Plan = NormalizarTexto(request.plan),
                        Documento = NormalizarTexto(request.comprobante),
                        Proceso = request.proceso,
                        Cedula = NormalizarTexto(item.cedula),
                        Nombre = NormalizarTexto(item.nombre),
                        Fondos = item.fondos,
                        Linea = linea,
                        Inicializa = linea == 1 ? 1 : 0
                    },
                    commandType: CommandType.StoredProcedure);
            }
        }

        private static List<FndPlanillaFondosDetalleData> ConsultarDeducciones(IDbConnection connection, CargarDeduccionesRequest request)
        {
            return connection.Query<FndPlanillaFondosDetalleData>(
                SpPlanillaDirectaConsulta,
                new
                {
                    Operadora = request.cod_operadora,
                    Plan = NormalizarTexto(request.plan),
                    Documento = NormalizarTexto(request.comprobante),
                    Revisar = 1
                },
                commandType: CommandType.StoredProcedure).ToList();
        }

        private static FndPlanillaFondosData CrearResumenDeducciones(List<FndPlanillaFondosDetalleData> detalles)
        {
            return new FndPlanillaFondosData
            {
                detalles = detalles,
                total_socios = detalles.Count(x => x.existe_persona),
                total_contratos = detalles.Count(x => x.existe_contrato),
                total_casos = detalles.Count,
                monto_total = detalles.Sum(x => x.fondos)
            };
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
