using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public sealed class FrmCrConsultaPlanillaAbonoDistDB
    {
        private const int CodigoValidacion = -2;

        private const string MensajeCedulaRequerida =
            "La cédula de la persona es requerida.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private const string MensajeInstitucionInvalida =
            "La institución deductora no es válida.";

        private const string MensajeProcesoInvalido =
            "El período de créditos no es válido.";

        private const string MensajeMontoInvalido =
            "El monto a distribuir no es válido.";

        private const string MensajeFechaCorteRequerida =
            "La fecha de corte es requerida.";

        private const string MensajeParametrosRequeridos =
            "Los parámetros de la consulta son requeridos.";

        private const string MensajeErrorInicializacion =
            "Ocurrió un error al inicializar la consulta.";

        private const string MensajeErrorPeriodo =
            "No fue posible obtener el período de créditos.";

        private const string MensajeErrorParametrosGenerales =
            "No fue posible obtener los parámetros generales.";

        private const string MensajeErrorUltimoMonto =
            "Ocurrió un error al obtener el último monto enviado.";

        private const string MensajeErrorDistribucion =
            "Ocurrió un error al calcular la distribución del abono.";

        private const string MensajePersonaNoEncontrada =
            "No se encontró la persona indicada.";

        private const string ConsultaSocioSql = """
            select
                isnull(cod_institucion, 0) as cod_institucion,
                rtrim(isnull(cedula, '')) as cedula,
                rtrim(isnull(nombre, '')) as nombre,
                Getdate() as fecha,
                @Proceso as proceso
            from socios
            where cedula = @Cedula;
            """;

        private const string ConsultaDeductorasSql = """
            exec spAFI_Institucion_Vinculadas
                @CodInstitucion,
                3;
            """;

        private const string ConsultaUltimoMontoSql = """
            select
                isnull(sum(Cuota), 0) as monto,
                isnull(max(FecPro), @Proceso) as proceso
            from PRM_ENVIADO_DETALLE
            where COD_INSTITUCION = @CodInstitucion
              and cedula = @Cedula
              and FECPRO in
              (
                  select max(proceso)
                  from PRM_BITACORA
                  where COD_INSTITUCION = @CodInstitucion
                    and GESTION = 'E'
              );
            """;

        private const string ConsultaDistribucionSql = """
            exec spPrmCreditoDetalleAbonos
                @CodInstitucion,
                @Proceso,
                @Cedula,
                @Monto,
                @Corte,
                'S',
                1,
                1,
                1;
            """;

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;

        public FrmCrConsultaPlanillaAbonoDistDB(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene los datos iniciales de la persona, el período de créditos,
        /// la fecha del servidor y las deductoras vinculadas.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa activa.
        /// </param>
        /// <param name="cedula">
        /// Identificación de la persona.
        /// </param>
        /// <param name="usuario">
        /// Usuario autenticado.
        /// </param>
        /// <returns>
        /// Datos iniciales del formulario.
        /// </returns>
        public ErrorDto<CrConsultaPlanillaAbonoDistInicialData>
            CR_ConsultaPlanillaAbonoDist_Inicializar(
                int codEmpresa,
                string cedula,
                string usuario)
        {
            var cedulaNormalizada =
                CR_ConsultaPlanillaAbonoDist_Texto_Normalizar(
                    cedula);

            var usuarioNormalizado =
                CR_ConsultaPlanillaAbonoDist_Texto_Normalizar(
                    usuario);

            var mensajeValidacion =
                CR_ConsultaPlanillaAbonoDist_Inicializacion_Validar(
                    cedulaNormalizada,
                    usuarioNormalizado);

            if (mensajeValidacion is not null)
            {
                return DbHelper.CreateErrorResponse(
                    mensajeValidacion,
                    CodigoValidacion,
                    new CrConsultaPlanillaAbonoDistInicialData());
            }

            var procesoResponse =
                CR_ConsultaPlanillaAbonoDist_Proceso_Obtener(
                    codEmpresa,
                    usuarioNormalizado);

            if (procesoResponse.Code != 0 ||
                procesoResponse.Result <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    procesoResponse.Description ??
                    MensajeErrorPeriodo,
                    procesoResponse.Code.GetValueOrDefault(-1),
                    new CrConsultaPlanillaAbonoDistInicialData());
            }

            var consultaResponse =
                DbHelper.WithConn<
                    CrConsultaPlanillaAbonoDistInicialData?>(
                    _portalDb,
                    codEmpresa,
                    connection =>
                        CR_ConsultaPlanillaAbonoDist_Inicializacion_Ejecutar(
                            connection,
                            cedulaNormalizada,
                            procesoResponse.Result));

            if (consultaResponse.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    consultaResponse.Description ??
                    MensajeErrorInicializacion,
                    consultaResponse.Code.GetValueOrDefault(-1),
                    new CrConsultaPlanillaAbonoDistInicialData());
            }

            if (consultaResponse.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    MensajePersonaNoEncontrada,
                    -1,
                    new CrConsultaPlanillaAbonoDistInicialData());
            }

            return DbHelper.CreateOkResponse(
                consultaResponse.Result);
        }

        /// <summary>
        /// Obtiene el último monto enviado por planilla para la institución
        /// deductora seleccionada.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa activa.
        /// </param>
        /// <param name="cedula">
        /// Identificación de la persona.
        /// </param>
        /// <param name="codInstitucion">
        /// Código de la institución deductora.
        /// </param>
        /// <param name="proceso">
        /// Período utilizado cuando no existen envíos anteriores.
        /// </param>
        /// <returns>
        /// Último monto y período enviado.
        /// </returns>
        public ErrorDto<CrConsultaPlanillaAbonoDistUltimoData>
            CR_ConsultaPlanillaAbonoDist_UltimoMonto(
                int codEmpresa,
                string cedula,
                int codInstitucion,
                int proceso)
        {
            var cedulaNormalizada =
                CR_ConsultaPlanillaAbonoDist_Texto_Normalizar(
                    cedula);

            var mensajeValidacion =
                CR_ConsultaPlanillaAbonoDist_UltimoMonto_Validar(
                    cedulaNormalizada,
                    codInstitucion,
                    proceso);

            if (mensajeValidacion is not null)
            {
                return DbHelper.CreateErrorResponse(
                    mensajeValidacion,
                    CodigoValidacion,
                    new CrConsultaPlanillaAbonoDistUltimoData
                    {
                        proceso = proceso
                    });
            }

            var resultado =
                DbHelper.ExecuteSingleQuery<
                    CrConsultaPlanillaAbonoDistUltimoData>(
                    _portalDb,
                    codEmpresa,
                    ConsultaUltimoMontoSql,
                    new CrConsultaPlanillaAbonoDistUltimoData
                    {
                        proceso = proceso
                    },
                    new
                    {
                        Cedula = cedulaNormalizada,
                        CodInstitucion = codInstitucion,
                        Proceso = proceso
                    });

            if (resultado.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    MensajeErrorUltimoMonto,
                    resultado.Code.GetValueOrDefault(-1),
                    new CrConsultaPlanillaAbonoDistUltimoData
                    {
                        proceso = proceso
                    });
            }

            return DbHelper.CreateOkResponse(
                resultado.Result ??
                new CrConsultaPlanillaAbonoDistUltimoData
                {
                    proceso = proceso
                });
        }

        /// <summary>
        /// Calcula la distribución preliminar del monto entre las operaciones
        /// de crédito de la persona.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa activa.
        /// </param>
        /// <param name="request">
        /// Parámetros requeridos para calcular la distribución.
        /// </param>
        /// <returns>
        /// Distribución calculada por operación.
        /// </returns>
        public ErrorDto<List<CrConsultaPlanillaAbonoDistDetalleData>>
            CR_ConsultaPlanillaAbonoDist_Consultar(
                int codEmpresa,
                CrConsultaPlanillaAbonoDistConsultaRequest? request)
        {
            if (request is null)
            {
                return
                    CR_ConsultaPlanillaAbonoDist_Consulta_Error_Crear(
                        MensajeParametrosRequeridos);
            }

            request.cedula =
                CR_ConsultaPlanillaAbonoDist_Texto_Normalizar(
                    request.cedula);

            var mensajeValidacion =
                CR_ConsultaPlanillaAbonoDist_Consulta_Validar(
                    request);

            if (mensajeValidacion is not null)
            {
                return
                    CR_ConsultaPlanillaAbonoDist_Consulta_Error_Crear(
                        mensajeValidacion);
            }

            var consultaResponse =
                DbHelper.ExecuteListQuery<
                    PlanillaAbonoDistribucionData>(
                    _portalDb,
                    codEmpresa,
                    ConsultaDistribucionSql,
                    new
                    {
                        CodInstitucion =
                            request.cod_institucion.GetValueOrDefault(),
                        Proceso =
                            request.proceso.GetValueOrDefault(),
                        Cedula = request.cedula,
                        Monto =
                            request.monto.GetValueOrDefault(),
                        Corte =
                            request.corte.GetValueOrDefault().Date
                    });

            if (consultaResponse.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    consultaResponse.Description ??
                    MensajeErrorDistribucion,
                    consultaResponse.Code.GetValueOrDefault(-1),
                    new List<
                        CrConsultaPlanillaAbonoDistDetalleData>());
            }

            var detalle =
                (consultaResponse.Result ??
                 new List<PlanillaAbonoDistribucionData>())
                .Select(
                    CR_ConsultaPlanillaAbonoDist_Detalle_Crear)
                .ToList();

            return DbHelper.CreateOkResponse(detalle);
        }

        /// <summary>
        /// Ejecuta las consultas requeridas para inicializar el formulario.
        /// </summary>
        /// <param name="connection">
        /// Conexión correspondiente a la empresa activa.
        /// </param>
        /// <param name="cedula">
        /// Identificación normalizada.
        /// </param>
        /// <param name="proceso">
        /// Período actual de créditos.
        /// </param>
        /// <returns>
        /// Datos iniciales o null cuando la persona no existe.
        /// </returns>
        private static CrConsultaPlanillaAbonoDistInicialData?
            CR_ConsultaPlanillaAbonoDist_Inicializacion_Ejecutar(
                SqlConnection connection,
                string cedula,
                int proceso)
        {
            var datos =
                connection.QueryFirstOrDefault<
                    CrConsultaPlanillaAbonoDistInicialData>(
                    ConsultaSocioSql,
                    new
                    {
                        Cedula = cedula,
                        Proceso = proceso
                    });

            if (datos is null)
            {
                return null;
            }

            datos.deductoras =
                CR_ConsultaPlanillaAbonoDist_Deductoras_Obtener(
                    connection,
                    datos.cod_institucion);

            return datos;
        }

        /// <summary>
        /// Obtiene el período de créditos desde los parámetros globales
        /// inicializados por el API.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa activa.
        /// </param>
        /// <param name="usuario">
        /// Usuario autenticado.
        /// </param>
        /// <returns>
        /// Período de créditos en formato AAAAMM.
        /// </returns>
        private ErrorDto<int>
            CR_ConsultaPlanillaAbonoDist_Proceso_Obtener(
                int codEmpresa,
                string usuario)
        {
            var globalesResponse =
                _mProGrxMain.sbSifParametrosInicializa(
                    codEmpresa,
                    usuario);

            if (globalesResponse.Code != 0 ||
                globalesResponse.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResponse.Description ??
                    MensajeErrorParametrosGenerales,
                    globalesResponse.Code.GetValueOrDefault(-1),
                    0);
            }

            var proceso = globalesResponse.Result.GlngFechaCR;

            if (proceso <= 0 ||
                proceso > int.MaxValue ||
                proceso != decimal.Truncate(proceso))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeProcesoInvalido,
                    CodigoValidacion,
                    0);
            }

            return DbHelper.CreateOkResponse(
                decimal.ToInt32(proceso));
        }

        /// <summary>
        /// Obtiene las instituciones deductoras vinculadas a la institución
        /// principal de la persona.
        /// </summary>
        /// <param name="connection">
        /// Conexión correspondiente a la empresa activa.
        /// </param>
        /// <param name="codInstitucion">
        /// Institución principal.
        /// </param>
        /// <returns>
        /// Instituciones deductoras vinculadas.
        /// </returns>
        private static List<DropDownListaGenericaModel>
            CR_ConsultaPlanillaAbonoDist_Deductoras_Obtener(
                SqlConnection connection,
                int codInstitucion)
        {
            return connection
                .Query<PlanillaAbonoDeductoraData>(
                    ConsultaDeductorasSql,
                    new
                    {
                        CodInstitucion = codInstitucion
                    })
                .Select(
                    registro =>
                        new DropDownListaGenericaModel
                        {
                            item = registro.idx.ToString(
                                CultureInfo.InvariantCulture),
                            descripcion =
                                CR_ConsultaPlanillaAbonoDist_Texto_Normalizar(
                                    registro.itmx)
                        })
                .ToList();
        }

        /// <summary>
        /// Homologa una fila devuelta por el procedimiento almacenado
        /// con el contrato público del formulario.
        /// </summary>
        /// <param name="registro">
        /// Fila devuelta por el procedimiento de distribución.
        /// </param>
        /// <returns>
        /// Detalle homologado de distribución.
        /// </returns>
        private static CrConsultaPlanillaAbonoDistDetalleData
            CR_ConsultaPlanillaAbonoDist_Detalle_Crear(
                PlanillaAbonoDistribucionData registro)
        {
            return new CrConsultaPlanillaAbonoDistDetalleData
            {
                operacion = registro.Operacion,
                linea =
                    CR_ConsultaPlanillaAbonoDist_Texto_Normalizar(
                        registro.Linea),
                tipo =
                    CR_ConsultaPlanillaAbonoDist_Texto_Normalizar(
                        registro.Tipo),
                proceso = registro.Proceso,
                ab_int_cor = registro.AbIntCor,
                ab_int_mor = registro.AbIntMor,
                ab_cargo = registro.AbCargo,
                ab_amortiza = registro.AbAmortiza,
                int_cor = registro.IntCor,
                int_mor = registro.IntMor,
                cargo = registro.Cargo,
                amortiza = registro.Amortiza,
                orden = registro.Orden
            };
        }

        /// <summary>
        /// Valida los parámetros requeridos durante la inicialización.
        /// </summary>
        /// <param name="cedula">
        /// Identificación normalizada.
        /// </param>
        /// <param name="usuario">
        /// Usuario normalizado.
        /// </param>
        /// <returns>
        /// Mensaje de validación o null.
        /// </returns>
        private static string?
            CR_ConsultaPlanillaAbonoDist_Inicializacion_Validar(
                string cedula,
                string usuario)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return MensajeCedulaRequerida;
            }

            return string.IsNullOrWhiteSpace(usuario)
                ? MensajeUsuarioRequerido
                : null;
        }

        /// <summary>
        /// Valida los parámetros de la consulta del último monto.
        /// </summary>
        /// <param name="cedula">
        /// Identificación normalizada.
        /// </param>
        /// <param name="codInstitucion">
        /// Institución deductora.
        /// </param>
        /// <param name="proceso">
        /// Período de créditos.
        /// </param>
        /// <returns>
        /// Mensaje de validación o null.
        /// </returns>
        private static string?
            CR_ConsultaPlanillaAbonoDist_UltimoMonto_Validar(
                string cedula,
                int codInstitucion,
                int proceso)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return MensajeCedulaRequerida;
            }

            if (codInstitucion <= 0)
            {
                return MensajeInstitucionInvalida;
            }

            return proceso <= 0
                ? MensajeProcesoInvalido
                : null;
        }

        /// <summary>
        /// Valida los parámetros de la consulta de distribución.
        /// </summary>
        /// <param name="request">
        /// Parámetros normalizados.
        /// </param>
        /// <returns>
        /// Mensaje de validación o null.
        /// </returns>
        private static string?
            CR_ConsultaPlanillaAbonoDist_Consulta_Validar(
                CrConsultaPlanillaAbonoDistConsultaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return MensajeCedulaRequerida;
            }

            if (request.cod_institucion is null ||
                request.cod_institucion <= 0)
            {
                return MensajeInstitucionInvalida;
            }

            if (request.proceso is null ||
                request.proceso <= 0)
            {
                return MensajeProcesoInvalido;
            }

            if (request.monto is null ||
                request.monto < 0)
            {
                return MensajeMontoInvalido;
            }

            return request.corte is null
                ? MensajeFechaCorteRequerida
                : null;
        }

        /// <summary>
        /// Construye una respuesta de validación para la distribución.
        /// </summary>
        /// <param name="mensaje">
        /// Mensaje que debe devolverse.
        /// </param>
        /// <returns>
        /// Respuesta de error con una lista vacía.
        /// </returns>
        private static ErrorDto<
            List<CrConsultaPlanillaAbonoDistDetalleData>>
            CR_ConsultaPlanillaAbonoDist_Consulta_Error_Crear(
                string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                CodigoValidacion,
                new List<
                    CrConsultaPlanillaAbonoDistDetalleData>());
        }

        /// <summary>
        /// Normaliza un texto recibido por el proceso.
        /// </summary>
        /// <param name="valor">
        /// Texto de entrada.
        /// </param>
        /// <returns>
        /// Texto sin espacios externos.
        /// </returns>
        private static string
            CR_ConsultaPlanillaAbonoDist_Texto_Normalizar(
                string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        private sealed class PlanillaAbonoDeductoraData
        {
            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public int idx { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public string itmx { get; set; } = string.Empty;
        }

        private sealed class PlanillaAbonoDistribucionData
        {
            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public long Operacion { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public string Linea { get; set; } = string.Empty;

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public string Tipo { get; set; } = string.Empty;

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public int Proceso { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public decimal AbIntCor { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public decimal AbIntMor { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public decimal AbCargo { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public decimal AbAmortiza { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public decimal IntCor { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public decimal IntMor { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public decimal Cargo { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public decimal Amortiza { get; set; }

            [SuppressMessage(
                "Minor Code Smell",
                "S3459:Unassigned members should be removed",
                Justification =
                    "Dapper asigna la propiedad desde el procedimiento almacenado.")]
            public int Orden { get; set; }
        }
    }
}