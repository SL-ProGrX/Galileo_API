using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_Pasivos
{
    public class FrmCrApaCortesGarantiasDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrApaCortesGarantiasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>Obtiene los acreedores disponibles para el proceso de cortes.</summary>
        public ErrorDto<List<FrmCrApaCortesGarantiasCatalogoDto>> CR_APA_CortesGarantias_Acreedores_Obtener(int codEmpresa)
            => DbHelper.ExecuteListQuery<FrmCrApaCortesGarantiasCatalogoDto>(
                _portalDb,
                codEmpresa,
                "select trim(COD_ACREEDOR) idx, trim(DESCRIPCION) itmx from CRD_APA_ACREEDORES order by DESCRIPCION");

        /// <summary>Obtiene las operaciones asociadas al acreedor indicado.</summary>
        public ErrorDto<List<FrmCrApaCortesGarantiasCatalogoDto>> CR_APA_CortesGarantias_Operaciones_Obtener(int codEmpresa, string cod_acreedor)
            => DbHelper.ExecuteListQuery<FrmCrApaCortesGarantiasCatalogoDto>(
                _portalDb,
                codEmpresa,
                "select trim(OPERACION) idx, trim(OPERACION) itmx from CRD_APA_OPERACIONES where COD_ACREEDOR = @Acreedor order by OPERACION",
                new { Acreedor = cod_acreedor.Trim() });

        /// <summary>Obtiene el acreedor y saldo de la operación seleccionada.</summary>
        public ErrorDto<FrmCrApaCortesGarantiasEncabezadoDto?> CR_APA_CortesGarantias_Encabezado_Obtener(int codEmpresa, string operacion)
            => DbHelper.ExecuteSingleQuery<FrmCrApaCortesGarantiasEncabezadoDto?>(
                _portalDb,
                codEmpresa,
                @"select trim(AC.COD_ACREEDOR) cod_acreedor,
                         trim(AC.DESCRIPCION) acreedor,
                         trim(OP.OPERACION) operacion,
                         isnull(OP.SALDO, 0) saldo
                  from CRD_APA_OPERACIONES OP
                  inner join CRD_APA_ACREEDORES AC on AC.COD_ACREEDOR = OP.COD_ACREEDOR
                  where OP.OPERACION = @Operacion",
                null,
                new { Operacion = operacion.Trim() });

        /// <summary>Obtiene un catálogo controlado usado por los filtros del formulario.</summary>
        public ErrorDto<List<FrmCrApaCortesGarantiasCatalogoDto>> CR_APA_CortesGarantias_Catalogo_Obtener(int codEmpresa, string tipo)
        {
            string sql = tipo.Trim().ToUpperInvariant() switch
            {
                "GARANTIAS" => "select trim(GARANTIA) idx, trim(DESCRIPCION) itmx from CRD_GARANTIA_TIPOS order by DESCRIPCION",
                "CATEGORIAS" => "select trim(COD_ANTIGUEDAD) idx, trim(DESCRIPCION) itmx from CBR_ANTIGUEDAD_TIPOS order by DESCRIPCION",
                "LINEAS" => "select trim(CODIGO) idx, trim(DESCRIPCION) itmx from CATALOGO where ACTIVO = 1 and RETENCION = 'N' and POLIZA = 'N' order by DESCRIPCION",
                "BUCKETS" => "select trim(COD_BUCKET) idx, trim(DESCRIPCION) itmx from CBR_BUCKETS order by COD_BUCKET",
                "DESTINOS" => "select trim(COD_DESTINO) idx, trim(DESCRIPCION) itmx from CATALOGO_DESTINOS order by DESCRIPCION",
                "RECURSOS" => "select trim(COD_GRUPO) idx, trim(DESCRIPCION) itmx from CATALOGO_GRUPOS order by DESCRIPCION",
                _ => string.Empty
            };

            return string.IsNullOrEmpty(sql)
                ? DbHelper.CreateErrorResponse<List<FrmCrApaCortesGarantiasCatalogoDto>>("El catálogo solicitado no es válido.")
                : DbHelper.ExecuteListQuery<FrmCrApaCortesGarantiasCatalogoDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>Consulta el histórico de cortes de una operación.</summary>
        public ErrorDto<List<FrmCrApaCortesGarantiasCorteDto>> CR_APA_CortesGarantias_Historico_Obtener(int codEmpresa, string cod_acreedor, string operacion)
            => DbHelper.ExecuteListQuery<FrmCrApaCortesGarantiasCorteDto>(
                _portalDb,
                codEmpresa,
                @"select FECHA_CORTE fecha_corte,
                         isnull(SALDO_OPERACION, 0) saldo_operacion,
                         dbo.fxCRDAPASaldoCorteGarantias(COD_ACREEDOR, OPERACION, FECHA_CORTE) saldo_garantias,
                         dbo.fxCRDAPASaldoCorteResponsabilidad(COD_ACREEDOR, OPERACION, FECHA_CORTE) responsabilidad,
                         dbo.fxCRDAPASaldoCorteDiferencia(COD_ACREEDOR, OPERACION, FECHA_CORTE) diferencia,
                         case ESTADO when 'A' then 'Abierto' when 'C' then 'Cerrado' else '' end estado,
                         REGISTRO_FECHA registro_fecha
                  from CRD_APA_GARANTIAS_CORTES
                  where COD_ACREEDOR = @Acreedor and OPERACION = @Operacion
                  order by FECHA_CORTE desc",
                new { Acreedor = cod_acreedor.Trim(), Operacion = operacion.Trim() });

        /// <summary>Consulta los datos de captura de un corte.</summary>
        public ErrorDto<FrmCrApaCortesGarantiasCorteDatosDto?> CR_APA_CortesGarantias_Corte_Obtener(int codEmpresa, FrmCrApaCortesGarantiasClaveRequest request)
            => DbHelper.ExecuteSingleQuery<FrmCrApaCortesGarantiasCorteDatosDto?>(
                _portalDb,
                codEmpresa,
                @"select COD_ACREEDOR cod_acreedor, OPERACION operacion, FECHA_CORTE fecha_corte,
                         REGISTRO_FECHA registro_fecha, REGISTRO_USUARIO registro_usuario,
                         CIERRE_FECHA cierre_fecha, CIERRE_USUARIO cierre_usuario, ESTADO estado,
                         case ESTADO when 'A' then 'Abierto' when 'C' then 'Cerrado' else '' end estado_desc,
                         isnull(NOTAS, '') notas, isnull(SALDO_OPERACION, 0) saldo_operacion,
                         isnull(SALDO_RESPONSABILIDAD, 0) saldo_responsabilidad
                  from CRD_APA_GARANTIAS_CORTES
                  where COD_ACREEDOR = @Acreedor and OPERACION = @Operacion and FECHA_CORTE = @FechaCorte",
                null,
                ClaveParametros(request));

        /// <summary>Consulta las garantías que forman parte del corte.</summary>
        public ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> CR_APA_CortesGarantias_Detalle_Obtener(int codEmpresa, FrmCrApaCortesGarantiasConsultaRequest request)
        {
            FrmCrApaCortesGarantiasFiltrosDto filtros = request.filtrar ? request.filtros : new();
            ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> response = ConsultarGarantias(
                codEmpresa,
                @"exec spCrd_APA_Corte_Garantias_Consulta
                    @Acreedor, @Operacion, @FechaCorte, @Categoria, @Garantias, @FechaDesde, @FechaHasta",
                new
                {
                    Acreedor = request.cod_acreedor.Trim(),
                    Operacion = request.operacion.Trim(),
                    FechaCorte = request.fecha_corte,
                    Categoria = NuloSiVacio(filtros.categoria),
                    Garantias = NuloSiVacio(filtros.garantias),
                    FechaDesde = request.filtrar
                        ? filtros.fecha_desde.Date
                        : new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                    FechaHasta = request.filtrar
                        ? filtros.fecha_hasta.Date.AddDays(1).AddTicks(-1)
                        : new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
                });

            NormalizarIdentificadores(response);
            return request.filtrar ? AplicarFiltrosResultado(response, filtros) : response;
        }

        /// <summary>Consulta las garantías candidatas a incluir en el corte.</summary>
        public ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> CR_APA_CortesGarantias_Inclusiones_Obtener(int codEmpresa, FrmCrApaCortesGarantiasConsultaRequest request)
        {
            FrmCrApaCortesGarantiasFiltrosDto filtros = request.filtros;
            ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> response = ConsultarGarantias(
                codEmpresa,
                @"exec spCrd_APA_Corte_Garantias_Inclusiones
                    @Acreedor, @Operacion, @FechaCorte, @Categoria, @Garantias, @FechaDesde, @FechaHasta,
                    @Destino, @Recurso, @Linea, @SaldoMayor, @MoraMayor, @Bucket",
                new
                {
                    Acreedor = request.cod_acreedor.Trim(),
                    Operacion = request.operacion.Trim(),
                    FechaCorte = request.fecha_corte,
                    Categoria = NuloSiVacio(filtros.categoria),
                    Garantias = NuloSiVacio(filtros.garantias),
                    FechaDesde = filtros.fecha_desde.Date,
                    FechaHasta = filtros.fecha_hasta.Date.AddDays(1).AddTicks(-1),
                    Destino = NuloSiVacio(filtros.destino),
                    Recurso = NuloSiVacio(filtros.recurso),
                    Linea = NuloSiVacio(filtros.linea),
                    SaldoMayor = filtros.saldo_mayor,
                    MoraMayor = filtros.mora_mayor,
                    Bucket = NuloSiVacio(filtros.bucket)
                });

            NormalizarIdentificadores(response);
            return AplicarFiltrosResultado(response, filtros);
        }

        /// <summary>Obtiene los totales del corte seleccionado.</summary>
        public ErrorDto<FrmCrApaCortesGarantiasTotalesDto?> CR_APA_CortesGarantias_Totales_Obtener(int codEmpresa, FrmCrApaCortesGarantiasClaveRequest request)
            => DbHelper.ExecuteSingleQuery<FrmCrApaCortesGarantiasTotalesDto?>(
                _portalDb,
                codEmpresa,
                @"select isnull(SALDO_OPERACION, 0) saldo_operacion,
                         dbo.fxCRDAPASaldoCorteGarantias(COD_ACREEDOR, OPERACION, FECHA_CORTE) saldo_garantias,
                         dbo.fxCRDAPASaldoCorteResponsabilidad(COD_ACREEDOR, OPERACION, FECHA_CORTE) responsabilidad,
                         dbo.fxCRDAPASaldoCorteDiferencia(COD_ACREEDOR, OPERACION, FECHA_CORTE) diferencia
                  from CRD_APA_GARANTIAS_CORTES
                  where COD_ACREEDOR = @Acreedor and OPERACION = @Operacion and FECHA_CORTE = @FechaCorte",
                null,
                ClaveParametros(request));

        /// <summary>Registra un corte nuevo o modifica sus notas.</summary>
        public ErrorDto CR_APA_CortesGarantias_Guardar(int codEmpresa, FrmCrApaCortesGarantiasGuardarRequest request)
        {
            if (request.editar)
            {
                return Ejecutar(codEmpresa, connection =>
                {
                    int actualizados = connection.Execute(
                        @"update CRD_APA_GARANTIAS_CORTES set NOTAS = @Notas
                          where COD_ACREEDOR = @Acreedor and OPERACION = @Operacion and FECHA_CORTE = @FechaCorte",
                        new
                        {
                            Acreedor = request.cod_acreedor.Trim(),
                            Operacion = request.operacion.Trim(),
                            FechaCorte = request.fecha_corte.Date,
                            Notas = request.notas.Trim()
                        });

                    if (actualizados == 0)
                    {
                        throw new InvalidOperationException("No se encontró el corte que se desea actualizar.");
                    }
                });
            }

            return Ejecutar(codEmpresa, connection =>
            {
                int existe = connection.ExecuteScalar<int>(
                    @"select count(*) from CRD_APA_GARANTIAS_CORTES
                      where COD_ACREEDOR = @Acreedor and OPERACION = @Operacion and (FECHA_CORTE = @FechaCorte or ESTADO = 'A')",
                    new { Acreedor = request.cod_acreedor.Trim(), Operacion = request.operacion.Trim(), FechaCorte = request.fecha_corte.Date });
                if (existe > 0)
                {
                    throw new InvalidOperationException("Ya existe un corte para la fecha seleccionada o la operación tiene un corte abierto.");
                }

                connection.Execute(
                    "exec spCRDAPAGARANTIASCORTES_A @Acreedor, @Operacion, @FechaCorte, @RegistroFecha, @Usuario, @Notas",
                    new { Acreedor = request.cod_acreedor.Trim(), Operacion = request.operacion.Trim(), FechaCorte = request.fecha_corte.Date, RegistroFecha = DateTime.Now, Usuario = request.usuario.Trim(), Notas = request.notas.Trim() });
            });
        }

        /// <summary>Cierra un corte activo cuando no presenta faltante.</summary>
        public ErrorDto CR_APA_CortesGarantias_Cerrar(int codEmpresa, FrmCrApaCortesGarantiasClaveRequest request)
            => Ejecutar(codEmpresa, connection =>
            {
                var estado = connection.QuerySingleOrDefault<(string Estado, decimal Diferencia)>(
                    @"select ESTADO Estado, dbo.fxCRDAPASaldoCorteDiferencia(COD_ACREEDOR, OPERACION, FECHA_CORTE) Diferencia
                      from CRD_APA_GARANTIAS_CORTES
                      where COD_ACREEDOR = @Acreedor and OPERACION = @Operacion and FECHA_CORTE = @FechaCorte",
                    ClaveParametros(request));
                if (!string.Equals(estado.Estado, "A", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Solo se pueden cerrar cortes en estado abierto.");
                }
                if (estado.Diferencia < 0)
                {
                    throw new InvalidOperationException("No se puede cerrar un corte con faltante.");
                }
                connection.Execute(
                    "exec spCRDAPAGARANTIASCORTES_CERRAR @Acreedor, @Operacion, @FechaCorte, @CierreFecha, @Usuario",
                    new { Acreedor = request.cod_acreedor.Trim(), Operacion = request.operacion.Trim(), FechaCorte = request.fecha_corte, CierreFecha = DateTime.Now, Usuario = request.usuario.Trim() });
            });

        /// <summary>Actualiza los datos financieros de las garantías de un corte activo.</summary>
        public ErrorDto CR_APA_CortesGarantias_Actualizar(int codEmpresa, FrmCrApaCortesGarantiasClaveRequest request)
            => EjecutarCorteActivo(codEmpresa, request, connection =>
                connection.Execute("exec spCRDAPAGARANTIAS_H_Actualiza @Acreedor, @Operacion, @FechaCorte", ClaveParametros(request)));

        /// <summary>Excluye en lote las garantías marcadas de un corte activo.</summary>
        public ErrorDto CR_APA_CortesGarantias_Excluir(int codEmpresa, FrmCrApaCortesGarantiasExcluirRequest request)
            => EjecutarCorteActivo(codEmpresa, request, connection =>
            {
                using var transaction = connection.BeginTransaction();
                try
                {
                    foreach (int solicitud in request.solicitudes.Distinct())
                    {
                        connection.Execute(
                            "exec spCRDAPAGARANTIAS_H_Excluir @Acreedor, @Operacion, @FechaCorte, @Solicitud, @Tipo",
                            new { Acreedor = request.cod_acreedor.Trim(), Operacion = request.operacion.Trim(), FechaCorte = request.fecha_corte, Solicitud = solicitud, request.tipo },
                            transaction);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

        /// <summary>Incluye en lote las garantías marcadas en un corte activo.</summary>
        public ErrorDto CR_APA_CortesGarantias_Incluir(int codEmpresa, FrmCrApaCortesGarantiasIncluirRequest request)
            => EjecutarCorteActivo(codEmpresa, request, connection =>
            {
                using var transaction = connection.BeginTransaction();
                try
                {
                    foreach (FrmCrApaCortesGarantiasDetalleDto garantia in request.garantias.GroupBy(item => item.id_solicitud).Select(group => group.First()))
                    {
                        connection.Execute(
                            @"exec spCRDAPAGARANTIAS_H_A @Acreedor, @Operacion, @FechaCorte, @Solicitud,
                              @Tasa, @Plazo, @Cuota, @Saldo, @Categoria, @MoraIntereses, @MoraPrincipal, @MoraCuotas",
                            new
                            {
                                Acreedor = request.cod_acreedor.Trim(),
                                Operacion = request.operacion.Trim(),
                                FechaCorte = request.fecha_corte,
                                Solicitud = garantia.id_solicitud,
                                garantia.tasa,
                                garantia.plazo,
                                garantia.cuota,
                                garantia.saldo,
                                garantia.categoria,
                                MoraIntereses = garantia.mora_intereses,
                                MoraPrincipal = garantia.mora_principal,
                                MoraCuotas = garantia.mora_cuotas
                            },
                            transaction);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

        private ErrorDto EjecutarCorteActivo(int codEmpresa, FrmCrApaCortesGarantiasClaveRequest request, Action<SqlConnection> action)
            => Ejecutar(codEmpresa, connection =>
            {
                string? estado = connection.QuerySingleOrDefault<string>(
                    @"select ESTADO from CRD_APA_GARANTIAS_CORTES
                      where COD_ACREEDOR = @Acreedor and OPERACION = @Operacion and FECHA_CORTE = @FechaCorte",
                    ClaveParametros(request));
                if (!string.Equals(estado, "A", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Solo se pueden modificar cortes en estado abierto.");
                }
                action(connection);
            });

        private ErrorDto Ejecutar(int codEmpresa, Action<SqlConnection> action)
        {
            try
            {
                using SqlConnection connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                action(connection);
                return DbHelper.OkResponse("Información procesada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static object ClaveParametros(FrmCrApaCortesGarantiasClaveRequest request)
            => new { Acreedor = request.cod_acreedor.Trim(), Operacion = request.operacion.Trim(), FechaCorte = request.fecha_corte.Date };

        private static ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> AplicarFiltrosResultado(
            ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> response,
            FrmCrApaCortesGarantiasFiltrosDto filtros)
        {
            if (response.Result is null)
            {
                return response;
            }

            IEnumerable<FrmCrApaCortesGarantiasDetalleDto> resultado = response.Result;
            string estado = NormalizarFiltro(filtros.estado);
            string linea = NormalizarFiltro(filtros.linea);
            string bucket = NormalizarFiltro(filtros.bucket);

            if (!string.IsNullOrEmpty(estado))
            {
                resultado = resultado.Where(item => CoincideEstado(item.estado, estado));
            }
            if (!string.IsNullOrEmpty(linea))
            {
                resultado = resultado.Where(item => CoincideTexto(item.linea, linea));
            }
            if (!string.IsNullOrEmpty(bucket))
            {
                resultado = resultado.Where(item => CoincideTexto(item.bucket, bucket));
            }
            if (filtros.saldo_mayor > 0)
            {
                resultado = resultado.Where(item => item.saldo >= filtros.saldo_mayor);
            }
            if (filtros.mora_mayor > 0)
            {
                resultado = resultado.Where(item => item.mora_cuotas >= filtros.mora_mayor);
            }

            response.Result = resultado.ToList();
            return response;
        }

        private static void NormalizarIdentificadores(ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> response)
        {
            if (response.Result is null)
            {
                return;
            }

            foreach (FrmCrApaCortesGarantiasDetalleDto item in response.Result)
            {
                if (item.id_solicitud == 0)
                {
                    item.id_solicitud = item.operacion != 0
                        ? item.operacion
                        : item.solicitud;
                }
                if (item.monto == 0 && item.monto_solicitado != 0)
                {
                    item.monto = item.monto_solicitado;
                }
                if (string.IsNullOrWhiteSpace(item.linea))
                {
                    item.linea = PrimerValor(item.codigo, item.cod_linea, item.linea_credito);
                }
                if (item.fecha_formalizacion is null)
                {
                    item.fecha_formalizacion = item.fecha_formaliza;
                }
            }
        }

        /// <summary>
        /// Ejecuta los procedimientos legacy de garantías y homologa sus columnas al contrato del formulario.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="sql">Ejecución parametrizada del procedimiento.</param>
        /// <param name="parametros">Parámetros requeridos por el procedimiento.</param>
        /// <returns>Garantías homologadas según el orden original del grid VB6.</returns>
        private ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> ConsultarGarantias(int codEmpresa, string sql, object parametros)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query(sql, parametros)
                    .Select(MapearGarantia)
                    .ToList());
        }

        private static FrmCrApaCortesGarantiasDetalleDto MapearGarantia(dynamic fila)
        {
            IDictionary<string, object> valores = (IDictionary<string, object>)fila;
            return new FrmCrApaCortesGarantiasDetalleDto
            {
                id_solicitud = ValorEntero(valores, 0, "id_solicitud", "operacion", "solicitud", "nsolicitud"),
                monto = ValorDecimal(valores, 1, "monto", "monto_solicitado", "monto_operacion"),
                cuota = ValorDecimal(valores, 2, "cuota"),
                saldo = ValorDecimal(valores, 3, "saldo"),
                categoria = ValorTexto(valores, 4, "categoria"),
                linea = ValorTexto(valores, 5, "linea", "codigo", "cod_linea", "linea_credito"),
                fecha_formalizacion = ValorFecha(valores, 6, "fecha_formalizacion", "fecha_formaliza"),
                garantia = ValorTexto(valores, 7, "garantia"),
                estado = ValorTexto(valores, 8, "estado"),
                tasa = ValorDecimal(valores, 9, "tasa"),
                plazo = ValorEntero(valores, 10, "plazo"),
                mora_cuotas = ValorEntero(valores, 11, "mora_cuotas"),
                mora_intereses = ValorDecimal(valores, 12, "mora_intereses"),
                mora_principal = ValorDecimal(valores, 13, "mora_principal"),
                fecha_termina = ValorFecha(valores, 14, "fecha_termina"),
                bucket = ValorTexto(valores, 15, "bucket"),
                correo = ValorTexto(valores, 16, "correo", "email"),
                telefono = ValorTexto(valores, 17, "telefono"),
                provincia = ValorTexto(valores, 18, "provincia"),
                canton = ValorTexto(valores, 19, "canton"),
                distrito = ValorTexto(valores, 20, "distrito")
            };
        }

        private static object? ValorFila(IDictionary<string, object> fila, int posicion, params string[] alias)
        {
            foreach (string nombre in alias)
            {
                KeyValuePair<string, object> valor = fila.FirstOrDefault(item =>
                    string.Equals(item.Key, nombre, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(valor.Key))
                {
                    return valor.Value is DBNull ? null : valor.Value;
                }
            }

            object? valorPosicional = fila.Values.ElementAtOrDefault(posicion);
            return valorPosicional is DBNull ? null : valorPosicional;
        }

        private static string ValorTexto(IDictionary<string, object> fila, int posicion, params string[] alias)
            => Convert.ToString(ValorFila(fila, posicion, alias), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;

        private static int ValorEntero(IDictionary<string, object> fila, int posicion, params string[] alias)
        {
            object? valor = ValorFila(fila, posicion, alias);
            return valor is null ? 0 : Convert.ToInt32(valor, CultureInfo.InvariantCulture);
        }

        private static decimal ValorDecimal(IDictionary<string, object> fila, int posicion, params string[] alias)
        {
            object? valor = ValorFila(fila, posicion, alias);
            return valor is null ? 0 : Convert.ToDecimal(valor, CultureInfo.InvariantCulture);
        }

        private static DateTime? ValorFecha(IDictionary<string, object> fila, int posicion, params string[] alias)
        {
            object? valor = ValorFila(fila, posicion, alias);
            return valor is null ? null : Convert.ToDateTime(valor, CultureInfo.InvariantCulture);
        }

        private static string PrimerValor(params string[] valores)
            => valores.FirstOrDefault(valor => !string.IsNullOrWhiteSpace(valor))?.Trim() ?? string.Empty;

        private static string NormalizarFiltro(string valor)
        {
            string filtro = valor.Trim().ToUpperInvariant();
            return filtro == "TODOS" ? string.Empty : filtro;
        }

        private static bool CoincideTexto(string valorRegistro, string filtro)
            => string.Equals(valorRegistro.Trim(), filtro, StringComparison.OrdinalIgnoreCase);

        private static bool CoincideEstado(string estadoRegistro, string filtro)
        {
            string estado = estadoRegistro.Trim().ToUpperInvariant();
            bool activo = estado == "A" || estado.StartsWith("ACTIV");
            bool cancelado = estado == "C" || estado.StartsWith("CANCEL");
            bool nulo = estado == "N" || estado.StartsWith("NUL");

            return filtro switch
            {
                "A" => activo,
                "C" => cancelado,
                "N" => nulo,
                "AC" => activo || cancelado,
                _ => true
            };
        }

        private static string? NuloSiVacio(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
