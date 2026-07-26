using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Net.Mail;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSeguimientoDesembolsosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCobroDb _mCobroDb;
        private readonly MCntLinkDB _mCntLinkDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const int ModuloCreditos = 1;
        private const string MovimientoRegistra = "REGISTRA-WEB";
        private const string MovimientoModifica = "MODIFICA-WEB";
        private const string MovimientoElimina = "ELIMINA-WEB";

        private const string MensajeOperacionRequerida = "La operación es requerida.";
        private const string MensajeUsuarioRequerido = "El usuario es requerido.";
        private const string MensajeDesembolsoRequerido = "El desembolso es requerido.";

        public FrmCrSeguimientoDesembolsosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mCobroDb = new MCobroDb(config);
            _mCntLinkDB = new MCntLinkDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la información inicial de la pantalla: resumen, bancos, tipos de identificación, divisas y desembolsos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoDesembolsosInicializarDto> CR_SeguimientoDesembolsos_Inicializar(
            int CodEmpresa,
            long operacion,
            string usuario)
        {
            if (operacion <= 0)
                return ErrorInicial(MensajeOperacionRequerida);

            if (string.IsNullOrWhiteSpace(usuario))
                return ErrorInicial(MensajeUsuarioRequerido);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var resumen = ObtenerResumenOperacion(CodEmpresa, conn, operacion);
                var result = new CrSeguimientoDesembolsosInicializarDto
                {
                    operacion = operacion,
                    monto_aprobado = resumen.monto_aprobado,
                    monto_registrado = resumen.monto_registrado,
                    monto_disponible = resumen.monto_disponible,
                    primer_cuota = resumen.primer_cuota,
                    poliza = resumen.poliza,
                    interes = resumen.interes,
                    bancos = ObtenerBancos(conn, usuario),
                    tipos_id = ObtenerTiposId(conn),
                    divisas = ObtenerDivisas(conn),
                    desembolsos = ObtenerDesembolsosBase(CodEmpresa, conn, operacion)
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrSeguimientoDesembolsosInicializarDto());
            }
        }

        /// <summary>
        /// Obtiene la lista de desembolsos de una operación usando FiltrosLazyLoadData para filtro/orden/paginación local.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CR_SeguimientoDesembolsos_Lista_Obtener(
            int CodEmpresa,
            string parametros)
        {
            var filtroResult = ParseFiltros(parametros);
            if (filtroResult.Error != null)
                return ErrorTabla(filtroResult.Error);

            if (filtroResult.Operacion <= 0)
                return ErrorTabla(MensajeOperacionRequerida);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var lista = ObtenerDesembolsosBase(CodEmpresa, conn, filtroResult.Operacion);
                lista = FiltrarDesembolsos(lista, filtroResult.Texto);
                lista = OrdenarDesembolsos(lista, filtroResult.Filtros);

                var total = lista.Count;
                lista = AplicarPaginacion(lista, filtroResult.Filtros);

                return DbHelper.CreateOkResponse(new TablasListaGenericaModel
                {
                    total = total,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return ErrorTabla(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista completa de desembolsos de una operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CR_SeguimientoDesembolsos_Lista_Export(
            int CodEmpresa,
            string parametros)
        {
            return CR_SeguimientoDesembolsos_Lista_Obtener(
                CodEmpresa,
                ResetPaginacion(parametros));
        }

        /// <summary>
        /// Obtiene el detalle completo de un desembolso usando spCrd_Desembolsos_Consulta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idDesembolso"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoDesembolsosData> CR_SeguimientoDesembolsos_Detalle_Obtener(
            int CodEmpresa,
            long idDesembolso)
        {
            if (idDesembolso <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeDesembolsoRequerido,
                    -2,
                    new CrSeguimientoDesembolsosData());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"exec spCrd_Desembolsos_Consulta null, @idDesembolso;";

                var data = conn.QueryFirstOrDefault<CrSeguimientoDesembolsosData>(
                    sql,
                    new { idDesembolso });

                if (data == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró el desembolso indicado.",
                        -2,
                        new CrSeguimientoDesembolsosData());
                }

                return DbHelper.CreateOkResponse(MapDesembolso(CodEmpresa, conn, data));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrSeguimientoDesembolsosData());
            }
        }

        /// <summary>
        /// Obtiene el catálogo de conceptos activos para búsqueda.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_SeguimientoDesembolsos_Conceptos_Obtener(
            int CodEmpresa,
            string? texto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                texto = (texto ?? string.Empty).Trim();
                var like = texto.Length > 0 ? $"%{texto}%" : null;

                const string sql = @"
                    select
                        cast(cod_condeb as varchar(20)) as item,
                        rtrim(descripcion) as descripcion
                    from concepto_desemb
                    where activo = 1
                      and (
                            @texto = ''
                         or cast(cod_condeb as varchar(20)) like @like
                         or descripcion like @like
                      )
                    order by cod_condeb;";

                var lista = conn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { texto, like })
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene las reglas del concepto seleccionado: retiene, modifica, cuenta y diferido.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codConcepto"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoDesembolsosConceptoDto> CR_SeguimientoDesembolsos_Concepto_Info_Obtener(
            int CodEmpresa,
            int codConcepto)
        {
            if (codConcepto <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El concepto es requerido.",
                    -2,
                    new CrSeguimientoDesembolsosConceptoDto());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var data = ObtenerConceptoInfo(CodEmpresa, conn, codConcepto);
                if (data == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró el concepto indicado.",
                        -2,
                        new CrSeguimientoDesembolsosConceptoDto());
                }

                return DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrSeguimientoDesembolsosConceptoDto());
            }
        }

        /// <summary>
        /// Obtiene bancos disponibles para desembolsos según usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CrSeguimientoDesembolsosBancoDto>> CR_SeguimientoDesembolsos_Bancos_Obtener(
            int CodEmpresa,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse(MensajeUsuarioRequerido, -2, new List<CrSeguimientoDesembolsosBancoDto>());

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                return DbHelper.CreateOkResponse(ObtenerBancos(conn, usuario));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<CrSeguimientoDesembolsosBancoDto>());
            }
        }

        /// <summary>
        /// Obtiene cuentas bancarias destino por identificación, banco y divisa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="bancoId"></param>
        /// <param name="divisaCheck"></param>
        /// <returns></returns>
        public ErrorDto<List<CrSeguimientoDesembolsosCuentaBancariaDto>> CR_SeguimientoDesembolsos_CuentasBancarias_Obtener(
            int CodEmpresa,
            string identificacion,
            int bancoId,
            int divisaCheck)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
                return DbHelper.CreateErrorResponse("La identificación es requerida.", -2, new List<CrSeguimientoDesembolsosCuentaBancariaDto>());

            if (bancoId <= 0)
                return DbHelper.CreateErrorResponse("El banco es requerido.", -2, new List<CrSeguimientoDesembolsosCuentaBancariaDto>());

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"exec spSys_Cuentas_Bancarias @Identificacion, @BancoId, @DivisaCheck;";

                var lista = conn.Query<CrSeguimientoDesembolsosCuentaBancariaDto>(
                    sql,
                    new
                    {
                        Identificacion = identificacion.Trim(),
                        BancoId = bancoId,
                        DivisaCheck = divisaCheck
                    })
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<CrSeguimientoDesembolsosCuentaBancariaDto>());
            }
        }

        /// <summary>
        /// Guarda un desembolso nuevo o actualiza uno existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoDesembolsosResumenDto> CR_SeguimientoDesembolsos_Guardar(
            int CodEmpresa,
            CrSeguimientoDesembolsosGuardarRequest request)
        {
            var validation = ValidarGuardar(CodEmpresa, request);
            if (validation.Code != 0)
                return validation;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            if (conn.State != ConnectionState.Open) conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                var data = NormalizarGuardar(CodEmpresa, request);
                var id = GuardarDesembolso(conn, tx, data);

                tx.Commit();

                RegistrarBitacoraGuardar(CodEmpresa, data, id);
                if (!data.id_solicitud.HasValue)
                {
                    return DbHelper.CreateErrorResponse(
                        "La operación es requerida.",
                        -2,
                        new CrSeguimientoDesembolsosResumenDto());
                }

                var resumen = ObtenerResumenOperacion(
                    CodEmpresa,
                    conn,
                    data.id_solicitud.Value);

                return DbHelper.CreateOkResponse(resumen);
            }
            catch (SqlException ex)
            {
                tx.Rollback();
                return DbHelper.CreateErrorResponse(ex.Message, -1, new CrSeguimientoDesembolsosResumenDto());
            }
        }

        /// <summary>
        /// Elimina un desembolso por id.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoDesembolsosResumenDto> CR_SeguimientoDesembolsos_Eliminar(
     int CodEmpresa,
     CrSeguimientoDesembolsosEliminarRequest request)
        {
            if (request == null || request.id_desembolso <= 0)
                return DbHelper.CreateErrorResponse(MensajeDesembolsoRequerido, -2, new CrSeguimientoDesembolsosResumenDto());

            if (!request.id_solicitud.HasValue || request.id_solicitud.Value <= 0)
                return DbHelper.CreateErrorResponse(MensajeOperacionRequerida, -2, new CrSeguimientoDesembolsosResumenDto());

            var idSolicitud = request.id_solicitud.Value;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"delete desembolsos where id_desembolso = @id_desembolso;";

                var rows = conn.Execute(sql, new { request.id_desembolso });
                if (rows <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró el desembolso para eliminar.",
                        -2,
                        new CrSeguimientoDesembolsosResumenDto());
                }

                RegistrarBitacora(
                    CodEmpresa,
                    request.usuario,
                    MovimientoElimina,
                    $"Seguimiento Desembolsos: elimina desembolso {request.id_desembolso} de operación {idSolicitud}");

                var resumen = ObtenerResumenOperacion(CodEmpresa, conn, idSolicitud);
                return DbHelper.CreateOkResponse(resumen);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new CrSeguimientoDesembolsosResumenDto());
            }
        }

        private static ErrorDto<CrSeguimientoDesembolsosInicializarDto> ErrorInicial(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new CrSeguimientoDesembolsosInicializarDto());
        }

        private static ErrorDto<TablasListaGenericaModel> ErrorTabla(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<CrSeguimientoDesembolsosData>()
                });
        }

        private List<CrSeguimientoDesembolsosData> ObtenerDesembolsosBase(
            int CodEmpresa,
            SqlConnection conn,
            long operacion)
        {
            const string sql = @"
                select
                    D.ID_DESEMBOLSO as id_desembolso,
                    D.ID_SOLICITUD as id_solicitud,
                    rtrim(isnull(D.CODIGO,'')) as codigo,
                    rtrim(isnull(D.CONCEPTO,'')) as concepto,
                    isnull(D.MONTO,0) as monto,
                    rtrim(isnull(D.CUENTA_CONTA,'')) as cuenta_conta,
                    rtrim(isnull(D.TDOCUMENTO,'')) as tdocumento,
                    isnull(D.DEPOSITAR,0) as depositar,
                    isnull(D.COD_BANCO,0) as banco,
                    rtrim(isnull(B.DESCRIPCION,'')) as banco_desc,
                    isnull(D.RETENER,0) as retener,
                    isnull(D.MODIFICA,0) as modifica,
                    isnull(D.DIFERIDO_APLICA,0) as diferido_aplica,
                    D.DIFERIDO_CORTE as diferido_corte,
                    rtrim(isnull(D.REFERENCIA,'')) as referencia,
                    rtrim(isnull(D.IDENTIFICACION,'')) as identificacion,
                    rtrim(isnull(D.CTA_BANCO,'')) as cta_banco,
                    isnull(D.TIPO_CED_DESTINO,0) as tipo_ced_destino,
                    rtrim(isnull(D.CEDULA_DESTINO,'')) as cedula_destino,
                    rtrim(isnull(D.ID_BANCO_DESTINO,'')) as id_banco_destino,
                    rtrim(isnull(D.CTA_IBAN_DESTINO,'')) as cta_iban_destino,
                    rtrim(isnull(D.COD_DIVISA,'')) as cod_divisa,
                    rtrim(isnull(D.CORREO_NOTIFICA,'')) as correo_notifica,
                    rtrim(isnull(D.DETALLE,'')) as detalle
                from Desembolsos D
                left join Tes_Bancos B on D.cod_Banco = B.id_Banco
                where D.id_solicitud = @operacion;";

            return conn.Query<CrSeguimientoDesembolsosData>(
                sql,
                new { operacion })
                .Select(x => MapDesembolso(CodEmpresa, conn, x))
                .ToList();
        }

        private CrSeguimientoDesembolsosData MapDesembolso(
     int CodEmpresa,
     SqlConnection conn,
     CrSeguimientoDesembolsosData item)
        {
            item.codigo = Clean(item.codigo);
            item.concepto = Clean(item.concepto);
            item.cuenta_conta = Clean(item.cuenta_conta);
            item.cuenta_conta_mask = string.IsNullOrWhiteSpace(item.cuenta_conta_mask)
                ? _mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, true, item.cuenta_conta, 0)
                : Clean(item.cuenta_conta_mask);
            item.cuenta_desc = string.IsNullOrWhiteSpace(item.cuenta_desc)
    ? ObtenerCuentaDesc(CodEmpresa, conn, item.cuenta_conta)
    : Clean(item.cuenta_desc);
            item.tdocumento = Clean(item.tdocumento);
            item.tipo_documento_desc = MFndFuncionesDb.fxTipoDocumento(item.tdocumento);
            item.banco_desc = Clean(item.banco_desc);
            item.referencia = Clean(item.referencia);
            item.identificacion = Clean(item.identificacion);
            item.id_banco_destino = Clean(item.id_banco_destino);
            item.cta_banco = Clean(item.cta_banco);
            item.cedula_destino = Clean(item.cedula_destino);
            item.cta_iban_destino = Clean(item.cta_iban_destino);
            item.cod_divisa = Clean(item.cod_divisa);
            item.correo_notifica = Clean(item.correo_notifica);
            item.detalle = Clean(item.detalle);

            return item;
        }

        private List<CrSeguimientoDesembolsosBancoDto> ObtenerBancos(
            SqlConnection conn,
            string usuario)
        {
            const string sql = @"exec spCrd_SGT_Bancos_Desembolso @Usuario;";

            return conn.Query<CrSeguimientoDesembolsosBancoDto>(
                sql,
                new { Usuario = usuario.Trim() })
                .ToList();
        }

        private static List<CrSeguimientoDesembolsosTipoIdDto> ObtenerTiposId(SqlConnection conn)
        {
            const string sql = @"
                select 
                    TIPO_ID as idx,
                    rtrim(Descripcion) as itmx
                from AFI_TIPOS_IDS
                order by Tipo_Id;";

            return conn.Query<CrSeguimientoDesembolsosTipoIdDto>(sql).ToList();
        }

        private static List<CrSeguimientoDesembolsosDivisaDto> ObtenerDivisas(SqlConnection conn)
        {
            const string sql = @"
                select
                    COD_DIVISA as idx,
                    DESCRIPCION as itmx
                from vSys_Divisas
                order by COD_DIVISA;";

            return conn.Query<CrSeguimientoDesembolsosDivisaDto>(sql).ToList();
        }

        private CrSeguimientoDesembolsosConceptoDto? ObtenerConceptoInfo(
     int CodEmpresa,
     SqlConnection conn,
     int codConcepto)
        {
            const string sql = @"
        select
            cod_condeb,
            rtrim(descripcion) as descripcion,
            isnull(retiene,0) as retiene,
            isnull(modifica,0) as modifica,
            rtrim(isnull(cod_cuenta,'')) as cod_cuenta,
            isnull(difiere,0) as difiere,
            dbo.MyGetdate() as difiere_fecha
        from concepto_desemb
        where cod_condeb = @codConcepto;";

            var data = conn.QueryFirstOrDefault<CrSeguimientoDesembolsosConceptoDto>(
                sql,
                new { codConcepto });

            if (data == null)
                return null;

            var cuenta = Clean(data.cod_cuenta);

            data.descripcion = Clean(data.descripcion);
            data.cod_cuenta = cuenta;
            data.cod_cuenta_mask = _mCntLinkDB.fxgCntCuentaFormato(
                CodEmpresa,
                true,
                cuenta,
                0);

            data.cuenta_desc = ObtenerCuentaDesc(CodEmpresa, conn, cuenta);

            return data;
        }
        private string ObtenerCuentaDesc(int CodEmpresa, SqlConnection conn, string cuenta)
        {
            cuenta = Clean(cuenta);

            if (string.IsNullOrWhiteSpace(cuenta))
                return string.Empty;

            var codContabilidad = ObtenerCodContabilidad(conn);

            return Clean(_mCntLinkDB.fxgCntCuentaDesc(
                CodEmpresa,
                cuenta,
                codContabilidad));
        }
        private CrSeguimientoDesembolsosResumenDto ObtenerResumenOperacion(
            int CodEmpresa,
            SqlConnection conn,
            long operacion)
        {
            var row = ObtenerOperacionCalculo(conn, operacion);
            if (row == null)
                return new CrSeguimientoDesembolsosResumenDto();

            var interes = CalcularInteresFormalizacion(CodEmpresa, row);
            var primerCuota = CalcularPrimerCuota(row, ref interes);
            var poliza = CalcularPoliza(CodEmpresa, row);
            var registrado = _mCobroDb.fxMontoEnGeneral(CodEmpresa, operacion);

            return new CrSeguimientoDesembolsosResumenDto
            {
                monto_aprobado = row.montoapr,
                monto_registrado = registrado,
                primer_cuota = primerCuota,
                interes = interes,
                poliza = poliza,
                monto_disponible = row.montoapr - (registrado + interes + primerCuota + poliza)
            };
        }

        private static OperacionCalculoRow? ObtenerOperacionCalculo(
            SqlConnection conn,
            long operacion)
        {
            const string sql = @"
                select
                    R.id_solicitud,
                    rtrim(R.codigo) as codigo,
                    rtrim(isnull(R.cod_destino,'')) as cod_destino,
                    rtrim(isnull(R.primer_cuota,'')) as primer_cuota,
                    rtrim(isnull(R.garantia,'')) as garantia,
                    isnull(R.montoapr,0) as montoapr,
                    isnull(R.cuota,0) as cuota,
                    isnull(R.int,0) as tasa_int,
                    isnull(R.interesv, R.int) as interesv,
                    isnull(R.prideduc,0) as prideduc,
                    isnull(R.dia_pago,32) as dia_pago,
                    R.fechaforp,
                    R.fecha_inicio_calculo,
                    rtrim(isnull(C.convenio,'N')) as convenio
                from reg_creditos R
                inner join Catalogo C on R.codigo = C.codigo
                where R.id_solicitud = @operacion;";

            return conn.QueryFirstOrDefault<OperacionCalculoRow>(
                sql,
                new { operacion });
        }

        private decimal CalcularInteresFormalizacion(
            int CodEmpresa,
            OperacionCalculoRow row)
        {
            var cobra = _mCobroDb.fxCobraTasaFormaliza(
                CodEmpresa,
                row.codigo,
                row.cod_destino);

            if (!cobra)
                return 0m;

            var fecha = row.fecha_inicio_calculo ?? row.fechaforp;

            return _mCobroDb.fxInteresesHastaFormalizar(
                CodEmpresa,
                row.id_solicitud,
                row.codigo,
                fecha,
                null,
                row.prideduc,
                row.dia_pago);
        }

        private static decimal CalcularPrimerCuota(
            OperacionCalculoRow row,
            ref decimal interes)
        {
            if (!string.Equals(row.primer_cuota, "S", StringComparison.OrdinalIgnoreCase))
                return 0m;

            if (interes > 0)
            {
                var fecha = row.fecha_inicio_calculo ?? row.fechaforp;
                interes = MCobroDb.fxInteresesDiasPrimerCuota(
                    fecha,
                    row.montoapr,
                    row.tasa_int);
            }

            return row.cuota;
        }

        private decimal CalcularPoliza(
            int CodEmpresa,
            OperacionCalculoRow row)
        {
            if (string.Equals(row.garantia, "H", StringComparison.OrdinalIgnoreCase))
                return 0m;

            if (!string.Equals(row.convenio, "N", StringComparison.OrdinalIgnoreCase))
                return 0m;

            return _mCobroDb.fxCuotaPolizaVida(CodEmpresa, row.montoapr, row.codigo);
        }

        private ErrorDto<CrSeguimientoDesembolsosResumenDto> ValidarGuardar(
            int CodEmpresa,
            CrSeguimientoDesembolsosGuardarRequest request)
        {
            var errores = new List<string>();

            if (request == null)
                errores.Add("La información del desembolso es requerida.");
            else
                ValidarGuardarData(CodEmpresa, request, errores);

            if (errores.Count == 0)
                return DbHelper.CreateOkResponse(new CrSeguimientoDesembolsosResumenDto());

            return DbHelper.CreateErrorResponse(
                string.Join(Environment.NewLine, errores),
                -2,
                new CrSeguimientoDesembolsosResumenDto());
        }

        private void ValidarGuardarData(
            int CodEmpresa,
            CrSeguimientoDesembolsosGuardarRequest request,
            List<string> errores)
        {
            ValidarBasicos(request, errores);
            ValidarTransferencia(request, errores);
            ValidarMontoDisponible(CodEmpresa, request, errores);
            ValidarCuenta(CodEmpresa, request, errores);
            ValidarDiferido(request, errores);
        }

        private static void ValidarBasicos(
            CrSeguimientoDesembolsosGuardarRequest request,
            List<string> errores)
        {
            if (request.id_solicitud <= 0)
                errores.Add("- La operación es requerida.");

            if (string.IsNullOrWhiteSpace(request.concepto))
                errores.Add("- El concepto no es válido.");

            if (request.monto <= 0)
                errores.Add("- El monto a desembolsar no es válido.");

            if (string.IsNullOrWhiteSpace(request.usuario))
                errores.Add("- El usuario es requerido.");
        }

        private static void ValidarTransferencia(
            CrSeguimientoDesembolsosGuardarRequest request,
            List<string> errores)
        {
            var tipo = ResolverTipoDocumento(request);

            if (tipo != "TE" && tipo != "TS")
                return;

            if (!EmailValido(request.correo_notifica))
                errores.Add("- El Correo Electrónico especificado no es válido.");

            if (string.IsNullOrWhiteSpace(request.cta_iban_destino))
                errores.Add("- Debe indicar una cuenta bancaria destino para este desembolso.");
        }

        private void ValidarMontoDisponible(
    int CodEmpresa,
    CrSeguimientoDesembolsosGuardarRequest request,
    List<string> errores)
        {
            if (!request.id_solicitud.HasValue || !request.id_desembolso.HasValue)
                return;

            var idSolicitud = request.id_solicitud.Value;
            var idDesembolso = request.id_desembolso.Value;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var resumen = ObtenerResumenOperacion(CodEmpresa, conn, idSolicitud);
            var montoActual = ObtenerMontoActual(conn, idDesembolso);
            var disponible = resumen.monto_disponible + montoActual;

            if (request.monto > disponible)
                errores.Add("- El monto a desembolsar es mayor al disponible del préstamo.");
        }

        private void ValidarCuenta(
            int CodEmpresa,
            CrSeguimientoDesembolsosGuardarRequest request,
            List<string> errores)
        {
            if (!_mCntLinkDB.fxgCntCuentaValida(CodEmpresa, request.cuenta_conta))
                errores.Add("- La cuenta contable no es válida.");
        }

        private static void ValidarDiferido(
            CrSeguimientoDesembolsosGuardarRequest request,
            List<string> errores)
        {
            if (request.diferido_aplica != 1 || request.diferido_corte == null)
                return;

            if (request.diferido_corte.Value.Date < DateTime.Today)
                errores.Add("- El corte para diferir no puede ser menor a hoy.");
        }

        private static decimal ObtenerMontoActual(
            SqlConnection conn,
            long idDesembolso)
        {
            if (idDesembolso <= 0)
                return 0m;

            const string sql = @"
                select isnull(monto,0)
                from desembolsos
                where id_desembolso = @idDesembolso;";

            return conn.QueryFirstOrDefault<decimal>(
                sql,
                new { idDesembolso });
        }

        private CrSeguimientoDesembolsosGuardarRequest NormalizarGuardar(
            int CodEmpresa,
            CrSeguimientoDesembolsosGuardarRequest request)
        {
            var data = request;
            data.concepto = Clean(data.concepto).ToUpperInvariant();
            data.codigo = Clean(data.codigo);
            data.cuenta_conta = _mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, false, data.cuenta_conta, 0);
            data.tdocumento = ResolverTipoDocumento(data);
            data.referencia = Clean(data.referencia);
            data.identificacion = Clean(data.identificacion);
            data.cta_banco = Clean(data.cta_iban_destino);
            data.cedula_destino = Clean(data.identificacion);
            data.cta_iban_destino = Clean(data.cta_iban_destino);
            data.cod_divisa = Clean(data.cod_divisa);
            data.correo_notifica = Clean(data.correo_notifica);
            data.detalle = Clean(data.detalle);
            data.usuario = Clean(data.usuario).ToUpperInvariant();

            if (data.retener == 1)
            {
                data.cod_banco = 0;
                data.tdocumento = "ND";
            }

            if (data.diferido_aplica != 1)
                data.diferido_corte = DateTime.Today;

            return data;
        }

        private static string ResolverTipoDocumento(CrSeguimientoDesembolsosGuardarRequest request)
        {
            var tipo = Clean(request.tdocumento);
            var convertido = MFndFuncionesDb.fxTipoDocumento(tipo);

            if (string.IsNullOrWhiteSpace(convertido))
                return tipo.ToUpperInvariant();

            return convertido.Length <= 3 ? convertido : tipo.ToUpperInvariant();
        }

        private static long GuardarDesembolso(
            SqlConnection conn,
            SqlTransaction tx,
            CrSeguimientoDesembolsosGuardarRequest data)
        {
            return data.id_desembolso > 0
                ? ActualizarDesembolso(conn, tx, data)
                : InsertarDesembolso(conn, tx, data);
        }

        private static long InsertarDesembolso(
            SqlConnection conn,
            SqlTransaction tx,
            CrSeguimientoDesembolsosGuardarRequest data)
        {
            const string sql = @"
                insert into desembolsos
                (
                    ID_SOLICITUD,
                    CODIGO,
                    CONCEPTO,
                    MONTO,
                    CUENTA_CONTA,
                    TDOCUMENTO,
                    DEPOSITAR,
                    COD_BANCO,
                    RETENER,
                    MODIFICA,
                    DIFERIDO_APLICA,
                    DIFERIDO_CORTE,
                    REFERENCIA,
                    IDENTIFICACION,
                    CTA_BANCO,
                    TIPO_CED_DESTINO,
                    CEDULA_DESTINO,
                    ID_BANCO_DESTINO,
                    CTA_IBAN_DESTINO,
                    COD_DIVISA,
                    CORREO_NOTIFICA,
                    DETALLE
                )
                output inserted.ID_DESEMBOLSO
                values
                (
                    @id_solicitud,
                    @codigo,
                    @concepto,
                    @monto,
                    @cuenta_conta,
                    @tdocumento,
                    0,
                    @cod_banco,
                    @retener,
                    @modifica,
                    @diferido_aplica,
                    @diferido_corte,
                    @referencia,
                    @identificacion,
                    @cta_banco,
                    @tipo_ced_destino,
                    @cedula_destino,
                    @id_banco_destino,
                    @cta_iban_destino,
                    @cod_divisa,
                    @correo_notifica,
                    @detalle
                );";

            return conn.QuerySingle<long>(sql, data, tx);
        }

        private static long ActualizarDesembolso(SqlConnection conn,SqlTransaction tx,CrSeguimientoDesembolsosGuardarRequest data)
        {
            const string sql = @"
        update desembolsos
        set concepto = @concepto,
            monto = @monto,
            cuenta_conta = @cuenta_conta,
            retener = @retener,
            modifica = @modifica,
            tdocumento = @tdocumento,
            cod_banco = @cod_banco,
            cta_banco = @cta_banco,
            diferido_aplica = @diferido_aplica,
            diferido_corte = @diferido_corte,
            referencia = @referencia,
            identificacion = @identificacion,
            tipo_ced_destino = @tipo_ced_destino,
            cedula_destino = @cedula_destino,
            id_banco_destino = @id_banco_destino,
            cta_iban_destino = @cta_iban_destino,
            cod_divisa = @cod_divisa,
            correo_notifica = @correo_notifica,
            detalle = @detalle
        where id_desembolso = @id_desembolso;";

            if (!data.id_desembolso.HasValue)
                throw new InvalidOperationException("El desembolso es requerido.");

            var idDesembolso = data.id_desembolso.Value;

            var rows = conn.Execute(sql, data, tx);
            if (rows <= 0)
                throw new InvalidOperationException("No se encontró el desembolso para actualizar.");

            return idDesembolso;
        }

        private void RegistrarBitacoraGuardar(
            int CodEmpresa,
            CrSeguimientoDesembolsosGuardarRequest data,
            long idDesembolso)
        {
            var movimiento = data.id_desembolso > 0 ? MovimientoModifica : MovimientoRegistra;
            var detalle = $"Seguimiento Desembolsos: operación {data.id_solicitud}, desembolso {idDesembolso}, monto {data.monto:N2}";

            RegistrarBitacora(CodEmpresa, data.usuario, movimiento, detalle);
        }

        private void RegistrarBitacora(
            int CodEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = Clean(usuario).ToUpperInvariant(),
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloCreditos
            });
        }

        private static List<CrSeguimientoDesembolsosData> FiltrarDesembolsos(
            List<CrSeguimientoDesembolsosData> lista,
            string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return lista;

            return lista.Where(x =>
                   Contiene(x.concepto, texto)
                || Contiene(x.monto, texto)
                || Contiene(x.cuenta_conta, texto)
                || Contiene(x.cuenta_conta_mask, texto)
                || Contiene(x.cuenta_desc, texto)
                || Contiene(x.banco_desc, texto)
                || Contiene(x.referencia, texto)
                || Contiene(x.identificacion, texto)
                || Contiene(x.tdocumento, texto)
                || Contiene(x.cta_banco, texto)
                || Contiene(x.cta_iban_destino, texto)
            ).ToList();
        }

        private static List<CrSeguimientoDesembolsosData> OrdenarDesembolsos(
            List<CrSeguimientoDesembolsosData> lista,
            FiltrosLazyLoadData filtros)
        {
            var sf = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            return sf switch
            {
                "concepto" => Ordenar(lista, asc, x => x.concepto),
                "monto" => Ordenar(lista, asc, x => x.monto),
                "cuenta_conta" => Ordenar(lista, asc, x => x.cuenta_conta),
                "banco_desc" => Ordenar(lista, asc, x => x.banco_desc),
                "referencia" => Ordenar(lista, asc, x => x.referencia),
                "identificacion" => Ordenar(lista, asc, x => x.identificacion),
                "tdocumento" => Ordenar(lista, asc, x => x.tdocumento),
                _ => lista.OrderBy(x => x.id_desembolso).ToList()
            };
        }

        private static List<T> Ordenar<T, TKey>(
            List<T> lista,
            bool asc,
            Func<T, TKey> key)
        {
            return asc
                ? lista.OrderBy(key).ToList()
                : lista.OrderByDescending(key).ToList();
        }

        private static List<T> AplicarPaginacion<T>(
            List<T> lista,
            FiltrosLazyLoadData filtros)
        {
            var pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
            var fetch = filtros.paginacion < 0 ? 0 : filtros.paginacion;

            if (fetch <= 0)
                return lista;

            return lista.Skip(pagina * fetch).Take(fetch).ToList();
        }

        private static FiltroDesembolsoResult ParseFiltros(string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                              ?? new FiltrosLazyLoadData();

                var filtro = ParseFiltroInterno(filtros.filtro);

                return new FiltroDesembolsoResult
                {
                    Filtros = filtros,
                    Operacion = filtro.operacion,
                    Texto = filtro.texto
                };
            }
            catch (JsonException ex)
            {
                return new FiltroDesembolsoResult
                {
                    Error = ex.Message,
                    Filtros = new FiltrosLazyLoadData()
                };
            }
        }

        private static CrSeguimientoDesembolsosFiltroDto ParseFiltroInterno(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return new CrSeguimientoDesembolsosFiltroDto();

            try
            {
                return JsonConvert.DeserializeObject<CrSeguimientoDesembolsosFiltroDto>(filtro)
                       ?? new CrSeguimientoDesembolsosFiltroDto();
            }
            catch (JsonException)
            {
                return new CrSeguimientoDesembolsosFiltroDto
                {
                    texto = filtro.Trim()
                };
            }
        }

        private static string ResetPaginacion(string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                              ?? new FiltrosLazyLoadData();

                filtros.pagina = 0;
                filtros.paginacion = 0;

                return JsonConvert.SerializeObject(filtros);
            }
            catch (JsonException)
            {
                return JsonConvert.SerializeObject(new FiltrosLazyLoadData
                {
                    pagina = 0,
                    paginacion = 0
                });
            }
        }

        private static bool EmailValido(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                _ = new MailAddress(email.Trim());
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        private static int ObtenerCodContabilidad(SqlConnection conn)
        {
            const string sql = @"
        select top 1 isnull(cod_empresa_enlace, 0)
        from sif_empresa;";

            return conn.QueryFirstOrDefault<int>(sql);
        }
        private static bool Contiene(string? valor, string texto)
        {
            return (valor ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase);
        }

        private static bool Contiene(decimal valor, string texto)
        {
            return valor.ToString("0.00").Contains(texto, StringComparison.OrdinalIgnoreCase)
                || valor.ToString("N2").Contains(texto, StringComparison.OrdinalIgnoreCase);
        }

        private static string Clean(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        public sealed class FiltroDesembolsoResult
        {
            public FiltrosLazyLoadData Filtros { get; set; } = new();
            public long Operacion { get; set; }
            public string Texto { get; set; } = string.Empty;
            public string? Error { get; set; }
        }

        public sealed class CrSeguimientoDesembolsosFiltroDto
        {
            public long operacion { get; set; }
            public string texto { get; set; } = string.Empty;
        }

        public sealed class OperacionCalculoRow
        {
            public long id_solicitud { get; set; }
            public string codigo { get; set; } = string.Empty;
            public string cod_destino { get; set; } = string.Empty;
            public string primer_cuota { get; set; } = string.Empty;
            public string garantia { get; set; } = string.Empty;
            public decimal montoapr { get; set; }
            public decimal cuota { get; set; }
            public decimal tasa_int { get; set; }
            public decimal interesv { get; set; }
            public decimal prideduc { get; set; }
            public int dia_pago { get; set; }
            public DateTime fechaforp { get; set; }
            public DateTime? fecha_inicio_calculo { get; set; }
            public string convenio { get; set; } = "N";
        }
    }
}