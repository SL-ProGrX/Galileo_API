using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Pasivos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrApaReportesController : ControllerBase
    {
        private readonly FrmCrApaReportesBL _bl;

        public FrmCrApaReportesController(IConfiguration config)
        {
            _bl = new FrmCrApaReportesBL(config);
        }

        [Authorize]
        [HttpGet("CR_APA_Reportes_Acreedores_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_APA_Reportes_Acreedores_Dropdown_Obtener(int codEmpresa, string ordenarPor)
        {
            return _bl.CR_APA_Reportes_Acreedores_Dropdown_Obtener(codEmpresa, ordenarPor);
        }

        [Authorize]
        [HttpGet("CR_APA_Reportes_Operaciones_Obtener")]
        public ErrorDto<List<CrApaReportesOperacion>> CR_APA_Reportes_Operaciones_Obtener(int codEmpresa, string codAcreedor)
        {
            return _bl.CR_APA_Reportes_Operaciones_Obtener(codEmpresa, codAcreedor);
        }

        [Authorize]
        [HttpGet("CR_APA_Reportes_Acreedor_Obtener")]
        public ErrorDto<FrmCrApaMovimientosAcreedorDto?> CR_APA_Reportes_Acreedor_Obtener(int codEmpresa, string codAcreedor)
        {
            return _bl.CR_APA_Reportes_Acreedor_Obtener(codEmpresa, codAcreedor);
        }

        [Authorize]
        [HttpGet("CR_APA_Reportes_Operacion_Obtener")]
        public ErrorDto<FrmCrApaMovimientosOperacionDto?> CR_APA_Reportes_Operacion_Obtener(int codEmpresa, string codAcreedor, string operacion)
        {
            return _bl.CR_APA_Reportes_Operacion_Obtener(codEmpresa, codAcreedor, operacion);
        }

        [Authorize]
        [HttpGet("CR_APA_Reportes_SaldosCorte_Obtener")]
        public ErrorDto<List<CrApaReportesSaldoCorte>> CR_APA_Reportes_SaldosCorte_Obtener(int codEmpresa, DateTime fechaCorte)
        {
            return _bl.CR_APA_Reportes_SaldosCorte_Obtener(codEmpresa, fechaCorte);
        }

        [Authorize]
        [HttpGet("CR_APA_Reportes_AuxiliarCorte_Existe")]
        public ErrorDto<int> CR_APA_Reportes_AuxiliarCorte_Existe(int codEmpresa, DateTime fechaCorte)
        {
            return _bl.CR_APA_Reportes_AuxiliarCorte_Existe(codEmpresa, fechaCorte);
        }

        [Authorize]
        [HttpPost("CR_APA_Reportes_AuxiliarCorte_Aplicar")]
        public ErrorDto CR_APA_Reportes_AuxiliarCorte_Aplicar(int codEmpresa, DateTime fechaCorte)
        {
            return _bl.CR_APA_Reportes_AuxiliarCorte_Aplicar(codEmpresa, fechaCorte);
        }
    }
}
