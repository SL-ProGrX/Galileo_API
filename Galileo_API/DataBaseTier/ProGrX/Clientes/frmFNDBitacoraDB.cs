using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmFndBitacoraDb
    {
        // Variable global para el módulo de Fondo de Inversión
        public const int vModulo = 18;
        private readonly IConfiguration _config;

        private const string SqlMovimientos = @"
                    SELECT MOVIMIENTO,
                           DESCRIPCION
                    FROM dbo.US_MOVIMIENTOS_BE
                    WHERE MODULO = @Modulo
                    ORDER BY MOVIMIENTO;";

        private const string SqlBitacoraCambios = @"
                    SELECT C.ID_BITACORA AS Id_Bitacora,
                           C.COD_OPERADORA AS Cod_Operadora,
                           C.COD_PLAN AS Cod_Plan,
                           C.COD_CONTRATO AS Cod_Contrato,
                           C.USUARIO AS Usuario,
                           C.FECHA AS Fecha,
                           C.MOVIMIENTO AS Movimiento,
                           C.DETALLE AS Detalle,
                           C.REVISADO_USUARIO AS Revisado_Usuario,
                           C.REVISADO_FECHA AS Revisado_Fecha,
                           S.CEDULA AS Cedula,
                           S.NOMBRE AS Nombre,
                           M.DESCRIPCION AS MovimientoDesc,
                           CASE WHEN C.REVISADO_FECHA IS NULL THEN 0 ELSE 1 END AS Revisado
                    FROM dbo.FND_CONTRATOS_CAMBIOS C
                    INNER JOIN dbo.FND_CONTRATOS X
                        ON C.COD_OPERADORA = X.COD_OPERADORA
                       AND C.COD_PLAN = X.COD_PLAN
                       AND C.COD_CONTRATO = X.COD_CONTRATO
                    INNER JOIN dbo.SOCIOS S
                        ON X.CEDULA = S.CEDULA
                    INNER JOIN dbo.US_MOVIMIENTOS_BE M
                        ON C.MOVIMIENTO = M.MOVIMIENTO
                    WHERE M.MODULO = @Modulo
                      AND (@Cedula IS NULL OR S.CEDULA LIKE @Cedula)
                      AND (@FiltraFecha = 0 OR C.FECHA BETWEEN @FechaIni AND @FechaFin)
                      AND (@FiltraMovimientos = 0 OR C.MOVIMIENTO IN @Movimientos)
                      AND (@CodPlan IS NULL OR C.COD_PLAN = @CodPlan)
                      AND (@CodOperadora IS NULL OR C.COD_OPERADORA = @CodOperadora)
                      AND (@CodContrato IS NULL OR C.COD_CONTRATO = @CodContrato)
                      AND (@SoloNoRevisados <> 'P' OR C.REVISADO_FECHA IS NULL)
                      AND (@SoloNoRevisados <> 'R' OR C.REVISADO_FECHA IS NOT NULL)
                    ORDER BY C.FECHA;";

        private const string SqlCambioRevisar = @"
                    UPDATE dbo.fnd_contratos_cambios
                    SET revisado_usuario = @RevisadoUsuario,
                        revisado_fecha = dbo.MyGetdate()
                    WHERE id_Bitacora = @IdBitacora;";

        private const string SpRegistraTags = "spSIFRegistraTags";

        public FrmFndBitacoraDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los movimientos de US_MOVIMIENTOS_BE para el módulo de Fondo de Inversión.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de movimientos.</returns>
        public ErrorDto<List<UsMovimiento>> Fnd_Movimientos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<UsMovimiento>(
                CreatePortalDb(),
                CodEmpresa,
                SqlMovimientos,
                new { Modulo = vModulo });
        }

        /// <summary>
        /// Obtiene los cambios de contratos en bitácora según filtros.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Filtros de búsqueda.</param>
        /// <returns>Listado de cambios de contratos.</returns>
        public ErrorDto<List<FndBitacoraCambiosResult>> Fnd_Bitacora_Cambios_Obtener(int CodEmpresa, FndBitacoraCambiosRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de bitácora son requeridos.",
                    -2,
                    new List<FndBitacoraCambiosResult>());
            }

            return DbHelper.ExecuteListQuery<FndBitacoraCambiosResult>(
                CreatePortalDb(),
                CodEmpresa,
                SqlBitacoraCambios,
                CrearParametrosBitacora(request));
        }

        /// <summary>
        /// Marca como revisado un cambio de contrato en la bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Id de bitácora y usuario que revisa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> Fnd_Bitacora_Cambio_Revisar(int CodEmpresa, FndBitacoraCambioRevisarRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse("Los datos de revisión son requeridos.", -2, false);
            }

            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                SqlCambioRevisar,
                new
                {
                    RevisadoUsuario = NormalizarTexto(request.Revisado_Usuario),
                    IdBitacora = request.Id_Bitacora
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result > 0)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al marcar cambio como revisado.", result.Code.GetValueOrDefault(-1), false);
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado spSIFRegistraTags para registrar tags.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Parámetros del tag a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> Sif_RegistraTags(int CodEmpresa, SifRegistraTagsRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse("Los datos del tag son requeridos.", -2, false);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    SpRegistraTags,
                    CrearParametrosTag(request),
                    commandType: System.Data.CommandType.StoredProcedure);

                return true;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al registrar tag.", result.Code.GetValueOrDefault(-1), false);
        }

        /// <summary>
        /// Crea parámetros seguros para consultar la bitácora.
        /// </summary>
        private static DynamicParameters CrearParametrosBitacora(FndBitacoraCambiosRequest request)
        {
            var parameters = new DynamicParameters();
            var cedula = NormalizarTexto(request.Cedula);
            var codPlan = NormalizarTexto(request.CodPlan);
            var soloNoRevisados = NormalizarTexto(request.SoloNoRevisados);
            var movimientos = request.Movimientos ?? new List<string>();
            var filtraFecha = request.FechaIni.HasValue && request.FechaFin.HasValue;

            parameters.Add("@Modulo", vModulo);
            parameters.Add("@Cedula", string.IsNullOrWhiteSpace(cedula) ? null : $"%{cedula}%");
            parameters.Add("@FiltraFecha", filtraFecha ? 1 : 0);
            parameters.Add("@FechaIni", filtraFecha ? request.FechaIni!.Value.Date : null);
            parameters.Add("@FechaFin", filtraFecha ? request.FechaFin!.Value.Date.AddDays(1).AddSeconds(-1) : null);
            parameters.Add("@FiltraMovimientos", movimientos.Count > 0 ? 1 : 0);
            parameters.Add("@Movimientos", movimientos);
            parameters.Add("@CodPlan", string.IsNullOrWhiteSpace(codPlan) ? null : codPlan);
            parameters.Add("@CodOperadora", request.CodOperadora);
            parameters.Add("@CodContrato", request.CodContrato);
            parameters.Add("@SoloNoRevisados", soloNoRevisados);

            return parameters;
        }

        /// <summary>
        /// Crea parámetros seguros para registrar tags.
        /// </summary>
        private static object CrearParametrosTag(SifRegistraTagsRequest request)
        {
            return new
            {
                pCodigo = request.Codigo,
                ptag = NormalizarTexto(request.Tag),
                Usuario = NormalizarTexto(request.Usuario),
                pObservacion = NormalizarTexto(request.Observacion),
                pDocumento = NormalizarTexto(request.Documento),
                pModulo = request.Modulo,
                pLlave_01 = NormalizarTexto(request.Llave_01),
                pLlave_02 = NormalizarTexto(request.Llave_02),
                pLlave_03 = NormalizarTexto(request.Llave_03)
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