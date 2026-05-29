using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFTelemarketingConsultasController : ControllerBase
    {
        private readonly FrmAFTelemarketingConsultasBL _bl;

        public FrmAFTelemarketingConsultasController(IConfiguration config)
        {
            _bl = new FrmAFTelemarketingConsultasBL(config);
        }

        #region Colocacion

        [Authorize]
        [HttpGet("AF_Telemarketing_Categoria_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Categoria_Obtener(int CodEmpresa)
        {
            return _bl.AF_Telemarketing_Categoria_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Telemarketing_Colocacion_Obtener")]
        public ErrorDto<List<AfTelemarketingColocacionData>> AF_Telemarketing_Colocacion_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_Telemarketing_Colocacion_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("AF_Telemarketing_Catalogos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Catalogos_Obtener(int CodEmpresa, string tipo)
        {
            return _bl.AF_Telemarketing_Catalogos_Obtener(CodEmpresa, tipo);
        }

        #endregion

        #region Clientes

        [Authorize]
        [HttpGet("AF_Telemarketing_Lineas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Lineas_Obtener(int CodEmpresa, int combo)
        {
            return _bl.AF_Telemarketing_Lineas_Obtener(CodEmpresa, combo);
        }

        [Authorize]
        [HttpPost("AF_Telemarketing_Clientes_Obtener")]
        public ErrorDto<List<AfTelemarketingClientesData>> AF_Telemarketing_Clientes_Obtener(int CodEmpresa, ClientesFiltros filtros)
        {
            return _bl.AF_Telemarketing_Clientes_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("AF_Telemarketing_ClientesDetalle_Obtener")]
        public ErrorDto<List<AfTelemarketingClientesDetalleData>> AF_Telemarketing_ClientesDetalle_Obtener(int CodEmpresa, string vCadena, string usuario)
        {
            return _bl.AF_Telemarketing_ClientesDetalle_Obtener(CodEmpresa, vCadena, usuario);
        }

        #endregion

        #region Contactos

        [Authorize]
        [HttpGet("AF_Telemarketing_EstadosPer_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_EstadosPer_Obtener(int CodEmpresa)
        {
            return _bl.AF_Telemarketing_EstadosPer_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Telemarketing_Contacto_Obtener")]
        public ErrorDto<List<AfTelemarketingContactoData>> AF_Telemarketing_Contacto_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_Telemarketing_Contacto_Obtener(CodEmpresa, filtros);
        }

        #endregion
    }
}