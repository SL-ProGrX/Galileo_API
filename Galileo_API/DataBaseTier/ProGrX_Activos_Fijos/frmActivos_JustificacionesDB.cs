using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using static PgxAPI.Models.ProGrX_Activos_Fijos.frmActivos_JustificacionesModels;

namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_JustificacionesDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 36;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmActivos_JustificacionesDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }


        /// <summary>
        /// Obtener lista de justificaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<ActivosJustificacionesLista> Activos_JustificacionesLista_Obtener(int CodEmpresa, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<ActivosJustificacionesFiltros>(filtros);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<ActivosJustificacionesLista>();
            response.Result = new ActivosJustificacionesLista();
            response.Code = 0;

            try
            {
                var query = "";
                string where = "", paginaActual = "", paginacionActual = "";

                using var connection = new SqlConnection(clienteConnString);
                {
                    if (vfiltro != null)
                    {
                        if (!string.IsNullOrEmpty(vfiltro.filtro))
                        {
                            where = "WHERE COD_JUSTIFICACION LIKE '%" + vfiltro.filtro + "%' OR DESCRIPCION LIKE '%" + vfiltro.filtro + "%' ";
                        }

                        if (vfiltro.pagina != null)
                        {
                            paginaActual = " OFFSET " + vfiltro.pagina + " ROWS ";
                            paginacionActual = " FETCH NEXT " + vfiltro.paginacion + " ROWS ONLY ";
                        }
                    }

                    // Total de registros
                    query = $"SELECT COUNT(*) FROM ACTIVOS_JUSTIFICACIONES {where}";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    // Datos paginados (solo código y descripción)
                    query = $@"
                    SELECT 
                        COD_JUSTIFICACION AS cod_justificacion, 
                        DESCRIPCION       AS descripcion
                    FROM ACTIVOS_JUSTIFICACIONES
                    {where}
                    ORDER BY COD_JUSTIFICACION
                    {paginaActual} {paginacionActual}";

                    response.Result.lista = connection.Query<ActivosJustificacionesData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = null;
            }

            return response;
        }


        /// <summary>
        /// Verifica si una justificación ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_justificacion"></param>
        /// <returns></returns>
        public ErrorDto Activos_JustificacionesExiste_Obtener(int CodEmpresa, string cod_justificacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                var query = @"SELECT COUNT(*) 
                      FROM dbo.ACTIVOS_JUSTIFICACIONES 
                      WHERE UPPER(COD_JUSTIFICACION) = @cod";
                int result = connection.QueryFirstOrDefault<int>(query, new { cod = cod_justificacion.ToUpper() });

                (resp.Code, resp.Description) =
                    (result == 0) ? (0, "JUSTIFICACION: Libre!") : (-2, "JUSTIFICACION: Ocupado!");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene los detalles de una justificación específica.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_justificacion"></param>
        /// <returns></returns>
        public ErrorDto<ActivosJustificacionesData> Activos_Justificaciones_Obtener(int CodEmpresa, string cod_justificacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<ActivosJustificacionesData> { Code = 0 };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @"
                    SELECT
                        j.COD_JUSTIFICACION                                                  AS cod_justificacion,
                        ISNULL(j.TIPO,'')                                                    AS tipo,
                        ISNULL(j.DESCRIPCION,'')                                             AS descripcion,
                        ISNULL(j.TIPO_ASIENTO,'')                                            AS tipo_asiento,
                        ISNULL(j.COD_CUENTA_01,'')                                           AS cod_cuenta_01,
                        ISNULL(j.COD_CUENTA_02,'')                                           AS cod_cuenta_02,
                        ISNULL(j.COD_CUENTA_03,'')                                           AS cod_cuenta_03,
                        ISNULL(j.COD_CUENTA_04,'')                                           AS cod_cuenta_04,
                        ISNULL(j.ESTADO,'')                                                  AS estado,
                        ISNULL(j.REGISTRO_USUARIO,'')                                        AS registro_usuario,
                        ISNULL(CONVERT(varchar(19), j.REGISTRO_FECHA,120),'')                 AS registro_fecha,
                        ISNULL(j.MODIFICA_USUARIO,'')                                        AS modifica_usuario,
                        ISNULL(CONVERT(varchar(19), j.MODIFICA_FECHA,120),'')                 AS modifica_fecha,

                        -- Decorados
                        ISNULL(ta.DESCRIPCION,'')                                            AS tipo_asiento_desc,

                        ISNULL(c1.COD_CUENTA_MASK,'')                                        AS cod_cuenta_01_mask,
                        ISNULL(c1.DESCRIPCION,'')                                            AS cod_cuenta_01_desc,

                        ISNULL(c2.COD_CUENTA_MASK,'')                                        AS cod_cuenta_02_mask,
                        ISNULL(c2.DESCRIPCION,'')                                            AS cod_cuenta_02_desc,

                        ISNULL(c3.COD_CUENTA_MASK,'')                                        AS cod_cuenta_03_mask,
                        ISNULL(c3.DESCRIPCION,'')                                            AS cod_cuenta_03_desc,

                        ISNULL(c4.COD_CUENTA_MASK,'')                                        AS cod_cuenta_04_mask,
                        ISNULL(c4.DESCRIPCION,'')                                            AS cod_cuenta_04_desc
                    FROM dbo.ACTIVOS_JUSTIFICACIONES j
                    LEFT JOIN dbo.CNTX_TIPOS_ASIENTOS ta  ON ta.TIPO_ASIENTO = j.TIPO_ASIENTO
                    LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c1 ON c1.COD_CUENTA = j.COD_CUENTA_01
                    LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c2 ON c2.COD_CUENTA = j.COD_CUENTA_02
                    LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c3 ON c3.COD_CUENTA = j.COD_CUENTA_03
                    LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c4 ON c4.COD_CUENTA = j.COD_CUENTA_04
                    WHERE j.COD_JUSTIFICACION = @cod;";

                    resp.Result = connection.QueryFirstOrDefault<ActivosJustificacionesData>(
                        query, new { cod = cod_justificacion.ToUpper() });

                    resp.Description = (resp.Result == null) ? "Justificación no encontrada." : "Ok";
                    resp.Code = (resp.Result == null) ? -2 : 0;
                }
            }
            catch (Exception)
            {
                resp.Code = -1;
                resp.Description = "Error al obtener la justificación.";
                resp.Result = null;
            }

            return resp;
        }


        /// <summary>
        /// Navegación (scroll) entre justificaciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scroll"></param>
        /// <param name="cod_justificacion"></param>
        /// <returns></returns>
        public ErrorDto<ActivosJustificacionesData> Activos_Justificacion_Scroll(int CodEmpresa, int scroll, string? cod_justificacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<ActivosJustificacionesData> { Code = 0 };

            try
            {
                string whereOrder = (scroll == 1)
                    ? " WHERE j.COD_JUSTIFICACION > @cod ORDER BY j.COD_JUSTIFICACION ASC "
                    : " WHERE j.COD_JUSTIFICACION < @cod ORDER BY j.COD_JUSTIFICACION DESC ";

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"
                    SELECT TOP 1
                        j.COD_JUSTIFICACION                                                  AS cod_justificacion,
                        ISNULL(j.TIPO,'')                                                    AS tipo,
                        ISNULL(j.DESCRIPCION,'')                                             AS descripcion,
                        ISNULL(j.TIPO_ASIENTO,'')                                            AS tipo_asiento,
                        ISNULL(j.COD_CUENTA_01,'')                                           AS cod_cuenta_01,
                        ISNULL(j.COD_CUENTA_02,'')                                           AS cod_cuenta_02,
                        ISNULL(j.COD_CUENTA_03,'')                                           AS cod_cuenta_03,
                        ISNULL(j.COD_CUENTA_04,'')                                           AS cod_cuenta_04,
                        ISNULL(j.ESTADO,'')                                                  AS estado,
                        ISNULL(j.REGISTRO_USUARIO,'')                                        AS registro_usuario,
                        ISNULL(CONVERT(varchar(19), j.REGISTRO_FECHA,120),'')                 AS registro_fecha,
                        ISNULL(j.MODIFICA_USUARIO,'')                                        AS modifica_usuario,
                        ISNULL(CONVERT(varchar(19), j.MODIFICA_FECHA,120),'')                 AS modifica_fecha,

                        -- Decorados
                        ISNULL(ta.DESCRIPCION,'')                                            AS tipo_asiento_desc,

                        ISNULL(c1.COD_CUENTA_MASK,'')                                        AS cod_cuenta_01_mask,
                        ISNULL(c1.DESCRIPCION,'')                                            AS cod_cuenta_01_desc,

                        ISNULL(c2.COD_CUENTA_MASK,'')                                        AS cod_cuenta_02_mask,
                        ISNULL(c2.DESCRIPCION,'')                                            AS cod_cuenta_02_desc,

                        ISNULL(c3.COD_CUENTA_MASK,'')                                        AS cod_cuenta_03_mask,
                        ISNULL(c3.DESCRIPCION,'')                                            AS cod_cuenta_03_desc,

                        ISNULL(c4.COD_CUENTA_MASK,'')                                        AS cod_cuenta_04_mask,
                        ISNULL(c4.DESCRIPCION,'')                                            AS cod_cuenta_04_desc
                    FROM dbo.ACTIVOS_JUSTIFICACIONES j
                    LEFT JOIN dbo.CNTX_TIPOS_ASIENTOS ta  ON ta.TIPO_ASIENTO = j.TIPO_ASIENTO
                    LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c1 ON c1.COD_CUENTA = j.COD_CUENTA_01
                    LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c2 ON c2.COD_CUENTA = j.COD_CUENTA_02
                    LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c3 ON c3.COD_CUENTA = j.COD_CUENTA_03
                    LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c4 ON c4.COD_CUENTA = j.COD_CUENTA_04
                    {whereOrder};";

                    resp.Result = connection.QueryFirstOrDefault<ActivosJustificacionesData>(
                        query, new { cod = (cod_justificacion ?? string.Empty).ToUpper() });

                    if (resp.Result == null)
                    {
                        resp.Code = -2;
                        resp.Description = "No se encontraron más resultados.";
                    }
                    else
                    {
                        resp.Description = "Ok";
                    }
                }
            }
            catch (Exception)
            {
                resp.Code = -1;
                resp.Description = "Error al obtener la justificación.";
                resp.Result = null;
            }

            return resp;
        }


        /// <summary>
        /// Guarda (inserta o actualiza) una justificación en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Activos_Justificaciones_Guardar(int CodEmpresa, ActivosJustificacionesData data)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = string.Empty };

            try
            {
                var errores = new List<string>();
                if (data == null)
                    return new ErrorDto { Code = -1, Description = "Datos de justificación no proporcionados." };

                if (string.IsNullOrWhiteSpace(data.cod_justificacion))
                    errores.Add("No ha indicado el código de justificación.");

                if (string.IsNullOrWhiteSpace(data.descripcion))
                    errores.Add("No ha indicado la descripción de la justificación.");

                if (errores.Count > 0)
                {
                    resp.Code = -1;
                    resp.Description = string.Join(" | ", errores);
                    return resp;
                }

                using var connection = new SqlConnection(clienteConnString);

                // 2) Existe?
                const string qExiste = @"
            SELECT COUNT(1)
            FROM dbo.ACTIVOS_JUSTIFICACIONES
            WHERE COD_JUSTIFICACION = @cod";
                int existe = connection.QueryFirstOrDefault<int>(qExiste, new { cod = data.cod_justificacion.ToUpper() });

                // 3) Upsert
                resp = (data.isNew)
                    ? (existe > 0
                        ? new ErrorDto { Code = -2, Description = $"La justificación {data.cod_justificacion.ToUpper()} ya existe." }
                        : Activos_Justificaciones_Insertar(CodEmpresa, data))
                    : (existe == 0
                        ? new ErrorDto { Code = -2, Description = $"La justificación {data.cod_justificacion.ToUpper()} no existe." }
                        : Activos_Justificaciones_Actualizar(CodEmpresa, data));
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }


        /// <summary>
        /// Inserta una nueva justificación en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto Activos_Justificaciones_Insertar(int CodEmpresa, ActivosJustificacionesData data)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @"
                INSERT INTO dbo.ACTIVOS_JUSTIFICACIONES
                    (COD_JUSTIFICACION, DESCRIPCION, TIPO, TIPO_ASIENTO,
                     COD_CUENTA_01, COD_CUENTA_02, COD_CUENTA_03, COD_CUENTA_04,
                     REGISTRO_FECHA, REGISTRO_USUARIO, MODIFICA_USUARIO, MODIFICA_FECHA)
                VALUES
                    (@cod, @descripcion, @tipo, @tipo_asiento,
                     @cta1, @cta2, @cta3, @cta4,
                     SYSDATETIME(), @reg_usuario, NULL, NULL)";

                    connection.Execute(query, new
                    {
                        cod = data.cod_justificacion.ToUpper(),
                        descripcion = data.descripcion?.ToUpper(),
                        tipo = (data.tipo ?? "").ToUpper(), // 'A','R','V','D','M'
                        tipo_asiento = data.tipo_asiento?.ToUpper(),
                        cta1 = string.IsNullOrWhiteSpace(data.cod_cuenta_01) ? null : data.cod_cuenta_01.ToUpper(),
                        cta2 = string.IsNullOrWhiteSpace(data.cod_cuenta_02) ? null : data.cod_cuenta_02.ToUpper(),
                        cta3 = string.IsNullOrWhiteSpace(data.cod_cuenta_03) ? null : data.cod_cuenta_03.ToUpper(),
                        cta4 = string.IsNullOrWhiteSpace(data.cod_cuenta_04) ? null : data.cod_cuenta_04.ToUpper(),
                        reg_usuario = string.IsNullOrWhiteSpace(data.registro_usuario) ? null : data.registro_usuario
                    });

                    // Bitácora
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = string.IsNullOrWhiteSpace(data.registro_usuario) ? "" : data.registro_usuario,
                        DetalleMovimiento = $"Justificación: {data.cod_justificacion} - {data.descripcion}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo // 36
                    });

                    resp.Description = "Justificación Ingresada Satisfactoriamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }
        /// <summary>
        /// Actualiza los detalles de una justificación existente en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDto Activos_Justificaciones_Actualizar(int CodEmpresa, ActivosJustificacionesData data)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @"
                UPDATE dbo.ACTIVOS_JUSTIFICACIONES
                   SET DESCRIPCION      = @descripcion,
                       TIPO             = @tipo,
                       TIPO_ASIENTO     = @tipo_asiento,
                       COD_CUENTA_01    = @cta1,
                       COD_CUENTA_02    = @cta2,
                       COD_CUENTA_03    = @cta3,
                       COD_CUENTA_04    = @cta4,
                       MODIFICA_USUARIO = @mod_usuario,
                       MODIFICA_FECHA   = SYSDATETIME()
                 WHERE COD_JUSTIFICACION = @cod";

                    connection.Execute(query, new
                    {
                        cod = data.cod_justificacion.ToUpper(),
                        descripcion = data.descripcion?.ToUpper(),
                        tipo = (data.tipo ?? "").ToUpper(),
                        tipo_asiento = data.tipo_asiento?.ToUpper(),
                        cta1 = string.IsNullOrWhiteSpace(data.cod_cuenta_01) ? null : data.cod_cuenta_01.ToUpper(),
                        cta2 = string.IsNullOrWhiteSpace(data.cod_cuenta_02) ? null : data.cod_cuenta_02.ToUpper(),
                        cta3 = string.IsNullOrWhiteSpace(data.cod_cuenta_03) ? null : data.cod_cuenta_03.ToUpper(),
                        cta4 = string.IsNullOrWhiteSpace(data.cod_cuenta_04) ? null : data.cod_cuenta_04.ToUpper(),
                        mod_usuario = string.IsNullOrWhiteSpace(data.modifica_usuario) ? null : data.modifica_usuario
                    });

                    // Bitácora
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = string.IsNullOrWhiteSpace(data.modifica_usuario) ? "" : data.modifica_usuario,
                        DetalleMovimiento = $"Justificación: {data.cod_justificacion} - {data.descripcion}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo // 36
                    });

                    resp.Description = "Justificación Actualizada Satisfactoriamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Elimina una justificación del sistema (no se permite de momento).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_justificacion"></param>
        /// <returns></returns>
        public ErrorDto Activos_Justificaciones_Eliminar(int CodEmpresa, string usuario, string cod_justificacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"DELETE FROM dbo.ACTIVOS_JUSTIFICACIONES 
                          WHERE COD_JUSTIFICACION = @cod_justificacion";

                    int rows = connection.Execute(query, new { cod_justificacion = cod_justificacion.ToUpper() });

                    if (rows == 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"La justificación {cod_justificacion.ToUpper()} no existe.";
                        return resp;
                    }

                    // Bitácora
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Justificación: {cod_justificacion}",
                        Movimiento = "Elimina - WEB",
                        Modulo = vModulo // 36
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene una lista de Tipos que se guardan en la tabla de justificaciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_JustificacionesTipos_Obtener(int CodEmpresa)
        {
            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>
        {
            new() { item = "A", descripcion = "Adiciones y Mejoras" },
            new() { item = "R", descripcion = "Retiros (Salidas)" },
            new() { item = "V", descripcion = "Revaluaciones" },
            new() { item = "D", descripcion = "Deterioros y Desvalorizaciones" },
            new() { item = "M", descripcion = "Mantenimiento" }
        }
            };
        }

        /// <summary>
        /// Obtener lista de tipos de asientos para justificaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_JustificacionesTiposAsientos_Obtener(int CodEmpresa, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "",
                Result = new List<DropDownListaGenericaModel>(),
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select Tipo_Asiento as 'item',descripcion from CNTX_TIPOS_ASIENTOS
                                    where cod_contabilidad = @contabilidad AND ACTIVO = 1
                                    order by descripcion asc";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { contabilidad = contabilidad }).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }
    }
}
