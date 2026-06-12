using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhExcedentesCapIndBL
    {
        private readonly FrmAhExcedentesCapIndDB _db;

        public FrmAhExcedentesCapIndBL(IConfiguration config)
        {
            _db = new FrmAhExcedentesCapIndDB(config);
        }

        public ErrorDto<FrmAhExcedentesCapIndCargarResponse> AH_ExcedentesCapInd_Cargar(
            int codEmpresa,
            FrmAhExcedentesCapIndListaRequest? request)
        {
            return _db.AH_ExcedentesCapInd_Cargar(codEmpresa, request);
        }

        public ErrorDto<List<FrmAhExcedentesCapIndListadoDto>> AH_ExcedentesCapInd_Capitalizaciones_Lista(
            int codEmpresa,
            FrmAhExcedentesCapIndListaRequest? request)
        {
            return _db.AH_ExcedentesCapInd_Capitalizaciones_Lista(codEmpresa, request);
        }

        public ErrorDto<FrmAhExcedentesCapIndCedulaDto> AH_ExcedentesCapInd_Cedula_Consultar(
            int codEmpresa,
            string cedula)
        {
            return _db.AH_ExcedentesCapInd_Cedula_Consultar(codEmpresa, cedula);
        }

        public ErrorDto<FrmAhExcedentesCapIndProcesoResponse> AH_ExcedentesCapInd_Guardar(
            int codEmpresa,
            FrmAhExcedentesCapIndGuardarRequest? request)
        {
            return _db.AH_ExcedentesCapInd_Guardar(codEmpresa, request);
        }

        public ErrorDto<FrmAhExcedentesCapIndProcesoResponse> AH_ExcedentesCapInd_Eliminar(
            int codEmpresa,
            int excCapInd,
            string usuario)
        {
            return _db.AH_ExcedentesCapInd_Eliminar(codEmpresa, excCapInd, usuario);
        }
    }
}
