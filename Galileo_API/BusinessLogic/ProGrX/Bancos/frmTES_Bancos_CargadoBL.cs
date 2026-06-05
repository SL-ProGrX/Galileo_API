using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesBancosCargadoBL
    {
        private readonly FrmTesBancosCargadoDB _Db;

        public FrmTesBancosCargadoBL(IConfiguration config)
        {
            _Db = new FrmTesBancosCargadoDB(config);
        }

        public ErrorDto<List<DropDownListaBancosCargados>> Tes_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            return _Db.Tes_Bancos_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<TesBancoCargadoConceptos>> Tes_BancosCargadoConceptos_Obtener(int CodEmpresa, string? concepto = null)
        {
            return _Db.Tes_BancosCargadoConceptos_Obtener(CodEmpresa, concepto);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroUnidades_Obtener(int CodEmpresa)
        {
            return _Db.Tes_BancosCargadoCentroUnidades_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroCostos_Obtener(int CodEmpresa)
        {
            return _Db.Tes_BancosCargadoCentroCostos_Obtener(CodEmpresa);
        }

        public ErrorDto<TesAutoRegistroLista> Tes_AutoRegistroLista_Obtener(int CodEmpresa, string filtros)
        {
            FiltrosLazyLoadData Jfiltros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
            return _Db.Tes_AutoRegistroLista_Obtener(CodEmpresa, Jfiltros);
        }

        public ErrorDto TES_BancosCargados_Aplicar(int CodEmpresa, string cod_banco, string usuario, List<TesCargadoExcelDto> file)
        {
            return _Db.TES_BancosCargados_Aplicar(CodEmpresa, cod_banco, usuario, file);
        }

        public ErrorDto<List<TeslistaRegistroBancosDto>> TES_ListaRegistroBancos_Obtener(int CodEmpresa, string filtros)
        {
            return _Db.TES_ListaRegistroBancos_Obtener(CodEmpresa, filtros);
        }


        public async Task<ErrorDto> TES_RegistrosBancosCargados_Aplicar(int CodEmpresa, string registroLista)
        {
            return await _Db.TES_RegistrosBancosCargados_Aplicar(CodEmpresa, registroLista);
        }

        public ErrorDto TES_RegistrosBancosCargados_Elimina(int CodEmpresa, string registroLista)
        {
            return _Db.TES_RegistrosBancosCargados_Elimina(CodEmpresa, registroLista);
        }

    }
}