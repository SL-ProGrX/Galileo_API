using Newtonsoft.Json;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasSaldosFavorLiquidaConfiguraBL
    {
        private readonly FrmCajasSaldosFavorLiquidaConfiguraDB _db;
        public FrmCajasSaldosFavorLiquidaConfiguraBL(IConfiguration config)
        {
            _db = new FrmCajasSaldosFavorLiquidaConfiguraDB(config);
        }

        public ErrorDto<CajasSaldosFavorTiposLista> CajasSaldosFavorTipos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CajasSaldosFavorTipos_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<CajasSaldosFavorUsuarioLiquidaLista> CajasSaldosFavorUsuariosLiquida_Obtener(int CodEmpresa, string usuario)
        {
            return _db.CajasSaldosFavorUsuariosLiquida_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto CajasSaldosFavorTipos_Guardar(int CodEmpresa, string usuario, CajasSaldosFavorTiposData data)
        {
            return _db.CajasSaldosFavorTipos_Guardar(CodEmpresa, usuario, data);
        }

        public ErrorDto CajasSaldosFavorTipos_Eliminar(int CodEmpresa, string usuario, string doc_tipo)
        {
            return _db.CajasSaldosFavorTipos_Eliminar(CodEmpresa, usuario, doc_tipo);
        }

        public ErrorDto CajasSaldosFavorTipoLiq_Asigna(int CodEmpresa, string usuarioG, CajasSaldosFavorUsuarioLiquidData data)
        {
            return _db.CajasSaldosFavorTipoLiq_Asigna(CodEmpresa, usuarioG, data);
        }
        
        public ErrorDto<List<DropDownListaGenericaModel>> CajasSaldosFavor_Usuarios_Obtener(int CodEmpresa)
        {
            return _db.CajasSaldosFavor_Usuarios_Obtener(CodEmpresa);
        }
    }
}