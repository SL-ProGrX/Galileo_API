using Galileo.DataBaseTier.ProGrX.Credito;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.BusinessLogic.ProGrX.Credito
{
    public class FrmCRSeguimientoFirmasBL
    {
        private readonly FrmCRSeguimientoFirmasDB _db;

        public FrmCRSeguimientoFirmasBL(IConfiguration config)
        {
            _db = new FrmCRSeguimientoFirmasDB(config);
        }

        public ErrorDto<List<CRSeguimientoFirmasData>> CR_SeguimientoFirmas_Obtener(int CodEmpresa, int operacion)
        {
            return _db.CR_SeguimientoFirmas_Obtener(CodEmpresa, operacion);
        }

        public ErrorDto CR_SeguimientoFirmas_Guardar(int CodEmpresa, CRSeguimientoFirmasData firmasData)
        {
            return _db.CR_SeguimientoFirmas_Guardar(CodEmpresa, firmasData);
        }
    }
}