using Microsoft.AspNetCore.Mvc;
using PgxAPI.BusinessLogic;
using PgxAPI.DataBaseTier;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;

namespace PgxAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class mProGrX_AuxiliarController : Controller
    {
        private readonly IConfiguration _config;

        public mProGrX_AuxiliarController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("fxInvPeriodos")]
        public bool fxInvPeriodos(int CodEmpresa, string vfecha)
        {
            return new mProGrX_AuxiliarBL(_config).fxInvPeriodos(CodEmpresa, vfecha);
        }


        [HttpPost("fxInvVerificaLineaDetalle")]
        public ErrorDTO fxInvVerificaLineaDetalle(int CodEmpresa, int ColCantidad, string vMov, int ColProd, int ColBod1, int ColBod2, List<FacturaDetalleDto> vGrid)
        {
            return new mProGrX_AuxiliarBL(_config).fxInvVerificaLineaDetalle(CodEmpresa
                              , ColCantidad, vMov, ColProd, ColBod1, ColBod2, vGrid);
        }


        [HttpGet("fxInvPeriodoEstado")]
        public bool fxInvPeriodoEstado(int CodEmpresa, string vfecha)
        {
            return new mProGrX_AuxiliarBL(_config).fxInvPeriodoEstado(CodEmpresa, vfecha);
        }


        [HttpPost("sbInvInventario")]
        public ErrorDTO sbInvInventario(int CodEmpresa, CompraInventarioDTO inventario)
        {
            return new mProGrX_AuxiliarBL(_config).sbInvInventario(CodEmpresa, inventario);
        }


        [HttpGet("fxCxPParametro")]
        public ErrorDTO<ParametroValor> fxCxPParametro(int CodEmpresa, string Cod_Parametro)
        {
            return new mProGrX_AuxiliarBL(_config).fxCxPParametro(CodEmpresa, Cod_Parametro);
        }


        [HttpGet("fxInvTransaccionesAutoriza")]
        public ErrorDTO fxInvTransaccionesAutoriza(int CodEmpresa, string Boleta, string TipoTran, string AutorizaUser)
        {
            return new mProGrX_AuxiliarBL(_config).fxInvTransaccionesAutoriza(CodEmpresa, Boleta, TipoTran, AutorizaUser);
        }

        [HttpGet("fxSIFCCodigos")]
        public ConsultaDescripcion fxSIFCCodigos(int CodEmpresa, string vTipoDC, string vCodDesX, string vTabla, int Cod_Conta)
        {
            return new mProGrX_AuxiliarBL(_config).fxSIFCCodigos(CodEmpresa, vTipoDC, vCodDesX, vTabla, Cod_Conta);
        }

        [HttpGet("ActivosSinAsignar_Obtener")]
        public ErrorDTO<int> ActivosSinAsignar_Obtener(int CodEmpresa, string usuario)
        {
            return new mProGrX_AuxiliarBL(_config).ActivosSinAsignar_Obtener(CodEmpresa, usuario);
        }

        [HttpPatch("FndControlAutoriza_Guardar")]
        public int FndControlAutoriza_Guardar(FndControlAutorizaData request)
        {
            return new mProGrX_AuxiliarBL(_config).FndControlAutoriza_Guardar(request);
        }

        [HttpDelete("FndControlAutoriza_Eliminar")]
        public int FndControlAutoriza_Eliminar(FndControlAutorizaData request)
        {
            return new mProGrX_AuxiliarDB(_config).FndControlAutoriza_Eliminar(request);
        }

        [HttpPost("FndControlAutoriza_Insertar")]
        public int FndControlAutoriza_Insertar(FndControlAutorizaData request)
        {
            return new mProGrX_AuxiliarDB(_config).FndControlAutoriza_Insertar(request);
        }

        [HttpPut("FndControlCambios_Autoriza")]
        public int FndControlCambios_Autoriza(int CodEmpresa, int idCambio, string usuario)
        {
            return new mProGrX_AuxiliarBL(_config).FndControlCambios_Autoriza(CodEmpresa, idCambio, usuario);
        }


    }
}
