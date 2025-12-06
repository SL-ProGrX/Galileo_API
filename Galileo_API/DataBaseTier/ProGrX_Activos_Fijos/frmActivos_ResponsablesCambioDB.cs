using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosResponsablesCambioDB
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        private const string BoletaIdParam          = "BoletaId";
        private const string UsuarioParam           = "Usuario";

        private const string CodTrasladoCol         = "cod_traslado";

        private const string MensajeOk              = "Ok";
        private const string MensajeDatosNoProv     = "Datos no proporcionados.";
        private const string MensajeBoletaNoEnc     = "Boleta no encontrada.";
        private const string MensajeErrorInsertar   = "Error al insertar";
        private const string MensajeErrorActualizar = " Error al actualizar";
        private const string MensajeErrorProcesar   = "Error al procesar";
        private const string MensajeErrorDescartar  = " Error al descartar";
        private const string MensajeNoMasRes        = "No se encontraron más resultados.";
        private const string MensajeNoEncontrado    = "No encontrado";

        private const string VistaTrasladosBoletas  = "vActivos_Traslados_Boletas";
        private const string WhereBase              = " WHERE 1=1 ";

        private const string BitacoraPrefijoBoleta  = "Boleta de Cambio Responsable: ";

        // Lista blanca para ORDER BY en boletas
        private static readonly Dictionary<string, string> SortFieldBoletasMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { CodTrasladoCol, CodTrasladoCol },
                { "identificacion", "identificacion" },
                { "persona", "persona" },
                { "estado_desc", "estado_desc" },
                { "registro_fecha", "registro_fecha" }
            };

        public FrmActivosResponsablesCambioDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB        = new PortalDB(config);
        }

        /// <summary>
        /// Obtener lista de boletas de traslado de responsable con paginación y filtros.
        /// </summary>
        public ErrorDto<ActivosResponsablesCambioBoletaLista> Activos_ResponsablesCambio_Boletas_Lista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<ActivosResponsablesCambioBoletaLista>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = new ActivosResponsablesCambioBoletaLista
                {
                    total = 0,
                    lista = new List<ActivosResponsablesCambioBoletaResumen>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var where = string.Empty;
                var p     = new DynamicParameters();

                var filtroTexto = (filtros?.filtro ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    where = @"
                        WHERE (
                               cod_traslado   LIKE @filtro
                            OR persona        LIKE @filtro
                            OR identificacion LIKE @filtro
                        )";
                    p.Add("@filtro", $"%{filtroTexto}%");
                }

                var qTotal = $@"
                    SELECT COUNT(1)
                    FROM {VistaTrasladosBoletas}
                    {where}";

                result.Result.total = connection.QueryFirstOrDefault<int>(qTotal, p);

                // ORDER BY con lista blanca
                var sortKey = string.IsNullOrWhiteSpace(filtros?.sortField)
                    ? CodTrasladoCol
                    : filtros.sortField!;

                if (!SortFieldBoletasMap.TryGetValue(sortKey, out var sortField))
                {
                    sortField = CodTrasladoCol;
                }

                var sortOrder = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";

                // Paginación (pagina 1-based)
                var pagina     = filtros?.pagina     ?? 1;
                var paginacion = filtros?.paginacion ?? 10;
                var offset     = pagina <= 1 ? 0 : (pagina - 1) * paginacion;

                p.Add("@offset", offset);
                p.Add("@fetch",  paginacion);

                var qDatos = $@"
                    SELECT 
                        cod_traslado,
                        identificacion,
                        persona,
                        estado_desc,
                        CONVERT(varchar(19), registro_fecha, 120) AS registro_fecha
                    FROM {VistaTrasladosBoletas}
                    {where}
                    ORDER BY {sortField} {sortOrder}
                    OFFSET @offset ROWS
                    FETCH NEXT @fetch ROWS ONLY;";

                result.Result.lista = connection
                    .Query<ActivosResponsablesCambioBoletaResumen>(qDatos, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = [];
            }

            return result;
        }

        /// <summary>
        /// Buscar personas para traslado de responsables (sin paginación).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ResponsablesCambio_Personas_Buscar(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var where = WhereBase;
                var p     = new DynamicParameters();

                var filtroTexto = (filtros?.filtro ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    where += " AND (Nombre LIKE @filtro OR Identificacion LIKE @filtro) ";
                    p.Add("@filtro", $"%{filtroTexto}%");
                }

                if (!string.IsNullOrWhiteSpace(filtros?.sortField) &&
                    filtros.sortField.StartsWith("excluir:", StringComparison.OrdinalIgnoreCase))
                {
                    var splitArr = filtros.sortField.Split(':');
                    var excluir  = splitArr[^1];

                    if (!string.IsNullOrWhiteSpace(excluir))
                    {
                        where += " AND Identificacion <> @excluir ";
                        p.Add("@excluir", excluir);
                    }
                }

                var q = $@"
                    SELECT Identificacion AS item, Nombre AS descripcion
                    FROM Activos_Personas
                    {where}
                    ORDER BY Nombre ASC";

                resp.Result = connection.Query<DropDownListaGenericaModel>(q, p).ToList();
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener boleta de traslado de responsable por código.
        /// </summary>
        public ErrorDto<ActivosResponsablesCambioBoleta> Activos_ResponsablesCambio_Boleta_Obtener(
            int CodEmpresa,
            string cod_traslado,
            string usuario)
        {
            var resp = new ErrorDto<ActivosResponsablesCambioBoleta>
            {
                Code        = 0,
                Description = string.Empty,
                Result      = null
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add(BoletaIdParam, cod_traslado);
                p.Add(UsuarioParam,  usuario);

                var data = connection.QueryFirstOrDefault<ActivosResponsablesCambioBoleta>(
                    "spActivos_Responsable_Cambio_Consulta",
                    p,
                    commandType: CommandType.StoredProcedure);

                if (data == null)
                {
                    resp.Code        = -2;
                    resp.Description = MensajeBoletaNoEnc;
                }
                else
                {
                    resp.Result      = data;
                    resp.Description = MensajeOk;
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener lista de placas de la boleta (pestaña Activos).
        /// </summary>
        public ErrorDto<List<ActivosResponsablesCambioPlaca>> Activos_ResponsablesCambio_Placas_Obtener(
            int CodEmpresa,
            string? cod_traslado,
            string identificacion,
            string usuario)
        {
            var resp = new ErrorDto<List<ActivosResponsablesCambioPlaca>>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = new List<ActivosResponsablesCambioPlaca>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add(BoletaIdParam, cod_traslado ?? string.Empty);
                p.Add("@Identificacion", identificacion);
                p.Add(UsuarioParam,  usuario);
                p.Add("@ModoRecepcion", 0);

                resp.Result = connection.Query<ActivosResponsablesCambioPlaca>(
                        "spActivos_Responsable_Cambio_Consulta_Placas",
                        p,
                        commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Verifica si una boleta de traslado de responsable existe.
        /// </summary>
        public ErrorDto Activos_ResponsablesCambio_Boleta_Existe_Obtener(int CodEmpresa, string cod_traslado)
        {
            var resp = new ErrorDto
            {
                Code        = 0,
                Description = MensajeOk
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add(BoletaIdParam, cod_traslado);
                p.Add(UsuarioParam,  string.Empty);

                var data = connection.QueryFirstOrDefault<ActivosResponsablesCambioBoleta>(
                    "spActivos_Responsable_Cambio_Consulta",
                    p,
                    commandType: CommandType.StoredProcedure);

                (resp.Code, resp.Description) = data == null
                    ? (0,  "BOLETA: Libre!")
                    : (-2, "BOLETA: Ocupada!");
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        // ---- Helpers de validación para bajar complejidad (S3776) ----

        private static List<string> ValidarBoletaDatos(ActivosResponsablesCambioBoleta data)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(data.cod_motivo))
                errores.Add("No ha indicado el motivo.");

            if (string.IsNullOrWhiteSpace(data.notas) || data.notas.Trim().Length < 3)
                errores.Add("No ha indicado una nota válida.");

            if (string.IsNullOrWhiteSpace(data.identificacion))
                errores.Add("No ha indicado el responsable actual.");

            if (string.IsNullOrWhiteSpace(data.identificacion_destino))
                errores.Add("No ha indicado el responsable destino.");

            if (string.IsNullOrWhiteSpace(data.fecha_aplicacion))
                errores.Add("No ha indicado la fecha de aplicación (YYYY-MM-DD).");

            return errores;
        }

        /// <summary>
        /// Guardar boleta de traslado de responsable (router por isNew).
        /// </summary>
        public ErrorDto<ActivosResponsablesCambioBoletaResult> Activos_ResponsablesCambio_Boleta_Guardar(
            int CodEmpresa,
            ActivosResponsablesCambioBoleta data)
        {
            var resp = new ErrorDto<ActivosResponsablesCambioBoletaResult>
            {
                Code        = 0,
                Description = string.Empty,
                Result      = null
            };

            try
            {
                if (data == null)
                {
                    return new ErrorDto<ActivosResponsablesCambioBoletaResult>
                    {
                        Code        = -1,
                        Description = MensajeDatosNoProv,
                        Result      = null
                    };
                }

                var errores = ValidarBoletaDatos(data);
                if (errores.Count > 0)
                {
                    return new ErrorDto<ActivosResponsablesCambioBoletaResult>
                    {
                        Code        = -1,
                        Description = string.Join(" | ", errores),
                        Result      = null
                    };
                }

                var codBoleta = data.cod_traslado ?? string.Empty;

                if (data.isNew)
                {
                    var exi = Activos_ResponsablesCambio_Boleta_Existe_Obtener(CodEmpresa, codBoleta);
                    if (exi.Code == -2)
                    {
                        return new ErrorDto<ActivosResponsablesCambioBoletaResult>
                        {
                            Code        = -2,
                            Description = $"La boleta {data.cod_traslado} ya existe.",
                            Result      = null
                        };
                    }

                    resp = Activos_ResponsablesCambio_Boleta_Insertar(CodEmpresa, data);
                }
                else
                {
                    var exi = Activos_ResponsablesCambio_Boleta_Existe_Obtener(CodEmpresa, codBoleta);
                    if (exi.Code == 0)
                    {
                        return new ErrorDto<ActivosResponsablesCambioBoletaResult>
                        {
                            Code        = -2,
                            Description = $"La boleta {data.cod_traslado} no existe.",
                            Result      = null
                        };
                    }

                    var upd = Activos_ResponsablesCambio_Boleta_Actualizar(CodEmpresa, data);
                    resp.Code        = upd.Code;
                    resp.Description = upd.Description;
                    resp.Result      = new ActivosResponsablesCambioBoletaResult
                    {
                        cod_traslado = codBoleta
                    };
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Insertar boleta de traslado de responsable.
        /// </summary>
        private ErrorDto<ActivosResponsablesCambioBoletaResult> Activos_ResponsablesCambio_Boleta_Insertar(
            int CodEmpresa,
            ActivosResponsablesCambioBoleta data)
        {
            var resp = new ErrorDto<ActivosResponsablesCambioBoletaResult>
            {
                Code        = 0,
                Description = string.Empty,
                Result      = null
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add(BoletaIdParam, data.cod_traslado);
                p.Add("@MotivoId",      data.cod_motivo);
                p.Add("@Notas",         data.notas);
                p.Add("@A_Id",          data.identificacion);
                p.Add("@N_Id",          data.identificacion_destino);
                p.Add(UsuarioParam,     data.registro_usuario);
                p.Add("@FechaAplicacion", data.fecha_aplicacion);

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_Responsable_Cambio_Boleta_Add",
                    p,
                    commandType: CommandType.StoredProcedure);

                var pass    = (int)(rs?.Pass    ?? 0);
                var mensaje = (string)(rs?.Mensaje ?? MensajeErrorInsertar);
                var boleta  = (string)(rs?.Boleta  ?? string.Empty);

                if (pass != 1)
                {
                    return new ErrorDto<ActivosResponsablesCambioBoletaResult>
                    {
                        Code        = -2,
                        Description = mensaje,
                        Result      = null
                    };
                }

                if (!string.IsNullOrWhiteSpace(boleta))
                {
                    data.cod_traslado = boleta;
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = data.registro_usuario ?? string.Empty,
                    DetalleMovimiento = $"{BitacoraPrefijoBoleta}{data.cod_traslado}",
                    Movimiento        = "Registra - WEB",
                    Modulo            = vModulo
                });

                resp.Code        = 0;
                resp.Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "Boleta registrada satisfactoriamente!"
                    : mensaje;

                resp.Result = new ActivosResponsablesCambioBoletaResult
                {
                    cod_traslado = data.cod_traslado ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Actualizar boleta de traslado de responsable.
        /// </summary>
        private ErrorDto Activos_ResponsablesCambio_Boleta_Actualizar(
            int CodEmpresa,
            ActivosResponsablesCambioBoleta data)
        {
            var resp = new ErrorDto
            {
                Code        = 0,
                Description = string.Empty
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add(BoletaIdParam, data.cod_traslado);
                p.Add("@MotivoId",      data.cod_motivo);
                p.Add("@Notas",         data.notas);
                p.Add("@A_Id",          data.identificacion);
                p.Add("@N_Id",          data.identificacion_destino);
                p.Add(UsuarioParam,     data.registro_usuario);
                p.Add("@FechaAplicacion", data.fecha_aplicacion);

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_Responsable_Cambio_Boleta_Add",
                    p,
                    commandType: CommandType.StoredProcedure);

                var pass    = (int)(rs?.Pass    ?? 0);
                var mensaje = (string)(rs?.Mensaje ?? MensajeErrorActualizar);

                if (pass != 1)
                {
                    return new ErrorDto
                    {
                        Code        = -2,
                        Description = mensaje
                    };
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = data.registro_usuario ?? string.Empty,
                    DetalleMovimiento = $"{BitacoraPrefijoBoleta}{data.cod_traslado}",
                    Movimiento        = "Modifica - WEB",
                    Modulo            = vModulo
                });

                resp.Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "Boleta actualizada satisfactoriamente!"
                    : mensaje;
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Guardar placa asociada a boleta (invocar N veces, primer_lote_bit=1 en la primera).
        /// </summary>
        public ErrorDto Activos_ResponsablesCambio_Boleta_Placa_Guardar(
            int CodEmpresa,
            ActivosResponsablesCambioPlacaGuardarRequest data)
        {
            var resp = new ErrorDto
            {
                Code        = 0,
                Description = MensajeOk
            };

            try
            {
                if (data == null)
                {
                    return new ErrorDto
                    {
                        Code        = -1,
                        Description = MensajeDatosNoProv
                    };
                }

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add(BoletaIdParam, data.cod_traslado);
                p.Add("@Placa",      data.num_placa);
                p.Add(UsuarioParam,  data.usuario);
                p.Add("@Inicial",    data.primer_lote_bit);

                connection.Execute(
                    "spActivos_Responsable_Cambio_Boleta_Placas",
                    p,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Procesar boleta de traslado de responsable.
        /// </summary>
        public ErrorDto Activos_ResponsablesCambio_Boleta_Procesar(
            int CodEmpresa,
            string cod_traslado,
            string usuario)
        {
            var resp = new ErrorDto
            {
                Code        = 0,
                Description = MensajeOk
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add("@Boleta",  cod_traslado);
                p.Add(UsuarioParam, usuario);

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_Responsable_Cambio_Procesa",
                    p,
                    commandType: CommandType.StoredProcedure);

                var pass    = (int)(rs?.Pass    ?? 0);
                var mensaje = (string)(rs?.Mensaje ?? MensajeErrorProcesar);

                if (pass != 1)
                {
                    return new ErrorDto
                    {
                        Code        = -2,
                        Description = mensaje
                    };
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario ?? string.Empty,
                    DetalleMovimiento = $"{BitacoraPrefijoBoleta}{cod_traslado}",
                    Movimiento        = "Procesa - WEB",
                    Modulo            = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Descartar boleta de traslado de responsable.
        /// </summary>
        public ErrorDto Activos_ResponsablesCambio_Boleta_Descartar(
            int CodEmpresa,
            string cod_traslado,
            string usuario)
        {
            var resp = new ErrorDto
            {
                Code        = 0,
                Description = MensajeOk
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add(BoletaIdParam, cod_traslado);
                p.Add(UsuarioParam,  usuario);

                var rs = connection.QueryFirstOrDefault<dynamic>(
                    "spActivos_Responsable_Cambio_Descarta",
                    p,
                    commandType: CommandType.StoredProcedure);

                var pass    = (int)(rs?.Pass    ?? 0);
                var mensaje = (string)(rs?.Mensaje ?? MensajeErrorDescartar);

                if (pass != 1)
                {
                    return new ErrorDto
                    {
                        Code        = -2,
                        Description = mensaje
                    };
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario ?? string.Empty,
                    DetalleMovimiento = $"{BitacoraPrefijoBoleta}{cod_traslado}",
                    Movimiento        = "Descarta - WEB",
                    Modulo            = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Navegación (scroll) entre boletas de traslado de responsable.
        /// </summary>
        public ErrorDto<ActivosResponsablesCambioBoleta> Activos_ResponsablesCambio_Boleta_Scroll(
            int CodEmpresa,
            int scroll,
            string? cod_traslado,
            string usuario)
        {
            var resp = new ErrorDto<ActivosResponsablesCambioBoleta>
            {
                Code        = 0,
                Description = string.Empty,
                Result      = null
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var sql = scroll == 1
                    ? $@"
                        SELECT TOP 1 {CodTrasladoCol}
                        FROM {VistaTrasladosBoletas}
                        WHERE {CodTrasladoCol} > @cod
                        ORDER BY {CodTrasladoCol} ASC;"
                    : $@"
                        SELECT TOP 1 {CodTrasladoCol}
                        FROM {VistaTrasladosBoletas}
                        WHERE {CodTrasladoCol} < @cod
                        ORDER BY {CodTrasladoCol} DESC;";

                var nextCode = connection.QueryFirstOrDefault<string>(
                    sql,
                    new { cod = cod_traslado ?? string.Empty });

                if (string.IsNullOrWhiteSpace(nextCode))
                {
                    return new ErrorDto<ActivosResponsablesCambioBoleta>
                    {
                        Code        = -2,
                        Description = MensajeNoMasRes,
                        Result      = null
                    };
                }

                resp = Activos_ResponsablesCambio_Boleta_Obtener(CodEmpresa, nextCode, usuario);
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener catálogo de motivos activos para traslado de responsables.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ResponsablesCambio_Motivos_Obtener(int CodEmpresa)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string q = @"
                    SELECT cod_motivo AS item, descripcion
                    FROM ACTIVOS_TRASLADOS_MOTIVOS
                    WHERE ACTIVO = 1
                    ORDER BY descripcion ASC";

                resp.Result = connection.Query<DropDownListaGenericaModel>(q).ToList();
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener datos de placas sin paginar.
        /// </summary>
        public ErrorDto<List<ActivosResponsablesCambioPlaca>> Activos_ResponsablesCambio_Placas_Export(
            int CodEmpresa,
            string cod_traslado,
            string identificacion,
            string usuario)
        {
            var resp = new ErrorDto<List<ActivosResponsablesCambioPlaca>>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = new List<ActivosResponsablesCambioPlaca>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add(BoletaIdParam, cod_traslado);
                p.Add("@Identificacion", identificacion);
                p.Add(UsuarioParam,  usuario);

                resp.Result = connection.Query<ActivosResponsablesCambioPlaca>(
                        "spActivos_Responsable_Cambio_Consulta_Placas",
                        p,
                        commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener datos de persona por identificación.
        /// </summary>
        public ErrorDto<ActivosResponsablesPersona> Activos_ResponsablesCambio_Persona_Obtener(
            int CodEmpresa,
            string identificacion)
        {
            var resp = new ErrorDto<ActivosResponsablesPersona>
            {
                Code        = 0,
                Description = MensajeOk,
                Result      = null
            };

            const string sql = @"
            SET NOCOUNT ON;

            SELECT TOP (1)
                p.IDENTIFICACION                         AS identificacion,
                p.NOMBRE                                 AS persona,
                p.COD_DEPARTAMENTO                       AS cod_departamento,
                ISNULL(d.DESCRIPCION, '')                AS departamento,
                p.COD_SECCION                            AS cod_seccion,
                ISNULL(s.DESCRIPCION, '')                AS seccion
            FROM ACTIVOS_PERSONAS p
            LEFT JOIN ACTIVOS_DEPARTAMENTOS d
                   ON d.COD_DEPARTAMENTO = p.COD_DEPARTAMENTO
            LEFT JOIN ACTIVOS_SECCIONES s
                   ON s.COD_DEPARTAMENTO = p.COD_DEPARTAMENTO
                  AND s.COD_SECCION      = p.COD_SECCION
            WHERE p.IDENTIFICACION = @identificacion;";

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                resp.Result = connection.QueryFirstOrDefault<ActivosResponsablesPersona>(
                    sql,
                    new { identificacion });

                if (resp.Result is null)
                {
                    resp.Description = MensajeNoEncontrado;
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = null;
            }

            return resp;
        }
    }
}