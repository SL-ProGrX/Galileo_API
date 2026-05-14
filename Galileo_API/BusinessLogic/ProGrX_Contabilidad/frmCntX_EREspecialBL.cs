using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXErEspecialBl
    {
        private readonly FrmCntXErEspecialDb _db;

        public FrmCntXErEspecialBl(IConfiguration config) =>
            _db = new FrmCntXErEspecialDb(config);

        public ErrorDto<CntXErEspecialDefinicionData?> CntX_EREspecial_Consulta_Obtener(
            int codEmpresa,
            int codContabilidad,
            int codErEspecial)
        {
            return _db.CntX_EREspecial_Consulta_Obtener(
                codEmpresa,
                codContabilidad,
                codErEspecial);
        }

        public ErrorDto<List<CntXErEspecialDefinicionData>> CntX_EREspecial_Lista_Obtener(
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
            CntXErEspecialDefinicionData request)
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

        public ErrorDto<List<CntXErEspecialCuentaNodeData>> CntX_EREspecial_Arbol_Obtener(
            int codEmpresa,
            int codContabilidad,
            CntXErEspecialArbolRequest request)
        {
            return _db.CntX_EREspecial_Arbol_Obtener(
                codEmpresa,
                codContabilidad,
                request);
        }

        public ErrorDto CntX_EREspecial_Cuentas_Guardar(
            int codEmpresa,
            int codContabilidad,
            CntXErEspecialCuentasGuardarRequest request)
        {
            return _db.CntX_EREspecial_Cuentas_Guardar(
                codEmpresa,
                codContabilidad,
                request);
        }
    }
}
