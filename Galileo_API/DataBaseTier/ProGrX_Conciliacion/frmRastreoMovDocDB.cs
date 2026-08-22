using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Conciliacion;

namespace Galileo_API.DataBaseTier.ProGrX_Conciliacion
{
    public sealed class FrmRastreoMovDocDb
    {
        private const string FiltrosRequeridos =
            "Los filtros de rastreo son requeridos.";

        private const string SqlResumen = """
            select
                count(*) as total,
                isnull(rtrim(COD_CUENTA_MASK), '')
                    as cod_cuenta_mask,
                isnull(rtrim(CUENTA_DESC), '')
                    as cuenta_desc,
                isnull(sum(MONTO_DEBITO), 0)
                    as debito,
                isnull(sum(MONTO_CREDITO), 0)
                    as credito,
                'Control Doc.' as ubicacion,
                isnull(
                    rtrim(convert(varchar(50), cod_unidad)),
                    ''
                ) as cod_unidad,
                isnull(
                    rtrim(convert(varchar(50), cod_Centro_Costo)),
                    ''
                ) as cod_centro_costo,
                isnull(
                    rtrim(convert(varchar(50), Cod_Divisa)),
                    ''
                ) as cod_divisa,
                isnull(avg(TIPO_CAMBIO), 0)
                    as tipo_cambio
            from vSys_Aux_Transacciones_Cuentas
            where Registro_Fecha >= @FechaInicio
              and Registro_Fecha < @FechaFin
              and
              (
                  @MostrarTodasCuentas = 1
                  or cod_cuenta between
                      @CuentaInicio and @CuentaCorte
              )
            group by
                COD_CUENTA_MASK,
                CUENTA_DESC,
                cod_unidad,
                cod_Centro_Costo,
                cod_Divisa
            order by COD_CUENTA_MASK;
            """;

        private const string SqlDetalle = """
            select top (@CantidadLineas)
                D.REGISTRO_FECHA as registro_fecha,
                isnull(
                    rtrim(
                        convert(
                            varchar(50),
                            A.TIPO_DOCUMENTO
                        )
                    ),
                    ''
                ) as tipo_documento,
                isnull(
                    rtrim(
                        convert(
                            varchar(50),
                            A.COD_TRANSACCION
                        )
                    ),
                    ''
                ) as cod_transaccion,
                isnull(
                    rtrim(
                        convert(
                            varchar(100),
                            A.COD_CUENTA
                        )
                    ),
                    ''
                ) as cod_cuenta,
                case
                    when A.TIPO_MOVIMIENTO = 'D'
                    then isnull(A.MONTO, 0)
                    else 0
                end as debito,
                case
                    when A.TIPO_MOVIMIENTO = 'D'
                    then 0
                    else isnull(A.MONTO, 0)
                end as credito,
                isnull(rtrim(CON.DESCRIPCION), '')
                    as concepto_desc,
                rtrim(
                    isnull(D.CLIENTE_IDENTIFICACION, '')
                )
                    + ' - '
                    + isnull(rtrim(D.CLIENTE_NOMBRE), '')
                    as cliente,
                isnull(
                    rtrim(
                        convert(
                            varchar(100),
                            D.DOCUMENTO
                        )
                    ),
                    ''
                ) as documento,
                isnull(rtrim(D.REGISTRO_USUARIO), '')
                    as registro_usuario,
                isnull(
                    rtrim(
                        convert(
                            varchar(50),
                            A.COD_UNIDAD
                        )
                    ),
                    ''
                ) as cod_unidad,
                isnull(
                    rtrim(
                        convert(
                            varchar(50),
                            A.COD_CENTRO_COSTO
                        )
                    ),
                    ''
                ) as cod_centro_costo,
                isnull(
                    rtrim(
                        convert(
                            varchar(50),
                            A.COD_DIVISA
                        )
                    ),
                    ''
                ) as cod_divisa,
                cast(1 as decimal(18, 6))
                    as tipo_cambio,
                isnull(
                    rtrim(
                        convert(
                            varchar(50),
                            D.COD_OFICINA
                        )
                    ),
                    ''
                ) as cod_oficina,
                isnull(
                    rtrim(
                        convert(
                            varchar(250),
                            A.REFERENCIA_01
                        )
                    ),
                    ''
                ) as referencia_01,
                isnull(
                    rtrim(
                        convert(
                            varchar(250),
                            A.REFERENCIA_02
                        )
                    ),
                    ''
                ) as referencia_02,
                isnull(
                    rtrim(
                        convert(
                            varchar(250),
                            A.REFERENCIA_03
                        )
                    ),
                    ''
                ) as referencia_03
            from SIF_TRANSACCIONES D
            inner join SIF_TRANSACCIONES_ASIENTO A
                on D.TIPO_DOCUMENTO = A.TIPO_DOCUMENTO
               and D.COD_TRANSACCION = A.COD_TRANSACCION
            inner join SIF_CONCEPTOS CON
                on D.COD_CONCEPTO = CON.COD_CONCEPTO
            where D.REGISTRO_FECHA >= @FechaInicio
              and D.REGISTRO_FECHA < @FechaFin
              and
              (
                  @MostrarTodasCuentas = 1
                  or A.COD_CUENTA between
                      @CuentaInicio and @CuentaCorte
              )
            order by D.REGISTRO_FECHA;
            """;

        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _cntLinkDb;

        public FrmRastreoMovDocDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _cntLinkDb = new MCntLinkDB(config);
        }

        /// <summary>
        /// Obtiene la fecha actual del servidor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<RastreoMovDocInicializaData>
            Conciliacion_RastreoMovDoc_Inicializar(
                int codEmpresa)
        {
            ErrorDto<DateTime?> resultado =
                DbHelper.ExecuteSingleQuery<DateTime?>(
                    _portalDb,
                    codEmpresa,
                    "select getdate();",
                    null);

            if (resultado.Code == -1)
            {
                return DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    "No fue posible obtener la fecha del servidor.",
                    -1,
                    new RastreoMovDocInicializaData());
            }

            return DbHelper.CreateOkResponse(
                new RastreoMovDocInicializaData
                {
                    fecha_servidor = resultado.Result
                });
        }

        /// <summary>
        /// Obtiene el resumen contable de movimientos por cuenta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<RastreoMovDocResumenData>>
            Conciliacion_RastreoMovDoc_Resumen_Obtener(
                int codEmpresa,
                RastreoMovDocConsultaRequest? request)
        {
            (
                RastreoMovDocFiltrosNormalizados? filtros,
                string? mensaje
            ) = Conciliacion_RastreoMovDoc_Filtros_Normalizar(
                codEmpresa,
                request);

            if (mensaje is not null)
            {
                return CrearErrorLista<RastreoMovDocResumenData>(
                    mensaje);
            }

            if (filtros is null)
            {
                return CrearErrorLista<RastreoMovDocResumenData>(
                    FiltrosRequeridos);
            }

            return DbHelper.ExecuteListQuery<RastreoMovDocResumenData>(
                _portalDb,
                codEmpresa,
                SqlResumen,
                Conciliacion_RastreoMovDoc_Parametros_Crear(
                    filtros));
        }

        /// <summary>
        /// Obtiene el detalle contable de movimientos por documento.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<RastreoMovDocDetalleData>>
            Conciliacion_RastreoMovDoc_Detalle_Obtener(
                int codEmpresa,
                RastreoMovDocConsultaRequest? request)
        {
            (
                RastreoMovDocFiltrosNormalizados? filtros,
                string? mensaje
            ) = Conciliacion_RastreoMovDoc_Filtros_Normalizar(
                codEmpresa,
                request);

            if (mensaje is not null)
            {
                return CrearErrorLista<RastreoMovDocDetalleData>(
                    mensaje);
            }

            if (filtros is null)
            {
                return CrearErrorLista<RastreoMovDocDetalleData>(
                    FiltrosRequeridos);
            }

            return DbHelper.ExecuteListQuery<RastreoMovDocDetalleData>(
                _portalDb,
                codEmpresa,
                SqlDetalle,
                Conciliacion_RastreoMovDoc_Parametros_Crear(
                    filtros));
        }

        /// <summary>
        /// Valida y normaliza defensivamente los filtros recibidos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private (
            RastreoMovDocFiltrosNormalizados? Filtros,
            string? Mensaje
        ) Conciliacion_RastreoMovDoc_Filtros_Normalizar(
            int codEmpresa,
            RastreoMovDocConsultaRequest? request)
        {
            if (
                request is null ||
                !request.fecha_inicio.HasValue ||
                !request.fecha_corte.HasValue
            )
            {
                return (
                    null,
                    "Debe indicar la fecha inicial y la fecha corte."
                );
            }

            DateTime fechaInicio =
                request.fecha_inicio.Value.Date;

            DateTime fechaCorte =
                request.fecha_corte.Value.Date;

            if (fechaInicio > fechaCorte)
            {
                return (
                    null,
                    "La fecha inicial no puede ser mayor "
                    + "que la fecha corte."
                );
            }

            if (request.cantidad_lineas <= 0)
            {
                return (
                    null,
                    "La cantidad de l&iacute;neas no es v&aacute;lida."
                );
            }

            var filtros = new RastreoMovDocFiltrosNormalizados
            {
                FechaInicio = fechaInicio,
                FechaFin =
                    Conciliacion_RastreoMovDoc_FechaFin_Obtener(
                        fechaCorte),
                CantidadLineas = Math.Clamp(
                    request.cantidad_lineas,
                    1,
                    100000),
                MostrarTodasCuentas =
                    request.mostrar_todas_cuentas
            };

            if (request.mostrar_todas_cuentas)
            {
                return (filtros, null);
            }

            string cuentaInicio =
                Conciliacion_RastreoMovDoc_Cuenta_Normalizar(
                    codEmpresa,
                    request.cuenta_inicio);

            string cuentaCorte =
                Conciliacion_RastreoMovDoc_Cuenta_Normalizar(
                    codEmpresa,
                    request.cuenta_corte);

            if (
                cuentaInicio == string.Empty ||
                cuentaCorte == string.Empty
            )
            {
                return (
                    null,
                    "Debe indicar una cuenta inicial y "
                    + "una cuenta corte v&aacute;lidas."
                );
            }

            if (
                string.CompareOrdinal(
                    cuentaInicio,
                    cuentaCorte) > 0
            )
            {
                return (
                    null,
                    "La cuenta inicial no puede ser mayor "
                    + "que la cuenta corte."
                );
            }

            filtros.CuentaInicio = cuentaInicio;
            filtros.CuentaCorte = cuentaCorte;

            return (filtros, null);
        }

        /// <summary>
        /// Normaliza una cuenta utilizando la configuración contable
        /// de la empresa, igual que fxgCntCuentaFormato del VB6.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        private string
            Conciliacion_RastreoMovDoc_Cuenta_Normalizar(
                int codEmpresa,
                string? cuenta)
        {
            string valor = cuenta?.Trim() ?? string.Empty;

            if (valor == string.Empty)
            {
                return string.Empty;
            }

            bool formatoValido =
                valor.All(
                    caracter =>
                        char.IsDigit(caracter) ||
                        caracter == '-' ||
                        char.IsWhiteSpace(caracter));

            if (!formatoValido)
            {
                return string.Empty;
            }

            string cuentaNormalizada =
                _cntLinkDb.fxgCntCuentaFormato(
                    codEmpresa,
                    false,
                    valor,
                    0);

            return cuentaNormalizada.All(char.IsDigit)
                ? cuentaNormalizada
                : string.Empty;
        }

        /// <summary>
        /// Construye los parámetros utilizados por las consultas.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static object
            Conciliacion_RastreoMovDoc_Parametros_Crear(
                RastreoMovDocFiltrosNormalizados filtros)
        {
            return new
            {
                filtros.FechaInicio,
                filtros.FechaFin,
                filtros.CantidadLineas,
                filtros.MostrarTodasCuentas,
                filtros.CuentaInicio,
                filtros.CuentaCorte
            };
        }

        /// <summary>
        /// Obtiene la fecha final exclusiva para consultar el día
        /// de corte completo.
        /// </summary>
        /// <param name="fechaCorte"></param>
        /// <returns></returns>
        private static DateTime
            Conciliacion_RastreoMovDoc_FechaFin_Obtener(
                DateTime fechaCorte)
        {
            return fechaCorte.Date == DateTime.MaxValue.Date
                ? DateTime.MaxValue
                : fechaCorte.Date.AddDays(1);
        }

        private static ErrorDto<List<T>>
            CrearErrorLista<T>(
                string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new List<T>());
        }

        private sealed class RastreoMovDocFiltrosNormalizados
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public int CantidadLineas { get; set; } = 1000;
            public bool MostrarTodasCuentas { get; set; } = true;
            public string CuentaInicio { get; set; } =
                string.Empty;
            public string CuentaCorte { get; set; } =
                string.Empty;
        }
    }
}