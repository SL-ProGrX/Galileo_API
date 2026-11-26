using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCRPolizaConsultaDB
    {
        private readonly PortalDB _portalDb;

        public FrmCRPolizaConsultaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta la lista de personas de póliza con orden dinámico.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetro de orden ("nombre" o "cedula").</param>
        /// <returns>Lista de personas de póliza.</returns>
        public ErrorDto<List<PolizaPersonaFiltroDto>> Poliza_Persona_Filtros_Lista(int codEmpresa, PolizaPersonaFiltroParams param)
        {
            string orderBy = param.Orden?.ToLower() == "cedula" ? "cedula" : "nombre";
            var query = $@"SELECT cedula, cedular, nombre FROM vPoliza_Persona_Filtros ORDER BY {orderBy}";
            return DbHelper.ExecuteListQuery<PolizaPersonaFiltroDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Consulta una página de personas para el diálogo de búsqueda de pólizas.
        /// </summary>
        public ErrorDto<PolizaPersonaFiltroPaginaResponse> Poliza_Persona_Filtros_Pagina(
            int codEmpresa, PolizaPersonaFiltroPaginaRequest request)
        {
            if (request == null)
                return DbHelper.CreateErrorResponse<PolizaPersonaFiltroPaginaResponse>("Debe indicar los filtros de búsqueda.", -2);

            if (request.Inicio < 0 || request.Cantidad < 1 || request.Cantidad > 100)
                return DbHelper.CreateErrorResponse<PolizaPersonaFiltroPaginaResponse>("Página o cantidad de registros inválida.", -2);

            var orden = request.Orden?.Trim().ToLowerInvariant() switch
            {
                "cedula" => "cedula",
                "cedular" => "cedular",
                _ => "nombre"
            };
            var sentido = request.Descendente ? "DESC" : "ASC";
            var filtro = request.Filtro?.Trim() ?? string.Empty;
            var ordenCompleto = orden == "cedula"
                ? $"cedula {sentido}"
                : $"{orden} {sentido}, cedula ASC";

            const string whereSql = @"WHERE @Filtro = ''
                OR CHARINDEX(@Filtro, ISNULL(cedula, '')) > 0
                OR CHARINDEX(@Filtro, ISNULL(cedular, '')) > 0
                OR CHARINDEX(@Filtro, ISNULL(nombre, '')) > 0";
            var sql = $@"SELECT COUNT(*) FROM dbo.vPoliza_Persona_Filtros {whereSql};
                SELECT cedula, cedular, nombre
                FROM dbo.vPoliza_Persona_Filtros {whereSql}
                ORDER BY {ordenCompleto}
                OFFSET @Inicio ROWS FETCH NEXT @Cantidad ROWS ONLY;";

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                using var results = connection.QueryMultiple(sql, new
                {
                    Filtro = filtro,
                    request.Inicio,
                    request.Cantidad
                });
                var total = results.ReadSingle<int>();
                var value = results.Read<PolizaPersonaFiltroDto>().AsList();
                return DbHelper.CreateOkResponse(new PolizaPersonaFiltroPaginaResponse
                {
                    Value = value,
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
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con la cédula.</param>
        /// <returns>Lista de operaciones de crédito.</returns>
        public ErrorDto<List<PolizaPersonaOperacionBaseDto>> Poliza_Persona_Creditos(int codEmpresa, PolizaPersonaCreditoParams param)
        {
            var sql = "spPoliza_Persona_Creditos";
            var parameters = new { param.Cedula };
            using var conn = _portalDb.CreateConnection(codEmpresa);
            var result = conn.Query<PolizaPersonaOperacionBaseDto>(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
            return DbHelper.CreateOkResponse(result.AsList());
        }

        /// <summary>
        /// Consulta las operaciones de pólizas de la persona por operación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetro con el Id de la operación.</param>
        /// <returns>Lista de operaciones de pólizas.</returns>
        public ErrorDto<List<PolizaPersonaOperacionPolizaDto>> Poliza_Persona_Operaciones_Polizas(int codEmpresa, PolizaPersonaOperacionPolizaParams param)
        {
            var sql = "spPoliza_Persona_Operaciones_Polizas";
            var parameters = new { param.Operacion };
            using var conn = _portalDb.CreateConnection(codEmpresa);
            var result = conn.Query<PolizaPersonaOperacionPolizaDto>(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
            return DbHelper.CreateOkResponse(result.AsList());
        }

        /// <summary>
        /// Consulta los reclamos de una operación y póliza.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con Id de operación y Id de póliza.</param>
        /// <returns>Lista de reclamos de la persona.</returns>
        public ErrorDto<List<PolizaPersonaReclamoDto>> Poliza_Persona_Reclamos(int codEmpresa, PolizaPersonaReclamoParams param)
        {
            var sql = "spPoliza_Persona_Reclamos";
            var parameters = new { param.Operacion, param.OperacionPoliza };
            using var conn = _portalDb.CreateConnection(codEmpresa);
            var result = conn.Query<PolizaPersonaReclamoDto>(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
            return DbHelper.CreateOkResponse(result.AsList());
        }

        /// <summary>
        /// Todos los reclamos de una persona (VB6: sbReclamos_Todos_Load -> spPoliza_Persona_Reclamos_Todos @Cedula).
        /// </summary>
        public ErrorDto<List<PolizaPersonaReclamoDto>> Poliza_Persona_Reclamos_Todos(int codEmpresa, PolizaPersonaCreditoParams param)
        {
            var sql = "spPoliza_Persona_Reclamos_Todos";
            var parameters = new { param.Cedula };
            using var conn = _portalDb.CreateConnection(codEmpresa);
            var result = conn.Query<PolizaPersonaReclamoDto>(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
            return DbHelper.CreateOkResponse(result.AsList());
        }

        
    }
}
