using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivRegistroAvaluoDb
    {
        private readonly PortalDB _portalDb;

        public FrmVivRegistroAvaluoDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el detalle funcional del formulario frmVivRegistroAvaluo.
        /// Replica fxTraerOperacionXIdGarantiaIng de VB6 usando spViv_Garantia_Consulta_Avaluo.
        /// </summary>
        public ErrorDto<FrmVivGarantiaAvaluoRegistroResponse> Viv_GarantiaAvaluo_Obtener(
            int codEmpresa,
            FrmVivGarantiaAvaluoRegistroRequest request)
        {
            const string query = @"
        EXEC dbo.spViv_Garantia_Consulta_Avaluo
            @TipoProfesional,
            @GarantiaId,
            @Operacion;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                new FrmVivGarantiaAvaluoRegistroResponse(),
                new
                {
                    TipoProfesional = "I",
                    GarantiaId = request.id_garantia,
                    Operacion = request.numero_operacion
                }
            );
        }

        /// <summary>
        /// Valida si la garantía ya tiene registro de avalúo definitivo.
        /// Replica fxValidaRegistroAvaluo de VB6.
        /// </summary>
        public ErrorDto<bool> Viv_GarantiaAvaluoRegistrado_Existe(
            int codEmpresa,
            long idGarantia)
        {
            const string query = @"
        SELECT
            CASE
                WHEN G.ValorConstruccion IS NULL THEN CAST(0 AS bit)
                ELSE CAST(1 AS bit)
            END AS existe
        FROM ViviendaGarantia AS G
        WHERE G.IdGarantia = @id_garantia;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                false,
                new
                {
                    id_garantia = idGarantia
                }
            );
        }

        /// <summary>
        /// Actualiza montos individuales del registro de avalúo.
        /// Replica btnIngCambios_Click de VB6 usando spVivAvaluos_Cambios.
        /// </summary>
        public ErrorDto<FrmVivGarantiaAvaluoMontoCambiarResponse> Viv_GarantiaAvaluoMonto_Guardar(
            int codEmpresa,
            FrmVivGarantiaAvaluoMontoCambiarRequest request)
        {
            const string query = @"
        EXEC dbo.spVivAvaluos_Cambios
            @GarantiaId,
            @TipoMov,
            @Monto,
            @Usuario;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                new FrmVivGarantiaAvaluoMontoCambiarResponse(),
                new
                {
                    GarantiaId = request.id_garantia,
                    TipoMov = request.tipo.Trim(),
                    Monto = request.monto,
                    Usuario = request.registro_usuario.Trim()
                }
            );
        }

    }
}
