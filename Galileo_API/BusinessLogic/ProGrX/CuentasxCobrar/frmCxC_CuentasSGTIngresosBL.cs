using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_CxC;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX_CxC
{
    public class FrmCxCCuentasSgtIngresosBl
    {
        private readonly FrmCxCCuentasSgtIngresosDb _db;

        public FrmCxCCuentasSgtIngresosBl(IConfiguration config)
        {
            _db = new FrmCxCCuentasSgtIngresosDb(config);
        }

        public ErrorDto<List<CxCIngresoDto>> ListarRegistrosIngresos(int codEmpresa, int operacion)
        {
            return _db.ListarRegistrosIngresos(codEmpresa, operacion);
        }

        public ErrorDto<bool> GuardarRegistrosIngresos(int codEmpresa, CxCIngresoGuardarDto dto)
        {
            return _db.GuardarRegistrosIngresos(codEmpresa, dto);
        }

        public ErrorDto<bool> EliminarRegistroIngresos(int codEmpresa, int operacion, int linea, string codCargo)
        {
            return _db.EliminarRegistroIngresos(codEmpresa, operacion, linea, codCargo);
        }

        public ErrorDto<bool> ActualizarRegistroingreso(int codEmpresa, int operacion, string usuario)
        {
            return _db.ActualizarRegistroingreso(codEmpresa, operacion, usuario);
        }

        public ErrorDto<CxCIngresoDto> Scroll(int codEmpresa, int operacion, string? codCargo, string direccion)
        {
            return _db.Scroll(codEmpresa, operacion, codCargo, direccion);
        }

        public ErrorDto<List<CxCIngresoDto>> IngresosListar(int codEmpresa)
        {
            return _db.IngresosListar(codEmpresa);
        }
    }
}