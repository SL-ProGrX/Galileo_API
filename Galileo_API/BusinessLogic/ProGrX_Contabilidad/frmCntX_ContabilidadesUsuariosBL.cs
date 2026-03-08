using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXContabilidadesUsuariosBl
    {
        private readonly FrmCntXContabilidadesUsuariosDb _db;

        public FrmCntXContabilidadesUsuariosBl(IConfiguration config)
            => _db = new FrmCntXContabilidadesUsuariosDb(config);

        public ErrorDto<List<CntXContabilidadUsuarioData>> CntXContaUser_ObtenerCatalogo(
            int codEmpresa, bool obtenerUsuarios)
        {
            return _db.CntXContaUser_ObtenerCatalogo(codEmpresa, obtenerUsuarios);
        }

        public ErrorDto<List<CntXContabilidadUsuarioData>> CntXContaUser_ObtenerRelaciones(
            int codEmpresa, bool porContabilidad, string valor)
        {
            return _db.CntXContaUser_ObtenerRelaciones(codEmpresa, porContabilidad, valor);
        }

        public ErrorDto CntXContaUser_GuardarRelacion(
            int codEmpresa, int codContabilidad, string usuario, string usuarioRegistro)
        {
            return _db.CntXContaUser_GuardarRelacion(codEmpresa, codContabilidad, usuario, usuarioRegistro);
        }

        public ErrorDto CntXContaUser_EliminarRelacion(int codEmpresa, int codContabilidad, string usuario)
        {
            return _db.CntXContaUser_EliminarRelacion(codEmpresa, codContabilidad, usuario);
        }
    }
}
