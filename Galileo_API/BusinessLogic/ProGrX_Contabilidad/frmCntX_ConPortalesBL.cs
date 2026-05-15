using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXConPortalesBl
    {
        private readonly FrmCntXConPortalesDb _db;

        public FrmCntXConPortalesBl(IConfiguration config) =>
            _db = new FrmCntXConPortalesDb(config);

        public ErrorDto<CntXConPortalesDefinicionData?> CntXConPortales_Consulta_Obtener(
            int codEmpresa,
            int codPortal)
        {
            return _db.CntXConPortales_Consulta_Obtener(codEmpresa, codPortal);
        }

        public ErrorDto<CntXConPortalesDefinicionData?> CntXConPortales_Scroll_Obtener(
            int codEmpresa,
            int codPortalActual,
            string direccion)
        {
            return _db.CntXConPortales_Scroll_Obtener(codEmpresa, codPortalActual, direccion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXConPortales_Lista_Obtener(int codEmpresa)
        {
            return _db.CntXConPortales_Lista_Obtener(codEmpresa);
        }

        public ErrorDto CntXConPortales_ProbarConexion(
            int codEmpresa,
            CntXConPortalesConexionRequest request)
        {
            return _db.CntXConPortales_ProbarConexion(codEmpresa, request);
        }

        public ErrorDto<List<CntXConPortalesContabilidadData>> CntXConPortales_Contabilidades_Obtener(
            int codEmpresa,
            CntXConPortalesConexionRequest request)
        {
            return _db.CntXConPortales_Contabilidades_Obtener(codEmpresa, request);
        }

        public ErrorDto CntXConPortales_Guardar(
            int codEmpresa,
            string usuario,
            CntXConPortalesGuardarRequest request)
        {
            return _db.CntXConPortales_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CntXConPortales_Borrar(
            int codEmpresa,
            int codPortal,
            string usuario)
        {
            return _db.CntXConPortales_Borrar(codEmpresa, codPortal, usuario);
        }
    }
}
