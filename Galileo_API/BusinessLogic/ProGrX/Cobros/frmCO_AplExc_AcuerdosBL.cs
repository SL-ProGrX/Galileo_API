using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplExcAcuerdosBl
    {
        private readonly FrmCoAplExcAcuerdosDb _db;

        public FrmCoAplExcAcuerdosBl(IConfiguration config)
            => _db = new FrmCoAplExcAcuerdosDb(config);

        public ErrorDto<CoAplExcAcuerdosData?> CoAplExcAcuerdos_Obtener(int codEmpresa, int idAcuerdo)
        {
            return _db.CoAplExcAcuerdos_Obtener(codEmpresa, idAcuerdo);
        }

        public ErrorDto<List<CoAplExcAcuerdosData>> CoAplExcAcuerdos_Lista_Obtener(int codEmpresa, string filtro, string estado)
        {
            return _db.CoAplExcAcuerdos_Lista_Obtener(codEmpresa, filtro, estado);
        }

        public ErrorDto CoAplExcAcuerdos_Guardar(int codEmpresa, CoAplExcAcuerdosData request)
        {
            return _db.CoAplExcAcuerdos_Guardar(codEmpresa, request);
        }
    }
}
