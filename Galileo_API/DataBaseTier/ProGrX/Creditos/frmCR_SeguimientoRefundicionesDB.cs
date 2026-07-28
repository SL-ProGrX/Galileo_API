using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSeguimientoRefundicionesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCobroDb _mCobroDb;

        private const string TipoCancelaCredito = "C";
        private const string TipoMorosidad = "M";
        private const string TipoPendientes = "P";

        private const string MensajeValidacion = "Validación.";
        private const string MensajeOperacionRequerida = "La operación es requerida.";
        private const string MensajeCedulaRequerida = "La cédula es requerida.";
        private const string MensajeCodigoRequerido = "El código es requerido.";

        public FrmCrSeguimientoRefundicionesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mCobroDb = new MCobroDb(config);
        }

        /// <summary>
        /// Inicializa la pantalla: calcula disponible, carga refundiciones registradas y créditos del socio.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoRefundicionesInicializarDto> CR_SeguimientoRefundiciones_Inicializar(int CodEmpresa,CrSeguimientoRefundicionesInicializarRequest request)
        {
            var validation = ValidarInicializar(request);
            if (validation.Code != 0)
            {
                return ErrorInicializar(
                    validation.Description ?? MensajeValidacion,
                    -2);
            }

            if (request.operacion is not long operacion)
                return ErrorInicializar(MensajeOperacionRequerida, -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var baseData = ObtenerOperacionBase(conn, operacion);
                if (baseData == null)
                    return ErrorInicializar(
                        "No se encontró la operación indicada.",
                        -2);

                baseData.fecha_desembolso = request.fecha_desembolso;
                baseData.pri_deduc = request.pri_deduc;
                baseData.dia_pago = request.dia_pago;

                var montos = CalcularMontosIniciales(
                    CodEmpresa,
                    operacion,
                    baseData);

                var disponible = CalcularDisponible(
                    CodEmpresa,
                    operacion,
                    baseData,
                    montos);

                return DbHelper.CreateOkResponse(
                    new CrSeguimientoRefundicionesInicializarDto
                    {
                        cedula = baseData.cedula,
                        codigo = baseData.codigo,
                        disponible = disponible,
                        primer_cuota = montos.PrimerCuota,
                        poliza = montos.Poliza,
                        interes = montos.Interes,
                        refundiciones = ObtenerRefundicionesLista(
                            conn,
                            operacion,
                            new FiltrosLazyLoadData()),
                        prestamos = ObtenerPrestamosSocioLista(
                            conn,
                            CrearPrestamosRequest(baseData, operacion),
                            new FiltrosLazyLoadData())
                    });
            }
            catch (SqlException ex)
            {
                return ErrorInicializar(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de refundiciones registradas.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoRefundicionesListaDto>CR_SeguimientoRefundiciones_Lista_Obtener(int CodEmpresa, CrSeguimientoRefundicionesListaRequest request)
        {
            var validation = ValidarListaRefundiciones(request);
            if (validation.Code != 0)
            {
                return ErrorListaRefundiciones(
                    validation.Description ?? MensajeValidacion,
                    -2);
            }

            if (request.operacion is not long operacion)
                return ErrorListaRefundiciones(MensajeOperacionRequerida, -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var filtros = ParseFiltros(request.filtros);

                return DbHelper.CreateOkResponse(
                    ObtenerRefundicionesLista(
                        conn,
                        operacion,
                        filtros));
            }
            catch (SqlException ex)
            {
                return ErrorListaRefundiciones(ex.Message);
            }
            catch (JsonException ex)
            {
                return ErrorListaRefundiciones(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de refundiciones para exportar.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoRefundicionesListaDto> CR_SeguimientoRefundiciones_Lista_Exportar(
            int CodEmpresa,
            CrSeguimientoRefundicionesListaRequest request)
        {
            request.filtros = ForzarExportarTodo(request.filtros);
            return CR_SeguimientoRefundiciones_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista paginada de créditos pendientes del socio.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Prestamos_Obtener(
            int CodEmpresa,
            CrSeguimientoRefundicionesPrestamosRequest request)
        {
            var validation = ValidarPrestamos(request);
            if (validation.Code != 0)
                return ErrorListaCreditos(validation.Description ?? MensajeValidacion, -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var filtros = ParseFiltros(request.filtros);
                return DbHelper.CreateOkResponse(ObtenerPrestamosSocioLista(conn, request, filtros));
            }
            catch (SqlException ex)
            {
                return ErrorListaCreditos(ex.Message);
            }
            catch (JsonException ex)
            {
                return ErrorListaCreditos(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de créditos pendientes del socio para exportar.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Prestamos_Exportar(
            int CodEmpresa,
            CrSeguimientoRefundicionesPrestamosRequest request)
        {
            request.filtros = ForzarExportarTodo(request.filtros);
            return CR_SeguimientoRefundiciones_Prestamos_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista paginada de créditos de terceros por cédula.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Terceros_Obtener(
            int CodEmpresa,
            CrSeguimientoRefundicionesConsultaTercerosRequest request)
        {
            var validation = ValidarTerceros(request);
            if (validation.Code != 0)
                return ErrorListaCreditos(validation.Description ?? MensajeValidacion, -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var filtros = ParseFiltros(request.filtros);
                return DbHelper.CreateOkResponse(ObtenerTercerosLista(conn, request, filtros));
            }
            catch (SqlException ex)
            {
                return ErrorListaCreditos(ex.Message);
            }
            catch (JsonException ex)
            {
                return ErrorListaCreditos(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de créditos de terceros para exportar.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Terceros_Exportar(
            int CodEmpresa,
            CrSeguimientoRefundicionesConsultaTercerosRequest request)
        {
            request.filtros = ForzarExportarTodo(request.filtros);
            return CR_SeguimientoRefundiciones_Terceros_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Recalcula los datos de una refundición según tipo C/M/P.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoRefundicionDatosDto> CR_SeguimientoRefundiciones_Refunde_Datos(
            int CodEmpresa,
            CrSeguimientoRefundicionesRefundeDatosRequest request)
        {
            var validation = ValidarRefundeDatos(request);
            if (validation.Code != 0)
                return DbHelper.CreateErrorResponse(
                    validation.Description ?? MensajeValidacion,
                    -2,
                    new CrSeguimientoRefundicionDatosDto());

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var data = ObtenerRefundeDatos(conn, request);
                if (data == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontraron datos para la operación.",
                        -2,
                        new CrSeguimientoRefundicionDatosDto());
                }

                data.tipo = ResolverTipo(request.tipo);
                data.total = CalcularTotal(data);

                return DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrSeguimientoRefundicionDatosDto());
            }
        }

        /// <summary>
        /// Inserta una refundición en la tabla refundiciones.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CR_SeguimientoRefundiciones_Guardar(int CodEmpresa,CrSeguimientoRefundicionGuardarRequest request)
        {
            var validation = ValidarGuardar(request);
            if (validation.Code != 0)
                return validation;

            var data = NormalizarGuardar(request);

            if (data.operacion_refunde is not long operacionRefunde)
                return DbHelper.ErrorResponse("No se ha seleccionado ninguna operación.");

            if (data.operacion_nueva is not long operacionNueva)
                return DbHelper.ErrorResponse("La operación nueva es requerida.");

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var total = data.total ?? 0m;
                var disponible = data.disponible ?? 0m;

                if (ExisteRefundicion(conn, operacionRefunde, operacionNueva))
                {
                    return DbHelper.ErrorResponse(
                        "Esta Refundición Se encuentra Registrada VERIFIQUE...");
                }

                if (total > disponible)
                {
                    return DbHelper.ErrorResponse(
                        "El monto a refundir de la operación es mayor al disponible...");
                }

                InsertarRefundicion(conn, data);

                return DbHelper.OkResponse(
                    "Refundición registrada correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una o varias refundiciones por operación nueva.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CR_SeguimientoRefundiciones_Eliminar(int CodEmpresa,CrSeguimientoRefundicionesEliminarRequest request)
        {
            var validation = ValidarEliminar(request);
            if (validation.Code != 0)
                return validation;

            if (request.operacion_nueva is not long operacionNueva)
                return DbHelper.ErrorResponse(MensajeOperacionRequerida);

            var operaciones = request.operaciones_refunde
                .Where(x => x.HasValue && x.Value > 0)
                .Select(x => x.GetValueOrDefault())
                .ToList();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
            delete refundiciones
            where id_solicitudr = @operacionNueva
              and id_solicitud in @operaciones;";

                var rows = conn.Execute(sql, new
                {
                    operacionNueva,
                    operaciones
                });

                if (rows <= 0)
                    return DbHelper.ErrorResponse("No se eliminó ninguna refundición.");

                return DbHelper.OkResponse("Refundición eliminada correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza/refresca montos y estado de operaciones a refinanciar/abonar.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CR_SeguimientoRefundiciones_Actualizar(
            int CodEmpresa,
            CrSeguimientoRefundicionesActualizarRequest request)
        {
            if (request == null || !request.operacion.HasValue || request.operacion.Value <= 0)
                return DbHelper.ErrorResponse(MensajeOperacionRequerida);

            var operacion = request.operacion.Value;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"exec spCrdSGTRefundicionesActualiza @Operacion;";
                conn.Execute(sql, new { Operacion = operacion });

                return DbHelper.OkResponse("Estado de las Operaciones a Refinanciar o Abonar actualizado!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene datos base de la operación nueva usados para calcular disponible.
        /// <param name="conn"></param>
        /// <param name="operacion"></param>
        /// </summary>
        /// <returns></returns>
        private static CrSeguimientoRefundicionesOperacionBaseDto? ObtenerOperacionBase(SqlConnection conn,long operacion)
        {
            const string sql = @"
        select
            rtrim(isnull(R.Primer_Cuota,'')) as primer_cuota,
            rtrim(isnull(R.Garantia,'')) as garantia,
            R.montoapr,
            R.cuota,
            R.int as int_credito,
            rtrim(isnull(C.convenio,'')) as convenio,
            rtrim(isnull(R.cod_destino,'')) as cod_destino,
            rtrim(isnull(R.cedula,'')) as cedula,
            rtrim(isnull(R.codigo,'')) as codigo
        from reg_creditos R
        inner join Catalogo C
                on R.codigo = C.codigo
        where R.id_solicitud = @operacion;";

            return conn.QueryFirstOrDefault<CrSeguimientoRefundicionesOperacionBaseDto>(
                sql,
                new { operacion });
        }

        /// <summary>
        /// Obtiene refundiciones registradas usando el SP legacy.
        /// <param name="conn"></param>
        /// <param name="operacion"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        private static CrSeguimientoRefundicionesListaDto ObtenerRefundicionesLista(
            SqlConnection conn,
            long operacion,
            FiltrosLazyLoadData filtros)
        {
            var lista = ObtenerRefundiciones(conn, operacion);
            var filtrada = FiltrarRefundiciones(lista, filtros.filtro);
            var ordenada = OrdenarRefundiciones(filtrada, filtros);

            return new CrSeguimientoRefundicionesListaDto
            {
                total = ordenada.Count,
                lista = ordenada
            };
        }

        /// <summary>
        /// Obtiene créditos pendientes del socio usando el SP legacy.
        /// <param name="conn"></param>
        /// <param name="request"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        private static CrSeguimientoRefundicionesCreditosListaDto ObtenerPrestamosSocioLista(
            SqlConnection conn,
            CrSeguimientoRefundicionesPrestamosRequest request,
            FiltrosLazyLoadData filtros)
        {
            var lista = ObtenerPrestamosSocio(conn, request);
            return AplicarFiltrosCreditos(lista, filtros);
        }

        /// <summary>
        /// Obtiene créditos de tercero usando el SP legacy.
        /// <param name="conn"></param>
        /// <param name="request"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        private static CrSeguimientoRefundicionesCreditosListaDto ObtenerTercerosLista(
            SqlConnection conn,
            CrSeguimientoRefundicionesConsultaTercerosRequest request,
            FiltrosLazyLoadData filtros)
        {
            var lista = ObtenerPrestamosTercero(conn, request);
            return AplicarFiltrosCreditos(lista, filtros);
        }

        /// <summary>
        /// Carga refundiciones registradas.
        /// <param name="conn"></param>
        /// <param name="operacion"></param>
        /// </summary>
        /// <returns></returns>
        private static List<CrSeguimientoRefundicionData> ObtenerRefundiciones(
            SqlConnection conn,
            long operacion)
        {
            const string sql = @"exec spCrd_SGT_Refundiciones_Lista @Operacion;";

            var lista = conn.Query<CrSeguimientoRefundicionData>(
                    sql,
                    new { Operacion = operacion })
                .ToList();

            lista.ForEach(x => x.tipo_desc = TipoDescripcion(x.tipo));

            return lista;
        }

        /// <summary>
        /// Carga créditos pendientes del socio.
        /// <param name="conn"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static List<CrSeguimientoRefundicionCreditoData> ObtenerPrestamosSocio(SqlConnection conn,CrSeguimientoRefundicionesPrestamosRequest request)
        {
            if (request.operacion is not long operacion)
            {
                throw new InvalidOperationException(
                    "La operación es requerida para consultar los préstamos del socio.");
            }

            const string sql = @"
        exec spCrd_SGT_Persona_Creditos_Pendientes_Lista
             @Operacion,
             @Cedula,
             'N',
             'S',
             @Codigo;";

            return ObtenerCreditos(conn, sql, new
            {
                Operacion = operacion,
                Cedula = Clean(request.cedula),
                Codigo = Clean(request.codigo)
            });
        }

        /// <summary>
        /// Carga créditos de tercero.
        /// <param name="conn"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static List<CrSeguimientoRefundicionCreditoData> ObtenerPrestamosTercero(
            SqlConnection conn,
            CrSeguimientoRefundicionesConsultaTercerosRequest request)
        {
            const string sql = @"
                exec spCrdSGTListaCreditosPersona
                     @Cedula,
                     'N',
                     'S',
                     @Codigo;";

            return ObtenerCreditos(conn, sql, new
            {
                Cedula = Clean(request.cedula),
                Codigo = Clean(request.codigo)
            });
        }

        /// <summary>
        /// Ejecuta consulta de créditos y normaliza descripción/totales.
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parametros"></param>
        /// </summary>
        /// <returns></returns>
        private static List<CrSeguimientoRefundicionCreditoData> ObtenerCreditos(
            SqlConnection conn,
            string sql,
            object parametros)
        {
            var lista = conn.Query<CrSeguimientoRefundicionCreditoData>(
                    sql,
                    parametros)
                .ToList();

            foreach (var item in lista)
            {
                item.total ??= CalcularTotalCredito(item);
                item.tipo_desc = TipoDescripcion(item.tipo);
            }

            return lista;
        }

        /// <summary>
        /// Aplica filtros, orden y paginación a créditos.
        /// <param name="lista"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        private static CrSeguimientoRefundicionesCreditosListaDto AplicarFiltrosCreditos(List<CrSeguimientoRefundicionCreditoData> lista, FiltrosLazyLoadData filtros)
        {
            var filtrada = FiltrarCreditos(lista, filtros.filtro);
            var ordenada = OrdenarCreditos(filtrada, filtros);

            return new CrSeguimientoRefundicionesCreditosListaDto
            {
                total = ordenada.Count,
                lista = ordenada
            };
        }

        /// <summary>
        /// Recalcula datos por tipo de aplicación C/M/P.
        /// <param name="conn"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static CrSeguimientoRefundicionDatosDto? ObtenerRefundeDatos(SqlConnection conn,CrSeguimientoRefundicionesRefundeDatosRequest request)
        {
            if (request.operacion is not long operacion)
            {
                throw new InvalidOperationException(
                    "La operación es requerida para consultar la refundición.");
            }

            const string sql = @"exec spCrd_SGT_Refunde_Datos @Operacion, @Tipo;";

            return conn.QueryFirstOrDefault<CrSeguimientoRefundicionDatosDto>(
                sql,
                new
                {
                    Operacion = operacion,
                    Tipo = ResolverTipo(request.tipo)
                });
        }

        /// <summary>
        /// Valida si la refundición ya existe.
        /// <param name="conn"></param>
        /// <param name="operacionRefunde"></param>
        /// <param name="operacionNueva"></param>
        /// </summary>
        /// <returns></returns>
        private static bool ExisteRefundicion(
            SqlConnection conn,
            long operacionRefunde,
            long operacionNueva)
        {
            const string sql = @"
                select isnull(count(*),0)
                from refundiciones
                where id_solicitud = @operacionRefunde
                  and id_solicitudr = @operacionNueva;";

            var existe = conn.QueryFirstOrDefault<int>(
                sql,
                new
                {
                    operacionRefunde,
                    operacionNueva
                });

            return existe > 0;
        }

        /// <summary>
        /// Inserta refundición con la misma estructura del VB6.
        /// <param name="conn"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static void InsertarRefundicion(
            SqlConnection conn,
            CrSeguimientoRefundicionGuardarRequest request)
        {
            const string sql = @"
                insert refundiciones
                (
                    id_solicitud,
                    codigo,
                    monto,
                    fecha,
                    codigor,
                    id_solicitudr,
                    intcor,
                    intmor,
                    saldo_anterior,
                    cargos,
                    polizas,
                    principal,
                    tipo,
                    IVA
                )
                values
                (
                    @operacion_refunde,
                    @codigo_refunde,
                    @total,
                    dbo.MyGetdate(),
                    @codigo_nuevo,
                    @operacion_nueva,
                    @intcor,
                    @intmor,
                    @saldo,
                    @cargos,
                    @polizas,
                    @principal,
                    @tipo,
                    @iva
                );";

            conn.Execute(sql, request);
        }

        /// <summary>
        /// Calcula montos iniciales de primer cuota, póliza e interés.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="baseData"></param>
        /// </summary>
        /// <returns></returns>
        private MontosIniciales CalcularMontosIniciales(int CodEmpresa,long operacion,CrSeguimientoRefundicionesOperacionBaseDto baseData)
        {
            var interes = CalcularInteresFormalizacion(CodEmpresa,operacion,baseData);

            var primerCuota = CalcularPrimerCuota(baseData);

            interes = AjustarInteresPrimerCuota(
                baseData,
                interes);

            var poliza = CalcularPoliza(
                CodEmpresa,
                baseData);

            if (_mCobroDb.fxCreditoExcedente(
                    CodEmpresa,
                    baseData.codigo))
            {
                interes = 0;
            }

            return new MontosIniciales
            {
                Interes = interes,
                PrimerCuota = primerCuota,
                Poliza = poliza
            };
        }

        /// <summary>
        /// Calcula el monto disponible de la operación.
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="baseData"></param>
        /// <param name="montos"></param>
        /// </summary>
        /// <returns></returns>
        private decimal CalcularDisponible(
    int CodEmpresa,
    long operacion,
    CrSeguimientoRefundicionesOperacionBaseDto baseData,
    MontosIniciales montos)
        {
            return (baseData.montoapr ?? 0)
                - (_mCobroDb.fxMontoEnGeneral(CodEmpresa, operacion)
                    + montos.Interes
                    + montos.PrimerCuota
                    + montos.Poliza);
        }

        /// <summary>
        /// Calcula interés hasta formalizar cuando aplica.
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="baseData"></param>
        /// </summary>
        /// <returns></returns>
        private decimal CalcularInteresFormalizacion(int CodEmpresa,long operacion,CrSeguimientoRefundicionesOperacionBaseDto baseData)
        {
            if (!_mCobroDb.fxCobraTasaFormaliza(
                    CodEmpresa,
                    baseData.codigo,
                    baseData.cod_destino))
            {
                return 0m;
            }

            if (baseData.fecha_desembolso is not DateTime fechaDesembolso)
            {
                throw new InvalidOperationException(
                    "La fecha de desembolso es requerida para calcular los intereses.");
            }

            if (baseData.pri_deduc is not decimal primeraDeduccion)
            {
                throw new InvalidOperationException(
                    "La primera deducción es requerida para calcular los intereses.");
            }

            if (baseData.dia_pago is not int diaPago)
            {
                throw new InvalidOperationException(
                    "El día de pago es requerido para calcular los intereses.");
            }

            return _mCobroDb.fxInteresesHastaFormalizar(
                CodEmpresa,
                operacion,
                baseData.codigo,
                fechaDesembolso,
                null,
                primeraDeduccion,
                diaPago);
        }

        /// <summary>
        /// Obtiene primer cuota según campo PRIMER_CUOTA.
        /// <param name="baseData"></param>
        /// </summary>
        /// <returns></returns>
        private static decimal CalcularPrimerCuota(
            CrSeguimientoRefundicionesOperacionBaseDto baseData)
        {
            if (!Clean(baseData.primer_cuota).Equals("S", StringComparison.OrdinalIgnoreCase))
                return 0m;

            return baseData.cuota ?? 0m;
        }

        /// <summary>
        /// Recalcula interés cuando existe primer cuota e interés formalización.
        /// <param name="request"></param>
        /// <param name="baseData"></param>
        /// <param name="interes"></param>
        /// </summary>
        /// <returns></returns>
        private static decimal AjustarInteresPrimerCuota(CrSeguimientoRefundicionesOperacionBaseDto baseData,decimal interes)
        {
            if (interes <= 0)
                return interes;

            if (!Clean(baseData.primer_cuota).Equals("S", StringComparison.OrdinalIgnoreCase))
                return interes;

            return MCobroDb.fxInteresesDiasPrimerCuota(
                baseData.fecha_desembolso ?? DateTime.Today,
                baseData.montoapr ?? 0,
                baseData.int_credito ?? 0);
        }

        /// <summary>
        /// Calcula póliza vida según garantía/convenio.
        /// <param name="CodEmpresa"></param>
        /// <param name="baseData"></param>
        /// </summary>
        /// <returns></returns>
        private decimal CalcularPoliza(
            int CodEmpresa,
            CrSeguimientoRefundicionesOperacionBaseDto baseData)
        {
            if (Clean(baseData.garantia).Equals("H", StringComparison.OrdinalIgnoreCase))
                return 0m;

            if (!Clean(baseData.convenio).Equals("N", StringComparison.OrdinalIgnoreCase))
                return 0m;

            return _mCobroDb.fxCuotaPolizaVida(
                CodEmpresa,
                baseData.montoapr ?? 0m);
        }

        /// <summary>
        /// Normaliza request antes de guardar.
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static CrSeguimientoRefundicionGuardarRequest NormalizarGuardar(
            CrSeguimientoRefundicionGuardarRequest request)
        {
            request.codigo_refunde = Clean(request.codigo_refunde);
            request.codigo_nuevo = Clean(request.codigo_nuevo);
            request.tipo = ResolverTipo(request.tipo);
            request.total ??= CalcularTotalGuardar(request);
            if (request.total <= 0)
                request.total = CalcularTotalGuardar(request);

            return request;
        }

        /// <summary>
        /// Valida inicialización.
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static ErrorDto ValidarInicializar(
    CrSeguimientoRefundicionesInicializarRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("La información de la operación es requerida.");

            if (!request.operacion.HasValue || request.operacion.Value <= 0)
                return DbHelper.ErrorResponse(MensajeOperacionRequerida);

            if (!request.fecha_desembolso.HasValue)
                return DbHelper.ErrorResponse("La fecha de desembolso es requerida.");

            if (!request.pri_deduc.HasValue || request.pri_deduc.Value <= 0)
                return DbHelper.ErrorResponse("La primera deducción es requerida.");

            if (!request.dia_pago.HasValue || request.dia_pago.Value <= 0)
                return DbHelper.ErrorResponse("El día de pago es requerido.");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida lista de refundiciones.
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static ErrorDto ValidarListaRefundiciones(
            CrSeguimientoRefundicionesListaRequest request)
        {
            if (request == null || !request.operacion.HasValue || request.operacion.Value <= 0)
                return DbHelper.ErrorResponse(MensajeOperacionRequerida);

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida lista de préstamos del socio.
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static ErrorDto ValidarPrestamos(
            CrSeguimientoRefundicionesPrestamosRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("La información de consulta es requerida.");

            if (!request.operacion.HasValue || request.operacion.Value <= 0)
                return DbHelper.ErrorResponse(MensajeOperacionRequerida);

            return ValidarOperacionCedulaCodigo(
                request.operacion.Value,
                request.cedula,
                request.codigo);
        }

        /// <summary>
        /// Valida operación, cédula y código.
        /// <param name="operacion"></param>
        /// <param name="cedula"></param>
        /// <param name="codigo"></param>
        /// </summary>
        /// <returns></returns>
        private static ErrorDto ValidarOperacionCedulaCodigo(
            long operacion,
            string? cedula,
            string? codigo)
        {
            if (operacion <= 0)
                return DbHelper.ErrorResponse(MensajeOperacionRequerida);

            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.ErrorResponse(MensajeCedulaRequerida);

            if (string.IsNullOrWhiteSpace(codigo))
                return DbHelper.ErrorResponse(MensajeCodigoRequerido);

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida búsqueda de créditos de terceros.
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static ErrorDto ValidarTerceros(
            CrSeguimientoRefundicionesConsultaTercerosRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("La información de consulta es requerida.");

            if (string.IsNullOrWhiteSpace(request.cedula))
                return DbHelper.ErrorResponse(MensajeCedulaRequerida);

            if (string.IsNullOrWhiteSpace(request.codigo))
                return DbHelper.ErrorResponse(MensajeCodigoRequerido);

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida recálculo por tipo.
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static ErrorDto ValidarRefundeDatos(
            CrSeguimientoRefundicionesRefundeDatosRequest request)
        {
            if (request == null || !request.operacion.HasValue || request.operacion.Value <= 0)
                return DbHelper.ErrorResponse(MensajeOperacionRequerida);

            if (!TipoValido(request.tipo))
                return DbHelper.ErrorResponse("El tipo de refundición no es válido.");

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida registro de refundición.
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static ErrorDto ValidarGuardar(
            CrSeguimientoRefundicionGuardarRequest request)
        {
            var errores = new List<string>();

            if (request == null)
                errores.Add("La información de refundición es requerida.");
            else
                ValidarGuardarData(request, errores);

            if (errores.Count == 0)
                return DbHelper.OkResponse("Ok");

            return new ErrorDto
            {
                Code = -2,
                Description = string.Join(Environment.NewLine, errores)
            };
        }

        /// <summary>
        /// Valida registro de refundición.
        /// <param name="request"></param>
        /// <param name="errores"></param>
        /// </summary>
        /// <returns></returns>
        private static void ValidarGuardarData(
            CrSeguimientoRefundicionGuardarRequest request,
            List<string> errores)
        {
            if (!request.operacion_refunde.HasValue || request.operacion_refunde.Value <= 0)
                errores.Add("- No se ha seleccionado ninguna operación");

            if (!request.operacion_nueva.HasValue || request.operacion_nueva.Value <= 0)
                errores.Add("- La operación nueva es requerida.");

            if (string.IsNullOrWhiteSpace(request.codigo_refunde))
                errores.Add("- El código de la operación a refundir es requerido.");

            if (string.IsNullOrWhiteSpace(request.codigo_nuevo))
                errores.Add("- El código de la operación nueva es requerido.");

            if (!TipoValido(request.tipo))
                errores.Add("- El tipo de refundición no es válido.");

            if ((request.saldo ?? 0m) < 0)
                errores.Add("- El saldo no es válido");

            if ((request.total ?? 0m) <= 0)
                errores.Add("- El monto a refundir no es válido.");
        }

        /// <summary>
        /// Valida eliminación de refundiciones.
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        private static ErrorDto ValidarEliminar(
            CrSeguimientoRefundicionesEliminarRequest request)
        {
            if (request == null || !request.operacion_nueva.HasValue || request.operacion_nueva.Value <= 0)
                return DbHelper.ErrorResponse(MensajeOperacionRequerida);

            if (request.operaciones_refunde == null ||
                request.operaciones_refunde.Count == 0 ||
                request.operaciones_refunde.Any(x => !x.HasValue || x.Value <= 0))
            {
                return DbHelper.ErrorResponse("Debe seleccionar al menos una refundición.");
            }

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Parsea filtros lazy load.
        /// <param name="filtrosJson"></param>
        /// </summary>
        /// <returns></returns>
        private static FiltrosLazyLoadData ParseFiltros(string? filtrosJson)
        {
            if (string.IsNullOrWhiteSpace(filtrosJson))
                return new FiltrosLazyLoadData();

            return JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtrosJson)
                   ?? new FiltrosLazyLoadData();
        }

        /// <summary>
        /// Fuerza exportación completa.
        /// <param name="filtrosJson"></param>
        /// </summary>
        /// <returns></returns>
        private static string ForzarExportarTodo(string? filtrosJson)
        {
            var filtros = ParseFiltros(filtrosJson);
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return JsonConvert.SerializeObject(filtros);
        }

        private static List<CrSeguimientoRefundicionData> FiltrarRefundiciones(
            List<CrSeguimientoRefundicionData> lista,
            string? filtro)
        {
            var texto = Clean(filtro).ToUpperInvariant();

            if (texto.Length == 0)
                return lista;

            return lista
                .Where(x => RefundicionContiene(x, texto))
                .ToList();
        }

        private static bool RefundicionContiene(
            CrSeguimientoRefundicionData item,
            string texto)
        {
            return item.id_solicitud.ToString().Contains(texto)
                   || Clean(item.codigo).ToUpperInvariant().Contains(texto)
                   || Clean(item.garantiax).ToUpperInvariant().Contains(texto)
                   || Clean(item.descripcion).ToUpperInvariant().Contains(texto)
                   || Clean(item.tipo_desc).ToUpperInvariant().Contains(texto);
        }

        private static List<CrSeguimientoRefundicionCreditoData> FiltrarCreditos(
            List<CrSeguimientoRefundicionCreditoData> lista,
            string? filtro)
        {
            var texto = Clean(filtro).ToUpperInvariant();

            if (texto.Length == 0)
                return lista;

            return lista
                .Where(x => CreditoContiene(x, texto))
                .ToList();
        }

        private static bool CreditoContiene(
            CrSeguimientoRefundicionCreditoData item,
            string texto)
        {
            return item.id_solicitud.ToString().Contains(texto)
                   || Clean(item.codigo).ToUpperInvariant().Contains(texto)
                   || Clean(item.garantiax).ToUpperInvariant().Contains(texto)
                   || Clean(item.descripcion).ToUpperInvariant().Contains(texto)
                   || Clean(item.tipo_desc).ToUpperInvariant().Contains(texto);
        }

        private static List<CrSeguimientoRefundicionData> OrdenarRefundiciones(
            List<CrSeguimientoRefundicionData> lista,
            FiltrosLazyLoadData filtros)
        {
            var sortField = Clean(filtros.sortField).ToLowerInvariant();
            var asc = filtros.sortOrder != 0;

            return sortField switch
            {
                "codigo" => Ordenar(lista, x => x.codigo, asc),
                "garantiax" => Ordenar(lista, x => x.garantiax, asc),
                "descripcion" => Ordenar(lista, x => x.descripcion, asc),
                "saldo_anterior" => Ordenar(lista, x => x.saldo_anterior ?? 0, asc),
                "intcor" => Ordenar(lista, x => x.intcor ?? 0, asc),
                "intmor" => Ordenar(lista, x => x.intmor ?? 0, asc),
                "cargos" => Ordenar(lista, x => x.cargos ?? 0, asc),
                "polizas" => Ordenar(lista, x => x.polizas ?? 0, asc),
                "principal" => Ordenar(lista, x => x.principal ?? 0, asc),
                "monto" => Ordenar(lista, x => x.monto ?? 0, asc),
                "iva" => Ordenar(lista, x => x.iva ?? 0, asc),
                "tipo_desc" => Ordenar(lista, x => x.tipo_desc, asc),
                _ => Ordenar(lista, x => x.id_solicitud, asc)
            };
        }

        private static List<CrSeguimientoRefundicionCreditoData> OrdenarCreditos(
            List<CrSeguimientoRefundicionCreditoData> lista,
            FiltrosLazyLoadData filtros)
        {
            var sortField = Clean(filtros.sortField).ToLowerInvariant();
            var asc = filtros.sortOrder != 0;

            return sortField switch
            {
                "codigo" => Ordenar(lista, x => x.codigo, asc),
                "garantiax" => Ordenar(lista, x => x.garantiax, asc),
                "descripcion" => Ordenar(lista, x => x.descripcion, asc),
                "saldo" => Ordenar(lista, x => x.saldo ?? 0, asc),
                "intc" => Ordenar(lista, x => x.intc ?? 0, asc),
                "intm" => Ordenar(lista, x => x.intm ?? 0, asc),
                "amortiza" => Ordenar(lista, x => x.amortiza ?? 0, asc),
                "cargos" => Ordenar(lista, x => x.cargos ?? 0, asc),
                "polizas" => Ordenar(lista, x => x.polizas ?? 0, asc),
                "iva" => Ordenar(lista, x => x.iva ?? 0, asc),
                "total" => Ordenar(lista, x => x.total ?? 0, asc),
                "tipo_desc" => Ordenar(lista, x => x.tipo_desc, asc),
                _ => Ordenar(lista, x => x.id_solicitud, asc)
            };
        }

        private static List<T> Ordenar<T, TKey>(
            List<T> lista,
            Func<T, TKey> keySelector,
            bool asc)
        {
            return asc
                ? lista.OrderBy(keySelector).ToList()
                : lista.OrderByDescending(keySelector).ToList();
        }
        private static decimal CalcularTotalGuardar(CrSeguimientoRefundicionGuardarRequest request)
        {
            return (request.principal ?? 0m)
                 + (request.intcor ?? 0m)
                 + (request.intmor ?? 0m)
                 + (request.cargos ?? 0m)
                 + (request.polizas ?? 0m)
                 + (request.iva ?? 0m);
        }

        private static decimal CalcularTotal(
            CrSeguimientoRefundicionDatosDto data)
        {
            return (data.principal ?? 0)
                   + (data.intcor ?? 0)
                   + (data.intmor ?? 0)
                   + (data.cargos ?? 0)
                   + (data.polizas ?? 0)
                   + (data.iva ?? 0);
        }

        private static decimal CalcularTotalCredito(
            CrSeguimientoRefundicionCreditoData data)
        {
            return (data.amortiza ?? 0)
                   + (data.intc ?? 0)
                   + (data.intm ?? 0)
                   + (data.cargos ?? 0)
                   + (data.polizas ?? 0)
                   + (data.iva ?? 0);
        }

        private static CrSeguimientoRefundicionesPrestamosRequest CrearPrestamosRequest(
    CrSeguimientoRefundicionesOperacionBaseDto baseData,
    long operacion)
        {
            return new()
            {
                operacion = operacion,
                cedula = baseData.cedula,
                codigo = baseData.codigo
            };
        }

        private static string ResolverTipo(string? tipo)
        {
            var value = Clean(tipo).ToUpperInvariant();

            if (value == TipoMorosidad || value.StartsWith("MOROSIDAD"))
                return TipoMorosidad;

            if (value == TipoPendientes || value.StartsWith("PENDIENTES"))
                return TipoPendientes;

            return TipoCancelaCredito;
        }

        private static bool TipoValido(string? tipo)
        {
            var value = Clean(tipo).ToUpperInvariant();

            return value == TipoCancelaCredito
                || value == TipoMorosidad
                || value == TipoPendientes
                || value.StartsWith("CANCELA", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("MOROSIDAD", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("PENDIENTES", StringComparison.OrdinalIgnoreCase);
        }

        private static string TipoDescripcion(string? tipo)
        {
            return ResolverTipo(tipo) switch
            {
                TipoMorosidad => "Morosidad",
                TipoPendientes => "Pendientes",
                _ => "Cancela Crédito"
            };
        }

        private static string Clean(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static ErrorDto<CrSeguimientoRefundicionesInicializarDto> ErrorInicializar(
            string mensaje,
            int code = -1)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                code,
                new CrSeguimientoRefundicionesInicializarDto());
        }

        private static ErrorDto<CrSeguimientoRefundicionesListaDto> ErrorListaRefundiciones(
            string mensaje,
            int code = -1)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                code,
                new CrSeguimientoRefundicionesListaDto());
        }

        private static ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> ErrorListaCreditos(
            string mensaje,
            int code = -1)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                code,
                new CrSeguimientoRefundicionesCreditosListaDto());
        }

        private sealed class MontosIniciales
        {
            public decimal PrimerCuota { get; set; }
            public decimal Poliza { get; set; }
            public decimal Interes { get; set; }
        }
    }
}