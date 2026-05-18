using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Personas;
using Galileo_API.Models.ProGrX_Personas;

namespace Galileo_API.BusinessLogic.ProGrX_Personas
{
    public class FrmAfZonasBL
    {
        private readonly FrmAfZonasDB _db;

        public FrmAfZonasBL(IConfiguration config)
            => _db = new FrmAfZonasDB(config);

        public ErrorDto<ZonasLista> AF_ZonasLista_Obtener(string filtro, string sortField, int sortOrder, int pagina, int paginacion)
        {
            return _db.AF_ZonasLista_Obtener(filtro, sortField, sortOrder, pagina, paginacion);
        }

        public ErrorDto<List<ZonasData>> AF_Zonas_Obtener(int codEmpresa, FiltrosLazyLoadData filtros)
        {
            return _db.AF_Zonas_Obtener(codEmpresa, filtros);
        }

        public ErrorDto AF_Zonas_Guardar(int codEmpresa, string usuario, ZonasData zona)
        {
            return _db.AF_Zonas_Guardar(codEmpresa, usuario, zona);
        }

        public ErrorDto AF_Zonas_Eliminar(int codEmpresa, string usuario, string codZona)
        {
            return _db.AF_Zonas_Eliminar(codEmpresa, usuario, codZona);
        }

        public ErrorDto<int> AF_Zonas_Valida(int codEmpresa, string codZona)
        {
            return _db.AF_Zonas_Valida(codEmpresa, codZona);
        }

        public ErrorDto<List<ZonaUsuarioAsignadoData>> AF_Zonas_UsuariosAsignados_Obtener(int codEmpresa, string codZona)
        {
            return _db.AF_Zonas_UsuariosAsignados_Obtener(codEmpresa, codZona);
        }

        public ErrorDto<List<ZonaInstitucionAsignadaData>> AF_Zonas_InstitucionesAsignadas_Obtener(int codEmpresa, string codZona)
        {
            return _db.AF_Zonas_InstitucionesAsignadas_Obtener(codEmpresa, codZona);
        }

        public ErrorDto AF_Zonas_InstitucionAsignar_Registrar(int codEmpresa, string codZona, string codInstitucion, bool asignar, string usuario)
        {
            return _db.AF_Zonas_InstitucionAsignar_Registrar(codEmpresa, codZona, codInstitucion, asignar, usuario);
        }

        public ErrorDto AF_Zonas_UsuarioAsignar_Registrar(int codEmpresa, string codZona, string codUsuario, bool asignar, string usuario)
        {
            return _db.AF_Zonas_UsuarioAsignar_Registrar(codEmpresa, codZona, codUsuario, asignar, usuario);
        }
    }
}
