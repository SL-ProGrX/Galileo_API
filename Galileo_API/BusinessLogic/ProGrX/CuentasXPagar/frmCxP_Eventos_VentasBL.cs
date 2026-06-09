using Galileo.DataBaseTier.ProGrX.CuentasXPagar;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPEventosVentasBL
    {
        private readonly FrmCxPEventosVentasDB _db;

        public FrmCxPEventosVentasBL(IConfiguration config)
        {
            _db = new FrmCxPEventosVentasDB(config);
        }

        public ErrorDto<List<CxpEventosDto>> Eventos_Obtener(int CodEmpresa)
        {
            return _db.Eventos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CxpEventosVentasDto>> Eventos_Ventas_Obtener(int CodEmpresa, string parametros)
        {
            return _db.Eventos_Ventas_Obtener(CodEmpresa, parametros);
        }
    }
}