using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesBancosSaldosController : ControllerBase
    {
        private readonly FrmTesBancosSaldosBL _bancosSaldosBL;

        public FrmTesBancosSaldosController(IConfiguration config)
        {
            _bancosSaldosBL = new FrmTesBancosSaldosBL(config);
        }

        [HttpGet("TES_BancosSaldos_Grupos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosSaldos_Grupos_Obtener(int CodEmpresa)
        {
            return _bancosSaldosBL.TES_BancosSaldos_Grupos_Obtener(CodEmpresa);
        }

        [HttpGet("TES_BancosSaldos_Cuentas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosSaldos_Cuentas_Obtener(int CodEmpresa, string CodGrupo)
        {
            return _bancosSaldosBL.TES_BancosSaldos_Cuentas_Obtener(CodEmpresa, CodGrupo);
        }

        [HttpGet("TES_BancosSaldos_Monitoreo_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_BancosSaldos_Monitoreo_Obtener(int CodEmpresa, string CodGrupo, string Filtros)
        {
            return _bancosSaldosBL.TES_BancosSaldos_Monitoreo_Obtener(CodEmpresa, CodGrupo, Filtros);
        }

        [HttpPost("TES_BancosSaldos_Monitoreo_Actualizar")]
        public ErrorDto TES_BancosSaldos_Monitoreo_Actualizar(int CodEmpresa, string Banco, bool Monitoreo)
        {
            return _bancosSaldosBL.TES_BancosSaldos_Monitoreo_Actualizar(CodEmpresa, Banco, Monitoreo);
        }

        [HttpGet("TES_BancosSaldos_Historico_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_BancosSaldos_Historico_Obtener(int CodEmpresa, int Banco, string Filtros)
        {
            return _bancosSaldosBL.TES_BancosSaldos_Historico_Obtener(CodEmpresa, Banco, Filtros);
        }

        [HttpGet("TES_BancosSaldos_Cierres_Obtener")]
        public ErrorDto<TesBancosSaldosCierresDto> TES_BancosSaldos_Cierres_Obtener(int CodEmpresa, int Banco)
        {
            return _bancosSaldosBL.TES_BancosSaldos_Cierres_Obtener(CodEmpresa, Banco);
        }

        [HttpPost("TES_BancosSaldos_Cierres_Actualizar")]
        public ErrorDto TES_BancosSaldos_Cierres_Actualizar(int CodEmpresa, string Usuario, string Datos)
        {
            return _bancosSaldosBL.TES_BancosSaldos_Cierres_Actualizar(CodEmpresa, Usuario, Datos);
        }
    }
}