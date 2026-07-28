using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCRemesasTesoreriaDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MProGrxMain _proGrxMain;

        private const int ModuloCxC = 31;

        public FrmCxCRemesasTesoreriaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _proGrxMain = new MProGrxMain(config);
        }

        #region Remesas
        /// <summary>
        /// Obtiene lista lazy de remesas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CxCRemesasTesoreriaRemesaLista> CxC_RemesasTesoreria_Remesas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtros = DeserializarFiltros(parametros);
            if (filtros.Code != 0)
            {
                return CrearRespuestaRemesasVacia(
                    filtros.Description ?? CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            if (filtros.Result is not FiltrosLazyLoadData filtrosData)
            {
                return CrearRespuestaRemesasVacia(
                    CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            return ObtenerRemesasLista(CodEmpresa, filtrosData, false);
        }
        /// <summary>
        /// Exporta lista de remesas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CxCRemesasTesoreriaRemesaLista> CxC_RemesasTesoreria_Remesas_Lista_Export(int CodEmpresa, string parametros)
        {
            var filtros = DeserializarFiltros(parametros);
            if (filtros.Code != 0)
            {
                return CrearRespuestaRemesasVacia(
                    filtros.Description ?? CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            if (filtros.Result is not FiltrosLazyLoadData filtrosData)
            {
                return CrearRespuestaRemesasVacia(
                    CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            filtrosData.pagina = 0;
            filtrosData.paginacion = 0;

            return ObtenerRemesasLista(CodEmpresa, filtrosData, true);
        }
        /// <summary>
        /// Obtiene una remesa por código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tesoreriaRemesa"></param>
        /// <returns></returns>
        public ErrorDto<CxCRemesasTesoreriaRemesaData> CxC_RemesasTesoreria_Remesa_Obtener(int CodEmpresa, int tesoreriaRemesa)
        {
            if (tesoreriaRemesa <= 0)
            {
                return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaRemesaData>(
                    CxCRemesasTesoreriaConstantes.MensajeRemesaRequerida,
                    result: new CxCRemesasTesoreriaRemesaData());
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var remesa = ObtenerRemesa(conn, tesoreriaRemesa);

                if (remesa is null)
                {
                    return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaRemesaData>(
                        CxCRemesasTesoreriaConstantes.MensajeRemesaNoExiste,
                        result: new CxCRemesasTesoreriaRemesaData());
                }

                return DbHelper.CreateOkResponse(remesa);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaRemesaData>(
                    $"No fue posible consultar la remesa. {ex.Message}",
                    result: new CxCRemesasTesoreriaRemesaData());
            }
        }
        /// <summary>
        /// Guarda o actualiza una remesa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CxCRemesasTesoreriaRemesaData> CxC_RemesasTesoreria_Remesa_Guardar(int CodEmpresa, CxCRemesasTesoreriaRemesaGuardarRequest request)
        {
            var validacion = ValidarRemesaGuardar(request);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaRemesaData>(
                    validacion.Description ?? CxCRemesasTesoreriaConstantes.MensajeValidacionFallida,
                    result: new CxCRemesasTesoreriaRemesaData());
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var tesoreriaRemesa = request.tesoreria_remesa.GetValueOrDefault();

                var existe = RemesaExiste(conn, tesoreriaRemesa);
                if (existe && !RemesaEstaAbierta(conn, tesoreriaRemesa))
                {
                    return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaRemesaData>(
                        CxCRemesasTesoreriaConstantes.MensajeRemesaNoAbierta,
                        result: new CxCRemesasTesoreriaRemesaData());
                }

                var remesaId = existe
                    ? ActualizarRemesa(conn, request)
                    : InsertarRemesa(conn, request);

                var remesa = ObtenerRemesa(conn, remesaId) ?? new CxCRemesasTesoreriaRemesaData();

                RegistrarBitacora(
                    CodEmpresa,
                    request.usuario,
                    existe
                        ? CxCRemesasTesoreriaConstantes.BitacoraModifica
                        : CxCRemesasTesoreriaConstantes.BitacoraRegistra,
                    $"Remesa de CxC Traslado a Tesoreria : {remesaId}");

                return DbHelper.CreateOkResponse(remesa);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaRemesaData>(
                    $"No fue posible guardar la remesa. {ex.Message}",
                    result: new CxCRemesasTesoreriaRemesaData());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaRemesaData>(
                    $"No fue posible guardar la remesa. {ex.Message}",
                    result: new CxCRemesasTesoreriaRemesaData());
            }
        }
        /// <summary>
        /// Elimina una remesa abierta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tesoreriaRemesa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CxC_RemesasTesoreria_Remesa_Eliminar(int CodEmpresa, int tesoreriaRemesa, string usuario)
        {
            if (tesoreriaRemesa <= 0)
            {
                return DbHelper.ErrorResponse(CxCRemesasTesoreriaConstantes.MensajeRemesaRequerida);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(CxCRemesasTesoreriaConstantes.MensajeUsuarioRequerido);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                conn.Open();

                if (!RemesaEstaAbierta(conn, tesoreriaRemesa))
                {
                    return DbHelper.ErrorResponse(CxCRemesasTesoreriaConstantes.MensajeRemesaNoAbierta);
                }

                using var tx = conn.BeginTransaction();

                const string sqlCuentas = @"
            update CxC_Cuentas
            set Tesoreria_Remesa = null
            where Tesoreria_Remesa = @tesoreriaRemesa;";

                const string sqlRemesa = @"
            delete CxC_REMESAS_TES
            where Tesoreria_Remesa = @tesoreriaRemesa;";

                conn.Execute(sqlCuentas, new { tesoreriaRemesa }, tx);
                conn.Execute(sqlRemesa, new { tesoreriaRemesa }, tx);

                tx.Commit();

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    CxCRemesasTesoreriaConstantes.BitacoraElimina,
                    $"Remesa de CxC Traslado a Tesoreria : {tesoreriaRemesa}");

                return DbHelper.OkResponse(CxCRemesasTesoreriaConstantes.MensajeEliminarOk);
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse($"No fue posible eliminar la remesa. {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse($"No fue posible eliminar la remesa. {ex.Message}");
            }
        }
        /// <summary>
        /// Obtiene remesas para combos según estado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_RemesasTesoreria_Remesas_Dropdown_Obtener(int CodEmpresa, string? estado)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                estado = Normalizar(estado);

                const string sql = @"
                    select
                        cast(Tesoreria_Remesa as varchar(20)) as item,
                        right('0000' + cast(Tesoreria_Remesa as varchar(20)), 4)
                            + '...' + rtrim(isnull(usuario, ''))
                            + '...' + convert(varchar(19), fecha, 120)
                            + ' I:' + convert(varchar(10), fecha_inicio, 103)
                            + ' C:' + convert(varchar(10), fecha_corte, 103) as descripcion
                    from CxC_REMESAS_TES
                    where (@estado = '' or estado = @estado)
                    order by fecha desc;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new { estado }).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    $"No fue posible consultar remesas. {ex.Message}",
                    result: new List<DropDownListaGenericaModel>());
            }
        }


        #endregion
        #region Carga
        /// <summary>
        /// Obtiene oficinas disponibles para carga según remesa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tesoreriaRemesa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_RemesasTesoreria_Oficinas_Carga_Dropdown_Obtener(int CodEmpresa, int tesoreriaRemesa)
        {
            return ObtenerOficinasPorRemesa(CodEmpresa, tesoreriaRemesa, false);
        }
        /// <summary>
        /// Obtiene lista de operaciones pendientes de carga.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tesoreriaRemesa"></param>
        /// <param name="codOficina"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Carga_Lista_Obtener(int CodEmpresa,int tesoreriaRemesa,string? codOficina, string parametros)
        {
            var filtros = DeserializarFiltros(parametros);
            if (filtros.Code != 0)
            {
                return CrearRespuestaOperacionesVacia(
                    filtros.Description ?? CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            if (filtros.Result is not FiltrosLazyLoadData filtrosData)
            {
                return CrearRespuestaOperacionesVacia(
                    CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            return ObtenerCargaLista(CodEmpresa,tesoreriaRemesa,codOficina, filtrosData,false);
        }
        /// <summary>
        /// Exporta lista de operaciones pendientes de carga.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tesoreriaRemesa"></param>
        /// <param name="codOficina"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Carga_Lista_Export(int CodEmpresa, int tesoreriaRemesa,string? codOficina, string parametros)
        {
            var filtros = DeserializarFiltros(parametros);
            if (filtros.Code != 0)
            {
                return CrearRespuestaOperacionesVacia(
                    filtros.Description ?? CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            if (filtros.Result is not FiltrosLazyLoadData filtrosData)
            {
                return CrearRespuestaOperacionesVacia(
                    CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            filtrosData.pagina = 0;
            filtrosData.paginacion = 0;

            return ObtenerCargaLista(CodEmpresa, tesoreriaRemesa,codOficina,filtrosData,true);
        }
        /// <summary>
        /// Aplica carga de operaciones seleccionadas a la remesa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CxC_RemesasTesoreria_Carga_Aplicar(int CodEmpresa, CxCRemesasTesoreriaCargaAplicarRequest request)
        {
            var validacion = ValidarCargaAplicar(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                conn.Open();

                var tesoreriaRemesa = request.tesoreria_remesa.GetValueOrDefault();

                if (!RemesaEstaAbierta(conn, tesoreriaRemesa))
                {
                    return DbHelper.ErrorResponse(
                        CxCRemesasTesoreriaConstantes.MensajeRemesaNoAbierta);
                }

                using var tx = conn.BeginTransaction();

                foreach (var operacion in request.operaciones.Distinct())
                {
                    EjecutarCargaOperacion(
                        conn,
                        tx,
                        operacion,
                        tesoreriaRemesa,
                        request.usuario);
                }

                tx.Commit();

                RegistrarBitacora(
                    CodEmpresa,
                    request.usuario,
                    CxCRemesasTesoreriaConstantes.BitacoraAplica,
                    $"Carga Remesa Traslado a Tesoreria : {tesoreriaRemesa}");

                return DbHelper.OkResponse(
                    CxCRemesasTesoreriaConstantes.MensajeCargaOk);
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible aplicar la carga. {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible aplicar la carga. {ex.Message}");
            }
        }
        /// <summary>
        /// Cierra una remesa abierta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
       public ErrorDto CxC_RemesasTesoreria_Carga_Cerrar(int CodEmpresa, CxCRemesasTesoreriaCerrarRequest request)
        {
            var tesoreriaRemesa = request.tesoreria_remesa.GetValueOrDefault();

            if (tesoreriaRemesa <= 0)
            {
                return DbHelper.ErrorResponse(
                    CxCRemesasTesoreriaConstantes.MensajeRemesaRequerida);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    CxCRemesasTesoreriaConstantes.MensajeUsuarioRequerido);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                if (!RemesaEstaAbierta(conn, tesoreriaRemesa))
                {
                    return DbHelper.ErrorResponse(
                        CxCRemesasTesoreriaConstantes.MensajeRemesaNoAbierta);
                }

                const string sql = @"
                    update CxC_REMESAS_TES
                    set estado = @estado
                    where Tesoreria_Remesa = @tesoreriaRemesa;";

                conn.Execute(sql, new
                {
                    estado = CxCRemesasTesoreriaConstantes.EstadoCerrada,
                    tesoreriaRemesa
                });

                RegistrarBitacora(
                    CodEmpresa,
                    request.usuario,
                    CxCRemesasTesoreriaConstantes.BitacoraAplica,
                    $"Cierra Remesa Traslado a Tesoreria : {tesoreriaRemesa}");

                return DbHelper.OkResponse(
                    CxCRemesasTesoreriaConstantes.MensajeCerrarOk);
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible cerrar la remesa. {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible cerrar la remesa. {ex.Message}");
            }
        }
        #endregion
        #region Traslado
        /// <summary>
        /// Obtiene lista de operaciones cargadas pendientes de traslado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tesoreriaRemesa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Traslado_Lista_Obtener(int CodEmpresa, int tesoreriaRemesa, string parametros)
        {
            var filtros = DeserializarFiltros(parametros);
            if (filtros.Code != 0)
            {
                return CrearRespuestaOperacionesVacia(filtros.Description ?? CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            if (filtros.Result is not FiltrosLazyLoadData filtrosData)
            {
                return CrearRespuestaOperacionesVacia(CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            return ObtenerTrasladoLista(CodEmpresa, tesoreriaRemesa, filtrosData, false);
        }

        /// <summary>
        /// Exporta lista de operaciones cargadas pendientes de traslado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tesoreriaRemesa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Traslado_Lista_Export(int CodEmpresa, int tesoreriaRemesa, string parametros)
        {
            var filtros = DeserializarFiltros(parametros);
            if (filtros.Code != 0)
            {
                return CrearRespuestaOperacionesVacia(filtros.Description ?? CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            if (filtros.Result is not FiltrosLazyLoadData filtrosData)
            {
                return CrearRespuestaOperacionesVacia(CxCRemesasTesoreriaConstantes.MensajeFiltrosInvalidos);
            }

            filtrosData.pagina = 0;
            filtrosData.paginacion = 0;

            return ObtenerTrasladoLista(CodEmpresa, tesoreriaRemesa, filtrosData, true);
        }

        /// <summary>
        /// Aplica traslado de operaciones cargadas a Tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CxC_RemesasTesoreria_Traslado_Aplicar(int CodEmpresa,CxCRemesasTesoreriaTrasladoAplicarRequest request)
        {
            var validacion = ValidarTrasladoAplicar(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            var tesoreriaRemesa = request.tesoreria_remesa.GetValueOrDefault();
            var agrupar = request.agrupar.GetValueOrDefault();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }

                if (!RemesaEstaCerrada(conn, tesoreriaRemesa))
                {
                    return DbHelper.ErrorResponse(
                        CxCRemesasTesoreriaConstantes.MensajeRemesaNoCerrada);
                }

                var fecha = fxFechaServidor(CodEmpresa);

                var unidadOmision = ObtenerParametroCxC(
                    conn,
                    CxCRemesasTesoreriaConstantes.ParametroUnidadOmision);

                var conceptoOmision = ObtenerParametroCxC(
                    conn,
                    CxCRemesasTesoreriaConstantes.ParametroConceptoOmision);

                using var tx = conn.BeginTransaction();

                var rows = ObtenerRowsTraslado(
                    conn,
                    tx,
                    tesoreriaRemesa,
                    agrupar);

                var contexto = new CxCRemesasTesoreriaTrasladoContext
                {
                    CodEmpresa = CodEmpresa,
                    TesoreriaRemesa = tesoreriaRemesa,
                    Usuario = request.usuario,
                    Agrupar = agrupar,
                    Fecha = fecha,
                    UnidadOmision = unidadOmision,
                    ConceptoOmision = conceptoOmision
                };

                foreach (var row in rows)
                {
                    ProcesarTrasladoRow(conn, tx, contexto, row);
                }

                ActualizarEstadoRemesaTrasladada(
                    conn,
                    tx,
                    tesoreriaRemesa);

                tx.Commit();

                return DbHelper.OkResponse(
                    CxCRemesasTesoreriaConstantes.MensajeTrasladoOk);
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible aplicar el traslado. {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible aplicar el traslado. {ex.Message}");
            }
        }
        #endregion
        #region Reportes

        /// <summary>
        /// Obtiene oficinas para filtro de reportes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="todasFechas"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_RemesasTesoreria_Oficinas_Reporte_Dropdown_Obtener(int CodEmpresa,DateTime fechaInicio,DateTime fechaCorte,bool todasFechas)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var inicio = todasFechas
                    ? new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Local)
                    : FechaLocal(fechaInicio);

                var corte = todasFechas
                    ? FechaLocal(fxFechaServidor(CodEmpresa))
                    : FechaLocal(fechaCorte);

                const string sql = @"
                    select
                        rtrim(cod_oficina) as item,
                        rtrim(descripcion) as descripcion
                    from SIF_Oficinas
                    where cod_oficina in (
                        select R.Cod_Oficina
                        from CxC_Cuentas R
                        inner join CxC_Conceptos C on R.cod_concepto = C.cod_concepto
                        where R.Autoriza_Estado = 'F'
                          and R.Registro_Fecha between @inicio and @corte
                          and R.TESORERIA_FECHA is null
                          and R.estado in ('A','C')
                        group by R.Cod_Oficina
                    )
                    order by cod_oficina;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    inicio,
                    corte = corte.AddDays(1).AddTicks(-1)
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    $"No fue posible consultar oficinas de reportes. {ex.Message}",
                    result: new List<DropDownListaGenericaModel>());
            }
        }
        #endregion
        #region Reactivacion
        /// <summary>
        /// Obtiene información de operación para reactivación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CxCRemesasTesoreriaReactivacionDto> CxC_RemesasTesoreria_Reactivacion_Operacion_Obtener(int CodEmpresa,int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaReactivacionDto>(
                    CxCRemesasTesoreriaConstantes.MensajeOperacionRequerida,
                    result: new CxCRemesasTesoreriaReactivacionDto());
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var data = ObtenerReactivacionOperacion(conn, operacion);

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaReactivacionDto>(
                        CxCRemesasTesoreriaConstantes.MensajeOperacionNoExiste,
                        result: new CxCRemesasTesoreriaReactivacionDto());
                }

                VerificarReactivacion(conn, data);

                return DbHelper.CreateOkResponse(data);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaReactivacionDto>(
                    $"No fue posible consultar la operación. {ex.Message}",
                    result: new CxCRemesasTesoreriaReactivacionDto());
            }
        }
        /// <summary>
        /// Reactiva una operación trasladada a tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CxC_RemesasTesoreria_Reactivacion_Aplicar(int CodEmpresa,CxCRemesasTesoreriaReactivacionAplicarRequest request)
        {
            var operacion = request.operacion.GetValueOrDefault();

            if (operacion <= 0)
            {
                return DbHelper.ErrorResponse(
                    CxCRemesasTesoreriaConstantes.MensajeOperacionRequerida);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    CxCRemesasTesoreriaConstantes.MensajeUsuarioRequerido);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var data = ObtenerReactivacionOperacion(conn, operacion);

                if (data is null)
                {
                    return DbHelper.ErrorResponse(
                        CxCRemesasTesoreriaConstantes.MensajeOperacionNoExiste);
                }

                VerificarReactivacion(conn, data);

                if (!data.puede_reactivar)
                {
                    return DbHelper.ErrorResponse(
                        CxCRemesasTesoreriaConstantes.MensajeOperacionNoReactivable);
                }

                const string sql = @"
            update CxC_Cuentas
            set tesoreria_Fecha = null
            where Operacion = @operacion;";

                conn.Execute(sql, new { operacion });

                RegistrarBitacora(
                    CodEmpresa,
                    request.usuario,
                    CxCRemesasTesoreriaConstantes.BitacoraAplica,
                    $"ReActivacion Traslado Tes. Op:{operacion}");

                return DbHelper.OkResponse(
                    CxCRemesasTesoreriaConstantes.MensajeReactivacionOk);
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible reactivar la operación. {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible reactivar la operación. {ex.Message}");
            }
        }
        #endregion
        #region Helpers
        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }
        private DateTime fxFechaServidor(int CodEmpresa)
        {
            return _proGrxMain.fxFechaServidor(CodEmpresa, 0);
        }
        private static ErrorDto<FiltrosLazyLoadData> DeserializarFiltros(string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                              ?? new FiltrosLazyLoadData();

                return DbHelper.CreateOkResponse(filtros);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<FiltrosLazyLoadData>(ex.Message, result: new FiltrosLazyLoadData());
            }
        }
        private static ErrorDto ValidarRemesaGuardar(CxCRemesasTesoreriaRemesaGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(CxCRemesasTesoreriaConstantes.MensajeUsuarioRequerido);
            }

            if (request.fecha_inicio == default || request.fecha_corte == default)
            {
                return DbHelper.ErrorResponse(CxCRemesasTesoreriaConstantes.MensajeFechasRemesaRequeridas);
            }

            return DbHelper.OkResponse(CxCRemesasTesoreriaConstantes.MensajeOk);
        }
        private static ErrorDto ValidarCargaAplicar(CxCRemesasTesoreriaCargaAplicarRequest request)
        {
            if (request.tesoreria_remesa <= 0)
            {
                return DbHelper.ErrorResponse(CxCRemesasTesoreriaConstantes.MensajeRemesaRequerida);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(CxCRemesasTesoreriaConstantes.MensajeUsuarioRequerido);
            }

            if (request.operaciones == null || request.operaciones.Count == 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar al menos una operación.");
            }

            return DbHelper.OkResponse(CxCRemesasTesoreriaConstantes.MensajeOk);
        }
        private ErrorDto<CxCRemesasTesoreriaRemesaLista> ObtenerRemesasLista(int CodEmpresa,FiltrosLazyLoadData filtros,bool esExportar)
        {
            var response = new ErrorDto<CxCRemesasTesoreriaRemesaLista>
            {
                Code = 0,
                Description = CxCRemesasTesoreriaConstantes.MensajeOk,
                Result = new CxCRemesasTesoreriaRemesaLista()
            };

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var parametros = CrearParametrosLista(filtros);
                var orderBy = ObtenerOrdenRemesas(filtros);
                var usarPaginacion = filtros.paginacion > 0 && !esExportar;

                const string where = @"
                    where
                        @filtro is null
                        or cast(T.Tesoreria_Remesa as varchar(20)) like @like
                        or isnull(T.usuario, '') like @like
                        or isnull(T.notas, '') like @like
                        or isnull(T.estado, '') like @like";

                var sqlCount = $@"
                    select count(1)
                    from CxC_REMESAS_TES T
                    {where};";

                var sqlLista = $@"
                    select
                        T.Tesoreria_Remesa as tesoreria_remesa,
                        isnull(rtrim(T.usuario), '') as usuario,
                        T.fecha,
                        isnull(rtrim(T.estado), '') as estado,
                        T.fecha_inicio,
                        T.fecha_corte,
                        isnull(D.Casos, 0) as casos,
                        isnull(D.Monto, 0) as monto,
                        isnull(T.notas, '') as notas
                    from CxC_REMESAS_TES T
                    left join vCxC_Remesa_Tes_Rsm D
                        on T.Tesoreria_Remesa = D.Tesoreria_Remesa
                    {where}
                    order by {orderBy}";

                if (usarPaginacion)
                {
                    sqlLista += CxCRemesasTesoreriaConstantes.PaginacionSql;
                }

                response.Result.total = conn.QuerySingle<int>(sqlCount, parametros);
                response.Result.lista = conn.Query<CxCRemesasTesoreriaRemesaData>(sqlLista, parametros)
                    .Select(CompletarEstadoDescripcion)
                    .ToList();

                return response;
            }
            catch (DbException ex)
            {
                return CrearRespuestaRemesasVacia($"No fue posible consultar remesas. {ex.Message}");
            }
        }
        private ErrorDto<CxCRemesasTesoreriaOperacionLista> ObtenerCargaLista(int CodEmpresa, int tesoreriaRemesa, string? codOficina, FiltrosLazyLoadData filtros, bool esExportar)
        {
            var fechas = ObtenerFechasRemesa(CodEmpresa, tesoreriaRemesa);
            if (fechas.Code != 0)
            {
                return CrearRespuestaOperacionesVacia(fechas.Description ?? CxCRemesasTesoreriaConstantes.MensajeFechasRemesaInvalidas);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var parametros = CrearParametrosOperaciones(filtros, fechas.Result.inicio, fechas.Result.corte, codOficina);
                var orderBy = ObtenerOrdenOperaciones(filtros);
                var usarPaginacion = filtros.paginacion > 0 && !esExportar;

                const string where = @"
            where Activa_Fecha between @inicio and @corte
              and (@codOficina = '' or Cod_Oficina = @codOficina)
              and (
                    @filtro is null
                 or cast(Operacion as varchar(20)) like @like
                 or isnull(Cod_Concepto, '') like @like
                 or isnull(Cedula, '') like @like
                 or isnull(Nombre, '') like @like
              )";

                var sqlCount = $@"select count(1) from vCxC_Cuentas_Desembolsos_Pendientes {where};";
                var sqlTotal = $@"select isnull(sum(DESEMBOLSO_PENDIENTE), 0) from vCxC_Cuentas_Desembolsos_Pendientes {where};";

                var sqlLista = $@"
            select
                Operacion as operacion,
                isnull(Cod_Concepto, '') as cod_concepto,
                isnull(Cedula, '') as cedula,
                isnull(Nombre, '') as nombre,
                isnull(Monto, 0) as monto,
                isnull(DESEMBOLSO_PENDIENTE, 0) as desembolso_monto,
                cast(0 as decimal(18,2)) as desembolsos,
                cast(0 as decimal(18,2)) as otros_giros,
                isnull(DESEMBOLSO_PENDIENTE, 0) as total
            from vCxC_Cuentas_Desembolsos_Pendientes
            {where}
            order by {orderBy}";

                if (usarPaginacion)
                {
                    sqlLista += CxCRemesasTesoreriaConstantes.PaginacionSql;
                }

                var result = new CxCRemesasTesoreriaOperacionLista
                {
                    total = conn.QuerySingle<int>(sqlCount, parametros),
                    total_monto = conn.QuerySingle<decimal>(sqlTotal, parametros),
                    lista = conn.Query<CxCRemesasTesoreriaOperacionData>(sqlLista, parametros).ToList()
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return CrearRespuestaOperacionesVacia($"No fue posible consultar operaciones de carga. {ex.Message}");
            }
        }
        private ErrorDto<(DateTime inicio, DateTime corte)> ObtenerFechasRemesa(int CodEmpresa, int tesoreriaRemesa)
        {
            if (tesoreriaRemesa <= 0)
            {
                return DbHelper.CreateErrorResponse<(DateTime inicio, DateTime corte)>(
                    CxCRemesasTesoreriaConstantes.MensajeRemesaRequerida);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                    select fecha_inicio, fecha_corte
                    from CxC_REMESAS_TES
                    where Tesoreria_Remesa = @tesoreriaRemesa;";

                var row = conn.QueryFirstOrDefault<(DateTime fecha_inicio, DateTime fecha_corte)>(
                    sql,
                    new { tesoreriaRemesa });

                if (row.fecha_inicio == default)
                {
                    return DbHelper.CreateErrorResponse<(DateTime inicio, DateTime corte)>(
                        CxCRemesasTesoreriaConstantes.MensajeRemesaNoExiste);
                }

                return DbHelper.CreateOkResponse((row.fecha_inicio.Date, row.fecha_corte.Date.AddDays(1).AddTicks(-1)));
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<(DateTime inicio, DateTime corte)>($"No fue posible obtener fechas de remesa. {ex.Message}");
            }
        }
        private ErrorDto<List<DropDownListaGenericaModel>> ObtenerOficinasPorRemesa(int CodEmpresa,int tesoreriaRemesa,bool usaTesoreriaFecha)
        {
            var fechas = ObtenerFechasRemesa(CodEmpresa, tesoreriaRemesa);
            if (fechas.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    fechas.Description ?? CxCRemesasTesoreriaConstantes.MensajeFechasRemesaInvalidas,
                    result: new List<DropDownListaGenericaModel>());
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
            select
                rtrim(cod_oficina) as item,
                rtrim(descripcion) as descripcion
            from SIF_Oficinas
            where cod_oficina in (
                select R.Cod_Oficina
                from CxC_Cuentas R
                inner join CxC_Conceptos C on R.cod_concepto = C.cod_concepto
                where R.Autoriza_Estado = 'F'
                  and R.Registro_Fecha between @inicio and @corte
                  and (
                        (@usaTesoreriaFecha = 1 and R.TESORERIA_FECHA is null)
                     or (@usaTesoreriaFecha = 0 and R.Tesoreria_Estado is null)
                  )
                  and R.estado in ('A','C')
                group by R.Cod_Oficina
            )
            order by cod_oficina;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    fechas.Result.inicio,
                    fechas.Result.corte,
                    usaTesoreriaFecha = usaTesoreriaFecha ? 1 : 0
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    $"No fue posible consultar oficinas. {ex.Message}",
                    result: new List<DropDownListaGenericaModel>());
            }
        }
        private static CxCRemesasTesoreriaReactivacionDto? ObtenerReactivacionOperacion(SqlConnection conn, int operacion)
        {
            const string sql = @"
                select
                    R.Operacion as operacion,
                    isnull(R.cod_concepto, '') as cod_concepto,
                    isnull(C.descripcion, '') as descripcion,
                    isnull(R.cedula, '') as cedula,
                    isnull(S.nombre, '') as nombre,
                    isnull(R.Desembolso_Monto, 0) as desembolso_monto,
                    R.Tesoreria_Solicitud as tesoreria_solicitud
                from CxC_Cuentas R
                inner join CxC_Personas S on R.cedula = S.cedula
                inner join CxC_Conceptos C on R.cod_concepto = C.cod_concepto
                where R.Operacion = @operacion
                  and R.estado = 'A';";

            return conn.QueryFirstOrDefault<CxCRemesasTesoreriaReactivacionDto>(sql, new { operacion });
        }
        private static void VerificarReactivacion(SqlConnection conn, CxCRemesasTesoreriaReactivacionDto data)
        {
            const string sql = @"
                select top 1
                    NSOLICITUD as tesoreria_solicitud,
                    id_banco,
                    tipo,
                    ndocumento as documento
                from Tes_Transacciones
                where Nsolicitud = @tesoreriaSolicitud
                  and estado in ('I','T','P');";

            var doc = conn.QueryFirstOrDefault<CxCRemesasTesoreriaReactivacionDto>(
                sql,
                new { tesoreriaSolicitud = data.tesoreria_solicitud ?? 0 });

            data.detalle = ConstruirDetalleReactivacion(data, doc);
            data.puede_reactivar = doc is null;
            data.documento = doc?.documento ?? string.Empty;
            data.tipo = doc?.tipo ?? string.Empty;
            data.id_banco = doc?.id_banco;
        }
        private static string ConstruirDetalleReactivacion(CxCRemesasTesoreriaReactivacionDto data, CxCRemesasTesoreriaReactivacionDto? doc)
        {
            var detalle = string.Concat(
                "Línea         : ", data.cod_concepto, Environment.NewLine,
                "Descripción   : ", data.descripcion, Environment.NewLine,
                "Cédula        : ", data.cedula, Environment.NewLine,
                "Nombre        : ", data.nombre, Environment.NewLine,
                "Monto a Girar : ", data.desembolso_monto.ToString("N2"), Environment.NewLine);

            if (doc is null)
            {
                return detalle;
            }

            return string.Concat(
                detalle,
                " / EXISTE UN DOCUMENTO O SOLICITUD DE EMISION EN TESORERIA / ", data.nombre, Environment.NewLine,
                "Solicitud :", doc.tesoreria_solicitud, Environment.NewLine,
                "Documento :", doc.documento, Environment.NewLine,
                "Tipo/Banco:", doc.tipo, "/", doc.id_banco, Environment.NewLine);
        }
        private static int InsertarRemesa(SqlConnection conn,CxCRemesasTesoreriaRemesaGuardarRequest request)
        {
                var tesoreriaRemesa = request.tesoreria_remesa.GetValueOrDefault();

                if (tesoreriaRemesa <= 0)
                {
                    const string sqlUltimo = @"
                select isnull(max(Tesoreria_Remesa),0) + 1
                from CxC_REMESAS_TES;";

                    tesoreriaRemesa = conn.QuerySingle<int>(sqlUltimo);
                }

                var fechaInicio = request.fecha_inicio.GetValueOrDefault().Date;
                var fechaCorte = request.fecha_corte.GetValueOrDefault().Date;

                const string sqlInsert = @"
            insert CxC_REMESAS_TES
            (
                Tesoreria_Remesa,
                usuario,
                fecha,
                estado,
                fecha_inicio,
                fecha_corte,
                notas
            )
            values
            (
                @tesoreriaRemesa,
                @usuario,
                dbo.MyGetdate(),
                @estado,
                @fechaInicio,
                @fechaCorte,
                @notas
            );";

                conn.Execute(sqlInsert, new
                {
                    tesoreriaRemesa,
                    usuario = Normalizar(request.usuario),
                    estado = CxCRemesasTesoreriaConstantes.EstadoAbierta,
                    fechaInicio,
                    fechaCorte,
                    notas = request.notas ?? string.Empty
                });

                return tesoreriaRemesa;
        }

        private static int ActualizarRemesa(SqlConnection conn, CxCRemesasTesoreriaRemesaGuardarRequest request)
        {
            var tesoreriaRemesa = request.tesoreria_remesa.GetValueOrDefault();
            var fechaInicio = request.fecha_inicio.GetValueOrDefault().Date;
            var fechaCorte = request.fecha_corte.GetValueOrDefault().Date;

            const string sql = @"
        update CxC_REMESAS_TES
        set usuario = @usuario,
            fecha_inicio = @fechaInicio,
            fecha_corte = @fechaCorte,
            notas = @notas
        where Tesoreria_Remesa = @tesoreriaRemesa;";

            conn.Execute(sql, new
            {
                usuario = Normalizar(request.usuario),
                fechaInicio,
                fechaCorte,
                notas = request.notas ?? string.Empty,
                tesoreriaRemesa
            });

            return tesoreriaRemesa;
        }
        private static bool RemesaExiste(SqlConnection conn, int tesoreriaRemesa)
        {
            const string sql = @"
        select count(1)
        from CxC_REMESAS_TES
        where Tesoreria_Remesa = @tesoreriaRemesa;";

            return conn.QuerySingle<int>(sql, new { tesoreriaRemesa }) > 0;
        }
        private static CxCRemesasTesoreriaRemesaData? ObtenerRemesa(SqlConnection conn, int tesoreriaRemesa)
        {
            const string sql = @"
                select
                    T.Tesoreria_Remesa as tesoreria_remesa,
                    isnull(rtrim(T.usuario), '') as usuario,
                    T.fecha,
                    isnull(rtrim(T.estado), '') as estado,
                    T.fecha_inicio,
                    T.fecha_corte,
                    isnull(D.Casos, 0) as casos,
                    isnull(D.Monto, 0) as monto,
                    isnull(T.notas, '') as notas
                from CxC_REMESAS_TES T
                left join vCxC_Remesa_Tes_Rsm D
                    on T.Tesoreria_Remesa = D.Tesoreria_Remesa
                where T.Tesoreria_Remesa = @tesoreriaRemesa;";

            var remesa = conn.QueryFirstOrDefault<CxCRemesasTesoreriaRemesaData>(sql, new { tesoreriaRemesa });
            return remesa is null ? null : CompletarEstadoDescripcion(remesa);
        }
        private static CxCRemesasTesoreriaRemesaData CompletarEstadoDescripcion(CxCRemesasTesoreriaRemesaData remesa)
        {
            remesa.estado_desc = remesa.estado switch
            {
                CxCRemesasTesoreriaConstantes.EstadoAbierta => CxCRemesasTesoreriaConstantes.EstadoAbiertaDesc,
                CxCRemesasTesoreriaConstantes.EstadoCerrada => CxCRemesasTesoreriaConstantes.EstadoCerradaDesc,
                CxCRemesasTesoreriaConstantes.EstadoTrasladada => CxCRemesasTesoreriaConstantes.EstadoTrasladadaDesc,
                _ => string.Empty
            };

            return remesa;
        }
        private static bool RemesaEstaAbierta(SqlConnection conn, int tesoreriaRemesa)
        {
            const string sql = @"
                select count(1)
                from CxC_REMESAS_TES
                where Tesoreria_Remesa = @tesoreriaRemesa
                  and estado = @estado;";

            return conn.QuerySingle<int>(sql, new
            {
                tesoreriaRemesa,
                estado = CxCRemesasTesoreriaConstantes.EstadoAbierta
            }) > 0;
        }
        private void EjecutarCargaOperacion(SqlConnection conn, SqlTransaction tx,int operacion,int tesoreriaRemesa,string usuario)
        {
            const string sql = @"
                exec spCxC_Cuenta_Desembolso_Carga
                    @Operacion,
                    @Remesa,
                    @Usuario;";

            conn.Execute(sql, new
            {
                Operacion = operacion,
                Remesa = tesoreriaRemesa,
                Usuario = Normalizar(usuario)
            }, tx);
        }
        private void RegistrarBitacora(int CodEmpresa, string usuario, string movimiento, string detalle)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = Normalizar(usuario),
                Movimiento = movimiento,
                DetalleMovimiento = detalle,
                Modulo = ModuloCxC
            });
        }
        private static DynamicParameters CrearParametrosLista(FiltrosLazyLoadData filtros)
        {
            var texto = Normalizar(filtros.filtro);
            var parametros = new DynamicParameters();

            parametros.Add("filtro", string.IsNullOrWhiteSpace(texto) ? null : texto);
            parametros.Add("like", string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%");
            parametros.Add("offset", filtros.pagina);
            parametros.Add("fetch", filtros.paginacion);

            return parametros;
        }
        private static DynamicParameters CrearParametrosOperaciones(FiltrosLazyLoadData filtros,DateTime inicio,DateTime corte,string? codOficina)
        {
            var parametros = CrearParametrosLista(filtros);
            parametros.Add("inicio", inicio);
            parametros.Add("corte", corte);
            parametros.Add("codOficina", Normalizar(codOficina));

            return parametros;
        }
        private static string ObtenerOrdenRemesas(FiltrosLazyLoadData filtros)
        {
            var campo = Normalizar(filtros.sortField).ToLowerInvariant();
            var direccion = filtros.sortOrder == 1 ? "asc" : "desc";

            var columna = campo switch
            {
                "tesoreria_remesa" => "T.Tesoreria_Remesa",
                "usuario" => "T.usuario",
                "fecha" => "T.fecha",
                "estado" => "T.estado",
                "fecha_inicio" => "T.fecha_inicio",
                "fecha_corte" => "T.fecha_corte",
                "casos" => "isnull(D.Casos, 0)",
                "monto" => "isnull(D.Monto, 0)",
                _ => "T.fecha"
            };

            return $"{columna} {direccion}";
        }
        private static string ObtenerOrdenOperaciones(FiltrosLazyLoadData filtros)
        {
            var campo = Normalizar(filtros.sortField).ToLowerInvariant();
            var direccion = filtros.sortOrder == 1 ? "asc" : "desc";

            var columna = campo switch
            {
                "operacion" => CxCRemesasTesoreriaConstantes.Operacion,
                "cod_concepto" => "Cod_Concepto",
                "cedula" => "Cedula",
                "nombre" => "Nombre",
                "monto" => "Monto",
                "desembolso_monto" => "DESEMBOLSO_PENDIENTE",
                "total" => "DESEMBOLSO_PENDIENTE",
                _ => "Operacion"
            };

            return $"{columna} {direccion}";
        }
        private static ErrorDto<CxCRemesasTesoreriaRemesaLista> CrearRespuestaRemesasVacia(string mensaje)
        {
            return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaRemesaLista>(
                mensaje,
                result: new CxCRemesasTesoreriaRemesaLista());
        }
        private static ErrorDto<CxCRemesasTesoreriaOperacionLista> CrearRespuestaOperacionesVacia(string mensaje)
        {
            return DbHelper.CreateErrorResponse<CxCRemesasTesoreriaOperacionLista>(
                mensaje,
                result: new CxCRemesasTesoreriaOperacionLista());
        }
        private static string Normalizar(string? valor)
        {
            return (valor ?? string.Empty).Trim();
        }
        private static DateTime FechaLocal(DateTime fecha)
        {
            return DateTime.SpecifyKind(fecha.Date, DateTimeKind.Local);
        }
        private ErrorDto<CxCRemesasTesoreriaOperacionLista> ObtenerTrasladoLista(int CodEmpresa,int tesoreriaRemesa,FiltrosLazyLoadData filtros,bool esExportar)
        {
            var fechas = ObtenerFechasRemesa(CodEmpresa, tesoreriaRemesa);
            if (fechas.Code != 0)
            {
                return CrearRespuestaOperacionesVacia(
                    fechas.Description ?? CxCRemesasTesoreriaConstantes.MensajeFechasRemesaInvalidas);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var filtro = Normalizar(filtros.filtro);
                var hasFiltro = !string.IsNullOrWhiteSpace(filtro);
                var pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
                var fetch = filtros.paginacion < 0 ? 0 : filtros.paginacion;
                var usarPaginacion = fetch > 0 && !esExportar;

                var parametros = new DynamicParameters();
                parametros.Add("inicio", fechas.Result.inicio);
                parametros.Add("corte", fechas.Result.corte);
                parametros.Add("tesoreriaRemesa", tesoreriaRemesa);
                parametros.Add("filtro", hasFiltro ? filtro : null);
                parametros.Add("like", hasFiltro ? $"%{filtro}%" : null);
                parametros.Add("offset", pagina * fetch);
                parametros.Add("fetch", fetch);

                var orderBy = ObtenerOrdenTraslado(filtros);

                const string where = @"
            where Registro_Fecha between @inicio and @corte
              and Tesoreria_Remesa = @tesoreriaRemesa
              and (
                    @filtro is null
                 or cast(Operacion as varchar(20)) like @like
                 or isnull(Cod_Concepto, '') like @like
                 or isnull(Cedula, '') like @like
                 or isnull(Nombre, '') like @like
              )";

                var sqlCount = $@"
            select count(1)
            from vCxC_Cuentas_Desembolsos_Cargados
            {where};";

                var sqlTotal = $@"
            select isnull(sum(Desembolso_Monto), 0)
            from vCxC_Cuentas_Desembolsos_Cargados
            {where};";

                var sqlLista = $@"
            select
                Operacion as operacion,
                isnull(Cod_Concepto, '') as cod_concepto,
                isnull(Cedula, '') as cedula,
                isnull(Nombre, '') as nombre,
                isnull(Monto, 0) as monto,
                isnull(Desembolso_Monto, 0) as desembolso_monto,
                cast(0 as decimal(18,2)) as desembolsos,
                cast(0 as decimal(18,2)) as otros_giros,
                isnull(Desembolso_Monto, 0) as total
            from vCxC_Cuentas_Desembolsos_Cargados
            {where}
            order by {orderBy}";

                if (usarPaginacion)
                {
                    sqlLista += CxCRemesasTesoreriaConstantes.PaginacionSql;
                }

                return DbHelper.CreateOkResponse(new CxCRemesasTesoreriaOperacionLista
                {
                    total = conn.QuerySingle<int>(sqlCount, parametros),
                    total_monto = conn.QuerySingle<decimal>(sqlTotal, parametros),
                    lista = conn.Query<CxCRemesasTesoreriaOperacionData>(sqlLista, parametros).ToList()
                });
            }
            catch (DbException ex)
            {
                return CrearRespuestaOperacionesVacia($"No fue posible consultar operaciones de traslado. {ex.Message}");
            }
        }

        private static ErrorDto ValidarTrasladoAplicar(CxCRemesasTesoreriaTrasladoAplicarRequest request)
        {
            if (request.tesoreria_remesa <= 0)
            {
                return DbHelper.ErrorResponse(CxCRemesasTesoreriaConstantes.MensajeRemesaRequerida);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(CxCRemesasTesoreriaConstantes.MensajeUsuarioRequerido);
            }

            return DbHelper.OkResponse(CxCRemesasTesoreriaConstantes.MensajeOk);
        }

        private static bool RemesaEstaCerrada(SqlConnection conn, int tesoreriaRemesa)
        {
            const string sql = @"
        select count(1)
        from CxC_REMESAS_TES
        where Tesoreria_Remesa = @tesoreriaRemesa
          and estado = @estado;";

            return conn.QuerySingle<int>(sql, new
            {
                tesoreriaRemesa,
                estado = CxCRemesasTesoreriaConstantes.EstadoCerrada
            }) > 0;
        }

        private static string ObtenerParametroCxC(SqlConnection conn, string codParametro)
        {
            const string sql = @"
        select top 1 rtrim(isnull(valor, ''))
        from CxC_Parametros
        where cod_parametro = @codParametro;";

            return conn.QueryFirstOrDefault<string>(sql, new
            {
                codParametro = Normalizar(codParametro)
            }) ?? string.Empty;
        }

        private static string ObtenerOrdenTraslado(FiltrosLazyLoadData filtros)
        {
            var campo = Normalizar(filtros.sortField).ToLowerInvariant();
            var direccion = filtros.sortOrder == 1 ? "asc" : "desc";

            var columna = campo switch
            {
                "operacion" => CxCRemesasTesoreriaConstantes.Operacion,
                "cod_concepto" => "Cod_Concepto",
                "cedula" => "Cedula",
                "nombre" => "Nombre",
                "monto" => "Monto",
                "desembolso_monto" => "Desembolso_Monto",
                "total" => "Desembolso_Monto",
                _ => "Operacion"
            };

            return $"{columna} {direccion}";
        }

        private static List<CxCRemesasTesoreriaTrasladoRow> ObtenerRowsTraslado(SqlConnection conn,SqlTransaction tx,int tesoreriaRemesa,bool agrupar)
        {
            if (agrupar)
            {
                const string sqlAgrupado = @"
            select
                rtrim(cedula) as cedula,
                rtrim(nombre) as nombre,
                rtrim(Emitir_Tipo) as emitir_tipo,
                isnull(Emitir_Banco, 0) as emitir_banco,
                sum(isnull(Desembolso_Monto, 0)) as desembolso_monto,
                rtrim(isnull(Emitir_Cuenta, '')) as emitir_cuenta,
                rtrim(isnull(cod_unidad, '')) as cod_unidad,
                rtrim(isnull(cod_centro_Costo, '')) as cod_centro_costo,
                rtrim(isnull(BancoCta, '')) as bancocta,
                'Agrupado' as cod_concepto,
                rtrim(min(isnull(Num_Documento, ''))) + '..' + rtrim(max(isnull(Num_Documento, ''))) as num_documento,
                rtrim(convert(varchar(30), min(Operacion))) + '..' + rtrim(convert(varchar(30), max(Operacion))) as operacion,
                '' as conceptocta,
                '' as cod_contrato
            from vCxC_Cuentas_Desembolsos_Traslado_Pendiente
            where Tesoreria_Remesa = @tesoreriaRemesa
            group by
                cedula,
                nombre,
                Emitir_Tipo,
                Emitir_Banco,
                Emitir_Cuenta,
                cod_unidad,
                cod_centro_Costo,
                BancoCta;";

                return conn.Query<CxCRemesasTesoreriaTrasladoRow>(
                    sqlAgrupado,
                    new { tesoreriaRemesa },
                    tx).ToList();
            }

            const string sql = @"
        select
            rtrim(cedula) as cedula,
            rtrim(nombre) as nombre,
            rtrim(Emitir_Tipo) as emitir_tipo,
            isnull(Emitir_Banco, 0) as emitir_banco,
            isnull(Desembolso_Monto, 0) as desembolso_monto,
            rtrim(isnull(Emitir_Cuenta, '')) as emitir_cuenta,
            rtrim(isnull(cod_unidad, '')) as cod_unidad,
            rtrim(isnull(cod_centro_Costo, '')) as cod_centro_costo,
            rtrim(isnull(BancoCta, '')) as bancocta,
            rtrim(isnull(Cod_Concepto, '')) as cod_concepto,
            rtrim(isnull(Num_Documento, '')) as num_documento,
            rtrim(convert(varchar(30), Operacion)) as operacion,
            rtrim(isnull(ConceptoCta, '')) as conceptocta,
            rtrim(isnull(cod_Contrato, '')) as cod_contrato
        from vCxC_Cuentas_Desembolsos_Traslado_Pendiente
        where Tesoreria_Remesa = @tesoreriaRemesa;";

            return conn.Query<CxCRemesasTesoreriaTrasladoRow>(
                sql,
                new { tesoreriaRemesa },
                tx).ToList();
        }

        private void ProcesarTrasladoRow( SqlConnection conn,SqlTransaction tx,CxCRemesasTesoreriaTrasladoContext contexto,CxCRemesasTesoreriaTrasladoRow row)
        {
            long solicitud = 0;

            if (DebeCrearSolicitudTesoreria(row))
            {
                solicitud = CrearMaestroTesoreria(conn, tx, contexto, row);

                CrearAsiento(conn, tx, new CxCRemesasTesoreriaAsientoData
                {
                    solicitud = solicitud,
                    cuenta = row.bancocta,
                    monto = row.desembolso_monto,
                    debehaber = "H",
                    linea = 1,
                    unidad = row.cod_unidad
                });

                if (contexto.Agrupar)
                {
                    CrearAsientosAgrupados(conn, tx, contexto, row, solicitud);
                }
                else
                {
                    CrearAsiento(conn, tx, new CxCRemesasTesoreriaAsientoData
                    {
                        solicitud = solicitud,
                        cuenta = row.conceptocta,
                        monto = row.desembolso_monto,
                        debehaber = "D",
                        linea = 2,
                        unidad = row.cod_unidad
                    });
                }
            }

            ActualizarTesoreriaCuenta(conn, tx, contexto, row, solicitud);

            RegistrarBitacora(
                contexto.CodEmpresa,
                contexto.Usuario,
                CxCRemesasTesoreriaConstantes.BitacoraRegistra,
                contexto.Agrupar
                    ? $"Traspaso a Tesoreria para Desembolso Cliente :{row.cedula}-{row.cod_concepto}"
                    : $"Traspaso a Tesoreria de la {CxCRemesasTesoreriaConstantes.Operacion} y Desembol OP:{row.operacion}");
        }
        private static bool DebeCrearSolicitudTesoreria(CxCRemesasTesoreriaTrasladoRow row)
        {
            var tipo = Normalizar(row.emitir_tipo);

            return row.desembolso_monto > 0
                && (tipo == CxCRemesasTesoreriaConstantes.TipoCheque
                    || tipo == CxCRemesasTesoreriaConstantes.TipoTransferencia);
        }

        private static long CrearMaestroTesoreria(SqlConnection conn,SqlTransaction tx,CxCRemesasTesoreriaTrasladoContext contexto,CxCRemesasTesoreriaTrasladoRow row)
        {
            const string sql = @"
        exec spCxC_Tesoreria_Maestro
            @Cedula,
            @TipoDocumento,
            @Banco,
            @Monto,
            @Codigo,
            @Beneficiario,
            @Detalle1,
            @Detalle2,
            @Cuenta,
            @Fecha,
            @UnidadOmision,
            @ConceptoOmision,
            @UsuarioSolicita,
            @TipoCambio,
            @Divisa,
            @OP,
            @Referencia;";

            return conn.QueryFirstOrDefault<long>(sql, new
            {
                Cedula = row.cedula,
                TipoDocumento = row.emitir_tipo,
                Banco = row.emitir_banco,
                Monto = row.desembolso_monto,
                Codigo = row.cedula,
                Beneficiario = row.nombre,
                Detalle1 = $"Ops:{row.operacion} Cp:{row.cod_concepto}",
                Detalle2 = $"Docs:{row.num_documento}",
                Cuenta = row.emitir_cuenta,
                Fecha = contexto.Fecha,
                UnidadOmision = contexto.UnidadOmision,
                ConceptoOmision = contexto.ConceptoOmision,
                UsuarioSolicita = Normalizar(contexto.Usuario),
                TipoCambio = 1m,
                Divisa = "COL",
                OP = 0,
                Referencia = 0L
            }, tx);
        }

        private static void CrearAsiento(SqlConnection conn,SqlTransaction tx,CxCRemesasTesoreriaAsientoData data)
        {
            const string sql = @"
        insert Tes_Trans_Asiento
        (
            nsolicitud,
            cuenta_contable,
            monto,
            debehaber,
            linea,
            cod_unidad
        )
        values
        (
            @solicitud,
            @cuenta,
            @monto,
            @debehaber,
            @linea,
            @unidad
        );";

            conn.Execute(sql, new
            {
                data.solicitud,
                cuenta = Normalizar(data.cuenta),
                data.monto,
                data.debehaber,
                data.linea,
                unidad = Normalizar(data.unidad)
            }, tx);
        }

        private static void CrearAsientosAgrupados(SqlConnection conn,SqlTransaction tx,CxCRemesasTesoreriaTrasladoContext contexto,CxCRemesasTesoreriaTrasladoRow row,long solicitud)
        {
            const string sql = @"
        select
            rtrim(isnull(cod_concepto, '')) as cod_concepto,
            rtrim(isnull(cedula, '')) as cedula,
            rtrim(isnull(nombre, '')) as nombre,
            rtrim(isnull(Emitir_Tipo, '')) as emitir_tipo,
            isnull(Emitir_Banco, 0) as emitir_banco,
            sum(isnull(Desembolso_Monto, 0)) as desembolso_monto,
            rtrim(isnull(Emitir_Cuenta, '')) as emitir_cuenta,
            rtrim(isnull(cod_unidad, '')) as cod_unidad,
            rtrim(isnull(cod_centro_Costo, '')) as cod_centro_costo,
            rtrim(isnull(cod_Contrato, '')) as cod_contrato,
            rtrim(isnull(ConceptoCta, '')) as conceptocta,
            rtrim(isnull(BancoCta, '')) as bancocta,
            rtrim(min(isnull(Num_Documento, ''))) + '..' + rtrim(max(isnull(Num_Documento, ''))) as num_documento,
            rtrim(convert(varchar(30), min(Operacion))) + '..' + rtrim(convert(varchar(30), max(Operacion))) as operacion
        from vCxC_Cuentas_Desembolsos_Traslado_Pendiente
        where Tesoreria_Remesa = @tesoreriaRemesa
          and Cedula = @cedula
        group by
            cedula,
            nombre,
            Emitir_Tipo,
            Emitir_Banco,
            Emitir_Cuenta,
            cod_unidad,
            cod_centro_Costo,
            cod_concepto,
            cod_Contrato,
            ConceptoCta,
            BancoCta;";

            var rows = conn.Query<CxCRemesasTesoreriaTrasladoDetalleRow>(
                sql,
                new
                {
                    tesoreriaRemesa = contexto.TesoreriaRemesa,
                    row.cedula
                },
                tx).ToList();

            var linea = 2;

            foreach (var detalle in rows)
            {
                CrearAsiento(conn, tx, new CxCRemesasTesoreriaAsientoData
                {
                    solicitud = solicitud,
                    cuenta = detalle.conceptocta,
                    monto = detalle.desembolso_monto,
                    debehaber = "D",
                    linea = linea,
                    unidad = detalle.cod_unidad
                });

                linea++;
            }
        }

        private static void ActualizarTesoreriaCuenta(SqlConnection conn,SqlTransaction tx,CxCRemesasTesoreriaTrasladoContext contexto,CxCRemesasTesoreriaTrasladoRow row,long solicitud)
        {
            if (contexto.Agrupar)
            {
                const string sqlAgrupado = @"
            exec spCxC_Cuenta_Desembolso_TesoreriaId_Agrupado
                @Cedula,
                @Remesa,
                @Solicitud,
                @Usuario;";

                conn.Execute(sqlAgrupado, new
                {
                    Cedula = row.cedula,
                    Remesa = contexto.TesoreriaRemesa,
                    Solicitud = solicitud,
                    Usuario = Normalizar(contexto.Usuario)
                }, tx);

                return;
            }

            const string sql = @"
        exec spCxC_Cuenta_Desembolso_TesoreriaId
            @Operacion,
            @Remesa,
            @Solicitud,
            @Usuario;";

            conn.Execute(sql, new
            {
                Operacion = Convert.ToInt32(row.operacion),
                Remesa = contexto.TesoreriaRemesa,
                Solicitud = solicitud,
                Usuario = Normalizar(contexto.Usuario)
            }, tx);
        }

        private static void ActualizarEstadoRemesaTrasladada(SqlConnection conn,SqlTransaction tx,int tesoreriaRemesa)
        {
            const string sql = @"
        update CxC_REMESAS_TES
        set Estado = @estado
        where Tesoreria_Remesa = @tesoreriaRemesa;";

            conn.Execute(sql, new
            {
                estado = CxCRemesasTesoreriaConstantes.EstadoTrasladada,
                tesoreriaRemesa
            }, tx);
        }

        private sealed class CxCRemesasTesoreriaTrasladoContext
        {
            public int CodEmpresa { get; set; }
            public int TesoreriaRemesa { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public bool Agrupar { get; set; }
            public DateTime Fecha { get; set; }
            public string UnidadOmision { get; set; } = string.Empty;
            public string ConceptoOmision { get; set; } = string.Empty;
        }

        private sealed class CxCRemesasTesoreriaAsientoData
        {
            public long solicitud { get; set; }
            public string cuenta { get; set; } = string.Empty;
            public decimal monto { get; set; }
            public string debehaber { get; set; } = string.Empty;
            public int linea { get; set; }
            public string unidad { get; set; } = string.Empty;
        }
        #endregion
    }
}