using Dapper;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;
using System.Globalization;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmCxPTrasladoAsientosDB
    {
        private const string SqlDateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        private readonly IConfiguration _config;
        private readonly MProGrXAuxiliarDB DBAuxiliar;
        private readonly MSecurityMainDb DBBitacora;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPTrasladoAsientosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPTrasladoAsientosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            DBAuxiliar = new MProGrXAuxiliarDB(_config);
            DBBitacora = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza un rango de fechas para consultas entre inicio y corte.
        /// </summary>
        /// <param name="inicio">Fecha inicial.</param>
        /// <param name="corte">Fecha final.</param>
        private static void NormalizeRangoFechas(ref string inicio, ref string corte)
        {
            if (DateTime.TryParse(inicio, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateI))
            {
                dateI = dateI.Date.AddHours(0).AddMinutes(0).AddSeconds(0);
                inicio = dateI.ToString(SqlDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
            }

            if (DateTime.TryParse(corte, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateC))
            {
                dateC = dateC.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                corte = dateC.ToString(SqlDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Obtiene la descripción corta del documento para el asiento.
        /// </summary>
        /// <param name="texto">Texto original.</param>
        /// <param name="maximo">Longitud máxima permitida.</param>
        /// <returns>Texto truncado.</returns>
        private static string TruncarTexto(string? texto, int maximo)
        {
            var valor = texto ?? string.Empty;
            return valor.Length > maximo ? valor[..maximo] : valor;
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene los documentos pendientes de traslado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Inicio">Fecha inicial del rango.</param>
        /// <param name="Corte">Fecha final del rango.</param>
        /// <returns>Conteos de documentos pendientes por tipo.</returns>
        public ErrorDto<DocsPendientesTraslado> DocPendientes_Obtener(int CodEmpresa, string Inicio, string Corte)
        {
            NormalizeRangoFechas(ref Inicio, ref Corte);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = new { Inicio, Corte };
                return new DocsPendientesTraslado
                {
                    Facturas_Registradas = connection.QueryFirstOrDefault<int>(
                        @"SELECT count(*)
                          FROM CxP_Facturas
                          WHERE Asiento_Generado = 'P'
                            AND Fecha BETWEEN @Inicio AND @Corte
                            AND dbo.fxCxP_AsientoBalanceado('factura', COD_PROVEEDOR, COD_FACTURA) = 1",
                        parametros),

                    Facturas_Anuladas = connection.QueryFirstOrDefault<int>(
                        @"SELECT count(*)
                          FROM CxP_Facturas
                          WHERE Estado = 'A'
                            AND anula_asiento_fecha IS NULL
                            AND anula_fecha BETWEEN @Inicio AND @Corte
                            AND dbo.fxCxP_AsientoBalanceado('factura', COD_PROVEEDOR, COD_FACTURA) = 1",
                        parametros),

                    Cargos_Flotante_Monto = connection.QueryFirstOrDefault<int>(
                        @"SELECT count(*)
                          FROM cxP_cargosPer C
                          INNER JOIN cxp_cargos T ON C.cod_Cargo = T.cod_Cargo
                          INNER JOIN cxp_proveedores P ON C.cod_Proveedor = P.cod_Proveedor
                          WHERE C.Tipo = 'M'
                            AND C.concepto NOT IN('*** PAGO ANTICIPADO ***')
                            AND C.REGISTRO_FECHA BETWEEN @Inicio AND @Corte
                            AND C.Asiento_Fecha IS NULL",
                        parametros),

                    Cargos_Flotante_Porc = connection.QueryFirstOrDefault<int>(
                        @"SELECT count(*)
                          FROM cxp_PagoProvCargos Car
                          INNER JOIN CXP_CARGOSPER Per ON Car.COD_PROVEEDOR = Per.COD_PROVEEDOR AND Car.ID = Per.ID
                          INNER JOIN cxp_Cargos T ON Car.cod_Cargo = T.cod_Cargo
                          INNER JOIN cxp_proveedores P ON Car.cod_Proveedor = P.cod_Proveedor
                          WHERE Per.TIPO = 'P'
                            AND Car.TIPO_PROCESO = 'F'
                            AND Car.Asiento_Fecha IS NULL
                            AND Car.REGISTRO_FECHA BETWEEN @Inicio AND @Corte",
                        parametros),

                    Cargos_Directos_Factura = connection.QueryFirstOrDefault<int>(
                        @"SELECT count(*)
                          FROM cxp_PagoProvCargos Car
                          INNER JOIN cxp_Cargos T ON Car.cod_Cargo = T.cod_Cargo
                          INNER JOIN cxp_proveedores P ON Car.cod_Proveedor = P.cod_Proveedor
                          WHERE Car.TIPO_PROCESO = 'D'
                            AND Car.Asiento_Fecha IS NULL
                            AND ISNULL(Car.ID, 0) = 0
                            AND Car.REGISTRO_FECHA BETWEEN @Inicio AND @Corte",
                        parametros),

                    Cargos_Flotantes_CobFactCancel_RetCargo = connection.QueryFirstOrDefault<int>(
                        @"SELECT count(*)
                          FROM CXP_PAGOPROV Pg
                          INNER JOIN cxp_PagoProvCargos Car ON Pg.COD_PROVEEDOR = Car.COD_PROVEEDOR AND Pg.COD_FACTURA = Car.COD_FACTURA AND Pg.NPAGO = Car.NPAGO
                          INNER JOIN CXP_ANTICIPOS At ON PG.COD_PROVEEDOR = At.COD_PROVEEDOR AND At.ID_CARGO = Car.ID
                          WHERE Pg.TIPO_CANCELACION = 'C'
                            AND Car.ASIENTO_FECHA IS NULL
                            AND ISNULL(Car.ID, 0) > 0
                            AND Car.REGISTRO_FECHA BETWEEN @Inicio AND @Corte",
                        parametros)
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new DocsPendientesTraslado())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener documentos pendientes de traslado.", result.Code.GetValueOrDefault(-1), new DocsPendientesTraslado());
        }

        /// <summary>
        /// Obtiene los documentos desbalanceados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Inicio">Fecha inicial del rango.</param>
        /// <param name="Corte">Fecha final del rango.</param>
        /// <returns>Listado de documentos desbalanceados.</returns>
        public ErrorDto<List<Desbalanceado>> Desbalanceados_Obtener(int CodEmpresa, string Inicio, string Corte)
        {
            NormalizeRangoFechas(ref Inicio, ref Corte);

            return DbHelper.ExecuteListQuery<Desbalanceado>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT 'Factura' AS Tipo,
                         cod_factura AS Transacccion,
                         creacion_fecha AS Fecha,
                         Creacion_User AS Usuario,
                         Total AS Monto,
                         'Proveedor.: ' + CONVERT(varchar(30), cod_proveedor) AS Referencia,
                         Notas
                  FROM CxP_Facturas
                  WHERE Asiento_Generado = 'P'
                    AND Fecha BETWEEN @Inicio AND @Corte
                    AND dbo.fxCxP_AsientoBalanceado('factura', COD_PROVEEDOR, COD_FACTURA) = 0
                  UNION
                  SELECT 'Factura' AS Tipo,
                         cod_factura AS Transacccion,
                         Anula_fecha AS Fecha,
                         Anula_User AS Usuario,
                         Total AS Monto,
                         'Proveedor.: ' + CONVERT(varchar(30), cod_proveedor) AS Referencia,
                         Notas
                  FROM CxP_Facturas
                  WHERE Estado = 'A'
                    AND anula_asiento_fecha IS NULL
                    AND anula_fecha BETWEEN @Inicio AND @Corte
                    AND dbo.fxCxP_AsientoBalanceado('factura', COD_PROVEEDOR, COD_FACTURA) = 0
                  ORDER BY FECHA",
                new { Inicio, Corte });
        }

        /// <summary>
        /// Reactiva los documentos procesados para revisión de traslado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Inicio">Fecha inicial.</param>
        /// <param name="Corte">Fecha final.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Reactivar(int CodEmpresa, string Inicio, string Corte)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<int>(
                    "[spSys_Asiento_Revisa_Traslado]",
                    new
                    {
                        Inicio,
                        Corte,
                        Auxiliar = "CxP"
                    },
                    commandType: CommandType.StoredProcedure).FirstOrDefault());

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al reactivar documentos procesados.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Valida que el periodo del asiento esté abierto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Fecha">Fecha a validar.</param>
        /// <returns><c>true</c> si el periodo está abierto; en caso contrario <c>false</c>.</returns>
        public bool fxValidaPeriodoAsiento(int CodEmpresa, string Fecha)
        {
            var result = DbHelper.ExecuteListQuery<Periodo>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT *
                  FROM CntX_periodos
                  WHERE anio = YEAR(@Fecha)
                    AND mes = MONTH(@Fecha)
                    AND estado = 'P'
                    AND cod_contabilidad = 1",
                new { Fecha });

            return result.Code == 0 && (result.Result?.Count ?? 0) > 0;
        }

        /// <summary>
        /// Elimina los casos de cargos flotantes con monto cero.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CasosCero_Borrar(int CodEmpresa)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE CXP_PAGOPROVCARGOS WHERE MONTO = 0");

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar casos con monto cero.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region Proceso de traslado

        /// <summary>
        /// Procesa el traslado de asientos en forma individual.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_contabilidad">Código de contabilidad.</param>
        /// <param name="data">Información del traslado.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto AsientoIndividual_Procesar(int CodEmpresa, int cod_contabilidad, AsientoInfo data)
        {
            var resp = new ErrorDto();

            try
            {
                NormalizeAsientoFechas(data);

                var result = DbHelper.WithConn(
                    CreatePortalDb(),
                    CodEmpresa,
                    connection => ProcesarAsientoIndividualConConexion(connection, CodEmpresa, cod_contabilidad, data));

                if (result.Code != 0)
                {
                    return DbHelper.ErrorResponse(result.Description ?? "Error al procesar el traslado de asientos.", result.Code.GetValueOrDefault(-1));
                }

                return result.Result ?? DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                return resp;
            }
        }

        /// <summary>
        /// Ejecuta el proceso de traslado individual usando una conexión activa.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="data">Información del traslado.</param>
        /// <returns>Resultado del procesamiento local.</returns>
        private ErrorDto ProcesarAsientoIndividualConConexion(IDbConnection connection, int codEmpresa, int codContabilidad, AsientoInfo data)
        {
            ErrorDto<ParametroValor> unidadInfo = DBAuxiliar.fxCxPParametro(codEmpresa, "01");
            var codUnidad = unidadInfo?.Result?.Valor ?? string.Empty;
            var info = connection.Query<TrasladoData>(BuildTrasladoQuery(data), new { Inicio = data.Inicio, Corte = data.Corte }).ToList();
            var msjError = new StringBuilder();
            var respuestaLocal = new ErrorDto();

            foreach (TrasladoData inf in info)
            {
                var procesoDetalleContext = new ProcesoDetalleContext
                {
                    Resp = respuestaLocal,
                    MsjError = msjError
                };

                ProcesarTrasladoIndividualItem(connection, codEmpresa, codContabilidad, data, inf, codUnidad, procesoDetalleContext);
            }

            if (msjError.Length > 0)
            {
                respuestaLocal.Code = -1;
                respuestaLocal.Description += msjError.ToString();
            }

            return respuestaLocal;
        }

        /// <summary>
        /// Procesa un documento individual del traslado.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="data">Información del traslado.</param>
        /// <param name="inf">Información del documento.</param>
        /// <param name="codUnidad">Unidad por defecto.</param>
        /// <param name="context">Contexto del proceso.</param>
        private void ProcesarTrasladoIndividualItem(IDbConnection connection, int codEmpresa, int codContabilidad, AsientoInfo data, TrasladoData inf, string codUnidad, ProcesoDetalleContext context)
        {
            if (!fxValidaPeriodoAsiento(codEmpresa, inf.Registro_Fecha))
            {
                context.Resp.Code = -1;
                context.Resp.Description = "Existen asientos que no pueden ser trasladados porque el periodo fue cerrado...";
                return;
            }

            string vNumAsiento = BuildNumAsiento(data, inf);
            if (!TryInsertAsientoCabecera(connection, codContabilidad, data, inf, vNumAsiento, context.Resp, context.MsjError))
            {
                return;
            }

            if (!TryProcesarDetallePorTipo(connection, codContabilidad, data, inf, vNumAsiento, codUnidad, context))
            {
                return;
            }

            if (context.Resp.Code == 0)
            {
                RegistrarBitacoraTraslado(codEmpresa, data);
            }
        }

        /// <summary>
        /// Normaliza las fechas del proceso de traslado.
        /// </summary>
        /// <param name="data">Información del traslado.</param>
        private static void NormalizeAsientoFechas(AsientoInfo data)
        {
            var inicio = data.Inicio;
            var corte = data.Corte;
            NormalizeRangoFechas(ref inicio, ref corte);
            data.Inicio = inicio;
            data.Corte = corte;
        }

        /// <summary>
        /// Construye el número de asiento.
        /// </summary>
        /// <param name="data">Información del traslado.</param>
        /// <param name="inf">Información del documento.</param>
        /// <returns>Número de asiento generado.</returns>
        private static string BuildNumAsiento(AsientoInfo data, TrasladoData inf)
        {
            if (TryBuildFormatoTransaccion(data.vMascara, inf.Cod_Transaccion, out var transaccionFormateada))
            {
                return data.vTipoDoc + "." + $"{inf.Cod_Proveedor:D2}" + "." + transaccionFormateada;
            }

            return data.vTipoDoc + "." + $"{inf.Cod_Proveedor:D2}" + "." + inf.Cod_Transaccion;
        }

        private static bool TryBuildFormatoTransaccion(string? mascara, int codTransaccion, out string transaccionFormateada)
        {
            transaccionFormateada = string.Empty;
            if (string.IsNullOrWhiteSpace(mascara) || !IsMascaraSegura(mascara))
            {
                return false;
            }

            transaccionFormateada = codTransaccion.ToString(mascara, CultureInfo.InvariantCulture);
            return true;
        }

        private static bool IsMascaraSegura(string mascara)
        {
            if (mascara.Length > 20)
            {
                return false;
            }

            foreach (var c in mascara)
            {
                if (c != '0' && c != '#' && c != '.' && c != ',')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Construye la consulta base del tipo de documento a trasladar.
        /// </summary>
        /// <param name="data">Información del traslado.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string BuildTrasladoQuery(AsientoInfo data)
        {
            switch (data.vTipoDoc)
            {
                case "FT":
                    return @"SELECT Ft.*, Ft.cod_factura AS Cod_Transaccion, Ft.Fecha AS Registro_Fecha,
                                    ISNULL(Ft.Creacion_User,'') + CHAR(13) + CHAR(10) + Prov.Descripcion + ' -> ' + Ft.Notas AS AsientoNotas,
                                    CONVERT(varchar(10),Ft.COD_PROVEEDOR) + '..' + Prov.Descripcion AS Referencia,
                                    'Factura No.:' + Ft.cod_factura + '.. Prov:' + CONVERT(varchar(10),Ft.COD_PROVEEDOR) AS AsientoDesc
                             FROM CxP_Facturas Ft
                             INNER JOIN CxP_Proveedores Prov ON Ft.cod_Proveedor = Prov.cod_Proveedor
                             WHERE Ft.Asiento_Generado = 'P'
                               AND Ft.Fecha BETWEEN @Inicio AND @Corte"
                             + (data.chkBalanceados ? " AND dbo.fxCxP_AsientoBalanceado('factura',Ft.COD_PROVEEDOR, Ft.COD_FACTURA) = 1" : string.Empty)
                             + " ORDER BY Creacion_Fecha";

                case "FA":
                    return @"SELECT Ft.*, Ft.cod_factura AS Cod_Transaccion, Ft.anula_fecha AS Registro_Fecha,
                                    ISNULL(Ft.Creacion_User,'') + CHAR(13) + CHAR(10) + Prov.Descripcion + ' -> ' + Ft.Notas AS AsientoNotas,
                                    CONVERT(VARCHAR(10), Ft.COD_PROVEEDOR) + '..' + Prov.Descripcion AS Referencia,
                                    'Factura Anulada No.:' + Ft.cod_factura + '.. Prov:' + CONVERT(VARCHAR(10), Ft.COD_PROVEEDOR) AS AsientoDesc
                             FROM CxP_Facturas Ft
                             INNER JOIN CxP_Proveedores Prov ON Ft.cod_Proveedor = Prov.cod_Proveedor
                             WHERE Ft.Estado = 'A'
                               AND Ft.anula_asiento_fecha IS NULL
                               AND Ft.anula_fecha BETWEEN @Inicio AND @Corte"
                             + (data.chkBalanceados ? " AND dbo.fxCxP_AsientoBalanceado('factura', Ft.COD_PROVEEDOR, Ft.COD_FACTURA) = 1" : string.Empty)
                             + " ORDER BY anula_fecha";

                case "CM":
                    return @"SELECT C.*, CONVERT(VARCHAR(10), [ID]) AS Cod_Transaccion, C.concepto AS Descripcion, C.detalle AS AsientoNotas,
                                    T.descripcion AS Cargo, T.cod_cuenta AS CtaCargo, P.cod_cuenta AS CtaProveedor,
                                    T.descripcion + '.. ID: ' + CONVERT(VARCHAR(10), C.[ID]) + '.. Prov: ' + CONVERT(VARCHAR(10), C.COD_PROVEEDOR) AS AsientoDesc,
                                    CONVERT(VARCHAR(10), P.COD_PROVEEDOR) + '..' + P.Descripcion AS Referencia
                             FROM cxP_cargosPer C
                             INNER JOIN cxp_cargos T ON C.cod_Cargo = T.cod_Cargo
                             INNER JOIN cxp_proveedores P ON C.cod_Proveedor = P.cod_Proveedor
                             WHERE C.Tipo = 'M'
                               AND C.concepto NOT IN ('*** PAGO ANTICIPADO ***')
                               AND C.REGISTRO_FECHA BETWEEN @Inicio AND @Corte
                               AND C.Asiento_Fecha IS NULL
                             ORDER BY C.REGISTRO_FECHA";

                case "CP":
                    return @"SELECT Car.*, CONVERT(VARCHAR(10), Car.[ID]) + '.' + RTRIM(Car.cod_Factura) AS Cod_Transaccion,
                                    Per.concepto AS Descripcion,
                                    CONVERT(VARCHAR(10), P.COD_PROVEEDOR) + '..' + P.Descripcion AS Referencia,
                                    'Cargo de Anticipo/Fact.Cancelada vía Ret. Prov:' + P.descripcion + '  Fact.:' + Car.cod_Factura + ' No.Pago: ' + CONVERT(VARCHAR(30), Pg.NPago) AS AsientoNotas,
                                    T.descripcion + '.. ID: ' + CONVERT(VARCHAR(10), Car.[ID]) + '.. Prov: ' + CONVERT(VARCHAR(10), Car.COD_PROVEEDOR) AS AsientoDesc,
                                    T.descripcion AS Cargo, T.cod_cuenta AS CtaCargo, P.cod_cuenta AS CtaProveedor
                             FROM cxp_PagoProvCargos Car
                             INNER JOIN CXP_CARGOSPER Per ON Car.COD_PROVEEDOR = Per.COD_PROVEEDOR AND Car.ID = Per.ID
                             INNER JOIN cxp_Cargos T ON Car.cod_Cargo = T.cod_Cargo
                             INNER JOIN cxp_proveedores P ON Car.cod_Proveedor = P.cod_Proveedor
                             INNER JOIN CXP_PAGOPROV Pg ON Pg.COD_PROVEEDOR = Car.COD_PROVEEDOR AND Pg.COD_FACTURA = Car.COD_FACTURA AND Pg.NPAGO = Car.NPAGO
                             WHERE Per.TIPO = 'P'
                               AND Car.TIPO_PROCESO = 'F'
                               AND Car.Asiento_Fecha IS NULL
                               AND Car.REGISTRO_FECHA BETWEEN @Inicio AND @Corte
                             ORDER BY Car.REGISTRO_FECHA";

                case "CD":
                    return @"SELECT Car.*, CONVERT(VARCHAR(10), Car.[IDX_Consec]) + '.' + RTRIM(Car.cod_Factura) AS Cod_Transaccion,
                                    'Cargo de Anticipo/Fact.Cancelada vía Ret. Prov:' + P.descripcion + '  Fact.:' + Car.cod_Factura + ' No.Pago: ' + CONVERT(VARCHAR(30), Car.NPago) AS AsientoNotas,
                                    T.descripcion + '.. ID: ' + CONVERT(VARCHAR(10), Car.[ID]) + '.. Prov: ' + CONVERT(VARCHAR(10), Car.COD_PROVEEDOR) AS AsientoDesc,
                                    T.descripcion AS Detalle, T.cod_cuenta AS CtaCargo, P.cod_cuenta AS CtaProveedor,
                                    CONVERT(VARCHAR(10), P.COD_PROVEEDOR) + '..' + P.Descripcion AS Referencia
                             FROM cxp_PagoProvCargos Car
                             INNER JOIN cxp_Cargos T ON Car.cod_Cargo = T.cod_Cargo
                             INNER JOIN cxp_proveedores P ON Car.cod_Proveedor = P.cod_Proveedor
                             WHERE Car.TIPO_PROCESO = 'D'
                               AND Car.Asiento_Fecha IS NULL
                               AND ISNULL(Car.ID, 0) = 0
                               AND Car.REGISTRO_FECHA BETWEEN @Inicio AND @Corte
                             ORDER BY Car.REGISTRO_FECHA";

                case "CA":
                    return @"SELECT Car.*, CONVERT(VARCHAR(10), Car.[ID]) + '.' + RTRIM(Car.cod_Factura) AS Cod_Transaccion,
                                    'Cargo de Anticipo/Fact.Cancelada vía Ret. Prov:' + P.descripcion + '  Fact.:' + Car.cod_Factura + ' No.Pago: ' + CONVERT(VARCHAR(30), Pg.NPago) AS AsientoNotas,
                                    T.descripcion + '.. ID: ' + CONVERT(VARCHAR(10), Car.[ID]) + '.. Prov: ' + CONVERT(VARCHAR(10), Car.COD_PROVEEDOR) AS AsientoDesc,
                                    T.descripcion AS Detalle, T.cod_cuenta AS CtaCargo, P.cod_cuenta AS CtaProveedor,
                                    CONVERT(VARCHAR(10), P.COD_PROVEEDOR) + '..' + P.Descripcion AS Referencia
                             FROM CXP_PAGOPROV Pg
                             INNER JOIN cxp_PagoProvCargos Car ON Pg.COD_PROVEEDOR = Car.COD_PROVEEDOR AND Pg.COD_FACTURA = Car.COD_FACTURA AND Pg.NPAGO = Car.NPAGO AND ISNULL(Car.ID, 0) > 0
                             INNER JOIN cxp_proveedores P ON Car.cod_Proveedor = P.cod_Proveedor
                             INNER JOIN cxp_Cargos T ON Car.cod_Cargo = T.cod_Cargo
                             INNER JOIN CXP_ANTICIPOS At ON PG.COD_PROVEEDOR = At.COD_PROVEEDOR AND At.ID_CARGO = Car.ID
                             WHERE Pg.TIPO_CANCELACION = 'C'
                               AND Car.ASIENTO_FECHA IS NULL
                               AND ISNULL(Car.ID, 0) > 0
                               AND Car.REGISTRO_FECHA BETWEEN @Inicio AND @Corte
                             ORDER BY Car.REGISTRO_FECHA";

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Inserta la cabecera del asiento.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="cod_contabilidad">Código de contabilidad.</param>
        /// <param name="data">Información del traslado.</param>
        /// <param name="inf">Información del documento.</param>
        /// <param name="vNumAsiento">Número de asiento.</param>
        /// <param name="resp">Respuesta acumulada.</param>
        /// <param name="msjError">Mensajes de error acumulados.</param>
        /// <returns><c>true</c> si la inserción fue exitosa; en caso contrario <c>false</c>.</returns>
        private static bool TryInsertAsientoCabecera(IDbConnection connection, int cod_contabilidad, AsientoInfo data, TrasladoData inf, string vNumAsiento, ErrorDto resp, StringBuilder msjError)
        {
            try
            {
                connection.Execute(
                    @"INSERT INTO CntX_Asientos(cod_contabilidad, Tipo_Asiento, Num_Asiento, Anio, Mes, Fecha_Asiento, descripcion, balanceado, modulo, notas, referencia)
                      VALUES (@cod_contabilidad, @TipoAsiento, @NumAsiento, YEAR(@FechaAsiento), MONTH(@FechaAsiento), @FechaAsiento, @Descripcion, 'S', 30, @Notas, @Referencia)",
                    new
                    {
                        cod_contabilidad,
                        TipoAsiento = data.vTipoAsiento,
                        NumAsiento = vNumAsiento,
                        FechaAsiento = inf.Registro_Fecha,
                        Descripcion = TruncarTexto(inf.AsientoDesc?.Trim(), 60),
                        Notas = inf.AsientoNotas,
                        Referencia = TruncarTexto(inf.Referencia, 200)
                    });
                resp.Code = 0;
                return true;
            }
            catch (Exception ex)
            {
                msjError.Append($" | No. Asiento: [{vNumAsiento}] - ERROR: {ex.Message} | ");
                return false;
            }
        }

        /// <summary>
        /// Contexto de proceso para el detalle del asiento.
        /// </summary>
        private sealed class ProcesoDetalleContext
        {
            public ErrorDto Resp { get; set; } = new ErrorDto();
            public StringBuilder MsjError { get; set; } = new StringBuilder();
        }

        /// <summary>
        /// Procesa el detalle del asiento según el tipo de documento.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="cod_contabilidad">Código de contabilidad.</param>
        /// <param name="data">Información del traslado.</param>
        /// <param name="inf">Información del documento.</param>
        /// <param name="vNumAsiento">Número de asiento.</param>
        /// <param name="codUnidad">Unidad por defecto.</param>
        /// <param name="context">Contexto del proceso.</param>
        /// <returns><c>true</c> si el proceso fue exitoso; en caso contrario <c>false</c>.</returns>
        private static bool TryProcesarDetallePorTipo(IDbConnection connection, int cod_contabilidad, AsientoInfo data, TrasladoData inf, string vNumAsiento, string codUnidad, ProcesoDetalleContext context)
        {
            try
            {
                switch (data.vTipoDoc)
                {
                    case "FT":
                        connection.Execute(
                            @"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito,
                                                               detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo)
                              SELECT Asi.COD_CONTABILIDAD, @TipoAsiento, @NumAsiento, Asi.LINEA, Asi.COD_CUENTA,
                                     CASE WHEN Asi.DebeHaber IN ('D') THEN Asi.MONTO ELSE 0 END,
                                     CASE WHEN Asi.DebeHaber NOT IN ('D') THEN Asi.MONTO ELSE 0 END,
                                     'Prov.' + CONVERT(varchar(10), Tra.cod_proveedor) + '.Fact.' + RTRIM(Tra.cod_factura),
                                     ISNULL(Tra.Cod_Factura, ''), Asi.COD_UNIDAD, Asi.COD_DIVISA, Asi.Tipo_Cambio, Asi.COD_CENTRO_COSTO
                              FROM cxp_facturas Tra
                              INNER JOIN cxp_facturas_detalle Asi ON Tra.cod_proveedor = Asi.cod_proveedor AND Tra.cod_factura = Asi.cod_factura
                              WHERE Tra.cod_proveedor = @CodProveedor AND Tra.cod_factura = @CodFactura",
                            new
                            {
                                TipoAsiento = data.vTipoAsiento,
                                NumAsiento = vNumAsiento,
                                CodProveedor = inf.Cod_Proveedor,
                                CodFactura = inf.Cod_Factura
                            });
                        break;

                    case "FA":
                        connection.Execute(
                            @"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito,
                                                               detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo)
                              SELECT Asi.COD_CONTABILIDAD, @TipoAsiento, @NumAsiento, Asi.LINEA, Asi.COD_CUENTA,
                                     CASE WHEN Asi.DebeHaber IN ('D') THEN 0 ELSE Asi.MONTO END,
                                     CASE WHEN Asi.DebeHaber NOT IN ('D') THEN 0 ELSE Asi.MONTO END,
                                     'Prov.' + CONVERT(varchar(10), Tra.cod_proveedor) + '.Fact.' + RTRIM(Tra.cod_factura),
                                     ISNULL(Tra.Cod_Factura, ''), Asi.COD_UNIDAD, Asi.COD_DIVISA, Asi.Tipo_Cambio, Asi.COD_CENTRO_COSTO
                              FROM cxp_facturas Tra
                              INNER JOIN cxp_facturas_detalle Asi ON Tra.cod_proveedor = Asi.cod_proveedor AND Tra.cod_factura = Asi.cod_factura
                              WHERE Tra.cod_proveedor = @CodProveedor AND Tra.cod_factura = @CodFactura",
                            new
                            {
                                TipoAsiento = data.vTipoAsiento,
                                NumAsiento = vNumAsiento,
                                CodProveedor = inf.Cod_Proveedor,
                                CodFactura = inf.Cod_Factura
                            });
                        break;

                    case "CM":
                        connection.Execute(
                            @"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito,
                                                               detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo)
                              VALUES (@CodContabilidad, @TipoAsiento, @NumAsiento, 1, @CuentaProveedor, @Valor, 0, @Detalle, @Documento, @CodUnidad, @CodDivisa, @TipoCambio, '')",
                            new
                            {
                                CodContabilidad = cod_contabilidad,
                                TipoAsiento = data.vTipoAsiento,
                                NumAsiento = vNumAsiento,
                                CuentaProveedor = inf.CtaProveedor,
                                Valor = inf.Valor,
                                Detalle = TruncarTexto(inf.Detalle, 100),
                                Documento = $"{inf.Cod_Proveedor:D2}.{inf.Cod_Transaccion}.{inf.Cod_Cargo?.Trim()}",
                                CodUnidad = codUnidad,
                                CodDivisa = inf.Cod_Divisa,
                                TipoCambio = inf.Tipo_Cambio
                            });
                        break;

                    case "CP":
                    case "CD":
                    case "CA":
                        connection.Execute(
                            @"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito,
                                                               detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo)
                              VALUES (@CodContabilidad, @TipoAsiento, @NumAsiento, 1, @CuentaProveedor, @Monto, 0, @Detalle, @Documento, @CodUnidad, @CodDivisa, @TipoCambio, '')",
                            new
                            {
                                CodContabilidad = cod_contabilidad,
                                TipoAsiento = data.vTipoAsiento,
                                NumAsiento = vNumAsiento,
                                CuentaProveedor = inf.CtaProveedor,
                                Monto = inf.Monto,
                                Detalle = TruncarTexto(inf.Detalle, 100),
                                Documento = $"{inf.Cod_Proveedor:D2}.{inf.Cod_Transaccion}",
                                CodUnidad = codUnidad,
                                CodDivisa = inf.Cod_Divisa,
                                TipoCambio = inf.Tipo_Cambio
                            });
                        break;
                }
            }
            catch (Exception ex)
            {
                context.MsjError.Append($" | No. Asiento: [{vNumAsiento}] - ERROR {data.vTipoDoc}: {ex.Message} | ");
                return false;
            }

            return ApplyTipoDocPostProcess(connection, cod_contabilidad, data, inf, vNumAsiento, codUnidad, context.Resp);
        }

        /// <summary>
        /// Ejecuta el procesamiento posterior según el tipo de documento.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="cod_contabilidad">Código de contabilidad.</param>
        /// <param name="data">Información del traslado.</param>
        /// <param name="inf">Información del documento.</param>
        /// <param name="vNumAsiento">Número de asiento.</param>
        /// <param name="codUnidad">Unidad por defecto.</param>
        /// <param name="resp">Respuesta acumulada.</param>
        /// <returns><c>true</c> si la operación fue exitosa; en caso contrario <c>false</c>.</returns>
        private static bool ApplyTipoDocPostProcess(IDbConnection connection, int cod_contabilidad, AsientoInfo data, TrasladoData inf, string vNumAsiento, string codUnidad, ErrorDto resp)
        {
            switch (data.vTipoDoc)
            {
                case "FT":
                    connection.Execute(
                        @"UPDATE cxp_facturas
                          SET asiento_fecha = GETDATE(), asiento_generado = 'G'
                          WHERE cod_proveedor = @CodProveedor AND cod_factura = @CodFactura",
                        new { CodProveedor = inf.Cod_Proveedor, CodFactura = inf.Cod_Factura });
                    resp.Code = 0;
                    resp.Description = "Factura registrada correctamente";
                    return true;

                case "FA":
                    connection.Execute(
                        @"UPDATE cxp_facturas
                          SET anula_asiento_fecha = GETDATE()
                          WHERE cod_proveedor = @CodProveedor AND cod_factura = @CodFactura",
                        new { CodProveedor = inf.Cod_Proveedor, CodFactura = inf.Cod_Factura });
                    resp.Code = 0;
                    resp.Description = "Factura anulada correctamente";
                    return true;

                case "CM":
                    connection.Execute(
                        @"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito,
                                                           detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo)
                          VALUES (@CodContabilidad, @TipoAsiento, @NumAsiento, 2, @CuentaCargo, 0, @Valor, @Detalle, @Documento, @CodUnidad, @CodDivisa, @TipoCambio, '')",
                        new
                        {
                            CodContabilidad = cod_contabilidad,
                            TipoAsiento = data.vTipoAsiento,
                            NumAsiento = vNumAsiento,
                            CuentaCargo = inf.CtaCargo,
                            Valor = inf.Valor,
                            Detalle = TruncarTexto(inf.Detalle, 100),
                            Documento = $"{inf.Cod_Proveedor:D2}.{inf.Cod_Transaccion}.{inf.Cod_Cargo?.Trim()}",
                            CodUnidad = codUnidad,
                            CodDivisa = inf.Cod_Divisa,
                            TipoCambio = inf.Tipo_Cambio
                        });

                    connection.Execute(
                        @"UPDATE cxP_cargosPer
                          SET asiento_fecha = GETDATE(), asiento_usuario = @Usuario
                          WHERE cod_proveedor = @CodProveedor AND [ID] = @Id AND cod_Cargo = @CodCargo",
                        new
                        {
                            Usuario = data.Usuario,
                            CodProveedor = inf.Cod_Proveedor,
                            Id = inf.Id,
                            CodCargo = inf.Cod_Cargo
                        });
                    resp.Code = 0;
                    resp.Description = "Ok";
                    return true;

                case "CP":
                case "CD":
                case "CA":
                    connection.Execute(
                        @"INSERT INTO CntX_Asientos_detalle(cod_contabilidad, TIPO_ASIENTO, num_asiento, num_linea, cod_cuenta, monto_debito, monto_credito,
                                                           detalle, documento, cod_unidad, cod_divisa, TIPO_Cambio, cod_centro_costo)
                          VALUES (@CodContabilidad, @TipoAsiento, @NumAsiento, 2, @CuentaCargo, 0, @Monto, @Detalle, @Documento, @CodUnidad, @CodDivisa, @TipoCambio, '')",
                        new
                        {
                            CodContabilidad = cod_contabilidad,
                            TipoAsiento = data.vTipoAsiento,
                            NumAsiento = vNumAsiento,
                            CuentaCargo = inf.CtaCargo,
                            Monto = inf.Monto,
                            Detalle = TruncarTexto(inf.Detalle, 100),
                            Documento = $"{inf.Cod_Proveedor:D2}.{inf.Cod_Transaccion}",
                            CodUnidad = codUnidad,
                            CodDivisa = inf.Cod_Divisa,
                            TipoCambio = inf.Tipo_Cambio
                        });

                    connection.Execute(
                        @"UPDATE cxp_PagoProvCargos
                          SET asiento_fecha = GETDATE(), asiento_usuario = @Usuario
                          WHERE cod_proveedor = @CodProveedor AND IDX_Consec = @IdxConsec",
                        new
                        {
                            Usuario = data.Usuario,
                            CodProveedor = inf.Cod_Proveedor,
                            IdxConsec = inf.IdX_Consec
                        });
                    resp.Code = 0;
                    resp.Description = "Ok";
                    return true;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Registra la bitácora del traslado realizado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Información del traslado.</param>
        private void RegistrarBitacoraTraslado(int CodEmpresa, AsientoInfo data)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = data.Usuario,
                DetalleMovimiento = "Traslada Asientos: " + DateTime.Parse(data.Inicio, System.Globalization.CultureInfo.CurrentCulture).ToString("dd/MM/yyyy")
                    + " - " + DateTime.Parse(data.Corte, System.Globalization.CultureInfo.CurrentCulture).ToString("dd/MM/yyyy"),
                Movimiento = "APLICA - WEB",
                Modulo = 30
            });
        }

        #endregion

        #region Bitácora

        /// <summary>
        /// Registra la bitácora.
        /// </summary>
        /// <param name="data">Datos de la bitácora.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        #endregion
    }
}