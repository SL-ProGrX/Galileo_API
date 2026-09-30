using System.Data;
using System.Globalization;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public class FrmFslParametrosDb
    {
        private const int ModuloFosol = 22;
        private const int CodigoValidacion = -2;
        private const string MensajeValorInvalido =
            "El valor indicado no es v&aacute;lido.";

        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _cntLinkDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmFslParametrosDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _cntLinkDb = new MCntLinkDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inicializa los parámetros generales requeridos por FOSOL.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Resultado de la inicialización.</returns>
        public ErrorDto FSL_Parametros_Inicializar(int CodCliente)
        {
            var response = DbHelper.WithConn(
                _portalDb,
                CodCliente,
                connection =>
                {
                    connection.Execute(
                        "spFSL_Parametros",
                        commandType: CommandType.StoredProcedure);

                    return true;
                });

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al inicializar los par&aacute;metros de FOSOL.");
            }

            return DbHelper.OkResponse(
                "Par&aacute;metros de FOSOL inicializados correctamente.");
        }

        /// <summary>
        /// Obtiene los parámetros generales de FOSOL con filtro,
        /// ordenamiento y paginación.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros serializados en formato JSON.</param>
        /// <returns>Lista de parámetros y cantidad total.</returns>
        public ErrorDto<FslParametrosListaDto> FSL_Parametros_Lista_Obtener(
            int CodCliente,
            string filtros)
        {
            FslParametrosFiltros request;

            try
            {
                request =
                    JsonConvert.DeserializeObject<FslParametrosFiltros>(
                        filtros) ??
                    new FslParametrosFiltros();
            }
            catch (JsonException)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros enviados no son v&aacute;lidos.",
                    CodigoValidacion,
                    new FslParametrosListaDto());
            }

            var filtro = request.filtro.Trim();
            var like = string.IsNullOrWhiteSpace(filtro)
                ? null
                : $"%{filtro}%";

            var offset = Math.Max(request.pagina, 0);
            var fetch = Math.Clamp(request.paginacion, 1, 500);

            var sortField = request.sort_field.Trim().ToLowerInvariant()
                switch
            {
                "detalle" => "detalle",
                "valor" => "valor",
                _ => "cod_parametro"
            };

            var sortOrder = request.sort_order == -1 ? -1 : 1;

            const string sql = """
                SELECT COUNT(*)
                FROM FSL_PARAMETROS P
                WHERE (
                    @like IS NULL
                    OR P.COD_PARAMETRO LIKE @like
                    OR P.DETALLE LIKE @like
                    OR P.VALOR LIKE @like
                    OR P.TIPO LIKE @like
                    OR P.NOTAS LIKE @like
                );

                SELECT
                    P.COD_PARAMETRO AS cod_parametro,
                    P.DETALLE AS detalle,
                    P.TIPO AS tipo,
                    P.VALOR AS valor,
                    P.NOTAS AS notas,
                    P.REGISTRO_FECHA AS registro_fecha,
                    P.REGISTRO_USUARIO AS registro_usuario,
                    CASE
                        WHEN UPPER(LTRIM(RTRIM(P.TIPO))) = 'CTA'
                        THEN ISNULL((
                            SELECT TOP 1
                                LTRIM(RTRIM(C.DESCRIPCION))
                            FROM CNTX_CUENTAS C
                            CROSS JOIN SIF_EMPRESA E
                            WHERE C.COD_CUENTA =
                                LTRIM(RTRIM(P.VALOR))
                              AND C.COD_CONTABILIDAD =
                                E.COD_EMPRESA_ENLACE
                        ), '')
                        ELSE ''
                    END AS valor_descripcion
                FROM FSL_PARAMETROS P
                WHERE (
                    @like IS NULL
                    OR P.COD_PARAMETRO LIKE @like
                    OR P.DETALLE LIKE @like
                    OR P.VALOR LIKE @like
                    OR P.TIPO LIKE @like
                    OR P.NOTAS LIKE @like
                )
                ORDER BY
                    CASE
                        WHEN @sortField = 'cod_parametro'
                         AND @sortOrder = 1
                        THEN P.COD_PARAMETRO
                    END ASC,
                    CASE
                        WHEN @sortField = 'cod_parametro'
                         AND @sortOrder = -1
                        THEN P.COD_PARAMETRO
                    END DESC,
                    CASE
                        WHEN @sortField = 'detalle'
                         AND @sortOrder = 1
                        THEN P.DETALLE
                    END ASC,
                    CASE
                        WHEN @sortField = 'detalle'
                         AND @sortOrder = -1
                        THEN P.DETALLE
                    END DESC,
                    CASE
                        WHEN @sortField = 'valor'
                         AND @sortOrder = 1
                        THEN P.VALOR
                    END ASC,
                    CASE
                        WHEN @sortField = 'valor'
                         AND @sortOrder = -1
                        THEN P.VALOR
                    END DESC,
                    P.COD_PARAMETRO ASC
                OFFSET @offset ROWS
                FETCH NEXT @fetch ROWS ONLY;
                """;

            var response = DbHelper.WithConn(
                _portalDb,
                CodCliente,
                connection =>
                {
                    using var result = connection.QueryMultiple(
                        sql,
                        new
                        {
                            like,
                            sortField,
                            sortOrder,
                            offset,
                            fetch
                        });

                    return new FslParametrosListaDto
                    {
                        total = result.ReadFirstOrDefault<int>(),
                        parametros = result
                            .Read<FslParametroDto>()
                            .ToList()
                    };
                });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al consultar los par&aacute;metros de FOSOL.",
                    result: new FslParametrosListaDto());
            }

            return response;
        }

        /// <summary>
        /// Valida y actualiza el valor de un parámetro FOSOL.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="request">Información del parámetro por actualizar.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto FSL_Parametros_Actualizar(
            int CodCliente,
            FslParametroActualizarRequest? request)
        {
            var validacion = FSL_Parametros_Request_Validar(request);

            if (validacion != null)
            {
                return validacion;
            }

            var codigo = request!.cod_parametro.Trim();
            var usuario = request.usuario.Trim().ToUpperInvariant();

            const string sqlTipo = """
                SELECT TOP 1
                    LTRIM(RTRIM(TIPO))
                FROM FSL_PARAMETROS
                WHERE COD_PARAMETRO = @cod_parametro;
                """;

            var tipoResponse = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                CodCliente,
                sqlTipo,
                defaultValue: string.Empty,
                parameters: new
                {
                    cod_parametro = codigo
                });

            if (tipoResponse.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    tipoResponse.Description ??
                    "Ocurri&oacute; un error al consultar el par&aacute;metro.");
            }

            if (string.IsNullOrWhiteSpace(tipoResponse.Result))
            {
                return DbHelper.ErrorResponse(
                    "El par&aacute;metro indicado no existe.",
                    CodigoValidacion);
            }

            var resultado = FSL_Parametros_Valor_Validar(
                CodCliente,
                request.valor,
                tipoResponse.Result.Trim().ToUpperInvariant());

            if (!resultado.esValido)
            {
                return DbHelper.ErrorResponse(
                    resultado.mensaje,
                    CodigoValidacion);
            }

            const string sqlActualizar = """
                UPDATE FSL_PARAMETROS
                SET REGISTRO_USUARIO = @registro_usuario,
                    REGISTRO_FECHA = GETDATE(),
                    VALOR = @valor
                WHERE COD_PARAMETRO = @cod_parametro;
                """;

            var updateResponse = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodCliente,
                sqlActualizar,
                new
                {
                    registro_usuario = usuario,
                    valor = resultado.valor,
                    cod_parametro = codigo
                });

            if (updateResponse.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    updateResponse.Description ??
                    "Ocurri&oacute; un error al actualizar el par&aacute;metro.");
            }

            if (updateResponse.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El par&aacute;metro indicado no existe.",
                    CodigoValidacion);
            }

            FSL_Parametros_Bitacora_Registrar(
                CodCliente,
                usuario,
                codigo,
                resultado.valor);

            return DbHelper.OkResponse(
                "Par&aacute;metro actualizado correctamente.");
        }

        /// <summary>
        /// Valida la información requerida para actualizar un parámetro.
        /// </summary>
        /// <param name="request">Información recibida.</param>
        /// <returns>Error de validación o null cuando la información es válida.</returns>
        private static ErrorDto? FSL_Parametros_Request_Validar(
            FslParametroActualizarRequest? request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(
                    "La informaci&oacute;n del par&aacute;metro es requerida.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(request.cod_parametro))
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo del par&aacute;metro es requerido.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoValidacion);
            }

            if (request.valor == null)
            {
                return DbHelper.ErrorResponse(
                    "El valor del par&aacute;metro es requerido.",
                    CodigoValidacion);
            }

            return null;
        }

        /// <summary>
        /// Valida y ajusta el valor de acuerdo con el tipo del parámetro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="valor">Valor ingresado.</param>
        /// <param name="tipo">Tipo configurado para el parámetro.</param>
        /// <returns>Resultado de la validación y valor ajustado.</returns>
        private (
            bool esValido,
            string valor,
            string mensaje)
            FSL_Parametros_Valor_Validar(
                int CodCliente,
                string valor,
                string tipo)
        {
            var valorAjustado = valor.Trim();

            return tipo switch
            {
                "DEC" or "NUM" or "POR" =>
                    FSL_Parametros_Valor_Numerico_Validar(
                        valorAjustado,
                        tipo),

                "CTA" or "CHR" or "PSN" =>
                    FSL_Parametros_Valor_Texto_Validar(
                        CodCliente,
                        valorAjustado,
                        tipo),

                "DTS" =>
                    FSL_Parametros_Valor_Fecha_Validar(
                        valorAjustado),

                _ => (
                    true,
                    valorAjustado,
                    string.Empty)
            };
        }

        /// <summary>
        /// Valida parámetros numéricos, decimales y porcentuales.
        /// </summary>
        /// <param name="valor">Valor recibido.</param>
        /// <param name="tipo">Tipo numérico configurado.</param>
        /// <returns>Resultado de la validación y valor ajustado.</returns>
        private static (
            bool esValido,
            string valor,
            string mensaje)
            FSL_Parametros_Valor_Numerico_Validar(
                string valor,
                string tipo)
        {
            if (!FSL_Parametros_Decimal_Convertir(
                valor,
                out var numero))
            {
                var mensaje = tipo == "POR"
                    ? "El valor indicado no es v&aacute;lido; suministre un porcentaje."
                    : MensajeValorInvalido;

                return (
                    false,
                    string.Empty,
                    mensaje);
            }

            if (tipo == "NUM")
            {
                if (numero < long.MinValue ||
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
                        .ToString(
                            CultureInfo.InvariantCulture),
                    string.Empty);
            }

            return (
                true,
                numero.ToString(
                    CultureInfo.InvariantCulture),
                string.Empty);
        }

        /// <summary>
        /// Valida parámetros de cuenta contable, caracteres y respuestas S/N.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="valor">Valor recibido.</param>
        /// <param name="tipo">Tipo de parámetro configurado.</param>
        /// <returns>Resultado de la validación y valor ajustado.</returns>
        private (
            bool esValido,
            string valor,
            string mensaje)
            FSL_Parametros_Valor_Texto_Validar(
                int CodCliente,
                string valor,
                string tipo)
        {
            if (tipo == "CTA")
            {
                var cuenta =
                    _cntLinkDb.fxgCntCuentaFormato(
                        CodCliente,
                        false,
                        valor,
                        0);

                if (string.IsNullOrWhiteSpace(cuenta) ||
                    !_cntLinkDb.fxgCntCuentaValida(
                        CodCliente,
                        cuenta))
                {
                    return (
                        false,
                        string.Empty,
                        "La cuenta indicada no es v&aacute;lida; presione F4 para buscar en el cat&aacute;logo.");
                }

                return (
                    true,
                    cuenta,
                    string.Empty);
            }

            if (tipo == "CHR")
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

            if (valor.Length > 0)
            {
                var respuesta =
                    char.ToUpperInvariant(valor[0]);

                if (respuesta == 'S' ||
                    respuesta == 'N')
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
        /// Valida y ajusta un parámetro de fecha.
        /// </summary>
        /// <param name="valor">Fecha recibida.</param>
        /// <returns>Resultado de la validación y fecha ajustada.</returns>
        private static (
            bool esValido,
            string valor,
            string mensaje)
            FSL_Parametros_Valor_Fecha_Validar(
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
        /// Convierte un valor decimal aceptando punto o coma decimal.
        /// </summary>
        /// <param name="valor">Valor recibido.</param>
        /// <param name="resultado">Valor decimal obtenido.</param>
        /// <returns>True cuando el valor pudo convertirse.</returns>
        private static bool FSL_Parametros_Decimal_Convertir(
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
        /// Registra la modificación del parámetro en la bitácora.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la modificación.</param>
        /// <param name="codigo">Código del parámetro.</param>
        /// <param name="valor">Nuevo valor del parámetro.</param>
        private void FSL_Parametros_Bitacora_Registrar(
            int CodCliente,
            string usuario,
            string codigo,
            string valor)
        {
            _ = _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = CodCliente,
                    Usuario = usuario,
                    Modulo = ModuloFosol,
                    Movimiento = "Modifica",
                    DetalleMovimiento =
                        $"Parámetro de FOSOL: {codigo} -> {valor}"
                });
        }
    }
}