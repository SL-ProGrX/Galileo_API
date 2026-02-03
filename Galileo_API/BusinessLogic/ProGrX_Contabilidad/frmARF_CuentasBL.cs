using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmArfCuentasBl
    {
        private readonly FrmArfCuentasDb _db;

        public FrmArfCuentasBl(IConfiguration config) => _db = new FrmArfCuentasDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> ArfCuentas_Divisas_Obtener(int codEmpresa)
        {
            return _db.ArfCuentas_Divisas_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfCuentas_Unidades_Obtener(int codEmpresa)
        {
            return _db.ArfCuentas_Unidades_Obtener(codEmpresa);
        }

        public ErrorDto<List<ArfCuentasDto>> ArfCuentas_Obtener(int codEmpresa, string codDivisa, string codUnidad)
        {
            return _db.ArfCuentas_Obtener(codEmpresa, codDivisa, codUnidad);
        }

        public ErrorDto ArfCuentas_Registrar(int codEmpresa, ArfCuentasRegistraRequest request)
        {
            return _db.ArfCuentas_Registrar(codEmpresa, request);
        }
    }
}
