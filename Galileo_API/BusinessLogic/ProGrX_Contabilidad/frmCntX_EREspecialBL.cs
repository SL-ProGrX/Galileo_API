using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXEREspecialBl
    {
        private readonly FrmCntXEREspecialDb _db;

        public FrmCntXEREspecialBl(IConfiguration config) =>
            _db = new FrmCntXEREspecialDb(config);

        public ErrorDto<CntXEREspecialDefinicionData?> CntX_EREspecial_Consulta_Obtener(
            int codEmpresa,
            int codContabilidad,
            int codErEspecial)
        {
            return _db.CntX_EREspecial_Consulta_Obtener(
                codEmpresa,
                codContabilidad,
                codErEspecial);
        }

        public ErrorDto<List<CntXEREspecialDefinicionData>> CntX_EREspecial_Lista_Obtener(
            int codEmpresa,
            int codContabilidad)
        {
            return _db.CntX_EREspecial_Lista_Obtener(
                codEmpresa,
                codContabilidad);
        }

        public ErrorDto CntX_EREspecial_Guardar(
            int codEmpresa,
            int codContabilidad,
            string usuario,
            CntXEREspecialDefinicionData request)
        {
            return _db.CntX_EREspecial_Guardar(
                codEmpresa,
                codContabilidad,
                usuario,
                request);
        }

        public ErrorDto CntX_EREspecial_Borrar(
            int codEmpresa,
            int codContabilidad,
            int codErEspecial,
            string usuario)
        {
            return _db.CntX_EREspecial_Borrar(
                codEmpresa,
                codContabilidad,
                codErEspecial,
                usuario);
        }

        public ErrorDto<List<CntXEREspecialCuentaNodeData>> CntX_EREspecial_Arbol_Obtener(
            int codEmpresa,
            int codContabilidad,
            CntXEREspecialArbolRequest request)
        {
            return _db.CntX_EREspecial_Arbol_Obtener(
                codEmpresa,
                codContabilidad,
                request);
        }

        public ErrorDto CntX_EREspecial_Cuentas_Guardar(
            int codEmpresa,
            int codContabilidad,
            CntXEREspecialCuentasGuardarRequest request)
        {
            return _db.CntX_EREspecial_Cuentas_Guardar(
                codEmpresa,
                codContabilidad,
                request);
        }
    }
}
