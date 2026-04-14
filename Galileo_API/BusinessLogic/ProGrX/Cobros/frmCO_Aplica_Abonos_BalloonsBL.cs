using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOAplicaAbonosBalloonsBL
    {
        private readonly FrmCOAplicaAbonosBalloonsDB _db;

        public FrmCOAplicaAbonosBalloonsBL(IConfiguration config)
        {
            _db = new FrmCOAplicaAbonosBalloonsDB(config);
        }
        public ErrorDto<CoAplicaAbonosBalloonsListaResult> CO_Aplica_Abonos_Balloons_Lista_Obtener(int CodEmpresa,string parametros)
        {
            return _db.CO_Aplica_Abonos_Balloons_Lista_Obtener(CodEmpresa, parametros);
        }
        public ErrorDto<CoAplicaAbonosBalloonsListaResult> CO_Aplica_Abonos_Balloons_Lista_Export(int CodEmpresa,string parametros)
        {
            return _db.CO_Aplica_Abonos_Balloons_Lista_Export(CodEmpresa, parametros);
        }
        public ErrorDto<CoAplicaAbonosBalloonsAplicarResult> CO_Aplica_Abonos_Balloons_Aplicar(int CodEmpresa,CoAplicaAbonosBalloonsAplicarRequest? req)
        {
            return _db.CO_Aplica_Abonos_Balloons_Aplicar(CodEmpresa, req);
        }
    }
}