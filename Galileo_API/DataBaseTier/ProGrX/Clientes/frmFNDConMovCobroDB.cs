using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmFndConMovCobroDb
    {
        private readonly IConfiguration _config;

        private const string PlanTodos = "T";
        private const string SpAcreditaMovCbrPendiente = "spFnd_AcreditaMovCbrPendiente";

        private const string SqlPlanesLista = @"
                    SELECT cod_plan AS item,
                           Descripcion AS descripcion
                    FROM dbo.fnd_planes;";

        private const string SqlMovimientosCobro = @"
                    SELECT P.codigo AS Codigo,
                           P.id_solicitud AS Id_Solicitud,
                           P.Principal,
                           P.fecha AS Fecha,
                           P.Proceso,
                           I.descripcion AS InstitucionX,
                           S.Nombre,
                           C.CTANAMORT,
                           C.CTAOAMORT,
                           Cnt.cedula AS Cedula,
                           Cnt.cod_plan AS Cod_Plan,
                           Cnt.cod_operadora AS Cod_Operadora,
                           Cnt.cod_contrato AS Cod_Contrato,
                           Cnt.Estado,
                           P.tcon AS Tcon,
                           P.ncon AS Ncon
                    FROM dbo.vCRDsReportesMov P
                    INNER JOIN dbo.fnd_contratos Cnt
                        ON P.id_solicitud = Cnt.operacion
                    INNER JOIN dbo.socios S
                        ON Cnt.cedula = S.cedula
                    INNER JOIN dbo.instituciones I
                        ON S.cod_institucion = I.cod_Institucion
                    INNER JOIN dbo.catalogo C
                        ON P.codigo = C.codigo
                    WHERE P.tcon IN ('1','PRM','PLA')
                      AND P.fecha BETWEEN @FechaInicio AND @FechaFin
                      AND dbo.fxFnd_MovimientoExiste(Cnt.cod_operadora, Cnt.cod_Plan, Cnt.cod_Contrato, P.Tcon, P.Ncon) = 0
                      AND (@FiltraPlan = 0 OR Cnt.cod_plan = @CodPlan);";

        private const string SqlMovimientosCobroSinContrato = @"
                    SELECT P.codigo AS Codigo,
                           P.id_solicitud AS Id_Solicitud,
                           P.Principal,
                           R.opex AS Opex,
                           P.fecha AS Fecha,
                           P.Proceso,
                           I.descripcion AS InstitucionX,
                           S.Nombre,
                           C.CTANAMORT,
                           C.CTAOAMORT,
                           R.cedula AS Cedula,
                           F.cod_plan AS Cod_Plan,
                           F.cod_operadora AS Cod_Operadora,
                           P.tcon AS Tcon,
                           P.ncon AS Ncon
                    FROM dbo.vCRDsReportesMov P
                    LEFT JOIN dbo.reg_creditos R
                        ON P.id_solicitud = R.id_solicitud
                    INNER JOIN dbo.socios S
                        ON R.cedula = S.cedula
                    INNER JOIN dbo.instituciones I
                        ON S.cod_institucion = I.cod_Institucion
                    INNER JOIN dbo.catalogo C
                        ON R.codigo = C.codigo
                    INNER JOIN dbo.fnd_planes F
                        ON P.codigo = F.codigo_ase
                    LEFT JOIN dbo.fnd_contratos Cnt
                        ON F.cod_Operadora = Cnt.Cod_Operadora
                       AND F.cod_plan = Cnt.Cod_Plan
                       AND P.id_solicitud = Cnt.Operacion
                    WHERE P.tcon IN ('1','PRM','PLA')
                      AND P.fecha BETWEEN @FechaInicio AND @FechaFin
                      AND Cnt.cod_contrato IS NULL
                      AND (@FiltraPlan = 0 OR F.cod_plan = @CodPlan);";

        private const string SqlEntradaPlanillaResumen = @"
                    SELECT ISNULL(SUM(Principal), 0) AS Monto,
                           COUNT(*) AS Casos
                    FROM dbo.vCRDsReportesMov
                    WHERE tcon IN ('1','PLA')
                      AND fecha BETWEEN @FechaInicio AND @FechaFin
                      AND codigo IN
                      (
                          SELECT codigo_ase
                          FROM dbo.fnd_planes
                          WHERE cod_plan = @CodPlan
                      );";

        private const string SqlPlanillaRegistradaResumen = @"
                    SELECT ISNULL(SUM(monto), 0) AS Monto,
                           COUNT(*) AS Casos
                    FROM dbo.fnd_contratos_Detalle
                    WHERE tcon IN ('1','PLA')
                      AND fecha BETWEEN @FechaInicio AND @FechaFin
                      AND cod_plan = @CodPlan;";

        public FrmFndConMovCobroDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de planes para combos genéricos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de planes.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Planes_Lista_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanesLista);
        }

        /// <summary>
        /// Obtiene los movimientos de cobro según filtros de fechas y plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Parámetros de filtro.</param>
        /// <returns>Listado de movimientos de cobro.</returns>
        public ErrorDto<List<FndConMovCobroResult>> Fnd_ConMovCobro_Obtener(int CodEmpresa, FndConMovCobroRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de movimientos de cobro son requeridos.",
                    -2,
                    new List<FndConMovCobroResult>());
            }

            return DbHelper.ExecuteListQuery<FndConMovCobroResult>(
                CreatePortalDb(),
                CodEmpresa,
                SqlMovimientosCobro,
                CrearParametrosMovimientos(request));
        }

        /// <summary>
        /// Obtiene los movimientos de cobro sin contrato según filtros de fechas y plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Parámetros de filtro.</param>
        /// <returns>Listado de movimientos sin contrato.</returns>
        public ErrorDto<List<FndConMovCobroResult>> Fnd_ConMovCobro_SinContrato_Obtener(int CodEmpresa, FndConMovCobroRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de movimientos de cobro son requeridos.",
                    -2,
                    new List<FndConMovCobroResult>());
            }

            return DbHelper.ExecuteListQuery<FndConMovCobroResult>(
                CreatePortalDb(),
                CodEmpresa,
                SqlMovimientosCobroSinContrato,
                CrearParametrosMovimientos(request));
        }

        /// <summary>
        /// Ejecuta el SP spFnd_AcreditaMovCbrPendiente para acreditar movimientos cobrados no reportados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Parámetros requeridos para la acreditación.</param>
        /// <returns>Resultado de la acreditación.</returns>
        public ErrorDto<bool> Fnd_AcreditaMovCbrPendiente(int CodEmpresa, FndAcreditaMovCbrPendienteRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse("Los datos de acreditación son requeridos.", -2, false);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    SpAcreditaMovCbrPendiente,
                    CrearParametrosAcreditacion(request),
                    commandType: System.Data.CommandType.StoredProcedure);

                return true;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al acreditar movimiento pendiente.", result.Code.GetValueOrDefault(-1), false);
        }

        /// <summary>
        /// Obtiene el resumen de movimientos de vCRDsReportesMov para un plan y rango de fechas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Parámetros de filtro.</param>
        /// <returns>Resumen de entrada de planilla.</returns>
        public ErrorDto<FndConMovCobroResumenResult?> Fnd_ConMovCobro_EntradaPlanilla_Obtener(int CodEmpresa, FndConMovCobroResumenRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<FndConMovCobroResumenResult?>("Los filtros de resumen son requeridos.", -2, null);
            }

            return DbHelper.ExecuteSingleQuery<FndConMovCobroResumenResult>(
                CreatePortalDb(),
                CodEmpresa,
                SqlEntradaPlanillaResumen,
                new FndConMovCobroResumenResult(),
                CrearParametrosResumen(request));
        }

        /// <summary>
        /// Obtiene el resumen de movimientos de fnd_contratos_Detalle para un plan y rango de fechas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Parámetros de filtro.</param>
        /// <returns>Resumen de planilla registrada.</returns>
        public ErrorDto<FndConMovCobroResumenResult?> Fnd_ConMovCobro_PlanillaRegistrada_Obtener(int CodEmpresa, FndConMovCobroResumenRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<FndConMovCobroResumenResult?>("Los filtros de resumen son requeridos.", -2, null);
            }

            return DbHelper.ExecuteSingleQuery<FndConMovCobroResumenResult>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanillaRegistradaResumen,
                new FndConMovCobroResumenResult(),
                CrearParametrosResumen(request));
        }

        /// <summary>
        /// Crea parámetros seguros para las consultas de movimientos.
        /// </summary>
        private static object CrearParametrosMovimientos(FndConMovCobroRequest request)
        {
            var codPlan = NormalizarTexto(request.CodPlan);
            var filtraPlan = !string.IsNullOrWhiteSpace(codPlan)
                && !string.Equals(codPlan, PlanTodos, StringComparison.OrdinalIgnoreCase);
            var fechaInicio = request.FechaInicio?.Date ?? DateTime.Today;
            var fechaFin = (request.FechaFin ?? request.FechaInicio ?? fechaInicio).Date.AddDays(1).AddSeconds(-1);

            return new
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                CodPlan = codPlan,
                FiltraPlan = filtraPlan ? 1 : 0
            };
        }

        /// <summary>
        /// Crea parámetros seguros para acreditar movimientos pendientes.
        /// </summary>
        private static object CrearParametrosAcreditacion(FndAcreditaMovCbrPendienteRequest request)
        {
            return new
            {
                request.Operacion,
                Usuario = NormalizarTexto(request.Usuario),
                request.Accion,
                TipoDoc = NormalizarTexto(request.TipoDoc),
                NumDoc = NormalizarTexto(request.NumDoc),
                request.Monto
            };
        }

        /// <summary>
        /// Crea parámetros seguros para consultar resúmenes.
        /// </summary>
        private static object CrearParametrosResumen(FndConMovCobroResumenRequest request)
        {
            var fechaInicio = request.FechaInicio?.Date ?? DateTime.Today;
            var fechaFin = (request.FechaFin ?? request.FechaInicio ?? fechaInicio).Date.AddDays(1).AddSeconds(-1);

            return new
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                CodPlan = NormalizarTexto(request.CodPlan)
            };
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}