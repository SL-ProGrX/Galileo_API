using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCntXAsientosInvController :
        ControllerBase
    {
        private readonly FrmCntXAsientosInvBl _bl;

        public FrmCntXAsientosInvController(
            IConfiguration config)
        {
            _bl = new FrmCntXAsientosInvBl(config);
        }

        [HttpGet(
            "CntX_frmCntX_AsientosInv_Asiento_Obtener")]
        public ErrorDto<CntXAsientosInvResponse?>
            CntX_frmCntX_AsientosInv_Asiento_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? numAsiento)
        {
            return _bl
                .CntX_frmCntX_AsientosInv_Asiento_Obtener(
                    codEmpresa,
                    codContabilidad,
                    numAsiento);
        }

        [HttpGet(
            "CntX_frmCntX_AsientosInv_Asientos_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
        CntX_frmCntX_AsientosInv_Asientos_Lista_Obtener(
            int codEmpresa,
            int cod_contabilidad,
            int anio,
            int mes)
        {
            return _bl
                .CntX_frmCntX_AsientosInv_Asientos_Lista_Obtener(
                    codEmpresa,
                    new CntXAsientosInvListaRequest
                    {
                        cod_contabilidad =
                            cod_contabilidad,
                        anio = anio,
                        mes = mes
                    });
        }

        [HttpGet(
            "CntX_frmCntX_AsientosInv_Cuenta_Obtener")]
        public ErrorDto<CntXAsientosInvCuentaData?>
            CntX_frmCntX_AsientosInv_Cuenta_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? cuenta)
        {
            return _bl
                .CntX_frmCntX_AsientosInv_Cuenta_Obtener(
                    codEmpresa,
                    codContabilidad,
                    cuenta);
        }


        [HttpPost(
            "CntX_frmCntX_AsientosInv_Guardar")]
        public ErrorDto
            CntX_frmCntX_AsientosInv_Guardar(
                int codEmpresa,
                CntXAsientosInvGuardarRequest request)
        {
            return _bl
                .CntX_frmCntX_AsientosInv_Guardar(
                    codEmpresa,
                    request);
        }

        [HttpDelete(
            "CntX_frmCntX_AsientosInv_Eliminar")]
        public ErrorDto
            CntX_frmCntX_AsientosInv_Eliminar(
                int codEmpresa,
                int cod_contabilidad,
                string num_asiento,
                string usuario)
        {
            return _bl
                .CntX_frmCntX_AsientosInv_Eliminar(
                    codEmpresa,
                    new CntXAsientosInvEliminarRequest
                    {
                        cod_contabilidad =
                            cod_contabilidad,
                        num_asiento =
                            num_asiento,
                        usuario =
                            usuario
                    });
        }
    }
}