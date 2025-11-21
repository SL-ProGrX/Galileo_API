using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class MenuDB
    {
        private readonly IConfiguration _config;
        private const string _connStrg = "DefaultConnString";

        public MenuDB(IConfiguration config)
        {
            _config = config;
        }

        public List<PrimeTreeDtoV2> Obtener_Menu(string Usuario, int Cliente)
        {
            List<PrimeTreeDtoV2> primeTreeDtos = MenuPrincipal();
            List<MenuDto> modulosPadre;
            List<MenuDto> modulosHijos;
            List<MenuDto> modulosFavoritos;

            modulosHijos = ObtenerModulosHijos(Usuario, Cliente);
            modulosPadre = modulosHijos
                .Where(e => e.TIPO == "M")
                .OrderBy(e => e.PRIORIDAD)
                .ToList();
            modulosFavoritos = ObtenerModulosFavoritos(Usuario, Cliente);

            foreach (PrimeTreeDtoV2 opcion in primeTreeDtos)
            {
                if (opcion.Key == "0")
                {
                    if (opcion.Children != null)
                    {
                        opcion.Children.AddRange(CrearFavoritos(modulosFavoritos));
                    }
                }
                else
                {
                    if (opcion.Children != null)
                    {
                        opcion.Children.AddRange(CrearPadres(opcion.modules != null ? opcion.modules.ToList() : new List<int>(), modulosPadre, modulosHijos));
                    }
                }
            }

            return primeTreeDtos;
        }

        private List<MenuDto> ObtenerModulosHijos(string usuario, int cliente)
        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(_connStrg)))
                {
                    var procedure = "[Obtener_MenuModulHijos_Por_Usuario]";
                    var values = new { Usuario = usuario, Cliente = cliente };
                    return connection.Query<MenuDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return [];
            }
        }

        private List<MenuDto> ObtenerModulosFavoritos(string usuario, int cliente)
        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(_connStrg)))
                {
                    var procedure = "[spSEG_MenuFavoritos]";
                    var values = new { Usuario = usuario, Cliente = cliente };
                    return connection.Query<MenuDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return [];
            }
        }

        private static List<PrimeTreeDtoV2> CrearFavoritos(List<MenuDto> modulosFavoritos)
        {
            var favoritos = new List<PrimeTreeDtoV2>();
            foreach (MenuDto item in modulosFavoritos)
            {
                favoritos.Add(new PrimeTreeDtoV2
                {
                    Expanded = false,
                    Key = item.MODULO.ToString() + '-' + item.MENU_NODO.ToString(),
                    Label = item.NODO_DESCRIPCION,
                    Selectable = true,
                    Icon = item.ICONO_WEB,
                    Children = [],
                    Data = item,
                    badge = 0,
                    leaf = IsLeaf(item.TIPO)
                });
            }
            return favoritos;
        }

        private static List<PrimeTreeDtoV2> CrearPadres(List<int> modules, List<MenuDto> modulosPadre, List<MenuDto> modulosHijos)
        {
            var padres = modulosPadre
                .Where(padre => modules.Contains(padre.MODULO))
                .Select(padre => new PrimeTreeDtoV2
                {
                    Expanded = false,
                    Key = padre.MODULO.ToString() + '-' + padre.MENU_NODO.ToString(),
                    Label = padre.NODO_DESCRIPCION,
                    Selectable = true,
                    Icon = padre.ICONO_WEB,
                    Children = BuscaHijos(padre, modulosHijos),
                    Data = padre.TIPO,
                    badge = 0,
                    leaf = IsLeaf(padre.TIPO)
                })
                .ToList();
            return padres;
        }

        private static List<PrimeTreeDtoV2> MenuPrincipal()
        {
            List<PrimeTreeDtoV2> menu = new List<PrimeTreeDtoV2> {

                    //new PrimeTreeDtoV2 { Key = "6", Label = "Migrados", Icon = "fa fa-list-alt", leaf = false,
                    //    modules = [], Children = new List<PrimeTreeDtoV2>() },
                    new PrimeTreeDtoV2 { Key = "0", Label = "Favoritos", Icon = "fas fa-star", leaf = false,
                        modules = [], Children = new List<PrimeTreeDtoV2>(), badge = 0 },
                    new PrimeTreeDtoV2 { Key = "1", Label = "Cuentas Corrientes", 
                        Icon = "fas fa-building-columns", leaf = false,
                        modules = [1, 2, 3, 4, 5, 6, 7, 9, 10, 18],
                        Children = new List<PrimeTreeDtoV2>(),
                        badge = 0},
                    new PrimeTreeDtoV2 { Key = "2", Label = "Retail", Icon = "fas fa-calendar-days", leaf = false,
                        modules = [9, 30, 31, 32, 33, 34, 35], Children = new List<PrimeTreeDtoV2>(), badge = 2 },
                    new PrimeTreeDtoV2 { Key = "3", Label = "Administrativos", Icon = "fas fa-user-tie", leaf = false,
                        modules = [8, 11, 14, 16, 17, 19, 21, 23, 30, 31, 37, 38, 40], Children = new List<PrimeTreeDtoV2>(), badge = 0 },
                    new PrimeTreeDtoV2 { Key = "4", Label = "Financieros", Icon = "fas fa-chart-simple", leaf = false,
                        modules = [12, 20, 21, 22, 24, 30, 31, 36], Children = new List<PrimeTreeDtoV2>(), badge = 0 },
                    new PrimeTreeDtoV2 { Key = "5", Label = "Configuración", Icon = "fas fa-gears", leaf = false,
                        modules = [0] , Children = new List<PrimeTreeDtoV2>(),  badge = 0}

            };

            return menu;
        }

        private static bool IsLeaf(string tipo)
        {
            if (tipo == "A")
            {
                return true;
            }
            return false;
        }

        private static List<PrimeTreeDtoV2> BuscaHijos(MenuDto nodoPadre, List<MenuDto> ModulosHijos)
        {
            try
            {
                List<PrimeTreeDtoV2> ItemChildern = new List<PrimeTreeDtoV2>();
                List<MenuDto> ModulosHijosF = ModulosHijos
                            .Where(e => e.NODO_PADRE == nodoPadre.MENU_NODO)
                            .OrderBy(e => e.PRIORIDAD) // Para orden ascendente
                            .ToList();

                foreach (MenuDto itemLvl2 in ModulosHijosF)
                {
                    ItemChildern.Add(new PrimeTreeDtoV2
                    {
                        Expanded = false,
                        Key = itemLvl2.MODULO.ToString() + '-' + itemLvl2.MENU_NODO.ToString(),
                        Label = itemLvl2.NODO_DESCRIPCION,
                        Selectable = true,
                        Icon = itemLvl2.ICONO_WEB,
                        Children = BuscaHijos(itemLvl2, ModulosHijos),
                        Data = itemLvl2,
                        badge = 0,
                        leaf = IsLeaf(itemLvl2.TIPO)
                    });
                }
                return ItemChildern;

            }
            catch (Exception)
            {
                return new List<PrimeTreeDtoV2>();
            }


        }

        public List<MenuDto> Obtener_MenuFavoritos(string Usuario, int Cliente)
        {
            List<MenuDto> resp = [];
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(_connStrg)))
                {
                    var procedure = "[spSEG_MenuFavoritos]";
                    var values = new
                    {
                        Usuario = Usuario,
                        Cliente = Cliente,
                    };
                    resp = connection.Query<MenuDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public int Agregar_MenuFavoritos(string Usuario, int Cliente, int Nodo, string opcion)
        {
            int resp = 0;
            try
            {
                opcion = (opcion == "mas") ? "+" : "-";
                using (var connection = new SqlConnection(_config.GetConnectionString(_connStrg)))
                {
                    var procedure = "[spSEG_MenuFavoritosAdd]";
                    var values = new
                    {
                        Usuario = Usuario,
                        Cliente = Cliente,
                        Nodo = Nodo,
                        Opcion = opcion
                    };
                    resp = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto<UsMenuManual> ManualMenu_Obtener(string key)
        {
            var response = new ErrorDto<UsMenuManual>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(_connStrg)))
                {
                    var query = $"SELECT MENU_NODO AS 'key', [FRAME_WEB] AS FRAME FROM [dbo].[US_MANUALES]  WHERE MENU_NODO = {key}";
                    response.Result = connection.Query<UsMenuManual>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new UsMenuManual();
            }
            return response;
        }

        public ErrorDto<string> ManualFormulario_Obtener(string formulario)
        {
            var response = new ErrorDto<string>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(_connStrg)))
                {
                    var query = $@"select um.FRAME_WEB   from US_MENUS m left join US_MANUALES um 
                                        ON m.MENU_NODO = um.MENU_NODO 
                                        where 
                                        m.FORMULARIO = @formulario";
                    response.Result = connection.QueryFirstOrDefault<string>(query,
                        new { formulario = formulario }
                        );
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "No se encontró URL de Manual en este momento";
                response.Result = "";
            }
            return response;
        }

    }
}
