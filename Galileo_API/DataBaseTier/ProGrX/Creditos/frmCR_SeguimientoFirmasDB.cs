using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public class FrmCRSeguimientoFirmasDB
    {
        private readonly IConfiguration _config;

        public FrmCRSeguimientoFirmasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Método para obtener el seguimiento de firmas de una operación de crédito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CRSeguimientoFirmasData>> CR_SeguimientoFirmas_Obtener(int CodEmpresa, int operacion)
        {
            return DbHelper.ExecuteListQuery<CRSeguimientoFirmasData>(
                CreatePortalDb(),
                CodEmpresa,
                @"select 'Deudor' as Tipo,
                         R.cedula,
                         S.nombre,
                         isnull(R.firma_deudor,0) as Firma,
                         R.ID_SOLICITUD as operacion
                  from reg_creditos R
                  inner join Socios S on R.cedula = S.cedula
                  where R.ID_SOLICITUD = @operacion
                  union
                  select 'Fiador' as Tipo,
                         S.cedula,
                         S.nombre,
                         case when F.firma = 'N' then 0 else 1 end as Firma,
                         F.ID_SOLICITUD as operacion
                  from fiadores F
                  inner join Socios S on F.cedulaf = S.cedula
                  where F.estado = 'A'
                    and F.id_solicitud = @operacion",
                new { operacion });
        }

        /// <summary>
        /// Método para guardar la firma de un deudor o fiador
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="firmasData"></param>
        /// <returns></returns>
        public ErrorDto CR_SeguimientoFirmas_Guardar(int CodEmpresa, CRSeguimientoFirmasData firmasData)
        {
            if (firmasData is null)
            {
                return DbHelper.ErrorResponse("Los datos de firma son requeridos.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                ObtenerQuerySeguimientoFirma(firmasData.tipo),
                new
                {
                    Firma = firmasData.firma,
                    operacion = firmasData.operacion,
                    Cedula = firmasData.cedula,
                    FirmaLetra = firmasData.firma ? "S" : "N"
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar seguimiento de firmas.", result.Code.GetValueOrDefault(-1));
        }

        private static string ObtenerQuerySeguimientoFirma(string? tipo)
        {
            return string.Equals(tipo, "Deudor", StringComparison.OrdinalIgnoreCase)
                ? @"update reg_creditos
                    set firma_deudor = @Firma,
                        fechaforf = dbo.MyGetdate()
                    where id_solicitud = @operacion"
                : @"update fiadores
                    set firma = @FirmaLetra
                    where ID_SOLICITUD = @operacion
                      and CedulaF = @Cedula";
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}