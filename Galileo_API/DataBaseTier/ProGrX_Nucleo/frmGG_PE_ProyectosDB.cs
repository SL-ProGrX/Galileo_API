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
    public class FrmGgPeProyectosDB
    {
        private readonly PortalDB _portalDB;

        public FrmGgPeProyectosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public ErrorDto<PeProyectosLista> PeProyectoLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<PeProyectosFiltros>(Jfiltros) ?? new PeProyectosFiltros();

            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                var hasFilter = TryAddProyectosFiltro(filtros, p);

                var offset = Math.Max(0, filtros.pagina ?? 0);

                const string countNoFilter = "select COUNT(1) from PE_PROYECTOS;";
                const string countWithFilter = @"select COUNT(1)
from PE_PROYECTOS
where CAST(PROYECTO_ID AS varchar(50)) LIKE @Q
   OR TIPO LIKE @Q
   OR NOMBRE LIKE @Q
   OR DESCRIPCION LIKE @Q
   OR RESPONSABLE LIKE @Q;";

                var total = connection.ExecuteScalar<int>(hasFilter ? countWithFilter : countNoFilter, p);

                var sb = new StringBuilder();
                sb.Append(@"select [PROYECTO_ID]
                          ,[PROGRAMA_ID]
                          ,[TIPO]
                          ,[NOMBRE]
                          ,[DESCRIPCION]
                          ,[RESPONSABLE]
                          ,[PRESUPUESTO]
                          ,[FECHA_INICIO]
                          ,[FECHA_FINALIZA]
                          ,[ACTIVO]
                          ,[REGISTRO_USUARIO]
                          ,[REGISTRO_FECHA]
                          ,[MODIFICA_FECHA]
                          ,[MODIFICA_USUARIO]
                   from PE_PROYECTOS ");

                if (hasFilter)
                {
                    sb.Append(@" where CAST(PROYECTO_ID AS varchar(50)) LIKE @Q
                         OR TIPO LIKE @Q
                         OR NOMBRE LIKE @Q
                         OR DESCRIPCION LIKE @Q
                         OR RESPONSABLE LIKE @Q ");
                }

                sb.Append(" order by PROYECTO_ID desc ");

                if (filtros.pagina != null)
                {
                    var pageFetch = filtros.paginacion ?? 30;
                    if (pageFetch < 1) pageFetch = 30;

                    p.Add("@OFFSET", offset, DbType.Int32);
                    p.Add("@FETCH", pageFetch, DbType.Int32);
                    sb.Append(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY ");
                }

                var proyectos = connection.Query<PeProyectosDto>(sb.ToString(), p).ToList();

                return new PeProyectosLista
                {
                    total = total,
                    proyectos = proyectos
                };
            });
        }

        private static bool TryAddProyectosFiltro(PeProyectosFiltros filtros, DynamicParameters p)
        {
            var q = (filtros?.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return false;

            p.Add("@Q", $"%{q}%", DbType.String);
            return true;
        }

        public ErrorDto PeProyecto_Guardar(int CodEmpresa, PeProyectosDto proyectos)
        {
            if (proyectos == null)
                return DbHelper.ErrorResponse("Datos inválidos");

            return proyectos.proyecto_id == 0
                ? PeProyecto_Insertar(CodEmpresa, proyectos)
                : PeProyecto_Actualizar(CodEmpresa, proyectos);
        }

        private static int ToActivoInt(bool activo) => activo ? 1 : 0;

        private static DynamicParameters BuildProyectoParams(PeProyectosDto proyectos)
        {
            var p = new DynamicParameters();

            p.Add("@proyecto_id", proyectos.proyecto_id, DbType.Int32);
            p.Add("@programa_id", proyectos.programa_id, DbType.Int32);
            p.Add("@tipo", proyectos.tipo, DbType.String);
            p.Add("@nombre", proyectos.nombre, DbType.String);
            p.Add("@descripcion", proyectos.descripcion, DbType.String);
            p.Add("@responsable", proyectos.responsable, DbType.String);
            p.Add("@presupuesto", proyectos.presupuesto, DbType.Decimal);
            p.Add("@fecha_inicio", proyectos.fecha_inicio, DbType.DateTime);
            p.Add("@fecha_finaliza", proyectos.fecha_finaliza, DbType.DateTime);
            p.Add("@activo", ToActivoInt(proyectos.activo), DbType.Int32);

            return p;
        }

        private static ErrorDto ToOkWithId(int id)
            => new ErrorDto { Code = 0, Description = id.ToString() };

        private ErrorDto PeProyecto_Insertar(int CodEmpresa, PeProyectosDto proyectos)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                const string queryID = "SELECT ISNULL(MAX(PROYECTO_ID),0) + 1 FROM PE_PROYECTOS";
                var secuencia = connection.Query<int>(queryID).FirstOrDefault();

                proyectos.proyecto_id = secuencia;
                proyectos.programa_id = proyectos.programa_id == 0 ? proyectos.proyecto_id : proyectos.programa_id;

                const string insertSql = @"INSERT INTO PE_PROYECTOS
                            ([PROYECTO_ID]
                            ,[PROGRAMA_ID]
                            ,[TIPO]
                            ,[NOMBRE]
                            ,[DESCRIPCION]
                            ,[RESPONSABLE]
                            ,[PRESUPUESTO]
                            ,[FECHA_INICIO]
                            ,[FECHA_FINALIZA]
                            ,[ACTIVO]
                            ,[REGISTRO_USUARIO]
                            ,[REGISTRO_FECHA])
                            VALUES
                            (@proyecto_id
                            ,@programa_id
                            ,@tipo
                            ,@nombre
                            ,@descripcion
                            ,@responsable
                            ,@presupuesto
                            ,@fecha_inicio
                            ,@fecha_finaliza
                            ,@activo
                            ,@registro_usuario
                            ,GetDate())";

                var p = BuildProyectoParams(proyectos);
                p.Add("@registro_usuario", proyectos.registro_usuario, DbType.String);

                connection.Execute(insertSql, p);
                return ToOkWithId(secuencia);
            }).Result ?? DbHelper.ErrorResponse("Error inesperado");
        }

        private ErrorDto PeProyecto_Actualizar(int CodEmpresa, PeProyectosDto proyectos)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                const string updateSql = @"UPDATE PE_PROYECTOS
                            SET [PROGRAMA_ID] = @programa_id
                            ,[TIPO] = @tipo
                            ,[NOMBRE] = @nombre
                            ,[DESCRIPCION] = @descripcion
                            ,[RESPONSABLE] = @responsable
                            ,[PRESUPUESTO] = @presupuesto
                            ,[FECHA_INICIO] = @fecha_inicio
                            ,[FECHA_FINALIZA] = @fecha_finaliza
                            ,[ACTIVO] = @activo
                            ,[MODIFICA_USUARIO] = @modifica_usuario
                            ,[MODIFICA_FECHA] = GetDate()
                            WHERE PROYECTO_ID = @proyecto_id";

                var p = BuildProyectoParams(proyectos);
                p.Add("@modifica_usuario", proyectos.modifica_usuario, DbType.String);

                connection.Execute(updateSql, p);
                return ToOkWithId(proyectos.proyecto_id);
            }).Result ?? DbHelper.ErrorResponse("Error inesperado");
        }

        public ErrorDto PeProyecto_Eliminar(int CodEmpresa, int proyecto_id)
        {
            var exec = DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                const string depSql = @"SELECT COUNT(*) FROM PE_PROYECTOS_OBJETIVOS WHERE PROYECTO_ID = @proyecto_id";
                var dependencias = connection.Query<int>(depSql, new { proyecto_id }).FirstOrDefault();

                if (dependencias > 0)
                    return new ErrorDto { Code = -1, Description = "No se puede eliminar el proyecto, tiene dependencias con objetivos" };

                const string delSql = @"DELETE FROM PE_PROYECTOS WHERE PROYECTO_ID = @proyecto_id";
                connection.Execute(delSql, new { proyecto_id });
                return DbHelper.CreateOkResponse();
            });

            if ((exec.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(exec.Description ?? "Error", exec.Code ?? -1);

            return exec.Result ?? DbHelper.CreateOkResponse();
        }

        public ErrorDto<List<PeProyectoObjetivosLista>> PeObservacionesProyectos_Obtener(int CodEmpresa, int proyecto_id)
        {
            const string query = @"SELECT 
                                O.OBJETIVO_ID, 
                                O.NOMBRE AS OBJETIVO, 
                                PR.DESCRIPCION AS PERSPECTIVA, 
                                PL.DESCRIPCION AS 'PLAN',
                                PO.REGISTRO_USUARIO,
                                CASE 
                                        WHEN PO.REGISTRO_USUARIO IS NULL THEN 0 
                                        ELSE 1 
                                    END AS asignado
                                FROM PE_OBJETIVOS O
                                LEFT JOIN PE_PERSPECTIVAS PR ON PR.PERSPECTIVA_ID = O.PERSPECTIVA_ID
                                LEFT JOIN PE_PLANES PL ON PL.PE_ID = PR.PE_ID
                                LEFT JOIN PE_PROYECTOS_OBJETIVOS PO ON PO.OBJETIVO_ID = O.OBJETIVO_ID AND PO.PROYECTO_ID = @proyecto_id
                              order by PO.REGISTRO_USUARIO desc ";

            return DbHelper.ExecuteListQuery<PeProyectoObjetivosLista>(_portalDB, CodEmpresa, query, new { proyecto_id });
        }

        public ErrorDto PeObjetivoProyecto_Asociar(int CodEmpresa, int proyecto_id, int objetivo_id, string usuario)
        {
            const string query = @"INSERT INTO PE_PROYECTOS_OBJETIVOS
                            ([PROYECTO_ID]
                            ,[OBJETIVO_ID]
                            ,[REGISTRO_USUARIO]
                            ,[REGISTRO_FECHA])
                            VALUES
                            (@proyecto_id
                            ,@objetivo_id
                            ,@usuario
                            ,GetDate())";

            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, new { proyecto_id, objetivo_id, usuario });
        }

        public ErrorDto<List<PeProyectoObjetivosExportar>> PeProyectoObj_Exportar(int CodEmpresa)
        {
            const string query = @"SELECT 
	                                    T.Proyecto_ID,
	                                    T.Proyecto,
	                                    T.Programa_ID,
	                                    T.Programa,
	                                    T.Tipo,
	                                    T.DESCRIPCION,
	                                    (T.PRESUPUESTO / (SELECT CASE 
                                            WHEN COUNT(*) = 0 THEN 1 
                                            ELSE COUNT(*) 
                                        END AS resultado FROM PE_PROYECTOS_OBJETIVOS PO WHERE PO.PROYECTO_ID = T.Proyecto_ID))AS PRESUPUESTO,
	                                    T.FECHA_INICIO,
	                                    T.FECHA_FINALIZA,
	                                    T.ACTIVO,
	                                    T.Objetivo,
	                                    T.[Descripcion_Objetivo]
                                    FROM (
                                    SELECT 
                                    A.PROYECTO_ID AS 'Proyecto_ID', 
                                    A.NOMBRE AS 'Proyecto', 
                                    A.PROGRAMA_ID AS 'Programa_ID', 
                                    (SELECT B.NOMBRE FROM PE_PROYECTOS B WHERE B.PROYECTO_ID = A.PROGRAMA_ID) AS 'Programa',
                                    A.TIPO, 
                                    A.DESCRIPCION,
                                    A.PRESUPUESTO, 
                                    A.FECHA_INICIO, 
                                    A.FECHA_FINALIZA, 
                                    A.ACTIVO
                                    ,O.NOMBRE AS 'Objetivo' 
                                    ,O.DESCRIPCION AS 'Descripcion_Objetivo'
                                    FROM PE_PROYECTOS A
                                    LEFT JOIN PE_PROYECTOS_OBJETIVOS PO ON PO.PROYECTO_ID = A.PROYECTO_ID AND PO.PROYECTO_ID = A.PROYECTO_ID
                                    LEFT JOIN PE_OBJETIVOS O ON O.OBJETIVO_ID = PO.OBJETIVO_ID 
                                    )T";

            return DbHelper.ExecuteListQuery<PeProyectoObjetivosExportar>(_portalDB, CodEmpresa, query);
        }
    }
}