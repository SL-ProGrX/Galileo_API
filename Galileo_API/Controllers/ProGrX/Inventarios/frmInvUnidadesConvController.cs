using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class FrmInvUnidadesConvController
        : ControllerBase
    {
        private readonly FrmInvUnidadesConvBl _bl;

        public FrmInvUnidadesConvController(
            IConfiguration config)
        {
            _bl = new FrmInvUnidadesConvBl(
                config);
        }

        [HttpGet(
            "INV_UnidadesConv_Unidades_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_UnidadesConv_Unidades_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_UnidadesConv_Unidades_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "INV_UnidadesConv_Lista_Obtener")]
        public ErrorDto<UnidadesConvLista>
            INV_UnidadesConv_Lista_Obtener(
                int CodEmpresa,
                string CodUnidad)
        {
            return _bl
                .INV_UnidadesConv_Lista_Obtener(
                    CodEmpresa,
                    CodUnidad);
        }

        [HttpPost(
            "INV_UnidadesConv_Guardar")]
        public ErrorDto INV_UnidadesConv_Guardar(
            int CodEmpresa,
            [FromBody]
            UnidadMedicionConvData equivalencia)
        {
            return _bl.INV_UnidadesConv_Guardar(
                CodEmpresa,
                equivalencia);
        }

        [HttpDelete(
            "INV_UnidadesConv_Eliminar")]
        public ErrorDto INV_UnidadesConv_Eliminar(
            int CodEmpresa,
            string CodUnidad,
            string CodUnidadDestino)
        {
            return _bl.INV_UnidadesConv_Eliminar(
                CodEmpresa,
                CodUnidad,
                CodUnidadDestino);
        }
    }
}