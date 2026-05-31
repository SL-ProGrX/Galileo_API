using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFComisionesParametrosController : ControllerBase
    {
        private readonly FrmAFComisionesParametrosBL ComisionesParametrosBL;

        public FrmAFComisionesParametrosController(IConfiguration config)
        {
            ComisionesParametrosBL = new FrmAFComisionesParametrosBL(config);
        }

        [Authorize]
        [HttpGet("AF_ComisionesParametros_Obtener")]
        public ErrorDto<TablasListaGenericaModel> AF_ComisionesParametros_Obtener(int CodEmpresa, string filtro)
        {
            return ComisionesParametrosBL.AF_ComisionesParametros_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("AF_ComisionesParametros_Guardar")]
        public ErrorDto AF_ComisionesParametros_Guardar(int CodEmpresa, int Contabilidad, string Usuario, string Parametros)
        {
            return ComisionesParametrosBL.AF_ComisionesParametros_Guardar(CodEmpresa, Contabilidad, Usuario, Parametros);
        }

        [Authorize]
        [HttpGet("AF_ComisionesParametros_Busqueda")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ComisionesParametros_Busqueda(int CodEmpresa, int Contabilidad, string Parametro)
        {
            return ComisionesParametrosBL.AF_ComisionesParametros_Busqueda(CodEmpresa, Contabilidad, Parametro);
        }
    }
}