using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic
{
    public class FrmInvKardexBl
    {
        private const string ErrorFiltrosRequeridos =
            "Los filtros del kardex son requeridos.";

        private const string ErrorFiltrosInvalidos =
            "El formato de los filtros del kardex no es v&aacute;lido.";

        private readonly FrmInvKardexDb _db;

        public FrmInvKardexBl(
            IConfiguration config)
        {
            _db = new FrmInvKardexDb(config);
        }

        public ErrorDto<List<InvKardexBodegaDto>>
            INV_Kardex_Bodegas_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_Kardex_Bodegas_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<InvKardexMovimientosListaDto>
            INV_Kardex_Movimientos_Obtener(
                int CodEmpresa,
                string filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
            {
                return DbHelper.CreateErrorResponse(
                    ErrorFiltrosRequeridos,
                    -2,
                    CrearResultadoVacio());
            }

            try
            {
                var filtro =
                    JsonConvert.DeserializeObject<
                        InvKardexMovimientosFiltro>(
                            filtros);

                if (filtro is null)
                {
                    return DbHelper.CreateErrorResponse(
                        ErrorFiltrosInvalidos,
                        -2,
                        CrearResultadoVacio());
                }

                return _db
                    .INV_Kardex_Movimientos_Obtener(
                        CodEmpresa,
                        filtro);
            }
            catch (JsonException)
            {
                return DbHelper.CreateErrorResponse(
                    ErrorFiltrosInvalidos,
                    -2,
                    CrearResultadoVacio());
            }
        }

        private static
            InvKardexMovimientosListaDto
            CrearResultadoVacio()
        {
            return new InvKardexMovimientosListaDto
            {
                total = 0,
                movimientos = []
            };
        }
    }
}