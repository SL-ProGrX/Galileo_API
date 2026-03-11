using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Procesos;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos
{
    public class FrmCcCuotaMantenimientoBL
    {
        private readonly FrmCcCuotaMantenimientoDB _db;

        public FrmCcCuotaMantenimientoBL(IConfiguration config)
        {
            _db = new FrmCcCuotaMantenimientoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_CuotaMantenimiento_Instituciones_Obtener(int codEmpresa)
                => _db.Crd_CuotaMantenimiento_Instituciones_Obtener(codEmpresa);

        public ErrorDto Crd_CuotaMantenimiento_Ejecutar(int codEmpresa, string usuario, int codContabilidad, int codInstitucion)
                     => _db.Crd_CuotaMantenimiento_Ejecutar(codEmpresa, usuario, codContabilidad, codInstitucion);

    }
}
