 
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo; 

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSugefInformesArchivosBL(IConfiguration config)
    {
        private readonly FrmSugefInformesArchivosDB _db = new FrmSugefInformesArchivosDB(config);

        public ErrorDto<List<SugefInformesArchivosData>> SUGEFInformesArchivos_Cortes_Obtener(int CodEmpresa)
        {
            return _db.SUGEFInformesArchivos_Cortes_Obtener(CodEmpresa);
        }

        public ErrorDto<List<SugefFacilidadesCrediticiasData>> SUGEFInformesArchivos_Obtener(int CodEmpresa, DateTime Corte)
        {
            return _db.SUGEFInformesArchivos_Obtener(CodEmpresa, Corte);
        }

        public ErrorDto SUGEFInformesArchivos_Corte_Procesar(int CodEmpresa, string Usuario, DateTime Corte, string Descripcion, DateTime RngInicio, DateTime RngCorte)
        {
            return _db.SUGEFInformesArchivos_Corte_Procesar(CodEmpresa, Usuario, Corte, Descripcion, RngInicio, RngCorte);
        }

        public ErrorDto SUGEFInformesArchivos_Archivo(int CodEmpresa, string Usuario, DateTime Corte)
        {
            return _db.SUGEFInformesArchivos_Archivo(CodEmpresa, Usuario, Corte);
        }
    }
}