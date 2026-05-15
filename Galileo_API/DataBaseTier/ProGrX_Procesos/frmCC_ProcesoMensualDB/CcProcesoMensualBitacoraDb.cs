using Dapper;
using Galileo.DataBaseTier; 
using Galileo.Models.ERROR;
using System.Data; 
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualEstadoModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualBitacoraDb
    {
        private readonly PortalDB _portalDb;  

        public CcProcesoMensualBitacoraDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);  

        }
        public ErrorDto<List<CcProcesoMensualBitacoraDbModel>> CcProcesoMensual_Bitacora_Obtener(int codEmpresa, int codInstitucion, int proceso)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
            SELECT
                id_seq AS Id_Seq,
                gestion AS Gestion,
                transaccion AS Transaccion,
                documento AS Documento,
                usuario AS Usuario,
                fecha AS Fecha
            FROM prm_bitacora
            WHERE cod_institucion = @CodInstitucion
              AND proceso = @Proceso
            ORDER BY id_seq";

                var parametros = new
                {
                    CodInstitucion = codInstitucion,
                    Proceso = proceso
                };

                return conn.Query<CcProcesoMensualBitacoraDbModel>(query, parametros)
                    .Select(MapearBitacora)
                    .ToList();
            });
        }
        private static CcProcesoMensualBitacoraDbModel MapearBitacora(CcProcesoMensualBitacoraDbModel item)
        {
            return new CcProcesoMensualBitacoraDbModel
            {
                Id_Seq = item.Id_Seq,
                Gestion = ObtenerGestionDescripcion(item.Gestion),
                Transaccion = MProcesoMensualDb.FxPlanillaTipoTransac(item.Transaccion),
                Documento = item.Documento,
                Usuario = item.Usuario,
                Fecha = item.Fecha
            };
        }
        private static string ObtenerGestionDescripcion(string gestion)
        {
            return string.Equals(gestion, "R", StringComparison.OrdinalIgnoreCase)
                ? "Recepción"
                : "Envio";
        }

    }
}
