using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCCuentasCorreccionesModels;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasCorreccionesBL
    {
        private readonly FrmCxCCuentasCorreccionesDb _db;

        public FrmCxCCuentasCorreccionesBL(IConfiguration config) => _db = new FrmCxCCuentasCorreccionesDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesBancos_Obtener(int CodEmpresa)
        {             
            return _db.CxC_CuentasCorreccionesBancos_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesCuentasBancarias_Obtener(int CodEmpresa, string cedula, string codBanco)
        {
            return _db.CxC_CuentasCorreccionesCuentasBancarias_Obtener(CodEmpresa, cedula, codBanco);
        }
        public ErrorDto<GeneralData> CxC_CuentasCorreccionesAutorizado_Consultar(int codEmpresa, string cedula, string cedulaAutorizado, int orden)
        {
            return _db.CxC_CuentasCorreccionesAutorizado_Consultar(codEmpresa, cedula, cedulaAutorizado, orden);
        }
        public ErrorDto<ContratoData> CxC_CuentasCorreccionesContrato_Consultar(int codEmpresa, int orden, string cedula, string concepto, string contrato)
        {
            return _db.CxC_CuentasCorreccionesContrato_Consultar(codEmpresa,  orden,  cedula,  concepto,  contrato);
        }
        public ErrorDto<ContratoData> CxC_ContratoDetalle_Consultar(int codEmpresa, string cedula, string contrato)
        {
            return _db.CxC_ContratoDetalle_Consultar(codEmpresa, cedula, contrato);
        }
        public ErrorDto<GeneralData> CxC_CuentasCorreccionesConceptos_Consultar(int codEmpresa, int orden, string concepto)
        {
            return _db.CxC_CuentasCorreccionesConceptos_Consultar( codEmpresa,  orden,  concepto);
        }
        public ErrorDto<GeneralData> CxC_CuentasCorreccionesPagadores_Consultar(int codEmpresa, int orden, bool mCntPagadorAbierto, string cedula, string pagadorCedula, string contrato)
        {
            return _db.CxC_CuentasCorreccionesPagadores_Consultar(codEmpresa,  orden,  mCntPagadorAbierto,  cedula,  pagadorCedula,  contrato);
        }
        public ErrorDto<CuentaPorCobrarData> CxC_CuentasCorrecciones_Consultar(int CodEmpresa, int operacion)
        {
            return _db.CxC_CuentasCorrecciones_Consultar(CodEmpresa, operacion);
        }
        public ErrorDto<string> CxC_CuentasCorreccionesClientesNombre_Consultar(int CodEmpresa, string cedula)
        {
            return _db.CxC_CuentasCorreccionesClientesNombre_Consultar(CodEmpresa, cedula);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesClientes_Listado(int CodEmpresa)
        {
            return _db.CxC_CuentasCorreccionesClientes_Listado(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesConceptos_Listado(int CodEmpresa)
        {
            return _db.CxC_CuentasCorreccionesConceptos_Listado(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesContratos_Listado(int CodEmpresa, string cedula, string concepto)
        {
            return _db.CxC_CuentasCorreccionesContratos_Listado(CodEmpresa,cedula, concepto);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesPagadores_Listado(int CodEmpresa, bool mCntPagadorAbierto, string cedula, string contrato) { 
            return _db.CxC_CuentasCorreccionesPagadores_Listado( CodEmpresa,  mCntPagadorAbierto,  cedula,  contrato);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesAutorizados_Listado(int CodEmpresa, string cedula)
        {
            return _db.CxC_CuentasCorreccionesAutorizados_Listado(CodEmpresa, cedula);
        }
        public ErrorDto<ConceptosData> CxC_CuentasCorreccionesConceptosDatos_Obtener(int CodEmpresa, string concepto) { 
             
            return _db.CxC_CuentasCorreccionesConceptosDatos_Obtener(CodEmpresa, concepto);
        }
        public ErrorDto CxC_CuentasCorrecciones_Actualizar(int codEmpresa, string usuario, CuentaPorCobrarData datos)
        {
            return _db.CxC_CuentasCorrecciones_Actualizar(codEmpresa, usuario, datos);
        }
   

    }
}
