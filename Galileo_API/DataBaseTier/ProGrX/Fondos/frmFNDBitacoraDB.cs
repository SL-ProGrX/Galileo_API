using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Fondos;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndBitacoraDb
    {
        private const int ModuloFondos = 18;

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;

        public FrmFndBitacoraDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene las operadoras disponibles para la bitácora de fondos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Operadoras_Obtener(int codEmpresa)
        {
            const string sql = @"
SELECT
    CAST(cod_operadora AS varchar(20)) AS item,
    RTRIM(descripcion) AS descripcion
FROM dbo.FND_Operadoras
ORDER BY descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene los movimientos de bitácora especial para el módulo de Fondos.
        /// </summary>
        public ErrorDto<List<FrmFndBitacoraMovimientoDto>> Fnd_Movimientos_Obtener(int codEmpresa)
        {
            const string sql = @"
SELECT
    RTRIM(MOVIMIENTO) AS movimiento,
    RTRIM(DESCRIPCION) AS descripcion
FROM dbo.US_MOVIMIENTOS_BE
WHERE MODULO = @Modulo
ORDER BY MOVIMIENTO;";

            return DbHelper.ExecuteListQuery<FrmFndBitacoraMovimientoDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Modulo = ModuloFondos });
        }

        /// <summary>
        /// Obtiene los cambios de la bitácora especial de fondos según los filtros solicitados.
        /// </summary>
        public ErrorDto<List<FrmFndBitacoraCambiosDto>> Fnd_Bitacora_Cambios_Obtener(
            int codEmpresa,
            FrmFndBitacoraCambiosRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<List<FrmFndBitacoraCambiosDto>>(
                    "Los filtros de la bitácora son requeridos.",
                    -1,
                    new List<FrmFndBitacoraCambiosDto>());
            }

            var movimientos = NormalizarMovimientos(request.Movimientos);
            if (movimientos.Count == 0)
            {
                return DbHelper.CreateOkResponse(new List<FrmFndBitacoraCambiosDto>());
            }

            var response = DbHelper.CreateOkResponse(new List<FrmFndBitacoraCambiosDto>());

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var sql = ConstruirSqlCambios(request);
                var parameters = ConstruirParametrosCambios(request, movimientos);

                var result = connection.Query<FrmFndBitacoraCambiosDto>(sql, parameters).ToList();
                response.Result = NormalizarResultado(result);

                return response;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmFndBitacoraCambiosDto>>(
                    ex.Message,
                    -1,
                    new List<FrmFndBitacoraCambiosDto>());
            }
        }

        /// <summary>
        /// Marca como revisado un registro de la bitácora especial.
        /// </summary>
        public ErrorDto<bool> Fnd_Bitacora_Cambio_Revisar(
            int codEmpresa,
            FrmFndBitacoraCambioRevisarRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse("La solicitud de revisión es requerida.", -1, false);
            }

            if (request.id_Bitacora <= 0)
            {
                return DbHelper.CreateErrorResponse("El id_Bitacora no es válido.", -1, false);
            }

            var usuario = (request.revisado_Usuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse("El usuario que revisa es requerido.", -1, false);
            }

            const string sql = @"
UPDATE dbo.fnd_contratos_cambios
SET revisado_usuario = @RevisadoUsuario,
    revisado_fecha = dbo.MyGetDate()
WHERE id_bitacora = @IdBitacora
  AND revisado_fecha IS NULL;";

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var rows = connection.Execute(sql, new
                {
                    IdBitacora = request.id_Bitacora,
                    RevisadoUsuario = usuario
                });

                if (rows <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "El registro no existe o ya fue revisado previamente.",
                        -1,
                        false);
                }

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, false);
            }
        }

        /// <summary>
        /// Registra tags reutilizando la implementación compartida de mProGrx_Main.
        /// </summary>
        public ErrorDto<bool> Sif_RegistraTags(
            int codEmpresa,
            FrmFndBitacoraSifRegistraTagsRequest request)
        {
            _ = codEmpresa;

            if (request == null)
            {
                return DbHelper.CreateErrorResponse("La solicitud de tags es requerida.", -1, false);
            }

            var reusableRequest = new SifRegistraTagsRequestDto
            {
                Codigo = (request.codigo ?? string.Empty).Trim(),
                Tag = (request.tag ?? string.Empty).Trim(),
                Usuario = (request.usuario ?? string.Empty).Trim(),
                Observacion = (request.observacion ?? string.Empty).Trim(),
                Documento = (request.documento ?? string.Empty).Trim(),
                Modulo = (request.modulo ?? string.Empty).Trim(),
                Llave_01 = (request.llave_01 ?? string.Empty).Trim(),
                Llave_02 = (request.llave_02 ?? string.Empty).Trim(),
                Llave_03 = (request.llave_03 ?? string.Empty).Trim()
            };

            var result = _mProGrxMain.SbSIFRegistraTags(reusableRequest);

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse(result.Description ?? "No fue posible registrar el tag.", result.Code ?? -1, false);
        }

        private static string ConstruirSqlCambios(FrmFndBitacoraCambiosRequest request)
        {
            var fechaCampo = request.BuscarUsuarioFechaRevision ? "C.revisado_fecha" : "C.fecha";
            var usuarioCampo = request.BuscarUsuarioFechaRevision ? "C.revisado_usuario" : "C.usuario";

            var sql = new StringBuilder(@"
SELECT
    C.id_bitacora AS id_Bitacora,
    C.cod_operadora AS cod_Operadora,
    RTRIM(C.cod_plan) AS cod_Plan,
    C.cod_contrato AS cod_Contrato,
    RTRIM(ISNULL(C.usuario, '')) AS usuario,
    C.fecha AS fecha,
    RTRIM(ISNULL(C.movimiento, '')) AS movimiento,
    RTRIM(ISNULL(C.detalle, '')) AS detalle,
    RTRIM(ISNULL(C.revisado_usuario, '')) AS revisado_Usuario,
    C.revisado_fecha AS revisado_Fecha,
    RTRIM(ISNULL(S.cedula, '')) AS cedula,
    RTRIM(ISNULL(S.nombre, '')) AS nombre,
    RTRIM(ISNULL(M.descripcion, '')) AS movimientoDesc,
    CASE WHEN C.revisado_fecha IS NULL THEN 0 ELSE 1 END AS revisado
FROM dbo.fnd_contratos_cambios C
INNER JOIN dbo.fnd_contratos X
    ON C.cod_operadora = X.cod_operadora
   AND C.cod_plan = X.cod_plan
   AND C.cod_contrato = X.cod_contrato
INNER JOIN dbo.socios S
    ON X.cedula = S.cedula
INNER JOIN dbo.US_MOVIMIENTOS_BE M
    ON C.movimiento = M.movimiento
WHERE M.modulo = @Modulo
  AND C.movimiento IN @Movimientos
  AND " + fechaCampo + @" BETWEEN @FechaInicio AND @FechaFin");

            if (!string.IsNullOrWhiteSpace(request.Cedula))
            {
                sql.AppendLine("  AND S.cedula = @Cedula");
            }

            if (!string.IsNullOrWhiteSpace(request.Usuario))
            {
                sql.AppendLine($"  AND {usuarioCampo} = @Usuario");
            }

            if (request.CodOperadora.HasValue)
            {
                sql.AppendLine("  AND C.cod_operadora = @CodOperadora");
            }

            if (!string.IsNullOrWhiteSpace(request.CodPlan))
            {
                sql.AppendLine("  AND C.cod_plan = @CodPlan");
            }

            if (request.CodContrato.HasValue)
            {
                sql.AppendLine("  AND C.cod_contrato = @CodContrato");
            }

            switch ((request.SoloNoRevisados ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "P":
                    sql.AppendLine("  AND C.revisado_fecha IS NULL");
                    break;

                case "R":
                    sql.AppendLine("  AND C.revisado_fecha IS NOT NULL");
                    break;
            }

            sql.AppendLine(request.BuscarUsuarioFechaRevision
                ? "ORDER BY C.revisado_fecha;"
                : "ORDER BY C.fecha;");

            return sql.ToString();
        }

        private static object ConstruirParametrosCambios(
            FrmFndBitacoraCambiosRequest request,
            List<string> movimientos)
        {
            var fechaInicio = (request.FechaIni ?? new DateTime(1900, 1, 1)).Date;
            var fechaFin = (request.FechaFin ?? new DateTime(2100, 12, 31)).Date.AddDays(1).AddTicks(-1);

            return new
            {
                Modulo = ModuloFondos,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Cedula = string.IsNullOrWhiteSpace(request.Cedula) ? null : request.Cedula.Trim(),
                Usuario = string.IsNullOrWhiteSpace(request.Usuario) ? null : request.Usuario.Trim(),
                CodOperadora = request.CodOperadora,
                CodPlan = string.IsNullOrWhiteSpace(request.CodPlan) ? null : request.CodPlan.Trim(),
                CodContrato = request.CodContrato,
                Movimientos = movimientos
            };
        }

        private static List<string> NormalizarMovimientos(List<string>? movimientos)
        {
            return (movimientos ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<FrmFndBitacoraCambiosDto> NormalizarResultado(List<FrmFndBitacoraCambiosDto> result)
        {
            foreach (var item in result)
            {
                item.cod_Plan = (item.cod_Plan ?? string.Empty).Trim();
                item.usuario = (item.usuario ?? string.Empty).Trim();
                item.movimiento = (item.movimiento ?? string.Empty).Trim();
                item.detalle = (item.detalle ?? string.Empty).Trim();
                item.revisado_Usuario = (item.revisado_Usuario ?? string.Empty).Trim();
                item.cedula = (item.cedula ?? string.Empty).Trim();
                item.nombre = (item.nombre ?? string.Empty).Trim();
                item.movimientoDesc = (item.movimientoDesc ?? string.Empty).Trim();
            }

            return result;
        }
    }
}
