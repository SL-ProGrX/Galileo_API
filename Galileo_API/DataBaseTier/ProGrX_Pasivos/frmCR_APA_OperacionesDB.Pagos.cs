using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;
using System.Data;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX_Pasivos
{
    public partial class FrmCrApaOperacionesDB
    {
        /// <summary>
        /// Obtiene pagos APA de una operación con filtros de fecha, estado y lazy load.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <param name="estado"></param>
        /// <param name="fecha_desde"></param>
        /// <param name="fecha_hasta"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaOperacionPagoListaDto> CR_APA_Operaciones_Pagos_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            string estado,
            DateTime? fecha_desde,
            DateTime? fecha_hasta,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            var result = new FrmCrApaOperacionPagoListaDto();

            try
            {
                filtros ??= new FiltrosLazyLoadData();
                var parameters = CR_APA_Operaciones_CrearParametrosLazy(filtros);
                parameters.Add("@cod_acreedor", (cod_acreedor ?? string.Empty).Trim(), DbType.String);
                parameters.Add("@operacion", (operacion ?? string.Empty).Trim(), DbType.String);
                parameters.Add("@estado", estado == "T" ? null : estado, DbType.String);
                parameters.Add("@fecha_desde", fecha_desde?.Date, DbType.DateTime);
                parameters.Add("@fecha_hasta", fecha_hasta?.Date.AddDays(1).AddTicks(-1), DbType.DateTime);
                parameters.Add("@sortCode", CR_APA_Operaciones_SortCode(filtros.sortField, "npago"), DbType.Int32);

                const string filtroSql = @"
FROM CRD_APA_PAGOS
WHERE COD_ACREEDOR = @cod_acreedor
  AND OPERACION = @operacion
  AND (@estado IS NULL OR ESTADO = @estado)
  AND (@fecha_desde IS NULL OR PAGO_FECHA >= @fecha_desde)
  AND (@fecha_hasta IS NULL OR PAGO_FECHA <= @fecha_hasta)
  AND (
      @hasFilter = 0
      OR CONVERT(varchar(20), NPAGO) LIKE @filtro
      OR CONVERT(varchar(20), PAGO_FECHA, 103) LIKE @filtro
      OR DOCUMENTO LIKE @filtro
      OR CONVERT(varchar(40), MONTO) LIKE @filtro
      OR CASE ESTADO WHEN 'P' THEN 'Pendiente' WHEN 'C' THEN 'Cancelado' WHEN 'E' THEN 'Ejecutado' WHEN 'D' THEN 'Detallado' ELSE '' END LIKE @filtro
  )";

                result.total = conn.ExecuteScalar<int>($"SELECT COUNT(1) {filtroSql};", parameters);

                var sqlData = @$"
WITH base AS
(
    SELECT
        ISNULL(NPAGO, 0) AS npago,
        PAGO_FECHA AS pago_fecha,
        RTRIM(ISNULL(DOCUMENTO, '')) AS documento,
        ISNULL(MONTO, 0) AS monto,
        CASE ESTADO WHEN 'P' THEN 'Pendiente' WHEN 'C' THEN 'Cancelado' WHEN 'E' THEN 'Ejecutado' WHEN 'D' THEN 'Detallado' ELSE '' END AS estado
    {filtroSql}
),
ordenado AS
(
    SELECT
        npago,
        pago_fecha,
        documento,
        monto,
        estado,
        ROW_NUMBER() OVER (
            ORDER BY
                CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN npago END ASC,
                CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN npago END DESC,
                CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN pago_fecha END ASC,
                CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN pago_fecha END DESC,
                CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN documento END ASC,
                CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN documento END DESC,
                CASE WHEN @sortCode = 4 AND @isAsc = 1 THEN monto END ASC,
                CASE WHEN @sortCode = 4 AND @isAsc = 0 THEN monto END DESC,
                CASE WHEN @sortCode = 5 AND @isAsc = 1 THEN estado END ASC,
                CASE WHEN @sortCode = 5 AND @isAsc = 0 THEN estado END DESC,
                npago DESC
        ) AS fila
    FROM base
)
SELECT npago, pago_fecha, documento, monto, estado
FROM ordenado
WHERE fila > @offset
  AND fila <= (@offset + @pageSize)
ORDER BY fila;";

                result.lista = conn.Query<FrmCrApaOperacionPagoGridDto>(sqlData, parameters).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaOperacionPagoListaDto>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaOperacionPagoListaDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el detalle de un pago APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <param name="npago"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaOperacionPagoDatosDto> CR_APA_Operaciones_Pago_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            int npago)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
SELECT
    RTRIM(P.COD_ACREEDOR) AS cod_acreedor,
    RTRIM(P.OPERACION) AS operacion,
    P.NPAGO AS npago,
    RTRIM(ISNULL(P.DOCUMENTO, '')) AS documento,
    RTRIM(ISNULL(P.PAGO_USUARIO, '')) AS pago_usuario,
    P.PAGO_FECHA AS pago_fecha,
    P.FECHA_PAGO AS fecha_pago,
    RTRIM(ISNULL(P.ESTADO, '')) AS estado,
    CASE P.ESTADO WHEN 'P' THEN 'Pendiente' WHEN 'C' THEN 'Cancelado' WHEN 'E' THEN 'Ejecutado' WHEN 'D' THEN 'Detallado' ELSE '' END AS estado_desc,
    ISNULL(P.MONTO, 0) AS monto,
    RTRIM(ISNULL(P.TESORERIA_SOLICITUD, '')) AS tesoreria_solicitud,
    RTRIM(ISNULL(P.TESORERIA_USUARIO, '')) AS tesoreria_usuario,
    P.TESORERIA_FECHA AS tesoreria_fecha,
    ISNULL(P.DETALLE_INTERESES, 0) AS detalle_intereses,
    ISNULL(P.DETALLE_CARGOS, 0) AS detalle_cargos,
    ISNULL(P.DETALLE_AMORTIZA, 0) AS detalle_amortiza,
    ISNULL(P.DETALLE_TASA, 0) AS detalle_tasa,
    ISNULL(P.DETALLE_SALDO, 0) AS detalle_saldo,
    ISNULL(P.DETALLE_COMISION, 0) AS detalle_comision,
    RTRIM(ISNULL(P.FORMA_PAGO, '')) AS forma_pago,
    CASE P.FORMA_PAGO WHEN 'DC' THEN 'Débito Cuenta' WHEN 'CK' THEN 'Solicitud Cheque' ELSE RTRIM(ISNULL(P.FORMA_PAGO, '')) END AS forma_pago_desc,
    RTRIM(ISNULL(P.CEDULA_AUTORIZADO, '')) AS cedula_autorizado,
    RTRIM(ISNULL(A.NOMBRE, '')) AS nombre_autorizado
FROM CRD_APA_PAGOS P
LEFT JOIN CRD_APA_AUTORIZADOSCK A
    ON P.COD_ACREEDOR = A.COD_ACREEDOR
   AND P.CEDULA_AUTORIZADO = A.CEDULA
WHERE P.COD_ACREEDOR = @cod_acreedor
  AND P.OPERACION = @operacion
  AND P.NPAGO = @npago;";

                var data = conn.QueryFirstOrDefault<FrmCrApaOperacionPagoDatosDto>(
                    sql,
                    new
                    {
                        cod_acreedor = (cod_acreedor ?? string.Empty).Trim(),
                        operacion = (operacion ?? string.Empty).Trim(),
                        npago
                    });

                return data == null
                    ? DbHelper.CreateErrorResponse<FrmCrApaOperacionPagoDatosDto>("No se encontró el pago.")
                    : DbHelper.CreateOkResponse(data);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaOperacionPagoDatosDto>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaOperacionPagoDatosDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los últimos valores de saldo, tasa y cuota usados para detallar un pago.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <param name="npago"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaOperacionUltimoPagoDto> CR_APA_Operaciones_UltimoPago_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            int? npago)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                FrmCrApaOperacionUltimoPagoDto? data;

                if (npago is > 1)
                {
                    data = conn.QueryFirstOrDefault<FrmCrApaOperacionUltimoPagoDto>(
                        @"SELECT
    ISNULL(MONTO, 0) AS ultima_cuota,
    ISNULL(DETALLE_TASA, 0) AS ultima_tasa,
    ISNULL(DETALLE_SALDO, 0) AS ultimo_saldo
FROM CRD_APA_PAGOS
WHERE COD_ACREEDOR = @cod_acreedor
  AND OPERACION = @operacion
  AND NPAGO = @npago;",
                        new
                        {
                            cod_acreedor = (cod_acreedor ?? string.Empty).Trim(),
                            operacion = (operacion ?? string.Empty).Trim(),
                            npago = npago.Value - 1
                        });
                }
                else if (npago == 1)
                {
                    data = conn.QueryFirstOrDefault<FrmCrApaOperacionUltimoPagoDto>(
                        @"SELECT
    ISNULL(CUOTA_ORIGINAL, 0) AS ultima_cuota,
    ISNULL(TASA_ORIGINAL, 0) AS ultima_tasa,
    ISNULL(MONTO, 0) AS ultimo_saldo
FROM CRD_APA_OPERACIONES
WHERE COD_ACREEDOR = @cod_acreedor
  AND OPERACION = @operacion;",
                        new { cod_acreedor = (cod_acreedor ?? string.Empty).Trim(), operacion = (operacion ?? string.Empty).Trim() });
                }
                else
                {
                    data = conn.QueryFirstOrDefault<FrmCrApaOperacionUltimoPagoDto>(
                        @"SELECT
    ISNULL(CUOTA, 0) AS ultima_cuota,
    ISNULL(TASA, 0) AS ultima_tasa,
    ISNULL(SALDO, 0) AS ultimo_saldo
FROM CRD_APA_OPERACIONES
WHERE COD_ACREEDOR = @cod_acreedor
  AND OPERACION = @operacion;",
                        new { cod_acreedor = (cod_acreedor ?? string.Empty).Trim(), operacion = (operacion ?? string.Empty).Trim() });
                }

                return DbHelper.CreateOkResponse(data ?? new FrmCrApaOperacionUltimoPagoDto());
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaOperacionUltimoPagoDto>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaOperacionUltimoPagoDto>(ex.Message);
            }
        }

        /// <summary>
        /// Registra un pago APA usando el procedimiento legado spCRDAPA_PAGOS_A.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CR_APA_Operaciones_Pago_Insertar(
            int codEmpresa,
            FrmCrApaOperacionPagoGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var ahora = conn.ExecuteScalar<DateTime>("SELECT dbo.MyGetdate()");
                var formaPago = (request.forma_pago ?? string.Empty).Trim();

                const string sql = @"
EXEC spCRDAPA_PAGOS_A
    @cod_acreedor,
    @operacion,
    @pago_usuario,
    @pago_fecha,
    @fecha_pago,
    @estado,
    @monto,
    @detalle_intereses,
    @detalle_cargos,
    @detalle_amortiza,
    @detalle_tasa,
    @detalle_saldo,
    @detalle_fecha,
    @detalle_usuario,
    @detalle_comision,
    @documento,
    @forma_pago;";

                conn.Execute(sql, new
                {
                    cod_acreedor = (request.cod_acreedor ?? string.Empty).Trim(),
                    operacion = (request.operacion ?? string.Empty).Trim(),
                    pago_usuario = (request.usuario ?? string.Empty).Trim(),
                    pago_fecha = ahora,
                    request.fecha_pago,
                    estado = "E",
                    request.monto,
                    request.detalle_intereses,
                    request.detalle_cargos,
                    request.detalle_amortiza,
                    request.detalle_tasa,
                    request.detalle_saldo,
                    detalle_fecha = ahora,
                    detalle_usuario = (request.usuario ?? string.Empty).Trim(),
                    request.detalle_comision,
                    documento = (request.documento ?? string.Empty).Trim(),
                    forma_pago = formaPago
                });

                return DbHelper.CreateOkResponse(1);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene autorizados disponibles para asignar a un pago.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaOperacionAutorizadoDto>> CR_APA_Operaciones_Autorizados_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
SELECT
    RTRIM(ISNULL(CEDULA, '')) AS cedula,
    RTRIM(ISNULL(NOMBRE, '')) AS nombre
FROM CRD_APA_AUTORIZADOSCK
WHERE COD_ACREEDOR = @cod_acreedor
ORDER BY NOMBRE;";

                return DbHelper.CreateOkResponse(
                    conn.Query<FrmCrApaOperacionAutorizadoDto>(sql, new { cod_acreedor = (cod_acreedor ?? string.Empty).Trim() }).ToList());
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaOperacionAutorizadoDto>>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaOperacionAutorizadoDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Asigna o limpia el autorizado de cheque de un pago APA si aún no fue tomado por tesorería.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CR_APA_Operaciones_Pago_Autorizado_Actualizar(
            int codEmpresa,
            FrmCrApaOperacionAsignarAutorizadoRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var locked = conn.ExecuteScalar<int>(
                    @"SELECT ISNULL(COUNT(*), 0)
FROM CRD_APA_PAGOS
WHERE COD_ACREEDOR = @cod_acreedor
  AND OPERACION = @operacion
  AND NPAGO = @npago
  AND TESORERIA_FECHA IS NOT NULL",
                    request);

                if (locked > 0)
                {
                    return DbHelper.CreateErrorResponse<int>("No se puede modificar el autorizado porque el pago ya fue tomado por Tesorería.");
                }

                var cedula = (request.cedula_autorizado ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(cedula))
                {
                    var existe = conn.ExecuteScalar<int>(
                        @"SELECT ISNULL(COUNT(*), 0)
FROM CRD_APA_AUTORIZADOSCK
WHERE COD_ACREEDOR = @cod_acreedor
  AND CEDULA = @cedula",
                        new { request.cod_acreedor, cedula });

                    if (existe == 0)
                    {
                        return DbHelper.CreateErrorResponse<int>("No existe un autorizado con esa cédula para este acreedor.");
                    }
                }

                var rows = conn.Execute(
                    @"UPDATE CRD_APA_PAGOS
SET CEDULA_AUTORIZADO = NULLIF(@cedula_autorizado, '')
WHERE COD_ACREEDOR = @cod_acreedor
  AND OPERACION = @operacion
  AND NPAGO = @npago",
                    new
                    {
                        cod_acreedor = (request.cod_acreedor ?? string.Empty).Trim(),
                        operacion = (request.operacion ?? string.Empty).Trim(),
                        request.npago,
                        cedula_autorizado = cedula
                    });

                return rows == 0
                    ? DbHelper.CreateErrorResponse<int>("No se encontró el pago.")
                    : DbHelper.CreateOkResponse(1);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }
    }
}
