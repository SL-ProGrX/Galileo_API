using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmFndConMovCobroBl
    {
        private readonly FrmFndConMovCobroDb _db;

        public FrmFndConMovCobroBl(IConfiguration config)
        {
            _db = new FrmFndConMovCobroDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Planes_Lista_Obtener(int CodEmpresa)
        {
            return _db.Fnd_Planes_Lista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FndConMovCobroResult>> Fnd_ConMovCobro_Obtener(int CodEmpresa, FndConMovCobroRequest request)
        {
            return _db.Fnd_ConMovCobro_Obtener(CodEmpresa, request);
        }

        public ErrorDto<List<FndConMovCobroResult>> Fnd_ConMovCobro_SinContrato_Obtener(int CodEmpresa, FndConMovCobroRequest request)
        {
            return _db.Fnd_ConMovCobro_SinContrato_Obtener(CodEmpresa, request);
        }

        public ErrorDto<bool> Fnd_AcreditaMovCbrPendiente(int CodEmpresa, FndAcreditaMovCbrPendienteRequest request)
        {
            return _db.Fnd_AcreditaMovCbrPendiente(CodEmpresa, request);
        }

        public ErrorDto<FndConMovCobroResumenResult?> Fnd_ConMovCobro_EntradaPlanilla_Obtener(int CodEmpresa, FndConMovCobroResumenRequest request)
        {
            return _db.Fnd_ConMovCobro_EntradaPlanilla_Obtener(CodEmpresa, request);
        }

        public ErrorDto<FndConMovCobroResumenResult?> Fnd_ConMovCobro_PlanillaRegistrada_Obtener(int CodEmpresa, FndConMovCobroResumenRequest request)
        {
            return _db.Fnd_ConMovCobro_PlanillaRegistrada_Obtener(CodEmpresa, request);
        }
    }
}