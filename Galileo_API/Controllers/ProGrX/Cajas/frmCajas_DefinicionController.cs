using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasDefinicionController : ControllerBase
    {
        private readonly FrmCajasDefinicionBL _bl;

        public FrmCajasDefinicionController(IConfiguration config)
        {
            _bl = new FrmCajasDefinicionBL(config);
        }

        [Authorize]
        [HttpGet("CajasDefinicion_Oficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CajasDefinicion_Oficinas_Obtener(int codEmpresa)
        {
            return _bl.ObtenerOficinasActivas(codEmpresa);
        }

        [Authorize]
        [HttpGet("CajasDefinicion_Cajas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CajasDefinicion_Cajas_Obtener(int codEmpresa)
        {
            return _bl.CajasDefinicion_Cajas_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CajasDefinicion_CajaDetalle_Obtener")]
        public ErrorDto<CajasDefinicionDetalleModel?> CajasDefinicion_CajaDetalle_Obtener(int codEmpresa, string codCaja, string gEnlace)
        {
            return _bl.CajasDefinicion_CajaDetalle_Obtener(codEmpresa, codCaja, gEnlace);
        }

        [Authorize]
        [HttpGet("CajasDefinicion_Recaudadores_Obtener")]
        public ErrorDto<List<CajasRecaudadorModel>> CajasDefinicion_Recaudadores_Obtener(int codEmpresa)
        {
            return _bl.CajasDefinicion_Recaudadores_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CajasDefinicion_DivisasPolitica_Obtener")]
        public ErrorDto<List<CajasDivisaPoliticaModel>> CajasDefinicion_DivisasPolitica_Obtener(int codEmpresa, string codCaja, string gEnlace)
        {
            return _bl.CajasDefinicion_DivisasPolitica_Obtener(codEmpresa, codCaja, gEnlace);
        }

        [Authorize]
        [HttpGet("CajasDefinicion_ServiciosAsignados_Obtener")]
        public ErrorDto<List<CajasServicioAsignadoModel>> CajasDefinicion_ServiciosAsignados_Obtener(int codEmpresa, string codCaja, string codRecaudador)
        {
            return _bl.CajasDefinicion_ServiciosAsignados_Obtener(codEmpresa, codCaja, codRecaudador);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_ServicioAsignar_Insertar")]
        public ErrorDto<bool> CajasDefinicion_ServicioAsignar_Insertar(int codEmpresa, [FromBody] CajasServicioAsignarParams param)
        {
            return _bl.CajasDefinicion_ServicioAsignar_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_ServicioAsignar_Eliminar")]
        public ErrorDto<bool> CajasDefinicion_ServicioAsignar_Eliminar(int codEmpresa, [FromBody] CajasServicioAsignarParams param)
        {
            return _bl.CajasDefinicion_ServicioAsignar_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_AuxiliaresCatalogo_Obtener")]
        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresCatalogo_Obtener(int codEmpresa, [FromBody] CajasAuxiliarFiltroParams param)
        {
            return _bl.CajasDefinicion_AuxiliaresCatalogo_Obtener(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_AuxiliaresFondos_Obtener")]
        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresFondos_Obtener(int codEmpresa, [FromBody] CajasAuxiliarFiltroParams param)
        {
            return _bl.CajasDefinicion_AuxiliaresFondos_Obtener(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_AuxiliaresCxc_Obtener")]
        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresCxc_Obtener(int codEmpresa, [FromBody] CajasAuxiliarFiltroParams param)
        {
            return _bl.CajasDefinicion_AuxiliaresCxc_Obtener(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_AuxiliaresFfp_Obtener")]
        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresFfp_Obtener(int codEmpresa, [FromBody] CajasAuxiliarFiltroParams param)
        {
            return _bl.CajasDefinicion_AuxiliaresFfp_Obtener(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_AuxiliarAsignar_Eliminar")]
        public ErrorDto<bool> CajasDefinicion_AuxiliarAsignar_Eliminar(int codEmpresa, string usuario, [FromBody] CajasAuxiliarAsignarParams param)
        {
            return _bl.CajasDefinicion_AuxiliarAsignar_Eliminar(codEmpresa, usuario, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_AuxiliarAsignar_Insertar")]
        public ErrorDto<bool> CajasDefinicion_AuxiliarAsignar_Insertar(int codEmpresa, string usuario, [FromBody] CajasAuxiliarAsignarParams param)
        {
            return _bl.CajasDefinicion_AuxiliarAsignar_Insertar(codEmpresa, usuario, param);
        }

        [Authorize]
        [HttpGet("CajasDefinicion_FormasPago_Obtener")]
        public ErrorDto<List<CajasFormaPagoAsignadoModel>> CajasDefinicion_FormasPago_Obtener(int codEmpresa, string codCaja)
        {
            return _bl.CajasDefinicion_FormasPago_Obtener(codEmpresa, codCaja);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_FormaPagoAsignar_Insertar")]
        public ErrorDto<bool> CajasDefinicion_FormaPagoAsignar_Insertar(int codEmpresa, [FromBody] CajasFormaPagoAsignarParams param)
        {
            return _bl.CajasDefinicion_FormaPagoAsignar_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_FormaPagoAsignar_Eliminar")]
        public ErrorDto<bool> CajasDefinicion_FormaPagoAsignar_Eliminar(int codEmpresa, [FromBody] CajasFormaPagoAsignarParams param)
        {
            return _bl.CajasDefinicion_FormaPagoAsignar_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CajasDefinicion_Documentos_Obtener")]
        public ErrorDto<List<CajasDocumentoAsignadoModel>> CajasDefinicion_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            return _bl.CajasDefinicion_Documentos_Obtener(codEmpresa, codCaja);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_DocumentoAsignar_Insertar")]
        public ErrorDto<bool> CajasDefinicion_DocumentoAsignar_Insertar(int codEmpresa, [FromBody] CajasDocumentoAsignarParams param)
        {
            return _bl.CajasDefinicion_DocumentoAsignar_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_DocumentoAsignar_Eliminar")]
        public ErrorDto<bool> CajasDefinicion_DocumentoAsignar_Eliminar(int codEmpresa, [FromBody] CajasDocumentoAsignarParams param)
        {
            return _bl.CajasDefinicion_DocumentoAsignar_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CajasDefinicion_UsuariosHistorial_Obtener")]
        public ErrorDto<List<CajasUsuarioHistorialModel>> CajasDefinicion_UsuariosHistorial_Obtener(int codEmpresa, string codCaja)
        {
            return _bl.CajasDefinicion_UsuariosHistorial_Obtener(codEmpresa, codCaja);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_Caja_Insertar")]
        public ErrorDto<bool> CajasDefinicion_Caja_Insertar(int codEmpresa, [FromBody] CajasDefinicionInsertParams param)
        {
            return _bl.CajasDefinicion_Caja_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_Caja_Copiar")]
        public ErrorDto<bool> CajasDefinicion_Caja_Copiar(int codEmpresa, [FromBody] CajasDefinicionCopiaParams param)
        {
            return _bl.CajasDefinicion_Caja_Copiar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDefinicion_Caja_Eliminar")]
        public ErrorDto<bool> CajasDefinicion_Caja_Eliminar(int codEmpresa, string codCaja, string usuario)
        {
            return _bl.CajasDefinicion_Caja_Eliminar(codEmpresa, codCaja, usuario);
        }
    }
}