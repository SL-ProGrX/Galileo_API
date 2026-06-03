using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAfSuspendidosGestionDb
    {
        private readonly IConfiguration _config;

        public FrmAfSuspendidosGestionDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtener información de Bitacora
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfSuspendidosBitacoraDto>> AF_Suspendidos_Bitacora_Obtener(int CodEmpresa, AfSuspendidosGestionFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de bitácora de suspendidos son requeridos.", -2, new List<AfSuspendidosBitacoraDto>());
            }

            var fechaInicio = filtros.inicio.Date;
            var fechaCorte = filtros.corte.Date.AddDays(1).AddTicks(-1);
            if (filtros.todas_fechas)
            {
                fechaInicio = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
                fechaCorte = new DateTime(2200, 1, 1, 23, 59, 59, DateTimeKind.Unspecified);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfSuspendidosBitacoraDto>(
                    "spPAT_AsociadosSinAportes_Bitacora",
                    new
                    {
                        Cedula = filtros.cedula,
                        Inicio = fechaInicio,
                        Corte = fechaCorte
                    },
                    commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfSuspendidosBitacoraDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener bitácora de suspendidos.", result.Code.GetValueOrDefault(-1), new List<AfSuspendidosBitacoraDto>());
        }

        /// <summary>
        /// Registrar Gestión de Suspendidos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <param name="Accion"></param>
        /// <param name="Notas"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Suspendidos_Gestion_Registrar(int CodEmpresa, string Cedula, int Accion, string Notas, string Usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "spPAT_AsociadosSinAportes_Gestion",
                    new
                    {
                        Cedula,
                        Accion,
                        Notas,
                        Usuario
                    },
                    commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar gestión de suspendidos.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Cargar Archivo de Suspendidos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Valor"></param>
        /// <param name="Usuario"></param>
        /// <param name="Lista"></param>
        /// <returns></returns>
        public ErrorDto<List<AfSuspendidosArchivoDto>> AF_Suspendidos_Archivo_Cargar(int CodEmpresa, int Valor, string Usuario, List<AfSuspendidosArchivoDto> Lista)
        {
            if (Lista is null)
            {
                return DbHelper.CreateErrorResponse("La lista del archivo de suspendidos es requerida.", -2, new List<AfSuspendidosArchivoDto>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                int pLinea = 0;
                int pClean = 1;

                foreach (var item in Lista)
                {
                    pLinea++;
                    if (pLinea != 1)
                    {
                        pClean = 0;
                    }

                    connection.Execute(
                        "spPAT_AsociadosSinAportes_Carga_Masiva",
                        new
                        {
                            Tipo = "A",
                            ProcesoId = "AFI-SUSP",
                            Usuario,
                            Llave01 = item.cedula.Trim(),
                            Llave02 = pLinea,
                            Ref_01 = item.nombre,
                            Clean = pClean
                        },
                        commandType: CommandType.StoredProcedure);
                }

                return connection.Query<AfSuspendidosArchivoDto>(
                    "spPAT_AsociadosSinAportes_Carga_Masiva_Consulta",
                    new
                    {
                        Tipo = "A",
                        ProcesoId = "AFI-SUSP",
                        Usuario,
                        Valor
                    },
                    commandType: CommandType.StoredProcedure).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfSuspendidosArchivoDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al cargar archivo de suspendidos.", result.Code.GetValueOrDefault(-1), new List<AfSuspendidosArchivoDto>());
        }

        /// <summary>
        /// Procesar/Aplicar Archivo de Suspendidos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Valor"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Suspendidos_Archivo_Procesar(int CodEmpresa, int Valor, string Usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "spPAT_AsociadosSinAportes_Carga_Masiva_Procesa",
                    new
                    {
                        Tipo = "A",
                        ProcesoId = "AFI-SUSP",
                        Usuario,
                        Valor
                    },
                    commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al procesar archivo de suspendidos.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtener lista de personas suspendidas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> AF_Suspendidos_Personas_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de personas suspendidas son requeridos.", -2, new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<AFCedulaDto>()
                });
            }

            var resultadoVacio = new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<AFCedulaDto>()
            };

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var salida = new TablasListaGenericaModel
                {
                    total = connection.Query<int>("SELECT count(*) From socios where estadoactual = 'S'").FirstOrDefault(),
                    lista = new List<AFCedulaDto>()
                };

                var filtroTexto = filtro.filtro?.Trim();
                var sortField = ObtenerSortFieldSuspendidos(filtro.sortField);
                var sortDirection = ObtenerSortDirectionSuspendidos(filtro.sortOrder);
                var offsetRows = filtro.pagina;
                var fetchRows = filtro.paginacion;

                var sql = @"
                    SELECT cedula, cedulaR, nombre
                    FROM socios
                    WHERE estadoactual = 'S'
                      AND (
                        @Filtro IS NULL
                        OR cedula LIKE @Filtro
                        OR cedulaR LIKE @Filtro
                        OR nombre LIKE @Filtro
                      )
                    ORDER BY
                        CASE WHEN @SortField = 'cedula' AND @SortDirection = 'ASC' THEN cedula END ASC,
                        CASE WHEN @SortField = 'cedula' AND @SortDirection = 'DESC' THEN cedula END DESC,
                        CASE WHEN @SortField = 'cedulaR' AND @SortDirection = 'ASC' THEN cedulaR END ASC,
                        CASE WHEN @SortField = 'cedulaR' AND @SortDirection = 'DESC' THEN cedulaR END DESC,
                        CASE WHEN @SortField = 'nombre' AND @SortDirection = 'ASC' THEN nombre END ASC,
                        CASE WHEN @SortField = 'nombre' AND @SortDirection = 'DESC' THEN nombre END DESC,
                        cedula ASC";

                if (fetchRows > 0)
                {
                    sql += " OFFSET @OffsetRows ROWS FETCH NEXT @FetchRows ROWS ONLY";
                }

                var parametros = new DynamicParameters();
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%");
                parametros.Add("SortField", sortField);
                parametros.Add("SortDirection", sortDirection);
                parametros.Add("OffsetRows", offsetRows);
                parametros.Add("FetchRows", fetchRows);

                salida.lista = connection.Query<AFCedulaDto>(sql, parametros).ToList();
                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? resultadoVacio)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener personas suspendidas.", result.Code.GetValueOrDefault(-1), resultadoVacio);
        }

        private static string ObtenerSortFieldSuspendidos(string? sortField)
        {
            return sortField switch
            {
                "cedula" => "cedula",
                "cedulaR" => "cedulaR",
                "nombre" => "nombre",
                _ => "cedula"
            };
        }

        private static string ObtenerSortDirectionSuspendidos(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
