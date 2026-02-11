using Galileo.Models.GG_PE;
using Dapper;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmGgPePlanesDB
    {
        private readonly PortalDB _portalDB;

        public FrmGgPePlanesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public ErrorDto<PePlanesDatosLista> PePlanesLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<PePlanesFiltros>(Jfiltros) ?? new PePlanesFiltros();

            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                var hasFilter = TryAddPlanesFiltro(filtros, p);

                var offset = filtros.pagina ?? 0;
                if (offset < 0) offset = 0;

                const string countNoFilter = "select COUNT(1) from PE_PLANES;";
                const string countWithFilter = @"select COUNT(1)
from PE_PLANES
where CAST(PE_ID AS varchar(50)) LIKE @Q
   OR DESCRIPCION LIKE @Q
   OR MISION LIKE @Q
   OR VISION LIKE @Q
   OR CAST(FINALIZACION AS varchar(50)) LIKE @Q
   OR CAST(INICIO AS varchar(50)) LIKE @Q;";

                var result = new PePlanesDatosLista
                {
                    total = connection.ExecuteScalar<int>(hasFilter ? countWithFilter : countNoFilter, p),
                    data = new List<PePlanesDto>()
                };

                var sb = new StringBuilder();
                sb.Append(@"select [PE_ID]
                          ,[DESCRIPCION]
                          ,[INICIO]
                          ,[FINALIZACION]
                          ,[ESTADO]
                          ,[MISION]
                          ,[VISION]
                          ,[MODIFICA_FECHA]
                          ,[MODIFICA_USUARIO]
                          ,[REGISTRO_USUARIO]
                          ,[REGISTRO_FECHA]
                   from PE_PLANES ");

                if (hasFilter)
                {
                    sb.Append(@" where CAST(PE_ID AS varchar(50)) LIKE @Q
                         OR DESCRIPCION LIKE @Q
                         OR MISION LIKE @Q
                         OR VISION LIKE @Q
                         OR CAST(FINALIZACION AS varchar(50)) LIKE @Q
                         OR CAST(INICIO AS varchar(50)) LIKE @Q ");
                }

                sb.Append(" order by PE_ID desc ");

                if (filtros.pagina != null)
                {
                    var pageFetch = filtros.paginacion ?? 30;
                    if (pageFetch < 1) pageFetch = 30;

                    p.Add("@OFFSET", offset, DbType.Int32);
                    p.Add("@FETCH", pageFetch, DbType.Int32);
                    sb.Append(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY ");
                }

                result.data = connection.Query<PePlanesDto>(sb.ToString(), p).ToList();
                return result;
            });
        }

        private static bool TryAddPlanesFiltro(PePlanesFiltros filtros, DynamicParameters p)
        {
            var q = (filtros?.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return false;

            p.Add("@Q", $"%{q}%", DbType.String);
            return true;
        }

        public ErrorDto PePlanes_Guardar(int CodEmpresa, PePlanesDto plan)
        {
            if (plan == null)
                return DbHelper.ErrorResponse("Datos inválidos");

            return plan.pe_id == 0
                ? PePlanes_Insertar(CodEmpresa, plan)
                : PePlanes_Actualizar(CodEmpresa, plan);
        }

        private sealed class WriteResult
        {
            public int Rows { get; init; }
            public int Id { get; init; }
        }

        private static DynamicParameters BuildPlanParams(PePlanesDto plan)
        {
            var p = new DynamicParameters();
            p.Add("@pe_id", plan.pe_id, DbType.Int32);
            p.Add("@descripcion", plan.descripcion, DbType.String);
            p.Add("@inicio", plan.inicio, DbType.DateTime);
            p.Add("@finalizacion", plan.finalizacion, DbType.DateTime);
            p.Add("@mision", plan.mision, DbType.String);
            p.Add("@vision", plan.vision, DbType.String);
            return p;
        }

        private const string InsertSql = @"INSERT INTO [dbo].[PE_PLANES]
       ([PE_ID]
       ,[DESCRIPCION]
       ,[INICIO]
       ,[FINALIZACION]
       ,[ESTADO]
       ,[MISION]
       ,[VISION]
       ,[REGISTRO_USUARIO]
       ,[REGISTRO_FECHA])
 VALUES
       (@pe_id
       ,@descripcion
       ,@inicio
       ,@finalizacion
       ,'A'
       ,@mision
       ,@vision
       ,@registro_usuario
       ,getDate());";

        private const string UpdateSql = @"UPDATE [dbo].[PE_PLANES]
   SET [DESCRIPCION] = @descripcion
      ,[INICIO] = @inicio
      ,[FINALIZACION] = @finalizacion
      ,[ESTADO] = @estado
      ,[MISION] = @mision
      ,[VISION] = @vision
      ,[MODIFICA_FECHA] = GetDate()
      ,[MODIFICA_USUARIO] = @modifica_usuario
 WHERE [PE_ID] = @pe_id;";

        private ErrorDto PePlanes_Insertar(int CodEmpresa, PePlanesDto plan)
        {
            var exec = DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                const string queryID = "SELECT ISNULL(MAX(PE_ID),0) + 1 FROM PE_PLANES";
                var secuencia = connection.QueryFirstOrDefault<int>(queryID);

                plan.pe_id = secuencia;

                var p = BuildPlanParams(plan);
                p.Add("@registro_usuario", plan.registro_usuario, DbType.String);

                var rows = connection.Execute(InsertSql, p);
                return new WriteResult { Rows = rows, Id = plan.pe_id };
            });

            if ((exec.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(exec.Description ?? "Error", exec.Code ?? -1);

            var wr = exec.Result ?? new WriteResult { Rows = 0, Id = 0 };
            return new ErrorDto { Code = wr.Rows, Description = wr.Id.ToString() };
        }

        private ErrorDto PePlanes_Actualizar(int CodEmpresa, PePlanesDto plan)
        {
            var exec = DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var p = BuildPlanParams(plan);
                p.Add("@estado", plan.estado, DbType.String);
                p.Add("@modifica_usuario", plan.modifica_usuario, DbType.String);

                var rows = connection.Execute(UpdateSql, p);
                return new WriteResult { Rows = rows, Id = plan.pe_id };
            });

            if ((exec.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(exec.Description ?? "Error", exec.Code ?? -1);

            var wr = exec.Result ?? new WriteResult { Rows = 0, Id = 0 };
            return new ErrorDto { Code = wr.Rows, Description = wr.Id.ToString() };
        }

        private sealed class DeleteResult
        {
            public int Code { get; init; }
            public string Description { get; init; } = "";
        }

        public ErrorDto PePlanes_Eliminar(int CodEmpresa, int pe_id)
        {
            var exec = DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                const string qCount = "SELECT COUNT(1) FROM PE_PERSPECTIVAS WHERE PE_ID = @pe_id";
                var count = connection.ExecuteScalar<int>(qCount, new { pe_id });

                if (count > 0)
                    return new DeleteResult { Code = -1, Description = "No se puede eliminar el plan, tiene perspectivas asociadas" };

                const string qDelete = "DELETE FROM [dbo].[PE_PLANES] WHERE [PE_ID] = @pe_id";
                var rows = connection.Execute(qDelete, new { pe_id });
                return new DeleteResult { Code = rows, Description = "Ok" };
            });

            if ((exec.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(exec.Description ?? "Error", exec.Code ?? -1);

            var dr = exec.Result ?? new DeleteResult { Code = 0, Description = "Ok" };
            return new ErrorDto { Code = dr.Code, Description = dr.Description };
        }

        public ErrorDto<List<PePlanesDto>> PePlanes_Exportar(int CodEmpresa)
        {
            const string query = @"select [PE_ID]
                              ,[DESCRIPCION]
                              ,[INICIO]
                              ,[FINALIZACION]
                              ,[ESTADO]
                              ,[MISION]
                              ,[VISION]
                              ,[MODIFICA_FECHA]
                              ,[MODIFICA_USUARIO]
                              ,[REGISTRO_USUARIO]
                              ,[REGISTRO_FECHA] from PE_PLANES order by PE_ID desc ";

            return DbHelper.ExecuteListQuery<PePlanesDto>(_portalDB, CodEmpresa, query);
        }
    }
}