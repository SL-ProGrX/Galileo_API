using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdCuentasBl
    {
        private readonly FrmAfCdCuentasDb _db;

        public FrmAfCdCuentasBl(IConfiguration config)
        {
            _db = new FrmAfCdCuentasDb(config);
        }

        public ErrorDto<AfCdCuentaData?> AfCdCuenta_Obtener(int codEmpresa, int operacion)
        {
            return _db.AfCdCuenta_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<AfCdCuentaData?> AfCdCuentas_Scroll_Obtener(int codEmpresa, int operacion, int scrollCode)
        {
            return _db.AfCdCuentas_Scroll_Obtener(codEmpresa, operacion, scrollCode);
        }

        public ErrorDto<List<AfCdActividadData>> AfCdActividades_Lista_Obtener(
            int codEmpresa, string tipo, int totalAsoc, int operacion, int comite)
        {
            return _db.AfCdActividades_Lista_Obtener(codEmpresa, tipo, totalAsoc, operacion, comite);
        }

        public ErrorDto<List<AfCdCuentaAdjuntosData>> AfCdCuenta_Adjuntos_Obtener(int codEmpresa, int operacion)
        {
            return _db.AfCdCuenta_Adjuntos_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<List<AfCdCuentaBitacoraData>> AfCdCuenta_Bitacora_Obtener(int codEmpresa, int operacion)
        {
            return _db.AfCdCuenta_Bitacora_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<List<AfCdCuentaData>> AfCdCuentas_Lista_Obtener(int codEmpresa)
        {
            return _db.AfCdCuentas_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
        {
            return _db.AfCdComites_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdCatalogo_Lista_Obtener(int codEmpresa, string origen)
        {
            return _db.AfCdCatalogo_Lista_Obtener(codEmpresa, origen);
        }

        public ErrorDto<List<AfCdCuentaBancariaData>> AfCdCuentasBancarias_Obtener(int codEmpresa, string cedula, int idBanco)
        {
            return _db.AfCdCuentasBancarias_Obtener(codEmpresa, cedula, idBanco);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdMiembros_Obtener(int codEmpresa, int codComite)
        {
            return _db.AfCdMiembros_Obtener(codEmpresa, codComite);
        }

        public ErrorDto<List<AfCdCuentaData>> AfCdLiquidacionesPendientes_Obtener(int codEmpresa, int codComite)
        {
            return _db.AfCdLiquidacionesPendientes_Obtener(codEmpresa, codComite);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdCargos_Lista_Obtener(int codEmpresa)
        {
            return _db.AfCdCargos_Lista_Obtener(codEmpresa);
        }
    }
}
