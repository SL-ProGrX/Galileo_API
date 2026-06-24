using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvParametrosBL
    {
        private readonly FrmInvParametrosDB _db;

        public FrmInvParametrosBL(IConfiguration config)
        {
            _db = new FrmInvParametrosDB(config);
        }

        public ErrorDto<ParametrosGenDto?> Parametros_Obtener(int CodEmpresa)
        {
            return _db.Parametros_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CntXContaDto>> obtenerContabilidades(int CodEmpresa)
        {
            return _db.ObtenerContabilidades(CodEmpresa);
        }

        public ErrorDto actualizar_Parametros(int CodEmpresa, ParametrosGenDto data)
        {
            return _db.actualizar_Parametros(CodEmpresa, data);
        }

        public ErrorDto<List<DescripcionCuentasDto>> Obtener_DescripcionesCuenta(int CodEmpresa)
        {
            return _db.Obtener_DescripcionesCuenta(CodEmpresa);
        }

        public ErrorDto<List<DescripcionTipoAsientoDto>> Obtener_DescripcionesAsiento(int CodEmpresa)
        {
            return _db.Obtener_DescripcionesAsiento(CodEmpresa);
        }

        public ErrorDto<List<DescripcionTipoAsientoDto>> Asientos_Obtener(int CodEmpresa)
        {
            return _db.Asientos_Obtener(CodEmpresa);
        }
    }
}