using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoComisionesDocumentosBL
    {

        private readonly FrmCoComisionesDocumentosDB _db;

        public FrmCoComisionesDocumentosBL(IConfiguration config)
        {
            _db = new FrmCoComisionesDocumentosDB(config);
        }

        public ErrorDto<List<CoComisionesDocumentosData>> CO_ComisionesDocumento_Obtener(int CodEmpresa)
        { 
            return _db.CO_ComisionesDocumento_Obtener(CodEmpresa);
        }
        public ErrorDto CO_ComisionesDocumento_Insertar(int CodEmpresa, string usuario, string tipo_documento)
        {
            return _db.CO_ComisionesDocumento_Insertar(CodEmpresa, usuario, tipo_documento);
        }
        public ErrorDto CO_ComisionesDocumento_Delete(int CodEmpresa, string usuario, string tipo_documento)
        {
            return _db.CO_ComisionesDocumento_Delete(CodEmpresa, usuario, tipo_documento);
        }
    }
}
