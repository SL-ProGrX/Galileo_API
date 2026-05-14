using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Fondos;

namespace Galileo_API.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndCdpsTasasController : ControllerBase
    {
        private readonly FrmFndCdpsTasasBl _bl;

        public FrmFndCdpsTasasController(IConfiguration config) => _bl = new FrmFndCdpsTasasBl(config);

        [Authorize]
        [HttpGet("Fnd_CdpsTasas_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_CdpsTasas_Catalogo_Obtener(int CodEmpresa, int Index)
        {
            return _bl.Fnd_CdpsTasas_Catalogo_Obtener(CodEmpresa, Index);
        }

        [Authorize]
        [HttpGet("Fnd_CdpsTasas_Obtener")]
        public ErrorDto<TablasListaGenericaModel> Fnd_CdpsTasas_Obtener(int CodEmpresa, bool Exporta, string Filtros)
        {
            return _bl.Fnd_CdpsTasas_Obtener(CodEmpresa, Exporta, Filtros);
        }

        [Authorize]
        [HttpPost("Fnd_CdpsTasas_Config_Guardar")]
        public ErrorDto Fnd_CdpsTasas_Config_Guardar(int CodEmpresa, FndCdpsTasaRefData Data)
        {
            return _bl.Fnd_CdpsTasas_Config_Guardar(CodEmpresa, Data);
        }

        [Authorize]
        [HttpGet("Fnd_CdpsTasas_Planes_Obtener")]
        public ErrorDto<List<FndCdpsTasaPlanesDto>> Fnd_CdpsTasas_Planes_Obtener(int CodEmpresa, string CodTasaRef, string? Filtro)
        {
            return _bl.Fnd_CdpsTasas_Planes_Obtener(CodEmpresa, CodTasaRef, Filtro);
        }

        [Authorize]
        [HttpPost("Fnd_CdpsTasas_Plan_Asignar")]
        public ErrorDto Fnd_CdpsTasas_Plan_Asignar(int CodEmpresa, string CodTasaRef, string CodPlan, string Usuario, int Accion)
        {
            return _bl.Fnd_CdpsTasas_Plan_Asignar(CodEmpresa, CodTasaRef, CodPlan, Usuario, Accion);
        }

        [Authorize]
        [HttpGet("Fnd_CdpsTasas_Vencimiento_Obtener")]
        public ErrorDto<List<FndCdpTasasVencimientoDto>> Fnd_CdpsTasas_Vencimiento_Obtener(int CodEmpresa, string CodTasaRef, int IdPlazo)
        {
            return _bl.Fnd_CdpsTasas_Vencimiento_Obtener(CodEmpresa, CodTasaRef, IdPlazo);
        }

        [Authorize]
        [HttpPost("Fnd_CdpsTasas_Vencimiento_Guardar")]
        public ErrorDto Fnd_CdpsTasas_Vencimiento_Guardar(int CodEmpresa, string CodTasaRef, int IdCupon, int IdPlazo, decimal Tasa, string Usuario)
        {
            return _bl.Fnd_CdpsTasas_Vencimiento_Guardar(CodEmpresa, CodTasaRef, IdCupon, IdPlazo, Tasa, Usuario);
        }

        [Authorize]
        [HttpPost("Fnd_CdpsTasas_Estado_Actualizar")]
        public ErrorDto Fnd_CdpsTasas_Estado_Actualizar(int CodEmpresa, string CodTasaRef, bool Estado, string Notas, string Usuario)
        {
            return _bl.Fnd_CdpsTasas_Estado_Actualizar(CodEmpresa, CodTasaRef, Estado, Notas, Usuario);
        }

        [Authorize]
        [HttpGet("Fnd_CdpsTasas_Bitacora_Obtener")]
        public ErrorDto<List<FndCdpsTasaBitacoraDto>> Fnd_CdpsTasas_Bitacora_Obtener(int CodEmpresa, string CodTasaRef)
        {
            return _bl.Fnd_CdpsTasas_Bitacora_Obtener(CodEmpresa, CodTasaRef);
        }

        [Authorize]
        [HttpDelete("Fnd_CdpsTasas_Eliminar")]
        public ErrorDto Fnd_CdpsTasas_Eliminar(int CodEmpresa, string CodTasaRef, string Usuario)
        {
            return _bl.Fnd_CdpsTasas_Eliminar(CodEmpresa, CodTasaRef, Usuario);
        }
    }
}