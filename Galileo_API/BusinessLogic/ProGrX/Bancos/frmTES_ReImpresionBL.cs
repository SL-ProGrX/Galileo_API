using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesReImpresionBL
    {
        private readonly FrmTesReImpresionDB _ReImpresionDb;

        public FrmTesReImpresionBL(IConfiguration config)
        {
            _ReImpresionDb = new FrmTesReImpresionDB(config);
        }

        public ErrorDto<TesReImpresionModels> TES_ReImpresion_Obtener(int CodEmpresa, int solicitud)
        {
            return _ReImpresionDb.TES_ReImpresion_Obtener(CodEmpresa, solicitud);
        }

        public ErrorDto<object> TES_ReImpresion_Guardar(int CodEmpresa, TesReImpresionModels solicitud)
        {
            return _ReImpresionDb.TES_ReImpresion_Guardar(CodEmpresa, solicitud);
        }
    }
}
