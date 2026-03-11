using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasAnulacionesBl
    {
        private readonly FrmCxCCuentasAnulacionesDb _db;

        public FrmCxCCuentasAnulacionesBl(IConfiguration config)
            => _db = new FrmCxCCuentasAnulacionesDb(config);

        public ErrorDto<CxcOperacionAnulacionData?> CxcOperacion_Obtener(int codEmpresa, int operacion)
        {
            return _db.CxcOperacion_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<List<CxcOperacionMovimientoData>> CxcOperacionMovimientos_Lista_Obtener(int codEmpresa, int operacion)
        {
            return _db.CxcOperacionMovimientos_Lista_Obtener(codEmpresa, operacion);
        }

        public ErrorDto CxcCuentasAbono_Anular(int codEmpresa, CxcAbonoAnularParams req)
        {
            return  _db.CxcCuentasAbono_Anular(codEmpresa, req);
        }
    }
}
