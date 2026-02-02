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
    [Authorize]
    public class FrmCajasCrdAbonosCtPController : ControllerBase
    {
        private readonly FrmCajasCrdAbonosCtPBl _bl;

        public FrmCajasCrdAbonosCtPController(IConfiguration config)
        {
            _bl = new FrmCajasCrdAbonosCtPBl(config);
        }

        [HttpGet("CajasCrdAbonosCtP_ConsultaOperacion_Obtener")]
        public ErrorDto<CajasCrdAbonosCtPData> CajasCrdAbonosCtP_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return _bl.CajasCrdAbonosCtP_ConsultaOperacion_Obtener(CodEmpresa, CodCaja, OperacionId);
        }

        [HttpGet("CajasCrdAbonosCtP_Operaciones_Obtener")]
        public ErrorDto<List<CajasCrdAbonosCtPData>> CajasCrdAbonosCtP_Operaciones_Obtener(int CodEmpresa)
        {
            return _bl.CajasCrdAbonosCtP_Operaciones_Obtener(CodEmpresa);
        }

        [HttpGet("CajasCrdAbonosCtP_TipoDoc_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CajasCrdAbonosCtP_TipoDoc_Obtener(int CodEmpresa, string Caja)
        {
            return _bl.CajasCrdAbonosCtP_TipoDoc_Obtener(CodEmpresa, Caja);
        }

        [HttpGet("CajasCrdAbonosCtP_OperacionTransac_Obtener")]
        public ErrorDto<List<CajasCrdOperacionTransacData>> CajasCrdAbonosCtP_OperacionTransac_Obtener(int CodEmpresa, int IdSolicitud)
        {
            return _bl.CajasCrdAbonosCtP_OperacionTransac_Obtener(CodEmpresa, IdSolicitud);
        }

        [HttpGet("CajasCrdAbonosCtP_DiasActivoFecha_Obtener")]
        public ErrorDto<long> CajasCrdAbonosCtP_DiasActivoFecha_Obtener(int CodEmpresa, string Request)
        {
            return _bl.CajasCrdAbonosCtP_DiasActivoFecha_Obtener(CodEmpresa, Request);
        }

        [HttpGet("CajasCrdAbonosCtP_InfoCancelacion_Obtener")]
        public ErrorDto<CajasCrdAbonosInfoCancelacionData> CajasCrdAbonosCtP_InfoCancelacion_Obtener(int CodEmpresa, string Request)
        {
            return _bl.CajasCrdAbonosCtP_InfoCancelacion_Obtener(CodEmpresa, Request);
        }

        [HttpGet("CajasCrdAbonosCtP_CuotasInfo_Obtener")]
        public ErrorDto<CajasCrdAbonosCuotasInfoData> CajasCrdAbonosCtP_CuotasInfo_Obtener(int codEmpresa, int vOperacion, int vCuotas)
        {
            return _bl.CajasCrdAbonosCtP_CuotasInfo_Obtener(codEmpresa, vOperacion, vCuotas);
        }

        [HttpGet("CajasCrdAbonosCtP_FechaProceso_Obtener")]
        public ErrorDto<int> CajasCrdAbonosCtP_FechaProceso_Obtener(int CodEmpresa, int Proceso, bool Siguiente)
        {
            return _bl.CajasCrdAbonosCtP_FechaProceso_Obtener(CodEmpresa, Proceso, Siguiente);
        }

        [HttpPost("CajasCrdAbonosCtP_Abono_Registrar")]
        public ErrorDto CajasCrdAbonosCtP_Abono_Registrar(int CodEmpresa, CajasCrdAbonosCtPRegistrarAbonoRequest Request)
        {
            return _bl.CajasCrdAbonosCtP_Abono_Registrar(CodEmpresa, Request);
        }
    }
}
