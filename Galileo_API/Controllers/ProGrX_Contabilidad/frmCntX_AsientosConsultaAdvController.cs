using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCntXAsientosConsultaAdvController : ControllerBase
    {
        private readonly FrmCntXAsientosConsultaAdvBl _bl;

        public FrmCntXAsientosConsultaAdvController(IConfiguration config)
        {
            _bl = new FrmCntXAsientosConsultaAdvBl(config);
        }

        [Authorize]
        [HttpPost]
        [Route("CntX_Movimientos_Consulta")]
        public ErrorDto<List<CntxMovimientoConsultaDto>> CntX_Movimientos_Consulta(
            int codEmpresa,
            int codContabilidad,
            CntxMovimientosFiltroDto filtros)
        {
            return _bl.CntX_Movimientos_Consulta(codEmpresa, codContabilidad, filtros);
        }

        [Authorize]
        [HttpGet]
        [Route("CntX_TiposAsiento_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsiento(int codEmpresa, int codContabilidad)
        {
            return _bl.CntX_TiposAsiento_Listar(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpGet]
        [Route("CntX_Unidades_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades(int codEmpresa, int codContabilidad)
        {
            return _bl.CntX_Unidades_Listar(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpGet]
        [Route("CntX_CentroCostos_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Centros(int codEmpresa, int codContabilidad)
        {
            return _bl.CntX_CentroCostos_Listar(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpGet]
        [Route("CntX_Divisas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Divisas(int codEmpresa, int codContabilidad)
        {
            return _bl.CntX_Divisas_Listar(codEmpresa, codContabilidad);
        }


        [Authorize]
        [HttpGet]
        [Route("Cntx_TiposAsientos_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsientosBuscar(int codEmpresa, int cod_contabilidad)
        {
            return _bl.Cntx_TiposAsientos_Buscar(codEmpresa, cod_contabilidad);
        }
    }
}