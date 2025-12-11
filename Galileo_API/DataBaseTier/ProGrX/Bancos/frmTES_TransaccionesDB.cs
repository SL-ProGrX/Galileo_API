using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesTransaccionesDb
    {
        private readonly IConfiguration? _config;
        private readonly MTesoreria mTesoreria;
        private readonly MProGrXAuxiliarDB _AuxiliarDB;
        private readonly int vModulo = 9;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly FrmCntXConsultaCuentasDb _ConsultaCuentasDB;
        private readonly VerificadorCoreFactory _factory;

        private readonly string descripcion = "descripcion";

        private readonly Dictionary<string, string> whitelist = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["item"] = "item",
            ["descripcion"] = "descripcion",
            ["correo"] = "correo"
        };
 
        public FrmTesTransaccionesDb(IConfiguration config)
        {
            _config = config;
            mTesoreria = new MTesoreria(config);
            _AuxiliarDB = new MProGrXAuxiliarDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
            _ConsultaCuentasDB = new FrmCntXConsultaCuentasDb(config);
            _factory = new VerificadorCoreFactory(config);
        }

        #region Helpers privados para reducir duplicidad

        private SqlConnection OpenConnection(int codEmpresa)
        {
            var cs = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(cs);
        }

        private ErrorDto<T> WithConn<T>(int codEmpresa, Func<SqlConnection, T> action)
        {
            try
            {
                using var conn = OpenConnection(codEmpresa);
                var result = action(conn);
                return new ErrorDto<T> { Code = 0, Description = "Ok", Result = result };
            }
            catch (Exception ex)
            {
                return new ErrorDto<T> { Code = -1, Description = ex.Message, Result = default };
            }
        }

      

        private static ErrorDto<T> Ok<T>(T result, string desc = "Ok") =>
            new ErrorDto<T> { Code = 0, Description = desc, Result = result };

        private static ErrorDto<T> Error<T>(string msg, int code = -1) =>
            new ErrorDto<T> { Code = code, Description = msg, Result = default };

        private static ErrorDto OkSimple(string desc = "Ok") =>
            new ErrorDto { Code = 0, Description = desc };

        private static ErrorDto ErrorSimple(string msg, int code = -1) =>
            new ErrorDto { Code = code, Description = msg };

        #endregion

        #region Cargas dropdown / catálogos

        public ErrorDto<List<DropDownListaGenericaModel>> TES_TiposDocumentos_Obtener(int CodEmpresa, string Usuario, int id_banco, string? tipo = "S")
            => mTesoreria.sbTesTiposDocsCargaCboAcceso(CodEmpresa, Usuario, id_banco, tipo);

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Unidades_Obtener(int CodEmpresa, string usuario, int banco, int contabilidad)
            => mTesoreria.sbTesUnidadesCargaCbo(CodEmpresa, usuario, banco, contabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
            => _AuxiliarDB.TiposIdentificacion_Obtener(CodEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Conceptos_Obtener(int CodEmpresa, string usuario, int banco)
            => mTesoreria.sbTesConceptosCargaCbo(CodEmpresa, usuario, banco);

        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosCarga_Obtener(int CodEmpresa, string usuario, string gestion)
            => mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, gestion);

        #endregion

        #region Consultas varias

        public ErrorDto<List<TesAfectacionDto>> TES_Afectaciones_Obtener(int CodEmpresa, int tesoreria)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"exec spTes_Consulta_Afectacion_Modulos @Solicitud";
                return conn.Query<TesAfectacionDto>(query, new { Solicitud = tesoreria }).ToList();
            });
        }

        public ErrorDto<List<TesBitacoraDto>> TES_Bitacora_Obtener(int CodEmpresa, int tesoreria)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    select H.ID, H.FECHA, H.USUARIO,ISNULL(M.DESCRIPCION,'No identificado') AS MOVIMIENTO,H.DETALLE
                    from TES_HISTORIAL H left join TES_TIPOS_MOVIMIENTOS M on H.COD_MOVIMIENTO = M.COD_MOVIMIENTO
                    WHERE H.NSOLICITUD = @Solicitud";
                return conn.Query<TesBitacoraDto>(query, new { Solicitud = tesoreria }).ToList();
            });
        }

        public ErrorDto<int> TES_Transaccion_Scroll(int CodEmpresa, int scrollCode, string codigo, int contabilidad)
        {
            try
            {
                using var conn = OpenConnection(CodEmpresa);

                var query = @"select Top 1 nsolicitud from Tes_Transacciones C
                              inner join CntX_Unidades U on C.cod_unidad = U.cod_unidad";

                switch (scrollCode)
                {
                    case 0:
                        if (codigo == "") codigo = "0";
                        query += $@" where C.nsolicitud > @codigo} AND U.cod_contabilidad = @contabilidad order by C.nsolicitud asc";
                        break;
                    case 1:
                        if (codigo == "0") codigo = "999999999";
                        query += $@" where C.nsolicitud <@codigo AND U.cod_contabilidad = @contabilidad order by C.nsolicitud desc";
                        break;
                }

                var result = conn.Query<int>(query, new { codigo, contabilidad }).FirstOrDefault();
                if (result == 0)
                    return TES_Transaccion_Scroll(CodEmpresa, scrollCode, result.ToString(), contabilidad);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }
        }

        public ErrorDto<TesTransaccionDto> TES_Transaccion_Obtener(int CodEmpresa, int tesoreria, int contabilidad)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"exec spTes_Transaccion_Consulta @Solicitud";
                var trx = conn.Query<TesTransaccionDto>(query, new { Solicitud = tesoreria }).FirstOrDefault();

                if (trx != null)
                {
                    trx.detalle = string.Join(" ",
                        trx.detalle1 ?? "",
                        trx.detalle2 ?? "",
                        trx.detalle3 ?? "",
                        trx.detalle4 ?? "",
                        trx.detalle5 ?? ""
                    ).Replace("null", "").Trim();
                }

                return trx;
            });
        }

        #endregion

        #region Asientos

        public ErrorDto<List<TesTransAsientoDto>> TES_TransaccionAsiento_Obtener(TesConsultaAsientos vSolicitud)
        {
            try
            {
                using var connection = OpenConnection(vSolicitud.CodEmpresa);

                var asientos = vSolicitud.solicitud > 0
                    ? ObtenerAsientosPorSolicitud(connection, vSolicitud)
                    : ObtenerAsientosPorDefecto(connection, vSolicitud);

                AjustarLineaBancoSiAplica(connection, vSolicitud, asientos);

                if (vSolicitud.solicitud <= 0)
                    AjustarMontosYTipoCambio(vSolicitud, asientos);

                AjustarDebeHaber(vSolicitud, asientos);

                return Ok(asientos);
            }
            catch (Exception ex)
            {
                return Error<List<TesTransAsientoDto>>(ex.Message);
            }
        }

        private List<TesTransAsientoDto> ObtenerAsientosPorSolicitud(SqlConnection connection, TesConsultaAsientos vSolicitud)
        {
            const string query = @"
                select C.cod_cuenta_Mask as Cod_Cuenta, C.descripcion, D.debehaber, D.monto, D.cod_unidad, Ch.Estado,
                       U.descripcion as UnidadX, D.cod_cc, X.descripcion as CCX, Ch.id_Banco, D.tipo_cambio, D.cod_divisa
                from Tes_Trans_Asiento D
                     inner join Tes_Transacciones Ch on D.nsolicitud = Ch.Nsolicitud
                     inner join CntX_Cuentas C on D.cuenta_contable = C.cod_cuenta and C.cod_contabilidad = @contabilidad
                     left join CntX_unidades U on D.cod_unidad = U.cod_unidad and U.cod_contabilidad = @contabilidad
                     left join CNTX_CENTRO_COSTOS X on D.cod_cc = X.COD_CENTRO_COSTO and X.cod_contabilidad = @contabilidad
                where D.nsolicitud = @solicitud
                order by D.linea;";

            return connection.Query<TesTransAsientoDto>(
                query, new { contabilidad = vSolicitud.contabilidad, solicitud = vSolicitud.solicitud }
            ).ToList();
        }

        private List<TesTransAsientoDto> ObtenerAsientosPorDefecto(SqlConnection connection, TesConsultaAsientos vSolicitud)
        {
            const string query = @"
                select TOP 1
                    C.cod_cuenta_Mask as Cod_Cuenta,
                    C.descripcion,
                    'D' as debehaber,
                    @monto as monto,
                    @cod_unidad as cod_uniadd,
                    @estado as estado,
                    (select descripcion from CntX_Unidades U where U.cod_unidad = @cod_unidad and U.cod_contabilidad = C.cod_contabilidad) as UnidadX,
                    '' as cod_cc,
                    '' as CCX,
                    B.id_banco,
                    0 as tipo_cambio,
                    C.cod_divisa
                from CntX_Cuentas C
                inner join Tes_Bancos B on C.cod_Cuenta = B.CtaConta
                where B.id_banco = @id_banco
                  and C.cod_contabilidad = @contabilidad

                UNION

                select TOP 1
                    C.cod_cuenta_Mask as Cod_Cuenta,
                    C.descripcion,
                    'H' as debehaber,
                    @monto as monto,
                    @cod_unidad as cod_uniadd,
                    @estado as estado,
                    (select descripcion from CntX_Unidades U where U.cod_unidad = @cod_unidad and U.cod_contabilidad = C.cod_contabilidad) as UnidadX,
                    '' as cod_cc,
                    '' as CCX,
                    @id_banco as id_banco,
                    0 as tipo_cambio,
                    C.cod_divisa
                from CntX_Cuentas C
                inner join Tes_Conceptos B on C.cod_Cuenta = B.cod_cuenta
                where B.cod_concepto = @cod_concepto
                  and C.cod_contabilidad = @contabilidad;";

            return connection.Query<TesTransAsientoDto>(query, new
            {
                contabilidad = vSolicitud.contabilidad,
                id_banco = vSolicitud.id_banco,
                cod_concepto = vSolicitud.cod_concepto,
                monto = vSolicitud.monto,
                cod_unidad = vSolicitud.cod_unidad,
                estado = vSolicitud.estado
            }).ToList();
        }

        private void AjustarLineaBancoSiAplica(SqlConnection connection, TesConsultaAsientos vSolicitud, List<TesTransAsientoDto> asientos)
        {
            if (asientos == null || asientos.Count == 0) return;

            var first = asientos[0];
            if (vSolicitud.id_banco == first.id_banco) return;

            const string query = @"
                select C.cod_cuenta_Mask as Cod_Cuenta, C.descripcion, C.cod_divisa
                from CntX_Cuentas C
                inner join Tes_Bancos B on C.cod_Cuenta = B.CtaConta
                where B.id_banco = @banco
                  and C.cod_contabilidad = @contabilidad;";

            var linea = connection.QueryFirstOrDefault<TesTransAsientoDto>(
                query, new { banco = vSolicitud.id_banco, contabilidad = vSolicitud.contabilidad });

            if (linea == null) return;

            first.cod_cuenta = linea.cod_cuenta;
            first.cod_divisa = linea.cod_divisa;
            first.descripcion = linea.descripcion;
        }

        private static void AjustarMontosYTipoCambio(TesConsultaAsientos vSolicitud, List<TesTransAsientoDto> asientos)
        {
            if (asientos == null) return;

            decimal tipoCambio = Convert.ToDecimal(vSolicitud.tipoCambio);

            foreach (var item in asientos)
            {
                item.cod_unidad = vSolicitud.cod_unidad;
                item.tipo_cambio = item.cod_divisa == "DOL" ? tipoCambio : 1m;
                item.monto = item.monto * tipoCambio;
            }
        }

        private void AjustarDebeHaber(TesConsultaAsientos vSolicitud, List<TesTransAsientoDto> asientos)
        {
            if (asientos == null || asientos.Count == 0) return;

            bool esAsientoA = mTesoreria.fxTesTiposDocAsiento(vSolicitud.CodEmpresa, vSolicitud.tipo) == "A";

            asientos[0].debehaber = esAsientoA ? "H" : "D";
            for (int i = 1; i < asientos.Count; i++)
                asientos[i].debehaber = esAsientoA ? "D" : "H";
        }

        #endregion

        #region Bitácoras extra

        public ErrorDto<List<TesLocalizacionDto>> TES_Localizacion_Obtener(int CodEmpresa, int solicitud)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    select D.fecha_rec,D.cod_remesa,U.descripcion,D.usuario_rec,D.observacion
                    from Tes_Ubi_RemDet D
                    inner join Tes_ubi_Remesa R on D.cod_Remesa = R.cod_remesa
                    inner join tes_Ubicaciones U on R.cod_ubicacion_destino = U.cod_ubicacion
                    Where D.nsolicitud = @solicitud And D.estado = 1
                    Order by D.fecha_rec desc";

                return conn.Query<TesLocalizacionDto>(query, new {Solicitud = solicitud}).ToList();
            });
        }

        public ErrorDto<List<TesReimpresionesDto>> TES_ReImpresiones_Obtener(int CodEmpresa, int solicitud)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    select Fecha,Usuario,Autoriza,Notas
                    from Tes_reImpresiones
                    where nsolicitud = @solicitud
                    order by fecha desc";

                return conn.Query<TesReimpresionesDto>(query, new {solicitud}).ToList();
            });
        }

        public ErrorDto<List<TesCambioFechasDto>> TES_CambioFechas_Obtener(int CodEmpresa, int solicitud)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    select Id as Idx,Fecha,Usuario,Detalle
                    from tes_historial
                    where nsolicitud = @solicitud
                      and cod_movimiento = '08'
                    order by fecha desc";

                return conn.Query<TesCambioFechasDto>(query, new {solicitud}).ToList();
            });
        }

        #endregion

        #region Solicitudes (lazy load)

        public ErrorDto<TablasListaGenericaModel> TES_Solicitudes_Obtener(int CodEmpresa, int contabilidad, FiltrosLazyLoadData filtro)
        {
            string search = filtro.filtro?.Trim();
            string sortField = string.IsNullOrWhiteSpace(filtro.sortField) ? "NSOLICITUD" : filtro.sortField;
            int sortOrder = (filtro.sortOrder == 0) ? 1 : filtro.sortOrder;
            int pagina = filtro.pagina;
            int paginacion = filtro.paginacion;

            return WithConn(CodEmpresa, conn =>
            {
                const string qTotal = @"
        select count(C.NSOLICITUD)
        from Tes_Transacciones C
        inner join CntX_Unidades U on C.cod_unidad = U.cod_unidad
        where U.cod_contabilidad = @Contabilidad;";

                var total = conn.Query<int>(qTotal, new { Contabilidad = contabilidad }).FirstOrDefault();

                // WHITELIST de columnas permitidas para ORDER BY
                var sortMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["NSOLICITUD"] = "NSOLICITUD",
                    ["TIPO"] = "TIPO",
                    ["CODIGO"] = "CODIGO",
                    ["BENEFICIARIO"] = "BENEFICIARIO",
                    ["MONTO"] = "MONTO",
                    ["ESTADO"] = "ESTADO",
                    ["COD_UNIDAD"] = "COD_UNIDAD"
                };

                // Si el campo no está en el mapa, usamos una por defecto (segura)
                if (!sortMap.TryGetValue(sortField, out string safeSortField))
                    safeSortField = "NSOLICITUD";

                // La dirección sólo la derivamos de un entero, no del usuario directamente
                string safeSortDir = sortOrder == -1 ? "DESC" : "ASC";

                var sql = @"
        select NSOLICITUD, TIPO, CODIGO, BENEFICIARIO, MONTO, ESTADO, COD_UNIDAD
        from (
            select
                C.NSOLICITUD,
                rtrim(T.descripcion) as TIPO,
                C.CODIGO,
                C.BENEFICIARIO,
                C.monto as MONTO,
                C.estado as ESTADO,
                C.COD_UNIDAD
            from Tes_Transacciones C
            inner join CntX_Unidades U on C.cod_unidad = U.cod_unidad
            inner join Tes_Tipos_doc T on C.tipo = T.tipo
            where U.cod_contabilidad = @Contabilidad
        ) X
    ";

                var parameters = new DynamicParameters();
                parameters.Add("@Contabilidad", contabilidad);
                parameters.Add("@Offset", pagina);
                parameters.Add("@PageSize", paginacion);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    sql += @"
            where (
                NSOLICITUD like @Search
                or TIPO like @Search
                or BENEFICIARIO like @Search
                or CODIGO like @Search
            )
        ";
                    parameters.Add("@Search", $"%{search}%");
                }

                // Sólo usamos valores previamente validados (whitelist)
                sql += $@"
        order by {safeSortField} {safeSortDir}
        offset @Offset rows fetch next @PageSize rows only;
    ";

                var lista = conn
                    .Query<Galileo.Models.ProGrX.Bancos.TesSolicitudesData>(sql, parameters)
                    .ToList();

                return new TablasListaGenericaModel { total = total, lista = lista };
            });
        }

        #endregion

        #region Scroll por documento

        public ErrorDto<int> TES_TransaccionDocumento_Scroll(int CodEmpresa, int scrollCode, TesSolicitudDocParametro parametros)
        {
            try
            {
                using var conn = OpenConnection(CodEmpresa);

                if (string.IsNullOrWhiteSpace(parametros.documento))
                    parametros.documento = "0";

                // Convertimos documento a número real para evitar CASTs inseguros
                if (!int.TryParse(parametros.documento, out int docValue))
                    docValue = 0;

                string query = @"
            select TOP 1 ndocumento
            from Tes_Transacciones
            where id_banco = @idBanco
              and Tipo = @Tipo
        ";

                string orderBy = "";
                string compareOperator = "";

                switch (scrollCode)
                {
                    case 0: // siguiente
                        compareOperator = ">";
                        orderBy = "asc";
                        break;

                    case 1: // anterior
                        compareOperator = "<";
                        orderBy = "desc";
                        break;

                    default:
                        compareOperator = ">";
                        orderBy = "asc";
                        break;
                }

                // Agregamos el filtro con parámetros, SIN concatenar strings de usuario
                query += $@"
            and TRY_CAST(ndocumento AS INT) {compareOperator} @NumDoc
            order by TRY_CAST(ndocumento AS INT) {orderBy};
        ";

                var documento = conn.Query<string>(
                    query,
                    new
                    {
                        idBanco = parametros.id_banco,
                        Tipo = parametros.tipo,
                        NumDoc = docValue
                    }
                ).FirstOrDefault();

                return TES_TransaccionDoc_Obtener(
                    CodEmpresa,
                    documento,
                    parametros.id_banco,
                    parametros.tipo,
                    parametros.contabilidad
                );
            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }
        }

        public ErrorDto<int> TES_TransaccionDoc_Obtener(int CodEmpresa, string documento, int banco, string tipo, int contabilidad)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    select Nsolicitud from Tes_Transacciones
                    where ndocumento = @Documento
                      and id_banco = @idBanco
                      and Tipo = @Tipo";

                return conn.Query<int>(query, new { Documento = documento, idBanco = banco, Tipo = tipo }).FirstOrDefault();
            });
        }

        #endregion

        #region Cambio cuenta bancaria

        public ErrorDto TES_CambioCuentaBancaria_Aplicar(int CodEmpresa, string usuario, int solicitud, string cuenta)
        {
            try
            {
                using var conn = OpenConnection(CodEmpresa);

                var query = @"exec spTes_Cuenta_Bancaria_Cambio @Solicitud, @Cuenta, @Usuario";
                var rows = conn.Execute(query, new { Solicitud = solicitud, Cuenta = cuenta, Usuario = usuario });

                return rows == 0
                    ? ErrorSimple("No se pudo realizar el cambio de cuenta bancaria", 0)
                    : OkSimple("Cambio de Cuenta Bancaria realizado satisfactoriamente!");
            }
            catch (Exception ex)
            {
                return ErrorSimple(ex.Message);
            }
        }

        #endregion

        #region Guardar transacción (insert/update)

        public ErrorDto TES_Transaccion_Guardar(int CodEmpresa, string usuario, int contabilidad, TesTransaccionDto transaccion)
        {
            try
            {
                NormalizarUsuarioSolicita(usuario, transaccion);
                AsegurarAsientoDetalle(CodEmpresa, contabilidad, transaccion);
                PrepararDetalleEnPartes(transaccion);

                var valida = fxValida(CodEmpresa, usuario, transaccion);
                if (valida.Code == -1)
                    return ErrorSimple(valida.Description, -1);

                AjustarTipoCedOrigen(transaccion);
                ProcesarRegAutorizacion(CodEmpresa, usuario, transaccion);

                var res = GuardarTransaccion(CodEmpresa, usuario, transaccion);

                if (EsGuardadoExitoso(res))
                    res = EmitirSiAplica(CodEmpresa, usuario, transaccion, res);

                return res;
            }
            catch (Exception ex)
            {
                return ErrorSimple(ex.Message, -1);
            }
        }

        private static void NormalizarUsuarioSolicita(string usuario, TesTransaccionDto t)
        {
            if (string.IsNullOrEmpty(t.user_solicita))
                t.user_solicita = usuario;
        }

        private void AsegurarAsientoDetalle(int CodEmpresa, int contabilidad, TesTransaccionDto t)
        {
            if (t.asientoDetalle != null) return;

            var solicitud = ConstruirSolicitudAsientos(CodEmpresa, contabilidad, t);
            t.asientoDetalle = TES_TransaccionAsiento_Obtener(solicitud).Result;
        }

        private static TesConsultaAsientos ConstruirSolicitudAsientos(int CodEmpresa, int contabilidad, TesTransaccionDto t) =>
            new TesConsultaAsientos
            {
                CodEmpresa = CodEmpresa,
                solicitud = t.nsolicitud,
                contabilidad = contabilidad,
                tipoCambio = float.Parse(t.tipo_cambio.ToString()),
                divisa = t.cod_divisa,
                estado = t.estado,
                monto = t.monto.Value,
                id_banco = (int)t.id_banco,
                cod_unidad = t.cod_unidad,
                cod_concepto = t.cod_concepto,
                tipo = t.tipo
            };

        private static void PrepararDetalleEnPartes(TesTransaccionDto t)
        {
            string[] partes = DividirEnCincoPartes(t.detalle);

            t.detalle1 = partes[0];
            t.detalle2 = partes[1];
            t.detalle3 = partes[2];
            t.detalle4 = partes[3];
            t.detalle5 = partes[4];

            t.detalle = null;
        }

        private static void AjustarTipoCedOrigen(TesTransaccionDto t)
        {
            var idOrigen = t.tipo_beneficiario - 1;
            t.tipo_ced_origen = idOrigen.HasValue ? idOrigen.Value : default;
        }

        private void ProcesarRegAutorizacion(int CodEmpresa, string usuario, TesTransaccionDto t)
        {
            var banco = mTesoreria.fxTesBancoDocsValor(CodEmpresa, (int)t.id_banco, t.tipo, "REG_AUTORIZACION");
            if (banco.Code == -1) return;

            t.entregado = "N";

            if (banco.Result == "1")
            {
                t.autoriza = "N";
                t.fecha_autorizacion = null;
                t.user_autoriza = null;
            }
            else
            {
                t.autoriza = "S";
                t.fecha_autorizacion = DateTime.Now;
                t.user_autoriza = usuario;
            }
        }

        private ErrorDto GuardarTransaccion(int CodEmpresa, string usuario, TesTransaccionDto t)
            => t.nsolicitud == 0
                ? TES_Transaccion_Insertar(CodEmpresa, usuario, t)
                : TES_Transaccion_Actualizar(CodEmpresa, usuario, t);

        private static bool EsGuardadoExitoso(ErrorDto res)
            => res.Code == 0 || res.Code == 1;

        private ErrorDto EmitirSiAplica(int CodEmpresa, string usuario, TesTransaccionDto t, ErrorDto res)
        {
            var emitir = mTesoreria.fxTesBancoDocsValor(CodEmpresa, (int)t.id_banco, t.tipo, "REG_EMISION").Result;
            if (emitir != "0") return res;

            if (t.nsolicitud == 0)
                t.nsolicitud = Convert.ToInt32(res.Description);

            t.ndocumento = "";

            var emision = mTesoreria.sbTesEmitirDocumento(CodEmpresa, usuario, vModulo, t.nsolicitud, t.ndocumento, null);

            if (emision.Code == -1)
                return ErrorSimple($"{t.nsolicitud}|{emision.Description}", -3);

            return OkSimple(t.nsolicitud.ToString());
        }

        private ErrorDto TES_Transaccion_Insertar(int CodEmpresa, string usuario, TesTransaccionDto transaccion)
        {
            try
            {
                string fechaAutoriza = _AuxiliarDB.validaFechaGlobal(transaccion.fecha_autorizacion);
                if (transaccion.user_autoriza == null) fechaAutoriza = null;

                using var connection = OpenConnection(CodEmpresa);

                var query = @"
                    INSERT INTO Tes_Transacciones (
                      ID_BANCO, TIPO, COD_CONCEPTO, COD_UNIDAD, CODIGO, BENEFICIARIO, MONTO, ESTADO, FECHA_SOLICITUD, USER_SOLICITA,
                      ESTADOI, MODULO, SUBMODULO, CTA_AHORROS, GENERA, ACTUALIZA, DETALLE1, DETALLE2, DETALLE3, DETALLE4, DETALLE5, REFERENCIA,
                      OP, ESTADO_ASIENTO, ENTREGADO, AUTORIZA, FECHA_AUTORIZACION, USER_AUTORIZA, NDOCUMENTO, TIPO_CAMBIO, COD_DIVISA, TIPO_BENEFICIARIO, COD_APP,
                      CORREO_NOTIFICA, TIPO_CED_ORIGEN, CTA_IBAN_ORIGEN, CEDULA_ORIGEN, tipo_ced_destino
                    )
                    OUTPUT  INSERTED.NSOLICITUD
                    VALUES (
                        @id_banco, @tipo, @cod_concepto, @cod_unidad, @codigo, @beneficiario, @monto,
                        'P', GETDATE(), @usuario, 'P', @vModulo, 'T', @cta_ahorros, 'S', 'N',
                        @detalle1, @detalle2, @detalle3, @detalle4, @detalle5, @referencia,
                        @op, 'N', @entregado, @autoriza, @fecha_autorizacion, @user_autoriza,
                        @ndocumento, @tipo_cambio, @cod_divisa, @tipo_beneficiario, 'ProGrx',
                        @correo_notifica, @tipo_ced_origen, @cta_iban_origen, @cedula_origen, @tipo_ced_destino
                    );";

                string vReferencia = (transaccion.referencia != null) ? transaccion.referencia.ToString() : "NULL";
                string vOp = (transaccion.op != null) ? transaccion.op.ToString() : "NULL";

                var solicitud = connection.QuerySingle<int>(query, new
                {
                    id_banco = transaccion.id_banco,
                    tipo = transaccion.tipo,
                    cod_concepto = transaccion.cod_concepto,
                    cod_unidad = transaccion.cod_unidad,
                    codigo = transaccion.codigo,
                    beneficiario = transaccion.beneficiario,
                    monto = transaccion.monto,
                    usuario = usuario,
                    vModulo = vModulo,
                    cta_ahorros = transaccion.cta_ahorros,
                    detalle1 = transaccion.detalle1,
                    detalle2 = transaccion.detalle2,
                    detalle3 = transaccion.detalle3,
                    detalle4 = transaccion.detalle4,
                    detalle5 = transaccion.detalle5,
                    referencia = vReferencia,
                    op = vOp,
                    entregado = transaccion.entregado,
                    autoriza = transaccion.autoriza,
                    fecha_autorizacion = fechaAutoriza,
                    user_autoriza = transaccion.user_autoriza,
                    ndocumento = transaccion.ndocumento,
                    tipo_cambio = transaccion.tipo_cambio,
                    cod_divisa = transaccion.cod_divisa,
                    tipo_beneficiario = transaccion.tipo_beneficiario,
                    correo_notifica = transaccion.correo_notifica,
                    tipo_ced_origen = transaccion.tipo_ced_origen,
                    cta_iban_origen = transaccion.cta_iban_origen,
                    cedula_origen = transaccion.cedula_origen,
                    tipo_ced_destino = transaccion.tipo_ced_destino
                });

                if (solicitud == 0)
                    return ErrorSimple("No se pudo realizar la transacción", 0);

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Solicitud : {solicitud}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                TES_TransaccionDetalleActualizar(CodEmpresa, solicitud, transaccion.asientoDetalle);

                return OkSimple(solicitud.ToString());
            }
            catch (Exception ex)
            {
                return ErrorSimple(ex.Message);
            }
        }

        private ErrorDto TES_Transaccion_Actualizar(int CodEmpresa, string usuario, TesTransaccionDto transaccion)
        {
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var query = @"
                    update Tes_Transacciones set 
                        id_banco = @id_banco,
                        tipo = @tipo,
                        cod_concepto = @cod_concepto,
                        cod_unidad = @cod_unidad,
                        codigo = @codigo,
                        Beneficiario = @Beneficiario,
                        monto = @monto,
                        cta_ahorros = @cta_ahorros,
                        detalle1 = @detalle1,
                        detalle2 = @detalle2,
                        detalle3 = @detalle3,
                        detalle4 = @detalle4,
                        detalle5 = @detalle5, 
                        tipo_beneficiario = @tipo_beneficiario,  
                        tipo_cambio = @tipo_cambio,
                        cod_divisa =  @cod_divisa,
                        referencia = @referencia,
                        Autoriza = @Autoriza, 
                        fecha_autorizacion = @fecha_autorizacion,
                        CORREO_NOTIFICA = @correo_notifica,
                        user_autoriza = @user_autoriza,
                        TIPO_CED_ORIGEN = @tipo_ced_origen, 
                        CTA_IBAN_ORIGEN= @cta_iban_origen,
                        CEDULA_ORIGEN= @cedula_origen, 
                        tipo_ced_destino = @tipo_ced_destino
                    where nsolicitud = @nsolicitud ";

                var rows = connection.Execute(query, new
                {
                    id_banco = transaccion.id_banco,
                    tipo = transaccion.tipo,
                    cod_concepto = transaccion.cod_concepto,
                    cod_unidad = transaccion.cod_unidad,
                    codigo = transaccion.codigo,
                    beneficiario = transaccion.beneficiario,
                    monto = transaccion.monto,
                    cta_ahorros = transaccion.cta_ahorros,
                    detalle1 = transaccion.detalle1,
                    detalle2 = transaccion.detalle2,
                    detalle3 = transaccion.detalle3,
                    detalle4 = transaccion.detalle4,
                    detalle5 = transaccion.detalle5,
                    tipo_beneficiario = transaccion.tipo_beneficiario,
                    tipo_cambio = transaccion.tipo_cambio,
                    cod_divisa = transaccion.cod_divisa,
                    referencia = transaccion.referencia,
                    autoriza = transaccion.autoriza,
                    fecha_autorizacion = transaccion.fecha_autorizacion,
                    correo_notifica = transaccion.correo_notifica,
                    user_autoriza = transaccion.user_autoriza,
                    nsolicitud = transaccion.nsolicitud,
                    tipo_ced_origen = transaccion.tipo_ced_origen,
                    cta_iban_origen = transaccion.cta_iban_origen,
                    cedula_origen = transaccion.cedula_origen,
                    tipo_ced_destino = transaccion.tipo_ced_destino
                });

                if (rows == 0)
                    return ErrorSimple("No se pudo realizar la transacción", 0);

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Solicitud : {transaccion.nsolicitud}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                TES_TransaccionDetalleActualizar(CodEmpresa, transaccion.nsolicitud, transaccion.asientoDetalle);

                return OkSimple(transaccion.nsolicitud.ToString());
            }
            catch (Exception ex)
            {
                return ErrorSimple(ex.Message);
            }
        }

        public ErrorDto TES_TransaccionDetalleActualizar(int CodEmpresa, int solicitud, List<TesTransAsientoDto> detalle)
        {
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var deleteSql = @"delete Tes_Trans_Asiento where nsolicitud = @solicitud";
                connection.Execute(deleteSql, new { solicitud });

                int linea = 0;
                const string insertSql = @"
                    insert Tes_Trans_Asiento(
                        nSolicitud, Linea, Cuenta_Contable, cod_unidad, cod_cc, cod_divisa, tipo_cambio, DebeHaber, Monto
                    ) values(
                        @nSolicitud, @linea, @Cuenta_Contable, @cod_unidad, @cod_cc, @cod_divisa, @tipo_cambio, @DebeHaber, @Monto
                    );";

                foreach (var item in detalle)
                {
                    linea++;
                    connection.Execute(insertSql, new
                    {
                        nSolicitud = solicitud,
                        linea,
                        Cuenta_Contable = item.cod_cuenta.Replace("-", ""),
                        cod_unidad = item.cod_unidad,
                        cod_cc = item.cod_cc,
                        cod_divisa = item.cod_divisa,
                        tipo_cambio = item.tipo_cambio,
                        DebeHaber = item.debehaber,
                        Monto = item.monto
                    });
                }

                return OkSimple();
            }
            catch (Exception ex)
            {
                return ErrorSimple(ex.Message);
            }
        }

        #endregion

        #region Emitir / eliminar

        public ErrorDto TES_TransaccionesDoc_Emite(int CodEmpresa, int id_banco, string tipo)
        {
            try
            {
                var banco = mTesoreria.fxTesBancoDocsValor(CodEmpresa, id_banco, tipo, "REG_EMISION");
                if (banco.Result == "0")
                {
                    // LLamo despues a reporte
                }

                return OkSimple();
            }
            catch (Exception ex)
            {
                return ErrorSimple(ex.Message);
            }
        }

        public ErrorDto TES_Transacciones_Eliminar(int CodEmpresa, int solicitud, string usuario)
        {
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var q1 = @"delete Tes_Trans_Asiento where nsolicitud = @solicitud";
                connection.Execute(q1, new { solicitud });

                var q2 = @"delete Tes_Transacciones where nsolicitud = @solicitud";
                connection.Execute(q2, new { solicitud });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Solicitud : {solicitud}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });

                return OkSimple();
            }
            catch (Exception ex)
            {
                return ErrorSimple(ex.Message);
            }
        }

        #endregion

        #region Beneficiarios por tipo (ya refactorizado por vos)

        public ErrorDto<TablasListaGenericaModel> TES_transaccionesBeneficiario_Obtener(int CodEmpresa, string tipo, string filtro)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro);
            try
            {
                NormalizarFiltros(filtros);

                using var connection = OpenConnection(CodEmpresa);

                var spec = ObtenerSpecPorTipo(tipo);
                if (spec == null)
                    return OkBeneficiarios(new TablasListaGenericaModel());

                int total = connection.Query<int>(spec.CountSql, spec.CountParams(filtros)).FirstOrDefault();

                var (sqlLista, parametrosLista) = spec.BuildListSql(filtros);
                var lista = connection.Query<object>(sqlLista, parametrosLista).ToList();

                return OkBeneficiarios(new TablasListaGenericaModel
                {
                    total = total,
                    lista = lista
                });
            }
            catch (Exception ex)
            {
                return ErrorBeneficiarios(ex.Message);
            }
        }

        private static void NormalizarFiltros(FiltrosLazyLoadData filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros.sortField))
                filtros.sortField = "item";

            if (filtros.sortOrder == 0)
                filtros.sortOrder = 1;

            if (filtros.pagina < 0) filtros.pagina = 0;
            if (filtros.paginacion <= 0) filtros.paginacion = 10;
        }

        private sealed record BeneficiarioSpec(
            string CountSql,
            Func<FiltrosLazyLoadData, object> CountParams,
            Func<FiltrosLazyLoadData, (string sql, object param)> BuildListSql,
            IReadOnlyDictionary<string, string> SortWhitelist
        );

        private BeneficiarioSpec? ObtenerSpecPorTipo(string tipo)
        {
            return tipo switch
            {
                "1" => PersonasSpec(),
                "2" => BancosSpec(),
                "3" => ProveedoresSpec(),
                "4" => AcreedoresSpec(),
                "5" => CxcSpec(),
                "6" => EmpleadosSpec(),
                "7" => DirectosSpec(),
                "8" => DesembolsosSpec(),
                _ => null
            };
        }

        private static string BuildWhereLike(FiltrosLazyLoadData f, params string[] columnas)
        {
            if (string.IsNullOrWhiteSpace(f.filtro)) return "";
            var ors = string.Join(" OR ", columnas.Select(c => $"{c} like @search"));
            return $" where ({ors})";
        }

        private static (string orderBy, string dir) BuildOrderBy(
            FiltrosLazyLoadData f,
            IReadOnlyDictionary<string, string> whitelist)
        {
            if (!whitelist.TryGetValue(f.sortField, out var safeField))
                safeField = whitelist["item"];

            var dir = (f.sortOrder == 0 ? "DESC" : "ASC");
            return (safeField, dir);
        }

        private static object BuildPagingParams(FiltrosLazyLoadData f) => new
        {
            offset = f.pagina,
            pageSize = f.paginacion,
            search = $"%{f.filtro?.Trim()}%"
        };

        private BeneficiarioSpec PersonasSpec()
        {
           
            return new BeneficiarioSpec(
                CountSql: "select count(cedula) from socios",
                CountParams: _ => new { },
                BuildListSql: f =>
                {
                    var where = BuildWhereLike(f, "Cedula", "nombre");
                    var (orderBy, dir) = BuildOrderBy(f, whitelist);

                    var sql = $@"
                        select item, descripcion, correo
                        from (
                            select cedula as item, nombre as descripcion, af_email as correo
                            from socios {where}
                        ) t
                        order by {orderBy} {dir}
                        offset @offset rows fetch next @pageSize rows only;";

                    return (sql, BuildPagingParams(f));
                },
                SortWhitelist: whitelist
            );
        }

        private BeneficiarioSpec BancosSpec()
        {

            return new BeneficiarioSpec(
                CountSql: "select count(id_banco) from tes_bancos where estado='A'",
                CountParams: _ => new { },
                BuildListSql: f =>
                {
                    var whereFiltro = string.IsNullOrWhiteSpace(f.filtro)
                        ? " where estado='A'"
                        : BuildWhereLike(f, "ID_BANCO", descripcion) + " AND estado='A'";

                    var (orderBy, dir) = BuildOrderBy(f, whitelist);

                    var sql = $@"
                        select item, descripcion
                        from (
                            select id_banco as item, descripcion
                            from tes_bancos {whereFiltro}
                        ) t
                        order by {orderBy} {dir}
                        offset @offset rows fetch next @pageSize rows only;";

                    return (sql, BuildPagingParams(f));
                },
                SortWhitelist: whitelist
            );
        }

        private BeneficiarioSpec ProveedoresSpec()
        {
           

            return new BeneficiarioSpec(
                CountSql: "select count(cod_proveedor) from cxp_proveedores",
                CountParams: _ => new { },
                BuildListSql: f =>
                {
                    var whereFiltro = string.IsNullOrWhiteSpace(f.filtro)
                        ? " where estado='A'"
                        : BuildWhereLike(f, "CEDJUR", descripcion) + " AND estado='A'";

                    var (orderBy, dir) = BuildOrderBy(f, whitelist);

                    var sql = $@"
                        select item, descripcion, correo
                        from (
                            select CEDJUR as item, descripcion, email as correo
                            from cxp_proveedores {whereFiltro}
                        ) t
                        order by {orderBy} {dir}
                        offset @offset rows fetch next @pageSize rows only;";

                    return (sql, BuildPagingParams(f));
                },
                SortWhitelist: whitelist
            );
        }

        private BeneficiarioSpec AcreedoresSpec()
        {
            return new BeneficiarioSpec(
                CountSql: "select count(cod_acreedor) from crd_apa_acreedores where estado='A'",
                CountParams: _ => new { },
                BuildListSql: f =>
                {
                    var whereFiltro = string.IsNullOrWhiteSpace(f.filtro)
                        ? " where estado='A'"
                        : BuildWhereLike(f, "cod_acreedor", descripcion) + " AND estado='A'";

                    var (orderBy, dir) = BuildOrderBy(f, whitelist);

                    var sql = $@"
                        select item, descripcion
                        from (
                            select cod_acreedor as item, descripcion
                            from crd_apa_acreedores {whereFiltro}
                        ) t
                        order by {orderBy} {dir}
                        offset @offset rows fetch next @pageSize rows only;";

                    return (sql, BuildPagingParams(f));
                },
                SortWhitelist: whitelist
            );
        }

        private BeneficiarioSpec CxcSpec()
        {
          
            return new BeneficiarioSpec(
                CountSql: "select count(cedula) from CXC_PERSONAS",
                CountParams: _ => new { },
                BuildListSql: f =>
                {
                    var where = BuildWhereLike(f, "cedula", "nombre");
                    var (orderBy, dir) = BuildOrderBy(f, whitelist);

                    var sql = $@"
                        select item, descripcion
                        from (
                            select cedula as item, nombre as descripcion
                            from CXC_PERSONAS {where}
                        ) t
                        order by {orderBy} {dir}
                        offset @offset rows fetch next @pageSize rows only;";

                    return (sql, BuildPagingParams(f));
                },
                SortWhitelist: whitelist
            );
        }

        private BeneficiarioSpec EmpleadosSpec()
        {
            return new BeneficiarioSpec(
                CountSql: "select count(Identificacion) from RH_PERSONAS",
                CountParams: _ => new { },
                BuildListSql: f =>
                {
                    var where = BuildWhereLike(f, "IDENTIFICACION", "Nombre_Completo");
                    var (orderBy, dir) = BuildOrderBy(f, whitelist);

                    var sql = $@"
                        select item, descripcion
                        from (
                            select Identificacion as item, Nombre_Completo as descripcion
                            from RH_PERSONAS {where}
                        ) t
                        order by {orderBy} {dir}
                        offset @offset rows fetch next @pageSize rows only;";

                    return (sql, BuildPagingParams(f));
                },
                SortWhitelist: whitelist
            );
        }

        private BeneficiarioSpec DirectosSpec()
        {

            return new BeneficiarioSpec(
                CountSql: "select count(Codigo) from vTes_Beneficiarios",
                CountParams: _ => new { },
                BuildListSql: f =>
                {
                    var where = BuildWhereLike(f, "CODIGO", "Nombre_Completo");
                    var (orderBy, dir) = BuildOrderBy(f, whitelist);

                    var sql = $@"
                        select item, descripcion
                        from (
                            select Codigo as item, Beneficiario as descripcion
                            from vTes_Beneficiarios {where}
                        ) t
                        order by {orderBy} {dir}
                        offset @offset rows fetch next @pageSize rows only;";

                    return (sql, BuildPagingParams(f));
                },
                SortWhitelist: whitelist
            );
        }

        private  BeneficiarioSpec DesembolsosSpec()
        {

            return new BeneficiarioSpec(
                CountSql: "select count(cedula) from vCxC_Cuentas_Desembolsos_Pendientes",
                CountParams: _ => new { },
                BuildListSql: f =>
                {
                    var where = BuildWhereLike(f, "cedula", "nombre");
                    var (orderBy, dir) = BuildOrderBy(f, whitelist);

                    var sql = $@"
                        select item, descripcion
                        from (
                            select cedula as item, nombre as descripcion
                            from vCxC_Cuentas_Desembolsos_Pendientes {where}
                        ) t
                        order by {orderBy} {dir}
                        offset @offset rows fetch next @pageSize rows only;";

                    return (sql, BuildPagingParams(f));
                },
                SortWhitelist: whitelist
            );
        }

        private static ErrorDto<TablasListaGenericaModel> OkBeneficiarios(TablasListaGenericaModel model) =>
            new ErrorDto<TablasListaGenericaModel> { Code = 0, Description = "Ok", Result = model };

        private static ErrorDto<TablasListaGenericaModel> ErrorBeneficiarios(string msg) =>
            new ErrorDto<TablasListaGenericaModel> { Code = -1, Description = msg, Result = null };

        #endregion

        #region Divisas / tipo cambio

        public ErrorDto<TesDivisaAsiento> TES_TransaccionesDivisa_Obtener(int CodEmpresa, string cuenta, int contabilidad)
        {
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var divisaSql = @"
                    select COD_DIVISA FROM CntX_Cuentas
                    WHERE COD_CUENTA_MASK = @cuenta AND COD_CONTABILIDAD = @contabilidad ";

                var codDivisa = connection.QueryFirstOrDefault<string>(divisaSql, new { cuenta, contabilidad });

                var tcSql = @"
                    SELECT Top 1 D.tc_compra
                    from CNTX_DIVISAS_TIPO_CAMBIO D
                    inner join CNTX_DIVISAS X on D.COD_DIVISA = X.COD_DIVISA
                    where D.COD_CONTABILIDAD = @cod_contabilidad
                      and D.cod_divisa = @cod_divisa
                    order by corte desc";

                var tc = connection.QueryFirstOrDefault<float>(tcSql, new
                {
                    cod_contabilidad = contabilidad,
                    cod_divisa = codDivisa
                });

                return Ok(new TesDivisaAsiento { cod_divisa = codDivisa, tipo_cambio = tc });
            }
            catch (Exception ex)
            {
                return Error<TesDivisaAsiento>(ex.Message);
            }
        }

        public ErrorDto<float> TES_TransaccionesTC_Obtener(int CodEmpresa, string cod_divisa, int contabilidad)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    SELECT Top 1 D.tc_compra
                    from CNTX_DIVISAS_TIPO_CAMBIO D
                    inner join CNTX_DIVISAS X on D.COD_DIVISA = X.COD_DIVISA
                    where D.COD_CONTABILIDAD = @cod_contabilidad
                      and D.cod_divisa = @cod_divisa
                    order by corte desc";

                return conn.QueryFirstOrDefault<float>(query, new
                {
                    cod_contabilidad = contabilidad,
                    cod_divisa
                });
            });
        }

        #endregion

        #region Validaciones SINPE / empresa

        public ErrorDto TES_TransferenciasSinpe_Valida(int CodEmpresa, int solicitud, string usuario)
        {
            try
            {
                return _factory.CrearServicio(CodEmpresa, usuario)
                               .fxValidacionSinpe(CodEmpresa, solicitud.ToString(), usuario);
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Error al validar el servicio." + ex
                };
            }
        }

        public ErrorDto<bool> TesEmpresaSinpe_Valida(int CodEmpresa)
            => mTesoreria.fxValidaEmpresaSinpe(CodEmpresa);

        #endregion

        #region Unidades / centro costos / control divisas

        public ErrorDto<List<DropDownListaGenericaModel>> TES_TransaccionesUnidades_Obtener(int CodEmpresa, int contabilidad)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    select cod_unidad as 'item', descripcion
                    from CntX_unidades
                    where Activa = 1 and cod_contabilidad = @contabilidad";

                return conn.Query<DropDownListaGenericaModel>(query, new { contabilidad }).ToList();
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_TransaccionesCC_Obtener(int CodEmpresa, string unidad)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    select distinct C.COD_CENTRO_COSTO as 'item', C.descripcion
                    from CNTX_CENTRO_COSTOS C
                    inner join CNTX_UNIDADES_CC A on C.COD_CENTRO_COSTO = A.COD_CENTRO_COSTO
                     and C.cod_contabilidad = A.cod_Contabilidad
                     and A.cod_unidad = @unidad";

                return conn.Query<DropDownListaGenericaModel>(query, new { unidad }).ToList();
            });
        }

        public ErrorDto<TesControlDivisas> TesControlDivisas_Obtener(int CodEmpresa, int id_banco, int contabilidad)
        {
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var query = @"
                    SELECT isnull(D.TC_COMPRA,1) as TC_COMPRA,
                           isnull(D.VARIACION,0) as VARIACION,
                           B.COD_DIVISA,
                           Di.DIVISA_LOCAL,
                           dbo.fxSys_Cadena_Capitaliza(di.DESCRIPCION) as DIVISA_DESC,
                           isnull(Di.CURRENCY_SIM,B.COD_DIVISA) as CURRENCY_SIM
                    FROM TES_BANCOS B
                    left JOIN CNTX_DIVISAS_TIPO_CAMBIO D
                           ON B.COD_DIVISA = D.COD_DIVISA
                          AND D.COD_CONTABILIDAD = @contabilidad
                          and dbo.MyGetdate() between inicio and corte 
                    inner join CNTX_DIVISAS Di on B.COD_DIVISA = Di.COD_DIVISA
                    where B.ID_BANCO = @banco";

                var ctrDivisa = connection.Query<TesControlDivisasData>(query, new
                {
                    banco = id_banco,
                    contabilidad
                }).FirstOrDefault();

                if (ctrDivisa == null)
                    return Error<TesControlDivisas>("No se encontró control de divisas");

                var resp = new TesControlDivisas
                {
                    gTipoCambio = ctrDivisa.tc_compra,
                    gVariacion = ctrDivisa.variacion,
                    gDivisaDesc = ctrDivisa.divisa_desc,
                    gDivisa = ctrDivisa.cod_divisa,
                    gDivisaCurrency = ctrDivisa.currency_sim,
                    pDivisaLocal = ctrDivisa.divisa_local
                };

                if (Convert.ToDecimal(resp.pDivisaLocal) == 0 &&
                    Convert.ToDecimal(resp.gTipoCambio) == 1m)
                {
                    var qTc = @"
                        SELECT Top 1 D.TC_COMPRA, D.VARIACION, X.Descripcion
                        from CNTX_DIVISAS_TIPO_CAMBIO D
                        inner join CNTX_DIVISAS X on D.COD_DIVISA = X.COD_DIVISA
                        where D.COD_CONTABILIDAD = @contabilidad
                          and D.cod_divisa = @cod_divisa
                        order by corte desc";

                    var tcDivisa = connection.Query<TesControlDivisasData>(qTc, new
                    {
                        cod_divisa = resp.gDivisa,
                        contabilidad
                    }).FirstOrDefault();

                    if (tcDivisa != null)
                    {
                        resp.gTipoCambio = tcDivisa.tc_compra;
                        resp.gVariacion = tcDivisa.variacion;
                    }
                }

                return Ok(resp);
            }
            catch (Exception ex)
            {
                return Error<TesControlDivisas>(ex.Message);
            }
        }

        #endregion

        #region Bitácora transacción

        public ErrorDto<List<TesBitacoraTransaccion>> Tes_BitacoraTransaccion(int CodEmpresa, string solicitud)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    SELECT fecha_solicitud AS fecha_hora, tt.NSOLICITUD as cod_bitacora, UPPER(tt.USER_SOLICITA) AS usuario, 'Solicitud' AS detalle
                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                    UNION ALL
                    SELECT fecha_emision AS fecha_hora, tt.NSOLICITUD, UPPER(tt.USER_GENERA), 'Emisión'
                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                    UNION ALL
                    SELECT tt.FECHA_ANULA AS fecha_hora, tt.NSOLICITUD, UPPER(tt.USER_ANULA), 'Anula'
                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                    UNION ALL
                    SELECT tt.FECHA_AUTORIZACION AS fecha_hora, tt.NSOLICITUD, UPPER(tt.USER_AUTORIZA), 'Autoriza'
                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                    UNION ALL
                    SELECT tt.FECHA_ASIENTO AS fecha_hora, tt.NSOLICITUD, UPPER(tt.USER_ASIENTO_EMISION), 'Asiento Emision'
                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                    UNION ALL
                    SELECT tt.FECHA_HOLD AS fecha_hora, tt.NSOLICITUD, UPPER(tt.USER_HOLD), 'Bloqueo'
                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                    UNION ALL
                    SELECT tt.FECHA_ENTREGA AS fecha_hora, tt.NSOLICITUD, UPPER(tt.USER_ENTREGA), 'Entrega'
                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                    UNION ALL
                    SELECT tt.FIRMAS_AUTORIZA_FECHA AS fecha_hora, tt.NSOLICITUD, UPPER(tt.FIRMAS_AUTORIZA_USUARIO), 'Firma Autoriza'
                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                    UNION ALL
                    SELECT tt.CONCILIA_FECHA AS fecha_hora, tt.NSOLICITUD, UPPER(tt.CONCILIA_USUARIO), 'Concilia'
                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                    UNION ALL
                    SELECT tt.REPOSICION_FECHA AS fecha_hora, tt.NSOLICITUD, UPPER(tt.REPOSICION_USUARIO), 'Reposición'
                    FROM TES_TRANSACCIONES tt where tt.NSOLICITUD = @solicitud
                    ORDER BY fecha_hora desc";

                return conn.Query<TesBitacoraTransaccion>(query, new { solicitud }).ToList();
            });
        }

        #endregion

        #region Auxiliares / validación

        public static ErrorDto<string> NumeroALetras(decimal numero)
            => MProGrXAuxiliarDB.NumeroALetras(numero);

        private ErrorDto<bool> fxValida(int CodEmpresa, string usuario, TesTransaccionDto transaccion)
        {
            try
            {
                var errores = new List<string>();
                var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                ValidarUsuarioDestino(CodEmpresa, usuario, transaccion, stringConn, errores);
                ValidarCamposBasicos(CodEmpresa, usuario, transaccion, errores);
                ValidarAutorizaciones(CodEmpresa, usuario, transaccion, errores);
                ValidarDocumentoSiAplica(CodEmpresa, transaccion, errores);
                ValidarAsiento(transaccion, errores);

                ValidarCuentasContables(transaccion, stringConn);

                return errores.Count > 0
                    ? ErrorBool(string.Join("", errores))
                    : OkBool();
            }
            catch
            {
                return ErrorBool("Error al validar la transacción");
            }
        }

        private void ValidarUsuarioDestino(int CodEmpresa, string usuario, TesTransaccionDto transaccion, string stringConn, List<string> errores)
        {
            if (mTesoreria.fxTesParametro(CodEmpresa, "12") != "S") return;

            using var conn = new SqlConnection(stringConn);

            const string qUser = @"
                SELECT CEDULA
                FROM USUARIOS
                WHERE UPPER(NOMBRE) LIKE @pattern";

            var pattern = $"%{usuario?.Trim().ToUpperInvariant()}%";
            var existe = conn.QueryFirstOrDefault<string>(qUser, new { pattern });

            if (!string.IsNullOrEmpty(existe) &&
                existe == transaccion.codigo?.ToString().Trim())
            {
                errores.Add("La identificación de destino no puede ser del usuario logeado.\n");
            }
        }

        private void ValidarCamposBasicos(int CodEmpresa, string usuario, TesTransaccionDto t, List<string> errores)
        {
            if (string.IsNullOrEmpty(usuario.Trim()))
                errores.Add("El usuario solicitante no es válido\n");

            if (t.monto == 0)
                errores.Add("El monto del documento no es válido\n");

            if (string.IsNullOrEmpty(t.codigo))
                errores.Add(" - Código del Beneficiario no es válido ...\n");

            if (string.IsNullOrEmpty(t.beneficiario))
                errores.Add(" - Beneficiario no es válido ...\n");

            if (t.tipo_ced_origen is null)
                errores.Add(" - Tipo Beneficiario no es válido ...\n");

            if (mTesoreria.fxTesCuentaObligatoriaVerifica(CodEmpresa, (int)t.id_banco).Result &&
                string.IsNullOrWhiteSpace(t.cta_ahorros))
            {
                errores.Add(" - La cuenta destino es requerida para este banco...\n");
            }

            t.detalle = (t.detalle1 ?? "") + (t.detalle2 ?? "") + (t.detalle3 ?? "") +
                        (t.detalle4 ?? "") + (t.detalle5 ?? "");

            if (t.detalle.Length == 0)
                errores.Add(" - El Detalle no es válido ...\n");

            if (t.estado != "P")
                errores.Add("- No se puede modificar este Documento porque se encuentra Emitido o Anulado ...\n");
        }

        private void ValidarAutorizaciones(int CodEmpresa, string usuario, TesTransaccionDto t, List<string> errores)
        {
            if (!mTesoreria.fxTesBancoValida(CodEmpresa, (int)t.id_banco, usuario).Result)
                errores.Add("- El Usuario Actual no esta Autorizado a utilizar este Banco...\n");

            if (!mTesoreria.fxTesTipoAccesoValida(CodEmpresa, t.id_banco.ToString(), usuario, t.tipo, "S").Result)
                errores.Add("- El Usuario Actual no esta Autorizado a utilizar este Tipo de Transacción...\n");

            if (!mTesoreria.fxTesConceptoValida(CodEmpresa, (int)t.id_banco, usuario, t.cod_concepto).Result)
                errores.Add(" - El Usuario Actual no esta Autorizado a utilizar este Concepto...\n");

            if (!mTesoreria.fxTesUnidadValida(CodEmpresa, (int)t.id_banco, usuario, t.cod_unidad).Result)
                errores.Add("- El Usuario Actual no esta Autorizado a utilizar esta unidad...\n");
        }

        private void ValidarDocumentoSiAplica(int CodEmpresa, TesTransaccionDto t, List<string> errores)
        {
            bool regEmision = mTesoreria.fxTesBancoDocsValor(CodEmpresa, (int)t.id_banco, t.tipo, "REG_EMISION").Result == "0";
            if (!regEmision) return;

            bool docAuto = mTesoreria.fxTesBancoDocsValor(CodEmpresa, (int)t.id_banco, t.tipo, "DOC_AUTO").Result == "0";
            if (!docAuto) return;

            var vDocumento = t.ndocumento ?? "";

            if (vDocumento.Length == 0)
            {
                errores.Add(" - Esta Solicitud se AutoEmite / Digite el #Documento para su Emisión...\n");
                return;
            }

            if (!mTesoreria.fxTesDocumentoVerifica(CodEmpresa, (int)t.id_banco, t.tipo, t.ndocumento).Result)
                errores.Add(" - Esta Solicitud se AutoEmite / El #Documento para su Emisión ya se encuentra registrado...\n");
        }

        private static void ValidarAsiento(TesTransaccionDto t, List<string> errores)
        {
            if (t.asientoDetalle == null || t.asientoDetalle.Count == 0)
            {
                errores.Add(" - El Asiento Contable no es válido ...\n");
                return;
            }

            decimal curMonto = 0;
            decimal totalDebe = 0;

            foreach (var item in t.asientoDetalle)
            {
                if (item.debehaber == "D")
                {
                    curMonto += item.monto;
                    totalDebe += item.monto;
                }
                else
                {
                    curMonto -= item.monto;
                }
            }

            if (curMonto != 0)
                errores.Add(" - El Asiento Contable debe estar Balanceado ...\n");

            decimal esperado = (decimal)(t.monto * t.tipo_cambio);

            if (t.asientoDetalle[0].monto != esperado)
                errores.Add(" -El Monto Linea 1 del Asiento no corresponde al original...\n");

            if (totalDebe != esperado)
                errores.Add(" -El Monto del Asiento no corresponde al original...\n");
        }

        private void ValidarCuentasContables(TesTransaccionDto t, string stringConn)
        {
            using var connection = new SqlConnection(stringConn);

            short inicializa = 0;
            const string gEnlace = "1";

            foreach (var item in t.asientoDetalle)
            {
                decimal pDebito = item.debehaber == "D" ? item.monto : 0;
                decimal pCredito = item.debehaber == "H" ? item.monto : 0;

                var parametros = new
                {
                    Contabilidad = gEnlace,
                    Usuario = t.user_solicita,
                    Modulo = "TES",
                    Cuenta = item.cod_cuenta.Replace("-", ""),
                    Divisa = item.cod_divisa,
                    Unidad = item.cod_unidad,
                    Centro = item.cod_cc,
                    TipoCambio = item.tipo_cambio,
                    Debito = pDebito,
                    Credito = pCredito,
                    Inicializa = inicializa
                };

                connection.QueryFirstOrDefault(
                    "spCntX_Cuentas_Valida_Load",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                inicializa++;
            }

            const string validaresultado = @"exec spCntX_Cuentas_Valida_Resultado @Usuario, 0";
            connection.QueryFirstOrDefault(validaresultado, new { Usuario = t.user_solicita });
        }

        private static ErrorDto<bool> OkBool() =>
            new ErrorDto<bool> { Code = 0, Result = true, Description = "" };

        private static ErrorDto<bool> ErrorBool(string msg) =>
            new ErrorDto<bool> { Code = -1, Result = false, Description = msg };

        #endregion

        #region Docs valor / cuentas bancarias / cuentas contables

        public ErrorDto<string> fxTesBancoDocsValor(int CodEmpresa, int banco, string tipo)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"select dbo.fxTes_DocumentoAutoEmite(@banco,@tipo) as AutoEmite";
                return conn.QueryFirstOrDefault<string>(query, new { banco, tipo });
            });
        }

        public ErrorDto<List<TesCuentasBancarias>> TES_TransaccionesCuentasBancarias_Obtener(int CodEmpresa, string identificacion, string banco, string? tipoOrigen = "1")
        {
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var infoSql = @"SELECT COD_GRUPO, CTA, COD_DIVISA, INT_GRUPOS_ASOCIADOS 
                        FROM TES_BANCOS 
                        WHERE ID_BANCO = @banco";

                var bancoInfo = connection.QueryFirstOrDefault<BancoValidaCuenta>(infoSql, new { banco });

                if (bancoInfo == null)
                    return Error<List<TesCuentasBancarias>>("Banco no encontrado.");

                string query;

                if (tipoOrigen == "1")
                {
                    if (!bancoInfo.int_grupos_asociados)
                    {
                        query = @"
                    SELECT 
                        C.CUENTA_INTERNA,
                        rtrim(C.cod_Banco) + ' - ' + C.CUENTA_INTERNA as cuenta_desc,
                        C.CUENTA_INTERNA as itmx,
                        @Idx as idx
                    FROM SYS_CUENTAS_BANCARIAS C
                    INNER JOIN TES_BANCOS_GRUPOS B ON C.cod_banco = B.cod_grupo
                    WHERE C.Identificacion = @Cedula
                      AND B.COD_GRUPO = @Grupo
                      AND C.COD_DIVISA = @Divisa
                      AND C.ACTIVA = 1
                ";
                    }
                    else
                    {
                        query = @"
                    SELECT 
                        C.CUENTA_INTERNA,
                        rtrim(C.cod_Banco) + ' - ' + C.CUENTA_INTERNA as cuenta_desc,
                        C.CUENTA_INTERNA as itmx,
                        @Idx as idx
                    FROM SYS_CUENTAS_BANCARIAS C
                    INNER JOIN TES_BANCOS_GRUPOS B ON C.cod_banco = B.cod_grupo
                    WHERE C.Identificacion = @Cedula
                      AND B.COD_GRUPO IN (
                            SELECT COD_GRUPO 
                            FROM TES_BANCOS_GRUPOS_ASG 
                            WHERE ID_BANCO = @BancoId
                      )
                      AND C.COD_DIVISA = @Divisa
                      AND C.ACTIVA = 1
                ";
                    }
                }
                else
                {
                    return Error<List<TesCuentasBancarias>>("TipoOrigen no soportado.");
                }

                var lista = connection.Query<TesCuentasBancarias>(
                    query,
                    new
                    {
                        Cedula = identificacion,
                        Grupo = bancoInfo.cod_grupo,
                        Divisa = bancoInfo.cod_divisa,
                        BancoId = banco,
                        Idx = bancoInfo.cta
                    }
                ).ToList();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return Error<List<TesCuentasBancarias>>(ex.Message);
            }
        }

        public ErrorDto<CtnxCuentasDto> ObtenerCuentas(int CodEmpresa, CuentaVarModel cuenta)
        {
            var result = new ErrorDto<CtnxCuentasDto>
            {
                Code = 1,
                Description = "Ok",
                Result = new CtnxCuentasDto()
            };

            var lista = _ConsultaCuentasDB.ObtenerCuentas(CodEmpresa, cuenta);
            result.Result = lista.Find(x => x.cod_cuenta == cuenta.Cuenta);

            return result;
        }

        public ErrorDto<TesCuentasBancarias> TES_TransaccionesCtaInterna_Obtener(int CodEmpresa, int id_banco)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    SELECT B.descripcion as cuenta_desc,
                           B.cta as cuenta_interna,
                           (SELECT se.CEDULA_JURIDICA FROM SIF_EMPRESA se where portal_id = @empresa) as itmx
                    FROM TES_BANCOS B
                    WHERE B.ID_BANCO = @banco";

                return conn.QueryFirstOrDefault<TesCuentasBancarias>(query, new
                {
                    banco = id_banco,
                    empresa = CodEmpresa
                });
            });
        }

        #endregion

        #region Utils

        public static string[] DividirEnCincoPartes(string texto)
        {
            var partes = new string[5];
            if (texto != null)
            {
                int longitudMax = 100;

                for (int i = 0; i < 5; i++)
                {
                    int inicio = i * longitudMax;

                    if (inicio >= texto.Length)
                        partes[i] = "";
                    else
                    {
                        int longitud = Math.Min(longitudMax, texto.Length - inicio);
                        partes[i] = texto.Substring(inicio, longitud);
                    }
                }
            }
            return partes;
        }

        #endregion
    }
}
