using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCorreccionCreditosBl
    {
        private readonly FrmCrCorreccionCreditosDb _db;

        public FrmCrCorreccionCreditosBl(IConfiguration config)
            => _db = new FrmCrCorreccionCreditosDb(config);

        public ErrorDto<CrCorreccionCreditosConsultaResponse> CR_CorreccionCreditos_Operacion_Obtener(
            int codEmpresa,
            int operacion,
            string usuario)
            => _db.CR_CorreccionCreditos_Operacion_Obtener(codEmpresa, operacion, usuario);

        public ErrorDto<List<DropDownListaGenericaModel>> CR_CorreccionCreditos_Catalogo_Obtener(
            int codEmpresa,
            int movimiento,
            string codigo)
            => _db.CR_CorreccionCreditos_Catalogo_Obtener(codEmpresa, movimiento, codigo);

        public ErrorDto<List<CrCorreccionCreditosDetalleSeleccion>> CR_CorreccionCreditos_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int movimiento)
            => _db.CR_CorreccionCreditos_Detalle_Obtener(codEmpresa, operacion, movimiento);

        public ErrorDto<int> CR_CorreccionCreditos_Proceso_Obtener(
            int codEmpresa,
            int proceso,
            int direccion)
            => _db.CR_CorreccionCreditos_Proceso_Obtener(codEmpresa, proceso, direccion);

        public ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Cambio_Aplicar(
            int codEmpresa,
            CrCorreccionCreditosAplicarRequest request)
            => _db.CR_CorreccionCreditos_Cambio_Aplicar(codEmpresa, request);

        public ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Formalizacion_Anular(
            int codEmpresa,
            CrCorreccionCreditosAnularRequest request)
            => _db.CR_CorreccionCreditos_Formalizacion_Anular(codEmpresa, request);

        public ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Operacion_Excluir(
            int codEmpresa,
            CrCorreccionCreditosExcluirRequest request)
            => _db.CR_CorreccionCreditos_Operacion_Excluir(codEmpresa, request);
    }
}
