using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmSifOficinasMetasController : ControllerBase
    {
        private readonly FrmSifOficinasMetasBL _bl;

        public FrmSifOficinasMetasController(IConfiguration config)
        {
            _bl = new FrmSifOficinasMetasBL(config);
        }

        [Authorize]
        [HttpGet("Sif_OficinasMetasLista_Obtener")]
        public ErrorDto<SifOficinasMetaLista> Sif_OficinasMetasLista_Obtener(int CodEmpresa, string oficina, int anio, string usuario)
        {
            return _bl.Sif_OficinasMetasLista_Obtener(CodEmpresa, oficina, anio, usuario);
        }

        [Authorize]
        [HttpGet("Sif_OficinasMetasPeriodos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasMetasPeriodos_Obtener(int CodEmpresa, string oficina)
        {
            return _bl.Sif_OficinasMetasPeriodos_Obtener(CodEmpresa, oficina);
        }

        [Authorize]
        [HttpPost("Sif_OficinasMetas_Actualizar")]
        public ErrorDto Sif_OficinasMetas_Actualizar(int CodEmpresa, string oficina, string usuario, List<SifOficinasMetaData> metas)
        {
            return _bl.Sif_OficinasMetas_Actualizar(CodEmpresa, oficina, usuario, metas);
        }

    }
}