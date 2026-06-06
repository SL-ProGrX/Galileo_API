using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhExcedentesParametrosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MCntLinkDB _cntLinkDb;
        private const int vModulo = 2;

        public FrmAhExcedentesParametrosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _cntLinkDb = new MCntLinkDB(config);
        }

        /// <summary>
        /// Inicializa y obtiene la lista de parámetros de excedentes.
        /// Ejecuta primero la lógica equivalente al VB6 `spEXC_Parametros`.
        /// </summary>
        public ErrorDto<List<FrmAhExcedentesParametroDto>> Ah_ExcedentesParametros_Lista(int codEmpresa)
        {
            const string sqlLista = @"
select
    rtrim(isnull(cod_parametro, '')) as cod_parametro,
    rtrim(isnull(descripcion, '')) as descripcion,
    rtrim(convert(varchar(500), isnull(valor, ''))) as valor,
    rtrim(isnull(tipo, '')) as tipo,
    rtrim(isnull(notas, '')) as notas,
    rtrim(isnull(modifica_usuario, '')) as modifica_usuario,
    modifica_fecha
from EXC_PARAMETROS
order by cod_parametro;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Execute("spEXC_Parametros", commandType: System.Data.CommandType.StoredProcedure);

                var lista = conn.Query<FrmAhExcedentesParametroDto>(sqlLista).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<FrmAhExcedentesParametroDto>());
            }
        }

        /// <summary>
        /// Actualiza el valor de un parámetro de excedentes según el tipo definido en la tabla.
        /// </summary>
        public ErrorDto<bool> Ah_ExcedentesParametros_Actualizar(
            int codEmpresa,
            FrmAhExcedentesParametroActualizarRequest? request)
        {
            var validacion = Ah_ExcedentesParametros_ValidarActualizarRequest(request);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            var codParametro = (request!.cod_parametro ?? string.Empty).Trim();
            var tipoNormalizado = Ah_ExcedentesParametros_NormalizarTipo(request.tipo);
            var usuario = (request.usuario ?? string.Empty).Trim();

            var normalizacionValor = Ah_ExcedentesParametros_NormalizarValorPorTipo(
                codEmpresa,
                tipoNormalizado,
                request.valor);

            if (!normalizacionValor.ok)
            {
                return DbHelper.CreateErrorResponse(
                    normalizacionValor.mensaje ?? "El valor indicado no es válido.",
                    -2,
                    false);
            }

            const string sqlExiste = @"
select cast(count(1) as int)
from EXC_PARAMETROS
where cod_parametro = @cod_parametro;";

            const string sqlUpdate = @"
update EXC_PARAMETROS
set
    valor = @valor,
    modifica_usuario = @usuario,
    modifica_fecha = dbo.MyGetdate()
where cod_parametro = @cod_parametro;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var existe = conn.QueryFirstOrDefault<int>(sqlExiste, new
                {
                    cod_parametro = codParametro
                }) > 0;

                if (!existe)
                {
                    return DbHelper.CreateErrorResponse(
                        "El parámetro indicado no existe.",
                        -2,
                        false);
                }

                conn.Execute(sqlUpdate, new
                {
                    cod_parametro = codParametro,
                    valor = normalizacionValor.valor,
                    usuario
                });

                Ah_ExcedentesParametros_RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    "Modifica - WEB",
                    $"Parámetro de Excedentes: {codParametro} -> {normalizacionValor.valor}");

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, false);
            }
        }

        private static ErrorDto<bool> Ah_ExcedentesParametros_ValidarActualizarRequest(
            FrmAhExcedentesParametroActualizarRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -2, false);
            }

            if (string.IsNullOrWhiteSpace(request.cod_parametro))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el código del parámetro.", -2, false);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, false);
            }

            var tipoNormalizado = Ah_ExcedentesParametros_NormalizarTipo(request.tipo);
            if (string.IsNullOrWhiteSpace(tipoNormalizado))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el tipo del parámetro.", -2, false);
            }

            return DbHelper.CreateOkResponse(true);
        }

        private (bool ok, string valor, string? mensaje) Ah_ExcedentesParametros_NormalizarValorPorTipo(
    int codEmpresa,
    string tipo,
    string? valorEntrada)
        {
            var valor = (valorEntrada ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(valor))
            {
                return (false, string.Empty, "El valor no puede quedar vacío.");
            }

            return tipo switch
            {
                "DEC" => Ah_ExcedentesParametros_NormalizarDecimal(valor),
                "NUM" => Ah_ExcedentesParametros_NormalizarEntero(valor),
                "POR" => Ah_ExcedentesParametros_NormalizarPorcentaje(valor),
                "CTA" => Ah_ExcedentesParametros_NormalizarCuenta(codEmpresa, valor),
                "CHR" => Ah_ExcedentesParametros_NormalizarCaracter(valor),
                "PSN" => Ah_ExcedentesParametros_NormalizarPsn(valor),
                "DTS" => Ah_ExcedentesParametros_NormalizarFecha(valor),
                _ => (true, valor, null)
            };
        }

        private static (bool ok, string valor, string? mensaje) Ah_ExcedentesParametros_NormalizarDecimal(string valor)
        {
            if (!decimal.TryParse(
                    valor.Replace(",", "."),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var numeroDecimal))
            {
                return (false, string.Empty, "El valor indicado no es válido.");
            }

            return (true, numeroDecimal.ToString(CultureInfo.InvariantCulture), null);
        }

        private static (bool ok, string valor, string? mensaje) Ah_ExcedentesParametros_NormalizarEntero(string valor)
        {
            if (!long.TryParse(valor, out var numeroEntero))
            {
                return (false, string.Empty, "El valor indicado no es válido.");
            }

            return (true, numeroEntero.ToString(CultureInfo.InvariantCulture), null);
        }

        private static (bool ok, string valor, string? mensaje) Ah_ExcedentesParametros_NormalizarPorcentaje(string valor)
        {
            if (!decimal.TryParse(
                    valor.Replace(",", "."),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var porcentaje))
            {
                return (false, string.Empty, "El valor indicado no es válido, suministre un porcentaje.");
            }

            return (true, porcentaje.ToString(CultureInfo.InvariantCulture), null);
        }

        private (bool ok, string valor, string? mensaje) Ah_ExcedentesParametros_NormalizarCuenta(
            int codEmpresa,
            string valor)
        {
            var cuenta = new string(valor.Where(char.IsDigit).ToArray());

            if (string.IsNullOrWhiteSpace(cuenta))
            {
                return (false, string.Empty, "La Cuenta indicada no es válida, presione F4 para buscar en el catálogo.");
            }

            var cuentaValida = _cntLinkDb.fxgCntCuentaValida(codEmpresa, cuenta);
            if (!cuentaValida)
            {
                return (false, string.Empty, "La Cuenta indicada no es válida, presione F4 para buscar en el catálogo.");
            }

            var cuentaNormalizada = _cntLinkDb.fxgCntCuentaFormato(codEmpresa, false, cuenta, 0);
            return (true, cuentaNormalizada, null);
        }

        private static (bool ok, string valor, string? mensaje) Ah_ExcedentesParametros_NormalizarCaracter(string valor)
        {
            if (valor.Contains('\''))
            {
                return (false, string.Empty, "El valor indicado contiene caracteres no válidos.");
            }

            return (true, valor, null);
        }

        private static (bool ok, string valor, string? mensaje) Ah_ExcedentesParametros_NormalizarPsn(string valor)
        {
            var letra = valor[..1].ToUpperInvariant();

            if (letra != "S" && letra != "N")
            {
                return (false, string.Empty, "El valor indicado no es válido. Indique [S] o [N].");
            }

            return (true, letra, null);
        }

        private static (bool ok, string valor, string? mensaje) Ah_ExcedentesParametros_NormalizarFecha(string valor)
        {
            if (!Ah_ExcedentesParametros_TryParseFecha(valor, out var fecha))
            {
                return (false, string.Empty, "La Fecha indicada no es válida.");
            }

            return (true, fecha.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture), null);
        }

        private static string Ah_ExcedentesParametros_NormalizarTipo(string? tipo)
        {
            return (tipo ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static bool Ah_ExcedentesParametros_TryParseFecha(string valor, out DateTime fecha)
        {
            var formatos = new[]
            {
                "yyyy/MM/dd",
                "yyyy-MM-dd",
                "dd/MM/yyyy",
                "dd-MM-yyyy",
                "MM/dd/yyyy",
                "MM-dd-yyyy"
            };

            return DateTime.TryParseExact(
                       valor,
                       formatos,
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out fecha)
                   || DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
        }

        private void Ah_ExcedentesParametros_RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
