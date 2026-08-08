using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using static Galileo_API.Models.ProGrX.Creditos.FrmCrComisionesPagoModels;


namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrComisionesPagoBL
    {
        private readonly FrmCrComisionesPagoDB _db;

        public FrmCrComisionesPagoBL(IConfiguration config)
        {
            _db = new FrmCrComisionesPagoDB(config);
        }


        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_Comisiones_Obtener(int codEmpresa)
              => _db.CrdComisionesPago_Comisiones_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_Bancos_Obtener(int CodEmpresa)
                => _db.CrdComisionesPago_Bancos_Obtener(CodEmpresa);
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_Oficinas_Obtener(int codEmpresa)
              => _db.CrdComisionesPago_Oficinas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_OficinasPendientes_Obtener(int codEmpresa, int codRemesa)
            => _db.CrdComisionesPago_OficinasPendientes_Obtener(codEmpresa, codRemesa);

        public ErrorDto<List<CrdComisionesPagoRemesaModel>> CrdComisionesPago_Remesas_Obtener(int codEmpresa, int cantidad = 50)
               => _db.CrdComisionesPago_Remesas_Obtener(codEmpresa, cantidad);

        public ErrorDto<CrdComisionesPagoRemesaModel?> CrdComisionesPago_Remesa_Obtener(int codEmpresa, int codRemesa)
            => _db.CrdComisionesPago_Remesa_Obtener(codEmpresa, codRemesa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_RemesasPorEstado_Obtener(int codEmpresa, string estado)
            => _db.CrdComisionesPago_RemesasPorEstado_Obtener(codEmpresa, estado);

        public ErrorDto<CrdComisionesPagoRemesaGuardarResponse> CrdComisionesPago_Remesa_Guardar(int codEmpresa, CrdComisionesPagoRemesaGuardarRequest request)
            => _db.CrdComisionesPago_Remesa_Guardar(codEmpresa, request);

        public ErrorDto<bool> CrdComisionesPago_Remesa_Eliminar(int codEmpresa, CrdComisionesPagoRemesaEliminarRequest request)
            => _db.CrdComisionesPago_Remesa_Eliminar(codEmpresa, request);

        public ErrorDto<List<CrdComisionesPagoPendienteModel>> CrdComisionesPago_Pendientes_Obtener(int codEmpresa, CrdComisionesPagoPendientesRequest request)
             => _db.CrdComisionesPago_Pendientes_Obtener(codEmpresa, request);

        public ErrorDto<CrdComisionesPagoProcesoResponse> CrdComisionesPago_Remesa_Cargar(int codEmpresa, CrdComisionesPagoCargaRequest request)
            => _db.CrdComisionesPago_Remesa_Cargar(codEmpresa, request);

        public ErrorDto<bool> CrdComisionesPago_Remesa_Cerrar(int codEmpresa, CrdComisionesPagoCerrarRequest request)
            => _db.CrdComisionesPago_Remesa_Cerrar(codEmpresa, request);

        public ErrorDto<List<CrdComisionesPagoTrasladoModel>> CrdComisionesPago_Traslado_Obtener(int codEmpresa, int codRemesa)
             => _db.CrdComisionesPago_Traslado_Obtener(codEmpresa, codRemesa);

        public ErrorDto<CrdComisionesPagoProcesoResponse> CrdComisionesPago_Remesa_Trasladar(int codEmpresa, CrdComisionesPagoTrasladarRequest request)
            => _db.CrdComisionesPago_Remesa_Trasladar(codEmpresa, request);

        public ErrorDto<List<CrdComisionesPagoReporteModel>> CrdComisionesPago_Reportes_Obtener(int codEmpresa, int cantidad)
              => _db.CrdComisionesPago_Reportes_Obtener(codEmpresa, cantidad);
    }
}
