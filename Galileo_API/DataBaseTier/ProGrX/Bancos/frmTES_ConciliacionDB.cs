using PgxAPI.Models.ERROR;
using PgxAPI.Models;
using PgxAPI.Models.ProGrX.Bancos;
using Microsoft.Data.SqlClient;
using Dapper;
using PgxAPI.BusinessLogic;
using PgxAPI.Models.CxP;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_ConciliacionDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 9;
        private readonly mSecurityMainDb _Security_MainDB;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;
        private readonly mCntLinkDB mCntLinkDB;

        public frmTES_ConciliacionDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
            mCntLinkDB = new mCntLinkDB(_config);
        }

        /// <summary>
        /// Obtiene las cuentas bancarias para conciliación de un usuario específico en una empresa dada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<TesConciliacionCuentaData>> TES_ConciliacionBancosLst_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesConciliacionCuentaData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<TesConciliacionCuentaData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spTes_Cuenta_Bancaria_Acceso_General '{usuario}', 'ASI'";
                    response.Result = connection.Query<TesConciliacionCuentaData>(query).ToList();
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al obtener las cuentas bancarias para conciliación.";
                response.Result = null;
            }

            return response;

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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesConciliacionHistorico>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<TesConciliacionHistorico>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spTes_Concilia_Periodo_Consulta {id_banco}, '{usuario}'";
                    response.Result = connection.Query<TesConciliacionHistorico>(query).ToList();
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesConciliaPeriodo>
            {
                Code = 0,
                Description = "OK",
                Result = new TesConciliaPeriodo()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Valido si existe
                    var query = $@"Select COUNT('X') from vTES_CONCILIA_PERIODO
                                    where id_Banco = @banco
                                      and Anio = @anio
                                      and Mes = @mes ";
                    var exists = connection.Query<int>(query, new
                    {
                        banco = id_banco,
                        anio = pAnio,
                        mes
                    }).FirstOrDefault();

                    if(exists == 0)
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
                    response.Result = connection.Query<TesConciliaPeriodo>(query, new
                    {
                        banco = id_banco,
                        anio = pAnio,
                        mes
                    }).FirstOrDefault();




                    if (response.Result == null)
                    {
                        response.Code = -1;
                        response.Description = "Error al obtener Periodo para la conciliación Bancaria";
                        response.Result = null;
                    }
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al obtener las cuentas bancarias para conciliación.";
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Actualiza el saldo de conciliación bancaria para un periodo específico, validando si el periodo está cerrado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionSaldo_Actualiza(int CodEmpresa, TesConciliaFiltros filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                };

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spTes_Concilia_Periodo_Actualiza_Saldo_Cta @banco, @ahno,@mes,@saldo,@usuario";
                    connection.Execute(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        saldo = filtro.saldo,
                        usuario = filtro.usuario
                    });

                    query = $@"exec spTes_Concilia_Periodo_Actualiza @banco, @ahno,@mes,@usuario";
                    connection.Execute(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        usuario = filtro.usuario
                    });
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al obtener las cuentas bancarias para conciliación.";
            }

            return response;
        }

        /// <summary>
        /// Guarda nota y saldo actual de conciliación bancaria para un periodo específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionResumen_Guardar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                }
                ;

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spTes_Concilia_Periodo_Add @banco,@ahno,@mes,'A',@notas,@usuarios,@saldos,@saldoActual";
                    connection.Execute(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        notas = filtro.notas ?? string.Empty,
                        usuarios = filtro.usuario,
                        saldos = filtro.saldo,
                        saldoActual = filtro.saldoActual
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;

        }

        /// <summary>
        /// Carga un archivo de conciliación bancaria, procesando cada fila y actualizando el saldo de conciliación para un periodo específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionResumenArchivo_Cargar(int CodEmpresa, TesConciliaFiltros filtro, List<TesConciliacioExcelDTO> file)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                };

                using var connection = new SqlConnection(stringConn);
                {

                    foreach (var row in file)
                    {
                        if(row.importe < 0)
                        {
                            row.tipo = "D";
                        }

                        string fechaExcel = _AuxiliarDB.validaFechaGlobal(row.fecha);

                        var query = $@"exec spTes_Concilia_Banco_Mov @banco,@fechaExcel,@ndocumento,@tipo,@importe,@descripcion,0,@usuario";
                        connection.Execute(query, new
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
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Cierra un periodo de conciliación bancaria para una empresa, banco, año y mes específicos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionResumenPeriodo_Cerrar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                };

                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"exec spTes_Concilia_Periodo_Cierra @banco,@ahno,@mes,@usuario";
                    connection.Execute(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        usuario = filtro.usuario
                    }, commandTimeout: 300);

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                }
                ;
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    switch(tipo)
                    {
                        case 0: // Concilia Movimientos (E/S) Banco
                            query = $@"exec spTes_Concilia_Bancos_EntreSi @banco,@ahno,@mes,@usuario";
                            break;
                        case 1: // Concilia Movimientos (E/S) Libros
                            query = $@"exec spTes_Concilia_Libros_EntreSi @banco,@ahno,@mes,@usuario";
                            break;
                        default: // Cierra  
                            response.Code = -1;
                            response.Description = "Tipo de operación no válido.";
                            return response;
                    }

                    connection.Execute(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        usuario = filtro.usuario
                    }, commandTimeout: 300);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Actualiza el saldo de conciliación bancaria de forma automática para un periodo específico, validando si el periodo está cerrado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_Conciliacion_Actualizar(int CodEmpresa, TesConciliaFiltros filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                }
                ;

                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"exec spTes_Concilia_Automatica @banco,@ahno,@mes,@usuario";
                    connection.Execute(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        usuario = filtro.usuario
                    }, commandTimeout: 900);

                    query = $@"exec spTes_Concilia_Periodo_Actualiza @banco,@ahno,@mes,@usuario";
                    connection.Execute(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        usuario = filtro.usuario
                    }, commandTimeout: 900);

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Inicializa un periodo de conciliación bancaria para una empresa, banco, año y mes específicos, permitiendo la actualización del saldo inicial.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto TES_Conciliacion_Inicializa(int CodEmpresa, TesConciliaFiltros filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                }
                ;

                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"exec spTes_Concilia_Periodo_Inicializa @banco,@ahno,@mes,@usuario";
                    connection.Execute(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        usuario = filtro.usuario
                    }, commandTimeout: 300);

                    query = $@"exec spTes_Concilia_Periodo_Actualiza @banco,@ahno,@mes,@usuario";
                    connection.Execute(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        usuario = filtro.usuario
                    }, commandTimeout: 300);

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesConciliaResultados>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<TesConciliaResultados>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spTes_Concilia_Periodo_Resultados @banco, @ahno ,@mes,@ubicacion,@tipoDoc,@estadoCasos";
                    response.Result = connection.Query<TesConciliaResultados>(query, new
                    {
                        banco = filtros.id_banco,
                        ahno = filtros.ahno,
                        mes = filtros.mes,
                        ubicacion = filtros.ubicacion,
                        tipoDoc = filtros.tipoDoc,
                        estadoCasos = filtros.estadoCasos
                    }).ToList();
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

        /// <summary>
        /// Registra automáticamente los resultados de conciliación bancaria para un periodo específico, actualizando el saldo y el estado del periodo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto TES_ConciliacionResultados_Autoregistro(int CodEmpresa, TesConciliacionResultosFiltro filtro, List<TesConciliaResultados> datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                };

                bool vCuenta = mCntLinkDB.fxgCntCuentaValida(CodEmpresa, filtro.ar_cuenta);
                if (!vCuenta)
                {
                    response.Code = -1;
                    response.Description = "La cuenta contable indicada para el auto-registro no es válida!";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    foreach (var item in datos)
                    {
                        query = $@"exec spTes_Concilia_Auto_Registro @bancos, @ahno , @mes , @id, @cuenta , @usuario , @chkAutoReg ";
                        connection.Execute(query, new
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
                    connection.Execute(query, new
                    {
                        bancos = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        usuario = filtro.usuario
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                }
                ;

                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    foreach (var item in datos)
                    {
                        query = $@"exec spTes_Concilia_Pendiente @bancos, @ahno , @mes , @id , @ubicacion , @usuario ";
                        connection.Execute(query, new
                        {
                            bancos = filtro.banco,
                            ahno = filtro.ahno,
                            mes = filtro.mes,
                            id = item.id,
                            ubicacion = filtro.ubicacion,
                            usuario = filtro.usuario
                        });
                    }

                    //'Actualiza Resumen> No Aplica porque Los pendientes se tienen que reflejar
                    //query = $@"exec spTes_Concilia_Periodo_Actualiza @bancos, @ahno , @mes , @usuarios ";
                    //connection.Execute(query, new
                    //{
                    //    bancos = filtro.banco,
                    //    ahno = filtro.ahno,
                    //    mes = filtro.mes,
                    //    usuario = filtro.usuario
                    //});
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesConciliaAsigna>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<TesConciliaAsigna>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    string fechaInicio = _AuxiliarDB.validaFechaGlobal(filtros.dtpConciliaInicio);
                    DateTime original = DateTime.Parse(fechaInicio);
                    DateTime inicioDia = original.Date;

                    string resultadoInicio = inicioDia.ToString("yyyy-MM-dd HH:mm:ss");


                    string fechaCorte = _AuxiliarDB.validaFechaGlobal(filtros.dtpConciliaCorte);
                    DateTime originalCorte = DateTime.Parse(fechaInicio);
                    DateTime afechaFin = originalCorte.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                    string resultadoCorte = afechaFin.ToString("yyyy-MM-dd HH:mm:ss");


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
                    response.Result = connection.Query<TesConciliaAsigna>(query, new
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

        /// <summary>
        /// Aplica la conciliación bancaria para un periodo específico, asignando los datos de conciliación a los movimientos correspondientes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto TES_Conciliacion_Aplicar(int CodEmpresa, TesConciliacionFiltro filtro, List<TesConciliaAsigna> datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                };

                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    foreach (var item in datos)
                    {
                        string pId_Bancos = "";
                        string pId_Libros = "";
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

                        if(filtro.movFiltro == "T")
                        {
                            query = $@"exec spTes_Concilia_Aplicacion  @bancos, @ahno , @mes , @id_bancos, @id_libros, @usuario ";
                        }
                        else
                        {
                            query = $@"exec spTes_Concilia_Aplicacion_Lote @bancos, @ahno , @mes , @id_bancos, @id_libros, @usuario ";
                        }

                        connection.Execute(query, new
                        {
                            bancos = filtro.banco,
                            ahno = filtro.ahno,
                            mes = filtro.mes,
                            id_bancos = pId_Bancos,
                            id_libros = pId_Libros,
                            usuario = filtro.usuario
                        });
                    }


                    //Actualiza Resumen
                    //query = $@"exec spTes_Concilia_Periodo_Actualiza @bancos, @ahno , @mes , @usuario ";
                    //connection.Execute(query, new
                    //{
                    //    bancos = filtro.banco,
                    //    ahno = filtro.ahno,
                    //    mes = filtro.mes,
                    //    usuario = filtro.usuario
                    //});
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Detalle de Transacciones Vinculadas 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public ErrorDto<List<TesConciliacionDetallesData>> TES_ConciliacionDetalle_Obtener(int CodEmpresa, TesConciliacionFiltro filtro )
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesConciliacionDetallesData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<TesConciliacionDetallesData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@" exec spTes_Concilia_Periodo_Resultados_Caso_Detalle  
                                            @banco,
                                            @ahno,
                                            @mes,
                                            @ubicacion,
                                            @caso
                                           ";
                    response.Result = connection.Query<TesConciliacionDetallesData>(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        ubicacion = filtro.ubicacion,
                        caso = filtro.caso
                    }).ToList();
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

        /// <summary>
        /// Obtiene los detalles de un lote de conciliación bancaria para un periodo específico, filtrando por varios criterios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<TesConciliacionDetallesLoteData>> TES_ConciliacionDetalleLote_Obtener(int CodEmpresa, TesConciliacionFiltro filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesConciliacionDetallesLoteData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<TesConciliacionDetallesLoteData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@" exec spTes_Concilia_Periodo_Resultados_Caso_Detalle  
                                            @banco,
                                            @ahno,
                                            @mes,
                                            @ubicacion,
                                            @caso
                                           ";
                    response.Result = connection.Query<TesConciliacionDetallesLoteData>(query, new
                    {
                        banco = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        ubicacion = filtro.ubicacion,
                        caso = filtro.caso
                    }).ToList();
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

        /// <summary>
        /// Metodo para revertir una conciliación bancaria, permitiendo deshacer la asignación de movimientos entre bancos o libros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto TES_Conciliacion_Reversa(int CodEmpresa, TesConciliacionFiltro filtro, List<TesConciliacionDetallesData> datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (!string.IsNullOrEmpty(filtro.periodoEstado) && filtro.periodoEstado.StartsWith("C"))
                {
                    response.Code = -1;
                    response.Description = "El periodo ya se encuentra cerrado, no es posible actualizar el saldo.";
                    return response;
                }
                ;

                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    foreach (var item in datos)
                    {
                        string pId_Bancos = "";
                        string pId_Libros = "";
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

                        if (filtro.movFiltro == "T")
                        {
                            query = $@"exec spTes_Concilia_Reversa @bancos, @ahno , @mes , @id_bancos, @id_libros, @usuario ";
                        }
                        else
                        {
                            query = $@"exec spTes_Concilia_Reversa @bancos, @ahno , @mes , @id_bancos, @id_libros, @usuario ";
                        }

                        connection.Execute(query, new
                        {
                            bancos = filtro.banco,
                            ahno = filtro.ahno,
                            mes = filtro.mes,
                            id_bancos = pId_Bancos,
                            id_libros = pId_Libros,
                            usuario = filtro.usuario
                        });
                    }


                    //Actualiza Resumen
                    query = $@"exec spTes_Concilia_Periodo_Actualiza @bancos, @ahno , @mes , @usuario ";
                    connection.Execute(query, new
                    {
                        bancos = filtro.banco,
                        ahno = filtro.ahno,
                        mes = filtro.mes,
                        usuario = filtro.usuario
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion


    }
}
