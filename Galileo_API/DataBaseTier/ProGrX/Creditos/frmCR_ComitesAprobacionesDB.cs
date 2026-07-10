using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrComitesAprobacionesDB
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
        /// Obtiene las actas registradas para el comité.
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
                    }).ToList();

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

        /// <summary>
        /// Obtiene causas y marca las registradas para el caso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_caso"></param>
        /// <param name="operacion"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesCausa>> CR_ComitesAprobaciones_Causas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string tipo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var joinGestion = EsSolicitud(tipo_caso)
                    ? "left join OPERACION_GESTION G on C.COD_CAUSAS = G.COD_CAUSAS and C.TIPO = G.TIPO and G.ID_SOLICITUD = @operacion"
                    : "left join CRD_PREA_GESTION G on C.COD_CAUSAS = G.COD_CAUSAS and C.TIPO = G.TIPO and G.COD_PREANALISIS = @operacion";

                var sql = $@"
                    select C.COD_CAUSAS as cod_causas,
                           C.DESCRIPCION as descripcion,
                           C.TIPO as tipo,
                           cast(case when G.COD_CAUSAS is null then 0 else 1 end as bit) as seleccionada
                    from OPERACION_CAUSAS C
                    {joinGestion}
                    where C.ESTADO = 1
                      and C.TIPO = @tipo
                    order by C.COD_CAUSAS;";

                var lista = conn.Query<CrComitesAprobacionesCausa>(
                    sql,
                    new { operacion = operacion.Trim(), tipo = tipo.Trim() }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesCausa>>(ex.Message, -1, new List<CrComitesAprobacionesCausa>());
            }
        }

        /// <summary>
        /// Registra la resolucion del comite.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesAprobaciones_Resolucion_Guardar(int CodEmpresa, CrComitesAprobacionesResolucionRequest request)
        {
            var validacion = ValidarResolucion(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var estado = NormalizarEstado(request.estado, out var estadoComite, out var editable);
                conn.Execute(
                    @"
                    exec spCrd_Comites_Resolucion_Add
                        @Comite,
                        @Acta,
                        @Usuario,
                        @Tipo,
                        @Operacion,
                        @Observacion,
                        @Estado,
                        @EstadoComite,
                        @Editable,
                        @AcuerdoJD,
                        @Usuario2,
                        @Usuario3;",
                    new
                    {
                        Comite = request.id_comite,
                        Acta = request.acta.Trim(),
                        Usuario = PrimerUsuario(request),
                        Tipo = request.tipo_caso.Trim(),
                        Operacion = request.operacion.Trim(),
                        Observacion = Truncar(request.observacion, 1000),
                        Estado = estado,
                        EstadoComite = estadoComite,
                        Editable = editable,
                        AcuerdoJD = request.acuerdo_jd.Trim(),
                        Usuario2 = UsuarioEnIndice(request, 1),
                        Usuario3 = UsuarioEnIndice(request, 2)
                    });

                foreach (var usuario in request.usuarios.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    conn.Execute(
                        @"
                        exec spCrd_Comites_Resolucion_Autorizadores_Add
                            @Comite,
                            @Acta,
                            @UsuarioRegistra,
                            @Tipo,
                            @Operacion,
                            @Observacion,
                            @Estado,
                            @UsuarioAutoriza;",
                        new
                        {
                            Comite = request.id_comite,
                            Acta = request.acta.Trim(),
                            UsuarioRegistra = request.usuario_registra.Trim(),
                            Tipo = request.tipo_caso.Trim(),
                            Operacion = request.operacion.Trim(),
                            Observacion = Truncar(request.observacion, 1000),
                            Estado = estado,
                            UsuarioAutoriza = usuario.Trim()
                        });
                }

                return DbHelper.OkResponse("Resolucion registrada correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Registra las causas seleccionadas para el caso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesAprobaciones_Causas_Guardar(int CodEmpresa, CrComitesAprobacionesCausasGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var tabla = EsSolicitud(request.tipo_caso) ? "OPERACION_GESTION" : "CRD_PREA_GESTION";
                var campoOperacion = EsSolicitud(request.tipo_caso) ? "ID_SOLICITUD" : "COD_PREANALISIS";

                conn.Execute(
                    $"delete from {tabla} where TIPO = @tipo and {campoOperacion} = @operacion",
                    new { tipo = request.tipo.Trim(), operacion = request.operacion.Trim() });

                foreach (var causa in request.causas.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                {
                    conn.Execute(
                        $@"
                        insert into {tabla}
                        (COD_CAUSAS, TIPO, {campoOperacion}, CODIGO, REGISTRO_FECHA, REGISTRO_USUARIO)
                        values (@causa, @tipo, @operacion, '', dbo.Mygetdate(), @usuario);",
                        new
                        {
                            causa = causa.Trim(),
                            tipo = request.tipo.Trim(),
                            operacion = request.operacion.Trim(),
                            usuario = request.usuario.Trim()
                        });
                }

                return DbHelper.OkResponse("Causas guardadas correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el acta abierta o seleccionada del comité y sus asistentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="acta"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaActual_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var actaSeleccionada = (acta ?? string.Empty).Trim();

            try
            {
                const string sqlActa = @"
                    declare @acta_consulta varchar(30) = @acta;

                    if @acta_consulta = ''
                    begin
                        select @acta_consulta = cast(dbo.fxCrd_Comites_Acta_Abierta(@id_comite) as varchar(30));
                    end;

                    if @acta_consulta <> '' and not exists (
                        select 1
                        from CRD_COMITES_ACTAS
                        where ID_COMITE = @id_comite
                          and cast(ACTA as varchar(30)) = @acta_consulta
                    )
                    begin
                        select top 1 @acta_consulta = cast(ACTA as varchar(30))
                        from CRD_COMITES_ACTAS
                        where ID_COMITE = @id_comite
                          and SESION_ID = @acta_consulta
                        order by cast(ACTA as int) desc;
                    end;

                    select top 1
                        @id_comite as id_comite,
                        isnull(cast(CA.ACTA as int),0) as id_acta,
                        rtrim(isnull(CA.SESION_ID,'')) as acta,
                        CA.FECHA as fecha,
                        case CA.ESTADO
                            when 'A' then 'Abierta'
                            when 'C' then 'Cerrada'
                            else rtrim(isnull(CA.ESTADO,''))
                        end as estado,
                        rtrim(isnull(CA.NOTAS,'')) as notas
                    from CRD_COMITES_ACTAS CA
                    where CA.ID_COMITE = @id_comite
                      and cast(CA.ACTA as varchar(30)) = @acta_consulta
                    order by isnull(cast(CA.ACTA as int),0) desc;";

                var actual = conn.QueryFirstOrDefault<CrComitesAprobacionesActaActual>(
                    sqlActa,
                    new { id_comite, acta = actaSeleccionada })
                    ?? new CrComitesAprobacionesActaActual { id_comite = id_comite, acta = actaSeleccionada };

                actual.asistencia = actual.id_acta > 0
                    ? ConsultarAsistenciaActa(conn, id_comite, actual.id_acta.ToString())
                    : new List<CrComitesAprobacionesActaAsistencia>();

                return DbHelper.CreateOkResponse(actual);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesActaActual>(ex.Message, -1, new CrComitesAprobacionesActaActual());
            }
        }

        /// <summary>
        /// Crea una nueva acta de comite usando el procedimiento original.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaNueva_Crear(int CodEmpresa, int id_comite, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var nueva = conn.QueryFirstOrDefault(
                    "exec spCrd_Comites_Acta_Nueva @id_comite, @usuario;",
                    new { id_comite, usuario = (usuario ?? string.Empty).Trim() });

                if (nueva == null)
                {
                    return DbHelper.CreateErrorResponse<CrComitesAprobacionesActaActual>("No fue posible generar el acta.", -1, new CrComitesAprobacionesActaActual());
                }

                var campos = (IDictionary<string, object>)nueva;
                var actaValor = ValorCampo(campos, "acta", "ACTA");
                var sesion = ValorCampo(campos, "Sesion", "SESION", "sesion");
                var fecha = ValorCampo(campos, "fecha", "Fecha", "FECHA");

                var actual = new CrComitesAprobacionesActaActual
                {
                    id_comite = id_comite,
                    id_acta = Convert.ToInt32(actaValor ?? 0),
                    acta = Convert.ToString(sesion ?? string.Empty)?.Trim() ?? string.Empty,
                    fecha = fecha == null || fecha == DBNull.Value ? null : Convert.ToDateTime(fecha),
                    estado = "Abierta",
                    notas = string.Empty,
                    asistencia = new List<CrComitesAprobacionesActaAsistencia>()
                };

                return DbHelper.CreateOkResponse(actual);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesActaActual>(ex.Message, -1, new CrComitesAprobacionesActaActual());
            }
        }

        /// <summary>
        /// Guarda la informacion del acta usando el procedimiento original.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesAprobaciones_Acta_Guardar(int CodEmpresa, CrComitesAprobacionesActaGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    exec spCrd_Comites_Acta
                        @id_comite,
                        @acta,
                        @fecha,
                        @notas,
                        @estado,
                        @usuario,
                        @sesion;";

                conn.Execute(
                    sql,
                    new
                    {
                        request.id_comite,
                        acta = (request.acta ?? string.Empty).Trim(),
                        fecha = request.fecha.Date,
                        notas = (request.notas ?? string.Empty).Trim(),
                        estado = EstadoActaSql(request.estado),
                        usuario = (request.usuario ?? string.Empty).Trim(),
                        sesion = (request.sesion ?? string.Empty).Trim()
                    });

                return DbHelper.OkResponse("Acta guardada correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Cierra el acta usando el procedimiento original.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="acta"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesAprobaciones_Acta_Cerrar(int CodEmpresa, int id_comite, string acta, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var cierre = conn.QueryFirstOrDefault(
                    "exec spCrd_Comites_Acta_Cierra @id_comite, @acta, @usuario;",
                    new
                    {
                        id_comite,
                        acta = (acta ?? string.Empty).Trim(),
                        usuario = (usuario ?? string.Empty).Trim()
                    });

                if (cierre == null)
                {
                    return DbHelper.ErrorResponse("No fue posible cerrar el acta.");
                }

                var campos = (IDictionary<string, object>)cierre;
                var pass = Convert.ToInt32(ValorCampo(campos, "Pass", "PASS") ?? 0);
                var mensaje = Convert.ToString(ValorCampo(campos, "Mensaje", "MENSAJE") ?? string.Empty)?.Trim() ?? string.Empty;

                return pass == 1
                    ? DbHelper.OkResponse(string.IsNullOrWhiteSpace(mensaje) ? "Acta cerrada satisfactoriamente." : mensaje)
                    : DbHelper.ErrorResponse(string.IsNullOrWhiteSpace(mensaje) ? "No fue posible cerrar el acta." : mensaje);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la asistencia registrada para el acta seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="acta"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesActaAsistencia>> CR_ComitesAprobaciones_ActaAsistencia_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var actaSeleccionada = (acta ?? string.Empty).Trim();

            try
            {
                return DbHelper.CreateOkResponse(ConsultarAsistenciaActa(conn, id_comite, actaSeleccionada));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesActaAsistencia>>(ex.Message, -1, new List<CrComitesAprobacionesActaAsistencia>());
            }
        }

        private static List<CrComitesAprobacionesActaAsistencia> ConsultarAsistenciaActa(SqlConnection conn, int id_comite, string acta)
        {
            const string sqlAsistencia = @"
                    declare @acta_consulta varchar(30) = @acta;

                    if @acta_consulta = ''
                    begin
                        select @acta_consulta = cast(dbo.fxCrd_Comites_Acta_Abierta(@id_comite) as varchar(30));
                    end;

                    if @acta_consulta <> '' and not exists (
                        select 1
                        from CRD_COMITES_ACTAS
                        where ID_COMITE = @id_comite
                          and cast(ACTA as varchar(30)) = @acta_consulta
                    )
                    begin
                        select top 1 @acta_consulta = cast(ACTA as varchar(30))
                        from CRD_COMITES_ACTAS
                        where ID_COMITE = @id_comite
                          and SESION_ID = @acta_consulta
                        order by cast(ACTA as int) desc;
                    end;

                    exec spCrd_Comites_Acta_Asistencia_Consulta @id_comite, @acta_consulta;";

            return conn.Query(sqlAsistencia, new { id_comite, acta = (acta ?? string.Empty).Trim() }, commandTimeout: 8)
                .Select(row =>
                {
                    var campos = (IDictionary<string, object>)row;
                    var asistencia = campos.TryGetValue("ASISTENCIA", out var asistenciaValor)
                        ? asistenciaValor
                        : campos.TryGetValue("Asistencia", out var asistenciaAlt)
                            ? asistenciaAlt
                            : 0;
                    var cedula = campos.TryGetValue("Cedula", out var cedulaValor)
                        ? cedulaValor
                        : campos.TryGetValue("CEDULA", out var cedulaAlt)
                            ? cedulaAlt
                            : string.Empty;
                    var nombre = campos.TryGetValue("Nombre", out var nombreValor)
                        ? nombreValor
                        : campos.TryGetValue("NOMBRE", out var nombreAlt)
                            ? nombreAlt
                            : string.Empty;

                    return new CrComitesAprobacionesActaAsistencia
                    {
                        seleccionado = Convert.ToInt32(asistencia ?? 0) == 1,
                        cedula = Convert.ToString(cedula ?? string.Empty)?.Trim() ?? string.Empty,
                        nombre = Convert.ToString(nombre ?? string.Empty)?.Trim() ?? string.Empty
                    };
                })
                .ToList();
        }

        /// <summary>
        /// Obtiene el histórico de actas de comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="fecha_inicio"></param>
        /// <param name="fecha_corte"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        private static object? ValorCampo(IDictionary<string, object> campos, params string[] nombres)
        {
            foreach (var nombre in nombres)
            {
                if (campos.TryGetValue(nombre, out var valor))
                {
                    return valor;
                }
            }

            return null;
        }

        private static string EstadoActaSql(string estado)
        {
            var valor = (estado ?? string.Empty).Trim();
            if (valor.Equals("Abierta", StringComparison.OrdinalIgnoreCase))
            {
                return "A";
            }

            if (valor.Equals("Cerrada", StringComparison.OrdinalIgnoreCase))
            {
                return "C";
            }

            return string.IsNullOrWhiteSpace(valor) ? "A" : valor.Substring(0, 1).ToUpperInvariant();
        }

        public ErrorDto<List<CrComitesAprobacionesActaHistorico>> CR_ComitesAprobaciones_ActasHistorico_Obtener(int CodEmpresa, int id_comite, DateTime fecha_inicio, DateTime fecha_corte, string identificacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select distinct
                        @id_comite as id_comite,
                        isnull(cast(A.ACTA as int),0) as id_acta,
                        rtrim(isnull(A.SESION_ID,'')) as sesion,
                        isnull(CA.FECHA, A.FECHA) as fecha,
                        case CA.ESTADO
                            when 'A' then 'Abierta'
                            when 'C' then 'Cerrada'
                            else rtrim(isnull(CA.ESTADO,''))
                        end as estado,
                        rtrim(isnull(C.DESCRIPCION,'')) as comite
                    from vCrd_Comites_Actas A
                    left join CRD_COMITES_ACTAS CA
                        on CA.ACTA = A.ACTA
                       and isnull(CA.SESION_ID,'') = isnull(A.SESION_ID,'')
                    inner join COMITES C on C.ID_COMITE = A.ID_COMITE
                    where A.ID_COMITE = @id_comite
                      and (
                        @identificacion = ''
                        or A.SESION_ID like '%' + @identificacion + '%'
                        or exists (
                            select 1
                            from REG_CREDITOS R
                            where R.ID_COMITE = A.ID_COMITE
                              and R.ACTA = A.ACTA
                              and R.CEDULA like '%' + @identificacion + '%'
                        )
                      )
                    order by isnull(cast(A.ACTA as int),0) desc;";

                var lista = conn.Query<CrComitesAprobacionesActaHistorico>(
                    sql,
                    new
                    {
                        id_comite,
                        FechaInicio = fecha_inicio.Date,
                        FechaCorte = fecha_corte.Date.AddDays(1).AddTicks(-1),
                        identificacion = identificacion.Trim()
                    }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesActaHistorico>>(ex.Message, -1, new List<CrComitesAprobacionesActaHistorico>());
            }
        }

        /// <summary>
        /// Obtiene resoluciones incluidas en el acta seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="acta"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesActaResolucion>> CR_ComitesAprobaciones_ActaResoluciones_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    declare @acta_consulta varchar(30) = @acta;

                    if @acta_consulta = ''
                    begin
                        select @acta_consulta = cast(dbo.fxCrd_Comites_Acta_Abierta(@id_comite) as varchar(30));
                    end;

                    if @acta_consulta <> '' and not exists (
                        select 1
                        from vCrd_Comites_Actas_Resoluciones
                        where ID_COMITE = @id_comite
                          and cast(ACTA as varchar(30)) = @acta_consulta
                    )
                    begin
                        select top 1 @acta_consulta = cast(ACTA as varchar(30))
                        from vCrd_Comites_Actas_Resoluciones
                        where ID_COMITE = @id_comite
                          and SESION_ID = @acta_consulta;
                    end;

                    select
                        ID_COMITE as id_comite,
                        isnull(cast(ACTA as int),0) as id_acta,
                        rtrim(isnull(SESION_ID,'')) as sesion,
                        rtrim(isnull(Cedula,'')) as cedula,
                        rtrim(isnull(Nombre,'')) as nombre,
                        rtrim(isnull(Cod_Linea,'')) as linea,
                        rtrim(isnull(Garantia,'')) as garantia,
                        rtrim(isnull(Estado,'')) as estado,
                        cast(isnull(Expediente,0) as varchar(30)) as operacion
                    from vCrd_Comites_Actas_Resoluciones
                    where ID_COMITE = @id_comite
                      and cast(ACTA as varchar(30)) = @acta_consulta
                    order by Nombre, Cedula;";

                var lista = conn.Query<CrComitesAprobacionesActaResolucion>(
                    sql,
                    new { id_comite, acta = acta.Trim() }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesActaResolucion>>(ex.Message, -1, new List<CrComitesAprobacionesActaResolucion>());
            }
        }

        private static string QuerySolicitudes(CrComitesAprobacionesSolicitudRequest request)
        {
            var estado = EstadoSql("R.ESTADOSOL", request.estado);
            var linea = @"
                and (
                    isnull((select top 1 LINEA_FILTRA from COMITES where ID_COMITE = @id_comite),0) = 0
                    or R.CODIGO in (select CODIGO from CRD_COMITES_LINEAS where ID_COMITE = @id_comite)
                )";

            return $@"
                select
                    dbo.fxSemaforo(R.ID_SOLICITUD,R.ID_COMITE,'S') as semaforo,
                    cast(R.ID_SOLICITUD as varchar(30)) as expediente,
                    R.USERREC as usuario,
                    rtrim(isnull(R.CEDULA,'')) as cedula,
                    rtrim(isnull(S.NOMBRE,'')) as nombre,
                    rtrim(isnull(R.CODIGO,'')) as codigo,
                    isnull(R.MONTOSOL,0) as monto,
                    isnull(R.CUOTA,0) as cuota,
                    isnull(R.PLAZO,0) as plazo,
                    isnull(R.INT,0) as tasa,
                    case R.ESTADOSOL when 'R' then 'Recibido' when 'P' then 'Pendiente' else R.ESTADOSOL end as estado,
                    R.FECHASOL as fecha,
                    rtrim(isnull(R.GARANTIA,'')) as garantia,
                    rtrim(isnull(Gt.DESCRIPCION,'')) as garantia_desc
                from REG_CREDITOS R
                inner join SOCIOS S on S.CEDULA = R.CEDULA
                inner join CRD_COMITES_RNG_GARANTIA G on G.COD_GARANTIA = R.GARANTIA and G.ID_COMITE = R.ID_COMITE
                inner join CRD_GARANTIA_TIPOS Gt on R.GARANTIA = Gt.GARANTIA
                where R.ID_COMITE = @id_comite
                  and R.MONTOSOL between G.RNG_INICIO and G.RNG_CORTE
                  and R.FECHASOL between @FechaInicio and @FechaCorte
                  {estado}
                  and dbo.fxCRDTagAprobacion(R.ID_SOLICITUD) = 0
                  {linea}
                order by R.FECHASOL;";
        }

        private static string QueryEstudios(CrComitesAprobacionesSolicitudRequest request)
        {
            var estado = EstadoSql("P.ESTADO", request.estado);
            return $@"
                select
                    dbo.fxSemaforo(P.COD_PREANALISIS,P.ID_COMITE,'P') as semaforo,
                    cast(P.COD_PREANALISIS as varchar(30)) as expediente,
                    P.USUARIO as usuario,
                    rtrim(isnull(P.CEDULA,'')) as cedula,
                    rtrim(isnull(S.NOMBRE,'')) as nombre,
                    rtrim(isnull(P.COD_LINEA,'')) as codigo,
                    isnull(P.MONTO,0) as monto,
                    isnull(P.CUOTA,0) as cuota,
                    isnull(P.PLAZO,0) as plazo,
                    isnull(P.TASA,0) as tasa,
                    case P.ESTADO when 'R' then 'Recibido' when 'P' then 'Pendiente' else P.ESTADO end as estado,
                    P.FECHA_CREACION as fecha,
                    rtrim(isnull(P.GARANTIA,'')) as garantia,
                    rtrim(isnull(Gt.DESCRIPCION,'')) as garantia_desc
                from CRD_PREA_PREANALISIS P
                inner join SOCIOS S on S.CEDULA = P.CEDULA
                inner join CRD_COMITES_RNG_GARANTIA G on G.COD_GARANTIA = P.GARANTIA and G.ID_COMITE = P.ID_COMITE
                inner join CRD_GARANTIA_TIPOS Gt on P.GARANTIA = Gt.GARANTIA
                where P.TIPO_PREANALISIS = 'E'
                  and P.ID_COMITE = @id_comite
                  and P.MONTO between G.RNG_INICIO and G.RNG_CORTE
                  and P.FECHA_CREACION between @FechaInicio and @FechaCorte
                  {estado}
                  and (
                    isnull((select top 1 LINEA_FILTRA from COMITES where ID_COMITE = @id_comite),0) = 0
                    or P.COD_LINEA in (select CODIGO from CRD_COMITES_LINEAS where ID_COMITE = @id_comite)
                  )
                order by P.FECHA_CREACION;";
        }

        private static string EstadoSql(string campo, string estado)
        {
            return estado switch
            {
                "Recibida" => $"and {campo} = 'R'",
                "Pendiente" => $"and {campo} = 'P'",
                _ => $"and {campo} in ('P','R')"
            };
        }

        private static ErrorDto ValidarFiltrosSolicitud(CrComitesAprobacionesSolicitudRequest request)
        {
            if (request == null || request.id_comite <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar un comite valido.", -2);
            }

            if (!EsSolicitud(request.tipo_caso) && !request.tipo_caso.Trim().Equals("E", StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse("Debe indicar un tipo de caso valido.", -2);
            }

            return DbHelper.OkResponse(string.Empty);
        }

        private static void CR_ComitesAprobaciones_Deudas_CargarResumen(
            IDbConnection conn,
            string cedula,
            CrComitesAprobacionesDeudasResponse response)
        {
            const string sqlResumen = @"
                select
                    isnull(sum(R.SALDO), 0) as total_saldo,
                    isnull(sum(R.CUOTA), 0) as total_cuota
                from REG_CREDITOS R
                where R.SALDO > 0
                  and R.ESTADO = 'A'
                  and R.CEDULA = @cedula;";

            var resumen = conn.QueryFirstOrDefault(sqlResumen, new { cedula });
            if (resumen == null)
            {
                return;
            }

            response.total_saldo = resumen.total_saldo ?? 0;
            response.total_cuota = resumen.total_cuota ?? 0;
        }

        private static void CR_ComitesAprobaciones_Deudas_CargarDeducciones(
            IDbConnection conn,
            string tipoCaso,
            string operacion,
            CrComitesAprobacionesDeudasResponse response)
        {
            var codPreanalisis = ObtenerCodPreanalisis(conn, tipoCaso, operacion);
            if (string.IsNullOrWhiteSpace(codPreanalisis) || codPreanalisis == "0")
            {
                return;
            }

            const string sqlDeducciones = @"
                select isnull(sum(CUOTA_MENSUAL), 0)
                from CRD_PREA_DETALLE_DEDUC
                where COD_PREANALISIS = @cod_preanalisis;";

            response.deducciones = conn.QueryFirstOrDefault<decimal>(
                sqlDeducciones,
                new { cod_preanalisis = codPreanalisis });
        }

        private static string ObtenerCodPreanalisis(IDbConnection conn, string tipoCaso, string operacion)
        {
            if (!EsSolicitud(tipoCaso))
            {
                return operacion?.Trim() ?? string.Empty;
            }

            const string sql = @"
                select isnull(COD_PREANALISIS, 0)
                from CRD_PREA_PREANALISIS
                where TIPO_PREANALISIS = 'E'
                  and ID_SOLICITUD = @id_solicitud;";

            return conn.QueryFirstOrDefault<string>(
                sql,
                new { id_solicitud = operacion?.Trim() ?? string.Empty }) ?? string.Empty;
        }

        private static string ObtenerIdSolicitudCaso(IDbConnection conn, string tipoCaso, string operacion)
        {
            var operacionNormalizada = operacion?.Trim() ?? string.Empty;
            if (EsSolicitud(tipoCaso))
            {
                return operacionNormalizada;
            }

            const string sql = @"
                select top 1 isnull(ID_SOLICITUD, 0)
                from CRD_PREA_PREANALISIS
                where TIPO_PREANALISIS = 'E'
                  and (
                    cast(COD_PREANALISIS as varchar(50)) = @operacion
                    or cast(COD_PREANALISIS_REF as varchar(50)) = @operacion
                  );";

            return conn.QueryFirstOrDefault<string>(
                sql,
                new { operacion = operacionNormalizada }) ?? string.Empty;
        }

        private static List<CrComitesAprobacionesDeuda> CR_ComitesAprobaciones_Deudas_CargarLista(IDbConnection conn, string cedula)
        {
            const string sqlLista = "exec spSIFEstadoCreditos @cedula";
            return conn.Query(sqlLista, new { cedula })
                .Select(CR_ComitesAprobaciones_Deudas_MapearRow)
                .ToList();
        }

        private static CrComitesAprobacionesDeuda CR_ComitesAprobaciones_Deudas_MapearRow(dynamic row)
        {
            var datos = (IDictionary<string, object>)row;
            return new CrComitesAprobacionesDeuda
            {
                semaforo = CR_ComitesAprobaciones_Deudas_ResolverSemaforo(datos),
                operacion = Texto(datos, "id_solicitud"),
                linea = Texto(datos, "codigo"),
                plazo = Decimal(datos, "plazo"),
                monto = Decimal(datos, "MontoApr"),
                saldo = Decimal(datos, "Saldo"),
                cuota = Decimal(datos, "Cuota"),
                monto_atrasado = Decimal(datos, "MoraPrincipal") + Decimal(datos, "MoraInt"),
                primer_deduc = CR_ComitesAprobaciones_Deudas_FormatearPrimerMovimiento(datos),
                ultimo_movimiento = Texto(datos, "UltMovimien"),
                termina = Texto(datos, "Termina"),
                garantia = Texto(datos, "Garantia"),
                estado = Texto(datos, "Estado"),
                proceso = Texto(datos, "ProcesoCod"),
                operacion_referencia = Texto(datos, "Referencia"),
                tasa_original = Decimal(datos, "TasaOriginal"),
                tasa_actual = Decimal(datos, "Tasa"),
            };
        }

        private static string CR_ComitesAprobaciones_Deudas_ResolverSemaforo(IDictionary<string, object> datos)
        {
            var moraCuota = Decimal(datos, "MoraCuota");
            var procesoCod = Texto(datos, "ProcesoCod");
            var estado = Texto(datos, "Estado");
            var referencia = Texto(datos, "Referencia");
            var indicadorCbr = Decimal(datos, "IndicadorCbr");

            if (moraCuota > 0 && procesoCod != "J")
            {
                return "rojo";
            }

            if (procesoCod == "J")
            {
                return "judicial";
            }

            if (!string.IsNullOrWhiteSpace(referencia) && moraCuota == 0)
            {
                return "amarillo";
            }

            if (indicadorCbr > 0)
            {
                return "reversado";
            }

            if (estado.StartsWith('C'))
            {
                return "cancelado";
            }

            return "verde";
        }

        private static string CR_ComitesAprobaciones_Deudas_FormatearPrimerMovimiento(IDictionary<string, object> datos)
        {
            var primerDeduc = Decimal(datos, "prideduc");
            return primerDeduc <= 0 ? string.Empty : primerDeduc.ToString("0000-00");
        }

        private static ErrorDto ValidarResolucion(CrComitesAprobacionesResolucionRequest request)
        {
            if (request == null || request.id_comite <= 0 || string.IsNullOrWhiteSpace(request.acta) || string.IsNullOrWhiteSpace(request.operacion))
            {
                return DbHelper.ErrorResponse("Debe indicar comite, acta y caso.", -2);
            }

            if (!request.usuarios.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                return DbHelper.ErrorResponse("Debe indicar al menos un usuario autorizador.", -2);
            }

            return DbHelper.OkResponse(string.Empty);
        }

        private static bool EsSolicitud(string tipoCaso)
        {
            return tipoCaso.Trim().Equals("S", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizarEstado(string estado, out string estadoComite, out int editable)
        {
            editable = 0;
            estadoComite = "APRO";

            switch (estado.Trim().ToUpperInvariant())
            {
                case "P":
                    estadoComite = "PEND";
                    editable = 1;
                    return "P";
                case "D":
                    estadoComite = "DESC";
                    return "D";
                case "V":
                    estadoComite = "PENVB";
                    editable = 1;
                    return "P";
                case "VL":
                    estadoComite = "PNVBL";
                    editable = 1;
                    return "P";
                default:
                    return "A";
            }
        }

        private static string PrimerUsuario(CrComitesAprobacionesResolucionRequest request)
        {
            return UsuarioEnIndice(request, 0);
        }

        private static string UsuarioEnIndice(CrComitesAprobacionesResolucionRequest request, int index)
        {
            return request.usuarios.Count > index ? request.usuarios[index].Trim() : string.Empty;
        }

        private static string Truncar(string valor, int max)
        {
            var texto = valor?.Trim() ?? string.Empty;
            return texto.Length <= max ? texto : texto[..max];
        }

        private static CrComitesAprobacionesDetalle MapDetalle(dynamic? row)
        {
            if (row == null)
            {
                return new CrComitesAprobacionesDetalle();
            }

            var datos = (IDictionary<string, object>)row;
            return new CrComitesAprobacionesDetalle
            {
                caso_id = Texto(datos, "Caso_Id"),
                cedula = Texto(datos, "Cedula"),
                nombre = Texto(datos, "Nombre"),
                membresia = Texto(datos, "Membresia"),
                codigo = Texto(datos, "Codigo"),
                estado_laboral_desc = Texto(datos, "EstadoLaboral_Desc"),
                estado_persona_desc = Texto(datos, "EstadoPersona_Desc"),
                monto = Decimal(datos, "Monto"),
                cuota = Decimal(datos, "Cuota"),
                monto_girado = Decimal(datos, "monto_girado"),
                desembolso_monto = Decimal(datos, "Desembolso_Monto"),
                desembolso_cuota = Decimal(datos, "DESEMBOLSO_CUOTA"),
                refunde_monto = Decimal(datos, "REFUNDE_MONTO"),
                refunde_cuota = Decimal(datos, "REFUNDE_CUOTA"),
                lugar_trabajo = Texto(datos, "LUGAR_TRABAJO"),
                ca = Decimal(datos, "CA"),
                cod_categoria_asociado = Texto(datos, "COD_CATEGORIA_ASOCIADO")
            };
        }

        private static string Texto(IDictionary<string, object> datos, string campo)
        {
            return TryGetCampo(datos, campo, out var valor) ? Convert.ToString(valor)?.Trim() ?? string.Empty : string.Empty;
        }

        private static decimal Decimal(IDictionary<string, object> datos, string campo)
        {
            if (!TryGetCampo(datos, campo, out var valor) || valor == null || valor == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToDecimal(valor);
        }

        private static bool TryGetCampo(IDictionary<string, object> datos, string campo, out object? valor)
        {
            if (datos.TryGetValue(campo, out var directo))
            {
                valor = directo;
                return true;
            }

            var llave = datos.Keys.FirstOrDefault(x => x.Equals(campo, StringComparison.OrdinalIgnoreCase));
            if (llave != null)
            {
                valor = datos[llave];
                return true;
            }

            valor = null;
            return false;
        }
    }
}
