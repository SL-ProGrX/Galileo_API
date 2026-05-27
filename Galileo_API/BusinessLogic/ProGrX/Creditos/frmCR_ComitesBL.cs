using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrComitesBL
    {
        private readonly FrmCrComitesDB _db;

        public FrmCrComitesBL(IConfiguration configuration)
        {
            _db = new FrmCrComitesDB(configuration);
        }

        public ErrorDto<CrComitesLista> CR_Comites_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _db.CR_Comites_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrComitesLista> CR_Comites_Lista_Export(int CodEmpresa, string parametros)
        {
            return _db.CR_Comites_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<CrComitesGuardarResult> CR_Comites_Guardar(int CodEmpresa, CrComitesGuardarRequest request, string usuario)
        {
            return _db.CR_Comites_Guardar(CodEmpresa, request, usuario);
        }

        public ErrorDto CR_Comites_Eliminar(int CodEmpresa, int id_comite, string usuario)
        {
            return _db.CR_Comites_Eliminar(CodEmpresa, id_comite, usuario);
        }

        public ErrorDto<List<CrComitesNivelAprobacionDto>> CR_Comites_NivelAprobacion_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CR_Comites_NivelAprobacion_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<CrComitesGarantiasLista> CR_Comites_Garantias_Lista_Obtener(int CodEmpresa, int id_comite, string usuario)
        {
            return _db.CR_Comites_Garantias_Lista_Obtener(CodEmpresa, id_comite, usuario);
        }

        public ErrorDto<CrComitesGarantiasLista> CR_Comites_Garantias_Lista_Export(int CodEmpresa, int id_comite, string usuario)
        {
            return _db.CR_Comites_Garantias_Lista_Export(CodEmpresa, id_comite, usuario);
        }

        public ErrorDto CR_Comites_Garantias_Guardar(int CodEmpresa, CrComitesGarantiasGuardarRequest request, string usuario)
        {
            return _db.CR_Comites_Garantias_Guardar(CodEmpresa, request, usuario);
        }

        public ErrorDto<CrComitesLineasLista> CR_Comites_Lineas_Lista_Obtener(int CodEmpresa, int id_comite)
        {
            return _db.CR_Comites_Lineas_Lista_Obtener(CodEmpresa, id_comite);
        }

        public ErrorDto CR_Comites_Lineas_Asignar(int CodEmpresa, CrComitesLineasAsignarRequest request, string usuario)
        {
            return _db.CR_Comites_Lineas_Asignar(CodEmpresa, request, usuario);
        }
    }
}