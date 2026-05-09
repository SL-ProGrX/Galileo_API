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
        public ErrorDto<ExisteDto?> AreaCuenta_Existe(int codEmpresa, int codigoConta, string cuentaNodo, int areaActual)
            => _db.AreaCuenta_Existe(codEmpresa, codigoConta, cuentaNodo, areaActual);
        public ErrorDto<bool> Area_Eliminar(int codEmpresa, int codigoConta, int areaActual)
            => _db.Area_Eliminar(codEmpresa, codigoConta, areaActual);
        public ErrorDto<bool> AreaCuenta_Insertar(int codEmpresa, int codigoConta, int areaActual, string cuentaMarcada)
            => _db.AreaCuenta_Insertar(codEmpresa, codigoConta, areaActual, cuentaMarcada);
        public ErrorDto<List<AreaCuentaDetalleDto>> AreaCuenta_DetalleLista(int codEmpresa, int codigoConta, int areaActual)
            => _db.AreaCuenta_DetalleLista(codEmpresa, codigoConta, areaActual);

        public ErrorDto<ExisteDto?> AreaCuenta_ExistePorCuenta(int codEmpresa, int codigoConta, string codCuenta, int areaActual)
            => _db.AreaCuenta_ExistePorCuenta(codEmpresa, codigoConta, codCuenta, areaActual);

        public ErrorDto<bool> AreaCuenta_InsertarMadre(int codEmpresa, int codigoConta, int areaActual, string cuentaMadre)
            => _db.AreaCuenta_InsertarMadre(codEmpresa, codigoConta, areaActual, cuentaMadre);

        public ErrorDto<int> AreaDefinicion_Insertar(int codEmpresa, int codigoConta, string nombreArea, bool chkActiva, string usuario)
            => _db.AreaDefinicion_Insertar(codEmpresa, codigoConta, nombreArea, chkActiva, usuario);

        public ErrorDto<List<UnidadDto>> Unidades_Lista(int codEmpresa, int codigoConta)
            => _db.Unidades_Lista(codEmpresa, codigoConta);

        public ErrorDto<List<CentroCostoDto>> CentroCostos_ListaPorUnidad(int codEmpresa, int codigoConta, string unidadActual)
            => _db.CentroCostos_ListaPorUnidad(codEmpresa, codigoConta, unidadActual);

        public ErrorDto<bool> AreaDefinicion_Eliminar(int codEmpresa, int codigoConta, int areaActual)
            => _db.AreaDefinicion_Eliminar(codEmpresa, codigoConta, areaActual);

    }
}
