using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvMargenUtilidadBL
    {
        private readonly FrmInvMargenUtilidadDB _db;

        public FrmInvMargenUtilidadBL(IConfiguration config)
        {
            _db = new FrmInvMargenUtilidadDB(config);
        }

        public ErrorDto<List<LineaDto>> Linea_Obtener(int CodEmpresa)
        {
            return _db.Linea_Obtener(CodEmpresa);
        }

        public ErrorDto<List<SubLineaDto>> SubLinea_Obtener(int CodEmpresa)
        {
            return _db.SubLinea_Obtener(CodEmpresa);
        }

        public ErrorDto<List<PrecioDto>> ListadoPrecios_Obtener(int CodEmpresa)
        {
            return _db.ListadoPrecios_Obtener(CodEmpresa);
        }

        public ErrorDto cambio_margen(int CodEmpresa, int monto, int cod_linea, int cod_sublinea, string cambio_margen)
        {
            return _db.cambio_margen(CodEmpresa, monto, cod_linea, cod_sublinea, cambio_margen);
        }

    }
}