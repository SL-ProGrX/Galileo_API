using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdLiquidacionesBl
    {
        private readonly FrmAfCdLiquidacionesDb _db;

        public FrmAfCdLiquidacionesBl(IConfiguration config)
        {
            _db = new FrmAfCdLiquidacionesDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
        {
            return _db.AfCdComites_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<string?> AfCdComite_Descripcion_Obtener(int codEmpresa, string codComite)
        {
            return _db.AfCdComite_Descripcion_Obtener(codEmpresa, codComite);
        }

        public ErrorDto<int> AfCdLiquidaciones_Pendientes_Obtener(int codEmpresa, string codComite)
        {
            return _db.AfCdLiquidaciones_Pendientes_Obtener(codEmpresa, codComite);
        }

        public ErrorDto<List<AfCdOperacionData>> AfCdOperaciones_Lista_Obtener(int codEmpresa, string codComite)
        {
            return _db.AfCdOperaciones_Lista_Obtener(codEmpresa , codComite);
        }

        public ErrorDto<List<AfCdOperacionData>> AfCdOperaciones_Detallar_Obtener(int codEmpresa, string codComite)
        {
            return _db.AfCdOperaciones_Detallar_Obtener(codEmpresa, codComite);
        }

        public ErrorDto<List<AfCdOperacionHistoricoData>> AfCdOperaciones_Historico_Obtener(int codEmpresa, string codComite)
        {
            return _db.AfCdOperaciones_Historico_Obtener(codEmpresa, codComite);
        }

        public ErrorDto<List<AfCdFacturaData>> AfCdFacturas_Obtener(int codEmpresa, int operacion)
        {
            return _db.AfCdFacturas_Obtener(codEmpresa, operacion);
        }

        public ErrorDto AfCdDetalleLiquidacion_Guardar(int codEmpresa, string usuario, AfCdFacturaData request)
        {
            return _db.AfCdDetalleLiquidacion_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto AfCdDetalleLiquidacion_Eliminar(int codEmpresa, int operacion, string documento, string usuario)
        {
            return _db.AfCdDetalleLiquidacion_Eliminar(codEmpresa, operacion, documento, usuario);
        }

        public ErrorDto<AfCdDetalleLiquidacionMontosData> AfCdDetalleLiquidacion_Montos_Obtener(int codEmpresa, int operacion)
        {
            return _db.AfCdDetalleLiquidacion_Montos_Obtener(codEmpresa, operacion);
        }

        public ErrorDto AfCdLiquidacion_Detallar(int codEmpresa, int operacion)
        {
            return _db.AfCdLiquidacion_Detallar(codEmpresa, operacion);
        }

        public ErrorDto<object> AfCdLiquidacionOperacion_Liquidar(int codEmpresa, int operacion, string usuario, string notas)
        {
            return _db.AfCdLiquidacionOperacion_Liquidar(codEmpresa, operacion, usuario, notas);
        }

        public ErrorDto<object> AfCdLiquidacion_Historico_Imprimir(int codEmpresa, int col, string opRef, string codigoComite, string usuario)
        {
            return _db.AfCdLiquidacion_Historico_Imprimir(codEmpresa, col, opRef, codigoComite, usuario);
        }
    }
}
