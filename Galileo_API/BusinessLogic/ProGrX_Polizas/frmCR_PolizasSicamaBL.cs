using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizasSicamaBl
    {
        private readonly FrmCrPolizasSicamaDb _db;

        public FrmCrPolizasSicamaBl(IConfiguration config)
        {
            _db = new FrmCrPolizasSicamaDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizasSicama_Polizas_Lista(int CodEmpresa)
        {
           return _db.Cr_PolizasSicama_Polizas_Lista(CodEmpresa);
        }

        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _db.fxFechaServidor(codEmpresa);
        }

        public ErrorDto<List<CrPolizasSicamaEnvioRow>> Cr_PolizasSicama_Envio_Consulta(
           int CodEmpresa,
           string Usuario,
           CrPolizasSicamaEnvioConsultaRequest request)
        {
            return _db.Cr_PolizasSicama_Envio_Consulta(CodEmpresa, Usuario, request);
        }

        public ErrorDto<List<CrPolizasSicamaEnvioRow>> Cr_PolizasSicama_Consulta_Obtener(
            int CodEmpresa,
            string Usuario,
            CrPolizasSicamaEnvioConsultaRequest request)
        {
            return _db.Cr_PolizasSicama_Consulta_Obtener(CodEmpresa, Usuario, request);
        }

        public ErrorDto<List<CrPolizasSicamaBeneficiariosRowDto>>
               Cr_PolizasSicama_Beneficiarios_Lista(
               int CodEmpresa,
               string Usuario,
               string poliza)
        {
            return _db.Cr_PolizasSicama_Beneficiarios_Lista(CodEmpresa, Usuario, poliza);
        }

        /// <summary>
        /// Genera el corte SICAMA para la fecha indicada.
        /// </summary>
        public ErrorDto<bool> Cr_PolizasSicama_Genera(int codEmpresa, DateTime fechaCorte, string usuario)
        {
            return _db.Cr_PolizasSicama_Genera(codEmpresa, fechaCorte, usuario);
        }

        public ErrorDto Cr_FndPlanillaDirecta_Sube(
           int CodEmpresa,
           string Usuario,
           CrFndPlanillaDirectaSubeRequest request)
        {
           return _db.Cr_FndPlanillaDirecta_Sube(CodEmpresa, Usuario, request);
        }

        public ErrorDto<List<CrFndPlanillaDirectaConsultaRowDto>>
            Cr_FndPlanillaDirecta_Consulta(
            int CodEmpresa,
            string Usuario,
            CrFndPlanillaDirectaConsultaRequest request)
        {
            return _db.Cr_FndPlanillaDirecta_Consulta(CodEmpresa, Usuario, request);
        }
    }
}
