using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.BusinessLogic
{
    public class FrmPresUsuariosBl
    {
        readonly FrmPresUsuariosDb _db;

        public FrmPresUsuariosBl(IConfiguration config)
        {
            _db = new FrmPresUsuariosDb(config);
        }

        public ErrorDto<List<PresUsuariosData>> Pres_Usuarios_Obtener(int CodEmpresa, string? Usuario)
        {
            return _db.Pres_Usuarios_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<List<PresContabilidadesData>> Pres_Contabilidades_Obtener(int CodEmpresa, string Usuario)
        {
            return _db.Pres_Contabilidades_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<List<PresUnidadesData>> Pres_Unidades_Obtener(int CodEmpresa, string Usuario, int CodContab)
        {
            return _db.Pres_Unidades_Obtener(CodEmpresa, Usuario, CodContab);
        }

        public ErrorDto Pres_Usuarios_Registro_SP(int CodEmpresa, PresUsuariosInsert request)
        {
            return _db.Pres_Usuarios_Registro_SP(CodEmpresa, request);
        }

        public ErrorDto Pres_Unidades_Registro_SP(int CodEmpresa, PresUnidadesInsert request)
        {
            return _db.Pres_Unidades_Registro_SP(CodEmpresa, request);
        }
    }
}