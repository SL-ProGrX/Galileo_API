using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvTomaFisicaBL
    {
        private const int CodigoValidacion = -2;

        private readonly FrmInvTomaFisicaDB _db;

        public FrmInvTomaFisicaBL(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmInvTomaFisicaDB(config);
        }

        public ErrorDto<List<TomaFisicaDto>> INV_TomaFisica_Lista_Obtener(
            int CodEmpresa,
            string filtros)
        {
            if (!INV_TomaFisica_Filtros_Deserializar(
                filtros,
                out TomaFisicaListaFiltros? request))
            {
                return INV_TomaFisica_Error_Crear(
                    "Los filtros de consulta no son v&aacute;lidos.",
                    new List<TomaFisicaDto>());
            }

            return _db.INV_TomaFisica_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<List<TomaFisicaDetalleDto>> INV_TomaFisica_Detalle_Obtener(
            int CodEmpresa,
            string filtros)
        {
            if (!INV_TomaFisica_Filtros_Deserializar(
                filtros,
                out TomaFisicaDetalleFiltros? request))
            {
                return INV_TomaFisica_Error_Crear(
                    "Los filtros de consulta no son v&aacute;lidos.",
                    new List<TomaFisicaDetalleDto>());
            }

            return _db.INV_TomaFisica_Detalle_Obtener(CodEmpresa, request);
        }

        public ErrorDto<TomaFisicaDto> INV_TomaFisica_Consecutivo_Obtener(
            int CodEmpresa,
            int consecutivo)
        {
            return _db.INV_TomaFisica_Consecutivo_Obtener(
                CodEmpresa,
                consecutivo);
        }

        public ErrorDto<TomaFisicaDto> INV_TomaFisica_Consecutivo_Navegar(
            int CodEmpresa,
            int consecutivo,
            string? tipo)
        {
            return _db.INV_TomaFisica_Consecutivo_Navegar(
                CodEmpresa,
                consecutivo,
                tipo);
        }

        public ErrorDto<TomaFisicaDetalleDto> INV_TomaFisica_Producto_Obtener(
            int CodEmpresa,
            string filtros)
        {
            if (!INV_TomaFisica_Filtros_Deserializar(
                filtros,
                out TomaFisicaProductoRequest? request))
            {
                return INV_TomaFisica_Error_Crear(
                    "Los filtros de b&uacute;squeda no son v&aacute;lidos.",
                    new TomaFisicaDetalleDto());
            }

            return _db.INV_TomaFisica_Producto_Obtener(CodEmpresa, request);
        }

        public ErrorDto<int> INV_TomaFisica_Guardar(
            int CodEmpresa,
            TomaFisicaGuardarRequest request)
        {
            return _db.INV_TomaFisica_Guardar(CodEmpresa, request);
        }

        public ErrorDto INV_TomaFisica_Eliminar(
            int CodEmpresa,
            int consecutivo,
            string? usuario)
        {
            return _db.INV_TomaFisica_Eliminar(
                CodEmpresa,
                consecutivo,
                usuario);
        }

        public ErrorDto INV_TomaFisica_Barras_Guardar(
            int CodEmpresa,
            TomaFisicaDetalleDto linea)
        {
            return _db.INV_TomaFisica_Barras_Guardar(CodEmpresa, linea);
        }

        public ErrorDto<List<TomaFisicaDetalleDto>>
            INV_TomaFisica_InventarioLogico_Obtener(
                int CodEmpresa,
                TomaFisicaInventarioRequest request)
        {
            return _db.INV_TomaFisica_InventarioLogico_Obtener(
                CodEmpresa,
                request);
        }

        public ErrorDto<List<TomaFisicaDetalleDto>> INV_TomaFisica_Comparar(
            int CodEmpresa,
            TomaFisicaGuardarRequest request)
        {
            return _db.INV_TomaFisica_Comparar(CodEmpresa, request);
        }

        private static bool INV_TomaFisica_Filtros_Deserializar<T>(
            string filtros,
            out T? request)
            where T : class
        {
            request = null;

            if (string.IsNullOrWhiteSpace(filtros))
            {
                return false;
            }

            try
            {
                request = JsonConvert.DeserializeObject<T>(filtros);
                return request is not null;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static ErrorDto<T> INV_TomaFisica_Error_Crear<T>(
            string descripcion,
            T resultado)
        {
            return DbHelper.CreateErrorResponse(
                descripcion,
                CodigoValidacion,
                resultado);
        }
    }
}
