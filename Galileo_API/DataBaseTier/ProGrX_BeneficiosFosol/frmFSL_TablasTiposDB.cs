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

        private const string CatalogosListaSql = @"
            DECLARE @Catalogo TABLE (codigo NVARCHAR(100), descripcion NVARCHAR(MAX), activa BIT);

            IF @tipo = 'G'
                INSERT INTO @Catalogo SELECT COD_GESTION, descripcion, Activa FROM FSL_TIPOS_GESTIONES;
            ELSE IF @tipo = 'A'
                INSERT INTO @Catalogo SELECT COD_APELACION, descripcion, Activa FROM FSL_TIPOS_APELACIONES;
            ELSE IF @tipo = 'E'
                INSERT INTO @Catalogo SELECT COD_ENFERMEDAD, descripcion, Activa FROM FSL_TIPOS_ENFERMEDADES;

            SELECT COUNT(*)
            FROM @Catalogo
            WHERE @like IS NULL OR codigo LIKE @like OR descripcion LIKE @like;

            SELECT codigo, descripcion, activa
            FROM @Catalogo
            WHERE @like IS NULL OR codigo LIKE @like OR descripcion LIKE @like
            ORDER BY codigo
            OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string ActualizarSql = @"
            IF @tipo = 'G'
                UPDATE FSL_TIPOS_GESTIONES SET descripcion = @descripcion, Activa = @activa WHERE COD_GESTION = @codigo;
            ELSE IF @tipo = 'A'
                UPDATE FSL_TIPOS_APELACIONES SET descripcion = @descripcion, Activa = @activa WHERE COD_APELACION = @codigo;
            ELSE IF @tipo = 'E'
                UPDATE FSL_TIPOS_ENFERMEDADES SET descripcion = @descripcion, Activa = @activa WHERE COD_ENFERMEDAD = @codigo;";

        private const string InsertarSql = @"
            IF @tipo = 'G'
                INSERT INTO FSL_TIPOS_GESTIONES (COD_GESTION, descripcion, Activa, registro_fecha, registro_usuario)
                VALUES (@codigo, @descripcion, @activa, GETDATE(), @usuario);
            ELSE IF @tipo = 'A'
                INSERT INTO FSL_TIPOS_APELACIONES (COD_APELACION, descripcion, Activa, registro_fecha, registro_usuario)
                VALUES (@codigo, @descripcion, @activa, GETDATE(), @usuario);
            ELSE IF @tipo = 'E'
                INSERT INTO FSL_TIPOS_ENFERMEDADES (COD_ENFERMEDAD, descripcion, Activa, registro_fecha, registro_usuario)
                VALUES (@codigo, @descripcion, @activa, GETDATE(), @usuario);";

        private const string ExisteSql = @"
            SELECT CASE @tipo
                WHEN 'G' THEN (SELECT ISNULL(COUNT(*), 0) FROM FSL_TIPOS_GESTIONES WHERE COD_GESTION = @codigo)
                WHEN 'A' THEN (SELECT ISNULL(COUNT(*), 0) FROM FSL_TIPOS_APELACIONES WHERE COD_APELACION = @codigo)
                WHEN 'E' THEN (SELECT ISNULL(COUNT(*), 0) FROM FSL_TIPOS_ENFERMEDADES WHERE COD_ENFERMEDAD = @codigo)
                ELSE 0
            END;";

        private const string EliminarSql = @"
            IF @tipo = 'G'
                DELETE FROM FSL_TIPOS_GESTIONES WHERE COD_GESTION = @codigo;
            ELSE IF @tipo = 'A'
                DELETE FROM FSL_TIPOS_APELACIONES WHERE COD_APELACION = @codigo;
            ELSE IF @tipo = 'E'
                DELETE FROM FSL_TIPOS_ENFERMEDADES WHERE COD_ENFERMEDAD = @codigo;";

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
        /// Indica si el tipo corresponde a uno de los catálogos permitidos.
        /// </summary>
        private static bool EsTipoValido(string tipo) => tipo is "G" or "A" or "E";

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
            if (!EsTipoValido(tipo))
            {
                return DbHelper.CreateErrorResponse<FslTablaTipoLista>(TipoCatalogoInvalido);
            }

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new FslTablaTipoLista();

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                using var resultados = connection.QueryMultiple(
                    CatalogosListaSql,
                    new { tipo, like, offset, fetch });
                response.Total = resultados.ReadSingle<int>();
                response.Lista = resultados.Read<FslTablaTipoData>().ToList();
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
            if (!EsTipoValido(tipo))
            {
                return DbHelper.ErrorResponse(TipoCatalogoInvalido);
            }

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, ActualizarSql, new
            {
                tipo,
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
            if (!EsTipoValido(tipo))
            {
                return DbHelper.ErrorResponse(TipoCatalogoInvalido);
            }

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                if (FslTablaTipoExiste(connection, tipo, tipoData.codigo))
                {
                    return FslTablaTipos_Actualizar(CodCliente, tipo, tipoData);
                }

                connection.Execute(InsertarSql, new
                {
                    tipo,
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
        private static bool FslTablaTipoExiste(SqlConnection connection, string tipo, string codigo)
        {
            return connection.QueryFirstOrDefault<int>(ExisteSql, new { tipo, codigo }) > 0;
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
            if (!EsTipoValido(tipo))
            {
                return DbHelper.ErrorResponse(TipoCatalogoInvalido);
            }

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, EliminarSql, new { tipo, codigo });
        }
    }
}
