using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFTiposIdsDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;

        private const string SqlTiposIdsTotal = @"
                    SELECT COUNT(Tipo_ID)
                    FROM dbo.vSys_Tipos_Ids
                    WHERE @hasFilter = 0 OR
                          Tipo_ID LIKE @filtro OR
                          descripcion LIKE @filtro OR
                          Tipo_Personeria_Desc LIKE @filtro;";

        private const string SqlTiposIdsLista = @"
                    SELECT Tipo_ID,
                           descripcion,
                           Tipo_Personeria_Desc,
                           Largo_Minimo,
                           Mascara,
                           CODIGO_SUGEF,
                           CODIGO_PIN,
                           CODIGO_HACIENDA,
                           CODIGO_SINPE
                    FROM dbo.vSys_Tipos_Ids
                    WHERE @hasFilter = 0 OR
                          Tipo_ID LIKE @filtro OR
                          descripcion LIKE @filtro OR
                          Tipo_Personeria_Desc LIKE @filtro
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN Tipo_ID END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN Tipo_ID END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN Tipo_Personeria_Desc END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN Tipo_Personeria_Desc END DESC,
                        Tipo_ID ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlTipoIdExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.afi_Tipos_IDs
                    WHERE Tipo_ID = @TipoId;";

        private const string SqlTipoIdInsert = @"
                    INSERT INTO dbo.afi_Tipos_IDs
                    (
                        Tipo_ID,
                        descripcion,
                        Tipo_Personeria,
                        Largo_Minimo,
                        Mascara,
                        CODIGO_SUGEF,
                        CODIGO_PIN,
                        CODIGO_HACIENDA,
                        CODIGO_SINPE,
                        Usuario,
                        Fecha
                    )
                    VALUES
                    (
                        @TipoId,
                        @Descripcion,
                        @TipoPersoneria,
                        @LargoMinimo,
                        @Mascara,
                        @CodigoSugef,
                        @CodigoPin,
                        @CodigoHacienda,
                        @CodigoSinpe,
                        @Usuario,
                        GETDATE()
                    );";

        private const string SqlTipoIdUpdate = @"
                    UPDATE dbo.afi_Tipos_IDs
                    SET descripcion = @Descripcion,
                        Tipo_Personeria = @TipoPersoneria,
                        Largo_Minimo = @LargoMinimo,
                        Mascara = @Mascara,
                        CODIGO_SUGEF = @CodigoSugef,
                        CODIGO_PIN = @CodigoPin,
                        CODIGO_HACIENDA = @CodigoHacienda,
                        CODIGO_SINPE = @CodigoSinpe,
                        MODIFICA_USUARIO = @Usuario,
                        MODIFICA_FECHA = GETDATE()
                    WHERE Tipo_ID = @TipoId;";

        private const string SqlTipoIdDelete = @"
                    DELETE FROM dbo.afi_Tipos_IDs
                    WHERE Tipo_ID = @TipoId;";

        private static readonly IReadOnlyDictionary<string, int> TiposIdsSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Tipo_ID"] = 1,
            ["tipo_Id"] = 1,
            ["descripcion"] = 2,
            ["Tipo_Personeria_Desc"] = 3,
            ["tipo_Personeria_Desc"] = 3
        };

        public FrmAFTiposIdsDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener tipos de identificaciones.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Lista paginada de tipos de identificación.</returns>
        public ErrorDto<AfTiposIdsLista> AF_TiposIds_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, TiposIdsSortMap, "Tipo_ID");

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new AfTiposIdsLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlTiposIdsTotal, spec.Params),
                lista = connection.Query<AfTiposIdsDto>(SqlTiposIdsLista, spec.Params).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener tipos de identificación.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaVacia());
        }

        /// <summary>
        /// Guardar tipo de identificación, insertando o actualizando según exista.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="Info">Datos del tipo de identificación.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto AF_TiposIds_Guardar(int CodEmpresa, string Usuario, AfTiposIdsDto Info)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos del tipo de identificación son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = CrearParametrosTipoId(Info, Usuario);
                var existe = connection.QueryFirstOrDefault<int>(SqlTipoIdExiste, parametros);
                connection.Execute(existe == 0 ? SqlTipoIdInsert : SqlTipoIdUpdate, parametros);
                return existe == 0 ? "Registra - WEB" : "Modifica - WEB";
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al guardar tipo de identificación.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraTipoId(CodEmpresa, Usuario, Info.tipo_Id.ToString(), result.Result ?? "Modifica - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Eliminar tipo de identificación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="TipoId">Código del tipo de identificación.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto AF_TiposIds_Eliminar(int CodEmpresa, string Usuario, int TipoId)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlTipoIdDelete,
                new { TipoId });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar tipo de identificación.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraTipoId(CodEmpresa, Usuario, TipoId.ToString(), "Elimina - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Crea parámetros seguros para guardar un tipo de identificación.
        /// </summary>
        private static object CrearParametrosTipoId(AfTiposIdsDto info, string usuario)
        {
            return new
            {
                TipoId = info.tipo_Id,
                Descripcion = NormalizarTexto(info.descripcion),
                TipoPersoneria = ObtenerTipoPersoneria(info.tipo_Personeria_Desc),
                LargoMinimo = info.largo_Minimo,
                Mascara = NormalizarTexto(info.mascara),
                CodigoSugef = NormalizarTexto(info.codigo_Sugef.ToString()),
                CodigoPin = NormalizarTexto(info.codigo_Pin.ToString()),
                CodigoHacienda = NormalizarTexto(info.codigo_Hacienda.ToString()),
                CodigoSinpe = NormalizarTexto(info.codigo_Sinpe.ToString()),
                Usuario = NormalizarTexto(usuario)
            };
        }

        /// <summary>
        /// Obtiene la primera letra del tipo de personería.
        /// </summary>
        private static string ObtenerTipoPersoneria(string? tipoPersoneria)
        {
            var texto = NormalizarTexto(tipoPersoneria);
            return string.IsNullOrWhiteSpace(texto) ? string.Empty : texto[..1];
        }

        /// <summary>
        /// Registra en bitácora el movimiento del tipo de identificación.
        /// </summary>
        private void RegistrarBitacoraTipoId(int codEmpresa, string usuario, string tipoId, string movimiento)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = $"Tipo de Idenficiación : {NormalizarTexto(tipoId)}",
                Movimiento = movimiento,
                Modulo = 9
            });
        }

        /// <summary>
        /// Crea una lista vacía de tipos de identificación.
        /// </summary>
        private static AfTiposIdsLista CrearListaVacia()
        {
            return new AfTiposIdsLista
            {
                total = 0,
                lista = new List<AfTiposIdsDto>()
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