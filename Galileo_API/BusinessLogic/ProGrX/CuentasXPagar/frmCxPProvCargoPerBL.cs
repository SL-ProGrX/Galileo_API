using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPProvCargoPerBL
    {
        private readonly FrmCxPProvCargoPerDB DBCargosProv;

        public FrmCxPProvCargoPerBL(IConfiguration config)
        {
            DBCargosProv = new FrmCxPProvCargoPerDB(config);
        }

        public ErrorDto<List<Secuencia>> Secuencias_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return DBCargosProv.Secuencias_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto<List<Cargo>> Cargos_Obtener(int CodEmpresa)
        {
            return DBCargosProv.Cargos_Obtener(CodEmpresa);
        }

        public ErrorDto<CargoPerDto> CargoDetalle_Obtener(int CodEmpresa, int Cod_Proveedor, int Id)
        {
            return DBCargosProv.CargoDetalle_Obtener(CodEmpresa, Cod_Proveedor, Id);
        }

        public ErrorDto<ProveedorInfo> ProveedorDetalle_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return DBCargosProv.ProveedorDetalle_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto<CargoPerDtoList> CargosPer_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return DBCargosProv.CargosPer_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        public ErrorDto<PagoProvCargosDtoList> Pagos_Obtener(int CodEmpresa, int Cod_Proveedor, int Id, int? pagina, int? paginacion, string? filtro)
        {
            return DBCargosProv.Pagos_Obtener(CodEmpresa, Cod_Proveedor, Id, pagina, paginacion, filtro);
        }

        public ErrorDto Cargo_Actualizar(int CodEmpresa, CargoPerDto data)
        {
            return DBCargosProv.Cargo_Actualizar(CodEmpresa, data);
        }

        public ErrorDto Cargo_Insertar(int CodEmpresa, CargoPerDto data)
        {
            return DBCargosProv.Cargo_Insertar(CodEmpresa, data);
        }

        public ErrorDto Cargo_Eliminar(int CodEmpresa, CargoPerDto data)
        {
            return DBCargosProv.Cargo_Eliminar(CodEmpresa, data);
        }
    }//end class
}//end namespace