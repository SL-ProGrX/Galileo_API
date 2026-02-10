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
        private const string SP_FACTURAS_CONSULTA = "spProGrX_Facturas_Consulta";
        private const string SP_FACTURA_DETALLE = "spProGrX_Factura_Detalle";
        private const string SP_FACTURAS_RSM = "spProGrX_Facturas_Consulta_Rsm";
        private static readonly string[] _whitelistEstados = new[] { "T", "A", "P", "R" };
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
        /// Conexion con la base de datos del proveedor.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_cliente"></param>
        /// <returns></returns>
        private SqlConnection OpenPortalProveedorConn(int CodEmpresa, string cod_cliente)
        {
            cod_cliente = (cod_cliente ?? "").Trim();
            if (string.IsNullOrWhiteSpace(cod_cliente))
                throw new InvalidOperationException("cod_cliente es requerido.");
            var csLocal = _portalDB.ObtenerDbConnStringEmpresa(CodEmpresa);
            using var connLocal = new SqlConnection(csLocal);
            connLocal.Open();

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
                throw new InvalidOperationException("No se encontró configuración del cliente en SYS_FE_PARAMETROS.");

            string portalServer = (cfg.portal_server ?? "").ToString().Trim();
            string portalDb = (cfg.portal_db ?? "").ToString().Trim();
            string portalUser = (cfg.portal_user ?? "").ToString().Trim();
            string portalKey = (cfg.portal_key ?? "").ToString().Trim();

            if (string.IsNullOrWhiteSpace(portalServer) ||
                string.IsNullOrWhiteSpace(portalDb) ||
                string.IsNullOrWhiteSpace(portalUser) ||
                string.IsNullOrWhiteSpace(portalKey))
                throw new InvalidOperationException("Credenciales del portal proveedor incompletas.");

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

            var connPortal = new SqlConnection(csb.ConnectionString);
            connPortal.Open();
            return connPortal;
        }
        /// <summary>
        /// Lista clientes para Facturación Electrónica.
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
        /// Lista los codigos Cabys.
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
        /// Obtiene estados para filtro de Facturas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Facturas_Estados_DropDown_Obtener(int CodEmpresa)
        {
            var lista = new List<DropDownListaGenericaModel>
            {
                new DropDownListaGenericaModel { item = "T", descripcion = "TODAS" },
                new DropDownListaGenericaModel { item = "A", descripcion = "Aceptada" },
                new DropDownListaGenericaModel { item = "R", descripcion = "Rechazada" }
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
        /// Lista cortes realizados por cliente con lazy load.
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
        /// Registra/Reprocesa corte usando el spCrd_Facturacion_Corte.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto FE_Corte_Registrar(int CodEmpresa, FeRegistrarCorteDto dto)
        {
            var p = new FeProcesarCorteDto
            {
                cod_cliente = dto.cod_cliente,
                fecha_corte = dto.fecha_corte,
                fecha_factura = dto.fecha_factura,
                usuario = dto.usuario
            };

            return FE_Corte_Procesar(CodEmpresa, p);
        }

        /// <summary>
        /// Proceso completo de corte.
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto FE_Corte_Procesar(int CodEmpresa, FeProcesarCorteDto dto)
        {
            const string MSG_OK = "Proceso de Corte + Facturación realizado satisfactoriamente!";

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


                using var connLocal = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                connLocal.Open();


                using var connProveedor = OpenPortalProveedorConn(CodEmpresa, codCliente);

                static bool TryReadSpError(dynamic? row, out int code, out string desc)
                {
                    code = 0;
                    desc = "";

                    if (row == null) return false;

                    if (row is IDictionary<string, object> d)
                    {
                        if (d.TryGetValue("code", out var c) && c != null)
                        {
                            code = Convert.ToInt32(c);
                            if (d.TryGetValue("description", out var m) && m != null) desc = Convert.ToString(m) ?? "";
                            return code != 0;
                        }
                        if (d.TryGetValue("Code", out var c2) && c2 != null)
                        {
                            code = Convert.ToInt32(c2);
                            if (d.TryGetValue("Description", out var m2) && m2 != null) desc = Convert.ToString(m2) ?? "";
                            return code != 0;
                        }
                    }

                    try
                    {
                        var t = row.GetType();
                        var pCode = t.GetProperty("code") ?? t.GetProperty("Code");
                        var pDesc = t.GetProperty("description") ?? t.GetProperty("Description");
                        if (pCode != null)
                        {
                            var v = pCode.GetValue(row);
                            if (v != null)
                            {
                                code = Convert.ToInt32(v);
                                if (pDesc != null)
                                {
                                    var v2 = pDesc.GetValue(row);
                                    if (v2 != null) desc = Convert.ToString(v2) ?? "";
                                }
                                return code != 0;
                            }
                        }
                    }
                    catch { /* no-op */ }

                    return false;
                }

                const string sqlParams = @"
                select
                    isnull(INCLUYE_POLIZAS,0)        as incluye_polizas,
                    isnull(INCLUYE_PRINCIPAL,0)     as incluye_principal,
                    rtrim(isnull(CABYS,''))         as cabys,
                    rtrim(isnull(ACTIVIDAD_ECONOMICA,'')) as actividad,
                    rtrim(isnull(MONEDA,''))        as moneda,
                    rtrim(isnull(SUCURSAL,''))      as sucursal,
                    rtrim(isnull(TERMINAL,''))      as terminal
                from SYS_FE_PARAMETROS
                where COD_CLIENTE = @cod_cliente;";

                var pCfg = connLocal.QueryFirstOrDefault<dynamic>(sqlParams, new { cod_cliente = codCliente });
                if (pCfg == null)
                    return DbHelper.ErrorResponse("No se encontró configuración en SYS_FE_PARAMETROS para el cliente.");

                bool incluyePolizas = Convert.ToInt32(pCfg.incluye_polizas) == 1;
                bool incluyePrincipal = Convert.ToInt32(pCfg.incluye_principal) == 1;
                string actividad = Convert.ToString(pCfg.actividad) ?? "";
                string monedaCfg = (Convert.ToString(pCfg.moneda) ?? "").Trim();

                string codSucursal = (Convert.ToString(pCfg.sucursal) ?? "").Trim();
                if (string.IsNullOrWhiteSpace(codSucursal)) codSucursal = "2";

                string terminalPOS = (Convert.ToString(pCfg.terminal) ?? "").Trim();
                if (string.IsNullOrWhiteSpace(terminalPOS)) terminalPOS = "00001";

                var rsConsec = connProveedor.QueryFirstOrDefault<dynamic>(
                    "exec spProGrX_Cliente_Consecutivo @cliente;",
                    new { cliente = codCliente }
                );

                int consecutivo = GetConsecutivo(rsConsec);
                if (consecutivo > 0)
                {
                    connLocal.Execute(
                        "update SYS_FE_PARAMETROS set CONSECUTIVO_FE  = @c where COD_CLIENTE = @cod_cliente;",
                        new { c = consecutivo, cod_cliente = codCliente }
                    );
                }

                {
                    var pCorte = new DynamicParameters();
                    pCorte.Add("@Cliente", codCliente, DbType.String);
                    pCorte.Add("@Corte", DayEnd(fechaCorte), DbType.DateTime);
                    pCorte.Add("@Usuario", usuario, DbType.String);
                    pCorte.Add("@FechaFactura", DateTime.SpecifyKind(fechaFactura, DateTimeKind.Unspecified), DbType.DateTime);

                    connLocal.Execute("spCrd_Facturacion_Corte", pCorte, commandType: CommandType.StoredProcedure);
                }

                var nuevos = connLocal.Query(
                    "spCrd_Facturacion_Notifica_Clientes",
                    new { Cliente = codCliente },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                if (nuevos.Count > 0)
                {
                    var sbCli = new System.Text.StringBuilder();

                    foreach (var r in nuevos)
                    {
                        string codCliRow = (Convert.ToString(r.COD_CLIENTE) ?? codCliente).Trim();
                        string tipoId = Convert.ToString(r.TIPO_ID) ?? Convert.ToString(r.Tipo_Id) ?? "";
                        string clienteId = Convert.ToString(r.CLIENTE_ID) ?? Convert.ToString(r.Cliente_ID) ?? "";
                        string cedula = (Convert.ToString(r.CEDULA) ?? Convert.ToString(r.Cedula) ?? "").Trim();
                        string nombre = Convert.ToString(r.NOMBRE) ?? Convert.ToString(r.Nombre) ?? "";
                        string email = Convert.ToString(r.EMAIL) ?? Convert.ToString(r.Email) ?? "";
                        string direccion = Convert.ToString(r.DIRECCION) ?? Convert.ToString(r.DIRECCION) ?? Convert.ToString(r.Direccion) ?? "";

                      
                        var ins = connProveedor.QueryFirstOrDefault<dynamic>(
                            "exec sp_IW_CLIENTEInsert_ProGrX @COD_CLIENTE,@TIPO_ID,@CLIENTE_ID,@CEDULA,@NOMBRE,@EMAIL,'','','',@DIRECCION,1,1,1,1,@FECHA_CORTE,30,1;",
                            new
                            {
                                COD_CLIENTE = codCliRow,
                                TIPO_ID = tipoId,
                                CLIENTE_ID = clienteId,
                                CEDULA = cedula,
                                NOMBRE = nombre,
                                EMAIL = email,
                                DIRECCION = direccion,
                                FECHA_CORTE = DateTime.SpecifyKind(fechaCorte, DateTimeKind.Unspecified)
                            }
                        );

                        if (TryReadSpError(ins, out var cErr, out var mErr))
                            return DbHelper.ErrorResponse(mErr.Length > 0 ? mErr : $"Error proveedor sp_IW_CLIENTEInsert_ProGrX (code {cErr}).");

                        long codigoInterno = GetCodigoInterno(ins);
                        if (codigoInterno <= 0)
                            return DbHelper.ErrorResponse("Proveedor no devolvió CLIENTE_ID_FE (código interno) para el cliente insertado.");

                     
                        sbCli.Append(" exec spCrd_Facturacion_Notifica_Clientes_Result ");
                        sbCli.Append($"'{SqlEscape(codCliRow)}','{SqlEscape(cedula)}',{codigoInterno};");
                        sbCli.AppendLine();

                        if (sbCli.Length > 20000)
                        {
                            connLocal.Execute(sbCli.ToString(), commandTimeout: 360);
                            sbCli.Clear();
                        }
                    }

                    if (sbCli.Length > 0)
                        connLocal.Execute(sbCli.ToString(), commandTimeout: 360);
                }

                var factRows = connLocal.Query(
                    "spCrd_Facturacion_Notifica",
                    new { ClienteID = codCliente },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                if (factRows.Count > 0)
                {
                    string cedulaEmisor = "";
                    {
                        const string sqlCol = "select col_length('dbo.SYS_FE_PARAMETROS','CEDULA_EMISOR');";
                        var col = connLocal.QueryFirstOrDefault<int?>(sqlCol);
                        if (col.HasValue && col.Value > 0)
                        {
                            const string sqlCed = "select rtrim(isnull(CEDULA_EMISOR,'')) from SYS_FE_PARAMETROS where COD_CLIENTE = @c;";
                            cedulaEmisor = (connLocal.QueryFirstOrDefault<string>(sqlCed, new { c = codCliente }) ?? "").Trim();
                        }

                        if (string.IsNullOrWhiteSpace(cedulaEmisor))
                            return DbHelper.ErrorResponse("No se pudo obtener la cédula del EMISOR. Defina SYS_FE_PARAMETROS.CEDULA_EMISOR (o indique la tabla correcta para la cédula jurídica del emisor).");
                    }

                    var sbFact = new System.Text.StringBuilder();

                    const string situacion = "1";
                    const string tipoComprobante = "01";

                    foreach (var r in factRows)
                    {
                        string comprobanteInterno = (Convert.ToString(r.FAC_NUMERO) ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(comprobanteInterno))
                            continue;

                        DateTime fechaTransac = DateTime.SpecifyKind(fechaFactura, DateTimeKind.Unspecified);

                        string clave50 = mHaciendaDB.fxHacienda_Clave50(
                            "506", fechaTransac, cedulaEmisor, codSucursal, terminalPOS,
                            comprobanteInterno, situacion, tipoComprobante);

                        string clave20 = mHaciendaDB.fxHacienda_Clave20(
                            codSucursal, terminalPOS, comprobanteInterno, tipoComprobante);

                        decimal intCor = r.INTCOR == null ? 0m : Convert.ToDecimal(r.INTCOR);
                        decimal intMor = r.INTMOR == null ? 0m : Convert.ToDecimal(r.INTMOR);
                        decimal cargos = r.CARGOS == null ? 0m : Convert.ToDecimal(r.CARGOS);
                        decimal poliza = r.POLIZA == null ? 0m : Convert.ToDecimal(r.POLIZA);
                        decimal principal = r.PRINCIPAL == null ? 0m : Convert.ToDecimal(r.PRINCIPAL);

                        decimal totalGravado = 0m;
                        decimal totalExento = intCor + intMor + cargos;
                        if (incluyePolizas) totalExento += poliza;
                        if (incluyePrincipal) totalExento += principal;

                        string moneda = ((Convert.ToString(r.MONEDA) ?? monedaCfg) ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(moneda)) moneda = "CRC";

                        decimal tipoCambio = r.TIPO_CAMBIO == null ? 1m : Convert.ToDecimal(r.TIPO_CAMBIO);

                        string emailDefaultApl = Convert.ToString(r.EMAIL_DEFAULT_APL) ?? "0";
                        bool apl = emailDefaultApl.Trim() == "1";

                        string emailDefault = Convert.ToString(r.EMAIL_DEFAULT) ?? "";
                        string emailRow = Convert.ToString(r.EMAIL) ?? Convert.ToString(r.Email) ?? "";
                        string emailDestino = apl ? emailDefault : emailRow;

                        string enviarCliente = Convert.ToString(r.EMAIL_CLIENTE_NO) ?? "0";
                        string clienteIdFE = Convert.ToString(r.CLIENTE_ID_FE) ?? "";
                        string clienteId = Convert.ToString(r.CLIENTE_ID) ?? "";
                        string tipoId = Convert.ToString(r.TIPO_ID) ?? Convert.ToString(r.Tipo_Id) ?? "";

                        string clienteIdDestino = string.IsNullOrWhiteSpace(clienteIdFE) ? clienteId : clienteIdFE;

                        var enc = connProveedor.QueryFirstOrDefault<dynamic>(
                            "exec sp_IW_ENC_FACTURAInsert_ProGrX " +
                            "@COD_CLIENTE,@CLAVE50,@CLAVE20,@CLIENTE_ID,@MONEDA,@SUCURSAL,'02',@FAC,30," +
                            "@EMAIL,@NOENV,@FECHA,0,'05','','','','',null,'','','','1',@TERM,@TC," +
                            "@TIPO_COMP,@TIPO_ID," +
                            "@TOT_GR,@TOT_EX,0,@TOT_GR,0,0,@TOT_GR,@TOT_EX,0," +
                            "@TOT,@DESC,@SUBTOT,@IMP,0,0,@TOTAL," +
                            "1,@USR,@NOW,'','','FACT. PROGRX',@ACT;",
                            new
                            {
                                COD_CLIENTE = codCliente,
                                CLAVE50 = clave50,
                                CLAVE20 = clave20,
                                CLIENTE_ID = clienteIdDestino,
                                MONEDA = moneda,
                                SUCURSAL = codSucursal,
                                FAC = comprobanteInterno,
                                EMAIL = emailDestino,
                                NOENV = enviarCliente,
                                FECHA = $"{fechaTransac:yyyy/MM/dd HH:mm:ss}",
                                TERM = terminalPOS,
                                TC = tipoCambio.ToString(CultureInfo.InvariantCulture),
                                TIPO_COMP = tipoComprobante,
                                TIPO_ID = (tipoId ?? "").PadLeft(2, '0'),
                                TOT_GR = totalGravado.ToString(CultureInfo.InvariantCulture),
                                TOT_EX = totalExento.ToString(CultureInfo.InvariantCulture),
                                TOT = (totalGravado + totalExento).ToString(CultureInfo.InvariantCulture),
                                DESC = "0",
                                SUBTOT = (totalGravado + totalExento).ToString(CultureInfo.InvariantCulture),
                                IMP = "0",
                                TOTAL = (totalGravado + totalExento).ToString(CultureInfo.InvariantCulture),
                                USR = usuario,
                                NOW = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}",
                                ACT = actividad
                            }
                        );

                        if (TryReadSpError(enc, out var cErrEnc, out var mErrEnc))
                            return DbHelper.ErrorResponse(mErrEnc.Length > 0 ? mErrEnc : $"Error proveedor sp_IW_ENC_FACTURAInsert_ProGrX (code {cErrEnc}).");

                        long idFactura = GetIdFactura(enc);
                        if (idFactura == 0)
                            return DbHelper.ErrorResponse("Proveedor no devolvió IdFactura al insertar encabezado.");

                        int linea = 0;

                        void InsDet(string codigo, string unidad, string detalle, decimal monto)
                        {
                            if (monto <= 0) return;
                            linea++;

                            var det = connProveedor.QueryFirstOrDefault<dynamic>(
                                "exec sp_IW_DET_FACTURAInsert_ProGrX " +
                                "@COD_CLIENTE,@ID_FACT,@LINEA,@CODIGO,1,@UNIDAD,@DETALLE," +
                                "@PRECIO,@MONTO,0,'DESCUENTO CLIENTES',@MONTO,'01',0,0," +
                                "null,null,null,null,null,null,null,'01',@CLAVE50;",
                                new
                                {
                                    COD_CLIENTE = codCliente,
                                    ID_FACT = idFactura,
                                    LINEA = linea,
                                    CODIGO = codigo,
                                    UNIDAD = unidad,
                                    DETALLE = detalle,
                                    PRECIO = monto.ToString(CultureInfo.InvariantCulture),
                                    MONTO = monto.ToString(CultureInfo.InvariantCulture),
                                    CLAVE50 = clave50
                                }
                            );

                            if (TryReadSpError(det, out var cErrDet, out var mErrDet))
                                throw new InvalidOperationException(mErrDet.Length > 0 ? mErrDet : $"Error proveedor sp_IW_DET_FACTURAInsert_ProGrX (code {cErrDet}).");
                        }

                        try
                        {
                            InsDet("CRD001", "I", "INTERES CORRIENTE DEL MES", intCor);
                            InsDet("CRD002", "I", "INTERES ATRASADOS", intMor);
                            InsDet("CRD003", "I", "CARGOS ADM Y DE FORMALIZACION", cargos);
                            if (incluyePolizas) InsDet("CRD004", "Unid", "POLIZAS DEL CREDITO", poliza);
                            if (incluyePrincipal) InsDet("CRD005", "Unid", "ABONO AL CREDITO", principal);
                        }
                        catch (InvalidOperationException exDet)
                        {
                            return DbHelper.ErrorResponse(exDet.Message);
                        }

                        sbFact.Append(" exec spCrd_Facturacion_Notifica_Result ");
                        sbFact.Append($"'{SqlEscape(codCliente)}','{SqlEscape(comprobanteInterno)}','{idFactura}','{SqlEscape(usuario)}';");
                        sbFact.AppendLine();

                        if (sbFact.Length > 20000)
                        {
                            connLocal.Execute(sbFact.ToString(), commandTimeout: 360);
                            sbFact.Clear();
                        }
                    }

                    if (sbFact.Length > 0)
                        connLocal.Execute(sbFact.ToString(), commandTimeout: 360);
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"FE Corte PROCESAR: Cliente {codCliente} Corte {dto.fecha_corte} Factura {dto.fecha_factura}",
                    Movimiento = "Corte + Facturación - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(MSG_OK);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
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
                using var conn = OpenPortalProveedorConn(CodEmpresa, dto.cod_cliente);

                var p = new DynamicParameters();
                p.Add("@ClienteId", dto.cod_cliente, DbType.String);
                p.Add("@Factura", dto.factura ?? "", DbType.String);
                p.Add("@Cedula", dto.identificacion ?? "", DbType.String);
                p.Add("@Nombre", dto.nombre ?? "", DbType.String);
                p.Add("@FechaInicio", ini, DbType.DateTime);
                p.Add("@FechaCorte", fin, DbType.DateTime);
                p.Add("@Estado", NormalizeEstado(dto.estado), DbType.String);

                var rows = conn.Query(
                    "spProGrX_Facturas_Consulta",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                var lista = new List<FeFacturaItem>();

                foreach (var r in rows)
                {
                    var tipoDoc = ExtractKeyFromParametros(r, "Tipo_Documento");

                    var item = new FeFacturaItem
                    {
                        tipo = (tipoDoc == "01") ? "FE" : "NC",
                        comprobante = "_" + (ExtractKeyFromParametros(r, "Numero_Consecutivo") ?? ""),

                        identificacion = ExtractKeyFromParametros(r, "Cedula"),
                        razon_social = ExtractKeyFromParametros(r, "Razon_Social"),

                        fecha = DateTime.TryParse(ExtractKeyFromParametros(r, "Fecha_Emision"), out DateTime f) ? f : (DateTime?)null,


                        total = decimal.TryParse(ExtractKeyFromParametros(r, "Total_Venta"), out decimal d1) ? d1 : 0m,
                        total_exento = decimal.TryParse(ExtractKeyFromParametros(r, "Total_Exento"), out decimal d2) ? d2 : 0m,
                        total_gravado = decimal.TryParse(ExtractKeyFromParametros(r, "Total_Gravado"), out decimal d3) ? d3 : 0m,
                        total_impuestos = decimal.TryParse(ExtractKeyFromParametros(r, "Total_Impuestos"), out decimal d4) ? d4 : 0m,
                        total_descuentos = decimal.TryParse(ExtractKeyFromParametros(r, "Total_Descuentos"), out decimal d5) ? d5 : 0m,
                        total_comprobante = decimal.TryParse(ExtractKeyFromParametros(r, "Total_Comprobante"), out decimal d6) ? d6 : 0m,

                        clave = "_" + (ExtractKeyFromParametros(r, "Clave") ?? ""),

                        xml_respuesta = ExtractKeyFromParametros(r, "XML_Respuesta"),
                        observaciones = ExtractKeyFromParametros(r, "Observaciones"),

                        id_factura = int.TryParse(
                        ExtractKeyFromParametros(r, "id_Factura"),
                        out int id) ? id : 0

                    };

                    lista.Add(item);
                }

                SortFacturas(lista, filtros!.sortField, filtros.sortOrder);

                response.Result.total = lista.Count;

                if (exportAll)
                {
                    response.Result.lista = lista;
                    return response;
                }

                var pagina = NormalizePagina(filtros!.pagina, exportAll);
                var paginacion = NormalizePaginacion(filtros!.paginacion, exportAll, 30);

                response.Result.lista = Page(lista, pagina, paginacion);

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
        public ErrorDto<List<FeFacturaDetalleItem>> FE_Factura_Detalle_Obtener(int CodEmpresa, string codCliente, string idFactura)
        {
            var resp = DbHelper.CreateOkResponse(new List<FeFacturaDetalleItem>());
            resp.Result ??= new List<FeFacturaDetalleItem>();

            try
            {
                codCliente = (codCliente ?? "").Trim();
                if (string.IsNullOrWhiteSpace(codCliente))
                    return DbHelper.CreateErrorResponse<List<FeFacturaDetalleItem>>("codCliente es requerido.");

                idFactura = (idFactura ?? "").Trim();
                if (string.IsNullOrWhiteSpace(idFactura))
                    return DbHelper.CreateErrorResponse<List<FeFacturaDetalleItem>>("idFactura es requerido.");

                if (!int.TryParse(idFactura, out int idFacturaInt) || idFacturaInt <= 0)
                    return DbHelper.CreateErrorResponse<List<FeFacturaDetalleItem>>("idFactura inválido.");

                using var conn = OpenPortalProveedorConn(CodEmpresa, codCliente);

                var p = new DynamicParameters();
                p.Add("@ClienteId", codCliente, DbType.String);
                p.Add("@IdFactura", idFacturaInt, DbType.Int32);

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
        /// Obtiene el resumen  usando spProGrX_Facturas_Consulta_Rsm.
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

                using var conn = OpenPortalProveedorConn(CodEmpresa, dto.cod_cliente);
                var pBase = BuildFacturasResumenBaseParams(dto, ini, fin);
                var head = ExecuteFacturasResumenHead(conn, pBase);
                resp.Result.cabecera = MapFacturasResumenCabecera(head);
                resp.Result.lista = ExecuteFacturasResumenDetalle(conn, pBase);

                var sf = (filtros!.sortField ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(sf))
                    SortFacturasResumen(resp.Result.lista, filtros.sortField, filtros.sortOrder);

                return resp;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<FeFacturasResumen>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta resumen usando spProGrX_Facturas_Consulta_Rsm.
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
        /// Lista clientes (IW_CLIENTE en portal proveedor) con lazy load.
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

            var identificacion = (ExtractKeyFromParametros(filtros.parametros, "identificacion") ?? "").Trim();
            var nombre = (ExtractKeyFromParametros(filtros.parametros, "nombre") ?? "").Trim();

            var exportAll = IsExportAll(filtros);

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
                    return DbHelper.CreateErrorResponse<FeClientesLista>(
                        "No se encontró configuración del portal para el cliente seleccionado."
                    );

                string portalServer = (cfg.portal_server ?? "").ToString().Trim();
                string portalDb = (cfg.portal_db ?? "").ToString().Trim();
                string portalUser = (cfg.portal_user ?? "").ToString().Trim();
                string portalKey = (cfg.portal_key ?? "").ToString().Trim();

                if (string.IsNullOrWhiteSpace(portalServer) ||
                    string.IsNullOrWhiteSpace(portalDb) ||
                    string.IsNullOrWhiteSpace(portalUser) ||
                    string.IsNullOrWhiteSpace(portalKey))
                {
                    return DbHelper.CreateErrorResponse<FeClientesLista>(
                        "Credenciales del portal proveedor incompletas para el cliente seleccionado."
                    );
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

                var exists = connPortal.QueryFirstOrDefault<int>(
                    "select case when object_id('dbo.IW_CLIENTE','U') is null then 0 else 1 end;"
                );

                if (exists == 0)
                    return DbHelper.CreateErrorResponse<FeClientesLista>(
                        "No existe la tabla IW_CLIENTE en el portal proveedor del cliente seleccionado."
                    );

                bool hasId = !string.IsNullOrWhiteSpace(identificacion);
                bool hasNom = !string.IsNullOrWhiteSpace(nombre);
                const string sql = @"
                select
                    rtrim(isnull(convert(varchar(30), CODIGO), '')) as id_prov,
                    rtrim(isnull(TIPO_CLIENTE, ''))                 as tipo_id,
                    rtrim(isnull(CEDULA, ''))                       as identificacion,
                    rtrim(isnull(RAZON_SOCIAL, ''))                 as razon_social,
                    rtrim(isnull(EMAIL1, ''))                       as email1,
                    rtrim(isnull(EMAIL2, ''))                       as email2,
                    rtrim(isnull(TELEFONO1, ''))                    as telefono1,
                    rtrim(isnull(TELEFONO2, ''))                    as telefono2,
                    rtrim(isnull(convert(varchar(10), COD_PROVINCIA), '')) as provincia,
                    rtrim(isnull(convert(varchar(10), COD_CANTON), ''))    as canton,
                    rtrim(isnull(convert(varchar(10), COD_DISTRITO), ''))  as distrito,
                    rtrim(isnull(convert(varchar(10), COD_BARRIO), ''))    as barrio,
                    rtrim(isnull(DIR_FISICA, ''))                   as direccion
                from IW_CLIENTE
                where ID_CLIENTE_ORIGEN = @cod_cliente
                  and (
                        @identificacion is null
                     or rtrim(isnull(CEDULA,'')) like @like_ident
                  )
                  and (
                        @nombre is null
                     or rtrim(isnull(RAZON_SOCIAL,'')) like @like_nombre
                  )
                order by [ID];";

                var p = new DynamicParameters();
                p.Add("@cod_cliente", codCliente, DbType.String);

                p.Add("@identificacion", hasId ? identificacion : null, DbType.String);
                p.Add("@like_ident", hasId ? $"%{identificacion}%" : null, DbType.String);

                p.Add("@nombre", hasNom ? nombre : null, DbType.String);
                p.Add("@like_nombre", hasNom ? $"%{nombre}%" : null, DbType.String);

                var data = connPortal.Query<FeClienteItem>(sql, p).ToList();

                SortClientes(data, filtros.sortField, filtros.sortOrder);

                response.Result.total = data.Count;

                if (exportAll)
                {
                    response.Result.lista = data;
                    return response;
                }

                var pagina = NormalizePagina(filtros.pagina, exportAll);
                var paginacion = NormalizePaginacion(filtros.paginacion, exportAll, 30);

                response.Result.lista = PageClientes(data, pagina, paginacion);
                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<FeClientesLista>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta lista de clientes.
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
        /// Obtiene configuración de Facturación Electrónica por cliente.
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
                    rtrim(P.COD_CLIENTE) as codigo,
                    rtrim(P.TIPO_ID) as tipo_id,
                    rtrim(P.CEDULA) as identificacion,
                    rtrim(P.RAZON_SOCIAL) as razon_social,
                    cast(isnull(P.ACTIVO,0) as smallint) as activa,
                    P.FECHA_INICIO as inicio,

                    rtrim(isnull(P.NOTIFICA_EMAIL,'')) as notifica_email,
                    cast(isnull(P.NOTIFICA_EMAIL_ACTIVO,0) as smallint) as notifica_activa,
                    cast(isnull(P.NOTIFICA_CLIENTE,0) as smallint) as notifica_cliente,

                    isnull(P.CONSECUTIVO_FE,0) as consec_fe,
                    isnull(P.CONSECUTIVO_NC,0) as consec_nc,
                    isnull(P.CONSECUTIVO_ND,0) as consec_nd,
                    isnull(P.CONSECUTIVO_TE,0) as consec_te,

                    rtrim(isnull(P.ACC_CODIGO,'')) as portal_codigo,
                    rtrim(isnull(P.ACC_SERVER,'')) as portal_server,
                    rtrim(isnull(P.ACC_DB,'')) as portal_db,
                    rtrim(isnull(P.ACC_USR,'')) as portal_user,
                    rtrim(isnull(P.ACC_KEY,'')) as portal_key,

                    rtrim(isnull(P.METODO_BASE,'')) as metodo,
                    cast(isnull(P.INCLUYE_POLIZAS,0) as smallint) as i_polizas,
                    cast(isnull(P.INCLUYE_PRINCIPAL,0) as smallint) as i_principal,

                    cast(isnull(P.MAX_MONTO_APL,0) as smallint) as mnt_max_apl,
                    cast(isnull(P.MAX_MONTO,0) as decimal(18,2)) as mnt_max,

                    rtrim(isnull(P.CABYS,'')) as cabys,
                    rtrim(isnull(P.SUCURSAL,'')) as sucursal,
                    rtrim(isnull(P.TERMINAL,'')) as terminal,

                    rtrim(isnull(C.DESCRIPCION,'')) as cabys_desc
                from SYS_FE_PARAMETROS P
                left join vINV_Cabys C on P.CABYS = C.COD_BYS
                where P.COD_CLIENTE = @codigo;";

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
        /// Guarda/Actualiza configuración con TipoMov='A'.
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

                var inicioTxt = (dto.inicio ?? "").Trim();
                DateTime inicio;
                var okInicio =
                    DateTime.TryParseExact(inicioTxt, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out inicio)
                 || DateTime.TryParseExact(inicioTxt, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out inicio);

                if (!okInicio)
                    return DbHelper.ErrorResponse("inicio inválido. Formato esperado: YYYY-MM-DD.");

                var inicio235959 = new DateTime(inicio.Year, inicio.Month, inicio.Day, 23, 59, 59, DateTimeKind.Unspecified);
                var metodoRaw = (dto.metodo ?? "").Trim().ToUpperInvariant();
                string metodo;
                if (metodoRaw == "E" || metodoRaw.StartsWith("E")) metodo = "E";
                else metodo = "D";

                var sucursal = (dto.sucursal ?? "").Trim();
                if (sucursal.Length > 3) sucursal = sucursal.Substring(0, 3);

                var terminal = (dto.terminal ?? "").Trim();
                if (terminal.Length > 5) terminal = terminal.Substring(0, 5);

                var cabys = (dto.cabys ?? "").Trim();
                if (cabys.Length > 30) cabys = cabys.Substring(0, 30);

                var p = new DynamicParameters();

                p.Add("@Codigo", codigo, DbType.String);
                p.Add("@TipoID", tipoId, DbType.String);
                p.Add("@Identificacion", identificacion, DbType.String);
                p.Add("@Razon_Social", razonSocial, DbType.String);
                p.Add("@Activa", dto.activa, DbType.Int16);
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
        /// Elimina configuración con TipoMov='E'.
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
                var inicio = c.inicio ?? DateTime.Now;
                var inicio235959 = new DateTime(inicio.Year, inicio.Month, inicio.Day, 23, 59, 59, DateTimeKind.Unspecified);

                var metodo = (c.metodo ?? "").Trim().ToUpperInvariant();
                if (metodo != "E") metodo = "D";

                var sucursal = (c.sucursal ?? "").Trim();
                if (sucursal.Length > 3) sucursal = sucursal.Substring(0, 3);

                var terminal = (c.terminal ?? "").Trim();
                if (terminal.Length > 5) terminal = terminal.Substring(0, 5);

                var cabys = (c.cabys ?? "").Trim();
                if (cabys.Length > 30) cabys = cabys.Substring(0, 30);

                var p = new DynamicParameters();

                p.Add("@Codigo", codigo, DbType.String);
                p.Add("@TipoID", (c.tipo_id ?? "").Trim(), DbType.String);
                p.Add("@Identificacion", (c.identificacion ?? "").Trim(), DbType.String);
                p.Add("@Razon_Social", (c.razon_social ?? "").Trim(), DbType.String);
                p.Add("@Activa", c.activa, DbType.Int16);
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

                p.Add("@Metodo", metodo, DbType.String);
                p.Add("@IPolizas", c.i_polizas, DbType.Int16);
                p.Add("@IPrincipal", c.i_principal, DbType.Int16);

                p.Add("@MntMaxApl", c.mnt_max_apl, DbType.Int16);
                p.Add("@MntMax", c.mnt_max, DbType.Decimal);

                p.Add("@Cabys", cabys, DbType.String);
                p.Add("@Sucursal", sucursal, DbType.String);
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
        /// Sincroniza SYS_FE_CLIENTES desde IW_CLIENTE para un COD_CLIENTE.
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


                if (connLocal.State != ConnectionState.Open)
                    connLocal.Open();

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
                var exists = connPortal.QueryFirstOrDefault<int>(
                    "select case when object_id('dbo.IW_CLIENTE','U') is null then 0 else 1 end;"
                );

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

                using var tx = connLocal.BeginTransaction();

                try
                {
                    const string sqlDelete = @"delete SYS_FE_CLIENTES where COD_CLIENTE = @cod_cliente;";
                    connLocal.Execute(sqlDelete, new { cod_cliente }, transaction: tx, commandTimeout: 360);

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
                        sb.Append("IF NOT EXISTS (SELECT 1 FROM SYS_FE_CLIENTES WHERE COD_CLIENTE = '");
                        sb.Append(SqlEscape(cod_cliente));
                        sb.Append("' AND CLIENTE_ID_FE = ");
                        sb.Append(clienteIdFe);
                        sb.AppendLine(")");
                        sb.Append("INSERT SYS_FE_CLIENTES (COD_CLIENTE, CEDULA, NOMBRE, CLIENTE_ID, CLIENTE_ID_FE, REGISTRO_FECHA, REGISTRO_USUARIO) ");
                        sb.Append("VALUES(");
                        sb.Append($"'{SqlEscape(cod_cliente)}','{cedula}','{nombre}',{clienteId},{clienteIdFe}, getdate(), '{SqlEscape(usuario)}');");
                        sb.AppendLine();

                        if (sb.Length > 20000)
                        {
                            connLocal.Execute(sb.ToString(), transaction: tx, commandTimeout: 360);
                            sb.Clear();
                        }
                    }

                    if (sb.Length > 0)
                    {
                        connLocal.Execute(sb.ToString(), transaction: tx, commandTimeout: 360);
                        sb.Clear();
                    }

                    tx.Commit();
                }
                catch (SqlException)
                {
                    try { tx.Rollback(); } catch { /* no-op */ }
                    throw;
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"FE Sincroniza Clientes: Cliente {cod_cliente} RegistrosPortal {portalRows.Count} Insertados {portalRows.Count}",
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
                tipo = (tipo ?? "").Trim();

                if (string.IsNullOrWhiteSpace(cod_cliente))
                    return DbHelper.CreateErrorResponse<List<FeExclusionItem>>("cod_cliente es requerido.");

                if (string.IsNullOrWhiteSpace(tipo))
                    return DbHelper.CreateErrorResponse<List<FeExclusionItem>>("tipo es requerido.");

                var tipoNorm = tipo.Trim().ToUpperInvariant();
                if (tipoNorm == "CXC") tipoNorm = "CXC";

                var tipoOk =
                    tipoNorm == "ESTP" ||
                    tipoNorm == "CRD" ||
                    tipoNorm == "CXC" ||
                    tipoNorm == "INST";

                if (!tipoOk)
                    return DbHelper.CreateErrorResponse<List<FeExclusionItem>>(
                        "tipo inválido. Valores esperados: 'ESTP', 'CRD', 'CxC', 'INST'."
                    );

                var p = new DynamicParameters();
                p.Add(P_CLIENTE_ID, cod_cliente, DbType.String);
                p.Add(P_TIPO, tipoNorm, DbType.String);

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
                tipo = (tipo ?? "").Trim();
                usuario = (usuario ?? "").Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(cod_cliente))
                    return DbHelper.ErrorResponse("cod_cliente es requerido.");

                if (string.IsNullOrWhiteSpace(codigo))
                    return DbHelper.ErrorResponse(COD_REQUERIDO);

                if (movimiento != "A" && movimiento != "E")
                    return DbHelper.ErrorResponse("movimiento inválido. Valores esperados: 'A' o 'E'.");

                if (string.IsNullOrWhiteSpace(tipo))
                    return DbHelper.ErrorResponse("tipo es requerido.");
                var tipoNorm = tipo.Trim().ToUpperInvariant();
                if (tipoNorm == "CXC") tipoNorm = "CXC";

                var tipoOk =
                    tipoNorm == "ESTP" ||
                    tipoNorm == "CRD" ||
                    tipoNorm == "CXC" ||
                    tipoNorm == "INST";

                if (!tipoOk)
                    return DbHelper.ErrorResponse("tipo inválido. Valores esperados: 'ESTP', 'CRD', 'CxC', 'INST'.");

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse(USUARIO_REQUERIDO);

                var p = new DynamicParameters();
                p.Add(P_CLIENTE_ID, cod_cliente, DbType.String);
                p.Add(P_CODIGO, codigo, DbType.String);
                p.Add(P_MOVIMIENTO, movimiento, DbType.String);
                p.Add(P_TIPO, tipoNorm, DbType.String);
                p.Add(P_USUARIO, usuario, DbType.String);

                conn.Execute("spSYS_FE_PARAMETROS_Exclusion", p, commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"FE Exclusión: Cliente {cod_cliente} Tipo {tipoNorm} Código {codigo} Mov {movimiento}",
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
                var fin = new DateTime(fecha_corte.Year, fecha_corte.Month, fecha_corte.Day, 23, 59, 59);

                var p = new DynamicParameters();
                p.Add("@Inicio", ini, DbType.DateTime);
                p.Add("@Corte", fin, DbType.DateTime);

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
            var s = (estado ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(s)) return "T";

            s = s.Substring(0, 1);
            return _whitelistEstados.Contains(s) ? s : "T";
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
            pBase.Add("@ClienteId", dto.cod_cliente, DbType.String);
            pBase.Add("@Factura", dto.factura ?? "", DbType.String);
            pBase.Add("@Cedula", dto.identificacion ?? "", DbType.String);
            pBase.Add("@Nombre", dto.nombre ?? "", DbType.String);

            pBase.Add("@FechaInicio", DateTime.SpecifyKind(ini, DateTimeKind.Unspecified), DbType.DateTime);
            pBase.Add("@FechaCorte", DateTime.SpecifyKind(fin, DateTimeKind.Unspecified), DbType.DateTime);

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

        /// <summary>
        /// Retorna consecutivo.
        /// <param name="rsConsec"></param>
        /// </summary>
        /// <returns></returns>
        private static int GetConsecutivo(object? rsConsec)
        {
            if (rsConsec == null) return 0;

            try
            {
                var d = rsConsec as IDictionary<string, object?>;
                if (d != null && d.TryGetValue("Consecutivo", out var v) && v != null)
                    return Convert.ToInt32(v);
            }
            catch { }

            try
            {
                var p = rsConsec.GetType().GetProperty("Consecutivo");
                var v = p?.GetValue(rsConsec, null);
                if (v == null) return 0;
                return Convert.ToInt32(v);
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// Retorna codigo interno.
        /// <param name="ins"></param>
        /// </summary>
        /// <returns></returns>
        private static long GetCodigoInterno(object? ins)
        {
            if (ins == null) return 0;

            try
            {
                var d = ins as IDictionary<string, object?>;
                if (d != null && d.TryGetValue("CODIGO_INTERNO", out var v) && v != null)
                    return Convert.ToInt64(v);
            }
            catch { }

            try
            {
                var p = ins.GetType().GetProperty("CODIGO_INTERNO");
                var v = p?.GetValue(ins, null);
                if (v == null) return 0;
                return Convert.ToInt64(v);
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// Retorna id de la factura.
        /// <param name="enc"></param>
        /// </summary>
        /// <returns></returns>
        private static long GetIdFactura(object? enc)
        {
            if (enc == null) return 0;

            try
            {
                var d = enc as IDictionary<string, object?>;
                if (d != null && d.TryGetValue("id_Factura", out var v) && v != null)
                    return Convert.ToInt64(v);
            }
            catch { }

            try
            {
                var p = enc.GetType().GetProperty("id_Factura");
                var v = p?.GetValue(enc, null);
                if (v == null) return 0;
                return Convert.ToInt64(v);
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// Aplica sort a la lista de clientes.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        private static void SortClientes(List<FeClienteItem> data, string? sortField, int sortOrder)
        {
            var sf = (sortField ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(sf))
                return;

            int dir = (sortOrder == 1) ? 1 : -1;

            int Cmp(string? a, string? b) =>
                dir * string.Compare(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase);

            switch (sf)
            {
                case "id_prov":
                    data.Sort((a, b) => Cmp(a.id_prov, b.id_prov));
                    return;
                case "tipo_id":
                    data.Sort((a, b) => Cmp(a.tipo_id, b.tipo_id));
                    return;
                case "identificacion":
                    data.Sort((a, b) => Cmp(a.identificacion, b.identificacion));
                    return;
                case "razon_social":
                case "nombre":
                    data.Sort((a, b) => Cmp(a.razon_social, b.razon_social));
                    return;
                case "email1":
                    data.Sort((a, b) => Cmp(a.email1, b.email1));
                    return;
                case "email2":
                    data.Sort((a, b) => Cmp(a.email2, b.email2));
                    return;
                case "telefono1":
                    data.Sort((a, b) => Cmp(a.telefono1, b.telefono1));
                    return;
                case "telefono2":
                    data.Sort((a, b) => Cmp(a.telefono2, b.telefono2));
                    return;
                case "provincia":
                    data.Sort((a, b) => Cmp(a.provincia, b.provincia));
                    return;
                case "canton":
                    data.Sort((a, b) => Cmp(a.canton, b.canton));
                    return;
                case "distrito":
                    data.Sort((a, b) => Cmp(a.distrito, b.distrito));
                    return;
                case "barrio":
                    data.Sort((a, b) => Cmp(a.barrio, b.barrio));
                    return;
                case "direccion":
                    data.Sort((a, b) => Cmp(a.direccion, b.direccion));
                    return;
                default:

                    return;
            }
        }
        /// <summary>
        /// Aplica paginacion a la lista de clientes.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <returns></returns>
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
        /// DEBUG: Describe columnas del resultset de SPs de corte (Notifica_Clientes y Notifica).
        /// Solo para DEV.
        /// </summary>
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
      
    }
}
