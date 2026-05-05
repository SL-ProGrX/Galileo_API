using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivRegistroAvaluoBl
    {
        private readonly FrmVivRegistroAvaluoDb _db;
        private readonly ClsConsultarBD _clsConsultar;
        private readonly ClsAgregarBD _clsAgregar;
        private readonly string pValidaGarantia = "Debe indicar una garantía válida.";

        public FrmVivRegistroAvaluoBl(IConfiguration config)
        {
            _db = new FrmVivRegistroAvaluoDb(config);
            _clsConsultar = new ClsConsultarBD(config);
            _clsAgregar = new ClsAgregarBD(config);
        }

        public ErrorDto<FrmVivGarantiaAvaluoRegistroResponse> Viv_GarantiaAvaluo_Obtener(
    int codEmpresa,
    FrmVivGarantiaAvaluoRegistroRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<FrmVivGarantiaAvaluoRegistroResponse>
                {
                    Code = -1,
                    Description = pValidaGarantia,
                    Result = new FrmVivGarantiaAvaluoRegistroResponse()
                };
            }

            return _db.Viv_GarantiaAvaluo_Obtener(codEmpresa, request);
        }

        public ErrorDto Viv_GarantiaAvaluo_Guardar(
            int codEmpresa,
            FrmVivGarantiaAvaluoGuardarRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar una garantía válida.");
            }

            if (request.id_contacto <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar un ingeniero válido.");
            }

            if (!request.fecha_inspeccion.HasValue)
            {
                return DbHelper.ErrorResponse("Debe indicar la fecha de inspección.");
            }

            string tipoPoliza = request.tipo_poliza.Trim().ToUpperInvariant();
            if (tipoPoliza != "P" && tipoPoliza != "C")
            {
                return DbHelper.ErrorResponse("Debe indicar un tipo de póliza válido.");
            }

            var estadoOperacion = _clsConsultar.fxEstadoOperacion(codEmpresa, request.numero_operacion);
            if (estadoOperacion.Code < 0)
            {
                return DbHelper.ErrorResponse(estadoOperacion.Description!);
            }

            if ((estadoOperacion.Result ?? string.Empty).Trim() != "F")
            {
                return DbHelper.ErrorResponse("El registro de avalúo solo aplica para operaciones en estado FORMALIZADA.");
            }

            var yaRegistrado = _db.Viv_GarantiaAvaluoRegistrado_Existe(codEmpresa, request.id_garantia);
            if (yaRegistrado.Code < 0)
            {
                return DbHelper.ErrorResponse(yaRegistrado.Description!);
            }

            if (yaRegistrado.Result)
            {
                return DbHelper.ErrorResponse("La información de avalúo no puede ser modificada, ya fue registrada.");
            }

            var existeIngeniero = _clsConsultar.fxTraerExisteContacto(codEmpresa, request.id_contacto, "I");
            if (existeIngeniero.Code < 0)
            {
                return DbHelper.ErrorResponse(existeIngeniero.Description!);
            }

            if (!existeIngeniero.Result)
            {
                return DbHelper.ErrorResponse("El ingeniero indicado no existe.");
            }


            var resp = _clsAgregar.fxRegistroAvaluo(
                codEmpresa,
                new FrmVivGarantiaAvaluoPosteriorRequest
                {
                    id_garantia = request.id_garantia,
                    id_ingeniero = request.id_contacto,
                    id_abogado = 0,
                    fecha_inspeccion = request.fecha_inspeccion,
                    valor_terreno = request.valor_terreno,
                    valor_construccion = request.valor_construccion,
                    observaciones_avaluo = request.observacion_avaluo?.Trim() ?? string.Empty,
                    registro_usuario = request.registro_usuario?.Trim() ?? string.Empty,
                    registro_fecha = null,
                    viaticos = request.viaticos,
                    tipo_poliza = tipoPoliza
                });

            return resp.Code < 0
                ? DbHelper.ErrorResponse(resp.Description!)
                : new ErrorDto
                {
                    Code = 0,
                    Description = "Información fue registrada correctamente."
                };
        }

        public ErrorDto<FrmVivGarantiaAvaluoMontoCambiarResponse> Viv_GarantiaAvaluoMonto_Guardar(
            int codEmpresa,
            FrmVivGarantiaAvaluoMontoCambiarRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<FrmVivGarantiaAvaluoMontoCambiarResponse>
                {
                    Code = -1,
                    Description = pValidaGarantia,
                    Result = new FrmVivGarantiaAvaluoMontoCambiarResponse()
                };
            }

            string tipo = request.tipo.Trim();
            if (tipo != "Avaluo" && tipo != "Viaticos" && tipo != "Honorarios" && tipo != "GLegales")
            {
                return new ErrorDto<FrmVivGarantiaAvaluoMontoCambiarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar un tipo de cambio válido.",
                    Result = new FrmVivGarantiaAvaluoMontoCambiarResponse()
                };
            }

            return _db.Viv_GarantiaAvaluoMonto_Guardar(codEmpresa, request);
        }
    }
}
