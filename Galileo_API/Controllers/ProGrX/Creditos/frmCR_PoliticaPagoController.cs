using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPoliticaPagoController : ControllerBase
    {
        private readonly FrmCrPoliticaPagoBl _bl;

        public FrmCrPoliticaPagoController(IConfiguration config)
        {
            _bl = new FrmCrPoliticaPagoBl(config);
        }

        [HttpGet("CR_PoliticaPago_Obtener")]
        public ErrorDto<List<CrPoliticaPagoData>> CR_PoliticaPago_Obtener(int codEmpresa)
        {
            return _bl.CR_PoliticaPago_Obtener(codEmpresa);
        }

        [HttpPost("CR_PoliticaPago_Guardar")]
        public ErrorDto CR_PoliticaPago_Guardar(
            int codEmpresa,
            string usuario,
            CrPoliticaPagoData request)
        {
            return _bl.CR_PoliticaPago_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("CR_PoliticaPago_Eliminar")]
        public ErrorDto CR_PoliticaPago_Eliminar(
            int codEmpresa,
            string usuario,
            int idPolitica)
        {
            return _bl.CR_PoliticaPago_Eliminar(codEmpresa, usuario, idPolitica);
        }

        [HttpGet("CR_PoliticaPago_Traslados_Obtener")]
        public ErrorDto<List<CrPoliticaPagoTrasladoData>> CR_PoliticaPago_Traslados_Obtener(
            int codEmpresa,
            string tipo)
        {
            return _bl.CR_PoliticaPago_Traslados_Obtener(codEmpresa, tipo);
        }

        [HttpPost("CR_PoliticaPago_Traslados_Guardar")]
        public ErrorDto CR_PoliticaPago_Traslados_Guardar(
            int codEmpresa,
            string usuario,
            CrPoliticaPagoTrasladoGuardarRequest request)
        {
            return _bl.CR_PoliticaPago_Traslados_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("CR_PoliticaPago_Traslados_Eliminar")]
        public ErrorDto CR_PoliticaPago_Traslados_Eliminar(
            int codEmpresa,
            string usuario,
            int idSeq)
        {
            return _bl.CR_PoliticaPago_Traslados_Eliminar(codEmpresa, usuario, idSeq);
        }

        [HttpPost("CR_PoliticaPago_TablasPago_Actualizar")]
        public ErrorDto CR_PoliticaPago_TablasPago_Actualizar(int codEmpresa, string usuario)
        {
            return _bl.CR_PoliticaPago_TablasPago_Actualizar(codEmpresa, usuario);
        }
    }
}
