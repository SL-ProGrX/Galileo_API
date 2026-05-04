using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using System.Data;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOAntiguedadTiposDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 4;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCOAntiguedadTiposDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de antigüedad (CBR_ANTIGUEDAD_TIPOS).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FrmCOAntiguedadTiposListaResult> Co_AntiguedadTipos_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var portalDb = new PortalDB(_config);
            var result = CrearResultadoListaAntiguedad();

            try
            {
                var consulta = CrearParametrosConsultaAntiguedad(filtros);
                var queryResult = DbHelper.WithConn(portalDb, CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(CrearSqlListaAntiguedad(consulta), consulta.Parametros);

                    return new FrmCOAntiguedadTiposListaResult
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<FrmCOAntiguedadTipoData>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return CrearErrorListaAntiguedad(queryResult.Description ?? "Error al consultar tipos de antigüedad.");
                }

                result.Result = queryResult.Result ?? new FrmCOAntiguedadTiposListaResult
                {
                    total = 0,
                    lista = new List<FrmCOAntiguedadTipoData>()
                };
            }
            catch (Exception ex)
            {
                result = CrearErrorListaAntiguedad(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Inserta o actualiza un tipo de antigüedad según isNew y existencia del código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto Co_AntiguedadTipos_Guardar(int CodEmpresa, string usuario, FrmCOAntiguedadTipoData tipo)
        {
            if (tipo is null)
            {
                return DbHelper.ErrorResponse("Datos del tipo de antigüedad no proporcionados.", -2);
            }

            var cod = NormalizarCodigo(tipo.cod_antiguedad);
            if (string.IsNullOrWhiteSpace(cod))
            {
                return DbHelper.ErrorResponse("Código inválido.", -2);
            }

            var existeResult = ExisteAntiguedad(CodEmpresa, cod);
            if (existeResult.Code != 0)
            {
                return DbHelper.ErrorResponse(existeResult.Description ?? "Error al validar el tipo de antigüedad.");
            }

            return ResolverGuardadoAntiguedad(CodEmpresa, usuario, tipo, cod, existeResult.Result);
        }
        /// <summary>
        /// Inserta un tipo de antigüedad según isNew y existencia del código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private ErrorDto Co_AntiguedadTipos_Insertar(int CodEmpresa, string usuario, FrmCOAntiguedadTipoData tipo)
        {
            const string query = @"
                    INSERT INTO dbo.CBR_ANTIGUEDAD_TIPOS
                    (
                        COD_ANTIGUEDAD,
                        descripcion,
                        DIAS_DESDE,
                        DIAS_HASTA,
                        ESTIMACION_NOCUBIERTA,
                        ESTIMACION_CUBIERTA,
                        Registro_Usuario,
                        Registro_Fecha
                    )
                    VALUES
                    (
                        @cod,
                        @desc,
                        @desde,
                        @hasta,
                        @noCub,
                        @cub,
                        @usuario,
                        dbo.MyGetdate()
                    );";

            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, CrearParametrosAntiguedad(tipo, usuario));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo Antigüedad: {NormalizarCodigo(tipo.cod_antiguedad)} - {tipo.descripcion}",
                "Registra - WEB");

            return result;
        }
        /// <summary>
        /// Actualiza un tipo de antigüedad según isNew y existencia del código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private ErrorDto Co_AntiguedadTipos_Actualizar(int CodEmpresa, string usuario, FrmCOAntiguedadTipoData tipo)
        {
            const string query = @"
                    UPDATE dbo.CBR_ANTIGUEDAD_TIPOS
                    SET
                        descripcion = @desc,
                        DIAS_DESDE = @desde,
                        DIAS_HASTA = @hasta,
                        ESTIMACION_NOCUBIERTA = @noCub,
                        ESTIMACION_CUBIERTA = @cub
                    WHERE UPPER(RTRIM(COD_ANTIGUEDAD)) = @cod;";

            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, CrearParametrosAntiguedad(tipo, usuario));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Tipo Antigüedad: {NormalizarCodigo(tipo.cod_antiguedad)} - {tipo.descripcion}",
                "Modifica - WEB");

            return result;
        }
        /// <summary>
        /// Elimina un tipo de antigüedad por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_antiguedad"></param>
        /// <returns></returns>
        public ErrorDto Co_AntiguedadTipos_Eliminar(int CodEmpresa, string usuario, string cod_antiguedad)
        {
            const string query = @"DELETE FROM dbo.CBR_ANTIGUEDAD_TIPOS WHERE UPPER(RTRIM(COD_ANTIGUEDAD)) = @cod;";

            var cod = NormalizarCodigo(cod_antiguedad);
            var result = DbHelper.ExecuteNonQuery(new PortalDB(_config), CodEmpresa, query, new { cod });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Tipo Antigüedad: {cod_antiguedad}", "Elimina - WEB");
            return result;
        }

        /// <summary>
        /// Obtiene el detalle (Garantía/Mitigador) para un tipo de antigüedad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_antiguedad"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCOAntiguedadGarantiaMitigadorData>> Co_AntiguedadTipos_Detalle_Obtener(int CodEmpresa,string cod_antiguedad, string usuario)
        {
            var codigo = NormalizarCodigo(cod_antiguedad);
            var user = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateErrorResponse<List<FrmCOAntiguedadGarantiaMitigadorData>>(
                    "Código inválido para consultar detalle.",
                    -2,
                    new List<FrmCOAntiguedadGarantiaMitigadorData>());
            }

            var parameters = CrearParametrosDetalleConsulta(codigo, user);
            var result = DbHelper.ExecuteStoredProcedureList<FrmCOAntiguedadGarantiaMitigadorData>(
                new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa),
                "spCbr_Garantia_Mitigador_Consulta",
                parameters);

            result.Result ??= new List<FrmCOAntiguedadGarantiaMitigadorData>();
            return result;
        }

        /// <summary>
        /// Registra o actualiza una línea del detalle (Garantía/Mitigador) para un tipo de antigüedad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto Co_AntiguedadTipos_Detalle_Guardar(int CodEmpresa,string usuario,FrmCOAntiguedadDetalleGuardarDto dto)
        {
            var codigo = NormalizarCodigo(dto?.cod_antiguedad);
            var garantia = NormalizarCodigo(dto?.garantia);
            var porcentaje = dto?.mitigador ?? 0m;
            var user = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(garantia))
            {
                return DbHelper.ErrorResponse("Datos incompletos para guardar detalle (Código/Garantía).", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                "spCbr_Garantia_Mitigador_Registra",
                CrearParametrosDetalleGuardar(codigo, garantia, porcentaje, user));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                user,
                $"Garantía/Mitigador: {codigo} - Garantía {garantia} - % {porcentaje:n2}",
                "Modifica - WEB");

            return result;
        }

        private ErrorDto<int> ExisteAntiguedad(int codEmpresa, string cod)
        {
            const string query = @"SELECT ISNULL(COUNT(1),0) FROM dbo.CBR_ANTIGUEDAD_TIPOS WHERE UPPER(RTRIM(COD_ANTIGUEDAD)) = @cod;";
            return DbHelper.ExecuteSingleQuery(new PortalDB(_config), codEmpresa, query, 0, new { cod });
        }

        private ErrorDto ResolverGuardadoAntiguedad(
            int codEmpresa,
            string usuario,
            FrmCOAntiguedadTipoData tipo,
            string cod,
            int existe)
        {
            if (tipo.isNew)
            {
                return ResolverInsercionAntiguedad(codEmpresa, usuario, tipo, cod, existe);
            }

            return ResolverActualizacionAntiguedad(codEmpresa, usuario, tipo, cod, existe);
        }

        private ErrorDto ResolverInsercionAntiguedad(
            int codEmpresa,
            string usuario,
            FrmCOAntiguedadTipoData tipo,
            string cod,
            int existe)
        {
            if (existe > 0)
            {
                return DbHelper.ErrorResponse($"El código {cod} ya existe.", -2);
            }

            return Co_AntiguedadTipos_Insertar(codEmpresa, usuario, tipo);
        }

        private ErrorDto ResolverActualizacionAntiguedad(
            int codEmpresa,
            string usuario,
            FrmCOAntiguedadTipoData tipo,
            string cod,
            int existe)
        {
            if (existe == 0)
            {
                return DbHelper.ErrorResponse($"El código {cod} no existe.", -2);
            }

            return Co_AntiguedadTipos_Actualizar(codEmpresa, usuario, tipo);
        }

        private static ErrorDto<FrmCOAntiguedadTiposListaResult> CrearResultadoListaAntiguedad()
        {
            return DbHelper.CreateOkResponse(new FrmCOAntiguedadTiposListaResult
            {
                total = 0,
                lista = new List<FrmCOAntiguedadTipoData>()
            });
        }

        private static ErrorDto<FrmCOAntiguedadTiposListaResult> CrearErrorListaAntiguedad(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new FrmCOAntiguedadTiposListaResult
                {
                    total = 0,
                    lista = new List<FrmCOAntiguedadTipoData>()
                });
        }

        private static FrmCOAntiguedadTiposConsultaParams CrearParametrosConsultaAntiguedad(FiltrosLazyLoadData? filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            var filtro = (filtros.filtro ?? string.Empty).Trim();
            var parametros = new DynamicParameters();
            AgregarFiltroAntiguedad(parametros, filtro);

            return new FrmCOAntiguedadTiposConsultaParams
            {
                Parametros = parametros,
                TieneFiltro = !string.IsNullOrWhiteSpace(filtro),
                SortField = ObtenerSortField(filtros.sortField),
                SortOrder = ObtenerSortOrder(filtros.sortOrder)
            };
        }

        private static void AgregarFiltroAntiguedad(DynamicParameters parametros, string filtro)
        {
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                parametros.Add("@q", $"%{filtro}%");
            }
        }

        private static string CrearSqlListaAntiguedad(FrmCOAntiguedadTiposConsultaParams consulta)
        {
            var whereSql = CrearWhereAntiguedad(consulta.TieneFiltro);

            return $@"
                    SELECT COUNT(1)
                    FROM dbo.CBR_ANTIGUEDAD_TIPOS
                    {whereSql};

                    SELECT
                        RTRIM(COD_ANTIGUEDAD) AS cod_antiguedad,
                        RTRIM(descripcion)    AS descripcion,
                        ISNULL(DIAS_DESDE,0)  AS dias_desde,
                        ISNULL(DIAS_HASTA,0)  AS dias_hasta,
                        ISNULL(ESTIMACION_NOCUBIERTA,0) AS estimacion_nocubierta,
                        ISNULL(ESTIMACION_CUBIERTA,0)   AS estimacion_cubierta,
                        CAST(0 AS bit) AS isNew
                    FROM dbo.CBR_ANTIGUEDAD_TIPOS
                    {whereSql}
                    ORDER BY {consulta.SortField} {consulta.SortOrder};";
        }

        private static string CrearWhereAntiguedad(bool tieneFiltro)
        {
            if (!tieneFiltro)
            {
                return string.Empty;
            }

            return @"WHERE (
                        UPPER(RTRIM(COD_ANTIGUEDAD)) LIKE UPPER(@q) OR
                        UPPER(RTRIM(descripcion))    LIKE UPPER(@q)
                    )";
        }

        private static string ObtenerSortField(string? sortField)
        {
            return (sortField ?? string.Empty).Trim() switch
            {
                "cod_antiguedad" => "COD_ANTIGUEDAD",
                "descripcion" => "descripcion",
                "dias_desde" => "DIAS_DESDE",
                "dias_hasta" => "DIAS_HASTA",
                "estimacion_nocubierta" => "ESTIMACION_NOCUBIERTA",
                "estimacion_cubierta" => "ESTIMACION_CUBIERTA",
                _ => "COD_ANTIGUEDAD"
            };
        }

        private static string ObtenerSortOrder(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
        }

        private static string NormalizarCodigo(string? valor)
        {
            return (valor ?? string.Empty).Trim().ToUpper();
        }

        private static object CrearParametrosAntiguedad(FrmCOAntiguedadTipoData tipo, string usuario)
        {
            return new
            {
                cod = NormalizarCodigo(tipo.cod_antiguedad),
                desc = (tipo.descripcion ?? string.Empty).Trim(),
                desde = tipo.dias_desde,
                hasta = tipo.dias_hasta,
                noCub = tipo.estimacion_nocubierta,
                cub = tipo.estimacion_cubierta,
                usuario
            };
        }

        private static DynamicParameters CrearParametrosDetalleConsulta(string codigo, string usuario)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Codigo", codigo);
            parameters.Add("@Tipo", "AS");
            parameters.Add("@Usuario", usuario);
            return parameters;
        }

        private static DynamicParameters CrearParametrosDetalleGuardar(string codigo, string garantia, decimal porcentaje, string usuario)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Codigo", codigo);
            parameters.Add("@Tipo", "AS");
            parameters.Add("@Garantia", garantia);
            parameters.Add("@Porcentaje", porcentaje);
            parameters.Add("@Usuario", usuario);
            return parameters;
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

    }

    internal sealed class FrmCOAntiguedadTiposConsultaParams
    {
        public DynamicParameters Parametros { get; init; } = new();
        public bool TieneFiltro { get; init; }
        public string SortField { get; init; } = "COD_ANTIGUEDAD";
        public string SortOrder { get; init; } = "ASC";
    }
}
