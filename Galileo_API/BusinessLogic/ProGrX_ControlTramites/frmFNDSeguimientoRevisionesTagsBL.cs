using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmFndSeguimientoRevisionesTagsBl
    {
        private readonly FrmFndSeguimientoRevisionesTagsDb _db;

        public FrmFndSeguimientoRevisionesTagsBl(IConfiguration config)
        {
            _db = new FrmFndSeguimientoRevisionesTagsDb(config);
        }

        public ErrorDto<List<FndSeguimientoRevisionFondoData>>
            FND_frmFNDSeguimientoRevisionesTags_Fondos_Obtener(
                int codEmpresa,
                string? cedula)
        {
            return _db.FND_frmFNDSeguimientoRevisionesTags_Fondos_Obtener(
                codEmpresa,
                cedula);
        }

        public ErrorDto<FndSeguimientoRevisionDetalleData?>
            FND_frmFNDSeguimientoRevisionesTags_Detalle_Obtener(
                int codEmpresa,
                string request)
        {
            if (!TryDeserializar(request, out FndSeguimientoRevisionClaveRequest data))
            {
                return DbHelper.CreateErrorResponse<FndSeguimientoRevisionDetalleData?>(
                    "La solicitud enviada no tiene un formato v&aacute;lido.",
                    -2,
                    null);
            }

            return _db.FND_frmFNDSeguimientoRevisionesTags_Detalle_Obtener(
                codEmpresa,
                data);
        }

        public ErrorDto<List<FndSeguimientoRevisionRegistroData>>
            FND_frmFNDSeguimientoRevisionesTags_Seguimiento_Obtener(
                int codEmpresa,
                string request)
        {
            if (!TryDeserializar(request, out FndSeguimientoRevisionClaveRequest data))
            {
                return DbHelper.CreateErrorResponse<List<FndSeguimientoRevisionRegistroData>>(
                    "La solicitud enviada no tiene un formato v&aacute;lido.",
                    -2,
                    []);
            }

            return _db.FND_frmFNDSeguimientoRevisionesTags_Seguimiento_Obtener(
                codEmpresa,
                data);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            FND_frmFNDSeguimientoRevisionesTags_Etiquetas_Obtener(
                int codEmpresa,
                string usuario)
        {
            return _db.FND_frmFNDSeguimientoRevisionesTags_Etiquetas_Obtener(
                codEmpresa,
                usuario);
        }

        public ErrorDto
            FND_frmFNDSeguimientoRevisionesTags_Aviso_Obtener(
                int codEmpresa,
                string tagCodigo)
        {
            return _db.FND_frmFNDSeguimientoRevisionesTags_Aviso_Obtener(
                codEmpresa,
                tagCodigo);
        }

        public ErrorDto<List<FndSeguimientoRevisionOmisionData>>
            FND_frmFNDSeguimientoRevisionesTags_Omisiones_Obtener(
                int codEmpresa,
                string request)
        {
            if (!TryDeserializar(request, out FndSeguimientoRevisionClaveRequest data))
            {
                return DbHelper.CreateErrorResponse<List<FndSeguimientoRevisionOmisionData>>(
                    "La solicitud enviada no tiene un formato v&aacute;lido.",
                    -2,
                    []);
            }

            return _db.FND_frmFNDSeguimientoRevisionesTags_Omisiones_Obtener(
                codEmpresa,
                data);
        }

        public ErrorDto<FndSeguimientoRevisionOmisionCambiarData>
            FND_frmFNDSeguimientoRevisionesTags_Omision_Cambiar(
                int codEmpresa,
                FndSeguimientoRevisionOmisionCambiarRequest request)
        {
            return _db.FND_frmFNDSeguimientoRevisionesTags_Omision_Cambiar(
                codEmpresa,
                request);
        }

        public ErrorDto
            FND_frmFNDSeguimientoRevisionesTags_Aplicar(
                int codEmpresa,
                FndSeguimientoRevisionAplicarRequest request)
        {
            return _db.FND_frmFNDSeguimientoRevisionesTags_Aplicar(
                codEmpresa,
                request);
        }

        private static bool TryDeserializar<T>(string request, out T data)
            where T : new()
        {
            data = new T();

            if (string.IsNullOrWhiteSpace(request))
            {
                return false;
            }

            try
            {
                T? resultado = JsonConvert.DeserializeObject<T>(request);

                if (resultado is null)
                {
                    return false;
                }

                data = resultado;
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}