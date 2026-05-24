using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF36AsoInsvaGenerar : CcProcesoMensualArchivoCadenaSpGeneratorBase
    {
        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["36"];

        protected override string CodigoPlanillaEnvio => "36";
        protected override string CodigoFormato => "F36";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        protected override string QueryCadenas => @"
            EXEC spPrm_Formato_INSVA
                @CodInstitucion,
                @FechaProceso";
    }
}
