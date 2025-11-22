using Galileo.DataBaseTier;
using Galileo.Models;
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

        public List<UsMenuDto> ObtenerUsMenusPorTipoYNodoPadreEsNull(string Tipo)
        {
            List<UsMenuDto> resultado = new List<UsMenuDto>();
            try
            {
                var lista = SecurityUsDB.ObtenerUsMenusPorTipoYNodoPadreEsNull(Tipo);

                foreach (var item in lista)
                {
                    resultado.Add(new UsMenuDto
                    {
                        MENU_NODO = item.MENU_NODO,
                        NODO_PADRE = item.NODO_PADRE,
                        NODO_DESCRIPCION = item.NODO_DESCRIPCION,
                        TIPO = item.TIPO,
                        ICONO = item.ICONO,
                        MODO = item.MODO,
                        MODAL = item.MODAL,
                        ACCESOS_DLL_ID = item.ACCESOS_DLL_ID,
                        ACCESOS_DLL_CLS = item.ACCESOS_DLL_CLS,
                        PRIORIDAD = item.PRIORIDAD,
                        FORMULARIO = item.FORMULARIO,
                        MODULO = item.MODULO,
                        Acceso = item.Acceso

                    });
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return resultado;

        }//end ObtenerUsMenusPorTipoYNodoPadreEsNull

        public List<UsMenuDto> ObtenerUsMenus()
        {
            return SecurityUsDB.ObtenerUsMenus();

        }//end ObtenerUsModulos

        public List<UsModuloDto> ObtenerUsModulos()
        {
            List<UsModuloDto> resultado = new List<UsModuloDto>();
            try
            {
                var lista = SecurityUsDB.ObtenerUsModulos();

                foreach (var item in lista)
                {
                    resultado.Add(new UsModuloDto
                    {
                        MODULO = item.MODULO,
                        NOMBRE = item.NOMBRE,
                        DESCRIPCION = item.DESCRIPCION,
                        ACTIVO = item.ACTIVO,
                        KEYENT = item.KEYENT

                    });

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return resultado;

        }//end ObtenerUsModulos

        public List<UsFormularioDto> ObtenerUsFormularios()
        {
            List<UsFormularioDto> resultado = new List<UsFormularioDto>();
            try
            {
                var lista = SecurityUsDB.ObtenerUsFormularios();

                foreach (var item in lista)
                {
                    resultado.Add(new UsFormularioDto
                    {
                        FORMULARIO = item.FORMULARIO,
                        MODULO = item.MODULO,
                        DESCRIPCION = item.DESCRIPCION,
                        REGISTRO_FECHA = item.REGISTRO_FECHA,
                        REGISTRO_USUARIO = item.REGISTRO_USUARIO,
                        Existe = item.Existe

                    });
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return resultado;

        }//end ObtenerUsFormularios

        public int? ObtenerMenuNodoPorNodoPadreYPrioridad(int NodoPadre, int Prioridad)
        {
            int? resultado = null;
            try
            {
                resultado = SecurityUsDB.ObtenerMenuNodoPorNodoPadreYPrioridad(NodoPadre, Prioridad);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }//end ObtenerMenuNodoPorNodoPadreYPrioridad

        public ResultadoCrearYEditarUsMenuDto? ActualizarUsMenu(UsMenuDto info)
        {
            ResultadoCrearYEditarUsMenuDto? resultado = null;
            try
            {
                resultado = SecurityUsDB.ActualizarUsMenu(info);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }//end ActualizarUsMenu

        public int? ObtenerMenuNodoConIsNull()
        {
            int? resultado = null;
            try
            {
                resultado = SecurityUsDB.ObtenerMenuNodoConIsNull();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }//end ObtenerMenuNodoConIsNull

        public int? ObtenerMenuPrioridadPorMenuNodoPadre(int NodoPadre)
        {
            int? resultado = null;
            try
            {
                resultado = SecurityUsDB.ObtenerMenuPrioridadPorMenuNodoPadre(NodoPadre);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;

        }

        public UsModuloDto ObtenerUsModulosOrdenadosPorTipo(string Tipo)
        {
            UsModuloDto resultado = new UsModuloDto();
            try
            {
                resultado = SecurityUsDB.ObtenerUsModulosOrdenadosPorTipo(Tipo);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }//end ObtenerUsModulosOrdenadosPorTipo

        public ResultadoCrearYEditarUsMenuDto? CrearUsMenu(UsMenuDto info)
        {

            ResultadoCrearYEditarUsMenuDto? resultado = null;
            try
            {

                resultado = SecurityUsDB.CrearUsMenu(info);

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;


        }//end CrearUsMenu

        public int? EliminarUnMenuPorNodoPadre(int NodoPadre)
        {
            int? resultado = null;
            try
            {
                resultado = SecurityUsDB.EliminarUnMenuPorNodoPadre(NodoPadre);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }//end EliminarUnMenuPorNodoPadre

        public int? EliminarUsMenusPorMenuNodo(int MenuNodo)
        {
            int? resultado = null;
            try
            {
                resultado = SecurityUsDB.EliminarUsMenusPorMenuNodo(MenuNodo);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }//end EliminarUsMenusPorMenuNodo

        public int? EliminarTodosLosMenusPorNodoPadre(int NodoPadre)
        {
            int? resultado = null;
            try
            {
                resultado = SecurityUsDB.EliminarTodosLosMenusPorNodoPadre(NodoPadre);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }//end EliminarTodosLosMenusPorNodoPadre

        public int? ObtenerMenuNodoPorMenuFormulario(string Formulario)
        {
            int? resultado = null;
            try
            {
                resultado = SecurityUsDB.ObtenerMenuNodoPorMenuFormulario(Formulario);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;

        }//end ObtenerMenuNodoPorMenuFormulario

        public UsFormularioDto ObtenerUsFormularioPorFormulario(string Formulario)
        {
            UsFormularioDto resultado = new UsFormularioDto();
            try
            {
                resultado = SecurityUsDB.ObtenerUsFormularioPorFormulario(Formulario);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }//end ObtenerUsFormularioPorFormulario

        public UsMenuDto ObtenerUsMenuPorMenuNodo(int MenuNodo)
        {
            UsMenuDto resultado = new UsMenuDto();
            try
            {
                resultado = SecurityUsDB.ObtenerUsMenuPorMenuNodo(MenuNodo);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }//end ObtenerUsMenuPorMenuNodo

        public List<UsIconWeb> ObtenerUsMenu_IconosWeb()
        {
            return SecurityUsDB.ObtenerUsMenu_IconosWeb();
        }

    }
}
