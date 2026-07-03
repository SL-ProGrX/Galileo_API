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

        /// <summary>
        /// Inicializa una nueva instancia para consultar la bitácora del proceso mensual.
        /// </summary>
        /// <param name="config">Configuración general de la aplicación.</param>
        public CcProcesoMensualBitacoraDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);  

        }

        /// <summary>
        /// Obtiene la bitácora de proceso mensual por institución y período.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="proceso">Período del proceso.</param>
        /// <returns>Listado de registros de bitácora.</returns>
        public ErrorDto<List<CcProcesoMensualBitacoraDbModel>> CcProcesoMensual_Bitacora_Obtener(int codEmpresa, int codInstitucion, decimal proceso)
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

        /// <summary>
        /// Mapea un registro de bitácora con descripciones funcionales.
        /// </summary>
        /// <param name="item">Registro original de bitácora.</param>
        /// <returns>Registro transformado para respuesta.</returns>
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

        /// <summary>
        /// Obtiene la descripción textual de la gestión según su código.
        /// </summary>
        /// <param name="gestion">Código de gestión.</param>
        /// <returns>Descripción de la gestión.</returns>
        private static string ObtenerGestionDescripcion(string gestion)
        {
            return string.Equals(gestion, "R", StringComparison.OrdinalIgnoreCase)
                ? "Recepción"
                : "Envio";
        }

    }
}
