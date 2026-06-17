using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasReporteCierresDb
    {
        private readonly PortalDB _portalDb;

        public FrmCajasReporteCierresDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCajasReporteCierresDb(PortalDB portalDb) => _portalDb = portalDb;

        /// <summary>
        /// Consulta las aperturas/cierres de caja por rango y filtro.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codCaja">Codigo de caja.</param>
        /// <param name="fechaInicio">Fecha inicial.</param>
        /// <param name="fechaCorte">Fecha final.</param>
        /// <param name="filtro">Filtro de estado: T, R o V.</param>
        /// <returns>Lista de aperturas de caja.</returns>
        public ErrorDto<List<CajasAperturaReporteDto>> Cajas_Aperturas_Consulta(
            int codEmpresa,
            string codCaja,
            DateTime fechaInicio,
            DateTime fechaCorte,
            string filtro)
        {
            var sql = @"
                SELECT
                    M.Cod_Apertura AS cod_apertura,
                    M.Apertura_Fecha AS apertura_fecha,
                    M.Apertura_Usuario AS apertura_usuario,
                    CASE WHEN M.Estado = 'C' THEN 'Cerrada' ELSE 'Abierta' END AS estado,
                    M.Cierre_Fecha AS cierre_fecha,
                    M.Cierre_Usuario AS cierre_usuario,
                    M.Recibe_Fecha AS recibe_fecha,
                    M.Recibe_Usuario AS recibe_usuario,
                    M.Revisa_Fecha AS revisa_fecha,
                    M.Revisa_Usuario AS revisa_usuario,
                    CASE WHEN ISNULL(D.Cierre_Tipo, 'C') = 'C' THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS cierre_ciego
                FROM CAJAS_APERTURAS_MAIN M
                LEFT JOIN CAJAS_DEFINICION D
                    ON M.COD_CAJA = D.COD_CAJA
                WHERE M.Apertura_Fecha BETWEEN @fechaInicio AND @fechaCorte
                  AND (@codCaja = '' OR M.COD_CAJA = @codCaja)
                  AND (@filtro <> 'R' OR M.Recibe_Fecha IS NULL)
                  AND (@filtro <> 'V' OR M.Revisa_Fecha IS NULL)
                ORDER BY M.Cod_Apertura DESC;";

            return DbHelper.ExecuteListQuery<CajasAperturaReporteDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { codCaja, fechaInicio, fechaCorte, filtro });
        }

        /// <summary>
        /// Consulta el historico de accesos a caja.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codCaja">Codigo de caja.</param>
        /// <param name="fechaInicio">Fecha inicial.</param>
        /// <param name="fechaCorte">Fecha final.</param>
        /// <returns>Lista de accesos.</returns>
        public ErrorDto<List<CajasAccesoDto>> Cajas_Accesos_Consulta(
            int codEmpresa,
            string codCaja,
            DateTime fechaInicio,
            DateTime fechaCorte)
        {
            var sql = @"
                SELECT
                    FechaIngreso AS fecha,
                    Caja AS caja,
                    Apertura AS apertura,
                    Usuario AS usuario,
                    SifVersion AS version
                FROM CAJAS_BITACORA_INGRESO
                WHERE FechaIngreso BETWEEN @fechaInicio AND @fechaCorte
                  AND (@codCaja = '' OR Caja = @codCaja)
                ORDER BY FechaIngreso DESC;";

            return DbHelper.ExecuteListQuery<CajasAccesoDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { codCaja, fechaInicio, fechaCorte });
        }

        /// <summary>
        /// Consulta depositos registrados al cierre.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codCaja">Codigo de caja.</param>
        /// <param name="codApertura">Codigo de apertura.</param>
        /// <returns>Lista de depositos.</returns>
        public ErrorDto<List<CajasDepositoDto>> Cajas_Depositos_Consulta(
            int codEmpresa,
            string codCaja,
            int codApertura)
        {
            const string sql = "exec spCajas_CierreDepositoDivisa @Caja, @Apertura, @Divisa";

            var result = DbHelper.ExecuteListQuery<CajasDepositoDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Caja = codCaja, Apertura = codApertura, Divisa = "COL" });

            if (result.Code == 0)
            {
                NormalizarDepositos(result.Result ?? []);
            }

            return result;
        }

        /// <summary>
        /// Aplica el cierre forzado original de caja.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codCaja">Codigo de caja.</param>
        /// <param name="codApertura">Codigo de apertura.</param>
        /// <param name="usuario">Usuario que aplica el cierre.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto<bool> Cajas_Cierre_Forzado(
            int codEmpresa,
            string codCaja,
            int codApertura,
            string usuario)
        {
            const string sql = "exec spCajas_Cierre_Forzado @codCaja, @codApertura, @usuario";

            var result = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new { codCaja, codApertura, usuario });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse(result.Description ?? string.Empty, result.Code.GetValueOrDefault(-1), false);
        }

        /// <summary>
        /// Mantiene compatibilidad con la ruta anterior de forzar cierre.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codCaja">Codigo de caja.</param>
        /// <param name="codApertura">Codigo de apertura.</param>
        /// <param name="usuario">Usuario que aplica el cierre.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto<bool> Cajas_Cierre_Forzar(
            int codEmpresa,
            string codCaja,
            int codApertura,
            string usuario)
        {
            return Cajas_Cierre_Forzado(codEmpresa, codCaja, codApertura, usuario);
        }

        /// <summary>
        /// Marca la recepcion de un cierre de caja.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codCaja">Codigo de caja.</param>
        /// <param name="codApertura">Codigo de apertura.</param>
        /// <param name="usuario">Usuario que recibe.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto<bool> Cajas_Cierre_Recibe(
            int codEmpresa,
            string codCaja,
            int codApertura,
            string usuario)
        {
            const string sql = @"
                UPDATE CAJAS_APERTURAS_MAIN
                SET RECIBE_FECHA = GETDATE(),
                    RECIBE_USUARIO = @usuario
                WHERE COD_CAJA = @codCaja
                  AND COD_APERTURA = @codApertura;";

            return EjecutarActualizacionCierre(codEmpresa, sql, codCaja, codApertura, usuario, esRevision: false);
        }

        /// <summary>
        /// Marca la revision de un cierre de caja.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codCaja">Codigo de caja.</param>
        /// <param name="codApertura">Codigo de apertura.</param>
        /// <param name="usuario">Usuario que revisa.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto<bool> Cajas_Cierre_Revisa(
            int codEmpresa,
            string codCaja,
            int codApertura,
            string usuario)
        {
            const string sql = @"
                UPDATE CAJAS_APERTURAS_MAIN
                SET REVISA_FECHA = GETDATE(),
                    REVISA_USUARIO = @usuario
                WHERE COD_CAJA = @codCaja
                  AND COD_APERTURA = @codApertura;";

            return EjecutarActualizacionCierre(codEmpresa, sql, codCaja, codApertura, usuario, esRevision: true);
        }

        /// <summary>
        /// Consulta las cajas disponibles para busqueda.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Lista generica de cajas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Definicion_Lista(int codEmpresa)
        {
            const string sql = @"
                SELECT 
                    COD_CAJA AS item,
                    DESCRIPCION AS descripcion
                FROM CAJAS_DEFINICION
                ORDER BY COD_CAJA;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Consulta la informacion de cierre y sus depositos.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codCaja">Codigo de caja.</param>
        /// <param name="codApertura">Codigo de apertura.</param>
        /// <returns>Datos de verificacion del cierre.</returns>
        public ErrorDto<CajasCierreVerificacionDto> Cajas_Cierre_Verificacion(
            int codEmpresa,
            string codCaja,
            int codApertura)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                const string sqlVerificacion = @"
                    SELECT
                        M.COD_CAJA AS caja,
                        C.DESCRIPCION AS cajaDesc,
                        M.COD_APERTURA AS apertura,
                        CASE WHEN M.Estado = 'C' THEN 'Cerrada' ELSE 'Abierta' END AS estado,
                        M.APERTURA_FECHA AS aperturaFecha,
                        M.APERTURA_USUARIO AS aperturaUsuario,
                        M.CIERRE_FECHA AS cierreFecha,
                        M.CIERRE_USUARIO AS cierreUsuario,
                        M.RECIBE_FECHA AS recibeFecha,
                        M.RECIBE_USUARIO AS recibeUsuario,
                        M.REVISA_FECHA AS revisaFecha,
                        M.REVISA_USUARIO AS revisaUsuario
                    FROM CAJAS_APERTURAS_MAIN M
                    INNER JOIN CAJAS_DEFINICION C
                        ON M.COD_CAJA = C.COD_CAJA
                    WHERE M.COD_CAJA = @codCaja
                      AND M.COD_APERTURA = @codApertura;";

                var verificacion = cn.QueryFirstOrDefault<CajasCierreVerificacionDetalleDto>(
                    sqlVerificacion,
                    new { codCaja, codApertura }) ?? new CajasCierreVerificacionDetalleDto();

                var depositos = cn.Query<CajasDepositoDto>(
                    "spCajas_CierreDepositoDivisa",
                    new { Caja = codCaja, Apertura = codApertura, Divisa = "COL" },
                    commandType: CommandType.StoredProcedure).ToList();

                NormalizarDepositos(depositos);

                var total = depositos
                    .Where(d => d.estado == "Activado" || d.estado == "En Bancos")
                    .Sum(d => d.monto);

                return new CajasCierreVerificacionDto
                {
                    verificacion = verificacion,
                    depositos = depositos,
                    total = total
                };
            });
        }

        /// <summary>
        /// Aplica el cierre preliminar requerido antes de imprimir reportes de aperturas abiertas.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codCaja">Codigo de caja.</param>
        /// <param name="codApertura">Codigo de apertura.</param>
        /// <param name="usuario">Usuario que genera el reporte.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto<bool> Cajas_Cierre_Preliminar_Aplicar(
            int codEmpresa,
            string codCaja,
            int codApertura,
            string usuario)
        {
            const string sql = "exec spCajas_CierreCajaMain @codCaja, @codApertura, @usuario, 1";

            var result = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new { codCaja, codApertura, usuario });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse(result.Description ?? string.Empty, result.Code.GetValueOrDefault(-1), false);
        }

        private ErrorDto<bool> EjecutarActualizacionCierre(
            int codEmpresa,
            string sql,
            string codCaja,
            int codApertura,
            string usuario,
            bool esRevision)
        {
            var response = DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                const string sqlEstado = @"
                    SELECT
                        CIERRE_FECHA AS cierreFecha,
                        CIERRE_USUARIO AS cierreUsuario,
                        RECIBE_FECHA AS recibeFecha,
                        REVISA_FECHA AS revisaFecha
                    FROM CAJAS_APERTURAS_MAIN
                    WHERE COD_CAJA = @codCaja
                      AND COD_APERTURA = @codApertura;";

                var cierre = cn.QueryFirstOrDefault<CajasCierreValidacionDto>(
                    sqlEstado,
                    new { codCaja, codApertura });

                var mensaje = ValidarAplicacionCierre(cierre, usuario, esRevision);
                if (!string.IsNullOrWhiteSpace(mensaje))
                {
                    return DbHelper.CreateErrorResponse(mensaje, -1, false);
                }

                var filas = cn.Execute(sql, new { codCaja, codApertura, usuario });

                return filas > 0
                    ? DbHelper.CreateOkResponse(true)
                    : DbHelper.CreateErrorResponse("No se encontro la apertura de caja indicada.", -1, false);
            });

            return response.Code == 0
                ? response.Result ?? DbHelper.CreateErrorResponse("No se pudo aplicar la actualizacion del cierre.", -1, false)
                : DbHelper.CreateErrorResponse(response.Description ?? string.Empty, response.Code.GetValueOrDefault(-1), false);
        }

        private static string ValidarAplicacionCierre(
            CajasCierreValidacionDto? cierre,
            string usuario,
            bool esRevision)
        {
            if (cierre is null)
            {
                return "No se encontro la apertura de caja indicada.";
            }

            if (cierre.cierreFecha is null)
            {
                return "No se ha realizado el cierre de esta caja, verifique!";
            }

            if (!string.IsNullOrWhiteSpace(cierre.cierreUsuario)
                && string.Equals(cierre.cierreUsuario.Trim(), usuario.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return esRevision
                    ? "El usuario actual no puede revisar el cierre de esta caja porque es el mismo que la cerro, verifique!"
                    : "El usuario actual no puede recibir el cierre de esta caja porque es el mismo que la cerro, verifique!";
            }

            if (!esRevision && cierre.recibeFecha is not null)
            {
                return "Ya fue aplicada la recepcion del cierre anteriormente, verifique!";
            }

            if (esRevision && cierre.recibeFecha is null)
            {
                return "NO se ha recibido este cierre, verifique!";
            }

            if (esRevision && cierre.revisaFecha is not null)
            {
                return "Este cierre ya fue Revisado anteriormente, verifique!";
            }

            return string.Empty;
        }

        private static void NormalizarDepositos(List<CajasDepositoDto> depositos)
        {
            foreach (var deposito in depositos)
            {
                deposito.estado = deposito.estado switch
                {
                    "1" => "Activado",
                    "2" => "En Bancos",
                    _ => "Anulado"
                };
            }
        }

        private sealed class CajasCierreValidacionDto
        {
            public DateTime? cierreFecha { get; set; } = null;
            public string? cierreUsuario { get; set; } = null;
            public DateTime? recibeFecha { get; set; } = null;
            public DateTime? revisaFecha { get; set; } = null;
        }
    }
}
