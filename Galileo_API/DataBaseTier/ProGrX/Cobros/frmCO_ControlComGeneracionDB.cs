using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoControlComGeneracionDB
    {
        private readonly PortalDB _portalDB;

        public FrmCoControlComGeneracionDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para actualizar la comisión de generación. Ejecuta el procedimiento almacenado "spCbrComision_Actualiza" en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto Co_ControlComGeneracion_Actualizar(int CodEmpresa)
        {
            try
            {
                return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, "exec spCbrComision_Actualiza");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al actualizar comision: {ex.Message}");
            }

        }
    }
}
