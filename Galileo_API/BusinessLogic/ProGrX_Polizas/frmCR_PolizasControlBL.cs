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

        public ErrorDto<List<CrPolizasControlCierreRowDto>> Cr_PolizasControl_Cierres_Lista(
           int CodEmpresa,
           string cod_poliza,
           string tipos)
        {
            return _DB.Cr_PolizasControl_Cierres_Lista(CodEmpresa, cod_poliza, tipos);
        }

        public ErrorDto Cr_PolizasControl_Nuevo(int CodEmpresa, CrPolizasControlNuevoRequestDto request)
        {
            return _DB.Cr_PolizasControl_Nuevo(CodEmpresa, request);
        }

        public ErrorDto Cr_PolizasControl_Actualizar(int CodEmpresa)
        {
            return _DB.Cr_PolizasControl_Actualizar(CodEmpresa);
        }

        public ErrorDto Cr_PolizasControl_Cierre_Eliminar(
            int CodEmpresa,
            string cod_poliza,
            int cod_corte,
            string Tipo,
            string usuario)
        {
            return _DB.Cr_PolizasControl_Cierre_Eliminar(CodEmpresa, cod_poliza, cod_corte, Tipo, usuario);
        }
    }
}
