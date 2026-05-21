using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFNDContratosInformesBL
    {
        private readonly FrmFNDContratosInformesDB _db;

        public FrmFNDContratosInformesBL(IConfiguration? config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _db = new FrmFNDContratosInformesDB(config);
        }

        public ErrorDto<FndContratosInformesContrato> Fnd_ContratosInformes_Contrato_Obtener(
            int CodEmpresa,
            int operadora,
            string plan,
            int contrato,
            string usuario)
        {
            return _db.Fnd_ContratosInformes_Contrato_Obtener(CodEmpresa, operadora, plan, contrato, usuario);
        }

        public ErrorDto<string> Fnd_ContratosInformes_Email_Enviar(
            int CodEmpresa,
            int operadora,
            string plan,
            int contrato,
            string usuario)
        {
            return _db.Fnd_ContratosInformes_Email_Enviar(CodEmpresa, operadora, plan, contrato, usuario);
        }

        public ErrorDto<FndContratosInformesLiquidacionesLista> Fnd_ContratosInformes_Retiros_Obtener(
            int CodEmpresa,
            int operadora,
            string plan,
            int contrato,
            string strFiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(strFiltros) ?? new FiltrosLazyLoadData();
            return _db.Fnd_ContratosInformes_Retiros_Obtener(CodEmpresa, operadora, plan, contrato, filtros);
        }
    }
}
