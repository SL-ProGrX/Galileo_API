using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using System.Linq;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhConfiguracionDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private readonly MCntLinkDB _mCntLinkDb;
        private const int vModulo = 2;
        private const string MovimientoModifica = "MODIFICA-WEB";

        public FrmAhConfiguracionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
            _mCntLinkDb = new MCntLinkDB(config);
        }


        private static string AH_Configuracion_NormalizarTexto(string? value)
            => (value ?? string.Empty).Trim();

        private string AH_Configuracion_FormatearCuenta(int codEmpresa, string? cuenta)
        {
            var cuentaNormalizada = AH_Configuracion_NormalizarTexto(cuenta);
            return string.IsNullOrWhiteSpace(cuentaNormalizada)
                ? string.Empty
                : _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, false, cuentaNormalizada, 0);
        }

        private bool AH_Configuracion_CuentaEsValida(int codEmpresa, string? cuenta)
        {
            var cuentaNormalizada = AH_Configuracion_NormalizarTexto(cuenta);
            return !string.IsNullOrWhiteSpace(cuentaNormalizada)
                && _mCntLinkDb.fxgCntCuentaValida(codEmpresa, cuentaNormalizada);
        }

        private ErrorDto AH_Configuracion_ValidarRequest(int codEmpresa, AhConfiguracionGuardarRequest request)
        {
            if (request.patrimonio == null)
            {
                return DbHelper.ErrorResponse("La sección de patrimonio es requerida.", -2);
            }

            if (request.excedentes == null)
            {
                return DbHelper.ErrorResponse("La sección de excedentes es requerida.", -2);
            }

            var cuentasPatrimonio = new Dictionary<string, string?>
            {
                ["cta_obrero"] = request.patrimonio.cta_obrero,
                ["cta_patronal"] = request.patrimonio.cta_patronal,
                ["cta_extra"] = request.patrimonio.cta_extra,
                ["cta_capitaliza"] = request.patrimonio.cta_capitaliza,
                ["cta_custodia"] = request.patrimonio.cta_custodia,
                ["cta_devoluciones"] = request.patrimonio.cta_devoluciones,
                ["cta_liqpas"] = request.patrimonio.cta_liqpas,
                ["cta_rentacap"] = request.patrimonio.cta_rentacap
            };

            var cuentaPatrimonioInvalida = cuentasPatrimonio
                .Where(item => !AH_Configuracion_CuentaEsValida(codEmpresa, item.Value))
                .FirstOrDefault();

            if (cuentaPatrimonioInvalida.Key != null)
            {
                return DbHelper.ErrorResponse($"La cuenta {cuentaPatrimonioInvalida.Key} no es válida.", -2);
            }

            if (request.excedentes_cfg == 1)
            {
                var cuentasExcedentes = new Dictionary<string, string?>
                {
                    ["cta_renta"] = request.excedentes.cta_renta,
                    ["cta_excdist"] = request.excedentes.cta_excdist,
                    ["cta_excpagar"] = request.excedentes.cta_excpagar,
                    ["cta_excnc"] = request.excedentes.cta_excnc,
                    ["cta_excajustecobrar"] = request.excedentes.cta_excajustecobrar,
                    ["cta_excajustepagar"] = request.excedentes.cta_excajustepagar,
                    ["cta_excdonacion"] = request.excedentes.cta_excdonacion,
                    ["cta_exc_reserva"] = request.excedentes.cta_exc_reserva
                };

                var cuentaExcedenteInvalida = cuentasExcedentes
                    .Where(item => !AH_Configuracion_CuentaEsValida(codEmpresa, item.Value))
                    .FirstOrDefault();

                if (cuentaExcedenteInvalida.Key != null)
                {
                    return DbHelper.ErrorResponse($"La cuenta {cuentaExcedenteInvalida.Key} no es válida.", -2);
                }
            }

            return DbHelper.CreateOkResponse();
        }
    }
}
