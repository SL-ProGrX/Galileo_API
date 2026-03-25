using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public partial class FrmCxcCuentasDB
    {
        #region Activacion

        /// <summary>
        /// Verifica si una operación de CxC puede activarse.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos mínimos requeridos para validar la activación.</param>
        /// <returns>Resultado de validación de activación.</returns>
        public ErrorDto<CxCCuentasActivacionVerificaResult> CxCCuentasActivacion_Verifica(
            int codEmpresa,
            CxCCuentasActivacionRequest request)
        {
           
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasActivacionVerificaResult>(CxCCuentasConstantes.solicitudRequerida);
            }

            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasActivacionVerificaResult>(CxCCuentasConstantes.operacionRequerida);
            }

            var emitirTipo = NormalizarMayusculas(request.emitir_tipo);
            var emitirCuenta = NormalizarTexto(request.emitir_cuenta);
            var mensajes = new List<string>();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                if (emitirTipo == "TE" && string.IsNullOrWhiteSpace(emitirCuenta))
                {
                    mensajes.Add("- No se ha especificado una cuenta de ahorros para realizarle la transferencia...");
                }

                const string sqlMontos = @"
                    SELECT
                        Monto,
                        dbo.fxCxC_CuentaRebajos(@operacion, 'TOT') AS Rebajos,
                        ISNULL(dbo.fxCxC_CuentaIngresos(@operacion), 0) AS Ingresos
                    FROM CxC_Cuentas
                    WHERE Operacion = @operacion;";

                var montos = conn.QueryFirstOrDefault(sqlMontos, new
                {
                    operacion = request.operacion
                });

                if (montos is null)
                {
                    mensajes.Add("- No existe la operación indicada.");
                }
                else
                {
                    decimal monto = montos.Monto ?? 0m;
                    decimal rebajos = montos.Rebajos ?? 0m;
                    decimal ingresos = montos.Ingresos ?? 0m;

                    if (rebajos > (monto + ingresos))
                    {
                        mensajes.Add("- El monto de los rebajos es mayor que el monto de la operación más otros ingresos.");
                    }
                }

                const string sqlFacturas = @"exec spCxC_Operacion_Facturas_Verifica @operacion;";

                var facturas = conn.Query(sqlFacturas, new
                {
                    operacion = request.operacion
                });

                foreach (var item in facturas)
                {
                    mensajes.Add($"- Factura No.: {item.cod_Factura}, se encuentra registrada en la Operación: {item.Operacion}");
                }

                return DbHelper.CreateOkResponse<CxCCuentasActivacionVerificaResult>(
                     new CxCCuentasActivacionVerificaResult
                     {
                         pass = mensajes.Count == 0,
                         mensaje = string.Join(Environment.NewLine, mensajes)
                     });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasActivacionVerificaResult>(
                    $"Error inesperado al verificar la activación. {ex.Message}");
            }
        }

        /// <summary>
        /// Activa una operación de CxC aplicando rebajos, ingresos y estado de tesorería.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos requeridos para activar la operación.</param>
        /// <returns>Resultado del proceso de activación.</returns>
        public ErrorDto<bool> CxCCuentasActivacion_Activar(
            int codEmpresa,
            CxCCuentasActivacionRequest request)
        {
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            if (request is null)
            {
                response.Code = -1;
                response.Description = CxCCuentasConstantes.solicitudRequerida;
                response.Result = false;
                return response;
            }

            if (request.operacion <= 0)
            {
                response.Code = -1;
                response.Description = CxCCuentasConstantes.operacionRequerida;
                response.Result = false;
                return response;
            }

            var usuario = NormalizarTexto(request.usuario);
            var emitirTipo = NormalizarMayusculas(request.emitir_tipo);
            var numDocumento = string.IsNullOrWhiteSpace(request.num_documento) ? null : request.num_documento.Trim();

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(emitirTipo))
            {
                response.Code = -1;
                response.Description = "Faltan datos requeridos para activar la operación.";
                response.Result = false;
                return response;
            }

            var verifica = CxCCuentasActivacion_Verifica(codEmpresa, request);
            if (verifica.Code == -1 || verifica.Result is null || !verifica.Result.pass)
            {
                response.Code = -1;
                response.Description = verifica.Result?.mensaje ?? verifica.Description;
                response.Result = false;
                return response;
            }

            DbTransaction? transaction = null;

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                transaction = conn.BeginTransaction();

                const string sqlContexto = @"
                    SELECT
                        C.Monto,
                        dbo.fxCxC_CuentaRebajos(C.Operacion, 'TOT') AS Rebajos,
                        ISNULL(dbo.fxCxC_CuentaIngresos(C.Operacion), 0) AS Ingresos,
                        ISNULL(P.Genera_Desembolso, 0) AS Genera_Desembolso,
                        dbo.MyGetdate() AS Fecha_Server
                    FROM CxC_Cuentas C
                    INNER JOIN CxC_Conceptos P
                        ON C.cod_concepto = P.cod_concepto
                    WHERE C.Operacion = @operacion;";

                var contexto = conn.QueryFirstOrDefault(sqlContexto, new
                {
                    operacion = request.operacion
                }, transaction);

                if (contexto is null)
                {
                    transaction.Rollback();
                    response.Code = -1;
                    response.Description = "No se encontró la operación para activar.";
                    response.Result = false;
                    return response;
                }

                decimal monto = contexto.Monto ?? 0m;
                decimal rebajos = contexto.Rebajos ?? 0m;
                decimal ingresos = contexto.Ingresos ?? 0m;
                int generaDesembolso = contexto.Genera_Desembolso ?? 0;
                DateTime fechaServer = contexto.Fecha_Server ?? DateTime.Now;

                const string sqlActiva = @"
                    UPDATE CxC_Cuentas
                    SET
                        Estado = 'A',
                        Activa_Fecha = dbo.MyGetdate(),
                        Activa_Usuario = @usuario,
                        Rebajos_Total = @rebajos,
                        Ingresos_Total = @ingresos,
                        Desembolso_Monto = Monto + @ingresos - @rebajos,
                        Num_Documento = @num_documento
                    WHERE Operacion = @operacion;";

                conn.Execute(sqlActiva, new
                {
                    operacion = request.operacion,
                    usuario,
                    rebajos,
                    ingresos,
                    num_documento = numDocumento
                }, transaction);

                if (request.es_factoreo)
                {
                    const string sqlPendienteFactoreo = @"
                        UPDATE CxC_Cuentas
                        SET Desembolso_Pendiente =
                            CASE
                                WHEN (Desembolso_Realizado + Desembolso_Pendiente) > Desembolso_Monto
                                    THEN Desembolso_Monto - Desembolso_Realizado
                                ELSE Desembolso_Pendiente
                            END
                        WHERE Operacion = @operacion;";

                    conn.Execute(sqlPendienteFactoreo, new
                    {
                        operacion = request.operacion
                    }, transaction);
                }
                else
                {
                    const string sqlPendiente = @"
                        UPDATE CxC_Cuentas
                        SET Desembolso_Pendiente = Desembolso_Monto - Desembolso_Realizado
                        WHERE Operacion = @operacion;";

                    conn.Execute(sqlPendiente, new
                    {
                        operacion = request.operacion
                    }, transaction);
                }

                if ((monto + ingresos) <= rebajos || generaDesembolso == 0)
                {
                    const string sqlTesoreriaC = @"
                        UPDATE CxC_Cuentas
                        SET
                            Tesoreria_Fecha = dbo.MyGetdate(),
                            Tesoreria_Solicitud = 0,
                            Tesoreria_Estado = 'C',
                            Tesoreria_Usuario = @usuario
                        WHERE Operacion = @operacion;";

                    conn.Execute(sqlTesoreriaC, new
                    {
                        operacion = request.operacion,
                        usuario
                    }, transaction);
                }
                else
                {
                    const string sqlTesoreriaP = @"
                        UPDATE CxC_Cuentas
                        SET Tesoreria_Estado = 'P'
                        WHERE Operacion = @operacion;";

                    conn.Execute(sqlTesoreriaP, new
                    {
                        operacion = request.operacion
                    }, transaction);
                }

                const string sqlDetalle = @"
                    exec spCxC_CuentaActivaDetalle @operacion, @fecha, @usuario;";

                conn.Execute(sqlDetalle, new
                {
                    operacion = request.operacion,
                    fecha = fechaServer.ToString(CxCCuentasConstantes.fechaFormat),
                    usuario
                }, transaction);

                transaction.Commit();
            }
            catch (DbException ex)
            {
                transaction?.Rollback();
                response.Code = -1;
                response.Description = $"No fue posible activar la operación. {ex.Message}";
                response.Result = false;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                response.Code = -1;
                response.Description = $"Error inesperado al activar la operación. {ex.Message}";
                response.Result = false;
            }

            return response;
        }

        /// <summary>
        /// Obtiene el detalle o resumen de activación de una operación de CxC según la opción seleccionada.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos de consulta del detalle de activación.</param>
        /// <returns>Detalle de activación y estado de tesorería.</returns>
        public ErrorDto<CxCCuentasActivacionDetalleResult> CxCCuentasActivacionDetalle_Obtener(
            int codEmpresa,
            CxCCuentasActivacionDetalleRequest request)
        {
            var response = new ErrorDto<CxCCuentasActivacionDetalleResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasActivacionDetalleResult()
            };

            if (request is null || request.operacion <= 0)
            {
                response.Code = -1;
                response.Description = CxCCuentasConstantes.operacionRequerida;
                response.Result = new CxCCuentasActivacionDetalleResult();
                return response;
            }

            var opcion = (request.opcion ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(opcion))
            {
                opcion = "RSM";
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sqlTesoreria = @"
                    SELECT TOP 1
                        ISNULL(C.Genera_Desembolso, 0) AS procesa_tesoreria
                    FROM CxC_Cuentas R
                    INNER JOIN CxC_Conceptos C
                        ON R.cod_concepto = C.cod_concepto
                    WHERE R.Operacion = @operacion;";

                response.Result.procesa_tesoreria = conn.QueryFirstOrDefault<bool>(sqlTesoreria, new
                {
                    operacion = request.operacion
                });

                switch (opcion)
                {
                    case "ING":
                        response.Result.lista = conn.Query<CxCCuentasActivacionDetalleItem>(@"
                            SELECT
                                ISNULL(R.cod_cargo, '') AS descripcion,
                                ISNULL(A.monto, 0) AS monto,
                                CONCAT(
                                    ISNULL(R.descripcion, ''),
                                    ' | ',
                                    CASE WHEN ISNULL(A.tipo, '') = 'P' THEN 'Porcentual' ELSE 'Monto' END,
                                    ' | Valor: ',
                                    CONVERT(varchar(50), ISNULL(A.valor, 0))
                                ) AS detalle
                            FROM CxC_Cargos R
                            INNER JOIN CxC_Cuentas_Ingresos A
                                ON R.cod_cargo = A.cod_cargo
                            WHERE A.Operacion = @operacion;", new
                        {
                            operacion = request.operacion
                        }).ToList();
                        break;

                    case "CRD":
                        response.Result.lista = conn.Query<CxCCuentasActivacionDetalleItem>(@"
                            SELECT
                                CONVERT(varchar(50), Reb.id_solicitud) AS descripcion,
                                ISNULL(Reb.monto, 0) AS monto,
                                CONCAT(ISNULL(Cat.codigo, ''), ' | ', ISNULL(Cat.descripcion, '')) AS detalle
                            FROM CxC_Cuentas_Rebajos_Crd Reb
                            INNER JOIN Reg_Creditos Crd
                                ON Reb.id_solicitud = Crd.id_Solicitud
                            INNER JOIN catalogo Cat
                                ON Crd.codigo = Cat.codigo
                            WHERE Reb.Operacion = @operacion;", new
                        {
                            operacion = request.operacion
                        }).ToList();
                        break;

                    case "CXC":
                        response.Result.lista = conn.Query<CxCCuentasActivacionDetalleItem>(@"
                            SELECT
                                CONVERT(varchar(50), R.Operacion_Aplicada) AS descripcion,
                                ISNULL(R.Monto, 0) AS monto,
                                CONCAT(ISNULL(Cta.cod_concepto, ''), ' | ', ISNULL(C.Descripcion, '')) AS detalle
                            FROM CxC_Cuentas_Rebajos R
                            INNER JOIN CxC_Cuentas Cta
                                ON R.Operacion_Aplicada = Cta.Operacion
                            INNER JOIN CxC_Conceptos C
                                ON Cta.cod_concepto = C.cod_concepto
                            WHERE R.Operacion = @operacion;", new
                        {
                            operacion = request.operacion
                        }).ToList();
                        break;

                    case "CAR":
                        response.Result.lista = conn.Query<CxCCuentasActivacionDetalleItem>(@"
                            SELECT
                                ISNULL(R.cod_cargo, '') AS descripcion,
                                ISNULL(A.monto, 0) AS monto,
                                CONCAT(
                                    ISNULL(R.descripcion, ''),
                                    ' | ',
                                    CASE WHEN ISNULL(A.tipo, '') = 'P' THEN 'Porcentual' ELSE 'Monto' END,
                                    ' | Valor: ',
                                    CONVERT(varchar(50), ISNULL(A.valor, 0))
                                ) AS detalle
                            FROM CxC_Cargos R
                            INNER JOIN CxC_Cuentas_Rebajos_Cargos A
                                ON R.cod_cargo = A.cod_cargo
                            WHERE A.Operacion = @operacion;", new
                        {
                            operacion = request.operacion
                        }).ToList();
                        break;

                    default:
                        var resumen = conn.QueryFirstOrDefault(@"
                            SELECT
                                C.Monto,
                                ISNULL(dbo.fxCxC_CuentaRebajos(@operacion, 'CRD'), 0) AS Crd,
                                ISNULL(dbo.fxCxC_CuentaRebajos(@operacion, 'CxC'), 0) AS CxC,
                                ISNULL(dbo.fxCxC_CuentaRebajos(@operacion, 'CAR'), 0) AS Car,
                                ISNULL(dbo.fxCxC_CuentaRebajos(@operacion, 'ADL'), 0) AS Adl,
                                ISNULL(dbo.fxCxC_CuentaIngresos(@operacion), 0) AS Ing
                            FROM CxC_Cuentas C
                            WHERE C.Operacion = @operacion;", new
                        {
                            operacion = request.operacion
                        });

                        if (resumen is not null)
                        {
                            decimal monto = resumen.Monto ?? 0m;
                            decimal ing = resumen.Ing ?? 0m;
                            decimal crd = resumen.Crd ?? 0m;
                            decimal cxc = resumen.CxC ?? 0m;
                            decimal car = resumen.Car ?? 0m;
                            decimal adl = resumen.Adl ?? 0m;
                            decimal desembolsar = monto + ing - (crd + cxc + car + adl);

                            response.Result.lista = new List<CxCCuentasActivacionDetalleItem>
                            {
                                new() { descripcion = "Monto Aprobado", monto = monto, detalle = string.Empty },
                                new() { descripcion = "(+) Otros Ingresos", monto = ing, detalle = string.Empty },
                                new() { descripcion = "(-) Abonos a Créditos", monto = crd, detalle = string.Empty },
                                new() { descripcion = "(-) Abonos a CxC Pendientes", monto = cxc, detalle = string.Empty },
                                new() { descripcion = "(-) Cargos Registrados", monto = car, detalle = string.Empty },
                                new() { descripcion = "(-) Adelantos", monto = adl, detalle = string.Empty },
                                new() { descripcion = "Monto a Desembolsar", monto = desembolsar, detalle = string.Empty }
                            };
                        }
                        break;
                }
            }
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"No fue posible consultar el detalle de activación. {ex.Message}";
                response.Result = new CxCCuentasActivacionDetalleResult();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al consultar el detalle de activación. {ex.Message}";
                response.Result = new CxCCuentasActivacionDetalleResult();
            }

            return response;
        }

        #endregion
    }
}
