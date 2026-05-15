using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.DataBaseTier.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndGestionesBl
    {
        private readonly FrmFndGestionesDb _db;

        public FrmFndGestionesBl(IConfiguration config)
        {
            _db = new FrmFndGestionesDb(config);
        }

        public ErrorDto<List<FndGestionesBuscarContratosResult>> Gestiones_BuscarContratos(FndGestionesBuscarContratosParams param)
        {
            return _db.Gestiones_BuscarContratos(param);
        }

        public ErrorDto<FndGestionesContratoResult> Gestiones_Contrato_Obtener(FndGestionesContratoParams param)
        {
            return _db.Gestiones_Contrato_Obtener(param);
        }

        public ErrorDto<List<FndGestionesContratosRenovacionResult>> Gestiones_ContratosRenovacion(FndGestionesContratosRenovacionParams param)
        {
            return _db.Gestiones_ContratosRenovacion(param);
        }

        public ErrorDto<FndGestionesContratoActualizarResult> Gestiones_Contrato_Actualizar(FndGestionesContratoActualizarParams param)
        {
            return _db.Gestiones_Contrato_Actualizar(param);
        }
    }
}