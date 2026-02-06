
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Globalization;
using static Org.BouncyCastle.Math.EC.ECCurve;
namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCParametrosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const int ModuloCxC = 31;
        private readonly MCntLinkDB mCntLink;

        private const string MovModifica = "MODIFICA - WEB";


        public FrmCxCParametrosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config!);
            mCntLink = new MCntLinkDB(config);
        }

        private void LogBitacora(int empresaId, string usuario, string detalle, string movimiento)
        {

            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloCxC
            });
        }


        private static readonly IReadOnlyDictionary<string, string> OrderableColumns =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["cod_parametro"] = "cod_parametro",
                ["descripcion"] = "descripcion",
                ["valor"] = "valor"
            };

        private const int SortDescendingValue = 1;


        private const string WhereFilter = @"
        WHERE (@like IS NULL)
           OR (cod_parametro LIKE @like)
           OR (descripcion  LIKE @like)
           OR (valor        LIKE @like)";


        private const string SelectColumns =
            "cod_parametro, descripcion, valor, notas, tipo, inicio_fecha, visible, modifica_usuario, modifica_fecha";



        /// <summary>
        /// Consulta de listado de parametros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CxCParametrosLista> CxCParametrosLista_Obtener(int codEmpresa, int codContabilidad, FiltrosLazyLoadData filtros, bool esExportar)
        {


            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse<CxCParametrosLista>("Los filtros no pueden ser nulos.");
            }

            try
            {
                CxCParametros_Cargar(codEmpresa);

                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
                //var response = new CxCParametrosLista();

                var like = BuildLike(filtros.filtro);
                var (orderBy, direction) = BuildOrder(filtros.sortField, filtros.sortOrder);

                var offset = Math.Max(filtros.pagina, 0);
                var fetch = Math.Max(filtros.paginacion, 0);
                var usarPaginacion = !esExportar && fetch > 0;





                var sqlList =
               $@"SELECT {SelectColumns}
                    FROM CxC_Parametros
                    {WhereFilter}
                    ORDER BY {orderBy} {direction}{BuildPagination(usarPaginacion)};";

                var sqlCount =
                        $@"SELECT COUNT(cod_parametro)
                FROM CxC_Parametros
                {WhereFilter};";

                var @params = new
                {
                    like,
                    offset,
                    fetch
                };


                var total = conn.QuerySingle<int>(sqlCount, @params);
                var lista = conn.Query<CxCParametrosData>(sqlList, @params).ToList();


                EnriquecerCuentas(lista, codEmpresa, codContabilidad, mCntLink);

                var response = new CxCParametrosLista
                {
                    total = total,
                    lista = lista
                };

                return DbHelper.CreateOkResponse(response);


            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCParametrosLista>(ex.Message);
            }

        }

        #region Helpers privados (reducen la complejidad cognitiva del método principal)

        private static string? BuildLike(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            var safe = texto.Trim();
            // Si necesitas case-insensitive en DB case-sensitive, evalúa UPPER(col) LIKE UPPER(@like).
            return string.Create(CultureInfo.InvariantCulture, $"%{safe}%");
        }

        private static (string orderBy, string direction) BuildOrder(string? sortField, int sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim();

            var orderBy = OrderableColumns.TryGetValue(field, out var col)
                ? col
                : "cod_parametro";

            var direction = sortOrder == SortDescendingValue ? "DESC" : "ASC";

            return (orderBy, direction);
        }

        private static string BuildPagination(bool usar)
            => usar ? "\nOFFSET @offset ROWS\nFETCH NEXT @fetch ROWS ONLY" : string.Empty;

        private static void EnriquecerCuentas(
            IEnumerable<CxCParametrosData> items,
            int codEmpresa,
            int codContabilidad,
             MCntLinkDB mCntLink)
        {
            foreach (var item in items)
            {
                
                if (!string.Equals(item.Tipo, "CTA", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Valor))
                {
                    item.cuentaMasck = null;
                    item.cuentaDetalle = null;
                    continue;
                }
               

                item.cuentaMasck = mCntLink.fxgCntCuentaFormato(
                    codEmpresa,
                    blnMascara: true,
                    pCuenta: item.Valor,
                    optMensaje: 1);


                item.cuentaDetalle = mCntLink.fxgCntCuentaDesc(
                    codContabilidad,
                    pCuenta: item.Valor);
            }
        }

        #endregion



        /// <summary>
        /// Metodo encargo de ejecutar proceso de carga de parametros iniciales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        private void CxCParametros_Cargar(int CodEmpresa)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var query = $@"exec spCxC_Parametros";
                conn.Execute(query);


            }
            catch (Exception ex)
            {
                throw;
            }

        }


        /// <summary>
        /// Guarda el cambio del valor de parametro modificado
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="valor"></param>
        /// <param name="codParametro"></param>
        /// <returns></returns>
        public ErrorDto CxCParametros_Guardar(int codEmpresa, string usuario, string valor, string codParametro)
        {

            const string sqlUpdate = @"
               

                UPDATE CxC_Parametros
                SET modifica_usuario = @usuario,
                    modifica_Fecha = dbo.MyGetdate(),
                    valor = @valor
                WHERE cod_parametro = @codParametro;

            ";

            var upsert = DbHelper.ExecuteSingleQuery<string>(
                _portalDB, codEmpresa, sqlUpdate, defaultValue: "",
                parameters: new
                {
                    usuario,
                    valor,
                    codParametro,
                });

            if (upsert.Code != 0)
            {
                return DbHelper.ErrorResponse("No fue posible actualizar el parametro.");

            }
            LogBitacora(codEmpresa, usuario, codParametro, MovModifica);


            return DbHelper.CreateOkResponse();
        }


    }
}

