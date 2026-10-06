using Galileo.DataBaseTier;
using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslRequisitosBL
    {
        private const int CodigoValidacion = -2;

        private readonly FrmFslRequisitosDB _db;

        public FrmFslRequisitosBL(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmFslRequisitosDB(config);
        }

        public ErrorDto<
            FslListaPaginadaDto<FslRequisitoDto>>
            FSL_Requisitos_Lista_Obtener(
                int CodEmpresa,
                string filtros)
        {
            if (!FSL_Requisitos_Filtros_Deserializar(
                filtros,
                out FslRequisitosFiltros request))
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros enviados no son v&aacute;lidos.",
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslRequisitoDto>());
            }

            return _db.FSL_Requisitos_Lista_Obtener(
                CodEmpresa,
                request);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_Requisitos_Planes_Obtener(
                int CodEmpresa)
        {
            return _db.FSL_Requisitos_Planes_Obtener(
                CodEmpresa);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_Requisitos_Causas_Obtener(
                int CodEmpresa,
                string codPlan)
        {
            return _db.FSL_Requisitos_Causas_Obtener(
                CodEmpresa,
                codPlan);
        }

        public ErrorDto<
            List<FslRequisitoCausaDto>>
            FSL_Requisitos_Asignaciones_Obtener(
                int CodEmpresa,
                string codPlan,
                string codCausa)
        {
            return _db
                .FSL_Requisitos_Asignaciones_Obtener(
                    CodEmpresa,
                    codPlan,
                    codCausa);
        }

        public ErrorDto
            FSL_Requisitos_Requisito_Registrar(
                int CodEmpresa,
                FslRequisitoGuardarRequest request)
        {
            return _db
                .FSL_Requisitos_Requisito_Registrar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_Requisitos_Requisito_Actualizar(
                int CodEmpresa,
                FslRequisitoGuardarRequest request)
        {
            return _db
                .FSL_Requisitos_Requisito_Actualizar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_Requisitos_Requisito_Eliminar(
                int CodEmpresa,
                string codRequisito,
                string usuario)
        {
            return _db
                .FSL_Requisitos_Requisito_Eliminar(
                    CodEmpresa,
                    codRequisito,
                    usuario);
        }

        public ErrorDto
            FSL_Requisitos_Asignacion_Actualizar(
                int CodEmpresa,
                FslRequisitoAsignacionRequest request)
        {
            return _db
                .FSL_Requisitos_Asignacion_Actualizar(
                    CodEmpresa,
                    request);
        }

        private static bool
            FSL_Requisitos_Filtros_Deserializar(
                string filtros,
                out FslRequisitosFiltros request)
        {
            request = new FslRequisitosFiltros();

            if (string.IsNullOrWhiteSpace(filtros))
            {
                return true;
            }

            try
            {
                request =
                    JsonConvert.DeserializeObject<
                        FslRequisitosFiltros>(
                            filtros) ??
                    new FslRequisitosFiltros();

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}