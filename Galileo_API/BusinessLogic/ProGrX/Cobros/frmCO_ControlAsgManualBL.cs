using Galileo.DataBaseTier.ProGrX_Cobros;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Cobros;

namespace Galileo.BusinessLogic.ProGrX_Cobros
{
    public class FrmCOControlAsgManualBL
    {
        private readonly FrmCOControlAsgManualDB _db;

        public FrmCOControlAsgManualBL(IConfiguration config)
        {
            _db = new FrmCOControlAsgManualDB(config);
        }
        public ErrorDto<List<CoControlAsgManualExpedienteItem>> Co_ControlAsgManual_Expedientes_Obtener(int CodEmpresa, int soloSinAsignar,int soloMorosos)
        {
            return _db.Co_ControlAsgManual_Expedientes_Obtener(CodEmpresa, soloSinAsignar, soloMorosos);
        }
        public ErrorDto<CoControlAsgManualExpedienteDetalle> Co_ControlAsgManual_Expediente_Detalle_Obtener(int CodEmpresa,string cedula)
        {
            return _db.Co_ControlAsgManual_Expediente_Detalle_Obtener(CodEmpresa, cedula);
        }
        public ErrorDto<List<CoControlAsgManualUsuarioItem>> Co_ControlAsgManual_Usuarios_Obtener(int CodEmpresa)
        {
            return _db.Co_ControlAsgManual_Usuarios_Obtener(CodEmpresa);
        }
        public ErrorDto Co_ControlAsgManual_Aplicar(int CodEmpresa,string usuario,CoControlAsgManualAplicarRequest data)
        {
            return _db.Co_ControlAsgManual_Aplicar(CodEmpresa, usuario, data);
        }
    }
}
