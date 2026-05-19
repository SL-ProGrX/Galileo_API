using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCatalogoGruposBl
    {
        private readonly FrmCrCatalogoGruposDb _db;

        public FrmCrCatalogoGruposBl(IConfiguration config)
        {
            _db = new FrmCrCatalogoGruposDb(config);
        }

        public ErrorDto<List<CrCatalogoGrupoData>> CrCatalogoGrupos_Obtener(
            int codEmpresa,
            bool? activos)
        {
            return _db.CrCatalogoGrupos_Obtener(codEmpresa, activos);
        }

        public ErrorDto<List<CrCatalogoGrupoConsultaData>> CrCatalogoGrupos_Consulta_Calcular(
            int codEmpresa,
            CrCatalogoGrupoConsultaRequest request)
        {
            return _db.CrCatalogoGrupos_Consulta_Calcular(codEmpresa, request);
        }

        public ErrorDto CrCatalogoGrupos_Guardar(
            int codEmpresa,
            string usuario,
            CrCatalogoGrupoData request)
        {
            return _db.CrCatalogoGrupos_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto<List<CrCatalogoGrupoAsignacionCatalogoData>> CrCatalogoGrupos_AsignacionCatalogos_Obtener(
            int codEmpresa,
            string codGrupo)
        {
            return _db.CrCatalogoGrupos_AsignacionCatalogos_Obtener(codEmpresa, codGrupo);
        }

        public ErrorDto CrCatalogoGrupos_Asignacion_Guardar(
            int codEmpresa,
            CrCatalogoGrupoAsignacionGuardarRequest request)
        {
            return _db.CrCatalogoGrupos_Asignacion_Guardar(codEmpresa, request);
        }

        public ErrorDto<List<CrCatalogoGrupoDiarioData>> CrCatalogoGrupos_Diario_Obtener(
            int codEmpresa,
            string codGrupo)
        {
            return _db.CrCatalogoGrupos_Diario_Obtener(codEmpresa, codGrupo);
        }

        public ErrorDto CrCatalogoGrupos_Diario_Guardar(
            int codEmpresa,
            CrCatalogoGrupoDiarioGuardarRequest request)
        {
            return _db.CrCatalogoGrupos_Diario_Guardar(codEmpresa, request);
        }

        public ErrorDto CrCatalogoGrupos_Eliminar(
            int codEmpresa,
            string usuario,
            string codGrupo)
        {
            return _db.CrCatalogoGrupos_Eliminar(codEmpresa, usuario, codGrupo);
        }
    }
}