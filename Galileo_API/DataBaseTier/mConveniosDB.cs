using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier
{
    /// <summary>
    /// Equivalente a mConvenios.bas (VB6): utilitarios compartidos del módulo de Convenios.
    /// Los valores del usuario viajan como parámetros; los nombres de tabla y columna salen de listas fijas.
    /// </summary>
    public class MConveniosDB
    {
        private const string ColDescripcionMin = "descripcion";
        private const string ColDescripcion = "Descripcion";

        private readonly PortalDB _portalDb;

        private static readonly Dictionary<string, ComboCatalogo> CombosCatalogo = new(StringComparer.OrdinalIgnoreCase)
        {
            ["INSTITUCIONES"] = new("instituciones", "cod_institucion", ColDescripcionMin),
            ["PROMOTORES"] = new("promotores", "ID_PROMOTOR", "Nombre"),
            ["PROFESIONES"] = new("AFI_PROFESIONES", "COD_PROFESION", ColDescripcionMin),
            ["SECTORES"] = new("AFI_SECTORES", "COD_SECTOR", ColDescripcionMin),
            ["BANCOS"] = new("Tes_Bancos", "id_banco", ColDescripcionMin),
            ["PROVINCIAS"] = new("provincias", "provincia", ColDescripcionMin),
            ["CANTONES"] = new("cantones", "canton", ColDescripcionMin, FiltraPorProvincia: true),
            ["DISTRITOS"] = new("distritos", "distrito", ColDescripcionMin, FiltraPorProvincia: true, FiltraPorCanton: true),
        };

        private static readonly Dictionary<string, CodigoTabla> CodigosTabla = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Proveedores"] = new("cxp_proveedores", "Cod_proveedor", ColDescripcion),
            ["Productos"] = new("pv_Productos", "cod_Producto", ColDescripcion),
            ["CargosProv"] = new("cxp_cargos", "Cod_cargo", ColDescripcion),
            ["Unidades"] = new("pv_unidades", "Cod_Unidad", ColDescripcion),
            ["Marcas"] = new("pv_marcas", "Cod_Marca", ColDescripcion),
            ["LineaProducto"] = new("pv_prod_clasifica", "cod_prodclas", ColDescripcion),
            ["Bancos"] = new("Tes_Bancos", "id_banco", ColDescripcion),
            ["Clientes"] = new("pv_clientes", "cedula", "nombre"),
            ["Bodegas"] = new("pv_bodegas", "cod_bodega", ColDescripcion),
            ["Precios"] = new("pv_tipos_precios", "cod_precio", ColDescripcion),
            ["Agentes"] = new("pv_agentes", "cod_agente", "Nombre"),
            ["Cajas"] = new("pv_cajas", "cod_caja", "Nombre"),
            ["Cuentas"] = new("CntX_cuentas", "cod_Cuenta", ColDescripcion, PorContabilidad: true),
        };

        public MConveniosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Equivalente a fxNombre del VB6: nombre del socio por cédula. Vacío si no existe.
        /// </summary>
        public ErrorDto<string> FxNombre(int codEmpresa, string cedula)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var nombre = conn.Query<string?>(
                    "select nombre from socios where cedula = @cedula;",
                    new { cedula }).FirstOrDefault();

                return DbHelper.CreateOkResponse(nombre ?? string.Empty);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, string.Empty);
            }
        }

        /// <summary>
        /// Equivalente a fxDescribeCodigo del VB6: descripción del código en el catálogo general.
        /// Si no existe devuelve código -2 con el mensaje original.
        /// </summary>
        public ErrorDto<string> FxDescribeCodigo(int codEmpresa, string codigo)
        {
            var codigoLimpio = (codigo ?? string.Empty).Trim();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var filas = conn.Query<string?>(
                    "select descripcion from catalogo where codigo = @codigo;",
                    new { codigo = codigoLimpio }).ToList();

                return filas.Count == 0
                    ? DbHelper.CreateErrorResponse($"No se encontró código - {codigoLimpio}", -2, string.Empty)
                    : DbHelper.CreateOkResponse(filas[0] ?? string.Empty);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, string.Empty);
            }
        }

        /// <summary>
        /// Equivalente a fxSIFCCodigos del VB6. tipoDC = "D": recibe el código y devuelve la descripción;
        /// cualquier otro valor: recibe la descripción y devuelve el código. Vacío si no encuentra nada.
        /// </summary>
        public ErrorDto<string> FxSIFCCodigos(int codEmpresa, int codContabilidad, string tipoDC, string codDes, string tabla)
        {
            if (string.IsNullOrWhiteSpace(codDes))
            {
                return DbHelper.CreateOkResponse(string.Empty);
            }

            if (!CodigosTabla.TryGetValue((tabla ?? string.Empty).Trim(), out var config))
            {
                return DbHelper.CreateErrorResponse<string>("La tabla de consulta indicada no es válida.", -2, string.Empty);
            }

            var porCodigo = tipoDC == "D";
            var sql = porCodigo ? config.SqlPorCodigo : config.SqlPorDescripcion;

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var valor = conn.Query<string?>(
                    sql,
                    new { valor = codDes.Trim(), codContabilidad }).FirstOrDefault();

                var resultado = porCodigo ? (valor ?? string.Empty).Trim() : (valor ?? string.Empty);
                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, string.Empty);
            }
        }

        /// <summary>
        /// Equivalente a sbCargaCbo del VB6: devuelve los elementos (llave y descripción) de un catálogo
        /// para llenar un combo. Los filtros de CANTONES y DISTRITOS reemplazan el vFiltro SQL del VB6.
        /// Si no hay elementos devuelve código -2 con el mensaje original.
        /// </summary>
        public ErrorDto<List<MConveniosCatalogoItem>> CargaCatalogoCombo(int codEmpresa, string tipo, int? provincia = null, string? canton = null)
        {
            var clave = (tipo ?? string.Empty).Trim();
            if (!CombosCatalogo.TryGetValue(clave, out var catalogo))
            {
                return DbHelper.CreateErrorResponse<List<MConveniosCatalogoItem>>("El catálogo indicado no es válido.", -2, new());
            }

            var (usaProvincia, usaCanton, parametros) = ConstruirFiltrosCombo(catalogo, provincia, canton);
            var sql = catalogo.ObtenerSql(usaProvincia, usaCanton);

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var lista = conn.Query<MConveniosCatalogoItem>(sql, parametros).ToList();
                foreach (var item in lista)
                {
                    item.Descripcion = (item.Descripcion ?? string.Empty).Trim();
                }

                return lista.Count == 0
                    ? DbHelper.CreateErrorResponse($"No existen {clave} Creadas...(Debe Crearlos)", -2, lista)
                    : DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<MConveniosCatalogoItem>());
            }
        }

        private static (bool usaProvincia, bool usaCanton, DynamicParameters parametros) ConstruirFiltrosCombo(ComboCatalogo catalogo, int? provincia, string? canton)
        {
            var parametros = new DynamicParameters();
            var usaProvincia = catalogo.FiltraPorProvincia && provincia.HasValue;
            var usaCanton = catalogo.FiltraPorCanton && !string.IsNullOrWhiteSpace(canton);

            if (usaProvincia)
            {
                parametros.Add("provincia", provincia!.Value);
            }

            if (usaCanton)
            {
                parametros.Add("canton", canton!.Trim());
            }

            return (usaProvincia, usaCanton, parametros);
        }

        /// <summary>
        /// Los nombres de tabla y columna salen solo de las listas fijas de esta clase; el SQL de cada
        /// catálogo se arma una sola vez al declararlo y los valores del usuario viajan como parámetros.
        /// </summary>
        private sealed record ComboCatalogo(
            string Tabla,
            string Llave,
            string Descripcion,
            bool FiltraPorProvincia = false,
            bool FiltraPorCanton = false)
        {
            private readonly string[] _sqlPorFiltro = ArmarSqlPorFiltro(Tabla, Llave, Descripcion);

            public string ObtenerSql(bool usaProvincia, bool usaCanton)
                => _sqlPorFiltro[(usaProvincia ? 1 : 0) + (usaCanton ? 2 : 0)];

            private static string[] ArmarSqlPorFiltro(string tabla, string llave, string descripcion)
            {
                var select = string.Concat("select convert(varchar(max), ", llave, ") as Llave, ", descripcion, " as Descripcion from ", tabla);
                var orden = string.Concat(" order by ", descripcion, ";");
                return
                [
                    select + ";",
                    select + " where provincia = @provincia" + orden,
                    select + " where canton = @canton" + orden,
                    select + " where provincia = @provincia and canton = @canton" + orden
                ];
            }
        }

        private sealed record CodigoTabla(
            string Tabla,
            string Codigo,
            string Descripcion,
            bool PorContabilidad = false)
        {
            public string SqlPorCodigo { get; } = ArmarSql(Tabla, Descripcion, Codigo, PorContabilidad);

            public string SqlPorDescripcion { get; } = ArmarSql(Tabla, Codigo, Descripcion, PorContabilidad);

            private static string ArmarSql(string tabla, string columnaRetorno, string columnaFiltro, bool porContabilidad)
            {
                var filtroContabilidad = porContabilidad ? " and cod_contabilidad = @codContabilidad" : string.Empty;
                return string.Concat("select convert(varchar(max), ", columnaRetorno, ") from ", tabla, " where ", columnaFiltro, " = @valor", filtroContabilidad, ";");
            }
        }
    }

    /// <summary>
    /// Elemento de catálogo para combos (equivale a xLlave / xDesc del VB6).
    /// </summary>
    public class MConveniosCatalogoItem
    {
        public string? Llave { get; set; }
        public string? Descripcion { get; set; }
    }
}
