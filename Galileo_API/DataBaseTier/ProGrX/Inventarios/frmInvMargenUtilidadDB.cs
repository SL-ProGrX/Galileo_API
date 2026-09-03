using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public sealed class FrmInvMargenUtilidadDb
    {
        private const int CodigoValidacion = -2;
        private const string ModoMargenesPrecios = "MU";
        private const string ModoSoloPrecios = "P";

        private const string MensajeEmpresaRequerida =
            "El c&oacute;digo de la empresa es requerido.";

        private const string MensajeSolicitudRequerida =
            "La informaci&oacute;n para actualizar los m&aacute;rgenes es requerida.";

        private const string MensajeLineaRequerida =
            "La l&iacute;nea de producto es requerida.";

        private const string MensajeSublineaRequerida =
            "La subl&iacute;nea de producto es requerida.";

        private const string MensajeModoRequerido =
            "El modo de actualizaci&oacute;n es requerido.";

        private const string MensajeModoInvalido =
            "El modo de actualizaci&oacute;n indicado no es v&aacute;lido.";

        private const string MensajeCodigoPrecioRequerido =
            "El c&oacute;digo del tipo de precio es requerido.";

        private const string MensajePrecioDuplicado =
            "No se permite incluir el mismo tipo de precio m&aacute;s de una vez.";

        private const string MensajeLineasError =
            "Ocurri&oacute; un error al consultar las l&iacute;neas de productos.";

        private const string MensajeSublineasError =
            "Ocurri&oacute; un error al consultar las subl&iacute;neas de productos.";

        private const string MensajePreciosError =
            "Ocurri&oacute; un error al consultar los tipos de precio.";

        private const string MensajeAplicarError =
            "Ocurri&oacute; un error al actualizar los m&aacute;rgenes y precios.";

        private const string MensajeMargenesActualizados =
            "M&aacute;rgenes de utilidad actualizados correctamente.";

        private const string MensajePreciosActualizados =
            "Precios actualizados correctamente.";

        private readonly PortalDB _portalDb;

        public FrmInvMargenUtilidadDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las lineas de productos disponibles.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_MargenUtilidad_Lineas_Obtener(
                int CodEmpresa)
        {
            var lista =
                new List<
                    DropDownListaGenericaModel<int>>();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    lista);
            }

            const string QueryLineas = """
            select
                COD_PRODCLAS as item,
                DESCRIPCION as descripcion
            from PV_PROD_CLASIFICA
            order by COD_PRODCLAS;
            """;

            var resultado =
                DbHelper.ExecuteListQuery<
                    DropDownListaGenericaModel<int>>(
                        _portalDb,
                        CodEmpresa,
                        QueryLineas);

            return INV_MargenUtilidad_Lista_Resultado_Obtener(
                resultado,
                MensajeLineasError);
        }

        /// <summary>
        /// Obtiene las sublineas pertenecientes a una linea de producto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codLinea"></param>
        /// <returns></returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_MargenUtilidad_Sublineas_Obtener(
                int CodEmpresa,
                int codLinea)
        {
            var lista =
                new List<
                    DropDownListaGenericaModel<int>>();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    lista);
            }

            if (codLinea <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeLineaRequerida,
                    CodigoValidacion,
                    lista);
            }

            const string QuerySublineas = """
            select
                COD_LINEA_SUB as item,
                DESCRIPCION as descripcion
            from PV_PROD_CLASIFICA_SUB
            where COD_PRODCLAS = @CodLinea
            order by COD_LINEA_SUB;
            """;

            var resultado =
                DbHelper.ExecuteListQuery<
                    DropDownListaGenericaModel<int>>(
                        _portalDb,
                        CodEmpresa,
                        QuerySublineas,
                        new
                        {
                            CodLinea = codLinea
                        });

            return INV_MargenUtilidad_Lista_Resultado_Obtener(
                resultado,
                MensajeSublineasError);
        }

        /// <summary>
        /// Obtiene los tipos de precio disponibles.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_MargenUtilidad_Precios_Obtener(
                int CodEmpresa)
        {
            var lista =
                new List<
                    DropDownListaGenericaModel<string>>();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    lista);
            }

            const string QueryTiposPrecio = """
            select
                cod_precio as item,
                descripcion
            from pv_tipos_precios
            order by cod_precio;
            """;

            var resultado =
                DbHelper.ExecuteListQuery<
                    DropDownListaGenericaModel<string>>(
                        _portalDb,
                        CodEmpresa,
                        QueryTiposPrecio);

            return INV_MargenUtilidad_Lista_Resultado_Obtener(
                resultado,
                MensajePreciosError);
        }

        /// <summary>
        /// Actualiza los márgenes y precios para la linea y sublinea seleccionadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            INV_MargenUtilidad_Cambios_Aplicar(
                int CodEmpresa,
                InvMargenUtilidadAplicarRequest request)
        {
            var validacion =
                INV_MargenUtilidad_Solicitud_Validar(
                    CodEmpresa,
                    request);

            if (validacion is not null)
            {
                return validacion;
            }

            var solicitud = request!;

            INV_MargenUtilidad_Solicitud_Normalizar(
                solicitud);

            var resultado = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    try
                    {
                        INV_MargenUtilidad_Proceso_Ejecutar(
                            connection,
                            transaction,
                            solicitud);

                        transaction.Commit();

                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ??
                    MensajeAplicarError,
                    resultado.Code.GetValueOrDefault(-1));
            }

            return DbHelper.OkResponse(
                solicitud.modo ==
                    ModoMargenesPrecios
                    ? MensajeMargenesActualizados
                    : MensajePreciosActualizados);
        }

        /// <summary>
        /// Ejecuta el proceso correspondiente al modo seleccionado.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="request"></param>
        private static void
            INV_MargenUtilidad_Proceso_Ejecutar(
                IDbConnection connection,
                IDbTransaction transaction,
                InvMargenUtilidadAplicarRequest request)
        {
            if (request.modo == ModoMargenesPrecios)
            {
                INV_MargenUtilidad_Margenes_Actualizar(
                    connection,
                    transaction,
                    request);

                return;
            }

            INV_MargenUtilidad_Precios_Actualizar(
                connection,
                transaction,
                request);
        }

        /// <summary>
        /// Actualiza los márgenes indicados y recalcula los precios.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="request"></param>
        private static void
            INV_MargenUtilidad_Margenes_Actualizar(
                IDbConnection connection,
                IDbTransaction transaction,
                InvMargenUtilidadAplicarRequest request)
        {
            const string QueryMargenPrecioRegular = """
            update pv_productos
            set
                precio_regular =
                    costo_regular +
                    (
                        costo_regular *
                        @UtilidadPrecioRegular / 100.0
                    ),
                porc_utilidad =
                    @UtilidadPrecioRegular
            where estado = 'A'
              and cod_prodclas = @CodLinea
              and COD_LINEA_SUB = @CodSublinea;
            """;

            INV_MargenUtilidad_PrecioRegular_Actualizar(
                connection,
                transaction,
                request,
                QueryMargenPrecioRegular);

            if (request.precios.Count == 0)
            {
                return;
            }

            var parametros =
                request.precios.Select(
                    precio => new
                    {
                        CodLinea = request.cod_linea,
                        CodSublinea =
                            request.cod_sublinea,
                        CodPrecio =
                            precio.cod_precio,
                        Utilidad =
                            precio.utilidad
                    });

           const string QueryMargenesTiposPrecio = """
            update X
            set
                X.porc_utilidad = @Utilidad,
                X.monto =
                    P.costo_regular +
                    (
                        P.costo_regular *
                        @Utilidad / 100.0
                    )
            from pv_productos P
            inner join pv_producto_precios X
                on P.cod_producto = X.cod_producto
            where P.estado = 'A'
              and P.cod_prodclas = @CodLinea
              and P.COD_LINEA_SUB = @CodSublinea
              and X.cod_precio = @CodPrecio;
            """;

            connection.Execute(
                QueryMargenesTiposPrecio,
                parametros,
                transaction);
        }

        /// <summary>
        /// Recalcula los precios utilizando los márgenes actuales.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="request"></param>
        private static void
            INV_MargenUtilidad_Precios_Actualizar(
                IDbConnection connection,
                IDbTransaction transaction,
                InvMargenUtilidadAplicarRequest request)
        {
            const string QueryPrecioRegularSegunMargen = """
            update pv_productos
            set precio_regular =
                costo_regular +
                (
                    costo_regular *
                    porc_utilidad / 100.0
                )
            where estado = 'A'
              and cod_prodclas = @CodLinea
              and COD_LINEA_SUB = @CodSublinea;
            """;

            INV_MargenUtilidad_PrecioRegular_Actualizar(
                 connection,
                 transaction,
                 request,
                 QueryPrecioRegularSegunMargen);

            if (request.precios.Count == 0)
            {
                return;
            }

            string[] codigosPrecio =
                request.precios
                    .Select(
                        precio =>
                            precio.cod_precio)
                    .ToArray(); 

            const string QueryTiposPrecioSegunMargen = """
            update X
            set X.monto =
                P.costo_regular +
                (
                    P.costo_regular *
                    X.porc_utilidad / 100.0
                )
            from pv_productos P
            inner join pv_producto_precios X
                on P.cod_producto = X.cod_producto
            where P.estado = 'A'
              and P.cod_prodclas = @CodLinea
              and P.COD_LINEA_SUB = @CodSublinea
              and X.cod_precio in @CodigosPrecio;
            """;

            connection.Execute(
                QueryTiposPrecioSegunMargen,
                new
                {
                    CodLinea = request.cod_linea,
                    CodSublinea =
                        request.cod_sublinea,
                    CodigosPrecio =
                        codigosPrecio
                },
                transaction);
        }

        /// <summary>
        /// Actualiza el precio regular cuando fue solicitado.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="request"></param>
        /// <param name="query"></param>
        private static void
            INV_MargenUtilidad_PrecioRegular_Actualizar(
                IDbConnection connection,
                IDbTransaction transaction,
                InvMargenUtilidadAplicarRequest request,
                string query)
        {
            if (!request.actualiza_precio_regular)
            {
                return;
            }

            connection.Execute(
                query,
                new
                {
                    CodLinea = request.cod_linea,
                    CodSublinea =
                        request.cod_sublinea,
                    UtilidadPrecioRegular =
                        request.utilidad_precio_regular
                },
                transaction);
        }

        /// <summary>
        /// Valida la empresa y la solicitud recibida.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static ErrorDto?
            INV_MargenUtilidad_Solicitud_Validar(
                int CodEmpresa,
                InvMargenUtilidadAplicarRequest? request)
        {
            if (CodEmpresa <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion);
            }

            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    MensajeSolicitudRequerida,
                    CodigoValidacion);
            }

            if (request.cod_linea <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeLineaRequerida,
                    CodigoValidacion);
            }

            if (request.cod_sublinea <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeSublineaRequerida,
                    CodigoValidacion);
            }

            return INV_MargenUtilidad_Modo_Validar(
                request);
        }

        /// <summary>
        /// Valida el modo y los precios de la solicitud.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static ErrorDto?
            INV_MargenUtilidad_Modo_Validar(
                InvMargenUtilidadAplicarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.modo))
            {
                return DbHelper.ErrorResponse(
                    MensajeModoRequerido,
                    CodigoValidacion);
            }

            string modo =
                request.modo
                    .Trim()
                    .ToUpperInvariant();

            if (
                modo != ModoMargenesPrecios &&
                modo != ModoSoloPrecios)
            {
                return DbHelper.ErrorResponse(
                    MensajeModoInvalido,
                    CodigoValidacion);
            }

            return INV_MargenUtilidad_Precios_Validar(
                request.precios);
        }

        /// <summary>
        /// Valida los tipos de precio recibidos.
        /// </summary>
        /// <param name="precios"></param>
        /// <returns></returns>
        private static ErrorDto?
            INV_MargenUtilidad_Precios_Validar(
                List<InvMargenUtilidadPrecioAplicarRequest>?
                    precios)
        {
            if (precios is null || precios.Count == 0)
            {
                return null;
            }

            if (
                precios.Any(
                    precio =>
                        precio is null ||
                        string.IsNullOrWhiteSpace(
                            precio.cod_precio)))
            {
                return DbHelper.ErrorResponse(
                    MensajeCodigoPrecioRequerido,
                    CodigoValidacion);
            }

            int preciosDistintos =
                precios
                    .Select(
                        precio =>
                            precio.cod_precio.Trim())
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .Count();

            return preciosDistintos == precios.Count
                ? null
                : DbHelper.ErrorResponse(
                    MensajePrecioDuplicado,
                    CodigoValidacion);
        }

        /// <summary>
        /// Normaliza los datos de la solicitud.
        /// </summary>
        /// <param name="request"></param>
        private static void
            INV_MargenUtilidad_Solicitud_Normalizar(
                InvMargenUtilidadAplicarRequest request)
        {
            request.modo =
                request.modo
                    .Trim()
                    .ToUpperInvariant();

            request.precios ??= [];

            foreach (var precio in request.precios)
            {
                precio.cod_precio =
                    precio.cod_precio.Trim();
            }
        }

        /// <summary>
        /// Convierte el resultado de una consulta en una respuesta estándar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resultado"></param>
        /// <param name="mensajeError"></param>
        /// <returns></returns>
        private static ErrorDto<List<T>>
            INV_MargenUtilidad_Lista_Resultado_Obtener<T>(
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