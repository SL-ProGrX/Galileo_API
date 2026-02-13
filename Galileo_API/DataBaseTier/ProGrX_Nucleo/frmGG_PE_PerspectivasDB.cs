using Galileo.Models.GG_PE;
using Dapper;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmGgPePerspectivasDB
    {
        private readonly PortalDB _portalDB;

        public FrmGgPePerspectivasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public ErrorDto<PePerspectivasDto> PePerspectiva_Obtener(int CodEmpresa, int perspectiva)
        {
            const string query = @"select [PERSPECTIVA_ID]
                                          ,[DESCRIPCION]
                                          ,[PE_ID]
                                          ,[OBJETIVO_A_1]
                                          ,[OBJETIVO_A_2]
                                          ,[OBJETIVO_A_3]
                                          ,[RESPONSABLE]
                                          ,[ACTIVA]
                                          ,[REGISTRO_USUARIO]
                                          ,[REGISTRO_FECHA]
                                          ,[MODIFICA_FECHA]
                                          ,[MODIFICA_USUARIO]
                                   from PE_PERSPECTIVAS
                                   WHERE PERSPECTIVA_ID = @perspectiva;";

            var r = DbHelper.ExecuteSingleQuery<PePerspectivasDto>(_portalDB, CodEmpresa, query, defaultValue: null, parameters: new { perspectiva });
            if ((r.Code ?? -1) != 0)
                return new ErrorDto<PePerspectivasDto> { Code = r.Code, Description = r.Description, Result = null };

            return new ErrorDto<PePerspectivasDto> { Code = 0, Description = "Ok", Result = r.Result };
        }

        public ErrorDto<PePerspectivasDto> PePerspectiva_Scroll(int CodEmpresa, int scroll, int? perspectiva)
        {
            const string qNext = @"select top 1 *
from PE_PERSPECTIVAS
where PERSPECTIVA_ID > @perspectiva
order by PERSPECTIVA_ID asc;";

            const string qPrev = @"select top 1 *
from PE_PERSPECTIVAS
where PERSPECTIVA_ID < @perspectiva
order by PERSPECTIVA_ID desc;";

            var query = (scroll == 1) ? qNext : qPrev;
            var param = new { perspectiva = perspectiva ?? 0 };

            var r = DbHelper.ExecuteSingleQuery<PePerspectivasDto>(_portalDB, CodEmpresa, query, defaultValue: null, parameters: param);
            if ((r.Code ?? -1) != 0)
                return new ErrorDto<PePerspectivasDto> { Code = r.Code, Description = r.Description, Result = null };

            return new ErrorDto<PePerspectivasDto> { Code = 0, Description = "Ok", Result = r.Result };
        }

        public ErrorDto PePerspectiva_Guardar(int CodEmpresa, PePerspectivasDto perspectiva)
        {
            if (perspectiva == null)
                return DbHelper.ErrorResponse("Datos inválidos");

            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                //Obtengo el siguiente ID si es nuevo
                if (perspectiva.perspectiva_id == 0)
                {
                    const string nextIdSql = @"SELECT ISNULL(MAX(PERSPECTIVA_ID),0) + 1 FROM PE_PERSPECTIVAS";
                    perspectiva.perspectiva_id = connection.QueryFirstOrDefault<int>(nextIdSql);
                    return ExecuteWrite(connection, InsertSql, BuildParams(perspectiva, isInsert: true));
                }

                return ExecuteWrite(connection, UpdateSql, BuildParams(perspectiva, isInsert: false));
            }).Result ?? DbHelper.ErrorResponse("Error inesperado");
        }

        private static DynamicParameters BuildParams(PePerspectivasDto p, bool isInsert)
        {
            var dp = new DynamicParameters();

            dp.Add("@perspectiva_id", p.perspectiva_id, DbType.Int32);
            dp.Add("@descripcion", p.descripcion, DbType.String);
            dp.Add("@pe_id", p.pe_id, DbType.Int32);
            dp.Add("@objetivo_a_1", p.objetivo_a_1, DbType.String);
            dp.Add("@objetivo_a_2", p.objetivo_a_2, DbType.String);
            dp.Add("@objetivo_a_3", p.objetivo_a_3, DbType.String);
            dp.Add("@responsable", p.responsable, DbType.String);
            dp.Add("@activa", p.activa ? 1 : 0, DbType.Int32);

            if (isInsert)
                dp.Add("@registro_usuario", p.registro_usuario, DbType.String);
            else
                dp.Add("@modifica_usuario", p.modifica_usuario, DbType.String);

            return dp;
        }

        private static ErrorDto ExecuteWrite(SqlConnection connection, string sql, DynamicParameters p)
        {
            try
            {
                connection.Execute(sql, p);
                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private const string InsertSql = @"INSERT INTO [dbo].[PE_PERSPECTIVAS]
                                       ([PERSPECTIVA_ID]
                                       ,[DESCRIPCION]
                                       ,[PE_ID]
                                       ,[OBJETIVO_A_1]
                                       ,[OBJETIVO_A_2]
                                       ,[OBJETIVO_A_3]
                                       ,[RESPONSABLE]
                                       ,[ACTIVA]
                                       ,[REGISTRO_USUARIO]
                                       ,[REGISTRO_FECHA])
                                 VALUES
                                       (@perspectiva_id
                                       ,@descripcion
                                       ,@pe_id
                                       ,@objetivo_a_1
                                       ,@objetivo_a_2
                                       ,@objetivo_a_3
                                       ,@responsable
                                       ,@activa
                                       ,@registro_usuario
                                       ,GetDate());";

        private const string UpdateSql = @"UPDATE [dbo].[PE_PERSPECTIVAS]
                                       SET [DESCRIPCION] = @descripcion
                                          ,[PE_ID] = @pe_id
                                          ,[OBJETIVO_A_1] = @objetivo_a_1
                                          ,[OBJETIVO_A_2] = @objetivo_a_2
                                          ,[OBJETIVO_A_3] = @objetivo_a_3
                                          ,[RESPONSABLE] = @responsable
                                          ,[ACTIVA] = @activa
                                          ,[MODIFICA_FECHA] = GetDate()
                                          ,[MODIFICA_USUARIO] = @modifica_usuario
                                     WHERE PERSPECTIVA_ID = @perspectiva_id;";

        public ErrorDto PePerspectiva_Eliminar(int CodEmpresa, int perspectiva)
        {
            var exec = DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                //Valida si existe en la tabla de objetivos
                const string qCount = @"SELECT COUNT(1) FROM [dbo].[PE_OBJETIVOS] WHERE PERSPECTIVA_ID = @perspectiva;";
                var count = connection.ExecuteScalar<int>(qCount, new { perspectiva });
                if (count > 0)
                {
                    return DbHelper.ErrorResponse("No se puede eliminar la perspectiva, ya que tiene objetivos asociados.");
                }

                const string qDelete = @"DELETE FROM [dbo].[PE_PERSPECTIVAS] WHERE PERSPECTIVA_ID = @perspectiva;";
                connection.Execute(qDelete, new { perspectiva });
                return DbHelper.CreateOkResponse();
            });

            if ((exec.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(exec.Description ?? "Error", exec.Code ?? -1);

            return exec.Result ?? DbHelper.CreateOkResponse();
        }

        public ErrorDto<List<PePerspectivasDto>> PePlanesLista_Obtener(int CodEmpresa)
        {
            const string query = @"select [PE_ID]
                                      ,[DESCRIPCION] from PE_PLANES Where ESTADO = 'A' 
                                       AND FINALIZACION > getDate() ";

            return DbHelper.ExecuteListQuery<PePerspectivasDto>(_portalDB, CodEmpresa, query);
        }

        public ErrorDto<PePerspectivasDatosLista> PePerpectivasLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<PePerspectivasFiltros>(Jfiltros) ?? new PePerspectivasFiltros();

            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var result = new PePerspectivasDatosLista
                {
                    total = 0,
                    data = new List<PePerspectivasDto>()
                };

                var p = new DynamicParameters();
                var hasFilter = TryAddPerspectivasFiltro(filtros, p);

                var offset = filtros.pagina ?? 0;
                if (offset < 0) offset = 0;

                const string countNoFilter = "select COUNT(1) from PE_PERSPECTIVAS;";
                const string countWithFilter = @"select COUNT(1)
from PE_PERSPECTIVAS
where CAST(perspectiva_id AS varchar(50)) LIKE @Q
   OR DESCRIPCION LIKE @Q;";

                result.total = connection.ExecuteScalar<int>(hasFilter ? countWithFilter : countNoFilter, p);

                var sb = new StringBuilder();
                sb.Append(@"select [PERSPECTIVA_ID]
                              ,[DESCRIPCION]
                              ,[PE_ID]
                              ,[OBJETIVO_A_1]
                              ,[OBJETIVO_A_2]
                              ,[OBJETIVO_A_3]
                              ,[RESPONSABLE]
                              ,[ACTIVA]
                              ,[REGISTRO_USUARIO]
                              ,[REGISTRO_FECHA]
                              ,[MODIFICA_FECHA]
                              ,[MODIFICA_USUARIO]
                       from PE_PERSPECTIVAS ");

                if (hasFilter)
                {
                    sb.Append(@" where CAST(perspectiva_id AS varchar(50)) LIKE @Q
                                 OR DESCRIPCION LIKE @Q ");
                }

                sb.Append(" order by PERSPECTIVA_ID desc ");

                if (filtros.pagina != null)
                {
                    var pageFetch = filtros.paginacion ?? 30;
                    if (pageFetch < 1) pageFetch = 30;

                    p.Add("@OFFSET", offset, DbType.Int32);
                    p.Add("@FETCH", pageFetch, DbType.Int32);
                    sb.Append(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY ");
                }

                result.data = connection.Query<PePerspectivasDto>(sb.ToString(), p).ToList();
                return result;
            });
        }

        private static bool TryAddPerspectivasFiltro(PePerspectivasFiltros filtros, DynamicParameters p)
        {
            var q = (filtros?.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return false;

            p.Add("@Q", $"%{q}%", DbType.String);
            return true;
        }
    }
}