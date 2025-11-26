using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.Controllers
{
    [Route("api/frmCC_ConsultaExcedente")]
    [ApiController]
    public class FrmCcConsultaExcedenteController : ControllerBase
    {
        readonly FrmCcConsultaExcedenteBl BL_CC_ConsultaExcedente;
        public FrmCcConsultaExcedenteController(IConfiguration config)
        {
            BL_CC_ConsultaExcedente = new FrmCcConsultaExcedenteBl(config);
        }

        [HttpGet("CC_Periodos_Obtener")]
        public List<CCPeriodoList> CC_Periodos_Obtener(int CodEmpresa)
        {
            return BL_CC_ConsultaExcedente.CC_Periodos_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Exc_Periodos_Obtener")]
        public CCExcPeriodoData CC_Exc_Periodos_Obtener(int CodEmpresa, int Id_Periodo)
        {
            return BL_CC_ConsultaExcedente.CC_Exc_Periodos_Obtener(CodEmpresa, Id_Periodo);
        }

        [HttpGet("CC_ValidaCedula_Obtener")]
        public ErrorDto CC_ValidaCedula_Obtener(int CodEmpresa, string Cedula, string Usuario)
        {
            return BL_CC_ConsultaExcedente.CC_ValidaCedula_Obtener(CodEmpresa, Cedula, Usuario);
        }

        [HttpGet("CC_ConsultaExcedente_Obtener")]
        public CCConsultaExcedenteData CC_ConsultaExcedente_Obtener(int CodEmpresa, int Id_Periodo, string Cedula)
        {
            return BL_CC_ConsultaExcedente.CC_ConsultaExcedente_Obtener(CodEmpresa, Id_Periodo, Cedula);
        }

        [HttpGet("CC_NotasMora_Obtener")]
        public List<VSifAuxCreditosMovDetalle> CC_NotasMora_Obtener(int CodEmpresa, int NC_Mora, string Cedula)
        {
            return BL_CC_ConsultaExcedente.CC_NotasMora_Obtener(CodEmpresa, NC_Mora, Cedula);
        }

        [HttpGet("CC_NotasOPCF_Obtener")]
        public List<VSifAuxCreditosMovDetalle> CC_NotasOPCF_Obtener(int CodEmpresa, int NC_OPCF, string Cedula)
        {
            return BL_CC_ConsultaExcedente.CC_NotasOPCF_Obtener(CodEmpresa, NC_OPCF, Cedula);
        }

        [HttpGet("CC_NotasSaldos_Obtener")]
        public List<VSifAuxCreditosMovDetalle> CC_NotasSaldos_Obtener(int CodEmpresa, int NC_Saldos, string Cedula)
        {
            return BL_CC_ConsultaExcedente.CC_NotasSaldos_Obtener(CodEmpresa, NC_Saldos, Cedula);
        }
    }
}