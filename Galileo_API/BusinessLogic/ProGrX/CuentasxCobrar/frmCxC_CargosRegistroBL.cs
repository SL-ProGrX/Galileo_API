using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCCargosRegistroBl
    {
        private readonly FrmCxCCargosRegistroDb _db;

        public FrmCxCCargosRegistroBl(IConfiguration config) =>
            _db = new FrmCxCCargosRegistroDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> CxCCargosRegistro_CargosAdicionales_Obtener(int codEmpresa)
        {
            return _db.CxCCargosRegistro_CargosAdicionales_Obtener(codEmpresa);
        }

        public ErrorDto<CxCCargosRegistroOperacionData?> CxCCargosRegistro_Operacion_Obtener(int codEmpresa, int operacion)
        {
            return _db.CxCCargosRegistro_Operacion_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<CxCCargosRegistroCargoData?> CxCCargosRegistro_Cargo_Obtener(int codEmpresa, string codCargo)
        {
            return _db.CxCCargosRegistro_Cargo_Obtener(codEmpresa, codCargo);
        }

        public ErrorDto<CxCCargosRegistroCargoReposicionData?> CxCCargosRegistro_CargoReposicion_Obtener(int codEmpresa, int operacion)
        {
            return _db.CxCCargosRegistro_CargoReposicion_Obtener(codEmpresa, operacion);
        }

        public ErrorDto CxCCargosRegistro_Aplicar(int codEmpresa, string usuario, CxCCargosRegistroAplicarRequest request)
        {
            return _db.CxCCargosRegistro_Aplicar(codEmpresa, usuario, request);
        }
    }
}
