using System.Globalization;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrCatParametrosDb
    {
        private const int CodigoMensajeControlado = -2;
        private const int ModuloInversiones = 22;

        private const string MensajeParametroNoExiste =
            "El par&aacute;metro indicado no existe.";

        private const string MensajeValorInvalido =
            "El valor indicado no es v&aacute;lido.";

        private const string MensajeCuentaInvalida =
            "La cuenta indicada no es v&aacute;lida; "
            + "presione F4 para buscar en el cat&aacute;logo.";

        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _cntLinkDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmIvrCatParametrosDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _cntLinkDb = new MCntLinkDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inicializa los parámetros generales requeridos por
        /// el módulo de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <returns>
        /// Resultado de la inicialización.
        /// </returns>
        public ErrorDto IVR_Cat_Parametros_Inicializar(
            int codEmpresa)
        {
            const string sql = """
                EXEC spIVR_PARAMETROS;
                """;

            var response = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql);

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al inicializar los par&aacute;metros de Inversiones.");
            }

            return DbHelper.OkResponse(
                "Par&aacute;metros de Inversiones inicializados correctamente.");
        }

        /// <summary>
        /// Obtiene los parámetros generales del módulo de Inversiones,
        /// conservando el orden utilizado por el formulario VB6.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <returns>
        /// Lista de parámetros generales.
        /// </returns>
        public ErrorDto<List<IvrCatParametroDto>>
            IVR_Cat_Parametros_Lista_Obtener(
                int codEmpresa)
        {
            const string sql = """
                SELECT
                    LTRIM(RTRIM(P.COD_PARAMETRO))
                        AS cod_parametro,
                    LTRIM(RTRIM(ISNULL(P.DESCRIPCION, '')))
                        AS descripcion,
                    UPPER(LTRIM(RTRIM(ISNULL(P.TIPO, ''))))
                        AS tipo,
                    LTRIM(RTRIM(ISNULL(P.VALOR, '')))
                        AS valor,
                    LTRIM(RTRIM(ISNULL(P.NOTAS, '')))
                        AS notas,
                    P.ACTUALIZA_FECHA
                        AS actualiza_fecha,
                    LTRIM(RTRIM(ISNULL(P.ACTUALIZA_USUARIO, '')))
                        AS actualiza_usuario
                FROM IVR_PARAMETROS P
                ORDER BY P.COD_PARAMETRO;
                """;

            var response =
                DbHelper.ExecuteListQuery<IvrCatParametroDto>(
                    _portalDb,
                    codEmpresa,
                    sql);

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al consultar los par&aacute;metros de Inversiones.",
                    result: new List<IvrCatParametroDto>());
            }

            var parametros =
                response.Result ??
                new List<IvrCatParametroDto>();

            IVR_Cat_Parametros_Cuentas_Completar(
                codEmpresa,
                parametros);

            return DbHelper.CreateOkResponse(parametros);
        }

        /// <summary>
        /// Valida y actualiza el valor de un parámetro del módulo
        /// de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="request">
        /// Información del parámetro que debe actualizarse.
        /// </param>
        /// <returns>
        /// Resultado de la actualización.
        /// </returns>
        public ErrorDto IVR_Cat_Parametros_Actualizar(
            int codEmpresa,
            IvrCatParametroActualizarRequest? request)
        {
            var requestValidation =
                IVR_Cat_Parametros_Request_Validar(request);

            if (requestValidation != null)
            {
                return requestValidation;
            }

            var codigo = request!.cod_parametro.Trim();
            var usuario = request.usuario
                .Trim()
                .ToUpperInvariant();

            var parametroResponse =
                IVR_Cat_Parametros_Parametro_Obtener(
                    codEmpresa,
                    codigo);

            if (parametroResponse.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    parametroResponse.Description ??
                    "Ocurri&oacute; un error al consultar el par&aacute;metro.");
            }

            var parametro = parametroResponse.Result;

            if (parametro == null)
            {
                return DbHelper.ErrorResponse(
                    MensajeParametroNoExiste,
                    CodigoMensajeControlado);
            }

            var valorValidation =
                IVR_Cat_Parametros_Valor_Validar(
                    codEmpresa,
                    request.valor,
                    parametro.tipo);

            if (!valorValidation.es_valido)
            {
                return DbHelper.ErrorResponse(
                    valorValidation.mensaje,
                    CodigoMensajeControlado);
            }

            var updateResponse =
                IVR_Cat_Parametros_Valor_Actualizar(
                    codEmpresa,
                    codigo,
                    valorValidation.valor,
                    usuario);

            if (updateResponse.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    updateResponse.Description ??
                    "Ocurri&oacute; un error al actualizar el par&aacute;metro.");
            }

            if (updateResponse.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeParametroNoExiste,
                    CodigoMensajeControlado);
            }

            IVR_Cat_Parametros_Bitacora_Registrar(
                codEmpresa,
                usuario,
                codigo,
                parametro.valor,
                valorValidation.valor);

            return DbHelper.OkResponse(
                "Par&aacute;metro actualizado satisfactoriamente.");
        }

        /// <summary>
        /// Obtiene un parámetro específico para validar su existencia,
        /// tipo y valor anterior.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codigo">
        /// Código del parámetro.
        /// </param>
        /// <returns>
        /// Parámetro encontrado o null.
        /// </returns>
        private ErrorDto<IvrCatParametroDto?>
            IVR_Cat_Parametros_Parametro_Obtener(
                int codEmpresa,
                string codigo)
        {
            const string sql = """
                SELECT TOP (1)
                    LTRIM(RTRIM(COD_PARAMETRO))
                        AS cod_parametro,
                    UPPER(LTRIM(RTRIM(ISNULL(TIPO, ''))))
                        AS tipo,
                    LTRIM(RTRIM(ISNULL(VALOR, '')))
                        AS valor
                FROM IVR_PARAMETROS
                WHERE COD_PARAMETRO = @cod_parametro;
                """;

            return DbHelper.ExecuteSingleQuery<IvrCatParametroDto>(
                _portalDb,
                codEmpresa,
                sql,
                defaultValue: null,
                parameters: new
                {
                    cod_parametro = codigo
                });
        }

        /// <summary>
        /// Actualiza el valor, usuario y fecha de modificación
        /// de un parámetro.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codigo">
        /// Código del parámetro.
        /// </param>
        /// <param name="valor">
        /// Valor validado y normalizado.
        /// </param>
        /// <param name="usuario">
        /// Usuario que realiza la modificación.
        /// </param>
        /// <returns>
        /// Cantidad de filas afectadas.
        /// </returns>
        private ErrorDto<int>
            IVR_Cat_Parametros_Valor_Actualizar(
                int codEmpresa,
                string codigo,
                string valor,
                string usuario)
        {
            const string sql = """
                UPDATE IVR_PARAMETROS
                SET ACTUALIZA_USUARIO = @actualiza_usuario,
                    ACTUALIZA_FECHA = dbo.MyGetdate(),
                    VALOR = @valor
                WHERE COD_PARAMETRO = @cod_parametro;
                """;

            return DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    actualiza_usuario = usuario,
                    valor,
                    cod_parametro = codigo
                });
        }

        /// <summary>
        /// Valida la información requerida para actualizar
        /// un parámetro.
        /// </summary>
        /// <param name="request">
        /// Información recibida.
        /// </param>
        /// <returns>
        /// Error controlado o null cuando la solicitud es válida.
        /// </returns>
        private static ErrorDto?
            IVR_Cat_Parametros_Request_Validar(
                IvrCatParametroActualizarRequest? request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(
                    "La informaci&oacute;n del par&aacute;metro es requerida.",
                    CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(
                    request.cod_parametro))
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo del par&aacute;metro es requerido.",
                    CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoMensajeControlado);
            }

            if (request.valor == null)
            {
                return DbHelper.ErrorResponse(
                    "El valor del par&aacute;metro es requerido.",
                    CodigoMensajeControlado);
            }

            return null;
        }

        /// <summary>
        /// Valida y normaliza el valor de acuerdo con el tipo
        /// almacenado para el parámetro.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="valor">
        /// Valor recibido.
        /// </param>
        /// <param name="tipo">
        /// Tipo almacenado en IVR_PARAMETROS.
        /// </param>
        /// <returns>
        /// Resultado de la validación.
        /// </returns>
        private (
            bool es_valido,
            string valor,
            string mensaje)
            IVR_Cat_Parametros_Valor_Validar(
                int codEmpresa,
                string valor,
                string tipo)
        {
            var valorNormalizado = valor.Trim();
            var tipoNormalizado = tipo
                .Trim()
                .ToUpperInvariant();

            return tipoNormalizado switch
            {
                "DEC" =>
                    IVR_Cat_Parametros_Decimal_Validar(
                        valorNormalizado,
                        false),

                "POR" =>
                    IVR_Cat_Parametros_Decimal_Validar(
                        valorNormalizado,
                        true),

                "NUM" =>
                    IVR_Cat_Parametros_Entero_Validar(
                        valorNormalizado),

                "CTA" =>
                    IVR_Cat_Parametros_Cuenta_Validar(
                        codEmpresa,
                        valorNormalizado),

                "CHR" =>
                    IVR_Cat_Parametros_Caracteres_Validar(
                        valorNormalizado),

                "PSN" =>
                    IVR_Cat_Parametros_Pregunta_Validar(
                        valorNormalizado),

                "DTS" =>
                    IVR_Cat_Parametros_Fecha_Validar(
                        valorNormalizado),

                _ => (
                    true,
                    valorNormalizado,
                    string.Empty)
            };
        }

        /// <summary>
        /// Valida y normaliza un valor decimal o porcentual.
        /// </summary>
        /// <param name="valor">
        /// Valor recibido.
        /// </param>
        /// <param name="esPorcentaje">
        /// Indica si debe utilizarse el mensaje para porcentajes.
        /// </param>
        /// <returns>
        /// Resultado de la validación.
        /// </returns>
        private static (
            bool es_valido,
            string valor,
            string mensaje)
            IVR_Cat_Parametros_Decimal_Validar(
                string valor,
                bool esPorcentaje)
        {
            if (!IVR_Cat_Parametros_Decimal_Convertir(
                    valor,
                    out var numero))
            {
                var mensaje = esPorcentaje
                    ? "El valor indicado no es v&aacute;lido; suministre un porcentaje."
                    : MensajeValorInvalido;

                return (
                    false,
                    string.Empty,
                    mensaje);
            }

            return (
                true,
                numero.ToString(
                    CultureInfo.InvariantCulture),
                string.Empty);
        }

        /// <summary>
        /// Valida y normaliza un número entero.
        /// </summary>
        /// <param name="valor">
        /// Valor recibido.
        /// </param>
        /// <returns>
        /// Resultado de la validación.
        /// </returns>
        private static (
            bool es_valido,
            string valor,
            string mensaje)
            IVR_Cat_Parametros_Entero_Validar(
                string valor)
        {
            if (!IVR_Cat_Parametros_Decimal_Convertir(
                    valor,
                    out var numero) ||
                numero < long.MinValue ||
                numero > long.MaxValue)
            {
                return (
                    false,
                    string.Empty,
                    MensajeValorInvalido);
            }

            return (
                true,
                Convert.ToInt64(numero)
                    .ToString(CultureInfo.InvariantCulture),
                string.Empty);
        }

        /// <summary>
        /// Valida y normaliza una cuenta contable utilizando
        /// las funciones compartidas de contabilidad.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="valor">
        /// Cuenta recibida.
        /// </param>
        /// <returns>
        /// Resultado de la validación.
        /// </returns>
        private (
            bool es_valido,
            string valor,
            string mensaje)
            IVR_Cat_Parametros_Cuenta_Validar(
                int codEmpresa,
                string valor)
        {
            var cuenta =
                _cntLinkDb.fxgCntCuentaFormato(
                    codEmpresa,
                    false,
                    valor,
                    0);

            if (string.IsNullOrWhiteSpace(cuenta) ||
                !_cntLinkDb.fxgCntCuentaValida(
                    codEmpresa,
                    cuenta))
            {
                return (
                    false,
                    string.Empty,
                    MensajeCuentaInvalida);
            }

            return (
                true,
                cuenta,
                string.Empty);
        }

        /// <summary>
        /// Valida un parámetro de caracteres.
        /// </summary>
        /// <param name="valor">
        /// Valor recibido.
        /// </param>
        /// <returns>
        /// Resultado de la validación.
        /// </returns>
        private static (
            bool es_valido,
            string valor,
            string mensaje)
            IVR_Cat_Parametros_Caracteres_Validar(
                string valor)
        {
            return valor.Contains('\'')
                ? (
                    false,
                    string.Empty,
                    "El valor indicado contiene caracteres no v&aacute;lidos.")
                : (
                    true,
                    valor,
                    string.Empty);
        }

        /// <summary>
        /// Valida un parámetro de respuesta S o N.
        /// </summary>
        /// <param name="valor">
        /// Valor recibido.
        /// </param>
        /// <returns>
        /// Resultado de la validación.
        /// </returns>
        private static (
            bool es_valido,
            string valor,
            string mensaje)
            IVR_Cat_Parametros_Pregunta_Validar(
                string valor)
        {
            if (valor.Length > 0)
            {
                var respuesta =
                    char.ToUpperInvariant(valor[0]);

                if (respuesta is 'S' or 'N')
                {
                    return (
                        true,
                        respuesta.ToString(),
                        string.Empty);
                }
            }

            return (
                false,
                string.Empty,
                "El valor indicado no es v&aacute;lido; indique S o N.");
        }

        /// <summary>
        /// Valida y normaliza un parámetro de fecha.
        /// </summary>
        /// <param name="valor">
        /// Valor recibido.
        /// </param>
        /// <returns>
        /// Resultado de la validación.
        /// </returns>
        private static (
            bool es_valido,
            string valor,
            string mensaje)
            IVR_Cat_Parametros_Fecha_Validar(
                string valor)
        {
            var cultura =
                CultureInfo.GetCultureInfo("es-CR");

            if (!DateTime.TryParse(
                    valor,
                    cultura,
                    DateTimeStyles.None,
                    out var fecha) &&
                !DateTime.TryParse(
                    valor,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out fecha))
            {
                return (
                    false,
                    string.Empty,
                    "La fecha indicada no es v&aacute;lida.");
            }

            return (
                true,
                fecha.ToString(
                    "yyyy/MM/dd",
                    CultureInfo.InvariantCulture),
                string.Empty);
        }

        /// <summary>
        /// Convierte un valor decimal aceptando los formatos
        /// utilizados por la aplicación.
        /// </summary>
        /// <param name="valor">
        /// Valor recibido.
        /// </param>
        /// <param name="resultado">
        /// Número convertido.
        /// </param>
        /// <returns>
        /// True cuando el valor es numérico.
        /// </returns>
        private static bool
            IVR_Cat_Parametros_Decimal_Convertir(
                string valor,
                out decimal resultado)
        {
            if (decimal.TryParse(
                    valor,
                    NumberStyles.Number,
                    CultureInfo.GetCultureInfo("es-CR"),
                    out resultado))
            {
                return true;
            }

            return decimal.TryParse(
                valor.Replace(',', '.'),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out resultado);
        }

        /// <summary>
        /// Completa la máscara y descripción de los parámetros
        /// que representan cuentas contables.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="parametros">
        /// Parámetros consultados.
        /// </param>
        private void IVR_Cat_Parametros_Cuentas_Completar(
            int codEmpresa,
            IEnumerable<IvrCatParametroDto> parametros)
        {
            foreach (var parametro in parametros.Where(
                         item => string.Equals(
                             item.tipo,
                             "CTA",
                             StringComparison.OrdinalIgnoreCase)))
            {
                parametro.valor_mascara =
                    _cntLinkDb.fxgCntCuentaFormato(
                        codEmpresa,
                        true,
                        parametro.valor,
                        0);

                parametro.valor_descripcion =
                    _cntLinkDb.fxgCntCuentaDesc(
                        codEmpresa,
                        parametro.valor) ??
                    string.Empty;
            }
        }

        /// <summary>
        /// Registra en bitácora la modificación realizada
        /// sobre un parámetro de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="usuario">
        /// Usuario que realiza la modificación.
        /// </param>
        /// <param name="codigo">
        /// Código del parámetro.
        /// </param>
        /// <param name="valorAnterior">
        /// Valor almacenado antes de la modificación.
        /// </param>
        /// <param name="valorNuevo">
        /// Nuevo valor almacenado.
        /// </param>
        private void IVR_Cat_Parametros_Bitacora_Registrar(
            int codEmpresa,
            string usuario,
            string codigo,
            string valorAnterior,
            string valorNuevo)
        {
            _ = _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    Modulo = ModuloInversiones,
                    Movimiento = "Modifica",
                    DetalleMovimiento =
                        $"Parámetro de Inversiones: {codigo} -> "
                        + $"{valorNuevo} (Ant: {valorAnterior})"
                });
        }
    }
}