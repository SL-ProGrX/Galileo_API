using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones;

[Route("api/frmIVR_Proc_Fondos_Mov")]
[Authorize]
[ApiController]
public sealed class FrmIvrProcFondosMovController : ControllerBase
{
    private readonly FrmIvrProcFondosMovBl _bl;

    public FrmIvrProcFondosMovController(IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _bl = new FrmIvrProcFondosMovBl(config);
    }

    [HttpGet("IVR_Proc_Fondos_Mov_Inicializar")]
    public ErrorDto<IvrProcFondosMovInicialData>
        IVR_Proc_Fondos_Mov_Inicializar(
            [FromQuery] int codEmpresa,
            [FromQuery] int tituloId,
            [FromQuery] string concepto)
    {
        return _bl.IVR_Proc_Fondos_Mov_Inicializar(
            codEmpresa, tituloId, concepto);
    }

    [HttpGet("IVR_Proc_Fondos_Mov_Movimiento_Consultar")]
    public ErrorDto<IvrProcFondosMovMovimientoData>
        IVR_Proc_Fondos_Mov_Movimiento_Consultar(
            [FromQuery] int codEmpresa,
            [FromQuery] int tituloId,
            [FromQuery] int transacId)
    {
        return _bl.IVR_Proc_Fondos_Mov_Movimiento_Consultar(
            codEmpresa, tituloId, transacId);
    }

    [HttpGet("IVR_Proc_Fondos_Mov_Transacciones_Obtener")]
    public ErrorDto<IvrProcFondosMovTransaccionesData>
        IVR_Proc_Fondos_Mov_Transacciones_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int transacId,
            [FromQuery] string concepto)
    {
        return _bl.IVR_Proc_Fondos_Mov_Transacciones_Obtener(
            codEmpresa, transacId, concepto);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("IVR_Proc_Fondos_Mov_Solicitud_Guardar")]
    public ErrorDto<IvrProcFondosMovGuardarData>
        IVR_Proc_Fondos_Mov_Solicitud_Guardar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrProcFondosMovGuardarRequest? request)
    {
        return _bl.IVR_Proc_Fondos_Mov_Solicitud_Guardar(
            codEmpresa, request);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("IVR_Proc_Fondos_Mov_Procesar")]
    public ErrorDto<IvrProcFondosMovGuardarData>
        IVR_Proc_Fondos_Mov_Procesar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrProcFondosMovGuardarRequest? request)
    {
        return _bl.IVR_Proc_Fondos_Mov_Procesar(
            codEmpresa, request);
    }

    [ValidateAntiForgeryToken]
    [HttpDelete("IVR_Proc_Fondos_Mov_Eliminar")]
    public ErrorDto<IvrProcFondosMovGuardarData>
        IVR_Proc_Fondos_Mov_Eliminar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrProcFondosMovGuardarRequest? request)
    {
        return _bl.IVR_Proc_Fondos_Mov_Eliminar(
            codEmpresa, request);
    }
}