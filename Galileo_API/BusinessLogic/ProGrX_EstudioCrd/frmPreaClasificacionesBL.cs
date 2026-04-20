using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaClasificacionesBl
    {
        private readonly FrmPreaClasificacionesDb _db;

        public FrmPreaClasificacionesBl(IConfiguration config)
            => _db = new FrmPreaClasificacionesDb(config);

        public ErrorDto<List<PreaClasificacionRazonData>> PreaClasificacion_Razones_Obtener(int codEmpresa)
        {
            return _db.PreaClasificacion_Razones_Obtener(codEmpresa);
        }

        public ErrorDto<List<PreaClasificacionData>> PreaClasificacion_Catalogo_Obtener(int codEmpresa, string catalogo)
        {
            return _db.PreaClasificacion_Catalogo_Obtener(codEmpresa, catalogo);
        }

        public ErrorDto PreaClasificacion_Razon_Guardar(int codEmpresa, string usuario, PreaClasificacionRazonData request)
        {
            return _db.PreaClasificacion_Razon_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto PreaClasificacion_Razon_Eliminar(int codEmpresa, string codRazon, string usuario)
        {
            return _db.PreaClasificacion_Razon_Eliminar(codEmpresa, codRazon, usuario);
        }
    }
}
