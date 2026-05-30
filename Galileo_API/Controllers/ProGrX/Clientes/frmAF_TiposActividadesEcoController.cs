using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFTiposActividadesEcoController : ControllerBase
    {
        private readonly FrmAFTiposActividadesEcoBL _bl;

        public FrmAFTiposActividadesEcoController(IConfiguration config)
        {
            _bl = new FrmAFTiposActividadesEcoBL(config);
        }

        [Authorize]
        [HttpGet("AF_TiposActividadesEco_Obtener")]
        public ErrorDto<AfTiposActividadesEcoLista> AF_TiposActividadesEco_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_TiposActividadesEco_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_TiposActividadesEco_Guardar")]
        public ErrorDto AF_TiposActividadesEco_Guardar(int CodEmpresa, string Usuario, AfTiposActividadesEcoDto Info)
        {
            return _bl.AF_TiposActividadesEco_Guardar(CodEmpresa, Usuario, Info);
        }

        [Authorize]
        [HttpDelete("AF_TiposActividadesEco_Eliminar")]
        public ErrorDto AF_TiposActividadesEco_Eliminar(int CodEmpresa, string Usuario, string CodActividad)
        {
            return _bl.AF_TiposActividadesEco_Eliminar(CodEmpresa, Usuario, CodActividad);
        }

        [Authorize]
        [HttpGet("AF_TiposActividadesEco_SubActividad_Obtener")]
        public ErrorDto<AfTiposActividadesEcoLista> AF_TiposActividadesEco_SubActividad_Obtener(int CodEmpresa, string CodActividad, string filtros)
        {
            return _bl.AF_TiposActividadesEco_SubActividad_Obtener(CodEmpresa, CodActividad, filtros);
        }

        [Authorize]
        [HttpPost("AF_TiposActividadesEco_SubActividad_Guardar")]
        public ErrorDto AF_TiposActividadesEco_SubActividad_Guardar(int CodEmpresa, string Usuario, AfTiposActividadesEcoDto Info)
        {
            return _bl.AF_TiposActividadesEco_SubActividad_Guardar(CodEmpresa, Usuario, Info);
        }

        [Authorize]
        [HttpDelete("AF_TiposActividadesEco_SubActividad_Eliminar")]
        public ErrorDto AF_TiposActividadesEco_SubActividad_Eliminar(int CodEmpresa, string Usuario, string CodActividad, string CodSubAct)
        {
            return _bl.AF_TiposActividadesEco_SubActividad_Eliminar(CodEmpresa, Usuario, CodActividad, CodSubAct);
        }
    }
}