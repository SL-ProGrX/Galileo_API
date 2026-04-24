using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaTiposSalariosBl
    {
        private readonly FrmPreaTiposSalariosDb _db;

        public FrmPreaTiposSalariosBl(IConfiguration config)
            => _db = new FrmPreaTiposSalariosDb(config);

        public ErrorDto<List<CrdPreaTiposSalariosData>> CrPreaTiposSalarios_Obtener(int codEmpresa)
        {
            return _db.CrPreaTiposSalarios_Obtener(codEmpresa);
        }

        public ErrorDto CrPreaTiposSalarios_Guardar(int codEmpresa, string usuario, CrdPreaTiposSalariosData request)
        {
            return _db.CrPreaTiposSalarios_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CrPreaTiposSalarios_Eliminar(int codEmpresa, string tipoSalario, string usuario)
        {
            return _db.CrPreaTiposSalarios_Eliminar(codEmpresa, tipoSalario, usuario);
        }
    }
}