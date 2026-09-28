using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAHExcedentesAplicadoresBL
    {
        private readonly FrmAHExcedentesAplicadoresDB _db;

        public FrmAHExcedentesAplicadoresBL(IConfiguration config)
        {
            _db = new FrmAHExcedentesAplicadoresDB(config);
        }
        public ErrorDto<List<ExcedenteAplicadorDto>>AH_Excedentes_Aplicadores_Lista_Obtener(int CodEmpresa)
        {
            return _db.AH_Excedentes_Aplicadores_Lista_Obtener(CodEmpresa);
        }
        public ErrorDto AH_Excedentes_Aplicadores_Guardar(int CodEmpresa,string usuario,ExcedenteAplicadorDto aplicador)
        {
            return _db.AH_Excedentes_Aplicadores_Guardar(CodEmpresa, usuario,aplicador);
        }
        public ErrorDto AH_Excedentes_Aplicadores_Eliminar(int CodEmpresa,string usuarioAplicador,string usuario)
        {
            return _db.AH_Excedentes_Aplicadores_Eliminar(CodEmpresa,usuarioAplicador,usuario);
        }
    }
}