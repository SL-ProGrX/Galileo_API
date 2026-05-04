using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCCActualizaUpUtBl
    {
        private readonly FrmCCActualizaUpUtDb _db;
    
        public FrmCCActualizaUpUtBl(IConfiguration config)
        {
            _db = new FrmCCActualizaUpUtDb(config);
        }

        public async Task<ErrorDto> CC_ActualizaUpUt_ProcesarArchivo(
            int codEmpresa,
            string usuario,
            IFormFile file)
        {
            return await _db.CC_ActualizaUpUt_ProcesarArchivo(codEmpresa, usuario, file);
        }
    }
}
