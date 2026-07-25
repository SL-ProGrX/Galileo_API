using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using static Galileo_API.Models.ProGrX_Contabilidad.FrmCntxConInformeEspecialModels;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntxConInformeEspecialBL
    {
        private readonly FrmCntxConInformeEspecialDB _db;

        public FrmCntxConInformeEspecialBL(IConfiguration config)
            => _db = new FrmCntxConInformeEspecialDB(config);

        public ErrorDto<ArchivoGeneradoModel> Cnt_ConsolidadoEspecial_Excel_Generar(int CodEmpresa, CntConsolidadoEspecialGenerarRequest request, string usuario)
              => _db.Cnt_ConsolidadoEspecial_Excel_Generar(CodEmpresa, request, usuario);


    }
}
