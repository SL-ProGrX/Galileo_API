using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrGeneraGarantiaDb
    {
        private static readonly string[] Provincias =
        [
            "San José", "Alajuela", "Cartago", "Heredia",
            "Guanacaste", "Puntarenas", "Limón"
        ];

        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos para la emisión de garantías.
        /// </summary>
        /// <param name="config">Configuración utilizada para resolver la conexión empresarial.</param>
        public FrmCrGeneraGarantiaDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Registra la información requerida para emitir el pagaré.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se procesa la operación.</param>
        /// <param name="request">Operación y opciones de emisión seleccionadas.</param>
        /// <returns>Cédula y secciones que requiere el reporte.</returns>
        public ErrorDto<CrGeneraGarantiaPagareDto> CR_GeneraGarantia_Pagare_Preparar(
            int codEmpresa,
            CrGeneraGarantiaOperacionRequest request)
        {
            var validacion = ValidarOperacion(request);
            if (validacion is not null)
                return DbHelper.CreateErrorResponse<CrGeneraGarantiaPagareDto>(validacion, -1, new());

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = connection.QueryFirstOrDefault<CrGeneraGarantiaPagareDto>(
                    @"exec spCrd_Operacion_Pagare_Registra
                        @Operacion, @Reemplazar, @Cedula, @Provincia, @Reservado",
                    new
                    {
                        Operacion = request.operacion,
                        Reemplazar = request.reemplazar_informacion ? 1 : 0,
                        Cedula = request.usar_cedula_real ? 1 : 0,
                        Provincia = request.lugar_firma.Trim(),
                        Reservado = 0
                    });

                return result is null
                    ? DbHelper.CreateErrorResponse<CrGeneraGarantiaPagareDto>(
                        "El proceso no devolvió información para generar el pagaré.", -1, new())
                    : DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrGeneraGarantiaPagareDto>(ex.Message, -1, new());
            }
        }

        /// <summary>
        /// Registra la información requerida para emitir el contrato de crédito.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se procesa la operación.</param>
        /// <param name="usuario">Usuario que solicita el contrato.</param>
        /// <param name="request">Operación y opciones de emisión seleccionadas.</param>
        /// <returns>Cédula que requiere el reporte.</returns>
        public ErrorDto<CrGeneraGarantiaContratoDto> CR_GeneraGarantia_Contrato_Preparar(
            int codEmpresa,
            string usuario,
            CrGeneraGarantiaOperacionRequest request)
        {
            var validacion = ValidarOperacion(request);
            if (validacion is not null)
                return DbHelper.CreateErrorResponse<CrGeneraGarantiaContratoDto>(validacion, -1, new());

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = connection.QueryFirstOrDefault<CrGeneraGarantiaContratoDto>(
                    @"exec spCrd_Operacion_Contrato_Registra
                        @Operacion, @Reemplazar, @Cedula, @Provincia,
                        @Reservado1, @Reservado2, @Usuario",
                    new
                    {
                        Operacion = request.operacion,
                        Reemplazar = request.reemplazar_informacion ? 1 : 0,
                        Cedula = request.usar_cedula_real ? 1 : 0,
                        Provincia = request.lugar_firma.Trim(),
                        Reservado1 = 0,
                        Reservado2 = 0,
                        Usuario = (usuario ?? string.Empty).Trim()
                    });

                return result is null
                    ? DbHelper.CreateErrorResponse<CrGeneraGarantiaContratoDto>(
                        "El proceso no devolvió información para generar el contrato.", -1, new())
                    : DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrGeneraGarantiaContratoDto>(ex.Message, -1, new());
            }
        }

        /// <summary>
        /// Genera y envía el pagaré digital mediante el procedimiento legado.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se procesa la operación.</param>
        /// <param name="request">Operación y opciones de emisión seleccionadas.</param>
        /// <returns>Correo al que se envió el documento.</returns>
        public ErrorDto<CrGeneraGarantiaEmailDto> CR_GeneraGarantia_PagareEmail_Enviar(
            int codEmpresa,
            CrGeneraGarantiaOperacionRequest request)
        {
            var validacion = ValidarOperacion(request);
            if (validacion is not null)
                return DbHelper.CreateErrorResponse<CrGeneraGarantiaEmailDto>(validacion, -1, new());

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = connection.QueryFirstOrDefault<CrGeneraGarantiaEmailDto>(
                    @"exec spCrd_Pagare_Email
                        @Operacion, @Reemplazar, @Cedula, @Provincia",
                    new
                    {
                        Operacion = request.operacion,
                        Reemplazar = request.reemplazar_informacion ? 1 : 0,
                        Cedula = request.usar_cedula_real ? 1 : 0,
                        Provincia = request.lugar_firma.Trim()
                    });

                return result is null
                    ? DbHelper.CreateErrorResponse<CrGeneraGarantiaEmailDto>(
                        "No fue posible confirmar el correo de destino.", -1, new())
                    : DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrGeneraGarantiaEmailDto>(ex.Message, -1, new());
            }
        }

        /// <summary>
        /// Obtiene las operaciones elegibles para imprimir letras de cambio.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se consultan las operaciones.</param>
        /// <param name="request">Rango inicial y final de operaciones.</param>
        /// <returns>Operaciones recibidas, pendientes o aprobadas dentro del rango.</returns>
        public ErrorDto<List<CrGeneraGarantiaLetraDto>> CR_GeneraGarantia_Letras_Obtener(
            int codEmpresa,
            CrGeneraGarantiaRangoRequest request)
        {
            if (request.inicio <= 0)
                return DbHelper.CreateErrorResponse<List<CrGeneraGarantiaLetraDto>>(
                    "Falta el #OP.Inicial.", -1, []);

            if (request.corte <= 0)
                return DbHelper.CreateErrorResponse<List<CrGeneraGarantiaLetraDto>>(
                    "Falta el #OP.Final.", -1, []);

            if (request.inicio > request.corte)
                return DbHelper.CreateErrorResponse<List<CrGeneraGarantiaLetraDto>>(
                    "El #OP.Inicial no puede ser mayor al #OP.Final.", -1, []);

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                const string sql = @"
                    select
                        id_solicitud,
                        rtrim(codigo) as codigo,
                        rtrim(cedula) as cedula,
                        isnull(montoapr, 0) as montoapr,
                        isnull(montosol, 0) as montosol,
                        rtrim(estadosol) as estadosol
                    from Reg_Creditos
                    where id_solicitud between @inicio and @corte
                      and estadosol in ('R', 'P', 'A')
                    order by id_solicitud;";

                var operaciones = connection.Query<CrGeneraGarantiaLetraDto>(
                    sql,
                    new { request.inicio, request.corte }).ToList();
                var fechaServidor = connection.ExecuteScalar<DateTime>("select getdate();");

                foreach (var operacion in operaciones)
                {
                    var monto = operacion.estadosol is "R" or "P"
                        ? operacion.montosol
                        : operacion.montoapr;
                    operacion.lugar_fecha =
                        $"San José {fechaServidor.Day:00} De {ObtenerMes(fechaServidor.Month)} De {fechaServidor.Year}";
                    operacion.monto_letras = ConvertirMontoALetras(monto);
                    operacion.monto = $"¢ {monto:N2}";
                }

                return DbHelper.CreateOkResponse(operaciones);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrGeneraGarantiaLetraDto>>(
                    ex.Message, -1, []);
            }
        }

        /// <summary>
        /// Prepara la información requerida para emitir un pagaré preimpreso.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se procesa la operación.</param>
        /// <param name="request">Operación y opciones seleccionadas para el pagaré.</param>
        /// <returns>Textos, monto, mora y configuración requeridos por el reporte.</returns>
        public ErrorDto<CrGeneraGarantiaPreImpresoDto> CR_GeneraGarantia_PreImpreso_Preparar(
            int codEmpresa,
            CrGeneraGarantiaOperacionRequest request)
        {
            var validacion = ValidarOperacion(request);
            if (validacion is not null)
                return DbHelper.CreateErrorResponse<CrGeneraGarantiaPreImpresoDto>(
                    validacion, -1, new());

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                var operacion = ObtenerOperacionPreImpresa(connection, request.operacion);
                if (operacion is null)
                    return DbHelper.CreateErrorResponse<CrGeneraGarantiaPreImpresoDto>(
                        $"No existe el número de operación indicado, verifique... {request.operacion}",
                        -1,
                        new());

                if (operacion.estadosol is not ("A" or "F"))
                    return DbHelper.CreateErrorResponse<CrGeneraGarantiaPreImpresoDto>(
                        $"Esta operación no se encuentra Aprobada o Formalizada, verifique... {request.operacion}",
                        -1,
                        new());

                var tieneFiadores = operacion.garantia.Equals(
                    "F",
                    StringComparison.OrdinalIgnoreCase);
                var calidadSolicitante = ObtenerCalidades(
                    connection,
                    operacion.cedula,
                    request.usar_cedula_real);
                var calidadesFiadores = tieneFiadores
                    ? ObtenerCalidadesFiadores(connection, request.operacion, request.usar_cedula_real)
                    : string.Empty;
                var mora = ObtenerMora(connection, operacion.codigo, operacion.montoapr);
                if (mora == 0)
                    mora = operacion.int_corriente;

                var resultado = new CrGeneraGarantiaPreImpresoDto
                {
                    formula01 = new string(' ', 30) + calidadSolicitante,
                    formula02 = new string(' ', 20)
                        + ConvertirNumeroALetras(operacion.plazo)
                        + " cuota(s) mensual(es) iguales",
                    monto_letras = new string(' ', 75) + ConvertirMontoALetras(operacion.montoapr),
                    prometo = tieneFiadores ? "            emos" : "            o",
                    mora = mora,
                    fiadores = request.imprimir_nombres_cedula ? "S" : "N"
                };

                using var transaction = connection.BeginTransaction();
                connection.Execute(
                    "delete from Tmp_Pagare where id_solicitud = @operacion;",
                    new { operacion = request.operacion },
                    transaction);
                connection.Execute(
                    @"insert into Tmp_Pagare (id_solicitud, pagare, pag_seccion_01)
                      values (@operacion, @pagare, @formula01);",
                    new
                    {
                        operacion = request.operacion,
                        pagare = calidadesFiadores,
                        resultado.formula01
                    },
                    transaction);
                transaction.Commit();

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrGeneraGarantiaPreImpresoDto>(
                    ex.Message, -1, new());
            }
        }

        /// <summary>
        /// Valida la operación y normaliza el lugar de firma recibido.
        /// </summary>
        /// <param name="request">Operación y opciones de emisión.</param>
        /// <returns>Mensaje de validación o <see langword="null"/> cuando los datos son válidos.</returns>
        private static string? ValidarOperacion(CrGeneraGarantiaOperacionRequest request)
        {
            if (request.operacion <= 0)
                return "Debe indicar un número de operación válido.";

            request.lugar_firma = (request.lugar_firma ?? string.Empty).Trim();
            return Provincias.Contains(request.lugar_firma, StringComparer.OrdinalIgnoreCase)
                ? null
                : "Debe seleccionar un lugar de firma válido.";
        }

        /// <summary>
        /// Obtiene los datos base de una operación para preparar el pagaré preimpreso.
        /// </summary>
        /// <param name="connection">Conexión empresarial abierta.</param>
        /// <param name="operacion">Número de operación solicitado.</param>
        /// <returns>Datos de la operación o <see langword="null"/> cuando no existe.</returns>
        private static CrGeneraGarantiaOperacionData? ObtenerOperacionPreImpresa(
            SqlConnection connection,
            long operacion) =>
            connection.QueryFirstOrDefault<CrGeneraGarantiaOperacionData>(
                @"select
                    rtrim(codigo) as codigo,
                    rtrim(cedula) as cedula,
                    rtrim(estadosol) as estadosol,
                    rtrim(garantia) as garantia,
                    isnull(montoapr, 0) as montoapr,
                    isnull(plazo, 0) as plazo,
                    isnull([int], 0) as int_corriente
                  from Reg_Creditos
                  where id_solicitud = @operacion;",
                new { operacion });

        /// <summary>
        /// Obtiene la tasa moratoria configurada para el crédito y su rango de monto.
        /// </summary>
        /// <param name="connection">Conexión empresarial abierta.</param>
        /// <param name="codigo">Código del crédito.</param>
        /// <param name="monto">Monto aprobado de la operación.</param>
        /// <returns>Tasa moratoria configurada o cero cuando no existe.</returns>
        private static decimal ObtenerMora(
            SqlConnection connection,
            string codigo,
            decimal monto) =>
            connection.QueryFirstOrDefault<decimal?>(
                @"select top 1 isnull(intm_soc, 0)
                  from Rangos
                  where codigo = @codigo
                    and @monto between [de] and hasta;",
                new { codigo, monto }) ?? 0;

        /// <summary>
        /// Construye las calidades de los primeros tres fiadores activos.
        /// </summary>
        /// <param name="connection">Conexión empresarial abierta.</param>
        /// <param name="operacion">Número de operación.</param>
        /// <param name="usarCedulaReal">Indica si debe utilizarse la cédula real.</param>
        /// <returns>Calidades de los fiadores separadas por punto y coma.</returns>
        private static string ObtenerCalidadesFiadores(
            SqlConnection connection,
            long operacion,
            bool usarCedulaReal)
        {
            var cedulas = connection.Query<string>(
                @"select top 3 rtrim(cedulaf)
                  from Fiadores
                  where estado = 'A'
                    and id_solicitud = @operacion
                  order by cedulaf;",
                new { operacion });

            return string.Join(
                ";",
                cedulas.Select(cedula => ObtenerCalidades(connection, cedula, usarCedulaReal)));
        }

        /// <summary>
        /// Construye las calidades legales de una persona asociada.
        /// </summary>
        /// <param name="connection">Conexión empresarial abierta.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="usarCedulaReal">Indica si debe utilizarse la cédula real.</param>
        /// <returns>Descripción de las calidades personales.</returns>
        private static string ObtenerCalidades(
            SqlConnection connection,
            string cedula,
            bool usarCedulaReal)
        {
            var socio = connection.QueryFirstOrDefault<CrGeneraGarantiaSocioData>(
                @"select
                    rtrim(isnull(S.nombre, '')) as nombre,
                    rtrim(isnull(S.estadocivil, '')) as estado_civil,
                    rtrim(isnull(S.sexo, '')) as sexo,
                    rtrim(isnull(S.cedular, '')) as cedular,
                    rtrim(isnull(Prov.descripcion, '')) as provincia_desc,
                    rtrim(isnull(Cant.descripcion, '')) as canton_desc,
                    rtrim(isnull(Dist.descripcion, '')) as distrito_desc,
                    rtrim(isnull(S.direccion, '')) as direccion
                  from Socios S
                  left join Provincias Prov
                    on S.provincia = Prov.provincia
                  left join Cantones Cant
                    on S.provincia = Cant.provincia
                   and S.canton = Cant.canton
                  left join Distritos Dist
                    on S.provincia = Dist.provincia
                   and S.canton = Dist.canton
                   and S.distrito = Dist.distrito
                  where S.cedula = @cedula;",
                new { cedula });

            if (socio is null)
                return "** calidades **";

            var cedulaMostrar = usarCedulaReal && !string.IsNullOrWhiteSpace(socio.cedular)
                ? socio.cedular.Trim()
                : cedula.Trim();
            var cedulaLetras = ConvertirCedulaALetras(cedulaMostrar);
            var estadoCivil = ObtenerEstadoCivil(socio.estado_civil, socio.sexo);

            return $"{socio.nombre.Trim()}, mayor, {estadoCivil} con cédula de identidad número "
                + $"{cedulaLetras}, con residencia en {socio.provincia_desc}, "
                + $"{socio.canton_desc}  distrito {socio.distrito_desc}, {socio.direccion.Trim()}";
        }

        /// <summary>
        /// Obtiene la descripción del estado civil según el sexo registrado.
        /// </summary>
        /// <param name="estadoCivil">Código de estado civil.</param>
        /// <param name="sexo">Código de sexo.</param>
        /// <returns>Descripción del estado civil.</returns>
        private static string ObtenerEstadoCivil(string estadoCivil, string sexo) =>
            estadoCivil.Trim().ToUpperInvariant() switch
            {
                "S" => sexo.Equals("M", StringComparison.OrdinalIgnoreCase) ? "Soltero" : "Soltera",
                "C" => sexo.Equals("M", StringComparison.OrdinalIgnoreCase) ? "Casado" : "Casada",
                "D" => sexo.Equals("M", StringComparison.OrdinalIgnoreCase) ? "Divorciado" : "Divorciada",
                "V" => sexo.Equals("M", StringComparison.OrdinalIgnoreCase) ? "Viudo" : "Viuda",
                "U" => "Union libre",
                "O" => "Otro",
                _ => estadoCivil
            };

        /// <summary>
        /// Convierte cada dígito de una cédula a su representación en letras.
        /// </summary>
        /// <param name="cedula">Número de identificación.</param>
        /// <returns>Cédula expresada en letras y con los separadores heredados.</returns>
        private static string ConvertirCedulaALetras(string cedula)
        {
            var partes = cedula
                .Where(char.IsDigit)
                .Select((digito, indice) =>
                {
                    var letras = ConvertirNumeroALetras(digito - '0').ToLowerInvariant();
                    return indice is 1 or 5 ? $" - {letras}" : $" {letras}";
                });

            return string.Concat(partes).Trim();
        }

        /// <summary>
        /// Convierte un número a letras mediante el utilitario compartido.
        /// </summary>
        /// <param name="numero">Número por convertir.</param>
        /// <returns>Número expresado en letras.</returns>
        private static string ConvertirNumeroALetras(decimal numero) =>
            MProGrXAuxiliarDB.NumeroALetras(numero).Result?.Trim() ?? numero.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Convierte un monto a letras conservando los céntimos sobre cien.
        /// </summary>
        /// <param name="monto">Monto monetario.</param>
        /// <returns>Monto expresado en letras.</returns>
        private static string ConvertirMontoALetras(decimal monto)
        {
            var entero = decimal.Truncate(monto);
            var centimos = (int)decimal.Round((monto - entero) * 100, 0);
            return $"{ConvertirNumeroALetras(entero)} con {centimos:00}/100";
        }

        /// <summary>
        /// Obtiene el nombre del mes en español de Costa Rica.
        /// </summary>
        /// <param name="mes">Número de mes.</param>
        /// <returns>Nombre localizado del mes.</returns>
        private static string ObtenerMes(int mes) =>
            CultureInfo.GetCultureInfo("es-CR").DateTimeFormat.GetMonthName(mes);
    }
}
