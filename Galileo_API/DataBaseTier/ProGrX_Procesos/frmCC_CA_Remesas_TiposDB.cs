using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Procesos
{
    public class FrmCccARemesasTiposDB
    {
        private readonly MSecurityMainDb _securityMainDb;
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 10;

        public FrmCccARemesasTiposDB(IConfiguration config)
        {
            _securityMainDb = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        private static string NormalizeText(string? value)
            => (value ?? string.Empty).Trim();

        private static IEnumerable<CcCaRemesasTiposData> ApplyFiltro(
            IEnumerable<CcCaRemesasTiposData> source,
            FiltrosLazyLoadData filtros)
        {
            var texto = NormalizeText(filtros?.filtro);

            if (string.IsNullOrWhiteSpace(texto))
                return source;

            return source.Where(x =>
                   (x.cod_remesa?.ToString() ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.descripcion ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.registro_usuario ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.modifica_usuario ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.cod_entidad ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.activo ? "ACTIVO" : "INACTIVO").Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<CcCaRemesasTiposData> ApplySort(
            IEnumerable<CcCaRemesasTiposData> source,
            FiltrosLazyLoadData filtros)
        {
            var sortField = NormalizeText(filtros?.sortField).ToLowerInvariant();
            var asc = filtros?.sortOrder != 0;

            return (sortField, asc) switch
            {
                ("cod_remesa", true) => source.OrderBy(x => x.cod_remesa),
                ("cod_remesa", false) => source.OrderByDescending(x => x.cod_remesa),

                ("descripcion", true) => source.OrderBy(x => x.descripcion),
                ("descripcion", false) => source.OrderByDescending(x => x.descripcion),

                ("activo", true) => source.OrderBy(x => x.activo),
                ("activo", false) => source.OrderByDescending(x => x.activo),

                ("registro_usuario", true) => source.OrderBy(x => x.registro_usuario),
                ("registro_usuario", false) => source.OrderByDescending(x => x.registro_usuario),

                ("registro_fecha", true) => source.OrderBy(x => x.registro_fecha),
                ("registro_fecha", false) => source.OrderByDescending(x => x.registro_fecha),

                ("modifica_usuario", true) => source.OrderBy(x => x.modifica_usuario),
                ("modifica_usuario", false) => source.OrderByDescending(x => x.modifica_usuario),

                ("modifica_fecha", true) => source.OrderBy(x => x.modifica_fecha),
                ("modifica_fecha", false) => source.OrderByDescending(x => x.modifica_fecha),

                ("cod_entidad", true) => source.OrderBy(x => x.cod_entidad),
                ("cod_entidad", false) => source.OrderByDescending(x => x.cod_entidad),

                (_, true) => source.OrderBy(x => x.cod_remesa),
                _ => source.OrderByDescending(x => x.cod_remesa)
            };
        }

        private static List<CcCaRemesasTiposData> ApplyPaginacion(
            IEnumerable<CcCaRemesasTiposData> source,
            FiltrosLazyLoadData filtros)
        {
            var offset = filtros?.pagina ?? 0;
            var fetch = filtros?.paginacion ?? 0;

            if (offset < 0)
                offset = 0;

            if (fetch <= 0)
                return source.ToList();

            return source.Skip(offset).Take(fetch).ToList();
        }

        private void LogBitacora(int empresaId, string usuario, string detalle, string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string NormalizeMovimientoWeb(string movimiento)
        {
            var mov = NormalizeText(movimiento);

            return mov.ToUpperInvariant() switch
            {
                "REGISTRA" => "Registra - WEB",
                "MODIFICA" => "Modifica - WEB",
                "ELIMINA" => "Elimina - WEB",
                _ => $"{mov} - WEB"
            };
        }

        private static CcCaRemesasTiposSpResult ReadSpResult(CcCaRemesasTiposSpResult? row)
        {
            return row ?? new CcCaRemesasTiposSpResult
            {
                Pass = 0,
                Mensaje = "No se obtuvo respuesta válida del procedimiento.",
                Movimiento = "Error",
                IdLLave = 0
            };
        }

        private List<CcCaRemesasTiposData> ObtenerListaBase(SqlConnection conn, string entidad)
        {
            return conn.Query<CcCaRemesasTiposData>(
                "spPrm_CA_Remesas_Tipos_Lista",
                new { Entidad = entidad },
                commandType: CommandType.StoredProcedure).ToList();
        }

        private ErrorDto EjecutarGuardarInterno(
            int CodEmpresa,
            string usuarioSesion,
            CcCaRemesasTiposData item,
            bool isInsert)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = ReadSpResult(
                    conn.QueryFirstOrDefault<CcCaRemesasTiposSpResult>(
                        "spPrm_CA_Remesas_Tipos_Add",
                        new
                        {
                            Remesa = isInsert ? 0 : item.cod_remesa!.Value,
                            Descripcion = NormalizeText(item.descripcion),
                            Activo = item.activo ? 1 : 0,
                            Entidad = NormalizeText(item.cod_entidad),
                            Usuario = NormalizeText(usuarioSesion)
                        },
                        commandType: CommandType.StoredProcedure));

                if (result.Pass != 1)
                    return DbHelper.ErrorResponse(result.Mensaje, -2);

                LogBitacora(
                    empresaId: CodEmpresa,
                    usuario: usuarioSesion,
                    detalle: result.Mensaje,
                    movimiento: NormalizeMovimientoWeb(result.Movimiento));

                return DbHelper.OkResponse($"{result.Mensaje} satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto RemesasTipos_Insertar(int CodEmpresa, string usuario, CcCaRemesasTiposData item)
        {
            return EjecutarGuardarInterno(CodEmpresa, usuario, item, true);
        }

        private ErrorDto RemesasTipos_Actualizar(int CodEmpresa, string usuario, CcCaRemesasTiposData item)
        {
            return EjecutarGuardarInterno(CodEmpresa, usuario, item, false);
        }

        /// <summary>
        /// Obtiene la lista paginada de tipos de remesa por entidad, aplicando filtro global,
        /// ordenamiento y paginación en memoria sobre el resultado del procedimiento almacenado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public ErrorDto<CcCaRemesasTiposLista> RemesasTipos_Lista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros,
            string entidad)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new CcCaRemesasTiposLista
            {
                total = 0,
                lista = new List<CcCaRemesasTiposData>()
            };

            try
            {
                entidad = NormalizeText(entidad);

                if (string.IsNullOrWhiteSpace(entidad))
                    return DbHelper.CreateOkResponse(response);

                var lista = ObtenerListaBase(conn, entidad);
                var filtrada = ApplyFiltro(lista, filtros);
                var ordenada = ApplySort(filtrada, filtros).ToList();

                response.total = ordenada.Count;
                response.lista = ApplyPaginacion(ordenada, filtros);

                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CcCaRemesasTiposLista>(ex.Message, -1, response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CcCaRemesasTiposLista>(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de tipos de remesa por entidad, aplicando filtro global
        /// y ordenamiento en memoria sobre el resultado del procedimiento almacenado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CcCaRemesasTiposData>> RemesasTipos_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros,
            string entidad)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                entidad = NormalizeText(entidad);

                if (string.IsNullOrWhiteSpace(entidad))
                    return DbHelper.CreateOkResponse(new List<CcCaRemesasTiposData>());

                var lista = ObtenerListaBase(conn, entidad);
                var filtrada = ApplyFiltro(lista, filtros);
                var ordenada = ApplySort(filtrada, filtros).ToList();

                return DbHelper.CreateOkResponse(ordenada);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CcCaRemesasTiposData>>(ex.Message);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CcCaRemesasTiposData>>(ex.Message);
            }
        }

        /// <summary>
        /// Inserta o actualiza un tipo de remesa según el estado del registro recibido.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public ErrorDto RemesasTipos_Guardar(
            int CodEmpresa,
            string usuario,
            CcCaRemesasTiposData item)
        {
            try
            {
                if (item == null)
                    return DbHelper.ErrorResponse("El registro es requerido.", -2);

                item.descripcion = NormalizeText(item.descripcion);
                item.cod_entidad = NormalizeText(item.cod_entidad);
                usuario = NormalizeText(usuario);

                if (string.IsNullOrWhiteSpace(item.cod_entidad))
                    return DbHelper.ErrorResponse("La entidad es requerida.", -2);

                if (string.IsNullOrWhiteSpace(item.descripcion))
                    return DbHelper.ErrorResponse("La descripción es requerida.", -2);

                if (!item.isNew && (!item.cod_remesa.HasValue || item.cod_remesa.Value <= 0))
                    return DbHelper.ErrorResponse("El Id de remesa es requerido para actualizar.", -2);

                if (item.isNew)
                    return RemesasTipos_Insertar(CodEmpresa, usuario, item);

                return RemesasTipos_Actualizar(CodEmpresa, usuario, item);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un tipo de remesa por Id utilizando el procedimiento almacenado correspondiente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto RemesasTipos_Eliminar(
            int CodEmpresa,
            int id,
            string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                usuario = NormalizeText(usuario);

                if (id <= 0)
                    return DbHelper.ErrorResponse("El Id indicado no es válido.", -2);

                var result = ReadSpResult(
                    conn.QueryFirstOrDefault<CcCaRemesasTiposSpResult>(
                        "spPrm_CA_Remesas_Tipos_Del",
                        new
                        {
                            Id = id,
                            Usuario = usuario
                        },
                        commandType: CommandType.StoredProcedure));

                if (result.Pass != 1)
                    return DbHelper.ErrorResponse(result.Mensaje, -2);

                LogBitacora(
                    empresaId: CodEmpresa,
                    usuario: usuario,
                    detalle: result.Mensaje,
                    movimiento: NormalizeMovimientoWeb(result.Movimiento));

                return DbHelper.OkResponse($"{result.Mensaje}, Eliminado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las entidades activas para el dropdown principal de la pantalla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> RemesasTipos_Entidades_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                SELECT
                    RTRIM(COD_ENTIDAD) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM PRM_CA_ENTIDAD
                WHERE ACTIVO = 1
                ORDER BY COD_ENTIDAD;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }
    }
}