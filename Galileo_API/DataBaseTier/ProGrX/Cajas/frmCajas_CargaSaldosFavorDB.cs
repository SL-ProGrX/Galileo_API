using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasCargaSaldosFavorDB
    {
        private readonly PortalDB _portalDb;

        public FrmCajasCargaSaldosFavorDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los tipos de saldo a favor activos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_Tipos_Obtener(int codEmpresa)
        {
            var query = @"SELECT rtrim(DOC_TIPO) AS item, rtrim(DESCRIPCION) AS descripcion 
                          FROM CAJAS_SALDOS_FAVOR_TIPOS 
                          WHERE ACTIVO = 1 
                          ORDER BY DOC_TIPO";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene las entidades pagadoras activas ordenadas por código o descripción.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="ordenPorDescripcion">Si es true, ordena por descripción; si es false, por código.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_EntidadesPagadoras_Obtener(int codEmpresa, bool ordenPorDescripcion)
        {
            var orderBy = ordenPorDescripcion ? "DESCRIPCION" : "COD_ENTIDAD_PAGO";
            var query = $@"SELECT COD_ENTIDAD_PAGO AS item, DESCRIPCION AS descripcion 
                           FROM SIF_ENTIDADES_PAGO 
                           WHERE ACTIVA = 1 
                           ORDER BY {orderBy}";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene los orígenes de recursos activos ordenados por código o descripción.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="ordenPorDescripcion">Si es true, ordena por descripción; si es false, por código.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_OrigenRecursos_Obtener(int codEmpresa, bool ordenPorDescripcion)
        {
            var orderBy = ordenPorDescripcion ? "DESCRIPCION" : "COD_ORIGEN_RECURSOS";
            var query = $@"SELECT COD_ORIGEN_RECURSOS AS item, DESCRIPCION AS descripcion 
                           FROM SIF_ORIGEN_RECURSOS 
                           WHERE ACTIVA = 1 
                           ORDER BY {orderBy}";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de tipos de liquidación autorizadas por usuario y tipo de documento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros: Usuario, TipoDoc.</param>
        /// <returns></returns>
        public ErrorDto<List<CajasSaldoFavorTipoLiquidacionResult>> CargaSaldosFavor_TipoLiquidacion_Obtener(int codEmpresa, CajasSaldoFavorTipoLiquidacionParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var parameters = new { param.Usuario, param.TipoDoc };
                return conn.Query<CajasSaldoFavorTipoLiquidacionResult>(
                    "spCajas_SaldoFavorTipoLiquidacion",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            });
        }

        /// <summary>
        /// Consulta la vista de saldos a favor con filtros dinámicos.
        /// </summary>
        /// <param name="param">Parámetros de filtro.</param>
        /// <returns></returns>
        public ErrorDto<List<CajasSaldosFavorConsultaResult>> CargaSaldosFavor_Consulta(CajasSaldosFavorConsultaParams param)
        {
            return DbHelper.WithConn(_portalDb, param.CodEmpresa ?? 0, conn =>
            {
                // Seguridad: whereClause solo contiene condiciones con nombres de columna fijos y todos los valores del usuario se pasan como parámetros.
                var (whereClause, parameters) = BuildSaldosFavorWhere(param);
                var sqlBuilder = new StringBuilder("SELECT * FROM vCajas_Saldos_Favor WHERE 1=1");
                sqlBuilder.Append(whereClause);
                sqlBuilder.Append(" ORDER BY REGISTRO_FECHA DESC");
                var sql = sqlBuilder.ToString();
                return conn.Query<CajasSaldosFavorConsultaResult>(sql, parameters).ToList();
            });
        }

        private static (string whereClause, DynamicParameters parameters) BuildSaldosFavorWhere(CajasSaldosFavorConsultaParams param)
        {
            var where = new StringBuilder();
            var parameters = new DynamicParameters();

            if (param.SaldoMayorCero.HasValue)
                where.Append(param.SaldoMayorCero.Value ? " AND Saldo > 0" : " AND Saldo <= 0");

            if (param.FiltrarFechas == true && param.FechaDesde.HasValue && param.FechaHasta.HasValue)
            {
                where.Append(" AND registro_fecha BETWEEN @FechaDesde AND @FechaHasta");
                parameters.Add("FechaDesde", param.FechaDesde.Value.Date);
                parameters.Add("FechaHasta", param.FechaHasta.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
            }

            AddLike(where, parameters, param.Cedula, "Cedula");
            AddLike(where, parameters, param.Nombre, "Nombre", "ISNULL(Nombre,'')");
            AddEquals(where, parameters, param.DocTipo, "Doc_Tipo");
            AddLike(where, parameters, param.DocNumero, "DocNumero", "Doc_Numero");
            AddLike(where, parameters, param.Usuario, "Usuario", "Registro_Usuario");
            AddEquals(where, parameters, param.CodEntidadPago, "COD_ENTIDAD_PAGO");
            AddEquals(where, parameters, param.CodOrigenRecursos, "COD_ORIGEN_RECURSOS");

            if (param.SoloConOrigenRecursos == true)
                where.Append(" AND COD_ORIGEN_RECURSOS IS NOT NULL");

            if (param.MontoInicio.HasValue && param.MontoFin.HasValue)
            {
                where.Append(" AND Monto BETWEEN @MontoInicio AND @MontoFin");
                parameters.Add("MontoInicio", param.MontoInicio.Value);
                parameters.Add("MontoFin", param.MontoFin.Value);
            }

            return (where.ToString(), parameters);
        }

        private static void AddLike(StringBuilder where, DynamicParameters parameters, string? value, string paramName, string? columnName = null)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                columnName ??= paramName;
                where.Append($" AND {columnName} LIKE @{paramName}");
                parameters.Add(paramName, $"%{value}%");
            }
        }

        private static void AddEquals(StringBuilder where, DynamicParameters parameters, string? value, string columnName)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                where.Append($" AND {columnName} = @{columnName}");
                parameters.Add(columnName, value);
            }
        }

        /// <summary>
        /// Obtiene la lista de cuentas bancarias autorizadas para depósitos directos en cajas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros: FormaPago.</param>
        /// <returns></returns>
        public ErrorDto<List<CajasDepositosCuentaBancariaAutResult>> CargaSaldosFavor_CuentasBancariasAut_Obtener(int codEmpresa, CajasDepositosCuentaBancariaAutParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var parameters = new { param.FormaPago };
                return conn.Query<CajasDepositosCuentaBancariaAutResult>(
                    "spCajas_DepositosCuentasBancariasAut",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            });
        }

        /// <summary>
        /// Consulta depósitos no identificados en vTes_Depositos_Tramite_Identifica con filtros dinámicos.
        /// </summary>
        /// <param name="param">Parámetros de filtro.</param>
        /// <returns></returns>
        public ErrorDto<List<CajasDepositosTramiteIdentificaResult>> Cajas_DepositosTramiteIdentifica_Consulta(CajasDepositosTramiteIdentificaParams param)
        {
            return DbHelper.WithConn(_portalDb, param.CodEmpresa ?? 0, conn =>
            {
                var sql = @"SELECT * FROM vTes_Depositos_Tramite_Identifica WHERE ID_REQUERIDA = 1 AND IDENTIFICADO = 0";
                var parameters = new DynamicParameters();

                if (param.FechaDesde.HasValue && param.FechaHasta.HasValue)
                {
                    sql += " AND Fecha BETWEEN @FechaDesde AND @FechaHasta";
                    parameters.Add("FechaDesde", param.FechaDesde.Value.Date);
                    parameters.Add("FechaHasta", param.FechaHasta.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
                }

                if (!string.IsNullOrWhiteSpace(param.Documento))
                {
                    sql += " AND Documento LIKE @Documento";
                    parameters.Add("Documento", $"%{param.Documento}%");
                }

                if (param.IdBanco.HasValue)
                {
                    sql += " AND Id_Banco = @IdBanco";
                    parameters.Add("IdBanco", param.IdBanco.Value);
                }

                if (param.MontoInicio.HasValue && param.MontoFin.HasValue)
                {
                    sql += " AND Monto BETWEEN @MontoInicio AND @MontoFin";
                    parameters.Add("MontoInicio", param.MontoInicio.Value);
                    parameters.Add("MontoFin", param.MontoFin.Value);
                }

                sql += " ORDER BY Fecha DESC";

                return conn.Query<CajasDepositosTramiteIdentificaResult>(sql, parameters).ToList();
            });
        }

        /// <summary>
        /// Obtiene las formas de pago activas para dropdown.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_FormasPago_Obtener(int codEmpresa)
        {
            var query = @"SELECT rtrim(COD_FORMA_PAGO) AS item, rtrim(DESCRIPCION) AS descripcion
                          FROM SIF_FORMAS_PAGO
                          WHERE ACTIVA = 1
                            AND TIPO IN ('B','T')
                          ORDER BY COD_FORMA_PAGO";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene el tipo de una forma de pago según el código.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codFormaPago">Código de la forma de pago.</param>
        /// <returns></returns>
        public ErrorDto<CajasFormasPagoTipoResult?> CargaSaldosFavor_FormaPagoTipo_Obtener(int codEmpresa, string codFormaPago)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var query = @"SELECT Tipo FROM sif_formas_pago WHERE COD_FORMA_PAGO = @codFormaPago";
                return conn.QueryFirstOrDefault<CajasFormasPagoTipoResult>(query, new { codFormaPago });
            });
        }

        /// <summary>
        /// Ejecuta la función fxTes_DP_Cargado para verificar si el depósito está cargado.
        /// </summary>
        /// <param name="param">Parámetros: CodEmpresa, IdBanco, Documento, Monto.</param>
        /// <returns></returns>
        public ErrorDto<CajasDepositosCargadoResult?> Cajas_DepositosCargado_Existe(CajasDepositosCargadoParams param)
        {
            return DbHelper.WithConn(_portalDb, param.CodEmpresa ?? 0, conn =>
            {
                var query = @"SELECT dbo.fxTes_DP_Cargado(@IdBanco, @Documento, '', @Monto) AS Existe";
                return conn.QueryFirstOrDefault<CajasDepositosCargadoResult>(query, new
                {
                    param.IdBanco,
                    param.Documento,
                    param.Monto
                });
            });
        }

        /// <summary>
        /// Inserta un registro en TES_DEPOSITOS_TRAMITE.
        /// </summary>
        /// <param name="param">Parámetros para el registro.</param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_DepositosTramite_Insertar(CajasDepositosTramiteInsertParams param)
        {
            return DbHelper.WithConn(_portalDb, param.CodEmpresa ?? 0, conn =>
            {
                var query = @"
                    INSERT TES_DEPOSITOS_TRAMITE(
                        id_Banco, documento, nsolicitud, fecha, monto, descripcion,
                        registro_fecha, registro_usuario, id_requerida, identificado, cod_cuenta
                    )
                    VALUES(
                        @IdBanco, @Documento, 0, @Fecha, @Monto, @Descripcion,
                        dbo.MyGetdate(), @Usuario, @IdRequerida, 0, @CodCuenta
                    )";
                conn.Execute(query, new
                {
                    param.IdBanco,
                    param.Documento,
                    param.Fecha,
                    param.Monto,
                    param.Descripcion,
                    param.Usuario,
                    param.IdRequerida,
                    param.CodCuenta
                });
                return true;
            });
        }

        /// <summary>
        /// Ejecuta el procedimiento spCajas_Identifica_TES_Depositos.
        /// </summary>
        /// <param name="param">Parámetros para la identificación.</param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_IdentificaTesDepositos(CajasIdentificaTesDepositosParams param)
        {
            return DbHelper.WithConn(_portalDb, param.CodEmpresa ?? 0, conn =>
            {
                var parameters = new
                {
                    param.IdBanco,
                    param.Documento,
                    param.Cedula,
                    param.Nombre,
                    param.Usuario
                };
                conn.Execute("spCajas_Identifica_TES_Depositos", parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });
        }

        /// <summary>
        /// Inserta un registro en TES_DEPOSITOS_TRAMITE_INCONSISTENCIAS.
        /// </summary>
        /// <param name="param">Parámetros para el registro.</param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_DepositosTramiteInconsistencia_Insertar(CajasDepositosTramiteInconsistenciaInsertParams param)
        {
            return DbHelper.WithConn(_portalDb, param.CodEmpresa ?? 0, conn =>
            {
                var query = @"
                    INSERT TES_DEPOSITOS_TRAMITE_INCONSISTENCIAS(
                        id_Banco, documento, fecha, monto, descripcion,
                        registro_fecha, registro_usuario, inconsistencia
                    )
                    VALUES(
                        @IdBanco, @Documento, @Fecha, @Monto, @Descripcion,
                        dbo.MyGetdate(), @Usuario, @Inconsistencia
                    )";
                conn.Execute(query, new
                {
                    param.IdBanco,
                    param.Documento,
                    param.Fecha,
                    param.Monto,
                    param.Descripcion,
                    param.Usuario,
                    param.Inconsistencia
                });
                return true;
            });
        }

        /// <summary>
        /// Ejecuta el procedimiento spCajas_SaldoFavorCarga.
        /// </summary>
        /// <param name="param">Parámetros: CodEmpresa, CodFormaPago,Documento, Cedula, Nombre, Usuario.</param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_SaldoFavorCarga(CajasSaldoFavorCargaParams param)
        {
            return DbHelper.WithConn(_portalDb, param.CodEmpresa ?? 0, conn =>
            {
                var parameters = new
                {
                    param.CodFormaPago,
                    param.Documento,
                    param.Cedula,
                    param.Nombre,
                    param.Usuario
                };
                conn.Execute("spCajas_SaldoFavorCarga", parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });
        }

        /// <summary>
        /// Ejecuta el procedimiento spCajas_Identifica_TES_Depositos con todos los parámetros.
        /// </summary>
        /// <param name="param">Parámetros: CodEmpresa, IdBanco, Documento, Cedula, Nombre, Usuario, CodEntidadPago, CodOrigenRecursos, DepositoId.</param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_IdentificaTesDepositos_Full(CajasIdentificaTesDepositosFullParams param)
        {
            return DbHelper.WithConn(_portalDb, param.CodEmpresa ?? 0, conn =>
            {
                var parameters = new
                {
                    param.IdBanco,
                    param.Documento,
                    param.Cedula,
                    param.Nombre,
                    param.Usuario,
                    param.CodEntidadPago,
                    param.CodOrigenRecursos,
                    param.DepositoId
                };
                conn.Execute("spCajas_Identifica_TES_Depositos", parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });
        }

        /// <summary>
        /// Ejecuta el procedimiento spCajasNotificaDepositos con IdSaldoFavor.
        /// </summary>
        /// <param name="param">Parámetros: CodEmpresa, IdSaldoFavor.</param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_NotificaDepositos(CajasNotificaDepositosParams param)
        {
            return DbHelper.WithConn(_portalDb, param.CodEmpresa ?? 0, conn =>
            {
                var parameters = new
                {
                    Param1 = (object?)null,
                    Param2 = (object?)null,
                    param.IdSaldoFavor
                };
                conn.Execute("spCajasNotificaDepositos", parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });
        }

        /// <summary>
        /// Ejecuta el SP de liquidación de saldo a favor según el método indicado.
        /// </summary>
        /// <param name="param">Parámetros: CodEmpresa, Metodo, Linea, Usuario.</param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_SaldoFavorLiquidacion(CajasSaldoFavorLiquidacionParams param)
        {
            return DbHelper.WithConn(_portalDb, param.CodEmpresa ?? 0, conn =>
            {
                string spName = param.Metodo switch
                {
                    "T" => "spCajas_SaldoFavorLiquidacionTesoreria",
                    "F" => "spCajas_SaldoFavorLiquidacionFondos",
                    "E" => "spCajas_SaldoFavorLiquidacionExclusion",
                    "C" => "spCajas_SaldoFavorLiquidacionRC_Efectivo",
                    _ => throw new ArgumentException("Método no válido")
                };

                var parameters = new
                {
                    param.Linea,
                    param.Usuario
                };

                conn.Execute(spName, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });
        }
    }
}
