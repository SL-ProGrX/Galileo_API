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
            )!;
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
                Result = general.Result!.Select(g => new FrmVivGarantiaGeneralItem
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
  null AS tipo_poliza_avaluo,

RTRIM(ISNULL(vGarantia.Gravamenes, '')) AS gravamenes,
RTRIM(ISNULL(vGarantia.AnotacionesGravamen, '')) AS anotaciones_gravamen,
ISNULL(vGarantia.MontoNoGravable, 0) AS monto_no_gravable

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
            )!;
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

        // <summary>
        /// Obtiene la lista de dueños registrados para una garantía.
        /// Replica la consulta VB6 fxTraerListaDuenosxGarantia.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Id de garantía.</param>
        /// <returns>Lista de dueños de la garantía.</returns>
        public ErrorDto<List<FrmVivGarantiaDerechoDuenoItem>> FrmVivGarantiaDerechos_Listar(
            int codEmpresa,
            FrmVivGarantiaIdGarantiaRequest request)
        {
            const string query = @"
                    SELECT
                        VDG.IdGarantia AS id_garantia,
                        RTRIM(ISNULL(VDG.Cedula, '')) AS cedula,
                        RTRIM(ISNULL(VDG.Nombre, '')) AS nombre,
                        VDG.PROVINCIA AS provincia_id,
                        VDG.CANTON AS canton_id,
                        VDG.DISTRITO AS distrito_id,
                        RTRIM(ISNULL(VDG.Direccion, '')) AS direccion,
                        RTRIM(ISNULL(P.DESCRIPCION, '')) AS desc_provincia,
                        RTRIM(ISNULL(C.DESCRIPCION, '')) AS desc_canton,
                        RTRIM(ISNULL(D.DESCRIPCION, '')) AS desc_distrito,
                        RTRIM(ISNULL(VDG.RegistroUsuario, '')) AS registro_usuario,
                        VDG.RegistroFecha AS registro_fecha
                    FROM CANTONES AS C
                    INNER JOIN PROVINCIAS AS P
                    INNER JOIN ViviendaDerechosGarantia AS VDG
                        ON P.PROVINCIA = VDG.PROVINCIA
                        ON C.CANTON = VDG.CANTON
                        AND C.PROVINCIA = VDG.PROVINCIA
                    LEFT JOIN DISTRITOS AS D
                        ON VDG.PROVINCIA = D.PROVINCIA
                        AND VDG.CANTON = D.CANTON
                        AND VDG.DISTRITO = D.DISTRITO
                    WHERE VDG.IdGarantia = @id_garantia
                    ORDER BY VDG.Cedula;";

            return DbHelper.ExecuteListQuery<FrmVivGarantiaDerechoDuenoItem>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    id_garantia = request.id_garantia
                }
            );
        }

        /// <summary>
        /// Obtiene el nombre de un socio por cédula.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Cédula del socio.</param>
        /// <returns>Datos básicos del socio.</returns>
        public ErrorDto<FrmVivGarantiaSocioItem> FrmVivGarantiaSocio_Obtener(
            int codEmpresa,
            FrmVivGarantiaSocioRequest request)
        {
            const string query = @"
SELECT
    RTRIM(ISNULL(Cedula, '')) AS cedula,
    RTRIM(ISNULL(Nombre, '')) AS nombre
FROM Socios
WHERE Cedula = @cedula;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                new FrmVivGarantiaSocioItem(),
                new
                {
                    cedula = request.cedula.Trim()
                }
            );
        }


        /// <summary>
        /// Busca socios para el formulario compartido frmBusquedas.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtro y paginación.</param>
        /// <returns>Lista paginada de socios.</returns>
        public ErrorDto<List<FrmVivGarantiaSocioItem>> FrmVivGarantiaSocios_Buscar(
            int codEmpresa,
            FrmVivGarantiaSociosBuscarRequest request)
        {
            const string query = @"
SELECT
    RTRIM(ISNULL(Cedula, '')) AS cedula,
    RTRIM(ISNULL(Nombre, '')) AS nombre,
    COUNT(1) OVER() AS total
FROM Socios
WHERE @filtro = ''
   OR Cedula LIKE '%' + @filtro + '%'
   OR Nombre LIKE '%' + @filtro + '%'
ORDER BY Nombre
OFFSET @first ROWS
FETCH NEXT @rows ROWS ONLY;";

            return DbHelper.ExecuteListQuery<FrmVivGarantiaSocioItem>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    filtro = request.filtro.Trim(),
                    first = request.first,
                    rows = request.rows <= 0 ? 30 : request.rows
                }
            );
        }
        #endregion

        #region Historial del Tramite

        /// <summary>
        /// Obtiene la información del historial del trámite por garantía y tipo profesional.
        /// Replica spCRDVivInfoTramite_T usado por VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="idGarantia">Id de garantía.</param>
        /// <param name="tipo">Tipo profesional: I ingeniero, A abogado.</param>
        /// <returns>Información del trámite.</returns>
        public ErrorDto<FrmVivGarantiaHistorialRawItem> FrmVivGarantiaHistorial_ObtenerPorTipo(
            int codEmpresa,
            long idGarantia,
            string tipo)
        {
            const string query = @"
              EXEC spCRDVivInfoTramite_T @IdGarantia, @tipo;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                new FrmVivGarantiaHistorialRawItem(),
                new
                {
                    IdGarantia = idGarantia,
                    tipo = tipo.Trim()
                }
            );
        }

        #endregion

        #region Fincas

        /// <summary>
        /// Obtiene la lista de fincas asociadas a una operación o expediente.
        /// Replica sbFincas_Asociadas del VB6 usando spCrd_Fincas_Asociadas.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Operación o expediente.</param>
        /// <returns>Lista de fincas asociadas.</returns>
        public ErrorDto<List<FrmVivGarantiaFincaAsociadaItem>> FrmVivGarantiaFincasAsociadas_Listar(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
            const string query = @"
                    CREATE TABLE #FincasAsociadas (
                        IdGarantia BIGINT NULL,
                        NumeroOperacion BIGINT NULL,
                        NumeroFinca VARCHAR(100) NULL,
                        NumPlanoCatastro VARCHAR(100) NULL,
                        Estado VARCHAR(20) NULL,
                        ValorTerreno DECIMAL(18, 2) NULL,
                        ValorConstruccion DECIMAL(18, 2) NULL,
                        AreaFinca DECIMAL(18, 2) NULL,
                        GradoHipoteca VARCHAR(50) NULL,
                        TipoPoliza VARCHAR(50) NULL,
                        Cedula VARCHAR(50) NULL,
                        Nombre VARCHAR(300) NULL,
                        Linea_Estado VARCHAR(50) NULL,
                        Saldo DECIMAL(18, 2) NULL,
                        Codigo VARCHAR(50) NULL,
                        Linea_Desc VARCHAR(300) NULL,
                        Poliza_Id BIGINT NULL,
                        Poliza_Cuota DECIMAL(18, 2) NULL,
                        Poliza_Estado VARCHAR(50) NULL,
                        Poliza_Codigo VARCHAR(50) NULL,
                        Poliza_Desc VARCHAR(300) NULL,
                        Tipo_Aplicacion VARCHAR(50) NULL
                    );

                    INSERT INTO #FincasAsociadas
                    EXEC spCrd_Fincas_Asociadas @operacion, @expediente;

                    SELECT
                        ISNULL(IdGarantia, 0) AS id_garantia,
                        ISNULL(NumeroOperacion, 0) AS numero_operacion,
                        RTRIM(ISNULL(NumeroFinca, '')) AS numero_finca,
                        RTRIM(ISNULL(NumPlanoCatastro, '')) AS num_plano_catastro,
                        RTRIM(ISNULL(Estado, '')) AS estado,
                        ISNULL(ValorTerreno, 0) AS valor_terreno,
                        ISNULL(ValorConstruccion, 0) AS valor_construccion,
                        ISNULL(AreaFinca, 0) AS area_finca,
                        RTRIM(ISNULL(GradoHipoteca, '')) AS grado_hipoteca,
                        RTRIM(ISNULL(TipoPoliza, '')) AS tipo_poliza,
                        RTRIM(ISNULL(Cedula, '')) AS cedula,
                        RTRIM(ISNULL(Nombre, '')) AS nombre,
                        RTRIM(ISNULL(Linea_Estado, '')) AS linea_estado,
                        ISNULL(Saldo, 0) AS saldo,
                        RTRIM(ISNULL(Codigo, '')) AS codigo,
                        RTRIM(ISNULL(Linea_Desc, '')) AS linea_desc,
                        ISNULL(Poliza_Id, 0) AS poliza_id,
                        ISNULL(Poliza_Cuota, 0) AS poliza_cuota,
                        RTRIM(ISNULL(Poliza_Estado, '')) AS poliza_estado,
                        RTRIM(ISNULL(Poliza_Codigo, '')) AS poliza_codigo,
                        RTRIM(ISNULL(Poliza_Desc, '')) AS poliza_desc,
                        RTRIM(ISNULL(Tipo_Aplicacion, '')) AS tipo_aplicacion
                    FROM #FincasAsociadas;

                    DROP TABLE #FincasAsociadas;";

            return DbHelper.ExecuteListQuery<FrmVivGarantiaFincaAsociadaItem>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    operacion = request.operacion,
                    expediente = request.expediente.Trim()
                }
            );
        }

        #endregion

        #region Notas

        /// <summary>
        /// Obtiene las notas del trámite por garantía y tipo profesional.
        /// Replica sbHipoteca_Tramite_Notas de VB6 usando spCrdViv_Garantia_TramiteNotas.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Id de garantía y tipo profesional: A abogado, I ingeniero.</param>
        /// <returns>Lista de notas del trámite.</returns>
        public ErrorDto<List<FrmVivGarantiaNotaTramiteRawItem>> FrmVivGarantiaNotas_Listar(
            int codEmpresa,
            FrmVivGarantiaNotasRequest request)
        {
            const string query = @"
EXEC spCrdViv_Garantia_TramiteNotas @GarantiaId, @TipoProfesional;";

            return DbHelper.ExecuteListQuery<FrmVivGarantiaNotaTramiteRawItem>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    GarantiaId = request.id_garantia,
                    TipoProfesionaltipo = request.tipo.Trim()
                }
            );
        }

        #endregion

    }
}
