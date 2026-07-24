using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosTrasladoDB
    {
        /// <summary>
        /// Obtiene los bancos con pagos pendientes de traslado.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="inicio">Fecha inicial (no aplicada actualmente).</param>
        /// <param name="corte">Fecha de corte (no aplicada actualmente).</param>
        /// <returns>Lista de bancos.</returns>
        public ErrorDto<List<AfiBeneTrasladoOpciones>> CargarBancos_Obtener(int CodCliente, string inicio, string corte)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT B.cod_Banco AS item, TB.descripcion AS descripcion
                                     FROM afi_bene_pago B
                                     INNER JOIN tes_bancos TB ON B.cod_banco = TB.id_banco
                                     INNER JOIN afi_bene_otorga O ON B.cod_beneficio = O.cod_beneficio AND B.consec = O.consec
                                     WHERE B.ESTADO = 'S' AND B.tesoreria IS NULL
                                     GROUP BY B.cod_Banco, TB.descripcion";
                return connection.Query<AfiBeneTrasladoOpciones>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los usuarios que registraron pagos pendientes de traslado.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="inicio">Fecha inicial (no aplicada actualmente).</param>
        /// <param name="corte">Fecha de corte (no aplicada actualmente).</param>
        /// <returns>Lista de usuarios.</returns>
        public ErrorDto<List<AfiBeneTrasladoOpciones>> CargarUsuarios_Obtener(int CodCliente, string inicio, string corte)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT DISTINCT O.Registra_User AS item, O.Registra_User AS descripcion
                                     FROM afi_bene_pago B
                                     INNER JOIN tes_bancos TB ON B.cod_banco = TB.id_banco
                                     INNER JOIN afi_bene_otorga O ON B.cod_beneficio = O.cod_beneficio AND B.consec = O.consec
                                     WHERE B.ESTADO = 'S' AND B.tesoreria IS NULL";
                return connection.Query<AfiBeneTrasladoOpciones>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los beneficios con pagos pendientes de traslado.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de beneficios.</returns>
        public ErrorDto<List<AfiBeneTrasladoOpciones>> CargarBeneficios_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT B.cod_beneficio AS item, TB.descripcion AS descripcion
                                     FROM afi_bene_pago B
                                     INNER JOIN afi_beneficios TB ON B.cod_beneficio = TB.cod_beneficio
                                     INNER JOIN afi_bene_otorga O ON B.cod_beneficio = O.cod_beneficio AND B.consec = O.consec
                                     WHERE B.ESTADO = 'S' AND B.tesoreria IS NULL
                                     GROUP BY B.cod_beneficio, TB.descripcion";
                return connection.Query<AfiBeneTrasladoOpciones>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene las cargas de beneficios pendientes de traslado con filtros y validación de pago.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con los filtros de búsqueda.</param>
        /// <returns>Lista de cargas y total.</returns>
        public ErrorDto<AfiBeneficiosCargasDataLista> BusquedaCargas_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<AfiFiltrosCargas>(filtros) ?? new AfiFiltrosCargas();

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfiBeneficiosCargasDataLista
                {
                    Total = connection.QueryFirstOrDefault<int>(SqlContarCargas)
                };

                var fechas = ObtenerFechasRemesa(connection, filtro.cod_remesa);
                var supervisar = filtro.cod_banco == string.Empty
                    || !filtro.extraFiltros
                    || fxSupervisaBanco(connection, filtro.cod_banco);

                var parametros = new DynamicParameters();
                var sql = ConstruirQueryCargas(CodCliente, connection, filtro, fechas, supervisar, parametros);

                response.Beneficios = connection.Query<AfiBeneficiosCargasData>(sql, parametros).ToList();
                ValidarCargas(CodCliente, response.Beneficios, filtro.registro_usuario);
                return response;
            });
        }

        /// <summary>
        /// Obtiene el rango de fechas de la remesa (o la última si no se indica).
        /// </summary>
        private static AfiBeneficiosRemesasDto ObtenerFechasRemesa(SqlConnection connection, long cod_remesa)
        {
            var sql = cod_remesa > 0
                ? "SELECT fecha_inicio, fecha_corte FROM AFI_BENEFICIOS_REMESAS WHERE cod_remesa = @cod_remesa"
                : "SELECT TOP 1 fecha_inicio, fecha_corte FROM AFI_BENEFICIOS_REMESAS ORDER BY FECHA_INICIO DESC";
            return connection.QueryFirstOrDefault<AfiBeneficiosRemesasDto>(sql, new { cod_remesa }) ?? new AfiBeneficiosRemesasDto();
        }

        /// <summary>
        /// Construye la consulta de cargas con los filtros aplicados de forma parametrizada.
        /// </summary>
        private string ConstruirQueryCargas(int CodCliente, SqlConnection connection, AfiFiltrosCargas filtro,
            AfiBeneficiosRemesasDto fechas, bool supervisar, DynamicParameters parametros)
        {
            var columnaDuplicado = supervisar
                ? "dbo.fxTesSupervisa(B.cedula,S.nombre,B.monto,0,'C') AS 'Duplicado',"
                : string.Empty;

            parametros.Add("fechaInicio", MProGrXAuxiliarDB.validaFechaGlobal(fechas.fecha_inicio, "yyyy-MM-dd") + " 00:00:00");
            parametros.Add("fechaCorte", MProGrXAuxiliarDB.validaFechaGlobal(fechas.fecha_corte, "yyyy-MM-dd") + " 23:59:59");

            var sql = $@"SELECT B.*, S.Nombre, E.Descripcion AS 'EstadoPersona', Ban.Descripcion AS 'BancoDesc',
                                O.id_beneficio, {columnaDuplicado} O.REGISTRA_FECHA,
                                (SELECT DESCRIPCION FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = B.COD_BENEFICIO) AS BENEFICIO_DESC,
                                A.COD_CATEGORIA
                         FROM afi_bene_pago B
                         INNER JOIN socios S ON B.cedula = S.cedula
                         INNER JOIN afi_bene_otorga O ON B.cod_beneficio = O.cod_beneficio AND B.consec = O.consec
                         INNER JOIN Afi_Estados_Persona E ON S.EstadoActual = E.Cod_Estado
                         INNER JOIN Tes_Bancos Ban ON B.cod_Banco = Ban.id_Banco
                         INNER JOIN AFI_BENEFICIOS A ON B.cod_beneficio = A.cod_beneficio
                         WHERE B.cod_remesa IS NULL
                           AND B.registro_fecha BETWEEN @fechaInicio AND @fechaCorte
                           AND B.ESTADO = 'S' AND B.tesoreria IS NULL
                           AND O.ESTADO IN (SELECT COD_ESTADO FROM AFI_BENE_ESTADOS WHERE P_FINALIZA = 1 AND PROCESO = 'A')";

            if (_mBeneficiosDB.fxSIFParametros(CodCliente, "16") == "S")
            {
                sql += " AND O.Analista_Revision = 'S' ";
            }

            sql += ConstruirFiltrosCargas(filtro, parametros);

            var paginado = string.Empty;
            if (filtro.pagina != null)
            {
                paginado = " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY ";
                parametros.Add("offset", filtro.pagina);
                parametros.Add("fetch", filtro.paginacion);
            }

            return sql + " ORDER BY B.cedula " + paginado;
        }

        /// <summary>
        /// Construye las condiciones de filtro adicionales de forma parametrizada.
        /// </summary>
        private static string ConstruirFiltrosCargas(AfiFiltrosCargas filtro, DynamicParameters parametros)
        {
            var sql = string.Empty;

            if (filtro.cod_oficina != "")
            {
                sql += " AND O.cod_oficina = @cod_oficina ";
                parametros.Add("cod_oficina", filtro.cod_oficina);
            }

            if (filtro.extraFiltros)
            {
                if (filtro.cod_banco != "")
                {
                    sql += " AND B.cod_banco = @cod_banco ";
                    parametros.Add("cod_banco", filtro.cod_banco);
                }
                if (filtro.usuario != "")
                {
                    sql += " AND B.Registro_usuario = @usuario ";
                    parametros.Add("usuario", filtro.usuario);
                }
                if (filtro.cod_beneficio != "")
                {
                    sql += " AND B.cod_beneficio = @cod_beneficio ";
                    parametros.Add("cod_beneficio", filtro.cod_beneficio);
                }
            }

            if (!string.IsNullOrEmpty(filtro.vfiltro))
            {
                sql += @" AND (B.cedula LIKE @vfiltro OR B.cta_Bancaria LIKE @vfiltro OR S.Nombre LIKE @vfiltro
                               OR Ban.Descripcion LIKE @vfiltro OR B.cod_Banco LIKE @vfiltro OR O.cod_beneficio LIKE @vfiltro
                               OR O.id_beneficio LIKE @vfiltro OR O.consec LIKE @vfiltro)
                          AND dbo.fxTesSupervisa(B.cedula,S.nombre,B.monto,0,'C') != 1";
                parametros.Add("vfiltro", $"%{filtro.vfiltro}%");
            }

            return sql;
        }

        /// <summary>
        /// Valida cada carga contra las reglas de pago del beneficio y anota los mensajes.
        /// </summary>
        private void ValidarCargas(int CodCliente, List<AfiBeneficiosCargasData> cargas, string registroUsuario)
        {
            foreach (var item in cargas)
            {
                var beneficio = new BeneficioGeneralDatos
                {
                    estado = new AfBeneficioIntegralDropsLista { item = item.Estado },
                    cedula = item.Cedula,
                    monto_aplicado = item.Monto,
                    registra_user = registroUsuario,
                    cod_beneficio = new AfBeneficioIntegralDropsLista { item = item.Cod_Beneficio }
                };

                var respuesta = _mBeneficiosDB.ValidaCargaPagos(CodCliente, beneficio);
                if (!string.IsNullOrWhiteSpace(respuesta.Description))
                {
                    item.Valida_Beneficio = respuesta.Description;
                }
            }
        }

        /// <summary>
        /// Indica si el banco requiere supervisión.
        /// </summary>
        private static bool fxSupervisaBanco(SqlConnection connection, string cod_banco)
        {
            try
            {
                const string sql = "SELECT ISNULL(SUPERVISION, 0) AS SUPERVISION FROM tes_bancos WHERE id_banco = @cod_banco";
                return connection.QueryFirstOrDefault<bool>(sql, new { cod_banco });
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Aplica una remesa a los beneficios seleccionados, validando estado y duplicados.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="carga">JSON con la remesa y los casos.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CargaCarga_Aplicar(int CodCliente, string carga)
        {
            var infoCarga = JsonConvert.DeserializeObject<AfiCargasAplicar>(carga) ?? new AfiCargasAplicar();

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var remesaAbierta = connection.QueryFirstOrDefault<int>(
                    "SELECT COUNT(*) FROM AFI_BENEFICIOS_REMESAS WHERE cod_remesa = @cod_remesa AND estado = 'A'",
                    new { infoCarga.cod_remesa });
                if (remesaAbierta == 0)
                {
                    return DbHelper.ErrorResponse("La Remesa actual; ya se encuentra cerrada...");
                }

                if (infoCarga.casos.Count == 0)
                {
                    return new ErrorDto { Code = 1, Description = "No se han seleccionado casos para procesar" };
                }

                var duplicado = ValidarDuplicados(connection, infoCarga);
                if (duplicado != null)
                {
                    return duplicado;
                }

                var errores = AplicarCasos(connection, infoCarga);
                if (errores == 0)
                {
                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodCliente,
                        Usuario = infoCarga.usuario.ToUpper(),
                        DetalleMovimiento = $"Carga Remesa Traslado a Tesoreria: {infoCarga.cod_remesa} ",
                        Movimiento = "Aplica - WEB",
                        Modulo = 7
                    });
                    return new ErrorDto { Code = 0 };
                }

                return DbHelper.ErrorResponse("Error al actualizar el registro");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Valida que ninguno de los casos tenga un pago duplicado en el rango de la remesa.
        /// </summary>
        private static ErrorDto? ValidarDuplicados(SqlConnection connection, AfiCargasAplicar infoCarga)
        {
            const string sqlDup = @"SELECT COUNT(*) FROM afi_bene_pago
                                    WHERE consec = @consec AND cod_beneficio = @cod_beneficio AND estado = 'E' AND tesoreria IS NOT NULL
                                      AND ENVIO_FECHA BETWEEN
                                          (SELECT FECHA_INICIO FROM AFI_BENEFICIOS_REMESAS WHERE COD_REMESA = @cod_remesa)
                                      AND (SELECT FECHA_CORTE FROM AFI_BENEFICIOS_REMESAS WHERE COD_REMESA = @cod_remesa)";

            foreach (var item in infoCarga.casos)
            {
                var existe = connection.QueryFirstOrDefault<int>(sqlDup, new { item.consec, item.cod_beneficio, infoCarga.cod_remesa });
                if (existe > 0)
                {
                    const string sqlExp = @"SELECT CONCAT(RIGHT(CONCAT('00000', ID_BENEFICIO), 5), TRIM(COD_BENEFICIO), RIGHT(CONCAT('00000', CONSEC), 5)) AS Expediente
                                            FROM AFI_BENE_OTORGA WHERE consec = @consec AND cod_beneficio = @cod_beneficio";
                    var expediente = connection.QueryFirstOrDefault<string>(sqlExp, new { item.consec, item.cod_beneficio });
                    return DbHelper.ErrorResponse("Ya se realizo el pago del beneficio con el expediente: " + expediente);
                }
            }

            return null;
        }

        /// <summary>
        /// Asocia la remesa a los otorgamientos y pagos de cada caso; devuelve la cantidad de errores.
        /// </summary>
        private static int AplicarCasos(SqlConnection connection, AfiCargasAplicar infoCarga)
        {
            var errores = 0;

            foreach (var item in infoCarga.casos)
            {
                var resp = connection.Execute(
                    "UPDATE afi_bene_otorga SET cod_remesa = @cod_remesa WHERE consec = @consec AND cod_beneficio = @cod_beneficio",
                    new { infoCarga.cod_remesa, item.consec, item.cod_beneficio });

                connection.Execute(
                    "UPDATE afi_bene_pago SET cod_remesa = @cod_remesa WHERE consec = @consec AND cod_beneficio = @cod_beneficio",
                    new { infoCarga.cod_remesa, item.consec, item.cod_beneficio });

                if (item.justificacion != null)
                {
                    connection.Execute(
                        "UPDATE afi_bene_pago SET justificacion = @justificacion WHERE consec = @consec AND cod_beneficio = @cod_beneficio",
                        new { item.justificacion, item.consec, item.cod_beneficio });
                }

                if (resp <= 0)
                {
                    errores++;
                }
            }

            return errores;
        }

        /// <summary>
        /// Cierra una remesa de traslado (estado 'C').
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <param name="usuario">Usuario que cierra.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CargasCarga_Cerrar(int CodCliente, string cod_remesa, string usuario)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var abierta = connection.QueryFirstOrDefault<int>(
                    "SELECT COUNT(*) FROM AFI_BENEFICIOS_REMESAS WHERE cod_remesa = @cod_remesa AND estado = 'A'", new { cod_remesa });
                if (abierta == 0)
                {
                    return DbHelper.ErrorResponse("La Remesa actual; ya se encuentra cerrada...");
                }

                connection.Execute("UPDATE AFI_BENEFICIOS_REMESAS SET estado = 'C' WHERE cod_remesa = @cod_remesa", new { cod_remesa });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodCliente,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = $"Cierra Remesa Traslado a Tesoreria: {cod_remesa} ",
                    Movimiento = "Aplica - WEB",
                    Modulo = 7
                });

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las remesas abiertas para cargas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de remesas abiertas.</returns>
        public ErrorDto<List<AfiBeneficiosRemesasDto>> AfiCargasRemesas_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT *, CONCAT(COD_REMESA, USUARIO, FECHA, FECHA_INICIO, FECHA_CORTE) AS DESCRIPCION
                                     FROM AFI_BENEFICIOS_REMESAS WHERE estado = 'A' ORDER BY fecha DESC";
                return connection.Query<AfiBeneficiosRemesasDto>(sql).ToList();
            });
        }

        private const string SqlContarCargas = @"SELECT COUNT(B.CEDULA)
            FROM afi_bene_pago B
            INNER JOIN socios S ON B.cedula = S.cedula
            INNER JOIN afi_bene_otorga O ON B.cod_beneficio = O.cod_beneficio AND B.consec = O.consec
            INNER JOIN Afi_Estados_Persona E ON S.EstadoActual = E.Cod_Estado
            INNER JOIN Tes_Bancos Ban ON B.cod_Banco = Ban.id_Banco
            INNER JOIN AFI_BENEFICIOS A ON B.cod_beneficio = A.cod_beneficio
            WHERE O.cod_remesa IS NULL
              AND O.ESTADO IN (SELECT COD_ESTADO FROM AFI_BENE_ESTADOS WHERE P_FINALIZA = 1 AND PROCESO = 'A')";
    }
}
