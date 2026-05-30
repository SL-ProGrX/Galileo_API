using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.BusinessLogic.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCongelarController : ControllerBase
    {
        private readonly FrmAFCongelarBL _bl;
        public FrmAFCongelarController(IConfiguration config)
        {
            _bl = new FrmAFCongelarBL(config);
        }

        #region Consulta
        
        [Authorize]
        [HttpGet("AF_Congela_Socios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Congela_Socios_Obtener(int CodEmpresa)
        {
            return _bl.AF_Congela_Socios_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_BloqueosCongelamientos_Obtener")]
        public ErrorDto<TablasListaGenericaModel> AF_BloqueosCongelamientos_Obtener(int CodEmpresa, string filtrosCongelar, string filtros)
        {
            return _bl.AF_BloqueosCongelamientos_Obtener(CodEmpresa, filtrosCongelar, filtros);
        }

        [Authorize]
        [HttpGet("AF_BloqueosCongelamientos_Exportar")]
        public ErrorDto<List<AFCongelarDto>> AF_BloqueosCongelamientos_Exportar(int CodEmpresa, string filtrosCongelar)
        {
            return _bl.AF_BloqueosCongelamientos_Exportar(CodEmpresa, filtrosCongelar);
        }

        #endregion

        #region Registro

        [Authorize]
        [HttpGet("AF_CongelarCausaLista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CongelarCausaLista_Obtener(int CodEmpresa)
        {
            return _bl.AF_CongelarCausaLista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_BloqueosCongelamientos_Guardar")]
        public ErrorDto AF_BloqueosCongelamientos_Guardar(int CodEmpresa, string usuario, AFCongelarDto congelar)
        {
            return _bl.AF_BloqueosCongelamientos_Guardar(CodEmpresa, usuario, congelar);
        }

        #endregion

        #region Mantenimiento

        [Authorize]
        [HttpGet("AF_CongelarCausaMant_Obtener")]
        public ErrorDto<List<AFCongelaCausaDto>> AF_CongelarCausaMant_Obtener(int CodEmpresa)
        {
            return _bl.AF_CongelarCausaMant_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpDelete("AF_CongelarCausaMant_Eliminar")]
        public ErrorDto AF_CongelarCausaMant_Eliminar(int CodEmpresa, string cod_causa)
        {
            return _bl.AF_CongelarCausaMant_Eliminar(CodEmpresa, cod_causa);
        }

        [Authorize]
        [HttpPost("AF_CongelarCausaMant_Guardar")]
        public ErrorDto AF_CongelarCausaMant_Guardar(int CodEmpresa, string usuario, AFCongelaCausaDto causa)
        {
            return _bl.AF_CongelarCausaMant_Guardar(CodEmpresa, usuario, causa);
        }

        #endregion

    }
}