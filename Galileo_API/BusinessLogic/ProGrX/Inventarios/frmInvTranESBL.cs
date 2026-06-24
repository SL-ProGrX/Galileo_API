using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTranEsBL
    {
        private readonly FrmInvTranEsDB _db;

        public FrmInvTranEsBL(IConfiguration config)
        {
            _db = new FrmInvTranEsDB(config);
        }

        public ErrorDto<TranESData> InvTranES_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            return _db.InvTranES_Obtener(CodEmpresa, CodBoleta, TipoTran);
        }

        public ErrorDto<List<InvProducLineas>> InvProducLineas_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            return _db.InvProducLineas_Obtener(CodEmpresa, CodBoleta, TipoTran);
        }

        public ErrorDto<TranESData> InvTranES_scroll(int CodEmpresa, int scrollValue, string? CodBoleta, string TipoTran)
        {
            return _db.InvTranES_scroll(CodEmpresa, scrollValue, CodBoleta, TipoTran);
        }

        public ErrorDto InvTranES_Insertar(int CodEmpresa, string TipoTran, TranESData request)
        {
            return _db.InvTranES_Insertar(CodEmpresa, TipoTran, request);
        }

        public ErrorDto InvTranES_Actualizar(int CodEmpresa, TranESUpdate request)
        {
            return _db.InvTranES_Actualizar(CodEmpresa, request);
        }

        public ErrorDto InvTranES_Eliminar(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            return _db.InvTranES_Eliminar(CodEmpresa, CodBoleta, TipoTran);
        }

        public ErrorDto InvProducLineas_Insertar(int CodEmpresa, string CodBoleta, string TipoTran, List<InvProducLineasInsert> request)
        {
            return _db.InvProducLineas_Insertar(CodEmpresa, CodBoleta, TipoTran, request);
        }

        public ErrorDto<List<InvTranPlantilla>> InvTranPlantilla_Obtener(int CodEmpresa, string TipoTran, string? CodBoleta, string? GeneraUser, string? GeneraFecha)
        {
            return _db.InvTranPlantilla_Obtener(CodEmpresa, TipoTran, CodBoleta, GeneraUser, GeneraFecha);
        }

        public ErrorDto InvProducLineas_Eliminar(int CodEmpresa, string CodBoleta, string TipoTran, int Linea)
        {
            return _db.InvProducLineas_Eliminar(CodEmpresa, CodBoleta, TipoTran, Linea);
        }
    }
}