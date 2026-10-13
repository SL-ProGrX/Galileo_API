using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Convenios;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    public class FrmCrConveniosParametrosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly MCntLinkDB _cntLinkDb;
        private const int VModulo = 16; // Módulo de Convenios (vModulo del formulario VB6)
        private const string MsgValorInvalido = "El valor indicado no es válido...!!!";
        private const string MsgCuentaInvalida = "La Cuenta indicada no es válida, presione F4 para buscar en el catálogo...!!!";

        public FrmCrConveniosParametrosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
            _cntLinkDb = new MCntLinkDB(config);
        }

        /// <summary>
        /// Inicializa (spConvenios_Parametros) y obtiene la lista de parámetros de convenios.
        /// </summary>
        public ErrorDto<List<CrConvenioParametroData>> CrConveniosParametros_Obtener(int codEmpresa, int codContabilidad)
        {
            const string sqlLista = @"
                select
                    rtrim(isnull(COD_PARAMETRO, '')) as cod_parametro,
                    rtrim(isnull(DESCRIPCION, '')) as descripcion,
                    rtrim(convert(varchar(500), isnull(VALOR, ''))) as valor,
                    rtrim(isnull(TIPO, '')) as tipo,
                    rtrim(isnull(NOTAS, '')) as notas,
                    rtrim(isnull(MODIFICA_USUARIO, '')) as modifica_usuario,
                    MODIFICA_FECHA as modifica_fecha
                from CRD_CONVENIOS_PARAMETROS
                order by COD_PARAMETRO;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Execute("spConvenios_Parametros", commandType: CommandType.StoredProcedure);

                var lista = conn.Query<CrConvenioParametroData>(sqlLista).ToList();

                foreach (var item in lista.Where(x => EsCuenta(x.tipo)))
                {
                    CompletarCuenta(codEmpresa, codContabilidad, item);
                }

                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<CrConvenioParametroData>());
            }
        }

        /// <summary>
        /// Valida según el tipo del parámetro y actualiza su valor.
        /// </summary>
        public ErrorDto<CrConvenioParametroData> CrConveniosParametros_Actualizar(
            int codEmpresa,
            int codContabilidad,
            CrConvenioParametroActualizarRequest? request)
        {
            var codParametro = (request?.cod_parametro ?? string.Empty).Trim();
            var usuario = (request?.usuario ?? string.Empty).Trim();

            if (codParametro.Length == 0)
            {
                return DbHelper.CreateErrorResponse<CrConvenioParametroData>("Debe indicar el código del parámetro.", -2);
            }

            if (usuario.Length == 0)
            {
                return DbHelper.CreateErrorResponse<CrConvenioParametroData>("Debe indicar el usuario.", -2);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var parametro = conn.QueryFirstOrDefault<CrConvenioParametroData>(@"
                    select
                        rtrim(COD_PARAMETRO) as cod_parametro,
                        rtrim(isnull(DESCRIPCION, '')) as descripcion,
                        rtrim(isnull(TIPO, '')) as tipo
                    from CRD_CONVENIOS_PARAMETROS
                    where COD_PARAMETRO = @codParametro;", new { codParametro });

                if (parametro == null)
                {
                    return DbHelper.CreateErrorResponse<CrConvenioParametroData>("El parámetro indicado no existe.", -2);
                }

                var (ok, valor, mensaje) = NormalizarValor(codEmpresa, parametro.tipo, request!.valor);
                if (!ok)
                {
                    return DbHelper.CreateErrorResponse<CrConvenioParametroData>(mensaje ?? MsgValorInvalido, -2);
                }

                conn.Execute(@"
                    update CRD_CONVENIOS_PARAMETROS
                    set MODIFICA_USUARIO = @usuario,
                        MODIFICA_FECHA = dbo.MyGetdate(),
                        VALOR = @valor
                    where COD_PARAMETRO = @codParametro;", new { usuario, valor, codParametro });

                _bitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Parámetro de Convenios: {codParametro} -> {valor}",
                    Movimiento = "Modifica - WEB",
                    Modulo = VModulo
                });

                parametro.valor = valor;
                parametro.modifica_usuario = usuario;
                parametro.modifica_fecha = DateTime.Now;
                if (EsCuenta(parametro.tipo))
                {
                    CompletarCuenta(codEmpresa, codContabilidad, parametro);
                }

                return DbHelper.CreateOkResponse(parametro, "Parámetro actualizado satisfactoriamente...!");
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrConvenioParametroData>(ex.Message, -1);
            }
        }

        private static bool EsCuenta(string? tipo)
            => string.Equals((tipo ?? string.Empty).Trim(), "CTA", StringComparison.OrdinalIgnoreCase);

        private void CompletarCuenta(int codEmpresa, int codContabilidad, CrConvenioParametroData item)
        {
            var cuenta = (item.valor ?? string.Empty).Trim();
            if (cuenta.Length == 0)
            {
                item.valor_mask = string.Empty;
                item.cuenta_descripcion = string.Empty;
                return;
            }

            item.valor_mask = _cntLinkDb.fxgCntCuentaFormato(codEmpresa, true, cuenta, 0);
            item.cuenta_descripcion = _cntLinkDb.fxgCntCuentaDesc(codEmpresa, cuenta, codContabilidad);
        }

        /// <summary>
        /// Equivalente a sbGuardaParametro del VB6: valida y normaliza el valor según el tipo.
        /// </summary>
        private (bool ok, string valor, string? mensaje) NormalizarValor(int codEmpresa, string? tipo, string? entrada)
        {
            var valor = (entrada ?? string.Empty).Trim();

            switch ((tipo ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "DEC":
                case "POR":
                    return decimal.TryParse(valor.Replace(",", "."), NumberStyles.Number,
                            CultureInfo.InvariantCulture, out var dec)
                        ? (true, dec.ToString(CultureInfo.InvariantCulture), null)
                        : (false, string.Empty, MsgValorInvalido);

                case "NUM":
                    return long.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num)
                        ? (true, num.ToString(CultureInfo.InvariantCulture), null)
                        : (false, string.Empty, MsgValorInvalido);

                case "CTA":
                    var cuenta = _cntLinkDb.fxgCntCuentaFormato(codEmpresa, false, valor, 0);
                    return cuenta.Length > 0 && _cntLinkDb.fxgCntCuentaValida(codEmpresa, cuenta)
                        ? (true, cuenta, null)
                        : (false, string.Empty, MsgCuentaInvalida);

                case "CHR":
                    return valor.Contains('\'')
                        ? (false, string.Empty, "El valor indicado contiene caracteres no válidos...!!!")
                        : (true, valor, null);

                case "PSN":
                    var letra = valor.Length > 0 ? valor[..1].ToUpperInvariant() : string.Empty;
                    return letra is "S" or "N"
                        ? (true, letra, null)
                        : (false, string.Empty, "El valor indicado no es válido > Indique [S] ó [N]...!!!");

                case "DTS":
                    return TryParseFecha(valor, out var fecha)
                        ? (true, fecha.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture), null)
                        : (false, string.Empty, "La Fecha indicada no es válida...!!!");

                default:
                    return (true, valor, null);
            }
        }

        private static bool TryParseFecha(string valor, out DateTime fecha)
        {
            var formatos = new[] { "yyyy/MM/dd", "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy" };

            return DateTime.TryParseExact(valor, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha)
                   || DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
        }
    }
}
