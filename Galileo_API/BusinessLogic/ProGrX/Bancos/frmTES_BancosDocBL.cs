using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesBancosDocBL
    {

        private readonly FrmTesBancosDocDB _bancosDocDb;

        public FrmTesBancosDocBL(IConfiguration config)
        {
            _bancosDocDb = new FrmTesBancosDocDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancoDocGrupos_Obtener(int CodEmpresa)
        { 
            return _bancosDocDb.Tes_BancoDocGrupos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancoDocBancos_Obtener(int CodEmpresa, string CodGrupo)
        {
            return _bancosDocDb.Tes_BancoDocBancos_Obtener(CodEmpresa, CodGrupo);
        }

        public ErrorDto<List<TesBancosDocData>> Tes_BancoDocTipos_Obtener(int CodEmpresa, string id_banco)
        {
            return _bancosDocDb.Tes_BancoDocTipos_Obtener(CodEmpresa, id_banco);
        }

        public ErrorDto<TesBancoDocDto> Tes_BancoDoc_Obtener(int CodEmpresa, int id_banco, string tipo)
        { 
            return _bancosDocDb.Tes_BancoDoc_Obtener(CodEmpresa, id_banco, tipo);
        }

        public ErrorDto Tes_BancoDoc_Guardar(int CodEmpresa, string jBancoDoc)
        {
            TesBancoDocTipoData bancoDoc = JsonConvert.DeserializeObject<TesBancoDocTipoData>(jBancoDoc) ?? new TesBancoDocTipoData();
            return _bancosDocDb.Tes_BancoDoc_Guardar(CodEmpresa, bancoDoc);
        }

        public ErrorDto TesBancoDoc_Eliminar(int CodEmpresa, int id_banco, string tipo, string usuario)
        { 
            return _bancosDocDb.TesBancoDoc_Eliminar(CodEmpresa, id_banco, tipo, usuario);
        }
    }
}
