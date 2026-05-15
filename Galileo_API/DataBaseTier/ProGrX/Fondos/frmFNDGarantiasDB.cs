using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndGrantiasDb
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 18;
        private readonly MSecurityMainDb _Security_MainDB;

        private const string SpGarantiaAhorrosConsulta = "spFnd_Garantia_Ahorros_Consulta";
        private const string SpGarantiaAhorrosRegistro = "spFnd_Garantia_Ahorros_Registro";

        private const string SqlGarantiasLazyLoad = @"
                    SELECT COUNT(1)
                    FROM dbo.fnd_garantias
                    WHERE @hasFilter = 0 OR
                    (
                        garantia_fnd LIKE @filtro OR
                        descripcion LIKE @filtro
                    );

                    SELECT
                        garantia_fnd AS Garantia_FND,
                        descripcion,
                        activa
                    FROM dbo.fnd_garantias
                    WHERE @hasFilter = 0 OR
                    (
                        garantia_fnd LIKE @filtro OR
                        descripcion LIKE @filtro
                    )
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN garantia_fnd END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN garantia_fnd END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN activa END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN activa END DESC,
                        garantia_fnd ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlGarantias = @"
                    SELECT
                        Garantia_FND,
                        Descripcion,
                        Activa
                    FROM dbo.FND_GARANTIAS
                    ORDER BY Garantia_FND;";

        private const string SqlValidaGarantia = @"
                    SELECT ISNULL(COUNT(1), 0) AS Existe
                    FROM dbo.fnd_garantias
                    WHERE garantia_fnd = @Garantia_FND;";

        private const string SqlInsertGarantia = @"
                    INSERT INTO dbo.fnd_garantias
                    (
                        garantia_fnd,
                        descripcion,
                        activa
                    )
                    VALUES
                    (
                        @Garantia_FND,
                        @Descripcion,
                        @Activa
                    );";

        private const string SqlUpdateGarantia = @"
                    UPDATE dbo.fnd_garantias
                    SET descripcion = @Descripcion,
                        activa = @Activa
                    WHERE garantia_fnd = @Garantia_FND;";

        private const string SqlDeleteGarantia = @"
                    DELETE FROM dbo.fnd_garantias
                    WHERE garantia_fnd = @Garantia_FND;";

        private const string SqlGarantiasDropdown = @"
                    SELECT
                        Garantia_FND AS item,
                        Descripcion AS descripcion
                    FROM dbo.FND_GARANTIAS
                    ORDER BY Garantia_FND;";

        private const string SqlOperadorasDropdown = @"
                    SELECT
                        RTRIM(cod_Operadora) AS item,
                        RTRIM(Descripcion) AS descripcion
                    FROM dbo.fnd_Operadoras
                    ORDER BY Descripcion;";

        private const string SqlEstadosPersonaDropdown = @"
                    SELECT
                        RTRIM(cod_estado) AS item,
                        RTRIM(Descripcion) AS descripcion
                    FROM dbo.afi_Estados_Persona
                    ORDER BY Descripcion;";

        private static readonly IReadOnlyDictionary<string, int> GarantiasSortMap = new Dictionary<string, int>
        {
            ["garantia_fnd"] = 1,
            ["descripcion"] = 2,
            ["activa"] = 3
        };

        public FrmFndGrantiasDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de garantías con lazy load (paginación y filtro).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FndGarantiasLista> Fnd_GarantiasLista_Obtener(int CodEmpresa, Models.FiltrosLazyLoadData filtros)
        {
            var result = DbHelper.CreateOkResponse(new FndGarantiasLista
            {
                total = 0,
                lista = new List<FndGarantiaModel>()
            });

            try
            {
                var spec = LazyLoadHelper.Build(filtros, GarantiasSortMap, "garantia_fnd");
                var queryResult = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(SqlGarantiasLazyLoad, spec.Params);
                    return new FndGarantiasLista
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<FndGarantiaModel>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        queryResult.Description ?? "Error al consultar garantías.",
                        queryResult.Code ?? -1,
                        new FndGarantiasLista { total = 0, lista = new List<FndGarantiaModel>() });
                }

                result.Result = queryResult.Result ?? new FndGarantiasLista
                {
                    total = 0,
                    lista = new List<FndGarantiaModel>()
                };
            }
            catch (Exception ex)
            {
                result = DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndGarantiasLista { total = 0, lista = new List<FndGarantiaModel>() });
            }

            return result;
        }

        /// <summary>
        /// Obtiene la lista de garantías sin paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FndGarantiaModel>> Fnd_Garantias_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<FndGarantiaModel>(new PortalDB(_config), CodEmpresa, SqlGarantias);
        }

        /// <summary>
        /// Valida si una garantía ya existe.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="garantiaFND"></param>
        /// <returns></returns>
        public ErrorDto<FndGarantiaValidaResult> Fnd_Garantias_Valida(int CodEmpresa, string garantiaFND)
        {
            var existe = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlValidaGarantia,
                0,
                new { Garantia_FND = NormalizarTexto(garantiaFND) });

            return new ErrorDto<FndGarantiaValidaResult>
            {
                Code = existe.Code,
                Description = existe.Description,
                Result = new FndGarantiaValidaResult
                {
                    Existe = existe.Result
                }
            };
        }

        /// <summary>
        /// Inserta o actualiza una garantía.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="garantia"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Garantias_Guardar(int CodEmpresa, FndGarantiaModel garantia)
        {
            if (garantia is null)
            {
                return DbHelper.ErrorResponse("Los datos de la garantía son requeridos.", -2);
            }

            var existe = Fnd_Garantias_Valida(CodEmpresa, garantia.Garantia_FND);
            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al validar garantía.", existe.Code ?? -1);
            }

            if (garantia.IsNew && existe.Result?.Existe > 0)
            {
                return DbHelper.ErrorResponse($"La garantía {garantia.Garantia_FND} ya existe.", -2);
            }

            if (!garantia.IsNew && existe.Result?.Existe == 0)
            {
                return DbHelper.ErrorResponse($"La garantía {garantia.Garantia_FND} no existe.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                garantia.IsNew ? SqlInsertGarantia : SqlUpdateGarantia,
                CrearParametrosGarantia(garantia));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                garantia.Usuario ?? string.Empty,
                $"Garantia de Fondo : {NormalizarTexto(garantia.Garantia_FND)}",
                garantia.IsNew ? "Registra - Web" : "Modifica - Web");

            return result;
        }

        /// <summary>
        /// Elimina una garantía por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="garantiaFND"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Garantias_Eliminar(int CodEmpresa, string garantiaFND, string usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlDeleteGarantia,
                new { Garantia_FND = NormalizarTexto(garantiaFND) });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Garantia de Fondo : {NormalizarTexto(garantiaFND)}",
                "Elimina - Web");

            return result;
        }

        /// <summary>
        /// Ejecuta el SP spFnd_Garantia_Ahorros_Consulta para consultar líneas de garantía de ahorros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<FndGarantiaAhorrosConsultaResult>> Fnd_Garantia_Ahorros_Consulta(int CodEmpresa, FndGarantiaAhorrosConsultaRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de consulta son requeridos.",
                    -2,
                    new List<FndGarantiaAhorrosConsultaResult>());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Query<FndGarantiaAhorrosConsultaResult>(
                    SpGarantiaAhorrosConsulta,
                    CrearParametrosAhorrosConsulta(request),
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<FndGarantiaAhorrosConsultaResult>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<FndGarantiaAhorrosConsultaResult>()
            };
        }

        /// <summary>
        /// Ejecuta el SP spFnd_Garantia_Ahorros_Registro para registrar o actualizar una línea de garantía de ahorros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Garantia_Ahorros_Registro(int CodEmpresa, FndGarantiaAhorrosRegistroRequest request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de registro son requeridos.", -2);
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    SpGarantiaAhorrosRegistro,
                    CrearParametrosAhorrosRegistro(request),
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al registrar garantía de ahorros.", result.Code ?? -1);
            }

            RegistrarBitacoraAhorros(CodEmpresa, request);
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Obtiene la lista de garantías para combos genéricos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Garantias_Lista_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, SqlGarantiasDropdown);
        }

        /// <summary>
        /// Obtiene la lista de operadoras para combos genéricos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Operadoras_Lista_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, SqlOperadorasDropdown);
        }

        /// <summary>
        /// Obtiene la lista de estados de persona para combos genéricos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_EstadosPersona_Lista_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, SqlEstadosPersonaDropdown);
        }

        private static object CrearParametrosGarantia(FndGarantiaModel garantia)
        {
            return new
            {
                Garantia_FND = NormalizarTexto(garantia.Garantia_FND),
                Descripcion = NormalizarTexto(garantia.Descripcion),
                garantia.Activa
            };
        }

        private static object CrearParametrosAhorrosConsulta(FndGarantiaAhorrosConsultaRequest request)
        {
            return new
            {
                Garantia = NormalizarTexto(request.Garantia_FND),
                Estado = NormalizarTexto(request.Cod_Estado)
            };
        }

        private static object CrearParametrosAhorrosRegistro(FndGarantiaAhorrosRegistroRequest request)
        {
            return new
            {
                Garantia = NormalizarTexto(request.Garantia_FND),
                Estado = NormalizarTexto(request.Cod_Estado),
                Linea = request.Linea_Id,
                MembresiaInicio = request.Membresia_Inicio,
                MembresiaCorte = request.Membresia_Corte,
                Patrimonio = request.Patrimonio ? 1 : 0,
                Operadora = request.Cod_Operadora,
                Plan = NormalizarTexto(request.Cod_Plan),
                Porcentaje = request.Porcentaje,
                Usuario = NormalizarTexto(request.Usuario),
                Mov = NormalizarTexto(request.Accion)
            };
        }

        private void RegistrarBitacoraAhorros(int codEmpresa, FndGarantiaAhorrosRegistroRequest request)
        {
            var movimiento = NormalizarTexto(request.Accion) switch
            {
                "A" => "Registra - Web",
                "E" => "Elimina - Web",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return;
            }

            var detalle = $"Garantia de s/Ahorros Extra, Linea: {request.Linea_Id}, Gar: {NormalizarTexto(request.Garantia_FND)}, Est: {NormalizarTexto(request.Cod_Estado)} Plan : {NormalizarTexto(request.Cod_Plan)} Porcentaje : {request.Porcentaje}, Mem.I: {request.Membresia_Inicio}, Mem.C: {request.Membresia_Corte}";
            RegistrarBitacora(codEmpresa, request.Usuario, detalle, movimiento);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
