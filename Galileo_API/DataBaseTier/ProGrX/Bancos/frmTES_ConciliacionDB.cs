using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
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
            mCntLinkDB = new MCntLinkDB(config!);
            _portalDB = new PortalDB(config!);
        }

        /// <summary>
        /// Obtiene las cuentas bancarias para conciliación de un usuario específico en una empresa dada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<TesConciliacionCuentaData>> TES_ConciliacionBancosLst_Obtener(int CodEmpresa, string usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"exec spTes_Cuenta_Bancaria_Acceso_General @usuario, 'ASI'";

                return conn.Query<TesConciliacionCuentaData>(query, new { usuario = usuario }).ToList();
            });
        }

        #region Historial

        /// <summary>
        /// Consulta el historial de conciliación bancaria para una empresa y banco específicos, filtrado por usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<TesConciliacionHistorico>> TES_ConciliacionHistorico_Obtener(int CodEmpresa, int id_banco, string usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"exec spTes_Concilia_Periodo_Consulta @id_banco, @usuario ";

                return conn.Query<TesConciliacionHistorico>(query, new { id_banco, usuario }).ToList();
            });
        }


        #endregion

        #region Resumen

        /// <summary>
        /// Consulta los periodos de conciliación bancaria para una empresa, banco, año y mes específicos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="pAnio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public ErrorDto<TesConciliaPeriodo> TES_ConciliacionPeriodo_Consulta(int CodEmpresa, string usuario, int id_banco, int pAnio, int mes)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                //Valido si existe
                var query = $@"Select COUNT('X') from vTES_CONCILIA_PERIODO
                                    where id_Banco = @banco
                                      and Anio = @anio
                                      and Mes = @mes ";
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

                query = $@"select *
                                    from vTES_CONCILIA_PERIODO
                                    where id_Banco = @banco
                                      and Anio = @anio
                                      and Mes = @mes ";
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
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<TesConciliaPeriodo>("Error al obtener las cuentas bancarias para conciliación.");
            }
        }

        /// <summary>
        /// Actualiza el saldo de conciliación bancaria para un periodo específico, validando si el periodo está cerrado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionSaldo_Actualiza(int CodEmpresa, TesConciliaFiltros filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }

                var query = $@"exec spTes_Concilia_Periodo_Actualiza_Saldo_Cta @banco, @ahno,@mes,@saldo,@usuario";
                conn.Execute(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    saldo = filtro.saldo,
                    usuario = filtro.usuario
                });

                query = $@"exec spTes_Concilia_Periodo_Actualiza @banco, @ahno,@mes,@usuario";
                conn.Execute(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    usuario = filtro.usuario
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse("Error al actualizar el saldo de conciliación bancaria.");
            }
        }

        /// <summary>
        /// Guarda nota y saldo actual de conciliación bancaria para un periodo específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionResumen_Guardar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }

                var query = $@"exec spTes_Concilia_Periodo_Add @banco,@ahno,@mes,'A',@notas,@usuarios,@saldos,@saldoActual";
                conn.Execute(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    notas = filtro.notas ?? string.Empty,
                    usuarios = filtro.usuario,
                    saldos = filtro.saldo,
                    saldoActual = filtro.saldoActual
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Carga un archivo de conciliación bancaria, procesando cada fila y actualizando el saldo de conciliación para un periodo específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionResumenArchivo_Cargar(int CodEmpresa, TesConciliaFiltros filtro, List<TesConciliacioExcelDto> file)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }

                foreach (var row in file)
                {
                    if (row.importe < 0)
                    {
                        row.tipo = "D";
                    }

                    string fechaExcel = MProGrXAuxiliarDB.validaFechaGlobal(row.fecha, vDateFormat) ?? string.Empty;

                    var query = $@"exec spTes_Concilia_Banco_Mov @banco,@fechaExcel,@ndocumento,@tipo,@importe,@descripcion,0,@usuario";
                    conn.Execute(query, new
                    {
                        banco = filtro.banco,
                        fechaExcel = fechaExcel,
                        ndocumento = row.documento,
                        tipo = row.tipo,
                        importe = row.importe,
                        descripcion = row.descripcion,
                        usuario = filtro.usuario
                    });

                }

                TES_Conciliacion_Actualizar(CodEmpresa, filtro);

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message.ToString());
            }

           
        }

        /// <summary>
        /// Cierra un periodo de conciliación bancaria para una empresa, banco, año y mes específicos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionResumenPeriodo_Cerrar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }

                var query = $@"exec spTes_Concilia_Periodo_Cierra @banco,@ahno,@mes,@usuario";
                conn.Execute(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    usuario = filtro.usuario
                }, commandTimeout: 300);

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Realiza la conciliación de movimientos entre bancos o libros, dependiendo del tipo especificado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionResumen_Concilia(int CodEmpresa, int tipo, TesConciliaFiltros filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }

                var query = "";
                switch (tipo)
                {
                    case 0: // Concilia Movimientos (E/S) Banco
                        query = $@"exec spTes_Concilia_Bancos_EntreSi @banco,@ahno,@mes,@usuario";
                        break;
                    case 1: // Concilia Movimientos (E/S) Libros
                        query = $@"exec spTes_Concilia_Libros_EntreSi @banco,@ahno,@mes,@usuario";
                        break;
                    default: // Cierra  
                        return DbHelper.ErrorResponse("Tipo de operación no válido.");
                }

                conn.Execute(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    usuario = filtro.usuario
                }, commandTimeout: 300);

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Actualiza el saldo de conciliación bancaria de forma automática para un periodo específico, validando si el periodo está cerrado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_Conciliacion_Actualizar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }

                var query = $@"exec spTes_Concilia_Automatica @banco,@ahno,@mes,@usuario";
                conn.Execute(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    usuario = filtro.usuario
                }, commandTimeout: 900);

                query = $@"exec spTes_Concilia_Periodo_Actualiza @banco,@ahno,@mes,@usuario";
                conn.Execute(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    usuario = filtro.usuario
                }, commandTimeout: 900);

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Inicializa un periodo de conciliación bancaria para una empresa, banco, año y mes específicos, permitiendo la actualización del saldo inicial.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_Conciliacion_Inicializa(int CodEmpresa, TesConciliaFiltros filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }

                var query = $@"exec spTes_Concilia_Periodo_Inicializa @banco,@ahno,@mes,@usuario";
                conn.Execute(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    usuario = filtro.usuario
                }, commandTimeout: 300);

                query = $@"exec spTes_Concilia_Periodo_Actualiza @banco,@ahno,@mes,@usuario";
                conn.Execute(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    usuario = filtro.usuario
                }, commandTimeout: 300);

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message.ToString());   
            }
        }

        #endregion

        #region Resultados

        /// <summary>
        /// Obtiene los resultados de conciliación bancaria para un periodo específico, filtrando por varios criterios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TesConciliaResultados>> TES_ConciliacionResultados_Obtener(int CodEmpresa, TesConciliaResultadoFiltros filtros)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"exec spTes_Concilia_Periodo_Resultados @banco, @ahno ,@mes,@ubicacion,@tipoDoc,@estadoCasos";

                return conn.Query<TesConciliaResultados>(query, new
                {
                    banco = filtros.id_banco,
                    ahno = filtros.ahno,
                    mes = filtros.mes,
                    ubicacion = filtros.ubicacion,
                    tipoDoc = filtros.tipoDoc,
                    estadoCasos = filtros.estadoCasos
                }).ToList();
            });
        }

        /// <summary>
        /// Registra automáticamente los resultados de conciliación bancaria para un periodo específico, actualizando el saldo y el estado del periodo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionResultados_Autoregistro(int CodEmpresa, TesConciliacionResultosFiltro filtro, List<TesConciliaResultados> datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }

                bool vCuenta = mCntLinkDB.fxgCntCuentaValida(CodEmpresa, filtro.ar_cuenta!);
                if (!vCuenta)
                {
                    return DbHelper.ErrorResponse("La cuenta contable indicada para el auto-registro no es válida!");
                }

                var query = "";
                foreach (var item in datos)
                {
                    query = $@"exec spTes_Concilia_Auto_Registro @bancos, @ahno , @mes , @id, @cuenta , @usuario , @chkAutoReg ";
                    conn.Execute(query, new
                    {
                        bancos = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        id = item.id,
                        cuenta = filtro.ar_cuenta,
                        usuario = filtro.usuario,
                        chkAutoReg = filtro.chkAutoReg ? 1 : 0
                    });
                }

                //Actualiza Resumen
                query = $@"exec spTes_Concilia_Periodo_Actualiza @bancos, @ahno , @mes , @usuario ";
                conn.Execute(query, new
                {
                    bancos = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    usuario = filtro.usuario
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionResultados_Pendiente(int CodEmpresa, TesConciliacionResultosFiltro filtro, List<TesConciliaResultados> datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }

                var query = "";
                foreach (var item in datos)
                {
                    query = $@"exec spTes_Concilia_Pendiente @bancos, @ahno , @mes , @id , @ubicacion , @usuario ";
                    conn.Execute(query, new
                    {
                        bancos = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        id = item.id,
                        ubicacion = filtro.ubicacion,
                        usuario = filtro.usuario
                    });
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message.ToString());
            }
        }


        #endregion

        #region Conciliación

        /// <summary>
        /// Obtiene los datos de conciliación asignados para un periodo específico, filtrando por varios criterios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TesConciliaAsigna>> TES_ConciliacionAsigna_Obtener(int CodEmpresa, TesConciliaAsignaFiltros filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string fechaInicio = MProGrXAuxiliarDB.validaFechaGlobal(filtros.dtpConciliaInicio, vDateFormat) ?? string.Empty;
                DateTime original = DateTime.ParseExact(fechaInicio, vDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                DateTime inicioDia = original.Date;

                string resultadoInicio = inicioDia.ToString(vDateFormat);


                string fechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(filtros.dtpConciliaCorte, vDateFormat) ?? string.Empty;
                DateTime originalCorte = DateTime.ParseExact(
                    fechaCorte,
                    vDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None);
                DateTime afechaFin = originalCorte.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                string resultadoCorte = afechaFin.ToString(vDateFormat);


                var query = $@" exec spTes_Concilia_Periodo_Disponibles 
                                            @banco,
                                            @ahno,
                                            @mes,
                                            @ubicacion,
                                            @tipoMov,
                                            @movImporte,
                                            @movFiltro,
                                            @chkConciliaPendientes,
                                            @chkConciliaFiltroMontos,
                                            @chkConciliaFiltroFechas,
                                            @dtpConciliaInicio,
                                            @dtpConciliaCorte";
                var response = conn.Query<TesConciliaAsigna>(query, new
                {
                    banco = filtros.banco,
                    ahno = filtros.ahno,
                    mes = filtros.mes,
                    ubicacion = filtros.ubicacion,
                    tipoMov = filtros.tipoMov,
                    movImporte = filtros.movImporte,
                    movFiltro = filtros.movFiltro,
                    chkConciliaPendientes = filtros.chkConciliaPendientes ? 1 : 0,
                    chkConciliaFiltroMontos = filtros.chkConciliaFiltroMontos ? 1 : 0,
                    chkConciliaFiltroFechas = filtros.chkConciliaFiltroFechas ? 1 : 0,
                    dtpConciliaInicio = resultadoInicio,
                    dtpConciliaCorte = resultadoCorte
                }).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesConciliaAsigna>>(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Aplica la conciliación bancaria para un periodo específico, asignando los datos de conciliación a los movimientos correspondientes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto TES_Conciliacion_Aplicar(int CodEmpresa, TesConciliacionFiltro filtro, List<TesConciliaAsigna> datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }

                var query = "";
                datos.ForEach(item => {
                    string pId_Bancos = "";
                    string pId_Libros = "";
                    if (filtro.ubicacion == "B")
                    {
                        pId_Bancos = filtro.mov_id!;
                        pId_Libros = item.id.ToString();
                    }
                    else
                    {
                        pId_Libros = filtro.mov_id!;
                        pId_Bancos = item.id.ToString();
                    }

                    if (filtro.movFiltro == "T")
                    {
                        query = $@"exec spTes_Concilia_Aplicacion  @bancos, @ahno , @mes , @id_bancos, @id_libros, @usuario ";
                    }
                    else
                    {
                        query = $@"exec spTes_Concilia_Aplicacion_Lote @bancos, @ahno , @mes , @id_bancos, @id_libros, @usuario ";
                    }

                    conn.Execute(query, new
                    {
                        bancos = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        id_bancos = pId_Bancos,
                        id_libros = pId_Libros,
                        usuario = filtro.usuario
                    });
                });
            
                return DbHelper.CreateOkResponse();

            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Detalle de Transacciones Vinculadas 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public ErrorDto<List<TesConciliacionDetallesData>> TES_ConciliacionDetalle_Obtener(int CodEmpresa, TesConciliacionFiltro filtro )
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@" exec spTes_Concilia_Periodo_Resultados_Caso_Detalle  
                                            @banco,
                                            @ahno,
                                            @mes,
                                            @ubicacion,
                                            @caso
                                           ";

                return conn.Query<TesConciliacionDetallesData>(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    ubicacion = filtro.ubicacion,
                    caso = filtro.caso
                }).ToList();
            });
        }

        /// <summary>
        /// Obtiene los detalles de un lote de conciliación bancaria para un periodo específico, filtrando por varios criterios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<TesConciliacionDetallesLoteData>> TES_ConciliacionDetalleLote_Obtener(int CodEmpresa, TesConciliacionFiltro filtro)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@" exec spTes_Concilia_Periodo_Resultados_Caso_Detalle  
                                            @banco,
                                            @ahno,
                                            @mes,
                                            @ubicacion,
                                            @caso
                                           ";

                return conn.Query<TesConciliacionDetallesLoteData>(query, new
                {
                    banco = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    ubicacion = filtro.ubicacion,
                    caso = filtro.caso
                }).ToList();
            });
        }

        /// <summary>
        /// Método para revertir una conciliación bancaria, permitiendo deshacer la asignación de movimientos entre bancos o libros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto TES_Conciliacion_Reversa(int CodEmpresa, TesConciliacionFiltro filtro, List<TesConciliacionDetallesData> datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith('C'))
                {
                    return DbHelper.ErrorResponse(vMensaje);
                }


                var query = "";

                datos.ForEach(item =>
                {
                    string pId_Bancos = "";
                    string pId_Libros = "";
                    if (filtro.ubicacion == "B")
                    {
                        pId_Bancos = filtro.mov_id!;
                        pId_Libros = item.id.ToString()!;
                    }
                    else
                    {
                        pId_Libros = filtro.mov_id!;
                        pId_Bancos = item.id.ToString()!;
                    }

                    if (filtro.movFiltro == "T")
                    {
                        query = $@"exec spTes_Concilia_Reversa @bancos, @ahno , @mes , @id_bancos, @id_libros, @usuario ";
                    }
                    else
                    {
                        query = $@"exec spTes_Concilia_Reversa @bancos, @ahno , @mes , @id_bancos, @id_libros, @usuario ";
                    }

                    conn.Execute(query, new
                    {
                        bancos = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        id_bancos = pId_Bancos,
                        id_libros = pId_Libros,
                        usuario = filtro.usuario
                    });
                });

                //Actualiza Resumen
                query = $@"exec spTes_Concilia_Periodo_Actualiza @bancos, @ahno , @mes , @usuario ";
                conn.Execute(query, new
                {
                    bancos = filtro.banco,
                    ahno = filtro.ahno,
                    mes = filtro.mes,
                    usuario = filtro.usuario
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message.ToString());
            }
        }

        #endregion


    }
}
