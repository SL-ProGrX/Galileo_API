using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFCrAutorizacionesDB
    {
        private readonly IConfiguration _config;

        private const string SqlAutorizacionesConsulta = @"
                    SELECT R.*,
                           S.Nombre,
                           ISNULL(R.autorizado_estado, 0) AS AutorizacionX
                    FROM dbo.afi_cr_renuncias R
                    INNER JOIN dbo.Socios S
                        ON R.cedula = S.cedula
                    WHERE R.resuelto_fecha BETWEEN @Inicio AND @Corte
                      AND R.Estado = 'R'
                      AND (@EstadoAutorizacion = '' OR
                          (@EstadoAutorizacion = 'A' AND R.autorizado_estado = 1) OR
                          (@EstadoAutorizacion = 'P' AND ISNULL(R.autorizado_estado, 0) = 0) OR
                          (@EstadoAutorizacion = 'D' AND R.autorizado_estado = 2));";

        private const string SqlAutorizacionUpdate = @"
                    UPDATE dbo.afi_cr_renuncias
                    SET autoriza_notas = @Observaciones,
                        Autorizado_Estado = @Autoriza,
                        Autorizado_Fecha = dbo.MyGetdate(),
                        Autorizado_Usuario = @Usuario
                    WHERE cod_renuncia = @CodRenuncia;";

        public FrmAFCrAutorizacionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }


        /// <summary>
        /// Obtiene la lista de autorizaciones según filtros de fecha y estado.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de fecha y estado de autorización.</param>
        /// <returns>Listado de autorizaciones de renuncias.</returns>
        public ErrorDto<List<AfCrAutorizacion>> AF_CRAutorizaciones_Obtener(int CodEmpresa, AfCrAutorizacionFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de autorización son requeridos.",
                    -2,
                    new List<AfCrAutorizacion>());
            }

            return DbHelper.ExecuteListQuery<AfCrAutorizacion>(
                CreatePortalDb(),
                CodEmpresa,
                SqlAutorizacionesConsulta,
                CrearParametrosConsulta(filtros));
        }


        /// <summary>
        /// Autoriza o deniega una renuncia, actualizando el estado y las notas de autorización.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRenuncia">Código de renuncia.</param>
        /// <param name="Observaciones">Observaciones de autorización.</param>
        /// <param name="pAutoriza">Estado de autorización.</param>
        /// <param name="Usuario">Usuario que realiza la autorización.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto AF_CRAutorizaciones_Autorizar(int CodEmpresa, int CodRenuncia, string Observaciones, int pAutoriza, string Usuario)
        {
            if (CodRenuncia <= 0)
            {
                return DbHelper.ErrorResponse("El código de renuncia es requerido.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlAutorizacionUpdate,
                new
                {
                    Observaciones = NormalizarTexto(Observaciones).ToUpperInvariant(),
                    Autoriza = pAutoriza,
                    Usuario = NormalizarTexto(Usuario),
                    CodRenuncia
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar autorización.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Crea parámetros seguros para consultar autorizaciones.
        /// </summary>
        /// <param name="filtros">Filtros de autorización.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosConsulta(AfCrAutorizacionFiltros filtros)
        {
            return new
            {
                Inicio = filtros.Inicio?.Date ?? DateTime.MinValue,
                Corte = filtros.Corte?.Date.AddHours(23).AddMinutes(59).AddSeconds(59) ?? DateTime.MaxValue,
                EstadoAutorizacion = NormalizarEstadoAutorizacion(filtros.EstadoAutorizacion)
            };
        }


        /// <summary>
        /// Normaliza el estado de autorización permitido.
        /// </summary>
        /// <param name="estado">Estado recibido.</param>
        /// <returns>Estado normalizado o cadena vacía.</returns>
        private static string NormalizarEstadoAutorizacion(string? estado)
        {
            var valor = NormalizarTexto(estado).ToUpperInvariant();
            return valor is "A" or "P" or "D" ? valor : string.Empty;
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}