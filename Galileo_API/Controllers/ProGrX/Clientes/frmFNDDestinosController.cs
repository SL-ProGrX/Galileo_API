using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndDestinosController : ControllerBase
    {
        private readonly FrmFndDestinosBL _bl;

        public FrmFndDestinosController(IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _bl = new FrmFndDestinosBL(config);
        }

        [Authorize]
        [HttpGet("Fnd_Destinos_Obtener")]
        public ErrorDto<List<FndDestinosData>> Fnd_Destinos_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Destinos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Fnd_DestinosLista_Obtener")]
        public ErrorDto<FndDestinosLista> Fnd_DestinosLista_Obtener(int CodEmpresa, [FromBody] FiltrosLazyLoadData filtros)
        {
            return _bl.Fnd_DestinosLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Fnd_Destinos_Valida")]
        public ErrorDto Fnd_Destinos_Valida(int CodEmpresa, string codDestino)
        {
            return _bl.Fnd_Destinos_Valida(CodEmpresa, codDestino);
        }

        [Authorize]
        [HttpPost("Fnd_Destinos_Guardar")]
        public ErrorDto Fnd_Destinos_Guardar(int CodEmpresa, string usuario, FndDestinosData destino)
        {
            return _bl.Fnd_Destinos_Guardar(CodEmpresa, usuario, destino);
        }

        [Authorize]
        [HttpDelete("Fnd_Destinos_Eliminar")]
        public ErrorDto Fnd_Destinos_Eliminar(int CodEmpresa, string usuario, string codDestino)
        {
            return _bl.Fnd_Destinos_Eliminar(CodEmpresa, usuario, codDestino);
        }

        [Authorize]
        [HttpGet("Fnd_Planes_Obtener")]
        public ErrorDto<List<FndPlanesDestinoData>> Fnd_Planes_Obtener(int CodEmpresa, string codDestino)
        {
            return _bl.Fnd_Planes_Obtener(CodEmpresa, codDestino);
        }

        [Authorize]
        [HttpPost("Fnd_PlanesLista_Obtener")]
        public ErrorDto<FndPlanesDestinoLista> Fnd_PlanesLista_Obtener(int CodEmpresa, string Cod_Destino, [FromBody] FiltrosLazyLoadData filtros)
        {
            return _bl.Fnd_PlanesLista_Obtener(CodEmpresa, Cod_Destino, filtros);
        }

        [Authorize]
        [HttpPost("Fnd_Planes_AsignarDesasignar")]
        public ErrorDto Fnd_Planes_AsignarDesasignar(int CodEmpresa, FndAsignarPlanRequest request)
        {
            return _bl.Fnd_Planes_AsignarDesasignar(CodEmpresa, request);
        }
    }
}