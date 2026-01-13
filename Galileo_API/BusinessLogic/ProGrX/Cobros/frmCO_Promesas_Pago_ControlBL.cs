using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOPromesasPagoControlBL
    {
        private readonly FrmCOPromesasPagoControlDB _db;

        public FrmCOPromesasPagoControlBL(IConfiguration config)
        {
            _db = new FrmCOPromesasPagoControlDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PromesasPago_Usuarios_Obtener(int codEmpresa)
        {
            return _db.PromesasPago_Usuarios_Obtener(codEmpresa);
        }

        public ErrorDto<List<PromesasPagoConsultaResult>> PromesasPago_Consulta(PromesasPagoConsultaParams param)
        {
            return _db.PromesasPago_Consulta(param);
        }
    }
}
