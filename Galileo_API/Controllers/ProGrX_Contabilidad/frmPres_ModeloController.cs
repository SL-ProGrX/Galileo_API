using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/frmPres_Modelo")]
    [Route("api/FrmPresModelo")]
    [ApiController]
    public class FrmPresModeloController : ControllerBase
    {
        readonly FrmPresModeloBl _bl;

        public FrmPresModeloController(IConfiguration config)
        {
            _bl = new FrmPresModeloBl(config);
        }

        [HttpGet("CntxContabilidades_Obtener")]
        [Authorize]
        public ErrorDto<List<CntxCData>> CntxContabilidades_Obtener(int CodEmpresa)
        {
            return _bl.CntxContabilidades_Obtener(CodEmpresa);
        }

        [HttpGet("CntxCierres_Obtener")]
        [Authorize]
        public ErrorDto<List<CntxCData>> CntxCierres_Obtener(int CodEmpresa, int CodContab)
        {
            return _bl.CntxCierres_Obtener(CodEmpresa, CodContab);
        }

        [HttpGet("Pres_Modelo_Obtener")]
        [Authorize]
        public ErrorDto<PresModeloData> Pres_Modelo_Obtener(int CodEmpresa, string CodModelo, int CodContab)
        {
            return _bl.Pres_Modelo_Obtener(CodEmpresa, CodModelo, CodContab);
        }

        [HttpGet("Pres_Modelo_scroll")]
        [Authorize]
        public ErrorDto<PresModeloData> Pres_Modelo_scroll(int CodEmpresa, int scrollValue, string? CodModelo, int CodContab)
        {
            return _bl.Pres_Modelo_scroll(CodEmpresa, scrollValue, CodModelo, CodContab);
        }

        [HttpPost("Pres_Modelo_Insertar")]
        [Authorize]
        public ErrorDto Pres_Modelo_Insertar(int CodEmpresa, PresModeloInsert request)
        {
            return _bl.Pres_Modelo_Insertar(CodEmpresa, request);
        }

        [HttpPost("Pres_MapeaCuentasSinCentroCosto_SP")]
        [Authorize]
        public ErrorDto Pres_MapeaCuentasSinCentroCosto_SP(int CodEmpresa, string CodModelo, int CodContab, string Usuario)
        {
            return _bl.Pres_MapeaCuentasSinCentroCosto_SP(CodEmpresa, CodModelo, CodContab, Usuario);
        }

        [HttpPost("Pres_Model_Reiniciar")]
        [Authorize]
        public ErrorDto Pres_Model_Reiniciar(int CodEmpresa, string CodModelo)
        {
            return _bl.Pres_Model_Reiniciar(CodEmpresa, CodModelo);
        }

        [HttpGet("Pres_Modelo_Usuarios_SP")]
        [Authorize]
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_Usuarios_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            return _bl.Pres_Modelo_Usuarios_SP(CodEmpresa, CodModelo, CodContab);
        }

        [HttpGet("Pres_Modelo_Ajustes_SP")]
        [Authorize]
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_Ajustes_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            return _bl.Pres_Modelo_Ajustes_SP(CodEmpresa, CodModelo, CodContab);
        }

        [HttpGet("Pres_Modelo_Ajustes_Autorizados_SP")]
        [Authorize]
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_Ajustes_Autorizados_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            return _bl.Pres_Modelo_Ajustes_Autorizados_SP(CodEmpresa, CodModelo, CodContab);
        }

        [HttpGet("Pres_Modelo_Usuarios_Autorizados_SP")]
        [Authorize]
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_Usuarios_Autorizados_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            return _bl.Pres_Modelo_Usuarios_Autorizados_SP(CodEmpresa, CodModelo, CodContab);
        }

        [HttpGet("Pres_Modelo_AjUs_Ajustes_SP")]
        [Authorize]
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_AjUs_Ajustes_SP(int CodEmpresa, string CodModelo, int CodContab, string Usuario)
        {
            return _bl.Pres_Modelo_AjUs_Ajustes_SP(CodEmpresa, CodModelo, CodContab, Usuario);
        }

        [HttpGet("Pres_Modelo_AjUs_Usuarios_SP")]
        [Authorize]
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_AjUs_Usuarios_SP(int CodEmpresa, string CodModelo, int CodContab, string CodAjuste)
        {
            return _bl.Pres_Modelo_AjUs_Usuarios_SP(CodEmpresa, CodModelo, CodContab, CodAjuste);
        }

        [HttpPost("Pres_Modelo_AjUs_Registro_SP")]
        [Authorize]
        public ErrorDto Pres_Modelo_AjUs_Registro_SP(int CodEmpresa, PressModeloAjUsRegistro request)
        {
            return _bl.Pres_Modelo_AjUs_Registro_SP(CodEmpresa, request);
        }

        [HttpPost("Pres_Modelo_Ajustes_Registro_SP")]
        [Authorize]
        public ErrorDto Pres_Modelo_Ajustes_Registro_SP(int CodEmpresa, PressModeloAjUsRegistro request)
        {
            return _bl.Pres_Modelo_Ajustes_Registro_SP(CodEmpresa, request);
        }

        [HttpPost("Pres_Modelo_Usuarios_Registro_SP")]
        [Authorize]
        public ErrorDto Pres_Modelo_Usuarios_Registro_SP(int CodEmpresa, PressModeloAjUsRegistro request)
        {
            return _bl.Pres_Modelo_Usuarios_Registro_SP(CodEmpresa, request);
        }

        [HttpGet("Pres_Modelos_Lista")]
        [Authorize]
        public ErrorDto<List<PresModeloData>> Pres_Modelos_Lista(int CodEmpresa, int CodContab)
        {
            return _bl.Pres_Modelos_Lista(CodEmpresa, CodContab);
        }

        [HttpPost("Pres_Model_Eliminar")]
        [Authorize]
        public ErrorDto Pres_Model_Eliminar(int CodEmpresa, string CodModelo)
        {
            return _bl.Pres_Model_Eliminar(CodEmpresa, CodModelo);
        }
    }
}