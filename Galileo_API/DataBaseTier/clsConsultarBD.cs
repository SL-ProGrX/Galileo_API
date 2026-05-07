using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier
{
    public class ClsConsultarBD
    {
        private readonly PortalDB _portalDB;

        public ClsConsultarBD(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las garantías asociadas a una operación o expediente.
        /// Replica la consulta VB6 del método fxTraerGarantiasxOperacion.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Request con operación o expediente.</param>
        /// <returns>Listado de garantías encontradas.</returns>
        public ErrorDto<List<FrmVivGarantiaOperacionGarantiaItem>> Viv_GarantiaTraerGarantiasxOperacion(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
            const string query = @"
SELECT
    vGarantia.IdGarantia AS id_garantia,
    vGarantia.UbicacionCanton AS ubicacion_canton,
    vGarantia.UbicacionDistrito AS ubicacion_distrito,
    vGarantia.IdZona AS id_zona,
    vGarantia.NumeroOperacion AS numero_operacion,
    RTRIM(ISNULL(vGarantia.NumeroFinca, '')) AS numero_finca,
    RTRIM(ISNULL(vGarantia.TipoDerecho, '')) AS tipo_derecho,
    RTRIM(ISNULL(vGarantia.NumPlanoCatastro, '')) AS num_plano_catastro,
    RTRIM(ISNULL(vGarantia.GradoHipoteca, '')) AS grado_hipoteca,
    CASE vGarantia.GradoHipoteca
        WHEN 'P' THEN 'Primer Grado'
        WHEN 'S' THEN 'Segundo Grado'
        WHEN 'T' THEN 'Tercer Grado'
        ELSE ''
    END AS desc_grado_hipoteca,
    ISNULL(vGarantia.AreaFinca, 0) AS area_finca,
    RTRIM(ISNULL(vGarantia.Estado, '')) AS estado,
    RTRIM(ISNULL(vGarantia.Direccion, '')) AS direccion,
    RTRIM(ISNULL(vGarantia.AnotacionesFinca, '')) AS anotaciones_finca,
    RTRIM(ISNULL(vGarantia.Gravamenes, '')) AS gravamenes,
    RTRIM(ISNULL(vGarantia.AnotacionesGravamen, '')) AS anotaciones_gravamen,
    RTRIM(ISNULL(vGarantia.ObservacionAvaluo, '')) AS observacion_avaluo,
    RTRIM(ISNULL(vGarantia.RegistroUsuario, '')) AS registro_usuario,
    vGarantia.RegistroFecha AS registro_fecha,
    RTRIM(ISNULL(vZona.Descripcion, '')) AS desc_zona,
    RTRIM(ISNULL(P.Descripcion, '')) AS desc_provincia,
    RTRIM(ISNULL(C.Descripcion, '')) AS desc_canton,
    RTRIM(ISNULL(D.Descripcion, '')) AS desc_distrito
FROM ViviendaZonas AS vZona
INNER JOIN PROVINCIAS AS P
    INNER JOIN CANTONES AS C
        INNER JOIN ViviendaGarantia AS vGarantia
            ON C.CANTON = vGarantia.UbicacionCanton
            AND C.PROVINCIA = vGarantia.UbicacionProvincia
        ON P.PROVINCIA = vGarantia.UbicacionProvincia
    ON vZona.IdZona = vGarantia.IdZona
LEFT JOIN DISTRITOS AS D
    ON vGarantia.UbicacionProvincia = D.PROVINCIA
    AND vGarantia.UbicacionCanton = D.CANTON
    AND vGarantia.UbicacionDistrito = D.DISTRITO
WHERE (
        @operacion > 0
        AND vGarantia.NumeroOperacion = @operacion
      )
   OR (
        @operacion <= 0
        AND vGarantia.Cod_PreAnalisis = @expediente
      );";

            return DbHelper.ExecuteListQuery<FrmVivGarantiaOperacionGarantiaItem>(
                _portalDB,
                codEmpresa,
                query,
                new
                {
                    operacion = request.operacion,
                    expediente = request.expediente.Trim()
                }
            );
        }

        public ErrorDto<string> fxEstadoOperacion(
    int codEmpresa,
    long numeroOperacion)
        {
            const string query = @"
SELECT TOP 1
    RTRIM(ISNULL(R.ESTADOSOL, '')) AS estado
FROM REG_CREDITOS AS R
WHERE R.ID_SOLICITUD = @numero_operacion;";

            return DbHelper.ExecuteSingleQuery(
                _portalDB,
                codEmpresa,
                query,
                string.Empty,
                new
                {
                    numero_operacion = numeroOperacion
                }
            )!;
        }

        public ErrorDto<bool> fxTraerExisteContacto(
    int codEmpresa,
    long idContacto,
    string tipoProfesional)
        {
            const string query = @"
SELECT
    CASE WHEN COUNT(1) > 0 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS existe
FROM ViviendaContactos
WHERE TipoProfesional = @tipo_profesional
  AND IdContacto = @id_contacto;";

            return DbHelper.ExecuteSingleQuery(
                _portalDB,
                codEmpresa,
                query,
                false,
                new
                {
                    id_contacto = idContacto,
                    tipo_profesional = tipoProfesional.Trim()
                }
            )!;
        }

    }
}
