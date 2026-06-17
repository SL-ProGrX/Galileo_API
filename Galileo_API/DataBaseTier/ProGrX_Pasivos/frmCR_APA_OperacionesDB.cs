using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Pasivos
{
    public partial class FrmCrApaOperacionesDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrApaOperacionesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene acreedores activos para el selector lateral de operaciones APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaOperacionAcreedorDto>> CR_APA_Operaciones_Acreedores_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
SELECT
    RTRIM(COD_ACREEDOR) AS cod_acreedor,
    RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion,
    RTRIM(ISNULL(ESTADO, '')) AS estado
FROM CRD_APA_ACREEDORES
ORDER BY DESCRIPCION;";

                return DbHelper.CreateOkResponse(conn.Query<FrmCrApaOperacionAcreedorDto>(sql).ToList());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaOperacionAcreedorDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene contactos de un acreedor para el selector lateral y datos de localización.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaOperacionContactoDto>> CR_APA_Operaciones_Contactos_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
SELECT
    RTRIM(ISNULL(COD_CONTACTO, '')) AS cod_contacto,
    RTRIM(ISNULL(NOMBRE, '')) AS nombre,
    RTRIM(ISNULL(TEL_CEL, '')) AS tel_cel,
    RTRIM(ISNULL(TEL_TRABAJO, '')) AS tel_trabajo,
    RTRIM(ISNULL(TEL_FAX, '')) AS tel_fax,
    RTRIM(ISNULL(EMAIL, '')) AS email
FROM CRD_APA_CONTACTOS
WHERE COD_ACREEDOR = @cod_acreedor
ORDER BY COD_CONTACTO;";

                return DbHelper.CreateOkResponse(
                    conn.Query<FrmCrApaOperacionContactoDto>(
                        sql,
                        new { cod_acreedor = (cod_acreedor ?? string.Empty).Trim() }).ToList());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaOperacionContactoDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene operaciones APA de un acreedor con filtro lazy.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <param name="estado"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaOperacionListaDto> CR_APA_Operaciones_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            string estado,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            var result = new FrmCrApaOperacionListaDto();

            try
            {
                filtros ??= new FiltrosLazyLoadData();
                var parameters = CR_APA_Operaciones_CrearParametrosLazy(filtros);
                parameters.Add("@cod_acreedor", (cod_acreedor ?? string.Empty).Trim(), DbType.String);
                parameters.Add("@operacion", string.IsNullOrWhiteSpace(operacion) ? null : $"%{operacion.Trim()}%", DbType.String);
                parameters.Add("@estado", estado == "T" ? null : estado, DbType.String);

                const string filtroSql = @"
FROM CRD_APA_OPERACIONES
WHERE COD_ACREEDOR = @cod_acreedor
  AND (@operacion IS NULL OR OPERACION LIKE @operacion)
  AND (@estado IS NULL OR ESTADO = @estado)
  AND (
      @hasFilter = 0
      OR OPERACION LIKE @filtro
      OR CONVERT(varchar(40), MONTO) LIKE @filtro
      OR CONVERT(varchar(40), CUOTA) LIKE @filtro
      OR CONVERT(varchar(40), SALDO) LIKE @filtro
      OR CASE WHEN ESTADO = 'A' THEN 'Activa' WHEN ESTADO = 'C' THEN 'Cancelado' ELSE '' END LIKE @filtro
  )";

                result.total = conn.ExecuteScalar<int>($"SELECT COUNT(1) {filtroSql};", parameters);

                var sortCode = CR_APA_Operaciones_SortCode(filtros.sortField, "operacion");
                parameters.Add("@sortCode", sortCode, DbType.Int32);

                var sqlData = @$"
WITH base AS
(
    SELECT
        RTRIM(OPERACION) AS operacion,
        ISNULL(MONTO, 0) AS monto,
        ISNULL(CUOTA, 0) AS cuota,
        ISNULL(SALDO, 0) AS saldo,
        CASE WHEN ESTADO = 'A' THEN 'Activa' WHEN ESTADO = 'C' THEN 'Cancelado' ELSE '' END AS estado
    {filtroSql}
),
ordenado AS
(
    SELECT
        operacion,
        monto,
        cuota,
        saldo,
        estado,
        ROW_NUMBER() OVER (
            ORDER BY
                CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN operacion END ASC,
                CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN operacion END DESC,
                CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN monto END ASC,
                CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN monto END DESC,
                CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN cuota END ASC,
                CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN cuota END DESC,
                CASE WHEN @sortCode = 4 AND @isAsc = 1 THEN saldo END ASC,
                CASE WHEN @sortCode = 4 AND @isAsc = 0 THEN saldo END DESC,
                CASE WHEN @sortCode = 5 AND @isAsc = 1 THEN estado END ASC,
                CASE WHEN @sortCode = 5 AND @isAsc = 0 THEN estado END DESC,
                operacion ASC
        ) AS fila
    FROM base
)
SELECT operacion, monto, cuota, saldo, estado
FROM ordenado
WHERE fila > @offset
  AND fila <= (@offset + @pageSize)
ORDER BY fila;";

                result.lista = conn.Query<FrmCrApaOperacionGridDto>(sqlData, parameters).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaOperacionListaDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el detalle de una operación APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaOperacionDatosDto> CR_APA_Operacion_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
SELECT
    RTRIM(O.COD_ACREEDOR) AS cod_acreedor,
    RTRIM(ISNULL(A.DESCRIPCION, '')) AS acreedor_desc,
    RTRIM(O.OPERACION) AS operacion,
    ISNULL(O.PORC_RESPONSABILIDAD, 0) AS porc_responsabilidad,
    RTRIM(ISNULL(O.TIPO, '')) AS tipo,
    CASE O.TIPO WHEN 'M' THEN 'Multiple' WHEN 'U' THEN 'Una a Una' WHEN 'C' THEN 'Capital de trabajo' ELSE RTRIM(ISNULL(O.TIPO, '')) END AS tipo_desc,
    RTRIM(ISNULL(O.NOTAS, '')) AS notas,
    ISNULL(O.MONTO, 0) AS monto,
    ISNULL(O.SALDO, 0) AS saldo,
    ISNULL(O.TASA, 0) AS tasa,
    ISNULL(O.PLAZO, 0) AS plazo,
    ISNULL(O.CUOTA, 0) AS cuota,
    O.FECHA_FORMALIZA AS fecha_formaliza,
    O.FECHA_PRIMER_PAGO AS fecha_primer_pago,
    ISNULL(O.DIA_DE_PAGO, 0) AS dia_de_pago,
    ISNULL(O.COMISION_ADMIN, 0) AS comision_admin,
    RTRIM(ISNULL(O.ESTADO, '')) AS estado,
    CASE O.ESTADO WHEN 'A' THEN 'Activa' WHEN 'C' THEN 'Cancelado' ELSE RTRIM(ISNULL(O.ESTADO, '')) END AS estado_desc,
    RTRIM(ISNULL(O.RESPONSABILIDAD_BASE, 'O')) AS responsabilidad_base,
    RTRIM(ISNULL(O.COMISION_BASE, 'O')) AS comision_base,
    RTRIM(ISNULL(O.COD_OFICINA, '')) AS cod_oficina,
    RTRIM(ISNULL(S.DESCRIPCION, '')) AS oficina_desc,
    RTRIM(ISNULL(O.PERIOCIDAD_PAGO, 'M')) AS periocidad_pago,
    CASE O.PERIOCIDAD_PAGO WHEN 'M' THEN 'Mensual' WHEN 'T' THEN 'Trimestral' WHEN 'S' THEN 'Semestral' WHEN 'A' THEN 'Anual' ELSE RTRIM(ISNULL(O.PERIOCIDAD_PAGO, '')) END AS periodicidad_desc,
    RTRIM(ISNULL(O.COD_DIVISA, '')) AS cod_divisa,
    RTRIM(ISNULL(D.DESCRIPCION, O.COD_DIVISA)) AS divisa_desc,
    ISNULL(O.TIPO_CAMBIO, 1) AS tipo_cambio,
    O.COD_LINEA AS cod_linea,
    RTRIM(ISNULL(L.DESCRIPCION, '')) AS linea_desc,
    O.MODIFICA_FECHA AS fecha_actualiza
FROM CRD_APA_OPERACIONES O
LEFT JOIN CRD_APA_ACREEDORES A ON O.COD_ACREEDOR = A.COD_ACREEDOR
LEFT JOIN SIF_OFICINAS S ON O.COD_OFICINA = S.COD_OFICINA
LEFT JOIN CNTX_DIVISAS D ON O.COD_DIVISA = D.COD_DIVISA
LEFT JOIN CRD_APA_LINEAS L ON O.COD_ACREEDOR = L.COD_ACREEDOR AND O.COD_LINEA = L.COD_LINEA
WHERE O.COD_ACREEDOR = @cod_acreedor
  AND O.OPERACION = @operacion;";

                var data = conn.QueryFirstOrDefault<FrmCrApaOperacionDatosDto>(
                    sql,
                    new
                    {
                        cod_acreedor = (cod_acreedor ?? string.Empty).Trim(),
                        operacion = (operacion ?? string.Empty).Trim()
                    });

                return data == null
                    ? DbHelper.CreateErrorResponse<FrmCrApaOperacionDatosDto>("No se encontró la operación.")
                    : DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaOperacionDatosDto>(ex.Message);
            }
        }

        /// <summary>
        /// Inserta una operación APA nueva.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CR_APA_Operacion_Insertar(
            int codEmpresa,
            FrmCrApaOperacionGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var existe = conn.ExecuteScalar<int>(
                    @"SELECT ISNULL(COUNT(*), 0) FROM CRD_APA_OPERACIONES WHERE COD_ACREEDOR = @cod_acreedor AND OPERACION = @operacion",
                    new { request.cod_acreedor, request.operacion });

                if (existe > 0)
                {
                    return DbHelper.CreateErrorResponse<int>("Ya existe una operación con ese código en este acreedor.");
                }

                const string sql = @"
INSERT INTO CRD_APA_OPERACIONES
(
    COD_ACREEDOR, OPERACION, PORC_RESPONSABILIDAD, TIPO, NOTAS, MONTO, SALDO,
    TASA, TASA_ORIGINAL, PLAZO, PLAZO_ORIGINAL, CUOTA, CUOTA_ORIGINAL,
    FECHA_FORMALIZA, FECHA_PRIMER_PAGO, DIA_DE_PAGO, COMISION_ADMIN, ESTADO,
    RESPONSABILIDAD_BASE, COMISION_BASE, COD_OFICINA, PERIOCIDAD_PAGO,
    FECHA_PROX_PAGO, COD_DIVISA, TIPO_CAMBIO, COD_LINEA, REGISTRO_FECHA,
    REGISTRO_USUARIO
)
VALUES
(
    @cod_acreedor, @operacion, @porc_responsabilidad, @tipo, @notas, @monto, @monto,
    @tasa, @tasa, @plazo, @plazo, @cuota, @cuota,
    @fecha_formaliza, @fecha_primer_pago, @dia_de_pago, @comision_admin, 'A',
    @responsabilidad_base, @comision_base, @cod_oficina, @periocidad_pago,
    @fecha_primer_pago, @cod_divisa, @tipo_cambio, @cod_linea, GETDATE(),
    @usuario
);";

                conn.Execute(sql, CR_APA_Operacion_NormalizarParametros(request));
                return DbHelper.CreateOkResponse(1);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una operación APA, respetando bloqueo de campos cuando ya tiene pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CR_APA_Operacion_Actualizar(
            int codEmpresa,
            FrmCrApaOperacionGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var pagos = conn.ExecuteScalar<int>(
                    @"SELECT ISNULL(COUNT(*), 0) FROM CRD_APA_PAGOS WHERE COD_ACREEDOR = @cod_acreedor AND OPERACION = @operacion",
                    new { request.cod_acreedor, request.operacion });

                var puedeEditarTodo = pagos == 0 && request.edita_todo;
                var sql = puedeEditarTodo ? CR_APA_Operacion_SqlActualizarTodo() : CR_APA_Operacion_SqlActualizarParcial();
                var rows = conn.Execute(sql, CR_APA_Operacion_NormalizarParametros(request));

                return rows == 0
                    ? DbHelper.CreateErrorResponse<int>("No se encontró la operación.")
                    : DbHelper.CreateOkResponse(1);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Cierra una operación APA activa con saldo cero.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<int> CR_APA_Operacion_Cerrar(int codEmpresa, string cod_acreedor, string operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var actual = conn.QueryFirstOrDefault<(decimal saldo, string estado)>(
                    @"SELECT ISNULL(SALDO, 0) AS saldo, ISNULL(ESTADO, '') AS estado
FROM CRD_APA_OPERACIONES
WHERE COD_ACREEDOR = @cod_acreedor AND OPERACION = @operacion",
                    new { cod_acreedor = (cod_acreedor ?? string.Empty).Trim(), operacion = (operacion ?? string.Empty).Trim() });

                if (actual == default)
                {
                    return DbHelper.CreateErrorResponse<int>("No se encontró la operación.");
                }

                if (actual.saldo > 0)
                {
                    return DbHelper.CreateErrorResponse<int>("No es posible cerrar la operación seleccionada porque tiene saldo mayor a cero.");
                }

                if (actual.estado != "A")
                {
                    return DbHelper.CreateErrorResponse<int>("Solo es posible cerrar operaciones activas.");
                }

                conn.Execute(
                    @"UPDATE CRD_APA_OPERACIONES SET ESTADO = 'C' WHERE COD_ACREEDOR = @cod_acreedor AND OPERACION = @operacion",
                    new { cod_acreedor = (cod_acreedor ?? string.Empty).Trim(), operacion = (operacion ?? string.Empty).Trim() });

                return DbHelper.CreateOkResponse(1);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la cantidad de pagos asociados a una operación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<int> CR_APA_Operacion_PagosCantidad(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var total = conn.ExecuteScalar<int>(
                    @"SELECT ISNULL(COUNT(*), 0) FROM CRD_APA_PAGOS WHERE COD_ACREEDOR = @cod_acreedor AND OPERACION = @operacion",
                    new { cod_acreedor = (cod_acreedor ?? string.Empty).Trim(), operacion = (operacion ?? string.Empty).Trim() });

                return DbHelper.CreateOkResponse(total);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene líneas activas por acreedor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaOperacionCatalogoDto>> CR_APA_Operaciones_Lineas_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
SELECT
    CONVERT(varchar(20), COD_LINEA) AS idx,
    RTRIM(ISNULL(DESCRIPCION, '')) AS itmx
FROM CRD_APA_LINEAS
WHERE ACTIVA = 1
  AND COD_ACREEDOR = @cod_acreedor
ORDER BY DESCRIPCION;";

                return DbHelper.CreateOkResponse(
                    conn.Query<FrmCrApaOperacionCatalogoDto>(sql, new { cod_acreedor = (cod_acreedor ?? string.Empty).Trim() }).ToList());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaOperacionCatalogoDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene oficinas para operaciones de capital de trabajo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaOperacionCatalogoDto>> CR_APA_Operaciones_Oficinas_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
SELECT
    RTRIM(CONVERT(varchar(20), COD_OFICINA)) AS idx,
    RTRIM(ISNULL(DESCRIPCION, '')) AS itmx
FROM SIF_OFICINAS
ORDER BY DESCRIPCION;";

                return DbHelper.CreateOkResponse(conn.Query<FrmCrApaOperacionCatalogoDto>(sql).ToList());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaOperacionCatalogoDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene divisas para operaciones APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaOperacionCatalogoDto>> CR_APA_Operaciones_Divisas_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
SELECT
    RTRIM(COD_DIVISA) AS idx,
    RTRIM(ISNULL(DESCRIPCION, COD_DIVISA)) AS itmx
FROM CNTX_DIVISAS
ORDER BY DESCRIPCION;";

                return DbHelper.CreateOkResponse(conn.Query<FrmCrApaOperacionCatalogoDto>(sql).ToList());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaOperacionCatalogoDto>>(ex.Message);
            }
        }

        private static DynamicParameters CR_APA_Operaciones_CrearParametrosLazy(FiltrosLazyLoadData filtros)
        {
            var hasFilter = !string.IsNullOrWhiteSpace(filtros.filtro);
            var parameters = new DynamicParameters();
            parameters.Add("@hasFilter", hasFilter ? 1 : 0, DbType.Int32);
            parameters.Add("@filtro", hasFilter ? $"%{filtros.filtro!.Trim()}%" : null, DbType.String);
            parameters.Add("@offset", filtros.pagina < 0 ? 0 : filtros.pagina, DbType.Int32);
            parameters.Add("@pageSize", filtros.paginacion <= 0 ? 30 : filtros.paginacion, DbType.Int32);
            parameters.Add("@isAsc", filtros.sortOrder != -1 && filtros.sortOrder != 2 ? 1 : 0, DbType.Int32);
            return parameters;
        }

        private static int CR_APA_Operaciones_SortCode(string? sortField, string defaultField)
        {
            return (sortField ?? defaultField).Trim().ToLowerInvariant() switch
            {
                "monto" => 2,
                "cuota" => 3,
                "saldo" => 4,
                "estado" => 5,
                "pago_fecha" => 2,
                "documento" => 3,
                _ => 1
            };
        }

        private static object CR_APA_Operacion_NormalizarParametros(FrmCrApaOperacionGuardarRequest request)
        {
            return new
            {
                cod_acreedor = (request.cod_acreedor ?? string.Empty).Trim(),
                operacion = (request.operacion ?? string.Empty).Trim(),
                request.porc_responsabilidad,
                tipo = (request.tipo ?? string.Empty).Trim(),
                notas = (request.notas ?? string.Empty).Trim(),
                request.monto,
                request.tasa,
                request.plazo,
                request.cuota,
                request.fecha_formaliza,
                request.fecha_primer_pago,
                request.dia_de_pago,
                request.comision_admin,
                responsabilidad_base = (request.responsabilidad_base ?? "O").Trim(),
                comision_base = (request.comision_base ?? "O").Trim(),
                cod_oficina = (request.cod_oficina ?? string.Empty).Trim(),
                periocidad_pago = (request.periocidad_pago ?? string.Empty).Trim(),
                cod_divisa = (request.cod_divisa ?? string.Empty).Trim(),
                request.tipo_cambio,
                request.cod_linea,
                usuario = (request.usuario ?? string.Empty).Trim()
            };
        }

        private static string CR_APA_Operacion_SqlActualizarTodo()
        {
            return @"
UPDATE CRD_APA_OPERACIONES
SET
    PORC_RESPONSABILIDAD = @porc_responsabilidad,
    TIPO = @tipo,
    FECHA_PRIMER_PAGO = @fecha_primer_pago,
    NOTAS = @notas,
    FECHA_FORMALIZA = @fecha_formaliza,
    MONTO = @monto,
    TASA = @tasa,
    TASA_ORIGINAL = @tasa,
    PLAZO = @plazo,
    PLAZO_ORIGINAL = @plazo,
    CUOTA = @cuota,
    CUOTA_ORIGINAL = @cuota,
    DIA_DE_PAGO = @dia_de_pago,
    RESPONSABILIDAD_BASE = @responsabilidad_base,
    COMISION_BASE = @comision_base,
    COMISION_ADMIN = @comision_admin,
    COD_OFICINA = @cod_oficina,
    PERIOCIDAD_PAGO = @periocidad_pago,
    COD_DIVISA = @cod_divisa,
    TIPO_CAMBIO = @tipo_cambio,
    COD_LINEA = @cod_linea,
    MODIFICA_FECHA = GETDATE(),
    MODIFICA_USUARIO = @usuario
WHERE COD_ACREEDOR = @cod_acreedor
  AND OPERACION = @operacion;";
        }

        private static string CR_APA_Operacion_SqlActualizarParcial()
        {
            return @"
UPDATE CRD_APA_OPERACIONES
SET
    PORC_RESPONSABILIDAD = @porc_responsabilidad,
    NOTAS = @notas,
    FECHA_PRIMER_PAGO = @fecha_primer_pago,
    FECHA_FORMALIZA = @fecha_formaliza,
    PLAZO = @plazo,
    DIA_DE_PAGO = @dia_de_pago,
    RESPONSABILIDAD_BASE = @responsabilidad_base,
    COMISION_BASE = @comision_base,
    COMISION_ADMIN = @comision_admin,
    COD_OFICINA = @cod_oficina,
    PERIOCIDAD_PAGO = @periocidad_pago,
    COD_DIVISA = @cod_divisa,
    TIPO_CAMBIO = @tipo_cambio,
    COD_LINEA = @cod_linea,
    MODIFICA_FECHA = GETDATE(),
    MODIFICA_USUARIO = @usuario
WHERE COD_ACREEDOR = @cod_acreedor
  AND OPERACION = @operacion;";
        }
    }
}
