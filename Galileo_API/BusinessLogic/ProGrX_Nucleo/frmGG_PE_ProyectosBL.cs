using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GG_PE;

namespace Galileo.BusinessLogic
{
    public class FrmGgPeProyectosBL (IConfiguration config)
    {
        private readonly FrmGgPeProyectosDB _db = new(config);

        public ErrorDto<PeProyectosLista> PeProyectoLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            return _db.PeProyectoLista_Obtener(CodEmpresa, Jfiltros);
        }

        public ErrorDto PeProyecto_Guardar(int CodEmpresa, PeProyectosDto proyectos)
        {
            return _db.PeProyecto_Guardar(CodEmpresa, proyectos);
        }

        public ErrorDto PeProyecto_Eliminar(int CodEmpresa, int proyecto_id)
        {
            return _db.PeProyecto_Eliminar(CodEmpresa, proyecto_id);
        }

        public ErrorDto<List<PeProyectoObjetivosLista>> PeObservacionesProyectos_Obtener(int CodEmpresa, int proyecto_id)
        {
            return _db.PeObservacionesProyectos_Obtener(CodEmpresa, proyecto_id);
        }

        public ErrorDto PeObjetivoProyecto_Asociar(int CodEmpresa, int proyecto_id, int objetivo_id, string usuario)
        {
            return _db.PeObjetivoProyecto_Asociar(CodEmpresa, proyecto_id, objetivo_id, usuario);
        }

        public ErrorDto<List<PeProyectoObjetivosExportar>> PeProyectoObj_Exportar(int CodEmpresa)
        {
            return _db.PeProyectoObj_Exportar(CodEmpresa);
        }

    }
}