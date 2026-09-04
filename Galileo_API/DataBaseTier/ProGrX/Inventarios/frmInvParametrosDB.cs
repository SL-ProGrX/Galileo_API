using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvParametrosDb
    {
        private const int CodigoValidacion = -2;

        private const string MensajeEmpresaRequerida =
            "El c&oacute;digo de la empresa es requerido.";

        private const string MensajeContabilidadRequerida =
            "La contabilidad es requerida.";

        private const string MensajeEnlaceContabilidadInvalido =
            "El valor de enlace con contabilidad no es v&aacute;lido.";

        private const string MensajeEnlaceCreditoInvalido =
            "El valor de enlace con cr&eacute;dito no es v&aacute;lido.";

        private const string MensajeParametrosError =
            "Ocurri&oacute; un error al consultar los par&aacute;metros generales.";

        private const string MensajeContabilidadesError =
            "Ocurri&oacute; un error al consultar las contabilidades.";

        private const string MensajeCuentasError =
            "Ocurri&oacute; un error al consultar las cuentas contables.";

        private const string MensajeAsientosError =
            "Ocurri&oacute; un error al consultar los tipos de asiento.";

        private const string MensajeActualizarError =
            "Ocurri&oacute; un error al actualizar los par&aacute;metros generales.";

        private const string MensajeActualizado =
            "Par&aacute;metros actualizados correctamente.";

        private readonly PortalDB _portalDb;

        public FrmInvParametrosDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los parametros generales de inventarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<ParametrosGenDto?>
            INV_Parametros_Parametros_Obtener(
                int CodEmpresa)
        {
            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    ParametrosGenDto?>(
                        MensajeEmpresaRequerida,
                        CodigoValidacion,
                        null);
            }

            const string QueryParametros = """
            select top (1)
                cod_par,
                cod_empresa,
                cta_comisiones,
                cta_imp_renta,
                cta_imp_consumo,
                cta_gastos,
                cta_costo_ventas,
                cta_recibos,
                cta_notas,
                cta_ventas_ing,
                ta_factura_man,
                ta_factura_auto,
                ta_entradas,
                ta_salidas,
                ta_traslados,
                ta_devoluciones,
                ta_nc,
                ta_recibos,
                ta_nd,
                ta_gen,
                enlace_conta,
                enlace_sif
            from pv_parametros_gen;
            """;

            var resultado =
                DbHelper.ExecuteSingleQuery<
                    ParametrosGenDto>(
                        _portalDb,
                        CodEmpresa,
                        QueryParametros);

            if (resultado.Code != 0)
            {
                return DbHelper.CreateErrorResponse<
                    ParametrosGenDto?>(
                        resultado.Description ??
                        MensajeParametrosError,
                        resultado.Code.GetValueOrDefault(-1),
                        null);
            }

            return DbHelper.CreateOkResponse(
                resultado.Result);
        }

        /// <summary>
        /// Obtiene las contabilidades disponibles.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXContaDto>>
            INV_Parametros_Contabilidades_Obtener(
                int CodEmpresa)
        {
            var lista = new List<CntXContaDto>();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    lista);
            }

            const string QueryContabilidades = """
            select
                cod_contabilidad,
                nombre
            from CntX_Contabilidades
            order by cod_contabilidad;
            """;

            var resultado =
                DbHelper.ExecuteListQuery<
                    CntXContaDto>(
                        _portalDb,
                        CodEmpresa,
                        QueryContabilidades);

            return INV_Parametros_Lista_Resultado_Obtener(
                resultado,
                MensajeContabilidadesError);
        }

        /// <summary>
        /// Obtiene las cuentas contables de una contabilidad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_Parametros_Cuentas_Descripciones_Obtener(
                int CodEmpresa,
                int codContabilidad)
        {
            var lista =
                new List<
                    DropDownListaGenericaModel<string>>();

            var validacion =
                INV_Parametros_Catalogo_Validar(
                    CodEmpresa,
                    codContabilidad,
                    lista);

            if (validacion is not null)
            {
                return validacion;
            }

            const string QueryCuentas = """
            select
                cod_cuenta as item,
                descripcion
            from CNTX_CUENTAS
            where cod_contabilidad = @CodContabilidad
            order by cod_cuenta;
            """;

            var resultado =
                DbHelper.ExecuteListQuery<
                    DropDownListaGenericaModel<string>>(
                        _portalDb,
                        CodEmpresa,
                        QueryCuentas,
                        new
                        {
                            CodContabilidad =
                                codContabilidad
                        });

            return INV_Parametros_Lista_Resultado_Obtener(
                resultado,
                MensajeCuentasError);
        }

        /// <summary>
        /// Obtiene los tipos de asiento de una contabilidad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_Parametros_Asientos_Obtener(
                int CodEmpresa,
                int codContabilidad)
        {
            var lista =
                new List<
                    DropDownListaGenericaModel<string>>();

            var validacion =
                INV_Parametros_Catalogo_Validar(
                    CodEmpresa,
                    codContabilidad,
                    lista);

            if (validacion is not null)
            {
                return validacion;
            }

            const string QueryAsientos = """
            select
                tipo_asiento as item,
                descripcion
            from CntX_Tipos_Asientos
            where cod_contabilidad = @CodContabilidad
            order by tipo_asiento;
            """;

            var resultado =
                DbHelper.ExecuteListQuery<
                    DropDownListaGenericaModel<string>>(
                        _portalDb,
                        CodEmpresa,
                        QueryAsientos,
                        new
                        {
                            CodContabilidad =
                                codContabilidad
                        });

            return INV_Parametros_Lista_Resultado_Obtener(
                resultado,
                MensajeAsientosError);
        }

        /// <summary>
        /// Registra o actualiza los parametros generales de inventarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            INV_Parametros_Actualizar(
                int CodEmpresa,
                ParametrosGenDto request)
        {
            if (CodEmpresa <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion);
            }

            var validacion =
                INV_Parametros_Solicitud_Validar(
                    request);

            if (validacion is not null)
            {
                return validacion;
            }

            const string QueryActualizar = """
            if exists
            (
                select 1
                from pv_parametros_gen
            )
            begin
                update pv_parametros_gen
                set
                    cod_empresa = @cod_empresa,
                    enlace_conta = @enlace_conta,
                    enlace_sif = @enlace_sif,
                    cta_ventas_ing = @cta_ventas_ing,
                    cta_gastos = @cta_gastos,
                    cta_comisiones = @cta_comisiones,
                    cta_imp_renta = @cta_imp_renta,
                    cta_imp_consumo = @cta_imp_consumo,
                    cta_costo_ventas = @cta_costo_ventas,
                    cta_recibos = @cta_recibos,
                    cta_notas = @cta_notas,
                    ta_factura_man = @ta_factura_man,
                    ta_factura_auto = @ta_factura_auto,
                    ta_entradas = @ta_entradas,
                    ta_salidas = @ta_salidas,
                    ta_traslados = @ta_traslados,
                    ta_nc = @ta_nc,
                    ta_recibos = @ta_recibos,
                    ta_devoluciones = @ta_devoluciones,
                    ta_nd = @ta_nd,
                    ta_gen = @ta_gen;
            end
            else
            begin
                insert into pv_parametros_gen
                (
                    tipo_cambio,
                    cod_empresa,
                    enlace_conta,
                    enlace_sif,
                    cta_ventas_ing,
                    cta_gastos,
                    cta_comisiones,
                    cta_imp_renta,
                    cta_imp_consumo,
                    cta_costo_ventas,
                    cta_recibos,
                    cta_notas,
                    ta_factura_man,
                    ta_factura_auto,
                    ta_entradas,
                    ta_salidas,
                    ta_traslados,
                    ta_nc,
                    ta_recibos,
                    ta_devoluciones,
                    ta_nd,
                    ta_gen
                )
                values
                (
                    1,
                    @cod_empresa,
                    @enlace_conta,
                    @enlace_sif,
                    @cta_ventas_ing,
                    @cta_gastos,
                    @cta_comisiones,
                    @cta_imp_renta,
                    @cta_imp_consumo,
                    @cta_costo_ventas,
                    @cta_recibos,
                    @cta_notas,
                    @ta_factura_man,
                    @ta_factura_auto,
                    @ta_entradas,
                    @ta_salidas,
                    @ta_traslados,
                    @ta_nc,
                    @ta_recibos,
                    @ta_devoluciones,
                    @ta_nd,
                    @ta_gen
                );
            end;
            """;

            var resultado =
                DbHelper.ExecuteNonQuery(
                    _portalDb,
                    CodEmpresa,
                    QueryActualizar,
                    INV_Parametros_Parametros_Crear(
                        request));

            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ??
                    MensajeActualizarError,
                    resultado.Code.GetValueOrDefault(-1));
            }

            return DbHelper.OkResponse(
                MensajeActualizado);
        }

        /// <summary>
        /// Valida los parametros generales recibidos.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static ErrorDto?
            INV_Parametros_Solicitud_Validar(
                ParametrosGenDto request)
        {
            if (request.cod_empresa <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeContabilidadRequerida,
                    CodigoValidacion);
            }

            if (
                !INV_Parametros_Indicador_Valido(
                    request.enlace_conta))
            {
                return DbHelper.ErrorResponse(
                    MensajeEnlaceContabilidadInvalido,
                    CodigoValidacion);
            }

            return
                INV_Parametros_Indicador_Valido(
                    request.enlace_sif)
                    ? null
                    : DbHelper.ErrorResponse(
                        MensajeEnlaceCreditoInvalido,
                        CodigoValidacion);
        }

        /// <summary>
        /// Valida los parametros requeridos para consultar catalogos.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="lista"></param>
        /// <returns></returns>
        private static ErrorDto<List<T>>?
            INV_Parametros_Catalogo_Validar<T>(
                int CodEmpresa,
                int codContabilidad,
                List<T> lista)
        {
            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    lista);
            }

            return codContabilidad > 0
                ? null
                : DbHelper.CreateErrorResponse(
                    MensajeContabilidadRequerida,
                    CodigoValidacion,
                    lista);
        }

        /// <summary>
        /// Valida un indicador de tipo si o no.
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private static bool
            INV_Parametros_Indicador_Valido(
                string? valor)
        {
            var indicador =
                valor?.Trim();

            return string.Equals(
                    indicador,
                    "S",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    indicador,
                    "N",
                    StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Crea los parametros normalizados para guardar.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static object
            INV_Parametros_Parametros_Crear(
                ParametrosGenDto request)
        {
            return new
            {
                request.cod_empresa,
                enlace_conta =
                    INV_Parametros_Texto_Normalizar(
                        request.enlace_conta)
                        .ToUpperInvariant(),
                enlace_sif =
                    INV_Parametros_Texto_Normalizar(
                        request.enlace_sif)
                        .ToUpperInvariant(),
                cta_ventas_ing =
                    INV_Parametros_Cuenta_Normalizar(
                        request.cta_ventas_ing),
                cta_gastos =
                    INV_Parametros_Cuenta_Normalizar(
                        request.cta_gastos),
                cta_comisiones =
                    INV_Parametros_Cuenta_Normalizar(
                        request.cta_comisiones),
                cta_imp_renta =
                    INV_Parametros_Cuenta_Normalizar(
                        request.cta_imp_renta),
                cta_imp_consumo =
                    INV_Parametros_Cuenta_Normalizar(
                        request.cta_imp_consumo),
                cta_costo_ventas =
                    INV_Parametros_Cuenta_Normalizar(
                        request.cta_costo_ventas),
                cta_recibos =
                    INV_Parametros_Cuenta_Normalizar(
                        request.cta_recibos),
                cta_notas =
                    INV_Parametros_Cuenta_Normalizar(
                        request.cta_notas),
                ta_factura_man =
                    INV_Parametros_Texto_Normalizar(
                        request.ta_factura_man),
                ta_factura_auto =
                    INV_Parametros_Texto_Normalizar(
                        request.ta_factura_auto),
                ta_entradas =
                    INV_Parametros_Texto_Normalizar(
                        request.ta_entradas),
                ta_salidas =
                    INV_Parametros_Texto_Normalizar(
                        request.ta_salidas),
                ta_traslados =
                    INV_Parametros_Texto_Normalizar(
                        request.ta_traslados),
                ta_nc =
                    INV_Parametros_Texto_Normalizar(
                        request.ta_nc),
                ta_recibos =
                    INV_Parametros_Texto_Normalizar(
                        request.ta_recibos),
                ta_devoluciones =
                    INV_Parametros_Texto_Normalizar(
                        request.ta_devoluciones),
                ta_nd =
                    INV_Parametros_Texto_Normalizar(
                        request.ta_nd),
                ta_gen =
                    INV_Parametros_Texto_Normalizar(
                        request.ta_gen)
            };
        }

        /// <summary>
        /// Elimina el formato visual de una cuenta contable.
        /// </summary>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        private static string
            INV_Parametros_Cuenta_Normalizar(
                string? cuenta)
        {
            return INV_Parametros_Texto_Normalizar(
                cuenta).Replace(
                    "-",
                    string.Empty,
                    StringComparison.Ordinal);
        }

        /// <summary>
        /// Normaliza un valor de texto.
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private static string
            INV_Parametros_Texto_Normalizar(
                string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Convierte el resultado de una consulta de lista en una respuesta estandar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resultado"></param>
        /// <param name="mensajeError"></param>
        /// <returns></returns>
        private static ErrorDto<List<T>>
            INV_Parametros_Lista_Resultado_Obtener<T>(
                ErrorDto<List<T>> resultado,
                string mensajeError)
        {
            var lista =
                resultado.Result ??
                new List<T>();

            if (resultado.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    mensajeError,
                    resultado.Code.GetValueOrDefault(-1),
                    lista);
            }

            return DbHelper.CreateOkResponse(lista);
        }
    }
}