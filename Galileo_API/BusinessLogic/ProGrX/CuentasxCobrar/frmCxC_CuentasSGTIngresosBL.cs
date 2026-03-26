using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_CxC;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX_CxC
{
    public class FrmCxCCuentasSGTIngresosBl
    {
        private readonly FrmCxCCuentasSGTIngresosDb _db;

        public FrmCxCCuentasSGTIngresosBl(IConfiguration config)
        {
            _db = new FrmCxCCuentasSGTIngresosDb(config);
        }

        public ErrorDto<List<CxCIngresoDto>> ListarRegistrosIngresos(int codEmpresa, int operacion)
            => _db.ListarRegistrosIngresos(codEmpresa, operacion);

        public ErrorDto<bool> GuardarRegistrosIngresos(int codEmpresa, CxCIngresoGuardarDto dto)
            => _db.GuardarRegistrosIngresos(codEmpresa, dto);

        public ErrorDto<bool> EliminarRegistroIngresos(int codEmpresa, int operacion, int linea, string codCargo)
            => _db.EliminarRegistroIngresos(codEmpresa, operacion, linea, codCargo);

        public ErrorDto<bool> ActualizarRegistroingreso(int codEmpresa, int operacion, string usuario)
            => _db.ActualizarRegistroingreso(codEmpresa, operacion, usuario);

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