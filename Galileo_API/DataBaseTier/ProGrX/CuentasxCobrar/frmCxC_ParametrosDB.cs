
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
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



        private const string SelectColumns =
            "cod_parametro, descripcion, valor, notas, tipo, inicio_fecha, visible, modifica_usuario, modifica_fecha";

        // Consulta LISTA totalmente estática (sin interpolación)
        private const string SqlList = @"
            SELECT " + SelectColumns + @"
            FROM CxC_Parametros
            WHERE (@like IS NULL)
               OR (cod_parametro LIKE @like)
               OR (descripcion  LIKE @like)
               OR (valor        LIKE @like)
            ORDER BY
                CASE WHEN @orderBy = 'cod_parametro' AND @desc = 0 THEN cod_parametro END ASC,
                CASE WHEN @orderBy = 'cod_parametro' AND @desc = 1 THEN cod_parametro END DESC,
                CASE WHEN @orderBy = 'descripcion'   AND @desc = 0 THEN descripcion   END ASC,
                CASE WHEN @orderBy = 'descripcion'   AND @desc = 1 THEN descripcion   END DESC,
                CASE WHEN @orderBy = 'valor'         AND @desc = 0 THEN valor         END ASC,
                CASE WHEN @orderBy = 'valor'         AND @desc = 1 THEN valor         END DESC
            OFFSET @offset ROWS
            FETCH NEXT @fetch ROWS ONLY;";

        // Consulta COUNT totalmente estática (sin interpolación)
        private const string SqlCount = @"
            SELECT COUNT(cod_parametro)
            FROM CxC_Parametros
            WHERE (@like IS NULL)
               OR (cod_parametro LIKE @like)
               OR (descripcion  LIKE @like)
               OR (valor        LIKE @like);";




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
             


                var like = BuildLike(filtros.filtro);
                var (orderBy, isDesc) = BuildOrder(filtros.sortField, filtros.sortOrder);

                var offset = Math.Max(filtros.pagina, 0);
                var fetch = Math.Max(filtros.paginacion, 0);
                var usarPaginacion = !esExportar && fetch > 0;

                var effectiveOffset = usarPaginacion ? offset : 0;
                var effectiveFetch = usarPaginacion ? fetch : int.MaxValue;


                var @params = new
                {
                    like,
                    orderBy,              
                    desc = isDesc ? 1 : 0, 
                    offset = effectiveOffset,
                    fetch = effectiveFetch
                };



                var total = conn.QuerySingle<int>(SqlCount, @params);
                var lista = conn.Query<CxCParametrosData>(SqlList, @params).ToList();



                EnriquecerCuentas(lista, codEmpresa, codContabilidad);

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

        #region Helpers privados (reducen la complejidad del método principal)


        private static string? BuildLike(string? texto)
            => string.IsNullOrWhiteSpace(texto) ? null : "%" + texto.Trim() + "%";

        private static (string orderBy, bool isDesc) BuildOrder(string? sortField, int sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim();
            var orderBy = OrderableColumns.TryGetValue(field, out var col) ? col : "cod_parametro";
            var isDesc = sortOrder == SortDescendingValue;
            return (orderBy, isDesc);
        }

        private void EnriquecerCuentas(
            IEnumerable<CxCParametrosData> items,
            int codEmpresa,
            int codContabilidad)
        { 
            ArgumentNullException.ThrowIfNull(mCntLink);

            foreach (var item in items)
            {
                if (!string.Equals(item.Tipo, "CTA", StringComparison.OrdinalIgnoreCase))
                    continue;

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

                item.cuentaDetalle = mCntLink.fxgCntCuentaDesc(codEmpresa,item.Valor,codContabilidad); 
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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var query = $@"exec spCxC_Parametros";
            conn.Execute(query);

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

