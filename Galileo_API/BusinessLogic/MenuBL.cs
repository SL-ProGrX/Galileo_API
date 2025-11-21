using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class MenuBL
    {
        private readonly IConfiguration _config;
        private readonly MenuDB _db;
        public MenuBL(IConfiguration config)
        {
            _config = config;
            _db = new MenuDB(_config);
        }

        public List<PrimeTreeDtoV2> GenerarMenuV2(string usuario, int cliente)
        {
            return _db.Obtener_Menu(usuario, cliente);
        }

        public int Agregar_MenuFavoritos(string Usuario, int Cliente, int Nodo, string opcion)
        {
            return _db.Agregar_MenuFavoritos(Usuario, Cliente, Nodo, opcion);
        }

        public List<PrimeTreeDto> Obtener_MenuFavoritos(string Usuario, int Cliente)
        {
            List<PrimeTreeDto> Arbol = [];

            var listaFavoritos = _db.Obtener_MenuFavoritos(Usuario, Cliente);


            foreach (var item in listaFavoritos)
            {
                var mod = new PrimeTreeDto
                {
                    Expanded = false,
                    Key = item.MODULO.ToString() + '-' + item.MENU_NODO.ToString(),
                    Label = item.NODO_DESCRIPCION,
                    Selectable = true,
                    Icon = item.ICONO_WEB,
                    Children = new List<PrimeTreeDto>(),
                    Data = item,
                    leaf = true

                };
                Arbol.Add(mod);
            }
            return Arbol;
        }

        public ErrorDto<UsMenuManual> ManualMenu_Obtener(string key)
        {
            return _db.ManualMenu_Obtener(key);
        }

        public ErrorDto<string> ManualFormulario_Obtener(string formulario)
        {
            return _db.ManualFormulario_Obtener(formulario);
        }

    }
}
