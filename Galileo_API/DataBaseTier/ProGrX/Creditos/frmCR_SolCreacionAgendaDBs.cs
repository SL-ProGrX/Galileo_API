using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public class FrmCRSolCreacionAgendaDBs
    {
        private readonly IConfiguration _config;

        public FrmCRSolCreacionAgendaDBs(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Método para obtener los comités activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_SolCreacionAgenda_Comites_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                "Select id_comite as item, descripcion from comites where estado = 1");
        }

        /// <summary>
        /// Método para generar el acta de la agenda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="acta"></param>
        /// <returns></returns>
        public ErrorDto<CrSolCreacionAgendaReporteData> CR_SolCreacionAgenda_Acta_Generar(int CodEmpresa, CrSolCreacionAgendaActaData acta)
        {
            if (acta is null)
            {
                return DbHelper.CreateErrorResponse("Datos de acta inválidos.", -2, new CrSolCreacionAgendaReporteData());
            }

            if (!fxValida(acta.acta, acta.id_comite ?? 0))
            {
                return DbHelper.CreateErrorResponse("Faltan datos obligatorios.", -1, new CrSolCreacionAgendaReporteData());
            }

            if (acta.validaActa == 3 || acta.validaActa == 0)
            {
                return DbHelper.CreateErrorResponse("Numero de Acta no puede ser mayor a numero sugerido.", -1, new CrSolCreacionAgendaReporteData());
            }

            if (acta.validaActa == 1)
            {
                return DbHelper.CreateOkResponse(new CrSolCreacionAgendaReporteData
                {
                    reporte = "REIMPRESION DE AGENDA",
                    reg_credito = "REG_CREDITOS.FECHASOL"
                });
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "UPDATE COMITES SET ACTA = @acta WHERE ID_COMITE = @id_comite",
                    new { acta = acta.acta, id_comite = acta.id_comite });

                var fechaInicioStr = MProGrXAuxiliarDB.validaFechaGlobal(acta.fechaInicio, "yyyy-MM-dd") ?? string.Empty;
                var fechaCorteStr = MProGrXAuxiliarDB.validaFechaGlobal(acta.fechaCorte, "yyyy-MM-dd") ?? string.Empty;

                var listaSolicitudes = connection.Query<int>(
                    @"select id_solicitud
                      from reg_creditos
                      where acta is null
                        and estadosol = 'R'
                        and fechasol between @fechaInicio and @fechaCorte
                        and id_comite = @comite",
                    new
                    {
                        fechaInicio = fechaInicioStr,
                        fechaCorte = fechaCorteStr,
                        comite = acta.id_comite
                    }).ToList();

                foreach (var solicitud in listaSolicitudes)
                {
                    connection.Execute(
                        "UPDATE REG_CREDITOS SET ACTA = @acta WHERE ID_SOLICITUD = @solicitud",
                        new { acta = acta.acta, solicitud });
                }

                return new CrSolCreacionAgendaReporteData
                {
                    reporte = "AGENDA",
                    reg_credito = "REG_CREDITOS.FECHASOL"
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new CrSolCreacionAgendaReporteData())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al generar acta de agenda.", result.Code.GetValueOrDefault(-1), new CrSolCreacionAgendaReporteData());
        }

        /// <summary>
        /// Método para validar los datos de entrada
        /// </summary>
        /// <param name="acta"></param>
        /// <param name="comite"></param>
        /// <returns></returns>
        private static bool fxValida(int? acta, int comite)
        {
            var response = true;

            if (!acta.HasValue)
            {
                response = false;
            }

            if (comite == 0)
            {
                response = false;
            }

            return response;

        }

        /// <summary>
        /// Método para consultar el acta actual del comité
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <returns></returns>
        public ErrorDto<int> CR_SolCreacionAgenda_Acta_Consulta(int CodEmpresa, int id_comite)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                "select isnull(acta,0) as Acta from comites where id_comite = @comites",
                0,
                new { comites = id_comite });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar acta actual del comité.", result.Code.GetValueOrDefault(-1), 0);
            }

            var actaActual = result.Result;
            var siguienteActa = actaActual >= 0 ? actaActual + 1 : 1;
            return DbHelper.CreateOkResponse(siguienteActa);
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}