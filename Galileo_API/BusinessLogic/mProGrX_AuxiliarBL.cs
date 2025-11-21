using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;


namespace Galileo.BusinessLogic
{
    public class MProGrXAuxiliarBl
    {
        private readonly IConfiguration _config;

        public MProGrXAuxiliarBl(IConfiguration config)
        {
            _config = config;
        }

        public bool fxInvPeriodos(int CodEmpresa, string vfecha)
        {
            return new MProGrXAuxiliarDB(_config).fxInvPeriodos(CodEmpresa, vfecha);
        }

        public ErrorDto fxInvVerificaLineaDetalle(int CodEmpresa, int ColCantidad, string vMov, int ColProd, int ColBod1, int ColBod2, List<FacturaDetalleDto> vGrid)
        {
            return new MProGrXAuxiliarDB(_config).fxInvVerificaLineaDetalle(CodEmpresa
                                             , ColCantidad, vMov, ColProd, ColBod1, ColBod2, vGrid);
        }

        public bool fxInvPeriodoEstado(int CodEmpresa, string vfecha)
        {
            return new MProGrXAuxiliarDB(_config).fxInvPeriodoEstado(CodEmpresa, vfecha);
        }

        public ErrorDto sbInvInventario(int CodEmpresa, CompraInventarioDto inventario)
        {
            return new MProGrXAuxiliarDB(_config).sbInvInventario(CodEmpresa, inventario);
        }

        public ErrorDto<ParametroValor> fxCxPParametro(int CodEmpresa, string Cod_Parametro)
        {
            return new MProGrXAuxiliarDB(_config).fxCxPParametro(CodEmpresa, Cod_Parametro);
        }

        public ErrorDto fxInvTransaccionesAutoriza(int CodEmpresa, string Boleta, string TipoTran, string AutorizaUser)
        {
            return new MProGrXAuxiliarDB(_config).fxInvTransaccionesAutoriza(CodEmpresa, Boleta, TipoTran, AutorizaUser);
        }

        public ConsultaDescripcion fxSIFCCodigos(int CodEmpresa, string vTipoDC, string vCodDesX, string vTabla, int Cod_Conta)
        {
            return new MProGrXAuxiliarDB(_config).fxSIFCCodigos(CodEmpresa, vTipoDC, vCodDesX, vTabla, Cod_Conta);
        }

        public ErrorDto<int> ActivosSinAsignar_Obtener(int CodEmpresa, string usuario)
        {
            return new MProGrXAuxiliarDB(_config).ActivosSinAsignar_Obtener(CodEmpresa, usuario);
        }

        public int FndControlAutoriza_Guardar(FndControlAutorizaData request)
        {
            return new MProGrXAuxiliarDB(_config).FndControlAutoriza_Guardar(request);
        }

        public int FndControlAutoriza_Eliminar(FndControlAutorizaData request)
        {
            return new MProGrXAuxiliarDB(_config).FndControlAutoriza_Eliminar(request);
        }

        public int FndControlAutoriza_Insertar(FndControlAutorizaData request)
        {
            return new MProGrXAuxiliarDB(_config).FndControlAutoriza_Insertar(request);
        }

        public int FndControlCambios_Autoriza(int CodEmpresa, int idCambio, string usuario)
        {
            return new MProGrXAuxiliarDB(_config).FndControlCambios_Autoriza(CodEmpresa, idCambio, usuario);
        }

    }
}
