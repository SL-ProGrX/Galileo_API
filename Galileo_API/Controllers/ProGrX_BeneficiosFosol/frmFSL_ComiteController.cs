using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_Comite")]
    [Authorize]
    [ApiController]
    public sealed class FrmFslComiteController
        : ControllerBase
    {
        private readonly FrmFslComiteBL _bl;

        public FrmFslComiteController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmFslComiteBL(config);
        }

        [HttpGet(
            "FSL_Comite_Comites_Obtener")]
        public ErrorDto<
            FslListaPaginadaDto<FslComiteDto>>
            FSL_Comite_Comites_Obtener(
                int CodEmpresa,
                string filtros = "")
        {
            return _bl.FSL_Comite_Comites_Obtener(
                CodEmpresa,
                filtros);
        }

        [HttpGet(
            "FSL_Comite_ComitesActivos_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_Comite_ComitesActivos_Obtener(
                int CodEmpresa)
        {
            return _bl
                .FSL_Comite_ComitesActivos_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "FSL_Comite_Miembros_Obtener")]
        public ErrorDto<
            FslListaPaginadaDto<FslComiteMiembroDto>>
            FSL_Comite_Miembros_Obtener(
                int CodEmpresa,
                string filtros = "")
        {
            return _bl.FSL_Comite_Miembros_Obtener(
                CodEmpresa,
                filtros);
        }

        [HttpPost(
            "FSL_Comite_Comite_Registrar")]
        public ErrorDto FSL_Comite_Comite_Registrar(
            int CodEmpresa,
            FslComiteGuardarRequest request)
        {
            return _bl.FSL_Comite_Comite_Registrar(
                CodEmpresa,
                request);
        }

        [HttpPut(
            "FSL_Comite_Comite_Actualizar")]
        public ErrorDto FSL_Comite_Comite_Actualizar(
            int CodEmpresa,
            FslComiteGuardarRequest request)
        {
            return _bl.FSL_Comite_Comite_Actualizar(
                CodEmpresa,
                request);
        }

        [HttpDelete(
            "FSL_Comite_Comite_Eliminar")]
        public ErrorDto FSL_Comite_Comite_Eliminar(
            int CodEmpresa,
            string codComite,
            string usuario)
        {
            return _bl.FSL_Comite_Comite_Eliminar(
                CodEmpresa,
                codComite,
                usuario);
        }

        [HttpPost(
            "FSL_Comite_Miembro_Registrar")]
        public ErrorDto FSL_Comite_Miembro_Registrar(
            int CodEmpresa,
            FslComiteMiembroGuardarRequest request)
        {
            return _bl.FSL_Comite_Miembro_Registrar(
                CodEmpresa,
                request);
        }

        [HttpPut(
            "FSL_Comite_Miembro_Actualizar")]
        public ErrorDto FSL_Comite_Miembro_Actualizar(
            int CodEmpresa,
            FslComiteMiembroGuardarRequest request)
        {
            return _bl.FSL_Comite_Miembro_Actualizar(
                CodEmpresa,
                request);
        }

        [HttpDelete(
            "FSL_Comite_Miembro_Eliminar")]
        public ErrorDto FSL_Comite_Miembro_Eliminar(
            int CodEmpresa,
            string codComite,
            string cedula,
            string usuario)
        {
            return _bl.FSL_Comite_Miembro_Eliminar(
                CodEmpresa,
                codComite,
                cedula,
                usuario);
        }
    }
}