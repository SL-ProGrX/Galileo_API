using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvPaquetesBL
    {
        private const int CodigoValidacion = -2;

        private readonly FrmInvPaquetesDB _db;

        public FrmInvPaquetesBL(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmInvPaquetesDB(config);
        }

        public ErrorDto<PaqueteDataLista>
            INV_Paquetes_Lista_Obtener(
                int CodEmpresa,
                string filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
            {
                return INV_Paquetes_Lista_Error(
                    "Los filtros de consulta son requeridos.");
            }

            try
            {
                PaquetesFiltrosDto? filtrosDeserializados =
                    JsonConvert.DeserializeObject<
                        PaquetesFiltrosDto>(
                            filtros);

                if (filtrosDeserializados is null)
                {
                    return INV_Paquetes_Lista_Error(
                        "Los filtros de consulta no son v&aacute;lidos.");
                }

                filtrosDeserializados.pagina =
                    Math.Max(
                        filtrosDeserializados.pagina,
                        0);

                filtrosDeserializados.paginacion =
                    Math.Max(
                        filtrosDeserializados.paginacion,
                        0);

                filtrosDeserializados.filtro =
                    filtrosDeserializados.filtro?.Trim() ??
                    string.Empty;

                return _db
                    .INV_Paquetes_Lista_Obtener(
                        CodEmpresa,
                        filtrosDeserializados);
            }
            catch (JsonException)
            {
                return INV_Paquetes_Lista_Error(
                    "Los filtros de consulta no son v&aacute;lidos.");
            }
        }

        public ErrorDto<List<PaqueteDto>>
            INV_Paquetes_Todos_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_Paquetes_Todos_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<PaqueteDto>
            INV_Paquetes_Encabezado_Obtener(
                int CodEmpresa,
                int codPaquete)
        {
            return _db
                .INV_Paquetes_Encabezado_Obtener(
                    CodEmpresa,
                    codPaquete);
        }

        public ErrorDto<List<PaqueteDetalleDto>>
            INV_Paquetes_Detalle_Obtener(
                int CodEmpresa,
                int codPaquete)
        {
            return _db
                .INV_Paquetes_Detalle_Obtener(
                    CodEmpresa,
                    codPaquete);
        }

        public ErrorDto INV_Paquetes_Registrar(
            int CodEmpresa,
            PaqueteDto request)
        {
            return _db.INV_Paquetes_Registrar(
                CodEmpresa,
                request);
        }

        public ErrorDto INV_Paquetes_Actualizar(
            int CodEmpresa,
            PaqueteDto request)
        {
            return _db.INV_Paquetes_Actualizar(
                CodEmpresa,
                request);
        }

        public ErrorDto
            INV_Paquetes_Detalle_Registrar(
                int CodEmpresa,
                PaqueteDetalleDto request)
        {
            return _db
                .INV_Paquetes_Detalle_Registrar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            INV_Paquetes_Detalle_Actualizar(
                int CodEmpresa,
                PaqueteDetalleDto request)
        {
            return _db
                .INV_Paquetes_Detalle_Actualizar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            INV_Paquetes_Detalle_Eliminar(
                int CodEmpresa,
                PaqueteDetalleDto request)
        {
            return _db
                .INV_Paquetes_Detalle_Eliminar(
                    CodEmpresa,
                    request);
        }

        private static ErrorDto<PaqueteDataLista>
            INV_Paquetes_Lista_Error(
                string descripcion)
        {
            return DbHelper.CreateErrorResponse(
                descripcion,
                CodigoValidacion,
                new PaqueteDataLista());
        }
    }
}