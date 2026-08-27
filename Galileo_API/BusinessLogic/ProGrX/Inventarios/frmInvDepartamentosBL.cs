using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvDepartamentosBL
    {
        private readonly FrmInvDepartamentosDB _db;

        public FrmInvDepartamentosBL(IConfiguration config)
        {
            _db = new FrmInvDepartamentosDB(config);
        }

        public ErrorDto<DepartamentosDataLista> Departamentos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Departamentos_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public ErrorDto Departamentos_Insertar(int CodEmpresa, string usuario, DepartamentosDto request)
        {
            return _db.Departamentos_Insertar(CodEmpresa, usuario, request);
        }

        public ErrorDto Departamentos_Actualizar(int CodEmpresa, string usuario, DepartamentosDto request)
        {
            return _db.Departamentos_Actualizar(CodEmpresa, usuario, request);
        }

        public ErrorDto Departamentos_Eliminar(int CodEmpresa, string departamento, string usuario)
        {
            return _db.Departamentos_Eliminar(CodEmpresa, departamento, usuario);
        }

        public ErrorDto<List<AsignacionesDto>> Asignaciones_ObtenerTodos(int CodEmpresa, string departamento)
        {
            return _db.Asignaciones_ObtenerTodos(CodEmpresa, departamento);
        }

        public ErrorDto Asignaciones_Insertar(int CodEmpresa, AsignacionesDto request)
        {
            return _db.Asignaciones_Insertar(CodEmpresa, request);
        }

        public ErrorDto Asignaciones_Eliminar(int CodEmpresa, string Cod_Departamento, string Cod_Prodclas)
        {
            return _db.Asignaciones_Eliminar(CodEmpresa, Cod_Departamento, Cod_Prodclas);
        }
    }
}
