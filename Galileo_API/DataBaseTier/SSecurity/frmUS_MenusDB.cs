using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.Security;
using System.Data;


namespace Galileo.DataBaseTier
{
    public class FrmUsMenusDb
    {

        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsMenusDb(IConfiguration config)
        {
            _config = config;
        }


        public List<UsMenuDto> ObtenerUsMenusPorTipoYNodoPadreEsNull(string? Tipo)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            Tipo ??= "M";
            List<UsMenuDto> Result = [];
            string sql = "select * from US_Menus where tipo = @Tipo and nodo_padre is null order by prioridad";
            var values = new
            {
                Tipo = Tipo,
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<UsMenuDto>(sql, values).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }//end ObtenerMenus

        public List<UsMenuDto> ObtenerUsMenus()
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            List<UsMenuDto> Result = [];
            string sql = "select * from US_Menus order by MENU_NODO";
            var values = new
            {

            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<UsMenuDto>(sql, values).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }//end ObtenerUsMenus

        public List<UsModuloDto> ObtenerUsModulos()
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            List<UsModuloDto> Result = [];
            string sql = "select * from us_modulos order by modulo";
            var values = new
            {

            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<UsModuloDto>(sql, values).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }//end ObtenerModulos

        public List<UsFormularioDto> ObtenerUsFormularios()
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            List<UsFormularioDto> Result = [];
            string sql = "select *,dbo.fxSEG_OpcionAsignada(Formulario,0) as 'Existe' from US_FORMULARIOS order by formulario";
            var values = new
            {

            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<UsFormularioDto>(sql, values).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }//end ObtenerFormularios

        public int? ObtenerMenuNodoPorNodoPadreYPrioridad(int NodoPadre, int Prioridad)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            int? Result = null;
            string sql = "select MENU_NODO  from US_MENUS where NODO_PADRE = @NodoPadre and PRIORIDAD = @Prioridad";
            var values = new
            {
                NodoPadre = NodoPadre,
                Prioridad = Prioridad
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<int?>(sql, values).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

            //example: select MENU_NODO  from US_MENUS where NODO_PADRE = 1 and PRIORIDAD = 1010
            //(output: 20)

        }//end ObtenerMenuNodoPorNodoPadreConPrioridad

        public ResultadoCrearYEditarUsMenuDto? ActualizarUsMenu(UsMenuDto info)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            ResultadoCrearYEditarUsMenuDto? resultado = null;
            try
            {
                using (var connection = new SqlConnection(stringConn))
                {
                    //AutoMapper?
                    var values = new
                    {
                        MENU_NODO = info.MENU_NODO,
                        NODO_PADRE = info.NODO_PADRE,
                        NODO_DESCRIPCION = info.NODO_DESCRIPCION,
                        TIPO = info.TIPO,
                        ICONO = info.ICONO,
                        MODO = info.MODO,
                        MODAL = info.MODAL,
                        ACCESOS_DLL_ID = info.ACCESOS_DLL_ID,
                        ACCESOS_DLL_CLS = info.ACCESOS_DLL_CLS,
                        PRIORIDAD = info.PRIORIDAD,
                        FORMULARIO = info.FORMULARIO,
                        MODULO = info.MODULO,
                        MIGRADO_WEB = info.MIGRADO_WEB,
                        ICONO_WEB = info.ICONO_WEB
                    };

                    resultado = connection.QueryFirst<ResultadoCrearYEditarUsMenuDto?>("spGa_Menu_Update", values, commandType: CommandType.StoredProcedure);

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;

        }//end ActualizarMenuPorMenuNodo

        public int? ObtenerMenuPrioridadPorMenuNodoPadre(int NodoPadre)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            int? Result = null;
            string sql = "select isnull(max(prioridad),1000) + 1 as 'Prioridad' from us_menus where Nodo_Padre is null";

            if (NodoPadre != 0)
            {
                sql = "select isnull(max(prioridad),1000) + 1 as 'Prioridad' from us_menus where Nodo_Padre = @NodoPadre";
            }

            var values = new
            {
                NodoPadre = NodoPadre
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<int?>(sql, values).FirstOrDefault();
            }

            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return Result;

        }//end ObtenerMenuPrioridadPorMenuNodoPadre

        public int? ObtenerMenuNodoConIsNull()
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            int? Result = null;
            string sql = "select isnull(max(menu_nodo),0) + 1 as MenuNodo from us_Menus";
            var values = new
            {

            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<int?>(sql, values).FirstOrDefault();
            }

            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return Result;

        }//end ObtenerMenuNodoConIsNull

        public UsModuloDto? ObtenerUsModulosOrdenadosPorTipo(string Tipo)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            //Tipo ??= "M"; //si Tipo es null, asignar "M"

            UsModuloDto? Result = null;
            string sql = "select * from US_modulos where modulo not in(select modulo from us_menus where tipo = @Tipo) order by modulo";
            var values = new
            {
                Tipo = Tipo
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                Result = connection.Query<UsModuloDto>(sql, values).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Result;

        }//end ObtenerModulosOrdenadosPorTipo

        public ResultadoCrearYEditarUsMenuDto? CrearUsMenu(UsMenuDto info)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            ResultadoCrearYEditarUsMenuDto? resultado = null;
            try
            {
                using (var connection = new SqlConnection(stringConn))
                {
                    //AutoMapper?
                    var values = new
                    {

                        MENU_NODO = info.MENU_NODO,
                        NODO_PADRE = info.NODO_PADRE,
                        NODO_DESCRIPCION = info.NODO_DESCRIPCION,
                        TIPO = info.TIPO,
                        ICONO = info.ICONO,
                        MODO = info.MODO,
                        MODAL = info.MODAL,
                        ACCESOS_DLL_ID = info.ACCESOS_DLL_ID,
                        ACCESOS_DLL_CLS = info.ACCESOS_DLL_CLS,
                        PRIORIDAD = info.PRIORIDAD,
                        FORMULARIO = info.FORMULARIO,
                        MODULO = info.MODULO,
                        MIGRADO_WEB = info.MIGRADO_WEB,
                        ICONO_WEB = info.ICONO_WEB
                    };

                    resultado = connection.QueryFirst<ResultadoCrearYEditarUsMenuDto?>("spGa_Menu_Crear", values, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;

        }//end CrearMenu

        public int? EliminarUnMenuPorNodoPadre(int NodoPadre)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            int? result = null;
            string sql = "delete us_menus where menu_nodo = @NodoPadre";
            var values = new
            {
                NodoPadre = NodoPadre
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                result = connection.Query<int?>(sql, values).FirstOrDefault();
            }

            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;

        }//end EliminarUnMenuPorNodoPadre

        public int? EliminarTodosLosMenusPorNodoPadre(int NodoPadre)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            int? result = null;
            string sql = "delete us_menus_usos where menu_nodo in(select menu_nodo from us_menus where nodo_padre = @NodoPadre)";
            var values = new
            {
                NodoPadre = NodoPadre
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                result = connection.Query<int?>(sql, values).FirstOrDefault();
            }

            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;

        }//end EliminarTodosLosMenusPorNodoPadre

        public int? EliminarUsMenusPorMenuNodo(int MenuNodo)
        {

            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            int? result = null;
            string sql = "DELETE FROM US_MENUS WHERE MENU_NODO = @MenuNodo OR NODO_PADRE = @MenuNodo";
            var values = new
            {
                MenuNodo = MenuNodo
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                result = connection.Query<int?>(sql, values).FirstOrDefault();
            }

            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;


        }//end EliminarUsMenusPorMenuNodo

        public int? ObtenerMenuNodoPorMenuFormulario(string Formulario)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            int? result = null;
            string sql = "select menu_nodo from us_menus where formulario = @Formulario";
            var values = new
            {
                Formulario = Formulario
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                result = connection.Query<int?>(sql, values).FirstOrDefault();
            }

            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;

        }//end ObtenerMenuNodoPorMenuFormulario

        public UsFormularioDto ObtenerUsFormularioPorFormulario(string Formulario)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            UsFormularioDto result = new UsFormularioDto();
            string sql = "select * from us_formularios where formulario = @Formulario";
            var values = new
            {
                Formulario = Formulario
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var queryResult = connection.Query<UsFormularioDto?>(sql, values).FirstOrDefault();
                if (queryResult != null)
                {
                    result = queryResult;
                }
            }

            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;


        }//end ObtenerUsFormularioPorFormulario

        public UsMenuDto ObtenerUsMenuPorMenuNodo(int MenuNodo)
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            UsMenuDto result = new UsMenuDto();
            string sql = "Select * from US_Menus where Menu_Nodo = @MenuNodo";
            var values = new
            {
                MenuNodo = MenuNodo
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var queryResult = connection.Query<UsMenuDto?>(sql, values).FirstOrDefault();
                if (queryResult != null)
                {
                    result = queryResult;
                }
            }

            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;

        }//end ObtenerUsMenuPorMenuNodo

        public List<UsIconWeb> ObtenerUsMenu_IconosWeb()
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            List<UsIconWeb> result = [];
            string sql = @"select DISTINCT ICONO_WEB as 'label',ICONO_WEB as 'value', ICONO_WEB as 'iconMenu' 
                                from US_Menus order by ICONO_WEB asc";
            try
            {
                using var connection = new SqlConnection(stringConn);
                result = connection.Query<UsIconWeb>(sql).ToList();
            }

            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;

        }//end ObtenerIconosWeb


    }//end class
}//end namespace
