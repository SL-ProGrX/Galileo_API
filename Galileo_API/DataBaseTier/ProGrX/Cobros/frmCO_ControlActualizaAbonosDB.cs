using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoControlActualizaAbonosDB
    {
        private readonly PortalDB _portalDB;

        public FrmCoControlActualizaAbonosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para actualizar los abonos en la base de datos.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Co_ControlActualizaAbonos_Actualizar(int CodEmpresa)
        {
            try
            {
                return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, "exec spCBRControlSGTAbonoGeneral");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al actualizar los abonos: {ex.Message}");
            }
            
        }
    }
}
