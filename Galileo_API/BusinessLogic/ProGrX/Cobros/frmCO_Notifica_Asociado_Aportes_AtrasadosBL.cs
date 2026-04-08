using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCONotificaAsociadoAportesAtrasadosBL
    {
        private readonly FrmCONotificaAsociadoAportesAtrasadosDB Db;

        public FrmCONotificaAsociadoAportesAtrasadosBL(IConfiguration config)
        {
            Db = new FrmCONotificaAsociadoAportesAtrasadosDB(config);
        }

        public ErrorDto<CoNotificaAsociadoAportesAtrasadosListaResult> CO_Notifica_Asociado_Aportes_Atrasados_Lista_Obtener(
            int CodEmpresa,
            string? cedula)
        {
            return Db.CO_Notifica_Asociado_Aportes_Atrasados_Lista_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto<CoNotificaAsociadoAportesAtrasadosListaResult> CO_Notifica_Asociado_Aportes_Atrasados_Lista_Export(
            int CodEmpresa,
            string? cedula)
        {
            return Db.CO_Notifica_Asociado_Aportes_Atrasados_Lista_Export(CodEmpresa, cedula);
        }

        public ErrorDto CO_Notifica_Asociado_Aportes_Atrasados_Enviar(
            int CodEmpresa,
            CoNotificaAsociadoAportesAtrasadosEnviarRequest req)
        {
            return Db.CO_Notifica_Asociado_Aportes_Atrasados_Enviar(CodEmpresa, req);
        }
    }
}