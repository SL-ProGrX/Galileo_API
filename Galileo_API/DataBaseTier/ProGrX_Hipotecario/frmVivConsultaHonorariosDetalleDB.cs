using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivConsultaHonorariosDetalleDB
    {
        private readonly PortalDB _portalDb;

        public FrmVivConsultaHonorariosDetalleDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el detalle de honorarios registrados para una garantía.
        /// Replica la consulta usada en sbHonorariosLoad del VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Garantía a consultar.</param>
        /// <returns>Listado de honorarios con información del socio y operación.</returns>
        public ErrorDto<List<FrmVivConsultaHonorariosDetalleRawItem>> Viv_ConsultaHonorariosDetalle_Obtener(
            int codEmpresa,
            FrmVivConsultaHonorariosDetalleRequest request)
        {

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
                        SELECT
                            ISNULL(ViviendaGarantia.NumeroOperacion, 0) AS numero_operacion,
                            RTRIM(ISNULL(SOCIOS.CEDULA, '')) AS cedula_socio,
                            RTRIM(ISNULL(SOCIOS.NOMBRE, '')) AS nombre_socio,
                            ISNULL(HonorariosDT.Linea, 0) AS linea,
                            RTRIM(ISNULL(HonorariosDT.Codigo, '')) AS codigo,
                            RTRIM(ISNULL(TD.Descripcion, '')) AS descripcion,
                            ISNULL(HonorariosDT.Monto, 0) AS monto,
                            RTRIM(ISNULL(ViviendaContactos.Nombre, '')) AS contacto,
                            RTRIM(ISNULL(HonorariosDT.Usuario, '')) AS usuario,
                            HonorariosDT.Fecha AS fecha_registro
                        FROM ViviendaDesembolsosPendientesDT AS HonorariosDT
                        INNER JOIN ViviendaTiposDesembolsos AS TD
                            ON HonorariosDT.Codigo = TD.Codigo
                        INNER JOIN ViviendaGarantia
                            ON HonorariosDT.IdGarantia = ViviendaGarantia.IdGarantia
                        INNER JOIN ViviendaContactos
                            ON HonorariosDT.IdContacto = ViviendaContactos.IdContacto
                        INNER JOIN REG_CREDITOS
                            ON ViviendaGarantia.NumeroOperacion = REG_CREDITOS.ID_SOLICITUD
                        INNER JOIN SOCIOS
                            ON REG_CREDITOS.CEDULA = SOCIOS.CEDULA
                        WHERE ViviendaGarantia.IdGarantia = @id_garantia
                        ORDER BY HonorariosDT.Fecha;";
                return conn.Query<FrmVivConsultaHonorariosDetalleRawItem>(query, new
                {
                    id_garantia = request.id_garantia
                }).ToList();
            });
        }
    }
}
