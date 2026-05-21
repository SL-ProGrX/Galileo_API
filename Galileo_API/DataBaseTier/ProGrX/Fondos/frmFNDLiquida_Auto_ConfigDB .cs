using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using System.Globalization;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndLiquidaAutoConfigDb
    {
        private readonly IConfiguration _config;

        public FrmFndLiquidaAutoConfigDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private const string SpPlanesGuardar = "spFnd_LiqAuto_Planes_Add";
        private const string SpPlanesPatronalGuardar = "spFnd_LiqAuto_Planes_PAT_Add";

        private const string SqlParametroGuardar = @"
                UPDATE dbo.FND_LIQUIDACION_AUTOMATICA_PARAMETROS
                   SET UsuarioActualiza = @Usuario,
                       FechaActualiza = dbo.MyGetdate(),
                       Valor = @Valor
                 WHERE IdRegistro = @IdRegistro;";

        private const string SqlParametros = @"
                SELECT
                    IdRegistro       AS idregistro,
                    Descripcion      AS descripcion,
                    Valor            AS valor,
                    TipoDato         AS tipodato,
                    UsuarioActualiza AS usuarioactualiza,
                    FechaActualiza   AS fechaactualiza
                FROM dbo.FND_LIQUIDACION_AUTOMATICA_PARAMETROS
                ORDER BY IdRegistro;";

        private const string SqlPlanes = @"
                SELECT
                    C.IdRegistro          AS idregistro,
                    C.Operadora           AS operadora,
                    C.CodPlan             AS cod_plan,
                    P.Descripcion         AS descripcion,
                    C.ComponentePatronal  AS patrimonio,
                    C.FechaRegistro       AS fecha,
                    C.UsuarioRegistro     AS usuario
                FROM dbo.FND_LIQUIDACION_AUTOMATICA_PLANES C
                INNER JOIN dbo.FND_PLANES P
                    ON C.Operadora = P.COD_OPERADORA
                   AND C.CodPlan   = P.COD_PLAN
                ORDER BY C.IdRegistro;";

        private const string SqlPlanesPatronal = @"
                SELECT
                    C.IdRegistro          AS idregistro,
                    C.Operadora           AS operadora,
                    C.CodPlan             AS cod_plan,
                    P.Descripcion         AS descripcion,
                    C.ComponentePatronal  AS patrimonio,
                    C.FechaRegistro       AS fecha,
                    C.UsuarioRegistro     AS usuario
                FROM dbo.FND_LIQUIDACION_AUTOMATICA_PLANES C
                INNER JOIN dbo.FND_PLANES P
                    ON C.Operadora = P.COD_OPERADORA
                   AND C.CodPlan   = P.COD_PLAN
                WHERE C.ComponentePatronal = 1
                ORDER BY C.IdRegistro;";

        private const string SqlReportes = @"
                SELECT
                    C.CodPlan          AS cod_plan,
                    P.Descripcion      AS descripcion,
                    C.CantidadClientes AS cantidad,
                    C.SaldoTotal       AS saldo,
                    C.FechaInserta     AS fecha,
                    C.UsuarioInserta   AS usuario,
                    CONVERT(varchar(4), C.Anio) + FORMAT(C.Mes, '00') AS proceso
                FROM dbo.FND_LIQUIDACION_AUTOMATICA_RESUMEN C
                INNER JOIN dbo.FND_PLANES P
                    ON C.CodPlan = P.COD_PLAN
                WHERE C.Anio = @Anio
                  AND C.Mes  = @Mes
                ORDER BY C.Id;";

        private const string SqlOperadoras = @"
                SELECT
                    RTRIM(cod_Operadora) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM dbo.FND_Operadoras
                ORDER BY descripcion;";

        private const string SqlProcesos = @"
                SELECT
                    CONVERT(varchar(4), Anio) + FORMAT(Mes, '00') AS item,
                    CONVERT(varchar(4), Anio) + FORMAT(Mes, '00') AS descripcion
                FROM dbo.FND_LIQUIDACION_AUTOMATICA_RESUMEN
                GROUP BY Anio, Mes
                ORDER BY Anio DESC, Mes DESC;";

        private const string SqlPlanesReporte = @"
                SELECT DISTINCT
                    C.CodPlan AS item,
                    P.Descripcion AS descripcion
                FROM dbo.FND_LIQUIDACION_AUTOMATICA_PLANES C
                INNER JOIN dbo.FND_PLANES P
                    ON C.Operadora = P.COD_OPERADORA
                   AND C.CodPlan = P.COD_PLAN
                ORDER BY P.Descripcion;";

        private const string SqlPlanesBuscar = @"
                SELECT
                    COD_PLAN AS item,
                    DESCRIPCION AS descripcion
                FROM dbo.FND_PLANES
                WHERE COD_OPERADORA = @Operadora
                ORDER BY COD_PLAN;";

        /// <summary>
        /// Obtiene los parametroas de liquidacion automatica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FndLiqAutoParametroDto>> Parametros_Lista(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<FndLiqAutoParametroDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlParametros);
        }

        /// <summary>
        /// Obtiene la lista de planes configurados para liquidacion automatica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FndLiqAutoPlanesDto>> Planes_Lista(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<FndLiqAutoPlanesDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanes);
        }
        /// <summary>
        /// Obtiene los planes patronales configurados para liquidacion automatica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FndLiqAutoPlanesPatronalDto>> LiqAuto_Planes_Patronal_Lista(int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<FndLiqAutoPlanesPatronalDto>(
                new PortalDB(_config),
                codEmpresa,
                SqlPlanesPatronal);
        }

        /// <summary>
        /// Obtiene los reportes de liquidacion automatica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public ErrorDto<List<FndLiqAutoReporteDto>> LiqAuto_Reportes_Lista(int codEmpresa, int anio, int mes)
        {
            return DbHelper.ExecuteListQuery<FndLiqAutoReporteDto>(
                new PortalDB(_config),
                codEmpresa,
                SqlReportes,
                new { Anio = anio, Mes = mes });
        }

        /// <summary>
        /// Obtiene la lista de operadoras
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Operadoras_Lista(int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                codEmpresa,
                SqlOperadoras);
        }
        /// <summary>
        /// Obtiene las fecha Procesos de liquidacion automatica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Procesos_Lista(int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                codEmpresa,
                SqlProcesos);
        }
        /// <summary>
        /// Obtiene los planes para el reporte de liquidacion automatica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PlanesReporte_Lista(int codEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                codEmpresa,
                SqlPlanesReporte);

            if (result.Code == 0)
            {
                result.Result ??= new List<DropDownListaGenericaModel>();
                result.Result.Insert(0, new DropDownListaGenericaModel
                {
                    item = "TODOS",
                    descripcion = "TODOS"
                });
            }

            return result;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PlanesBuscar_Lista(int codEmpresa, string operadora)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                codEmpresa,
                SqlPlanesBuscar,
                new { Operadora = operadora });
        }

        /// <summary>
        /// Guarda o elimina un plan de liquidacion automatica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<bool> Planes_Guardar(int CodEmpresa, FndLiqAutoPlanesAddRequestDto dto)
        {
            if (dto is null)
            {
                return DbHelper.CreateErrorResponse("Los datos del plan son requeridos.", -2, false);
            }

            var procedimiento = dto.patrimonio ? SpPlanesPatronalGuardar : SpPlanesGuardar;

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    procedimiento,
                    CrearParametrosPlan(dto),
                    commandType: System.Data.CommandType.StoredProcedure));

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al guardar plan de liquidación automática.", result.Code.GetValueOrDefault(-1), false);
        }

        public ErrorDto<bool> Parametros_Guardar(int CodEmpresa, FndLiqAutoParametroGuardarRequestDto dto)
        {
            if (dto is null)
            {
                return DbHelper.CreateErrorResponse("Los datos del parámetro son requeridos.", -2, false);
            }

            var validacion = NormalizarValorParametro(dto.valor, dto.tipodato);
            if (!validacion.Valido)
            {
                return DbHelper.CreateErrorResponse(validacion.Mensaje, -2, false);
            }

            var result = DbHelper.ExecuteNonQueryWithResult(
                new PortalDB(_config),
                CodEmpresa,
                SqlParametroGuardar,
                new
                {
                    IdRegistro = dto.idregistro,
                    Valor = validacion.Valor,
                    Usuario = NormalizarTexto(dto.usuario)
                });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al guardar parámetro de liquidación automática.", result.Code.GetValueOrDefault(-1), false);
            }

            return result.Result > 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse("No se encontró el parámetro indicado.", -2, false);
        }

        private static object CrearParametrosPlan(FndLiqAutoPlanesAddRequestDto dto)
        {
            return new
            {
                Operadora = dto.operadora,
                Plan = NormalizarTexto(dto.cod_plan),
                I_Patronal = dto.patrimonio ? 1 : 0,
                Usuario = NormalizarTexto(dto.usuario),
                Mov = NormalizarTexto(dto.accion)
            };
        }

        private static (bool Valido, string Valor, string Mensaje) NormalizarValorParametro(string? valor, string? tipo)
        {
            var valorNormalizado = NormalizarTexto(valor);
            var tipoNormalizado = NormalizarTexto(tipo).ToUpperInvariant();
            tipoNormalizado = tipoNormalizado.Length > 3 ? tipoNormalizado[..3] : tipoNormalizado;

            switch (tipoNormalizado)
            {
                case "DEC":
                case "INT":
                case "POR":
                    return decimal.TryParse(valorNormalizado, NumberStyles.Number, CultureInfo.CurrentCulture, out var decimalValor)
                        || decimal.TryParse(valorNormalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out decimalValor)
                            ? (true, decimalValor.ToString(CultureInfo.InvariantCulture), string.Empty)
                            : (false, valorNormalizado, "El valor indicado no es válido.");

                case "NUM":
                    return long.TryParse(valorNormalizado, NumberStyles.Integer, CultureInfo.CurrentCulture, out var enteroValor)
                        || long.TryParse(valorNormalizado, NumberStyles.Integer, CultureInfo.InvariantCulture, out enteroValor)
                            ? (true, enteroValor.ToString(CultureInfo.InvariantCulture), string.Empty)
                            : (false, valorNormalizado, "El valor indicado no es válido.");

                case "CHR":
                    return valorNormalizado.Contains('\'')
                        ? (false, valorNormalizado, "El valor indicado contiene caracteres no válidos.")
                        : (true, valorNormalizado, string.Empty);

                case "PSN":
                    var opcion = valorNormalizado.Length > 0 ? valorNormalizado[..1].ToUpperInvariant() : string.Empty;
                    return opcion is "S" or "N"
                        ? (true, opcion, string.Empty)
                        : (false, valorNormalizado, "El valor indicado no es válido. Indique S o N.");

                case "DTS":
                    return DateTime.TryParse(valorNormalizado, CultureInfo.CurrentCulture, DateTimeStyles.None, out var fecha)
                        || DateTime.TryParse(valorNormalizado, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha)
                            ? (true, fecha.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture), string.Empty)
                            : (false, valorNormalizado, "La fecha indicada no es válida.");

                default:
                    return (true, valorNormalizado, string.Empty);
            }
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
