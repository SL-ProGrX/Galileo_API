using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXUtilEliminaContaBl
    {
        private readonly FrmCntXUtilEliminaContaDb _db;

        public FrmCntXUtilEliminaContaBl(IConfiguration config) => _db = new FrmCntXUtilEliminaContaDb(config);


        public ErrorDto<List<CntxContabilidadListaDto>> CntxUtil_Contabilidades_Obtener(int codEmpresa)
        {
            return _db.CntxUtil_Contabilidades_Obtener(codEmpresa);
        }

        public ErrorDto<bool> CntxUtil_Contabilidades_Eliminar(CntxUtilEliminaContabilidadesRequestDto request)
        {
            return _db.CntxUtil_Contabilidades_Eliminar(request);
        }
    }
}