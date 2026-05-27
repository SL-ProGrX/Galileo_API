using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Galileo.Models;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCalculoOperacionBl
    {
        private readonly FrmCrCalculoOperacionDb _db;

        public FrmCrCalculoOperacionBl(IConfiguration config)
        {
            _db = new FrmCrCalculoOperacionDb(config);
        }

        public ErrorDto<CrCalculoOperacionPantallaData> CrCalculoOperacion_Cedula_Obtener(int codEmpresa, string cedula)
            => _db.CrCalculoOperacion_Cedula_Obtener(codEmpresa, cedula);

        public ErrorDto<CrCalculoOperacionCodigoData> CrCalculoOperacion_Codigo_Obtener(int codEmpresa, string cedula, string codigo)
            => _db.CrCalculoOperacion_Codigo_Obtener(codEmpresa, cedula, codigo);

        public ErrorDto<CrCalculoOperacionRangosData> CrCalculoOperacion_Rangos_Obtener(int codEmpresa, string codigo, decimal monto)
            => _db.CrCalculoOperacion_Rangos_Obtener(codEmpresa, codigo, monto);

        public ErrorDto<List<CrCalculoOperacionDisponibleData>> CrCalculoOperacion_Disponibles_Obtener(int codEmpresa, string cedula)
            => _db.CrCalculoOperacion_Disponibles_Obtener(codEmpresa, cedula);

        public ErrorDto<List<DropDownListaGenericaModel>> CrCalculoOperacion_Catalogo_Obtener(int codEmpresa)
            => _db.CrCalculoOperacion_Catalogo_Obtener(codEmpresa);
    }
}