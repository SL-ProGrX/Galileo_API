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
        private readonly PortalDB _portalDb;

        private static readonly Dictionary<string, ComboCatalogo> CombosCatalogo = new(StringComparer.OrdinalIgnoreCase)
        {
            ["INSTITUCIONES"] = new("instituciones", "cod_institucion", "descripcion"),
            ["PROMOTORES"] = new("promotores", "ID_PROMOTOR", "Nombre"),
            ["PROFESIONES"] = new("AFI_PROFESIONES", "COD_PROFESION", "descripcion"),
            ["SECTORES"] = new("AFI_SECTORES", "COD_SECTOR", "descripcion"),
            ["BANCOS"] = new("Tes_Bancos", "id_banco", "descripcion"),
            ["PROVINCIAS"] = new("provincias", "provincia", "descripcion"),
            ["CANTONES"] = new("cantones", "canton", "descripcion", FiltraPorProvincia: true),
            ["DISTRITOS"] = new("distritos", "distrito", "descripcion", FiltraPorProvincia: true, FiltraPorCanton: true),
        };

        private static readonly Dictionary<string, CodigoTabla> CodigosTabla = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Proveedores"] = new("cxp_proveedores", "Cod_proveedor", "Descripcion"),
            ["Productos"] = new("pv_Productos", "cod_Producto", "Descripcion"),
            ["CargosProv"] = new("cxp_cargos", "Cod_cargo", "Descripcion"),
            ["Unidades"] = new("pv_unidades", "Cod_Unidad", "Descripcion"),
            ["Marcas"] = new("pv_marcas", "Cod_Marca", "Descripcion"),
            ["LineaProducto"] = new("pv_prod_clasifica", "cod_prodclas", "Descripcion"),
            ["Bancos"] = new("Tes_Bancos", "id_banco", "Descripcion"),
            ["Clientes"] = new("pv_clientes", "cedula", "nombre"),
            ["Bodegas"] = new("pv_bodegas", "cod_bodega", "Descripcion"),
            ["Precios"] = new("pv_tipos_precios", "cod_precio", "Descripcion"),
            ["Agentes"] = new("pv_agentes", "cod_agente", "Nombre"),
            ["Cajas"] = new("pv_cajas", "cod_caja", "Nombre"),
            ["Cuentas"] = new("CntX_cuentas", "cod_Cuenta", "Descripcion", PorContabilidad: true),
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
            var columnaRetorno = porCodigo ? config.Descripcion : config.Codigo;
            var columnaFiltro = porCodigo ? config.Codigo : config.Descripcion;
            var filtroContabilidad = config.PorContabilidad ? " and cod_contabilidad = @codContabilidad" : string.Empty;

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var valor = conn.Query<string?>(
                    $"select convert(varchar(max), {columnaRetorno}) from {config.Tabla} where {columnaFiltro} = @valor{filtroContabilidad};",
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

            var (where, parametros) = ConstruirFiltrosCombo(catalogo, provincia, canton);
            var sql = $"select convert(varchar(max), {catalogo.Llave}) as Llave, {catalogo.Descripcion} as Descripcion from {catalogo.Tabla}"
                    + (where.Count > 0 ? $" where {string.Join(" and ", where)} order by {catalogo.Descripcion}" : string.Empty)
                    + ";";

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

        private static (List<string> where, DynamicParameters parametros) ConstruirFiltrosCombo(ComboCatalogo catalogo, int? provincia, string? canton)
        {
            var where = new List<string>();
            var parametros = new DynamicParameters();

            if (catalogo.FiltraPorProvincia && provincia.HasValue)
            {
                where.Add("provincia = @provincia");
                parametros.Add("provincia", provincia.Value);
            }

            if (catalogo.FiltraPorCanton && !string.IsNullOrWhiteSpace(canton))
            {
                where.Add("canton = @canton");
                parametros.Add("canton", canton.Trim());
            }

            return (where, parametros);
        }

        private sealed record ComboCatalogo(
            string Tabla,
            string Llave,
            string Descripcion,
            bool FiltraPorProvincia = false,
            bool FiltraPorCanton = false);

        private sealed record CodigoTabla(
            string Tabla,
            string Codigo,
            string Descripcion,
            bool PorContabilidad = false);
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
