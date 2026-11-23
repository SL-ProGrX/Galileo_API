using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public partial class FrmPolizaReclamoDB
    {
        //Helpers para validaciones comunes
        private static string TrimOrEmpty(string? value) => value?.Trim() ?? string.Empty;

        private static ErrorDto ValidarRequestBasico(int? reclamoId, string? usuario)
        {
            if (!reclamoId.HasValue || reclamoId.Value <= 0)
                return DbHelper.ErrorResponse(PolizaReclamoConstFrm.valReclamo, -2);

            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.ErrorResponse(PolizaReclamoConstFrm.valUsuario, -2);

            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto<T> ValidarRequestBasico<T>(int? reclamoId, string? usuario)
        {
            if (!reclamoId.HasValue || reclamoId.Value <= 0)
                return DbHelper.CreateErrorResponse<T>(PolizaReclamoConstFrm.valReclamo, -2);

            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<T>(PolizaReclamoConstFrm.valUsuario, -2);

            return DbHelper.CreateOkResponse<T>();
        }

        private static ErrorDto ValidarResultadoOperacion(
            dynamic? row,
            string mensajeNull,
            string mensajeDefault)
        {
            if (row == null)
                return DbHelper.ErrorResponse(mensajeNull);

            return (row.Pass ?? 0) == 1
                ? DbHelper.OkResponse(row.Mensaje?.ToString() ?? mensajeDefault)
                : DbHelper.ErrorResponse(row.Mensaje?.ToString() ?? mensajeDefault, -2);
        }

        private static ErrorDto<T> ValidarResultadoOperacion<T>(
            dynamic? row,
            string mensajeNull,
            string mensajeDefault,
            Func<dynamic, T> mapResponse)
        {
            if (row == null)
                return DbHelper.CreateErrorResponse<T>(mensajeNull);

            if ((row.Pass ?? 0) != 1)
                return DbHelper.CreateErrorResponse<T>(row.Mensaje?.ToString() ?? mensajeDefault, -2);

            return DbHelper.CreateOkResponse(mapResponse(row));
        }
    }
}
