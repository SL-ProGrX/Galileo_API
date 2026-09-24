using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmGenParametrosProDb
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 34; // vModulo del Form_Load VB6

        private const string MsgActualizado = "Parámetros actualizados satisfactoriamente.";
        private static readonly string[] TiposCambio = { "V", "C", "R" };

        public FrmGenParametrosProDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        // ==========================
        // Helpers
        // ==========================

        private static int ABit(int valor) => valor == 1 ? 1 : 0;

        private static string NormalizarTipoCambio(string? valor)
            => (valor ?? string.Empty).Trim().ToUpperInvariant();

        private static bool SonTiposCambioValidos(params string?[] valores)
            => valores.All(v => TiposCambio.Contains(NormalizarTipoCambio(v)));

        private static ErrorDto ErrorDatos(string mensaje) => DbHelper.ErrorResponse(mensaje, -2);

        private void LogBitacora(int empresaId, string usuario, string detalle)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = "Actualiza - WEB",
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Ejecuta el update de una sección de pv_parametros_mod y registra la bitácora.
        /// La tabla es de una sola fila, por eso el update no lleva WHERE (igual que VB6).
        /// </summary>
        private ErrorDto ActualizarSeccion(int CodEmpresa, string usuario, string sql, object parametros, string detalleBitacora)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Execute(sql, parametros);
                LogBitacora(CodEmpresa, (usuario ?? string.Empty).Trim(), detalleBitacora);
                return DbHelper.OkResponse(MsgActualizado);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        // ==========================
        // Públicos
        // ==========================

        /// <summary>
        /// Inserta la fila por defecto de pv_parametros_mod si la tabla está vacía.
        /// Equivale a sbInicializa de frmGenParametrosPro.
        /// </summary>
        public ErrorDto Gen_ParametrosPro_Inicializar(int CodEmpresa)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM pv_parametros_mod)
    INSERT pv_parametros_mod
        (chk_factura_min, chk_descuento_bifiv, chk_costo_ultcomp, chk_costo_cero,
         chk_modo_asiento, aplica_iv_sobre,
         cxp_tc_nc, cxp_tc_nd, cxp_tc_pago,
         inv_tc_entrada, inv_tc_salida, inv_tc_traslado, inv_tc_compra,
         pos_tc_factura, pos_tc_recibo,
         pos_rei_user, pos_rei_clave, pos_cp_user, pos_cp_clave,
         tc_compra, tc_venta, tc_fecha, tc_usuario)
    VALUES
        (0, 1, 0, 1,
         1, 'SB',
         'C', 'V', 'C',
         'C', 'C', 'C', 'C',
         'V', 'C',
         '', '', '', '',
         0, 0, dbo.MyGetdate(), 'InI');";

            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql);
        }

        /// <summary>
        /// Obtiene los parámetros de los módulos comerciales (sin claves POS).
        /// Equivale a sbCargaParGen de frmGenParametrosPro.
        /// </summary>
        public ErrorDto<GenParametrosProData?> Gen_ParametrosPro_Obtener(int CodEmpresa)
        {
            const string sql = @"
SELECT TOP 1
    CAST(ISNULL(chk_factura_min, 0) AS int)     AS chk_factura_min,
    CAST(ISNULL(chk_descuento_bifiv, 0) AS int) AS chk_descuento_bifiv,
    CAST(ISNULL(chk_costo_ultcomp, 0) AS int)   AS chk_costo_ultcomp,
    CAST(ISNULL(chk_costo_cero, 0) AS int)      AS chk_costo_cero,
    CAST(ISNULL(chk_modo_asiento, 0) AS int)    AS chk_modo_asiento,
    UPPER(LTRIM(RTRIM(ISNULL(aplica_iv_sobre, 'SB')))) AS aplica_iv_sobre,
    UPPER(LTRIM(RTRIM(ISNULL(cxp_tc_nc, ''))))       AS cxp_tc_nc,
    UPPER(LTRIM(RTRIM(ISNULL(cxp_tc_nd, ''))))       AS cxp_tc_nd,
    UPPER(LTRIM(RTRIM(ISNULL(cxp_tc_pago, ''))))     AS cxp_tc_pago,
    UPPER(LTRIM(RTRIM(ISNULL(inv_tc_entrada, ''))))  AS inv_tc_entrada,
    UPPER(LTRIM(RTRIM(ISNULL(inv_tc_salida, ''))))   AS inv_tc_salida,
    UPPER(LTRIM(RTRIM(ISNULL(inv_tc_traslado, '')))) AS inv_tc_traslado,
    UPPER(LTRIM(RTRIM(ISNULL(inv_tc_compra, ''))))   AS inv_tc_compra,
    UPPER(LTRIM(RTRIM(ISNULL(pos_tc_factura, ''))))  AS pos_tc_factura,
    UPPER(LTRIM(RTRIM(ISNULL(pos_tc_recibo, ''))))   AS pos_tc_recibo,
    LTRIM(RTRIM(ISNULL(pos_rei_user, '')))           AS pos_rei_user,
    LTRIM(RTRIM(ISNULL(pos_cp_user, '')))            AS pos_cp_user,
    ISNULL(tc_compra, 0) AS tc_compra,
    ISNULL(tc_venta, 0)  AS tc_venta
FROM pv_parametros_mod;";

            return DbHelper.ExecuteSingleQuery<GenParametrosProData>(_portalDB, CodEmpresa, sql);
        }

        /// <summary>
        /// Actualiza el tab General. Equivale a cmdCambiaParGen_Click
        /// (corrige el cruce chk_costo_ultcomp / chk_costo_cero del VB6).
        /// </summary>
        public ErrorDto Gen_ParametrosProGeneral_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
        {
            if (parametros == null)
                return ErrorDatos("Los parámetros son requeridos.");

            const string sql = @"
UPDATE pv_parametros_mod SET
      chk_factura_min     = @chk_factura_min
    , chk_descuento_bifiv = @chk_descuento_bifiv
    , chk_costo_ultcomp   = @chk_costo_ultcomp
    , chk_costo_cero      = @chk_costo_cero
    , chk_modo_asiento    = @chk_modo_asiento
    , aplica_iv_sobre     = @aplica_iv_sobre;";

            var valores = new
            {
                chk_factura_min = ABit(parametros.chk_factura_min),
                chk_descuento_bifiv = ABit(parametros.chk_descuento_bifiv),
                chk_costo_ultcomp = ABit(parametros.chk_costo_ultcomp),
                chk_costo_cero = ABit(parametros.chk_costo_cero),
                chk_modo_asiento = ABit(parametros.chk_modo_asiento),
                aplica_iv_sobre = NormalizarTipoCambio(parametros.aplica_iv_sobre) == "SC" ? "SC" : "SB"
            };

            return ActualizarSeccion(CodEmpresa, usuario, sql, valores, "Parámetros Generales del SIF.C");
        }

        /// <summary>
        /// Actualiza el tab CxP. Equivale a cmdCambiaParCxP_Click
        /// (corrige el cruce NC / ND del VB6).
        /// </summary>
        public ErrorDto Gen_ParametrosProCxP_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
        {
            if (parametros == null)
                return ErrorDatos("Los parámetros son requeridos.");

            if (!SonTiposCambioValidos(parametros.cxp_tc_nc, parametros.cxp_tc_nd, parametros.cxp_tc_pago))
                return ErrorDatos("Debe seleccionar un tipo de cambio válido en todos los campos.");

            const string sql = @"
UPDATE pv_parametros_mod SET
      cxp_tc_nc   = @cxp_tc_nc
    , cxp_tc_nd   = @cxp_tc_nd
    , cxp_tc_pago = @cxp_tc_pago;";

            var valores = new
            {
                cxp_tc_nc = NormalizarTipoCambio(parametros.cxp_tc_nc),
                cxp_tc_nd = NormalizarTipoCambio(parametros.cxp_tc_nd),
                cxp_tc_pago = NormalizarTipoCambio(parametros.cxp_tc_pago)
            };

            return ActualizarSeccion(CodEmpresa, usuario, sql, valores, "Parámetros Generales de CxP");
        }

        /// <summary>
        /// Actualiza el tab Inv / Compras. Equivale a cmdCambiaParInv_Click.
        /// </summary>
        public ErrorDto Gen_ParametrosProInv_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
        {
            if (parametros == null)
                return ErrorDatos("Los parámetros son requeridos.");

            if (!SonTiposCambioValidos(parametros.inv_tc_entrada, parametros.inv_tc_salida,
                                       parametros.inv_tc_traslado, parametros.inv_tc_compra))
                return ErrorDatos("Debe seleccionar un tipo de cambio válido en todos los campos.");

            const string sql = @"
UPDATE pv_parametros_mod SET
      inv_tc_entrada  = @inv_tc_entrada
    , inv_tc_salida   = @inv_tc_salida
    , inv_tc_traslado = @inv_tc_traslado
    , inv_tc_compra   = @inv_tc_compra;";

            var valores = new
            {
                inv_tc_entrada = NormalizarTipoCambio(parametros.inv_tc_entrada),
                inv_tc_salida = NormalizarTipoCambio(parametros.inv_tc_salida),
                inv_tc_traslado = NormalizarTipoCambio(parametros.inv_tc_traslado),
                inv_tc_compra = NormalizarTipoCambio(parametros.inv_tc_compra)
            };

            return ActualizarSeccion(CodEmpresa, usuario, sql, valores, "Parámetros Generales de Inventario/Compras");
        }

        /// <summary>
        /// Actualiza los tipos de cambio del tab POS. Equivale a cmdCambiaParPos_Click
        /// (corrige la comilla faltante del SQL VB6).
        /// </summary>
        public ErrorDto Gen_ParametrosProPos_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
        {
            if (parametros == null)
                return ErrorDatos("Los parámetros son requeridos.");

            if (!SonTiposCambioValidos(parametros.pos_tc_factura, parametros.pos_tc_recibo))
                return ErrorDatos("Debe seleccionar un tipo de cambio válido en todos los campos.");

            const string sql = @"
UPDATE pv_parametros_mod SET
      pos_tc_factura = @pos_tc_factura
    , pos_tc_recibo  = @pos_tc_recibo;";

            var valores = new
            {
                pos_tc_factura = NormalizarTipoCambio(parametros.pos_tc_factura),
                pos_tc_recibo = NormalizarTipoCambio(parametros.pos_tc_recibo)
            };

            return ActualizarSeccion(CodEmpresa, usuario, sql, valores, "Parámetros Generales de POS/TC:Fac/Rec");
        }
    }
}
