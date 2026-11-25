using System.Data;
using Dapper;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public partial class FrmCprSolicitudDB
    {
        // ===========================
        //  MENSAJES DE PERMISOS
        // ===========================

        private sealed class UenContableRow
        {
            public string? Descripcion { get; set; }
            public string? Unidad { get; set; }
            public string? CentroCosto { get; set; }
        }

        private sealed class EstadoSolicitudRow
        {
            public string? Estado { get; set; }
            public string? AutorizaUsuario { get; set; }
        }

        // Pendiente = sin resolver. Una solicitud autorizada puede pasar a 'V', por eso se revisa también quién autorizó.
        private static ErrorDto ValidarSolicitudPendiente(IDbConnection conn, int cprId)
        {
            var solicitud = conn.QueryFirstOrDefault<EstadoSolicitudRow>(
                @"SELECT ESTADO AS Estado, AUTORIZA_USUARIO AS AutorizaUsuario
                  FROM CPR_SOLICITUD
                  WHERE CPR_ID = @Id;",
                new { Id = cprId });

            if (solicitud == null)
                return DbHelper.ErrorResponse("La solicitud no existe.", -1);

            var resuelta = solicitud.Estado is "A" or "F" or "D"
                || !string.IsNullOrWhiteSpace(solicitud.AutorizaUsuario);

            return resuelta
                ? DbHelper.ErrorResponse("Solo se pueden autorizar o denegar solicitudes pendientes; esta ya fue resuelta.", -1)
                : DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Valida que la solicitud esté pendiente y que el usuario pueda resolverla.
        /// </summary>
        public ErrorDto ValidarResolucion(int codEmpresa, int cprId, string usuario)
        {
            var pendiente = DbHelper.WithConn<ErrorDto>(_portalDb, codEmpresa, conn => ValidarSolicitudPendiente(conn, cprId));

            if (pendiente.Code != 0 || pendiente.Result == null)
                return DbHelper.ErrorResponse(pendiente.Description ?? ErrorLiteral, pendiente.Code ?? -1);

            return pendiente.Result.Code != 0
                ? pendiente.Result
                : ValidarRolResolucion(codEmpresa, cprId, usuario);
        }

        /// <summary>
        /// Valida que el usuario tenga el rol Autoriza en la UEN solicitante.
        /// </summary>
        public ErrorDto ValidarRolResolucion(int codEmpresa, int cprId, string usuario)
        {
            var unidad = DbHelper.WithConn<string?>(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<string>(
                    "SELECT COD_UNIDAD_SOLICITANTE FROM CPR_SOLICITUD WHERE CPR_ID = @Id;",
                    new { Id = cprId }));

            if (unidad.Code != 0)
                return DbHelper.ErrorResponse(unidad.Description ?? ErrorLiteral, unidad.Code ?? -1);

            return ValidaUsuarioSolicitud(codEmpresa, usuario, "A", unidad.Result);
        }

        private string MensajePermisoFaltante(int codEmpresa, string? permiso, string? codUnidad)
        {
            return $"No tiene permiso para realizar esta acción: falta {NombreRol(permiso)} {DescribirUen(codEmpresa, codUnidad)}. "
                 + "Solicítelo en Núcleo > Información de Usuarios Operativos, pestaña Miembro de, sección Roles.";
        }

        private static string NombreRol(string? permiso) => permiso switch
        {
            "C" => "el rol Consulta",
            "A" => "el rol Autoriza",
            "S" => "el rol Solicita",
            "E" => "el rol Encargado",
            "L" => "el rol Líder",
            _ => "ser miembro con algún rol"
        };

        private string DescribirUen(int codEmpresa, string? codUnidad)
        {
            if (string.IsNullOrWhiteSpace(codUnidad))
                return "en alguna UEN";

            var r = DbHelper.WithConn<UenContableRow?>(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<UenContableRow>(
                    @"SELECT DESCRIPCION AS Descripcion,
                             CntX_Unidad AS Unidad,
                             CntX_Centro_Costo AS CentroCosto
                      FROM CORE_UENS
                      WHERE COD_UNIDAD = @CodUnidad;",
                    new { CodUnidad = codUnidad }));

            var uen = r.Result;
            if (uen == null)
                return $"en la UEN {codUnidad}";

            return $"en la UEN {codUnidad} - {uen.Descripcion} (Unidad: {uen.Unidad ?? "Sin asignar"}, Centro de costo: {uen.CentroCosto ?? "Sin asignar"})";
        }
    }
}
