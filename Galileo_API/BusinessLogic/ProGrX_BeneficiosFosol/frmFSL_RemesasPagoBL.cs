using Galileo.DataBaseTier;
using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslRemesasPagoBL
    {
        private const int CodigoValidacion = -2;

        private readonly FrmFslRemesasPagoDB _db;

        public FrmFslRemesasPagoBL(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmFslRemesasPagoDB(config);
        }

        public ErrorDto<FslRemesaDto?>
            FSL_RemesasPago_Remesa_Obtener(
                int CodEmpresa,
                long codRemesa)
        {
            return _db
                .FSL_RemesasPago_Remesa_Obtener(
                    CodEmpresa,
                    codRemesa);
        }

        public ErrorDto<
            FslListaPaginadaDto<FslRemesaDto>>
            FSL_RemesasPago_Remesas_Obtener(
                int CodEmpresa,
                string filtros)
        {
            if (!FSL_RemesasPago_Filtros_Deserializar(
                filtros,
                out FslRemesasFiltros request))
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros enviados no son v&aacute;lidos.",
                    CodigoValidacion,
                    new FslListaPaginadaDto<FslRemesaDto>());
            }

            return _db
                .FSL_RemesasPago_Remesas_Obtener(
                    CodEmpresa,
                    request);
        }

        public ErrorDto<List<FslRemesaDto>>
            FSL_RemesasPago_Cargas_Obtener(
                int CodEmpresa)
        {
            return _db
                .FSL_RemesasPago_Cargas_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            FslListaPaginadaDto<FslExpedienteRemesaDto>>
            FSL_RemesasPago_CargasLista_Obtener(
                int CodEmpresa,
                string filtros)
        {
            if (!FSL_RemesasPago_Filtros_Deserializar(
                filtros,
                out FslExpedientesFiltros request))
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros enviados no son v&aacute;lidos.",
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslExpedienteRemesaDto>());
            }

            return _db
                .FSL_RemesasPago_CargasLista_Obtener(
                    CodEmpresa,
                    request);
        }

        public ErrorDto<List<FslRemesaDto>>
            FSL_RemesasPago_Traslados_Obtener(
                int CodEmpresa)
        {
            return _db
                .FSL_RemesasPago_Traslados_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<List<FslExpedienteRemesaDto>>
            FSL_RemesasPago_TrasladoLista_Obtener(
                int CodEmpresa,
                long codRemesa)
        {
            return _db
                .FSL_RemesasPago_TrasladoLista_Obtener(
                    CodEmpresa,
                    codRemesa);
        }

        public ErrorDto
            FSL_RemesasPago_Remesa_Registrar(
                int CodEmpresa,
                FslRemesaGuardarRequest request)
        {
            return _db
                .FSL_RemesasPago_Remesa_Registrar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_RemesasPago_Remesa_Actualizar(
                int CodEmpresa,
                FslRemesaGuardarRequest request)
        {
            return _db
                .FSL_RemesasPago_Remesa_Actualizar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_RemesasPago_Remesa_Eliminar(
                int CodEmpresa,
                long codRemesa,
                string usuario)
        {
            return _db
                .FSL_RemesasPago_Remesa_Eliminar(
                    CodEmpresa,
                    codRemesa,
                    usuario);
        }

        public ErrorDto
            FSL_RemesasPago_Remesa_Cerrar(
                int CodEmpresa,
                FslRemesaCerrarRequest request)
        {
            return _db
                .FSL_RemesasPago_Remesa_Cerrar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_RemesasPago_Cargas_Aplicar(
                int CodEmpresa,
                FslRemesaAplicarRequest request)
        {
            return _db
                .FSL_RemesasPago_Cargas_Aplicar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_RemesasPago_Traslado_Aplicar(
                int CodEmpresa,
                FslRemesaAplicarRequest request)
        {
            return _db
                .FSL_RemesasPago_Traslado_Aplicar(
                    CodEmpresa,
                    request);
        }

        private static bool
            FSL_RemesasPago_Filtros_Deserializar<T>(
                string filtros,
                out T request)
            where T : new()
        {
            request = new T();

            if (string.IsNullOrWhiteSpace(filtros))
            {
                return true;
            }

            try
            {
                request =
                    JsonConvert.DeserializeObject<T>(
                        filtros) ??
                    new T();

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}