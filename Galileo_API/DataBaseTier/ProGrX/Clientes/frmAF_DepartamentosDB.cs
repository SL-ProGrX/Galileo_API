using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFDepartamentosDB
    {
        private readonly IConfiguration _config;

        private const string SqlInstituciones = @"
                    SELECT cod_institucion AS item,
                           descripcion
                    FROM dbo.instituciones;";

        private const string SqlDepartamentosTotal = @"
                    SELECT COUNT(cod_departamento)
                    FROM dbo.AFDepartamentos
                    WHERE cod_institucion = @institucion
                      AND (@hasFilter = 0 OR
                           descripcion LIKE @filtro OR
                           cod_departamento LIKE @filtro);";

        private const string SqlDepartamentosLista = @"
                    SELECT cod_departamento,
                           descripcion,
                           cod_institucion
                    FROM dbo.AFDepartamentos
                    WHERE cod_institucion = @institucion
                      AND (@hasFilter = 0 OR
                           descripcion LIKE @filtro OR
                           cod_departamento LIKE @filtro)
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN cod_departamento END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN cod_departamento END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        cod_departamento ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlSeccionesTotal = @"
                    SELECT COUNT(cod_seccion)
                    FROM dbo.AfSecciones
                    WHERE cod_institucion = @institucion
                      AND cod_departamento = @departamento
                      AND (@hasFilter = 0 OR
                           descripcion LIKE @filtro OR
                           cod_seccion LIKE @filtro);";

        private const string SqlSeccionesLista = @"
                    SELECT cod_seccion,
                           descripcion,
                           cod_institucion,
                           cod_departamento
                    FROM dbo.AfSecciones
                    WHERE cod_institucion = @institucion
                      AND cod_departamento = @departamento
                      AND (@hasFilter = 0 OR
                           descripcion LIKE @filtro OR
                           cod_seccion LIKE @filtro)
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN cod_seccion END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN cod_seccion END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        cod_seccion ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlDepartamentoExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.AfDepartamentos
                    WHERE cod_institucion = @institucion
                      AND cod_departamento = @departamento;";

        private const string SqlDepartamentoInsert = @"
                    INSERT INTO dbo.AfDepartamentos
                    (
                        cod_institucion,
                        cod_departamento,
                        descripcion
                    )
                    VALUES
                    (
                        @institucion,
                        @departamento,
                        @descripcion
                    );";

        private const string SqlSeccionDefaultInsert = @"
                    INSERT INTO dbo.AfSecciones
                    (
                        cod_institucion,
                        cod_departamento,
                        cod_seccion,
                        descripcion
                    )
                    VALUES
                    (
                        @institucion,
                        @departamento,
                        '',
                        'Sin Descripción'
                    );";

        private const string SqlDepartamentoUpdate = @"
                    UPDATE dbo.AfDepartamentos
                    SET descripcion = @descripcion
                    WHERE cod_institucion = @institucion
                      AND cod_departamento = @departamento;";

        private const string SqlSeccionExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.AfSecciones
                    WHERE cod_institucion = @institucion
                      AND cod_departamento = @departamento
                      AND cod_seccion = @seccion;";

        private const string SqlSeccionInsert = @"
                    INSERT INTO dbo.AfSecciones
                    (
                        cod_institucion,
                        cod_departamento,
                        cod_seccion,
                        descripcion
                    )
                    VALUES
                    (
                        @institucion,
                        @departamento,
                        @seccion,
                        @descripcion
                    );";

        private const string SqlSeccionUpdate = @"
                    UPDATE dbo.AfSecciones
                    SET descripcion = @descripcion
                    WHERE cod_institucion = @institucion
                      AND cod_departamento = @departamento
                      AND cod_seccion = @seccion;";

        private const string SqlSeccionesDepartamentoDelete = @"
                    DELETE FROM dbo.AfSecciones
                    WHERE cod_institucion = @Institucion
                      AND cod_departamento = @Departamento;";

        private const string SqlDepartamentoDelete = @"
                    DELETE FROM dbo.AfDepartamentos
                    WHERE cod_institucion = @Institucion
                      AND cod_departamento = @Departamento;";

        private const string SqlSeccionDelete = @"
                    DELETE FROM dbo.AfSecciones
                    WHERE cod_institucion = @Institucion
                      AND cod_departamento = @Departamento
                      AND cod_seccion = @Seccion;";

        private static readonly IReadOnlyDictionary<string, int> DepartamentosSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["cod_departamento"] = 1,
            ["descripcion"] = 2
        };

        private static readonly IReadOnlyDictionary<string, int> SeccionesSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["cod_seccion"] = 1,
            ["descripcion"] = 2
        };

        public FrmAFDepartamentosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de instituciones disponibles para departamentos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de instituciones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_DepartamentosInstituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstituciones);
        }


        /// <summary>
        /// Obtiene la lista paginada de departamentos de una institución.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="institucion">Código de institución.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Lista paginada de departamentos.</returns>
        public ErrorDto<AfDepartamentosLista> AF_DepartamentosLista_Obtener(int CodEmpresa, int institucion, FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, DepartamentosSortMap, "cod_departamento");
            spec.Params.Add("institucion", institucion);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new AfDepartamentosLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlDepartamentosTotal, spec.Params),
                lista = connection.Query<AfDepartamentosDto>(SqlDepartamentosLista, spec.Params).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new AfDepartamentosLista { total = 0, lista = new List<AfDepartamentosDto>() })
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener departamentos.",
                    result.Code.GetValueOrDefault(-1),
                    new AfDepartamentosLista { total = 0, lista = new List<AfDepartamentosDto>() });
        }


        /// <summary>
        /// Obtiene la lista paginada de secciones de un departamento.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="institucion">Código de institución.</param>
        /// <param name="departamento">Código de departamento.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Lista paginada de secciones.</returns>
        public ErrorDto<AfSeccionesLista> AF_DepartamentosSecciones_Obtener(int CodEmpresa, int institucion, string departamento, FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, SeccionesSortMap, "cod_seccion");
            spec.Params.Add("institucion", institucion);
            spec.Params.Add("departamento", NormalizarTexto(departamento));

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new AfSeccionesLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlSeccionesTotal, spec.Params),
                lista = connection.Query<AfSeccionesDto>(SqlSeccionesLista, spec.Params).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new AfSeccionesLista { total = 0, lista = new List<AfSeccionesDto>() })
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener secciones.",
                    result.Code.GetValueOrDefault(-1),
                    new AfSeccionesLista { total = 0, lista = new List<AfSeccionesDto>() });
        }


        /// <summary>
        /// Inserta o actualiza un departamento de una institución.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Info">Datos del departamento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_Departamentos_Guardar(int CodEmpresa, AfDepartamentosDto Info)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos del departamento son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = CrearParametrosDepartamento(Info);
                var existe = connection.QueryFirstOrDefault<int>(SqlDepartamentoExiste, parametros);

                if (existe == 0)
                {
                    connection.Execute(SqlDepartamentoInsert, parametros);
                    connection.Execute(SqlSeccionDefaultInsert, parametros);
                }
                else
                {
                    connection.Execute(SqlDepartamentoUpdate, parametros);
                }

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar departamento.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Inserta o actualiza una sección de un departamento.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Info">Datos de la sección.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_DepartamentosSecciones_Guardar(int CodEmpresa, AfSeccionesDto Info)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos de la sección son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = CrearParametrosSeccion(Info);
                var existe = connection.QueryFirstOrDefault<int>(SqlSeccionExiste, parametros);

                connection.Execute(existe == 0 ? SqlSeccionInsert : SqlSeccionUpdate, parametros);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar sección.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Elimina un departamento y sus secciones asociadas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Institucion">Código de institución.</param>
        /// <param name="Departamento">Código de departamento.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto AF_Departamentos_Borrar(int CodEmpresa, int Institucion, string Departamento)
        {
            var parametros = new
            {
                Institucion,
                Departamento = NormalizarTexto(Departamento)
            };

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(SqlSeccionesDepartamentoDelete, parametros);
                connection.Execute(SqlDepartamentoDelete, parametros);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar departamento.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Elimina una sección específica de un departamento.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Institucion">Código de institución.</param>
        /// <param name="Departamento">Código de departamento.</param>
        /// <param name="Seccion">Código de sección.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto AF_DepartamentosSecciones_Borrar(int CodEmpresa, int Institucion, string Departamento, string Seccion)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlSeccionDelete,
                new
                {
                    Institucion,
                    Departamento = NormalizarTexto(Departamento),
                    Seccion = NormalizarTexto(Seccion)
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar sección.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea parámetros seguros para guardar departamentos.
        /// </summary>
        /// <param name="info">Datos del departamento.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosDepartamento(AfDepartamentosDto info)
        {
            return new
            {
                institucion = info.cod_institucion,
                departamento = NormalizarTexto(info.cod_departamento),
                descripcion = NormalizarTexto(info.descripcion)
            };
        }


        /// <summary>
        /// Crea parámetros seguros para guardar secciones.
        /// </summary>
        /// <param name="info">Datos de la sección.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosSeccion(AfSeccionesDto info)
        {
            return new
            {
                institucion = info.cod_institucion,
                departamento = NormalizarTexto(info.cod_departamento),
                seccion = NormalizarTexto(info.cod_seccion),
                descripcion = NormalizarTexto(info.descripcion)
            };
        }


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}