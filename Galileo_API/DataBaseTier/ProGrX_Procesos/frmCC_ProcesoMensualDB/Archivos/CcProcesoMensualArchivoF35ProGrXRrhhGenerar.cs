
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF35ProGrXRrhhGenerar : CcProcesoMensualArchivoCadenaSpGeneratorBase
    {
        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["35"];


        public CcProcesoMensualArchivoF35ProGrXRrhhGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        {
        }

        protected override string CodigoPlanillaEnvio => "35";
        protected override string CodigoFormato => "F35";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        protected override string QueryCadenas => @"
            EXEC spPrm_Formato_ProGrX_RRHH
                @CodInstitucion,
                @FechaProceso";
    }
}
