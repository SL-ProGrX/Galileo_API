using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosResponsablesCambioController : ControllerBase
    {
        
        private readonly FrmActivosResponsablesCambioBL _bl;

        public FrmActivosResponsablesCambioController(IConfiguration config)
        {
            _bl = new FrmActivosResponsablesCambioBL(config);
        }

        [HttpGet("Activos_ResponsablesCambio_Boletas_Lista_Obtener")]
        [Authorize]
        public ErrorDto<ActivosResponsablesCambioBoletaLista> Activos_ResponsablesCambio_Boletas_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_ResponsablesCambio_Boletas_Lista_Obtener(CodEmpresa, filtros);
        }
        [HttpGet("Activos_ResponsablesCambio_Placas_Export")]
        [Authorize]
        public ErrorDto<List<ActivosResponsablesCambioPlaca>> Activos_ResponsablesCambio_Placas_Export(
            int CodEmpresa, string cod_traslado, string identificacion, string usuario)
        {
            return _bl.Activos_ResponsablesCambio_Placas_Export(CodEmpresa, cod_traslado, identificacion, usuario);
        }

        [HttpGet("Activos_ResponsablesCambio_Motivos_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ResponsablesCambio_Motivos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_ResponsablesCambio_Motivos_Obtener(CodEmpresa);
        }

        [HttpGet("Activos_ResponsablesCambio_Personas_Buscar")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ResponsablesCambio_Personas_Buscar(int CodEmpresa, string filtros)
        {
            return _bl.Activos_ResponsablesCambio_Personas_Buscar(CodEmpresa, filtros);
        }

        [HttpGet("Activos_ResponsablesCambio_Boleta_Obtener")]
        [Authorize]
        public ErrorDto<ActivosResponsablesCambioBoleta> Activos_ResponsablesCambio_Boleta_Obtener(int CodEmpresa, string cod_traslado, string usuario)
        {
            return _bl.Activos_ResponsablesCambio_Boleta_Obtener(CodEmpresa, cod_traslado, usuario);
        }

        [HttpGet("Activos_ResponsablesCambio_Placas_Obtener")]
        [Authorize]
        public ErrorDto<List<ActivosResponsablesCambioPlaca>> Activos_ResponsablesCambio_Placas_Obtener(int CodEmpresa,string? cod_traslado,string identificacion,string usuario)
        {
            return _bl.Activos_ResponsablesCambio_Placas_Obtener(CodEmpresa, cod_traslado, identificacion, usuario);
        }

        [HttpGet("Activos_ResponsablesCambio_Boleta_Existe_Obtener")]
        [Authorize]
        public ErrorDto Activos_ResponsablesCambio_Boleta_Existe_Obtener(int CodEmpresa, string cod_traslado)
        {
            return _bl.Activos_ResponsablesCambio_Boleta_Existe_Obtener(CodEmpresa, cod_traslado);
        }

        [HttpPost("Activos_ResponsablesCambio_Boleta_Guardar")]
        [Authorize]
        public ErrorDto<ActivosResponsablesCambioBoletaResult> Activos_ResponsablesCambio_Boleta_Guardar(int CodEmpresa, ActivosResponsablesCambioBoleta boleta)
        {
            return _bl.Activos_ResponsablesCambio_Boleta_Guardar(CodEmpresa, boleta);
        }

        [HttpPost("Activos_ResponsablesCambio_Boleta_Placa_Guardar")]
        [Authorize]
        public ErrorDto Activos_ResponsablesCambio_Boleta_Placa_Guardar(int CodEmpresa, ActivosResponsablesCambioPlacaGuardarRequest data)
        {
            return _bl.Activos_ResponsablesCambio_Boleta_Placa_Guardar(CodEmpresa, data);
        }

        [HttpPost("Activos_ResponsablesCambio_Boleta_Procesar")]
        [Authorize]
        public ErrorDto Activos_ResponsablesCambio_Boleta_Procesar(int CodEmpresa, string cod_traslado, string usuario)
        {
            return _bl.Activos_ResponsablesCambio_Boleta_Procesar(CodEmpresa, cod_traslado, usuario);
        }

        [HttpPost("Activos_ResponsablesCambio_Boleta_Descartar")]
        [Authorize]
        public ErrorDto Activos_ResponsablesCambio_Boleta_Descartar(int CodEmpresa, string cod_traslado, string usuario)
        {
            return _bl.Activos_ResponsablesCambio_Boleta_Descartar(CodEmpresa, cod_traslado, usuario);
        }

        [HttpGet("Activos_ResponsablesCambio_Boleta_Scroll")]
        [Authorize]
        public ErrorDto<ActivosResponsablesCambioBoleta> Activos_ResponsablesCambio_Boleta_Scroll(int CodEmpresa, int scroll, string? cod_traslado, string usuario)
        {
            return _bl.Activos_ResponsablesCambio_Boleta_Scroll(CodEmpresa, scroll, cod_traslado, usuario);
        }

        [HttpGet("Activos_ResponsablesCambio_Persona_Obtener")]
        [Authorize]
        public ErrorDto<ActivosResponsablesPersona> Activos_ResponsablesCambio_Persona_Obtener(int CodEmpresa, string identificacion)
        {
            return _bl.Activos_ResponsablesCambio_Persona_Obtener(CodEmpresa, identificacion);
        }
    }
}
