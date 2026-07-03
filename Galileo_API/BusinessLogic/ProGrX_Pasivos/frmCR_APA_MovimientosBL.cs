using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Pasivos
{
    public class FrmCrApaMovimientosBL
    {
        private readonly FrmCrApaMovimientosDB _db;

        public FrmCrApaMovimientosBL(IConfiguration config)
        {
            _db = new FrmCrApaMovimientosDB(config);
        }

        public ErrorDto<FrmCrApaMovimientosAcreedorDto?> CR_APA_Movimientos_Acreedor_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return _db.CR_APA_Movimientos_Acreedor_Obtener(codEmpresa, cod_acreedor);
        }

        public ErrorDto<FrmCrApaMovimientosOperacionDto?> CR_APA_Movimientos_Operacion_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return _db.CR_APA_Movimientos_Operacion_Obtener(codEmpresa, cod_acreedor, operacion);
        }

        public ErrorDto<List<FrmCrApaMovimientosDetalleDto>> CR_APA_Movimientos_Detalle_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return _db.CR_APA_Movimientos_Detalle_Obtener(codEmpresa, cod_acreedor, operacion);
        }

        public ErrorDto<FrmCrApaMovimientosCuentaDto?> CR_APA_Movimientos_Cuenta_Obtener(int codEmpresa, string usuario)
        {
            return _db.CR_APA_Movimientos_Cuenta_Obtener(codEmpresa, usuario);
        }

        public ErrorDto<FrmCrApaMovimientosNavegarDto?> CR_APA_Movimientos_Operacion_Navegar(
            int codEmpresa,
            string request)
        {
            var dto = JsonConvert.DeserializeObject<FrmCrApaMovimientosNavegarRequest>(request)
                      ?? new FrmCrApaMovimientosNavegarRequest();

            return _db.CR_APA_Movimientos_Operacion_Navegar(codEmpresa, dto);
        }

        public ErrorDto<FrmCrApaMovimientosAplicarResultadoDto?> CR_APA_Movimientos_Aplicar(
            int codEmpresa,
            FrmCrApaMovimientosAplicarRequest request)
        {
            return _db.CR_APA_Movimientos_Aplicar(codEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_APA_Movimientos_Acreedores_Obtener(int codEmpresa)
        {
            return _db.CR_APA_Movimientos_Acreedores_Obtener(codEmpresa);
        }

        public ErrorDto<List<FrmCrApaMovimientosOperacionBusquedaDto>> CR_APA_Movimientos_Operaciones_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return _db.CR_APA_Movimientos_Operaciones_Obtener(codEmpresa, cod_acreedor);
        }
    }
}