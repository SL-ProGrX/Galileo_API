using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTomaFisicaBL
    {
        private readonly FrmInvTomaFisicaDB _db;

        public FrmInvTomaFisicaBL(IConfiguration config)
        {
            _db = new FrmInvTomaFisicaDB(config);
        }

        public ErrorDto<List<TomaFisicaDto>> TomaFisica_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _db.TomaFisica_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        public ErrorDto<List<TomaFisicaDetalleDto>> tomasFisicasDetalle_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _db.tomasFisicasDetalle_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        public ErrorDto tomaFisica_Insertar(int CodEmpresa, TomaFisicaDto data)
        {
            return _db.tomaFisica_Insertar(CodEmpresa, data);
        }

        public ErrorDto tomaFisicaDetalle_Insertar(int CodEmpresa, TomaFisicaDetalleDto data)
        {
            return _db.tomaFisicaDetalle_Insertar(CodEmpresa, data);
        }

        public ErrorDto<TomaFisicaDto> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            return _db.ConsultaAscDesc(CodEmpresa, consecutivo, tipo);
        }

        public ErrorDto<TomaFisicaDto> tomaFisicaConsecutivo_Obtener(int CodEmpresa, int consecutivo)
        {
            return _db.tomaFisicaConsecutivo_Obtener(CodEmpresa, consecutivo);
        }

        public ErrorDto actualizarTomaFisica(int CodEmpresa, TomaFisicaDto request)
        {
            return _db.actualizarTomaFisica(CodEmpresa, request);
        }

        public ErrorDto actualizarTomaFisicaDetalle(int CodEmpresa, TomaFisicaDetalleDto data)
        {
            return _db.actualizarTomaFisicaDetalle(CodEmpresa, data);
        }

        public ErrorDto EliminarDetalleTomaFisica(int CodEmpresa, int consecutivo, string cod_producto)
        {
            return _db.EliminarDetalleTomaFisica(CodEmpresa, consecutivo, cod_producto);
        }

        public ErrorDto EliminarTomaFisica(int CodEmpresa, int consecutivo)
        {
            return _db.EliminarTomaFisica(CodEmpresa, consecutivo);
        }

        public ErrorDto<TomaFisicaDetalleDto> TomaFisicaProdBarras_Obtener(
            int CodEmpresa, string cod_bodega, string cod_barras, string tipo)
        {
            return _db.TomaFisicaProdBarras_Obtener(CodEmpresa, cod_bodega, cod_barras, tipo);
        }

        public ErrorDto TomaFisicaBarras_Guardar(int CodEmpresa, TomaFisicaDetalleDto linea)
        {
            return _db.TomaFisicaBarras_Guardar(CodEmpresa, linea);
        }

    }
}