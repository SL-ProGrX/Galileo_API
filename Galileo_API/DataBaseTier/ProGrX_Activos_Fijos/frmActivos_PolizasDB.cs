using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosPolizasDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        private const string _numplaca = "A.NUM_PLACA";

        public FrmActivosPolizasDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener lista de pólizas (paginada y con filtro).
        /// </summary>
        public ErrorDto<ActivosPolizasLista> Activos_PolizasLista_Obtener(int CodEmpresa, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<ActivosPolizasFiltros>(filtros);
            var response = new ErrorDto<ActivosPolizasLista>
            {
                Code = 0,
                Result = new ActivosPolizasLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var where = string.Empty;
                var p = new DynamicParameters();

                if (vfiltro != null && !string.IsNullOrWhiteSpace(vfiltro.filtro))
                {
                    where = @"
                        WHERE (
                               COD_POLIZA              LIKE @filtro
                            OR DESCRIPCION            LIKE @filtro
                            OR ISNULL(NUM_POLIZA,'')  LIKE @filtro
                            OR ISNULL(DOCUMENTO,'')   LIKE @filtro
                        )";
                    p.Add("@filtro", $"%{vfiltro.filtro}%");
                }

                // Total
                var sqlCount = $@"SELECT COUNT(*) FROM ACTIVOS_POLIZAS {where}";
                response.Result.total = connection.QueryFirstOrDefault<int>(sqlCount, p);

                int pagina      = vfiltro?.pagina     ?? 0;
                int paginacion  = vfiltro?.paginacion ?? 50;

                var sqlPage = $@"
                    SELECT 
                        COD_POLIZA  AS cod_poliza,
                        DESCRIPCION AS descripcion
                    FROM ACTIVOS_POLIZAS
                    {where}
                    ORDER BY COD_POLIZA
                    OFFSET @offset ROWS 
                    FETCH NEXT @rows ROWS ONLY;";

                p.Add("@offset", pagina);
                p.Add("@rows",   paginacion);

                response.Result.lista = connection.Query<ActivosPolizasData>(sqlPage, p).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = [];
            }

            return response;
        }

        /// <summary>
        /// Verifica si una póliza ya existe.
        /// </summary>
        public ErrorDto Activos_PolizasExiste_Obtener(int CodEmpresa, string cod_poliza)
        {
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = @"SELECT COUNT(*) 
                              FROM dbo.ACTIVOS_POLIZAS 
                              WHERE UPPER(COD_POLIZA) = @cod";
                int result = connection.QueryFirstOrDefault<int>(query, new { cod = cod_poliza.ToUpper() });

                (resp.Code, resp.Description) =
                    (result == 0) ? (0, "POLIZA: Libre") : (-2, "POLIZA: Ocupado");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene los detalles de una póliza.
        /// </summary>
        public ErrorDto<ActivosPolizasData> Activos_Polizas_Obtener(int CodEmpresa, string cod_poliza)
        {
            var resp = new ErrorDto<ActivosPolizasData> { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = @"
                    SELECT
                        p.COD_POLIZA                                         AS cod_poliza,
                        ISNULL(p.TIPO_POLIZA,'')                              AS tipo_poliza,
                        ISNULL(p.DESCRIPCION,'')                              AS descripcion,
                        ISNULL(p.OBSERVACION,'')                              AS observacion,
                        ISNULL(CONVERT(varchar(10), p.FECHA_INICIO,23),'')    AS fecha_inicio,
                        ISNULL(CONVERT(varchar(10), p.FECHA_VENCE,23),'')     AS fecha_vence,
                        ISNULL(p.MONTO,0)                                     AS monto,
                        ISNULL(p.NUM_POLIZA,'')                               AS num_poliza,
                        ISNULL(p.DOCUMENTO,'')                                AS documento,

                        CASE WHEN p.FECHA_VENCE < GETDATE() 
                             THEN 'VENCIDA' ELSE 'ACTIVA' END                 AS estado,

                        ISNULL(t.DESCRIPCION,'')                              AS tipo_poliza_desc,

                        ISNULL(p.REGISTRO_USUARIO,'')                         AS registro_usuario,
                        ISNULL(CONVERT(varchar(19), p.REGISTRO_FECHA,120),'') AS registro_fecha,
                        ISNULL(p.MODIFICA_USUARIO,'')                         AS modifica_usuario,
                        ISNULL(CONVERT(varchar(19), p.MODIFICA_FECHA,120),'') AS modifica_fecha
                    FROM dbo.ACTIVOS_POLIZAS p
                    LEFT JOIN dbo.ACTIVOS_POLIZAS_TIPOS t ON t.TIPO_POLIZA = p.TIPO_POLIZA
                    WHERE p.COD_POLIZA = @cod;";

                resp.Result = connection.QueryFirstOrDefault<ActivosPolizasData>(
                    query, new { cod = cod_poliza.ToUpper() });

                resp.Description = (resp.Result == null) ? "Póliza no encontrada." : "Ok";
                resp.Code = (resp.Result == null) ? -2 : 0;
            }
            catch (Exception)
            {
                resp.Code = -1;
                resp.Description = "Error al obtener la póliza.";
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Guarda (inserta o actualiza) una póliza.
        /// </summary>
        public ErrorDto Activos_Polizas_Guardar(int CodEmpresa, ActivosPolizasData data)
        {
            var resp = new ErrorDto { Code = 0, Description = string.Empty };

            try
            {
                var validacion = ValidarDatosPoliza(data);
                if (validacion.Code != 0)
                    return validacion;

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string qExiste = @"
                    SELECT COUNT(1)
                    FROM dbo.ACTIVOS_POLIZAS
                    WHERE COD_POLIZA = @cod";
                int existe = connection.QueryFirstOrDefault<int>(qExiste, new { cod = data.cod_poliza.ToUpper() });

                if (data.isNew)
                {
                    if (existe > 0)
                    {
                        resp = new ErrorDto { Code = -2, Description = $"La póliza {data.cod_poliza.ToUpper()} ya existe." };
                    }
                    else
                    {
                        resp = Activos_Polizas_Insertar(CodEmpresa, data);
                    }
                }
                else
                {
                    if (existe == 0)
                    {
                        resp = new ErrorDto { Code = -2, Description = $"La póliza {data.cod_poliza.ToUpper()} no existe." };
                    }
                    else
                    {
                        resp = Activos_Polizas_Actualizar(CodEmpresa, data);
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        private ErrorDto ValidarDatosPoliza(ActivosPolizasData data)
        {
            var errores = new List<string>();
            if (data == null)
                return new ErrorDto { Code = -1, Description = "Datos de póliza no proporcionados." };

            if (string.IsNullOrWhiteSpace(data.cod_poliza))
                errores.Add("No ha indicado el código de la póliza.");

            if (string.IsNullOrWhiteSpace(data.descripcion))
                errores.Add("No ha indicado la descripción de la póliza.");

            if (string.IsNullOrWhiteSpace(data.tipo_poliza))
                errores.Add("No ha indicado el tipo de póliza.");

            if (!string.IsNullOrWhiteSpace(data.fecha_inicio) &&
                !string.IsNullOrWhiteSpace(data.fecha_vence) &&
                DateTime.TryParse(
                    data.fecha_inicio,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var fi) &&
                DateTime.TryParse(
                    data.fecha_vence,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var fv) &&
                fv < fi)
            {
                errores.Add("La fecha de vencimiento no puede ser menor a la inicial.");
            }

            if (errores.Count > 0)
            {
                return new ErrorDto { Code = -1, Description = string.Join(" | ", errores) };
            }

            return new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Inserta una nueva póliza.
        /// </summary>
        private ErrorDto Activos_Polizas_Insertar(int CodEmpresa, ActivosPolizasData data)
        {
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = @"
                        INSERT INTO dbo.ACTIVOS_POLIZAS
                            (COD_POLIZA, TIPO_POLIZA, DESCRIPCION, OBSERVACION, 
                             FECHA_INICIO, FECHA_VENCE, MONTO, NUM_POLIZA, DOCUMENTO,
                             REGISTRO_FECHA, REGISTRO_USUARIO, MODIFICA_USUARIO, MODIFICA_FECHA)
                        VALUES
                            (@cod, @tipo, @descripcion, @observacion,
                             @fi, @fv, @monto, @num_poliza, @documento,
                             SYSDATETIME(), @reg_usuario, NULL, NULL)";

                connection.Execute(query, new
                {
                    cod         = data.cod_poliza.ToUpper(),
                    tipo        = data.tipo_poliza.ToUpper(),
                    descripcion = data.descripcion?.ToUpper(),
                    observacion = string.IsNullOrWhiteSpace(data.observacion) ? null : data.observacion,
                    fi          = string.IsNullOrWhiteSpace(data.fecha_inicio) ? null : data.fecha_inicio,
                    fv          = string.IsNullOrWhiteSpace(data.fecha_vence) ? null : data.fecha_vence,
                    monto       = data.monto,
                    num_poliza  = string.IsNullOrWhiteSpace(data.num_poliza) ? null : data.num_poliza,
                    documento   = string.IsNullOrWhiteSpace(data.documento) ? null : data.documento,
                    reg_usuario = string.IsNullOrWhiteSpace(data.registro_usuario) ? null : data.registro_usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = string.IsNullOrWhiteSpace(data.registro_usuario) ? "" : data.registro_usuario,
                    DetalleMovimiento = $"Póliza: {data.cod_poliza} - {data.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                resp.Description = "Póliza Ingresada Satisfactoriamente!";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Actualiza una póliza existente.
        /// </summary>
        private ErrorDto Activos_Polizas_Actualizar(int CodEmpresa, ActivosPolizasData data)
        {
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = @"
                        UPDATE dbo.ACTIVOS_POLIZAS
                           SET TIPO_POLIZA      = @tipo,
                               DESCRIPCION      = @descripcion,
                               OBSERVACION      = @observacion,
                               FECHA_INICIO     = @fi,
                               FECHA_VENCE      = @fv,
                               MONTO            = @monto,
                               NUM_POLIZA       = @num_poliza,
                               DOCUMENTO        = @documento,
                               MODIFICA_USUARIO = @mod_usuario,
                               MODIFICA_FECHA   = SYSDATETIME()
                         WHERE COD_POLIZA = @cod";

                connection.Execute(query, new
                {
                    cod         = data.cod_poliza.ToUpper(),
                    tipo        = data.tipo_poliza.ToUpper(),
                    descripcion = data.descripcion?.ToUpper(),
                    observacion = string.IsNullOrWhiteSpace(data.observacion) ? null : data.observacion,
                    fi          = string.IsNullOrWhiteSpace(data.fecha_inicio) ? null : data.fecha_inicio,
                    fv          = string.IsNullOrWhiteSpace(data.fecha_vence) ? null : data.fecha_vence,
                    monto       = data.monto,
                    num_poliza  = string.IsNullOrWhiteSpace(data.num_poliza) ? null : data.num_poliza,
                    documento   = string.IsNullOrWhiteSpace(data.documento) ? null : data.documento,
                    mod_usuario = string.IsNullOrWhiteSpace(data.modifica_usuario) ? null : data.modifica_usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = string.IsNullOrWhiteSpace(data.modifica_usuario) ? "" : data.modifica_usuario,
                    DetalleMovimiento = $"Póliza: {data.cod_poliza} - {data.descripcion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                resp.Description = "Póliza Actualizada Satisfactoriamente!";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Elimina una póliza.
        /// </summary>
        public ErrorDto Activos_Polizas_Eliminar(int CodEmpresa, string usuario, string cod_poliza)
        {
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var qAsg = @"SELECT COUNT(*) FROM dbo.ACTIVOS_POLIZAS_ASG WHERE COD_POLIZA = @cod";
                int cntAsg = connection.QueryFirstOrDefault<int>(qAsg, new { cod = cod_poliza.ToUpper() });
                if (cntAsg > 0)
                {
                    resp.Code = -2;
                    resp.Description = "La póliza tiene activos asignados. Debe desasignarlos antes de eliminar.";
                    return resp;
                }

                var query = @"DELETE FROM dbo.ACTIVOS_POLIZAS 
                              WHERE COD_POLIZA = @cod_poliza";

                int rows = connection.Execute(query, new { cod_poliza = cod_poliza.ToUpper() });

                if (rows == 0)
                {
                    resp.Code = -2;
                    resp.Description = $"La póliza {cod_poliza.ToUpper()} no existe.";
                    return resp;
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Póliza: {cod_poliza}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Lista de tipos de póliza.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Polizas_Tipos_Listar(int CodEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "",
                Result = new List<DropDownListaGenericaModel>(),
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = @"SELECT TIPO_POLIZA AS item, DESCRIPCION 
                              FROM dbo.ACTIVOS_POLIZAS_TIPOS
                              ORDER BY DESCRIPCION";
                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Lista de tipos de activo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Tipo_Activo_Listar(int CodEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "",
                Result = new List<DropDownListaGenericaModel>(),
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = @"
                    SELECT TIPO_ACTIVO AS item,
                           DESCRIPCION
                    FROM dbo.ACTIVOS_TIPO_ACTIVO
                    ORDER BY DESCRIPCION";
                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Lista de activos para asignación de una póliza con lazy loading.
        /// </summary>
        public ErrorDto<ActivosPolizasLista> Activos_Polizas_Asignacion_Listar(
            int CodEmpresa,
            string cod_poliza,
            string? tipo_activo,
            FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<ActivosPolizasLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosPolizasLista()
            };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_poliza))
                {
                    resp.Code = -1;
                    resp.Description = "Debe indicar la póliza.";
                    resp.Result.total = 0;
                    resp.Result.lista = [];
                    return resp;
                }

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var where = " WHERE 1=1 ";
                var p = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(tipo_activo))
                {
                    where += " AND A.TIPO_ACTIVO = @tipo_activo";
                    p.Add("@tipo_activo", tipo_activo);
                }

                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    where += @" AND (
                                   A.NUM_PLACA LIKE @filtro
                                OR A.NOMBRE    LIKE @filtro
                               )";
                    p.Add("@filtro", $"%{filtros.filtro}%");
                }

                p.Add("@p", cod_poliza.ToUpper());

                // total
                string queryTotal = $@"
                    SELECT COUNT(1)
                    FROM dbo.ACTIVOS_PRINCIPAL A
                    {where}";
                resp.Result.total = cn.QueryFirstOrDefault<int>(queryTotal, p);

                // Ordenamiento seguro
                var sortFieldRaw  = filtros?.sortField ?? _numplaca;
                var sortFieldNorm = sortFieldRaw.Trim().ToUpperInvariant();

                string orderByCol = sortFieldNorm switch
                {
                    _numplaca or "NUM_PLACA" => _numplaca,
                    "A.NOMBRE"    or "NOMBRE"    => "A.NOMBRE",
                    "A.ESTADO"    or "ESTADO"    => "A.ESTADO",
                    _                            => _numplaca
                };

                string sortOrder = filtros?.sortOrder == 0 ? "DESC" : "ASC";

                int pagina     = filtros?.pagina     ?? 0;
                int paginacion = filtros?.paginacion ?? 50;

                string query = $@"
                    SELECT 
                        A.NUM_PLACA AS num_placa,
                        A.NOMBRE    AS nombre,
                        A.ESTADO    AS estado,
                        IIF(X.COD_POLIZA IS NULL, 0, 1) AS asignado
                    FROM dbo.ACTIVOS_PRINCIPAL A
                    LEFT JOIN dbo.ACTIVOS_POLIZAS_ASG X
                           ON X.NUM_PLACA = A.NUM_PLACA
                          AND X.COD_POLIZA = @p
                    {where}
                    ORDER BY {orderByCol} {sortOrder}
                    OFFSET @offset ROWS 
                    FETCH NEXT @rows ROWS ONLY;";

                p.Add("@offset", pagina);
                p.Add("@rows",   paginacion);

                var filas = cn.Query<ActivosPolizasAsignacionItem>(query, p).ToList();
                resp.Description = JsonConvert.SerializeObject(filas); // si ya lo usabas así, lo mantengo
                resp.Result.lista = filas.Select(f => new ActivosPolizasData
                {
                    cod_poliza   = cod_poliza.ToUpper(),
                    descripcion  = f.nombre
                }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = [];
            }

            return resp;
        }

        /// <summary>
        /// Lista de activos para asignación de una póliza sin paginación (export).
        /// </summary>
        public ErrorDto<List<ActivosPolizasAsignacionItem>> Activos_Polizas_Asignacion_Listar_Export(
            int CodEmpresa,
            string cod_poliza,
            string? tipo_activo,
            FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<List<ActivosPolizasAsignacionItem>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosPolizasAsignacionItem>()
            };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_poliza))
                {
                    resp.Code = -1;
                    resp.Description = "Debe indicar la póliza.";
                    resp.Result = null;
                    return resp;
                }

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var where = " WHERE 1=1 ";
                var p = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(tipo_activo))
                {
                    where += " AND A.TIPO_ACTIVO = @tipo_activo";
                    p.Add("@tipo_activo", tipo_activo);
                }

                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    where += @" AND (
                                   A.NUM_PLACA LIKE @filtro
                                OR A.NOMBRE    LIKE @filtro
                               )";
                    p.Add("@filtro", $"%{filtros.filtro}%");
                }

                p.Add("@p", cod_poliza.ToUpper());

                string query = $@"
                    SELECT 
                        A.NUM_PLACA AS num_placa,
                        A.NOMBRE    AS nombre,
                        A.ESTADO    AS estado,
                        IIF(X.COD_POLIZA IS NULL, 0, 1) AS asignado
                    FROM dbo.ACTIVOS_PRINCIPAL A
                    LEFT JOIN dbo.ACTIVOS_POLIZAS_ASG X
                           ON X.NUM_PLACA = A.NUM_PLACA
                          AND X.COD_POLIZA = @p
                    {where}
                    ORDER BY A.NUM_PLACA;";

                resp.Result = cn.Query<ActivosPolizasAsignacionItem>(query, p).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Asigna una lista de placas a la póliza.
        /// </summary>
        public ErrorDto Activos_Polizas_Asignar(int CodEmpresa, string usuario, string cod_poliza, List<string> placas)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_poliza) || placas == null || placas.Count == 0)
                    return new ErrorDto { Code = -1, Description = "Datos insuficientes para asignar." };

                using var connection = _portalDB.CreateConnection(CodEmpresa);
                connection.Open();
                using var tx = connection.BeginTransaction();

                var insert = @"
                    IF NOT EXISTS(SELECT 1 FROM dbo.ACTIVOS_POLIZAS_ASG WHERE COD_POLIZA=@p AND NUM_PLACA=@pl)
                    INSERT INTO dbo.ACTIVOS_POLIZAS_ASG (COD_POLIZA, NUM_PLACA, REGISTRO_FECHA, REGISTRO_USUARIO)
                    VALUES (@p, @pl, SYSDATETIME(), @u)";

                foreach (var pl in placas)
                {
                    connection.Execute(insert, new { p = cod_poliza.ToUpper(), pl, u = usuario }, tx);
                }

                tx.Commit();

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Póliza: {cod_poliza} - Asigna {placas.Count} activo(s)",
                    Movimiento = "Asigna - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Desasigna una lista de placas de la póliza.
        /// </summary>
        public ErrorDto Activos_Polizas_Desasignar(int CodEmpresa, string usuario, string cod_poliza, List<string> placas)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_poliza) || placas == null || placas.Count == 0)
                    return new ErrorDto { Code = -1, Description = "Datos insuficientes para desasignar." };

                using var connection = _portalDB.CreateConnection(CodEmpresa);
                connection.Open();
                using var tx = connection.BeginTransaction();

                var delete = @"
                    DELETE FROM dbo.ACTIVOS_POLIZAS_ASG
                    WHERE COD_POLIZA=@p AND NUM_PLACA=@pl";

                foreach (var pl in placas)
                {
                    connection.Execute(delete, new { p = cod_poliza.ToUpper(), pl }, tx);
                }

                tx.Commit();

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Póliza: {cod_poliza} - Desasigna {placas.Count} activo(s)",
                    Movimiento = "Desasigna - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }
    }
}