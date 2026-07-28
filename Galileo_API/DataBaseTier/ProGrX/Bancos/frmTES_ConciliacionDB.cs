using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesConciliacionDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCntLinkDB mCntLinkDB;
        private readonly string vDateFormat = "yyyy-MM-dd HH:mm:ss";
        private readonly string vMensaje = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";

        public FrmTesConciliacionDB(IConfiguration? config)
        {
            mCntLinkDB = new MCntLinkDB(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las cuentas bancarias para conciliación de un usuario específico en una empresa dada.
        /// </summary>
        public ErrorDto<List<TesConciliacionCuentaData>> TES_ConciliacionBancosLst_Obtener(int CodEmpresa, string usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spTes_Cuenta_Bancaria_Acceso_General @Usuario, 'ASI'";
                return conn.Query<TesConciliacionCuentaData>(query, new { Usuario = usuario }).ToList();
            });
        }

        #region Historial

        /// <summary>
        /// Consulta el historial de conciliación bancaria para una empresa y banco específicos, filtrado por usuario.
        /// </summary>
        public ErrorDto<List<TesConciliacionHistorico>> TES_ConciliacionHistorico_Obtener(int CodEmpresa, int id_banco, string usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spTes_Concilia_Periodo_Consulta @BancoId, @Usuario";
                return conn.Query<TesConciliacionHistorico>(query, new { BancoId = id_banco, Usuario = usuario }).ToList();
            });
        }

        #endregion

        #region Resumen

        /// <summary>
        /// Consulta los periodos de conciliación bancaria para una empresa, banco, año y mes específicos.
        /// </summary>
        public ErrorDto<TesConciliaPeriodo> TES_ConciliacionPeriodo_Consulta(int CodEmpresa, string usuario, int id_banco, int pAnio, int mes)
        {
            return Exec(CodEmpresa, conn =>
            {
                // Valido si existe
                var query = @"Select COUNT('X') from vTES_CONCILIA_PERIODO
                              where id_Banco = @banco
                                and Anio = @anio
                                and Mes = @mes";

                var exists = conn.Query<int>(query, new
                {
                    banco = id_banco,
                    anio = pAnio,
                    mes
                }).FirstOrDefault();

                if (exists == 0)
                {
                    TES_ConciliacionResumen_Guardar(CodEmpresa, new TesConciliaFiltros
                    {
                        periodoEstado = "A",
                        banco = id_banco,
                        ahno = pAnio,
                        mes = mes,
                        saldo = 0,
                        usuario = usuario,
                        notas = "",
                        saldoActual = 0
                    });
                }

                query = @"select *
                          from vTES_CONCILIA_PERIODO
                          where id_Banco = @banco
                            and Anio = @anio
                            and Mes = @mes";

                var result = conn.Query<TesConciliaPeriodo>(query, new
                {
                    banco = id_banco,
                    anio = pAnio,
                    mes
                }).FirstOrDefault();

                if (result == null)
                {
                    return DbHelper.CreateErrorResponse<TesConciliaPeriodo>("Error al obtener Periodo para la conciliación Bancaria");
                }

                return DbHelper.CreateOkResponse(result);
            },
            "Error al obtener las cuentas bancarias para conciliación.");
        }

        /// <summary>
        /// Actualiza el saldo de conciliación bancaria para un periodo específico, validando si el periodo está cerrado.
        /// </summary>
        public ErrorDto TES_ConciliacionSaldo_Actualiza(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    const string query = @"exec spTes_Concilia_Periodo_Actualiza_Saldo_Cta @BancoId, @Anio,@Mes,@CtaSaldo,@Usuario";

                    conn.Execute(query, new
                    {
                        BancoId = filtro.banco,
                        Anio = filtro.ahno,
                        Mes = filtro.mes,
                        CtaSaldo = filtro.saldo,
                        Usuario = filtro.usuario
                    });

                    spTesConciliaPeriodoActualiza(CodEmpresa, filtro.banco, filtro.ahno, filtro.mes, filtro.usuario);

                    return DbHelper.CreateOkResponse();
                }),
                "Error al actualizar el saldo de conciliación bancaria.");
        }

        /// <summary>
        /// Guarda nota y saldo actual de conciliación bancaria para un periodo específico.
        /// </summary>
        public ErrorDto TES_ConciliacionResumen_Guardar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    const string query = @"exec spTes_Concilia_Periodo_Add @BancoId,@Anio,@Mes,'A',@Notas,@Usuario,@Libros_Saldo,@Cta_Saldo";

                    conn.Execute(query, new
                    {
                        BancoId = filtro.banco,
                        Anio = filtro.ahno,
                        Mes = filtro.mes,
                        Notas = filtro.notas ?? string.Empty,
                        Usuario = filtro.usuario,
                        Libros_Saldo = filtro.saldo,
                        Cta_Saldo = filtro.saldoActual
                    });

                    return DbHelper.CreateOkResponse();
                }),
                "Error al guardar el resumen de conciliación bancaria.");
        }

        /// <summary>
        /// Carga un archivo de conciliación bancaria, procesando cada fila y actualizando el saldo de conciliación para un periodo específico.
        /// </summary>
        public ErrorDto TES_ConciliacionResumenArchivo_Cargar(int CodEmpresa, TesConciliaFiltros filtro, List<TesConciliacioExcelDto> file)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    foreach (var row in file)
                    {
                        if (row.importe < 0)
                        {
                            row.tipo = "D";
                        }

                        string fechaExcel = MProGrXAuxiliarDB.validaFechaGlobal(row.fecha, vDateFormat) ?? string.Empty;

                        const string query = @"exec spTes_Concilia_Banco_Mov @BancoId,@Fecha,@Documento,@TipoMov,@Importe,@Descripcion ,0,@Usuario";

                        conn.Execute(query, new
                        {
                            BancoId = filtro.banco,
                            Fecha = fechaExcel,
                            Documento = row.documento,
                            TipoMov = row.tipo,
                            Importe = row.importe,
                            Descripcion = row.descripcion,
                            Usuario = filtro.usuario
                        });
                    }

                    TES_Conciliacion_Actualizar(CodEmpresa, filtro);

                    return DbHelper.CreateOkResponse();
                }),
                "Error al cargar el archivo de conciliación bancaria.");
        }

        /// <summary>
        /// Cierra un periodo de conciliación bancaria para una empresa, banco, año y mes específicos.
        /// </summary>
        public ErrorDto TES_ConciliacionResumenPeriodo_Cerrar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    const string query = @"exec spTes_Concilia_Periodo_Cierra @BancoId,@Anio,@Mes,@Usuario";

                    conn.Execute(query, new
                    {
                        BancoId = filtro.banco,
                        Anio = filtro.ahno,
                        Mes = filtro.mes,
                        Usuario = filtro.usuario
                    }, commandTimeout: 300);

                    return DbHelper.CreateOkResponse();
                }),
                "Error al cerrar el periodo de conciliación bancaria.");
        }

        /// <summary>
        /// Realiza la conciliación de movimientos entre bancos o libros, dependiendo del tipo especificado.
        /// </summary>
        public ErrorDto TES_ConciliacionResumen_Concilia(int CodEmpresa, int tipo, TesConciliaFiltros filtro)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    string query;
                    switch (tipo)
                    {
                        case 0:
                            query = @"exec spTes_Concilia_Bancos_EntreSi @BancosId,@Anio,@Mes,@Usuario";
                            break;
                        case 1:
                            query = @"exec spTes_Concilia_Libros_EntreSi @BancosId,@Anio,@Mes,@Usuario";
                            break;
                        default:
                            return DbHelper.ErrorResponse("Tipo de operación no válido.");
                    }

                    conn.Execute(query, new
                    {
                        BancosId = filtro.banco,
                        Anio = filtro.ahno,
                        Mes = filtro.mes,
                        Usuario = filtro.usuario
                    }, commandTimeout: 300);

                    return DbHelper.CreateOkResponse();
                }),
                "Error al conciliar los movimientos.");
        }

        /// <summary>
        /// Actualiza el saldo de conciliación bancaria de forma automática para un periodo específico, validando si el periodo está cerrado.
        /// </summary>
        public ErrorDto TES_Conciliacion_Actualizar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    const string query = @"exec spTes_Concilia_Automatica @BancoId,@Anio,@Mes,@Usuario";

                    conn.Execute(query, new
                    {
                        BancoId = filtro.banco,
                        Anio = filtro.ahno,
                        Mes = filtro.mes,
                        Usuario = filtro.usuario
                    }, commandTimeout: 900);

                    spTesConciliaPeriodoActualiza(CodEmpresa, filtro.banco, filtro.ahno, filtro.mes, filtro.usuario);

                    return DbHelper.CreateOkResponse();
                }),
                "Error al actualizar la conciliación bancaria.");
        }

        /// <summary>
        /// Inicializa un periodo de conciliación bancaria para una empresa, banco, año y mes específicos.
        /// </summary>
        public ErrorDto TES_Conciliacion_Inicializa(int CodEmpresa, TesConciliaFiltros filtro)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    const string query = @"exec spTes_Concilia_Periodo_Inicializa @BancoId,@Anio,@Mes,@Usuario";

                    conn.Execute(query, new
                    {
                        BancoId = filtro.banco,
                        Anio = filtro.ahno,
                        Mes = filtro.mes,
                        Usuario = filtro.usuario
                    }, commandTimeout: 300);

                    spTesConciliaPeriodoActualiza(CodEmpresa, filtro.banco, filtro.ahno, filtro.mes, filtro.usuario);

                    return DbHelper.CreateOkResponse();
                }),
                "Error al inicializar el periodo de conciliación bancaria.");
        }

        #endregion

        #region Resultados

        /// <summary>
        /// Obtiene los resultados de conciliación bancaria para un periodo específico, filtrando por varios criterios.
        /// </summary>
        public ErrorDto<List<TesConciliaResultados>> TES_ConciliacionResultados_Obtener(int CodEmpresa, TesConciliaResultadoFiltros filtros)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spTes_Concilia_Periodo_Resultados @BancoId, @Anio ,@Mes,@Ubicacion,@Tipo,@Estado";

                string estado = filtros.estadoCasos.Substring(0, 1);

                return conn.Query<TesConciliaResultados>(query, new
                {
                    BancoId = filtros.id_banco,
                    Anio = filtros.ahno,
                    Mes = filtros.mes,
                    Ubicacion = filtros.ubicacion,
                    Tipo = filtros.tipoDoc,
                    Estado = estado
                }).ToList();
            });
        }

        /// <summary>
        /// Registra automáticamente los resultados de conciliación bancaria para un periodo específico.
        /// </summary>
        public ErrorDto TES_ConciliacionResultados_Autoregistro(int CodEmpresa, TesConciliacionResultosFiltro filtro, List<TesConciliaResultados> datos)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    bool vCuenta = mCntLinkDB.fxgCntCuentaValida(CodEmpresa, filtro.ar_cuenta);
                    if (!vCuenta)
                    {
                        return DbHelper.ErrorResponse("La cuenta contable indicada para el auto-registro no es válida!");
                    }

                    const string query = @"exec spTes_Concilia_Auto_Registro @BancoId, @Anio , @Mes , @Id, @Cuenta , @Usuario , @Conta_Registro";

                    foreach (var item in datos)
                    {
                        conn.Execute(query, new
                        {
                            BancoId = filtro.banco,
                            Anio = filtro.ahno,
                            Mes = filtro.mes,
                            Id = item.id,
                            Cuenta = filtro.ar_cuenta,
                            Usuario = filtro.usuario,
                            Conta_Registro = filtro.chkAutoReg ? 1 : 0
                        });
                    }

                    spTesConciliaPeriodoActualiza(CodEmpresa, filtro.banco, filtro.ahno, filtro.mes, filtro.usuario);

                    return DbHelper.CreateOkResponse();
                }),
                "Error al realizar el auto-registro de resultados.");
        }

        /// <summary>
        /// Marca resultados como pendientes.
        /// </summary>
        public ErrorDto TES_ConciliacionResultados_Pendiente(int CodEmpresa, TesConciliacionResultosFiltro filtro, List<TesConciliaResultados> datos)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    const string query = @"exec spTes_Concilia_Pendiente @BancoId, @Anio , @Mes , @Id , @Ubicacion , @Usuario";

                    foreach (var item in datos)
                    {
                        conn.Execute(query, new
                        {
                            BancoId = filtro.banco,
                            Anio = filtro.ahno,
                            Mes = filtro.mes,
                            Id = item.id,
                            Ubicacion = filtro.ubicacion,
                            Usuario = filtro.usuario
                        });
                    }

                    return DbHelper.CreateOkResponse();
                }),
                "Error al marcar pendientes de conciliación.");
        }

        #endregion

        #region Conciliación

        /// <summary>
        /// Obtiene los datos de conciliación asignados para un periodo específico.
        /// </summary>
        public ErrorDto<List<TesConciliaAsigna>> TES_ConciliacionAsigna_Obtener(int CodEmpresa, TesConciliaAsignaFiltros filtros)
        {
            return Exec(CodEmpresa, conn =>
            {
                string fechaInicio = MProGrXAuxiliarDB.validaFechaGlobal(filtros.dtpConciliaInicio, vDateFormat) ?? string.Empty;
                DateTime original = DateTime.ParseExact(fechaInicio, vDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                DateTime inicioDia = original.Date;
                string resultadoInicio = inicioDia.ToString(vDateFormat);

                string fechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(filtros.dtpConciliaCorte, vDateFormat) ?? string.Empty;
                DateTime originalCorte = DateTime.ParseExact(fechaCorte, vDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                DateTime afechaFin = originalCorte.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                string resultadoCorte = afechaFin.ToString(vDateFormat);

                const string query = @"exec spTes_Concilia_Periodo_Disponibles 
                                            @BancoId,
                                            @Anio,
                                            @Mes,
                                            @Ubicacion,
                                            @Tipo,
                                            @Importe,
                                            @Filtro,
                                            @Pendientes,
                                            @FiltroMnts,
                                            @FiltroFechas,
                                            @fInicio,
                                            @fCorte";

                var response = conn.Query<TesConciliaAsigna>(query, new
                {
                    BancoId = filtros.banco,
                    Anio = filtros.ahno,
                    Mes = filtros.mes,
                    Ubicacion = filtros.ubicacion,
                    Tipo = filtros.tipoMov,
                    Importe = filtros.movImporte,
                    Filtro = filtros.movFiltro,
                    Pendientes = filtros.chkConciliaPendientes ? 1 : 0,
                    FiltroMnts = filtros.chkConciliaFiltroMontos ? 1 : 0,
                    FiltroFechas = filtros.chkConciliaFiltroFechas ? 1 : 0,
                    fInicio = resultadoInicio,
                    fCorte = resultadoCorte
                }).ToList();

                return DbHelper.CreateOkResponse(response);
            }, "Error al obtener datos para asignación de conciliación.");
        }

        /// <summary>
        /// Aplica la conciliación bancaria para un periodo específico.
        /// </summary>
        public ErrorDto TES_Conciliacion_Aplicar(int CodEmpresa, TesConciliacionFiltro filtro, List<TesConciliaAsigna> datos)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    datos.ForEach(item =>
                    {
                        string pId_Bancos;
                        string pId_Libros;

                        if (filtro.ubicacion == "B")
                        {
                            pId_Bancos = filtro.mov_id;
                            pId_Libros = item.id.ToString();
                        }
                        else
                        {
                            pId_Libros = filtro.mov_id;
                            pId_Bancos = item.id.ToString();
                        }

                        var storedProcedure = filtro.movFiltro == "T"
                            ? "spTes_Concilia_Aplicacion"
                            : "spTes_Concilia_Aplicacion_Lote";

                        conn.Execute(storedProcedure, new
                        {
                            BancoId = filtro.banco,
                            Anio = filtro.ahno,
                            Mes = filtro.mes,
                            Bancos_Id = pId_Bancos,
                            Libros_Id = pId_Libros,
                            Usuario = filtro.usuario
                        }, commandType: CommandType.StoredProcedure);
                    });

                    return DbHelper.CreateOkResponse();
                }),
                "Error al aplicar la conciliación bancaria.");
        }

        /// <summary>
        /// Detalle de Transacciones Vinculadas
        /// </summary>
        public ErrorDto<List<TesConciliacionDetallesData>> TES_ConciliacionDetalle_Obtener(int CodEmpresa, TesConciliacionFiltro filtro)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spTes_Concilia_Periodo_Resultados_Caso_Detalle  
                                            @BancoId,
                                            @Anio,
                                            @Mes,
                                            @Ubicacion,
                                            @Id";

                return conn.Query<TesConciliacionDetallesData>(query, new
                {
                    BancoId = filtro.banco,
                    Anio = filtro.ahno,
                    Mes = filtro.mes,
                    Ubicacion = filtro.ubicacion,
                    Id = filtro.caso
                }).ToList();
            });
        }

        /// <summary>
        /// Obtiene los detalles de un lote de conciliación bancaria para un periodo específico.
        /// </summary>
        public ErrorDto<List<TesConciliacionDetallesLoteData>> TES_ConciliacionDetalleLote_Obtener(int CodEmpresa, TesConciliacionFiltro filtro)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spTes_Concilia_Periodo_Resultados_Caso_Lote 
                                            @BancoId,
                                            @Anio,
                                            @Mes,
                                            @Ubicacion,
                                            @Id";

                return conn.Query<TesConciliacionDetallesLoteData>(query, new
                {
                    BancoId = filtro.banco,
                    Anio = filtro.ahno,
                    Mes = filtro.mes,
                    Ubicacion = filtro.ubicacion,
                    Id = filtro.caso
                }).ToList();
            });
        }

        /// <summary>
        /// Revierte una conciliación bancaria.
        /// </summary>
        public ErrorDto TES_Conciliacion_Reversa(int CodEmpresa, TesConciliacionFiltro filtro, List<TesConciliacionDetallesData> datos)
        {
            return Exec(CodEmpresa, conn =>
                GuardPeriodoAbierto(filtro.periodoEstado, () =>
                {
                    const string query = @"exec spTes_Concilia_Reversa @BancoId, @Anio , @Mes , @Bancos_Id, @Libros_Id, @Usuario";

                    datos.ForEach(item =>
                    {
                        string pId_Bancos;
                        string pId_Libros;

                        if (filtro.ubicacion == "B")
                        {
                            pId_Bancos = filtro.mov_id;
                            pId_Libros = item.id.ToString();
                        }
                        else
                        {
                            pId_Libros = filtro.mov_id;
                            pId_Bancos = item.id.ToString();
                        }

                        conn.Execute(query, new
                        {
                            BancoId = filtro.banco,
                            Anio = filtro.ahno,
                            Mes = filtro.mes,
                            Bancos_Id = pId_Bancos,
                            Libros_Id = pId_Libros,
                            Usuario = filtro.usuario
                        });
                    });

                    spTesConciliaPeriodoActualiza(CodEmpresa, filtro.banco, filtro.ahno, filtro.mes, filtro.usuario);

                    return DbHelper.CreateOkResponse();
                }),
                "Error al revertir la conciliación bancaria.");
        }

        #endregion

        private void spTesConciliaPeriodoActualiza(int CodEmpresa, int banco, int ahno, int mes, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            const string query = @"exec spTes_Concilia_Periodo_Actualiza @BancoId, @Anio , @Mes , @Usuario";

            conn.Execute(query, new
            {
                BancoId = banco,
                Anio = ahno,
                Mes = mes,
                Usuario = usuario
            }, commandTimeout: 900);
        }

        private ErrorDto RunIfPeriodoAbierto(string? periodoEstado)
        {
            if (!string.IsNullOrEmpty(periodoEstado) && periodoEstado.StartsWith('C'))
            {
                return DbHelper.ErrorResponse(vMensaje);
            }
            return DbHelper.CreateOkResponse();
        }

        // ✅ Helper único para NO repetir el if(...) en 10 métodos (reduce duplicidad de Sonar)
        private ErrorDto GuardPeriodoAbierto(string? periodoEstado, Func<ErrorDto> action)
        {
            var check = RunIfPeriodoAbierto(periodoEstado);
            if (check.Code == -1) return check; // ya trae vMensaje
            return action();
        }

        // Tus helpers: quedan intactos
        private ErrorDto Exec(int codEmpresa, Func<IDbConnection, ErrorDto> action, string errorMsg)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                return action(conn);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(string.IsNullOrWhiteSpace(ex.Message) ? errorMsg : ex.Message);
            }
        }

        private ErrorDto<T> Exec<T>(int codEmpresa, Func<IDbConnection, ErrorDto<T>> action, string errorMsg)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                return action(conn);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<T>(string.IsNullOrWhiteSpace(ex.Message) ? errorMsg : ex.Message);
            }
        }
    }
}
