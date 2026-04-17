using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasAutorizacionSaldosEnTransitoBL
    {
        private readonly FrmCajasAutorizacionSaldosEnTransitoDB _db;

        public FrmCajasAutorizacionSaldosEnTransitoBL(IConfiguration config)
        {
            _db = new FrmCajasAutorizacionSaldosEnTransitoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>Cajas_SaldosFavor_TiposDoc_DropDown_Obtener(int CodEmpresa)
        {
            return _db.Cajas_SaldosFavor_Tipos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>Cajas_SaldosFavor_EntidadesPago_DropDown_Obtener(int CodEmpresa)
        {
            return _db.Cajas_EntidadesPagadoras_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>Cajas_SaldosFavor_OrigenRecursos_DropDown_Obtener(int CodEmpresa)
        {
            return _db.Cajas_OrigenRecursos_Obtener(CodEmpresa);
        }

        public ErrorDto<CajasSaldosFavorLista>Cajas_SaldosFavor_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosSaldosFavorTransito>(jfiltros)?? new FiltrosSaldosFavorTransito();
            return _db.Cajas_SaldosFavor_ListaObtener(CodEmpresa, filtros);
        }

        public ErrorDto<CajasSaldosFavorLista> Cajas_SaldosFavor_Lista_Export(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosSaldosFavorTransito>(jfiltros)
                          ?? new FiltrosSaldosFavorTransito();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return _db.Cajas_SaldosFavor_ListaObtener(CodEmpresa, filtros);
        }

        public ErrorDto Cajas_SaldosFavor_ValoresTransito_Autorizar(int CodEmpresa, CajasSaldosFavorAutorizaRequest data)
        {
            return _db.Cajas_SaldosFavor_Autoriza(CodEmpresa, data);
        }

        public ErrorDto<CajasEmpresaInfoDto> Cajas_SaldosFavor_EmpresaInfo_Obtener(int CodEmpresa)
        {
            return _db.Cajas_SaldosFavor_EmpresaInfo_Obtener(CodEmpresa);
        }
    }
}