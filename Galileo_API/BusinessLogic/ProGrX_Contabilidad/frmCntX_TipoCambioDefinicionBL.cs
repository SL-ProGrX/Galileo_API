using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXTipoCambioDefinicionBl
    {
        private readonly FrmCntXTipoCambioDefinicionDb _db;

        public FrmCntXTipoCambioDefinicionBl(IConfiguration config) =>
            _db = new FrmCntXTipoCambioDefinicionDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXDivisas_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<CntXTipoCambioData>> CntXTipoCambio_Obtener(int codEmpresa, int codConta, string codDivisa, int lineas)
        {
            return _db.CntXTipoCambio_Obtener(codEmpresa, codConta, codDivisa, lineas);
        }

        public ErrorDto CntXTipoCambio_Guardar(int codEmpresa, int codConta, string usuario, CntXTipoCambioData request)
        {
            return _db.CntXTipoCambio_Guardar(codEmpresa, codConta, usuario, request);
        }

        public ErrorDto CntXTipoCambio_Eliminar(int codEmpresa, int codConta, string usuario, string codDivisa, int idCambio)
        {
            return _db.CntXTipoCambio_Eliminar(codEmpresa, codConta, usuario, codDivisa, idCambio);
        }
    }
}
