using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivConsultaHonorariosDetalleBL
    {
        private readonly FrmVivConsultaHonorariosDetalleDB _db;

        public FrmVivConsultaHonorariosDetalleBL(IConfiguration config)
        {
            _db = new FrmVivConsultaHonorariosDetalleDB(config);
        }

        public ErrorDto<FrmVivConsultaHonorariosDetalleResponse> Viv_ConsultaHonorariosDetalle_Obtener(
            int codEmpresa,
            FrmVivConsultaHonorariosDetalleRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<FrmVivConsultaHonorariosDetalleResponse>
                {
                    Code = -1,
                    Description = "Debe indicar una garantía válida.",
                    Result = new FrmVivConsultaHonorariosDetalleResponse()
                };
            }

            var resp = _db.Viv_ConsultaHonorariosDetalle_Obtener(codEmpresa, request);
            if (resp.Code < 0)
            {
                return new ErrorDto<FrmVivConsultaHonorariosDetalleResponse>
                {
                    Code = resp.Code,
                    Description = resp.Description,
                    Result = new FrmVivConsultaHonorariosDetalleResponse()
                };
            }

            var lista = resp.Result ?? [];
            var primero = lista.FirstOrDefault();

            return new ErrorDto<FrmVivConsultaHonorariosDetalleResponse>
            {
                Code = 0,
                Description = string.Empty,
                Result = new FrmVivConsultaHonorariosDetalleResponse
                {
                    numero_operacion = primero?.numero_operacion ?? 0,
                    cedula_socio = primero?.cedula_socio ?? string.Empty,
                    nombre_socio = primero?.nombre_socio ?? string.Empty,
                    total_monto = lista.Sum(x => x.monto),
                    detalle = lista.Select(x => new FrmVivConsultaHonorariosDetalleItem
                    {
                        linea = x.linea,
                        codigo = x.codigo,
                        descripcion = x.descripcion,
                        monto = x.monto,
                        contacto = x.contacto,
                        usuario = x.usuario,
                        fecha_registro = x.fecha_registro
                    }).ToList()
                }
            };
        }
    }
}
