using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmAfLiquidacionwDb
    {
        private readonly IConfiguration _config;
        private readonly PortalDB _portalDb;

        private const string SpBancos = "spCrd_SGT_Bancos";
        private const string SpRenunciaEmiteTDoc = "spAFI_Renuncia_Emite_TDoc";
        private const string SpCuentasBancarias = "spSys_Cuentas_Bancarias";
        private const string SpConsultaPatrimonio = "spAFI_Liq_Consulta_Patrimonio";
        private const string SpRentaGlobal = "spFnd_Renta_Global";
        private const string SpListaPlanes = "spAfiLiquidaListaPlanes";
        private const string SpCreditosPersona = "spAfi_Liquidacion_CreditosPersona";
        private const string SpLiquidacionPatrimonio = "spAfi_Liquidacion_Patrimonio";
        private const string SpLiquidaPlanes = "spAfiLiquidaPlanes";
        private const string SpAbonosPlanPagos = "spAfi_Liquidacion_Abonos_PlanPagos";
        private const string SpLiquidacionAsiento = "spAFI_Liquidacion_Asiento";
        private const string SpTrasladoOpEx = "spAFI_Liquidacion_Traslado_OpEx";
        private const string SpFondosDevolucion = "spAFI_Liquidacion_Fondos_Devolucion";

        private const string SqlTipoAccion = @"
                    SELECT Id_Documento AS item,
                           Descripcion AS descripcion
                    FROM dbo.AFI_CR_RENUNCIAS_TIPO_DOCUMENTO;";

        private const string SqlCausaDetalle = @"
                    SELECT mortalidad,
                           liq_alterna,
                           Tipo_Apl,
                           AJUSTE_TASAS
                    FROM dbo.causas_renuncias
                    WHERE id_causa = @Causa;";

        private const string SqlFondos = "SELECT dbo.fxAFI_Liquidacion_FP_Fondos() AS Flag;";
        private const string SqlActivarControl = "SELECT TOP 1 Activar_Control FROM dbo.afi_cr_parametros;";

        private const string SqlRenunciasSinLiquidar = @"
                    SELECT Cedula,
                           Id_Alterno,
                           Nombre
                    FROM dbo.vAFI_Renuncias_SinLiquidar;";

        private const string SqlSociosActivos = @"
                    SELECT Cedula,
                           Nombre
                    FROM dbo.socios
                    WHERE EstadoActual IN ('S','A')
                    ORDER BY Cedula;";

        private const string SqlSociosRenunciaActiva = @"
                    SELECT S.Cedula,
                           S.Nombre
                    FROM dbo.socios S
                    INNER JOIN dbo.afi_cr_renuncias R
                        ON S.cedula = R.cedula
                       AND R.liq IS NULL
                       AND R.estado IN ('P','V');";

        private const string SqlSociosTodos = @"
                    SELECT S.Cedula,
                           S.Nombre
                    FROM dbo.socios S;";

        private const string SqlActualizarEstadoRenuncias = @"
                    UPDATE dbo.afi_cr_renuncias
                    SET estado = 'V'
                    WHERE vencimiento < dbo.MyGetdate()
                      AND estado = 'T';";

        private const string SqlSocioDetalle = @"
                    SELECT S.cedula,
                           S.nombre,
                           S.fechaingreso,
                           S.estadoactual,
                           0 AS Boleta,
                           ISNULL(E.descripcion,'') AS EstadoPersona
                    FROM dbo.socios S
                    INNER JOIN dbo.AFI_ESTADOS_PERSONA E
                        ON S.estadoActual = E.cod_estado
                    WHERE S.cedula = @Cedula;";

        private const string SqlCausasRenuncia = @"
                    SELECT id_Causa AS item,
                           Descripcion AS descripcion
                    FROM dbo.causas_renuncias
                    WHERE ACTIVO = 1
                      AND Tipo_Apl IN ('A', @TipoApl);";

        private const string SqlCausaAccion = @"
                    SELECT mortalidad,
                           liq_alterna
                    FROM dbo.causas_renuncias
                    WHERE id_causa = @IdCausa;";

        private const string SqlSocioExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.socios
                    WHERE cedula = @Cedula;";

        private const string SqlCodRenuncia = @"
                    SELECT TOP 1 cod_renuncia AS Cod_Renuncia
                    FROM dbo.afi_cr_renuncias
                    WHERE liq IS NULL
                      AND estado IN ('P','V')
                      AND cedula = @Cedula
                    ORDER BY cod_renuncia DESC;";

        private const string SqlSocioDatosBasicos = @"
                    SELECT Cedula,
                           Nacta,
                           id_Promotor,
                           Id_Boleta_Af
                    FROM dbo.Socios
                    WHERE Cedula = @Cedula;";

        private const string SqlMorosidadPorSolicitud = @"
                    SELECT *
                    FROM dbo.morosidad
                    WHERE estado = 'A'
                      AND id_solicitud = @IdSolicitud;";

        public FrmAfLiquidacionwDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _portalDb = new PortalDB(_config);
        }

        /// <summary>
        /// Actualiza el estado de las renuncias a 'V' donde la fecha de vencimiento es menor a la fecha actual.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<int> AF_Liquidacion_ActualizarEstadoRenuncias(int CodEmpresa)
        {
            return DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodEmpresa,
                SqlActualizarEstadoRenuncias);
        }

        private ErrorDto<List<T>> EjecutarStoredProcedureList<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al ejecutar procedimiento almacenado.", result.Code.GetValueOrDefault(-1), new List<T>());
        }

        private ErrorDto<T?> EjecutarStoredProcedureSingle<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.QueryFirstOrDefault<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure));

            return result.Code == 0
                ? DbHelper.CreateOkResponse<T?>(result.Result)
                : DbHelper.CreateErrorResponse<T?>(result.Description ?? "Error al ejecutar procedimiento almacenado.", result.Code.GetValueOrDefault(-1), default);
        }

        private ErrorDto<bool> EjecutarStoredProcedureBool(int codEmpresa, string storedProcedure, object parameters, string errorMessage)
        {
            var result = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                connection.Execute(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse(errorMessage, result.Code.GetValueOrDefault(-1), false);
        }


        private static string NormalizarTipoAplicacion(string? tipo)
        {
            return string.Equals(NormalizarTexto(tipo), "I", StringComparison.OrdinalIgnoreCase) ? "I" : "P";
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        public ErrorDto AF_Liquidacion_Bitacora_Insertar(int CodEmpresa, string usuario, string detalle, string movimiento, int modulo = 7)
        {
            var mProGrX_Security = new MSecurityMainDb(_config);
            return mProGrX_Security.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = modulo
            });
        }
    }
}