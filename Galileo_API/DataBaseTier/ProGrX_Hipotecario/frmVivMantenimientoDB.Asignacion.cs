using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public partial class FrmVivMantenimientoDb
    {
        /// <summary>
        /// Asigna o desasigna una zona a un contacto profesional del mantenimiento de vivienda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivMantenimiento_ZonaContacto_Asignar(int codEmpresa, VivMantenimientoZonaContactoAsignarRequest request)
        {
            if (request.idZona <= 0 || request.idContacto <= 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar una zona y un contacto validos.");
            }

            const string sqlAsignar = @"
                IF NOT EXISTS (
                    SELECT 1
                    FROM ViviendaContactosXZona
                    WHERE IdZona = @idZona
                        AND IdContacto = @idContacto
                )
                BEGIN
                    INSERT INTO ViviendaContactosXZona
                        (IdZona, IdContacto, RegistroUsuario, RegistroFecha)
                    VALUES
                        (@idZona, @idContacto, @usuario, dbo.MyGetdate());
                END";

            const string sqlDesasignar = @"
                DELETE dbo.ViviendaContactosXZona
                WHERE IdZona = @idZona
                    AND IdContacto = @idContacto";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                request.asignar ? sqlAsignar : sqlDesasignar,
                request);
        }
    }
}
