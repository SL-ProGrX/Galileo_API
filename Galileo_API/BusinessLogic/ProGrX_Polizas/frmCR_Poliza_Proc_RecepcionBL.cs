using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using static Galileo_API.Models.ProGrX_Polizas.FrmCrPolizaProcRecepcionModels;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizaProcRecepcionBL
    {

        private readonly FrmCrPolizaProcRecepcionDB _db;

        public FrmCrPolizaProcRecepcionBL(IConfiguration config)
        {
            _db = new FrmCrPolizaProcRecepcionDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PolizaProcRecepcion_Listar(int codEmpresa)
          => _db.PolizaProcRecepcion_Listar(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> PolizaUnidades_Listar(int codEmpresa, int codContabilidad)
        => _db.PolizaUnidades_Listar(codEmpresa, codContabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> PolizaFacturables_Listar(int codEmpresa)
         => _db.PolizaFacturables_Listar(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> PolizaCentrosCosto_Listar(int codEmpresa, int codContabilidad, string codUnidad)
        => _db.PolizaCentrosCosto_Listar(codEmpresa, codContabilidad, codUnidad);

        public ErrorDto<List<DropDownListaGenericaModel>> PolizaDivisas_Listar(int codEmpresa, int codContabilidad)
        => _db.PolizaDivisas_Listar(codEmpresa, codContabilidad);
        public ErrorDto<DropDownListaGenericaModel> PolizaDivisasLocal_Consulta(int codEmpresa, int codContabilidad)
        => _db.PolizaDivisasLocal_Consulta(codEmpresa, codContabilidad);

        public ErrorDto<PolizaAseguradoraCorte> PolizaAseguradoraCorte_Valida(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
        => _db.PolizaAseguradoraCorte_Valida(codEmpresa, corte, codPoliza, idFactura);

        public ErrorDto<PolizaAseguradoraCorte> PolizaAseguradoraCorte_Agregar(int codEmpresa, string usuario, PolizaAseguradoraCorteData datos)
        => _db.PolizaAseguradoraCorte_Agregar(codEmpresa, usuario, datos);

        public ErrorDto<int> PolizaAseguradoraCorteDetalle_Agregar(int codEmpresa, string usuario, int scFacturaId, IEnumerable<PolizaAseguradoraCorteDetalleData> datos)
        => _db.PolizaAseguradoraCorteDetalle_Agregar(codEmpresa, usuario, scFacturaId, datos);

        public ErrorDto<PolizaAseguradoraCorte> PolizaAseguradoraCorte_Pago(int codEmpresa, string usuario, DateTime corte, string codPoliza, int idFactura)
        => _db.PolizaAseguradoraCorte_Pago(codEmpresa, usuario, corte, codPoliza, idFactura);

        public ErrorDto<decimal> TipoCambio_Consultar(int codEmpresa, int contabilidad, string divisa)
        => _db.TipoCambio_Consultar(codEmpresa, contabilidad, divisa);

        public ErrorDto<PolizaDatos> PolizaPolizaDatos(int codEmpresa, string codPoliza)
        => _db.PolizaPolizaDatos(codEmpresa, codPoliza);

        public ErrorDto<List<PolizaAseguradoraCorteDetalleConsulta>> PolizaAseguradoraCorteDetalle_Consulta(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
        => _db.PolizaAseguradoraCorteDetalle_Consulta(codEmpresa, corte, codPoliza, idFactura);
        public ErrorDto<PolizaDatos> PolizaAseguradoraCorte_Consulta(int codEmpresa, DateTime corte, string codPoliza)
        => _db.PolizaAseguradoraCorte_Consulta(codEmpresa, corte, codPoliza); 

    }
}
