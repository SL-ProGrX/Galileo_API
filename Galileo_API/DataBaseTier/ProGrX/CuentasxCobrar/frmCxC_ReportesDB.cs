using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_CxC
{
    public class FrmCxCReportesDb
    {
        private readonly PortalDB _portalDb;

        public FrmCxCReportesDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCxCReportesDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Lista los clientes
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Clientes_Listar(int codEmpresa)
        {
            return ObtenerPersonas(codEmpresa);
        }

        /// <summary>
        /// Lista los pagadores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Pagadores_Listar(int codEmpresa)
        {
            return ObtenerPersonas(codEmpresa);
        }

        /// <summary>
        /// Lista los conceptos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Conceptos_Listar(int codEmpresa)
        {
            const string sql = @"
                SELECT 
                    RTRIM(cod_concepto) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CxC_Conceptos
                ORDER BY descripcion
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql
            );
        }

        /// <summary>
        /// Lista los cargos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Cargos_Listar(int codEmpresa)
        {
            const string sql = @"
                SELECT 
                    RTRIM(cod_cargo) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CxC_Cargos
                ORDER BY descripcion
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql
            );
        }

        private ErrorDto<List<DropDownListaGenericaModel>> ObtenerPersonas(int codEmpresa)
        {
            const string sql = @"
                SELECT 
                    RTRIM(cedula) AS item,
                    RTRIM(nombre) AS descripcion
                FROM CxC_Personas
                ORDER BY nombre
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql
            );
        }
    }
}