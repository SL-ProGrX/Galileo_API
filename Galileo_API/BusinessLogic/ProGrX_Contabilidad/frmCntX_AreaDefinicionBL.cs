using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXAreaDefinicionBL
    {

        private readonly FrmCntXAreaDefinicionDB _db;

        public FrmCntXAreaDefinicionBL(IConfiguration config)
        {
            _db = new FrmCntXAreaDefinicionDB(config);
        }
        public ErrorDto<List<AreaDefinicionDto>> AreaDefinicion_Lista(int codEmpresa, int codigoConta, string order)
            => _db.AreaDefinicion_Lista(codEmpresa, codigoConta, order);
        public ErrorDto<List<TipoCuentaDto>> TiposCuentas_Lista(int codEmpresa, int codigoConta)
            => _db.TiposCuentas_Lista(codEmpresa, codigoConta);
        public ErrorDto<List<CuentaNodoDto>> Cuentas_ListaNodo(int codEmpresa, int codigoConta, string tipoCuenta, string cuentaActual, string nodo)
            => _db.Cuentas_ListaNodo(codEmpresa, codigoConta, tipoCuenta, cuentaActual, nodo);
        public ErrorDto<ExisteDto> AreaCuenta_Existe(int codEmpresa, int codigoConta, string cuentaNodo, int areaActual)
            => _db.AreaCuenta_Existe(codEmpresa, codigoConta, cuentaNodo, areaActual);

    }
}
