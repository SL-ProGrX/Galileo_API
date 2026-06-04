using Galileo.DataBaseTier.ProGrX_Procesos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;

namespace Galileo.BusinessLogic.ProGrX_Procesos
{
    public class FrmCCPlanillaUsuariosBL
    {
        private readonly FrmCCPlanillaUsuariosDB _db;

        public FrmCCPlanillaUsuariosBL(IConfiguration config)
        {
            _db = new FrmCCPlanillaUsuariosDB(config);
        }

        public ErrorDto<List<CCPlanillaListaData>> CC_Planilla_Lista_Obtener(int CodEmpresa, string modo)
        {
            return _db.CC_Planilla_Lista_Obtener(CodEmpresa, modo);
        }

        public ErrorDto<List<CCPlanillaDetalleData>> CC_Planilla_Detalle_Obtener(int CodEmpresa, string modo, string dato)
        {
            return _db.CC_Planilla_Detalle_Obtener(CodEmpresa, modo, dato);
        }

        public ErrorDto CC_Planilla_Aplica(int CodEmpresa, string usuarioSesion, CCPlanillaAplicaRequest req)
        {
            return _db.CC_Planilla_Aplica(CodEmpresa, usuarioSesion, req);
        }

        public ErrorDto CC_Planilla_Todos_Aplica(int CodEmpresa, string usuarioSesion, CCPlanillaTodosRequest req)
        {
            return _db.CC_Planilla_Todos_Aplica(CodEmpresa, usuarioSesion, req);
        }
    }
}