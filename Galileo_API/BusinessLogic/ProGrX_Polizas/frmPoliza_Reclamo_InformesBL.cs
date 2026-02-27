using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizaReclamoInformesBL
    {
        private readonly FrmPolizaReclamoInformesDB _db;

        public FrmPolizaReclamoInformesBL(IConfiguration config)
        {
            _db = new FrmPolizaReclamoInformesDB(config);
        }

        public DateTime fxFechaServidor(int codEmpresa)
        { 
            return _db.fxFechaServidor(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Informes_Polizas_Lista(int CodEmpresa)
        {
            return _db.Poliza_Reclamo_Informes_Polizas_Lista(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Informes_Estados_Lista(int CodEmpresa)
        {
           return _db.Poliza_Reclamo_Informes_Estados_Lista(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Informes_Motivos_Lista(int CodEmpresa, string codPoliza)
        {
            return _db.Poliza_Reclamo_Informes_Motivos_Lista(CodEmpresa, codPoliza);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
        Poliza_Reclamo_Informes_Causas_Lista(int CodEmpresa, string codPoliza)
        {
            return _db.Poliza_Reclamo_Informes_Causas_Lista(CodEmpresa, codPoliza);
        }

        public ErrorDto Poliza_Reclamo_Informes_Preparar_Filtros(
           int CodEmpresa, string usuario, PolizaReclamoInformesPrepararFiltrosRequest request)
        {
            return _db.Poliza_Reclamo_Informes_Preparar_Filtros(CodEmpresa, usuario, request);
        }
    }
}
