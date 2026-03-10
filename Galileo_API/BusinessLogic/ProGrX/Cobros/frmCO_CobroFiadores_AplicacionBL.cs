using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoCobroFiadoresAplicacionBL
    {
        private readonly FrmCoCobroFiadoresAplicacionDB _db;

        public FrmCoCobroFiadoresAplicacionBL(IConfiguration config)
        {
            _db = new FrmCoCobroFiadoresAplicacionDB(config);
        }
        public ErrorDto<CoCobroFiadoresAplicacionPendientesDto> CO_CobroFiadores_Aplicacion_Pendientes_Obtener(int CodEmpresa)
        {
            return _db.CO_CobroFiadores_Aplicacion_Pendientes_Obtener(CodEmpresa);
        }
        public ErrorDto<CoCobroFiadoresAplicacionProcesarResponse> CO_CobroFiadores_Aplicacion_Procesar(int CodEmpresa,CoCobroFiadoresAplicacionProcesarRequest data)
        {
            return _db.CO_CobroFiadores_Aplicacion_Procesar(CodEmpresa, data);
        }
        public ErrorDto CO_CobroFiadores_Aplicacion_Cancelar(int CodEmpresa,CoCobroFiadoresAplicacionCancelarRequest data)
        {
            return _db.CO_CobroFiadores_Aplicacion_Cancelar(CodEmpresa, data);
        }
    }
}