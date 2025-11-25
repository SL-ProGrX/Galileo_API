using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/mProGrX_Auxiliar")]
    [Route("api/mProGrXAuxiliar")]
    [ApiController]
    public class MProGrXAuxiliarController : ControllerBase
    {
        private readonly IConfiguration _config;

        public MProGrXAuxiliarController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("fxInvPeriodos")]
        public bool fxInvPeriodos(int CodEmpresa, string vfecha)
        {
            return new MProGrXAuxiliarBl(_config).fxInvPeriodos(CodEmpresa, vfecha);
        }


        [HttpPost("fxInvVerificaLineaDetalle")]
        public ErrorDto fxInvVerificaLineaDetalle(int CodEmpresa, int ColCantidad, string vMov, int ColProd, int ColBod1, int ColBod2, List<FacturaDetalleDto> vGrid)
        {
            return new MProGrXAuxiliarBl(_config).fxInvVerificaLineaDetalle(CodEmpresa
                              , ColCantidad, vMov, ColProd, ColBod1, ColBod2, vGrid);
        }


        [HttpGet("fxInvPeriodoEstado")]
        public bool fxInvPeriodoEstado(int CodEmpresa, string vfecha)
        {
            return new MProGrXAuxiliarBl(_config).fxInvPeriodoEstado(CodEmpresa, vfecha);
        }


        [HttpPost("sbInvInventario")]
        public ErrorDto sbInvInventario(int CodEmpresa, CompraInventarioDto inventario)
        {
            return new MProGrXAuxiliarBl(_config).sbInvInventario(CodEmpresa, inventario);
        }


        [HttpGet("fxCxPParametro")]
        public ErrorDto<ParametroValor> fxCxPParametro(int CodEmpresa, string Cod_Parametro)
        {
            return new MProGrXAuxiliarBl(_config).fxCxPParametro(CodEmpresa, Cod_Parametro);
        }


        [HttpGet("fxInvTransaccionesAutoriza")]
        public ErrorDto fxInvTransaccionesAutoriza(int CodEmpresa, string Boleta, string TipoTran, string AutorizaUser)
        {
            return new MProGrXAuxiliarBl(_config).fxInvTransaccionesAutoriza(CodEmpresa, Boleta, TipoTran, AutorizaUser);
        }

        [HttpGet("fxSIFCCodigos")]
        public ConsultaDescripcion fxSIFCCodigos(int CodEmpresa, string vTipoDC, string vCodDesX, string vTabla, int Cod_Conta)
        {
            return new MProGrXAuxiliarBl(_config).fxSIFCCodigos(CodEmpresa, vTipoDC, vCodDesX, vTabla, Cod_Conta);
        }

        [HttpGet("ActivosSinAsignar_Obtener")]
        public ErrorDto<int> ActivosSinAsignar_Obtener(int CodEmpresa, string usuario)
        {
            return new MProGrXAuxiliarBl(_config).ActivosSinAsignar_Obtener(CodEmpresa, usuario);
        }

        [HttpPatch("FndControlAutoriza_Guardar")]
        public int FndControlAutoriza_Guardar(FndControlAutorizaData request)
        {
            return new MProGrXAuxiliarBl(_config).FndControlAutoriza_Guardar(request);
        }

        [HttpDelete("FndControlAutoriza_Eliminar")]
        public int FndControlAutoriza_Eliminar(FndControlAutorizaData request)
        {
            return new MProGrXAuxiliarDB(_config).FndControlAutoriza_Eliminar(request);
        }

        [HttpPost("FndControlAutoriza_Insertar")]
        public int FndControlAutoriza_Insertar(FndControlAutorizaData request)
        {
            return new MProGrXAuxiliarDB(_config).FndControlAutoriza_Insertar(request);
        }

        [HttpPut("FndControlCambios_Autoriza")]
        public int FndControlCambios_Autoriza(int CodEmpresa, int idCambio, string usuario)
        {
            return new MProGrXAuxiliarBl(_config).FndControlCambios_Autoriza(CodEmpresa, idCambio, usuario);
        }
    }
}
