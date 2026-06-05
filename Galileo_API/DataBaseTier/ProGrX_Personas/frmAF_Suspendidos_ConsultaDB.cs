using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAfSuspendidosConsultaDb
    {
        private readonly IConfiguration _config;

        public FrmAfSuspendidosConsultaDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public ErrorDto<List<AfSuspendidosConsultaDto>> AF_Suspendidos_Consulta_Obtener(int CodEmpresa, AfSuspendidosConsultaFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de suspendidos son requeridos.", -2, new List<AfSuspendidosConsultaDto>());
            }

            var fechaInicio = filtros.inicio.Date;
            var fechaCorte = filtros.corte.Date.AddDays(1).AddTicks(-1);
            if (filtros.todas_fechas)
            {
                fechaInicio = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
                fechaCorte = new DateTime(2200, 1, 1, 23, 59, 59, DateTimeKind.Unspecified);
            }

            var cedula = filtros.GetType().GetProperty("cedula")?.GetValue(filtros)
                         ?? filtros.GetType().GetProperty("Cedula")?.GetValue(filtros)
                         ?? string.Empty;

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfSuspendidosConsultaDto>(
                    "spPAT_AsociadosSinAportes_Gestion_Consulta",
                    new
                    {
                        Inicio = fechaInicio,
                        Corte = fechaCorte,
                        Evento = filtros.evento,
                        Cedula = cedula,
                        Nombre = filtros.nombre
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 180
                ).Select(x =>
                {
                    x.capitalizacion = x.capitalización;
                    return x;
                }).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfSuspendidosConsultaDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar afiliados suspendidos.", result.Code.GetValueOrDefault(-1), new List<AfSuspendidosConsultaDto>());
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
