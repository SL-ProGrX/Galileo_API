using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasEfectivosDenominacionesController : ControllerBase
    {
        private readonly FrmCajasEfectivoDenominacionesBL _bl;
        public FrmCajasEfectivosDenominacionesController(IConfiguration config)
        {
            _bl = new FrmCajasEfectivoDenominacionesBL(config);
        }

        [Authorize]
        [HttpGet("Cajas_EfectivosDenominaciones_Obtener")]
        public ErrorDto<List<CajasEfectivosDenominacionesData>> Cajas_EfectivosDenominaciones_Obtener(int CodEmpresa,string cod_divisa,string filtros)
        {
            return _bl.Cajas_EfectivosDenominaciones_Obtener(CodEmpresa, cod_divisa, filtros);
        }

        [Authorize]
        [HttpPost("Cajas_EfectivosDenominaciones_Guardar")]
        public ErrorDto Cajas_EfectivosDenominaciones_Guardar(int CodEmpresa, string usuario, CajasEfectivosDenominacionesData denominacion)
        {
            return _bl.Cajas_EfectivosDenominaciones_Guardar(CodEmpresa, usuario, denominacion);
        }

        [Authorize]
        [HttpDelete("Cajas_EfectivosDenominaciones_Eliminar")]
        public ErrorDto Cajas_EfectivosDenominaciones_Eliminar(int CodEmpresa,string usuario,string cod_divisa,decimal denominacion)
        {
            return _bl.Cajas_EfectivosDenominaciones_Eliminar(CodEmpresa, usuario, cod_divisa, denominacion);
        }

        [Authorize]
        [HttpGet("Cajas_EfectivosDenominaciones_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_EfectivosDenominaciones_Divisas_Obtener(int CodEmpresa,int codContabilidad)
        {
            return _bl.Cajas_EfectivosDenominaciones_Divisas_Obtener(CodEmpresa, codContabilidad);
        }
    }
}