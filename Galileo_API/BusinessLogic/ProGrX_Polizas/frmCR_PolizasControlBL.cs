using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizasControlBL
    {
            private readonly FrmCrPolizasControlDB _DB;
    
            public FrmCrPolizasControlBL(IConfiguration config)
            {
                _DB = new FrmCrPolizasControlDB(config);
            }

        public ErrorDto<PolizaLookupResponseDto> Cr_PolizasControl_Obtener(int CodEmpresa, string CodPoliza)
        {
           return _DB.Cr_PolizasControl_Obtener(CodEmpresa, CodPoliza);
        }

        public ErrorDto<PolizaLookupResponseDto?> Cr_PolizasControl_Scroll(
                int codEmpresa,
                string codPolizaActual,
                int direccion)
        {
            return _DB.Cr_PolizasControl_Scroll(codEmpresa, codPolizaActual, direccion);
        }
    }
}
