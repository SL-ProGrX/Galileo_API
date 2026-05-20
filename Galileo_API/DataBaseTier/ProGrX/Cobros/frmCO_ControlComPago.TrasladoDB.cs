using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public partial class FrmCoControlComPagoDB
    {
        /// <summary>
        /// Consulta las operaciones pendientes de traslado a tesorería para una remesa cerrada.
        /// </summary>
        public ErrorDto<List<CoControlComPagoTrasladoData>> CO_ControlComPago_TrasladoPendientes_Obtener(int CodEmpresa, int cod_remesa)
        {
            const string sql = @"
                SELECT
                    P.Usuario AS usuario,
                    U.Nombre AS nombre,
                    B.Descripcion AS banco_desc,
                    P.Comision AS comision,
                    P.Cod_Banco AS cod_banco,
                    P.Tipo_Emision AS tipo_emision,
                    ISNULL(P.Cuenta_Ahorros,'') AS cuenta_ahorros,
                    U.Cedula AS cedula,
                    ISNULL(B.CtaConta,'') AS cta_conta
                FROM dbo.CBR_REMESAS_PAGO P
                    INNER JOIN dbo.CBR_USUARIOS U ON P.USUARIO = U.USUARIO
                    INNER JOIN dbo.Tes_Bancos B ON P.COD_BANCO = B.ID_BANCO
                WHERE P.COD_REMESA = @cod_remesa
                    AND P.TESORERIA_FECHA IS NULL
                ORDER BY U.Nombre;";

            return DbHelper.ExecuteListQuery<CoControlComPagoTrasladoData>(
                _portalDB,
                CodEmpresa,
                sql,
                new { cod_remesa });
        }

        /// <summary>
        /// Traslada a tesorería los pagos de comisión seleccionados.
        /// </summary>
        public ErrorDto<CoControlComPagoProcesoResult> CO_ControlComPago_Traslado_Aplicar(int CodEmpresa, string usuario, CoControlComPagoTrasladoAplicarRequest request)
        {
            if (request is null || request.cod_remesa <= 0 || request.usuarios.Count == 0)
            {
                return DbHelper.CreateErrorResponse("Debe seleccionar al menos un usuario para trasladar.", -2, new CoControlComPagoProcesoResult());
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                conn.Open();
                using var tran = conn.BeginTransaction();

                var parametros = ObtenerParametrosTraslado(CodEmpresa);
                var pendientes = CO_ControlComPago_TrasladoPendientesSeleccionados(conn, tran, request);
                var procesados = 0;

                foreach (var item in pendientes)
                {
                    var solicitud = CO_ControlComPago_TesoreriaMaestro_Insertar(conn, tran, usuario, request.cod_remesa, parametros, item);
                    CO_ControlComPago_TesoreriaDetalle_Insertar(conn, tran, solicitud, item.cta_conta, item.comision, "H", 1, parametros.unidad);
                    CO_ControlComPago_TesoreriaDetalle_Insertar(conn, tran, solicitud, parametros.cuenta_gasto, item.comision, "D", 2, parametros.unidad);
                    CO_ControlComPago_RemesaPago_MarcarTesoreria(conn, tran, request.cod_remesa, item.usuario, solicitud, usuario);

                    RegistrarBitacora(CodEmpresa, usuario, $"Traspaso de Remesa de Cobros a Tesoreria:{item.usuario}", "Registra - WEB");
                    procesados++;
                }

                if (procesados > 0)
                {
                    CO_ControlComPago_Remesa_MarcarTrasladada(conn, tran, request.cod_remesa);
                    RegistrarBitacora(CodEmpresa, usuario, $"Traslado de Remesa de Cobros a Tesoreria: {request.cod_remesa}", "Aplica - WEB");
                }

                tran.Commit();
                return DbHelper.CreateOkResponse(new CoControlComPagoProcesoResult { procesados = procesados });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new CoControlComPagoProcesoResult());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -2, new CoControlComPagoProcesoResult());
            }
        }

        private List<CoControlComPagoTrasladoData> CO_ControlComPago_TrasladoPendientesSeleccionados(
            SqlConnection conn,
            SqlTransaction tran,
            CoControlComPagoTrasladoAplicarRequest request)
        {
            const string sql = @"
                SELECT
                    P.Usuario AS usuario,
                    U.Nombre AS nombre,
                    B.Descripcion AS banco_desc,
                    P.Comision AS comision,
                    P.Cod_Banco AS cod_banco,
                    P.Tipo_Emision AS tipo_emision,
                    ISNULL(P.Cuenta_Ahorros,'') AS cuenta_ahorros,
                    U.Cedula AS cedula,
                    ISNULL(B.CtaConta,'') AS cta_conta
                FROM dbo.CBR_REMESAS_PAGO P
                    INNER JOIN dbo.CBR_USUARIOS U ON P.USUARIO = U.USUARIO
                    INNER JOIN dbo.Tes_Bancos B ON P.COD_BANCO = B.ID_BANCO
                WHERE P.COD_REMESA = @cod_remesa
                    AND P.TESORERIA_FECHA IS NULL
                    AND P.Usuario IN @usuarios
                ORDER BY U.Nombre;";

            var usuarios = request.usuarios.Select(NormalizarUsuario).Where(x => x.Length > 0).Distinct().ToList();
            if (usuarios.Count == 0)
            {
                throw new InvalidOperationException("Debe seleccionar al menos un usuario válido.");
            }

            var pendientes = conn.Query<CoControlComPagoTrasladoData>(
                sql,
                new { request.cod_remesa, usuarios },
                tran).ToList();

            if (pendientes.Count == 0)
            {
                throw new InvalidOperationException("No se encontraron pagos pendientes de traslado para la selección.");
            }

            return pendientes;
        }

        private CoControlComPagoTrasladoParametros ObtenerParametrosTraslado(int codEmpresa)
        {
            var parametros = new CoControlComPagoTrasladoParametros
            {
                unidad = _mCobroDb.fxCBRParametro(codEmpresa, "24"),
                concepto = _mCobroDb.fxCBRParametro(codEmpresa, "26"),
                cuenta_gasto = _mCobroDb.fxCBRParametro(codEmpresa, "02")
            };

            if (string.IsNullOrWhiteSpace(parametros.unidad)
                || string.IsNullOrWhiteSpace(parametros.concepto)
                || string.IsNullOrWhiteSpace(parametros.cuenta_gasto))
            {
                throw new InvalidOperationException("No se encontraron todos los parámetros de cobros requeridos para el traslado.");
            }

            return parametros;
        }

        private static long CO_ControlComPago_TesoreriaMaestro_Insertar(
            SqlConnection conn,
            SqlTransaction tran,
            string usuario,
            int codRemesa,
            CoControlComPagoTrasladoParametros parametros,
            CoControlComPagoTrasladoData item)
        {
            const string sql = @"
                INSERT INTO dbo.Tes_Transacciones
                (
                    cod_concepto,
                    cod_unidad,
                    id_banco,
                    tipo,
                    codigo,
                    beneficiario,
                    monto,
                    fecha_solicitud,
                    estado,
                    estadoi,
                    modulo,
                    submodulo,
                    cta_ahorros,
                    detalle1,
                    detalle2,
                    referencia,
                    op,
                    genera,
                    actualiza,
                    user_solicita,
                    autoriza,
                    user_autoriza,
                    fecha_autorizacion
                )
                VALUES
                (
                    @concepto,
                    @unidad,
                    @cod_banco,
                    @tipo_emision,
                    @cedula,
                    @nombre,
                    @comision,
                    CONVERT(date, dbo.MyGetdate()),
                    'P',
                    'P',
                    'CC',
                    'C',
                    @cuenta_ahorros,
                    'Módulo de Cobros',
                    @detalle2,
                    0,
                    0,
                    'S',
                    'S',
                    @usuario,
                    @autoriza,
                    @usuario_autoriza,
                    @fecha_autorizacion
                );

                SELECT CONVERT(bigint, SCOPE_IDENTITY());";

            var esCheque = string.Equals(item.tipo_emision, "CK", StringComparison.OrdinalIgnoreCase);
            return conn.ExecuteScalar<long>(
                sql,
                new
                {
                    parametros.concepto,
                    parametros.unidad,
                    item.cod_banco,
                    item.tipo_emision,
                    item.cedula,
                    item.nombre,
                    item.comision,
                    item.cuenta_ahorros,
                    detalle2 = $"Pago de Comisiones: {codRemesa}",
                    usuario = NormalizarUsuario(usuario),
                    autoriza = esCheque ? "S" : "N",
                    usuario_autoriza = esCheque ? NormalizarUsuario(usuario) : null,
                    fecha_autorizacion = esCheque ? DateTime.Now : (DateTime?)null
                },
                tran);
        }

        private static void CO_ControlComPago_TesoreriaDetalle_Insertar(
            SqlConnection conn,
            SqlTransaction tran,
            long solicitud,
            string cuenta,
            decimal monto,
            string debeHaber,
            int linea,
            string unidad)
        {
            const string sql = @"
                INSERT INTO dbo.Tes_Trans_Asiento
                (
                    nsolicitud,
                    cuenta_contable,
                    monto,
                    debehaber,
                    linea,
                    cod_unidad
                )
                VALUES
                (
                    @solicitud,
                    @cuenta,
                    @monto,
                    @debeHaber,
                    @linea,
                    @unidad
                );";

            conn.Execute(sql, new { solicitud, cuenta = cuenta.Trim(), monto, debeHaber, linea, unidad }, tran);
        }

        private static void CO_ControlComPago_RemesaPago_MarcarTesoreria(
            SqlConnection conn,
            SqlTransaction tran,
            int codRemesa,
            string usuarioPago,
            long solicitud,
            string usuario)
        {
            const string sql = @"
                UPDATE dbo.CBR_REMESAS_PAGO
                SET
                    tesoreria_nSolicitud = @solicitud,
                    tesoreria_fecha = dbo.MyGetdate(),
                    tesoreria_Usuario = @usuario
                WHERE cod_remesa = @codRemesa
                    AND Usuario = @usuarioPago;";

            conn.Execute(sql, new { solicitud, usuario = NormalizarUsuario(usuario), codRemesa, usuarioPago }, tran);
        }

        private static void CO_ControlComPago_Remesa_MarcarTrasladada(SqlConnection conn, SqlTransaction tran, int codRemesa)
        {
            const string sql = @"
                UPDATE dbo.CBR_REMESAS
                SET Estado = 'T'
                WHERE cod_Remesa = @codRemesa;";

            conn.Execute(sql, new { codRemesa }, tran);
        }
    }

    internal sealed class CoControlComPagoTrasladoParametros
    {
        public string unidad { get; init; } = string.Empty;
        public string concepto { get; init; } = string.Empty;
        public string cuenta_gasto { get; init; } = string.Empty;
    }
}
