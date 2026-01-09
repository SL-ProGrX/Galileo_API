using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCprTiposOrdenDB
    {
        private readonly PortalDB _portalDb;

        public FrmCprTiposOrdenDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<TiposOrdenLista> ObtenerTiposOrdenes(int codEmpresa, string jFiltros)
        {
            var filtro = SafeParseFiltro(jFiltros);

            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var (whereSql, whereParams) = BuildFiltroTiposOrden(filtro.filtro);

                var total = conn.QueryFirstOrDefault<int>(
                    $"SELECT COUNT(Tipo_Orden) FROM cpr_Tipo_Orden {whereSql}",
                    whereParams
                );

                var (pagingSql, pagingParams) = BuildPaging(filtro.pagina, filtro.paginacion);
                var prms = MergeParams(whereParams, pagingParams);

                var lista = conn.Query<TiposOrdenDto>(
                    $@"SELECT Tipo_Orden, descripcion, activo
                       FROM cpr_Tipo_Orden
                       {whereSql}
                       ORDER BY Tipo_Orden
                       {pagingSql}",
                    prms
                ).ToList();

                return new TiposOrdenLista
                {
                    total = total,
                    lista = lista
                };
            });

            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<TiposOrdenLista>(r.Description ?? "Error", r.Code ?? -1, null!);

            return DbHelper.CreateOkResponse(r.Result ?? new TiposOrdenLista { total = 0, lista = new List<TiposOrdenDto>() });
        }

        public ErrorDto TipoOrden_Actualizar(int codEmpresa, TiposOrdenDto tiposOrden)
        {
            var activo = tiposOrden.activo ? 1 : 0;

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"UPDATE cpr_Tipo_Orden
                  SET activo = @Activo,
                      descripcion = @Descripcion
                  WHERE Tipo_Orden = @TipoOrden",
                new
                {
                    Activo = activo,
                    Descripcion = tiposOrden.descripcion,
                    TipoOrden = tiposOrden.tipo_orden
                }
            );
        }

        public ErrorDto TipoOrden_Eliminar(int codEmpresa, string tiposOrden)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"DELETE FROM cpr_Tipo_Orden
                  WHERE Tipo_Orden = @TipoOrden",
                new { TipoOrden = tiposOrden }
            );
        }

        public ErrorDto TipoOrden_Insertar(int codEmpresa, TiposOrdenDto tiposOrden)
        {
            // Mantengo tu lógica: secuencia como string 000.
            var seq = ObtenerSequencia(codEmpresa);
            var activo = tiposOrden.activo ? 1 : 0;

            tiposOrden.tipo_orden = seq;

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"INSERT INTO cpr_Tipo_Orden (Tipo_Orden, descripcion, activo)
                  VALUES (@TipoOrden, @Descripcion, @Activo)",
                new
                {
                    TipoOrden = tiposOrden.tipo_orden,
                    Descripcion = tiposOrden.descripcion,
                    Activo = activo
                }
            );
        }

        public string ObtenerSequencia(int codEmpresa)
        {
            // Si falla, devolvemos "00" como tu código original
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                return conn.QueryFirstOrDefault<string>(
                    @"SELECT FORMAT(ISNULL(MAX(CAST(Tipo_Orden AS INT)), 0) + 1, '000')
                      FROM cpr_Tipo_Orden"
                ) ?? "00";
            });

            return (r.Code == 0 && !string.IsNullOrWhiteSpace(r.Result)) ? r.Result! : "00";
        }

        public ErrorDto<List<RangosMontos>> rangosMontos_Obtener(int codEmpresa, string usuario)
        {
            return DbHelper.ExecuteListQuery<RangosMontos>(
                _portalDb,
                codEmpresa,
                @"SELECT
                      r.cod_rango as item,
                      CONCAT(r.descripcion, ' - Mínimo: ', r.monto_minimo, ' Máximo: ', r.monto_maximo) AS descripcion,
                      r.MONTO_MAXIMO, r.MONTO_MINIMO
                  FROM cpr_orden_rangos r
                  INNER JOIN CPR_RANGO_USUARIO u ON r.cod_rango = u.cod_rango
                  WHERE r.REGISTRO_USUARIO = @Usuario",
                new { Usuario = usuario }
            );
        }

        // ----------------- Helpers -----------------

        private static TipoOrdenFiltro SafeParseFiltro(string jFiltros)
        {
            try
            {
                return JsonConvert.DeserializeObject<TipoOrdenFiltro>(jFiltros) ?? new TipoOrdenFiltro();
            }
            catch
            {
                return new TipoOrdenFiltro();
            }
        }

        private static (string whereSql, object parameters) BuildFiltroTiposOrden(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return (string.Empty, new { });

            var like = $"%{filtro.Trim()}%";
            return ("WHERE Tipo_Orden LIKE @F OR descripcion LIKE @F", new { F = like });
        }

        private static (string pagingSql, object parameters) BuildPaging(int pagina, int paginacion)
        {
            if (pagina <= 0 || paginacion <= 0)
                return (string.Empty, new { });

            // tu implementación: pagina es OFFSET
            return ("OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY", new { Offset = pagina, Fetch = paginacion });
        }

        private static object MergeParams(object a, object b)
        {
            var p = new DynamicParameters(a);
            p.AddDynamicParams(b);
            return p;
        }
    }
}