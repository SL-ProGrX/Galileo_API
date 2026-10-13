using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using static Galileo_API.Models.ProGrX_Polizas.FrmCrPolizaProcRecepcionModels;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    /// <summary>
    /// Pólizas: Recepción y Facturación (VB6: frmCR_Poliza_Proc_Recepcion).
    /// Archivo principal: catálogos, consultas y helpers compartidos.
    /// Carga de archivo: frmCR_Poliza_Proc_RecepcionDB.carga.cs
    /// Eliminar y generar factura: frmCR_Poliza_Proc_RecepcionDB.factura.cs
    /// </summary>
    public partial class FrmCrPolizaProcRecepcionDB
    {
        private const int ModuloPolizas = 11;

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCrPolizaProcRecepcionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        #region Catalogos

        /// <summary>
        /// Lista las pólizas facturables (VB6: cboPoliza, spPoliza_Facturables_Lista).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_Polizas_Listar(int codEmpresa)
        {
            // El SP devuelve las columnas itmX / IdX (VB6: sbCbo_Llena_New), no item / descripcion.
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<PolizaFacturableItem>("exec dbo.spPoliza_Facturables_Lista")
                    .Select(x => new DropDownListaGenericaModel
                    {
                        item = x.idx,
                        descripcion = x.itmx ?? string.Empty
                    })
                    .ToList());
        }

        /// <summary>
        /// Fila del SP spPoliza_Facturables_Lista (itmX = texto, IdX = código de póliza).
        /// </summary>
        private sealed class PolizaFacturableItem
        {
            public string? itmx { get; set; }
            public object? idx { get; set; }
        }

        /// <summary>
        /// Lista las unidades activas de la contabilidad (VB6: cboUnidad).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_Unidades_Listar(int codEmpresa, int codContabilidad)
        {
            const string query = @"
                SELECT RTRIM(cod_unidad) AS item,
                       RTRIM(descripcion) AS descripcion
                FROM CntX_unidades
                WHERE cod_contabilidad = @codContabilidad AND activa = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codContabilidad });
        }

        /// <summary>
        /// Lista los centros de costo activos asociados a la unidad (VB6: cboUnidad_Click, cboCentroCosto).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_CentrosCosto_Listar(int codEmpresa, int codContabilidad, string codUnidad)
        {
            const string query = @"
                SELECT RTRIM(COD_CENTRO_COSTO) AS item,
                       RTRIM(descripcion) AS descripcion
                FROM CNTX_CENTRO_COSTOS
                WHERE COD_CONTABILIDAD = @codContabilidad
                  AND ACTIVO = 1
                  AND COD_CENTRO_COSTO IN (
                      SELECT COD_CENTRO_COSTO
                      FROM CNTX_UNIDADES_CC
                      WHERE COD_CONTABILIDAD = @codContabilidad
                        AND COD_UNIDAD = @codUnidad)";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codContabilidad, codUnidad });
        }

        /// <summary>
        /// Lista las divisas de la contabilidad, la divisa local primero (VB6: cboDivisa).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_Divisas_Listar(int codEmpresa, int codContabilidad)
        {
            const string query = @"
                SELECT RTRIM(cod_divisa) AS item,
                       RTRIM(descripcion) AS descripcion
                FROM CntX_Divisas
                WHERE cod_contabilidad = @codContabilidad
                ORDER BY divisa_local DESC, cod_divisa";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene la divisa local de la contabilidad (VB6: vDivisa / vDivisaLocal en Form_Load).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<DropDownListaGenericaModel?> Crd_PolizaProcRecepcion_DivisaLocal_Obtener(int codEmpresa, int codContabilidad)
        {
            const string query = @"
                SELECT RTRIM(cod_divisa) AS item,
                       RTRIM(descripcion) AS descripcion
                FROM CntX_Divisas
                WHERE cod_contabilidad = @codContabilidad AND Divisa_Local = 1";
            return DbHelper.ExecuteSingleQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, null, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene el tipo de cambio de venta del día para la divisa (VB6: cboDivisa_Click, fxCntXTipoCambio).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="divisa"></param>
        /// <returns></returns>
        public ErrorDto<decimal> Crd_PolizaProcRecepcion_TipoCambio_Obtener(int codEmpresa, int codContabilidad, string divisa)
        {
            const string sql = "select dbo.fxCntXTipoCambio(@codContabilidad, @divisa, dbo.MyGetdate(), 'V')";
            return DbHelper.ExecuteSingleQuery<decimal>(_portalDb, codEmpresa, sql, 0m, new { codContabilidad, divisa });
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Datos de la póliza seleccionada: proveedor, aseguradora, factura, unidad, centro de costo y corte
        /// (VB6: cboPoliza_Click, spPoliza_Poliza_Datos).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPoliza"></param>
        /// <returns></returns>
        public ErrorDto<PolizaDatos?> Crd_PolizaProcRecepcion_Poliza_Datos_Obtener(int codEmpresa, string codPoliza)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<PolizaDatos>(
                    "exec dbo.spPoliza_Poliza_Datos @poliza",
                    new { poliza = codPoliza }));
        }

        /// <summary>
        /// Consulta si el corte ya tiene información cargada (VB6: dtpCorte_Change, spPoliza_Aseguradora_Corte_Consulta).
        /// Devuelve null cuando no hay corte cargado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="corte"></param>
        /// <param name="codPoliza"></param>
        /// <returns></returns>
        public ErrorDto<PolizaAseguradoraCorteConsulta?> Crd_PolizaProcRecepcion_Corte_Consultar(int codEmpresa, DateTime corte, string codPoliza)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<PolizaAseguradoraCorteConsulta>(
                    "exec dbo.spPoliza_Aseguradora_Corte_Consulta @corte, @poliza",
                    new { corte = corte.Date, poliza = codPoliza }));
        }

        /// <summary>
        /// Detalle del corte (VB6: dtpCorte_Change, spPoliza_Aseguradora_Corte_Detalle_Consulta).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="corte"></param>
        /// <param name="codPoliza"></param>
        /// <param name="idFactura"></param>
        /// <returns></returns>
        public ErrorDto<List<PolizaAseguradoraCorteDetalleConsulta>> Crd_PolizaProcRecepcion_Corte_Detalle_Consultar(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<PolizaAseguradoraCorteDetalleConsulta>(
                    "exec dbo.spPoliza_Aseguradora_Corte_Detalle_Consulta @corte, @poliza, @factura",
                    new { corte = corte.Date, poliza = codPoliza, factura = idFactura }).AsList());
        }

        /// <summary>
        /// Valida si el corte puede modificarse (VB6: spPoliza_Aseguradora_Corte_Valida, Pass = 0 es rechazo).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="corte"></param>
        /// <param name="codPoliza"></param>
        /// <param name="idFactura"></param>
        /// <returns></returns>
        public ErrorDto<PolizaAseguradoraCorte?> Crd_PolizaProcRecepcion_Corte_Validar(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
        {
            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                return ResultadoSp(SpCorteValida(conn, corte, codPoliza, idFactura));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaAseguradoraCorte?>(ex.Message);
            }
        }

        #endregion

        #region Helpers compartidos

        /// <summary>
        /// Convierte el resultado Pass/Mensaje de un SP en ErrorDto (Pass = 0 se devuelve como error con el mensaje).
        /// </summary>
        private static ErrorDto<PolizaAseguradoraCorte?> ResultadoSp(PolizaAseguradoraCorte? resultado)
        {
            if (resultado is null)
            {
                return DbHelper.CreateErrorResponse<PolizaAseguradoraCorte?>("El proceso no devolvió resultado.");
            }

            return resultado.pass == 0
                ? DbHelper.CreateErrorResponse<PolizaAseguradoraCorte?>(resultado.mensaje, -1, resultado)
                : DbHelper.CreateOkResponse<PolizaAseguradoraCorte?>(resultado);
        }

        /// <summary>
        /// Ejecuta spPoliza_Aseguradora_Corte_Valida con parametros posicionales (igual que VB6).
        /// </summary>
        private static PolizaAseguradoraCorte? SpCorteValida(SqlConnection conn, DateTime corte, string codPoliza, int idFactura)
        {
            return conn.QueryFirstOrDefault<PolizaAseguradoraCorte>(
                "exec dbo.spPoliza_Aseguradora_Corte_Valida @corte, @poliza, @factura",
                new { corte = corte.Date, poliza = codPoliza, factura = idFactura });
        }

        /// <summary>
        /// Ejecuta spPoliza_Aseguradora_Corte_Actualiza (recalcula el resumen del corte).
        /// </summary>
        private static PolizaAseguradoraCorte? SpCorteActualiza(SqlConnection conn, DateTime corte, string codPoliza, int idFactura)
        {
            return conn.QueryFirstOrDefault<PolizaAseguradoraCorte>(
                "exec dbo.spPoliza_Aseguradora_Corte_Actualiza @corte, @poliza, @factura",
                new { corte = corte.Date, poliza = codPoliza, factura = idFactura });
        }

        /// <summary>
        /// Ejecuta spPoliza_Aseguradora_Corte_Add (crea o actualiza el encabezado de la factura).
        /// </summary>
        private static PolizaAseguradoraCorte? SpCorteAdd(SqlConnection conn, PolizaAseguradoraCorteData datos, string usuario)
        {
            const string sql = @"exec dbo.spPoliza_Aseguradora_Corte_Add
                @corte, @poliza, @factura, @aseguradora, @proveedor, @numFactura,
                @formaPago, @vence, @divisa, @unidad, @centroCosto, @tipoCambio, @notas, @usuario";

            return conn.QueryFirstOrDefault<PolizaAseguradoraCorte>(sql, new
            {
                corte = datos.corte!.Value.Date,
                poliza = datos.cod_poliza,
                factura = datos.id_factura ?? 0,
                aseguradora = datos.cod_aseguradora,
                proveedor = datos.cod_proveedor,
                numFactura = (datos.factura ?? string.Empty).Trim(),
                formaPago = NormalizarFormaPago(datos.forma_pago),
                vence = datos.vence?.Date,
                divisa = datos.cod_divisa,
                unidad = datos.cod_unidad,
                centroCosto = datos.cod_centro_costo,
                tipoCambio = datos.tipo_cambio ?? 1m,
                notas = (datos.notas ?? string.Empty).Trim(),
                usuario
            });
        }

        /// <summary>
        /// VB6: IIf(cboFormaPago.Text = "Contado", "Cnt", "Crd").
        /// </summary>
        private static string NormalizarFormaPago(string? formaPago)
            => string.Equals(formaPago?.Trim(), "Cnt", StringComparison.OrdinalIgnoreCase) ? "Cnt" : "Crd";

        /// <summary>
        /// Valida los campos obligatorios del encabezado antes de enviarlo a los SP.
        /// Devuelve el mensaje de error o null si es válido.
        /// </summary>
        private static string? ValidarEncabezado(PolizaAseguradoraCorteData? datos)
        {
            if (datos?.corte is null || string.IsNullOrWhiteSpace(datos.cod_poliza))
            {
                return "Indique la póliza y el corte.";
            }

            return string.IsNullOrWhiteSpace(datos.factura)
                ? "Indique un número de factura válido, no puede enviar a Cuentas por Pagar"
                : null;
        }

        /// <summary>
        /// Registra la bitácora del módulo de pólizas (VB6: Call Bitacora(Movimiento, Mensaje)).
        /// </summary>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Movimiento = movimiento,
                DetalleMovimiento = detalle,
                Modulo = ModuloPolizas
            });
        }

        #endregion
    }
}
