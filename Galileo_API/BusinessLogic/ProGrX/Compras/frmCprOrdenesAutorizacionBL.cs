using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprOrdenesAutorizacionBL
    {
        private readonly FrmCprOrdenesAutorizacionDB _db;

        public FrmCprOrdenesAutorizacionBL(IConfiguration config)
        {
            _db = new FrmCprOrdenesAutorizacionDB(config);
        }

        public ErrorDto<OrdenCompraDto> OrdenesCompra_Autorizacion_Obtener(
            int CodEmpresa,
            OrdenCompraRequestDto ordenCompraRequestDto)
        {
            return _db.OrdenesCompra_Autorizacion_Obtener(
                CodEmpresa,
                ordenCompraRequestDto);
        }

        public ErrorDto OrdenCompra_Autorizar(int CodEmpresa, OrdenCompraResolucionRequestDto ordenCompraRequestDto)
        {
            return _db.OrdenCompra_Autorizar(CodEmpresa, ordenCompraRequestDto);
        }

        public ErrorDto OrdenCompra_Rechazar(int CodEmpresa, OrdenCompraResolucionRequestDto ordenCompraRequestDto)
        {
            return _db.OrdenCompra_Rechazar(CodEmpresa, ordenCompraRequestDto);
        }
    }
}
