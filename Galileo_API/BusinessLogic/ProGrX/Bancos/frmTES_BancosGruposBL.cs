using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesBancosGruposBL
    {
        private readonly FrmTesBancosGruposDB _db;

        public FrmTesBancosGruposBL(IConfiguration config)
        {
            _db = new FrmTesBancosGruposDB(config);
        }

        public ErrorDto<TesBancosGruposLista> Tes_BancosGruposLista_Obtener(int CodEmpresa, string filtros)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
            return _db.Tes_BancosGruposLista_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<TesBancosGruposData>> Tes_BancosGruposExportar_Obtener(int CodEmpresa)
        {
            return _db.Tes_BancosGruposExportar_Obtener(CodEmpresa);
        }

        public ErrorDto Tes_BancoGrupoFirma_Guardar(TesBancosGruposImgData firma)
        {
            return _db.Tes_BancoGrupoFirma_Guardar(firma);
        }

        public ErrorDto Tes_BancosGrupo_Guardar(int CodEmpresa, TesBancosGruposData banco)
        {
            return _db.Tes_BancosGrupo_Guardar(CodEmpresa, banco);
        }

        public ErrorDto Tes_BancoGrupo_Eliminar(int CodEmpresa, string cod_grupo)
        {
            return _db.Tes_BancoGrupo_Eliminar(CodEmpresa, cod_grupo);
        }

        public ErrorDto Tes_BancosGrupo_Valida(int CodEmpresa, string cod_grupo)
        {
            return _db.Tes_BancosGrupo_Valida(CodEmpresa, cod_grupo);
        }

    }
}
