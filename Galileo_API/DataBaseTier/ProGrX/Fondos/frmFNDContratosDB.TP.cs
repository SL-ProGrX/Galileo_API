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
        /// <returns>Última solicitud registrada; Result es null si el contrato no tiene solicitudes (VB6: rs.EOF).</returns>
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
                    commandType: System.Data.CommandType.StoredProcedure));
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

            // Paridad VB6: exec posicional (los nombres reales de parámetros del SP no se asumen)
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndSolicitudTpData>(
                    $"exec {SpTpSolicitud} @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12",
                    new
                    {
                        p1 = solicitud.operadora,
                        p2 = NormalizarTexto(solicitud.cod_plan),
                        p3 = solicitud.contrato,
                        p4 = NormalizarTexto(solicitud.cedula),
                        p5 = solicitud.tasa_calculada,
                        p6 = solicitud.margen_maximo,
                        p7 = solicitud.tasa_solicitada,
                        p8 = solicitud.plazo,
                        p9 = solicitud.frecuencia,
                        p10 = solicitud.inversion,
                        p11 = NormalizarTexto(solicitud.usuario),
                        p12 = NormalizarTexto(solicitud.notas)
                    })
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
                    $"exec {SpTpEstado} @p1",
                    new { p1 = gestion_id })
                ?? new FndSolicitudTpData());
        }

        #endregion
    }
}