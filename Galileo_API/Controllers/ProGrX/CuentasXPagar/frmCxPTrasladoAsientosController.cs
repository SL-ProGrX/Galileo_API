using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.CxP;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxPTrasladoAsientosController : ControllerBase
    {
        private readonly FrmCxPTrasladoAsientosBL _bl;

        public FrmCxPTrasladoAsientosController(IConfiguration config)
        {
            _bl = new FrmCxPTrasladoAsientosBL(config);
        }

        [HttpGet("DocPendientes_Obtener")]
        public ErrorDto<DocsPendientesTraslado> DocPendientes_Obtener(int CodEmpresa, string Inicio, string Corte)
        {
            return _bl.DocPendientes_Obtener(CodEmpresa, Inicio, Corte);
        }

        [HttpGet("Desbalanceados_Obtener")]
        public ErrorDto<List<Desbalanceado>> Desbalanceados_Obtener(int CodEmpresa, string Inicio, string Corte)
        {
            return _bl.Desbalanceados_Obtener(CodEmpresa, Inicio, Corte);
        }

        [HttpPost("Reactivar")]
        public ErrorDto Reactivar(int CodEmpresa, string Inicio, string Corte)
        {
            return _bl.Reactivar(CodEmpresa, Inicio, Corte);
        }

        [HttpGet("fxValidaPeriodoAsiento")]
        public bool fxValidaPeriodoAsiento(int CodEmpresa, string Fecha)
        {
            return _bl.fxValidaPeriodoAsiento(CodEmpresa, Fecha);
        }

        [HttpPost("CasosCero_Borrar")]
        public ErrorDto CasosCero_Borrar(int CodEmpresa)
        {
            return _bl.CasosCero_Borrar(CodEmpresa);
        }

        [HttpPost("AsientoIndividual_Procesar")]
        public ErrorDto AsientoIndividual_Procesar(int CodEmpresa,int cod_contabilidad, AsientoInfo data)
        {
            return _bl.AsientoIndividual_Procesar(CodEmpresa, cod_contabilidad, data);
        }
    }
}