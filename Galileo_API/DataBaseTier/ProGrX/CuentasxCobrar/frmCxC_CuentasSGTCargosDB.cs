using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasCargosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 31;
        private const string MENSAJEOPERACION = "La operación indicada no existe.";
        public FrmCxCCuentasCargosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene información base de la operación y la lista de cargos registrados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CxCCuentasCargoOperacionDto> CxC_Cuentas_Cargos_Operacion_Obtener(int CodEmpresa, int operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var contexto = ObtenerContextoOperacion(conn, operacion);
                if (contexto == null)
                    return DbHelper.CreateErrorResponse<CxCCuentasCargoOperacionDto>(MENSAJEOPERACION, -2);

                var lista = ObtenerCargosOperacion(conn, operacion);

                contexto.lista = lista;
                contexto.total = lista.Count;

                return DbHelper.CreateOkResponse(contexto);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasCargoOperacionDto>(ex.Message, -1);
            }
        }

        /// <summary>
        /// Exporta la lista completa de cargos registrados de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CxCCuentasCargosListaResult> CxC_Cuentas_Cargos_Operacion_Export(int CodEmpresa, int operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!OperacionExiste(conn, operacion))
                    return DbHelper.CreateErrorResponse<CxCCuentasCargosListaResult>(MENSAJEOPERACION, -2);

                var lista = ObtenerCargosOperacion(conn, operacion);

                return DbHelper.CreateOkResponse(new CxCCuentasCargosListaResult
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasCargosListaResult>(ex.Message, -1);
            }
        }

        /// <summary>
        /// Obtiene cargos disponibles para F4, excluyendo los ya registrados en la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Cuentas_Cargos_Disponibles_Obtener(int CodEmpresa, int operacion, string? filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!OperacionExiste(conn, operacion))
                    return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(MENSAJEOPERACION, -2);

                var texto = (filtro ?? string.Empty).Trim();
                var like = texto.Length > 0 ? $"%{texto}%" : string.Empty;

                const string sql = @"
                    SELECT
                        RTRIM(c.cod_cargo) AS item,
                        RTRIM(c.descripcion) AS descripcion
                    FROM CxC_Cargos c
                    WHERE c.activo = 1
                      AND c.tipo = 'C'
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM CxC_Cuentas_Rebajos_Cargos r
                          WHERE r.operacion = @operacion
                            AND r.cod_cargo = c.cod_cargo
                      )
                      AND (
                            @texto = ''
                            OR c.cod_cargo LIKE @like
                            OR c.descripcion LIKE @like
                          )
                    ORDER BY c.cod_cargo;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    operacion,
                    texto,
                    like
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1);
            }
        }

        /// <summary>
        /// Navega al siguiente o anterior cargo disponible para la operación.
        /// scrollCode: 1=siguiente, 2=anterior.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="scrollCode"></param>
        /// <param name="cargoActual"></param>
        /// <returns></returns>
        public ErrorDto<CxCCuentasCargoDisponibleDto> CxC_Cuentas_Cargos_Scroll_Obtener(int CodEmpresa, int operacion, int scrollCode, string? cargoActual)
        {
            var actual = (cargoActual ?? string.Empty).Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!OperacionExiste(conn, operacion))
                    return DbHelper.CreateErrorResponse<CxCCuentasCargoDisponibleDto>(MENSAJEOPERACION, -2);

                string? cargoObjetivo;

                if (string.IsNullOrWhiteSpace(actual))
                {
                    const string sqlPrimero = @"
                    SELECT TOP 1 c.cod_cargo
                    FROM CxC_Cargos c
                    WHERE c.activo = 1
                      AND c.tipo = 'C'
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM CxC_Cuentas_Rebajos_Cargos r
                          WHERE r.operacion = @operacion
                            AND r.cod_cargo = c.cod_cargo
                      )
                    ORDER BY
                        CASE WHEN @scroll = 1 THEN c.cod_cargo END ASC,
                        CASE WHEN @scroll <> 1 THEN c.cod_cargo END DESC;";

                    cargoObjetivo = conn.QueryFirstOrDefault<string>(sqlPrimero, new
                    {
                        operacion,
                        scroll = scrollCode
                    });
                }
                else
                {
                    const string sql = @"
SELECT TOP 1 c.cod_cargo
FROM CxC_Cargos c
WHERE c.activo = 1
  AND c.tipo = 'C'
  AND NOT EXISTS
  (
      SELECT 1
      FROM CxC_Cuentas_Rebajos_Cargos r
      WHERE r.operacion = @operacion
        AND r.cod_cargo = c.cod_cargo
  )
  AND (
        (@scroll = 1 AND c.cod_cargo > @actual)
        OR (@scroll <> 1 AND c.cod_cargo < @actual)
      )
ORDER BY
    CASE WHEN @scroll = 1 THEN c.cod_cargo END ASC,
    CASE WHEN @scroll <> 1 THEN c.cod_cargo END DESC;";

                    cargoObjetivo = conn.QueryFirstOrDefault<string>(sql, new
                    {
                        operacion,
                        scroll = scrollCode,
                        actual
                    });

                    if (string.IsNullOrWhiteSpace(cargoObjetivo))
                        cargoObjetivo = actual;
                }

                cargoObjetivo = (cargoObjetivo ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(cargoObjetivo))
                    return DbHelper.CreateErrorResponse<CxCCuentasCargoDisponibleDto>("No hay cargos disponibles para navegar.", -2);

                const string sqlDetalle = @"
SELECT
    RTRIM(cod_cargo) AS cod_cargo,
    RTRIM(descripcion) AS descripcion
FROM CxC_Cargos
WHERE cod_cargo = @codCargo;";

                var item = conn.QueryFirstOrDefault<CxCCuentasCargoDisponibleDto>(sqlDetalle, new
                {
                    codCargo = cargoObjetivo
                });

                if (item == null)
                    return DbHelper.CreateErrorResponse<CxCCuentasCargoDisponibleDto>("No se encontró el cargo solicitado.", -2);

                return DbHelper.CreateOkResponse(item);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasCargoDisponibleDto>(ex.Message, -1);
            }
        }

        /// <summary>
        /// Inserta o actualiza un cargo de rebajo de la operación según la propiedad isNew.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cargo"></param>
        /// <returns></returns>
        public ErrorDto CxC_Cuentas_Cargos_Guardar(int CodEmpresa, string usuario, CxCCuentasCargoData cargo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (cargo == null)
                    return DbHelper.ErrorResponse("El cargo es requerido.", -2);

                var validacion = ValidarCargoGuardar(conn, cargo);
                if (validacion.Code != 0)
                    return validacion;

                var operacion = cargo.operacion ?? 0;
                var isNew = cargo.isNew ?? false;
                var existe = CargoOperacionExiste(conn, operacion, cargo.cod_cargo);
                if (cargo.isNew == true)
                {
                    if (existe)
                        return DbHelper.ErrorResponse($"El cargo {cargo.cod_cargo.Trim()} ya existe en la operación {cargo.operacion}.", -2);

                    return CxC_Cuentas_Cargos_Insertar(CodEmpresa, usuario, cargo);
                }

                if (!existe)
                    return DbHelper.ErrorResponse($"El cargo {cargo.cod_cargo.Trim()} no existe en la operación {cargo.operacion}.", -2);

                return CxC_Cuentas_Cargos_Actualizar(CodEmpresa, usuario, cargo);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un cargo registrado de la operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="operacion"></param>
        /// <param name="codCargo"></param>
        /// <returns></returns>
        public ErrorDto CxC_Cuentas_Cargos_Eliminar(int CodEmpresa, string usuario, int operacion, string codCargo)
        {
            var cargo = (codCargo ?? string.Empty).Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (operacion <= 0)
                    return DbHelper.ErrorResponse("La operación es requerida.", -2);

                if (string.IsNullOrWhiteSpace(cargo))
                    return DbHelper.ErrorResponse("El código de cargo es requerido.", -2);

                if (!OperacionExiste(conn, operacion))
                    return DbHelper.ErrorResponse(MENSAJEOPERACION, -2);

                if (!CargoOperacionExiste(conn, operacion, cargo))
                    return DbHelper.ErrorResponse($"El cargo {cargo} no existe en la operación {operacion}.", -2);

                const string sql = @"
                    DELETE FROM CxC_Cuentas_Rebajos_Cargos
                    WHERE operacion = @operacion
                      AND cod_cargo = @codCargo;";

                var afectados = conn.Execute(sql, new
                {
                    operacion,
                    codCargo = cargo
                });

                if (afectados <= 0)
                    return DbHelper.ErrorResponse("No se pudo eliminar el cargo indicado.", -2);

                var bitacora = CrearBitacora(CodEmpresa, usuario, $"Elimina - Cargo {cargo} en operación CxC #{operacion}");
                _ = Bitacora(bitacora);

                return DbHelper.OkResponse("Cargo eliminado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        private ErrorDto CxC_Cuentas_Cargos_Insertar(int CodEmpresa, string usuario, CxCCuentasCargoData cargo)
            => UpsertCargo(CodEmpresa, usuario, cargo, isInsert: true);
        private ErrorDto CxC_Cuentas_Cargos_Actualizar(int CodEmpresa, string usuario, CxCCuentasCargoData cargo)
            => UpsertCargo(CodEmpresa, usuario, cargo, isInsert: false);
        private ErrorDto UpsertCargo(int CodEmpresa, string usuario, CxCCuentasCargoData cargo, bool isInsert)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var operacion = cargo.operacion ?? 0;
                var valor = cargo.valor ?? 0m;

                var contexto = ObtenerContextoOperacion(conn, operacion);
                if (contexto == null)
                    return DbHelper.ErrorResponse(MENSAJEOPERACION, -2);

                var codCargo = cargo.cod_cargo.Trim();
                var tipo = ObtenerTipoNormalizado(cargo.tipo);
                var detalle = (cargo.detalle ?? string.Empty).Trim();
                var montoCalculado = CalcularMontoCargo(contexto.monto_operacion, tipo, valor);

                if (isInsert)
                {
                    const string sqlInsert = @"
                INSERT INTO CxC_Cuentas_Rebajos_Cargos
                (
                    cod_cargo,
                    operacion,
                    tipo,
                    monto,
                    valor,
                    modifica,
                    detalle,
                    registro_usuario,
                    registro_fecha
                )
                VALUES
                (
                    @codCargo,
                    @operacion,
                    @tipo,
                    @monto,
                    @valor,
                    1,
                    @detalle,
                    @usuario,
                    dbo.MyGetdate()
                );";

                    conn.Execute(sqlInsert, new
                    {
                        codCargo,
                        operacion,
                        tipo,
                        monto = montoCalculado,
                        valor,
                        detalle,
                        usuario
                    });

                    var bitacoraInsert = CrearBitacora(CodEmpresa, usuario, $"Registra - Cargo {codCargo} en operación CxC #{operacion}");
                    _ = Bitacora(bitacoraInsert);

                    return DbHelper.OkResponse("Cargo registrado satisfactoriamente.");
                }

                const string sqlUpdate = @"
            UPDATE CxC_Cuentas_Rebajos_Cargos
            SET tipo = @tipo,
                monto = @monto,
                valor = @valor,
                detalle = @detalle,
                registro_usuario = @usuario,
                registro_fecha = dbo.MyGetdate()
            WHERE operacion = @operacion
              AND cod_cargo = @codCargo;";

                var rows = conn.Execute(sqlUpdate, new
                {
                    tipo,
                    monto = montoCalculado,
                    valor,
                    detalle,
                    usuario,
                    operacion,
                    codCargo
                });

                if (rows <= 0)
                    return DbHelper.ErrorResponse("No se pudo actualizar el cargo indicado.", -2);

                var bitacoraUpdate = CrearBitacora(CodEmpresa, usuario, $"Modifica - Cargo {codCargo} en operación CxC #{operacion}");
                _ = Bitacora(bitacoraUpdate);

                return DbHelper.OkResponse("Cargo actualizado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        private static string ObtenerTipoNormalizado(string? tipo)
        {
            var tipoTrim = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            return tipoTrim.StartsWith('P') ? "P" : "M";
        }
        private static decimal CalcularMontoCargo(decimal montoOperacion, string tipo, decimal valor)
        {
            return tipo == "P"
                ? Math.Round(montoOperacion * valor / 100m, 2, MidpointRounding.AwayFromZero)
                : Math.Round(valor, 2, MidpointRounding.AwayFromZero);
        }
        private static ErrorDto ValidarCargoGuardar(SqlConnection conn, CxCCuentasCargoData cargo)
        {
            var operacion = cargo.operacion ?? 0;
            var valor = cargo.valor ?? 0m;
            var codCargo = (cargo.cod_cargo ?? string.Empty).Trim();
            var tipo = (cargo.tipo ?? string.Empty).Trim();

            if (operacion <= 0)
                return DbHelper.ErrorResponse("La operación es requerida.", -2);

            if (string.IsNullOrWhiteSpace(codCargo))
                return DbHelper.ErrorResponse("El código de cargo es requerido.", -2);

            if (string.IsNullOrWhiteSpace(tipo))
                return DbHelper.ErrorResponse("El tipo de cargo es requerido.", -2);

            if (valor < 0)
                return DbHelper.ErrorResponse("El valor del cargo no puede ser negativo.", -2);

            if (!OperacionExiste(conn, operacion))
                return DbHelper.ErrorResponse(MENSAJEOPERACION, -2);

            if (!CargoCatalogoValido(conn, codCargo))
                return DbHelper.ErrorResponse($"El cargo {codCargo} no existe, no está activo o no es de tipo 'C'.", -2);

            return DbHelper.OkResponse(string.Empty);
        }
        private static bool OperacionExiste(SqlConnection conn, int operacion)
        {
            const string sql = @"
SELECT ISNULL(COUNT(*), 0)
FROM CxC_Cuentas
WHERE operacion = @operacion;";

            return conn.QueryFirstOrDefault<int>(sql, new { operacion }) > 0;
        }
        private static bool CargoCatalogoValido(SqlConnection conn, string codCargo)
        {
            const string sql = @"
SELECT ISNULL(COUNT(*), 0)
FROM CxC_Cargos
WHERE cod_cargo = @codCargo
  AND activo = 1
  AND tipo = 'C';";

            return conn.QueryFirstOrDefault<int>(sql, new { codCargo }) > 0;
        }
        private static bool CargoOperacionExiste(SqlConnection conn, int operacion, string codCargo)
        {
            const string sql = @"
SELECT ISNULL(COUNT(*), 0)
FROM CxC_Cuentas_Rebajos_Cargos
WHERE operacion = @operacion
  AND cod_cargo = @codCargo;";

            return conn.QueryFirstOrDefault<int>(sql, new
            {
                operacion,
                codCargo
            }) > 0;
        }
        private static CxCCuentasCargoOperacionDto? ObtenerContextoOperacion(SqlConnection conn, int operacion)
        {
            const string sql = @"
SELECT
    c.operacion,
    RTRIM(ISNULL(c.cedula, '')) AS cedula,
    CAST(ISNULL(c.monto, 0) AS decimal(16,2)) AS monto_operacion,
    CAST(ISNULL(dbo.fxCxC_CuentaRebajos(c.operacion, 'TOT'), 0) AS decimal(16,2)) AS rebajos_totales,
    CAST(ISNULL(dbo.fxCxC_CuentaIngresos(c.operacion), 0) AS decimal(16,2)) AS ingresos_totales
FROM CxC_Cuentas c
WHERE c.operacion = @operacion;";

            return conn.QueryFirstOrDefault<CxCCuentasCargoOperacionDto>(sql, new { operacion });
        }
        private static List<CxCCuentasCargoData> ObtenerCargosOperacion(SqlConnection conn, int operacion)
        {
            const string sql = @"
SELECT
    r.operacion,
    RTRIM(r.cod_cargo) AS cod_cargo,
    RTRIM(ISNULL(c.descripcion, '')) AS descripcion,
    CAST(ISNULL(r.monto, 0) AS decimal(12,2)) AS monto,
    CASE WHEN ISNULL(r.tipo, 'M') = 'P' THEN 'Porcentual' ELSE 'Monto' END AS tipo,
    CAST(ISNULL(r.valor, 0) AS decimal(12,4)) AS valor,
    CAST(ISNULL(r.modifica, 0) AS smallint) AS modifica,
    RTRIM(ISNULL(r.detalle, '')) AS detalle,
    RTRIM(ISNULL(r.registro_usuario, '')) AS registro_usuario,
    r.registro_fecha,
    CAST(0 AS bit) AS isNew
FROM CxC_Cuentas_Rebajos_Cargos r
INNER JOIN CxC_Cargos c
    ON c.cod_cargo = r.cod_cargo
WHERE r.operacion = @operacion
ORDER BY r.cod_cargo;";

            return conn.Query<CxCCuentasCargoData>(sql, new { operacion }).ToList();
        }
        private BitacoraInsertarDto CrearBitacora(int codEmpresa, string usuario, string detalle)
        {
            return new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).Trim(),
                Movimiento = "Modifica - WEB",
                DetalleMovimiento = detalle,
                Modulo = vModulo
            };
        }
    }
}