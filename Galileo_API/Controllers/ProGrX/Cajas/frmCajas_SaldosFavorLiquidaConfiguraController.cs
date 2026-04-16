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
    public class FrmCajasSaldosFavorLiquidaConfiguraController : ControllerBase
    {
        private readonly FrmCajasSaldosFavorLiquidaConfiguraBL _bl;

        public FrmCajasSaldosFavorLiquidaConfiguraController(IConfiguration config)
        {
            _bl = new FrmCajasSaldosFavorLiquidaConfiguraBL(config);
        }

        [Authorize]
        [HttpGet("CajasSaldosFavorTipos_Obtener")]
        public ErrorDto<CajasSaldosFavorTiposLista> CajasSaldosFavorTipos_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CajasSaldosFavorTipos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("CajasSaldosFavorUsuariosLiquida_Obtener")]
        public ErrorDto<CajasSaldosFavorUsuarioLiquidaLista> CajasSaldosFavorUsuariosLiquida_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.CajasSaldosFavorUsuariosLiquida_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpPost("CajasSaldosFavorTipos_Guardar")]
        public ErrorDto CajasSaldosFavorTipos_Guardar(int CodEmpresa, string usuario, CajasSaldosFavorTiposData data)
        {
            return _bl.CajasSaldosFavorTipos_Guardar(CodEmpresa, usuario, data);
        }
        
        [Authorize]
        [HttpDelete("CajasSaldosFavorTipos_Eliminar")]
        public ErrorDto CajasSaldosFavorTipos_Eliminar(int CodEmpresa, string usuario, string doc_tipo)
        {
            return _bl.CajasSaldosFavorTipos_Eliminar(CodEmpresa, usuario, doc_tipo);
        }

        [Authorize]
        [HttpPost("CajasSaldosFavorTipoLiq_Asigna")]
        public ErrorDto CajasSaldosFavorTipoLiq_Asigna(int CodEmpresa, string usuarioG, CajasSaldosFavorUsuarioLiquidData data)
        {
            return _bl.CajasSaldosFavorTipoLiq_Asigna(CodEmpresa, usuarioG, data);
        }

        [Authorize]
        [HttpGet("CajasSaldosFavor_Usuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CajasSaldosFavor_Usuarios_Obtener(int CodEmpresa)
        {
            return _bl.CajasSaldosFavor_Usuarios_Obtener(CodEmpresa);
        }
    }
}