using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXDivisasDCBl
    {
        private readonly FrmCntXDivisasDCcB _db;

        public FrmCntXDivisasDCBl(IConfiguration config)
        {
            _db = new FrmCntXDivisasDCcB(config);
        }

        public ErrorDto<List<DivisaDto>> ObtenerDivisas(int codEmpresa)
        {
            return _db.ObtenerDivisas(codEmpresa);
        }

        public ErrorDto<List<TipoCambioDto>> ObtenerTiposCambio(int codEmpresa,int anio,int mes,string codDivisa)
        {
            return _db.ObtenerTiposCambio(codEmpresa, anio, mes, codDivisa);
        }

        public ErrorDto Procesar(int codEmpresa,int anio,int mes,ProcesarDiferencialRequestDto request,string usuario)
        {
            return _db.Procesar(codEmpresa,anio,mes,request.codDivisa!,request.tcCompra,request.tcVenta,usuario
            );
        }
    }
}
