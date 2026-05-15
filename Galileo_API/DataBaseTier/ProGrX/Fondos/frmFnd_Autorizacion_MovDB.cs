using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;
using System.Data;


namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndAutorizacionMovDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 18;
        private readonly MSecurityMainDb _Security_MainDB;


        private const string IdAutorizacionField = "id_autorizacion";

        private static readonly IReadOnlyDictionary<string, int> AutorizacionSortMap = new Dictionary<string, int>
        {
            [IdAutorizacionField] = 1,
            ["ESTADO_DESC"] = 2,
            ["Cedula"] = 3,
            ["Nombre"] = 4,
            ["Cod_Plan"] = 5,
            ["Cod_Contrato"] = 6,
            ["Registro_Fecha"] = 7,
            ["Registro_Usuario"] = 8
        };

        private const string SqlAutorizaciones = @"
                    SELECT COUNT(1)
                    FROM dbo.vFnd_Gestiones_List
                    WHERE Estado = @pEstado
                      AND Registro_Fecha BETWEEN @fechaInicio AND @fechaCorte
                      AND (@usuario IS NULL OR Registro_Usuario LIKE @usuario)
                      AND (@cedula IS NULL OR Cedula LIKE @cedula)
                      AND (@nombre IS NULL OR Nombre LIKE @nombre)
                      AND dbo.fxFnd_Autorizado_Valida_Lista(@logUsuario, Registro_Usuario, Cod_Plan, Monto_Solicitado, Tipo) > 0
                      AND (@hasFilter = 0 OR
                          CONVERT(varchar(30), id_autorizacion) LIKE @filtro OR
                          ESTADO_DESC LIKE @filtro OR
                          Cedula LIKE @filtro OR
                          Nombre LIKE @filtro OR
                          Cod_Plan LIKE @filtro OR
                          CONVERT(varchar(30), Cod_Contrato) LIKE @filtro);

                    SELECT
                        id_autorizacion,
                        ESTADO_DESC,
                        Cedula,
                        Nombre,
                        Cod_Plan,
                        Cod_Contrato,
                        Registro_Fecha,
                        Registro_Usuario,
                        Monto_Solicitado,
                        Tipo,
                        Estado
                    FROM dbo.vFnd_Gestiones_List
                    WHERE Estado = @pEstado
                      AND Registro_Fecha BETWEEN @fechaInicio AND @fechaCorte
                      AND (@usuario IS NULL OR Registro_Usuario LIKE @usuario)
                      AND (@cedula IS NULL OR Cedula LIKE @cedula)
                      AND (@nombre IS NULL OR Nombre LIKE @nombre)
                      AND dbo.fxFnd_Autorizado_Valida_Lista(@logUsuario, Registro_Usuario, Cod_Plan, Monto_Solicitado, Tipo) > 0
                      AND (@hasFilter = 0 OR
                          CONVERT(varchar(30), id_autorizacion) LIKE @filtro OR
                          ESTADO_DESC LIKE @filtro OR
                          Cedula LIKE @filtro OR
                          Nombre LIKE @filtro OR
                          Cod_Plan LIKE @filtro OR
                          CONVERT(varchar(30), Cod_Contrato) LIKE @filtro)
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN id_autorizacion END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN id_autorizacion END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN ESTADO_DESC END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN ESTADO_DESC END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN Cedula END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN Cedula END DESC,
                        CASE WHEN @sortCode = 4 AND @isAsc = 1 THEN Nombre END ASC,
                        CASE WHEN @sortCode = 4 AND @isAsc = 0 THEN Nombre END DESC,
                        CASE WHEN @sortCode = 5 AND @isAsc = 1 THEN Cod_Plan END ASC,
                        CASE WHEN @sortCode = 5 AND @isAsc = 0 THEN Cod_Plan END DESC,
                        CASE WHEN @sortCode = 6 AND @isAsc = 1 THEN Cod_Contrato END ASC,
                        CASE WHEN @sortCode = 6 AND @isAsc = 0 THEN Cod_Contrato END DESC,
                        CASE WHEN @sortCode = 7 AND @isAsc = 1 THEN Registro_Fecha END ASC,
                        CASE WHEN @sortCode = 7 AND @isAsc = 0 THEN Registro_Fecha END DESC,
                        CASE WHEN @sortCode = 8 AND @isAsc = 1 THEN Registro_Usuario END ASC,
                        CASE WHEN @sortCode = 8 AND @isAsc = 0 THEN Registro_Usuario END DESC,
                        id_autorizacion ASC
                    OFFSET @offset ROWS FETCH NEXT @fetchSeguro ROWS ONLY;";

        public FrmFndAutorizacionMovDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Método para validar si un usuario es autorizador
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private ErrorDto Fnd_Autorizacion_Mov_AuthValida(int CodEmpresa, string usuario)
        {
            const string query = @"SELECT dbo.fxFnd_Autorizado_Valida(@pAutorizador) AS Estado;";

            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                query,
                0,
                new { pAutorizador = NormalizarTexto(usuario) });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al validar autorizador.");
            }

            return result.Result == 1
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse("El usuario actual no es un Autorizador de Gestiones de Fondos!");
        }

        /// <summary>
        /// Método para obtener las autorizaciones de movimientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> Fnd_Autorizacion_Mov_Obtener(int CodEmpresa, bool exporta ,FndAutorizacionMovFiltros data, FiltrosLazyLoadData filtro)
        {
            var result = DbHelper.CreateOkResponse(new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<FndAutorizacionMovData>()
            });

            try
            {
                var filtroSeguro = filtro ?? new FiltrosLazyLoadData();
                PrepararExportacion(filtroSeguro, exporta);

                var spec = LazyLoadHelper.Build(filtroSeguro, AutorizacionSortMap, "id_autorizacion");
                var parametros = CrearParametrosConsulta(data, spec);
                var queryResult = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(SqlAutorizaciones, parametros);

                    return new TablasListaGenericaModel
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<FndAutorizacionMovData>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return CrearErrorLista(queryResult.Description ?? "Error al consultar autorizaciones de movimientos.");
                }

                result.Result = queryResult.Result ?? new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<FndAutorizacionMovData>()
                };
            }
            catch (Exception ex)
            {
                result = CrearErrorLista(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Método para autorizar o denegar movimientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pGestion"></param>
        /// <param name="pAutorizador"></param>
        /// <param name="movimiento"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Autorizacion_Mov_Autoriza(int CodEmpresa, string pGestion, string pAutorizador, List<FndAutorizacionMovData> movimiento)
        {
            var valida = Fnd_Autorizacion_Mov_AuthValida(CodEmpresa, pAutorizador);
            if (valida.Code != 0)
            {
                return valida;
            }

            var movimientos = movimiento?.Where(item => item != null).ToList() ?? new List<FndAutorizacionMovData>();
            if (movimientos.Count == 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar al menos una gestión para procesar.", -2);
            }

            var resultado = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                var errores = new System.Text.StringBuilder();
                foreach (var item in movimientos)
                {
                    ProcesarAutorizacionIndividual(connection, CodEmpresa, pGestion, pAutorizador, item, errores);
                }

                return errores.ToString();
            });

            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(resultado.Description ?? "Error al procesar autorizaciones.");
            }

            if (!string.IsNullOrWhiteSpace(resultado.Result))
            {
                return DbHelper.ErrorResponse(resultado.Result);
            }

            return DbHelper.OkResponse($"La {ObtenerDescripcionGestion(pGestion)} realizada satisfactoriamente.!");
        }

        private static void PrepararExportacion(FiltrosLazyLoadData filtro, bool exporta)
        {
            if (!exporta)
            {
                return;
            }

            filtro.pagina = 0;
            filtro.paginacion = int.MaxValue;
        }

        private static DynamicParameters CrearParametrosConsulta(FndAutorizacionMovFiltros? data, LazyLoadSpec spec)
        {
            var filtros = data ?? new FndAutorizacionMovFiltros();
            var parametros = new DynamicParameters(spec.Params);
            parametros.Add("@fetchSeguro", spec.PageSize == int.MaxValue ? 2147483647 : spec.PageSize, DbType.Int32);
            parametros.Add("@pEstado", filtros.estado);
            parametros.Add("@fechaInicio", filtros.fecha_inicio.GetValueOrDefault().Date);
            parametros.Add("@fechaCorte", filtros.fecha_corte.GetValueOrDefault().Date.AddDays(1).AddSeconds(-1));
            parametros.Add("@usuario", CrearFiltroLike(filtros.usuario));
            parametros.Add("@cedula", CrearFiltroLike(filtros.cedula));
            parametros.Add("@nombre", CrearFiltroLike(filtros.nombre));
            parametros.Add("@logUsuario", NormalizarTexto(filtros.logUsuario));
            return parametros;
        }

        private static string? CrearFiltroLike(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";
        }

        private static ErrorDto<TablasListaGenericaModel> CrearErrorLista(string mensaje) =>
            DbHelper.CreateErrorResponse(mensaje, -1, new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<FndAutorizacionMovData>()
            });

        private void ProcesarAutorizacionIndividual(
            SqlConnection connection,
            int codEmpresa,
            string pGestion,
            string pAutorizador,
            FndAutorizacionMovData item,
            System.Text.StringBuilder errores)
        {
            try
            {
                connection.Execute(
                    "dbo.spFnd_Autorizaciones_Registro",
                    new
                    {
                        GestionId = item.id_autorizacion,
                        Estado = NormalizarGestion(pGestion),
                        Usuario = NormalizarTexto(pAutorizador),
                        Nota = string.Empty
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                RegistrarBitacora(codEmpresa, pAutorizador, item, ObtenerDescripcionGestion(pGestion));
            }
            catch (Exception ex)
            {
                errores.AppendLine($"Error en la gestión {item.id_autorizacion}: {ex.Message}");
            }
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, FndAutorizacionMovData item, string estado)
        {
            var bitacora = new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"{estado} de Gestión de Fondo Id: {item.id_autorizacion} ..Nombre: {item.nombre}",
                Movimiento = "Aplica - WEB",
                Modulo = vModulo
            };

            _Security_MainDB.Bitacora(bitacora);
        }

        private static string ObtenerDescripcionGestion(string? gestion) =>
            string.Equals(NormalizarGestion(gestion), "A", StringComparison.OrdinalIgnoreCase) ? "Autorización" : "Denegación";

        private static string NormalizarGestion(string? gestion) =>
            string.Equals(NormalizarTexto(gestion), "A", StringComparison.OrdinalIgnoreCase) ? "A" : "D";

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
