using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using static Galileo_API.Models.ProGrX_EstudioCrd.FrmPreaConsultasModels;
using Galileo.Models;


namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaConsultasBL
    {
        private readonly FrmPreaConsultasDB _db;

        public FrmPreaConsultasBL(IConfiguration config)
            => _db = new FrmPreaConsultasDB(config);

        public ErrorDto<PreaConsultasCatalogosResponse> PreaConsultas_Catalogos_Obtener(int codEmpresa)
        {
            return _db.PreaConsultas_Catalogos_Obtener(codEmpresa);
        }
        public ErrorDto<ConsultaLista> PreaConsultas_Grid_Obtener(int codEmpresa, bool esExportar, PreaConsultasFiltroRequest request)
        {
            return _db.PreaConsultas_Grid_Obtener(codEmpresa, request, esExportar);
        }
        public ErrorDto<List<PreaConsultasResumenModel>> PreaConsultas_Resumen_Obtener(int codEmpresa, PreaConsultasFiltroRequest request)
        {
            return _db.PreaConsultas_Resumen_Obtener(codEmpresa, request);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Usuarios_Obtener(int codEmpresa)
        {
            return _db.PreaConsultas_Usuarios_Obtener(codEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Lineas_Obtener(int codEmpresa)
        {
            return _db.PreaConsultas_Lineas_Obtener(codEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Destinos_Obtener(int codEmpresa)
        {
            return _db.PreaConsultas_Destinos_Obtener(codEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Instituciones_Obtener(int codEmpresa)
        {
            return _db.PreaConsultas_Instituciones_Obtener(codEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Comites_Obtener(int codEmpresa)
        {
            return _db.PreaConsultas_Comites_Obtener(codEmpresa);
        }
    }
}
