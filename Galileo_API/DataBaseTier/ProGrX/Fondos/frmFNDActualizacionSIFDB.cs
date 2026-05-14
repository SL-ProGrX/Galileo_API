using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndActualizacionSifDb
    {
        private readonly MSecurityMainDb _securityMainDB;
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 18;

        public FrmFndActualizacionSifDb(IConfiguration config)
        {
            _securityMainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Aplica la actualización SIF, 
        /// Sincroniza Contratos con Operaciones de Retención
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto Fnd_ActualizacionSif_Aplicar(int CodEmpresa, string Usuario)
        {
            var response = DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                "exec spFndSincronizaContratos");

            _securityMainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = Usuario.ToUpper(),
                DetalleMovimiento = "Sincronización de Fondos con Retenciones",
                Movimiento = "Aplica - WEB",
                Modulo = vModulo
            });

            return response;
        }
    }
}