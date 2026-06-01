using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhExcedentesRentaTablaBL
    {
        private readonly FrmAhExcedentesRentaTablaDB _db;

        public FrmAhExcedentesRentaTablaBL(IConfiguration config)
        {
            _db = new FrmAhExcedentesRentaTablaDB(config);
        }

        public ErrorDto<List<RentaExcedenteDto>> AH_ExcedentesRentaTabla_Obtener(int codEmpresa)
        {
            return _db.AH_ExcedentesRentaTabla_Obtener(codEmpresa);
        }

        public ErrorDto AH_ExcedentesRentaTabla_Guardar(int codEmpresa, string usuario, RentaExcedenteDto request)
        {
            return _db.AH_ExcedentesRentaTabla_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto AH_ExcedentesRentaTabla_Eliminar(int codEmpresa, int idRenta, string usuario)
        {
            return _db.AH_ExcedentesRentaTabla_Eliminar(codEmpresa, idRenta, usuario);
        }
    }
}
