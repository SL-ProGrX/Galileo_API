using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Reflection;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoControlComGeneracionDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;

        public FrmCoControlComGeneracionDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Método para actualizar la comisión de generación. Ejecuta el procedimiento almacenado "spCbrComision_Actualiza" en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto Co_ControlComGeneracion_Actualizar(int CodEmpresa, string usuario)
        {
            try
            {

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                connection.Execute(
                    sql: "exec spCbrComision_Actualiza",
                    commandTimeout: 0
                );

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Comisiones: Actualización de Recuperación",
                    Movimiento = "Aplica - WEB",
                    Modulo = 4
                });

                return DbHelper.OkResponse("Proceso de Actualización de comisiones realizado satisfactoriamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al actualizar comision: {ex.Message}");
            }

        }
    }
}
