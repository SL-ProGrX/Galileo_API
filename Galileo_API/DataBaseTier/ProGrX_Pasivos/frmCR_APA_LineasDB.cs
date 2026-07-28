using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_Pasivos
{
    public class FrmCrApaLineasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMain;
        private const int Modulo = 14;

        public FrmCrApaLineasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mSecurityMain = new MSecurityMainDb(config);
        }

        /// <summary>Obtiene los catálogos requeridos por el mantenimiento de líneas APA.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Catálogos de acreedores, divisas, recursos y unidades.</returns>
        public ErrorDto<FrmCrApaLineaCatalogosDto> CR_APA_Lineas_Catalogos_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                var result = new FrmCrApaLineaCatalogosDto
                {
                    acreedores = conn.Query<FrmCrApaLineaCatalogoDto>(
                        "SELECT RTRIM(COD_ACREEDOR) idx, RTRIM(DESCRIPCION) itmx FROM CRD_APA_ACREEDORES WHERE ESTADO = 'A' ORDER BY DESCRIPCION").ToList(),
                    divisas = conn.Query<FrmCrApaLineaCatalogoDto>(
                        "SELECT RTRIM(COD_DIVISA) idx, RTRIM(DESCRIPCION) itmx FROM vSys_Divisas ORDER BY DESCRIPCION").ToList(),
                    recursos = conn.Query<FrmCrApaLineaCatalogoDto>(
                        "SELECT RTRIM(COD_GRUPO) idx, RTRIM(DESCRIPCION) itmx FROM CATALOGO_GRUPOS WHERE ESTADO = 1 ORDER BY DESCRIPCION").ToList(),
                    unidades = conn.Query<FrmCrApaLineaCatalogoDto>(
                        "SELECT RTRIM(cod_unidad) idx, RTRIM(descripcion) itmx FROM vCNTX_UNIDADES_LOCAL ORDER BY descripcion").ToList(),
                    fecha_servidor = conn.QuerySingle<DateTime>("SELECT dbo.MyGetdate()")
                };
                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaLineaCatalogosDto>(ex.Message);
            }
        }

        /// <summary>Obtiene centros de costo asociados a una unidad.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="cod_unidad">Código de unidad.</param>
        /// <returns>Lista de centros de costo.</returns>
        public ErrorDto<List<FrmCrApaLineaCatalogoDto>> CR_APA_Lineas_CentrosCosto_Obtener(int codEmpresa, string cod_unidad)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                const string sql = """
                    SELECT RTRIM(CC.cod_Centro_Costo) idx, RTRIM(CC.descripcion) itmx
                    FROM vCNTX_CENTRO_COSTO_LOCAL CC
                    INNER JOIN CntX_Unidades_CC CA ON CC.cod_contabilidad = CA.Cod_Contabilidad
                    WHERE CA.cod_unidad = @cod_unidad
                    ORDER BY CC.Descripcion
                    """;
                return DbHelper.CreateOkResponse(conn.Query<FrmCrApaLineaCatalogoDto>(
                    sql, new { cod_unidad = (cod_unidad ?? string.Empty).Trim() }).ToList());
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaLineaCatalogoDto>>(ex.Message);
            }
        }

        /// <summary>Consulta las líneas de crédito APA con los filtros del formulario legado.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtros de acreedor, estado y vencimiento.</param>
        /// <returns>Líneas encontradas.</returns>
        public ErrorDto<List<FrmCrApaLineaGridDto>> CR_APA_Lineas_Consultar(int codEmpresa, FrmCrApaLineaConsultaRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Acreedor", (request.cod_acreedor ?? string.Empty).Trim(), DbType.String);
                parameters.Add("@Estado", (request.estado ?? string.Empty).Trim(), DbType.String);
                parameters.Add("@Inicio", request.fecha_inicio, DbType.DateTime);
                parameters.Add("@Vence", request.fecha_vence, DbType.DateTime);
                var lineas = conn.Query(
                    "spCrd_APA_Acreedor_Lineas_Consulta", parameters,
                    commandType: CommandType.StoredProcedure)
                    .Select(MapearLineaConsulta)
                    .ToList();
                return DbHelper.CreateOkResponse(lineas);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaLineaGridDto>>(ex.Message);
            }
        }

        /// <summary>Convierte una fila dinámica del procedimiento legado al DTO de consulta.</summary>
        /// <param name="fila">Fila devuelta por el procedimiento almacenado.</param>
        /// <returns>DTO normalizado para la tabla de líneas.</returns>
        private static FrmCrApaLineaGridDto MapearLineaConsulta(dynamic fila)
        {
            IDictionary<string, object> valores = (IDictionary<string, object>)fila;
            return new FrmCrApaLineaGridDto
            {
                cod_linea = ValorEntero(valores, 0, "cod_linea"),
                cod_acreedor = ValorTexto(valores, 1, "cod_acreedor"),
                acreedor_desc = ValorTexto(valores, 2, "acreedor_desc"),
                cod_divisa = ValorTexto(valores, 3, "cod_divisa"),
                codigo = ValorTexto(valores, 4, "codigo"),
                descripcion = ValorTexto(valores, 5, "descripcion"),
                estado_desc = ValorTexto(valores, 6, "estado_desc"),
                revolutiva_desc = ValorTexto(valores, 7, "revolutiva_desc"),
                tipo_desc = ValorTexto(valores, 8, "tipo_desc"),
                fecha_inicio = ValorFecha(valores, 9, "fecha_inicio"),
                fecha_vence = ValorFecha(valores, 10, "fecha_vence"),
                monto_aprobado = ValorDecimal(valores, 11, "monto_aprobado"),
                tasa = ValorDecimal(valores, 12, "tasa"),
                plazo = ValorEntero(valores, 13, "plazo"),
                cuota_inicial = ValorDecimal(valores, 14, "cuota_inicial"),
                comision = ValorDecimal(valores, 15, "comision"),
                unidad_desc = ValorTexto(valores, 16, "unidad_desc"),
                centro_costo_desc = ValorTexto(valores, 17, "centro_costo_desc"),
                recurso_desc = ValorTexto(valores, 18, "recurso_desc"),
                notas = ValorTexto(valores, 19, "notas")
            };
        }

        /// <summary>Obtiene un valor por alias de columna o por su posición legado.</summary>
        /// <param name="fila">Fila con los valores obtenidos.</param>
        /// <param name="posicion">Posición alternativa de la columna.</param>
        /// <param name="alias">Nombres posibles de la columna.</param>
        /// <returns>Valor localizado o <see langword="null"/>.</returns>
        private static object? ValorFila(IDictionary<string, object> fila, int posicion, params string[] alias)
        {
            foreach (string nombre in alias)
            {
                var columna = fila.FirstOrDefault(item =>
                    string.Equals(item.Key, nombre, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(columna.Key))
                {
                    return columna.Value is DBNull ? null : columna.Value;
                }
            }

            object? valorPosicional = fila.Count > posicion ? fila.ElementAt(posicion).Value : null;
            return valorPosicional is DBNull ? null : valorPosicional;
        }

        /// <summary>Obtiene un texto normalizado desde una fila dinámica.</summary>
        /// <param name="fila">Fila con los valores obtenidos.</param>
        /// <param name="posicion">Posición alternativa de la columna.</param>
        /// <param name="alias">Nombres posibles de la columna.</param>
        /// <returns>Texto recortado o una cadena vacía.</returns>
        private static string ValorTexto(IDictionary<string, object> fila, int posicion, params string[] alias) =>
            Convert.ToString(ValorFila(fila, posicion, alias), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;

        /// <summary>Obtiene un entero desde una fila dinámica.</summary>
        /// <param name="fila">Fila con los valores obtenidos.</param>
        /// <param name="posicion">Posición alternativa de la columna.</param>
        /// <param name="alias">Nombres posibles de la columna.</param>
        /// <returns>Valor entero o cero.</returns>
        private static int ValorEntero(IDictionary<string, object> fila, int posicion, params string[] alias)
        {
            object? valor = ValorFila(fila, posicion, alias);
            return valor is null ? 0 : Convert.ToInt32(valor, CultureInfo.InvariantCulture);
        }

        /// <summary>Obtiene un decimal desde una fila dinámica.</summary>
        /// <param name="fila">Fila con los valores obtenidos.</param>
        /// <param name="posicion">Posición alternativa de la columna.</param>
        /// <param name="alias">Nombres posibles de la columna.</param>
        /// <returns>Valor decimal o cero.</returns>
        private static decimal ValorDecimal(IDictionary<string, object> fila, int posicion, params string[] alias)
        {
            object? valor = ValorFila(fila, posicion, alias);
            return valor is null ? 0 : Convert.ToDecimal(valor, CultureInfo.InvariantCulture);
        }

        /// <summary>Obtiene una fecha opcional desde una fila dinámica.</summary>
        /// <param name="fila">Fila con los valores obtenidos.</param>
        /// <param name="posicion">Posición alternativa de la columna.</param>
        /// <param name="alias">Nombres posibles de la columna.</param>
        /// <returns>Fecha localizada o <see langword="null"/>.</returns>
        private static DateTime? ValorFecha(IDictionary<string, object> fila, int posicion, params string[] alias)
        {
            object? valor = ValorFila(fila, posicion, alias);
            return valor is null ? null : Convert.ToDateTime(valor, CultureInfo.InvariantCulture);
        }

        /// <summary>Obtiene una línea APA para edición.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="cod_linea">Identificador de línea.</param>
        /// <returns>Datos de la línea.</returns>
        public ErrorDto<FrmCrApaLineaDatosDto> CR_APA_Lineas_Obtener(int codEmpresa, int cod_linea)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                var result = conn.QueryFirstOrDefault<FrmCrApaLineaDatosDto>(
                    "spCrd_APA_Acreedor_Linea_Load", new { LineaId = cod_linea },
                    commandType: CommandType.StoredProcedure);
                return DbHelper.CreateOkResponse(result ?? new FrmCrApaLineaDatosDto());
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaLineaDatosDto>(ex.Message);
            }
        }

        /// <summary>Registra o actualiza una línea APA mediante el contrato legado.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos completos de la línea.</param>
        /// <returns>Resultado funcional devuelto por el procedimiento.</returns>
        public ErrorDto<FrmCrApaLineaGuardarResultadoDto> CR_APA_Lineas_Guardar(int codEmpresa, FrmCrApaLineaGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                var p = new DynamicParameters();
                p.Add("@LineaId", request.cod_linea);
                p.Add("@Acreedor", request.cod_acreedor?.Trim());
                p.Add("@Codigo", request.codigo?.Trim());
                p.Add("@LineaDesc", request.descripcion?.Trim());
                p.Add("@Activa", request.activa ? 1 : 0);
                p.Add("@Prorrateo", request.prorrateo?.Trim());
                p.Add("@Revolutiva", request.linea_revolutiva ? 1 : 0);
                p.Add("@Inicio", request.fecha_inicio);
                p.Add("@Vence", request.fecha_vence);
                p.Add("@Monto", request.monto_aprobado);
                p.Add("@Tasa", request.tasa);
                p.Add("@Comision", request.comision);
                p.Add("@Plazo", request.plazo);
                p.Add("@CuotaInicial", request.cuota_inicial);
                p.Add("@Unidad", request.cod_unidad?.Trim());
                p.Add("@CentroCosto", request.cod_centro_costo?.Trim());
                p.Add("@RecursoCrd", request.cod_recurso?.Trim());
                p.Add("@Divisa", request.cod_divisa?.Trim());
                p.Add("@TipoCambio", request.tipo_cambio);
                p.Add("@Notas", request.notas?.Trim());
                p.Add("@Usuario", request.usuario?.Trim());
                var result = conn.QueryFirst<FrmCrApaLineaGuardarResultadoDto>(
                    "spCrd_APA_Acreedor_Linea_Add", p, commandType: CommandType.StoredProcedure);
                if (result.pass == 1)
                {
                    RegistrarBitacora(
                        codEmpresa,
                        request.usuario?.Trim() ?? string.Empty,
                        result.movimiento,
                        result.mensaje);
                }
                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaLineaGuardarResultadoDto>(ex.Message);
            }
        }

        /// <summary>Registra en la bitácora el movimiento confirmado por el procedimiento legado.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecutó el movimiento.</param>
        /// <param name="movimiento">Tipo de movimiento realizado.</param>
        /// <param name="detalle">Detalle funcional devuelto por el procedimiento.</param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _mSecurityMain.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = Modulo
            });
        }
    }
}
