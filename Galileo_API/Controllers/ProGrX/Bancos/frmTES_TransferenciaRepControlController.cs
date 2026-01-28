using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PgxAPI.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesTransferenciaRepControlController : ControllerBase
    {
        private readonly FrmTesTransferenciaRepControlBL _transferenciaRepControlBL;

        public FrmTesTransferenciaRepControlController(IConfiguration config)
        {
            _transferenciaRepControlBL = new FrmTesTransferenciaRepControlBL(config);
        }

        [HttpGet("TES_TransferenciaRepControl_Catalogos_Obtener")]
        public ErrorDto<TransferenciaRepControlCatalogoDto> TES_TransferenciaRepControl_Catalogos_Obtener(int CodEmpresa, int Banco)
        {
            return _transferenciaRepControlBL.TES_TransferenciaRepControl_Catalogos_Obtener(CodEmpresa, Banco);
        }

        [HttpGet("TES_TransferenciaRepControl_NTran_Obtener")]
        public ErrorDto<long> TES_TransferenciaRepControl_NTran_Obtener(int CodEmpresa, int Banco, string TipoDoc, string Plan)
        {
            return _transferenciaRepControlBL.TES_TransferenciaRepControl_NTran_Obtener(CodEmpresa, Banco, TipoDoc, Plan);
        }

        [HttpGet("TES_TransferenciaRepControl_Carta_Obtener")]
        public ErrorDto<TesReporteTransferenciaDto> TES_TransferenciaRepControl_Carta_Obtener(int CodEmpresa, int Banco, long NTransac, string TipoDoc, string Plan)
        {
            return _transferenciaRepControlBL.TES_TransferenciaRepControl_Carta_Obtener(CodEmpresa, Banco, NTransac, TipoDoc, Plan);
        }

        [HttpGet("TES_TransferenciaRepControl_Archivo_Generar")]
        public ErrorDto<object> TES_TransferenciaRepControl_Archivo_Generar(int CodEmpresa, int Banco, int NTransac, string TipoDoc, string Formato, string Plan)
        {
            return _transferenciaRepControlBL.TES_TransferenciaRepControl_Archivo_Generar(CodEmpresa, Banco, NTransac, TipoDoc, Formato, Plan);
        }
    }
}
