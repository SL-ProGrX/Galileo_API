using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.General;
using Galileo_API.Models.ProGrX.General;

namespace Galileo_API.BusinessLogic.ProGrX.General
{
    public class FrmCcAnomaliasBL
    {
        private readonly FrmCcAnomaliasDB _db;

        public FrmCcAnomaliasBL(IConfiguration config)
        {
            _db = new FrmCcAnomaliasDB(config);
        }

        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasSaldosMenores_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            return _db.CcAnomaliasSaldosMenores_Obtener(codEmpresa, filtro);
        }

        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasSaldosNegativos_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            return _db.CcAnomaliasSaldosNegativos_Obtener(codEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasCreditos_Obtener(int codEmpresa)
        {
            return _db.CcAnomaliasCreditos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasDestinos_Obtener(int codEmpresa)
        {
            return _db.CcAnomaliasDestinos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasInstituciones_Obtener(int codEmpresa)
        {
            return _db.CcAnomaliasInstituciones_Obtener(codEmpresa);
        }

        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasMoraMenor_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            return _db.CcAnomaliasMoraMenor_Obtener(codEmpresa, filtro);
        }

        public ErrorDto<List<CcAnomaliaCtaDerivadaItemDto>> CcAnomaliasCtaDerivadaMenor_Obtener(int codEmpresa, CcAnomaliaCtaDerivadaFiltroDto filtro)
        {
            return _db.CcAnomaliasCtaDerivadaMenor_Obtener(codEmpresa, filtro);
        }
    }
}
