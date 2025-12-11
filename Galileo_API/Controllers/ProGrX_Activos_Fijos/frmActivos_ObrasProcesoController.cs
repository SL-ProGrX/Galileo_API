using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosObrasProcesoController : ControllerBase
    {
        private readonly FrmActivosObrasProcesoBL _bl;

        public FrmActivosObrasProcesoController(IConfiguration config)
        {
            _bl = new FrmActivosObrasProcesoBL(config);
        }

        [Authorize]
        [HttpPost("Activos_Obras_Actualizar")]
        public ErrorDto Activos_Obras_Actualizar(int CodEmpresa, string estado, DateTime fecha_finiquito, string contrato)
        {
            return _bl.Activos_Obras_Actualizar(CodEmpresa, estado, fecha_finiquito, contrato);
        }

        [Authorize]
        [HttpGet("Activos_ObrasTipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ObrasTipos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_ObrasTipos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_ObrasTiposDesem_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ObrasTiposDesem_Obtener(int CodEmpresa)
        {
            return _bl.Activos_ObrasTiposDesem_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Obras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Obras_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Obras_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Obra_Proveedores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Obra_Proveedores_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Obra_Proveedores_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Obras_Consultar")]
        public ErrorDto<ActivosObrasData?> Activos_Obras_Consultar(int CodEmpresa, string contrato)
        {
            return _bl.Activos_Obras_Consultar(CodEmpresa, contrato);
        }

        [Authorize]
        [HttpGet("Activos_ObrasAdendums_Obtener")]
        public ErrorDto<List<ActivosObrasProcesoAdendumsData>> Activos_ObrasAdendums_Obtener(int CodEmpresa, string contrato, string filtros)
        {
            return _bl.Activos_ObrasAdendums_Obtener(CodEmpresa, contrato, filtros);
        }

        [Authorize]
        [HttpGet("Activos_ObrasDesembolsos_Obtener")]
        public ErrorDto<List<ActivosObrasProcesoDesembolsosData>> Activos_ObrasDesembolsos_Obtener(int CodEmpresa, string contrato, string filtros)
        {
            return _bl.Activos_ObrasDesembolsos_Obtener(CodEmpresa, contrato, filtros);
        }

        [Authorize]
        [HttpGet("Activos_ObrasResultados_Obtener")]
        public ErrorDto<List<ActivosObrasProcesoResultadosData>> Activos_ObrasResultados_Obtener(int CodEmpresa, string contrato)
        {
            return _bl.Activos_ObrasResultados_Obtener(CodEmpresa, contrato);
        }

        [Authorize]
        [HttpPost("Activos_Obras_Modificar")]
        public ErrorDto Activos_Obras_Modificar(int CodEmpresa, ActivosObrasData data, string usuario)
        {
            return _bl.Activos_Obras_Modificar(CodEmpresa, data, usuario);
        }

        [Authorize]
        [HttpPost("Activos_Obras_Insertar")]
        public ErrorDto Activos_Obras_Insertar(int CodEmpresa, ActivosObrasData data, string usuario)
        {
            return _bl.Activos_Obras_Insertar(CodEmpresa, data, usuario);
        }

        [Authorize]
        [HttpDelete("Activos_Obra_Eliminar")]
        public ErrorDto Activos_Obra_Eliminar(int CodEmpresa, string contrato, string usuario)
        {
            return _bl.Activos_Obra_Eliminar(CodEmpresa, contrato, usuario);
        }

        [Authorize]
        [HttpPost("Activos_ObrasAdendum_Guardar")]
        public ErrorDto Activos_ObrasAdendum_Guardar(int CodEmpresa, ActivosObrasProcesoAdendumsData dato, string usuario, string contrato, decimal addendums, decimal presu_actual)
        {
            return _bl.Activos_ObrasAdendum_Guardar(CodEmpresa, dato, usuario, contrato, addendums, presu_actual);
        }

        [Authorize]
        [HttpPost("Activos_ObrasDesembolso_Guardar")]
        public ErrorDto Activos_ObrasDesembolso_Guardar(int CodEmpresa, ActivosObrasProcesoDesembolsosData dato, string usuario, string contrato)
        {
            return _bl.Activos_ObrasDesembolso_Guardar(CodEmpresa, dato, usuario, contrato);
        }
    }
}