using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;
using System.Data;

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
        /// Obtiene la cantidad de garantías registradas para una operación.
        /// Replica fxTraerNumGarantiasOperacion de VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="operacion">Número de operación.</param>
        /// <returns>Cantidad de garantías.</returns>
        public ErrorDto<int> FrmVivGarantiaCantidadGarantias_Obtener(
            int codEmpresa,
            long operacion)
        {
            const string query = @"
SELECT
    COUNT(1) AS cantidad
FROM ViviendaGarantia
WHERE NumeroOperacion = @operacion;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                0,
                new
                {
                    operacion
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


        /// <summary>
        /// Obtiene el estado de una operación para validar si permite movimientos.
        /// Replica la validación fxEstadoOperacion usada por VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="numeroOperacion">Número de operación.</param>
        /// <returns>Estado de la operación.</returns>
        public ErrorDto<string> FrmVivGarantiaEstadoOperacion_Obtener(
            int codEmpresa,
            long numeroOperacion)
        {
            const string query = @"
SELECT TOP 1
    RTRIM(ISNULL(R.ESTADOSOL, '')) AS estado
FROM REG_CREDITOS AS R
WHERE R.ID_SOLICITUD = @numero_operacion;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                string.Empty,
                new
                {
                    numero_operacion = numeroOperacion
                }
            )!;
        }

        /// <summary>
        /// Valida si el cambio de grado hipotecario permite continuar según detalle de acreedores.
        /// Replica fxValidaDetalleGarantia de VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="idGarantia">Id de garantía.</param>
        /// <param name="gradoHipoteca">Nuevo grado hipotecario.</param>
        /// <returns>True si permite guardar.</returns>
        public ErrorDto<bool> FrmVivGarantiaDetalleGrado_Validar(
            int codEmpresa,
            long idGarantia,
            string gradoHipoteca)
        {
            string query = string.Empty;

            if (gradoHipoteca == "P")
            {
                query = @"
SELECT
    CASE WHEN COUNT(1) > 0 THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS permite
FROM ViviendaGarantiaDetalle
WHERE IdGarantia = @id_garantia;";
            }

            if (gradoHipoteca == "S")
            {
                query = @"
SELECT
    CASE WHEN COUNT(1) > 0 THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS permite
FROM ViviendaGarantiaDetalle
WHERE IdGarantia = @id_garantia
  AND GradoHipoteca IN ('S', 'T');";
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return new ErrorDto<bool>
                {
                    Code = 0,
                    Description = string.Empty,
                    Result = true
                };
            }

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                query,
                true,
                new
                {
                    id_garantia = idGarantia
                }
            )!;
        }

        /// <summary>
        /// Guarda o modifica una garantía hipotecaria.
        /// Replica ObjAgregar.fxViviendaGarantia usando spCRDVivGarantia_A.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la garantía.</param>
        /// <returns>Id de garantía generado o actualizado.</returns>
        public ErrorDto<FrmVivGarantiaGuardarResponse> FrmVivGarantiaGuardar(
            int codEmpresa,
            FrmVivGarantiaGuardarRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var parametros = new DynamicParameters();

                parametros.Add("@IdGarantia", request.id_garantia, DbType.Int32, ParameterDirection.InputOutput);
                parametros.Add("@IdZona", request.id_zona, DbType.Int32);
                parametros.Add("@UbicacionProvincia", request.ubicacion_provincia.ToString(), DbType.String);
                parametros.Add("@UbicacionCanton", request.ubicacion_canton.ToString(), DbType.String);
                parametros.Add("@UbicacionDistrito", request.ubicacion_distrito?.ToString(), DbType.String);
                parametros.Add("@NumeroOperacion", request.numero_operacion.ToString(), DbType.String);
                parametros.Add("@NumeroFinca", request.numero_finca.Trim(), DbType.String);
                parametros.Add("@TipoDerecho", request.tipo_derecho.Trim(), DbType.String);
                parametros.Add("@NumPlanoCatastro", request.num_plano_catastro.Trim(), DbType.String);
                parametros.Add("@GradoHipoteca", request.grado_hipoteca.Trim(), DbType.String);
                parametros.Add("@AreaFinca", request.area_finca.ToString(), DbType.String);
                parametros.Add("@Estado", "S", DbType.String);
                parametros.Add("@Direccion", TextoONulo(request.direccion), DbType.String);
                parametros.Add("@AnotacionesFinca", TextoONulo(request.anotaciones_finca), DbType.String);
                parametros.Add("@Gravamenes", TextoONulo(request.gravamenes), DbType.String);
                parametros.Add("@AnotacionesGravamen", TextoONulo(request.anotaciones_gravamen), DbType.String);
                parametros.Add("@ObservacionAvaluo", null, DbType.String);
                parametros.Add("@RegistroUsuario", request.registro_usuario.Trim(), DbType.String);
                parametros.Add("@RegistroFecha", null, DbType.String);
                parametros.Add("@CoberturaPrimerGrado", BoolSmallInt(request.cobertura_primer_grado), DbType.Int16);
                parametros.Add("@RegistraCalAvaluo", BoolSmallInt(request.registrar_calculo_avaluo), DbType.Int16);
                parametros.Add("@RegistraCalHonorarios", BoolSmallInt(request.registrar_calculo_honorarios), DbType.Int16);
                parametros.Add("@RegistraCalHonorariosDT", BoolSmallInt(request.registrar_detalle_manual), DbType.Int16);
                parametros.Add("@Tipo_Poliza", request.tipo_poliza.Trim(), DbType.String);
                parametros.Add("@CodPreanalisis", request.expediente.Trim(), DbType.String);

                long idGarantia = conn.QueryFirstOrDefault<long>(
                    "dbo.spCRDVivGarantia_A",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (idGarantia <= 0)
                {
                    idGarantia = parametros.Get<int>("@IdGarantia");
                }

                return new FrmVivGarantiaGuardarResponse
                {
                    id_garantia = idGarantia
                };
            });
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
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<FrmVivGarantiaDetalleResponse>
                {
                    Code = -1,
                    Description = "Debe indicar una garantía válida.",
                    Result = new FrmVivGarantiaDetalleResponse()
                };
            }

            const string query = @"
                    SELECT
                        VGarantia.IdGarantia AS id_garantia,
                        RTRIM(ISNULL(VGarantia.NumeroFinca, '')) AS numero_finca,
                        RTRIM(ISNULL(VGarantia.TipoDerecho, '')) AS tipo_derecho,
                        RTRIM(ISNULL(VGarantia.NumPlanoCatastro, '')) AS num_plano_catastro,
                        ISNULL(VGarantia.AreaFinca, 0) AS area_finca,
                        RTRIM(ISNULL(VGarantia.GradoHipoteca, '')) AS grado_hipoteca,

                        VGarantia.UbicacionProvincia AS ubicacion_provincia,
                        VGarantia.UbicacionCanton AS ubicacion_canton,
                        VGarantia.UbicacionDistrito AS ubicacion_distrito,
                        VGarantia.IdZona AS id_zona,
                        RTRIM(ISNULL(VGarantia.Direccion, '')) AS direccion,

                        RTRIM(ISNULL(VGarantia.Tipo_Poliza, '')) AS tipo_poliza,
                        RTRIM(ISNULL(VGarantia.AnotacionesFinca, '')) AS anotaciones_finca,
                        RTRIM(ISNULL(VGarantia.Gravamenes, '')) AS gravamenes,
                        RTRIM(ISNULL(VGarantia.AnotacionesGravamen, '')) AS anotaciones_gravamen,
                        RTRIM(ISNULL(VGarantia.ObservacionAvaluo, '')) AS observacion_avaluo,

                        CAST(ISNULL(VGarantia.CoberturaPrimerGrado, 0) AS bit) AS cobertura_primer_grado,
                        CAST(ISNULL(VGarantia.RegistraCalAvaluo, 0) AS bit) AS registrar_calculo_avaluo,
                        CAST(ISNULL(VGarantia.RegistraCalHonorarios, 0) AS bit) AS registrar_calculo_honorarios,
                        CAST(ISNULL(VGarantia.RegistraCalHonorariosDT, 0) AS bit) AS registrar_detalle_manual,

                        CAST(NULL AS datetime) AS fecha_inspeccion,
                        CAST(0 AS decimal(18, 2)) AS viaticos,
                        ISNULL(VGarantia.ValorTerreno, 0) AS valor_terreno,
                        CAST(0 AS decimal(18, 2)) AS valor_construccion,
                        ISNULL(VGarantia.ValorTerreno, 0) AS valor_total_inmueble,

                        CAST('' AS varchar(250)) AS ingeniero_nombre,
                        CAST('' AS varchar(250)) AS abogado_nombre,
                        CAST('' AS varchar(1)) AS tipo_poliza_avaluo,

                        ISNULL(VGarantia.MontoNoGravable, 0) AS monto_no_gravable
                    FROM DISTRITOS AS D
                    RIGHT JOIN ViviendaGarantia AS VGarantia
                        INNER JOIN CANTONES AS C
                            INNER JOIN PROVINCIAS AS P
                                ON C.PROVINCIA = P.PROVINCIA
                            ON VGarantia.UbicacionProvincia = C.PROVINCIA
                            AND VGarantia.UbicacionCanton = C.CANTON
                        ON D.DISTRITO = VGarantia.UbicacionDistrito
                        AND D.CANTON = VGarantia.UbicacionCanton
                        AND D.PROVINCIA = VGarantia.UbicacionProvincia
                    LEFT JOIN ViviendaZonas AS vZonas
                        ON VGarantia.IdZona = vZonas.IdZona
                    WHERE VGarantia.IdGarantia = @id_garantia;";

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

        /// <summary>
        /// Busca profesionales de vivienda para frmBusquedas.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtro, tipo profesional y paginación.</param>
        /// <returns>Lista paginada de profesionales.</returns>
        public ErrorDto<List<FrmVivGarantiaProfesionalItem>> FrmVivGarantiaProfesionales_Buscar(
            int codEmpresa,
            FrmVivGarantiaProfesionalesBuscarRequest request)
        {
            const string query = @"
                    SELECT
                        IdContacto AS id_contacto,
                        RTRIM(ISNULL(Identificacion, '')) AS identificacion,
                        RTRIM(ISNULL(Nombre, '')) AS nombre,
                        COUNT(1) OVER() AS total
                    FROM ViviendaContactos
                    WHERE TipoProfesional = @tipo_profesional
                      AND (
                            @filtro = ''
                            OR CONVERT(VARCHAR(30), IdContacto) LIKE '%' + @filtro + '%'
                            OR Identificacion LIKE '%' + @filtro + '%'
                            OR Nombre LIKE '%' + @filtro + '%'
                          )
                    ORDER BY Nombre
                    OFFSET @first ROWS
                    FETCH NEXT @rows ROWS ONLY;";

            return DbHelper.ExecuteListQuery<FrmVivGarantiaProfesionalItem>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    filtro = request.filtro.Trim(),
                    tipo_profesional = request.tipo_profesional.Trim(),
                    first = request.first,
                    rows = request.rows <= 0 ? 30 : request.rows
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
            )!;
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

        /// <summary>
        /// Guarda o modifica un dueño de garantía.
        /// Replica ObjAgregar.fxDerechosGarantia usando spCRDVivDerechosGarantia_A.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del dueño.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FrmVivGarantiaDerecho_Guardar(
            int codEmpresa,
            FrmVivGarantiaDerechoGuardarRequest request)
        {
            const string query = @"
EXEC dbo.spCRDVivDerechosGarantia_A
    @Actualiza,
    @Cedula,
    @IdGarantia,
    @Provincia,
    @Canton,
    @Distrito,
    @Nombre,
    @Direccion,
    @RegistroUsuario,
    @RegistroFecha;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Actualiza = request.actualiza,
                    Cedula = request.cedula.Trim(),
                    IdGarantia = request.id_garantia,
                    Provincia = request.provincia.ToString(),
                    Canton = request.canton.ToString(),
                    Distrito = request.distrito?.ToString(),
                    Nombre = request.nombre.Trim(),
                    Direccion = string.IsNullOrWhiteSpace(request.direccion) ? null : request.direccion.Trim(),
                    RegistroUsuario = request.registro_usuario.Trim(),
                    RegistroFecha = (string?)null
                }
            );
        }

        /// <summary>
        /// Borra un dueño registrado de una garantía.
        /// Replica ObjBorrar.fxDerechoDeGarantia de VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Id de garantía y cédula.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FrmVivGarantiaDerecho_Borrar(
            int codEmpresa,
            FrmVivGarantiaDerechoBorrarRequest request)
        {
            const string query = @"
DELETE dbo.ViviendaDerechosGarantia
WHERE IdGarantia = @id_garantia
  AND Cedula = @cedula;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    id_garantia = request.id_garantia,
                    cedula = request.cedula.Trim()
                }
            );
        }

        /// <summary>
        /// Valida si existe un contacto de vivienda por tipo profesional.
        /// Replica fxTraerExisteContacto de VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="idContacto">Id del contacto.</param>
        /// <param name="tipoProfesional">Tipo profesional: I ingeniero, A abogado.</param>
        /// <returns>True si existe.</returns>
        public ErrorDto<bool> FrmVivGarantiaContacto_Existe(
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
                _portalDb,
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

        /// <summary>
        /// Guarda el avalúo posterior de una garantía.
        /// Replica ObjAgregar.fxRegistroAvaluoPosterior usando spCRDVivGarantiaAvaluo_Posterior.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del avalúo posterior.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FrmVivGarantiaAvaluoPosterior_Guardar(
            int codEmpresa,
            FrmVivGarantiaAvaluoPosteriorRequest request)
        {
            const string query = @"
                EXEC dbo.spCRDVivGarantiaAvaluo_Posterior
                    @IdGarantia,
                    @IdContacto,
                    @FechaInspeccion,
                    @ValorTerreno,
                    @ValorConstruccion,
                    @ObservacionesAvaluo,
                    @RegistroUsuario,
                    @RegistroFecha,
                    @Viaticos,
                    @Tipo_Poliza,
                    @IdAbogado;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    IdGarantia = request.id_garantia,
                    IdContacto = request.id_ingeniero,
                    FechaInspeccion = request.fecha_inspeccion?.ToString("yyyy-MM-dd"),
                    ValorTerreno = request.valor_terreno,
                    ValorConstruccion = request.valor_construccion,
                    ObservacionesAvaluo = string.IsNullOrWhiteSpace(request.observaciones_avaluo)
                        ? null
                        : request.observaciones_avaluo.Trim(),
                    RegistroUsuario = request.registro_usuario.Trim(),
                    RegistroFecha = (string?)null,
                    Viaticos = request.viaticos,
                    Tipo_Poliza = request.tipo_poliza.Trim(),
                    IdAbogado = request.id_abogado
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
            )!;
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
                    TipoProfesional = request.tipo.Trim()
                }
            );
        }

        #endregion

        /// <summary>
        /// Convierte valores booleanos a smallint para mantener compatibilidad con el SP legado.
        /// </summary>
        private static short BoolSmallInt(bool valor)
        {
            return Convert.ToInt16(valor);
        }

        /// <summary>
        /// Normaliza textos vacíos como null para enviar a base de datos.
        /// </summary>
        private static string? TextoONulo(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            return valor.Trim();
        }
    }
}
