using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesTransferenciaReversaBL
    {
        private readonly FrmTesTransferenciaReversaDB _TransferenciaReversaDB;

        public FrmTesTransferenciaReversaBL(IConfiguration config)
        {
            _TransferenciaReversaDB = new FrmTesTransferenciaReversaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGestion(int CodEmpresa, string usuario, string gestion)
        {
            return _TransferenciaReversaDB.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, gestion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_ReversaPlanes_Obtener(int CodEmpresa, string id_banco)
        {
            return _TransferenciaReversaDB.TES_ReversaPlanes_Obtener(CodEmpresa, id_banco);
        }

        public ErrorDto<long> sbNTrasnferencia(int CodEmpresa, int id_banco, string tipo, string avance, string plan)
        {
            return _TransferenciaReversaDB.sbNTrasnferencia(CodEmpresa, id_banco, tipo, avance, plan);
        }

        public ErrorDto<List<TransferenciaSolicitudData>> TES_TransferenciaReversa_Obtener(int CodEmpresa, string solicitud)
        {
            var solicitaData = JsonConvert.DeserializeObject<TransferenciaSolicitudData>(solicitud) ?? new TransferenciaSolicitudData();
            return _TransferenciaReversaDB.TES_TransferenciaReversa_Obtener(CodEmpresa, solicitaData);
        }

        public ErrorDto TES_TransferenciaReversa_Aplicar(int CodEmpresa, TransferenciaReversaAplicaModel transferencia)
        {
            return _TransferenciaReversaDB.TES_TransferenciaReversa_Aplicar(CodEmpresa, transferencia);
        }

        public ErrorDto<List<TesReversionData>> TES_TransferenciaConsulta_Obtener(
            int CodEmpresa,
            int id_banco,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            return _TransferenciaReversaDB.TES_TransferenciaConsulta_Obtener(CodEmpresa, id_banco, fechaInicio, fechaFin);
        }

        public ErrorDto<List<TransferenciaDetalleModel>> TES_TransferenciaReversa_Detalle(int CodEmpresa, string id_reversion)
        {
            return _TransferenciaReversaDB.TES_TransferenciaReversa_Detalle(CodEmpresa, id_reversion);
        }

        public ErrorDto<List<TransferenciaSolicitudData>> TES_TransferenciaRevSinpe_Obtener(string reversa)
        {
            var solicitaData = JsonConvert.DeserializeObject<TesReversaSinpeRequest>(reversa) ?? new TesReversaSinpeRequest();
            return _TransferenciaReversaDB.TES_TransferenciaRevSinpe_Obtener(solicitaData);
        }

        public ErrorDto TES_TransferenciaRevSinpe_Aplicar(TesReversaSinpeModel reversa)
        {
            return _TransferenciaReversaDB.TES_TransferenciaRevSinpe_Aplicar(reversa);
        }

    }
}
