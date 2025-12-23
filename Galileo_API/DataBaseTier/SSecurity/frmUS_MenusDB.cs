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

        // === Helpers comunes ===

        private SqlConnection CreateConnection()
        {
            string? stringConn = _config.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(stringConn))
                throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

            return new SqlConnection(stringConn);
        }

        private List<T> QueryList<T>(string sql, object? parameters = null)
        {
            try
            {
                using var connection = CreateConnection();
                return connection.Query<T>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new List<T>();
            }
        }

        private T? QueryFirstOrDefault<T>(string sql, object? parameters = null)
        {
            try
            {
                using var connection = CreateConnection();
                return connection.Query<T?>(sql, parameters).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return default;
            }
        }

        private int? ExecuteReturningInt(string sql, object? parameters = null)
        {
            try
            {
                using var connection = CreateConnection();
                return connection.Query<int?>(sql, parameters).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return null;
            }
        }

        private object BuildUsMenuParams(UsMenuDto info) => new
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

        private ResultadoCrearYEditarUsMenuDto? EjecutarSpMenu(string spName, UsMenuDto info)
        {
            try
            {
                using var connection = CreateConnection();
                return connection.QueryFirst<ResultadoCrearYEditarUsMenuDto?>(
                    spName,
                    BuildUsMenuParams(info),
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return null;
            }
        }

        // === Métodos públicos ===

        public List<UsMenuDto> ObtenerUsMenusPorTipoYNodoPadreEsNull(string? Tipo)
        {
            Tipo ??= "M";
            const string sql = "select * from US_Menus where tipo = @Tipo and nodo_padre is null order by prioridad";

            return QueryList<UsMenuDto>(sql, new { Tipo });
        }

        public List<UsMenuDto> ObtenerUsMenus()
        {
            const string sql = "select * from US_Menus order by MENU_NODO";
            return QueryList<UsMenuDto>(sql);
        }

        public List<UsModuloDto> ObtenerUsModulos()
        {
            const string sql = "select * from us_modulos order by modulo";
            return QueryList<UsModuloDto>(sql);
        }

        public List<UsFormularioDto> ObtenerUsFormularios()
        {
            const string sql = "select *,dbo.fxSEG_OpcionAsignada(Formulario,0) as 'Existe' from US_FORMULARIOS order by formulario";
            return QueryList<UsFormularioDto>(sql);
        }

        public int? ObtenerMenuNodoPorNodoPadreYPrioridad(int NodoPadre, int Prioridad)
        {
            const string sql = "select MENU_NODO  from US_MENUS where NODO_PADRE = @NodoPadre and PRIORIDAD = @Prioridad";

            return QueryFirstOrDefault<int?>(sql, new
            {
                NodoPadre,
                Prioridad
            });
        }

        public ResultadoCrearYEditarUsMenuDto? ActualizarUsMenu(UsMenuDto info)
        {
            return EjecutarSpMenu("spGa_Menu_Update", info);
        }

        public int? ObtenerMenuPrioridadPorMenuNodoPadre(int NodoPadre)
        {
            string sql = "select isnull(max(prioridad),1000) + 1 as 'Prioridad' from us_menus where Nodo_Padre is null";

            if (NodoPadre != 0)
            {
                sql = "select isnull(max(prioridad),1000) + 1 as 'Prioridad' from us_menus where Nodo_Padre = @NodoPadre";
            }

            return QueryFirstOrDefault<int?>(sql, new { NodoPadre });
        }

        public int? ObtenerMenuNodoConIsNull()
        {
            const string sql = "select isnull(max(menu_nodo),0) + 1 as MenuNodo from us_Menus";
            return QueryFirstOrDefault<int?>(sql);
        }

        public UsModuloDto? ObtenerUsModulosOrdenadosPorTipo(string Tipo)
        {
            const string sql = "select * from US_modulos where modulo not in(select modulo from us_menus where tipo = @Tipo) order by modulo";

            return QueryFirstOrDefault<UsModuloDto>(sql, new { Tipo });
        }

        public ResultadoCrearYEditarUsMenuDto? CrearUsMenu(UsMenuDto info)
        {
            return EjecutarSpMenu("spGa_Menu_Crear", info);
        }

        public int? EliminarUnMenuPorNodoPadre(int NodoPadre)
        {
            const string sql = "delete us_menus where menu_nodo = @NodoPadre";
            return ExecuteReturningInt(sql, new { NodoPadre });
        }

        public int? EliminarTodosLosMenusPorNodoPadre(int NodoPadre)
        {
            const string sql = "delete us_menus_usos where menu_nodo in(select menu_nodo from us_menus where nodo_padre = @NodoPadre)";
            return ExecuteReturningInt(sql, new { NodoPadre });
        }

        public int? EliminarUsMenusPorMenuNodo(int MenuNodo)
        {
            const string sql = "DELETE FROM US_MENUS WHERE MENU_NODO = @MenuNodo OR NODO_PADRE = @MenuNodo";
            return ExecuteReturningInt(sql, new { MenuNodo });
        }

        public int? ObtenerMenuNodoPorMenuFormulario(string Formulario)
        {
            const string sql = "select menu_nodo from us_menus where formulario = @Formulario";
            return QueryFirstOrDefault<int?>(sql, new { Formulario });
        }

        public UsFormularioDto ObtenerUsFormularioPorFormulario(string Formulario)
        {
            const string sql = "select * from us_formularios where formulario = @Formulario";

            var result = QueryFirstOrDefault<UsFormularioDto>(sql, new { Formulario });
            return result ?? new UsFormularioDto();
        }

        public UsMenuDto ObtenerUsMenuPorMenuNodo(int MenuNodo)
        {
            const string sql = "Select * from US_Menus where Menu_Nodo = @MenuNodo";

            var result = QueryFirstOrDefault<UsMenuDto>(sql, new { MenuNodo });
            return result ?? new UsMenuDto();
        }

        public List<UsIconWeb> ObtenerUsMenu_IconosWeb()
        {
            const string sql = @"select DISTINCT ICONO_WEB as 'label',ICONO_WEB as 'value', ICONO_WEB as 'iconMenu' 
                                 from US_Menus order by ICONO_WEB asc";

            return QueryList<UsIconWeb>(sql);
        }
    }
}