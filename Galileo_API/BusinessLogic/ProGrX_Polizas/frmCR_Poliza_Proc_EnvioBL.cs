using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizaProcEnvioBl
    {
        private readonly FrmCrPolizaProcEnvioDb _db;

        public FrmCrPolizaProcEnvioBl(IConfiguration config)
        {
            _db = new FrmCrPolizaProcEnvioDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasProcEnvio_Catalogo_Obtener(int CodEmpresa)
        {
            return _db.Crd_PolizasProcEnvio_Catalogo_Obtener(CodEmpresa);
        }

        public ErrorDto<CrdPolizaGridMetaResponseDto> Crd_PolizasProcEnvio_GridMeta_Obtener(int CodEmpresa, CrdPolizaGridMetaRequestDto req)
        {
            return _db.Crd_PolizasProcEnvio_GridMeta_Obtener(CodEmpresa, req);
        }

        public ErrorDto<CrdPolizaConsultaResponseDto> Crd_PolizasProcEnvio_Consultar(
           int CodEmpresa,
           CrdPolizaConsultaRequestDto req)
        {
            return _db.Crd_PolizasProcEnvio_Consultar(CodEmpresa, req);
        }
    }
}
