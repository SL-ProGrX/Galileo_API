using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmFndTrasladoPatrimonioDB
    {
        public ErrorDto<SimpleSuccessResult> Fnd_ContratoDetalle_Insertar(
            int CodEmpresa,
            FndContratoDetalleInsertRequest request)
        {
            if (request is null)
            {
                return ErrorSimple("Los datos del detalle de contrato son requeridos.", -2);
            }

            return EjecutarSimple(
                CodEmpresa,
                SqlContratoDetalleInsert,
                CrearParametrosContratoDetalle(request),
                "Error al insertar detalle de contrato.");
        }

        public ErrorDto<SimpleSuccessResult> Fnd_Contrato_UpdateAportesRendimiento(
            int CodEmpresa,
            FndContratoUpdateRequest request)
        {
            if (request is null)
            {
                return ErrorSimple("Los datos del contrato son requeridos.", -2);
            }

            return EjecutarSimple(
                CodEmpresa,
                SqlContratoUpdateAportes,
                request,
                "Error al actualizar aportes y rendimiento.");
        }

        public ErrorDto<SimpleSuccessResult> SifTransaccionPatrimonio_Insertar(
            int CodEmpresa,
            SifTransaccionPatrimonioInsertRequest request)
        {
            if (request is null)
            {
                return ErrorSimple("Los datos de la transacción de patrimonio son requeridos.", -2);
            }

            var oficinaTitular = ObtenerOficinaTitular(CodEmpresa, request.Usuario);
            if (string.IsNullOrWhiteSpace(oficinaTitular))
            {
                return ErrorSimple("No se encontró la oficina titular del usuario.", -2);
            }

            request.OficinaTitular = oficinaTitular;

            return EjecutarSimple(
                CodEmpresa,
                SqlSifTransaccionPatrimonioInsert,
                CrearParametrosTransaccionPatrimonio(request),
                "Error al insertar transacción de patrimonio.");
        }

        private string ObtenerOficinaTitular(int CodEmpresa, string usuario)
        {
            var globales = _mProGrx
                .sbSifParametrosInicializa(CodEmpresa, usuario)
                .Result;

            return globales?.GOficinaTitular?.Trim() ?? string.Empty;
        }
    }
}
