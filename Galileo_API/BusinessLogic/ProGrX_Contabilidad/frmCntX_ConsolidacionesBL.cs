using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXConsolidacionesBl
    {
        private readonly FrmCntXConsolidacionesDb _db;

        public FrmCntXConsolidacionesBl(IConfiguration config) =>
            _db = new FrmCntXConsolidacionesDb(config);

        public ErrorDto<CntXConsolidacionDefinicionData?> CntXConsolidaciones_Consulta_Obtener(
            int codEmpresa,
            int codConsolida)
        {
            return _db.CntXConsolidaciones_Consulta_Obtener(codEmpresa, codConsolida);
        }

        public ErrorDto<CntXConsolidacionDefinicionData?> CntXConsolidaciones_Scroll_Obtener(
            int codEmpresa,
            int codConsolidaActual,
            string direccion)
        {
            return _db.CntXConsolidaciones_Scroll_Obtener(
                codEmpresa,
                codConsolidaActual,
                direccion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXConsolidaciones_Lista_Obtener(int codEmpresa)
        {
            return _db.CntXConsolidaciones_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXConsolidaciones_Contabilidades_Obtener(int codEmpresa)
        {
            return _db.CntXConsolidaciones_Contabilidades_Obtener(codEmpresa);
        }

        public ErrorDto<List<CntXConsolidacionContabilidadData>> CntXConsolidaciones_ContabilidadesLocales_Obtener(
            int codEmpresa,
            int codContabilidad,
            int codConsolida)
        {
            return _db.CntXConsolidaciones_ContabilidadesLocales_Obtener(codEmpresa, codContabilidad, codConsolida);
        }

        public ErrorDto<List<CntXConsolidacionPortalNodeData>> CntXConsolidaciones_PortalesRaiz_Obtener(int codEmpresa)
        {
            return _db.CntXConsolidaciones_PortalesRaiz_Obtener(codEmpresa);
        }

        public ErrorDto<List<CntXConsolidacionPortalNodeData>> CntXConsolidaciones_PortalesContabilidades_Obtener(
            int codEmpresa,
            int codPortal,
            int codContabilidadBase,
            int codConsolida)
        {
            return _db.CntXConsolidaciones_PortalesContabilidades_Obtener(
                codEmpresa,
                codPortal,
                codContabilidadBase,
                codConsolida);
        }

        public ErrorDto CntXConsolidaciones_Guardar(
            int codEmpresa,
            string usuario,
            CntXConsolidacionesGuardarRequest request)
        {
            return _db.CntXConsolidaciones_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CntXConsolidaciones_Borrar(
            int codEmpresa,
            int codConsolida,
            string usuario)
        {
            return _db.CntXConsolidaciones_Borrar(codEmpresa, codConsolida, usuario);
        }
    }
}

