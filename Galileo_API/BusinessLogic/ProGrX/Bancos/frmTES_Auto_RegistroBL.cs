using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesAutoRegistroBL
    {

        private readonly FrmTesAutoRegistroDB _autoRegistroDb;

        public FrmTesAutoRegistroBL(IConfiguration config)
        {
            _autoRegistroDb = new FrmTesAutoRegistroDB(config);
        }

        public ErrorDto<TesAutoRegistroDto> Tes_AutoRegistro_Consultar(int CodEmpresa, int autoReg)
        {
            return _autoRegistroDb.Tes_AutoRegistro_Consultar(CodEmpresa, autoReg);
        }

        public ErrorDto Tes_AutoRegistro_Guardar(int CodEmpresa, TesAutoRegistroDto registro)
        {
            return _autoRegistroDb.Tes_AutoRegistro_Guardar(CodEmpresa, registro);
        }

        public ErrorDto Tes_AutoRegistro_Eliminar(int CodEmpresa, string registro)
        {
            TesAutoRegistroDto tesAuto = JsonConvert.DeserializeObject<TesAutoRegistroDto>(registro) ?? new TesAutoRegistroDto();
            return _autoRegistroDb.Tes_AutoRegistro_Eliminar(CodEmpresa, tesAuto);
        }

        public ErrorDto<List<TesAutoRegCtaBancariasData>> Tes_AutoRegistroCtaBancos_Obtener(int CodEmpresa, int? codigo, string? FiltraCtas)
        {
            return _autoRegistroDb.Tes_AutoRegistroCtaBancos_Obtener(CodEmpresa, codigo, FiltraCtas);
        }

        public ErrorDto Tes_AutoRegistroCtaBancos_Asignar(int CodEmpresa, int codigo, int cta, bool asignado, string usuario)
        {
            return _autoRegistroDb.Tes_AutoRegistroCtaBancos_Asignar(CodEmpresa, codigo, cta, asignado, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroTipos_Obtener(int CodEmpresa, int? tipo, string? filtro)
        { 
            return _autoRegistroDb.Tes_AutoRegistroTipos_Obtener(CodEmpresa, tipo, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroCentroCostos_Obtener(int CodEmpresa)
        { 
            return _autoRegistroDb.Tes_AutoRegistroCentroCostos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroCodigoDesc_Obtener(int CodEmpresa, string tipo, string codigo)
        { 
            return _autoRegistroDb.Tes_AutoRegistroCodigoDesc_Obtener(CodEmpresa, tipo, codigo);
        }

        public ErrorDto<List<TesAutoregistroConceptos>> Tes_AutoRegistroConceptos_Obtener(int CodEmpresa, string? concepto = null)
        { 
            return _autoRegistroDb.Tes_AutoRegistroConceptos_Obtener(CodEmpresa, concepto);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroCentroUnidades_Obtener(int CodEmpresa)
        { 
            return _autoRegistroDb.Tes_AutoRegistroCentroUnidades_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroTiposDoc_Obtener(int CodEmpresa, string TipoMov)
        {
            return _autoRegistroDb.Tes_AutoRegistroTiposDoc_Obtener(CodEmpresa, TipoMov);
        }

        public ErrorDto<TesAutoRegistroLista> Tes_AutoRegistroLista_Obtener(int CodEmpresa, string filtros)
        {
            FiltrosLazyLoadData Jfiltros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
            return _autoRegistroDb.Tes_AutoRegistroLista_Obtener(CodEmpresa, Jfiltros);
        }

        public ErrorDto<TesAutoRegistroDto> Tes_AutoRegistro_scroll(int CodEmpresa, int autoReg, int? scroll)
        {
            return _autoRegistroDb.Tes_AutoRegistro_scroll(CodEmpresa, autoReg, scroll);
        }

    }
}
