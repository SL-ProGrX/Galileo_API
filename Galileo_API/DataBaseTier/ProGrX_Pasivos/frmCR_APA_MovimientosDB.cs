using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;
using System.Data;
using System.Dynamic;

namespace Galileo_API.DataBaseTier.ProGrX_Pasivos
{
    public class FrmCrApaMovimientosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MRecibos _mRecibos;
        private readonly MCntLinkDB _mCntLinkDb;

        private const string MensajeAcreedorRequerido = "Debe indicar el acreedor.";
        private const string MensajeOperacionRequerida = "Debe indicar la operaci&oacute;n.";

        public FrmCrApaMovimientosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
            _mRecibos = new MRecibos(config);
            _mCntLinkDb = new MCntLinkDB(config);
        }

        /// <summary>
        /// Consulta los datos base del acreedor seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosAcreedorDto> CR_APA_Movimientos_Acreedor_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            var acreedor = (cod_acreedor ?? string.Empty).Trim();
            var response = new FrmCrApaMovimientosAcreedorDto();

            if (string.IsNullOrWhiteSpace(acreedor))
            {
                return DbHelper.CreateErrorResponse(MensajeAcreedorRequerido, -2, response);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var row = conn.QueryFirstOrDefault(
                    "exec spAPA_ConsultaAcreedor @cod_acreedor",
                    new { cod_acreedor = acreedor });

                if (row == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontr&oacute; informaci&oacute;n para el acreedor indicado.",
                        -2,
                        response);
                }

                var values = CR_APA_Movimientos_RowToDictionary(row);

                response.cod_acreedor = acreedor;
                response.descripcion = CR_APA_Movimientos_GetString(values, "DESCRIPCION", "descripcion");
                response.saldo = CR_APA_Movimientos_GetDecimal(values, "Saldo", "saldo");

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Consulta el resumen principal de una operacion APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosOperacionDto> CR_APA_Movimientos_Operacion_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            var acreedor = (cod_acreedor ?? string.Empty).Trim();
            var numeroOperacion = (operacion ?? string.Empty).Trim();
            var response = new FrmCrApaMovimientosOperacionDto();

            if (string.IsNullOrWhiteSpace(acreedor))
            {
                return DbHelper.CreateErrorResponse(MensajeAcreedorRequerido, -2, response);
            }

            if (string.IsNullOrWhiteSpace(numeroOperacion))
            {
                return DbHelper.CreateErrorResponse(MensajeOperacionRequerida, -2, response);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var row = conn.QueryFirstOrDefault(
                    "exec spAPA_ConsultaOperacion @cod_acreedor, @operacion",
                    new
                    {
                        cod_acreedor = acreedor,
                        operacion = numeroOperacion
                    });

                if (row == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontr&oacute; la operaci&oacute;n indicada.",
                        -2,
                        response);
                }

                var values = CR_APA_Movimientos_RowToDictionary(row);

                response.estado_desc = CR_APA_Movimientos_GetString(values, "Estado_Desc", "estado_desc");
                response.cod_divisa = CR_APA_Movimientos_GetString(values, "Cod_Divisa", "cod_divisa");
                response.monto = CR_APA_Movimientos_GetDecimal(values, "Monto", "monto");
                response.plazo = CR_APA_Movimientos_GetInt(values, "Plazo", "plazo");
                response.tasa = CR_APA_Movimientos_GetDecimal(values, "Tasa", "tasa");
                response.saldo = CR_APA_Movimientos_GetDecimal(values, "Saldo", "saldo");
                response.notas = CR_APA_Movimientos_GetString(values, "Notas", "notas");
                response.fecha_formaliza = CR_APA_Movimientos_GetDate(values, "Fecha_Formaliza", "fecha_formaliza");
                response.fecha_primer_pago = CR_APA_Movimientos_GetDate(values, "Fecha_Primer_Pago", "fecha_primer_pago");
                response.fecha_prox_pago = CR_APA_Movimientos_GetDate(values, "Fecha_Prox_Pago", "fecha_prox_pago");
                response.dia_de_pago = CR_APA_Movimientos_GetString(values, "dia_de_pago", "Dia_De_Pago", "dia_pago");
                response.mov_amortiza = CR_APA_Movimientos_GetDecimal(values, "Mov_Amortiza", "mov_amortiza");
                response.mov_intereses = CR_APA_Movimientos_GetDecimal(values, "Mov_Intereses", "mov_intereses");
                response.mov_comision = CR_APA_Movimientos_GetDecimal(values, "Mov_Comision", "mov_comision");
                response.mov_cargos = CR_APA_Movimientos_GetDecimal(values, "Mov_Cargos", "mov_cargos");

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Consulta el detalle historico de movimientos de la operacion APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaMovimientosDetalleDto>> CR_APA_Movimientos_Detalle_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            var acreedor = (cod_acreedor ?? string.Empty).Trim();
            var numeroOperacion = (operacion ?? string.Empty).Trim();
            var response = new List<FrmCrApaMovimientosDetalleDto>();

            if (string.IsNullOrWhiteSpace(acreedor))
            {
                return DbHelper.CreateErrorResponse(MensajeAcreedorRequerido, -2, response);
            }

            if (string.IsNullOrWhiteSpace(numeroOperacion))
            {
                return DbHelper.CreateErrorResponse(MensajeOperacionRequerida, -2, response);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var rows = conn.Query(
                    "exec spAPA_ConsultaOperacionDetalle @cod_acreedor, @operacion",
                    new
                    {
                        cod_acreedor = acreedor,
                        operacion = numeroOperacion
                    }).ToList();

                response = rows
                    .Select(CR_APA_Movimientos_MapDetalle)
                    .ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Obtiene la cuenta contable por defecto para afectar movimientos APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosCuentaDto> CR_APA_Movimientos_Cuenta_Obtener(int codEmpresa)
        {
            var response = new FrmCrApaMovimientosCuentaDto();

            try
            {
                var globalesResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, string.Empty);
                if (globalesResp.Code != 0 || globalesResp.Result is null)
                {
                    return DbHelper.CreateErrorResponse(
                        globalesResp.Description ?? "No fue posible obtener los par&aacute;metros globales.",
                        globalesResp.Code.GetValueOrDefault(-1),
                        response);
                }

                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                select
                    rtrim(isnull(D.COD_CUENTA, '')) as cod_cuenta,
                    rtrim(isnull(C.COD_CUENTA_MASK, C.COD_CUENTA)) as cuenta_mask,
                    rtrim(isnull(C.DESCRIPCION, '')) as descripcion
                from SIF_DOCUMENTOS D
                left join CntX_Cuentas C
                    on C.cod_Contabilidad = @enlace
                   and D.cod_cuenta = C.cod_Cuenta
                where D.TIPO_DOCUMENTO = 'APA';";

                var data = conn.QueryFirstOrDefault<FrmCrApaMovimientosCuentaDto>(
                    sql,
                    new { enlace = globalesResp.Result.GEnlace });

                return data == null
                    ? DbHelper.CreateErrorResponse<FrmCrApaMovimientosCuentaDto>(
                        "No se encontr&oacute; configuraci&oacute;n de cuenta para el documento APA.")
                    : DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Navega a la operacion anterior o siguiente del mismo acreedor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosNavegarDto> CR_APA_Movimientos_Operacion_Navegar(
            int codEmpresa,
            FrmCrApaMovimientosNavegarRequest request)
        {
            var response = new FrmCrApaMovimientosNavegarDto();
            var acreedor = (request.cod_acreedor ?? string.Empty).Trim();
            var operacion = (request.operacion ?? string.Empty).Trim();
            var direccion = (request.direccion ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(acreedor))
            {
                return DbHelper.CreateErrorResponse(MensajeAcreedorRequerido, -2, response);
            }

            if (string.IsNullOrWhiteSpace(operacion))
            {
                return DbHelper.CreateErrorResponse(MensajeOperacionRequerida, -2, response);
            }

            if (direccion != "A" && direccion != "S")
            {
                return DbHelper.CreateErrorResponse(
                    "La direcci&oacute;n de navegaci&oacute;n no es v&aacute;lida.",
                    -2,
                    response);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var operador = direccion == "S" ? ">" : "<";
                var orden = direccion == "S" ? "asc" : "desc";

                var sql = $@"
                select top 1
                    rtrim(OPERACION) as operacion
                from CRD_APA_OPERACIONES
                where COD_ACREEDOR = @cod_acreedor
                  and OPERACION {operador} @operacion
                  and (@solo_con_saldo = 0 or isnull(SALDO, 0) > 0)
                order by OPERACION {orden};";

                var data = conn.QueryFirstOrDefault<FrmCrApaMovimientosNavegarDto>(
                    sql,
                    new
                    {
                        cod_acreedor = acreedor,
                        operacion,
                        solo_con_saldo = request.solo_con_saldo ? 1 : 0
                    });

                return data == null
                    ? DbHelper.CreateErrorResponse(
                        "No se encontr&oacute; otra operaci&oacute;n para navegar.",
                        -2,
                        response)
                    : DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Aplica un movimiento APA y devuelve la informacion del recibo generado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosAplicarResultadoDto> CR_APA_Movimientos_Aplicar(
            int codEmpresa,
            FrmCrApaMovimientosAplicarRequest request)
        {
            var response = new FrmCrApaMovimientosAplicarResultadoDto();

            request.cod_acreedor = (request.cod_acreedor ?? string.Empty).Trim();
            request.operacion = (request.operacion ?? string.Empty).Trim();
            request.usuario = (request.usuario ?? string.Empty).Trim();
            request.tipo = (request.tipo ?? string.Empty).Trim().ToUpperInvariant();
            request.notas = (request.notas ?? string.Empty).Trim();
            request.cuenta = (request.cuenta ?? string.Empty).Trim();
            request.doc_ref = (request.doc_ref ?? string.Empty).Trim();

            var validacion = CR_APA_Movimientos_Aplicar_Validar(request, response);
            if (validacion is not null)
            {
                return validacion;
            }

            try
            {
                var cuenta = _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, false, request.cuenta, 0);
                if (string.IsNullOrWhiteSpace(cuenta))
                {
                    return DbHelper.CreateErrorResponse(
                        "La cuenta contable indicada no es v&aacute;lida.",
                        -2,
                        response);
                }

                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var row = conn.QueryFirstOrDefault(
                    @"exec spAPA_Movimiento
                        @cod_acreedor,
                        @operacion,
                        @usuario,
                        @tipo,
                        @amortiza,
                        @intereses,
                        @comision,
                        @cargos,
                        @notas,
                        @cuenta,
                        @doc_ref",
                    new
                    {
                        cod_acreedor = request.cod_acreedor,
                        operacion = request.operacion,
                        usuario = request.usuario,
                        tipo = request.tipo,
                        amortiza = request.amortiza,
                        intereses = request.intereses,
                        comision = request.comision,
                        cargos = request.cargos,
                        notas = request.notas,
                        cuenta,
                        doc_ref = request.doc_ref
                    });

                if (row == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "El proceso no devolvi&oacute; informaci&oacute;n del documento generado.",
                        -1,
                        response);
                }

                var values = CR_APA_Movimientos_RowToDictionary(row);

                response.cod_transaccion = CR_APA_Movimientos_GetString(values, "Cod_Transaccion", "cod_transaccion");
                response.tipo_documento = CR_APA_Movimientos_GetString(values, "Tipo_Documento", "tipo_documento");

                if (string.IsNullOrWhiteSpace(response.cod_transaccion) || string.IsNullOrWhiteSpace(response.tipo_documento))
                {
                    return DbHelper.CreateErrorResponse(
                        "No fue posible identificar el documento generado del movimiento.",
                        -1,
                        response);
                }

                var impresionResp = _mRecibos.sbImprimeRecibo(
                    codEmpresa,
                    response.cod_transaccion,
                    response.tipo_documento,
                    request.usuario);

                response.reporte_resultado = impresionResp.Code == -1
                    ? null
                    : impresionResp.Result?.ToString();

                response.mensaje = impresionResp.Code == -1
                    ? $"Movimiento realizado satisfactoriamente, pero no se pudo generar el recibo: {impresionResp.Description}"
                    : "Movimiento realizado satisfactoriamente!";

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Obtiene la lista de acreedores.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_APA_Movimientos_Acreedores_Obtener(int codEmpresa)
        {
            var response = new List<DropDownListaGenericaModel>();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                select
                    rtrim(isnull(COD_ACREEDOR, '')) as item,
                    rtrim(isnull(DESCRIPCION, '')) as descripcion
                from CRD_APA_ACREEDORES
                order by COD_ACREEDOR;";

                response = conn.Query<DropDownListaGenericaModel>(sql).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Obtiene la lista de operaciones del acreedor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaMovimientosOperacionBusquedaDto>> CR_APA_Movimientos_Operaciones_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            var acreedor = (cod_acreedor ?? string.Empty).Trim();
            var response = new List<FrmCrApaMovimientosOperacionBusquedaDto>();

            if (string.IsNullOrWhiteSpace(acreedor))
            {
                return DbHelper.CreateErrorResponse(MensajeAcreedorRequerido, -2, response);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                select
                    rtrim(isnull(OPERACION, '')) as operacion,
                    rtrim(isnull(COD_ACREEDOR, '')) as cod_acreedor,
                    isnull(MONTO, 0) as monto,
                    isnull(SALDO, 0) as saldo,
                    FECHA_FORMALIZA as fecha_formaliza
                from CRD_APA_OPERACIONES
                where COD_ACREEDOR = @cod_acreedor
                order by OPERACION;";

                response = conn.Query<FrmCrApaMovimientosOperacionBusquedaDto>(
                    sql,
                    new { cod_acreedor = acreedor }).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        private static ErrorDto<FrmCrApaMovimientosAplicarResultadoDto>? CR_APA_Movimientos_Aplicar_Validar(
            FrmCrApaMovimientosAplicarRequest request,
            FrmCrApaMovimientosAplicarResultadoDto response)
        {
            if (string.IsNullOrWhiteSpace(request.cod_acreedor))
            {
                return DbHelper.CreateErrorResponse(MensajeAcreedorRequerido, -2, response);
            }

            if (string.IsNullOrWhiteSpace(request.operacion))
            {
                return DbHelper.CreateErrorResponse(MensajeOperacionRequerida, -2, response);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, response);
            }

            if (request.tipo != "E" && request.tipo != "N")
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es v&aacute;lido.",
                    -2,
                    response);
            }

            if (string.IsNullOrWhiteSpace(request.doc_ref))
            {
                return DbHelper.CreateErrorResponse(
                    "No se ha indicado un documento de referencia.",
                    -2,
                    response);
            }

            if ((request.notas ?? string.Empty).Length <= 10)
            {
                return DbHelper.CreateErrorResponse(
                    "No se ha indicado una nota v&aacute;lida.",
                    -2,
                    response);
            }

            if (request.amortiza < 0 || request.intereses < 0 || request.comision < 0 || request.cargos < 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Los montos del movimiento no pueden ser negativos.",
                    -2,
                    response);
            }

            if (request.amortiza + request.intereses + request.comision + request.cargos <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No se ha indicado un monto para el movimiento.",
                    -2,
                    response);
            }

            if (string.IsNullOrWhiteSpace(request.cuenta))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la cuenta contable a afectar.",
                    -2,
                    response);
            }

            return null;
        }

        private static FrmCrApaMovimientosDetalleDto CR_APA_Movimientos_MapDetalle(object row)
        {
            var values = CR_APA_Movimientos_RowToDictionary(row);

            return new FrmCrApaMovimientosDetalleDto
            {
                fecha = CR_APA_Movimientos_FormatDate(
                    CR_APA_Movimientos_GetDate(values, "fecha", "mov_fecha", "registro_fecha", "pago_fecha")),
                tipo_movimiento = CR_APA_Movimientos_GetString(values, "tipo_movimiento", "tipo", "movimiento", "tipo_documento"),
                documento = CR_APA_Movimientos_GetString(values, "documento", "num_documento", "cod_transaccion", "doc_ref"),
                amortiza = CR_APA_Movimientos_GetDecimal(values, "amortiza", "mov_amortiza", "detalle_amortiza"),
                intereses = CR_APA_Movimientos_GetDecimal(values, "intereses", "mov_intereses", "detalle_intereses"),
                comision = CR_APA_Movimientos_GetDecimal(values, "comision", "mov_comision", "detalle_comision"),
                cargos = CR_APA_Movimientos_GetDecimal(values, "cargos", "mov_cargos", "detalle_cargos"),
                total = CR_APA_Movimientos_GetDecimal(values, "total", "mov_total", "monto"),
                usuario = CR_APA_Movimientos_GetString(values, "usuario", "registro_usuario", "mov_usuario"),
                notas = CR_APA_Movimientos_GetString(values, "notas", "detalle", "mov_notas")
            };
        }

        private static IDictionary<string, object?> CR_APA_Movimientos_RowToDictionary(object row)
        {
            if (row is IDictionary<string, object?> genericDict)
            {
                return genericDict;
            }

            if (row is IDictionary<string, object> dict)
            {
                return dict.ToDictionary(x => x.Key, x => (object?)x.Value);
            }

            if (row is ExpandoObject expando)
            {
                return expando;
            }

            return row.GetType()
                .GetProperties()
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.GetValue(row, null));
        }

        private static string CR_APA_Movimientos_GetString(
            IDictionary<string, object?> values,
            params string[] names)
        {
            foreach (var name in names)
            {
                var key = values.Keys.FirstOrDefault(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
                if (key is null)
                {
                    continue;
                }

                var value = values[key];
                return (value?.ToString() ?? string.Empty).Trim();
            }

            return string.Empty;
        }

        private static decimal CR_APA_Movimientos_GetDecimal(
            IDictionary<string, object?> values,
            params string[] names)
        {
            foreach (var name in names)
            {
                var key = values.Keys.FirstOrDefault(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
                if (key is null)
                {
                    continue;
                }

                var value = values[key];
                if (value == null || value == DBNull.Value)
                {
                    return 0;
                }

                if (decimal.TryParse(value.ToString(), out var result))
                {
                    return result;
                }
            }

            return 0;
        }

        private static int CR_APA_Movimientos_GetInt(
            IDictionary<string, object?> values,
            params string[] names)
        {
            foreach (var name in names)
            {
                var key = values.Keys.FirstOrDefault(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
                if (key is null)
                {
                    continue;
                }

                var value = values[key];
                if (value == null || value == DBNull.Value)
                {
                    return 0;
                }

                if (int.TryParse(value.ToString(), out var result))
                {
                    return result;
                }
            }

            return 0;
        }

        private static DateTime? CR_APA_Movimientos_GetDate(
            IDictionary<string, object?> values,
            params string[] names)
        {
            foreach (var name in names)
            {
                var key = values.Keys.FirstOrDefault(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
                if (key is null)
                {
                    continue;
                }

                var value = values[key];
                if (value == null || value == DBNull.Value)
                {
                    return null;
                }

                if (value is DateTime date)
                {
                    return date;
                }

                if (DateTime.TryParse(value.ToString(), out var parsed))
                {
                    return parsed;
                }
            }

            return null;
        }

        private static string CR_APA_Movimientos_FormatDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("dd/MM/yyyy") : string.Empty;
        }
    }
}