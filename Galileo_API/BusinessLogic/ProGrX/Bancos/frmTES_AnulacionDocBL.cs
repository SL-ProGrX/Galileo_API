using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesAnulacionDocBl
    {

        private readonly FrmTesAnulacionDocDb _AnulacionDocDb;

        public FrmTesAnulacionDocBl(IConfiguration config)
        {
            _AnulacionDocDb = new FrmTesAnulacionDocDb(config);
        }

        public ErrorDto<TesAnulacionDocData> TES_Anulacion_Obtener(int CodEmpresa, int solicitud, string usuario)
        {
            return _AnulacionDocDb.TES_Anulacion_Obtener(CodEmpresa, solicitud, usuario);
        }

        public ErrorDto TES_Anulacion_Anular(int CodEmpresa, string usuario, TesAnulacionAnulaModel anula)
        {
            return _AnulacionDocDb.TES_Anulacion_Anular(CodEmpresa, usuario, anula);
        }

        public ErrorDto TES_AnulacionCopiaSolicitud(int CodEmpresa, string usuario, TesAnulacionAnulaModel anula)
        {
            return _AnulacionDocDb.TES_AnulacionCopiaSolicitud(CodEmpresa, usuario, anula);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_AnulacionConceptos_Obtener(int CodEmpresa, string tipo)
        {
            return _AnulacionDocDb.TES_AnulacionConceptos_Obtener(CodEmpresa, tipo);
        }

    }
}
