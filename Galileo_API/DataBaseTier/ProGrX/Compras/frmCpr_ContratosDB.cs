using System.Data;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCprContratosDB
    {
        private readonly PortalDB _portalDB;
        private readonly EnvioCorreoDB _envioCorreoDB;

        private readonly string sendEmail;
        private readonly string nofiticacionConfeccionContrato;
        private readonly string codNotificaciones;

        private const string Ok = "Ok";
        private const string ErrorDesconocido = "Error desconocido.";

        private const string MovInserta = "Inserta";
        private const string MovInsertar = "Insertar";
        private const string MovActualiza = "Actualiza";
        private const string MovElimina = "Elimina";
        private const string MovNotifica = "Notifica";

        private const int ErrorCode = -1;

        public FrmCprContratosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _envioCorreoDB = new EnvioCorreoDB(config);

            sendEmail = config.GetSection("AppSettings").GetSection("EnviaEmail").Value?.ToString() ?? "";
            nofiticacionConfeccionContrato = config.GetSection("Crp_Compras").GetSection("NotiConfeccionContrato").Value?.ToString() ?? "";
            codNotificaciones = config.GetSection("AppSettings").GetSection("Notificaciones").Value?.ToString() ?? "";
        }

        // Helper: usa DbHelper.WithConn y devuelve ErrorDto plano (sin ErrorDto<ErrorDto>)
        private ErrorDto WithConn(int codEmpresa, Func<SqlConnection, ErrorDto> action)
        {
            var r = DbHelper.WithConn(_portalDB, codEmpresa, action);

            return r.Code == 0
                ? (r.Result ?? DbHelper.ErrorResponse(ErrorDesconocido, ErrorCode))
                : DbHelper.ErrorResponse(r.Description ?? ErrorDesconocido, ErrorCode);
        }

        /// <summary>
        /// Obtiene un contrato mediante el código de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<CprContratosDto> CprContrato_Obtener(int CodEmpresa, string cod_contrato)
        {
            try
            {
                var sql = @"
                        SELECT C.*, P.DESCRIPCION as PROVEEDOR
                        FROM CPR_CONTRATOS C
                        INNER JOIN CXP_PROVEEDORES P ON C.COD_PROVEEDOR = P.COD_PROVEEDOR
                        WHERE C.COD_CONTRATO = @cod_contrato;";

                var r = DbHelper.ExecuteSingleQuery<CprContratosDto>(
                    _portalDB,
                    CodEmpresa,
                    sql,
                    defaultValue: null,
                    parameters: new { cod_contrato }
                );

                if (r.Code != 0)
                    return new ErrorDto<CprContratosDto> { Code = ErrorCode, Description = r.Description, Result = null };

                return new ErrorDto<CprContratosDto> { Code = 0, Description = Ok, Result = r.Result };
            }
            catch (Exception ex)
            {
                return new ErrorDto<CprContratosDto> { Code = ErrorCode, Description = ex.Message, Result = null };
            }
        }


        /// <summary>
        /// Obtiene la lista de contratos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CprContratosLista> CprContratosLista_Obtener(int CodEmpresa, string filtros)
        {
            var response = new ErrorDto<CprContratosLista>
            {
                Code = 0,
                Result = new CprContratosLista { total = 0, contratos = new List<CprContratosDto>() }
            };

            try
            {
                var filtro = JsonConvert.DeserializeObject<CprContratosFiltros>(filtros) ?? new CprContratosFiltros();
                var p = new DynamicParameters();
                var where = BuildContratoListWhere(filtro, p);
                var paginaSql = BuildPaginacionSql(filtro, p);

                var countSql = $@"
                                SELECT COUNT(*)
                                FROM CPR_CONTRATOS C
                                INNER JOIN CXP_PROVEEDORES P ON C.COD_PROVEEDOR = P.COD_PROVEEDOR
                                {where};";

                var dataSql = $@"
                            SELECT
                                C.*,
                                P.DESCRIPCION AS PROVEEDOR,
                                V.ESTADO AS estado
                            FROM CPR_CONTRATOS C
                            INNER JOIN CXP_PROVEEDORES P ON C.COD_PROVEEDOR = P.COD_PROVEEDOR
                            OUTER APPLY
                            (
                                SELECT TOP 1 ce.ESTADO
                                FROM CPR_CONTRATOS_ESTADOS ce
                                WHERE ce.COD_CONTRATO = C.COD_CONTRATO
                                AND ce.FECHA_INICIO <= GETDATE()
                                ORDER BY ce.FECHA_INICIO DESC
                            ) V
                            {where}
                            ORDER BY C.COD_CONTRATO DESC
                            {paginaSql};";

                var totalResp = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, countSql, 0, p);
                if (totalResp.Code != 0)
                {
                    response.Code = ErrorCode;
                    response.Description = totalResp.Description;
                    response.Result = null;
                    return response;
                }

                response.Result.total = totalResp.Result;

                var listResp = DbHelper.ExecuteListQuery<CprContratosDto>(_portalDB, CodEmpresa, dataSql, p);
                if (listResp.Code != 0)
                {
                    response.Code = ErrorCode;
                    response.Description = listResp.Description;
                    response.Result = null;
                    return response;
                }

                response.Result.contratos = listResp.Result ?? new List<CprContratosDto>();
            }
            catch (Exception ex)
            {
                response.Code = ErrorCode;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Construye la cláusula WHERE para la obtención de la lista de contratos.
        /// </summary>
        /// <param name="filtro"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static string BuildContratoListWhere(CprContratosFiltros filtro, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtro.filtro)) return string.Empty;
            p.Add("@Q", $"%{filtro.filtro}%");
            return "WHERE (C.cod_contrato LIKE @Q OR C.descripcion LIKE @Q OR P.DESCRIPCION LIKE @Q)";
        }


        /// <summary>
        /// Construye la cláusula de paginación SQL.
        /// </summary>
        /// <param name="filtro"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static string BuildPaginacionSql(CprContratosFiltros filtro, DynamicParameters p)
        {
            if (filtro.pagina == null || filtro.paginacion == null) return string.Empty;
            p.Add("@Off", filtro.pagina);
            p.Add("@Take", filtro.paginacion);
            return "OFFSET @Off ROWS FETCH NEXT @Take ROWS ONLY";
        }


        /// <summary>
        /// Inserta un contrato.
        /// </summary>
        public ErrorDto CprContrato_Insertar(int CodEmpresa, CprContratosDto contrato)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();


                    if (ContratoExiste(conn, tx, contrato.cod_contrato?.Trim() ?? ""))
                        return DbHelper.ErrorResponse($"Ya existe el registro de un contrato con el código: {contrato.cod_contrato}", ErrorCode);

                    InsertarContrato(conn, tx, contrato);
                    InsertarEstadoInicialBorrador(conn, tx, contrato.cod_contrato ?? "", contrato.registro_usuario ?? "");

                    InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                    {
                        cod_contrato = contrato.cod_contrato ?? string.Empty,
                        movimiento = MovInserta,
                        detalle = "Ingresa Contrato",
                        registro_usuario = contrato.registro_usuario ?? string.Empty
                    });

                    tx.Commit();
                    return DbHelper.OkResponse("Contrato agregado correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        /// Verifica si un contrato ya existe.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tx"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        private static bool ContratoExiste(SqlConnection conn, IDbTransaction tx, string codContrato)
        {
            const string sql = "SELECT COUNT(*) FROM CPR_CONTRATOS WHERE COD_CONTRATO = @cod_contrato;";
            return conn.ExecuteScalar<int>(sql, new { cod_contrato = codContrato }, tx) > 0;
        }


        /// <summary>
        /// Inserta la información del contrato.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tx"></param>
        /// <param name="c"></param>
        private static void InsertarContrato(SqlConnection conn, IDbTransaction tx, CprContratosDto c)
        {
            const string sql = @"
                            INSERT INTO CPR_CONTRATOS
                            (
                                COD_CONTRATO, DESCRIPCION, COD_PROVEEDOR, TIPO_CONTRATO, REGISTRO_FECHA, REGISTRO_USUARIO,
                                MONTO, CTA_CONTABLE, NOTAS, DIVISA, COD_CENTRO_COSTO, FISCAL,
                                PORCENTAJE_GARANTIA, MONTO_GARANTIA, DIVISA_GARANTIA,
                                FECHA_INICIO, FECHA_CORTE, PLAZO, CANTIDAD_PLAZO, PERIODO_GARANTIA, FECHA_VENCIMIENTO
                            )
                            VALUES
                            (
                                @cod_contrato, @descripcion, @cod_proveedor, @tipo_contrato, GETDATE(), @registro_usuario,
                                @monto, @cta_contable, @notas, @divisa, @cod_centro_costo, @fiscal,
                                @porcentaje_garantia, @monto_garantia, @divisa_garantia,
                                @fecha_inicio, @fecha_corte, @plazo, @cantidad_plazo, @periodo_garantia, @fecha_vencimiento
                            );";

            conn.Execute(sql, new
            {
                c.cod_contrato,
                c.descripcion,
                c.cod_proveedor,
                c.tipo_contrato,
                c.registro_usuario,
                c.monto,
                c.cta_contable,
                c.notas,
                c.divisa,
                c.cod_centro_costo,
                c.fiscal,
                c.porcentaje_garantia,
                c.monto_garantia,
                c.divisa_garantia,
                c.fecha_inicio,
                c.fecha_corte,
                c.plazo,
                c.cantidad_plazo,
                c.periodo_garantia,
                c.fecha_vencimiento
            }, tx);
        }


        /// <summary>
        /// Inserta el estado inicial de borrador para un contrato.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tx"></param>
        /// <param name="codContrato"></param>
        /// <param name="usuario"></param>
        private static void InsertarEstadoInicialBorrador(SqlConnection conn, IDbTransaction tx, string codContrato, string usuario)
        {
            const string sql = @"
                            INSERT INTO CPR_CONTRATOS_ESTADOS
                            (COD_CONTRATO, ESTADO, FECHA_INICIO, NOTAS, REGISTRO_FECHA, REGISTRO_USUARIO)
                            VALUES
                            (@cod_contrato, 'B', GETDATE(), 'Se crea borrador', GETDATE(), @registro_usuario);";

            conn.Execute(sql, new { cod_contrato = codContrato, registro_usuario = usuario }, tx);
        }


        /// <summary>
        /// Actualiza información del contrato.
        /// </summary>
        public ErrorDto CprContrato_Actualizar(int CodEmpresa, CprContratosDto contrato)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    var (sql, args) = BuildContratoUpdate(contrato);
                    conn.Execute(sql, args, tx);

                    InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                    {
                        cod_contrato = contrato.cod_contrato,
                        movimiento = MovActualiza,
                        detalle = "Modifica datos de Contrato",
                        registro_usuario = contrato.registro_usuario
                    });

                    tx.Commit();
                    return DbHelper.OkResponse("Contrato actualizado correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        /// Construye la sentencia SQL para actualizar un contrato.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static (string sql, object args) BuildContratoUpdate(CprContratosDto c)
        {
            var sb = new StringBuilder(@"
                                    UPDATE CPR_CONTRATOS
                                    SET
                                        DESCRIPCION = @descripcion,
                                        COD_PROVEEDOR = @cod_proveedor,
                                        TIPO_CONTRATO = @tipo_contrato,
                                        MODIFICA_FECHA = GETDATE(),
                                        MODIFICA_USUARIO = @modifica_usuario,
                                        MONTO = @monto,
                                        CTA_CONTABLE = @cta_contable,
                                        NOTAS = @notas,
                                        DIVISA = @divisa,
                                        COD_CENTRO_COSTO = @cod_centro_costo,
                                        FISCAL = @fiscal,
                                        PORCENTAJE_GARANTIA = @porcentaje_garantia,
                                        MONTO_GARANTIA = @monto_garantia,
                                        DIVISA_GARANTIA = @divisa_garantia
                                    ");

            if (c.fecha_inicio != null) sb.Append(", FECHA_INICIO = @fecha_inicio");
            if (c.fecha_corte != null) sb.Append(", FECHA_CORTE = @fecha_corte");
            if (c.plazo != null) sb.Append(", PLAZO = @plazo, CANTIDAD_PLAZO = @cantidad_plazo");
            if (c.periodo_garantia != null) sb.Append(", PERIODO_GARANTIA = @periodo_garantia");
            if (c.fecha_vencimiento != null) sb.Append(", FECHA_VENCIMIENTO = @fecha_vencimiento");

            sb.Append(" WHERE COD_CONTRATO = @cod_contrato;");

            return (sb.ToString(), new
            {
                c.descripcion,
                c.cod_proveedor,
                c.tipo_contrato,
                c.modifica_usuario,
                c.monto,
                c.cta_contable,
                c.notas,
                c.divisa,
                c.cod_centro_costo,
                c.fiscal,
                c.porcentaje_garantia,
                c.monto_garantia,
                c.divisa_garantia,
                c.fecha_inicio,
                c.fecha_corte,
                c.plazo,
                c.cantidad_plazo,
                c.periodo_garantia,
                c.fecha_vencimiento,
                c.cod_contrato
            });
        }


        /// <summary>
        /// Elimina información del contrato.
        /// </summary>
        public ErrorDto CprContrato_Eliminar(int CodEmpresa, string cod_contrato, string usuario)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    conn.Execute("DELETE FROM CPR_CONTRATOS WHERE COD_CONTRATO = @cod_contrato;", new { cod_contrato }, tx);

                    InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                    {
                        cod_contrato = cod_contrato,
                        movimiento = MovElimina,
                        detalle = "Elimina Contrato",
                        registro_usuario = usuario
                    });

                    tx.Commit();
                    return DbHelper.OkResponse("Contrato eliminado correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        /// Obtiene los adendums de un contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosAdendumsDto>> CprContrato_Adendums_Obtener(int CodEmpresa, string cod_contrato)
        {
            return DbHelper.ExecuteListQuery<CprContratosAdendumsDto>(
                _portalDB,
                CodEmpresa,
                "SELECT * FROM CPR_CONTRATOS_ADENDUMS WHERE COD_CONTRATO = @cod_contrato;",
                new { cod_contrato }
            );
        }

        /// <summary>
        ///     Guarda un adendum de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="adendum"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Adendum_Guardar(int CodEmpresa, CprContratosAdendumsDto adendum)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    bool existe = conn.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM CPR_CONTRATOS_ADENDUMS WHERE COD_CONTRATO=@cod AND COD_CONTRATO_MADRE=@madre;",
                        new { cod = adendum.cod_contrato, madre = adendum.cod_contrato_madre },
                        tx
                    ) > 0;

                    if (existe)
                    {
                        conn.Execute(
                            @"UPDATE CPR_CONTRATOS_ADENDUMS
                              SET NOTAS = @notas
                              WHERE COD_CONTRATO = @cod AND COD_CONTRATO_MADRE = @madre;",
                            new { notas = adendum.notas, cod = adendum.cod_contrato, madre = adendum.cod_contrato_madre },
                            tx
                        );

                        InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                        {
                            cod_contrato = adendum.cod_contrato,
                            movimiento = MovActualiza,
                            detalle = "Modifica Adendum con Contrato " + (adendum.cod_contrato_madre ?? ""),
                            registro_usuario = adendum.registro_usuario
                        });
                    }
                    else
                    {
                        conn.Execute(
                            @"INSERT INTO CPR_CONTRATOS_ADENDUMS
                              (COD_CONTRATO, COD_CONTRATO_MADRE, NOTAS, REGISTRO_FECHA, REGISTRO_USUARIO)
                              VALUES (@cod, @madre, @notas, GETDATE(), @usr);",
                            new { cod = adendum.cod_contrato, madre = adendum.cod_contrato_madre, notas = adendum.notas, usr = adendum.registro_usuario },
                            tx
                        );

                        InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                        {
                            cod_contrato = adendum.cod_contrato,
                            movimiento = MovInsertar,
                            detalle = "Ingresa Adendum con Contrato " + (adendum.cod_contrato_madre ?? ""),
                            registro_usuario = adendum.registro_usuario
                        });
                    }

                    tx.Commit();
                    return DbHelper.OkResponse("Adendum guardado correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        ///   Elimina un adendum de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_adendum"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Adendum_Eliminar(int CodEmpresa, int id_adendum, string usuario)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    var info = conn.QueryFirstOrDefault<(string? cod, string? madre)>(
                        "SELECT COD_CONTRATO AS cod, COD_CONTRATO_MADRE AS madre FROM CPR_CONTRATOS_ADENDUMS WHERE ID_ADDENDUM=@id;",
                        new { id = id_adendum },
                        tx
                    );

                    var cod = info.cod ?? "";
                    var madre = info.madre ?? "";

                    conn.Execute("DELETE FROM CPR_CONTRATOS_ADENDUMS WHERE ID_ADDENDUM=@id;", new { id = id_adendum }, tx);

                    InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                    {
                        cod_contrato = cod,
                        movimiento = MovElimina,
                        detalle = "Elimina de Adendum con Contrato " + madre,
                        registro_usuario = usuario
                    });

                    tx.Commit();
                    return DbHelper.OkResponse("Adendum eliminado correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        /// Obtiene los estados de un contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosEstadosDto>> CprContrato_Estados_Obtener(int CodEmpresa, string cod_contrato)
        {
            return DbHelper.ExecuteListQuery<CprContratosEstadosDto>(
                _portalDB,
                CodEmpresa,
                "SELECT * FROM CPR_CONTRATOS_ESTADOS WHERE COD_CONTRATO = @cod_contrato;",
                new { cod_contrato }
            );
        }


        /// <summary>
        /// Guarda un estado de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Estados_Guardar(int CodEmpresa, CprContratosEstadosDto estado)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    bool existe = conn.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM CPR_CONTRATOS_ESTADOS WHERE COD_CONTRATO=@cod AND ESTADO=@est;",
                        new { cod = estado.cod_contrato, est = estado.estado },
                        tx
                    ) > 0;

                    if (existe)
                    {
                        conn.Execute(
                            @"UPDATE CPR_CONTRATOS_ESTADOS
                              SET FECHA_INICIO=@fecha_inicio, NOTAS=@notas, REGISTRO_FECHA=GETDATE(), REGISTRO_USUARIO=@usr
                              WHERE COD_CONTRATO=@cod AND ESTADO=@est;",
                            new
                            {
                                fecha_inicio = estado.fecha_inicio,
                                notas = estado.notas,
                                usr = estado.registro_usuario,
                                cod = estado.cod_contrato,
                                est = estado.estado
                            },
                            tx
                        );
                    }
                    else
                    {
                        conn.Execute(
                            @"INSERT INTO CPR_CONTRATOS_ESTADOS
                              (COD_CONTRATO, ESTADO, FECHA_INICIO, NOTAS, REGISTRO_FECHA, REGISTRO_USUARIO)
                              VALUES (@cod, @est, @fecha_inicio, @notas, GETDATE(), @usr);",
                            new
                            {
                                cod = estado.cod_contrato,
                                est = estado.estado,
                                fecha_inicio = estado.fecha_inicio,
                                notas = estado.notas,
                                usr = estado.registro_usuario
                            },
                            tx
                        );
                    }

                    var descEstado = EstadoDescripcion_Obtener(CodEmpresa, estado.estado ?? "");

                    InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                    {
                        cod_contrato = estado.cod_contrato,
                        movimiento = existe ? MovActualiza : MovInserta,
                        detalle = (existe ? "Modifica datos de estado: " : "Agrega estado: ") + descEstado,
                        registro_usuario = estado.registro_usuario
                    });

                    tx.Commit();
                    return DbHelper.OkResponse("Estado guardado correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        ///   Elimina un estado de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="linea_id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Estados_Eliminar(int CodEmpresa, int linea_id, string usuario)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    var info = conn.QueryFirstOrDefault<(string? cod, string? est)>(
                        "SELECT COD_CONTRATO AS cod, ESTADO AS est FROM CPR_CONTRATOS_ESTADOS WHERE LINEA_ID=@id;",
                        new { id = linea_id },
                        tx
                    );

                    var cod = info.cod ?? "";
                    var est = info.est ?? "";

                    conn.Execute("DELETE FROM CPR_CONTRATOS_ESTADOS WHERE LINEA_ID=@id;", new { id = linea_id }, tx);

                    InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                    {
                        cod_contrato = cod,
                        movimiento = MovElimina,
                        detalle = "Elimina estado: " + EstadoDescripcion_Obtener(CodEmpresa, est),
                        registro_usuario = usuario
                    });

                    tx.Commit();
                    return DbHelper.OkResponse("Estado eliminado correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        ///  Obtiene la descripción de un estado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosProductosDto>> CprContrato_Productos_Obtener(int CodEmpresa, string cod_contrato)
        {
            var sql = @"
                    SELECT C.*, P.DESCRIPCION
                    FROM CPR_CONTRATOS_PRODUCTOS C
                    LEFT JOIN PV_PRODUCTOS P ON C.COD_PRODUCTO = P.COD_PRODUCTO
                    WHERE C.COD_CONTRATO = @cod_contrato;";

            return DbHelper.ExecuteListQuery<CprContratosProductosDto>(_portalDB, CodEmpresa, sql, new { cod_contrato });
        }


        /// <summary>
        ///  Guarda un producto de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="producto"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Producto_Guardar(int CodEmpresa, CprContratosProductosDto producto)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    bool existe = conn.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM CPR_CONTRATOS_PRODUCTOS WHERE COD_CONTRATO=@cod AND COD_PRODUCTO=@prod;",
                        new { cod = producto.cod_contrato, prod = producto.cod_producto },
                        tx
                    ) > 0;

                    if (existe)
                        return DbHelper.ErrorResponse($"El producto código {producto.cod_producto} ya se encontraba agregado", ErrorCode);

                    conn.Execute(
                        @"INSERT INTO CPR_CONTRATOS_PRODUCTOS (COD_CONTRATO, COD_PRODUCTO, REGISTRO_FECHA, REGISTRO_USUARIO)
                          VALUES (@cod, @prod, GETDATE(), @usr);",
                        new { cod = producto.cod_contrato, prod = producto.cod_producto, usr = producto.registro_usuario },
                        tx
                    );

                    InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                    {
                        cod_contrato = producto.cod_contrato,
                        movimiento = MovInserta,
                        detalle = "Agrega producto Cod. " + (producto.cod_producto ?? ""),
                        registro_usuario = producto.registro_usuario
                    });

                    tx.Commit();
                    return DbHelper.OkResponse("Producto agregado correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        ///  Elimina un producto de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="linea_id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Producto_Eliminar(int CodEmpresa, int linea_id, string usuario)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    var info = conn.QueryFirstOrDefault<(string? cod, string? prod)>(
                        "SELECT COD_CONTRATO AS cod, COD_PRODUCTO AS prod FROM CPR_CONTRATOS_PRODUCTOS WHERE LINEA_ID=@id;",
                        new { id = linea_id },
                        tx
                    );

                    var cod = info.cod ?? "";
                    var prod = info.prod ?? "";

                    conn.Execute("DELETE FROM CPR_CONTRATOS_PRODUCTOS WHERE LINEA_ID=@id;", new { id = linea_id }, tx);

                    InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                    {
                        cod_contrato = cod,
                        movimiento = MovElimina,
                        detalle = "Elimina producto Cod. " + prod,
                        registro_usuario = usuario
                    });

                    tx.Commit();
                    return DbHelper.OkResponse("Linea eliminada correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        /// Obtiene las prorrogas de un contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosProrrogasDto>> CprContrato_Prorroga_Obtener(int CodEmpresa, string cod_contrato)
        {
            return DbHelper.ExecuteListQuery<CprContratosProrrogasDto>(
                _portalDB,
                CodEmpresa,
                "SELECT * FROM CPR_CONTRATOS_PRORROGAS WHERE COD_CONTRATO = @cod_contrato;",
                new { cod_contrato }
            );
        }


        /// <summary>
        ///  Guarda una prorroga de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="prorroga"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Prorroga_Guardar(int CodEmpresa, CprContratosProrrogasDto prorroga)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    if (prorroga.id_prorroga == 0)
                    {
                        var id = conn.QuerySingle<int>(
                            @"INSERT INTO CPR_CONTRATOS_PRORROGAS (COD_CONTRATO, FECHA, MOTIVOS, REGISTRO_FECHA, REGISTRO_USUARIO)
                              VALUES (@cod, @fecha, @motivos, GETDATE(), @usr);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);",
                            new { cod = prorroga.cod_contrato, fecha = prorroga.fecha, motivos = prorroga.motivos, usr = prorroga.registro_usuario },
                            tx
                        );

                        InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                        {
                            cod_contrato = prorroga.cod_contrato,
                            movimiento = MovInserta,
                            detalle = "Ingresa prorroga Id: " + id,
                            registro_usuario = prorroga.registro_usuario
                        });

                        tx.Commit();
                        return DbHelper.OkResponse("Prorroga agregada correctamente");
                    }

                    conn.Execute(
                        @"UPDATE CPR_CONTRATOS_PRORROGAS
                          SET FECHA = @fecha, MOTIVOS = @motivos
                          WHERE ID_PRORROGA = @id;",
                        new { fecha = prorroga.fecha, motivos = prorroga.motivos, id = prorroga.id_prorroga },
                        tx
                    );

                    InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                    {
                        cod_contrato = prorroga.cod_contrato,
                        movimiento = MovActualiza,
                        detalle = "Modifica datos de prorroga Id: " + prorroga.id_prorroga,
                        registro_usuario = prorroga.registro_usuario
                    });

                    tx.Commit();
                    return DbHelper.OkResponse("Prorroga actualizada correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        ///  Elimina una prorroga de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_prorroga"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CprContrato_Prorroga_Eliminar(int CodEmpresa, int id_prorroga, string usuario)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Open();
                    using var tx = conn.BeginTransaction();

                    var codContrato = conn.QueryFirstOrDefault<string>(
                        "SELECT COD_CONTRATO FROM CPR_CONTRATOS_PRORROGAS WHERE ID_PRORROGA = @id;",
                        new { id = id_prorroga },
                        tx
                    ) ?? "";

                    conn.Execute("DELETE FROM CPR_CONTRATOS_PRORROGAS WHERE ID_PRORROGA = @id;", new { id = id_prorroga }, tx);

                    InsertarBitacora(conn, tx, new CprContratosBitacoraDto
                    {
                        cod_contrato = codContrato,
                        movimiento = MovElimina,
                        detalle = "Elimina prorroga Id: " + id_prorroga,
                        registro_usuario = usuario
                    });

                    tx.Commit();
                    return DbHelper.OkResponse("Prorroga eliminada correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, ErrorCode);
            }
        }


        /// <summary>
        /// Obtiene la bitácora de un contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CprContratosBitacoraDto>> CprContrato_Bitacora_Obtener(int CodEmpresa, string cod_contrato)
        {
            return DbHelper.ExecuteListQuery<CprContratosBitacoraDto>(
                _portalDB,
                CodEmpresa,
                "SELECT * FROM CPR_CONTRATOS_BITACORA WHERE COD_CONTRATO = @cod_contrato;",
                new { cod_contrato }
            );
        }


        /// <summary>
        ///   Inserta un registro en la bitácora de un contrato.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tx"></param>
        /// <param name="req"></param>
        private static void InsertarBitacora(SqlConnection conn, IDbTransaction tx, CprContratosBitacoraDto req)
        {
            conn.Execute(
                @"INSERT INTO CPR_CONTRATOS_BITACORA
                  (COD_CONTRATO, MOVIMIENTO, DETALLE, REGISTRO_FECHA, REGISTRO_USUARIO)
                  VALUES (@cod_contrato, @movimiento, @detalle, GETDATE(), @registro_usuario);",
                req,
                tx
            );
        }


        /// <summary>
        /// Obtiene la descripción de un estado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        private string EstadoDescripcion_Obtener(int CodEmpresa, string estado)
        {
            try
            {
                var sql = @"
                        SELECT DESCRIPCION
                        FROM CPR_CATALOGOS
                        WHERE Tipo_Id = (SELECT TIPO_ID FROM CPR_CATALOGOS_TIPOS WHERE DESCRIPCION = 'Estados Contrato')
                        AND CATALOGO_ID = @estado;";

                var r = DbHelper.ExecuteSingleQuery<string>(_portalDB, CodEmpresa, sql, estado, new { estado });
                return r.Code == 0 && !string.IsNullOrWhiteSpace(r.Result) ? r.Result : estado;
            }
            catch
            {
                return estado;
            }
        }


        /// <summary>
        ///  Envía notificación para confección de contrato.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_contrato"></param>
        /// <param name="mensaje"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public async Task<ErrorDto> CprContratoNotificacion_Enviar(int CodEmpresa, string cod_contrato, string mensaje, string usuario)
        {
            var response = new ErrorDto { Code = 0 };

            try
            {
                var info = await ObtenerInfoNotificacion(CodEmpresa, cod_contrato);

                var eConfigResp = _envioCorreoDB.CorreoConfig(CodEmpresa, codNotificaciones);
                var eConfig = (eConfigResp != null && eConfigResp.Code == 0) ? eConfigResp.Result : null;

                if (eConfig == null)
                    return DbHelper.ErrorResponse($"No se pudo obtener la configuración de correo: {eConfigResp?.Description}", ErrorCode);

                info.InfoContrato.divisa = DivisaNombre(info.InfoContrato.divisa);
                var body = BuildNotificacionBody(mensaje, info.InfoContrato);

                if (sendEmail == "Y" && !string.IsNullOrWhiteSpace(info.EmailConfeccionContrato))
                {
                    var emailRequest = new EmailRequest
                    {
                        To = info.EmailConfeccionContrato,
                        From = eConfig.User,
                        Subject = "Confección de Contrato " + cod_contrato,
                        Body = body
                    };

                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, response);
                }

                var upd = DbHelper.ExecuteNonQuery(
                    _portalDB,
                    CodEmpresa,
                    "UPDATE CPR_CONTRATOS SET FECHA_NOTIFICACION = GETDATE() WHERE COD_CONTRATO = @cod_contrato;",
                    new { cod_contrato }
                );

                if (upd.Code != 0)
                    return DbHelper.ErrorResponse(upd.Description ?? ErrorDesconocido, ErrorCode);

                DbHelper.ExecuteNonQuery(
                    _portalDB,
                    CodEmpresa,
                    @"INSERT INTO CPR_CONTRATOS_BITACORA (COD_CONTRATO, MOVIMIENTO, DETALLE, REGISTRO_FECHA, REGISTRO_USUARIO)
                      VALUES (@cod_contrato, @mov, @detalle, GETDATE(), @registro_usuario);",
                    new
                    {
                        cod_contrato,
                        mov = MovNotifica,
                        detalle = "Se envía notificación para Confección del Contrato",
                        registro_usuario = usuario
                    }
                );

                response.Description = "Notificación enviada correctamente";
            }
            catch (Exception ex)
            {
                response.Code = ErrorCode;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene la información necesaria para la notificación de confección de contrato.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private Task<(CprContratosDto InfoContrato, string EmailConfeccionContrato)> ObtenerInfoNotificacion(int codEmpresa, string codContrato)
        {
            var contratoSql = @"
                                SELECT
                                    COD_CONTRATO, DESCRIPCION,
                                    CTA_CONTABLE, FISCAl, NOTAS, MONTO, DIVISA,
                                    (SELECT TOP 1 DESCRIPCION FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = C.COD_PROVEEDOR) AS PROVEEDOR,
                                    (SELECT TOP 1 DESCRIPCION FROM CNTX_CENTRO_COSTOS WHERE COD_CENTRO_COSTO = C.COD_CENTRO_COSTO) AS COD_CENTRO_COSTO,
                                    (SELECT TOP 1 DESCRIPCION
                                    FROM CPR_CATALOGOS
                                    WHERE Tipo_Id = (SELECT TIPO_ID FROM CPR_CATALOGOS_TIPOS WHERE DESCRIPCION = 'Contratos')
                                    AND CATALOGO_ID = C.TIPO_CONTRATO) AS TIPO_CONTRATO,
                                    PORCENTAJE_GARANTIA, MONTO_GARANTIA, DIVISA_GARANTIA
                                FROM CPR_CONTRATOS C
                                WHERE COD_CONTRATO = @cod_contrato;";

            var infoResp = DbHelper.ExecuteSingleQuery<CprContratosDto>(
                _portalDB,
                codEmpresa,
                contratoSql,
                defaultValue: null,
                parameters: new { cod_contrato = codContrato }
            );

            if (infoResp.Code != 0 || infoResp.Result == null)
                throw new InvalidOperationException(infoResp.Description ?? "No se pudo obtener información del contrato.");

            var emailSql = "SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = @cod_parametro;";
            var emailResp = DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                codEmpresa,
                emailSql,
                "",
                new { cod_parametro = nofiticacionConfeccionContrato }
            );

            return Task.FromResult((infoResp.Result, emailResp.Result ?? ""));
        }


        /// <summary>
        ///   Obtiene el nombre completo de la divisa.
        /// </summary>
        /// <param name="divisa"></param>
        /// <returns></returns>
        private static string DivisaNombre(string? divisa) =>
            divisa switch
            {
                "C" => "Colones",
                "D" => "Dólares",
                _ => divisa ?? ""
            };


        /// <summary>
        ///  Construye el cuerpo del correo de notificación.
        /// </summary>
        /// <param name="mensaje"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static string BuildNotificacionBody(string mensaje, CprContratosDto c)
        {
            return $@"
                    <html lang=""es"">
                    <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Solicitud para Confección de Contrato</title>
                    <style>
                        table {{ border-collapse: collapse; width: 100%; }}
                        td {{ border: 1px solid #000; padding: 8px; vertical-align: top; }}
                        td.label {{ font-weight: bold; width: 30%; background-color: #f0f0f0; }}
                    </style>
                    </head>
                    <body>
                    <p>{mensaje}</p>
                    <table>
                        <tr><td class=""label"">No. Contrato</td><td>{c.cod_contrato}</td></tr>
                        <tr><td class=""label"">Descripción</td><td>{c.descripcion}</td></tr>
                        <tr><td class=""label"">Proveedor</td><td>{c.proveedor}</td></tr>
                        <tr><td class=""label"">Tipo Contrato</td><td>{c.tipo_contrato}</td></tr>
                        <tr><td class=""label"">Monto</td><td>{c.monto}</td></tr>
                        <tr><td class=""label"">Divisa</td><td>{c.divisa}</td></tr>
                        <tr><td class=""label"">CTA Contable</td><td>{c.cta_contable}</td></tr>
                        <tr><td class=""label"">Centro Costo</td><td>{c.cod_centro_costo}</td></tr>
                        <tr><td class=""label"">Garantía de Cumplimiento</td><td>{c.porcentaje_garantia} %</td></tr>
                        <tr><td class=""label"">Monto de Garantía</td><td>{c.monto_garantia}</td></tr>
                        <tr><td class=""label"">Divisa Garantía</td><td>{c.divisa_garantia}</td></tr>
                        <tr><td class=""label"">Fiscalizador</td><td>{c.fiscal}</td></tr>
                        <tr><td class=""label"">Notas</td><td>{c.notas}</td></tr>
                    </table>
                    </body>
                    </html>";
        }

        /// <summary>
        /// Obtiene la lista de contratos de los proveedores de una solicitud de compra mediante el cpr_id.
        /// </summary>
        public ErrorDto<List<CprContratosDto>> CprContratosPorSolicitud_Obtener(int CodEmpresa, int cpr_id)
        {
            try
            {
                var sql = @"
                        SELECT
                            C.*,
                            P.DESCRIPCION as PROVEEDOR,
                            V.ESTADO as estado
                        FROM CPR_CONTRATOS C
                        INNER JOIN CXP_PROVEEDORES P ON C.COD_PROVEEDOR = P.COD_PROVEEDOR
                        OUTER APPLY
                        (
                            SELECT TOP 1 ce.ESTADO
                            FROM CPR_CONTRATOS_ESTADOS ce
                            WHERE ce.COD_CONTRATO = C.COD_CONTRATO
                            AND ce.FECHA_INICIO <= GETDATE()
                            ORDER BY ce.FECHA_INICIO DESC
                        ) V
                        WHERE C.COD_PROVEEDOR IN
                        (
                            SELECT PROVEEDOR_CODIGO
                            FROM CPR_SOLICITUD_PROV
                            WHERE CPR_ID = @cpr_id
                        );";

                return DbHelper.ExecuteListQuery<CprContratosDto>(_portalDB, CodEmpresa, sql, new { cpr_id });
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<CprContratosDto>> { Code = ErrorCode, Description = ex.Message, Result = null };
            }
        }
    }
}
