using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOAplExcContratosInformesBL
    {
        private readonly FrmCOAplExcContratosInformesDB _db;

        public FrmCOAplExcContratosInformesBL(IConfiguration config)
        {
            _db = new FrmCOAplExcContratosInformesDB(config);
        }

        public ErrorDto<List<CoAplExcContratosInformeItemDto>> CO_AplExc_Contratos_Informes_Catalogo_Obtener(int CodEmpresa)
        {
            return _db.CO_AplExc_Contratos_Informes_Catalogo_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CoAplExcContratosAplicacionF4Dto>> CO_AplExc_Contratos_Informes_Aplicaciones_F4_Obtener(int CodEmpresa, string? texto)
        {
            return _db.CO_AplExc_Contratos_Informes_Aplicaciones_F4_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<List<CoAplExcContratosPersonaF4Dto>> CO_AplExc_Contratos_Informes_Personas_F4_Obtener(int CodEmpresa, string? texto)
        {
            return _db.CO_AplExc_Contratos_Informes_Personas_F4_Obtener(CodEmpresa, texto);
        }
    }
}