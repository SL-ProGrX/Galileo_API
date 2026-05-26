using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public interface ICcProcesoMensualArchivoGenerator
    {
        IReadOnlyCollection<string> CodigosPlanillaEnvio { get; }

        CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request);
    }
}
