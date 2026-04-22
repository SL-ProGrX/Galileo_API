using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.Models.ProGrX_Comites;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdRemesasComitesDB
    {
        private readonly PortalDB _portalDb;

        public FrmAfCdRemesasComitesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las últimas 50 remesas de tesorería.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de remesas.</returns>
        public ErrorDto<List<AfCdRemesaTesDto>> AfCdRemesasTes_Lista(int codEmpresa)
        {
            var sql = @"
                SELECT TOP 50
                    COD_REMESA AS Cod_Remesa,
                    FECHA AS Fecha,
                    USUARIO AS Usuario,
                    FECHA_INICIO AS Fecha_Inicio,
                    FECHA_CORTE AS Fecha_Corte,
                    NOTAS AS Notas,
                    ESTADO AS Estado
                FROM afi_cd_remesas_tes
                ORDER BY FECHA DESC";

            return DbHelper.ExecuteListQuery<AfCdRemesaTesDto>(
                _portalDb,
                codEmpresa,
                sql,
                null
            );
        }

        /// <summary>
        /// Inserta o actualiza una remesa de tesorería.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="dto">Datos de la remesa.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> AfCdRemesasTes_Guardar(int codEmpresa, AfCdRemesaTesSaveDto dto)
        {
            if (dto.Cod_Remesa == 0)
            {
                // Insertar: calcular nuevo código
                var nuevoCodigo = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                    conn.QueryFirstOrDefault<int>(
                        "SELECT COALESCE(MAX(cod_remesa),0) + 1 FROM afi_cd_remesas_tes"
                    )
                );
                dto.Cod_Remesa = nuevoCodigo.Result;

                var sql = @"
                    INSERT INTO afi_cd_remesas_tes
                        (cod_remesa, usuario, fecha, estado, fecha_inicio, fecha_corte, notas)
                    VALUES
                        (@Cod_Remesa, @Usuario, GETDATE(), 'A', @Fecha_Inicio, @Fecha_Corte, @Notas)";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    conn.Execute(sql, dto);
                    return true;
                });
            }
            else
            {
                var sql = @"
                    UPDATE afi_cd_remesas_tes
                    SET usuario = @Usuario,
                        fecha_inicio = @Fecha_Inicio,
                        fecha_corte = @Fecha_Corte,
                        notas = @Notas
                    WHERE cod_remesa = @Cod_Remesa";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    conn.Execute(sql, dto);
                    return true;
                });
            }

            return new ErrorDto<bool> { Result = true };
        }

        /// <summary>
        /// Elimina una remesa de tesorería.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codRemesa">Código de la remesa a eliminar.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> AfCdRemesasTes_Eliminar(int codEmpresa, int codRemesa)
        {
            var sql = @"DELETE FROM afi_cd_remesas_tes WHERE cod_remesa = @Cod_Remesa";
            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, new { Cod_Remesa = codRemesa });
                return true;
            });
            return new ErrorDto<bool> { Result = true };
        }

        /// <summary>
        /// Obtiene las remesas activas o pendientes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de remesas activas o pendientes.</returns>
        public ErrorDto<List<AfCdRemesaTesDto>> AfCdRemesasTes_ActivasPendientes(int codEmpresa)
        {
            var sql = @"
                SELECT
                    COD_REMESA AS Cod_Remesa,
                    FECHA AS Fecha,
                    USUARIO AS Usuario,
                    FECHA_INICIO AS Fecha_Inicio,
                    FECHA_CORTE AS Fecha_Corte,
                    NOTAS AS Notas,
                    ESTADO AS Estado
                FROM afi_cd_remesas_tes
                WHERE estado IN ('A', 'P')
                ORDER BY fecha DESC";

            return DbHelper.ExecuteListQuery<AfCdRemesaTesDto>(
                _portalDb,
                codEmpresa,
                sql,
                null
            );
        }

        /// <summary>
        /// Obtiene las fechas de inicio y corte de una remesa.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codRemesa">Código de la remesa seleccionada.</param>
        /// <returns>Fechas de la remesa.</returns>
        public ErrorDto<AfCdRemesaTesFechasDto> AfCdRemesasTes_Fechas(int codEmpresa, int codRemesa)
        {
            var sql = @"
                SELECT fecha_inicio AS Fecha_Inicio, fecha_corte AS Fecha_Corte
                FROM afi_cd_remesas_tes
                WHERE cod_remesa = @Cod_Remesa";

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<AfCdRemesaTesFechasDto>(sql, new { Cod_Remesa = codRemesa })
            );

            return new ErrorDto<AfCdRemesaTesFechasDto>
            {
                Result = result.Result,
                Code = result.Result != null ? 0 : -2,
                Description = result.Result != null ? "Ok" : "No se encontró la remesa."
            };
        }

        /// <summary>
        /// Obtiene los bancos asociados a cuentas en un rango de fechas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="fechaInicio">Fecha de inicio.</param>
        /// <param name="fechaCorte">Fecha de corte.</param>
        /// <returns>Lista de bancos.</returns>
        public ErrorDto<List<AfCdBancoDto>> AfCdRemesasTes_BancosPorFechas(int codEmpresa, DateTime fechaInicio, DateTime fechaCorte)
        {
            var sql = @"
                SELECT B.id_banco AS Id_Banco, B.descripcion AS Descripcion
                FROM afi_cd_cuentas C
                INNER JOIN bancos B ON C.id_banco = B.id_banco
                WHERE C.registro_fecha BETWEEN @FechaInicio AND @FechaCorte
                GROUP BY B.id_banco, B.descripcion";

            // El between debe ser desde 00:00:00 hasta 23:59:59
            var fechaIni = fechaInicio.Date;
            var fechaFin = fechaCorte.Date.AddDays(1).AddSeconds(-1);

            return DbHelper.ExecuteListQuery<AfCdBancoDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { FechaInicio = fechaIni, FechaCorte = fechaFin }
            );
        }

        /// <summary>
        /// Obtiene las operaciones de cuentas por banco y fechas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="idBanco">ID del banco seleccionado.</param>
        /// <param name="fechaInicio">Fecha de inicio.</param>
        /// <param name="fechaCorte">Fecha de corte.</param>
        /// <returns>Lista de operaciones.</returns>
        public ErrorDto<List<AfCdCuentaOperacionDto>> AfCdRemesasTes_OperacionesPorBanco(
            int codEmpresa, int idBanco, DateTime fechaInicio, DateTime fechaCorte)
        {
            var sql = @"
                SELECT C.noperacion AS NOperacion,
                       C.cod_comite AS Cod_Comite,
                       P.descripcion AS Descripcion,
                       C.cedula AS Cedula,
                       S.nombre AS Asociado,
                       C.cuenta AS Cuenta,
                       C.registro_usuario AS Registro_Usuario,
                       C.tipo AS Tipo
                FROM uprogramatica P
                INNER JOIN afi_cd_cuentas C ON P.codigo = C.cod_comite
                INNER JOIN Socios S ON C.cedula = S.cedula
                WHERE C.id_banco = @Id_Banco
                  AND C.registro_fecha BETWEEN @FechaInicio AND @FechaCorte
                  AND C.estado = 'A'";

            var fechaIni = fechaInicio.Date;
            var fechaFin = fechaCorte.Date.AddDays(1).AddSeconds(-1);

            return DbHelper.ExecuteListQuery<AfCdCuentaOperacionDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Id_Banco = idBanco, FechaInicio = fechaIni, FechaCorte = fechaFin }
            );
        }

        /// <summary>
        /// Obtiene las actividades asociadas a una operación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="noperacion">Número de operación.</param>
        /// <returns>Lista de actividades de la operación.</returns>
        public ErrorDto<List<AfCdCuentaActividadDto>> AfCdRemesasTes_ActividadesPorOperacion(
            int codEmpresa, int noperacion)
        {
            var sql = @"
                SELECT *
                FROM afi_cd_cuentas_actividades
                WHERE noperacion = @NOperacion";

            return DbHelper.ExecuteListQuery<AfCdCuentaActividadDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { NOperacion = noperacion }
            );
        }

        /// <summary>
        /// Obtiene el estado de una remesa según el valor recibido.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codRemesa">Código de la remesa.</param>
        /// <param name="estado">Estado a buscar.</param>
        /// <returns>Estado de la remesa.</returns>
        public ErrorDto<AfCdRemesaEstadoDto> AfCdRemesasTes_ObtenerEstado(int codEmpresa, int codRemesa, string estado)
        {
            var sql = @"
                SELECT estado AS Estado
                FROM afi_cd_remesas_tes
                WHERE cod_remesa = @Cod_Remesa
                  AND estado = @Estado";

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<AfCdRemesaEstadoDto>(sql, new { Cod_Remesa = codRemesa, Estado = estado })
            );

            return new ErrorDto<AfCdRemesaEstadoDto>
            {
                Result = result.Result,
                Code = result.Result != null ? 0 : -2,
                Description = result.Result != null ? "Ok" : "No se encontró la remesa con ese estado."
            };
        }

        /// <summary>
        /// Obtiene la remesa asociada a una cuenta y banco.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codRemesa">Código de la remesa.</param>
        /// <param name="idBanco">ID del banco.</param>
        /// <returns>Código de la remesa.</returns>
        public ErrorDto<AfCdCuentaRemesaDto> AfCdRemesasTes_ObtenerRemesaPorBanco(int codEmpresa, int codRemesa, int idBanco)
        {
            var sql = @"
                SELECT cod_remesa AS Cod_Remesa
                FROM afi_cd_cuentas
                WHERE cod_remesa = @Cod_Remesa
                  AND id_banco = @Id_Banco";

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<AfCdCuentaRemesaDto>(sql, new { Cod_Remesa = codRemesa, Id_Banco = idBanco })
            );

            return new ErrorDto<AfCdCuentaRemesaDto>
            {
                Result = result.Result,
                Code = result.Result != null ? 0 : -2,
                Description = result.Result != null ? "Ok" : "No se encontró la remesa para ese banco."
            };
        }

        /// <summary>
        /// Ejecuta el SP spAFI_CD_Cuenta_Remesa para actualizar el estado de la cuenta/remesa.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del SP.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> AfCdRemesasTes_CuentaRemesaSp(int codEmpresa, AfCdCuentaRemesaSpParams param)
        {
            var sql = "spAFI_CD_Cuenta_Remesa";
            var parameters = new
            {
                Operacion = param.NOperacion,
                RemesaId = param.Cod_Remesa,
                param.Estado,
                param.Usuario,
                Notas = param.Notas ?? $"Cargado en Remesa: {param.Cod_Remesa}",
                param.TesoreriaId
            };

            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return new ErrorDto<bool> { Result = true };
        }

        /// <summary>
        /// Actualiza el estado de una remesa.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codRemesa">Código de la remesa.</param>
        /// <param name="estado">Nuevo estado.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> AfCdRemesasTes_ActualizarEstado(int codEmpresa, int codRemesa, string estado)
        {
            var sql = @"
                UPDATE afi_cd_remesas_tes
                SET estado = @Estado
                WHERE cod_remesa = @Cod_Remesa";

            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, new { Cod_Remesa = codRemesa, Estado = estado });
                return true;
            });

            return new ErrorDto<bool> { Result = true };
        }

        /// <summary>
        /// Actualiza el estado de todas las cuentas asociadas a una remesa.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codRemesa">Código de la remesa.</param>
        /// <param name="estado">Nuevo estado.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> AfCdCuentas_ActualizarEstadoPorRemesa(int codEmpresa, int codRemesa, string estado)
        {
            var sql = @"
                UPDATE afi_cd_cuentas
                SET estado = @Estado
                WHERE cod_remesa = @Cod_Remesa";

            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, new { Cod_Remesa = codRemesa, Estado = estado });
                return true;
            });

            return new ErrorDto<bool> { Result = true };
        }

        /// <summary>
        /// Obtiene las remesas de tesorería filtradas por estado y cantidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Parámetros de filtro (cantidad, estado).</param>
        /// <returns>Lista de remesas.</returns>
        public ErrorDto<List<AfCdRemesaTesDto>> AfCdRemesasTes_Filtradas(int codEmpresa, AfCdRemesaTesFiltroParams filtro)
        {
            var sql = $@"
                SELECT TOP (@Cantidad)
                    COD_REMESA AS Cod_Remesa,
                    FECHA AS Fecha,
                    USUARIO AS Usuario,
                    FECHA_INICIO AS Fecha_Inicio,
                    FECHA_CORTE AS Fecha_Corte,
                    NOTAS AS Notas,
                    ESTADO AS Estado
                FROM afi_cd_remesas_tes
                {{WHERE_CLAUSE}}
                ORDER BY FECHA DESC";

            string where = "";
            if (!string.IsNullOrEmpty(filtro.Estado))
                where = "WHERE ESTADO = @Estado";

            sql = sql.Replace("{WHERE_CLAUSE}", where);

            return DbHelper.ExecuteListQuery<AfCdRemesaTesDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { filtro.Cantidad, filtro.Estado }
            );
        }

        /// <summary>
        /// Obtiene el detalle de remesas por comité y rango de fechas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de filtro (comite, fechaInicio, fechaCorte).</param>
        /// <returns>Lista de detalles de remesas por comité.</returns>
        public ErrorDto<List<AfCdRemesaComiteDetalleDto>> AfCdRemesasComiteDetalle_Lista(int codEmpresa, AfCdRemesaComiteDetalleParams param)
        {
            var sql = @"
                SELECT C.cod_Remesa,
                       C.noperacion,
                       D.nsolicitud,
                       C.tesoreria_fecha,
                       RTRIM(P.cod_Comite + ' - ' + P.Descripcion) AS Comite,
                       D.monto,
                       P.cod_Comite
                FROM AFI_CD_COMITES P
                INNER JOIN afi_cd_cuentas C
                        ON P.cod_comite = C.cod_comite
                INNER JOIN afi_cd_remesas_tes_detalle D
                        ON C.cod_remesa = D.cod_remesa
                       AND C.noperacion = D.noperacion
                WHERE C.estado IN ('T','L')
                  AND C.cod_comite LIKE @Comite
                  AND C.tesoreria_fecha BETWEEN @FechaInicio AND @FechaCorte
            ";

            // El LIKE debe ser '%valor%', así que lo preparamos aquí
            var parametros = new
            {
                Comite = $"%{param.Comite}%",
                FechaInicio = param.FechaInicio.Date,
                FechaCorte = param.FechaCorte.Date.AddDays(1).AddSeconds(-1)
            };

            return DbHelper.ExecuteListQuery<AfCdRemesaComiteDetalleDto>(
                _portalDb,
                codEmpresa,
                sql,
                parametros
            );
        }

        /// <summary>
        /// Obtiene el resumen de remesas cerradas (estado 'C') con monto y casos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de resúmenes de remesas.</returns>
        public ErrorDto<List<AfCdRemesaResumenDto>> AfCdRemesasTes_ResumenCerradas(int codEmpresa)
        {
            var sql = @"
                SELECT R.cod_remesa,
                       R.fecha,
                       R.fecha_inicio,
                       R.fecha_corte,
                       R.usuario,
                       SUM(C.monto) AS Monto,
                       COUNT(*) AS Casos
                FROM afi_cd_remesas_tes R
                INNER JOIN AFI_CD_CUENTAS C
                        ON R.cod_remesa = C.cod_remesa
                WHERE R.estado = 'C'
                GROUP BY R.cod_remesa,
                         R.fecha,
                         R.fecha_inicio,
                         R.fecha_corte,
                         R.usuario";
            return DbHelper.ExecuteListQuery<AfCdRemesaResumenDto>(
                _portalDb,
                codEmpresa,
                sql,
                null
            );
        }

        /// <summary>
        /// Obtiene el detalle de cuentas de una remesa seleccionada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codRemesa">Código de la remesa seleccionada.</param>
        /// <returns>Lista de detalles de la remesa.</returns>
        public ErrorDto<List<AfCdRemesaDetalleDto>> AfCdRemesasTes_DetallePorRemesa(int codEmpresa, int codRemesa)
        {
            var sql = @"
                SELECT C.noperacion,
                       C.cedula,
                       S.nombre,
                       C.cuenta,
                       C.monto,
                       C.id_banco,
                       B.descripcion AS Banco,
                       RTRIM(Com.Cod_Comite) + ' - ' + Com.Descripcion AS Comite
                FROM afi_cd_cuentas C
                INNER JOIN Tes_bancos B
                        ON C.id_banco = B.id_banco
                INNER JOIN socios S
                        ON C.cedula = S.cedula
                INNER JOIN Afi_Cd_Comites Com
                        ON C.cod_Comite = Com.cod_comite
                WHERE C.cod_remesa = @Cod_Remesa";
            return DbHelper.ExecuteListQuery<AfCdRemesaDetalleDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Cod_Remesa = codRemesa }
            );
        }

        /// <summary>
        /// Obtiene el token activo más reciente.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Token activo.</returns>
        public ErrorDto<TesTokenDto> TesTokens_ObtenerActivo(int codEmpresa)
        {
            var sql = @"
                SELECT TOP 1 id_token AS Id_Token
                FROM tes_tokens
                WHERE estado = 'A'
                ORDER BY registro_fecha";

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<TesTokenDto>(sql)
            );

            return new ErrorDto<TesTokenDto>
            {
                Result = result.Result,
                Code = result.Result != null ? 0 : -2,
                Description = result.Result != null ? "Ok" : "No se encontró token activo."
            };
        }

        /// <summary>
        /// Obtiene el consecutivo para un token basado en la fecha.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="fecha">Fecha base para el token.</param>
        /// <returns>Consecutivo.</returns>
        public ErrorDto<TesTokenConsecDto> TesTokens_ObtenerConsec(int codEmpresa, string fecha)
        {
            var sql = @"
                SELECT ISNULL(COUNT(id_token),0) + 1 AS Consec
                FROM tes_tokens
                WHERE id_token LIKE (@Fecha + '.%')";

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<TesTokenConsecDto>(sql, new { Fecha = fecha })
            );

            return new ErrorDto<TesTokenConsecDto>
            {
                Result = result.Result,
                Code = 0,
                Description = "Ok"
            };
        }

        /// <summary>
        /// Inserta un nuevo token.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="dto">Datos del token.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> TesTokens_Insertar(int codEmpresa, TesTokenInsertDto dto)
        {
            var sql = @"
                INSERT INTO tes_tokens(id_token, registro_fecha, registro_usuario, estado)
                VALUES(@Id_Token, dbo.MyGetdate(), @Usuario, 'A')";

            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, dto);
                return true;
            });

            return new ErrorDto<bool> { Result = true };
        }

        /// <summary>
        /// Ejecuta el SP spAFI_CD_Remesa_Desembolso para procesar el desembolso de una remesa.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del SP (remesa, token, usuario).</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> AfCdRemesasTes_Desembolso(int codEmpresa, AfCdRemesaDesembolsoParams param)
        {
            var sql = "spAFI_CD_Remesa_Desembolso";
            var parameters = new
            {
                Remesa = param.Remesa,
                Token = param.Token,
                Usuario = param.Usuario
            };

            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return new ErrorDto<bool> { Result = true };
        }
    }
}
