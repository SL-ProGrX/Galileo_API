using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasConsultaMovimientosFormaPagoBL
    {
        private readonly FrmCajasConsultaMovimientosFormaPagoDB _db;

        public FrmCajasConsultaMovimientosFormaPagoBL(IConfiguration config)
        {
            _db = new FrmCajasConsultaMovimientosFormaPagoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_FormasPago_DropDown_Obtener(int CodEmpresa)
        {
            return _db.Cajas_FormasPago_DropDown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Cajas_DropDown_Obtener(int CodEmpresa)
        {
            return _db.Cajas_Cajas_DropDown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_EntidadesPago_DropDown_Obtener(int CodEmpresa)
        {
            return _db.Cajas_EntidadesPagadoras_DropDown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_OrigenRecursos_DropDown_Obtener(int CodEmpresa)
        {
            return _db.Cajas_OrigenRecursos_DropDown_Obtener(CodEmpresa);
        }

        public ErrorDto<long> Cajas_UltimaApertura_Obtener(int CodEmpresa, string CodCaja)
            => _db.Cajas_UltimaApertura_Obtener(CodEmpresa, CodCaja);

        public ErrorDto<CajasMovimientosFormaPagoLista> Cajas_ConsultaMovimientos_FormaPago_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosMovimientosFormaPago>(jfiltros) ?? new FiltrosMovimientosFormaPago();
            return _db.Cajas_ConsultaMovimientos_FormaPago_ListaObtener(CodEmpresa, filtros);
        }

        public ErrorDto<CajasMovimientosFormaPagoLista> Cajas_ConsultaMovimientos_FormaPago_Lista_Export(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosMovimientosFormaPago>(jfiltros) ?? new FiltrosMovimientosFormaPago();
            filtros.pagina = 0;
            filtros.paginacion = 0;
            return _db.Cajas_ConsultaMovimientos_FormaPago_ListaObtener(CodEmpresa, filtros);
        }
    }
}