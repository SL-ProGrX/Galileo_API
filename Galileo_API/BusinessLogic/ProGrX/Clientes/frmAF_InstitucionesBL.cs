using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFInstitucionesBL
    {
        private readonly FrmAFInstitucionesDB _db;

        public FrmAFInstitucionesBL(IConfiguration config)
        {
            _db = new FrmAFInstitucionesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Lista_Obtener(int CodEmpresa)
        {
            return _db.AF_Instituciones_Lista_Obtener(CodEmpresa);
        }

        public ErrorDto<AfInstitucionDto?> AF_Instituciones_Scroll_Obtener(int CodEmpresa, int ScrollCode, int CodInstitucion)
        {
            return _db.AF_Instituciones_Scroll_Obtener(CodEmpresa, ScrollCode, CodInstitucion);
        }

        public ErrorDto<AfInstitucionDto?> AF_Institucion_Obtener(int CodEmpresa, int CodInstitucion)
        {
            return _db.AF_Institucion_Obtener(CodEmpresa, CodInstitucion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_CargaCombo_Obtener(int CodEmpresa, string Tipo, int Conta)
        {
            return _db.AF_Instituciones_CargaCombo_Obtener(CodEmpresa, Tipo, Conta);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Planes_Obtener(int CodEmpresa, int CodOperadora, string CodMoneda)
        {
            return _db.AF_Instituciones_Planes_Obtener(CodEmpresa, CodOperadora, CodMoneda);
        }

        public ErrorDto<List<AfInstitucionEmpresasDto>> AF_Institucion_Empresas_Obtener(int CodEmpresa, int CodInstitucion, int Tipo)
        {
            return _db.AF_Institucion_Empresas_Obtener(CodEmpresa, CodInstitucion, Tipo);
        }

        public ErrorDto<List<AfInstitucionesCodigosDto>> AF_Instituciones_Codigos_Obtener(int CodEmpresa, int CodInstitucion)
        {
            return _db.AF_Instituciones_Codigos_Obtener(CodEmpresa, CodInstitucion);
        }

        public ErrorDto<List<AfInstitucionesCodigosLineasDto>> AF_Instituciones_Codigos_Lineas_Obtener(int CodEmpresa, int CodInstitucion, string Codigo, int rbCodigo)
        {
            return _db.AF_Instituciones_Codigos_Lineas_Obtener(CodEmpresa, CodInstitucion, Codigo, rbCodigo);
        }

        public ErrorDto<List<AfInstitucionDepartamentosDto>> AF_Institucion_Departamentos_Obtener(int CodEmpresa, int CodInstitucion)
        {
            return _db.AF_Institucion_Departamentos_Obtener(CodEmpresa, CodInstitucion);
        }

        public ErrorDto<List<AfInstitucionSeccionesDto>> AF_Institucion_Secciones_Obtener(int CodEmpresa, int CodInstitucion, string CodDepartamento)
        {
            return _db.AF_Institucion_Secciones_Obtener(CodEmpresa, CodInstitucion, CodDepartamento);
        }

        public ErrorDto AF_Institucion_CambiarFecha(int CodEmpresa, int CodInstitucion, string FechaCorte, string Usuario)
        {
            return _db.AF_Institucion_CambiarFecha(CodEmpresa, CodInstitucion, FechaCorte, Usuario);
        }

        public ErrorDto AF_Institucion_InicializarDeduccion(int CodEmpresa, int CodInstitucion, string Proceso, string Usuario)
        {
            return _db.AF_Institucion_InicializarDeduccion(CodEmpresa, CodInstitucion, Proceso, Usuario);
        }

        public ErrorDto AF_Instituciones_Codigo_Guardar(int CodEmpresa, AfInstitucionesCodigosDto Info, string Usuario)
        {
            return _db.AF_Instituciones_Codigo_Guardar(CodEmpresa, Info, Usuario);
        }

        public ErrorDto AF_Instituciones_Codigo_Eliminar(int CodEmpresa, int CodInstitucion, string CodDeduccion, string Usuario)
        {
            return _db.AF_Instituciones_Codigo_Eliminar(CodEmpresa, CodInstitucion, CodDeduccion, Usuario);
        }

        public ErrorDto AF_Instituciones_Lineas_Asignacion_Guardar(int CodEmpresa, int CodInstitucion, string CodDeduccion, string Codigo, bool Checked, string Usuario)
        {
            return _db.AF_Instituciones_Lineas_Asignacion_Guardar(CodEmpresa, CodInstitucion, CodDeduccion, Codigo, Checked, Usuario);
        }

        public ErrorDto AF_Institucion_Empresas_Guardar(int CodEmpresa, int CodInstitucion, int CodDeductora, bool Checked, string Usuario)
        {
            return _db.AF_Institucion_Empresas_Guardar(CodEmpresa, CodInstitucion, CodDeductora, Checked, Usuario);
        }

        public ErrorDto AF_Institucion_Copiar(int CodEmpresa, int CodInstitucion, string CopiaDesc, string CopiaDescCorta, string Usuario)
        {
            return _db.AF_Institucion_Copiar(CodEmpresa, CodInstitucion, CopiaDesc, CopiaDescCorta, Usuario);
        }

        public ErrorDto AF_Institucion_Guardar(int CodEmpresa, AfInstitucionDto Info, string Usuario, bool vEdita)
        {
            return _db.AF_Institucion_Guardar(CodEmpresa, Info, Usuario, vEdita);
        }

        public ErrorDto AF_Institucion_Eliminar(int CodEmpresa, int CodInstitucion, string Usuario)
        {
            return _db.AF_Institucion_Eliminar(CodEmpresa, CodInstitucion, Usuario);
        }
    }
}