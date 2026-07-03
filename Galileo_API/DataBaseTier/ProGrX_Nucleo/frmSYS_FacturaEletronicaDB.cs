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
        private const string TIPO = "@Tipo";
        private const string SqlOffsetFetchLower = " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";
        private const string COD_REQUERIDO = "Código es requerido.";
        private const string INICIO = "@Inicio";
        private const string USUARIO_REQUERIDO = "Usuario es requerido.";
        private const string USUARIO= "@Usuario";
        private const string P_CLIENTE_ID = "@ClienteId";
        private const string P_CODIGO = "@Codigo";
        private const string P_MOVIMIENTO = "@Movimiento";
        private const string P_TIPO = TIPO;
        private const string P_USUARIO = USUARIO;
        private const string P_COD_CLIENTE = "@cod_cliente";
        private const string MENSAJE_COD_CLIENTE = "cod_cliente es requerido.";
        private const string SP_FACTURAS_CONSULTA = "spProGrX_Facturas_Consulta";
        private const string SP_FACTURA_DETALLE = "spProGrX_Factura_Detalle";
        private const string PARAM_NOMBRE = "nombre";
        private const string KEY_TIPO_DOCUMENTO = "Tipo_Documento";
        private const string KEY_NUMERO_CONSECUTIVO = "Numero_Consecutivo";
        private const string KEY_CEDULA = "Cedula";
        private const string KEY_RAZON_SOCIAL = "Razon_Social";
        private const string KEY_FECHA_EMISION = "Fecha_Emision";
        private const string KEY_TOTAL_VENTA = "Total_Venta";
        private const string KEY_TOTAL_EXENTO = "Total_Exento";
        private const string KEY_TOTAL_GRAVADO = "Total_Gravado";
        private const string KEY_TOTAL_IMPUESTOS = "Total_Impuestos";
        private const string KEY_TOTAL_DESCUENTOS = "Total_Descuentos";
        private const string KEY_TOTAL_COMPROBANTE = "Total_Comprobante";
        private const string KEY_CLAVE = "Clave";
        private const string KEY_XML_RESPUESTA = "XML_Respuesta";
        private const string KEY_OBSERVACIONES = "Observaciones";
        private const string KEY_ID_FACTURA = "id_Factura";
        private const string IDENTIFICACION = "identificacion";
        private const string FACTURA = "@Factura";
        private static readonly string[] FECHA_FORMATOS = new[]
        {
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy/MM/dd",
            "yyyy/MM/dd HH:mm:ss"
        };
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
                throw new InvalidOperationException(MENSAJE_COD_CLIENTE);
            var csLocal = _portalDB.ObtenerDbConnStringEmpresa(CodEmpresa);
            using var connLocal = new SqlConnection(csLocal);
            connLocal.Open();

            const string sqlCfg = @"
            select
                rtrim(isnull(ACC_SERVER,'')) as portal_server,
                rtrim(isnull(ACC_DB,''))     as portal_db,
                rtrim(isnull(ACC_USR,''))    as portal_user,
                rtrim(isnull(ACC_KEY,''))    as portal_secret 
            from SYS_FE_PARAMETROS
            where COD_CLIENTE = @cod_cliente;";

            var cfg = connLocal.QueryFirstOrDefault<PortalProveedorConfigRow>(sqlCfg, new { cod_cliente });

            if (cfg == null)
                throw new InvalidOperationException("No se encontró configuración del cliente en SYS_FE_PARAMETROS.");

            string portalServer = (cfg.portal_server ?? "").ToString().Trim();
            string portalDb = (cfg.portal_db ?? "").ToString().Trim();
            string portalUser = (cfg.portal_user ?? "").ToString().Trim();
            string portalSecret = (cfg.portal_secret ?? "").ToString().Trim();

            if (string.IsNullOrWhiteSpace(portalServer) ||
                string.IsNullOrWhiteSpace(portalDb) ||
                string.IsNullOrWhiteSpace(portalUser) ||
                string.IsNullOrWhiteSpace(portalSecret))
                throw new InvalidOperationException("Credenciales del portal proveedor incompletas.");

            var csb = new SqlConnectionStringBuilder
            {
                DataSource = portalServer,
                InitialCatalog = portalDb,
                UserID = portalUser,
                Password = portalSecret,
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
                p.Add(P_COD_CLIENTE, codCliente, DbType.String);
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
                if (!TryParseProcesarCorteInputs(dto, out var codCliente, out var usuario, out var fechaCorte, out var fechaFactura, out var err))
                    return DbHelper.ErrorResponse(err);

                using var connLocal = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                connLocal.Open();

                using var connProveedor = OpenPortalProveedorConn(CodEmpresa, codCliente);

                var cfg = LoadCorteCfg(connLocal, codCliente);
                var cedulaEmisor = ObtenerCedulaEmisor(connLocal, codCliente);
                if (string.IsNullOrWhiteSpace(cedulaEmisor))
                {
                    return DbHelper.ErrorResponse(
                        $"No se pudo obtener la cédula del emisor para el cliente {codCliente}."
                    );
                }

                SyncConsecutivo(connLocal, connProveedor, codCliente);

                EjecutarCorte(connLocal, codCliente, usuario, fechaCorte, fechaFactura);

                var nuevos = ObtenerNuevosClientes(connLocal, codCliente);
                if (nuevos.Count > 0)
                {
                    var respCli = ProcesarNuevosClientes(connLocal, connProveedor, nuevos, codCliente, fechaCorte);
                    if (respCli.Code != 0) return respCli;
                }

                var factRows = ObtenerFactRows(connLocal, codCliente);
                if (factRows.Count > 0)
                {
                    var respFact = ProcesarFacturas(
                        connLocal,
                        connProveedor,
                        factRows,
                        codCliente,
                        usuario,
                        fechaFactura,
                        cfg
                    );

                    if (respFact.Code != 0) return respFact;
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

                var rows = ExecuteFacturasConsulta(conn, dto, ini, fin);
                var lista = MapFacturasRows(rows);

                SortFacturas(lista, filtros!.sortField, filtros.sortOrder);

                response.Result.total = lista.Count;
                response.Result.lista = exportAll
                    ? lista
                    : Page(lista, NormalizePagina(filtros.pagina, exportAll), NormalizePaginacion(filtros.paginacion, exportAll, 30));

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
        /// <param name="comprobante"></param>
        /// <returns></returns>
        public ErrorDto<List<FeFacturaDetalleItem>> FE_Factura_Detalle_Obtener(int CodEmpresa,string codCliente,string idFactura,string tipo)
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
                tipo = (tipo ?? "").Trim();
                if (string.IsNullOrWhiteSpace(tipo))
                    return DbHelper.CreateErrorResponse<List<FeFacturaDetalleItem>>("comprobante es requerido.");

                using var conn = OpenPortalProveedorConn(CodEmpresa, codCliente);
                var rows = conn.Query(
                    $"exec {SP_FACTURA_DETALLE} @p1, @p2, @p3;",
                    new
                    {
                        p1 = codCliente,
                        p2 = idFacturaInt,
                        p3 = tipo
                    },
                    commandType: CommandType.Text
                ).ToList();

                var lista = new List<FeFacturaDetalleItem>(rows.Count);

                foreach (var r in rows)
                {
                    int linea = TryParseInt(ExtractKeyFromParametros(r, "NUM_LINEA"));

                    string? codigo =
                        ExtractKeyFromParametros(r, "DTOCODIGO") ??
                        ExtractKeyFromParametros(r, "TIPO_PRODUCTO");

                    string? producto = ExtractKeyFromParametros(r, "DETALLE");

                    decimal precioUd = TryParseDecimal(ExtractKeyFromParametros(r, "PRECIO_UNITARIO"));
                    decimal qty = TryParseDecimal(ExtractKeyFromParametros(r, "CANTIDAD"));

                    string? unidad = ExtractKeyFromParametros(r, "UNIDAD_MEDIDA");

                    decimal total = TryParseDecimal(ExtractKeyFromParametros(r, "MONTO_TOTAL"));
                    decimal descuento = TryParseDecimal(ExtractKeyFromParametros(r, "MONTO_DESCUENTO"));
                    decimal impuesto = TryParseDecimal(ExtractKeyFromParametros(r, "MONTO_IMPUESTO"));

                    string? cabys = ExtractKeyFromParametros(r, "CABYS_DESC");

                    bool filaVacia =
                        linea == 0
                        && string.IsNullOrWhiteSpace(codigo)
                        && string.IsNullOrWhiteSpace(producto)
                        && precioUd == 0m
                        && qty == 0m
                        && string.IsNullOrWhiteSpace(unidad)
                        && total == 0m
                        && descuento == 0m
                        && impuesto == 0m
                        && string.IsNullOrWhiteSpace(cabys);

                    if (filaVacia)
                        continue;

                    lista.Add(new FeFacturaDetalleItem
                    {
                        linea = linea,
                        codigo = codigo,
                        producto = producto,
                        precio_ud = precioUd,
                        qty = qty,
                        unidad = unidad,
                        total = total,
                        descuento = descuento,
                        impuesto = impuesto,
                        cabys = cabys
                    });
                }

                resp.Result = lista;
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

            var identificacion = (ExtractKeyFromParametros(filtros.parametros, IDENTIFICACION) ?? "").Trim();
            var nombre = (ExtractKeyFromParametros(filtros.parametros, PARAM_NOMBRE) ?? "").Trim();

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
                p.Add(P_COD_CLIENTE, codCliente, DbType.String);

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
                if (metodoRaw == "E" || metodoRaw.StartsWith('E'))
                    metodo = "E";
                else
                    metodo = "D";
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

                return DbHelper.OkResponse(
    "Configuración de Facturación Electrónica guardada correctamente."
);
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

                return DbHelper.OkResponse(
    "Configuración de Facturación Electrónica eliminada correctamente."
);
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
                    return DbHelper.ErrorResponse(MENSAJE_COD_CLIENTE);

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse("Usuario es requerido.");

                using var connLocal = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                EnsureOpen(connLocal);
                AcquireSyncLock(connLocal, cod_cliente);

                var (portalServer, portalDb, portalUser, portalKey) = GetPortalAccesos(connLocal, cod_cliente);

                if (string.IsNullOrWhiteSpace(portalServer) ||
                    string.IsNullOrWhiteSpace(portalDb) ||
                    string.IsNullOrWhiteSpace(portalUser) ||
                    string.IsNullOrWhiteSpace(portalKey))
                    return DbHelper.ErrorResponse("Credenciales del portal proveedor incompletas.");

                using var connPortal = OpenPortalConn(portalServer, portalDb, portalUser, portalKey);
                if (!PortalHasIwCliente(connPortal))
                    return DbHelper.ErrorResponse("No existe la tabla IW_CLIENTE en el portal proveedor. Verifique base de datos/servidor configurados.");

                var portalRows = LoadPortalClientes(connPortal, cod_cliente);

                SyncLocalClientes(connLocal, cod_cliente, usuario, portalRows);

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
                    return DbHelper.CreateErrorResponse<List<FeExclusionItem>>(MENSAJE_COD_CLIENTE);

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
                    return DbHelper.ErrorResponse(MENSAJE_COD_CLIENTE);

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

                var mensaje = movimiento == "A"
                    ? "Exclusión registrada correctamente."
                    : "Exclusión eliminada correctamente.";

                return DbHelper.OkResponse(mensaje);
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
                var fin = new DateTime(fecha_corte.Year,fecha_corte.Month, fecha_corte.Day,23, 59, 59,DateTimeKind.Unspecified);

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

                return DbHelper.OkResponse(
    "Reactivación ejecutada correctamente."
);
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
                identificacion = (ExtractKeyFromParametros(filtros.parametros, IDENTIFICACION) ?? "").Trim(),
                nombre = (ExtractKeyFromParametros(filtros.parametros, PARAM_NOMBRE) ?? "").Trim(),
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
                errMsg = MENSAJE_COD_CLIENTE;
                return false;
            }

            if (!DateTime.TryParseExact(dto.fecha_inicio, FE_DATE_FMT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaInicio))
            {
                errMsg = "Fecha inicio inválida. Formato esperado: YYYY-MM-DD.";
                return false;
            }

            if (!DateTime.TryParseExact(dto.fecha_corte, FE_DATE_FMT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaCorte))
            {
                errMsg = "Fecha de corte inválida. Formato esperado: YYYY-MM-DD.";
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
                IDENTIFICACION => CmpStr(a.identificacion, b.identificacion),
                "razon_social" => CmpStr(a.razon_social, b.razon_social),
                PARAM_NOMBRE => CmpStr(a.razon_social, b.razon_social),
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
            var p = new DynamicParameters();
            p.Add("@ClienteId", dto.cod_cliente, DbType.String);
            p.Add("@Cedula", (dto.identificacion ?? "").Trim(), DbType.String);
            p.Add("@Nombre", (dto.nombre ?? "").Trim(), DbType.String);
            p.Add(FACTURA, (dto.factura ?? "").Trim(), DbType.String);

            p.Add("@FechaInicio", ini, DbType.DateTime);
            p.Add("@FechaCorte", fin, DbType.DateTime);
            p.Add("@Estado", (dto.estado ?? "T").Trim(), DbType.String);

            return p;
        }
        /// <summary>
        /// Ejecuta cabecera (Tipo = 'R').
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="pBase"></param>
        /// <returns></returns>
        private static object? ExecuteFacturasResumenHead(IDbConnection conn, DynamicParameters pBase)
        {
            var p = new DynamicParameters(pBase);
            p.Add(TIPO, "R", DbType.String);

            return conn.QueryFirstOrDefault(
                "spProGrX_Facturas_Consulta_Rsm",
                p,
                commandType: CommandType.StoredProcedure
            );
        }
        private static FeFacturasResumenCabecera MapFacturasResumenCabecera(object? head)
        {
            var cab = new FeFacturasResumenCabecera
            {
                no_facturas = 0,
                inicio = null,
                corte = null,
                monto_facturado = 0m
            };

            if (head is not IDictionary<string, object?> d)
                return cab;

            cab.no_facturas = TryParseInt(GetVal(d, "FACTURAS"));
            cab.monto_facturado = TryParseDecimal(GetVal(d, "TOTAL_VENTA"));
            cab.inicio = TryParseDate(GetVal(d, "INICIO"));
            cab.corte = TryParseDate(GetVal(d, "CORTE"));

            return cab;
        }
        private static string? GetVal(IDictionary<string, object?> d, string key)
        {
            if (!d.TryGetValue(key, out var v) || v == null || v == DBNull.Value)
                return null;
            return v is DateTime dt ? dt.ToString("yyyy-MM-ddTHH:mm:ss") : v.ToString();
        }
        private static DateTime? TryParseDate(string? s)
        {
            s = (s ?? "").Trim();
            if (s.Length == 0) return null;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            if (DateTime.TryParseExact(s, FECHA_FORMATOS, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;

            return null;
        }
        private static DateTime? TryParseDateTimeNullable(string? s)
        {
            s = (s ?? "").Trim();
            if (s.Length == 0) return null;

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;

            if (DateTime.TryParse(s, new CultureInfo("es-CR"), DateTimeStyles.None, out d))
                return d;

            return null;
        }
        /// <summary>
        /// Ejecuta detalle.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="pBase"></param>
        /// <returns></returns>
        private List<FeFacturaResumenItem> ExecuteFacturasResumenDetalle(IDbConnection conn, DynamicParameters pBase)
        {
            var p = new DynamicParameters(pBase);
            p.Add(TIPO, "D", DbType.String);

            var rows = conn.Query(
                "spProGrX_Facturas_Consulta_Rsm",
                p,
                commandType: CommandType.StoredProcedure
            ).ToList();

            var lista = new List<FeFacturaResumenItem>(rows.Count);
            foreach (var r in rows)
                lista.Add(MapFacturasResumenDetalleItem(r));

            return lista;
        }
        private FeFacturaResumenItem MapFacturasResumenDetalleItem(object r)
        {
            var item = new FeFacturaResumenItem
            {
                tipo = FirstNonEmpty(r, "TIPO", "TIPO_DOC", "TIPO_DOCUMENTO", "DOC", "TIPO_COMPROBANTE"),
                lineas = FirstNonZeroInt(r, "LINEAS", "NUM_LINEAS", "CANTIDAD"),
                detalle = FirstNonEmpty(r, "DETALLE", "DESCRIPCION", "PRODUCTO"),
                facturado = FirstNonZeroDecimal( r,"TOTAL_VENTA","FACTURADO","MONTO_FACTURADO","TOTAL_COMPROBANTE","SUBTOTAL","MONTO_TOTAL","TOTAL"),
                inicio = TryParseDateTimeNullable(FirstAny(r, "INICIO", "FECHA_INICIO")),
                corte = TryParseDateTimeNullable(FirstAny(r, "CORTE", "FECHA_CORTE")),
                xml_respuesta = FirstNonEmpty(r, "XML_RESPUESTA", "ESTADO", "RESPUESTA"),
            };

            NormalizeTipo(item);
            return item;
        }
        private static void NormalizeTipo(FeFacturaResumenItem item)
        {
            if (string.IsNullOrWhiteSpace(item.tipo)) return;

            var t = item.tipo.Trim();
            if (t == "01") item.tipo = "FE";
            else if (t == "03") item.tipo = "NC";
        }
        private static string? FirstAny(object r, params string[] keys)
        {
            if (keys == null || keys.Length == 0) return null;

            return keys
                .Select(k => ExtractKeyFromParametros(r, k))
                .FirstOrDefault(v => v != null);
        }
        private static string? FirstNonEmpty(object r, params string[] keys)
        {
            if (keys == null || keys.Length == 0) return null;

            return keys
                .Select(k => ExtractKeyFromParametros(r, k))
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        }
        private static int FirstNonZeroInt(object r, params string[] keys)
        {
            if (keys == null || keys.Length == 0)
                return TryParseInt(ExtractKeyFromParametros(r, ""));

            var firstNonZero = keys
                .Select(k => TryParseInt(ExtractKeyFromParametros(r, k)))
                .FirstOrDefault(n => n != 0);

            return firstNonZero != 0
                ? firstNonZero
                : TryParseInt(ExtractKeyFromParametros(r, keys[^1]));
        }
        private static decimal FirstNonZeroDecimal(object r, params string[] keys)
        {
            if (keys == null || keys.Length == 0)
                return TryParseDecimal(ExtractKeyFromParametros(r, ""));

            var firstNonZero = keys
                .Select(k => TryParseDecimal(ExtractKeyFromParametros(r, k)))
                .FirstOrDefault(n => n != 0m);

            return firstNonZero != 0m
                ? firstNonZero
                : TryParseDecimal(ExtractKeyFromParametros(r, keys[^1]));
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

        private static List<object> ExecuteFacturasConsulta(SqlConnection conn, FeFacturasParametrosDto dto, DateTime ini, DateTime fin)
        {
            var p = new DynamicParameters();
            p.Add(P_CLIENTE_ID, dto.cod_cliente, DbType.String);
            p.Add(FACTURA, dto.factura ?? "", DbType.String);
            p.Add("@Cedula", dto.identificacion ?? "", DbType.String);
            p.Add("@Nombre", dto.nombre ?? "", DbType.String);
            p.Add("@FechaInicio", ini, DbType.DateTime);
            p.Add("@FechaCorte", fin, DbType.DateTime);
            p.Add("@Estado", NormalizeEstado(dto.estado), DbType.String);

            return conn.Query(
                SP_FACTURAS_CONSULTA,
                p,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }
        private static List<FeFacturaItem> MapFacturasRows(List<object> rows)
        {
            var lista = new List<FeFacturaItem>();
            foreach (var r in rows)
                lista.Add(MapFacturaRow(r));

            return lista;
        }
        private static FeFacturaItem MapFacturaRow(object r)
        {
            var tipoDoc = ExtractKeyFromParametros(r, KEY_TIPO_DOCUMENTO);
            var fecha = ReadFechaEmision(r);

            var numeroConsecutivo = (ExtractKeyFromParametros(r, KEY_NUMERO_CONSECUTIVO) ?? "").Trim();
            var claveComprobante = (ExtractKeyFromParametros(r, KEY_CLAVE) ?? "").Trim();

            return new FeFacturaItem
            {
                tipo = (tipoDoc == "01") ? "FE" : "NC",

                comprobante = numeroConsecutivo,

                identificacion = ExtractKeyFromParametros(r, KEY_CEDULA),
                razon_social = ExtractKeyFromParametros(r, KEY_RAZON_SOCIAL),

                fecha = fecha,

                total = TryParseDecimal(ExtractKeyFromParametros(r, KEY_TOTAL_VENTA)),
                total_exento = TryParseDecimal(ExtractKeyFromParametros(r, KEY_TOTAL_EXENTO)),
                total_gravado = TryParseDecimal(ExtractKeyFromParametros(r, KEY_TOTAL_GRAVADO)),
                total_impuestos = TryParseDecimal(ExtractKeyFromParametros(r, KEY_TOTAL_IMPUESTOS)),
                total_descuentos = TryParseDecimal(ExtractKeyFromParametros(r, KEY_TOTAL_DESCUENTOS)),
                total_comprobante = TryParseDecimal(ExtractKeyFromParametros(r, KEY_TOTAL_COMPROBANTE)),
                clave = claveComprobante,

                xml_respuesta = ExtractKeyFromParametros(r, KEY_XML_RESPUESTA),
                observaciones = ExtractKeyFromParametros(r, KEY_OBSERVACIONES),

                id_factura = TryParseInt(ExtractKeyFromParametros(r, KEY_ID_FACTURA))
            };
        }
        private static DateTime? ReadFechaEmision(object r)
        {
            if (r is IDictionary<string, object> d &&
                d.TryGetValue(KEY_FECHA_EMISION, out var v) &&
                v != null && v != DBNull.Value)
            {
                return ParseFechaValue(v);
            }

            var s = ExtractKeyFromParametros(r, KEY_FECHA_EMISION);
            return ParseFechaString(s);
        }
        private static DateTime? ParseFechaValue(object v)
        {
            if (v is DateTime dt)
                return dt;

            if (v is DateTimeOffset dto)
                return dto.DateTime;

            return ParseFechaString(v.ToString());
        }
        private static DateTime? ParseFechaString(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;

            if (DateTime.TryParseExact(
                    s,
                    FECHA_FORMATOS,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var f))
                return f;

            if (DateTime.TryParse(
                    s,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var f2))
                return f2;

            return null;
        }
        private static decimal TryParseDecimal(string? s)
        {
            s = (s ?? "").Trim();
            if (s.Length == 0) return 0m;

            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return d;

            if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("es-CR"), out d))
                return d;
            s = s.Replace(",", ".");
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                return d;

            return 0m;
        }
        private static int TryParseInt(string? s)
        {
            s = (s ?? "").Trim();
            return int.TryParse(s, out var i) ? i : 0;
        }
        /// <summary>
        /// Retorna consecutivo.
        /// </summary>
        private static int GetConsecutivo(object? rsConsec)
        {
            if (rsConsec == null) return 0;

            if (rsConsec is IDictionary<string, object?> d)
            {
                if (d.TryGetValue("Consecutivo", out var v) && v != null && v != DBNull.Value)
                    return Convert.ToInt32(v);

                if (d.TryGetValue("CONSECUTIVO", out v) && v != null && v != DBNull.Value)
                    return Convert.ToInt32(v);
            }

            var p = rsConsec.GetType().GetProperty("Consecutivo") ?? rsConsec.GetType().GetProperty("CONSECUTIVO");
            if (p == null) return 0;

            var val = p.GetValue(rsConsec, null);
            if (val == null || val == DBNull.Value) return 0;

            return Convert.ToInt32(val);
        }
        /// <summary>
        /// Retorna codigo interno.
        /// </summary>
        private static long GetCodigoInterno(object? ins)
        {
            if (ins == null) return 0;

            if (ins is IDictionary<string, object?> d)
            {
                if (d.TryGetValue("CODIGO_INTERNO", out var v) && v != null && v != DBNull.Value)
                    return Convert.ToInt64(v);

                if (d.TryGetValue("Codigo_Interno", out v) && v != null && v != DBNull.Value)
                    return Convert.ToInt64(v);
            }

            var t = ins.GetType();
            var p = t.GetProperty("CODIGO_INTERNO") ?? t.GetProperty("Codigo_Interno");
            if (p == null) return 0;

            var val = p.GetValue(ins, null);
            if (val == null || val == DBNull.Value) return 0;

            return Convert.ToInt64(val);
        }
        /// <summary>
        /// Retorna id de la factura.
        /// </summary>
        private static long GetIdFactura(object? enc)
        {
            if (enc == null) return 0;

            if (enc is IDictionary<string, object?> d)
            {
                if (d.TryGetValue("id_Factura", out var v) && v != null && v != DBNull.Value)
                    return Convert.ToInt64(v);

                if (d.TryGetValue("ID_FACTURA", out v) && v != null && v != DBNull.Value)
                    return Convert.ToInt64(v);
            }

            var t = enc.GetType();
            var p = t.GetProperty("id_Factura") ?? t.GetProperty("ID_FACTURA");
            if (p == null) return 0;

            var val = p.GetValue(enc, null);
            if (val == null || val == DBNull.Value) return 0;

            return Convert.ToInt64(val);
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
                case IDENTIFICACION:
                    data.Sort((a, b) => Cmp(a.identificacion, b.identificacion));
                    return;
                case "razon_social":
                case PARAM_NOMBRE:
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
        private static (string server, string db, string usr, string key) GetPortalAccesos(SqlConnection conn, string cod_cliente)
        {
            const string sql = @"
            select
                rtrim(isnull(ACC_SERVER,'')) as server,
                rtrim(isnull(ACC_DB,''))     as db,
                rtrim(isnull(ACC_USR,''))    as usr,
                rtrim(isnull(ACC_KEY,''))    as [secret]
            from SYS_FE_PARAMETROS
            where COD_CLIENTE = @cod_cliente;";

            var row = conn.QueryFirstOrDefault<PortalSyncConfigRow>(sql, new { cod_cliente });
            if (row == null)
                return ("", "", "", "");

            return (
                (row.server ?? string.Empty).Trim(),
                (row.db ?? string.Empty).Trim(),
                (row.usr ?? string.Empty).Trim(),
                (row.secret ?? string.Empty).Trim()
            );
        }
        private static bool TryParseProcesarCorteInputs(
    FeProcesarCorteDto dto,
    out string codCliente,
    out string usuario,
    out DateTime fechaCorte,
    out DateTime fechaFactura,
    out string err)
        {
            codCliente = (dto.cod_cliente ?? "").Trim();
            usuario = (dto.usuario ?? "").Trim().ToUpperInvariant();
            fechaCorte = default;
            fechaFactura = default;
            err = "";

            if (string.IsNullOrWhiteSpace(codCliente))
            {
                err = MENSAJE_COD_CLIENTE;
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                err = USUARIO_REQUERIDO;
                return false;
            }

            if (!DateTime.TryParseExact(
                    (dto.fecha_corte ?? "").Trim(),
                    FE_DATE_FMT,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out fechaCorte))
            {
                err = "fecha_corte inválida. Formato esperado: YYYY-MM-DD.";
                return false;
            }

            if (!DateTime.TryParseExact(
                    (dto.fecha_factura ?? "").Trim(),
                    FE_DATE_FMT,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out fechaFactura))
            {
                err = "fecha_factura inválida. Formato esperado: YYYY-MM-DD.";
                return false;
            }

            var hoy = DateTime.Today;

            if (fechaFactura.Date > hoy)
            {
                err = "La fecha de facturación no puede ser mayor a la fecha actual.";
                return false;
            }

            if (fechaFactura.Date < hoy.AddDays(-3))
            {
                err = "La fecha de facturación no puede tener más de tres días de antigüedad.";
                return false;
            }

            return true;
        }
        private readonly record struct CorteCfg(
    bool incluyePolizas,
    bool incluyePrincipal,
    string actividad,
    string monedaCfg,
    string codSucursal,
    string terminalPOS
);
        private static CorteCfg LoadCorteCfg(SqlConnection connLocal, string codCliente)
        {
            const string sqlParams = @"
            select
                isnull(INCLUYE_POLIZAS,0)        as incluye_polizas,
                isnull(INCLUYE_PRINCIPAL,0)     as incluye_principal,
                rtrim(isnull(ACTIVIDAD_ECONOMICA,'')) as actividad,
                rtrim(isnull(MONEDA,''))        as moneda,
                rtrim(isnull(SUCURSAL,''))      as sucursal,
                rtrim(isnull(TERMINAL,''))      as terminal
            from SYS_FE_PARAMETROS
            where COD_CLIENTE = @cod_cliente;";

            var pCfg = connLocal.QueryFirstOrDefault<CorteCfgRow>(sqlParams, new { cod_cliente = codCliente });
            if (pCfg == null)
                throw new InvalidOperationException("No se encontró configuración en SYS_FE_PARAMETROS para el cliente.");

            bool incluyePolizas = Convert.ToInt32(pCfg.incluye_polizas) == 1;
            bool incluyePrincipal = Convert.ToInt32(pCfg.incluye_principal) == 1;

            string actividad = Convert.ToString(pCfg.actividad) ?? "";
            string monedaCfg = (Convert.ToString(pCfg.moneda) ?? "").Trim();

            string codSucursal = (Convert.ToString(pCfg.sucursal) ?? "").Trim();
            if (string.IsNullOrWhiteSpace(codSucursal))
                codSucursal = "2";

            string terminalPOS = (Convert.ToString(pCfg.terminal) ?? "").Trim();
            if (string.IsNullOrWhiteSpace(terminalPOS))
                terminalPOS = "00001";

            return new CorteCfg(
                incluyePolizas,
                incluyePrincipal,
                actividad,
                monedaCfg,
                codSucursal,
                terminalPOS
            );
        }
        private static void SyncConsecutivo(SqlConnection connLocal, SqlConnection connProveedor, string codCliente)
        {
            var rsConsec = connProveedor.QueryFirstOrDefault<dynamic>(
                "exec spProGrX_Cliente_Consecutivo @cliente;",
                new { cliente = codCliente }
            );

            int consecutivo = GetConsecutivo(rsConsec);
            if (consecutivo > 0)
            {
                connLocal.Execute(
                    "update SYS_FE_PARAMETROS set CONSECUTIVO_FE = @c where COD_CLIENTE = @cod_cliente;",
                    new { c = consecutivo, cod_cliente = codCliente }
                );
            }
        }
        private static void EjecutarCorte(SqlConnection connLocal, string codCliente, string usuario, DateTime fechaCorte, DateTime fechaFactura)
        {
            var pCorte = new DynamicParameters();
            pCorte.Add("@Cliente", codCliente, DbType.String);
            pCorte.Add("@Corte", DayEnd(fechaCorte), DbType.DateTime);
            pCorte.Add(USUARIO, usuario, DbType.String);
            pCorte.Add("@FechaFactura", DateTime.SpecifyKind(fechaFactura, DateTimeKind.Unspecified), DbType.DateTime);

            connLocal.Execute("spCrd_Facturacion_Corte", pCorte, commandType: CommandType.StoredProcedure);
        }
        private static List<dynamic> ObtenerNuevosClientes(SqlConnection connLocal, string codCliente)
        {
            return connLocal.Query(
                "spCrd_Facturacion_Notifica_Clientes",
                new { Cliente = codCliente },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }
        private static ErrorDto ProcesarNuevosClientes(SqlConnection connLocal,SqlConnection connProveedor,List<dynamic> nuevos,string codClienteDefault,DateTime fechaCorte)
        {
            foreach (var r in nuevos)
            {
                string codCliRow = (Convert.ToString(r.COD_CLIENTE) ?? codClienteDefault).Trim();
                string tipoId = Convert.ToString(r.TIPO_ID) ?? Convert.ToString(r.Tipo_Id) ?? "";
                string clienteId = Convert.ToString(r.CLIENTE_ID) ?? Convert.ToString(r.Cliente_ID) ?? "";
                string cedula = (Convert.ToString(r.CEDULA) ?? Convert.ToString(r.Cedula) ?? "").Trim();
                string nombre = Convert.ToString(r.NOMBRE) ?? Convert.ToString(r.Nombre) ?? "";
                string email = Convert.ToString(r.EMAIL) ?? Convert.ToString(r.Email) ?? "";
                string direccion = Convert.ToString(r.DIRECCION) ?? Convert.ToString(r.Direccion) ?? "";

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

                int cErr;
                string mErr;
                if (TryReadSpError(ins, out cErr, out mErr))
                    return DbHelper.ErrorResponse(mErr.Length > 0 ? mErr : $"Error proveedor sp_IW_CLIENTEInsert_ProGrX (code {cErr}).");

                long codigoInterno = GetCodigoInterno(ins);
                if (codigoInterno <= 0)
                    return DbHelper.ErrorResponse("Proveedor no devolvió CLIENTE_ID_FE (código interno) para el cliente insertado.");

                if (codigoInterno > int.MaxValue)
                    return DbHelper.ErrorResponse("Proveedor devolvió CLIENTE_ID_FE fuera de rango (int).");

                var p = new DynamicParameters();
                p.Add("@Cliente", codCliRow, DbType.String);
                p.Add("@Identificacion", cedula, DbType.String);
                p.Add("@Id", (int)codigoInterno, DbType.Int32);

                connLocal.Execute(
                    "spCrd_Facturacion_Notifica_Clientes_Result",
                    p,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 360
                );
            }

            return DbHelper.OkResponse("Ok");
        }
        private static List<dynamic> ObtenerFactRows(SqlConnection connLocal, string codCliente)
        {
            return connLocal.Query(
                "spCrd_Facturacion_Notifica",
                new { ClienteID = codCliente },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }
        private static string ObtenerCedulaEmisor(SqlConnection connLocal, string codCliente)
        {
            const string sql = @"
        select top 1 rtrim(isnull(CEDULA,''))
        from SYS_FE_PARAMETROS
        where COD_CLIENTE = @cod_cliente;";

            return (connLocal.QueryFirstOrDefault<string>(
                sql,
                new { cod_cliente = codCliente }
            ) ?? "").Trim();
        }
        private static ErrorDto ProcesarFacturas(SqlConnection connLocal,SqlConnection connProveedor,List<dynamic> factRows,string codCliente,string usuario,DateTime fechaFactura,CorteCfg cfg)
        {
            string cedulaEmisor = ObtenerCedulaEmisor(connLocal, codCliente);
            if (string.IsNullOrWhiteSpace(cedulaEmisor))
                return DbHelper.ErrorResponse("No se pudo obtener la cédula del EMISOR.");

            const string situacion = "1";
            const string tipoComprobante = "01";

            var mapCtx = new FacturaMapCtx(
                fechaFactura: fechaFactura,
                cfg: cfg,
                cedulaEmisor: cedulaEmisor,
                situacion: situacion,
                tipoComprobante: tipoComprobante
            );
            var buffer = new List<NotificaFacturaItem>(256);

            foreach (var r in factRows)
            {
                var dto = MapFacturaProveedorRow(r, mapCtx);
                if (dto == null) continue;

                var respEnc = InsertEncProveedor(connProveedor, codCliente, usuario, dto, cfg);
                if (respEnc.Code != 0) return respEnc;

                var respDet = InsertDetProveedor(connProveedor, codCliente, dto, cfg);
                if (respDet.Code != 0) return respDet;

                buffer.Add(new NotificaFacturaItem(codCliente, dto.comprobanteInterno, dto.idFactura, usuario));

                if (buffer.Count >= 200)
                {
                    FlushNotificaLocal(connLocal, buffer);
                }
            }

            FlushNotificaLocal(connLocal, buffer);

            return DbHelper.OkResponse("Ok");
        }
        private static void FlushNotificaLocal(SqlConnection connLocal, List<NotificaFacturaItem> buffer)
        {
            if (buffer.Count == 0) return;

            foreach (var it in buffer)
            {
                var p = new DynamicParameters();
                p.Add("@Cliente", it.CodCliente, DbType.String);
                p.Add(FACTURA, it.ComprobanteInterno, DbType.String);
                p.Add("@Id", (int)it.IdFactura, DbType.Int32);
                p.Add(USUARIO, it.Usuario, DbType.String);

                connLocal.Execute(
                    "spCrd_Facturacion_Notifica_Result",
                    p,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 360
                );
            }

            buffer.Clear();
        }
        private static FacturaProcDto? MapFacturaProveedorRow(dynamic r, FacturaMapCtx ctx)
        {
            string comprobanteInterno = (Convert.ToString(r.FAC_NUMERO) ?? "").Trim();
            if (string.IsNullOrWhiteSpace(comprobanteInterno))
                return null;

            DateTime fechaTransac = DateTime.SpecifyKind(ctx.fechaFactura, DateTimeKind.Unspecified);

            string clave50 = MHaciendaDB.fxHacienda_Clave50((
                codPais: "506",
                fechaTransac: fechaTransac,
                idEmpresa: ctx.cedulaEmisor,
                codSucursal: ctx.cfg.codSucursal,
                terminalPOS: ctx.cfg.terminalPOS,
                comprobanteInterno: comprobanteInterno,
                situacionComprobante: ctx.situacion,
                tipoComprobante: ctx.tipoComprobante
            ));

            string clave20 = MHaciendaDB.fxHacienda_Clave20(
                ctx.cfg.codSucursal, ctx.cfg.terminalPOS, comprobanteInterno, ctx.tipoComprobante
            );

            decimal intCor = r.INTCOR == null ? 0m : Convert.ToDecimal(r.INTCOR);
            decimal intMor = r.INTMOR == null ? 0m : Convert.ToDecimal(r.INTMOR);
            decimal cargos = r.CARGOS == null ? 0m : Convert.ToDecimal(r.CARGOS);
            decimal poliza = r.POLIZA == null ? 0m : Convert.ToDecimal(r.POLIZA);
            decimal principal = r.PRINCIPAL == null ? 0m : Convert.ToDecimal(r.PRINCIPAL);

            decimal totalGravado = 0m;
            decimal totalExento = intCor + intMor + cargos;

            if (ctx.cfg.incluyePolizas) totalExento += poliza;
            if (ctx.cfg.incluyePrincipal) totalExento += principal;

            string moneda = ((Convert.ToString(r.MONEDA) ?? ctx.cfg.monedaCfg) ?? "").Trim();
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

            return new FacturaProcDto
            {
                comprobanteInterno = comprobanteInterno,
                clave50 = clave50,
                clave20 = clave20,

                intCor = intCor,
                intMor = intMor,
                cargos = cargos,
                poliza = poliza,
                principal = principal,

                totalGravado = totalGravado,
                totalExento = totalExento,

                moneda = moneda,
                tipoCambio = tipoCambio,

                emailDestino = emailDestino,
                enviarCliente = enviarCliente,

                clienteIdDestino = clienteIdDestino,
                tipoId = tipoId.PadLeft(2, '0')
            };
        }
        private static ErrorDto InsertEncProveedor(SqlConnection connProveedor,string codCliente,string usuario, FacturaProcDto dto,CorteCfg cfg)
        {
            DateTime now = DateTime.Now;
            string fechaSp = $"{DateTime.SpecifyKind(now, DateTimeKind.Unspecified):yyyy/MM/dd HH:mm:ss}";

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
                    CLAVE50 = dto.clave50,
                    CLAVE20 = dto.clave20,
                    CLIENTE_ID = dto.clienteIdDestino,
                    MONEDA = dto.moneda,
                    SUCURSAL = cfg.codSucursal,
                    FAC = dto.comprobanteInterno,
                    EMAIL = dto.emailDestino,
                    NOENV = dto.enviarCliente,
                    FECHA = fechaSp,
                    TERM = cfg.terminalPOS,
                    TC = dto.tipoCambio.ToString(CultureInfo.InvariantCulture),
                    TIPO_COMP = "01",
                    TIPO_ID = dto.tipoId,

                    TOT_GR = dto.totalGravado.ToString(CultureInfo.InvariantCulture),
                    TOT_EX = dto.totalExento.ToString(CultureInfo.InvariantCulture),
                    TOT = (dto.totalGravado + dto.totalExento).ToString(CultureInfo.InvariantCulture),
                    DESC = "0",
                    SUBTOT = (dto.totalGravado + dto.totalExento).ToString(CultureInfo.InvariantCulture),
                    IMP = "0",
                    TOTAL = (dto.totalGravado + dto.totalExento).ToString(CultureInfo.InvariantCulture),

                    USR = usuario,
                    NOW = $"{now:yyyy/MM/dd HH:mm:ss}",
                    ACT = cfg.actividad
                }
            );

            int cErrEnc;
            string mErrEnc;
            if (TryReadSpError(enc, out cErrEnc, out mErrEnc))
                return DbHelper.ErrorResponse(mErrEnc.Length > 0 ? mErrEnc : $"Error proveedor sp_IW_ENC_FACTURAInsert_ProGrX (code {cErrEnc}).");

            long idFactura = GetIdFactura(enc);
            if (idFactura == 0)
                return DbHelper.ErrorResponse("Proveedor no devolvió IdFactura al insertar encabezado.");

            dto.idFactura = idFactura;
            return DbHelper.OkResponse("Ok");
        }
        private static ErrorDto InsertDetProveedor(SqlConnection connProveedor,string codCliente,FacturaProcDto dto,CorteCfg cfg)
        {
            try
            {
                int linea = 0;

                InsertFacturaDetalleProveedor(
                    connProveedor,
                    codCliente,
                    dto.idFactura,
                    ref linea,
                    new FacturaDetDto("CRD001", "I", "INTERES CORRIENTE DEL MES", dto.intCor, dto.clave50)
                );

                InsertFacturaDetalleProveedor(
                    connProveedor,
                    codCliente,
                    dto.idFactura,
                    ref linea,
                    new FacturaDetDto("CRD002", "I", "INTERES ATRASADOS", dto.intMor, dto.clave50)
                );

                InsertFacturaDetalleProveedor(
                    connProveedor,
                    codCliente,
                    dto.idFactura,
                    ref linea,
                    new FacturaDetDto("CRD003", "I", "CARGOS ADM Y DE FORMALIZACION", dto.cargos, dto.clave50)
                );

                if (cfg.incluyePolizas)
                {
                    InsertFacturaDetalleProveedor(
                        connProveedor,
                        codCliente,
                        dto.idFactura,
                        ref linea,
                        new FacturaDetDto("CRD004", "Unid", "POLIZAS DEL CREDITO", dto.poliza, dto.clave50)
                    );
                }

                if (cfg.incluyePrincipal)
                {
                    InsertFacturaDetalleProveedor(
                        connProveedor,
                        codCliente,
                        dto.idFactura,
                        ref linea,
                        new FacturaDetDto("CRD005", "Unid", "ABONO AL CREDITO", dto.principal, dto.clave50)
                    );
                }

                return DbHelper.OkResponse("Ok");
            }
            catch (InvalidOperationException exDet)
            {
                return DbHelper.ErrorResponse(exDet.Message);
            }
        }

        private static void InsertFacturaDetalleProveedor(SqlConnection connProveedor, string codCliente, long idFactura, ref int linea,FacturaDetDto dto)
        {
            if (dto.monto <= 0) return;
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
                    CODIGO = dto.codigo,
                    UNIDAD = dto.unidad,
                    DETALLE = dto.detalle,
                    PRECIO = dto.monto.ToString(CultureInfo.InvariantCulture),
                    MONTO = dto.monto.ToString(CultureInfo.InvariantCulture),
                    CLAVE50 = dto.clave50
                }
            );

            int cErrDet;
            string mErrDet;
            if (TryReadSpError(det, out cErrDet, out mErrDet))
                throw new InvalidOperationException(
                    mErrDet.Length > 0 ? mErrDet : $"Error proveedor sp_IW_DET_FACTURAInsert_ProGrX (code {cErrDet})."
                );
        }
        private static bool TryReadSpError(object? row, out int code, out string desc)
        {
            code = 0;
            desc = "";

            if (row == null)
                return false;

            if (row is IDictionary<string, object?> dict)
            {
                if (TryReadFromDict(dict, out code, out desc))
                    return code != 0;

                return false;
            }
            try
            {
                var type = row.GetType();

                var pCode = type.GetProperty("code") ?? type.GetProperty("Code");
                if (pCode == null)
                    return false;

                var vCode = pCode.GetValue(row);
                if (vCode == null)
                    return false;

                code = Convert.ToInt32(vCode);

                var pDesc = type.GetProperty("description") ?? type.GetProperty("Description");
                if (pDesc != null)
                {
                    var vDesc = pDesc.GetValue(row);
                    if (vDesc != null)
                        desc = Convert.ToString(vDesc) ?? "";
                }

                return code != 0;
            }
            catch
            {
                return false;
            }
        }
        private static bool TryReadFromDict(IDictionary<string, object?> dict,out int code,out string desc)
        {
            code = 0;
            desc = "";

            if (TryGetDictValue(dict, "code", out var c) ||
                TryGetDictValue(dict, "Code", out c))
            {
                code = Convert.ToInt32(c);

                if (TryGetDictValue(dict, "description", out var d) ||
                    TryGetDictValue(dict, "Description", out d))
                {
                    desc = Convert.ToString(d) ?? "";
                }

                return true;
            }

            return false;
        }
        private static bool TryGetDictValue(IDictionary<string, object?> dict,string key,out object? value)
        {
            if (dict.TryGetValue(key, out value) && value != null)
                return true;

            value = null;
            return false;
        }
        private static void EnsureOpen(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }
        private static void AcquireSyncLock(SqlConnection connLocal, string cod_cliente)
        {
            connLocal.Execute(
                "exec sp_getapplock @Resource=@r, @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=15000;",
                new { r = $"FE_CLIENTES_SINC_{cod_cliente}" },
                commandTimeout: 360
            );
        }
        private static SqlConnection OpenPortalConn(string server, string db, string usr, string secret)
        {
            var csb = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = db,
                UserID = usr,
                Password = secret,
                ConnectTimeout = 15,
                Encrypt = false,
                TrustServerCertificate = true
            };

            var connPortal = new SqlConnection(csb.ConnectionString);
            connPortal.Open();
            return connPortal;
        }
        private static bool PortalHasIwCliente(SqlConnection connPortal)
        {
            var exists = connPortal.QueryFirstOrDefault<int>(
                "select case when object_id('dbo.IW_CLIENTE','U') is null then 0 else 1 end;"
            );
            return exists == 1;
        }
        private static List<dynamic> LoadPortalClientes(SqlConnection connPortal, string cod_cliente)
        {
            const string sqlPortal = @"
        select
            [ID]                as CLIENTE_ID_FE,
            CODIGO              as CLIENTE_ID,
            rtrim(CEDULA)       as CEDULA,
            rtrim(RAZON_SOCIAL) as NOMBRE
        from IW_CLIENTE
        where ID_CLIENTE_ORIGEN = @cod_cliente
        order by [ID];";

            return connPortal.Query(sqlPortal, new { cod_cliente }).ToList();
        }
        private static void SyncLocalClientes(SqlConnection connLocal, string cod_cliente, string usuario, List<dynamic> portalRows)
        {
            using var tx = connLocal.BeginTransaction();

            try
            {
                DeleteLocalClientes(connLocal, tx, cod_cliente);
                InsertLocalClientes(connLocal, tx, cod_cliente, usuario, portalRows);
                tx.Commit();
            }
            catch (SqlException)
            {
                try { tx.Rollback(); } catch { /* no-op */ }
                throw;
            }
        }
        private static void DeleteLocalClientes(SqlConnection connLocal, SqlTransaction tx, string cod_cliente)
        {
            const string sqlDelete = @"delete SYS_FE_CLIENTES where COD_CLIENTE = @cod_cliente;";
            connLocal.Execute(sqlDelete, new { cod_cliente }, transaction: tx, commandTimeout: 360);
        }
        private static void InsertLocalClientes(SqlConnection connLocal,SqlTransaction tx,string cod_cliente,string usuario,List<dynamic> portalRows)
        {
            var seenClienteId = new HashSet<long>();
            var seenClienteIdFe = new HashSet<long>();
            const string SQL_INSERT = @"
                insert into SYS_FE_CLIENTES (COD_CLIENTE, USUARIO, CLIENTE_ID, CLIENTE_ID_FE, CEDULA, NOMBRE)
                values (@COD_CLIENTE, @USUARIO, @CLIENTE_ID, @CLIENTE_ID_FE, @CEDULA, @NOMBRE);
                ";

            foreach (var r in portalRows)
            {
                if (!TryGetClienteIds(r, out long clienteId, out long clienteIdFe))
                    continue;

                if (!seenClienteId.Add(clienteId)) continue;
                if (!seenClienteIdFe.Add(clienteIdFe)) continue;

                string cedula = ((r.CEDULA ?? "").ToString()).Trim();
                string nombre = ((r.NOMBRE ?? "").ToString()).Trim();

                connLocal.Execute(
                    SQL_INSERT,
                    new
                    {
                        COD_CLIENTE = (cod_cliente ?? "").Trim(),
                        USUARIO = (usuario ?? "").Trim(),
                        CLIENTE_ID = clienteId,
                        CLIENTE_ID_FE = clienteIdFe,
                        CEDULA = cedula,
                        NOMBRE = nombre
                    },
                    transaction: tx,
                    commandTimeout: 360
                );
            }
        }
        private static bool TryGetClienteIds(dynamic r, out long clienteId, out long clienteIdFe)
        {
            clienteId = 0;
            clienteIdFe = 0;

            if (r.CLIENTE_ID != null) clienteId = Convert.ToInt64(r.CLIENTE_ID);
            if (r.CLIENTE_ID_FE != null) clienteIdFe = Convert.ToInt64(r.CLIENTE_ID_FE);

            return (clienteId > 0 && clienteIdFe > 0);
        }

        private sealed class FacturaProcDto
        {
            public string comprobanteInterno = "";
            public string clave50 = "";
            public string clave20 = "";

            public decimal intCor;
            public decimal intMor;
            public decimal cargos;
            public decimal poliza;
            public decimal principal;

            public decimal totalGravado;
            public decimal totalExento;

            public string moneda = "CRC";
            public decimal tipoCambio = 1m;

            public string emailDestino = "";
            public string enviarCliente = "0";

            public string clienteIdDestino = "";
            public string tipoId = "";

            public long idFactura;
        }
        private readonly struct FacturaDetDto
        {
            public readonly string codigo;
            public readonly string unidad;
            public readonly string detalle;
            public readonly decimal monto;
            public readonly string clave50;

            public FacturaDetDto(string codigo, string unidad, string detalle, decimal monto, string clave50)
            {
                this.codigo = (codigo ?? "").Trim();
                this.unidad = (unidad ?? "").Trim();
                this.detalle = (detalle ?? "").Trim();
                this.monto = monto;
                this.clave50 = (clave50 ?? "").Trim();
            }
        }
        private readonly struct FacturaMapCtx
        {
            public readonly DateTime fechaFactura;
            public readonly CorteCfg cfg;
            public readonly string cedulaEmisor;
            public readonly string situacion;
            public readonly string tipoComprobante;

            public FacturaMapCtx(
                DateTime fechaFactura,
                CorteCfg cfg,
                string cedulaEmisor,
                string situacion,
                string tipoComprobante)
            {
                this.fechaFactura = fechaFactura;
                this.cfg = cfg;
                this.cedulaEmisor = (cedulaEmisor ?? "").Trim();
                this.situacion = situacion;
                this.tipoComprobante = tipoComprobante;
            }
        }
        private sealed class NotificaFacturaItem
        {
            public string CodCliente { get; }
            public string ComprobanteInterno { get; }
            public long IdFactura { get; }
            public string Usuario { get; }

            public NotificaFacturaItem(string codCliente, string comprobanteInterno, long idFactura, string usuario)
            {
                CodCliente = (codCliente ?? "").Trim();
                ComprobanteInterno = (comprobanteInterno ?? "").Trim();
                IdFactura = idFactura;
                Usuario = (usuario ?? "").Trim();
            }
        }


    }
}
