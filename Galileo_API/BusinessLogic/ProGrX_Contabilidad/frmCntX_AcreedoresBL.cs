using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ARF;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_ARF
{
    public class FrmArfAcreedoresBl
    {
        private readonly FrmArfAcreedoresDb _db;

        public FrmArfAcreedoresBl(IConfiguration config)
        {
            _db = new FrmArfAcreedoresDb(config);
        }

        public ErrorDto<ArfAcreedorDto?> Consultar(int codEmpresa, int codigo)
            => _db.Consultar(codEmpresa, codigo);

        public ErrorDto<int> Insertar(int codEmpresa, ArfAcreedorDto m)
            => _db.Insertar(codEmpresa, m);

        public ErrorDto<int> Actualizar(int codEmpresa, ArfAcreedorDto m)
            => _db.Actualizar(codEmpresa, m);

        public ErrorDto<int> Borrar(int codEmpresa, int codigo)
            => _db.Borrar(codEmpresa, codigo);

        public ErrorDto<int?> Scroll(int codEmpresa, int? codigoActual, int direccion)
            => _db.Scroll(codEmpresa, codigoActual, direccion);

        public ErrorDto<List<CuentaBancariaAcreedorDto>> CuentasBancarias(int codEmpresa, string identificacion)
            => _db.CuentasBancarias(codEmpresa, identificacion);

        public ErrorDto<List<ProvinciaAcreedorDto>> ObtenerProvincias(int codEmpresa)
            => _db.ObtenerProvincias(codEmpresa);

        public ErrorDto<List<CantonAcreedorDto>> ObtenerCantones(int codEmpresa, string provincia)
            => _db.ObtenerCantones(codEmpresa, provincia);

        public ErrorDto<List<DistritoAcreedorDto>> ObtenerDistritos(int codEmpresa, string provincia, string canton)
            => _db.ObtenerDistritos(codEmpresa, provincia, canton);

        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerTiposIdentificacion(int codEmpresa)
         => _db.ObtenerTiposIdentificacion(codEmpresa);

        public ErrorDto<List<ArfAcreedorDto>> BuscarAcreedores(int codEmpresa, string? filtro)
        => _db.BuscarAcreedores(codEmpresa, filtro);

        public ErrorDto<List<DropDownListaGenericaModel>> BuscarProveedores(int codEmpresa, string? filtro)
        => _db.BuscarProveedores(codEmpresa, filtro);

        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerBancos(int codEmpresa,string usuario)
        => _db.ObtenerBancos(codEmpresa, usuario);
    }
}
