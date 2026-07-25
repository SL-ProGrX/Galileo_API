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
        private const string TipoCatalogoInvalido = "Tipo de catálogo inválido";
        private readonly IConfiguration _config;

        private static readonly FslTablaTipoSql GestionesSql = new(
            @"SELECT COUNT(*) FROM FSL_TIPOS_GESTIONES
              WHERE (@like IS NULL OR COD_GESTION LIKE @like OR descripcion LIKE @like)",
            @"SELECT COD_GESTION AS codigo, descripcion, Activa AS activa
              FROM FSL_TIPOS_GESTIONES
              WHERE (@like IS NULL OR COD_GESTION LIKE @like OR descripcion LIKE @like)
              ORDER BY COD_GESTION
              OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY",
            "UPDATE FSL_TIPOS_GESTIONES SET descripcion = @descripcion, Activa = @activa WHERE COD_GESTION = @codigo",
            @"INSERT INTO FSL_TIPOS_GESTIONES (COD_GESTION, descripcion, Activa, registro_fecha, registro_usuario)
              VALUES (@codigo, @descripcion, @activa, GETDATE(), @usuario)",
            "SELECT ISNULL(COUNT(*), 0) FROM FSL_TIPOS_GESTIONES WHERE COD_GESTION = @codigo",
            "DELETE FROM FSL_TIPOS_GESTIONES WHERE COD_GESTION = @codigo");

        private static readonly FslTablaTipoSql ApelacionesSql = new(
            @"SELECT COUNT(*) FROM FSL_TIPOS_APELACIONES
              WHERE (@like IS NULL OR COD_APELACION LIKE @like OR descripcion LIKE @like)",
            @"SELECT COD_APELACION AS codigo, descripcion, Activa AS activa
              FROM FSL_TIPOS_APELACIONES
              WHERE (@like IS NULL OR COD_APELACION LIKE @like OR descripcion LIKE @like)
              ORDER BY COD_APELACION
              OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY",
            "UPDATE FSL_TIPOS_APELACIONES SET descripcion = @descripcion, Activa = @activa WHERE COD_APELACION = @codigo",
            @"INSERT INTO FSL_TIPOS_APELACIONES (COD_APELACION, descripcion, Activa, registro_fecha, registro_usuario)
              VALUES (@codigo, @descripcion, @activa, GETDATE(), @usuario)",
            "SELECT ISNULL(COUNT(*), 0) FROM FSL_TIPOS_APELACIONES WHERE COD_APELACION = @codigo",
            "DELETE FROM FSL_TIPOS_APELACIONES WHERE COD_APELACION = @codigo");

        private static readonly FslTablaTipoSql EnfermedadesSql = new(
            @"SELECT COUNT(*) FROM FSL_TIPOS_ENFERMEDADES
              WHERE (@like IS NULL OR COD_ENFERMEDAD LIKE @like OR descripcion LIKE @like)",
            @"SELECT COD_ENFERMEDAD AS codigo, descripcion, Activa AS activa
              FROM FSL_TIPOS_ENFERMEDADES
              WHERE (@like IS NULL OR COD_ENFERMEDAD LIKE @like OR descripcion LIKE @like)
              ORDER BY COD_ENFERMEDAD
              OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY",
            "UPDATE FSL_TIPOS_ENFERMEDADES SET descripcion = @descripcion, Activa = @activa WHERE COD_ENFERMEDAD = @codigo",
            @"INSERT INTO FSL_TIPOS_ENFERMEDADES (COD_ENFERMEDAD, descripcion, Activa, registro_fecha, registro_usuario)
              VALUES (@codigo, @descripcion, @activa, GETDATE(), @usuario)",
            "SELECT ISNULL(COUNT(*), 0) FROM FSL_TIPOS_ENFERMEDADES WHERE COD_ENFERMEDAD = @codigo",
            "DELETE FROM FSL_TIPOS_ENFERMEDADES WHERE COD_ENFERMEDAD = @codigo");

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
        /// Obtiene las consultas predefinidas del catálogo según el tipo (G/A/E).
        /// </summary>
        private static FslTablaTipoSql? ResolverSql(string tipo) => tipo switch
        {
            "G" => GestionesSql,
            "A" => ApelacionesSql,
            "E" => EnfermedadesSql,
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
            var catalogoSql = ResolverSql(tipo);
            if (catalogoSql == null)
            {
                return DbHelper.CreateErrorResponse<FslTablaTipoLista>(TipoCatalogoInvalido);
            }

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new FslTablaTipoLista();

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                response.Total = connection.QueryFirstOrDefault<int>(catalogoSql.Conteo, new { like });
                response.Lista = connection.Query<FslTablaTipoData>(
                    catalogoSql.Lista,
                    new { like, offset, fetch }).ToList();
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
            var catalogoSql = ResolverSql(tipo);
            if (catalogoSql == null)
            {
                return DbHelper.ErrorResponse(TipoCatalogoInvalido);
            }

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, catalogoSql.Actualizar, new
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
            var catalogoSql = ResolverSql(tipo);
            if (catalogoSql == null)
            {
                return DbHelper.ErrorResponse(TipoCatalogoInvalido);
            }

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                if (FslTablaTipoExiste(connection, catalogoSql.Existe, tipoData.codigo))
                {
                    return FslTablaTipos_Actualizar(CodCliente, tipo, tipoData);
                }

                connection.Execute(catalogoSql.Insertar, new
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
        private static bool FslTablaTipoExiste(SqlConnection connection, string sql, string codigo)
        {
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
            var catalogoSql = ResolverSql(tipo);
            if (catalogoSql == null)
            {
                return DbHelper.ErrorResponse(TipoCatalogoInvalido);
            }

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, catalogoSql.Eliminar, new { codigo });
        }

        private sealed record FslTablaTipoSql(
            string Conteo,
            string Lista,
            string Actualizar,
            string Insertar,
            string Existe,
            string Eliminar);
    }
}
