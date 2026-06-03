using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFAfiliacionTagsDB
    {
        private readonly IConfiguration _config;

        private const string SpAfiliacionesControlConsulta = "spAFI_Afiliaciones_Control_Consulta";
        private const string SpAfiliacionRecepcionAplica = "spAFI_Afiliacion_Recepcion_Aplica";
        private const string SpAfiliacionRevisionAplica = "spAFI_Afiliacion_Revision_Aplica";
        private const string SpAfiliacionesEtiquetasConsulta = "spAFI_Afiliaciones_Etiquetas_Consulta";
        private const string SpAfiliacionRevisionReversar = "spAFI_Afiliacion_Revision_Reversar";

        private const string SqlBoletasPendientesRecibir = @"
                select CONSEC, CEDULA, NOMBRE, Tipo_Desc
                from vAFI_Afiliaciones_Pendientes_Recibir";

        private const string SqlBoletasAfiliacionesList = @"
                select CONSEC, CEDULA, NOMBRE, tipo_desc
                from vAFI_Afiliaciones_List";

        private const string SqlRevisionReversarValida = @"
                select dbo.fxAFI_Afiliacion_Revision_Reversar_Valida(@boleta)";

        public FrmAFAfiliacionTagsDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene Recepcion Afiliaciones
        /// </summary>
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Recepcion(int CodEmpresa, string estado, string filtro)
        {
            return ConsultarAfiliacionesControl(CodEmpresa, estado, filtro);
        }

        /// <summary>
        /// Obtiene afiliaciones recibidas
        /// </summary>
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Recibidas(int CodEmpresa, string estado, string filtro)
        {
            return ConsultarAfiliacionesControl(CodEmpresa, estado, filtro);
        }

        /// <summary>
        /// Obtiene Afiliaciones pendientes
        /// </summary>
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Pendientes(int CodEmpresa, string estado, string filtro)
        {
            return ConsultarAfiliacionesControl(CodEmpresa, estado, filtro);
        }

        /// <summary>
        /// Obtiene boletas afiliaciones
        /// </summary>
        public ErrorDto<List<AfBoletasAfiliacion>> AF_CR_BoletasAfiliacion_Obtener(int CodEmpresa)
        {
            return ConsultarBoletasAfiliacion(CodEmpresa, SqlBoletasPendientesRecibir);
        }

        /// <summary>
        /// Aplica recepcion
        /// </summary>
        public ErrorDto AFI_Afiliacion_Recepcion_Aplica(int codEmpresa, int boleta, string usuario)
        {
            return EjecutarStoredProcedure(
                codEmpresa,
                SpAfiliacionRecepcionAplica,
                new
                {
                    CodBoleta = boleta,
                    Usuario = usuario,
                    Nota = $"Recibe Afiliacion No. {boleta}",
                    Maquina = string.Empty,
                    AppVersion = "Galileo"
                });
        }

        /// <summary>
        /// Aplica revision
        /// </summary>
        public ErrorDto AFI_Afiliacion_Revision_Aplica(int codEmpresa, int consec, string estado, string usuario, string nota)
        {
            return EjecutarStoredProcedure(
                codEmpresa,
                SpAfiliacionRevisionAplica,
                new
                {
                    Consec = consec,
                    Estado = estado,
                    Usuario = usuario,
                    Nota = nota ?? string.Empty,
                });
        }

        /// <summary>
        /// Consulta etiquetas
        /// </summary>
        public ErrorDto<List<AfiEtiquetaDto>> AFI_Afiliaciones_Etiquetas_Consulta(int CodEmpresa, int boleta)
        {
            return DbHelper.ExecuteListQuery<AfiEtiquetaDto>(
                CreatePortalDb(),
                CodEmpresa,
                $"EXEC {SpAfiliacionesEtiquetasConsulta} @BoletaId",
                new { BoletaId = boleta });
        }

        /// <summary>
        /// Aplica revision y reversion
        /// </summary>
        public ErrorDto AFI_Afiliacion_Revision_Reversar(int CodEmpresa, int boleta, string usuario, string nota)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var valida = connection.QuerySingle<int>(SqlRevisionReversarValida, new { boleta });
                if (valida == 0)
                {
                    return DbHelper.ErrorResponse("No procede la reversión, la afiliación ya fue remesada", -2);
                }

                connection.Execute(
                    $"exec {SpAfiliacionRevisionReversar} @boleta, @usuario, @nota",
                    new { boleta, usuario, nota });

                return DbHelper.OkResponse("Ok");
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al reversar revisión de afiliación.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Agrega recepcion afiliacion
        /// </summary>
        public ErrorDto AFI_Afiliacion_Recepcion_Agregar(int CodEmpresa, int boleta, string usuario)
        {
            var result = EjecutarStoredProcedure(
                CodEmpresa,
                SpAfiliacionRecepcionAplica,
                new
                {
                    Boleta = boleta,
                    Usuario = usuario
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Boleta agregada correctamente")
                : result;
        }

        /// <summary>
        /// Obtiene lista de afiliaciones
        /// </summary>
        public ErrorDto<List<AfBoletasAfiliacion>> AF_BoletasAfiliacionLista_Obtener(int CodEmpresa)
        {
            return ConsultarBoletasAfiliacion(CodEmpresa, SqlBoletasAfiliacionesList);
        }

        private ErrorDto<List<AfiAfiliacionControlDto>> ConsultarAfiliacionesControl(int codEmpresa, string estado, string filtro)
        {
            return DbHelper.ExecuteListQuery<AfiAfiliacionControlDto>(
                CreatePortalDb(),
                codEmpresa,
                SpAfiliacionesControlConsulta,
                new
                {
                    Estado = estado,
                    Filtro = filtro
                });
        }

        private ErrorDto<List<AfBoletasAfiliacion>> ConsultarBoletasAfiliacion(int codEmpresa, string sql)
        {
            return DbHelper.ExecuteListQuery<AfBoletasAfiliacion>(
                CreatePortalDb(),
                codEmpresa,
                sql);
        }

        private ErrorDto EjecutarStoredProcedure(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(
                    storedProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? $"Error al ejecutar {storedProcedure}.", result.Code.GetValueOrDefault(-1));
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
