using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosDepartamentosBl
    {
        private readonly FrmActivosSeccionesDb _db;

        public FrmActivosDepartamentosBl(IConfiguration config)
        {
            _db = new FrmActivosSeccionesDb(config);
        }
        public ErrorDto<ActivosDepartamentosLista> Activos_DepartamentosLista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_DepartamentosLista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto<List<ActivosDepartamentosData>> Activos_Departamentos_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.Activos_Departamentos_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto Activos_Departamentos_Guardar(int CodEmpresa, string usuario, ActivosDepartamentosData departamento)
        {
            return _db.Activos_Departamentos_Guardar(CodEmpresa, usuario, departamento);
        }
        public ErrorDto Activos_Departamentos_Eliminar(int CodEmpresa, string usuario, string cod_departamento)
        {
            return _db.Activos_Departamentos_Eliminar(CodEmpresa, usuario, cod_departamento);
        }
        public ErrorDto Activos_Departamentos_Valida(int CodEmpresa, string cod_departamento)
        {
            return _db.Activos_Departamentos_Valida(CodEmpresa, cod_departamento);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Unidades_Obtener(int CodEmpresa, int contabilidad)
        {
            return _db.Activos_Departamentos_Unidades_Obtener(CodEmpresa, contabilidad);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.Activos_Departamentos_Dropdown_Obtener(CodEmpresa);
        }
        public ErrorDto<ActivosSeccionesLista> Activos_SeccionesLista_Obtener(int CodEmpresa, string? cod_departamento, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.Activos_SeccionesLista_Obtener(CodEmpresa, cod_departamento, filtros);
        }
        public ErrorDto<List<ActivosSeccionesData>> Activos_Secciones_Obtener(int CodEmpresa, string? cod_departamento, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.Activos_Secciones_Obtener(CodEmpresa, cod_departamento, filtros);
        }
        public ErrorDto Activos_Secciones_Guardar(int CodEmpresa, string usuario, ActivosSeccionesData seccion)
        {
            return _db.Activos_Secciones_Guardar(CodEmpresa, usuario, seccion);
        }
        public ErrorDto Activos_Secciones_Eliminar(int CodEmpresa, string usuario, string cod_departamento, string cod_seccion)
        {
            return _db.Activos_Secciones_Eliminar(CodEmpresa, usuario, cod_departamento, cod_seccion);
        }
        public ErrorDto Activos_Secciones_Valida(int CodEmpresa, string cod_departamento, string cod_seccion)
        {
            return _db.Activos_Secciones_Valida(CodEmpresa, cod_departamento, cod_seccion);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Secciones_CentrosCostos_Obtener(int CodEmpresa, int contabilidad)
        {
            return _db.Activos_Secciones_CentrosCostos_Obtener(CodEmpresa, contabilidad);
        }
    }
}
