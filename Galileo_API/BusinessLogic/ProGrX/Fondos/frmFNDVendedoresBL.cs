using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndVendedoresBl
    {
        private readonly FrmFndVendedoresDb _Db;

        public FrmFndVendedoresBl(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndVendedoresDb(config);
        }

        public ErrorDto<List<CuentaBancariaVendedorDto>> SYS_CuentasBancarias_Obtener(int codEmpresa, string cedula)
        {
            return _Db.SYS_CuentasBancarias_Obtener(codEmpresa, cedula);
        }

        public ErrorDto<FndVendedorDto> Fnd_Vendedores_Obtener(int CodEmpresa, int cod_vendedor)
        {
            return _Db.Fnd_Vendedores_Obtener(CodEmpresa, cod_vendedor);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            return _Db.Fnd_Bancos_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<List<FndVendedorListaDto>> Fnd_Vendedores_Listas_Obtener(int CodEmpresa)
        {
            return _Db.Fnd_Vendedores_Listas_Obtener(CodEmpresa);
        }

        public ErrorDto Fnd_Vendedores_Insertar(int CodEmpresa, FndVendedorDto request)
        {
            return _Db.Fnd_Vendedores_Insertar(CodEmpresa, request);
        }

        public ErrorDto Fnd_Vendedores_Actualizar(int CodEmpresa, FndVendedorDto request)
        {
            return _Db.Fnd_Vendedores_Actualizar(CodEmpresa, request);
        }

        public ErrorDto Fnd_Vendedores_Eliminar(int CodEmpresa, int cod_vendedor)
        {
            return _Db.Fnd_Vendedores_Eliminar(CodEmpresa, cod_vendedor);
        }

        public ErrorDto<FndVendedorDto> FND_Vendedor_Scroll_Obtener(int CodEmpresa, int cod_vendedor, int scrollCode)
        {
            return _Db.FND_Vendedor_Scroll_Obtener(CodEmpresa, cod_vendedor, scrollCode);
        }
    }
}