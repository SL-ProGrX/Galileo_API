using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCFacturasCancelaBL
    {
        private readonly FrmCxCFacturasCancelaDB _db;

        public FrmCxCFacturasCancelaBL(IConfiguration config)
        {
            _db = new FrmCxCFacturasCancelaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasCancelaPagadores_Obtener(
            int codEmpresa,
            string cedulaCliente)
        {
            return _db.CxCFacturasCancelaPagadores_Obtener(codEmpresa, cedulaCliente);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasCancelaDivisas_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaPagador)
        {
            return _db.CxCFacturasCancelaDivisas_Obtener(codEmpresa, cedulaCliente, cedulaPagador);
        }

        public ErrorDto<List<CxCFacturasCancelaPendienteDto>> CxCFacturasCancelaFacturas_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaPagador,
            string codDivisa)
        {
            return _db.CxCFacturasCancelaFacturas_Obtener(codEmpresa, cedulaCliente, cedulaPagador, codDivisa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasCancelaTipoDocumento_Obtener(
            int codEmpresa,
            string codigoCaja)
        {
            return _db.CxCFacturasCancelaTipoDocumento_Obtener(codEmpresa, codigoCaja);
        }

        public ErrorDto<bool> CxCFacturasCancelaFactura_Registrar(
            int codEmpresa,
            CxCFacturasCancelaFacturaRequestDto request)
        {
            return _db.CxCFacturasCancelaFactura_Registrar(codEmpresa, request);
        }

        public ErrorDto<bool> CxCFacturasCancelaAbono_Registrar(
            int codEmpresa,
            CxCFacturasCancelaAbonoRequestDto request)
        {
            return _db.CxCFacturasCancelaAbono_Registrar(codEmpresa, request);
        }
    }
}
