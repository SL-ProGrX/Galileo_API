using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;

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

        public ErrorDto Crd_CuotaMantenimiento_Ejecutar(CcCuotaMantenimientoEjecutarRequest request)
                     => _db.Crd_CuotaMantenimiento_Ejecutar(request);

        public int Crd_CuotaMantenimiento_Derecho_Obtener(int codEmpresa, string usuario)
                => _db.CmdAplicar_Derecho_Obtener(codEmpresa, usuario);
        
    }
}
