using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAHExcedentesDistribucionController : ControllerBase
    {
        private readonly FrmAHExcedentesDistribucionBL _bl;

        public FrmAHExcedentesDistribucionController(IConfiguration config)
        {
            _bl = new FrmAHExcedentesDistribucionBL(config);
        }

        [Authorize]
        [HttpGet("AH_Excedentes_Distribucion_Periodos_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            AH_Excedentes_Distribucion_Periodos_Lista_Obtener(int CodEmpresa)
        {
            return _bl.AH_Excedentes_Distribucion_Periodos_Lista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AH_Excedentes_Distribucion_Cortes_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>AH_Excedentes_Distribucion_Cortes_Lista_Obtener(int CodEmpresa,string periodo)
        {
            return _bl.AH_Excedentes_Distribucion_Cortes_Lista_Obtener(CodEmpresa,periodo);
        }

        [Authorize]
        [HttpGet("AH_Excedentes_Distribucion_Tipos_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>AH_Excedentes_Distribucion_Tipos_Lista_Obtener(int CodEmpresa,string usuario)
        {
            return _bl.AH_Excedentes_Distribucion_Tipos_Lista_Obtener(CodEmpresa,usuario);
        }

        [Authorize]
        [HttpGet("AH_Excedentes_Distribucion_Montos_Lista_Obtener")]
        public ErrorDto<List<AHExcMontoListadoDto>>AH_Excedentes_Distribucion_Montos_Lista_Obtener(int CodEmpresa,string periodo)
        {
            return _bl.AH_Excedentes_Distribucion_Montos_Lista_Obtener(CodEmpresa,periodo);
        }

        [Authorize]
        [HttpGet("AH_Excedentes_Distribucion_Monto_Calculo_Obtener")]
        public ErrorDto<decimal> AH_Excedentes_Distribucion_Monto_Calculo_Obtener(int CodEmpresa,string periodo, string corte,string tipo,string baseCalculo,decimal porcentaje)
        {
            return _bl.AH_Excedentes_Distribucion_Monto_Calculo_Obtener( CodEmpresa,periodo,corte,tipo,baseCalculo,porcentaje);
        }

        [Authorize]
        [HttpGet("AH_Excedentes_Distribucion_Aplicada_Obtener")]
        public ErrorDto<bool> AH_Excedentes_Distribucion_Aplicada_Obtener(int CodEmpresa,string periodo,string corte)
        {
            return _bl.AH_Excedentes_Distribucion_Aplicada_Obtener(CodEmpresa,periodo,corte);
        }

        [Authorize]
        [HttpPost("AH_Excedentes_Distribucion_Monto_Guardar")]
        public ErrorDto AH_Excedentes_Distribucion_Monto_Guardar(int CodEmpresa,string usuario,[FromBody] AHExcMontoDto dto)
        {
            return _bl.AH_Excedentes_Distribucion_Monto_Guardar(CodEmpresa,usuario,dto);
        }

        [Authorize]
        [HttpPost("AH_Excedentes_Distribucion_Monto_Eliminar")]
        public ErrorDto AH_Excedentes_Distribucion_Monto_Eliminar(int CodEmpresa, string usuario, [FromBody] AHExcMontoDto dto)
        {
            return _bl.AH_Excedentes_Distribucion_Monto_Eliminar(CodEmpresa, usuario, dto);
        }
    }
}