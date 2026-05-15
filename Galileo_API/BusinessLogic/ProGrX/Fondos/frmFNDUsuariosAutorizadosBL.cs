using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndUsuariosAutorizadosBl
    {
        private readonly FrmFndUsuariosAutorizadosDb _db;

        public FrmFndUsuariosAutorizadosBl(IConfiguration config)
        {
            _db = new FrmFndUsuariosAutorizadosDb(config);
        }

        public ErrorDto<List<FndColaboradoresCcData>> FndColaboradoresCc_Obtener(int CodEmpresa)
        {
            return _db.FndColaboradoresCc_Obtener(CodEmpresa);
        }

        public ErrorDto FndColaboradoresCc_Valida(int CodEmpresa, string usuario)
        {
            return _db.FndColaboradoresCc_Valida(CodEmpresa, usuario);
        }

        public ErrorDto FndColaboradoresCc_Guardar(int CodEmpresa, string usuarioLogueado, FndColaboradoresCcData colaborador)
        {
            return _db.FndColaboradoresCc_Guardar(CodEmpresa, usuarioLogueado, colaborador);
        }

        public ErrorDto FndColaboradoresCc_Eliminar(int CodEmpresa, string usuario)
        {
            return _db.FndColaboradoresCc_Eliminar(CodEmpresa, usuario);
        }
    }
}