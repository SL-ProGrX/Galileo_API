using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.GG_PE;

namespace Galileo.DataBaseTier
{
    public class FrmGgPeObjetivosDb
    {
        private readonly IConfiguration _config;

        public FrmGgPeObjetivosDb(IConfiguration config)
        {
            _config = config;
        }

        private string GetConnString(int codEmpresa)
            => new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

        private static ErrorDto<T> Ok<T>(T result, string description = "Ok")
            => new() { Code = 0, Description = description, Result = result };

        private static ErrorDto Fail(Exception ex, int code = -1)
            => new() { Code = code, Description = ex.Message };

        private static ErrorDto<T> Fail<T>(Exception ex, T? result = default, int code = -1)
            => new() { Code = code, Description = ex.Message, Result = result };

        private ErrorDto<T> WithConn<T>(int codEmpresa, Func<SqlConnection, T> work, Func<T> empty)
        {
            try
            {
                using var connection = new SqlConnection(GetConnString(codEmpresa));
                return Ok(work(connection));
            }
            catch (Exception ex)
            {
                return Fail(ex, empty());
            }
        }


        public ErrorDto<PeObjetivosEstrategicosDatosLista> PeObjetivosEstrategicosLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            PeObjetivosEstrategicosFiltros filtros = JsonConvert.DeserializeObject<PeObjetivosEstrategicosFiltros>(Jfiltros)
                ?? new PeObjetivosEstrategicosFiltros();

            return WithConn(
                CodEmpresa,
                connection =>
                {
                    var p = new DynamicParameters();
                    bool hasFilter = TryAddObjetivosFiltro(filtros, p);

                    int offset = filtros.pagina ?? 0;
                    if (offset < 0) offset = 0;

                    const string countNoFilter = "SELECT COUNT(1) FROM PE_OBJETIVOS O;";
                    const string countWithFilter = @"SELECT COUNT(1)
FROM PE_OBJETIVOS O
WHERE (
     CAST(O.objetivo_id AS varchar(50)) LIKE @Q
  OR O.DESCRIPCION LIKE @Q
  OR O.nombre LIKE @Q
  OR O.indicador_clave LIKE @Q
  OR O.meta LIKE @Q
  OR O.unidad_medida LIKE @Q
);";

                    var result = new PeObjetivosEstrategicosDatosLista
                    {
                        total = connection.ExecuteScalar<int>(hasFilter ? countWithFilter : countNoFilter, p),
                        data = new List<PeObjetivosEstrategicosDto>()
                    };

                    var sb = new StringBuilder();
                    sb.Append(@"SELECT  O.[OBJETIVO_ID]
                          ,O.[PERSPECTIVA_ID]
                          ,P.DESCRIPCION AS nombre_pespectiva
                          ,O.[NOMBRE]
                          ,O.[DESCRIPCION]
                          ,O.[INDICADOR_CLAVE]
                          ,O.[META]
                          ,O.[UNIDAD_MEDIDA]
                          ,O.[ACTIVO]
                          ,O.[REGISTRO_USUARIO]
                          ,O.[REGISTRO_FECHA]
                          ,O.[MODIFICA_FECHA]
                          ,O.[MODIFICA_USUARIO]
                    FROM PE_OBJETIVOS O
                    LEFT JOIN PE_PERSPECTIVAS P ON P.PERSPECTIVA_ID = O.PERSPECTIVA_ID ");

                    if (hasFilter)
                    {
                        sb.Append(@" WHERE (
                 CAST(O.objetivo_id AS varchar(50)) LIKE @Q
              OR O.DESCRIPCION LIKE @Q
              OR O.nombre LIKE @Q
              OR O.indicador_clave LIKE @Q
              OR O.meta LIKE @Q
              OR O.unidad_medida LIKE @Q
            ) ");
                    }

                    sb.Append(" ORDER BY O.objetivo_id DESC ");

                    // Paginación opcional (solo si viene pagina)
                    if (filtros.pagina != null)
                    {
                        int pageFetch = filtros.paginacion ?? 30;
                        if (pageFetch < 1) pageFetch = 30;
                        p.Add("@OFFSET", offset);
                        p.Add("@FETCH", pageFetch);
                        sb.Append(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY ");
                    }

                    result.data = connection.Query<PeObjetivosEstrategicosDto>(sb.ToString(), p).ToList();
                    return result;
                },
                () => new PeObjetivosEstrategicosDatosLista { total = 0, data = new List<PeObjetivosEstrategicosDto>() }
            );
        }

        private static bool TryAddObjetivosFiltro(PeObjetivosEstrategicosFiltros filtros, DynamicParameters p)
        {
            string q = (filtros?.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return false;

            p.Add("@Q", $"%{q}%");
            return true;
        }

        public ErrorDto ObjetivosEstrategicos_Guardar(int CodEmpresa, PeObjetivosEstrategicosDto objetivo)
        {
            try
            {
                return objetivo.objetivo_id == 0
                    ? ObjetivosEstrategicos_Insertar(CodEmpresa, objetivo)
                    : ObjetivosEstrategicos_Actualizar(CodEmpresa, objetivo);
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }

        private ErrorDto ObjetivosEstrategicos_Insertar(int CodEmpresa, PeObjetivosEstrategicosDto objetivo)
        {
            try
            {
                using var connection = new SqlConnection(GetConnString(CodEmpresa));

                const string queryID = "SELECT ISNULL(MAX(OBJETIVO_ID),0) + 1 FROM PE_OBJETIVOS";
                var secuencia = connection.Query<int>(queryID).FirstOrDefault();
                objetivo.objetivo_id = secuencia;

                int activa = objetivo.activo ? 1 : 0;

                const string insert = @"INSERT INTO PE_OBJETIVOS
(OBJETIVO_ID, PERSPECTIVA_ID, NOMBRE, DESCRIPCION, INDICADOR_CLAVE, META, UNIDAD_MEDIDA, ACTIVO, REGISTRO_USUARIO, REGISTRO_FECHA)
VALUES
(@objetivo_id, @perspectiva_id, @nombre, @descripcion, @indicador_clave, @meta, @unidad_medida, @activa, @registro_usuario, GetDate());";

                var p = new
                {
                    objetivo_id = objetivo.objetivo_id,
                    perspectiva_id = objetivo.perspectiva_id,
                    nombre = objetivo.nombre,
                    descripcion = objetivo.descripcion,
                    indicador_clave = objetivo.indicador_clave,
                    meta = objetivo.meta,
                    unidad_medida = objetivo.unidad_medida,
                    activa,
                    registro_usuario = objetivo.registro_usuario
                };

                var rows = connection.Execute(insert, p);
                return new ErrorDto { Code = rows, Description = secuencia.ToString() };
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }

        private ErrorDto ObjetivosEstrategicos_Actualizar(int CodEmpresa, PeObjetivosEstrategicosDto objetivo)
        {
            try
            {
                using var connection = new SqlConnection(GetConnString(CodEmpresa));
                int activa = objetivo.activo ? 1 : 0;

                const string update = @"UPDATE PE_OBJETIVOS
SET PERSPECTIVA_ID = @perspectiva_id,
    NOMBRE = @nombre,
    DESCRIPCION = @descripcion,
    INDICADOR_CLAVE = @indicador_clave,
    META = @meta,
    UNIDAD_MEDIDA = @unidad_medida,
    ACTIVO = @activa,
    MODIFICA_USUARIO = @modifica_usuario,
    MODIFICA_FECHA = GetDate()
WHERE OBJETIVO_ID = @objetivo_id;";

                var p = new
                {
                    objetivo_id = objetivo.objetivo_id,
                    perspectiva_id = objetivo.perspectiva_id,
                    nombre = objetivo.nombre,
                    descripcion = objetivo.descripcion,
                    indicador_clave = objetivo.indicador_clave,
                    meta = objetivo.meta,
                    unidad_medida = objetivo.unidad_medida,
                    activa,
                    modifica_usuario = objetivo.modifica_usuario
                };

                var rows = connection.Execute(update, p);
                return new ErrorDto { Code = rows, Description = objetivo.objetivo_id.ToString() };
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }

        public ErrorDto ObjetivosEstrategicos_Eliminar(int CodEmpresa, int objetivo_id)
        {
            try
            {
                using var connection = new SqlConnection(GetConnString(CodEmpresa));

                // busco si se encuentra en proyectos_objetivos
                const string qProyectos = "SELECT COUNT(1) FROM PE_PROYECTOS_OBJETIVOS WHERE OBJETIVO_ID = @objetivo_id";
                var count = connection.ExecuteScalar<int>(qProyectos, new { objetivo_id });

                if (count > 0)
                    return new ErrorDto { Code = -2, Description = "No se puede eliminar el objetivo, ya que se encuentra asociado a un proyecto" };

                // Busco si se encuentra en KPIS
                const string qKpis = "SELECT COUNT(1) FROM PE_KPIS WHERE OBJETIVO_ID = @objetivo_id";
                count = connection.ExecuteScalar<int>(qKpis, new { objetivo_id });

                if (count > 0)
                    return new ErrorDto { Code = -2, Description = "No se puede eliminar el objetivo, ya que se encuentra asociado a un KPI" };

                const string delete = "DELETE FROM PE_OBJETIVOS WHERE OBJETIVO_ID = @objetivo_id";
                var rows = connection.Execute(delete, new { objetivo_id });
                return new ErrorDto { Code = rows, Description = "Ok" };
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }

        public ErrorDto<List<PeObjetivosEstrategicosDto>> PePerspectivaLista_Obtener(int CodEmpresa)
        {
            return WithConn(
                CodEmpresa,
                connection =>
                {
                    const string query = @"SELECT PERSPECTIVA_ID, DESCRIPCION AS nombre_pespectiva
FROM PE_PERSPECTIVAS
WHERE ACTIVA = 1;";
                    return connection.Query<PeObjetivosEstrategicosDto>(query).ToList();
                },
                () => new List<PeObjetivosEstrategicosDto>()
            );
        }

        public ErrorDto<List<PeObjetivosEstrategicosDto>> PeObservacionesExportar_Obtener(int CodEmpresa)
        {
            return WithConn(
                CodEmpresa,
                connection =>
                {
                    const string query = @"select  O.[OBJETIVO_ID]
                  ,O.[PERSPECTIVA_ID]
                  ,P.DESCRIPCION AS nombre_pespectiva
                  ,O.[NOMBRE]
                  ,O.[DESCRIPCION]
                  ,O.[INDICADOR_CLAVE]
                  ,O.[META]
                  ,O.[UNIDAD_MEDIDA]
                  ,O.[ACTIVO]
                  ,O.[REGISTRO_USUARIO]
                  ,O.[REGISTRO_FECHA]
                  ,O.[MODIFICA_FECHA]
                  ,O.[MODIFICA_USUARIO]
            from PE_OBJETIVOS O
            left join PE_PERSPECTIVAS P ON P.PERSPECTIVA_ID = O.PERSPECTIVA_ID
            order by O.objetivo_id desc;";
                    return connection.Query<PeObjetivosEstrategicosDto>(query).ToList();
                },
                () => new List<PeObjetivosEstrategicosDto>()
            );
        }
    }
}