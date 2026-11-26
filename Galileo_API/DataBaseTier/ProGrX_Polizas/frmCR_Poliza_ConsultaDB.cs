using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCRPolizaConsultaDB
    {
        private const string OrdenCedula = "cedula";
        private const string OrdenCedular = "cedular";
        private const string OrdenNombre = "nombre";

        private readonly PortalDB _portalDb;

        public FrmCRPolizaConsultaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta la lista completa de personas de póliza para consumidores existentes.
        /// </summary>
        public ErrorDto<List<PolizaPersonaFiltroDto>> Poliza_Persona_Filtros_Lista(
            int codEmpresa,
            PolizaPersonaFiltroParams param)
        {
            if (param == null)
                return DbHelper.CreateErrorResponse<List<PolizaPersonaFiltroDto>>(
                    "Debe indicar los filtros de búsqueda.", -2);

            var orden = param.Orden?.Trim().ToLowerInvariant() == OrdenCedula
                ? OrdenCedula
                : OrdenNombre;

            const string sql = @"
                SELECT cedula, cedular, nombre
                FROM dbo.vPoliza_Persona_Filtros
                ORDER BY
                    CASE WHEN @Orden = 'cedula' THEN cedula END ASC,
                    CASE WHEN @Orden = 'nombre' THEN nombre END ASC,
                    cedula ASC;";

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var personas = connection.Query<PolizaPersonaFiltroDto>(
                    sql,
                    new { Orden = orden });

                return DbHelper.CreateOkResponse(personas.AsList());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<PolizaPersonaFiltroDto>>(
                    $"Error al consultar personas: {ex.Message}");
            }
        }

        /// <summary>
        /// Consulta una página filtrada de personas para el diálogo de búsqueda.
        /// </summary>
        public ErrorDto<PolizaPersonaFiltroPaginaResponse> Poliza_Persona_Filtros_Pagina(
            int codEmpresa,
            PolizaPersonaFiltroPaginaRequest request)
        {
            if (request == null)
                return DbHelper.CreateErrorResponse<PolizaPersonaFiltroPaginaResponse>(
                    "Debe indicar los filtros de búsqueda.", -2);

            if (request.Inicio < 0 || request.Cantidad < 1 || request.Cantidad > 100)
                return DbHelper.CreateErrorResponse<PolizaPersonaFiltroPaginaResponse>(
                    "Página o cantidad de registros inválida.", -2);

            var ordenSolicitado = request.Orden?.Trim().ToLowerInvariant();
            var orden = ordenSolicitado is OrdenCedula or OrdenCedular
                ? ordenSolicitado
                : OrdenNombre;
            var filtro = request.Filtro?.Trim() ?? string.Empty;

            const string sql = @"
                SELECT COUNT(*)
                FROM dbo.vPoliza_Persona_Filtros
                WHERE @Filtro = ''
                   OR CHARINDEX(@Filtro, ISNULL(cedula, '')) > 0
                   OR CHARINDEX(@Filtro, ISNULL(cedular, '')) > 0
                   OR CHARINDEX(@Filtro, ISNULL(nombre, '')) > 0;

                SELECT cedula, cedular, nombre
                FROM dbo.vPoliza_Persona_Filtros
                WHERE @Filtro = ''
                   OR CHARINDEX(@Filtro, ISNULL(cedula, '')) > 0
                   OR CHARINDEX(@Filtro, ISNULL(cedular, '')) > 0
                   OR CHARINDEX(@Filtro, ISNULL(nombre, '')) > 0
                ORDER BY
                    CASE
                        WHEN @Orden = 'cedula' AND @Descendente = 0
                        THEN cedula
                    END ASC,
                    CASE
                        WHEN @Orden = 'cedula' AND @Descendente = 1
                        THEN cedula
                    END DESC,
                    CASE
                        WHEN @Orden = 'cedular' AND @Descendente = 0
                        THEN cedular
                    END ASC,
                    CASE
                        WHEN @Orden = 'cedular' AND @Descendente = 1
                        THEN cedular
                    END DESC,
                    CASE
                        WHEN @Orden = 'nombre' AND @Descendente = 0
                        THEN nombre
                    END ASC,
                    CASE
                        WHEN @Orden = 'nombre' AND @Descendente = 1
                        THEN nombre
                    END DESC,
                    cedula ASC
                OFFSET @Inicio ROWS
                FETCH NEXT @Cantidad ROWS ONLY;";

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                using var results = connection.QueryMultiple(sql, new
                {
                    Filtro = filtro,
                    Orden = orden,
                    request.Descendente,
                    request.Inicio,
                    request.Cantidad
                });

                var total = results.ReadSingle<int>();
                var personas = results.Read<PolizaPersonaFiltroDto>().AsList();

                return DbHelper.CreateOkResponse(
                    new PolizaPersonaFiltroPaginaResponse
                    {
                        Value = personas,
                        Total = total
                    });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaPersonaFiltroPaginaResponse>(
                    $"Error al buscar personas: {ex.Message}");
            }
        }

        /// <summary>
        /// Consulta las operaciones de crédito de una persona por cédula.
        /// </summary>
        public ErrorDto<List<PolizaPersonaOperacionBaseDto>> Poliza_Persona_Creditos(
            int codEmpresa,
            PolizaPersonaCreditoParams param)
        {
            var sql = "spPoliza_Persona_Creditos";
            var parameters = new { param.Cedula };

            using var conn = _portalDb.CreateConnection(codEmpresa);
            var result = conn.Query<PolizaPersonaOperacionBaseDto>(
                sql,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return DbHelper.CreateOkResponse(result.AsList());
        }

        /// <summary>
        /// Consulta las pólizas de la persona por operación.
        /// </summary>
        public ErrorDto<List<PolizaPersonaOperacionPolizaDto>>
            Poliza_Persona_Operaciones_Polizas(
                int codEmpresa,
                PolizaPersonaOperacionPolizaParams param)
        {
            var sql = "spPoliza_Persona_Operaciones_Polizas";
            var parameters = new { param.Operacion };

            using var conn = _portalDb.CreateConnection(codEmpresa);
            var result = conn.Query<PolizaPersonaOperacionPolizaDto>(
                sql,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return DbHelper.CreateOkResponse(result.AsList());
        }

        /// <summary>
        /// Consulta los reclamos de una operación y póliza.
        /// </summary>
        public ErrorDto<List<PolizaPersonaReclamoDto>> Poliza_Persona_Reclamos(
            int codEmpresa,
            PolizaPersonaReclamoParams param)
        {
            var sql = "spPoliza_Persona_Reclamos";
            var parameters = new { param.Operacion, param.OperacionPoliza };

            using var conn = _portalDb.CreateConnection(codEmpresa);
            var result = conn.Query<PolizaPersonaReclamoDto>(
                sql,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return DbHelper.CreateOkResponse(result.AsList());
        }

        /// <summary>
        /// Consulta todos los reclamos de una persona.
        /// VB6: sbReclamos_Todos_Load.
        /// </summary>
        public ErrorDto<List<PolizaPersonaReclamoDto>>
            Poliza_Persona_Reclamos_Todos(
                int codEmpresa,
                PolizaPersonaCreditoParams param)
        {
            var sql = "spPoliza_Persona_Reclamos_Todos";
            var parameters = new { param.Cedula };

            using var conn = _portalDb.CreateConnection(codEmpresa);
            var result = conn.Query<PolizaPersonaReclamoDto>(
                sql,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return DbHelper.CreateOkResponse(result.AsList());
        }
    }
}