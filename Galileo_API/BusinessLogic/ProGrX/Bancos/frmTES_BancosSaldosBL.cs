using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Newtonsoft.Json;
using PgxAPI.DataBaseTier;


namespace Galileo_API.BusinessLogic
{
    public class FrmTesBancosSaldosBL
    {
        private readonly FrmTesBancosSaldosDB _bancosSaldosDb;

        public FrmTesBancosSaldosBL(IConfiguration config)
        {
            _bancosSaldosDb = new FrmTesBancosSaldosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosSaldos_Grupos_Obtener(int CodEmpresa)
        {
            return _bancosSaldosDb.TES_BancosSaldos_Grupos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosSaldos_Cuentas_Obtener(int CodEmpresa, string CodGrupo)
        {
            return _bancosSaldosDb.TES_BancosSaldos_Cuentas_Obtener(CodEmpresa, CodGrupo);
        }

        public ErrorDto<TablasListaGenericaModel> TES_BancosSaldos_Monitoreo_Obtener(int CodEmpresa, string CodGrupo, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _bancosSaldosDb.TES_BancosSaldos_Monitoreo_Obtener(CodEmpresa, CodGrupo, filtros);
        }

        public ErrorDto TES_BancosSaldos_Monitoreo_Actualizar(int CodEmpresa, string Banco, bool Monitoreo)
        {
            return _bancosSaldosDb.TES_BancosSaldos_Monitoreo_Actualizar(CodEmpresa, Banco, Monitoreo);
        }

        public ErrorDto<TablasListaGenericaModel> TES_BancosSaldos_Historico_Obtener(int CodEmpresa, int Banco, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _bancosSaldosDb.TES_BancosSaldos_Historico_Obtener(CodEmpresa, Banco, filtros);
        }

        public ErrorDto<TesBancosSaldosCierresDto> TES_BancosSaldos_Cierres_Obtener(int CodEmpresa, int Banco)
        {
            return _bancosSaldosDb.TES_BancosSaldos_Cierres_Obtener(CodEmpresa, Banco);
        }

        public ErrorDto TES_BancosSaldos_Cierres_Actualizar(int CodEmpresa, string Usuario, string Datos)
        {
            TesBancosSaldosCierresDto datos = JsonConvert.DeserializeObject<TesBancosSaldosCierresDto>(Datos) ?? new TesBancosSaldosCierresDto();
            return _bancosSaldosDb.TES_BancosSaldos_Cierres_Actualizar(CodEmpresa, Usuario, datos);
        }
    }
}