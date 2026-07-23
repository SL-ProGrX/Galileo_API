using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Contabilidad.FrmCntxConInformeEspecialModels;


namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class FrmCntxConInformeEspecialController : ControllerBase
    {
        private readonly FrmCntxConInformeEspecialBL _bl;

        public FrmCntxConInformeEspecialController(IConfiguration config) =>
            _bl = new FrmCntxConInformeEspecialBL(config);


        [HttpPost("Cnt_ConsolidadoEspecial_Excel_Generar")]
        public ErrorDto<ArchivoGeneradoModel> Cnt_ConsolidadoEspecial_Excel_Generar(int CodEmpresa, string usuario, CntConsolidadoEspecialGenerarRequest request)
              => _bl.Cnt_ConsolidadoEspecial_Excel_Generar(CodEmpresa, request, usuario);
    }
}
