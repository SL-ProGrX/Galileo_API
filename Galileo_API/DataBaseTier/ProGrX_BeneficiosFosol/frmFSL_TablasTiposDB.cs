using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de los catálogos de Tipos Fosol (frmFSL_TablasTipos): gestiones, apelaciones y enfermedades.
    /// </summary>
    public partial class FrmFslTablasTiposDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslTablasTiposDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene el mapeo de tabla y columna código según el tipo (G/A/E). Whitelist para evitar inyección.
        /// </summary>
        private static (string tabla, string colCodigo)? ResolverTabla(string tipo) => tipo switch
        {
            "G" => ("FSL_TIPOS_GESTIONES", "COD_GESTION"),
            "A" => ("FSL_TIPOS_APELACIONES", "COD_APELACION"),
            "E" => ("FSL_TIPOS_ENFERMEDADES", "COD_ENFERMEDAD"),
            _ => null
        };

        /// <summary>
        /// Obtiene la lista de tipos (gestiones, apelaciones o enfermedades) con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tipo">Tipo de catálogo (G/A/E).</param>
        /// <param name="filtro">Filtro por código o descripción.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <returns>Lista de tipos y total.</returns>
        public ErrorDto<FslTablaTipoLista> FslTablaTipos_Obtener(int CodCliente, string tipo, string? filtro, int? pagina, int? paginacion)
        {
            var mapeo = ResolverTabla(tipo);
            if (mapeo == null)
            {
                return DbHelper.CreateErrorResponse<FslTablaTipoLista>("Tipo de catálogo inválido");
            }

            var (tabla, colCodigo) = mapeo.Value;

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new FslTablaTipoLista();

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                var whereClause = $"WHERE (@like IS NULL OR {colCodigo} LIKE @like OR descripcion LIKE @like)";

                var sqlCount = $"SELECT COUNT(*) FROM {tabla} {whereClause}";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, new { like });

                var sql = $@"SELECT {colCodigo} AS codigo, descripcion, Activa AS activa
                             FROM {tabla} {whereClause}
                             ORDER BY {colCodigo}
                             OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";
                response.Lista = connection.Query<FslTablaTipoData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Actualiza un tipo (gestión, apelación o enfermedad).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tipo">Tipo de catálogo (G/A/E).</param>
        /// <param name="tipoData">Datos del tipo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslTablaTipos_Actualizar(int CodCliente, string tipo, FslTablaTipoData tipoData)
        {
            var mapeo = ResolverTabla(tipo);
            if (mapeo == null)
            {
                return DbHelper.ErrorResponse("Tipo de catálogo inválido");
            }

            var (tabla, colCodigo) = mapeo.Value;
            var sql = $@"UPDATE {tabla} SET descripcion = @descripcion, Activa = @activa WHERE {colCodigo} = @codigo";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new
            {
                tipoData.descripcion,
                activa = tipoData.activa ? 1 : 0,
                tipoData.codigo
            });

            if (result.Code == 0)
            {
                result.Description = "Registro actualizado satisfactoriamente!";
            }

            return result;
        }

        /// <summary>
        /// Inserta un tipo; si ya existe, delega en la actualización.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tipo">Tipo de catálogo (G/A/E).</param>
        /// <param name="usuario">Usuario que registra.</param>
        /// <param name="tipoData">Datos del tipo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslTablaTipo_Insertar(int CodCliente, string tipo, string usuario, FslTablaTipoData tipoData)
        {
            var mapeo = ResolverTabla(tipo);
            if (mapeo == null)
            {
                return DbHelper.ErrorResponse("Tipo de catálogo inválido");
            }

            var (tabla, colCodigo) = mapeo.Value;

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                if (FslTablaTipoExiste(connection, tabla, colCodigo, tipoData.codigo))
                {
                    return FslTablaTipos_Actualizar(CodCliente, tipo, tipoData);
                }

                var sql = $@"INSERT INTO {tabla} ({colCodigo}, descripcion, Activa, registro_fecha, registro_usuario)
                             VALUES (@codigo, @descripcion, @activa, GETDATE(), @usuario)";
                connection.Execute(sql, new
                {
                    tipoData.codigo,
                    tipoData.descripcion,
                    activa = tipoData.activa ? 1 : 0,
                    usuario
                });

                return DbHelper.OkResponse("Registro agregado satisfactoriamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Verifica si existe un código en la tabla de tipo indicada.
        /// </summary>
        private static bool FslTablaTipoExiste(SqlConnection connection, string tabla, string colCodigo, string codigo)
        {
            var sql = $"SELECT ISNULL(COUNT(*), 0) FROM {tabla} WHERE {colCodigo} = @codigo";
            return connection.QueryFirstOrDefault<int>(sql, new { codigo }) > 0;
        }

        /// <summary>
        /// Elimina un tipo (gestión, apelación o enfermedad).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tipo">Tipo de catálogo (G/A/E).</param>
        /// <param name="codigo">Código del tipo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslTablaTipo_Eliminar(int CodCliente, string tipo, string codigo)
        {
            var mapeo = ResolverTabla(tipo);
            if (mapeo == null)
            {
                return DbHelper.ErrorResponse("Tipo de catálogo inválido");
            }

            var (tabla, colCodigo) = mapeo.Value;
            var sql = $"DELETE FROM {tabla} WHERE {colCodigo} = @codigo";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { codigo });
        }
    }
}
