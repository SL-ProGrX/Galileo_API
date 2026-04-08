using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdPreCalculo;


namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdPreCalculoBL
    {
        private readonly FrmAfCdPreCalculoDB _db;

        public FrmAfCdPreCalculoBL(IConfiguration config)
        {
            _db = new FrmAfCdPreCalculoDB(config);
        }

        public ErrorDto<CrdPreCalculoPantallaInicialResponse> CrdPreCalculo_PantallaInicial_Obtener(int codEmpresa) 
                 => _db.CrdPreCalculo_PantallaInicial_Obtener(codEmpresa);

        public ErrorDto<CrdPreCalculoComiteResponse> CrdPreCalculo_Comite_Obtener(int codEmpresa, CrdPreCalculoComiteRequest request)
                  => _db.CrdPreCalculo_Comite_Obtener(codEmpresa, request);

        public ErrorDto<CrdPreCalculoGridResponse> CrdPreCalculo_Grid_Obtener(int codEmpresa, CrdPreCalculoGridRequest request)
               => _db.CrdPreCalculo_Grid_Obtener(codEmpresa, request);

        public ErrorDto<List<DropDownListaGenericaModel>> CrdPreCalculo_ComiteDesc_Obtener(int codEmpresa)
                 => _db.CrdPreCalculo_ComiteDesc_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrdPreCalculo_ComiteId_Obtener(int codEmpresa)
                 => _db.CrdPreCalculo_ComiteId_Obtener(codEmpresa);


    }
}
