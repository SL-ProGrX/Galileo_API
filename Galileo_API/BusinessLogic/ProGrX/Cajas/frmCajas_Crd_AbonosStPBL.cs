using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasCrdAbonosStpBL
    {
        private readonly FrmCajasCrdAbonosStpDB _db;

        public FrmCajasCrdAbonosStpBL(IConfiguration config)
        {
            _db = new FrmCajasCrdAbonosStpDB(config);
        }

        public ErrorDto<int> CajasCrdAbonosSt_fxCrdParametro(int CodEmpresa, string parametro)
        {
            return _db.CajasCrdAbonosSt_fxCrdParametro(CodEmpresa, parametro);
        }

        public ErrorDto<List<CajasCrdAbonosStPDData>> CajasCrdAbonosSt_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return _db.CajasCrdAbonosSt_ConsultaOperacion_Obtener(CodEmpresa, CodCaja, OperacionId);
        }

        public ErrorDto<List<CajasCrdAbonoMorosidadData>> CajasCrdAbonosSt_MoraConsulta(int CodEmpresa, int Operacion, DateTime FechaPago)
        {
            return _db.CajasCrdAbonosSt_MoraConsulta(CodEmpresa, Operacion, FechaPago);
        }
    }
}
