using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCatalogoPolizasController : ControllerBase
    {
        private readonly FrmCrCatalogoPolizasBL _BL;

        public FrmCrCatalogoPolizasController(IConfiguration config)
        {
            _BL = new FrmCrCatalogoPolizasBL(config);
        }

        [HttpGet("Crd_CatalogoPolizas_GrupoAplicacion_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_CatalogoPolizas_GrupoAplicacion_Listar(int CodEmpresa)
        {
            return _BL.Crd_CatalogoPolizas_GrupoAplicacion_Listar(CodEmpresa);
        }

        [HttpGet("Crd_CatalogoPolizas_Aseguradoras_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_CatalogoPolizas_Aseguradoras_Listar(int CodEmpresa)
        {
            return _BL.Crd_CatalogoPolizas_Aseguradoras_Listar(CodEmpresa);
        }

        [HttpGet("Crd_CatalogoPolizas_Obtener")]
        public ErrorDto<List<CrdCatalogoPolizasListDto>> Crd_CatalogoPolizas_Obtener(int CodEmpresa)
        {
            return _BL.Crd_CatalogoPolizas_Obtener(CodEmpresa);
        }

        [HttpGet("Crd_CatalogoPoliza_Obtener")]
        public ErrorDto<CrdCatalogoPolizasConsultaDto?> Crd_CatalogoPoliza_Obtener(int CodEmpresa, string? cod_poliza)
        {
            return _BL.Crd_CatalogoPoliza_Obtener(CodEmpresa, cod_poliza);
        }

        [HttpGet("Crd_CatalogoPolizas_Navegar")]
        public ErrorDto<CrdCatalogoPolizasConsultaDto?> Crd_CatalogoPolizas_Navegar(
             int CodEmpresa,
             string cod_poliza,
             string direccion // "N" = siguiente, "A" = anterior
         )
        {
            return _BL.Crd_CatalogoPolizas_Navegar(CodEmpresa, cod_poliza, direccion);
        }


        [HttpPost("Crd_CatalogoPolizas_ActualizarMasivo")]
        public ErrorDto<bool> Crd_CatalogoPolizas_ActualizarMasivo(int CodEmpresa, string usuario)
        {
            return _BL.Crd_CatalogoPolizas_ActualizarMasivo(CodEmpresa, usuario);
        }

        [HttpGet("Crd_CatalogoPolizas_Acreedores_Obtener")]
        public ErrorDto<List<CrdCatalogoPolizasAcreedorDto>> Crd_CatalogoPolizas_Acreedores_Obtener(
          int CodEmpresa,
          string? cod_poliza)
        {
            return _BL.Crd_CatalogoPolizas_Acreedores_Obtener(CodEmpresa, cod_poliza);
        }

        [HttpPost("Crd_CatalogoPolizas_Acreedor_Asignar")]
        public ErrorDto<bool> Crd_CatalogoPolizas_Acreedor_Asignar(int CodEmpresa, string usuario, CrdCatalogoPolizasAcreedorAsignarReq req)
        {
            return _BL.Crd_CatalogoPolizas_Acreedor_Asignar(CodEmpresa, usuario, req);
        }

        [HttpGet("Crd_CatalogoPolizas_Garantias_Listar")]
        public ErrorDto<List<CrdCatalogoPolizasGarantiaDto>>
        Crd_CatalogoPolizas_Garantias_Listar(int CodEmpresa, string? cod_poliza)
        {
            return _BL.Crd_CatalogoPolizas_Garantias_Listar(CodEmpresa, cod_poliza);
        }

        [HttpPost("Crd_CatalogoPolizas_Garantia_Asignar")]
        public ErrorDto<CrdCatalogoPolizasGarantiaAsignaDto?>
       Crd_CatalogoPolizas_Garantia_Asignar(
           int CodEmpresa,
           string usuario,
           CrdCatalogoPolizasGarantiaAsignarReq req)
        {
            return _BL.Crd_CatalogoPolizas_Garantia_Asignar(CodEmpresa, usuario, req);
        }
    }
}
