using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhConfiguracionDB
    {
        /// <summary>
        /// Guarda la configuración de cuentas contables del formulario frmAH_Configuracion.
        /// </summary>
        public ErrorDto AH_Configuracion_Parametros_Guardar(int codEmpresa, AhConfiguracionGuardarRequest request, string usuario)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("Datos requeridos.", -2);
            }

            var codDivisa = AH_Configuracion_NormalizarTexto(request.cod_divisa);
            var usuarioNorm = AH_Configuracion_NormalizarTexto(usuario).ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(codDivisa))
            {
                return DbHelper.ErrorResponse("La divisa es requerida.", -2);
            }

            if (string.IsNullOrWhiteSpace(usuarioNorm))
            {
                return DbHelper.ErrorResponse("El usuario es requerido.", -2);
            }

            var validacion = AH_Configuracion_ValidarRequest(codEmpresa, request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
update par_afah
set cta_obrero           = @cta_obrero,
    cta_patronal         = @cta_patronal,
    cta_extra            = @cta_extra,
    cta_capitaliza       = @cta_capitaliza,
    cta_custodia         = @cta_custodia,
    cta_devoluciones     = @cta_devoluciones,
    cta_excdist          = @cta_excdist,
    cta_excpagar         = @cta_excpagar,
    cta_excajustepagar   = @cta_excajustepagar,
    cta_excajustecobrar  = @cta_excajustecobrar,
    cta_excnc            = @cta_excnc,
    cta_excdonacion      = @cta_excdonacion,
    cta_renta            = @cta_renta,
    cta_liqpas           = @cta_liqpas,
    cta_rentacap         = @cta_rentacap,
    cta_exc_reserva      = @cta_exc_reserva
where cod_divisa = @cod_divisa;";

                var rows = conn.Execute(sql, new
                {
                    cod_divisa = codDivisa,
                    cta_obrero = AH_Configuracion_FormatearCuenta(codEmpresa, request.patrimonio.cta_obrero),
                    cta_patronal = AH_Configuracion_FormatearCuenta(codEmpresa, request.patrimonio.cta_patronal),
                    cta_extra = AH_Configuracion_FormatearCuenta(codEmpresa, request.patrimonio.cta_extra),
                    cta_capitaliza = AH_Configuracion_FormatearCuenta(codEmpresa, request.patrimonio.cta_capitaliza),
                    cta_custodia = AH_Configuracion_FormatearCuenta(codEmpresa, request.patrimonio.cta_custodia),
                    cta_devoluciones = AH_Configuracion_FormatearCuenta(codEmpresa, request.patrimonio.cta_devoluciones),
                    cta_excdist = AH_Configuracion_FormatearCuenta(codEmpresa, request.excedentes.cta_excdist),
                    cta_excpagar = AH_Configuracion_FormatearCuenta(codEmpresa, request.excedentes.cta_excpagar),
                    cta_excajustepagar = AH_Configuracion_FormatearCuenta(codEmpresa, request.excedentes.cta_excajustepagar),
                    cta_excajustecobrar = AH_Configuracion_FormatearCuenta(codEmpresa, request.excedentes.cta_excajustecobrar),
                    cta_excnc = AH_Configuracion_FormatearCuenta(codEmpresa, request.excedentes.cta_excnc),
                    cta_excdonacion = AH_Configuracion_FormatearCuenta(codEmpresa, request.excedentes.cta_excdonacion),
                    cta_renta = AH_Configuracion_FormatearCuenta(codEmpresa, request.excedentes.cta_renta),
                    cta_liqpas = AH_Configuracion_FormatearCuenta(codEmpresa, request.patrimonio.cta_liqpas),
                    cta_rentacap = AH_Configuracion_FormatearCuenta(codEmpresa, request.patrimonio.cta_rentacap),
                    cta_exc_reserva = AH_Configuracion_FormatearCuenta(codEmpresa, request.excedentes.cta_exc_reserva)
                });

                if (rows <= 0)
                {
                    return DbHelper.ErrorResponse("No se encontró registro para actualizar la divisa indicada.", -2);
                }

                _dbBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuarioNorm,
                    Modulo = vModulo,
                    Movimiento = MovimientoModifica,
                    DetalleMovimiento = $"PAT Configuración Divisa: {codDivisa}"
                });

                return DbHelper.OkResponse( "La información se guardó satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
