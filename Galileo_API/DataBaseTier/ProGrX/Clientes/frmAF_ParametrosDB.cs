using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFParametrosDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;

        private const string SpParametros = "spAFIParametros";

        private const string SqlParametrosTotal = @"
                    SELECT COUNT(cod_parametro)
                    FROM dbo.afi_parametros
                    WHERE @hasFilter = 0 OR
                          cod_parametro LIKE @filtro OR
                          descripcion LIKE @filtro OR
                          valor LIKE @filtro;";

        private const string SqlParametrosLista = @"
                    SELECT cod_parametro,
                           descripcion,
                           valor
                    FROM dbo.afi_parametros
                    WHERE @hasFilter = 0 OR
                          cod_parametro LIKE @filtro OR
                          descripcion LIKE @filtro OR
                          valor LIKE @filtro
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN cod_parametro END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN cod_parametro END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN valor END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN valor END DESC,
                        cod_parametro ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlParametroUpdate = @"
                    UPDATE dbo.afi_parametros
                    SET valor = @Valor
                    WHERE cod_parametro = @Codigo;";

        private static readonly IReadOnlyDictionary<string, int> ParametrosSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["cod_parametro"] = 1,
            ["descripcion"] = 2,
            ["valor"] = 3
        };

        public FrmAFParametrosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la lista paginada de parámetros generales de afiliación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Lista paginada de parámetros.</returns>
        public ErrorDto<AfParametrosLista> AF_Parametros_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, ParametrosSortMap, "cod_parametro");

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(SpParametros, commandType: System.Data.CommandType.StoredProcedure);

                return new AfParametrosLista
                {
                    total = connection.QueryFirstOrDefault<int>(SqlParametrosTotal, spec.Params),
                    lista = connection.Query<AfParametrosDto>(SqlParametrosLista, spec.Params).ToList()
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener parámetros generales de afiliación.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaVacia());
        }


        /// <summary>
        /// Actualiza el valor de un parámetro general de afiliación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la actualización.</param>
        /// <param name="Codigo">Código del parámetro.</param>
        /// <param name="Valor">Nuevo valor del parámetro.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto AF_Parametros_Actualizar(int CodEmpresa, string Usuario, string Codigo, string Valor)
        {
            var codigoSeguro = NormalizarTexto(Codigo);
            if (string.IsNullOrWhiteSpace(codigoSeguro))
            {
                return DbHelper.ErrorResponse("El código del parámetro es requerido.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlParametroUpdate,
                new
                {
                    Codigo = codigoSeguro,
                    Valor = NormalizarTexto(Valor)
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al actualizar parámetro general de afiliación.",
                    result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraParametro(CodEmpresa, Usuario, codigoSeguro);
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Crea una lista vacía de parámetros.
        /// </summary>
        private static AfParametrosLista CrearListaVacia()
        {
            return new AfParametrosLista
            {
                total = 0,
                lista = new List<AfParametrosDto>()
            };
        }

        /// <summary>
        /// Registra en bitácora la modificación de un parámetro general de afiliación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la actualización.</param>
        /// <param name="codigo">Código del parámetro.</param>
        private void RegistrarBitacoraParametro(int codEmpresa, string usuario, string codigo)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = $"Parametro General de Afiliación : {codigo}",
                Movimiento = "Modifica - WEB",
                Modulo = 9
            });
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