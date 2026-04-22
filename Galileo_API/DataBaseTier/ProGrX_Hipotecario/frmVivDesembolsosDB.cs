using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivDesembolsosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmVivDesembolsosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmVivDesembolsosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            var response = new ErrorDto<List<OperacionBusquedaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
                                SELECT TOP 10
                                    id_solicitud AS operacion,
                                    RTRIM(codigo) AS codigo,
                                    RTRIM(cedula) AS cedula,
                                    montoapr,
                                    saldo
                                FROM reg_creditos
                                WHERE estadosol = 'F'
                                ORDER BY id_solicitud
                            ";

                response.Result = cn.Query<OperacionBusquedaDto>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

    }
}
