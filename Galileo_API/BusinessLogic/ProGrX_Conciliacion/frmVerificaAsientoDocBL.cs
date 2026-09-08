using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;

namespace Galileo_API.BusinessLogic.ProGrX.Conciliacion
{
    public class FrmVerificaAsientosDocumentoBL
    {
        private readonly FrmVerificaAsientosDocumentoDB _db;

        public FrmVerificaAsientosDocumentoBL(IConfiguration config)
        {
            _db = new FrmVerificaAsientosDocumentoDB(config);
        }

        public ErrorDto<AseVerificaAsientosDocumentoInicialData> ASE_VerificaAsientosDocumento_Inicial_Obtener(int CodEmpresa)
        {
            return _db.ASE_VerificaAsientosDocumento_Inicial_Obtener(CodEmpresa);
        }

        public ErrorDto<AseVerificaAsientosDocumentoListaResult> ASE_VerificaAsientosDocumento_Lista_Obtener(int CodEmpresa, AseVerificaAsientosDocumentoListaRequest? request)
        {
            return _db.ASE_VerificaAsientosDocumento_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<AseVerificaAsientosDocumentoListaResult> ASE_VerificaAsientosDocumento_Lista_Export(int CodEmpresa, AseVerificaAsientosDocumentoListaRequest? request)
        {
            return _db.ASE_VerificaAsientosDocumento_Lista_Export(CodEmpresa, request);
        }
    }
}