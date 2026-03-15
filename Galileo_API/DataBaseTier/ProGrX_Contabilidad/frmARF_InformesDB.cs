using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ARF
{
    public class FrmArfInformesDb
    {
        private readonly PortalDB _portalDb;

        public FrmArfInformesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }


        private ErrorDto<T> Ejecutar<T>(int codEmpresa, Func<SqlConnection, T> accion)
        {
            var response = new ErrorDto<T>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = accion(cn);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ARF_Unidades_Listar(int codEmpresa)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(

                                @"SELECT
                                COD_LOCAL AS item,
                                Descripcion AS descripcion
                                FROM ARF_UNIDADES
                                ORDER BY COD_LOCAL"

                ).ToList();
            });
        }


        /// <summary>
        /// Obtiene arrendadores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ARF_Arrendadores_Listar(int codEmpresa)
        {
            return Ejecutar(codEmpresa, cn =>
            {
                return cn.Query<DropDownListaGenericaModel>(

                            @"SELECT
                            COD_ACREEDOR AS item,
                            Descripcion AS descripcion
                            FROM ARF_ACREEDORES
                            ORDER BY COD_ACREEDOR"

                ).ToList();
            });
        }

    }
}