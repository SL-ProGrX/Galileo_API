using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones;

public sealed class FrmIvrProcFondosMovBl
{
    private readonly FrmIvrProcFondosMovDb _db;

    public FrmIvrProcFondosMovBl(IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _db = new FrmIvrProcFondosMovDb(config);
    }

    public ErrorDto<IvrProcFondosMovInicialData>
        IVR_Proc_Fondos_Mov_Inicializar(
            int codEmpresa,
            int tituloId,
            string concepto)
    {
        return _db.IVR_Proc_Fondos_Mov_Inicializar(
            codEmpresa, tituloId, concepto);
    }

    public ErrorDto<IvrProcFondosMovMovimientoData>
        IVR_Proc_Fondos_Mov_Movimiento_Consultar(
            int codEmpresa,
            int tituloId,
            int transacId)
    {
        return _db.IVR_Proc_Fondos_Mov_Movimiento_Consultar(
            codEmpresa, tituloId, transacId);
    }

    public ErrorDto<IvrProcFondosMovTransaccionesData>
        IVR_Proc_Fondos_Mov_Transacciones_Obtener(
            int codEmpresa,
            int transacId,
            string concepto)
    {
        return _db.IVR_Proc_Fondos_Mov_Transacciones_Obtener(
            codEmpresa, transacId, concepto);
    }

    public ErrorDto<IvrProcFondosMovGuardarData>
        IVR_Proc_Fondos_Mov_Solicitud_Guardar(
            int codEmpresa,
            IvrProcFondosMovGuardarRequest? request)
    {
        return _db.IVR_Proc_Fondos_Mov_Solicitud_Guardar(
            codEmpresa, request);
    }

    public ErrorDto<IvrProcFondosMovGuardarData>
        IVR_Proc_Fondos_Mov_Procesar(
            int codEmpresa,
            IvrProcFondosMovGuardarRequest? request)
    {
        return _db.IVR_Proc_Fondos_Mov_Procesar(
            codEmpresa, request);
    }

    public ErrorDto<IvrProcFondosMovGuardarData>
        IVR_Proc_Fondos_Mov_Eliminar(
            int codEmpresa,
            IvrProcFondosMovGuardarRequest? request)
    {
        return _db.IVR_Proc_Fondos_Mov_Eliminar(
            codEmpresa, request);
    }
}