using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizasBalanzaPagosBL
    {
        private readonly FrmPolizasBalanzaPagosDB _db;

        public FrmPolizasBalanzaPagosBL(IConfiguration config)
        {
            _db = new FrmPolizasBalanzaPagosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Polizas_Combo_Lista(int codEmpresa)
            => _db.Polizas_Combo_Lista(codEmpresa);

        public ErrorDto<List<PolizaBalancePagoResumenDto>> Poliza_Informe_Balance_Pago_Resumen(int codEmpresa, PolizaBalancePagoParams param)
            => _db.Poliza_Informe_Balance_Pago<PolizaBalancePagoResumenDto>(codEmpresa, param);

        public ErrorDto<List<PolizaBalancePagoDetalleDto>> Poliza_Informe_Balance_Pago_Detalle(int codEmpresa, PolizaBalancePagoParams param)
            => _db.Poliza_Informe_Balance_Pago <PolizaBalancePagoDetalleDto>(codEmpresa, param);
    }
}
