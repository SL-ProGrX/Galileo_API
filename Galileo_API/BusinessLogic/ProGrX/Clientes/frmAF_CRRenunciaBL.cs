using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFCrRenunciaBL
    {
        private readonly FrmAFCrenunciaDB _db;

        public FrmAFCrRenunciaBL(IConfiguration config)
        {
            _db = new FrmAFCrenunciaDB(config);
        }

        public ErrorDto<List<AfRenunciasSocios>> AF_CR_RenunciasSocios_Obtener(int CodEmpresa)
        {
            return _db.AF_CR_RenunciasSocios_Obtener(CodEmpresa);
        }

        public ErrorDto<AfRenunciasSocioDetalle?> AF_CR_Renuncias_Estado_Obtener(int CodEmpresa, string cedula)
        {
            return _db.AF_CR_Renuncias_Estado_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto<List<AfRenunciaBancos>> AF_CR_Renuncias_Bancos_Obtener(int CodEmpresa, AfRenunciaBancoFiltro filtro)
        {
            return _db.AF_CR_Renuncias_Bancos_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<AfRenunciaEmiteTDoc>> AF_CR_Renuncias_Emite_TDoc(int CodEmpresa, AfRenunciaEmiteTDocFiltro filtro)
        {
            return _db.AF_CR_Renuncias_Emite_TDoc(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Renuncias_TipoAccion_Obtener(int CodEmpresa)
        {
            return _db.AF_CR_Renuncias_TipoAccion_Obtener(CodEmpresa);
        }

        public ErrorDto<AfRenunciaCausasDetalle?> AF_CR_Renuncias_Causas_ObtenerDetalle(int CodEmpresa, int Causa)
        {
            return _db.AF_CR_Renuncias_Causas_ObtenerDetalle(CodEmpresa, Causa);
        }

        public ErrorDto<List<AfRenunciaLiqConsultaPatrimonio>> AF_CR_Renuncias_Liq_Consulta_Patrimonio(int CodEmpresa, String Cedula)
        {
            return _db.AF_CR_Renuncias_Liq_Consulta_Patrimonio(CodEmpresa, Cedula);
        }

        public ErrorDto<List<AfRenunciaExcRentaDetallada>> AF_CR_Renuncias_Exc_Renta_Detallada(int CodEmpresa, decimal Monto)
        {
            return _db.AF_CR_Renuncias_Exc_Renta_Detallada(CodEmpresa, Monto);
        }

        public ErrorDto<List<AfRenunciaLiquidaListaPlanes>> AF_CR_Renuncias_Liquida_ListaPlanes(int CodEmpresa, AfRenunciaLiquidaListaPlanesFiltro filtro)
        {
            return _db.AF_CR_Renuncias_Liquida_ListaPlanes(CodEmpresa, filtro);
        }

        public ErrorDto<List<AfRenunciaCuentaBancaria>> AF_CR_Renuncias_CuentasBancarias_Obtener(int CodEmpresa, AfRenunciaCuentaBancariaFiltro filtro)
        {
            return _db.AF_CR_Renuncias_CuentasBancarias_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<AfRenunciaPromotor>> AF_CR_Renuncias_Promotores_Obtener(int CodEmpresa)
        {
            return _db.AF_CR_Renuncias_Promotores_Obtener(CodEmpresa);
        }

        public ErrorDto<int> AF_CR_Renuncias_Activa(int CodEmpresa, string cedula)
        {
            return _db.AF_CR_Renuncias_Activa(CodEmpresa, cedula);
        }

        public ErrorDto<int> AF_CR_Renuncias_Activa_Otra(int CodEmpresa, string cedula, int codigo)
        {
            return _db.AF_CR_Renuncias_Activa_Otra(CodEmpresa, cedula, codigo);
        }

        public ErrorDto<int> AF_CR_Renuncias_Socio_Existe(int CodEmpresa, string cedula)
        {
            return _db.AF_CR_Renuncias_Socio_Existe(CodEmpresa, cedula);
        }

        public ErrorDto<AfRenuncia?> AF_CR_Renuncias_ObtenerPorId(int CodEmpresa, long CodRenuncia)
        {
            return _db.AF_CR_Renuncias_ObtenerPorId(CodEmpresa, CodRenuncia);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Renuncia_Obtener_Causas(int CodEmpresa, String Tipo)
        {
            return _db.AF_CR_Renuncia_Obtener_Causas(CodEmpresa, Tipo);
        }

        public ErrorDto<AfRenunciaRentaGlobal> AF_CR_Renuncias_Renta_Global(int CodEmpresa, AfRenunciaRentaGlobalFiltro filtro)
        {
            return _db.AF_CR_Renuncias_Renta_Global(CodEmpresa, filtro);
        }

        public ErrorDto<List<AfRenunciaLiquidacionCreditosPersona>> AF_CR_Renuncias_Liquidacion_CreditosPersona(int CodEmpresa, AfRenunciaLiquidacionCreditosPersonaFiltro filtro)
        {
            return _db.AF_CR_Renuncias_Liquidacion_CreditosPersona(CodEmpresa, filtro);
        }

        public ErrorDto<AfRenunciaSinpeNegativo> AF_CR_Renuncias_Sinpe_Negativo(int CodEmpresa, String Cedula )
        {
            return _db.AF_CR_Renuncias_Sinpe_Negativo(CodEmpresa, Cedula);
        }

        public ErrorDto<List<AfRenunciaDetalleHistorico>> AF_CR_Renuncias_ObtenerHistorico(int CodEmpresa, string Cedula)
        {
            return _db.AF_CR_Renuncias_ObtenerHistorico(CodEmpresa, Cedula);
        }

        public ErrorDto<AfRenuncia?> AF_CR_Renuncias_ObtenerPorCodigo(int CodEmpresa, long CodRenuncia)
        {
            return _db.AF_CR_Renuncias_ObtenerPorCodigo(CodEmpresa, CodRenuncia);
        }

        public ErrorDto<int> AF_CR_Renuncias_Liquidacion_Guarda(int CodEmpresa, AfRenunciaLiquidacion request)
        {
            return _db.AF_CR_Renuncias_Liquidacion_Guarda(CodEmpresa, request);
        }

        public ErrorDto<bool> AF_CR_Renuncias_Plan_Insertar(int CodEmpresa, AfRenunciaPlan request)
        {
            return _db.AF_CR_Renuncias_Plan_Insertar(CodEmpresa, request);
        }

        public ErrorDto<bool> AF_CR_Renuncias_Abono_Insertar(int CodEmpresa, AfRenunciaAbono request)
        {
            return _db.AF_CR_Renuncias_Abono_Insertar(CodEmpresa, request);
        }
    }
}