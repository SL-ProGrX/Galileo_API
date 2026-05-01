using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOIncobrablesListBL
    {
        private readonly FrmCOIncobrablesListDB _db;

        public FrmCOIncobrablesListBL(IConfiguration config)
        {
            _db = new FrmCOIncobrablesListDB(config);
        }

        public ErrorDto<string> Nombre_Obtener(int codEmpresa, string cedula)
        {
            return _db.Nombre_Obtener(codEmpresa, cedula);
        }

        public ErrorDto<List<CbrIncobrableListaItem>> CoIncobrablesList_Obtener(int codEmpresa, string cedula)
        {
            return _db.CoIncobrablesList_Obtener(codEmpresa, cedula);
        }

        public ErrorDto<List<CbrIncobrableMovimientoItem>> CoIncobrablesListMovimientos_Obtener(
            int codEmpresa,
            int operacion,
            int cxcOperacion)
        {
            return _db.CoIncobrablesListMovimientos_Obtener(codEmpresa, operacion, cxcOperacion);
        }

    }

}