using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXEmpresaBl
    {
        private readonly FrmCntXEmpresaDb _db;

        public FrmCntXEmpresaBl(IConfiguration config) => _db = new FrmCntXEmpresaDb(config);

        public ErrorDto<CntXEmpresaDto> CntXEmpresa_Obtener(int codEmpresa)
        {
            return _db.CntXEmpresa_Obtener(codEmpresa);
        }

        public ErrorDto CntXEmpresa_Guardar(int codEmpresa, string usuario, CntXEmpresaDto request)
        {
            return _db.CntXEmpresa_Guardar(codEmpresa, usuario, request);
        }
    }
}
