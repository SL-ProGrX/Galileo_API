
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
            
            try
            {

                CxCParametros_Cargar(codEmpresa);

                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var response = new CxCParametrosLista();



                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = !esExportar && fetch > 0;

                var like = BuildLike(filtros.filtro);
                var (orderBy, direction) = BuildOrder(filtros.sortField, filtros.sortOrder);


                //var offset = filtros.pagina!;
                //var fetch = filtros.paginacion!;
                //var usarPaginacion = fetch > 0 && !esExportar;

                //var texto = filtros.filtro?.Trim();
                //var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                //var like = hasFiltro ? $"%{texto}%" : null;

                //var sortField = (filtros.sortField ?? string.Empty).Trim();
                //var orderByField = sortField switch
                //{
                //    "cod_parametro" => "cod_parametro",
                //    "descripcion" => "descripcion",
                //    "valor" => "valor",
                //    _ => "cod_parametro"
                //};
                //var direction = filtros.sortOrder == 1 ? "DESC" : "ASC";


                const string where = @"
                    WHERE
                        (@filtro IS NULL)
                        OR (cod_parametro LIKE @like)
                        OR (descripcion LIKE @like)
                        OR (valor LIKE @like)";

                var sqlList = $@"
                    SELECT cod_parametro, descripcion, valor, notas,tipo,inicio_fecha,visible,modifica_usuario,modifica_fecha
                    FROM CxC_Parametros
                       {where}
                        ORDER BY {orderBy} {direction}{BuildPagination(usarPaginacion)};";
                //ORDER BY {orderByField} {direction}

                var @params = new { like, offset, fetch };

                var sqlCount = $@"
                    SELECT COUNT(cod_parametro)
                    FROM CxC_Parametros {where}";

                response.total = conn.QuerySingle<int>(sqlCount, @params);

             

                var lista = conn.Query<CxCParametrosData>(sqlList, @params).ToList();
                EnriquecerCuentas(lista, codEmpresa, codContabilidad);
                response.lista = lista;



                //if (response.lista != null)
                //{

                //    foreach (var item in response.lista)
                //    {
                //        if (item.Tipo == "CTA")
                //        {
                //            item.cuentaMasck = string.IsNullOrWhiteSpace(item.Valor)
                //                ? null
                //                : mCntLink.fxgCntCuentaFormato(codEmpresa, blnMascara: true, pCuenta: item.Valor, optMensaje: 1);

                //            item.cuentaDetalle = string.IsNullOrWhiteSpace(item.Valor)
                //               ? null
                //               : mCntLink.fxgCntCuentaDesc(codContabilidad, pCuenta: item.Valor);
                //        }
                //    }

                //}
                return DbHelper.CreateOkResponse(response);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCParametrosLista>(ex.Message);
            }

        }


        static string? BuildLike(string? texto)
                => string.IsNullOrWhiteSpace(texto) ? null : $"%{texto.Trim()}%";

        static (string orderBy, string direction) BuildOrder(string? sortField, int sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim();
            var orderBy = field switch
            {
                "cod_parametro" => "cod_parametro",
                "descripcion" => "descripcion",
                "valor" => "valor",
                _ => "cod_parametro"
            };
            var direction = (sortOrder == 1) ? "DESC" : "ASC";
            return (orderBy, direction);
        }


        static string BuildPagination(bool usar)
               => usar ? "\nOFFSET @offset ROWS\nFETCH NEXT @fetch ROWS ONLY" : string.Empty;


        void EnriquecerCuentas(IEnumerable<CxCParametrosData> items, int empresa, int contab)
        {
            foreach (var item in items)
            {
                // Early-continue: reduce anidación
                if (!string.Equals(item.Tipo, "CTA", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.IsNullOrWhiteSpace(item.Valor))
                {
                    item.cuentaMasck = null;
                    item.cuentaDetalle = null;
                    continue;
                }

                item.cuentaMasck = mCntLink.fxgCntCuentaFormato(
                    empresa,
                    blnMascara: true,
                    pCuenta: item.Valor,
                    optMensaje: 1);

                item.cuentaDetalle = mCntLink.fxgCntCuentaDesc(
                    contab,
                    pCuenta: item.Valor);
            }
        }
 

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

