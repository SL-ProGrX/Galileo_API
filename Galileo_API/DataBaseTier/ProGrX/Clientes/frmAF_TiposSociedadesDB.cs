using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFTiposSociedadesDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;

        private const string SqlSociedadesTotal = @"
                    SELECT COUNT(Cod_Sociedad)
                    FROM dbo.AFI_Sociedades_Tipos
                    WHERE @hasFilter = 0 OR
                          Cod_Sociedad LIKE @filtro OR
                          descripcion LIKE @filtro;";

        private const string SqlSociedadesLista = @"
                    SELECT Cod_Sociedad,
                           descripcion,
                           Activa
                    FROM dbo.AFI_Sociedades_Tipos
                    WHERE @hasFilter = 0 OR
                          Cod_Sociedad LIKE @filtro OR
                          descripcion LIKE @filtro
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN Cod_Sociedad END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN Cod_Sociedad END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        Cod_Sociedad ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlSociedadExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.AFI_Sociedades_Tipos
                    WHERE Cod_Sociedad = @CodSociedad;";

        private const string SqlSociedadInsert = @"
                    INSERT INTO dbo.AFI_Sociedades_Tipos
                    (
                        Cod_Sociedad,
                        descripcion,
                        Activa
                    )
                    VALUES
                    (
                        @CodSociedad,
                        @Descripcion,
                        @Activa
                    );";

        private const string SqlSociedadUpdate = @"
                    UPDATE dbo.AFI_Sociedades_Tipos
                    SET descripcion = @Descripcion,
                        Activa = @Activa
                    WHERE Cod_Sociedad = @CodSociedad;";

        private const string SqlSociedadDelete = @"
                    DELETE FROM dbo.AFI_Sociedades_Tipos
                    WHERE Cod_Sociedad = @CodSociedad;";

        private static readonly IReadOnlyDictionary<string, int> SociedadesSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Cod_Sociedad"] = 1,
            ["cod_Sociedad"] = 1,
            ["descripcion"] = 2
        };

        public FrmAFTiposSociedadesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener tipos de sociedades.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Lista paginada de tipos de sociedades.</returns>
        public ErrorDto<AfTiposSociedadesLista> AF_TiposSociedades_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, SociedadesSortMap, "Cod_Sociedad");

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new AfTiposSociedadesLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlSociedadesTotal, spec.Params),
                lista = connection.Query<AfTiposSociedadesDto>(SqlSociedadesLista, spec.Params).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener tipos de sociedades.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaVacia());
        }

        /// <summary>
        /// Guardar tipos de sociedades, insertando o actualizando según exista.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="Info">Datos del tipo de sociedad.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto AF_TiposSociedades_Guardar(int CodEmpresa, string Usuario, AfTiposSociedadesDto Info)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos del tipo de sociedad son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = CrearParametrosSociedad(Info);
                var existe = connection.QueryFirstOrDefault<int>(SqlSociedadExiste, parametros);
                connection.Execute(existe == 0 ? SqlSociedadInsert : SqlSociedadUpdate, parametros);
                return existe == 0 ? "Registra - WEB" : "Modifica - WEB";
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al guardar tipo de sociedad.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraSociedad(CodEmpresa, Usuario, Info.cod_Sociedad, result.Result ?? "Modifica - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Eliminar tipo de sociedad.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="CodSociedad">Código de sociedad.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto AF_TiposSociedades_Eliminar(int CodEmpresa, string Usuario, string CodSociedad)
        {
            var codSociedadSeguro = NormalizarTexto(CodSociedad);
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlSociedadDelete,
                new { CodSociedad = codSociedadSeguro });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar tipo de sociedad.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraSociedad(CodEmpresa, Usuario, codSociedadSeguro, "Elimina - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Crea parámetros seguros para guardar un tipo de sociedad.
        /// </summary>
        private static object CrearParametrosSociedad(AfTiposSociedadesDto info)
        {
            return new
            {
                CodSociedad = NormalizarTexto(info.cod_Sociedad),
                Descripcion = NormalizarTexto(info.descripcion),
                Activa = info.activa ? 1 : 0
            };
        }

        /// <summary>
        /// Registra en bitácora el movimiento del tipo de sociedad.
        /// </summary>
        private void RegistrarBitacoraSociedad(int codEmpresa, string usuario, string codSociedad, string movimiento)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = $"Sociedades : {NormalizarTexto(codSociedad)}",
                Movimiento = movimiento,
                Modulo = 9
            });
        }

        /// <summary>
        /// Crea una lista vacía de tipos de sociedades.
        /// </summary>
        private static AfTiposSociedadesLista CrearListaVacia()
        {
            return new AfTiposSociedadesLista
            {
                total = 0,
                lista = new List<AfTiposSociedadesDto>()
            };
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}