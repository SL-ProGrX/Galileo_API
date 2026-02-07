using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysFacturaElectronicaDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 10;

        private const string FE_DATE_FMT = "yyyy-MM-dd";
        private const string P_CORTE = "@Corte";
        private const string C_CLIENTE = "@CodCliente";
        private const string TIPO = "@Tipo";
        private const string EST_TODAS = "T";
        private const string SqlOffsetFetchLower = " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";
        private const string COD_REQUERIDO = "Código es requerido.";
        private const string INICIO = "@Inicio";
        private const string USUARIO_REQUERIDO = "Usuario es requerido.";
        private const string USUARIO= "@Usuario";
        private const string P_CLIENTE_ID = "@ClienteId";
        private const string P_CODIGO = "@Codigo";
        private const string P_MOVIMIENTO = "@Movimiento";
        private const string P_TIPO = "@Tipo";
        private const string P_USUARIO = "@Usuario";

        public FrmSysFacturaElectronicaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Lista clientes para Facturación Electrónica (SYS_FE_PARAMETROS).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Clientes_DropDown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                SELECT
                    RTRIM(COD_CLIENTE)  AS item,
                    RTRIM(RAZON_SOCIAL) AS descripcion
                FROM SYS_FE_PARAMETROS
                ORDER BY RAZON_SOCIAL;";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }
        /// <summary>
        /// F4 CABYS (vINV_Cabys) - filtro por código o descripción.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Cabys_DropDown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
            SELECT
                RTRIM(COD_BYS)      AS item,
                RTRIM(DESCRIPCION)  AS descripcion
            FROM vINV_Cabys
            ORDER BY DESCRIPCION;";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Lista cortes realizados por cliente (SYS_FE_CLIENTE_CORTES) con lazy load.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<FeCortesLista> FE_Cortes_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtrosResp = TryParseFiltros(parametros, out var filtros, out var error);
            if (!filtrosResp)
                return DbHelper.CreateErrorResponse<FeCortesLista>(error!);

            var response = DbHelper.CreateOkResponse(new FeCortesLista());
            response.Result ??= new FeCortesLista();

            try
            {
                var codCliente = (ExtractKeyFromParametros(filtros!.parametros, "cod_cliente") ?? "").Trim();
                if (string.IsNullOrWhiteSpace(codCliente))
                    return DbHelper.CreateErrorResponse<FeCortesLista>("parametros.cod_cliente es requerido para listar cortes.");

                var filtroGlobal = (filtros.filtro ?? "").Trim();
                var hasTexto = !string.IsNullOrWhiteSpace(filtroGlobal);
                var like = hasTexto ? $"%{filtroGlobal}%" : null;

                bool exportAll = IsExportAll(filtros);

                int pagina = NormalizePagina(filtros.pagina, exportAll);
                int paginacion = NormalizePaginacion(filtros.paginacion, exportAll, 30);

                int offset = exportAll ? 0 : (pagina - 1) * paginacion;
                int fetch = exportAll ? 1000000 : paginacion;

                string sf = (filtros.sortField ?? "").Trim().ToLowerInvariant();
                int so = (filtros.sortOrder == 1) ? 1 : 0;

                const string sqlCount = @"
                select count(1)
                from SYS_FE_CLIENTE_CORTES
                where COD_CLIENTE = @cod_cliente
                  and (
                       @texto is null
                    or REGISTRO_USUARIO like @like
                    or cast(CORTE_ID as varchar(20)) like @like
                  );";

                var sqlData = @"
                select
                    CORTE_ID as corte_id,
                    CORTE as corte,
                    FACTURACION as facturacion,
                    case when METODO_BASE = 'D' then 'Devengado' else 'Efectivo' end as metodo,
                    REGISTRO_USUARIO as reg_usuario,
                    REGISTRO_FECHA as reg_fecha
                from SYS_FE_CLIENTE_CORTES
                where COD_CLIENTE = @cod_cliente
                  and (
                       @texto is null
                    or REGISTRO_USUARIO like @like
                    or cast(CORTE_ID as varchar(20)) like @like
                  )
                order by
                    case when @sf = 'corte_id' and @so = 1 then CORTE_ID end asc,
                    case when @sf = 'corte_id' and @so = 0 then CORTE_ID end desc,

                    case when @sf = 'corte' and @so = 1 then CORTE end asc,
                    case when @sf = 'corte' and @so = 0 then CORTE end desc,

                    case when @sf = 'facturacion' and @so = 1 then FACTURACION end asc,
                    case when @sf = 'facturacion' and @so = 0 then FACTURACION end desc,

                    case when (@sf = 'metodo' or @sf = 'metodo_base') and @so = 1 then METODO_BASE end asc,
                    case when (@sf = 'metodo' or @sf = 'metodo_base') and @so = 0 then METODO_BASE end desc,

                    case when (@sf = 'reg_usuario' or @sf = 'registro_usuario') and @so = 1 then REGISTRO_USUARIO end asc,
                    case when (@sf = 'reg_usuario' or @sf = 'registro_usuario') and @so = 0 then REGISTRO_USUARIO end desc,

                    case when (@sf = 'reg_fecha' or @sf = 'registro_fecha') and @so = 1 then REGISTRO_FECHA end asc,
                    case when (@sf = 'reg_fecha' or @sf = 'registro_fecha') and @so = 0 then REGISTRO_FECHA end desc,

                    CORTE desc" + SqlOffsetFetchLower + @";";

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var p = new DynamicParameters();
                p.Add("@cod_cliente", codCliente, DbType.String);
                p.Add("@texto", hasTexto ? filtroGlobal : null, DbType.String);
                p.Add("@like", like, DbType.String);
                p.Add("@sf", sf, DbType.String);
                p.Add("@so", so, DbType.Int32);
                p.Add("@offset", offset, DbType.Int32);
                p.Add("@fetch", fetch, DbType.Int32);

                response.Result.total = conn.QuerySingle<int>(sqlCount, p);
                response.Result.lista = conn.Query<FeCorteItem>(sqlData, p).ToList();

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<FeCortesLista>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta lista de cortes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<FeCortesLista> FE_Cortes_Lista_Export(int CodEmpresa, string parametros)
        {
            if (!TryParseFiltros(parametros, out var filtros, out var error))
                return DbHelper.CreateErrorResponse<FeCortesLista>(error!);

            filtros!.pagina = 0;
            filtros.paginacion = 0;

            return FE_Cortes_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Registra/Reprocesa corte usando spCrd_Facturacion_Corte.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto FE_Corte_Registrar(int CodEmpresa, FeRegistrarCorteDto dto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var codCliente = (dto.cod_cliente ?? "").Trim();
                var usuario = (dto.usuario ?? "").Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(codCliente))
                    return DbHelper.ErrorResponse("cod_cliente es requerido.");

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse(USUARIO_REQUERIDO);

                if (!DateTime.TryParseExact((dto.fecha_corte ?? "").Trim(), FE_DATE_FMT,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaCorte))
                    return DbHelper.ErrorResponse("fecha_corte inválida. Formato esperado: YYYY-MM-DD.");

                if (!DateTime.TryParseExact((dto.fecha_factura ?? "").Trim(), FE_DATE_FMT,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaFactura))
                    return DbHelper.ErrorResponse("fecha_factura inválida. Formato esperado: YYYY-MM-DD.");

                var p = new DynamicParameters();
                p.Add("@Cliente", codCliente, DbType.String);
                p.Add(P_CORTE, DayEnd(fechaCorte), DbType.DateTime);
                p.Add(USUARIO, usuario, DbType.String);
                p.Add("@FechaFactura", DateTime.SpecifyKind(fechaFactura, DateTimeKind.Unspecified), DbType.DateTime);

                conn.Execute("spCrd_Facturacion_Corte", p, commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"FE Corte Cliente: {codCliente} Corte: {dto.fecha_corte} Factura: {dto.fecha_factura}",
                    Movimiento = "Registra Corte - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Ok");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Lista facturas (Detalle) usando spProGrX_Facturas_Consulta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<FeFacturasLista> FE_Facturas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            if (!TryParseFiltros(parametros, out var filtros, out var error))
                return DbHelper.CreateErrorResponse<FeFacturasLista>(error!);

            var response = DbHelper.CreateOkResponse(new FeFacturasLista());
            response.Result ??= new FeFacturasLista();

            var dto = ReadFacturasParams(filtros!);

            if (!ValidateFacturasParams(dto, out var ini, out var fin, out var errMsg))
                return DbHelper.CreateErrorResponse<FeFacturasLista>(errMsg!);

            var exportAll = IsExportAll(filtros!);

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var p = new DynamicParameters();
                p.Add(C_CLIENTE, dto.cod_cliente, DbType.String);
                p.Add("@FiltroFactura", dto.factura, DbType.String);
                p.Add("@FiltroId", dto.identificacion, DbType.String);
                p.Add("@FiltroRazonSocial", dto.nombre, DbType.String);
                p.Add(INICIO, ini, DbType.DateTime);
                p.Add(P_CORTE, fin, DbType.DateTime);
                p.Add("@Estado", NormalizeEstado(dto.estado), DbType.String);

                var data = conn.Query<FeFacturaItem>(
                    "spProGrX_Facturas_Consulta",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                SortFacturas(data, filtros!.sortField, filtros.sortOrder);

                response.Result.total = data.Count;

                if (exportAll)
                {
                    response.Result.lista = data;
                    return response;
                }

                var pagina = NormalizePagina(filtros!.pagina, exportAll);
                var paginacion = NormalizePaginacion(filtros!.paginacion, exportAll, 30);
                response.Result.lista = Page(data, pagina, paginacion);

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<FeFacturasLista>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta lista de facturas (Detalle).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<FeFacturasLista> FE_Facturas_Lista_Export(int CodEmpresa, string parametros)
        {
            if (!TryParseFiltros(parametros, out var filtros, out var error))
                return DbHelper.CreateErrorResponse<FeFacturasLista>(error!);

            filtros!.pagina = 0;
            filtros.paginacion = 0;

            return FE_Facturas_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Obtiene el detalle de líneas de una factura usando spProGrX_Factura_Detalle.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codCliente"></param>
        /// <param name="idFactura"></param>
        /// <returns></returns>
        public ErrorDto<List<FeFacturaDetalleItem>> FE_Factura_Detalle_Obtener(int CodEmpresa, string codCliente, int idFactura)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var resp = DbHelper.CreateOkResponse(new List<FeFacturaDetalleItem>());

            try
            {
                codCliente = (codCliente ?? "").Trim();
                if (string.IsNullOrWhiteSpace(codCliente))
                    return DbHelper.CreateErrorResponse<List<FeFacturaDetalleItem>>("codCliente es requerido.");

                if (idFactura <= 0)
                    return DbHelper.CreateErrorResponse<List<FeFacturaDetalleItem>>("idFactura inválido.");

                var p = new DynamicParameters();
                p.Add(C_CLIENTE, codCliente, DbType.String);
                p.Add("@IdFactura", idFactura, DbType.Int32);

                resp.Result = conn.Query<FeFacturaDetalleItem>(
                    "spProGrX_Factura_Detalle",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return resp;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<FeFacturaDetalleItem>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el resumen (cabecera + lista) usando spProGrX_Facturas_Consulta_Rsm.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<FeFacturasResumen> FE_Facturas_Resumen_Obtener(int CodEmpresa, string parametros)
        {
            if (!TryParseFiltros(parametros, out var filtros, out var error))
                return DbHelper.CreateErrorResponse<FeFacturasResumen>(error!);

            var resp = DbHelper.CreateOkResponse(new FeFacturasResumen());
            resp.Result ??= new FeFacturasResumen();

            try
            {
                var dto = ReadFacturasParams(filtros!);

                if (!ValidateFacturasParams(dto, out var ini, out var fin, out var errMsg))
                    return DbHelper.CreateErrorResponse<FeFacturasResumen>(errMsg ?? "Parámetros inválidos.");

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var pBase = BuildFacturasResumenBaseParams(dto, ini, fin);

                var head = ExecuteFacturasResumenHead(conn, pBase);

                resp.Result.cabecera = MapFacturasResumenCabecera(head);
                resp.Result.lista = ExecuteFacturasResumenDetalle(conn, pBase);

                SortFacturasResumen(resp.Result.lista, filtros!.sortField, filtros.sortOrder);

                return resp;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<FeFacturasResumen>(ex.Message);
            }
        }
        /// <summary>
        /// Exporta resumen (cabecera + lista) usando spProGrX_Facturas_Consulta_Rsm.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<FeFacturasResumen> FE_Facturas_Resumen_Export(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<FeFacturasResumen>(jex.Message);
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return FE_Facturas_Resumen_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Obtiene estados para filtro de Facturas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Facturas_Estados_DropDown_Obtener(int CodEmpresa)
        {
            var lista = new List<DropDownListaGenericaModel>
        {
            new DropDownListaGenericaModel { item = EST_TODAS, descripcion = "TODAS" },
            new DropDownListaGenericaModel { item = "A", descripcion = "ACEPTADAS" },
            new DropDownListaGenericaModel { item = "P", descripcion = "PENDIENTES" },
            new DropDownListaGenericaModel { item = "R", descripcion = "RECHAZADAS" }
        };

            return DbHelper.CreateOkResponse(lista);
        }
        /// <summary>
        /// Obtiene personas (Cedula/Nombre) para F4 de Identificación (Facturas/Clientes).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Personas_DropDown_Obtener(int CodEmpresa, string? filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                filtro = (filtro ?? "").Trim();
                var has = !string.IsNullOrWhiteSpace(filtro);
                var like = has ? $"%{filtro}%" : null;

                const string sql = @"
                select
                    rtrim(CEDULA) as item,
                    rtrim(NOMBRE) as descripcion
                from SYS_FE_CLIENTES
                where (
                      @filtro is null
                   or CEDULA like @like
                   or NOMBRE like @like
                )
                order by NOMBRE;";

                var p = new DynamicParameters();
                p.Add("@filtro", has ? filtro : null, DbType.String);
                p.Add("@like", like, DbType.String);

                var lista = conn.Query<DropDownListaGenericaModel>(sql, p).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }


        /// <summary>
        /// Lista clientes para el TAB "Clientes" (SYS_FE_CLIENTES) con lazy load.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<FeClientesLista> FE_Clientes_Lista_Obtener(int CodEmpresa, string parametros)
        {
            if (!TryParseFiltros(parametros, out var filtros, out var error))
                return DbHelper.CreateErrorResponse<FeClientesLista>(error!);

            var response = DbHelper.CreateOkResponse(new FeClientesLista());
            response.Result ??= new FeClientesLista();

            var codCliente = (ExtractKeyFromParametros(filtros!.parametros, "cod_cliente") ?? "").Trim();
            if (string.IsNullOrWhiteSpace(codCliente))
                return DbHelper.CreateErrorResponse<FeClientesLista>("Seleccione un cliente.");

            var identificacion = (ExtractKeyFromParametros(filtros!.parametros, "identificacion") ?? "").Trim();
            if (string.IsNullOrWhiteSpace(identificacion))
                return DbHelper.CreateErrorResponse<FeClientesLista>("Digite la identificación.");

            var exportAll = IsExportAll(filtros!);

            try
            {
                using var connLocal = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sqlCfg = @"
                select
                    rtrim(isnull(ACC_SERVER,'')) as portal_server,
                    rtrim(isnull(ACC_DB,''))     as portal_db,
                    rtrim(isnull(ACC_USR,''))    as portal_user,
                    rtrim(isnull(ACC_KEY,''))    as portal_key
                from SYS_FE_PARAMETROS
                where COD_CLIENTE = @cod_cliente;";

                var cfg = connLocal.QueryFirstOrDefault<dynamic>(sqlCfg, new { cod_cliente = codCliente });

                if (cfg == null)
                    return DbHelper.CreateErrorResponse<FeClientesLista>("No se encontró configuración del portal para el cliente seleccionado.");

                string portalServer = (cfg.portal_server ?? "").ToString().Trim();
                string portalDb = (cfg.portal_db ?? "").ToString().Trim();
                string portalUser = (cfg.portal_user ?? "").ToString().Trim();
                string portalKey = (cfg.portal_key ?? "").ToString().Trim();

                if (string.IsNullOrWhiteSpace(portalServer) ||
                    string.IsNullOrWhiteSpace(portalDb) ||
                    string.IsNullOrWhiteSpace(portalUser) ||
                    string.IsNullOrWhiteSpace(portalKey))
                {
                    return DbHelper.CreateErrorResponse<FeClientesLista>("Credenciales del portal proveedor incompletas para el cliente seleccionado.");
                }

                var csb = new SqlConnectionStringBuilder
                {
                    DataSource = portalServer,
                    InitialCatalog = portalDb,
                    UserID = portalUser,
                    Password = portalKey,
                    ConnectTimeout = 15,
                    Encrypt = false,
                    TrustServerCertificate = true
                };

                using var connPortal = new SqlConnection(csb.ConnectionString);
                connPortal.Open();

                var exists = connPortal.QueryFirstOrDefault<int>("select case when object_id('dbo.IW_CLIENTE','U') is null then 0 else 1 end;");
                if (exists == 0)
                    return DbHelper.CreateErrorResponse<FeClientesLista>("No existe la tabla IW_CLIENTE en el portal proveedor del cliente seleccionado.");

                                const string sql = @"
                select
                    rtrim(isnull(convert(varchar(30), CODIGO), '')) as id_prov,
                    rtrim(isnull(TIPO_CLIENTE, ''))                as tipo_id,
                    rtrim(isnull(CEDULA, ''))                      as identificacion,
                    rtrim(isnull(RAZON_SOCIAL, ''))                as razon_social,
                    rtrim(isnull(EMAIL1, ''))                      as email1,
                    rtrim(isnull(EMAIL2, ''))                      as email2,
                    ''                                             as telefono1,
                    ''                                             as telefono2,
                    ''                                             as provincia,
                    ''                                             as canton,
                    ''                                             as distrito,
                    ''                                             as barrio,
                    ''                                             as direccion
                from IW_CLIENTE
                where ID_CLIENTE_ORIGEN = @cod_cliente
                  and rtrim(isnull(CEDULA,'')) = @identificacion
                order by [ID];";

                var p = new DynamicParameters();
                p.Add("@cod_cliente", codCliente, DbType.String);
                p.Add("@identificacion", identificacion, DbType.String);

                var data = connPortal.Query<FeClienteItem>(sql, p).ToList();

                SortClientes(data, filtros!.sortField, filtros.sortOrder);

                response.Result.total = data.Count;

                if (exportAll)
                {
                    response.Result.lista = data;
                    return response;
                }

                var pagina = NormalizePagina(filtros!.pagina, exportAll);
                var paginacion = NormalizePaginacion(filtros!.paginacion, exportAll, 30);
                response.Result.lista = PageClientes(data, pagina, paginacion);

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<FeClientesLista>(ex.Message);
            }
        }



        private static void SortClientes(List<FeClienteItem> data, string? sortField, int sortOrder)
        {
            var sf = (sortField ?? "").Trim().ToLowerInvariant();
            int dir = (sortOrder == 1) ? 1 : -1;

            int Cmp(string? a, string? b) =>
                dir * string.Compare(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(sf))
            {
                data.Sort((a, b) => Cmp(a.identificacion, b.identificacion));
                return;
            }

            data.Sort((a, b) => sf switch
            {
                "id_prov" => Cmp(a.id_prov, b.id_prov),
                "tipo_id" => Cmp(a.tipo_id, b.tipo_id),
                "identificacion" => Cmp(a.identificacion, b.identificacion),
                "razon_social" => Cmp(a.razon_social, b.razon_social),
                "nombre" => Cmp(a.razon_social, b.razon_social),
                "email1" => Cmp(a.email1, b.email1),
                "email2" => Cmp(a.email2, b.email2),
                "telefono1" => Cmp(a.telefono1, b.telefono1),
                "telefono2" => Cmp(a.telefono2, b.telefono2),
                "provincia" => Cmp(a.provincia, b.provincia),
                "canton" => Cmp(a.canton, b.canton),
                "distrito" => Cmp(a.distrito, b.distrito),
                "barrio" => Cmp(a.barrio, b.barrio),
                "direccion" => Cmp(a.direccion, b.direccion),
                _ => Cmp(a.identificacion, b.identificacion)
            });
        }

        private static List<FeClienteItem> PageClientes(List<FeClienteItem> data, int pagina, int paginacion)
        {
            if (data.Count == 0) return new List<FeClienteItem>();

            if (pagina <= 0) pagina = 1;
            if (paginacion <= 0) paginacion = 30;

            int start = (pagina - 1) * paginacion;
            if (start >= data.Count) return new List<FeClienteItem>();

            int count = Math.Min(paginacion, data.Count - start);
            return data.GetRange(start, count);
        }
        /// <summary>
        /// Exporta lista de clientes (TAB Clientes).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<FeClientesLista> FE_Clientes_Lista_Export(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<FeClientesLista>(jex.Message);
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return FE_Clientes_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }
        /// <summary>
        /// Obtiene configuración de Facturación Electrónica por cliente (SYS_FE_PARAMETROS).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<FeConfiguracionModel> FE_Configuracion_Obtener(int CodEmpresa, string codigo)
        {
            var resp = DbHelper.CreateOkResponse(new FeConfiguracionModel());
            resp.Result ??= new FeConfiguracionModel();

            try
            {
                codigo = (codigo ?? "").Trim();
                if (string.IsNullOrWhiteSpace(codigo))
                    return DbHelper.CreateErrorResponse<FeConfiguracionModel>(COD_REQUERIDO);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                select
                    rtrim(COD_CLIENTE) as codigo,
                    rtrim(TIPO_ID) as tipo_id,
                    rtrim(CEDULA) as identificacion,
                    rtrim(RAZON_SOCIAL) as razon_social,
                    cast(isnull(ACTIVO,0) as smallint) as activa,
                    FECHA_INICIO as inicio,

                    rtrim(isnull(NOTIFICA_EMAIL,'')) as notifica_email,
                    cast(isnull(NOTIFICA_EMAIL_ACTIVO,0) as smallint) as notifica_activa,
                    cast(isnull(NOTIFICA_CLIENTE,0) as smallint) as notifica_cliente,

                    isnull(CONSECUTIVO_FE,0) as consec_fe,
                    isnull(CONSECUTIVO_NC,0) as consec_nc,
                    isnull(CONSECUTIVO_ND,0) as consec_nd,
                    isnull(CONSECUTIVO_TE,0) as consec_te,

                    rtrim(isnull(ACC_CODIGO,'')) as portal_codigo,
                    rtrim(isnull(ACC_SERVER,'')) as portal_server,
                    rtrim(isnull(ACC_DB,'')) as portal_db,
                    rtrim(isnull(ACC_USR,'')) as portal_user,
                    rtrim(isnull(ACC_KEY,'')) as portal_key,

                    rtrim(isnull(METODO_BASE,'')) as metodo,
                    cast(isnull(INCLUYE_POLIZAS,0) as smallint) as i_polizas,
                    cast(isnull(INCLUYE_PRINCIPAL,0) as smallint) as i_principal,

                    cast(isnull(MAX_MONTO_APL,0) as smallint) as mnt_max_apl,
                    cast(isnull(MAX_MONTO,0) as decimal(18,2)) as mnt_max,

                    rtrim(isnull(CABYS,'')) as cabys,
                    rtrim(isnull(SUCURSAL,'')) as sucursal,
                    rtrim(isnull(TERMINAL,'')) as terminal
                from SYS_FE_PARAMETROS
                where COD_CLIENTE = @codigo;";

                var p = new DynamicParameters();
                p.Add("@codigo", codigo, DbType.String);

                var row = conn.QueryFirstOrDefault<FeConfiguracionModel>(sql, p);
                if (row != null) resp.Result = row;

                return resp;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<FeConfiguracionModel>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda/Actualiza configuración (spSYS_FE_PARAMETROS_Registra) con TipoMov='A'.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto FE_Configuracion_Guardar(int CodEmpresa, FeConfiguracionGuardarDto dto)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var codigo = (dto.codigo ?? "").Trim();
                var tipoId = (dto.tipo_id ?? "").Trim();
                var identificacion = (dto.identificacion ?? "").Trim();
                var razonSocial = (dto.razon_social ?? "").Trim();
                var notificaEmail = (dto.notifica_email ?? "").Trim();
                var portalCodigo = (dto.portal_codigo ?? "").Trim();
                var portalServer = (dto.portal_server ?? "").Trim();
                var portalDb = (dto.portal_db ?? "").Trim();
                var portalUser = (dto.portal_user ?? "").Trim();
                var portalKey = (dto.portal_key ?? "").Trim();
                var usuario = (dto.usuario ?? "").Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(codigo))
                    return DbHelper.ErrorResponse(COD_REQUERIDO);

                if (string.IsNullOrWhiteSpace(tipoId))
                    return DbHelper.ErrorResponse("tipo_id es requerido.");

                if (string.IsNullOrWhiteSpace(identificacion))
                    return DbHelper.ErrorResponse("identificacion es requerido.");

                if (string.IsNullOrWhiteSpace(razonSocial))
                    return DbHelper.ErrorResponse("razon_social es requerido.");

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse(USUARIO_REQUERIDO);

                if (!DateTime.TryParseExact((dto.inicio ?? "").Trim(), FE_DATE_FMT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var inicio))
                    return DbHelper.ErrorResponse("inicio inválido. Formato esperado: YYYY-MM-DD.");

          
                var metodoRaw = (dto.metodo ?? "").Trim().ToUpperInvariant();
                string metodo;
                if (metodoRaw == "E") metodo = "E";
                else metodo = "D";

                var sucursal = (dto.sucursal ?? "").Trim();
                if (string.IsNullOrWhiteSpace(sucursal)) sucursal = "2";
                if (sucursal.Length > 1) sucursal = sucursal.Substring(0, 1);

                var terminal = (dto.terminal ?? "").Trim();
                if (string.IsNullOrWhiteSpace(terminal)) terminal = "00001";
                if (terminal.Length > 5) terminal = terminal.Substring(0, 5);

                var cabys = (dto.cabys ?? "").Trim();
                if (cabys.Length > 30) cabys = cabys.Substring(0, 30);

                var p = new DynamicParameters();

                p.Add("@Codigo", codigo, DbType.String);
                p.Add("@TipoID", tipoId, DbType.String);
                p.Add("@Identificacion", identificacion, DbType.String);
                p.Add("@Razon_Social", razonSocial, DbType.String);
                p.Add("@Activa", dto.activa, DbType.Int16);
                var inicio235959 = new DateTime(inicio.Year, inicio.Month, inicio.Day, 23, 59, 59, DateTimeKind.Unspecified);
                p.Add(INICIO, inicio235959, DbType.DateTime);

                p.Add("@Notifica_Email", notificaEmail, DbType.String);
                p.Add("@Notifica_Activa", dto.notifica_activa, DbType.Int16);
                p.Add("@Notifica_Cliente", dto.notifica_cliente, DbType.Int16);

                p.Add("@ConsecFE", dto.consec_fe, DbType.Int32);
                p.Add("@ConsecNC", dto.consec_nc, DbType.Int32);
                p.Add("@ConsecND", dto.consec_nd, DbType.Int32);
                p.Add("@ConsecTE", dto.consec_te, DbType.Int32);

                p.Add("@Portal_Codigo", portalCodigo, DbType.String);
                p.Add("@Portal_Server", portalServer, DbType.String);
                p.Add("@Portal_DB", portalDb, DbType.String);
                p.Add("@Portal_User", portalUser, DbType.String);
                p.Add("@Portal_Key", portalKey, DbType.String);

                p.Add("@TipoMov", "A", DbType.String);
                p.Add(USUARIO, usuario, DbType.String);

                p.Add("@Metodo", metodo, DbType.String);
                p.Add("@IPolizas", dto.i_polizas, DbType.Int16);
                p.Add("@IPrincipal", dto.i_principal, DbType.Int16);

                p.Add("@MntMaxApl", dto.mnt_max_apl, DbType.Int16);
                p.Add("@MntMax", dto.mnt_max, DbType.Decimal);

                p.Add("@Cabys", cabys, DbType.String);
                p.Add("@Sucursal", sucursal, DbType.String);
                p.Add("@Terminal", terminal, DbType.String);

                conn.Execute("spSYS_FE_PARAMETROS_Registra", p, commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"FE Configuración: Cliente {codigo} Id {identificacion} RS {razonSocial}",
                    Movimiento = "Guarda Configuración - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Ok");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina configuración (spSYS_FE_PARAMETROS_Registra) con TipoMov='E'.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto FE_Configuracion_Eliminar(int CodEmpresa, string codigo, string usuario)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                codigo = (codigo ?? "").Trim();
                usuario = (usuario ?? "").Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(codigo))
                    return DbHelper.ErrorResponse(COD_REQUERIDO);

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse(USUARIO_REQUERIDO);

                var cfg = FE_Configuracion_Obtener(CodEmpresa, codigo);
                if (cfg.Code != 0)
                    return DbHelper.ErrorResponse(cfg.Description ?? "No se pudo obtener configuración.");

                var c = cfg.Result ?? new FeConfiguracionModel();

                var p = new DynamicParameters();
                p.Add("@Codigo", codigo, DbType.String);
                p.Add("@TipoID", (c.tipo_id ?? "").Trim(), DbType.String);
                p.Add("@Identificacion", (c.identificacion ?? "").Trim(), DbType.String);
                p.Add("@Razon_Social", (c.razon_social ?? "").Trim(), DbType.String);
                p.Add("@Activa", c.activa, DbType.Int16);

                var inicio = c.inicio ?? DateTime.Now;
                var inicio235959 = new DateTime(inicio.Year, inicio.Month, inicio.Day, 23, 59, 59, DateTimeKind.Unspecified);
                p.Add(INICIO, inicio235959, DbType.DateTime);

                p.Add("@Notifica_Email", (c.notifica_email ?? "").Trim(), DbType.String);
                p.Add("@Notifica_Activa", c.notifica_activa, DbType.Int16);
                p.Add("@Notifica_Cliente", c.notifica_cliente, DbType.Int16);

                p.Add("@ConsecFE", c.consec_fe, DbType.Int32);
                p.Add("@ConsecNC", c.consec_nc, DbType.Int32);
                p.Add("@ConsecND", c.consec_nd, DbType.Int32);
                p.Add("@ConsecTE", c.consec_te, DbType.Int32);

                p.Add("@Portal_Codigo", (c.portal_codigo ?? "").Trim(), DbType.String);
                p.Add("@Portal_Server", (c.portal_server ?? "").Trim(), DbType.String);
                p.Add("@Portal_DB", (c.portal_db ?? "").Trim(), DbType.String);
                p.Add("@Portal_User", (c.portal_user ?? "").Trim(), DbType.String);
                p.Add("@Portal_Key", (c.portal_key ?? "").Trim(), DbType.String);

                p.Add("@TipoMov", "E", DbType.String);
                p.Add(USUARIO, usuario, DbType.String);

                var metodo = (c.metodo ?? "").Trim().ToUpperInvariant();
                if (metodo != "E") metodo = "D";
                p.Add("@Metodo", metodo, DbType.String);

                p.Add("@IPolizas", c.i_polizas, DbType.Int16);
                p.Add("@IPrincipal", c.i_principal, DbType.Int16);

                p.Add("@MntMaxApl", c.mnt_max_apl, DbType.Int16);
                p.Add("@MntMax", c.mnt_max, DbType.Decimal);

                var cabys = (c.cabys ?? "").Trim();
                if (cabys.Length > 30) cabys = cabys.Substring(0, 30);
                p.Add("@Cabys", cabys, DbType.String);

                var sucursal = (c.sucursal ?? "").Trim();
                if (string.IsNullOrWhiteSpace(sucursal)) sucursal = "2";
                if (sucursal.Length > 1) sucursal = sucursal.Substring(0, 1);
                p.Add("@Sucursal", sucursal, DbType.String);

                var terminal = (c.terminal ?? "").Trim();
                if (string.IsNullOrWhiteSpace(terminal)) terminal = "00001";
                if (terminal.Length > 5) terminal = terminal.Substring(0, 5);
                p.Add("@Terminal", terminal, DbType.String);

                conn.Execute("spSYS_FE_PARAMETROS_Registra", p, commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"FE Configuración: Elimina Cliente {codigo}",
                    Movimiento = "Elimina Configuración - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Ok");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Sincroniza SYS_FE_CLIENTES desde IW_CLIENTE para un COD_CLIENTE (VB6: btnConfig_Clientes_Sinc_Click).
        /// Borra y reconstruye la tabla de clientes del proveedor.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_cliente"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto FE_Clientes_Sincronizar(int CodEmpresa, string cod_cliente, string usuario)
        {
            const string MSG_OK = "Sincronización de Clientes versus Proveedor de Facturación Electrónica realizado satisfactoriamente!";

            try
            {
                cod_cliente = (cod_cliente ?? "").Trim();
                usuario = (usuario ?? "").Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(cod_cliente))
                    return DbHelper.ErrorResponse("cod_cliente es requerido.");

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse("Usuario es requerido.");

                using var connLocal = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                connLocal.Execute(
                    "exec sp_getapplock @Resource=@r, @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=15000;",
                    new { r = $"FE_CLIENTES_SINC_{cod_cliente}" },
                    commandTimeout: 360
                );

                const string sqlCfg = @"
                select
                    rtrim(isnull(ACC_SERVER,'')) as portal_server,
                    rtrim(isnull(ACC_DB,''))     as portal_db,
                    rtrim(isnull(ACC_USR,''))    as portal_user,
                    rtrim(isnull(ACC_KEY,''))    as portal_key
                from SYS_FE_PARAMETROS
                where COD_CLIENTE = @cod_cliente;";

                var cfg = connLocal.QueryFirstOrDefault<dynamic>(sqlCfg, new { cod_cliente });

                if (cfg == null)
                    return DbHelper.ErrorResponse("No se encontró configuración del cliente en SYS_FE_PARAMETROS.");

                string portalServer = (cfg.portal_server ?? "").ToString().Trim();
                string portalDb = (cfg.portal_db ?? "").ToString().Trim();
                string portalUser = (cfg.portal_user ?? "").ToString().Trim();
                string portalKey = (cfg.portal_key ?? "").ToString().Trim();

                if (string.IsNullOrWhiteSpace(portalServer) ||
                    string.IsNullOrWhiteSpace(portalDb) ||
                    string.IsNullOrWhiteSpace(portalUser) ||
                    string.IsNullOrWhiteSpace(portalKey))
                {
                    return DbHelper.ErrorResponse("Credenciales del portal proveedor incompletas.");
                }

                var csb = new SqlConnectionStringBuilder
                {
                    DataSource = portalServer,
                    InitialCatalog = portalDb,
                    UserID = portalUser,
                    Password = portalKey,
                    ConnectTimeout = 15,
                    Encrypt = false,
                    TrustServerCertificate = true
                };

                using var connPortal = new SqlConnection(csb.ConnectionString);
                connPortal.Open();

                var exists = connPortal.QueryFirstOrDefault<int>("select case when object_id('dbo.IW_CLIENTE','U') is null then 0 else 1 end;");
                if (exists == 0)
                    return DbHelper.ErrorResponse("No existe la tabla IW_CLIENTE en el portal proveedor. Verifique base de datos/servidor configurados.");

                const string sqlPortal = @"
                select
                    [ID]                as CLIENTE_ID_FE,
                    CODIGO              as CLIENTE_ID,
                    rtrim(CEDULA)       as CEDULA,
                    rtrim(RAZON_SOCIAL) as NOMBRE
                from IW_CLIENTE
                where ID_CLIENTE_ORIGEN = @cod_cliente
                order by [ID];";

                var portalRows = connPortal.Query(sqlPortal, new { cod_cliente }).ToList();

                const string sqlDelete = @"delete SYS_FE_CLIENTES where COD_CLIENTE = @cod_cliente;";
                connLocal.Execute(sqlDelete, new { cod_cliente }, commandTimeout: 360);

                var seenClienteId = new HashSet<long>();
                var seenClienteIdFe = new HashSet<long>();

                var sb = new System.Text.StringBuilder();

                foreach (var r in portalRows)
                {
                    long clienteId = 0;
                    long clienteIdFe = 0;

                    if (r.CLIENTE_ID != null) clienteId = Convert.ToInt64(r.CLIENTE_ID);
                    if (r.CLIENTE_ID_FE != null) clienteIdFe = Convert.ToInt64(r.CLIENTE_ID_FE);

                    if (clienteId <= 0 || clienteIdFe <= 0) continue;

                    if (!seenClienteId.Add(clienteId)) continue;
                    if (!seenClienteIdFe.Add(clienteIdFe)) continue;

                    string cedula = SqlEscape((r.CEDULA ?? "").ToString());
                    string nombre = SqlEscape((r.NOMBRE ?? "").ToString());

                    sb.AppendLine(" ");
                    sb.Append("INSERT SYS_FE_CLIENTES (COD_CLIENTE, CEDULA, NOMBRE, CLIENTE_ID, CLIENTE_ID_FE, REGISTRO_FECHA, REGISTRO_USUARIO) ");
                    sb.Append("VALUES(");
                    sb.Append($"'{SqlEscape(cod_cliente)}','{cedula}','{nombre}',{clienteId},{clienteIdFe}, getdate(), '{SqlEscape(usuario)}');");
                    sb.AppendLine();

                    if (sb.Length > 20000)
                    {
                        connLocal.Execute(sb.ToString(), commandTimeout: 360);
                        sb.Clear();
                    }
                }

                if (sb.Length > 0)
                {
                    connLocal.Execute(sb.ToString(), commandTimeout: 360);
                    sb.Clear();
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"FE Sincroniza Clientes: Cliente {cod_cliente} RegistrosPortal {portalRows.Count} Insertados {seenClienteId.Count}",
                    Movimiento = "Sincronizar Clientes - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(MSG_OK);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static string SqlEscape(string s)
        {
            s = (s ?? "").Trim();
            return s.Replace("'", "''");
        }



        private static (string server, string db, string usr, string key) GetPortalAccesos(SqlConnection conn, string cod_cliente)
        {
            const string sql = @"
            select
                rtrim(isnull(ACC_SERVER,'')) as server,
                rtrim(isnull(ACC_DB,''))     as db,
                rtrim(isnull(ACC_USR,''))    as usr,
                rtrim(isnull(ACC_KEY,''))    as [key]
            from SYS_FE_PARAMETROS
            where COD_CLIENTE = @cod_cliente;";

            var row = conn.QueryFirstOrDefault<dynamic>(sql, new { cod_cliente });
            if (row == null) return ("", "", "", "");

            string server = Convert.ToString(row.server) ?? "";
            string dbn = Convert.ToString(row.db) ?? "";
            string usr = Convert.ToString(row.usr) ?? "";
            string key = Convert.ToString(row.key) ?? "";
            return (server.Trim(), dbn.Trim(), usr.Trim(), key.Trim());
        }

        /// <summary>
        /// Consulta exclusiones por tipo (spSYS_FE_PARAMETROS_Exclusion_Consulta).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_cliente"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<FeExclusionItem>> FE_Exclusiones_Consulta(int CodEmpresa, string cod_cliente, string tipo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var resp = DbHelper.CreateOkResponse(new List<FeExclusionItem>());

            try
            {
                cod_cliente = (cod_cliente ?? "").Trim();
                tipo = (tipo ?? "").Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(cod_cliente))
                    return DbHelper.CreateErrorResponse<List<FeExclusionItem>>("cod_cliente es requerido.");

                if (string.IsNullOrWhiteSpace(tipo))
                    return DbHelper.CreateErrorResponse<List<FeExclusionItem>>("tipo es requerido.");

                var p = new DynamicParameters();
                p.Add(P_CLIENTE_ID, cod_cliente, DbType.String);
                p.Add(P_TIPO, tipo, DbType.String);

                resp.Result = conn.Query<FeExclusionItem>(
                    "spSYS_FE_PARAMETROS_Exclusion_Consulta",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return resp;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<FeExclusionItem>>(ex.Message);
            }
        }

        /// <summary>
        /// Asigna o elimina una exclusión (spSYS_FE_PARAMETROS_Exclusion).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_cliente"></param>
        /// <param name="codigo"></param>
        /// <param name="movimiento"></param>
        /// <param name="tipo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto FE_Exclusion_Procesar(int CodEmpresa, string cod_cliente, string codigo, string movimiento, string tipo, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                cod_cliente = (cod_cliente ?? "").Trim();
                codigo = (codigo ?? "").Trim();
                movimiento = (movimiento ?? "").Trim().ToUpperInvariant();
                tipo = (tipo ?? "").Trim().ToUpperInvariant();
                usuario = (usuario ?? "").Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(cod_cliente))
                    return DbHelper.ErrorResponse("cod_cliente es requerido.");

                if (string.IsNullOrWhiteSpace(codigo))
                    return DbHelper.ErrorResponse(COD_REQUERIDO);

                if (movimiento != "A" && movimiento != "E")
                    return DbHelper.ErrorResponse("movimiento inválido. Valores esperados: 'A' o 'E'.");

                if (string.IsNullOrWhiteSpace(tipo))
                    return DbHelper.ErrorResponse("tipo es requerido.");

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse(USUARIO_REQUERIDO);

                var p = new DynamicParameters();
                p.Add(P_CLIENTE_ID, cod_cliente, DbType.String);
                p.Add(P_CODIGO, codigo, DbType.String);
                p.Add(P_MOVIMIENTO, movimiento, DbType.String);
                p.Add(P_TIPO, tipo, DbType.String);
                p.Add(P_USUARIO, usuario, DbType.String);

                conn.Execute("spSYS_FE_PARAMETROS_Exclusion", p, commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"FE Exclusión: Cliente {cod_cliente} Tipo {tipo} Código {codigo} Mov {movimiento}",
                    Movimiento = "Exclusión - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Ok");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Reactiva casos excluidos por montos (spCrd_Operacion_Factura_Reactivar).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fecha_inicio"></param>
        /// <param name="fecha_corte"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto FE_Reactivacion_Ejecutar(int CodEmpresa, DateTime fecha_inicio, DateTime fecha_corte, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                usuario = (usuario ?? "").Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse(USUARIO_REQUERIDO);

                var ini = DayStart(fecha_inicio);
                var fin = DayEnd(fecha_corte);

                var p = new DynamicParameters();
                p.Add(INICIO, ini, DbType.DateTime);
                p.Add(P_CORTE, fin, DbType.DateTime);

                conn.Execute("spCrd_Operacion_Factura_Reactivar", p, commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"FE Reactivación: Inicio {ini:yyyy-MM-dd HH:mm:ss} Corte {fin:yyyy-MM-dd HH:mm:ss}",
                    Movimiento = "Reactivación - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Ok");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Extrae un parámetro string desde filtros.parametros (JSON), case-insensitive.
        /// </summary>
        /// <param name="parametros"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string? ExtractKeyFromParametros(object? parametros, string key)
        {
            if (parametros == null) return null;
            if (string.IsNullOrWhiteSpace(key)) return null;
            if (parametros is JToken jt)
                return TryGetFromJToken(jt, key);

            if (parametros is IDictionary<string, object> dictObj)
                return TryGetFromDictObj(dictObj, key);

            if (parametros is IDictionary<string, string> dictStr)
                return TryGetFromDictStr(dictStr, key);

            var s = parametros.ToString();
            return TryGetFromStringJson(s, key);
        }

        private static string? TryGetFromJToken(JToken jt, string key)
        {
            if (jt.Type == JTokenType.Object)
            {
                var jo = (JObject)jt;
                var token = jo.GetValue(key, StringComparison.OrdinalIgnoreCase);
                return NormalizeTokenValue(token);
            }

            return NormalizeString(jt.ToString());
        }

        private static string? TryGetFromDictObj(IDictionary<string, object> dictObj, string key)
        {
            foreach (var kv in dictObj)
            {
                if (!KeyEquals(kv.Key, key)) continue;

                if (kv.Value == null) return null;

                if (kv.Value is JToken jt)
                    return NormalizeString(NormalizeTokenValue(jt));

                return NormalizeString(kv.Value.ToString());
            }

            return null;
        }

        private static string? TryGetFromDictStr(IDictionary<string, string> dictStr, string key)
        {
            foreach (var kv in dictStr)
            {
                if (!KeyEquals(kv.Key, key)) continue;
                return NormalizeString(kv.Value);
            }

            return null;
        }

        private static string? TryGetFromStringJson(string? s, string key)
        {
            s = NormalizeString(s);
            if (s == null) return null;

            if (!LooksLikeJsonObject(s)) return null;

            try
            {
                var jo = JObject.Parse(s);
                var token = jo.GetValue(key, StringComparison.OrdinalIgnoreCase);
                return NormalizeTokenValue(token);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static bool KeyEquals(string? a, string b) =>
            a != null && a.Equals(b, StringComparison.OrdinalIgnoreCase);

        private static bool LooksLikeJsonObject(string s) =>
            s.Length >= 2 && s[0] == '{' && s[^1] == '}';

        private static string? NormalizeString(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return s.Length == 0 ? null : s;
        }

        private static string? NormalizeTokenValue(JToken? token)
        {
            if (token == null) return null;
            if (token.Type == JTokenType.Null) return null;
            var v = token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
            return NormalizeString(v);
        }
        /// <summary>
        /// Intenta deserializar filtros desde string.
        /// </summary>
        /// <param name="parametros"></param>
        /// <param name="filtros"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private static bool TryParseFiltros(string parametros, out FiltrosLazyLoadData? filtros, out string? error)
        {
            filtros = null;
            error = null;

            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
                return true;
            }
            catch (JsonException jex)
            {
                error = jex.Message;
                return false;
            }
        }

        /// <summary>
        /// Determina si la consulta es exportación (pagina=0 o paginacion=0).
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static bool IsExportAll(FiltrosLazyLoadData filtros)
        {
            return (filtros.pagina == 0 || filtros.paginacion == 0);
        }

        /// <summary>
        /// Normaliza página.
        /// </summary>
        /// <param name="pagina"></param>
        /// <param name="exportAll"></param>
        /// <returns></returns>
        private static int NormalizePagina(int pagina, bool exportAll)
        {
            if (exportAll) return 0;
            return (pagina <= 0) ? 1 : pagina;
        }

        /// <summary>
        /// Normaliza paginación.
        /// </summary>
        /// <param name="paginacion"></param>
        /// <param name="exportAll"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static int NormalizePaginacion(int paginacion, bool exportAll, int defaultValue)
        {
            if (exportAll) return 0;
            return (paginacion <= 0) ? defaultValue : paginacion;
        }

        /// <summary>
        /// Normaliza estado para SP: 'T' o primer caracter (Trim).
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        private static string NormalizeEstado(string? estado)
        {
            var s = (estado ?? "").Trim();
            return string.IsNullOrWhiteSpace(s) ? "T" : s.Substring(0, 1);
        }

        private static DateTime DayStart(DateTime d) => new(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified);
        private static DateTime DayEnd(DateTime d) => new(d.Year, d.Month, d.Day, 23, 59, 59, DateTimeKind.Unspecified);

        /// <summary>
        /// Lee parámetros de filtros.parametros para consulta de facturas.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static FeFacturasParametrosDto ReadFacturasParams(FiltrosLazyLoadData filtros)
        {
            return new FeFacturasParametrosDto
            {
                cod_cliente = (ExtractKeyFromParametros(filtros.parametros, "cod_cliente") ?? "").Trim(),
                identificacion = (ExtractKeyFromParametros(filtros.parametros, "identificacion") ?? "").Trim(),
                nombre = (ExtractKeyFromParametros(filtros.parametros, "nombre") ?? "").Trim(),
                factura = (ExtractKeyFromParametros(filtros.parametros, "factura") ?? "").Trim(),
                fecha_inicio = (ExtractKeyFromParametros(filtros.parametros, "fecha_inicio") ?? "").Trim(),
                fecha_corte = (ExtractKeyFromParametros(filtros.parametros, "fecha_corte") ?? "").Trim(),
                estado = (ExtractKeyFromParametros(filtros.parametros, "estado") ?? "T").Trim()
            };
        }


        /// <summary>
        /// Valida parámetros y construye rango [00:00:00–23:59:59].
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="ini"></param>
        /// <param name="fin"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        private static bool ValidateFacturasParams(FeFacturasParametrosDto dto, out DateTime ini, out DateTime fin, out string? errMsg)
        {
            ini = default;
            fin = default;
            errMsg = null;

            if (string.IsNullOrWhiteSpace(dto.cod_cliente))
            {
                errMsg = "parametros.cod_cliente es requerido.";
                return false;
            }

            if (!DateTime.TryParseExact(dto.fecha_inicio, FE_DATE_FMT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaInicio))
            {
                errMsg = "parametros.fecha_inicio inválida. Formato esperado: YYYY-MM-DD.";
                return false;
            }

            if (!DateTime.TryParseExact(dto.fecha_corte, FE_DATE_FMT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaCorte))
            {
                errMsg = "parametros.fecha_corte inválida. Formato esperado: YYYY-MM-DD.";
                return false;
            }

            ini = DayStart(fechaInicio);
            fin = DayEnd(fechaCorte);

            return true;
        }

        /// <summary>
        /// Ordena lista de facturas por sortField/sortOrder.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        private static void SortFacturas(List<FeFacturaItem> data, string? sortField, int sortOrder)
        {
            var sf = (sortField ?? "").Trim().ToLowerInvariant();
            int dir = (sortOrder == 1) ? 1 : -1;

            int CmpStr(string? a, string? b) =>
                dir * string.Compare(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase);

            int CmpDec(decimal a, decimal b) => dir * a.CompareTo(b);

            int CmpDate(DateTime? a, DateTime? b) => dir * Nullable.Compare(a, b);

            if (string.IsNullOrWhiteSpace(sf))
            {
                data.Sort((a, b) => Nullable.Compare(b.fecha, a.fecha));
                return;
            }

            data.Sort((a, b) => sf switch
            {
                "tipo" => CmpStr(a.tipo, b.tipo),
                "comprobante" => CmpStr(a.comprobante, b.comprobante),
                "identificacion" => CmpStr(a.identificacion, b.identificacion),
                "razon_social" => CmpStr(a.razon_social, b.razon_social),
                "nombre" => CmpStr(a.razon_social, b.razon_social),
                "fecha" => CmpDate(a.fecha, b.fecha),
                "total" => CmpDec(a.total, b.total),
                "total_exento" => CmpDec(a.total_exento, b.total_exento),
                "total_gravado" => CmpDec(a.total_gravado, b.total_gravado),
                "total_impuestos" => CmpDec(a.total_impuestos, b.total_impuestos),
                "total_descuentos" => CmpDec(a.total_descuentos, b.total_descuentos),
                "total_comprobante" => CmpDec(a.total_comprobante, b.total_comprobante),
                _ => CmpDate(a.fecha, b.fecha)
            });
        }

        /// <summary>
        /// Pagina una lista (slice).
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <returns></returns>
        private static List<FeFacturaItem> Page(List<FeFacturaItem> data, int pagina, int paginacion)
        {
            if (data.Count == 0) return new List<FeFacturaItem>();

            if (pagina <= 0) pagina = 1;
            if (paginacion <= 0) paginacion = 30;

            int start = (pagina - 1) * paginacion;
            if (start >= data.Count) return new List<FeFacturaItem>();

            int count = Math.Min(paginacion, data.Count - start);
            return data.GetRange(Math.Max(0, start), count);
        }

        /// <summary>
        /// Construye parámetros base para spProGrX_Facturas_Consulta_Rsm.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="ini"></param>
        /// <param name="fin"></param>
        /// <returns></returns>
        private static DynamicParameters BuildFacturasResumenBaseParams(FeFacturasParametrosDto dto, DateTime ini, DateTime fin)
        {
            var pBase = new DynamicParameters();
            pBase.Add(C_CLIENTE, dto.cod_cliente, DbType.String);
            pBase.Add("@FiltroFactura", dto.factura ?? "", DbType.String);
            pBase.Add("@FiltroId", dto.identificacion ?? "", DbType.String);
            pBase.Add("@FiltroRazonSocial", dto.nombre ?? "", DbType.String);

            pBase.Add(INICIO, DateTime.SpecifyKind(ini, DateTimeKind.Unspecified), DbType.DateTime);
            pBase.Add(P_CORTE, DateTime.SpecifyKind(fin, DateTimeKind.Unspecified), DbType.DateTime);

            pBase.Add("@Estado", NormalizeEstado(dto.estado), DbType.String);

            return pBase;
        }

        /// <summary>
        /// Ejecuta cabecera (Tipo = 'R').
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="pBase"></param>
        /// <returns></returns>
        private static dynamic? ExecuteFacturasResumenHead(SqlConnection conn, DynamicParameters pBase)
        {
            var pR = new DynamicParameters(pBase);
            pR.Add(TIPO, "R", DbType.String);

            return conn.QueryFirstOrDefault<dynamic>(
                "spProGrX_Facturas_Consulta_Rsm",
                pR,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Mapea cabecera desde dynamic.
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        private static FeFacturasResumenCabecera MapFacturasResumenCabecera(dynamic? head)
        {
            var cab = new FeFacturasResumenCabecera();

            if (head == null) return cab;

            cab.no_facturas = head.Facturas == null ? 0 : Convert.ToInt32(head.Facturas);
            cab.inicio = head.Inicio == null ? (DateTime?)null : Convert.ToDateTime(head.Inicio);
            cab.corte = head.Corte == null ? (DateTime?)null : Convert.ToDateTime(head.Corte);
            cab.monto_facturado = head.Total_Venta == null ? 0m : Convert.ToDecimal(head.Total_Venta);

            return cab;
        }

        /// <summary>
        /// Ejecuta detalle.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="pBase"></param>
        /// <returns></returns>
        private static List<FeFacturaResumenItem> ExecuteFacturasResumenDetalle(SqlConnection conn, DynamicParameters pBase)
        {
            var pD = new DynamicParameters(pBase);
            pD.Add(TIPO, "D", DbType.String);

            return conn.Query<FeFacturaResumenItem>(
                "spProGrX_Facturas_Consulta_Rsm",
                pD,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        /// <summary>
        /// Ordena lista de resumen por sortField/sortOrder.
        /// </summary>
        /// <param name="lista"></param>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        private static void SortFacturasResumen(List<FeFacturaResumenItem> lista, string? sortField, int sortOrder)
        {
            int dir = (sortOrder == 1) ? 1 : -1;
            var sf = (sortField ?? "").Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(sf))
            {
                lista.Sort((a, b) => b.facturado.CompareTo(a.facturado));
                return;
            }

            lista.Sort((a, b) => sf switch
            {
                "tipo" => dir * string.Compare(a.tipo ?? "", b.tipo ?? "", StringComparison.OrdinalIgnoreCase),
                "lineas" => dir * a.lineas.CompareTo(b.lineas),
                "detalle" => dir * string.Compare(a.detalle ?? "", b.detalle ?? "", StringComparison.OrdinalIgnoreCase),
                "facturado" => dir * a.facturado.CompareTo(b.facturado),
                _ => dir * a.facturado.CompareTo(b.facturado)
            });
        }
    }
}
