using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSeguimientoTramitesDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrSeguimientoTramitesDB(IConfiguration config)
        {
           _portalDb = new PortalDB(config);   
        }

        public ErrorDto<List<dynamic>> Cr_SeguimientoTramites_Obtener(int CodEmpresa, string? filtro)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                    SELECT 
                        IdTramite,
                        CodCredito,
                        NombreCliente,
                        TipoTramite,
                        EstadoTramite,
                        FechaCreacion
                    FROM CRD_SEGUIMIENTO_TRAMITES
                    WHERE (@Filtro IS NULL OR NombreCliente LIKE '%' + @Filtro + '%')
                    ORDER BY FechaCreacion DESC";
                var result = conn.Query<dynamic>(query, new { Filtro = filtro }).ToList();
                return result;
            });
        }

    }
}
