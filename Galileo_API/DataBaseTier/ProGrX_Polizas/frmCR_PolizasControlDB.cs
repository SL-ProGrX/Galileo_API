using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Clientes;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizasControlDB
    {
       private readonly PortalDB _portalDb;
    
       public FrmCrPolizasControlDB(IConfiguration config)
       {
          _portalDb = new PortalDB(config);
       }

        public ErrorDto<PolizaLookupResponseDto> Cr_PolizasControl_Obtener(int CodEmpresa, string CodPoliza)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT 
                            COD_POLIZA CodPoliza,
                            DESCRIPCION Descripcion
                        FROM CRD_CATALOGO_POLIZAS
                        WHERE COD_POLIZA = @CodPoliza";

                return conn.QueryFirstOrDefault<PolizaLookupResponseDto>(
                    query,
                    new { Cedula = CodPoliza.Trim() }
                ) ?? new PolizaLookupResponseDto();
            });
        }

        public ErrorDto<PolizaLookupResponseDto?> Cr_PolizasControl_Scroll(
                int codEmpresa,
                string codPolizaActual,
                int direccion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            string sql;

            if (direccion == 1)
            {
                sql = @"
            SELECT TOP 1
                COD_POLIZA CodPoliza,
                DESCRIPCION Descripcion
            FROM CRD_CATALOGO_POLIZAS
            WHERE COD_POLIZA > @CodPolizaActual
            ORDER BY COD_POLIZA ASC";
            }
            else
            {
                sql = @"
            SELECT TOP 1
                COD_POLIZA CodPoliza,
                DESCRIPCION Descripcion
            FROM CRD_CATALOGO_POLIZAS
            WHERE COD_POLIZA < @CodPolizaActual
            ORDER BY COD_POLIZA DESC";
            }

            var result = connection.QueryFirstOrDefault<PolizaLookupResponseDto>(
                sql,
                new { CodPolizaActual = codPolizaActual }) ?? new PolizaLookupResponseDto();

            return DbHelper.CreateOkResponse<PolizaLookupResponseDto>(result);
        }
    }
}
