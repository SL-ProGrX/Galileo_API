using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOAplExcInformesBL
    {
        private readonly FrmCOAplExcInformesDB _db;

        public FrmCOAplExcInformesBL(IConfiguration config)
        {
            _db = new FrmCOAplExcInformesDB(config);
        }

        public ErrorDto<List<CoAplExcInformeItemDto>> CO_AplExc_Informes_Catalogo_Obtener(int CodEmpresa)
        {
            return _db.CO_AplExc_Informes_Catalogo_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CoAplExcAplicacionF4Dto>> CO_AplExc_Informes_Aplicaciones_F4_Obtener(int CodEmpresa, string? texto)
        {
            return _db.CO_AplExc_Informes_Aplicaciones_F4_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<List<CoAplExcPersonaF4Dto>> CO_AplExc_Informes_Personas_F4_Obtener(int CodEmpresa, string? texto)
        {
            return _db.CO_AplExc_Informes_Personas_F4_Obtener(CodEmpresa, texto);
        }
    }
}