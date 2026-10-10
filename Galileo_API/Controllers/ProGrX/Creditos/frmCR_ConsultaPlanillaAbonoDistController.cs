using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class FrmCrConsultaPlanillaAbonoDistController
        : ControllerBase
    {
        private readonly FrmCrConsultaPlanillaAbonoDistBL _bl;

        public FrmCrConsultaPlanillaAbonoDistController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmCrConsultaPlanillaAbonoDistBL(config);
        }

        [HttpGet("CR_ConsultaPlanillaAbonoDist_Inicializar")]
        public ErrorDto<CrConsultaPlanillaAbonoDistInicialData>
            CR_ConsultaPlanillaAbonoDist_Inicializar(
                int codEmpresa,
                string cedula,
                string usuario)
        {
            return _bl.CR_ConsultaPlanillaAbonoDist_Inicializar(
                codEmpresa,
                cedula,
                usuario);
        }

        [HttpGet("CR_ConsultaPlanillaAbonoDist_UltimoMonto")]
        public ErrorDto<CrConsultaPlanillaAbonoDistUltimoData>
            CR_ConsultaPlanillaAbonoDist_UltimoMonto(
                int codEmpresa,
                string cedula,
                int codInstitucion,
                int proceso)
        {
            return _bl.CR_ConsultaPlanillaAbonoDist_UltimoMonto(
                codEmpresa,
                cedula,
                codInstitucion,
                proceso);
        }
            
        [HttpGet("CR_ConsultaPlanillaAbonoDist_Consultar")]
        public ErrorDto<List<CrConsultaPlanillaAbonoDistDetalleData>>
            CR_ConsultaPlanillaAbonoDist_Consultar(
                int codEmpresa,
                [FromQuery]
                CrConsultaPlanillaAbonoDistConsultaRequest? request)
        {
            return _bl.CR_ConsultaPlanillaAbonoDist_Consultar(
                codEmpresa,
                request);
        }
    }
}