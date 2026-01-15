using Galileo.Models;
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

        public ErrorDto<CajasCrdAbonosStPDData> CajasCrdAbonosSt_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return _db.CajasCrdAbonosSt_ConsultaOperacion_Obtener(CodEmpresa, CodCaja, OperacionId);
        }

        public ErrorDto<List<CajasCrdAbonoMorosidadData>> CajasCrdAbonosSt_MoraConsulta(int CodEmpresa, int Operacion, DateTime FechaPago)
        {
            return _db.CajasCrdAbonosSt_MoraConsulta(CodEmpresa, Operacion, FechaPago);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CajasCrdAbonosSt_Documentos_Obtener(int CodEmpresa, string codCaja)
        {
            return _db.CajasCrdAbonosSt_Documentos_Obtener(CodEmpresa, codCaja);
        }

        public ErrorDto<List<CajasCrdAbonosStPDData>> CajasCrdAbonosSt_Operaciones_Obtener(int CodEmpresa)
        {
            return _db.CajasCrdAbonosSt_Operaciones_Obtener(CodEmpresa);
        }

        public ErrorDto<CajasCrdAbonoCargaOperacionData> CajasCrdAbonosSt_CargaOperacionCodCed(int CodEmpresa, string cedula, string codigo)
        {
            return _db.CajasCrdAbonosSt_CargaOperacionCodCed(CodEmpresa, cedula, codigo);
        }
    }
}
