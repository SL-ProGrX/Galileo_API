using System.Diagnostics.CodeAnalysis;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;
using System.Data;
using System.Linq;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public partial class FrmCRConsultaCreditosDB
    {
        private readonly IConfiguration _config;
        private readonly MProGrxMain _mProGrx_Main;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MAfilicacionDB _mAfilicacionDB;
        private const string FormatoFechaIso = "yyyy-MM-dd";
        private const string MensajeOperacionRealizadaCorrectamente = "Operación realizada correctamente";

        public FrmCRConsultaCreditosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mProGrx_Main = new MProGrxMain(_config);
            _Security_MainDB = new MSecurityMainDb(_config);
            _mAfilicacionDB = new MAfilicacionDB(_config);
        }

        /// <summary>
        /// Consulta los tipos de garantía disponibles para el formulario en la tabla CRD_GARANTIA_TIPOS.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que define la conexión de consulta.</param>
        /// <returns>Listado de tipos de garantía disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ConsultaCrdGarantiaTipo_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"select GARANTIA as item,
                         rtrim(DESCRIPCION) as descripcion
                  from CRD_GARANTIA_TIPOS
                  where FORMULARIO = 'F01'
                  order by Garantia");
        }

        /// <summary>
        /// Consulta los socios disponibles para el formulario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que define la conexión de consulta.</param>
        /// <returns>Listado de socios disponibles para búsqueda.</returns>
        public ErrorDto<List<CrConsultaCrdSociosData>> CR_ConsultaCrdSocios_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<CrConsultaCrdSociosData>(
                CreatePortalDb(),
                CodEmpresa,
                "Select cedula, cedular, nombre from SOCIOS");
        }

        /// <summary>
        /// Resuelve el criterio de consulta como cédula o número de operación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que define la conexión de consulta.</param>
        /// <param name="criterio">Cédula o número de operación digitado.</param>
        /// <returns>Cédula que debe utilizar la consulta integrada.</returns>
        public ErrorDto<string> CR_ConsultaCrdCriterio_Resolver(
            int CodEmpresa,
            string criterio)
        {
            var criterioNormalizado = (criterio ?? string.Empty).Trim();
            long? operacion = long.TryParse(criterioNormalizado, out var numeroOperacion)
                ? numeroOperacion
                : null;

            const string sql = @"
                SELECT TOP 1
                    candidato.cedula
                FROM (
                    SELECT socios.cedula, 0 AS prioridad
                    FROM SOCIOS AS socios
                    WHERE socios.cedula = @criterio

                    UNION ALL

                    SELECT creditos.cedula, 1 AS prioridad
                    FROM REG_CREDITOS AS creditos
                    WHERE @operacion IS NOT NULL
                      AND creditos.ID_SOLICITUD = @operacion
                      AND NOT EXISTS (
                          SELECT 1
                          FROM SOCIOS
                          WHERE cedula = @criterio
                      )
                ) AS candidato
                ORDER BY candidato.prioridad";

            return DbHelper.ExecuteSingleQuery<string>(
                CreatePortalDb(),
                CodEmpresa,
                sql,
                string.Empty,
                new { criterio = criterioNormalizado, operacion });
        }

        /// <summary>
        /// Consulta los datos de la persona para el formulario de consulta integrada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que define la conexión de consulta.</param>
        /// <param name="cedula">Identificación de la persona consultada.</param>
        /// <param name="usuario">Usuario que realiza la consulta.</param>
        /// <returns>Contexto integrado de la persona consultada.</returns>
        public ErrorDto<CrConsultaCrdData> CR_ConsultaCrdConsulta_Integrada_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            var cedulaNormalizada = (cedula ?? string.Empty).Trim();
            var usuarioNormalizado = usuario ?? string.Empty;

            var validaCadena = _mProGrx_Main.fxSIFValidaCadena(cedulaNormalizada);
            if (validaCadena.Code == -1)
            {
                return DbHelper.CreateErrorResponse(
                    validaCadena.Description ?? "La identificación consultada no es válida.",
                    validaCadena.Code.GetValueOrDefault(-1),
                    new CrConsultaCrdData());
            }

            var vRA_Access = _mProGrx_Main.fxSys_RA_Consulta(CodEmpresa, cedulaNormalizada, usuarioNormalizado);
            if (!vRA_Access.Result)
            {
                return DbHelper.CreateErrorResponse(
                    "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorización para Consultar!",
                    -1,
                    new CrConsultaCrdData());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var persona = connection.QueryFirstOrDefault<CrConsultaCrdData>(
                    "spSys_Consulta_Integrada",
                    new { cedula = cedulaNormalizada },
                    commandType: CommandType.StoredProcedure);

                if (persona is null)
                {
                    return DbHelper.CreateErrorResponse("No se encontró información de la persona.", -1, new CrConsultaCrdData());
                }

                PrepararConsultaIntegrada(connection, CodEmpresa, cedulaNormalizada, persona);
                return DbHelper.CreateOkResponse(persona);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar información integrada.", result.Code.GetValueOrDefault(-1), new CrConsultaCrdData());
        }

        /// <summary>
        /// Obtiene la causa de liquidación más reciente de un socio.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que define la conexión de consulta.</param>
        /// <param name="cedula">Identificación de la persona consultada.</param>
        /// <returns>Descripción de la causa de liquidación o una cadena vacía.</returns>
        private string fxLiquidacion(int CodEmpresa, string cedula)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<string>(
                    @"select C.descripcion
                      from liquidacion L
                      inner join Causas_Renuncias C on C.id_causa = L.id_causa
                      where consec in(
                          select max(consec)
                          from liquidacion
                          where cedula = @cedula)",
                    new { cedula }));

            if (result.Code != 0 || string.IsNullOrWhiteSpace(result.Result))
            {
                return string.Empty;
            }

            return $"[CAUSA: {result.Result}]";
        }

        /// <summary>
        /// Método actualiza nota socio.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que define la conexión.</param>
        /// <param name="cedula">Identificación del socio.</param>
        /// <param name="nota">Texto de la nota que se registrará.</param>
        /// <param name="usuario">Usuario que registra la nota.</param>
        /// <returns>Resultado de la operación de registro.</returns>
        public ErrorDto CR_Socios_RegistrarNota(int CodEmpresa, string cedula, string nota, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string query = @"
                UPDATE socios
                   SET notas = UPPER(LTRIM(RTRIM(@Nota))),
                       Nota_User = @Usuario,
                       Nota_Fecha = dbo.MyGetdate()
                 WHERE cedula = @Cedula;

                INSERT INTO socios_mensajes (fecha, cedula, usuario, vencimiento, mensaje, tipo)
                VALUES (dbo.MyGetdate(), @Cedula, @Usuario, '2100-01-01', @Nota, 'G');";

                connection.Execute(query, new
                {
                    Cedula = cedula,
                    Usuario = usuario,
                    Nota = nota
                });

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar nota del socio.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Registra el bloqueo o desbloqueo de nuevos créditos para una persona.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que define la conexión.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="bloqueo">Indica si se activa o se elimina el bloqueo.</param>
        /// <param name="nota">Nota asociada al cambio de indicador.</param>
        /// <param name="usuario">Usuario que registra el cambio.</param>
        /// <returns>Resultado del registro del indicador.</returns>
        public ErrorDto CR_Socios_BloqueoCreditos_Guardar(
            int CodEmpresa,
            string cedula,
            bool bloqueo,
            string nota,
            string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Cedula", (cedula ?? string.Empty).Trim());
                parameters.Add("@Indicador", 19);
                parameters.Add("@Valor", bloqueo ? 1 : 0);
                parameters.Add("@Usuario", (usuario ?? string.Empty).Trim());
                parameters.Add("@Nota", (nota ?? string.Empty).Trim());

                connection.Execute(
                    "spAFI_Persona_Indicadores",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(
                    result.Description ?? "Error al registrar el bloqueo de créditos.",
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Consulta el saldo a favor disponible para una persona.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que define la conexión.</param>
        /// <param name="cedula">Identificación de la persona consultada.</param>
        /// <returns>Saldo a favor disponible.</returns>
        public ErrorDto<decimal> fxCajas_SaldoaFavor(int CodEmpresa, string cedula)
        {
            var result = DbHelper.ExecuteSingleQuery<decimal>(
                CreatePortalDb(),
                CodEmpresa,
                "select dbo.fxCajas_SaldoaFavor(@cedula) as Cajas_Saldo_Favor",
                0m,
                new { cedula });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar saldo a favor.", result.Code.GetValueOrDefault(-1), 0m);
        }













        /// <summary>
        /// Completa los indicadores y textos derivados de la consulta integrada.
        /// </summary>
        /// <param name="connection">Conexión activa de la empresa.</param>
        /// <param name="codEmpresa">Código de la empresa consultada.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="persona">Modelo que recibirá los datos derivados.</param>
        private void PrepararConsultaIntegrada(SqlConnection connection, int codEmpresa, string cedula, CrConsultaCrdData persona)
        {
            persona.vMora = false;
            ConfigurarMembresia(connection, codEmpresa, cedula, persona);
            ConfigurarIndicadoresConsultaIntegrada(persona);
            ConfigurarConsentimiento(persona);
            CargarMensajesPersona(connection, cedula, persona);
            persona.pat_tipoSaldo = "Saldos en Garantía";
            ConfigurarMora(codEmpresa, cedula, persona);
        }

        /// <summary>
        /// Configura las leyendas de membresía y renuncia de la persona.
        /// </summary>
        /// <param name="connection">Conexión activa de la empresa.</param>
        /// <param name="codEmpresa">Código de la empresa consultada.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="persona">Modelo que recibirá las leyendas.</param>
        private void ConfigurarMembresia(
            SqlConnection connection,
            int codEmpresa,
            string cedula,
            CrConsultaCrdData persona)
        {
            var fechaIngreso = persona.fechaingreso ?? DateTime.Now;
            persona.membresiaCaption = "Membresía: NADA";
            persona.membresiaToolTip = fxLiquidacion(codEmpresa, cedula);

            if (persona.estadoactual != "S")
            {
                return;
            }

            persona.membresiaCaption = "Membresía: " + MCredito.fxMembresia(fechaIngreso);
            persona.membresiaToolTip = "[Ing.:" + fechaIngreso.ToString("dd/MM/yyyy") + "]";

            var renuncia = connection.QueryFirstOrDefault<CrConsultaCrdData>(
                "spAFI_ConsultaRenunciaTransito",
                new { cedula },
                commandType: CommandType.StoredProcedure);

            if (renuncia is null)
            {
                return;
            }

            persona.membresiaCaption =
                $"Renuncia: {renuncia.cod_Renuncia} ¦ {renuncia.registro_fecha} ¦ {renuncia.registro_user}";
            persona.membresiaToolTip =
                $"{renuncia.estado} ¦ {renuncia.tipo} ¦ {renuncia.descripcion}";
        }

        /// <summary>
        /// Configura los textos derivados de indicadores de la consulta integrada.
        /// </summary>
        /// <param name="persona">Modelo que recibirá los textos derivados.</param>
        private static void ConfigurarIndicadoresConsultaIntegrada(CrConsultaCrdData persona)
        {
            var estadoPersona = persona.estadoactual == "S"
                ? "Asociado"
                : "No Asociado";
            persona.estadox = string.IsNullOrWhiteSpace(persona.estadox)
                ? estadoPersona
                : persona.estadox.Trim();
            persona.institucionx = string.IsNullOrWhiteSpace(persona.institucionx)
                ? "Empresa/Deductora?"
                : persona.institucionx.Trim();
            persona.clasificacionCaption =
                $"Clasificación Crediticia : [{(string.IsNullOrWhiteSpace(persona.clasificacion) ? "?" : persona.clasificacion.Trim())}]";
            persona.salarioTrasladaCaption = persona.salario_traslada == 1
                ? "Traslada Salario: Sí"
                : "Sin Tramite (Traslado Salario)";
            persona.patrimonio =
                persona.ahorro + persona.aporte + persona.custodia + persona.capitaliza;
            persona.tarjetaCaption =
                $"Tarjeta: {(string.IsNullOrWhiteSpace(persona.tarjeta_numero) ? "No" : persona.tarjeta_numero.Trim())}";
            persona.ibanCaption =
                $"IBAN: {(string.IsNullOrWhiteSpace(persona.iban) ? "No" : persona.iban.Trim())}";
            persona.estadoMensajesCaption = persona.indmensajes == 0
                ? "Mensajes ?"
                : $"Mensajes ({persona.indmensajes})";
            persona.estadoCobrosCaption = persona.indcobro == 0
                ? "Sin Gestión de Cobro"
                : $"Gestiones de Cobro ({persona.indcobro})";
            persona.estadoAdvertenciaCaption = persona.indadvertencias == 0
                ? "Sin Advertencias"
                : $"Advertencias ({persona.indadvertencias})";

            if (!string.IsNullOrWhiteSpace(persona.pat_advertencia))
            {
                persona.estadoAdvertenciaCaption = "Advertencia de Aportes no cotizados";
            }

            persona.fianzasCaption = persona.indfianzas == false
                ? "Fianzas al Día"
                : "Fianzas en Mora";
        }

        /// <summary>
        /// Configura el detalle del consentimiento de contacto.
        /// </summary>
        /// <param name="persona">Modelo que recibirá el detalle del consentimiento.</param>
        private static void ConfigurarConsentimiento(CrConsultaCrdData persona)
        {
            if (persona.consentimiento_contacto_fecha is DateTime fecha)
            {
                persona.estadoConsentimientoToolTip =
                    $"Fecha : {fecha:dd/MM/yyyy} | Usuario: {persona.consentimiento_contacto_usuario}";
                return;
            }

            persona.estadoConsentimientoToolTip = string.Empty;
            persona.consentimiento_contacto_usuario = null;
        }

        /// <summary>
        /// Configura el indicador de mora a partir de las operaciones vigentes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa consultada.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="persona">Modelo que recibirá el indicador de mora.</param>
        private void ConfigurarMora(
            int codEmpresa,
            string cedula,
            CrConsultaCrdData persona)
        {
            var listCredito = CR_ConsultaCrd_Creditos_Obtener(codEmpresa, cedula, "A");
            var credito = (listCredito.Result ?? new List<CrConsultaCrdCreditosData>())
                .FirstOrDefault(c => c.procesoCod == "J" || (c.moraCuota ?? 0) > 0);

            if (credito is null)
            {
                return;
            }

            persona.vMora = true;
            persona.vMoraCaption = credito.procesoCod == "J"
                ? $">> Cobro Judicial << | Fecha : {credito.fecha_enviaProceso} | Nota : {credito.observacion_proceso}"
                : $"Morosidad: {credito.moraCuota ?? 0} cuota(s)";
        }

        /// <summary>
        /// Carga los contadores y leyendas de mensajes asociados a la persona.
        /// </summary>
        /// <param name="connection">Conexión activa de la empresa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="persona">Modelo que recibirá los indicadores de mensajes.</param>
        private static void CargarMensajesPersona(SqlConnection connection, string cedula, CrConsultaCrdData persona)
        {
            persona.indmensajes = connection.QuerySingleOrDefault<int?>(
                "SELECT dbo.fxSIFMensajesNumero(@cedula)",
                new { cedula }) ?? 0;

            var mensajes = connection.QueryFirstOrDefault<CrConsultaCrdData>(
                "spSIFPersonaMensajes",
                new { cedula },
                commandType: CommandType.StoredProcedure);

            if (mensajes != null)
            {
                persona.pendientes = mensajes.pendientes;
                persona.advertencias = mensajes.advertencias;
                persona.generales = mensajes.generales;
                persona.morosidad = mensajes.morosidad;
                persona.bloqueos = mensajes.bloqueos;
            }

            persona.pendientesCaption = persona.pendientes > 0 ? $"Pendientes ({persona.pendientes})" : "Msj. Pendientes?";
            persona.advertenciasCaption = persona.advertencias > 0 ? $"Advertencias ({persona.advertencias})" : "Msj Advertencias?";
            persona.generalesCaption = persona.generales > 0 ? $"General ({persona.generales})" : "Msj Generales?";
            persona.morosidadCaption = persona.morosidad > 0 ? $"Morosidad ({persona.morosidad})" : "Msj Morosidad?";
            persona.bloqueosCaption = persona.bloqueos > 0 ? $"Bloqueos ({persona.bloqueos})" : "Msj Bloqueos?";
            persona.estadoMensajesCaption = persona.indmensajes > 0
                ? $"Mensajes ({persona.indmensajes})"
                : "Mensajes ?";
        }

        /// <summary>
        /// Calcula el patrimonio disponible y los saldos asociados a un tipo de garantía.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que define la conexión.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="garantia">Código del tipo de garantía.</param>
        /// <returns>Totales de patrimonio y saldos comprometidos.</returns>
        public ErrorDto<CrPatrimonioGarantiaData?> CR_Patrimonio_Garantia_Obtener(
            int codEmpresa,
            string cedula,
            string garantia)
        {
            return DbHelper.ExecuteSingleQuery<CrPatrimonioGarantiaData>(
                CreatePortalDb(),
                codEmpresa,
                @"SELECT
                    dbo.fxCrdGarantiaPatMnt(S.Cedula, @Garantia, 'M') AS pat_garantia_total,
                    dbo.fxCrdGarantiaPatMnt(S.Cedula, @Garantia, 'S')
                      + dbo.fxCrdGarantiaPatMnt_SldTramite(S.Cedula, 'A') AS pat_garantia_saldos
                  FROM Socios S
                  WHERE S.Cedula = @Cedula;",
                null,
                new { Cedula = cedula, Garantia = garantia });
        }

        /// <summary>
        /// Calcula el siguiente período mensual en formato AAAAMM.
        /// </summary>
        /// <param name="proceso">Período actual en formato AAAAMM.</param>
        /// <returns>Siguiente período mensual.</returns>
        private static int SiguienteProceso(int proceso)
        {
            var anio = proceso / 100;
            var mes = proceso % 100;
            return mes >= 12
                ? (anio + 1) * 100 + 1
                : anio * 100 + mes + 1;
        }

        private sealed class CrConsultaCancelacionLegacyRow
        {
            [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Dapper asigna esta propiedad por reflexión desde una columna de consulta.")]
            public decimal saldo { get; set; }
            [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Dapper asigna esta propiedad por reflexión desde una columna de consulta.")]
            public decimal interesv { get; set; }
            [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Dapper asigna esta propiedad por reflexión desde una columna de consulta.")]
            public int fecUlt { get; set; }
            [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Dapper asigna esta propiedad por reflexión desde una columna de consulta.")]
            public decimal intMora { get; set; }
            [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Dapper asigna esta propiedad por reflexión desde una columna de consulta.")]
            public decimal cargos { get; set; }
            [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Dapper asigna esta propiedad por reflexión desde una columna de consulta.")]
            public int moraCuota { get; set; }
            [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Dapper asigna esta propiedad por reflexión desde una columna de consulta.")]
            public decimal principalAtrasado { get; set; }
            [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Dapper asigna esta propiedad por reflexión desde una columna de consulta.")]
            public int priDeduc { get; set; }
        }

        private ErrorDto<List<T>> EjecutarStoredProcedureList<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al ejecutar procedimiento almacenado.", result.Code.GetValueOrDefault(-1), new List<T>());
        }

        /// <summary>
        /// Normaliza un código compuesto para conservar dos dígitos en su parte entera.
        /// </summary>
        /// <param name="codigo">Código que se normalizará.</param>
        /// <returns>Código normalizado.</returns>
        private static string FormatearCodigoCompuesto(string? codigo)
        {
            var valor = codigo ?? string.Empty;
            if (valor.Contains('.'))
            {
                var partes = valor.Split('.');
                var entero = partes[0].PadLeft(2, '0');
                return $"{entero}.{partes[1]}";
            }

            return valor.PadLeft(2, '0');
        }

        /// <summary>
        /// Crea el acceso a datos configurado para la empresa solicitada.
        /// </summary>
        /// <returns>Instancia configurada de acceso al portal.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}
