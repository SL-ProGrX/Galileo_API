using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessTier.ProGrX_Contabilidad
{
    public class FrmCntXPeriodosDefinicionBL
    {
        private readonly FrmCntXPeriodosDefinicionDb _db;

        public FrmCntXPeriodosDefinicionBL(IConfiguration config)
        {
            _db = new FrmCntXPeriodosDefinicionDb(config);
        }

        public ErrorDto<PeriodosDefinicionDto> Inicial(int codEmpresa, int codContabilidad)
        {
            return _db.Inicial(codEmpresa, codContabilidad);
        }

        public ErrorDto Crear(int codEmpresa, int codContabilidad, PeriodosDefinicionDto dto)
        {

            return _db.Crear(codEmpresa, codContabilidad, dto);
        }
    }
}
