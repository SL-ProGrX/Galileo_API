using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrComitesAprobacionesDB
    {
        private readonly PortalDB _portalDB;

        public FrmCrComitesAprobacionesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene comites activos mancomunados para busqueda F4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        cast(ID_COMITE as varchar(20)) as item,
                        rtrim(isnull(DESCRIPCION,'')) as descripcion
                    from COMITES
                    where ESTADO = 1
                      and TIPO_APROBACION = 'M'
                    order by DESCRIPCION;";

                return DbHelper.CreateOkResponse(conn.Query<DropDownListaGenericaModel>(sql).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene el comite activo y el acta abierta asociada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesComite> CR_ComitesAprobaciones_Comite_Obtener(int CodEmpresa, int id_comite)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        C.ID_COMITE as id_comite,
                        rtrim(isnull(C.DESCRIPCION,'')) as descripcion,
                        rtrim(isnull(C.TIPO_APROBACION,'')) as tipo_aprobacion,
                        isnull(C.NAPROBACIONES,1) as naprobaciones,
                        isnull(C.ACTA,0) as acta,
                        isnull((
                        select top 1 cast(isnull(A.ACTA,0) as varchar(30))
                        from CRD_COMITES_ACTAS A
                        where A.ACTA = dbo.fxCrd_Comites_Acta_Abierta(C.ID_COMITE)
                        ), '') as acta_abierta,
                        cast(isnull(C.LINEA_FILTRA,0) as bit) as linea_filtra
                    from COMITES C
                    where C.ESTADO = 1
                      and C.ID_COMITE = @id_comite;";

                var comite = conn.QueryFirstOrDefault<CrComitesAprobacionesComite>(sql, new { id_comite })
                    ?? new CrComitesAprobacionesComite();

                return comite.id_comite <= 0
                    ? DbHelper.CreateErrorResponse<CrComitesAprobacionesComite>("No se encontro el comite indicado.", -2, comite)
                    : DbHelper.CreateOkResponse(comite);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesComite>(ex.Message, -1, new CrComitesAprobacionesComite());
            }
        }

        /// <summary>
        /// Obtiene el comite anterior o siguiente para los botones de desplazamiento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesComite> CR_ComitesAprobaciones_Comite_Scroll(int CodEmpresa, int id_comite, int direccion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    declare @ComiteBuscar int;

                    if @id_comite <= 0
                    begin
                        select top 1 @ComiteBuscar = ID_COMITE
                        from COMITES
                        where ESTADO = 1
                          and TIPO_APROBACION = 'M'
                        order by ID_COMITE asc;
                    end
                    else if @direccion < 0
                    begin
                        select top 1 @ComiteBuscar = ID_COMITE
                        from COMITES
                        where ESTADO = 1
                          and TIPO_APROBACION = 'M'
                          and ID_COMITE < @id_comite
                        order by ID_COMITE desc;
                    end
                    else
                    begin
                        select top 1 @ComiteBuscar = ID_COMITE
                        from COMITES
                        where ESTADO = 1
                          and TIPO_APROBACION = 'M'
                          and ID_COMITE > @id_comite
                        order by ID_COMITE asc;
                    end;

                    if @ComiteBuscar is null
                    begin
                        set @ComiteBuscar = @id_comite;
                    end;

                    select
                        C.ID_COMITE as id_comite,
                        rtrim(isnull(C.DESCRIPCION,'')) as descripcion,
                        rtrim(isnull(C.TIPO_APROBACION,'')) as tipo_aprobacion,
                        isnull(C.NAPROBACIONES,1) as naprobaciones,
                        isnull(C.ACTA,0) as acta,
                        isnull((
                        select top 1 cast(isnull(A.ACTA,0) as varchar(30))
                        from CRD_COMITES_ACTAS A
                        where A.ACTA = dbo.fxCrd_Comites_Acta_Abierta(C.ID_COMITE)
                        ), '') as acta_abierta,
                        cast(isnull(C.LINEA_FILTRA,0) as bit) as linea_filtra
                    from COMITES C
                    where C.ESTADO = 1
                      and C.TIPO_APROBACION = 'M'
                      and C.ID_COMITE = @ComiteBuscar;";

                var comite = conn.QueryFirstOrDefault<CrComitesAprobacionesComite>(
                    sql,
                    new { id_comite, direccion })
                    ?? new CrComitesAprobacionesComite();

                return comite.id_comite <= 0
                    ? DbHelper.CreateErrorResponse<CrComitesAprobacionesComite>("No se encontro el comite indicado.", -2, comite)
                    : DbHelper.CreateOkResponse(comite);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesComite>(ex.Message, -1, new CrComitesAprobacionesComite());
            }
        }

        /// <summary>
        /// Obtiene las actas registradas para el comite.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Actas_Dropdown_Obtener(int CodEmpresa, int id_comite)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    exec spCrd_Comites_Actas_Abiertas @id_comite;";

                return DbHelper.CreateOkResponse(
                    conn.Query(sql, new { id_comite })
                        .Select(row =>
                        {
                            var datos = (IDictionary<string, object>)row;

                            return new DropDownListaGenericaModel
                            {
                                item = Texto(datos, "IdX"),
                                descripcion = Texto(datos, "itmX")
                            };
                        })
                        .ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene usuarios activos para busqueda F4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener(int CodEmpresa, string filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select top 500
                        rtrim(isnull(Nombre,'')) as item,
                        rtrim(isnull(Descripcion,'')) as descripcion
                    from Usuarios
                    where (
                        @filtro = ''
                        or Nombre like '%' + @filtro + '%'
                        or Descripcion like '%' + @filtro + '%'
                      )
                    order by Nombre;";

                return DbHelper.CreateOkResponse(
                    conn.Query<DropDownListaGenericaModel>(sql, new { filtro = filtro.Trim() }).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, new List<DropDownListaGenericaModel>());
            }
        }

        public ErrorDto<List<CrComitesAprobacionesSocio>> CR_ComitesAprobaciones_Socios_Dropdown_Obtener(int CodEmpresa, string filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select top 200
                        rtrim(isnull(Cedula,'')) as cedula,
                        rtrim(isnull(CedulaR,'')) as cedulaR,
                        rtrim(isnull(Nombre,'')) as nombre
                    from Socios
                    where @filtro = ''
                       or Cedula like '%' + @filtro + '%'
                       or CedulaR like '%' + @filtro + '%'
                       or Nombre like '%' + @filtro + '%'
                    order by Nombre;";

                return DbHelper.CreateOkResponse(
                    conn.Query<CrComitesAprobacionesSocio>(sql, new { filtro = (filtro ?? string.Empty).Trim() }).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesSocio>>(ex.Message, -1, new List<CrComitesAprobacionesSocio>());
            }
        }

        /// <summary>
        /// Obtiene las solicitudes o estudios de credito pendientes de resolucion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesSolicitudesLista> CR_ComitesAprobaciones_Solicitudes_Obtener(int CodEmpresa, CrComitesAprobacionesSolicitudRequest request)
        {
            var validacion = ValidarFiltrosSolicitud(request);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesSolicitudesLista>(
                    validacion.Description ?? "Datos requeridos.",
                    validacion.Code.GetValueOrDefault(),
                    new CrComitesAprobacionesSolicitudesLista());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var sql = request.tipo_caso.Trim().Equals("S", StringComparison.OrdinalIgnoreCase)
                    ? QuerySolicitudes(request)
                    : QueryEstudios(request);

                var fechaCorte = request.fecha_corte.Date.AddDays(1).AddTicks(-1);
                var lista = conn.Query<CrComitesAprobacionesSolicitud>(
                    sql,
                    new
                    {
                        request.id_comite,
                        FechaInicio = request.fecha_inicio.Date,
                        FechaCorte = fechaCorte,
                    },
                    commandTimeout: 15).ToList();

                return DbHelper.CreateOkResponse(new CrComitesAprobacionesSolicitudesLista
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesSolicitudesLista>(ex.Message, -1, new CrComitesAprobacionesSolicitudesLista());
            }
        }

        /// <summary>
        /// Obtiene el detalle de credito del caso seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_caso"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesDetalle> CR_ComitesAprobaciones_Detalle_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var tipos = EsSolicitud(tipo_caso)
                    ? new[] { "T", "S" }
                    : new[] { "E", "P" };
                dynamic? row = null;
                foreach (var spTipo in tipos)
                {
                    row = conn.QueryFirstOrDefault(
                        "exec spCrd_Comites_Caso_CRD @Operacion, @Tipo",
                        new { Operacion = operacion.Trim(), Tipo = spTipo });

                    if (row != null)
                    {
                        break;
                    }
                }

                var detalle = MapDetalle(row);

                return DbHelper.CreateOkResponse(detalle);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesDetalle>(ex.Message, -1, new CrComitesAprobacionesDetalle());
            }
        }

        /// <summary>
        /// Obtiene el patrimonio del asociado seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesPatrimonio> CR_ComitesAprobaciones_Patrimonio_Obtener(int CodEmpresa, string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return DbHelper.CreateOkResponse(new CrComitesAprobacionesPatrimonio());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sqlPorcentajeObrero = @"
                    select isnull(PORCENTAJE, 0)
                    from CRD_GARANTIAS_PATRIMONIO
                    where TIPO = 'OBR';";

                const string sqlPorcentajeCapitalizacion = @"
                    select isnull(PORCENTAJE, 0)
                    from CRD_GARANTIAS_PATRIMONIO
                    where TIPO = 'CAP';";

                var porcObrero = conn.QueryFirstOrDefault<decimal>(sqlPorcentajeObrero);
                var porcCapitalizacion = conn.QueryFirstOrDefault<decimal>(sqlPorcentajeCapitalizacion);

                const string sqlPatrimonio = @"
                    select
                        isnull(A.APORTE, 0) as patronal,
                        isnull(A.AHORRO, 0) as aporte_obrero,
                        isnull(A.CAPITALIZA, 0) as capitalizacion,
                        A.FECAPORTE as fecha_corte,
                        (isnull(sum(F.APORTES), 0) + isnull(sum(F.RENDIMIENTO), 0)) as ahorros_extra
                    from AHORRO_CONSOLIDADO A
                    left join FND_CONTRATOS F
                        on A.CEDULA = F.CEDULA
                       and F.ESTADO = 'A'
                    where A.CEDULA = @cedula
                    group by
                        A.APORTE,
                        A.AHORRO,
                        A.CAPITALIZA,
                        A.FECAPORTE;";

                var result = conn.QueryFirstOrDefault<CrComitesAprobacionesPatrimonio>(
                    sqlPatrimonio,
                    new { cedula = cedula.Trim() }) ?? new CrComitesAprobacionesPatrimonio();

                const string sqlSaldoPrestamos = @"
                    select isnull(sum(SALDO), 0)
                    from REG_CREDITOS
                    where CEDULA = @cedula
                      and SALDO > 0
                      and PROCESO = 'N'
                      and ESTADO = 'A'
                      and GARANTIA = 'A';";

                result.saldo_prestamos = conn.QueryFirstOrDefault<decimal>(
                    sqlSaldoPrestamos,
                    new { cedula = cedula.Trim() });

                result.total =
                    result.patronal +
                    result.aporte_obrero +
                    result.ahorros_extra +
                    result.capitalizacion +
                    result.patronal_custodia;

                result.ahorros_fecha =
                    result.aporte_obrero +
                    result.capitalizacion;

                result.disponible_bruto =
                    (result.aporte_obrero * (porcObrero / 100m)) +
                    (result.capitalizacion * (porcCapitalizacion / 100m));

                result.disponible =
                    result.disponible_bruto -
                    result.saldo_prestamos;

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesPatrimonio>(ex.Message, -1, new CrComitesAprobacionesPatrimonio());
            }
        }

        /// <summary>
        /// Obtiene la clasificacion del asociado para el caso seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_caso"></param>
        /// <param name="operacion"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesClasificacion>> CR_ComitesAprobaciones_Clasificacion_Obtener(int CodEmpresa, string tipo_caso, string operacion, string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(operacion))
            {
                return DbHelper.CreateOkResponse(new List<CrComitesAprobacionesClasificacion>());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var estudioCredito = ObtenerCodPreanalisis(conn, tipo_caso, operacion);
                if (string.IsNullOrWhiteSpace(estudioCredito) || estudioCredito == "0")
                {
                    return DbHelper.CreateOkResponse(new List<CrComitesAprobacionesClasificacion>());
                }

                const string sqlLiquidez = @"
                    select (isnull(LIQUIDEZ_CFIANZAS,0) / isnull(nullif(DEVENGADO_MES,0),1)) * 100
                    from CRD_PREA_PREANALISIS
                    where TIPO_PREANALISIS = 'E'
                      and COD_PREANALISIS = @estudioCredito;";

                var liquidez = conn.QueryFirstOrDefault<decimal>(
                    sqlLiquidez,
                    new { estudioCredito });

                var lista = conn.Query<CrComitesAprobacionesClasificacion>(
                    "exec spCRDPreaClasificacionNew @cedula, @liquidez, @estudioCredito",
                    new
                    {
                        cedula = cedula.Trim(),
                        liquidez,
                        estudioCredito
                    }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesClasificacion>>(ex.Message, -1, new List<CrComitesAprobacionesClasificacion>());
            }
        }

        /// <summary>
        /// Obtiene el resumen y detalle de deudas del asociado seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_caso"></param>
        /// <param name="operacion"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesDeudasResponse> CR_ComitesAprobaciones_Deudas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return DbHelper.CreateOkResponse(new CrComitesAprobacionesDeudasResponse());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var response = new CrComitesAprobacionesDeudasResponse();
                var cedulaNormalizada = cedula.Trim();

                CR_ComitesAprobaciones_Deudas_CargarResumen(conn, cedulaNormalizada, response);
                CR_ComitesAprobaciones_Deudas_CargarDeducciones(conn, tipo_caso, operacion, response);
                response.lista = CR_ComitesAprobaciones_Deudas_CargarLista(conn, cedulaNormalizada);

                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesDeudasResponse>(ex.Message, -1, new CrComitesAprobacionesDeudasResponse());
            }
        }

        /// <summary>
        /// Obtiene el resumen y detalle de fianzas del asociado seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesFianzasResponse> CR_ComitesAprobaciones_Fianzas_Obtener(int CodEmpresa, string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return DbHelper.CreateOkResponse(new CrComitesAprobacionesFianzasResponse());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var cedulaNormalizada = cedula.Trim();
                var response = new CrComitesAprobacionesFianzasResponse();

                const string sqlResumen = @"
                    select
                        isnull(sum(R.MONTOAPR), 0) as monto,
                        isnull(sum(R.SALDO), 0) as saldo,
                        isnull(sum(R.CUOTA), 0) as cuota
                    from REG_CREDITOS R
                    where R.SALDO > 0
                      and R.ESTADO = 'A'
                      and R.ID_SOLICITUD in
                      (
                          select ID_SOLICITUD
                          from FIADORES
                          where CEDULAF = @cedula
                            and FIRMA = 'S'
                      );";

                var resumen = conn.QueryFirstOrDefault(sqlResumen, new { cedula = cedulaNormalizada });
                if (resumen != null)
                {
                    response.monto = resumen.monto ?? 0;
                    response.saldo = resumen.saldo ?? 0;
                    response.cuota = resumen.cuota ?? 0;
                }

                const string sqlLista = @"
                    select
                        cast(R.ID_SOLICITUD as varchar(50)) as operacion,
                        rtrim(R.CODIGO) as linea,
                        rtrim(R.CEDULA) as cedula_deudor,
                        rtrim(S.NOMBRE) as nombre,
                        isnull(R.MONTOAPR, 0) as monto,
                        isnull(R.SALDO, 0) as saldo,
                        isnull(R.CUOTA, 0) as cuota
                    from REG_CREDITOS R
                    inner join FIADORES F on R.ID_SOLICITUD = F.ID_SOLICITUD
                    inner join SOCIOS S on R.CEDULA = S.CEDULA
                    where R.SALDO > 0
                      and R.ESTADO = 'A'
                      and F.CEDULAF = @cedula
                      and F.FIRMA = 'S'
                    group by R.ID_SOLICITUD, R.CODIGO, R.CEDULA, S.NOMBRE, R.MONTOAPR, R.SALDO, R.CUOTA
                    order by R.ID_SOLICITUD;";

                response.lista = conn.Query<CrComitesAprobacionesFianza>(
                    sqlLista,
                    new { cedula = cedulaNormalizada }).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesFianzasResponse>(ex.Message, -1, new CrComitesAprobacionesFianzasResponse());
            }
        }

        /// <summary>
        /// Obtiene las refundiciones asociadas al caso seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_caso"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesRefundicion>> CR_ComitesAprobaciones_Refundiciones_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            if (string.IsNullOrWhiteSpace(operacion))
            {
                return DbHelper.CreateOkResponse(new List<CrComitesAprobacionesRefundicion>());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var idSolicitud = ObtenerIdSolicitudCaso(conn, tipo_caso, operacion);
                if (string.IsNullOrWhiteSpace(idSolicitud) || idSolicitud == "0")
                {
                    return DbHelper.CreateOkResponse(new List<CrComitesAprobacionesRefundicion>());
                }

                const string sql = @"
                    select
                        cast(R.ID_SOLICITUD as varchar(50)) as operacion,
                        rtrim(R.CODIGO) as linea,
                        isnull(R.PLAZO, 0) as plazo,
                        isnull(R.MONTOAPR, 0) as monto,
                        isnull(RE.MONTO, 0) as refundicion,
                        isnull(R.CUOTA, 0) as cuota,
                        '' as tipo_movimiento,
                        cast(0 as decimal(18,2)) as interes_corriente,
                        cast(0 as decimal(18,2)) as interes_moratorio,
                        cast(0 as decimal(18,2)) as principal,
                        cast(0 as decimal(18,2)) as cargos,
                        cast(0 as decimal(18,2)) as polizas,
                        isnull(rtrim(R.GARANTIA), '') as garantia
                    from REG_CREDITOS R
                    inner join REFUNDICIONES RE on R.ID_SOLICITUD = RE.ID_SOLICITUD
                    where RE.ID_SOLICITUDR = @id_solicitud
                    order by R.ID_SOLICITUD;";

                var lista = conn.Query<CrComitesAprobacionesRefundicion>(
                    sql,
                    new { id_solicitud = idSolicitud }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesRefundicion>>(ex.Message, -1, new List<CrComitesAprobacionesRefundicion>());
            }
        }

        /// <summary>
        /// Obtiene los desembolsos asociados al caso seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_caso"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesDesembolso>> CR_ComitesAprobaciones_Desembolsos_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            if (string.IsNullOrWhiteSpace(operacion))
            {
                return DbHelper.CreateOkResponse(new List<CrComitesAprobacionesDesembolso>());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var codPreanalisis = ObtenerCodPreanalisis(conn, tipo_caso, operacion);
                if (string.IsNullOrWhiteSpace(codPreanalisis) || codPreanalisis == "0")
                {
                    return DbHelper.CreateOkResponse(new List<CrComitesAprobacionesDesembolso>());
                }

                const string sql = @"
                    select
                        rtrim(DESCRIPCION) as concepto,
                        isnull(MONTO, 0) as monto,
                        isnull(CUOTA, 0) as cuota
                    from CRD_PREA_DETALLE_DESEMBOLSOS
                    where COD_PREANALISIS = @cod_preanalisis
                    order by DESCRIPCION;";

                var lista = conn.Query<CrComitesAprobacionesDesembolso>(
                    sql,
                    new { cod_preanalisis = codPreanalisis }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesDesembolso>>(ex.Message, -1, new List<CrComitesAprobacionesDesembolso>());
            }
        }

        /// <summary>
        /// Obtiene el seguimiento de tags del caso seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_caso"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesSeguimiento>> CR_ComitesAprobaciones_Seguimiento_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var sql = EsSolicitud(tipo_caso)
                    ? @"
                        select T.DESCRIPCION as descripcion, isnull(OT.NOTAS,'') as notas,
                               OT.REGISTRO_FECHA as registro_fecha, OT.REGISTRO_USUARIO as registro_usuario
                        from CRD_OPERACION_TAGS OT
                        inner join CRD_TAGS T on OT.TAG_CODIGO = T.TAG_CODIGO
                        where OT.ID_SOLICITUD = @operacion"
                    : @"
                        select T.DESCRIPCION as descripcion, isnull(OT.NOTAS,'') as notas,
                               OT.REGISTRO_FECHA as registro_fecha, OT.REGISTRO_USUARIO as registro_usuario
                        from CRD_PREA_TAGS OT
                        inner join CRD_TAGS T on OT.TAG_CODIGO = T.TAG_CODIGO
                        where OT.COD_PREANALISIS = @operacion";

                var lista = conn.Query<CrComitesAprobacionesSeguimiento>(sql, new { operacion = operacion.Trim() }).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesSeguimiento>>(ex.Message, -1, new List<CrComitesAprobacionesSeguimiento>());
            }
        }

        /// <summary>
        /// Obtiene los fiadores o persona asociada al estudio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_caso"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesFiador>> CR_ComitesAprobaciones_Fiadores_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var sql = EsSolicitud(tipo_caso)
                    ? @"
                        select F.CEDULAF as cedula, isnull(S.NOMBRE, F.NOMBRE) as nombre
                        from FIADORES F
                        left join SOCIOS S on F.CEDULAF = S.CEDULA
                        where F.ID_SOLICITUD = @operacion"
                    : @"
                        select P.CEDULA as cedula, isnull(S.NOMBRE, P.NOMBRE) as nombre
                        from CRD_PREA_PREANALISIS P
                        left join SOCIOS S on P.CEDULA = S.CEDULA
                        where P.COD_PREANALISIS_REF = @operacion";

                var lista = conn.Query<CrComitesAprobacionesFiador>(sql, new { operacion = operacion.Trim() }).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesFiador>>(ex.Message, -1, new List<CrComitesAprobacionesFiador>());
            }
        }

        /// <summary>
        /// Obtiene datos financieros del fiador.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="estudioCredito"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesFiadorDetalle> CR_ComitesAprobaciones_FiadorDetalle_Obtener(int CodEmpresa, string cedula, string estudioCredito)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        dbo.fxEC_Membresia(S.CEDULA,dbo.MyGetdate()) as membresia,
                        isnull(Est.DESCRIPCION,'') as estado_actual,
                        isnull(El.DESCRIPCION,'No Indica') as estado_laboral_desc,
                        isnull(convert(varchar(10), S.FECHAINGRESO, 103),'') as fecha_ingreso,
                        isnull(I.DESCRIPCION,'') as institucion,
                        isnull(coalesce(UP.DESCRIPCION, Dept.DESCRIPCION),'') as lugar_trabajo,
                        isnull(P.SALARIO_LIQUIDO,0) as salario_liquido,
                        isnull(P.LIQUIDEZ_SIMPLE,0) as liquidez_simple,
                        isnull(P.LIQUIDEZ_CFIANZAS,0) as liquidez_cfianzas,
                        isnull(P.DEVENGADO_MES,0) as devengado_mes
                    from SOCIOS S
                    inner join INSTITUCIONES I on S.COD_INSTITUCION = I.COD_INSTITUCION
                    inner join AFI_ESTADOS_PERSONA Est on S.EstadoActual = Est.COD_ESTADO
                    left join AFI_ESTADO_LABORAL El on S.EstadoLaboral = El.Estado_Laboral
                    left join UPROGRAMATICA UP on S.UP = UP.CODIGO
                    left join AFDepartamentos Dept on S.COD_INSTITUCION = Dept.COD_INSTITUCION
                        and S.COD_DEPARTAMENTO = Dept.COD_DEPARTAMENTO
                    left join CRD_PREA_PREANALISIS P on S.CEDULA = P.CEDULA
                        and P.COD_PREANALISIS_REF = @estudioCredito
                    where S.CEDULA = @cedula;";

                var detalle = conn.QueryFirstOrDefault<CrComitesAprobacionesFiadorDetalle>(
                    sql,
                    new { cedula = cedula.Trim(), estudioCredito = estudioCredito.Trim() })
                    ?? new CrComitesAprobacionesFiadorDetalle();

                return DbHelper.CreateOkResponse(detalle);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesFiadorDetalle>(ex.Message, -1, new CrComitesAprobacionesFiadorDetalle());
            }
        }

    }
}
