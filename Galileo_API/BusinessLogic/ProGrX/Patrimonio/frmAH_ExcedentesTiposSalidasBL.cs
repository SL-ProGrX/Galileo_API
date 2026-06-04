using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhExcedentesTiposSalidasBL
    {
        private readonly FrmAhExcedentesTiposSalidasDB _db;

        public FrmAhExcedentesTiposSalidasBL(IConfiguration config)
        {
            _db = new FrmAhExcedentesTiposSalidasDB(config);
        }

        public ErrorDto<List<FrmAhExcedentesTiposSalidasDto>> Ah_ExcedentesTiposSalidas_Lista(int codEmpresa)
        {
            return _db.Ah_ExcedentesTiposSalidas_Lista(codEmpresa);
        }

        public ErrorDto<List<FrmAhExcedentesTiposSalidasPlanDto>> Ah_ExcedentesTiposSalidas_Planes_Lista(int codEmpresa)
        {
            return _db.Ah_ExcedentesTiposSalidas_Planes_Lista(codEmpresa);
        }

        public ErrorDto<List<FrmAhExcedentesTiposSalidasBancoDto>> Ah_ExcedentesTiposSalidas_Bancos_Lista(int codEmpresa)
        {
            return _db.Ah_ExcedentesTiposSalidas_Bancos_Lista(codEmpresa);
        }

        public ErrorDto<FrmAhExcedentesTiposSalidasGuardarResponse> Ah_ExcedentesTiposSalidas_Insertar(
            int codEmpresa,
            FrmAhExcedentesTiposSalidasGuardarRequest request)
        {
            return _db.Ah_ExcedentesTiposSalidas_Insertar(codEmpresa, request);
        }

        public ErrorDto<FrmAhExcedentesTiposSalidasGuardarResponse> Ah_ExcedentesTiposSalidas_Actualizar(
            int codEmpresa,
            FrmAhExcedentesTiposSalidasGuardarRequest request)
        {
            return _db.Ah_ExcedentesTiposSalidas_Actualizar(codEmpresa, request);
        }

        public ErrorDto<bool> Ah_ExcedentesTiposSalidas_Eliminar(
            int codEmpresa,
            string codSalida,
            string usuario)
        {
            return _db.Ah_ExcedentesTiposSalidas_Eliminar(codEmpresa, codSalida, usuario);
        }
    }
}
