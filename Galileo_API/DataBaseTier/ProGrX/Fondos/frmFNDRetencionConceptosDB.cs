using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndRetencionConceptosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 18; 

        private const string SqlConceptosBase = @"
                            SELECT
                                C.RETENCION_CODIGO AS RetencionCodigo,
                                C.descripcion AS Descripcion,
                                C.Activo,
                                C.cod_Cuenta AS CodCuenta,
                                CntX.cod_Cuenta_Mask AS CuentaMask,
                                CntX.descripcion AS CtaDesc
                            FROM dbo.FND_RETENCION_CONCEPTOS C
                            LEFT JOIN dbo.CntX_cuentas CntX
                                ON CntX.cod_Cuenta = C.cod_cuenta
                               AND CntX.cod_contabilidad = @enlace
                            WHERE @hasFilter = 0 OR
                            (
                                C.RETENCION_CODIGO LIKE @filtro OR
                                C.descripcion LIKE @filtro OR
                                C.cod_Cuenta LIKE @filtro
                            )";

        private const string SqlConceptosLista = SqlConceptosBase + @"
                            ORDER BY
                                CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN C.RETENCION_CODIGO END ASC,
                                CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN C.RETENCION_CODIGO END DESC,
                                CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN C.descripcion END ASC,
                                CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN C.descripcion END DESC,
                                C.RETENCION_CODIGO ASC
                            OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlConceptosExporta = SqlConceptosBase + @"
                            ORDER BY C.RETENCION_CODIGO;";

        private const string SqlConceptosTotal = @"
                            SELECT COUNT(1)
                            FROM dbo.FND_RETENCION_CONCEPTOS C
                            WHERE @hasFilter = 0 OR
                            (
                                C.RETENCION_CODIGO LIKE @filtro OR
                                C.descripcion LIKE @filtro OR
                                C.cod_Cuenta LIKE @filtro
                            );";

        private const string SqlExisteConcepto = @"
                            SELECT ISNULL(COUNT(1), 0)
                            FROM dbo.FND_RETENCION_CONCEPTOS
                            WHERE UPPER(RETENCION_CODIGO) = @codigo;";

        private const string SqlInsertConcepto = @"
                            INSERT INTO dbo.FND_RETENCION_CONCEPTOS
                            (
                                RETENCION_CODIGO,
                                descripcion,
                                Activo,
                                cod_cuenta
                            )
                            VALUES
                            (
                                @RetencionCodigo,
                                @Descripcion,
                                @Activo,
                                @CodCuenta
                            );";

        private const string SqlUpdateConcepto = @"
                            UPDATE dbo.FND_RETENCION_CONCEPTOS
                            SET descripcion = @Descripcion,
                                Activo = @Activo,
                                cod_cuenta = @CodCuenta
                            WHERE RETENCION_CODIGO = @RetencionCodigo;";

        private const string SqlDeleteConcepto = @"
                            DELETE FROM dbo.FND_RETENCION_CONCEPTOS
                            WHERE RETENCION_CODIGO = @RetencionCodigo;";

        private static readonly IReadOnlyDictionary<string, int> SortMap = new Dictionary<string, int>
        {
            ["RETENCION_CODIGO"] = 1,
            ["RetencionCodigo"] = 1,
            ["Descripcion"] = 2,
            ["descripcion"] = 2
        };
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmFndRetencionConceptosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de conceptos de retención sin paginación, con filtros aplicados (exportar).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="enlace"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<FndRetencionConceptoData>> FND_RetencionConceptos_Obtener(int CodEmpresa, string enlace, Models.FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, SortMap, "RETENCION_CODIGO");

            return DbHelper.ExecuteListQuery<FndRetencionConceptoData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlConceptosExporta,
                MergeParametros(spec, enlace));
        }
        
        /// <summary>
        /// Obtiene la lista de conceptos de retención con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FndRetencionConceptoLista> FND_RetencionConceptosLista_Obtener(int CodEmpresa, string enlace, Models.FiltrosLazyLoadData filtros)
        {
            var result = DbHelper.CreateOkResponse(new FndRetencionConceptoLista
            {
                Total = 0,
                Lista = new List<FndRetencionConceptoData>()
            });

            try
            {
                var spec = LazyLoadHelper.Build(filtros, SortMap, "RETENCION_CODIGO");

                var data = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                {
                    var total = connection.QueryFirstOrDefault<int>(SqlConceptosTotal, MergeParametros(spec, enlace));
                    var lista = connection.Query<FndRetencionConceptoData>(SqlConceptosLista, MergeParametros(spec, enlace)).ToList();

                    return new FndRetencionConceptoLista
                    {
                        Total = total,
                        Lista = lista
                    };
                });

                if (data.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        data.Description ?? "Error al obtener conceptos de retención.",
                        data.Code.GetValueOrDefault(-1),
                        new FndRetencionConceptoLista());
                }

                result.Result = data.Result ?? new FndRetencionConceptoLista();
            }
            catch (Exception ex)
            {
                result = DbHelper.CreateErrorResponse(ex.Message, -1, new FndRetencionConceptoLista());
            }

            return result;
        }

        /// <summary>
        /// Inserta o actualiza un concepto de retención.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        public ErrorDto FND_RetencionConceptos_Guardar(int CodEmpresa, string usuario, FndRetencionConceptoData concepto)
        {
            if (concepto is null)
            {
                return DbHelper.ErrorResponse("Los datos del concepto son requeridos.", -2);
            }

            var codigo = NormalizarTexto(concepto.RetencionCodigo).ToUpperInvariant();

            var existe = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlExisteConcepto,
                0,
                new { codigo });

            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al validar concepto.", existe.Code.GetValueOrDefault(-1));
            }

            if (concepto.isNew)
            {
                if (existe.Result > 0)
                {
                    return DbHelper.ErrorResponse($"El concepto con el código {codigo} ya existe.", -2);
                }

                return FND_RetencionConceptos_Insertar(CodEmpresa, usuario, concepto);
            }

            if (existe.Result == 0)
            {
                return DbHelper.ErrorResponse($"El concepto con el código {codigo} no existe.", -2);
            }

            return FND_RetencionConceptos_Actualizar(CodEmpresa, usuario, concepto);
        }

        /// <summary>
        /// Inserta un nuevo concepto de retención.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        private ErrorDto FND_RetencionConceptos_Insertar(int CodEmpresa, string usuario, FndRetencionConceptoData concepto)
        {
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlInsertConcepto,
                CrearParametrosConcepto(concepto));

            if (result.Code == 0)
            {
                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Retención Doc.: {NormalizarTexto(concepto.RetencionCodigo)} - {NormalizarTexto(concepto.Descripcion)}",
                    "Registra - WEB");
            }

            return result;
        }

        /// <summary>
        /// Actualiza un concepto de retención existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        private ErrorDto FND_RetencionConceptos_Actualizar(int CodEmpresa, string usuario, FndRetencionConceptoData concepto)
        {
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlUpdateConcepto,
                CrearParametrosConcepto(concepto));

            if (result.Code == 0)
            {
                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Retención Doc.: {NormalizarTexto(concepto.RetencionCodigo)} - {NormalizarTexto(concepto.Descripcion)}",
                    "Modifica - WEB");
            }

            return result;
        }

        /// <summary>
        /// Elimina un concepto de retención por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="retencionCodigo"></param>
        /// <returns></returns>
        public ErrorDto FND_RetencionConceptos_Eliminar(int CodEmpresa, string usuario, string retencionCodigo)
        {
            var codigo = NormalizarTexto(retencionCodigo).ToUpperInvariant();

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlDeleteConcepto,
                new { RetencionCodigo = codigo });

            if (result.Code == 0)
            {
                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Retención Doc.: {codigo}",
                    "Elimina - WEB");
            }

            return result;
        }

        /// <summary>
        /// Valida si un código de concepto de retención ya existe.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="retencionCodigo"></param>
        /// <returns></returns>
        public ErrorDto FND_RetencionConceptos_Valida(int CodEmpresa, string retencionCodigo)
        {
            var codigo = NormalizarTexto(retencionCodigo).ToUpperInvariant();

            var existe = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlExisteConcepto,
                0,
                new { codigo });

            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al validar concepto.", existe.Code.GetValueOrDefault(-1));
            }

            return existe.Result > 0
                ? DbHelper.ErrorResponse("El código de concepto de retención ya existe.", -1)
                : DbHelper.OkResponse("El código de concepto de retención es válido.");
        }
        private static object MergeParametros(LazyLoadSpec spec, string enlace)
        {
            return new
            {
                hasFilter = spec.HasFilter ? 1 : 0,
                filtro = spec.HasFilter ? spec.Params.Get<string>("@filtro") : null,
                sortCode = spec.SortCode,
                isAsc = spec.IsAsc ? 1 : 0,
                offset = spec.Offset,
                fetch = spec.PageSize,
                enlace = NormalizarTexto(enlace)
            };
        }

        private static object CrearParametrosConcepto(FndRetencionConceptoData concepto)
        {
            return new
            {
                RetencionCodigo = NormalizarTexto(concepto.RetencionCodigo).ToUpperInvariant(),
                Descripcion = NormalizarTexto(concepto.Descripcion),
                Activo = concepto.Activo ? 1 : 0,
                CodCuenta = NormalizarTexto(concepto.CodCuenta)
            };
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}