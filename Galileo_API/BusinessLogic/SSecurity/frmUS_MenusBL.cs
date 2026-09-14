using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;


namespace Galileo.BusinessLogic
{
    public class FrmUsMenusBl
    {

        readonly FrmUsMenusDb SecurityUsDB;

        public FrmUsMenusBl(IConfiguration config)
        {
            SecurityUsDB = new FrmUsMenusDb(config);
        }

        public ErrorDto<List<UsMenuDto>> ObtenerUsMenusPorTipoYNodoPadreEsNull(string Tipo)
            => SecurityUsDB.ObtenerUsMenusPorTipoYNodoPadreEsNull(Tipo);

        public ErrorDto<List<UsMenuDto>> ObtenerUsMenus()
            => SecurityUsDB.ObtenerUsMenus();

        public ErrorDto<List<UsModuloDto>> ObtenerUsModulos()
            => SecurityUsDB.ObtenerUsModulos();

        public ErrorDto<List<UsFormularioDto>> ObtenerUsFormularios()
            => SecurityUsDB.ObtenerUsFormularios();

        public ErrorDto<int?> ObtenerMenuNodoPorNodoPadreYPrioridad(int NodoPadre, int Prioridad)
            => SecurityUsDB.ObtenerMenuNodoPorNodoPadreYPrioridad(NodoPadre, Prioridad);

        public ErrorDto<ResultadoCrearYEditarUsMenuDto?> ActualizarUsMenu(UsMenuDto info)
            => SecurityUsDB.ActualizarUsMenu(info);

        public ErrorDto<int?> ObtenerMenuNodoConIsNull()
            => SecurityUsDB.ObtenerMenuNodoConIsNull();

        public ErrorDto<int?> ObtenerMenuPrioridadPorMenuNodoPadre(int NodoPadre)
            => SecurityUsDB.ObtenerMenuPrioridadPorMenuNodoPadre(NodoPadre);

        public ErrorDto<UsModuloDto?> ObtenerUsModulosOrdenadosPorTipo(string Tipo)
            => SecurityUsDB.ObtenerUsModulosOrdenadosPorTipo(Tipo);

        public ErrorDto<ResultadoCrearYEditarUsMenuDto?> CrearUsMenu(UsMenuDto info)
            => SecurityUsDB.CrearUsMenu(info);

        public ErrorDto<int?> EliminarUnMenuPorNodoPadre(int NodoPadre)
            => SecurityUsDB.EliminarUnMenuPorNodoPadre(NodoPadre);

        public ErrorDto<int?> EliminarUsMenusPorMenuNodo(int MenuNodo)
            => SecurityUsDB.EliminarUsMenusPorMenuNodo(MenuNodo);

        public ErrorDto<int?> EliminarTodosLosMenusPorNodoPadre(int NodoPadre)
            => SecurityUsDB.EliminarTodosLosMenusPorNodoPadre(NodoPadre);

        public ErrorDto<int?> ObtenerMenuNodoPorMenuFormulario(string Formulario)
            => SecurityUsDB.ObtenerMenuNodoPorMenuFormulario(Formulario);

        public ErrorDto<UsFormularioDto> ObtenerUsFormularioPorFormulario(string Formulario)
            => SecurityUsDB.ObtenerUsFormularioPorFormulario(Formulario);

        public ErrorDto<UsMenuDto> ObtenerUsMenuPorMenuNodo(int MenuNodo)
            => SecurityUsDB.ObtenerUsMenuPorMenuNodo(MenuNodo);

        public ErrorDto<List<UsIconWeb>> ObtenerUsMenu_IconosWeb()
            => SecurityUsDB.ObtenerUsMenu_IconosWeb();

    }
}
