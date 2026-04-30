using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Hipotecario;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivRemesasTesoreriaDB
    {        
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 3; // Modulo de Hipoteca
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmVivRemesasTesoreriaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mSecurityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene las 50 remesas de tesorería más recientes, incluyendo los campos calculados Casos y Monto.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <returns>ErrorDto con la lista de remesas y sus datos asociados.</returns>
        public ErrorDto<List<RemesasTesoreriaObtenerDto>> RemesasTesoreria_Obtener(int codEmpresa)
        {
            var sql = @"select TOP 50
                T.*, isnull(D.Casos, 0) as Casos, isnull(D.Monto, 0) as Monto
                from viviendaRemesasTesoreria T
                left join vCrd_Hipotecario_Remesa_Tes_Rsm D on T.Remesa = D.Remesa
                order by T.RegistroFecha desc";
            return DbHelper.ExecuteListQuery<RemesasTesoreriaObtenerDto>(_portalDb, codEmpresa, sql, null);
        }


        /// <summary>
        /// Inserta una nueva remesa de tesorería y retorna el nuevo id Remesa.
        /// </summary>
        public ErrorDto<int> RemesasTesoreria_Insertar(int codEmpresa, RemesaTesoreriaUpsertDto dto)
        {
            var sqlUltimo = "select isnull(max(Remesa), 0) + 1 as Ultimo from viviendaRemesasTesoreria";
            int nuevoId = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QuerySingle<int>(sqlUltimo)
            ).Result;

            var sqlInsert = @"insert viviendaRemesasTesoreria
                            (
                                Remesa, RegistroUsuario, RegistroFecha, Estado, FechaInicio, FechaCorte, notas
                            )
                            values
                            (
                                @Remesa, @Usuario, dbo.MyGetdate(), 'A', @FechaInicio, @FechaCorte, @Notas
                            )";
            var parametros = new {
                Remesa = nuevoId,
                dto.Usuario,
                dto.FechaInicio,
                dto.FechaCorte,
                dto.Notas
            };
            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Execute(sqlInsert, parametros)
            );
            return new ErrorDto<int> { Result = nuevoId, Code = 0, Description = "Ok" };
        }

        /// <summary>
        /// Actualiza una remesa de tesorería existente.
        /// </summary>
        public ErrorDto<bool> RemesasTesoreria_Actualizar(int codEmpresa, RemesaTesoreriaUpsertDto dto)
        {
            var sql = @"update viviendaRemesasTesoreria
                    set RegistroUsuario = @Usuario, FechaInicio = @FechaInicio, FechaCorte = @FechaCorte, notas = @Notas
                    where Remesa = @Remesa";
            var parametros = new {
                dto.Remesa,
                dto.Usuario,
                dto.FechaInicio,
                dto.FechaCorte,
                dto.Notas
            };
            int rows = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Execute(sql, parametros)
            ).Result;
            return new ErrorDto<bool> { Result = rows > 0, Code = rows > 0 ? 0 : -2, Description = rows > 0 ? "Ok" : "No se actualizó" };
        }

        /// <summary>
        /// Elimina los detalles de una remesa y limpia los campos de Tesorería en ViviendaDesembolsos.
        /// </summary>
        public ErrorDto<bool> RemesasTesoreriaDetalle_Eliminar(int codEmpresa, int remesa)
        {
            var sqlDelete = "DELETE FROM viviendaRemesasTesoreria_detalle WHERE Remesa = @Remesa";
            var sqlUpdate = @"UPDATE ViviendaDesembolsos
                        SET TesoreriaRemesa = NULL, TesoreriaSolicitud = NULL, TesoreriaFecha = NULL, TesoreriaUsuario = NULL
                        WHERE TesoreriaRemesa = @Remesa";
            var parameters = new { Remesa = remesa };
            int rowsDelete = DbHelper.WithConn(_portalDb, codEmpresa, conn => conn.Execute(sqlDelete, parameters)).Result;
            int rowsUpdate = DbHelper.WithConn(_portalDb, codEmpresa, conn => conn.Execute(sqlUpdate, parameters)).Result;
            bool ok = rowsDelete > 0 || rowsUpdate > 0;
            return new ErrorDto<bool> { Result = ok, Code = ok ? 0 : -2, Description = ok ? "Ok" : "No se eliminó ningún registro" };
        }

        /// <summary>
        /// Obtiene las remesas de tesorería filtradas por tipo (abierta/traslado).
        /// </summary>
        public ErrorDto<List<RemesasTesoreriaObtenerDto>> RemesasTesoreria_Filtrar(int codEmpresa, string tipo)
        {
            var sql = new System.Text.StringBuilder(@"select * from viviendaRemesasTesoreria where 1 = 1");
            if (!string.IsNullOrEmpty(tipo))
            {
                if (tipo.ToLower() == "abierta")
                    sql.Append(" AND Estado IN ('A', 'X')");
                else if (tipo.ToLower() == "traslado")
                    sql.Append(" AND Estado = 'C'");
            }
            sql.Append(" order by RegistroFecha desc");
            return DbHelper.ExecuteListQuery<RemesasTesoreriaObtenerDto>(_portalDb, codEmpresa, sql.ToString(), null);
        }

        /// <summary>
        /// Obtiene los desembolsos disponibles para una remesa seleccionada, usando el rango de fechas de la remesa.
        /// </summary>
        public ErrorDto<List<RemesaTesoreriaDesembolsoDisponibleDto>> RemesasTesoreria_DesembolsosDisponibles(int codEmpresa, int remesaSeleccionada)
        {
            // Obtener fechas de la remesa seleccionada
            var fechas = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<(DateTime FechaInicio, DateTime FechaCorte)>(
                    "select FechaInicio, FechaCorte from viviendaRemesasTesoreria where Remesa = @Remesa",
                    new { Remesa = remesaSeleccionada })
            ).Result;
            if (fechas.FechaInicio == default || fechas.FechaCorte == default)
                return new ErrorDto<List<RemesaTesoreriaDesembolsoDisponibleDto>> { Result = new List<RemesaTesoreriaDesembolsoDisponibleDto>(), Code = -2, Description = "Fechas no encontradas" };

            var sql = @"select
                    D.CodigoDesembolso,
                    D.NumeroOperacion,
                    D.Beneficiario,
                    D.Monto,
                    D.RegistroFecha,
                    D.RegistroUsuario,
                    S.cedula,
                    S.nombre,
                    R.codigo,
                    D.TES_SUPERVISION_FECHA,
                    dbo.fxTesSupervisa(D.Identificacion, D.Beneficiario, D.Monto, 0, 'V') as Duplicado
                from ViviendaDesembolsos D
                inner join Reg_Creditos R on D.numeroOperacion = R.id_solicitud
                inner join Socios S on R.cedula = S.cedula
                where D.TesoreriaRemesa is null
                  and D.RegistroFecha between @FechaInicio and @FechaCorte";
            var parametros = new {
                FechaInicio = fechas.FechaInicio.Date,
                FechaCorte = fechas.FechaCorte.Date.AddDays(1).AddSeconds(-1)
            };
            return DbHelper.ExecuteListQuery<RemesaTesoreriaDesembolsoDisponibleDto>(_portalDb, codEmpresa, sql, parametros);
        }

        /// <summary>
        /// Valida si la remesa sigue abierta (estado A o X).
        /// </summary>
        public ErrorDto<RemesaTesoreriaExisteDto> RemesasTesoreria_ValidarAbierta(int codEmpresa, int remesaSeleccionada)
        {
            var sql = @"select count(*) as Existe from viviendaRemesasTesoreria where remesa = @Remesa and estado in ('A', 'X')";
            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<RemesaTesoreriaExisteDto>(sql, new { Remesa = remesaSeleccionada })
            );
            return new ErrorDto<RemesaTesoreriaExisteDto> { Result = result.Result, Code = 0, Description = "Ok" };
        }

        /// <summary>
        /// Valida si la remesa está cerrada (estado C).
        /// </summary>
        public ErrorDto<RemesaTesoreriaExisteDto> RemesasTesoreria_ValidarCerrada(int codEmpresa, int remesaSeleccionada)
        {
            var sql = @"select count(*) as Existe from ViviendaRemesasTesoreria where Remesa = @Remesa and estado in ('C')";
            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<RemesaTesoreriaExisteDto>(sql, new { Remesa = remesaSeleccionada })
            );
            return new ErrorDto<RemesaTesoreriaExisteDto> { Result = result.Result, Code = 0, Description = "Ok" };
        }

        /// <summary>
        /// Carga un desembolso a la remesa y actualiza el estado de la remesa a 'X' si fue exitoso.
        /// </summary>
        public ErrorDto<bool> RemesasTesoreria_CargarDesembolso(int codEmpresa, int remesaSeleccionada, int codigoDesembolso)
        {
            var sqlUpdateDesembolso = @"update viviendaDesembolsos set TesoreriaRemesa = @Remesa where CodigoDesembolso = @CodigoDesembolso";
            var parametros = new { Remesa = remesaSeleccionada, CodigoDesembolso = codigoDesembolso };
            int rows = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Execute(sqlUpdateDesembolso, parametros)
            ).Result;
            if (rows > 0)
            {
                var sqlUpdateRemesa = @"update viviendaRemesasTesoreria set estado = 'X' where remesa = @Remesa";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                    conn.Execute(sqlUpdateRemesa, new { Remesa = remesaSeleccionada })
                );
            }
            return new ErrorDto<bool> { Result = rows > 0, Code = rows > 0 ? 0 : -2, Description = rows > 0 ? "Ok" : "No se cargó el desembolso" };
        }

        /// <summary>
        /// Actualiza la remesa a estado 'P' (proceso) y registra en bitácora.
        /// </summary>
        public ErrorDto<bool> RemesasTesoreria_ActualizarProceso(int codEmpresa, int remesaSeleccionada, string usuario, int idDesem)
        {
            var sql = "update ViviendaRemesasTesoreria set estado = 'P' where Remesa = @Remesa";
            var parametros = new { Remesa = remesaSeleccionada };
            int rows = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Execute(sql, parametros)
            ).Result;
            if (rows > 0)
            {
                _mSecurityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Desembolso de Vivienda a Tesoreria Remesa:{remesaSeleccionada} IdDesem:{idDesem}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });
            }
            return new ErrorDto<bool> { Result = rows > 0, Code = rows > 0 ? 0 : -2, Description = rows > 0 ? "Ok" : "No se actualizó la remesa a proceso" };
        }

        /// <summary>
        /// Cierra una remesa (estado = 'C') y registra en bitácora.
        /// </summary>
        public ErrorDto<bool> RemesasTesoreria_Cerrar(int codEmpresa, int remesaSeleccionada, string usuario)
        {
            var sql = "update viviendaRemesasTesoreria set estado = 'C' where Remesa = @Remesa";
            var parametros = new { Remesa = remesaSeleccionada };
            int rows = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Execute(sql, parametros)
            ).Result;
            if (rows > 0)
            {                
                _mSecurityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Cierra Remesa Crd Hipotecario Traslado a Tesoreria : {remesaSeleccionada}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });

            }
            return new ErrorDto<bool> { Result = rows > 0, Code = rows > 0 ? 0 : -2, Description = rows > 0 ? "Ok" : "No se cerró la remesa" };
        }

        /// <summary>
        /// Obtiene los desembolsos asignados a una remesa seleccionada.
        /// </summary>
        public ErrorDto<List<RemesaTesoreriaDesembolsoAsignadoDto>> RemesasTesoreria_DesembolsosAsignados(int codEmpresa, int remesaSeleccionada)
        {
            var sql = @"select
                    D.CodigoDesembolso,
                    D.NumeroOperacion,
                    D.Beneficiario,
                    D.Monto,
                    D.RegistroFecha,
                    D.RegistroUsuario,
                    S.cedula,
                    S.nombre,
                    R.codigo
                from ViviendaDesembolsos D
                inner join Reg_Creditos R on D.numeroOperacion = R.id_solicitud
                inner join Socios S on R.cedula = S.cedula
                where D.TesoreriaFecha is null
                  and D.TesoreriaRemesa = @RemesaSeleccionada";
            var parametros = new { RemesaSeleccionada = remesaSeleccionada };
            return DbHelper.ExecuteListQuery<RemesaTesoreriaDesembolsoAsignadoDto>(_portalDb, codEmpresa, sql, parametros);
        }
    }
}
