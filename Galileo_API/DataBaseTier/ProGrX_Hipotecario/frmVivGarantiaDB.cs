using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivGarantiaDB
    {
        private readonly PortalDB _portalDb;
        private readonly ClsConsultarBD _clsConsultar;

        public FrmVivGarantiaDB(IConfiguration confi)
        {
            _portalDb = new PortalDB(confi);
            _clsConsultar = new ClsConsultarBD(confi);
        }

        #region Principal

        /// <summary>
        /// Obtiene la información principal de la operación según número de operación o expediente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FrmVivGarantiaOperacionResponse> FrmVivGarantiaOperacion_Obtener(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
            string query;

            if (request.operacion > 0)
            {
                query = @"
SELECT
    RTRIM(S.CEDULA) AS cedula,
    RTRIM(S.NOMBRE) AS nombre,
    R.ID_SOLICITUD AS id_solicitud,
    RTRIM(R.CODIGO) AS codigo,
    RTRIM(C.DESCRIPCION) AS desc_linea,
    RTRIM(ISNULL(P.COD_PREANALISIS, '')) AS expediente,
    RTRIM(R.ESTADOSOL) AS estadosol
FROM REG_CREDITOS R
INNER JOIN SOCIOS S
    ON R.CEDULA = S.CEDULA
INNER JOIN CATALOGO C
    ON R.CODIGO = C.CODIGO
LEFT JOIN CRD_PREA_PREANALISIS P
    ON R.ID_SOLICITUD = P.ID_SOLICITUD
WHERE R.ID_SOLICITUD = @operacion;";
            }
            else
            {
                query = @"
SELECT
    RTRIM(S.CEDULA) AS cedula,
    RTRIM(S.NOMBRE) AS nombre,
    P.ID_SOLICITUD AS id_solicitud,
    RTRIM(P.COD_LINEA) AS codigo,
    RTRIM(C.DESCRIPCION) AS desc_linea,
    RTRIM(ISNULL(P.COD_PREANALISIS, '')) AS expediente,
    RTRIM(ISNULL(R.ESTADOSOL, 'P')) AS estadosol
FROM CRD_PREA_PREANALISIS P
INNER JOIN SOCIOS S
    ON P.CEDULA = S.CEDULA
INNER JOIN CATALOGO C
    ON P.COD_LINEA = C.CODIGO
LEFT JOIN REG_CREDITOS R
    ON P.ID_SOLICITUD = R.ID_SOLICITUD
WHERE P.COD_PREANALISIS = @expediente;";
            }

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                new FrmVivGarantiaOperacionResponse(),
                new
                {
                    operacion = request.operacion,
                    expediente = request.expediente.Trim()
                }
            );
        }

        /// <summary>
        /// Obtiene el catálogo de provincias para la garantía.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivGarantiaProvincias_Obtener(int codEmpresa)
        {
            const string query = @"
SELECT
    Provincia AS item,
    RTRIM(Descripcion) AS descripcion
FROM Provincias
ORDER BY Descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query
            );
        }

        /// <summary>
        /// Obtiene el catálogo de zonas para la garantía.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivGarantiaZonas_Obtener(int codEmpresa)
        {
            const string query = @"
SELECT
    IdZona AS item,
    RTRIM(Descripcion) AS descripcion
FROM ViviendaZonas
ORDER BY Descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query
            );
        }
        #endregion

        #region General

        /// <summary>
        /// Obtiene el listado general de garantías según número de operación o expediente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmVivGarantiaGeneralItem>> FrmVivGarantiaGeneral_Listar(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
           var general = _clsConsultar.FrmVivGarantiaTraerGarantiasxOperacion(codEmpresa, request);

            //mapeo los datos a la respuesta
            var result = new ErrorDto<List<FrmVivGarantiaGeneralItem>>
            {
                Result = general.Result.Select(g => new FrmVivGarantiaGeneralItem
                {
                    id_garantia = g.id_garantia,
                    numero_finca = g.numero_finca,
                    num_plano_catastro = g.num_plano_catastro,
                    tipo_derecho = g.tipo_derecho,
                    desc_grado_hipoteca = g.desc_grado_hipoteca,
                    area_finca = g.area_finca,
                    registro_usuario = g.registro_usuario,
                    registro_fecha = g.registro_fecha
                }).ToList()
            };

            return result;
        }

        #endregion

        #region Garantia

        /// <summary>
        /// Obtiene el detalle de una garantía por id.
        /// Replica la carga usada por sbTraerGarantia / sbLigarDatosGarantia.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Id de la garantía.</param>
        /// <returns>Detalle de la garantía.</returns>
        public ErrorDto<FrmVivGarantiaDetalleResponse> FrmVivGarantiaDetalle_Obtener(
            int codEmpresa,
            FrmVivGarantiaDetalleRequest request)
        {
            const string query = @"
SELECT
    vGarantia.IdGarantia AS id_garantia,
    RTRIM(ISNULL(vGarantia.NumeroFinca, '')) AS numero_finca,
    RTRIM(ISNULL(vGarantia.TipoDerecho, '')) AS tipo_derecho,
    RTRIM(ISNULL(vGarantia.NumPlanoCatastro, '')) AS num_plano_catastro,
    ISNULL(vGarantia.AreaFinca, 0) AS area_finca,
    RTRIM(ISNULL(vGarantia.GradoHipoteca, '')) AS grado_hipoteca,

    vGarantia.UbicacionProvincia AS ubicacion_provincia,
    vGarantia.UbicacionCanton AS ubicacion_canton,
    vGarantia.UbicacionDistrito AS ubicacion_distrito,
    vGarantia.IdZona AS id_zona,
    RTRIM(ISNULL(vGarantia.Direccion, '')) AS direccion,

    RTRIM(ISNULL(vGarantia.tipo_poliza, '')) AS tipo_poliza,
    RTRIM(ISNULL(vGarantia.AnotacionesFinca, '')) AS anotaciones_finca,
    RTRIM(ISNULL(vGarantia.ObservacionAvaluo, '')) AS observacion_avaluo,

   -- CAST(ISNULL(vGarantia.AplicaCoberturaPrimerGrado, 0) AS bit) 
   null AS cobertura_primer_grado,
    CAST(ISNULL(vGarantia.RegistraCalAvaluo, 0) AS bit) AS registrar_calculo_avaluo,
    CAST(ISNULL(vGarantia.RegistraCalHonorariosDT, 0) AS bit) AS registrar_calculo_honorarios,
  --  CAST(ISNULL(vGarantia.RegistraDetalleManual, 0) AS bit) 
    null AS registrar_detalle_manual,

    vGarantia.FechaInspeccion AS fecha_inspeccion,
    ISNULL(vGarantia.Viaticos, 0) AS viaticos,
    ISNULL(vGarantia.ValorTerreno, 0) AS valor_terreno,
    ISNULL(vGarantia.ValorConstruccion, 0) AS valor_construccion,
  --  ISNULL(vGarantia.ValorTotalInmueble, 0) AS 
    null as valor_total_inmueble,

  --  RTRIM(ISNULL(vGarantia.NombreIngeniero, '')) 
  null AS ingeniero_nombre,
  --  RTRIM(ISNULL(vGarantia.NombreAbogado, '')) 
   null AS abogado_nombre,
  --  RTRIM(ISNULL(vGarantia.TipoPolizaAvaluo, '')) 
  null AS tipo_poliza_avaluo
FROM ViviendaGarantia AS vGarantia
WHERE vGarantia.IdGarantia = @id_garantia;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                new FrmVivGarantiaDetalleResponse(),
                new
                {
                    id_garantia = request.id_garantia
                }
            );
        }

        /// <summary>
        /// Obtiene los cantones de una provincia.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Provincia a consultar.</param>
        /// <returns>Listado de cantones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivGarantiaCantones_Obtener(
            int codEmpresa,
            FrmVivGarantiaProvinciaRequest request)
        {
            const string query = @"
SELECT
    CANTON AS item,
    RTRIM(DESCRIPCION) AS descripcion
FROM CANTONES
WHERE PROVINCIA = @provincia
ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    provincia = request.provincia
                }
            );
        }


        /// <summary>
        /// Obtiene los distritos de una provincia y cantón.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Provincia y cantón a consultar.</param>
        /// <returns>Listado de distritos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivGarantiaDistritos_Obtener(
            int codEmpresa,
            FrmVivGarantiaCantonRequest request)
        {
            const string query = @"
SELECT
    DISTRITO AS item,
    RTRIM(DESCRIPCION) AS descripcion
FROM DISTRITOS
WHERE PROVINCIA = @provincia
  AND CANTON = @canton
ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    provincia = request.provincia,
                    canton = request.canton
                }
            );
        }

        #endregion

        #region Derechos
        #endregion

        #region Historial del Tramite
        #endregion

        #region Fincas
        #endregion

        #region Notas
        #endregion

    }
}
