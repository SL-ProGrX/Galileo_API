using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXAsientosInvBl
    {
        private readonly FrmCntXAsientosInvDb _db;

        public FrmCntXAsientosInvBl(
            IConfiguration config)
        {
            _db = new FrmCntXAsientosInvDb(config);
        }

        public ErrorDto<CntXAsientosInvResponse?>
            CntX_frmCntX_AsientosInv_Asiento_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? numAsiento)
        {
            return _db
                .CntX_frmCntX_AsientosInv_Asiento_Obtener(
                    codEmpresa,
                    codContabilidad,
                    numAsiento);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            CntX_frmCntX_AsientosInv_Asientos_Lista_Obtener(
                int codEmpresa,
                CntXAsientosInvListaRequest request)
        {
            return _db
                .CntX_frmCntX_AsientosInv_Asientos_Lista_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<CntXAsientosInvCuentaData?>
            CntX_frmCntX_AsientosInv_Cuenta_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? cuenta)
        {
            return _db
                .CntX_frmCntX_AsientosInv_Cuenta_Obtener(
                    codEmpresa,
                    codContabilidad,
                    cuenta);
        }

        public ErrorDto CntX_frmCntX_AsientosInv_Guardar(
            int codEmpresa,
            CntXAsientosInvGuardarRequest request) 
        {
            return _db.CntX_frmCntX_AsientosInv_Guardar(
                codEmpresa,
                request);
        }

        public ErrorDto
            CntX_frmCntX_AsientosInv_Eliminar(
                int codEmpresa,
                CntXAsientosInvEliminarRequest request)
        {
            return _db
                .CntX_frmCntX_AsientosInv_Eliminar(
                    codEmpresa,
                    request);
        }
    }
}