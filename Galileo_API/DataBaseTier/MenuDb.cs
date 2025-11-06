using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class MenuDB
    {
        private readonly IConfiguration _config;

        public MenuDB(IConfiguration config)
        {
            _config = config;
        }

        public List<PrimeTreeDtoV2> Obtener_Menu(string Usuario, int Cliente)
        {
            //RAIZ
            List<PrimeTreeDtoV2> primeTreeDtos = new List<PrimeTreeDtoV2>();
            primeTreeDtos = MenuPrincipal();
            //MENU PABRE
            List<MenuDto> ModulosPadre = [];
            //MENU HIJOS 
            List<MenuDto> ModulosHijos = [];
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[Obtener_MenuModulHijos_Por_Usuario]";
                    var values = new
                    {
                        Usuario = Usuario,
                        Cliente = Cliente
                    };
                    ModulosHijos = connection.Query<MenuDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            ModulosPadre = ModulosHijos
                        .Where(e => e.TIPO == "M")
                        .OrderBy(e => e.PRIORIDAD) // Para orden ascendente
                        .ToList();

            List<MenuDto> ModulosFavoritos = [];
            //Menu Favoritos
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spSEG_MenuFavoritos]";
                    var values = new
                    {
                        Usuario = Usuario,
                        Cliente = Cliente,
                    };
                    ModulosFavoritos = connection.Query<MenuDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            try
            {
                // Primero, mapea todas las opciones principales.
                foreach (PrimeTreeDtoV2 opcion in primeTreeDtos)
                {
                    switch (opcion.Key)
                    {
                        case "0":
                            foreach (MenuDto itemLvl2 in ModulosFavoritos)
                            {
                                opcion.Children.Add(new PrimeTreeDtoV2
                                {
                                    Expanded = false,
                                    Key = itemLvl2.MODULO.ToString() + '-' + itemLvl2.MENU_NODO.ToString(),
                                    Label = itemLvl2.NODO_DESCRIPCION,
                                    Selectable = true,
                                    Icon = itemLvl2.ICONO_WEB,
                                    Children = [],
                                    Data = itemLvl2,
                                    badge = 0,
                                    leaf = IsLeaf(itemLvl2.TIPO)
                                });
                            }
                            break;
                        default:
                            foreach (MenuDto itemLvl2 in ModulosPadre)
                            {
                                foreach (var item in opcion.modules)
                                {
                                    if (item == itemLvl2.MODULO)
                                    {
                                        opcion.Children.Add(new PrimeTreeDtoV2
                                        {
                                            Expanded = false,
                                            Key = itemLvl2.MODULO.ToString() + '-' + itemLvl2.MENU_NODO.ToString(),
                                            Label = itemLvl2.NODO_DESCRIPCION,
                                            Selectable = true,
                                            Icon = itemLvl2.ICONO_WEB,
                                            Children = BuscaHijos(itemLvl2, ModulosHijos),
                                            Data = itemLvl2.TIPO,
                                            badge = 0,
                                            leaf = IsLeaf(itemLvl2.TIPO)
                                        });
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return primeTreeDtos;
        }

        private List<PrimeTreeDtoV2> MenuPrincipal()
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

        private bool IsLeaf(string tipo)
        {
            if (tipo == "A")
            {
                return true;
            }
            return false;
        }

        private List<PrimeTreeDtoV2> BuscaHijos(MenuDto nodoPadre, List<MenuDto> ModulosHijos)
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
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "No se encontró URL de Manual en este momento";
                response.Result = "";
            }
            return response;
        }

    }
}
