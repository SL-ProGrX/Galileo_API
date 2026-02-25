using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRVerificaDatosPersonalesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _security_MainDB;
        private const int MODULO = 1;

        private const string USUARIO_SESION_INVALIDO = "Usuario de sesión inválido.";
        private const string IDENTIFICACION_INVALIDA = "Identificación inválida.";
        private const string TIPO_INVALIDO = "Tipo inválido.";
        private const string ITEM_INVALIDO = "Ítem inválido.";

        public FrmCRVerificaDatosPersonalesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _security_MainDB = new MSecurityMainDb(config);
        }
        /// <summary>
        /// Obtiene la informacion completa de la persona.
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrVerificaDatosCompletoDto> CR_VerificaDatos_Completo_Obtener(int CodEmpresa, string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
                return DbHelper.CreateErrorResponse<CrVerificaDatosCompletoDto>(IDENTIFICACION_INVALIDA, -2);

            var pId = identificacion.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();
                const string sql = "exec spAFI_Persona_Consulta @Cedula";
                var row = conn.QueryFirstOrDefault(sql, new { Cedula = pId });

                var dto = new CrVerificaDatosCompletoDto();

                if (row != null)
                {
                    var d = (IDictionary<string, object?>)row;

                    dto.cabecera = new CrVerificaDatosCabeceraDto
                    {
                        cod_institucion = S(V(d, "cod_institucion")),
                        institucion = S(V(d, "InstitucionDesc", "instituciondesc")),
                        cod_departamento = S(V(d, "cod_departamento")),
                        departamento = S(V(d, "DepartamentoDesc", "departamentodesc")),
                        cod_seccion = S(V(d, "cod_seccion")),
                        seccion = S(V(d, "SeccionDesc", "secciondesc")),
                        usuario = pId,
                        nombre = S(V(d, "Nombre", "nombre"))
                    };

                    dto.contacto = new CrVerificaDatosContactoDto
                    {
                        genero = S(V(d, "sexo", "Sexo")),
                        estado_civil = S(V(d, "EstadoCivilDesc", "estadocivildesc")),
                        nacionalidad = S(V(d, "Nacionalidad", "nacionalidad")),
                        nacimiento = S(V(d, "fecha_nac", "Fecha_Nac", "nacimiento")),
                        email_1 = S(V(d, "AF_Email", "af_email")),
                        email_2 = S(V(d, "Email_02", "email_02")),
                        apto_postal = S(V(d, "apto", "Apto")),
                        provincia = S(V(d, "ProvinciaDesc", "provinciadesc")),
                        canton = S(V(d, "CantonDesc", "cantondesc")),
                        distrito = S(V(d, "DistritoDesc", "distritodesc")),
                        direccion = S(V(d, "direccion", "Direccion")),
                        notificaciones = S(V(d, "Notificaciones", "notificaciones")),
                        provincia_cod = S(V(d, "Provincia", "provincia")),
                        canton_cod = S(V(d, "Canton", "canton")),
                        distrito_cod = S(V(d, "Distrito", "distrito")),
                        estado_civil_cod = S(V(d, "EstadoCivil", "estadocivil")),
                        cod_nacionalidad = S(V(d, "Cod_Nacionalidad", "COD_NACIONALIDAD", "cod_nacionalidad")),
                        estado_laboral = S(V(d, "EstadoLaboral", "estadolaboral", "ESTADOLABORAL")),
                        nombramiento_fecha = S(V(d, "Nombramiento_Fecha", "nombramiento_fecha")),
                        fecha_ingreso = S(V(d, "FechaIngreso", "fechaingreso", "fecha_ingreso", "FECHAINGRESO")),
                    };

                    dto.conyuge = new CrVerificaDatosConyugeDto
                    {
                        conyuge_identificacion = S(V(d, "conyuge_cedula", "Conyuge_Cedula")),
                        conyuge_nombre = S(V(d, "conyuge_nombre", "Conyuge_Nombre")),
                        conyuge_trabajo = S(V(d, "conyuge_TelTra", "Conyuge_TelTra")),
                        conyuge_extension = S(V(d, "conyuge_TelTraExt", "Conyuge_TelTraExt")),
                        conyuge_movil = S(V(d, "conyuge_TelCell", "Conyuge_TelCell")),
                        albacea_identificacion = S(V(d, "albacea_Cedula", "Albacea_cedula", "albacea_cedula")),
                        albacea_nombre = S(V(d, "albacea_nombre", "Albacea_nombre"))
                    };
                }
                dto.bienes = ObtenerChecklistCRM(conn, pId, "BIENES");
                dto.canales = ObtenerChecklistCRM(conn, pId, "CANALES");
                dto.gustos = ObtenerChecklistCRM(conn, pId, "GUSTOS");
                dto.escolaridad = ObtenerChecklistCRM(conn, pId, "ESCOLARIDAD");

                return DbHelper.CreateOkResponse(dto);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrVerificaDatosCompletoDto>(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene el catalogo de personas.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_VerificaDatos_Persona_F4_Obtener(int CodEmpresa, string? filtro)
        {
            var pFiltro = (filtro ?? string.Empty).Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                const string sqlAll = @"
                SELECT
                    RTRIM(LTRIM(ISNULL(CEDULA,''))) AS item,
                    RTRIM(LTRIM(ISNULL(NOMBRE,''))) AS descripcion
                FROM SOCIOS
                WHERE ISNULL(CEDULA,'') <> ''
                ORDER BY NOMBRE;";

                        const string sqlFiltrado = @"
                SELECT
                    RTRIM(LTRIM(ISNULL(CEDULA,''))) AS item,
                    RTRIM(LTRIM(ISNULL(NOMBRE,''))) AS descripcion
                FROM SOCIOS
                WHERE ISNULL(CEDULA,'') <> ''
                  AND (
                        CEDULA LIKE '%' + @f + '%'
                     OR CEDULAR LIKE '%' + @f + '%'
                     OR NOMBRE LIKE '%' + @f + '%'
                  )
                ORDER BY NOMBRE;";

                var rows = string.IsNullOrWhiteSpace(pFiltro)
                    ? conn.Query(sqlAll)
                    : conn.Query(sqlFiltrado, new { f = pFiltro });

                var lista = new List<DropDownListaGenericaModel>();

                foreach (var r in rows)
                {
                    var d = (IDictionary<string, object?>)r;

                    var item = S(V(d, "item"));
                    if (string.IsNullOrWhiteSpace(item)) continue;

                    lista.Add(new DropDownListaGenericaModel
                    {
                        item = item,
                        descripcion = S(V(d, "descripcion"))
                    });
                }

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene la lista completa de los nombramientos de una persona.
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="parametros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrVerificaDatosListaResult<CrVerificaDatosNombramientoItem>> CR_VerificaDatos_Nombramientos_Lista_Obtener(int CodEmpresa, string identificacion, string parametros)
        {
            return CR_VerificaDatos_Nombramientos_Listar_Core(CodEmpresa, identificacion, parametros, exportAll: false);
        }
        /// <summary>
        /// Guarda la información de una persona.
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CR_VerificaDatos_Guardar(int CodEmpresa, CrVerificaDatosGuardarRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse("Request inválido.", -2);

            var pId = (req.identificacion ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pId))
                return DbHelper.ErrorResponse(IDENTIFICACION_INVALIDA, -2);

            var pUsuarioSesion = (req.usuario_sesion ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pUsuarioSesion))
                return DbHelper.ErrorResponse(USUARIO_SESION_INVALIDO, -2);
            var pProvincia = (req.provincia ?? string.Empty).Trim();
            var pCanton = (req.canton ?? string.Empty).Trim();
            var pDistrito = (req.distrito ?? string.Empty).Trim();
            var pDireccion = (req.direccion ?? string.Empty).Trim();
            var pApto = (req.apto_postal ?? string.Empty).Trim();
            var pEmail1 = (req.email_1 ?? string.Empty).Trim();
            var pEmail2 = (req.email_2 ?? string.Empty).Trim();
            var pNotificaciones = (req.notificaciones ?? string.Empty).Trim();

            var pEstadoCivil = (req.estado_civil ?? string.Empty).Trim();
            var pCodNacionalidad = (req.cod_nacionalidad ?? string.Empty).Trim();
            var pEstadoLaboral = (req.estado_laboral ?? string.Empty).Trim();

            var pSexo = (req.sexo ?? string.Empty).Trim().ToUpperInvariant();

            if (pSexo == "FEMENINO" || pSexo == "F") pSexo = "F";
            else pSexo = "M";

            if (!TryParseFecha(req.fecha_nac, out var dtNac))
                return DbHelper.ErrorResponse("Fecha de nacimiento inválida.", -2);

            if (!TryParseFecha(req.nombramiento_fecha, out var dtNombramiento))
                return DbHelper.ErrorResponse("Fecha de nombramiento inválida.", -2);

            var pConyugeCed = (req.conyuge_cedula ?? string.Empty).Trim();
            var pConyugeNom = (req.conyuge_nombre ?? string.Empty).Trim();
            var pConyugeCell = (req.conyuge_tel_cell ?? string.Empty).Trim();
            var pConyugeTra = (req.conyuge_tel_tra ?? string.Empty).Trim();
            var pConyugeExt = (req.conyuge_tel_tra_ext ?? string.Empty).Trim();

            var pAlbCed = (req.albacea_cedula ?? string.Empty).Trim();
            var pAlbNom = (req.albacea_nombre ?? string.Empty).Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();
                const string sqlUpdate = @"
                UPDATE SOCIOS
                SET
                    provincia = @provincia,
                    canton = @canton,
                    distrito = @distrito,
                    estadocivil = @estado_civil,
                    direccion = @direccion,
                    fecha_nac = @fecha_nac,
                    apto = @apto,
                    af_email = @email_1,
                    sexo = @sexo,
                    EstadoLaboral = @estado_laboral,
                    ActualizaFecha = dbo.MyGetdate(),
                    ActualizaUser = @usr,
                    Nombramiento_Fecha = @nombramiento_fecha,
                    Conyuge_Cedula = @conyuge_cedula,
                    Conyuge_Nombre = @conyuge_nombre,
                    Conyuge_TelCell = @conyuge_tel_cell,
                    Conyuge_TelTra = @conyuge_tel_tra,
                    Conyuge_TelTraExt = @conyuge_tel_tra_ext,
                    Notificaciones = @notificaciones,
                    Albacea_cedula = @albacea_cedula,
                    Albacea_nombre = @albacea_nombre,
                    Email_02 = @email_2,
                    COD_NACIONALIDAD = CASE WHEN @cod_nacionalidad = '' THEN COD_NACIONALIDAD ELSE @cod_nacionalidad END
                WHERE Cedula = @cedula;";

                conn.Execute(sqlUpdate, new
                {
                    cedula = pId,
                    provincia = pProvincia,
                    canton = pCanton,
                    distrito = pDistrito,
                    estado_civil = pEstadoCivil,
                    direccion = pDireccion,
                    fecha_nac = dtNac,
                    apto = pApto,
                    email_1 = pEmail1,
                    email_2 = pEmail2,
                    sexo = pSexo,
                    estado_laboral = pEstadoLaboral,
                    usr = pUsuarioSesion,
                    nombramiento_fecha = dtNombramiento,
                    conyuge_cedula = pConyugeCed,
                    conyuge_nombre = pConyugeNom,
                    conyuge_tel_cell = pConyugeCell,
                    conyuge_tel_tra = pConyugeTra,
                    conyuge_tel_tra_ext = pConyugeExt,
                    notificaciones = pNotificaciones,
                    albacea_cedula = pAlbCed,
                    albacea_nombre = pAlbNom,
                    cod_nacionalidad = pCodNacionalidad
                });

                const string sqlNom = "exec spAFI_Persona_Nombramientos_Add @Cedula, @EstadoLaboral, @Fecha, @Usuario, @Mov";
                conn.Execute(sqlNom, new
                {
                    Cedula = pId,
                    EstadoLaboral = pEstadoLaboral,
                    Fecha = dtNombramiento,
                    Usuario = pUsuarioSesion,
                    Mov = "A"
                });
                const string sqlDir = @"
                exec spAFI_Persona_Direccion_Add
                    @Cedula, @Provincia, @Canton, @Distrito, @Direccion,
                    @Email01, @Email02, @Telefono01, @Telefono02,
                    @Usuario, @Mov, @CodApp, @Tipo;";

                conn.Execute(sqlDir, new
                {
                    Cedula = pId,
                    Provincia = pProvincia,
                    Canton = pCanton,
                    Distrito = pDistrito,
                    Direccion = pDireccion,
                    Email01 = pEmail1,
                    Email02 = pEmail2,
                    Telefono01 = "",
                    Telefono02 = "",
                    Usuario = pUsuarioSesion,
                    Mov = "A",
                    CodApp = "ProGrX",
                    Tipo = 1
                });

                if (req.guardar_direccion_trabajo)
                {
                    var tProv = (req.tra_provincia ?? string.Empty).Trim();
                    var tCant = (req.tra_canton ?? string.Empty).Trim();
                    var tDist = (req.tra_distrito ?? string.Empty).Trim();
                    var tDir = (req.tra_direccion ?? string.Empty).Trim();

                    if (tProv != "" && tCant != "" && tDist != "" && tDir != "")
                    {
                        conn.Execute(sqlDir, new
                        {
                            Cedula = pId,
                            Provincia = tProv,
                            Canton = tCant,
                            Distrito = tDist,
                            Direccion = tDir,
                            Email01 = pEmail1,
                            Email02 = pEmail2,
                            Telefono01 = "",
                            Telefono02 = "",
                            Usuario = pUsuarioSesion,
                            Mov = "A",
                            CodApp = "ProGrX",
                            Tipo = 2 
                        });
                    }
                }

                RegistrarBitacora(
                    CodEmpresa,
                    MODULO,
                    pUsuarioSesion,
                    $"Créditos > Verifica Datos Personales > Guardar (ID {pId})"
                );

                return DbHelper.OkResponse("Información Actualizada Satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Exporta la lista completa de los nombramientos de una persona.
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <param name="parametros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrVerificaDatosListaResult<CrVerificaDatosNombramientoItem>> CR_VerificaDatos_Nombramientos_Lista_Export(int CodEmpresa, string identificacion, string parametros)
        {
            return CR_VerificaDatos_Nombramientos_Listar_Core(CodEmpresa, identificacion, parametros, exportAll: true);
        }
        /// <summary>
        /// Obtiene el catálogo de Estado Laboral (AFI_ESTADO_LABORAL) para dropdown.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_EstadoLaboral_Dropdown_Obtener(int CodEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                conn.Open();

                const string sql = @"
                select 
                    rtrim(Estado_Laboral) as item,
                    rtrim(Descripcion)    as descripcion
                from AFI_ESTADO_LABORAL
                where Activo = 1
                order by Descripcion asc;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql).ToList();

                response.Code = 0;
                response.Description = "Ok";
                response.Result = lista;
                return response;
            }
            catch (SqlException ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result ??= new List<DropDownListaGenericaModel>();
                return response;
            }
        }

        /// <summary>
        /// Obtiene el catalogo de estado civil
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_EstadoCivil_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                const string sql = @"
                select
                  rtrim(Estado_Civil) as item,
                  rtrim(Descripcion)  as descripcion
                from SYS_ESTADO_CIVIL
                where Activo = 1
                order by Descripcion asc;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el catalogo de nacionalidades
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Nacionalidades_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                const string sql = @"
                select
                  rtrim(cod_nacionalidad) as item,
                  rtrim(Descripcion)      as descripcion
                from sys_nacionalidades
                where Activo = 1
                order by Omision desc, Descripcion asc;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene el catalogo de provincias
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Provincias_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                const string sql = @"
            select
                rtrim(Provincia) as item,
                rtrim(Descripcion) as descripcion
            from Provincias
            order by Descripcion;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene el catalogo de cantones
        /// <param name="CodEmpresa"></param>
        /// <param name="provincia"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Cantones_Dropdown_Obtener(int CodEmpresa, string provincia)
        {
            var pProv = (provincia ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pProv))
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>("Provincia inválida.", -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                const string sql = @"
            select
                rtrim(Canton) as item,
                rtrim(Descripcion) as descripcion
            from Cantones
            where Provincia = @provincia
            order by Descripcion;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new { provincia = pProv }).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el catalogo de distritos
        /// <param name="CodEmpresa"></param>
        /// <param name="provincia"></param>
        /// <param name="canton"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Distritos_Dropdown_Obtener(int CodEmpresa, string provincia, string canton)
        {
            var pProv = (provincia ?? string.Empty).Trim();
            var pCant = (canton ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(pProv))
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>("Provincia inválida.", -2);

            if (string.IsNullOrWhiteSpace(pCant))
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>("Cantón inválido.", -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                const string sql = @"
            select
                rtrim(Distrito) as item,
                rtrim(Descripcion) as descripcion
            from Distritos
            where Provincia = @provincia
              and Canton = @canton
            order by Descripcion;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new { provincia = pProv, canton = pCant }).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }
        /// <summary>
        /// Ejecuta el sp para agregar un nombramiento.
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CR_VerificaDatos_Nombramiento_Agregar(int CodEmpresa, CrVerificaDatosNombramientoAgregarRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse("Request inválido.", -2);

            var pId = (req.identificacion ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pId))
                return DbHelper.ErrorResponse(IDENTIFICACION_INVALIDA, -2);

            var pEstado = (req.estado ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pEstado))
                return DbHelper.ErrorResponse("Estado inválido.", -2);

            var pAPartir = (req.a_partir ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pAPartir))
                return DbHelper.ErrorResponse("A partir inválido.", -2);

            var pUsuarioSesion = (req.usuario_sesion ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pUsuarioSesion))
                return DbHelper.ErrorResponse(USUARIO_SESION_INVALIDO, -2);
            if (!DateTime.TryParse(pAPartir, out var dt))
                return DbHelper.ErrorResponse("A partir inválido.", -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                const string sql = "exec spAFI_Persona_Nombramientos_Add @Cedula, @EstadoLaboral, @Fecha, @Usuario, @Mov";
                conn.Execute(sql, new
                {
                    Cedula = pId,
                    EstadoLaboral = pEstado,
                    Fecha = dt,
                    Usuario = pUsuarioSesion,
                    Mov = "A"
                });

                RegistrarBitacora(
                    CodEmpresa,
                    MODULO,
                    pUsuarioSesion,
                    $"Créditos > Verifica Datos Personales > Agrega Nombramiento (ID {pId}) Estado={pEstado} Fecha={pAPartir}"
                );

                return DbHelper.OkResponse("Nombramiento registrado satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Asigna un catálogo a una persona.
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CR_VerificaDatos_Catalogo_Asignar(int CodEmpresa, CrVerificaDatosAsignarCatalogoRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse("Request inválido.", -2);

            var pId = (req.identificacion ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pId))
                return DbHelper.ErrorResponse(IDENTIFICACION_INVALIDA, -2);

            var pTipo = (req.tipo ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(pTipo))
                return DbHelper.ErrorResponse(TIPO_INVALIDO, -2);

            var pCodItem = (req.cod_item ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pCodItem))
                return DbHelper.ErrorResponse(ITEM_INVALIDO, -2);

            var pUsuarioSesion = (req.usuario_sesion ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pUsuarioSesion))
                return DbHelper.ErrorResponse(USUARIO_SESION_INVALIDO, -2);
            var tipoMov = req.asignar ? "A" : "E";

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                string sql;
                object param;

                switch (pTipo)
                {
                    case "BIENES":
                        sql = "exec spAFI_PERSONA_BIENES_Registra @Cedula, @Codigo, @TipoMov, @Usuario";
                        param = new { Cedula = pId, Codigo = pCodItem, TipoMov = tipoMov, Usuario = pUsuarioSesion };
                        break;

                    case "CANALES":
                        sql = "exec spAFI_Persona_Canales_Registra @Cedula, @Canal, @TipoMov, @Usuario";
                        param = new { Cedula = pId, Canal = pCodItem, TipoMov = tipoMov, Usuario = pUsuarioSesion };
                        break;

                    case "GUSTOS":
                        sql = "exec spAFI_Persona_Preferencias_Registra @Cedula, @Codigo, @TipoMov, @Usuario";
                        param = new { Cedula = pId, Codigo = pCodItem, TipoMov = tipoMov, Usuario = pUsuarioSesion };
                        break;

                    case "ESCOLARIDAD":
                        sql = "exec spAFI_PERSONA_ESCOLARIDAD_Registra @Cedula, @Codigo, @TipoMov, @Usuario";
                        param = new { Cedula = pId, Codigo = pCodItem, TipoMov = tipoMov, Usuario = pUsuarioSesion };
                        break;

                    default:
                        return DbHelper.ErrorResponse(TIPO_INVALIDO, -2);
                }

                conn.Execute(sql, param);

                RegistrarBitacora(
                    CodEmpresa,
                    MODULO,
                    pUsuarioSesion,
                    $"Créditos > Verifica Datos Personales > {pTipo} => {(req.asignar ? "Asignar" : "Eliminar")} (ID {pId}, Item {pCodItem})"
                );

                return DbHelper.OkResponse("Proceso concluido satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        private ErrorDto<CrVerificaDatosListaResult<CrVerificaDatosNombramientoItem>> CR_VerificaDatos_Nombramientos_Listar_Core(int CodEmpresa,string identificacion,string parametros,bool exportAll)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
                return DbHelper.CreateErrorResponse<CrVerificaDatosListaResult<CrVerificaDatosNombramientoItem>>(IDENTIFICACION_INVALIDA, -2);

            var pId = identificacion.Trim();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = DbHelper.CreateOkResponse(new CrVerificaDatosListaResult<CrVerificaDatosNombramientoItem>());

            try
            {
                conn.Open();

                const string sql = "exec spAFI_Persona_Nombramientos_Consulta @Cedula";
                var rows = conn.Query(sql, new { Cedula = pId });

                var lista = new List<CrVerificaDatosNombramientoItem>();

                foreach (var r in rows)
                {
                    var d = (IDictionary<string, object?>)r;
                    lista.Add(new CrVerificaDatosNombramientoItem
                    {
                        estado = S(V(d, "EstadoLaboralDesc", "ESTADOLABORALDESC")),
                        a_partir = S(V(d, "FECHA", "fecha")),
                        fecha = S(V(d, "REGISTRO_FECHA", "registro_fecha")),
                        usuario = S(V(d, "REGISTRO_USUARIO", "registro_usuario"))
                    });
                }

                response.Result!.total = lista.Count;
                response.Result!.lista = lista;

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrVerificaDatosListaResult<CrVerificaDatosNombramientoItem>>(ex.Message);
            }
        }
        private List<CrVerificaDatosChecklistItem> ObtenerChecklistCRM(SqlConnection conn, string identificacion, string tipo)
        {
            var pTipo = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            string sp;
            string keyField;

            switch (pTipo)
            {
                case "BIENES":
                    sp = "exec spAFI_PERSONA_BIENES_Consulta @Cedula, @Todos";
                    keyField = "BIEN_TIPO";
                    break;

                case "CANALES":
                    sp = "exec spAFI_Persona_Canales_Consulta @Cedula, @Todos";
                    keyField = "CANAL_TIPO";
                    break;

                case "GUSTOS":
                    sp = "exec spAFI_Persona_Preferencias_Consulta @Cedula, @Todos";
                    keyField = "COD_PREFERENCIA";
                    break;

                case "ESCOLARIDAD":
                    sp = "exec spAFI_PERSONA_ESCOLARIDAD_Consulta @Cedula, @Todos";
                    keyField = "ESCOLARIDAD_TIPO";
                    break;

                default:
                    return new List<CrVerificaDatosChecklistItem>();
            }

            var rows = conn.Query(sp, new { Cedula = identificacion, Todos = 1 });

            var lista = new List<CrVerificaDatosChecklistItem>();

            foreach (var r in rows)
            {
                var d = (IDictionary<string, object?>)r;

                lista.Add(new CrVerificaDatosChecklistItem
                {
                    cod_item = S(V(d, keyField)),
                    descripcion = S(V(d, "DESCRIPCION", "Descripcion", "descripcion")),
                    asignado = ToInt(V(d, "ASIGNADO", "Asignado", "asignado"))
                });
            }

            return lista;
        }
        private void RegistrarBitacora(int CodEmpresa,int vModulo,string userSesion,string detalleMovimiento,string movimiento = "Modifica - WEB")
        {
            _security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = userSesion,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
        private static object? V(IDictionary<string, object?> d, params string[] keys)
        {
            foreach (var k in keys)
            {
                foreach (var kv in d)
                {
                    if (string.Equals(kv.Key, k, StringComparison.OrdinalIgnoreCase))
                        return kv.Value;
                }
            }
            return null;
        }
        private static string S(object? v) => (Convert.ToString(v) ?? string.Empty).Trim();
        private static int ToInt(object? v)
        {
            var s = S(v);
            if (int.TryParse(s, out var n)) return n;
            return 0;
        }
        private sealed class CatalogoCfg
        {
            public string TablaCatalogo { get; }
            public string TablaAsignacion { get; }

            public CatalogoCfg(string tablaCatalogo, string tablaAsignacion)
            {
                TablaCatalogo = tablaCatalogo;
                TablaAsignacion = tablaAsignacion;
            }
        }
        private static bool TryParseFecha(string? s, out DateTime dt)
        {
            dt = default;

            var v = (s ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(v)) return false;

            var formats = new[]
            {
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "dd/MM/yyyy",
                "dd-MM-yyyy",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss.fff"
            };

            return DateTime.TryParseExact(
                v,
                formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out dt);
        }
    }
}