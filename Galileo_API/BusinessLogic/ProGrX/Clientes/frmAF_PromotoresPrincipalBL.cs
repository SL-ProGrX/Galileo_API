using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFPromotoresPrincipalBL
    {
        private readonly FrmAFPromotoresPrincipalDB _db;

        public FrmAFPromotoresPrincipalBL(IConfiguration config)
        {
            _db = new FrmAFPromotoresPrincipalDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Promotores_Lista_Obtener(int CodEmpresa)
        {
            return _db.AF_Promotores_Lista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Promotores_Usuarios_Obtener(int CodEmpresa)
        {
            return _db.AF_Promotores_Usuarios_Obtener(CodEmpresa);
        }

        public ErrorDto<AfPromotoresPrincipalDto?> AF_Promotores_Scroll_Obtener(int CodEmpresa, int ScrollCode, int Codigo)
        {
            return _db.AF_Promotores_Scroll_Obtener(CodEmpresa, ScrollCode, Codigo);
        }

        public ErrorDto<AfPromotoresPrincipalDto?> AF_Promotor_Obtener(int CodEmpresa, int Codigo)
        {
            return _db.AF_Promotor_Obtener(CodEmpresa, Codigo);
        }

        public ErrorDto<List<AfPromotoresCuentasDto>> AF_Promotores_Cuentas_Obtener(int CodEmpresa, string CodComision)
        {
            return _db.AF_Promotores_Cuentas_Obtener(CodEmpresa, CodComision);
        }

        public ErrorDto<AfPromotoresPrincipalLista> AF_Promotores_ListadoConsulta_Obtener(int CodEmpresa, string Tipo, int Estado, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_Promotores_ListadoConsulta_Obtener(CodEmpresa, Tipo, Estado, filtros);
        }

        public ErrorDto<List<AfPromotoresBancoDto>> AF_Promotores_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            return _db.AF_Promotores_Bancos_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto AF_Promotores_Guardar(int CodEmpresa, string Usuario, AfPromotoresPrincipalDto Info)
        {
            return _db.AF_Promotores_Guardar(CodEmpresa, Usuario, Info);
        }

        public ErrorDto AF_Promotores_Eliminar(int CodEmpresa, string Usuario, int Codigo)
        {
            return _db.AF_Promotores_Eliminar(CodEmpresa, Usuario, Codigo);
        }
    }
}