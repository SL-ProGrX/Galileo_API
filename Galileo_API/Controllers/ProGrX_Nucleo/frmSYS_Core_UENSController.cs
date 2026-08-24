using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.SYS;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysCoreUensController : ControllerBase
    {
        private readonly FrmSysCoreUensBL _bl;
        public FrmSysCoreUensController(IConfiguration config)
        {
            _bl = new FrmSysCoreUensBL(config);
        }

        [HttpGet("Core_UENS_Obtener")]
        [Authorize]
        public ErrorDto<CoreUeNsDtoList> Core_UENS_Obtener(int CodCliente, string filtros)
        {
            return _bl.Core_UENS_Obtener(CodCliente, filtros);
        }

        [HttpPost("Core_UENS_Upsert")]
        [Authorize]
        public ErrorDto Core_UENS_Upsert(int CodCliente, string usuario, CoreUeNsDto request)
        {
            return _bl.Core_UENS_Upsert(CodCliente, usuario, request);
        }

        [HttpPost("Core_SubUnidad_Upsert")]
        [Authorize]
        public ErrorDto Core_SubUnidad_Upsert(int CodCliente, string usuario, string? unidad_anterior, CoreUeNsDto request)
        {
            return _bl.Core_SubUnidad_Upsert(CodCliente, usuario, unidad_anterior, request);
        }

        [HttpPost("Core_SubCentroCosto_Upsert")]
        [Authorize]
        public ErrorDto Core_SubCentroCosto_Upsert(int CodCliente, string usuario, CoreUeNsDto request)
        {
            return _bl.Core_SubCentroCosto_Upsert(CodCliente, usuario, request);
        }

        [HttpDelete("Core_UENS_Delete")]
        [Authorize]
        public ErrorDto Core_UENS_Delete(int CodCliente, string cod_unidad)
        {
            return _bl.Core_UENS_Delete(CodCliente, cod_unidad);
        }

        [HttpDelete("Core_SubUnidad_Delete")]
        public ErrorDto Core_SubUnidad_Delete(int CodCliente, string cod_unidad, string cntx_unidad)
        {
            return _bl.Core_SubUnidad_Delete(CodCliente, cod_unidad, cntx_unidad);
        }

        [HttpDelete("Core_SubCentroCosto_Delete")]
        [Authorize]
        public ErrorDto Core_SubCentroCosto_Delete(int CodCliente, string cod_unidad)
        {
            return _bl.Core_SubCentroCosto_Delete(CodCliente, cod_unidad);
        }

        [HttpGet("Core_Miembros_Obtener")]
        [Authorize]
        public ErrorDto<List<CoreUsuariosDto>> Core_Miembros_Obtener(int CodCliente, string cod_unidad, string? filtro)
        {
            return _bl.Core_Miembros_Obtener(CodCliente, cod_unidad, filtro);
        }

        [HttpPost("Core_Miembros_Registro")]
        [Authorize]
        public ErrorDto Core_Miembros_Registro(int CodCliente, string cod_unidad, CoreUsuariosDto request)
        {
            return _bl.Core_Miembros_Registro(CodCliente, cod_unidad, request);
        }

        [HttpGet("Core_Roles_Obtener")]
        [Authorize]
        public ErrorDto<List<CoreRolesDto>> Core_Roles_Obtener(int CodCliente, string cod_unidad, string? filtro)
        {
            return _bl.Core_Roles_Obtener(CodCliente, cod_unidad, filtro);
        }

        [HttpPost("Core_Roles_Registro")]
        [Authorize]
        public ErrorDto Core_Roles_Registro(int CodCliente, string cod_unidad, CoreRolesDto request)
        {
            return _bl.Core_Roles_Registro(CodCliente, cod_unidad, request);
        }

        [HttpGet("Core_UENLista_Obtener")]
        [Authorize]
        public ErrorDto<List<UensListaDatos>> Core_UENLista_Obtener(int CodCliente, string? usuario)
        {
            return _bl.Core_UENLista_Obtener(CodCliente, usuario);
        }

        [HttpGet("Core_UENSPrincipales_Obtener")]
        [Authorize]
        public ErrorDto<CoreUeNsDtoList> Core_UENSPrincipales_Obtener(int CodCliente, string filtros)
        {
            return _bl.Core_UENSPrincipales_Obtener(CodCliente, filtros);
        }

        [HttpGet("Core_SubUnidades_Obtener")]
        [Authorize]
        public ErrorDto<CoreUeNsDtoList> Core_SubUnidades_Obtener(int CodCliente, string cod_unidad, int contabilidad)
        {
            return _bl.Core_SubUnidades_Obtener(CodCliente, cod_unidad, contabilidad);
        }

        [HttpGet("Core_SubCentroCosto_Obtener")]
        [Authorize]
        public ErrorDto<CoreUeNsDtoList> Core_SubCentroCosto_Obtener(int CodCliente, string cod_unidad, string sub_unidad)
        {
            return _bl.Core_SubCentroCosto_Obtener(CodCliente, cod_unidad, sub_unidad);
        }

    }
}