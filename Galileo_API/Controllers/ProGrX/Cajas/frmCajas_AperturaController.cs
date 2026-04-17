using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasAperturaController : ControllerBase
    {
        private readonly FrmCajasAperturaBl BlCajasApertura;

        public FrmCajasAperturaController(IConfiguration config)
        {
            BlCajasApertura = new FrmCajasAperturaBl(config);
        }

        [Authorize]
        [HttpGet("Cajas_Asignadas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Asignadas_Obtener(int CodEmpresa, string Usuario)
        {
            return BlCajasApertura.Cajas_Asignadas_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpGet("Cajas_Apertura_Divisas_Obtener")]
        public ErrorDto<List<CajasDivisaDto>> Cajas_Apertura_Divisas_Obtener(int CodEmpresa, int CodConta)
        {
            return BlCajasApertura.Cajas_Apertura_Divisas_Obtener(CodEmpresa, CodConta);
        }

        [Authorize]
        [HttpGet("Cajas_Apertura_Detalle_Obtener")]
        public ErrorDto<CajaAperturaDetalleDto?> Cajas_Apertura_Detalle_Obtener(int CodEmpresa, string CodCaja)
        {
            return BlCajasApertura.Cajas_Apertura_Detalle_Obtener(CodEmpresa, CodCaja);
        }

        [Authorize]
        [HttpGet("Cajas_Apertura_TEConsulta_Obtener")]
        public ErrorDto<List<CajasAperturaTeConsultaData>> Cajas_Apertura_TEConsulta_Obtener(int CodEmpresa, string CodCaja)
        {
            return BlCajasApertura.Cajas_Apertura_TEConsulta_Obtener(CodEmpresa, CodCaja);
        }

        [Authorize]
        [HttpGet("Cajas_Apertura_UsuarioAutorizado_Validar")]
        public ErrorDto Cajas_Apertura_UsuarioAutorizado_Validar(int CodEmpresa, string Usuario, string Clave, string CodCaja)
        {
            return BlCajasApertura.Cajas_Apertura_UsuarioAutorizado_Validar(CodEmpresa, Usuario, Clave, CodCaja);
        }

        [Authorize]
        [HttpPost("Cajas_Apertura_Aplicar")]
        public ErrorDto<CajaAperturaResponseDto> Cajas_Apertura_Aplicar(int CodEmpresa, CajaAperturaRequestDto req)
        {
            return BlCajasApertura.Cajas_Apertura_Aplicar(CodEmpresa, req);
        }
    }
}