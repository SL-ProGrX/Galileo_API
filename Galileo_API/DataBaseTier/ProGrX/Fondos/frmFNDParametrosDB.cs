using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndParametrosDB
    {
        private readonly IConfiguration _config;
        private readonly MCntLinkDB mCntLinkDB;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 18;

        private const string SpParametrosInicializa = "spFndParametros";

        private const string SqlParametrosLista = @"
                    SELECT COUNT(1)
                    FROM dbo.FND_Parametros
                    WHERE @hasFilter = 0 OR
                    (
                        cod_parametro LIKE @filtro OR
                        descripcion LIKE @filtro OR
                        valor LIKE @filtro OR
                        tipo LIKE @filtro OR
                        modifica_usuario LIKE @filtro
                    );

                    SELECT
                        cod_parametro,
                        descripcion,
                        valor,
                        tipo,
                        modifica_usuario,
                        modifica_fecha
                    FROM dbo.FND_Parametros
                    WHERE @hasFilter = 0 OR
                    (
                        cod_parametro LIKE @filtro OR
                        descripcion LIKE @filtro OR
                        valor LIKE @filtro OR
                        tipo LIKE @filtro OR
                        modifica_usuario LIKE @filtro
                    )
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN cod_parametro END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN cod_parametro END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN valor END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN valor END DESC,
                        CASE WHEN @sortCode = 4 AND @isAsc = 1 THEN tipo END ASC,
                        CASE WHEN @sortCode = 4 AND @isAsc = 0 THEN tipo END DESC,
                        cod_parametro ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlParametrosExporta = @"
                    SELECT
                        cod_parametro,
                        descripcion,
                        valor,
                        tipo,
                        modifica_usuario,
                        modifica_fecha
                    FROM dbo.FND_Parametros
                    WHERE @hasFilter = 0 OR
                    (
                        cod_parametro LIKE @filtro OR
                        descripcion LIKE @filtro OR
                        valor LIKE @filtro OR
                        tipo LIKE @filtro OR
                        modifica_usuario LIKE @filtro
                    )
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN cod_parametro END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN cod_parametro END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN valor END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN valor END DESC,
                        CASE WHEN @sortCode = 4 AND @isAsc = 1 THEN tipo END ASC,
                        CASE WHEN @sortCode = 4 AND @isAsc = 0 THEN tipo END DESC,
                        cod_parametro ASC;";

        private const string SqlParametroActualizar = @"
                    UPDATE dbo.FND_Parametros
                    SET modifica_usuario = @Usuario,
                        modifica_Fecha = dbo.MyGetdate(),
                        valor = @Valor
                    WHERE cod_parametro = @Parametro;";

        private static readonly IReadOnlyDictionary<string, int> ParametrosSortMap = new Dictionary<string, int>
        {
            ["cod_parametro"] = 1,
            ["descripcion"] = 2,
            ["valor"] = 3,
            ["tipo"] = 4
        };

        public FrmFndParametrosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            mCntLinkDB = new MCntLinkDB(_config);
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Método para obtener los parámetros de fondos con filtros, paginación y ordenamiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="exporta"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> Fnd_Parametros_Obtener(int CodEmpresa, bool exporta, int cod_contabilidad,  FiltrosLazyLoadData filtro)
        {
            var response = DbHelper.CreateOkResponse(new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<FndParametrosDto>()
            });

            try
            {
                InicializarParametros(CodEmpresa);

                var spec = LazyLoadHelper.Build(filtro, ParametrosSortMap, "cod_parametro");
                var queryResult = exporta
                    ? ObtenerParametrosExportacion(CodEmpresa, spec)
                    : ObtenerParametrosPaginados(CodEmpresa, spec);

                if (queryResult.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        queryResult.Description ?? "Error al obtener parámetros de fondos.",
                        queryResult.Code.GetValueOrDefault(-1),
                        new TablasListaGenericaModel { total = 0, lista = new List<FndParametrosDto>() });
                }

                var data = queryResult.Result ?? new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<FndParametrosDto>()
                };
                var listaParametros = data.lista as List<FndParametrosDto> ?? new List<FndParametrosDto>();
                CompletarDatosCuenta(CodEmpresa, cod_contabilidad, listaParametros);
                data.lista = listaParametros;
                response.Result = data;
            }
            catch (Exception ex)
            {
                response = DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new TablasListaGenericaModel { total = 0, lista = new List<FndParametrosDto>() });
            }

            return response;
        }

        /// <summary>
        /// Método para guardar los cambios realizados a un parámetro de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Parametros_Guardar(int CodEmpresa, string usuario, FndParametrosDto data)
        {
            if (data is null)
            {
                return DbHelper.ErrorResponse("Los datos del parámetro son requeridos.", -2);
            }

            var validationResult = ValidateParametro(CodEmpresa, data);
            if (!validationResult.IsValid)
            {
                return DbHelper.ErrorResponse(validationResult.Message, -1);
            }

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlParametroActualizar,
                new
                {
                    Usuario = NormalizarTexto(usuario),
                    Valor = NormalizarTexto(data.valor),
                    Parametro = NormalizarTexto(data.cod_parametro)
                });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(CodEmpresa, usuario, data);
            return result;
        }


        private void InicializarParametros(int codEmpresa)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
                connection.Execute(
                    SpParametrosInicializa,
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                throw new InvalidOperationException(result.Description ?? "Error al inicializar parámetros de fondos.");
            }
        }

        private ErrorDto<TablasListaGenericaModel> ObtenerParametrosPaginados(int codEmpresa, LazyLoadSpec spec)
        {
            return DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
            {
                using var multi = connection.QueryMultiple(SqlParametrosLista, spec.Params);
                return new TablasListaGenericaModel
                {
                    total = multi.ReadFirstOrDefault<int>(),
                    lista = multi.Read<FndParametrosDto>().ToList()
                };
            });
        }

        private ErrorDto<TablasListaGenericaModel> ObtenerParametrosExportacion(int codEmpresa, LazyLoadSpec spec)
        {
            return DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
            {
                var lista = connection.Query<FndParametrosDto>(SqlParametrosExporta, spec.Params).ToList();
                return new TablasListaGenericaModel
                {
                    total = lista.Count,
                    lista = lista
                };
            });
        }

        private void CompletarDatosCuenta(int codEmpresa, int codContabilidad, List<FndParametrosDto> lista)
        {
            foreach (var item in lista.Where(x => string.Equals(x.tipo, "CTA", StringComparison.OrdinalIgnoreCase)))
            {
                item.valorCuenta = mCntLinkDB.fxgCntCuentaFormato(codEmpresa, false, item.valor, 0);
                item.cuentaDesc = mCntLinkDB.fxgCntCuentaDesc(codEmpresa, item.valor, codContabilidad);
            }
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, FndParametrosDto data)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = $"Parámetro de Fondos: {NormalizarTexto(data.cod_parametro)} - {NormalizarTexto(data.descripcion)} -> {NormalizarTexto(data.valor)}",
                Movimiento = "Modifica - WEB",
                Modulo = vModulo
            });
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private (bool IsValid, string Message) ValidateParametro(int CodEmpresa, FndParametrosDto data)
        {
            if (data.tipo == null)
                return (false, "Tipo de parámetro no especificado.");

            string tipo = data.tipo.Trim().ToUpper();
            switch (tipo)
            {
                case "DEC":
                    return ValidateDecimal(data);
                case "NUM":
                    return ValidateNumero(data);
                case "POR":
                    return ValidatePorcentaje(data);
                case "CTA":
                    return ValidateCuenta(CodEmpresa, data);
                case "CHR":
                    return ValidateCaracteres(data);
                case "PSN":
                    return ValidatePreguntaSN(data);
                case "DTS":
                    return ValidateFecha(data);
                default:
                    return (true, "");
            }
        }

        private static (bool, string) ValidateDecimal(FndParametrosDto data)
        {
            if (decimal.TryParse(data.valor, out var decValor))
            {
                data.valor = decValor.ToString();
                return (true, "");
            }
            return (false, "El valor indicado no es válido...!!!");
        }

        private static(bool, string) ValidateNumero(FndParametrosDto data)
        {
            if (long.TryParse(data.valor, out var numValor))
            {
                data.valor = numValor.ToString();
                return (true, "");
            }
            return (false, "El valor indicado no es válido...!!!");
        }

        private static(bool, string) ValidatePorcentaje(FndParametrosDto data)
        {
            if (decimal.TryParse(data.valor, out var porValor))
            {
                data.valor = porValor.ToString();
                return (true, "");
            }
            return (false, "El valor indicado no es válido, suministre un porcentaje ..!!!");
        }

        private (bool, string) ValidateCuenta(int CodEmpresa, FndParametrosDto data)
        {
            var cuentaFormateada = mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, false, data.valor, 0);
            bool isValid = mCntLinkDB.fxgCntCuentaValida(CodEmpresa, cuentaFormateada);
            if (!isValid)
            {
                return (false, "La Cuenta indicada no es válida, presione F4 para buscar en el catálogo...!!!");
            }
            data.valor = cuentaFormateada;
            return (true, "");
        }

        private static (bool, string) ValidateCaracteres(FndParametrosDto data)
        {
            if (NormalizarTexto(data.valor).Contains("'", StringComparison.Ordinal))
            {
                return (false, "El valor indicado contiene caracteres no válidos...!!!");
            }
            return (true, "");
        }

        private static(bool, string) ValidatePreguntaSN(FndParametrosDto data)
        {
            var valor = NormalizarTexto(data.valor);
            if (!string.IsNullOrEmpty(valor))
            {
                var c = valor[..1].ToUpperInvariant();
                if (c == "S" || c == "N")
                {
                    data.valor = c;
                    return (true, "");
                }
                return (false, "El valor indicado no es válido > Indique [S] ó [N]...!!!");
            }
            return (false, "El valor indicado no es válido > Indique [S] ó [N]...!!!");
        }

        private static(bool, string) ValidateFecha(FndParametrosDto data)
        {
            if (DateTime.TryParse(NormalizarTexto(data.valor), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var fecha))
            {
                data.valor = fecha.ToString("yyyy/MM/dd");
                return (true, "");
            }
            return (false, "La Fecha indicada no es válida...!!!");
        }

    }
}
