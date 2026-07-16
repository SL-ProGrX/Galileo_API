using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Dapper;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCtaCatalogoDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmCrCtaCatalogoDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        public ErrorDto<CrCtaCatalogoCuenta?> CrCtaCatalogo_Cuentas_Obtener(int codEmpresa, string codigo)
        {
            codigo = (codigo ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<CrCtaCatalogoCuenta?>
                {
                    Code = -1,
                    Description = "Debe indicar el código de línea.",
                    Result = null
                };
            }

            const string sqlQuery = @"
                SELECT
                    CODIGO AS Codigo,
                    DESCRIPCION AS Descripcion,
                    IMPUESTO AS Impuesto,
                    ctaNIntc AS CtaNIntc,
                    ctaNIntc_Mask AS CtaNIntc_Mask,
                    ctaNIntc_Desc AS CtaNIntc_Desc,
                    ctaNIntm AS CtaNIntm,
                    ctaNIntm_Mask AS CtaNIntm_Mask,
                    ctaNIntm_Desc AS CtaNIntm_Desc,
                    ctaNAmort AS CtaNAmort,
                    ctaNAmort_Mask AS CtaNAmort_Mask,
                    ctaNAmort_Desc AS CtaNAmort_Desc,
                    ctaOIntc AS CtaOIntc,
                    ctaOIntc_Mask AS CtaOIntc_Mask,
                    ctaOIntc_Desc AS CtaOIntc_Desc,
                    ctaOIntm AS CtaOIntm,
                    ctaOIntm_Mask AS CtaOIntm_Mask,
                    ctaOIntm_Desc AS CtaOIntm_Desc,
                    ctaOAmort AS CtaOAmort,
                    ctaOAmort_Mask AS CtaOAmort_Mask,
                    ctaOAmort_Desc AS CtaOAmort_Desc,
                    ctaCIntc AS CtaCIntc,
                    ctaCIntc_Mask AS CtaCIntc_Mask,
                    ctaCIntc_Desc AS CtaCIntc_Desc,
                    ctaCIntm AS CtaCIntm,
                    ctaCIntm_Mask AS CtaCIntm_Mask,
                    ctaCIntm_Desc AS CtaCIntm_Desc,
                    ctaCAmort AS CtaCAmort,
                    ctaCAmort_Mask AS CtaCAmort_Mask,
                    ctaCAmort_Desc AS CtaCAmort_Desc,
                    ctaPuente AS CtaPuente,
                    ctaPuente_Mask AS CtaPuente_Mask,
                    ctaPuente_Desc AS CtaPuente_Desc,
                    CTA_CAR_PRODUCTO AS CtaCarProducto,
                    CTA_CAR_PRODUCTO_Mask AS CtaCarProducto_Mask,
                    CTA_CAR_PRODUCTO_Desc AS CtaCarProducto_Desc,
                    CTA_PROD_ACUM AS CtaProdAcum,
                    CTA_PROD_ACUM_Mask AS CtaProdAcum_Mask,
                    CTA_PROD_ACUM_Desc AS CtaProdAcum_Desc,
                    CTA_INT_ADELANTADO AS CtaIntAdelantado,
                    CTA_INT_ADELANTADO_Mask AS CtaIntAdelantado_Mask,
                    CTA_INT_ADELANTADO_Desc AS CtaIntAdelantado_Desc,
                    CTA_PS_DEUDORA AS CtaPsDeudora,
                    CTA_PS_DEUDORA_Mask AS CtaPsDeudora_Mask,
                    CTA_PS_DEUDORA_Desc AS CtaPsDeudora_Desc,
                    CTA_PS_ACREADORA AS CtaPsAcreadora,
                    CTA_PS_ACREADORA_Mask AS CtaPsAcreadora_Mask,
                    CTA_PS_ACREADORA_Desc AS CtaPsAcreadora_Desc,
                    PS_REGISTRA AS PsRegistra,
                    CTA_CARGOS_ANTICIPO AS CtaCargosAnticipo,
                    CTA_CARGOS_ANTICIPO_Mask AS CtaCargosAnticipo_Mask,
                    CTA_CARGOS_ANTICIPO_Desc AS CtaCargosAnticipo_Desc,
                    CTAIVA AS CtaIva,
                    CTA_IVA_Mask AS CtaIva_Mask,
                    CTA_IVA_Desc AS CtaIva_Desc
                FROM vCrd_Catalogo_Cuentas
                WHERE codigo = @Codigo;";

            return DbHelper.ExecuteSingleQuery<CrCtaCatalogoCuenta>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                null,
                new { Codigo = codigo });
        }

        public ErrorDto CrCtaCatalogo_Cuentas_Guardar(int codEmpresa, CrCtaCatalogoCuentasGuardarRequest request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("La solicitud es requerida.");
            }

            request.Codigo = (request.Codigo ?? string.Empty).Trim().ToUpperInvariant();
            request.Usuario = (request.Usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.Codigo))
            {
                return DbHelper.ErrorResponse("Debe indicar el código de línea.");
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.");
            }

            var spResponse = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<CrCtaCatalogoCuentasGuardarResultado>(
                    "spCrd_Catalogo_Cuentas_Update",
                    new
                    {
                        request.Codigo,
                        request.CtaNIntCor,
                        request.CtaNIntMor,
                        request.CtaNPrincipal,
                        request.CtaOIntCor,
                        request.CtaOIntMor,
                        request.CtaOPrincipal,
                        request.CtaCIntCor,
                        request.CtaCIntMor,
                        request.CtaCPrincipal,
                        request.CtaPuente,
                        request.CtaPagoAnticipado,
                        CtaIVA = request.CtaIva,
                        I_IVA = request.IIva,
                        request.CtaIntCobAdelantado,
                        CtaPA_Efectos = request.CtaPaEfectos,
                        CtaPA_Cartera = request.CtaPaCartera,
                        I_PA_Suspenso = request.IPaSuspenso,
                        CtaPS_Deudora = request.CtaPsDeudora,
                        CtaPS_Acreedora = request.CtaPsAcreedora,
                        request.Usuario
                    },
                    commandType: CommandType.StoredProcedure));

            if (spResponse.Code < 0)
            {
                return DbHelper.ErrorResponse(
                    spResponse.Description ?? "Error al actualizar cuentas de catálogo.",
                    spResponse.Code.GetValueOrDefault(-1));
            }

            var resultado = spResponse.Result;
            if (resultado is null)
            {
                return DbHelper.ErrorResponse("No se obtuvo respuesta del proceso de actualización.");
            }

            if (resultado.Aplica != 1)
            {
                return DbHelper.ErrorResponse(string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? "No fue posible actualizar las cuentas de catálogo."
                    : resultado.Mensaje);
            }

            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = request.Usuario,
                DetalleMovimiento = $"Actualiza cuentas en catalogo:{request.Codigo}",
                Movimiento = "Modifica - Web",
                Modulo = vModulo
            });

            return DbHelper.OkResponse("Información guardada satisfactoriamente...");
        }
    }
}
