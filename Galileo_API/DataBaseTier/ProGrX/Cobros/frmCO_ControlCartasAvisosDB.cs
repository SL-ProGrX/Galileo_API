using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using System.Text;
using System.Data.Common;
using Galileo.Models.ProGrX.Cobros;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace PgxAPI.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoControlCartasAvisosDB
    {

        private readonly PortalDB _portalDB;

        public FrmCoControlCartasAvisosDB(IConfiguration config)
        {

            _portalDB = new PortalDB(config);
        }



        public ErrorDto<List<DropDownListaGenericaModel>> CO_ControlCartasAvisos_Usuarios_Consultar(int codEmpresa)
        {

            const string sql = @"
                        SELECT
                            usuario AS item,
                            nombre  AS descripcion
                        FROM cbr_usuarios
                        WHERE estado = @estado
                        ORDER BY nombre;";


            var resp = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                codEmpresa,
                sql,
                new { estado = 1 }
            );

            return resp;
        }


        public ErrorDto<CoControlCartasAvisosLista> CO_ControlCartasAvisos_Buscar(
            int codEmpresa,
            bool esExportar,
            FiltrosLazyLoadData filtros,
            string usuario 
        )
        {
            if (filtros == null)
                return DbHelper.CreateErrorResponse<CoControlCartasAvisosLista>("El parámetro 'filtros' es obligatorio.");

            var term = filtros.filtro?.Trim();
            var sortKey = GetSortKeyCartas(filtros);
            var orderBySql = BuildOrderBySqlCartas(sortKey);
            var (offset, pageSize) = GetPagingCartas(filtros, esExportar);

          
            const string selectBase = @"
                        SELECT
                            fecha_asignacion,
                            cedula,
                            nombre,
                            mantener,
                            mora,
                            CuotaMora
                        FROM vCBRControlListado";

            var whereSql = BuildWhereSqlCartas(term);
            var pagingSql = esExportar ? string.Empty : " OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            var listSql = $"{selectBase}{whereSql}{orderBySql}{pagingSql}";
            var countSql = $@"SELECT COUNT(1) FROM vCBRControlListado {whereSql}";

        
            var p = new DynamicParameters();
            p.Add("@usuario", usuario);                 
            p.Add("@term", term ?? string.Empty);
            if (!esExportar)
            {
                p.Add("@offset", Math.Max(0, offset));
                p.Add("@pageSize", Math.Max(1, pageSize));
            }

            var totalResp = DbHelper.ExecuteSingleQuery<int>(
                _portalDB, codEmpresa, countSql, defaultValue: 0, parameters: p);

           

            var listResp = DbHelper.ExecuteListQuery<CoControlCartasAvisosData>(
                _portalDB, codEmpresa, listSql, parameters: p);

          
            var result = new CoControlCartasAvisosLista
            {
                total = totalResp.Result,
                lista = listResp.Result ?? new List<CoControlCartasAvisosData>()
            };

            return DbHelper.CreateOkResponse(result);
        }

   
        private enum CCASortKey
        {
            CedulaAsc, CedulaDesc,
            NombreAsc, NombreDesc,
            FechaAsignacionAsc, FechaAsignacionDesc,
            MoraAsc, MoraDesc,
            CuotaMoraAsc, CuotaMoraDesc,
            MantenerAsc, MantenerDesc
        }

        
        private static readonly Dictionary<string, (CCASortKey Asc, CCASortKey Desc)> CcaSortMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["cedula"] = (CCASortKey.CedulaAsc, CCASortKey.CedulaDesc),
                ["nombre"] = (CCASortKey.NombreAsc, CCASortKey.NombreDesc),
                ["fecha_asignacion"] = (CCASortKey.FechaAsignacionAsc, CCASortKey.FechaAsignacionDesc),
                ["mora"] = (CCASortKey.MoraAsc, CCASortKey.MoraDesc),
                ["cuotamora"] = (CCASortKey.CuotaMoraAsc, CCASortKey.CuotaMoraDesc),
                ["mantener"] = (CCASortKey.MantenerAsc, CCASortKey.MantenerDesc),
            };

        private static CCASortKey GetSortKeyCartas(FiltrosLazyLoadData filtros)
        {
            var field = string.IsNullOrWhiteSpace(filtros.sortField) ? "cedula" : filtros.sortField.Trim();
            var asc = filtros.sortOrder != 0;

            CCASortKey key;

            if (CcaSortMap.TryGetValue(field, out var pair))
            {
                key = asc ? pair.Asc : pair.Desc;
            }
            else
            {
                key = CCASortKey.CedulaAsc;
            }

            return key;

        }

        private static string BuildOrderBySqlCartas(CCASortKey key) =>
            key switch
            {
                CCASortKey.CedulaAsc => " ORDER BY cedula ASC",
                CCASortKey.CedulaDesc => " ORDER BY cedula DESC",
                CCASortKey.NombreAsc => " ORDER BY nombre ASC",
                CCASortKey.NombreDesc => " ORDER BY nombre DESC",
                CCASortKey.FechaAsignacionAsc => " ORDER BY fecha_asignacion ASC",
                CCASortKey.FechaAsignacionDesc => " ORDER BY fecha_asignacion DESC",
                CCASortKey.MoraAsc => " ORDER BY mora ASC",
                CCASortKey.MoraDesc => " ORDER BY mora DESC",
                CCASortKey.CuotaMoraAsc => " ORDER BY CuotaMora ASC",
                CCASortKey.CuotaMoraDesc => " ORDER BY CuotaMora DESC",
                CCASortKey.MantenerAsc => " ORDER BY mantener ASC",
                CCASortKey.MantenerDesc => " ORDER BY mantener DESC",
                _ => " ORDER BY cedula ASC"
            };

   
        private static string BuildWhereSqlCartas(string? term)
        {
            var parts = new List<string>
                {
                    "Usuario = @usuario",
                    "mora > 0", 
                    @"cedula NOT IN (
                        SELECT s.cedula
                        FROM cbr_seguimiento s
                        WHERE DATEADD(D, s.tiempo_resolucion, s.fecha) >= dbo.MyGetdate()
                          AND s.cod_gestion IN (
                                SELECT valor
                                FROM cbr_parametros
                                WHERE cod_parametro IN ('03','04')
                          )
                    )"
                };

            if (!string.IsNullOrWhiteSpace(term))
            {
                // LIKE parametrizado; CAST por si CuotaMora es numérico
                parts.Add("(cedula LIKE '%' + @term + '%' OR nombre LIKE '%' + @term + '%' OR CAST(CuotaMora AS NVARCHAR(50)) LIKE '%' + @term + '%')");
            }

            return " WHERE " + string.Join(" AND ", parts);
        }

     
        private static (int offset, int pageSize) GetPagingCartas(FiltrosLazyLoadData f, bool esExportar)
        {
            if (esExportar) return (0, 0);
            var pageSize = Math.Max(1, f.paginacion);
            var offset = Math.Max(0, f.pagina); // si 'pagina' fuera N° de página, usa: (Math.Max(1, f.pagina) - 1) * pageSize
            return (offset, pageSize);
        }

    }
}
