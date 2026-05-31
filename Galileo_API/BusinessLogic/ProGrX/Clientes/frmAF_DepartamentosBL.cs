using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFDepartamentosBL
    {
        private readonly FrmAFDepartamentosDB _db;

        public FrmAFDepartamentosBL(IConfiguration config)
        {
            _db = new FrmAFDepartamentosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_DepartamentosInstituciones_Obtener(int CodEmpresa)
        {
            return _db.AF_DepartamentosInstituciones_Obtener(CodEmpresa);
        }

        public ErrorDto<AfDepartamentosLista> AF_DepartamentosLista_Obtener(int CodEmpresa, int Institucion, string Filtros)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(Filtros) ?? new FiltrosLazyLoadData();
            return _db.AF_DepartamentosLista_Obtener(CodEmpresa, Institucion, filtro);
        }

        public ErrorDto<AfSeccionesLista> AF_DepartamentosSecciones_Obtener(int CodEmpresa, int Institucion, string Departamento, string Filtros)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(Filtros) ?? new FiltrosLazyLoadData();
            return _db.AF_DepartamentosSecciones_Obtener(CodEmpresa, Institucion, Departamento, filtro);
        }

        public ErrorDto AF_Departamentos_Guardar(int CodEmpresa, AfDepartamentosDto Info)
        {
            return _db.AF_Departamentos_Guardar(CodEmpresa, Info);
        }

        public ErrorDto AF_DepartamentosSecciones_Guardar(int CodEmpresa, AfSeccionesDto Info)
        {
            return _db.AF_DepartamentosSecciones_Guardar(CodEmpresa, Info);
        }

        public ErrorDto AF_Departamentos_Borrar(int CodEmpresa, int Institucion, string Departamento)
        {
            return _db.AF_Departamentos_Borrar(CodEmpresa, Institucion, Departamento);
        }

        public ErrorDto AF_DepartamentosSecciones_Borrar(int CodEmpresa, int Institucion, string Departamento, string Seccion)
        {
            return _db.AF_DepartamentosSecciones_Borrar(CodEmpresa, Institucion, Departamento, Seccion);
        }
    }
}