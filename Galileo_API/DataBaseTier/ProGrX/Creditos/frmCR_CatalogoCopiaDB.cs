using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCatalogoCopiaDB
    {
        private const int ModuloCreditos = 3;
        private const int ScrollSiguiente = 1;

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MCobroDb _mCobroDb;

        private const string SqlLineasActivas = @"
            select
                rtrim(codigo) as codigo,
                rtrim(descripcion) as descripcion
            from catalogo
            where activo = 1
            order by codigo;";

        private const string SqlLineasF4 = @"
            select
                rtrim(codigo) as codigo,
                rtrim(descripcion) as descripcion
            from catalogo
            where retencion = 'N'
              and poliza = 'N'
              and (
                    @texto = ''
                 or codigo like @like
                 or descripcion like @like
              )
            order by codigo;";

        private const string SqlExisteLinea = @"
            select count(1)
            from catalogo
            where codigo = @codigo;";

        private const string SqlLineaScrollNext = @"
            select top 1
                rtrim(codigo) as codigo,
                rtrim(descripcion) as descripcion
            from catalogo
            where retencion = 'N'
              and poliza = 'N'
              and codigo > @codigo
            order by codigo asc;";

        private const string SqlLineaScrollPrev = @"
            select top 1
                rtrim(codigo) as codigo,
                rtrim(descripcion) as descripcion
            from catalogo
            where retencion = 'N'
              and poliza = 'N'
              and codigo < @codigo
            order by codigo desc;";

        public FrmCrCatalogoCopiaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mCobroDb = new MCobroDb(config);
        }

        /// <summary>
        /// Registra movimientos de la pantalla en bitácora.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene las líneas activas disponibles como destino de copia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCopiaLineaDto>> CR_CatalogoCopia_Lineas_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
                conn.Query<CrCatalogoCopiaLineaDto>(SqlLineasActivas).ToList());
        }

        /// <summary>
        /// Obtiene las líneas disponibles para búsqueda F4 de línea base.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCopiaLineaDto>> CR_CatalogoCopia_Lineas_F4_Obtener(int CodEmpresa, string? texto)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var filtro = NormalizeCode(texto);
                var like = filtro.Length > 0 ? $"%{filtro}%" : null;

                return conn.Query<CrCatalogoCopiaLineaDto>(
                    SqlLineasF4,
                    new
                    {
                        texto = filtro,
                        like
                    }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la descripción de una línea de crédito por código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CrCatalogoCopiaDescripcionDto> CR_CatalogoCopia_Linea_Descripcion_Obtener(int CodEmpresa, string codigo)
        {
            var response = DbHelper.CreateOkResponse(new CrCatalogoCopiaDescripcionDto());

            try
            {
                var codigoNormalizado = NormalizeCode(codigo);
                var descripcion = _mCobroDb.fxDescribeCodigo(CodEmpresa, codigoNormalizado);

                response.Result = new CrCatalogoCopiaDescripcionDto
                {
                    codigo = codigoNormalizado,
                    descripcion = descripcion
                };

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrCatalogoCopiaDescripcionDto>(
                    ex.Message,
                    -1,
                    new CrCatalogoCopiaDescripcionDto());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrCatalogoCopiaDescripcionDto>(
                    ex.Message,
                    -1,
                    new CrCatalogoCopiaDescripcionDto());
            }
        }

        /// <summary>
        /// Navega entre líneas de crédito válidas para línea base.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scroll"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CrCatalogoCopiaScrollDto> CR_CatalogoCopia_Linea_Scroll_Obtener(int CodEmpresa, int scroll, string? codigo)
        {
            var response = DbHelper.CreateOkResponse(new CrCatalogoCopiaScrollDto());

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var sql = scroll == ScrollSiguiente ? SqlLineaScrollNext : SqlLineaScrollPrev;

                response.Result = conn.QueryFirstOrDefault<CrCatalogoCopiaScrollDto>(
                    sql,
                    new { codigo = NormalizeCode(codigo) });

                if (response.Result == null)
                {
                    response.Code = -2;
                    response.Description = "No se encontraron más resultados.";
                }
                else
                {
                    response.Description = "Ok";
                }
            }
            catch (SqlException ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            catch (InvalidOperationException)
            {
                SetScrollError(response);
            }
            catch (ArgumentException)
            {
                SetScrollError(response);
            }

            return response;
        }

        private static void SetScrollError(ErrorDto<CrCatalogoCopiaScrollDto> response)
        {
            response.Code = -1;
            response.Description = "Ocurrió un error inesperado al obtener la línea de crédito.";
            response.Result = null;
        }
        /// <summary>
        /// Copia la configuración de una línea base hacia líneas destino o una nueva línea.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrCatalogoCopiaResultadoDto> CR_CatalogoCopia_Copiar(int CodEmpresa, CrCatalogoCopiaRequest request)
        {
            var response = DbHelper.CreateOkResponse(new CrCatalogoCopiaResultadoDto());

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var validation = ValidarRequest(conn, request);
                if (validation.Code != 0)
                    return validation;

                var data = NormalizarRequest(request);
                var destinos = ResolverDestinos(data);

                response.Result.detalle.AddRange(
                    destinos
                        .Select(destino => CopiarDestino(conn, CodEmpresa, data, destino))
                        .ToList());

                response.Result.total_procesadas = response.Result.detalle.Count(x => x.procesada);
                response.Description = "Copia Realizada Satisfactoriamente.";

                return response;
            }
            catch (SqlException ex)
            {
                return CrearErrorSqlCopia(ex);
            }
            catch (InvalidOperationException)
            {
                return CrearErrorCopiaGenerico();
            }
            catch (ArgumentException)
            {
                return CrearErrorCopiaGenerico();
            }
        }
        private static ErrorDto<CrCatalogoCopiaResultadoDto> CrearErrorCopiaGenerico()
        {
            return DbHelper.CreateErrorResponse<CrCatalogoCopiaResultadoDto>(
                "No se pudo completar la copia de configuración. Verifique los datos e intente nuevamente.",
                -1,
                new CrCatalogoCopiaResultadoDto());
        }
        private static ErrorDto<CrCatalogoCopiaResultadoDto> CrearErrorSqlCopia(SqlException ex)
        {
            return DbHelper.CreateErrorResponse<CrCatalogoCopiaResultadoDto>(
                GetMensajeSqlAmigable(ex),
                -2,
                new CrCatalogoCopiaResultadoDto());
        }

        private static string GetMensajeSqlAmigable(SqlException ex)
        {
            var msg = ex.Message ?? string.Empty;

            if (ex.Number == 547 && msg.Contains("CATALOGO_DESTINOSASG", StringComparison.OrdinalIgnoreCase))
            {
                return "No se pueden reemplazar los destinos asignados porque una o más líneas destino tienen información relacionada.";
            }

            if (ex.Number == 547)
            {
                return "No se puede completar la copia porque una o más líneas destino tienen información relacionada.";
            }

            return "No se pudo completar la copia de configuración. Verifique los datos e intente nuevamente.";
        }

        private static CrCatalogoCopiaRequest NormalizarRequest(CrCatalogoCopiaRequest request)
        {
            request.linea_origen = NormalizeCode(request.linea_origen);
            request.usuario = NormalizeUser(request.usuario);
            request.flags ??= new CrCatalogoCopiaFlagsDto();
            request.lineas_destino ??= new List<string>();

            request.lineas_destino = request.lineas_destino
                .Select(NormalizeCode)
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (request.nueva_linea != null)
            {
                request.nueva_linea.codigo = NormalizeCode(request.nueva_linea.codigo);
                request.nueva_linea.descripcion = NormalizeText(request.nueva_linea.descripcion);
            }

            return request;
        }

        private static ErrorDto<CrCatalogoCopiaResultadoDto> ValidarRequest(
            IDbConnection conn,
            CrCatalogoCopiaRequest request)
        {
            var data = NormalizarRequest(request);

            if (string.IsNullOrWhiteSpace(data.linea_origen))
                return CrearValidacion("Debe indicar la línea base.");

            if (string.IsNullOrWhiteSpace(data.usuario))
                return CrearValidacion("Debe indicar el usuario.");

            if (!ExisteLinea(conn, data.linea_origen))
                return CrearValidacion($"La línea base {data.linea_origen} no existe.");

            if (!TieneDestinos(data))
                return CrearValidacion("Debe seleccionar al menos una línea destino o indicar una nueva línea.");

            return DbHelper.CreateOkResponse(new CrCatalogoCopiaResultadoDto());
        }

        private static bool TieneDestinos(CrCatalogoCopiaRequest data)
        {
            return data.lineas_destino.Count > 0 || TieneNuevaLineaCompleta(data);
        }

        private static bool TieneNuevaLineaCompleta(CrCatalogoCopiaRequest data)
        {
            return data.nueva_linea != null
                && !string.IsNullOrWhiteSpace(data.nueva_linea.codigo);
        }

        private static ErrorDto<CrCatalogoCopiaResultadoDto> CrearValidacion(string mensaje)
        {
            return new ErrorDto<CrCatalogoCopiaResultadoDto>
            {
                Code = -2,
                Description = mensaje,
                Result = new CrCatalogoCopiaResultadoDto()
            };
        }

        private static bool ExisteLinea(IDbConnection conn, string codigo)
        {
            var total = conn.QueryFirstOrDefault<int>(SqlExisteLinea, new { codigo });
            return total > 0;
        }

        private static List<CrCatalogoCopiaDestinoDto> ResolverDestinos(CrCatalogoCopiaRequest data)
        {
            var destinos = data.lineas_destino
                .Where(x => !IgualCodigo(x, data.linea_origen))
                .Select(x => new CrCatalogoCopiaDestinoDto
                {
                    codigo = x,
                    descripcion = string.Empty,
                    es_nueva = false
                })
                .ToList();

            if (TieneNuevaLineaCompleta(data) && !IgualCodigo(data.nueva_linea.codigo, data.linea_origen))
            {
                destinos.Add(new CrCatalogoCopiaDestinoDto
                {
                    codigo = data.nueva_linea.codigo,
                    descripcion = data.nueva_linea.descripcion,
                    es_nueva = true
                });
            }

            return destinos
                .GroupBy(x => x.codigo, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToList();
        }

        private CrCatalogoCopiaResultadoItemDto CopiarDestino(
            IDbConnection conn,
            int CodEmpresa,
            CrCatalogoCopiaRequest data,
            CrCatalogoCopiaDestinoDto destino)
        {
            EjecutarSpCopia(conn, data, destino);
            RegistrarBitacora(CodEmpresa, data, destino);

            return new CrCatalogoCopiaResultadoItemDto
            {
                linea_destino = destino.codigo,
                descripcion = destino.descripcion,
                es_nueva = destino.es_nueva,
                procesada = true,
                mensaje = "Procesada correctamente."
            };
        }

        private static void EjecutarSpCopia(
            IDbConnection conn,
            CrCatalogoCopiaRequest data,
            CrCatalogoCopiaDestinoDto destino)
        {
            var p = CrearParametrosSp(data, destino);

            conn.Execute(
                "spCrdLineaCreditoCopia",
                p,
                commandType: CommandType.StoredProcedure);
        }

        private static DynamicParameters CrearParametrosSp(
            CrCatalogoCopiaRequest data,
            CrCatalogoCopiaDestinoDto destino)
        {
            var flags = data.flags;

            var p = new DynamicParameters();
            p.Add("@Origen", data.linea_origen);
            p.Add("@Destino", destino.codigo);
            p.Add("@Usuario", data.usuario);
            p.Add("@General", ToSmallInt(flags.general));
            p.Add("@Cuentas", ToSmallInt(flags.cuentas));
            p.Add("@Rangos", ToSmallInt(flags.rangos));
            p.Add("@Destinos", ToSmallInt(flags.destinos));
            p.Add("@Cargos", ToSmallInt(flags.cargos));
            p.Add("@Recursos", ToSmallInt(flags.recursos));
            p.Add("@Requisitos", ToSmallInt(flags.requisitos));
            p.Add("@Cobro", ToSmallInt(flags.cobro));
            p.Add("@Descripcion", destino.es_nueva ? destino.descripcion : string.Empty);
            p.Add("@Resolucion", ToSmallInt(flags.resolucion));
            p.Add("@Refundibles", ToSmallInt(flags.refundibles));
            p.Add("@Adjuntos", ToSmallInt(flags.adjuntos));

            return p;
        }

        private void RegistrarBitacora(
            int CodEmpresa,
            CrCatalogoCopiaRequest data,
            CrCatalogoCopiaDestinoDto destino)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = data.usuario,
                DetalleMovimiento = BuildDetalleBitacora(data, destino.codigo),
                Movimiento = "APLICA-WEB",
                Modulo = ModuloCreditos
            });
        }

        private static string BuildDetalleBitacora(CrCatalogoCopiaRequest data, string destino)
        {
            var f = data.flags;

            return $"Copia Línea.:{data.linea_origen} a {destino} " +
                   $"(Cg:{ToSmallInt(f.general)} " +
                   $"Cc:{ToSmallInt(f.cuentas)} " +
                   $"Da:{ToSmallInt(f.destinos)} " +
                   $"Ca:{ToSmallInt(f.cargos)} " +
                   $"Rp:{ToSmallInt(f.recursos)} " +
                   $"Ra:{ToSmallInt(f.requisitos)} " +
                   $"CCa:{ToSmallInt(f.cobro)} " +
                   $"Rmpt:{ToSmallInt(f.rangos)} " +
                   $"NivRes:{ToSmallInt(f.resolucion)} " +
                   $"Lref:{ToSmallInt(f.refundibles)} " +
                   $"Adj:{ToSmallInt(f.adjuntos)})";
        }

        private static short ToSmallInt(bool value)
        {
            return value ? (short)1 : (short)0;
        }

        private static bool IgualCodigo(string a, string b)
        {
            return string.Equals(
                NormalizeCode(a),
                NormalizeCode(b),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeCode(string? value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string NormalizeUser(string? value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string NormalizeText(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private sealed class CrCatalogoCopiaDestinoDto
        {
            public string codigo { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public bool es_nueva { get; set; } = false;
        }
    }
}