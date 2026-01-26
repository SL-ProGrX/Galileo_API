using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.TES;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesParametrosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;

        public FrmTesParametrosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtener lista de parametros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_Parametros_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var response = new TablasListaGenericaModel();

                var pagina = Math.Max(0, filtros?.pagina ?? 0);
                var paginacion = Math.Max(1, filtros?.paginacion ?? 10);

                var rawFilter = filtros?.filtro?.Trim();
                var hasFilter = !string.IsNullOrWhiteSpace(rawFilter);

                var parameters = new Dapper.DynamicParameters();
                parameters.Add("@offset", pagina);
                parameters.Add("@take", paginacion);

                const string CountNoFilter = @"
            SELECT COUNT(cod_parametro)
            FROM tes_parametros;";

                const string SelectNoFilter = @"
            SELECT cod_parametro, descripcion, valor
            FROM tes_parametros
            ORDER BY cod_parametro
            OFFSET @offset ROWS
            FETCH NEXT @take ROWS ONLY;";

                const string CountWithFilter = @"
            SELECT COUNT(cod_parametro)
            FROM tes_parametros
            WHERE (cod_parametro LIKE @filter
                   OR descripcion LIKE @filter
                   OR valor LIKE @filter);";

                const string SelectWithFilter = @"
            SELECT cod_parametro, descripcion, valor
            FROM tes_parametros
            WHERE (cod_parametro LIKE @filter
                   OR descripcion LIKE @filter
                   OR valor LIKE @filter)
            ORDER BY cod_parametro
            OFFSET @offset ROWS
            FETCH NEXT @take ROWS ONLY;";

                if (hasFilter)
                {
                    parameters.Add("@filter", $"%{rawFilter}%");
                    response.total = conn.QueryFirstOrDefault<int>(CountWithFilter, parameters);
                    response.lista = conn.Query<TesParametrosDto>(SelectWithFilter, parameters).ToList();
                }
                else
                {
                    response.total = conn.QueryFirstOrDefault<int>(CountNoFilter);
                    response.lista = conn.Query<TesParametrosDto>(SelectNoFilter, parameters).ToList();
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
        }

        /// <summary>
        /// Actualizar valor de un parametro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ErrorDto TES_Parametros_Guardar(int CodEmpresa, string Usuario, TesParametrosDto param)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = @"update tes_parametros set valor = @valor
                    where cod_parametro = @parametro";
                conn.Execute(query, new
                {
                    parametro = param.cod_parametro,
                    param.valor
                });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario.ToUpper(),
                    DetalleMovimiento = "Parametro de Tesorería : " + param.cod_parametro,
                    Movimiento = "MODIFICA - WEB",
                    Modulo = 9
                });

                return DbHelper.OkResponse("Registro actualizado correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);   
            }
        }
    }
}