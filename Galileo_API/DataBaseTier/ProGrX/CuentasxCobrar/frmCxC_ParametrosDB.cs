
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
            CxCParametros_Cargar(codEmpresa);

            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var response = new CxCParametrosLista();


                var offset = filtros.pagina!;
                var fetch = filtros.paginacion!;
                var usarPaginacion = fetch > 0 && !esExportar;

                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                var sortField = (filtros.sortField ?? string.Empty).Trim();
                var orderByField = sortField switch
                {
                    "cod_parametro" => "cod_parametro",
                    "descripcion" => "descripcion",
                    "valor" => "valor",
                    _ => "cod_parametro"
                };
                var direction = filtros?.sortOrder == 1 ? "DESC" : "ASC";


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
                    ORDER BY {orderByField} {direction}";



                var sqlCount = $@"
                    SELECT COUNT(cod_parametro)
                    FROM CxC_Parametros {where}";

                if (usarPaginacion)
                {
                    sqlList += @"
                        OFFSET @offset ROWS
                        FETCH NEXT @fetch ROWS ONLY;";
                }


                var @params = new
                {
                    filtro = hasFiltro ? texto : null,
                    like,
                    offset,
                    fetch
                };

                response.total = conn.QuerySingle<int>(sqlCount, @params);


                response.lista = conn.Query<CxCParametrosData>(sqlList, @params).ToList();


                if (response.lista != null)
                {

                    foreach (var item in response.lista)
                    {
                        if (item.Tipo == "CTA")
                        {
                            item.cuentaMasck = string.IsNullOrWhiteSpace(item.Valor)
                                ? null
                                : mCntLink.fxgCntCuentaFormato(codEmpresa, blnMascara: true, pCuenta: item.Valor, optMensaje: 1);

                            item.cuentaDetalle = string.IsNullOrWhiteSpace(item.Valor)
                               ? null
                               : mCntLink.fxgCntCuentaDesc(codContabilidad, pCuenta: item.Valor);
                        }
                    }

                }
                return DbHelper.CreateOkResponse(response);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCParametrosLista>(ex.Message);
            }

        }
      
        /// <summary>
        /// Metodo encargo de ejecutar proceso de carga de parametros iniciales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        private ErrorDto CxCParametros_Cargar(int CodEmpresa)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var query = $@"exec spCxC_Parametros";
                conn.Execute(query);

                return DbHelper.OkResponse("Parametros cargados");
            }
            catch (Exception ex)
            {

                return DbHelper.ErrorResponse(ex.Message);
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

