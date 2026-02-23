using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXExploradorContableBl
    {
        private readonly FrmCntXExploradorContableDb _db;

        public FrmCntXExploradorContableBl(IConfiguration config)
        {
            _db = new FrmCntXExploradorContableDb(config);
        }

        #region TREE

        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas_Obtener(int codEmpresa)
            => _db.Cntx_Cuentas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsiento_Obtener(int codEmpresa)
            => _db.Cntx_TiposAsiento_Obtener(codEmpresa);

        public ErrorDto<List<CntxPeriodoDto>> Periodos_Obtener(int codEmpresa, string estado)
            => _db.Cntx_Periodos_Obtener(codEmpresa, estado);

        #endregion

        #region LISTADOS

        public ErrorDto<List<CntxAsientoRsmDto>> Asientos_Listar(int codEmpresa,CntxExploradorFiltrosDto filtros)
            => _db.Cntx_Asientos_Listar(codEmpresa, filtros);

        public ErrorDto<List<CntxAsientoDetDto>> AsientoDetalle_Listar(
            int codEmpresa,
            CntxExploradorFiltrosDto filtros)
            => _db.AsientoDetalle_Listar(codEmpresa, filtros);

        #endregion

        #region AUX

        public ErrorDto<string> FechaServidor_Obtener(int codEmpresa)
            => _db.FechaServidor_Obtener(codEmpresa);

        #endregion
    }
}