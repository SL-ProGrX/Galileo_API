using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCoControlSeguimientoController : ControllerBase
    {
        private readonly FrmCoControlSeguimientoBL BL;

        public FrmCoControlSeguimientoController(IConfiguration config)
        {
            BL = new FrmCoControlSeguimientoBL(config);
        }
        [Authorize]
        [HttpGet("CO_Expediente_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Expedientes_Obtener(int CodEmpresa, string? texto)
        {
            return BL.CO_Expedientes_Obtener(CodEmpresa, texto);
        }
        [Authorize]
        [HttpGet("CO_Gestiones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Gestiones_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_Gestiones_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CO_CausasMora_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_CausasMora_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_CausasMora_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CO_Arreglos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Arreglos_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_Arreglos_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CO_Gestion_Info_Obtener")]
        public ErrorDto<CoControlSegGestionInfoDto> CO_Gestion_Info_Obtener(int CodEmpresa, string cod_gestion, string usuario)
        {
            return BL.CO_Gestion_Info_Obtener(CodEmpresa, cod_gestion, usuario);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_Vence_Rango_Obtener")]
        public ErrorDto<CoControlSegVenceRangoDto> CO_ControlSeguimiento_Vence_Rango_Obtener(int CodEmpresa, string usuario)
        {
            return BL.CO_ControlSeguimiento_Vence_Rango_Obtener(CodEmpresa, usuario);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_HistGestiones_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistGestiones_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CO_ControlSeguimiento_HistGestiones_Lista_Obtener(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_HistGestiones_Lista_Export")]
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistGestiones_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CO_ControlSeguimiento_HistGestiones_Lista_Export(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_HistOficiales_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistOficiales_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CO_ControlSeguimiento_HistOficiales_Lista_Obtener(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_HistOficiales_Lista_Export")]
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistOficiales_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CO_ControlSeguimiento_HistOficiales_Lista_Export(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpPost("CO_ControlSeguimiento_HistOficiales_Actualizar")]
        public ErrorDto CO_ControlSeguimiento_HistOficiales_Actualizar(int CodEmpresa, CoControlSegHistOficialActualizarDto data)
        {
            return BL.CO_ControlSeguimiento_HistOficiales_Actualizar(CodEmpresa, data);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_Fiadores_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Fiadores_Lista_Obtener(int CodEmpresa, string parametros, bool soloOperacionesAtrasadas)
        {
            return BL.CO_ControlSeguimiento_Fiadores_Lista_Obtener(CodEmpresa, parametros, soloOperacionesAtrasadas);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_Fiadores_Lista_Export")]
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Fiadores_Lista_Export(int CodEmpresa, string parametros, bool soloOperacionesAtrasadas)
        {
            return BL.CO_ControlSeguimiento_Fiadores_Lista_Export(CodEmpresa, parametros, soloOperacionesAtrasadas);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_Comisiones_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Comisiones_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CO_ControlSeguimiento_Comisiones_Lista_Obtener(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_Comisiones_Lista_Export")]
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Comisiones_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CO_ControlSeguimiento_Comisiones_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CO_ControlSeguimiento_Registrar")]
        public ErrorDto CO_ControlSeguimiento_Registrar(int CodEmpresa, CoControlSegRegistrarDto data)
        {
            return BL.CO_ControlSeguimiento_Registrar(CodEmpresa, data);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_Estado_Obtener")]
        public ErrorDto<CoControlSegEstadoDto> CO_ControlSeguimiento_Estado_Obtener(int CodEmpresa, string cedula)
        {
            return BL.CO_ControlSeguimiento_Estado_Obtener(CodEmpresa, cedula);
        }
        [Authorize]
        [HttpGet("CO_ControlSeguimiento_HistDetalle_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistDetalle_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CO_ControlSeguimiento_HistDetalle_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CO_ControlSeguimiento_HistDetalle_Lista_Export")]
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistDetalle_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CO_ControlSeguimiento_HistDetalle_Lista_Export(CodEmpresa, parametros);
        }

    }
}
