using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhExcedentesParametrosBL
    {
        private readonly FrmAhExcedentesParametrosDB _db;

        public FrmAhExcedentesParametrosBL(IConfiguration config)
        {
            _db = new FrmAhExcedentesParametrosDB(config);
        }

        public ErrorDto<List<FrmAhExcedentesParametroDto>> Ah_ExcedentesParametros_Lista(int codEmpresa)
        {
            return _db.Ah_ExcedentesParametros_Lista(codEmpresa);
        }

        public ErrorDto<bool> Ah_ExcedentesParametros_Actualizar(
            int codEmpresa,
            FrmAhExcedentesParametroActualizarRequest request)
        {
            return _db.Ah_ExcedentesParametros_Actualizar(codEmpresa, request);
        }
    }
}
