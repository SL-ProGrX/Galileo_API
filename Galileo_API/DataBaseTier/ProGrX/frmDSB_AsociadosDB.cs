using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;
using System.Data;
using System.Globalization;

namespace Galileo.DataBaseTier.ProGrX
{
    public class FrmDsbAsociadosDB
    {
        private const int ErrorValidacion = -1;
        private const int ErrorAcceso = -2;
        private const string MensajeExpedienteRestringido =
            "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorización para Consultar!";
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _main;

        public FrmDsbAsociadosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _main = new MProGrxMain(config);
        }

        public ErrorDto<DashboardAsociadosData> Asociado_Obtener(
            int codEmpresa,
            string cedula,
            string usuario)
        {
            var identificacion = (cedula ?? string.Empty).Trim();
            var usuarioConsulta = (usuario ?? string.Empty).Trim();
            var respuestaVacia = new DashboardAsociadosData();

            if (string.IsNullOrWhiteSpace(identificacion) || identificacion.Length > 20)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar una identificación válida de máximo 20 caracteres.",
                    ErrorValidacion,
                    respuestaVacia);
            }

            if (string.IsNullOrWhiteSpace(usuarioConsulta))
            {
                return DbHelper.CreateErrorResponse(
                    "No fue posible identificar al usuario.",
                    ErrorAcceso,
                    respuestaVacia);
            }

            var validacion = _main.fxSIFValidaCadena(identificacion);
            if (validacion.Code == ErrorValidacion)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? "La identificación consultada no es válida.",
                    ErrorValidacion,
                    respuestaVacia);
            }

            var acceso = _main.fxSys_RA_Consulta(codEmpresa, identificacion, usuarioConsulta);
            if (!acceso.Result)
            {
                return DbHelper.CreateErrorResponse(
                    acceso.Code == 0
                        ? MensajeExpedienteRestringido
                        : acceso.Description ?? MensajeExpedienteRestringido,
                    ErrorAcceso,
                    respuestaVacia);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var asociado = connection.QueryFirstOrDefault<DashboardAsociadosData>(
                    "spDSB_Persona_Consulta",
                    new { Cedula = identificacion },
                    commandType: CommandType.StoredProcedure);

                if (asociado is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró registro de la persona solicitada.",
                        ErrorValidacion,
                        respuestaVacia);
                }

                CompletarMembresia(connection, identificacion, asociado);
                asociado.EstadoBeneficiariosToolTip = $"Fecha   .: {FechaTooltip(asociado.Ben_Update_Fecha)}\nUsuario .: {asociado.Ben_Update_Usuario ?? string.Empty}";
                asociado.EstadoConsentimientoToolTip = $"Fecha   .: {FechaTooltip(asociado.Consentimiento_Contacto_Fecha)}\nUsuario .: {asociado.Consentimiento_Contacto_Usuario ?? string.Empty}";
                asociado.Patrimonio = ObtenerSerie(connection, identificacion, "PAT");
                asociado.Creditos = ObtenerSerie(connection, identificacion, "CRD");
                asociado.Fondos = ObtenerSerie(connection, identificacion, "FND");
                asociado.Beneficios = ObtenerSerie(connection, identificacion, "BEN");

                return DbHelper.CreateOkResponse(asociado);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, respuestaVacia);
            }
        }

        private static List<DashboardAsociadosPuntoData> ObtenerSerie(
            IDbConnection connection,
            string cedula,
            string tipo)
            => connection.Query<DashboardAsociadosPuntoData>(
                "spDSB_Asociados_Series",
                new { Cedula = cedula, Tipo = tipo },
                commandType: CommandType.StoredProcedure).ToList();

        private static void CompletarMembresia(
            IDbConnection connection,
            string cedula,
            DashboardAsociadosData asociado)
        {
            if (string.Equals(asociado.EstadoActual, "S", StringComparison.OrdinalIgnoreCase))
            {
                asociado.MembresiaCaption = $"Membresía: {asociado.Membresia ?? string.Empty}";
                asociado.FechaIngresoReferencia = asociado.FechaIngreso
                    ?? connection.QueryFirstOrDefault<DateTime?>("select dbo.MyGetdate()");
                asociado.MembresiaToolTip = $"[Ing.: {FechaTooltip(asociado.FechaIngresoReferencia)}]";

                var renuncia = connection.QueryFirstOrDefault<DashboardAsociadosRenunciaData>(
                    "spAFI_ConsultaRenunciaTransito",
                    new { Cedula = cedula },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0);

                if (renuncia is not null)
                {
                    asociado.MembresiaEsRenuncia = true;
                    asociado.MembresiaCaption = $"Renuncia: {renuncia.Cod_Renuncia} ¦ {FechaTooltip(renuncia.Registro_fecha)} ¦ {renuncia.registro_user}";
                    asociado.MembresiaToolTip = $"{renuncia.Estado} ¦ {renuncia.Tipo} ¦ {renuncia.Descripcion?.Trim()}";
                }
                return;
            }

            asociado.MembresiaCaption = "Membresía: NADA";
            var causa = connection.QueryFirstOrDefault<string>(
                """
                select C.descripcion
                from liquidacion L
                inner join Causas_Renuncias C on C.id_causa = L.id_causa
                where L.consec in(
                    select max(consec)
                    from liquidacion
                    where cedula = @Cedula)
                """,
                new { Cedula = cedula });
            asociado.MembresiaToolTip = string.IsNullOrWhiteSpace(causa)
                ? string.Empty
                : $"[CAUSA: {causa.Trim()}]";
        }

        private static string FechaTooltip(DateTime? fecha)
            => fecha.HasValue
                ? fecha.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("es-CR"))
                : string.Empty;
    }
}
