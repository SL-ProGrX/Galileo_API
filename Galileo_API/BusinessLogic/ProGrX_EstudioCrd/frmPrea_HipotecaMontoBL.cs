using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaHipotecaMontoBL
    {
        private readonly FrmPreaHipotecaMontoDB _db;

        public FrmPreaHipotecaMontoBL(IConfiguration config)
        {
            _db = new FrmPreaHipotecaMontoDB(config);
        }

        public ErrorDto<FrmPreaHipotecaMontoListaResponse> Prea_frmPreaHipotecaMonto_Lista_Obtener(
            int codEmpresa,
            string cod_preanalisis,
            string tipo)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaHipotecaMontoListaResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente.",
                    Result = new FrmPreaHipotecaMontoListaResponse()
                };
            }

            var tipoNormalizado = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            if (tipoNormalizado != "TRA" && tipoNormalizado != "CAN" && tipoNormalizado != "CON")
            {
                return new ErrorDto<FrmPreaHipotecaMontoListaResponse>
                {
                    Code = -1,
                    Description = "El tipo de gasto de hipoteca no es válido.",
                    Result = new FrmPreaHipotecaMontoListaResponse()
                };
            }

            return _db.Prea_frmPreaHipotecaMonto_Lista_Obtener(codEmpresa, cod_preanalisis, tipoNormalizado);
        }

        public ErrorDto<FrmPreaHipotecaMontoGuardarResponse> Prea_frmPreaHipotecaMonto_Seleccion_Guardar(
            int codEmpresa,
            FrmPreaHipotecaMontoGuardarRequest request)
        {
            if (request is null)
            {
                return new ErrorDto<FrmPreaHipotecaMontoGuardarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la información de la selección.",
                    Result = new FrmPreaHipotecaMontoGuardarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return new ErrorDto<FrmPreaHipotecaMontoGuardarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el usuario.",
                    Result = new FrmPreaHipotecaMontoGuardarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaHipotecaMontoGuardarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente.",
                    Result = new FrmPreaHipotecaMontoGuardarResponse()
                };
            }

            request.tipo = (request.tipo ?? string.Empty).Trim().ToUpperInvariant();
            if (request.tipo != "TRA" && request.tipo != "CAN" && request.tipo != "CON")
            {
                return new ErrorDto<FrmPreaHipotecaMontoGuardarResponse>
                {
                    Code = -1,
                    Description = "El tipo de gasto de hipoteca no es válido.",
                    Result = new FrmPreaHipotecaMontoGuardarResponse()
                };
            }

            if (request.id_param <= 0)
            {
                return new ErrorDto<FrmPreaHipotecaMontoGuardarResponse>
                {
                    Code = -1,
                    Description = "Debe seleccionar un registro válido.",
                    Result = new FrmPreaHipotecaMontoGuardarResponse()
                };
            }

            return _db.Prea_frmPreaHipotecaMonto_Seleccion_Guardar(codEmpresa, request);
        }
    }
}
