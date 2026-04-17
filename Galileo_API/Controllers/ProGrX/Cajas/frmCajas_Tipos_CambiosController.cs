using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers.ProGrX.Cajas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCajasTiposCambiosController : ControllerBase
    {
        private readonly FrmCajasTiposCambiosBL _bl;

        public FrmCajasTiposCambiosController(IConfiguration config)
        {
            _bl = new FrmCajasTiposCambiosBL(config);
        }
        [Authorize]
        [HttpGet("Cajas_TiposCambios_Obtener")]
        public ErrorDto<List<CajasTiposCambiosData>> Cajas_TiposCambios_Obtener(int CodEmpresa,int codContabilidad,string cod_divisa,string filtros)
        {
            return _bl.Cajas_TiposCambios_Obtener(CodEmpresa, codContabilidad, cod_divisa, filtros);
        }
        [Authorize]
        [HttpPost("Cajas_TiposCambios_Guardar")]
        public ErrorDto Cajas_TiposCambios_Guardar(int CodEmpresa,string usuario,CajasTiposCambiosData cambio)
        {
            return _bl.Cajas_TiposCambios_Guardar(CodEmpresa, usuario, cambio);
        }

        [Authorize]
        [HttpDelete("Cajas_TiposCambios_Eliminar")]
        public ErrorDto Cajas_TiposCambios_Eliminar( int CodEmpresa,string usuario,int codContabilidad,string cod_divisa,int id_cambio)
        {
            return _bl.Cajas_TiposCambios_Eliminar(CodEmpresa, usuario, codContabilidad, cod_divisa, id_cambio);
        }

        [Authorize]
        [HttpGet("Cajas_TiposCambios_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TiposCambios_Divisas_Obtener(int CodEmpresa,int codContabilidad)
        {
            return _bl.Cajas_TiposCambios_Divisas_Obtener(CodEmpresa, codContabilidad);
        }
    }
}