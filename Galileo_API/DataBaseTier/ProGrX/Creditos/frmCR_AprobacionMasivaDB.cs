using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrAprobacionMasivaDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrAprobacionMasivaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta las operaciones disponibles para aprobación masiva.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrAprobacionMasivaOperacionData>> CrAprobacionMasiva_Consulta_Obtener(
            int codEmpresa,
            CrAprobacionMasivaConsultaRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.codigo = NormalizarTexto(request.codigo);

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new List<CrAprobacionMasivaOperacionData>());
            }

            if (!request.fecha_inicio.HasValue || !request.fecha_corte.HasValue)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el rango de fechas.",
                    -2,
                    new List<CrAprobacionMasivaOperacionData>());
            }

            if (request.fecha_corte.Value.Date < request.fecha_inicio.Value.Date)
            {
                return DbHelper.CreateErrorResponse(
                    "La fecha corte no puede ser menor que la fecha inicio.",
                    -2,
                    new List<CrAprobacionMasivaOperacionData>());
            }

            const string sql = @"
                exec spCrd_AprobacionMasiva_Consulta
                    @Codigo,
                    @FechaInicio,
                    @FechaCorte,
                    @Usuario;";

            var response = DbHelper.ExecuteListQuery<CrAprobacionMasivaOperacionData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Codigo = request.codigo,
                    FechaInicio = request.fecha_inicio.Value.Date,
                    FechaCorte = request.fecha_corte.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                    Usuario = request.usuario
                });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    response.Description ?? "No fue posible consultar las operaciones.",
                    response.Code.GetValueOrDefault(-1),
                    new List<CrAprobacionMasivaOperacionData>());
            }

            return DbHelper.CreateOkResponse(response.Result ?? new List<CrAprobacionMasivaOperacionData>());
        }

        /// <summary>
        /// Obtiene la lista de la líneas de crédito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrAprobacionMasiva_LineasCatalogo_Obtener(
            int codEmpresa,
            string? codigo)
        {
            codigo = NormalizarTexto(codigo ?? string.Empty);

            string sql = @"
                select
                    rtrim(codigo) as item,
                    rtrim(descripcion) as descripcion
                from catalogo
                where retencion = 'N'
                  and poliza = 'N'";

            if (!string.IsNullOrWhiteSpace(codigo))
            {
                sql += " and codigo = @Codigo";
            }

            sql += " order by codigo;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = codigo });
        }

        /// <summary>
        /// Formaliza las operaciones seleccionadas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrAprobacionMasiva_Formalizar(
            int codEmpresa,
            CrAprobacionMasivaFormalizarRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.operaciones = request.operaciones
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el usuario.",
                    -2);
            }

            if (request.operaciones.Count == 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe seleccionar al menos una operacion.",
                    -2);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var tx = conn.BeginTransaction();

                const string sql = @"
                    exec spCrd_AprobacionMasiva_Formaliza
                        @Operacion,
                        @Usuario;";

                foreach (var operacion in request.operaciones)
                {
                    conn.Execute(
                        sql,
                        new
                        {
                            Operacion = operacion,
                            Usuario = request.usuario
                        },
                        tx);
                }

                tx.Commit();

                return DbHelper.OkResponse("Operaciones Procesadas Satisfactoriamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private static string NormalizarTexto(string valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}