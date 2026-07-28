using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;
using System.Data;
using System.Data.Common;

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
                var lineas = conn.Query<FrmCrApaLineaGridDto>(
                    "spCrd_APA_Acreedor_Lineas_Consulta", parameters,
                    commandType: CommandType.StoredProcedure)
                    .ToList();
                return DbHelper.CreateOkResponse(lineas);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaLineaGridDto>>(ex.Message);
            }
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
                return DbHelper.CreateOkResponse(result ?? new FrmCrApaLineaDatosDto
                {
                    cod_linea = 0,
                    tipo_cambio = 0,
                    activa = false,
                    linea_revolutiva = false,
                    fecha_inicio = DateTime.MinValue,
                    fecha_vence = DateTime.MinValue,
                    monto_aprobado = 0,
                    tasa = 0,
                    comision = 0,
                    cuota_inicial = 0,
                    plazo = 0
                });
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
