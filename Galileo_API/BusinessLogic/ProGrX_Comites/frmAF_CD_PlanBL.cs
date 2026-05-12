using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdPlanBl
    {
        private readonly FrmAfCdPlanDb _db;

        public FrmAfCdPlanBl(IConfiguration config) =>
            _db = new FrmAfCdPlanDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
        {
            return _db.AfCdComites_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<List<AfCdPlanMensajeData>> AfCdPlanMensajes_Lista_Obtener(int codEmpresa, string codComite)
        {
            return _db.AfCdPlanMensajes_Lista_Obtener(codEmpresa, codComite);
        }

        public ErrorDto AfCdPlanMensaje_Guardar(int codEmpresa, AfCdPlanMensajeData request)
        {
            return _db.AfCdPlanMensaje_Guardar(codEmpresa, request);
        }

        public ErrorDto AfCdPlanMensajes_Eliminar(int codEmpresa, string codComite, int numMensaje)
        {
            return _db.AfCdPlanMensajes_Eliminar(codEmpresa, codComite, numMensaje);
        }
    }
}
