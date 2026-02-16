using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizaAsociadosBL
    {
        private readonly FrmPolizaAsociadosDB _db;

        public FrmPolizaAsociadosBL(IConfiguration config)
        {
            _db = new FrmPolizaAsociadosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_AsociadoCatalogo_Listar(int CodEmpresa, string tipo = "PA")
        {
            return _db.Poliza_AsociadoCatalogo_Listar(CodEmpresa, tipo);
        }

        public ErrorDto<List<PolizaAsociadoCorteSpDto>> Poliza_Asociados_Corte_Listar(
        int CodEmpresa,
        string Usuario,
        DateTime FechaCorte,
        string? Tipo)
        {
            return _db.Poliza_Asociados_Corte_Listar(CodEmpresa, Usuario, FechaCorte, Tipo);
        }

        public ErrorDto<List<PolizaBeneficiariosSpDto>> Poliza_Beneficiarios_Listar(
          int CodEmpresa,
          string CodPoliza)
        {
            return _db.Poliza_Beneficiarios_Listar(CodEmpresa, CodPoliza);
        }
    }

}
