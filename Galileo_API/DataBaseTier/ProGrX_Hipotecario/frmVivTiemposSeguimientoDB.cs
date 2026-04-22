using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivTiemposSeguimientoDb
    {
        private readonly PortalDB _portalDb;

        public FrmVivTiemposSeguimientoDb(IConfiguration config)
           : this(
                 new PortalDB(config))
        {
        }

        public FrmVivTiemposSeguimientoDb(PortalDB portalDB)
        {
            _portalDb = portalDB;
        }

        /// <summary>
        /// Obtiene lista de tiempos de seguimiento
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<VivTiemposSeguimientoData>> VivTiemposSeguimiento_Obtener(int codEmpresa)
        {
            const string querySP = @"exec spCRDVivTiemposSeguimiento";
            DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, querySP);

            const string query = @"SELECT 
                DescProceso = CASE Proceso WHEN 'E' THEN 'Entrega de Garantía'
                WHEN 'F' THEN 'Registro de Firmas'
                WHEN 'I' THEN 'Inscripción de garantía'
                WHEN 'X' THEN 'Recepción Información Avaluo'
                WHEN 'R' THEN 'Registro Información Avaluo'
                Else Proceso END, TiempoMaximo, TiempoAlerta, Proceso, Profesional,
                DescProfesional = 
                CASE Profesional WHEN 'A' THEN 'Abogado'
                WHEN 'I' THEN 'Ingeniero' end 
            From ViviendaTiemposSeguimiento
            ORDER BY Profesional, orden";
            return DbHelper.ExecuteListQuery<VivTiemposSeguimientoData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Guarda el tiempo de seguimiento
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivTiemposSeguimiento_Guardar(int codEmpresa, VivTiemposSeguimientoData request)
        {
            const string sql = @"Update dbo.ViviendaTiemposSeguimiento SET TiempoMaximo = @TMaximo, 
                TiempoAlerta = @TAlerta WHERE Profesional = @Profesional AND Proceso = @Proceso";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    TMaximo = request.tiempomaximo,
                    TAlerta = request.tiempoalerta,
                    Profesional = request.profesional,
                    Proceso = request.proceso
                });
        }
    }
}
