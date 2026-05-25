using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRConfiguracionDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MCntLinkDB _mCntLinkDB;
        private readonly MBeneficiosDB _mBeneficiosDB;
        private const int ModuloCreditos = 3;
        private const string ModificaWeb = "MODIFICA-WEB";
        private const string TipoCuenta = "CTA";
        private const string TipoDecimal = "DEC";
        private const string TipoNumero = "NUM";
        private const string TipoPorcentaje = "POR";
        private const string TipoCaracter = "CHR";
        private const string TipoPregunta = "PSN";
        private const string TipoFecha = "DTS";
        private const string DATOSREQUERIDOS = "Datos requeridos.";

        public FrmCRConfiguracionDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mCntLinkDB = new MCntLinkDB(config);
            _mBeneficiosDB = new MBeneficiosDB(config);
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
        /// Obtiene la lista completa de parámetros generales de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConfiguracionGeneralDto>> CR_Configuracion_Generales_Lista_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                InicializarParametros(conn);

                const string sql = @"
                    select
                        rtrim(COD_PARAMETRO) as cod_parametro,
                        rtrim(DESCRIPCION) as descripcion,
                        isnull(rtrim(VALOR), '') as valor,
                        isnull(rtrim(TIPO), '') as tipo,
                        isnull(rtrim(VISIBLE), '') as visible,
                        isnull(rtrim(NOTAS), '') as notas,
                        INICIO_FECHA as inicio_fecha,
                        isnull(rtrim(MODIFICA_USUARIO), '') as modifica_usuario,
                        MODIFICA_FECHA as modifica_fecha
                    from CRD_PARAMETROS
                    order by COD_PARAMETRO;";

                var lista = conn.Query<CrConfiguracionGeneralDto>(sql).ToList();
                CompletarInfoParametros(CodEmpresa, lista);

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrConfiguracionGeneralDto>>(ex.Message, -1, new List<CrConfiguracionGeneralDto>());
            }
        }

        /// <summary>
        /// Exporta la lista completa de parámetros generales de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConfiguracionGeneralDto>> CR_Configuracion_Generales_Lista_Export(int CodEmpresa)
        {
            return CR_Configuracion_Generales_Lista_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Guarda un parámetro general de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Configuracion_Generales_Guardar(
            int CodEmpresa,
            CrConfiguracionGeneralGuardarDto request,
            string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse(DATOSREQUERIDOS, -2);
                }

                var codParametro = NormalizarTexto(request.cod_parametro);
                var tipo = NormalizarTexto(request.tipo).ToUpperInvariant();
                var usuarioNorm = NormalizarTexto(usuario).ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(codParametro))
                {
                    return DbHelper.ErrorResponse("Código de parámetro requerido.", -2);
                }

                var existe = conn.QuerySingle<int>(
                    "select isnull(count(1), 0) from CRD_PARAMETROS where COD_PARAMETRO = @codParametro;",
                    new { codParametro });

                if (existe <= 0)
                {
                    return DbHelper.ErrorResponse("El parámetro indicado no existe.", -2);
                }

                var valorResult = NormalizarValorParametro(CodEmpresa, request.valor, tipo);
                if (valorResult.Code != 0)
                {
                    return valorResult;
                }

                const string sql = @"
                    update CRD_PARAMETROS
                    set MODIFICA_USUARIO = @usuario,
                        MODIFICA_FECHA = dbo.MyGetdate(),
                        VALOR = @valor
                    where COD_PARAMETRO = @codParametro;";

                conn.Execute(sql, new
                {
                    usuario = usuarioNorm,
                    valor = valorResult.Description ?? string.Empty,
                    codParametro
                });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = $"Parámetro de Crédito: {codParametro} -> {valorResult.Description}",
                    Movimiento = ModificaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Parámetro actualizado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene parámetros operativos de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CrConfiguracionOperativosDto> CR_Configuracion_Operativos_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                InicializarParametros(conn);

                const string sql = @"
                    select top 1
                        CR_FECHA_CALCULO as cr_fecha_calculo,
                        isnull(CR_POR_AHORRO, 100) as cr_por_ahorro,
                        isnull(CR_TBP, 15) as cr_tbp,
                        isnull(rtrim(CR_CTA_DESEMBOLSO), '') as cr_cta_desembolso,
                        isnull(rtrim(CR_CTA_POLIZAS), '') as cr_cta_polizas,
                        isnull(CR_PSDMNT, 0) as cr_psdmnt,
                        isnull(REGLA_MONTO, 0) as regla_monto,
                        isnull(COD_BANCO, 0) as cod_banco,
                        isnull(rtrim(TIPODOC), '') as tipodoc,
                        isnull(COD_BANCO_MEN, 0) as cod_banco_men,
                        isnull(rtrim(COD_TIPO_MEN), '') as cod_tipo_men,
                        isnull(REGLA_BANCO, 0) as regla_banco
                    from PAR_AHCR;";

                var dto = conn.QueryFirstOrDefault<CrConfiguracionOperativosDto>(sql)
                    ?? new CrConfiguracionOperativosDto();

                CompletarInfoOperativos(CodEmpresa, dto);

                return DbHelper.CreateOkResponse(dto);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConfiguracionOperativosDto>(ex.Message, -1, new CrConfiguracionOperativosDto());
            }
        }

        /// <summary>
        /// Guarda parámetros operativos de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Configuracion_Operativos_Guardar(int CodEmpresa, CrConfiguracionOperativosGuardarDto request, string usuario)
        {
            using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse(DATOSREQUERIDOS, -2);
                }

                var usuarioNorm = NormalizarTexto(usuario).ToUpperInvariant();
                var validacion = ValidarOperativos(CodEmpresa, request);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                conn.Open();

                var crPsdmnt = request.cr_psdmnt.GetValueOrDefault();
                var reglaMonto = request.regla_monto.GetValueOrDefault();
                var codBanco = request.cod_banco.GetValueOrDefault();
                var codBancoMen = request.cod_banco_men.GetValueOrDefault();
                var reglaBanco = request.regla_banco.GetValueOrDefault() ? 1 : 0;

                var ctaDesembolso = _mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, false, request.cr_cta_desembolso, 0);
                var ctaPolizas = _mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, false, request.cr_cta_polizas, 0);
                var tipoDoc = NormalizarTipoDocumento(request.tipodoc);
                var tipoMen = NormalizarTipoDocumento(request.cod_tipo_men);

                using var tx = conn.BeginTransaction();

                const string sqlPar = @"
            update PAR_AHCR
            set CR_CTA_DESEMBOLSO = @ctaDesembolso,
                CR_CTA_POLIZAS = @ctaPolizas,
                CR_PSDMNT = @crPsdmnt,
                COD_BANCO = @codBanco,
                TIPODOC = @tipoDoc,
                COD_BANCO_MEN = @codBancoMen,
                COD_TIPO_MEN = @tipoMen,
                REGLA_BANCO = @reglaBanco,
                REGLA_MONTO = @reglaMonto;";

                conn.Execute(sqlPar, new
                {
                    ctaDesembolso,
                    ctaPolizas,
                    crPsdmnt,
                    codBanco,
                    tipoDoc,
                    codBancoMen,
                    tipoMen,
                    reglaBanco,
                    reglaMonto
                }, tx);

                GuardarParametroInterno(conn, tx, "03", ctaDesembolso, usuarioNorm);
                GuardarParametroInterno(conn, tx, "04", ctaPolizas, usuarioNorm);
                GuardarParametroInterno(conn, tx, "06", ToSqlDecimal(crPsdmnt), usuarioNorm);
                GuardarParametroInterno(conn, tx, "10", ToSqlDecimal(reglaMonto), usuarioNorm);
                GuardarParametroInterno(conn, tx, "11", codBanco.ToString(CultureInfo.InvariantCulture), usuarioNorm);
                GuardarParametroInterno(conn, tx, "12", tipoDoc, usuarioNorm);
                GuardarParametroInterno(conn, tx, "13", codBancoMen.ToString(CultureInfo.InvariantCulture), usuarioNorm);
                GuardarParametroInterno(conn, tx, "14", tipoMen, usuarioNorm);
                GuardarParametroInterno(conn, tx, "15", reglaBanco.ToString(CultureInfo.InvariantCulture), usuarioNorm);

                tx.Commit();

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = "Parámetros de Créditos : Cuentas y Desembolsos",
                    Movimiento = ModificaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("La información se guardó satisfactoriamente.");
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
        /// Guarda la fecha de corte para cálculo de intereses.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Configuracion_FechaCorte_Guardar(int CodEmpresa,CrConfiguracionFechaCorteGuardarDto request,string usuario)
        {
            using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

            try
            {
                if (request == null || !request.cr_fecha_calculo.HasValue)
                {
                    return DbHelper.ErrorResponse("Fecha de corte requerida.", -2);
                }

                conn.Open();

                var usuarioNorm = NormalizarTexto(usuario).ToUpperInvariant();
                var fecha = request.cr_fecha_calculo.Value;

                using var tx = conn.BeginTransaction();

                const string sqlPar = @"
            update PAR_AHCR
            set CR_FECHA_CALCULO = @fecha;";

                conn.Execute(sqlPar, new { fecha }, tx);

                GuardarParametroInterno(
                    conn,
                    tx,
                    "09",
                    fecha.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
                    usuarioNorm);

                tx.Commit();

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = $"Fecha Calculo Intereses : {fecha:yyyy/MM/dd}",
                    Movimiento = ModificaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("La Fecha de Corte Se Cambió Satisfactoriamente.");
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
        /// Guarda la tasa básica pasiva.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Configuracion_TBP_Guardar(int CodEmpresa,CrConfiguracionTbpGuardarDto request,string usuario)
        {
            using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse(DATOSREQUERIDOS, -2);
                }

                if (request.cr_tbp < 0 || request.cr_tbp > 100)
                {
                    return DbHelper.ErrorResponse("La Tasa Básica Pasiva debe estar entre 0 y 100.", -2);
                }

                conn.Open();

                var usuarioNorm = NormalizarTexto(usuario).ToUpperInvariant();

                using var tx = conn.BeginTransaction();

                const string sqlPar = @"
            update PAR_AHCR
            set CR_TBP = @tbp;";

                conn.Execute(sqlPar, new { tbp = request.cr_tbp }, tx);

                GuardarParametroInterno(
                conn,
                tx,
                "07",
                ToSqlDecimal(request.cr_tbp ?? 0),
                usuarioNorm);

                tx.Commit();

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = $"% Tasa Básica Pasiva {request.cr_tbp}",
                    Movimiento = ModificaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Actualización Realizada.");
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
        /// Obtiene lista de bancos activos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Configuracion_Bancos_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        cast(ID_BANCO as varchar(20)) as item,
                        rtrim(DESCRIPCION) as descripcion
                    from TES_BANCOS
                    where ESTADO = 'A'
                    order by DESCRIPCION;";

                return DbHelper.CreateOkResponse(conn.Query<DropDownListaGenericaModel>(sql).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene lista fija de tipos de documento.
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Configuracion_TiposDocumento_Dropdown_Obtener()
        {
            var lista = new List<DropDownListaGenericaModel>
            {
                new() { item = "CK", descripcion = "01 - Cheque" },
                new() { item = "TE", descripcion = "02 - Transferencia" }
            };

            return DbHelper.CreateOkResponse(lista);
        }

        private static void InicializarParametros(IDbConnection conn)
        {
            conn.Execute("spCRDParametros", commandType: CommandType.StoredProcedure);
        }

        private void CompletarInfoParametros(int CodEmpresa, List<CrConfiguracionGeneralDto> lista)
        {
            foreach (var item in lista)
            {
                var tipo = NormalizarTexto(item.tipo).ToUpperInvariant();

                if (tipo != TipoCuenta)
                {
                    item.valor_mask = item.valor;
                    item.cuenta_descripcion = string.Empty;
                    continue;
                }

                var cuenta = NormalizarTexto(item.valor);
                var cuentaFormato = cuenta.Length == 0
                    ? string.Empty
                    : _mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, false, cuenta, 0);

                item.valor_mask = cuenta.Length == 0
                    ? string.Empty
                    : _mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, true, cuenta, 0);

                item.cuenta_descripcion = cuentaFormato.Length == 0
                    ? string.Empty
                    : ObtenerDescripcionCuenta(CodEmpresa, cuentaFormato);
            }
        }

        private void CompletarInfoOperativos(int CodEmpresa, CrConfiguracionOperativosDto dto)
        {
            var ctaDesembolso = string.IsNullOrWhiteSpace(dto.cr_cta_desembolso)
                ? string.Empty
                : _mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, false, dto.cr_cta_desembolso, 0);

            var ctaPolizas = string.IsNullOrWhiteSpace(dto.cr_cta_polizas)
                ? string.Empty
                : _mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, false, dto.cr_cta_polizas, 0);

            dto.cr_cta_desembolso_mask = string.IsNullOrWhiteSpace(dto.cr_cta_desembolso)
                ? string.Empty
                : _mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, true, dto.cr_cta_desembolso, 0);

            dto.cr_cta_desembolso_desc = string.IsNullOrWhiteSpace(ctaDesembolso)
                ? string.Empty
                : ObtenerDescripcionCuenta(CodEmpresa, ctaDesembolso);

            dto.cr_cta_polizas_mask = string.IsNullOrWhiteSpace(dto.cr_cta_polizas)
                ? string.Empty
                : _mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, true, dto.cr_cta_polizas, 0);

            dto.cr_cta_polizas_desc = string.IsNullOrWhiteSpace(ctaPolizas)
                ? string.Empty
                : ObtenerDescripcionCuenta(CodEmpresa, ctaPolizas);

            dto.cod_banco_desc = dto.cod_banco > 0
                ? (_mBeneficiosDB.fxDescribeBanco(CodEmpresa, dto.cod_banco).Description ?? string.Empty)
                : string.Empty;

            dto.cod_banco_men_desc = dto.cod_banco_men > 0
                ? (_mBeneficiosDB.fxDescribeBanco(CodEmpresa, dto.cod_banco_men).Description ?? string.Empty)
                : string.Empty;

            dto.tipodoc = NormalizarTipoDocumento(dto.tipodoc);
            dto.cod_tipo_men = NormalizarTipoDocumento(dto.cod_tipo_men);
        }
        private string ObtenerDescripcionCuenta(int CodEmpresa, string cuenta)
        {
            using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

            const string sql = @"
        select top 1
            isnull(rtrim(c.Descripcion), '') as descripcion
        from CntX_Cuentas c
        where c.cod_cuenta = @cuenta
          and c.cod_contabilidad = (
              select top 1 cod_empresa_enlace
              from sif_empresa
          );";

            return conn.QueryFirstOrDefault<string>(sql, new { cuenta }) ?? string.Empty;
        }
        private ErrorDto ValidarOperativos(int CodEmpresa, CrConfiguracionOperativosGuardarDto request)
        {
            if (!_mCntLinkDB.fxgCntCuentaValida(CodEmpresa, request.cr_cta_desembolso))
            {
                return DbHelper.ErrorResponse("No se encontró la cuenta contable para desembolsos.", -2);
            }

            if (!_mCntLinkDB.fxgCntCuentaValida(CodEmpresa, request.cr_cta_polizas))
            {
                return DbHelper.ErrorResponse("No se encontró la cuenta contable para pólizas.", -2);
            }

            var tipoDoc = NormalizarTipoDocumento(request.tipodoc);
            if (tipoDoc != "CK" && tipoDoc != "TE")
            {
                return DbHelper.ErrorResponse("Tipo documento requerido.", -2);
            }

            var tipoMen = NormalizarTipoDocumento(request.cod_tipo_men);
            if (tipoMen != "CK" && tipoMen != "TE")
            {
                return DbHelper.ErrorResponse("Tipo documento menor requerido.", -2);
            }

            if (request.cod_banco.GetValueOrDefault() <= 0)
            {
                return DbHelper.ErrorResponse("Banco requerido.", -2);
            }

            if (request.cod_banco_men.GetValueOrDefault() <= 0)
            {
                return DbHelper.ErrorResponse("Banco menor requerido.", -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private ErrorDto NormalizarValorParametro(int CodEmpresa, string valor, string tipo)
        {
            var value = NormalizarTexto(valor);

            switch (tipo)
            {
                case TipoDecimal:
                case TipoPorcentaje:
                    return ValidarDecimal(value, "El valor indicado no es válido.");

                case TipoNumero:
                    return ValidarNumero(value);

                case TipoCuenta:
                    return ValidarCuenta(CodEmpresa, value);

                case TipoCaracter:
                    if (value.Contains('\''))
                    {
                        return DbHelper.ErrorResponse("El valor indicado contiene caracteres no válidos.", -2);
                    }
                    return DbHelper.OkResponse(value);

                case TipoPregunta:
                    return ValidarPregunta(value);

                case TipoFecha:
                    return ValidarFecha(value);

                default:
                    return DbHelper.OkResponse(value);
            }
        }

        private static ErrorDto ValidarDecimal(string value, string mensaje)
        {
            if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out var dec))
            {
                return DbHelper.ErrorResponse(mensaje, -2);
            }

            return DbHelper.OkResponse(ToSqlDecimal(dec));
        }

        private static ErrorDto ValidarNumero(string value)
        {
            if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var num))
            {
                return DbHelper.ErrorResponse("El valor indicado no es válido.", -2);
            }

            return DbHelper.OkResponse(num.ToString(CultureInfo.InvariantCulture));
        }

        private ErrorDto ValidarCuenta(int CodEmpresa, string value)
        {
            if (!_mCntLinkDB.fxgCntCuentaValida(CodEmpresa, value))
            {
                return DbHelper.ErrorResponse("La cuenta indicada no es válida, presione F4 para buscar en el catálogo.", -2);
            }

            return DbHelper.OkResponse(_mCntLinkDB.fxgCntCuentaFormato(CodEmpresa, false, value, 0));
        }

        private static ErrorDto ValidarPregunta(string value)
        {
            var result = NormalizarTexto(value).ToUpperInvariant();

            if (result.Length > 0)
            {
                result = result[..1];
            }

            if (result != "S" && result != "N")
            {
                return DbHelper.ErrorResponse("El valor indicado no es válido. Indique [S] ó [N].", -2);
            }

            return DbHelper.OkResponse(result);
        }

        private static ErrorDto ValidarFecha(string value)
        {
            if (!DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var fecha))
            {
                return DbHelper.ErrorResponse("La fecha indicada no es válida.", -2);
            }

            return DbHelper.OkResponse(fecha.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture));
        }

        private static void GuardarParametroInterno(
            IDbConnection conn,
            IDbTransaction tx,
            string codParametro,
            string valor,
            string usuario)
        {
            const string sql = @"
                update CRD_PARAMETROS
                set MODIFICA_USUARIO = @usuario,
                    MODIFICA_FECHA = dbo.MyGetdate(),
                    VALOR = @valor
                where COD_PARAMETRO = @codParametro;";

            conn.Execute(sql, new
            {
                usuario,
                valor,
                codParametro
            }, tx);
        }

        private static string NormalizarTipoDocumento(string value)
        {
            var val = NormalizarTexto(value).ToUpperInvariant();

            if (val.StartsWith("01", StringComparison.Ordinal))
            {
                return "CK";
            }

            if (val.StartsWith("02", StringComparison.Ordinal))
            {
                return "TE";
            }

            return val;
        }

        private static string ToSqlDecimal(decimal value)
        {
            return value.ToString("0.############################", CultureInfo.InvariantCulture);
        }

        private static string NormalizarTexto(string? value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}