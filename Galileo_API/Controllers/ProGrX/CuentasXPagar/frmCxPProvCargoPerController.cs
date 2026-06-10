
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxPProvCargoPerController : ControllerBase
    {
        private readonly FrmCxPProvCargoPerBL _bl;

        public FrmCxPProvCargoPerController(IConfiguration config)
        {
            _bl = new FrmCxPProvCargoPerBL(config);
        }

        [HttpGet("Secuencias_Obtener")]
        public ErrorDto<List<Secuencia>> Secuencias_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.Secuencias_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("Cargos_Obtener")]
        public ErrorDto<List<Cargo>> Cargos_Obtener(int CodEmpresa)
        {
            return _bl.Cargos_Obtener(CodEmpresa);
        }

        [HttpGet("CargoDetalle_Obtener")]
        public ErrorDto<CargoPerDto> CargoDetalle_Obtener(int CodEmpresa, int Cod_Proveedor, int Id)
        {
            return _bl.CargoDetalle_Obtener(CodEmpresa, Cod_Proveedor, Id);
        }

        [HttpGet("ProveedorDetalle_Obtener")]
        public ErrorDto<ProveedorInfo> ProveedorDetalle_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.ProveedorDetalle_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("CargosPer_Obtener")]
        public ErrorDto<CargoPerDtoList> CargosPer_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.CargosPer_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        [HttpGet("Pagos_Obtener")]
        public ErrorDto<PagoProvCargosDtoList> Pagos_Obtener(int CodEmpresa, int Cod_Proveedor, int Id, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Pagos_Obtener(CodEmpresa, Cod_Proveedor, Id, pagina, paginacion, filtro);
        }

        [HttpPost("Cargo_Actualizar")]
        public ErrorDto Cargo_Actualizar(int CodEmpresa, CargoPerDto data)
        {
            return _bl.Cargo_Actualizar(CodEmpresa, data);
        }

        [HttpPost("Cargo_Insertar")]
        public ErrorDto Cargo_Insertar(int CodEmpresa, CargoPerDto data)
        {
            return _bl.Cargo_Insertar(CodEmpresa, data);
        }

        [HttpPost("Cargo_Eliminar")]
        public ErrorDto Cargo_Eliminar(int CodEmpresa, CargoPerDto data)
        {
            return _bl.Cargo_Eliminar(CodEmpresa, data);
        }
    }
}