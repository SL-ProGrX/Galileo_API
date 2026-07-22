using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndCalculoRendimientoDb
    {
        private readonly IConfiguration _config;
        private readonly MProGrxMain _mProGrxMain;
        private const string SpAjusteTasaContratosVencidos = "dbo.spFndAjusteTasaCntVencidos";
        private const string SpGenerarRendimientoPlanSql = @"
            EXEC dbo.spFndRndGenPlanMain
                @Operadora,
                @Plan,
                @Usuario,
                @Oficina,
                @Corte,
                @Tasa,
                @Modo,
                @AppProductName,
                @TCP";

        public FrmFndCalculoRendimientoDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene operadoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Operadoras_Lista(int CodEmpresa)
        {
            const string query = @"
                  SELECT
                      descripcion AS descripcion,
                      cod_operadora AS item
                  FROM dbo.FND_Operadoras
                  ORDER BY descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                query);
        }

    /// <summary>
    /// Planes obtener
    /// </summary>
    /// <param name="CodEmpresa"></param>
    /// <param name="CodOperadora"></param>
    /// <param name="CodPlan"></param>
    /// <returns></returns>
        public ErrorDto<FndPlanDatosDto> Plan_Obtener(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            const string query = @"
                  SELECT
                      cod_plan,
                      Descripcion,
                      rend_corte,
                      ISNULL(UltTasa, Tasa_Base) AS UltTasa,
                      Tasa_Base,
                      UTILIZA_TASA_FLUCTUANTE,
                      UTILIZA_TBP,
                      dbo.fxFndTasaReferencia('TBP') AS tbp,
                      dbo.fxFndTasaReferencia('TCP') AS tcp
                  FROM dbo.Fnd_Planes
                  WHERE Cod_Operadora = @CodOperadora
                    AND Cod_Plan = @CodPlan;";

            var result = DbHelper.ExecuteSingleQuery<FndPlanDatosDto>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                default,
                new
                {
                    CodOperadora,
                    CodPlan = NormalizarTexto(CodPlan)
                });

            return new ErrorDto<FndPlanDatosDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? CrearPlanDatosVacio()
            };
        }

        private static FndPlanDatosDto CrearPlanDatosVacio()
        {
            return new FndPlanDatosDto
            {
                cod_plan = string.Empty,
                descripcion = string.Empty,
                rend_corte = DateTime.MinValue,
                ult_tasa = 0,
                tasa_base = 0
            };
        }

        /// <summary>
        /// Obtiene planes scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOperadora"></param>
        /// <param name="CodPlan"></param>
        /// <param name="ScrollCode"></param>
        /// <returns></returns>
        public ErrorDto<FndPlanDatosDto> Plan_Scroll(int CodEmpresa, int CodOperadora, string? CodPlan, int ScrollCode)
        {
            const string query = @"
                  SELECT LTRIM(RTRIM(cod_plan))
                  FROM dbo.vFnd_Planes
                  WHERE cod_operadora = @CodOperadora
                    AND calcula_rend = 1
                  ORDER BY cod_plan;";

            var planesResult = DbHelper.ExecuteListQuery<string>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new { CodOperadora });

            if (planesResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    planesResult.Description ?? "Error al obtener planes.",
                    planesResult.Code ?? -1,
                    CrearPlanDatosVacio());
            }

            var planes = planesResult.Result?
                .Select(x => x.Trim())
                .ToList() ?? new List<string>();

            if (planes.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No hay planes configurados.",
                    -1,
                    CrearPlanDatosVacio());
            }

            var siguientePlan = ObtenerPlanScroll(planes, CodPlan, ScrollCode);
            if (string.IsNullOrWhiteSpace(siguientePlan))
            {
                return DbHelper.CreateErrorResponse(
                    "No hay más registros.",
                    -1,
                    CrearPlanDatosVacio());
            }

            return Plan_Obtener(CodEmpresa, CodOperadora, siguientePlan);
        }

        public ErrorDto<FndRendimientoResultadoDto> AplicarRendimientos(int CodEmpresa, FndRendimientoRequestDto dto)
        {
            if (dto is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos de rendimiento son requeridos.",
                    -2,
                    new FndRendimientoResultadoDto());
            }

            dto.oficina = ObtenerOficinaRendimiento(CodEmpresa, dto);
            if (string.IsNullOrWhiteSpace(dto.oficina))
            {
                return DbHelper.CreateErrorResponse(
                    "No se pudo obtener la oficina titular del usuario.",
                    -2,
                    new FndRendimientoResultadoDto());
            }
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                EjecutarAjusteTasa(connection, dto);

                var inicial = EjecutarProcesoRendimiento(connection, dto, 1);
                ProcesarPendientes(connection, dto, inicial.pendientes);

                return EjecutarProcesoRendimiento(connection, dto, 3);
            });

            return new ErrorDto<FndRendimientoResultadoDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndRendimientoResultadoDto()
            };
        }

        private string ObtenerOficinaRendimiento(int CodEmpresa, FndRendimientoRequestDto dto)
        {
            var oficina = NormalizarTexto(dto.oficina);
            if (!string.IsNullOrWhiteSpace(oficina))
            {
                return oficina;
            }

            var oficinas = _mProGrxMain.CargaOficinas(dto.usuario, CodEmpresa);
            return NormalizarTexto(oficinas?.FirstOrDefault()?.Titular) ?? string.Empty;
        }
        /// <summary>
        /// Obtener lista historial rendimiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOperadora"></param>
        /// <param name="CodPlan"></param>
        /// <returns></returns>
        public ErrorDto<List<FndHistorialRendDto>> HistorialRend_Lista(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            const string query = @"
                      SELECT TOP 24
                          corte,
                          tasa,
                          tcp,
                          usuario,
                          fecha_sys
                      FROM dbo.FND_HISTORIAL_REND
                      WHERE cod_operadora = @CodOperadora
                        AND cod_plan = @CodPlan
                      ORDER BY idx DESC;";

            return DbHelper.ExecuteListQuery<FndHistorialRendDto>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new
                {
                    CodOperadora,
                    CodPlan = NormalizarTexto(CodPlan)
                });
        }

        /// <summary>
        /// Obtener planes lista
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOperadora"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Planes_Lista(int CodEmpresa, int CodOperadora)
        {
            const string query = @"
                SELECT
                    cod_plan AS item,
                    descripcion
                FROM dbo.vFnd_Planes
                WHERE cod_operadora = @CodOperadora
                  AND calcula_rend = 1
                ORDER BY cod_plan;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new { CodOperadora });
        }


        /// <summary>
        /// Obtiene la fecha del servidor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            const string query = "SELECT dbo.MyGetdate() AS Fecha;";

            var result = DbHelper.ExecuteSingleQuery<DateTime>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                DateTime.MinValue);

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al obtener fecha servidor.");
            }

            return DbHelper.OkResponse(result.Result.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        private static string? ObtenerPlanScroll(List<string> planes, string? codPlan, int scrollCode)
        {
            var planActual = NormalizarTexto(codPlan);
            var index = planes.IndexOf(planActual);

            if (index == -1)
            {
                index = planes.FindIndex(x => x.StartsWith(planActual, StringComparison.OrdinalIgnoreCase));
            }

            if (index == -1)
            {
                index = 0;
            }

            return scrollCode == 1
                ? ObtenerPlanSiguiente(planes, index)
                : ObtenerPlanAnterior(planes, index);
        }

        private static string? ObtenerPlanSiguiente(List<string> planes, int index)
        {
            return index < planes.Count - 1
                ? planes[index + 1]
                : null;
        }

        private static string? ObtenerPlanAnterior(List<string> planes, int index)
        {
            return index > 0
                ? planes[index - 1]
                : null;
        }

        private static void EjecutarAjusteTasa(IDbConnection connection, FndRendimientoRequestDto dto)
        {
            connection.Execute(
                SpAjusteTasaContratosVencidos,
                new
                {
                    Operadora = dto.operadora,
                    Plan = NormalizarTexto(dto.plan)
                },
                commandType: CommandType.StoredProcedure);
        }

        private static DynamicParameters CrearParametrosRendimiento(FndRendimientoRequestDto dto, int modo)
        {
            var parametros = new DynamicParameters();

            parametros.Add("@Operadora", dto.operadora, DbType.Int32);
            parametros.Add("@Plan", NormalizarTexto(dto.plan), DbType.String);
            parametros.Add("@Usuario", NormalizarTexto(dto.usuario), DbType.String);
            parametros.Add("@Oficina", NormalizarTexto(dto.oficina), DbType.String);
            parametros.Add("@Corte", dto.fecha_corte, DbType.DateTime);
            parametros.Add("@Tasa", dto.tasa, DbType.Decimal);
            parametros.Add("@TCP", dto.tcp, DbType.Decimal);
            parametros.Add("@AppProductName", dto.aplicacion, DbType.String);
            parametros.Add("@Modo", modo, DbType.Int32);

            return parametros;
        }

        private static void ProcesarPendientes(IDbConnection connection, FndRendimientoRequestDto dto, int pendientes)
        {
            while (pendientes > 0)
            {
                var step = EjecutarProcesoRendimiento(connection, dto, 2);
                pendientes = step.pendientes;
            }
        }

        private static FndRendimientoResultadoDto EjecutarProcesoRendimiento(
            IDbConnection connection,
            FndRendimientoRequestDto dto,
            int modo)
        {
            return connection.QueryFirst<FndRendimientoResultadoDto>(
                SpGenerarRendimientoPlanSql,
                CrearParametrosRendimiento(dto, modo));
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
