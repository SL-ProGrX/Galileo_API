using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesAnulacionDocController : ControllerBase
    {
        private readonly FrmTesAnulacionDocBl _AnulacionDocBL;


        public FrmTesAnulacionDocController(IConfiguration config)
        {
            _AnulacionDocBL = new FrmTesAnulacionDocBl(config);
        }

        [HttpGet("TES_Anulacion_Obtener")]
        public ErrorDto<TesAnulacionDocData> TES_Anulacion_Obtener(int CodEmpresa, int solicitud, string usuario)
        {
            return _AnulacionDocBL.TES_Anulacion_Obtener(CodEmpresa, solicitud, usuario);
        }

        [HttpPost("TES_Anulacion_Anular")]
        public ErrorDto TES_Anulacion_Anular(int CodEmpresa, string usuario, TesAnulacionAnulaModel anula)
        {
            return _AnulacionDocBL.TES_Anulacion_Anular(CodEmpresa, usuario, anula);
        }

        [HttpPost("TES_AnulacionCopiaSolicitud")]
        public ErrorDto TES_AnulacionCopiaSolicitud(int CodEmpresa, string usuario, TesAnulacionAnulaModel anula)
        {
            return _AnulacionDocBL.TES_AnulacionCopiaSolicitud(CodEmpresa, usuario, anula);
        }

        [HttpGet("TES_AnulacionConceptos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_AnulacionConceptos_Obtener(int CodEmpresa, string tipo)
        {
            return _AnulacionDocBL.TES_AnulacionConceptos_Obtener(CodEmpresa, tipo);
        }


    }
}
