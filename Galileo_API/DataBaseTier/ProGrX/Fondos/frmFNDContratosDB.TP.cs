using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {
        private const string SpTpSolicitudUltima = "spFnd_TP_Solicitud_Ultima";
        private const string SpTpSolicitud = "spFnd_TP_Solicitud";
        private const string SpTpEstado = "spFnd_TP_Estado";

        #region TP

        /// <summary>
        /// Obtiene la última solicitud de tasa preferencial registrada para un contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="plan">Código del plan.</param>
        /// <param name="contrato">Número de contrato.</param>
        /// <param name="cedula">Cédula del titular.</param>
        /// <returns>Información de la tasa preferencial asociada al contrato.</returns>
        public ErrorDto<FndContratoTasaPreferencial> Fnd_Contratos_TP_Obtener(int CodEmpresa, int operadora, string plan, int contrato, string cedula)
        {
            if (contrato == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Registre el Contrato Primero, luego indique la solicitud de Tasa Preferencial!",
                    -1,
                    new FndContratoTasaPreferencial());
            }

            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndContratoTasaPreferencial>(
                    SpTpSolicitudUltima,
                    new
                    {
                        operadora,
                        plan = NormalizarTexto(plan),
                        contrato,
                        cedula = NormalizarTexto(cedula)
                    },
                    commandType: System.Data.CommandType.StoredProcedure)
                ?? new FndContratoTasaPreferencial());
        }

        /// <summary>
        /// Registra una solicitud de tasa preferencial para un contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="solicitud">Datos de la solicitud de tasa preferencial.</param>
        /// <returns>Resultado de la solicitud registrada.</returns>
        public ErrorDto<FndSolicitudTpData> Fnd_Contratos_TP_Solicita(int CodEmpresa, FndContratoTasaPreferencial solicitud)
        {
            if (solicitud is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos de la solicitud son requeridos.",
                    -2,
                    new FndSolicitudTpData());
            }

            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndSolicitudTpData>(
                    SpTpSolicitud,
                    new
                    {
                        Operadora = solicitud.operadora,
                        Plan = NormalizarTexto(solicitud.cod_plan),
                        Contrato = solicitud.contrato,
                        Cedula = NormalizarTexto(solicitud.cedula),
                        TasaCalulada = solicitud.tasa_calculada,
                        MargenMax = solicitud.margen_maximo,
                        TasaSolicitada = solicitud.tasa_solicitada,
                        Plazo = solicitud.plazo,
                        fCuponId = solicitud.frecuencia,
                        Inversion = solicitud.inversion,
                        Usuario = NormalizarTexto(solicitud.usuario),
                        Observacion = NormalizarTexto(solicitud.notas)
                    },
                    commandType: System.Data.CommandType.StoredProcedure)
                ?? new FndSolicitudTpData());
        }

        /// <summary>
        /// Obtiene el estado actual de una gestión de tasa preferencial.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="gestion_id">Identificador de la gestión.</param>
        /// <returns>Estado de la gestión de tasa preferencial.</returns>
        public ErrorDto<FndSolicitudTpData> Fnd_Contratos_TP_Estado(int CodEmpresa, int gestion_id)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndSolicitudTpData>(
                    SpTpEstado,
                    new { gestion_id },
                    commandType: System.Data.CommandType.StoredProcedure)
                ?? new FndSolicitudTpData());
        }

        #endregion
    }
}