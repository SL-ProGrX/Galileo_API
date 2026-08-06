using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRSeguimientoRegTagBL
    {
        private readonly FrmCRSeguimientoRegTagDB _db;

        public FrmCRSeguimientoRegTagBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmCRSeguimientoRegTagDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_SeguimientoRegTag_Etiquetas_Obtener(
            int codEmpresa, string usuario)
        {
            return _db.CR_SeguimientoRegTag_Etiquetas_Obtener(codEmpresa, usuario);
        }

        public ErrorDto<List<CrSeguimientoRegTagOperacionDto>> CR_SeguimientoRegTag_Operaciones_Obtener(
            int codEmpresa, CrSeguimientoRegTagConsultaRequest request)
        {
            return _db.CR_SeguimientoRegTag_Operaciones_Obtener(codEmpresa, request);
        }

        public ErrorDto CR_SeguimientoRegTag_Aplicar(
            int codEmpresa, CrSeguimientoRegTagAplicarRequest request)
        {
            return _db.CR_SeguimientoRegTag_Aplicar(codEmpresa, request);
        }
    }
}
