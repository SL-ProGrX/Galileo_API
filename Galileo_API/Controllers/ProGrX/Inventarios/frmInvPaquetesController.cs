using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public sealed class FrmInvPaquetesController
        : ControllerBase
    {
        private readonly FrmInvPaquetesBL _bl;

        public FrmInvPaquetesController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmInvPaquetesBL(config);
        }

        [HttpGet(
            "INV_Paquetes_Lista_Obtener")]
        public ErrorDto<PaqueteDataLista>
            INV_Paquetes_Lista_Obtener(
                int CodEmpresa,
                string filtros)
        {
            return _bl
                .INV_Paquetes_Lista_Obtener(
                    CodEmpresa,
                    filtros);
        }

        [HttpGet(
            "INV_Paquetes_Todos_Obtener")]
        public ErrorDto<List<PaqueteDto>>
            INV_Paquetes_Todos_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_Paquetes_Todos_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "INV_Paquetes_Encabezado_Obtener")]
        public ErrorDto<PaqueteDto>
            INV_Paquetes_Encabezado_Obtener(
                int CodEmpresa,
                int codPaquete)
        {
            return _bl
                .INV_Paquetes_Encabezado_Obtener(
                    CodEmpresa,
                    codPaquete);
        }

        [HttpGet(
            "INV_Paquetes_Detalle_Obtener")]
        public ErrorDto<List<PaqueteDetalleDto>>
            INV_Paquetes_Detalle_Obtener(
                int CodEmpresa,
                int codPaquete)
        {
            return _bl
                .INV_Paquetes_Detalle_Obtener(
                    CodEmpresa,
                    codPaquete);
        }

        [HttpPost(
            "INV_Paquetes_Registrar")]
        public ErrorDto INV_Paquetes_Registrar(
            int CodEmpresa,
            PaqueteDto request)
        {
            return _bl.INV_Paquetes_Registrar(
                CodEmpresa,
                request);
        }

        [HttpPut(
            "INV_Paquetes_Actualizar")]
        public ErrorDto INV_Paquetes_Actualizar(
            int CodEmpresa,
            PaqueteDto request)
        {
            return _bl.INV_Paquetes_Actualizar(
                CodEmpresa,
                request);
        }

        [HttpPost(
            "INV_Paquetes_Detalle_Registrar")]
        public ErrorDto
            INV_Paquetes_Detalle_Registrar(
                int CodEmpresa,
                PaqueteDetalleDto request)
        {
            return _bl
                .INV_Paquetes_Detalle_Registrar(
                    CodEmpresa,
                    request);
        }

        [HttpPut(
            "INV_Paquetes_Detalle_Actualizar")]
        public ErrorDto
            INV_Paquetes_Detalle_Actualizar(
                int CodEmpresa,
                PaqueteDetalleDto request)
        {
            return _bl
                .INV_Paquetes_Detalle_Actualizar(
                    CodEmpresa,
                    request);
        }

        [HttpDelete(
            "INV_Paquetes_Detalle_Eliminar")]
        public ErrorDto
            INV_Paquetes_Detalle_Eliminar(
                int CodEmpresa,
                PaqueteDetalleDto request)
        {
            return _bl
                .INV_Paquetes_Detalle_Eliminar(
                    CodEmpresa,
                    request);
        }
    }
}