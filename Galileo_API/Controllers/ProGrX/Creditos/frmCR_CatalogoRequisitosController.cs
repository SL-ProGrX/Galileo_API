using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCatalogoRequisitosController : ControllerBase
    {
        private readonly FrmCrCatalogoRequisitosBl _bl;

        public FrmCrCatalogoRequisitosController(IConfiguration config)
        {
            _bl = new FrmCrCatalogoRequisitosBl(config);
        }

        [HttpGet("CrCatalogoRequisitos_Obtener")]
        public ErrorDto<List<CrRequisitosData>> CrCatalogoRequisitos_Obtener(int codEmpresa)
        {
            return _bl.CrCatalogoRequisitos_Obtener(codEmpresa);
        }

        [HttpGet("CrCatalogosTipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogosTipos_Obtener(int codEmpresa, string nivel)
        {
            return _bl.CrCatalogosTipos_Obtener(codEmpresa, nivel);
        }

        [HttpGet("CrRequisitos_Asignados_Obtener")]
        public ErrorDto<List<CrRequisitosData>> CrRequisitos_Asignados_Obtener(int codEmpresa, string nivel, string codigo)
        {
            return _bl.CrRequisitos_Asignados_Obtener(codEmpresa, nivel, codigo);
        }

        [HttpPost("CrCatalogoRequisitos_Guardar")]
        public ErrorDto CrCatalogoRequisitos_Guardar(int codEmpresa, string usuario, CrRequisitosData request)
        {
            return _bl.CrCatalogoRequisitos_Guardar(codEmpresa, usuario, request);
        }
        
        [HttpDelete("CrCatalogoRequisitos_Eliminar")]
        public ErrorDto CrCatalogoRequisitos_Eliminar(int codEmpresa, string codigo, string usuario)
        {
            return _bl.CrCatalogoRequisitos_Eliminar(codEmpresa, codigo, usuario);
        }

        [HttpPost("CrCatalogoRequisitos_Asignar")]
        public ErrorDto CrCatalogoRequisitos_Asignar(int codEmpresa, CrRequisitoAsignacionRequest request)
        {
            return _bl.CrCatalogoRequisitos_Asignar(codEmpresa, request);
        }
    }
}
