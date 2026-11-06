using Dapper;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using System.Text.RegularExpressions;

namespace Galileo.DataBaseTier
{
    public partial class FrmAFPrincipalDB
    {
        /// <summary>
        /// Valida Persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Validar(int CodEmpresa, string req)
        {
            var lectura = AF_Persona_Solicitud_Leer(req);
            if (lectura.Code != 0 || lectura.Result is null)
            {
                return DbHelper.ErrorResponse(lectura.Description ?? "No se recibieron datos de la persona.",
                    lectura.Code.GetValueOrDefault(-1));
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Open();
                return AF_Persona_Datos_Validar(connection, lectura.Result);
            });
            return AF_Persona_Resultado_Obtener(result);
        }

        /// <summary>
        /// Deserializa y valida la estructura; el movimiento explícito del guardado prevalece.
        /// </summary>
        private static ErrorDto<AfPersonaAddRequestDto> AF_Persona_Solicitud_Leer(
            string request, string? movimiento = null)
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                return DbHelper.CreateErrorResponse<AfPersonaAddRequestDto>(
                    "No se recibieron los datos de la persona.", -2);
            }

            try
            {
                var dto = JsonConvert.DeserializeObject<AfPersonaAddRequestDto>(request);
                if (dto is null)
                {
                    return DbHelper.CreateErrorResponse<AfPersonaAddRequestDto>(
                        "No se recibieron los datos de la persona.", -2);
                }

                var mov = (movimiento ?? dto.Mov.ToString()).Trim().ToUpperInvariant();
                if (mov is not ("A" or "E"))
                {
                    return DbHelper.CreateErrorResponse<AfPersonaAddRequestDto>(
                        "La operación debe indicar agregar o editar una persona.", -2);
                }

                dto.Mov = mov[0];
                dto.Cedula = dto.Cedula?.Trim();
                dto.Usuario = dto.Usuario?.Trim();
                var validacion = AF_Persona_Solicitud_Validar(dto);
                return validacion.Code == 0
                    ? DbHelper.CreateOkResponse(dto)
                    : DbHelper.CreateErrorResponse<AfPersonaAddRequestDto>(
                        validacion.Description ?? "Solicitud incompleta.", -2);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<AfPersonaAddRequestDto>(ex.Message);
            }
        }

        /// <summary>
        /// Rechaza solicitudes incompletas antes de consultar o ejecutar el procedimiento.
        /// </summary>
        private static ErrorDto AF_Persona_Solicitud_Validar(AfPersonaAddRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Cedula))
            {
                return DbHelper.ErrorResponse("Debe indicar la cédula de la persona.", -2);
            }
            if (dto.Cedula.Length > 20)
            {
                return DbHelper.ErrorResponse("La cédula no puede superar 20 caracteres.", -2);
            }
            if (dto.TipoId <= 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar el tipo de identificación.", -2);
            }
            if (string.IsNullOrWhiteSpace(dto.Usuario))
            {
                return DbHelper.ErrorResponse("No se recibió el usuario que realiza la operación.", -2);
            }
            return DbHelper.OkResponse("Solicitud válida");
        }

        /// <summary>
        /// Conserva errores de conexión y validación sin convertir resultados ausentes en éxito.
        /// </summary>
        private static ErrorDto AF_Persona_Resultado_Obtener(ErrorDto<ErrorDto> resultado)
        {
            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(resultado.Description ?? "Error al procesar la persona.",
                    resultado.Code.GetValueOrDefault(-1));
            }
            return resultado.Result ?? DbHelper.ErrorResponse("No se recibió el resultado de la operación.");
        }

        /// <summary>
        /// Admite el indicador numérico del front-end y el formato S/N del modelo y del SP.
        /// </summary>
        private static string AF_Persona_Embargo_Normalizar(string? valor)
        {
            return valor?.Trim().ToUpperInvariant() is "1" or "S" ? "S" : "N";
        }

        /// <summary>Construye los parámetros originales del procedimiento de guardado.</summary>
        private static DynamicParameters AF_Persona_Parametros_Construir(AfPersonaAddRequestDto req, string mov)
        {
            var p = new DynamicParameters();
            p.Add("@TipoId", req.TipoId);
            p.Add("@Cedula", req.Cedula);
            p.Add("@Id_Alterno", req.Id_Alterno);
            p.Add("@Nombre_Completo", req.Nombre_Completo);
            p.Add("@Apellido_1", req.Apellido_1);
            p.Add("@Apellido_2", req.Apellido_2);
            p.Add("@Nombre", req.Nombre);
            p.Add("@RazonSocial", req.RazonSocial);
            p.Add("@Estado", req.Estado);
            p.Add("@EstadoCivil", req.EstadoCivil);
            p.Add("@Genero", req.Genero);
            p.Add("@fNacimiento", req.fNacimiento);
            p.Add("@fCedulaVence", req.fCedulaVence);
            p.Add("@PromotorId", req.PromotorId);
            p.Add("@Boleta", req.Boleta);
            p.Add("@fIngreso", req.fIngreso);
            p.Add("@EstadoLaboral", req.EstadoLaboral);
            p.Add("@PaisNac", req.PaisNac);
            p.Add("@Nacionalidad", req.Nacionalidad);
            p.Add("@Email_1", req.Email_1);
            p.Add("@Email_2", req.Email_2);
            p.Add("@Provincia", req.Provincia);
            p.Add("@Canton", req.Canton);
            p.Add("@Distrito", req.Distrito);
            p.Add("@Direccion", req.Direccion);
            p.Add("@AptoPostal", req.AptoPostal);
            p.Add("@Notificacion", req.Notificacion);
            p.Add("@Institucion", req.Institucion);
            p.Add("@Departamento", req.Departamento);
            p.Add("@Seccion", req.Seccion);
            p.Add("@UP", req.UP);
            p.Add("@UT", req.UT);
            p.Add("@CT", req.CT);
            p.Add("@Deductora", req.Deductora);
            p.Add("@Profesion", req.Profesion);
            p.Add("@Sector", req.Sector);
            p.Add("@NPagos", req.NPagos);
            p.Add("@NHijos", req.NHijos);
            p.Add("@PriDeduc", req.PriDeduc);
            p.Add("@fNombramiento", req.fNombramiento);
            p.Add("@NivelAcademico", req.NivelAcademico);
            p.Add("@Sociedad", req.Sociedad);
            p.Add("@Actividad", req.Actividad);
            p.Add("@Propiedades", req.Propiedades);
            p.Add("@Oficina", req.Oficina);
            p.Add("@facebook", req.Facebook);
            p.Add("@Twitter", req.Twitter);
            p.Add("@LinkedIn", req.LinkedIn);
            p.Add("@Instagram", req.Instagram);
            p.Add("@Blog", req.Blog);
            p.Add("@ConyugeCedula", req.ConyugeCedula);
            p.Add("@ConyugeNombre", req.ConyugeNombre);
            p.Add("@ConyugeTelCel", req.ConyugeTelCel);
            p.Add("@ConyugeTelTra", req.ConyugeTelTra);
            p.Add("@ConyugeTelTraExt", req.ConyugeTelTraExt);
            p.Add("@AlbaceaCedula", req.AlbaceaCedula);
            p.Add("@AlbaceaNombre", req.AlbaceaNombre);
            p.Add("@AlbaceaTelCel", req.AlbaceaTelCel);
            p.Add("@AlbaceaTelTra", req.AlbaceaTelTra);
            p.Add("@AlbaceaTelTraExt", req.AlbaceaTelTraExt);
            p.Add("@SalarioTipo", req.SalarioTipo);
            p.Add("@SalarioDivisa", req.SalarioDivisa);
            p.Add("@SalarioFecha", req.SalarioFecha);
            p.Add("@SalarioDevengado", req.SalarioDevengado);
            p.Add("@SalarioRebajos", req.SalarioRebajos);
            p.Add("@SalarioNeto", req.SalarioNeto);
            p.Add("@SalarioEmbargo", AF_Persona_Embargo_Normalizar(req.SalarioEmbargo));
            p.Add("@AdminitraAportePatronal", req.AdministraAportePatronal);
            p.Add("@Sugef", req.Sugef);
            p.Add("@I_Beneficiario", req.I_Beneficiario);
            p.Add("@I_TrabajoPropio", req.I_TrabajoPropio);
            p.Add("@Tipo_Patron", req.Tipo_Patron);
            p.Add("@CargoDesc", req.CargoDesc);
            p.Add("@PEP_Ind", req.PEP_Ind);
            p.Add("@PEP_Inicio", req.PEP_Inicio);
            p.Add("@PEP_Corte", req.PEP_Corte);
            p.Add("@PEP_Cargo", req.PEP_Cargo);
            p.Add("@TipoCES", req.TipoCES);
            p.Add("@C_Actividad", req.C_Actividad);
            p.Add("@Usuario", req.Usuario);
            p.Add("@Mov", mov);
            p.Add("@TraProvincia", req.TraProvincia);
            p.Add("@TraCanton", req.TraCanton);
            p.Add("@TraDistrito", req.TraDistrito);
            p.Add("@TraDireccion", req.TraDireccion);
            p.Add("@PaisResidencia", req.PaisResidencia);
            p.Add("@OrigenRecursosId", req.OrigenRecursos);
            p.Add("@OrigenRecursosOtros", req.OrigenRecursosOtros);
            p.Add("@OtrosIngresos", req.OtrosIngresos);

            return p;
        }

        /// <summary>
        /// Valida los datos principales de una persona antes de guardar.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        private static ErrorDto AF_Persona_Datos_Validar(SqlConnection connection, AfPersonaAddRequestDto dto)
        {
            var errores = new List<string>();


            AF_Persona_Cedula_Validar(connection, dto, errores);
            AF_Persona_Emails_Validar(dto, errores);
            AF_Persona_Basicos_Validar(dto, errores);
            AF_Persona_Salario_Validar(dto, errores);
            AF_Persona_Ubicacion_Validar(dto, errores);
            AF_Persona_PromotorFechas_Validar(connection, dto, errores);
            AF_Persona_Laborales_Validar(dto, errores);
            AF_Persona_Relacionados_Validar(connection, dto, errores);

            return errores.Count > 0
                ? DbHelper.ErrorResponse(string.Join(Environment.NewLine, errores), -2)
                : DbHelper.OkResponse("Validación correcta");
        }

        /// <summary>Valida los campos de la persona según las reglas vigentes del formulario.</summary>
        private static void AF_Persona_Cedula_Validar(SqlConnection connection, AfPersonaAddRequestDto dto, List<string> errores)
        {
            var largo = connection.QueryFirstOrDefault<int>(
                "SELECT LARGO_MINIMO FROM AFI_TIPOS_IDS WHERE TIPO_ID = @TipoId",
                new { dto.TipoId });

            if (dto.Mov == 'A' && !string.IsNullOrEmpty(dto.Cedula) && dto.Cedula.Length != largo)
            {
                errores.Add($"Número de Identidad inválido, se espera {largo} caracteres.");
            }

            if (!string.IsNullOrEmpty(dto.Cedula) && dto.Cedula.Length > 20)
            {
                errores.Add("Número de Identidad no puede superar 20 caracteres.");
            }
        }

        /// <summary>Valida los campos de la persona según las reglas vigentes del formulario.</summary>
        private static void AF_Persona_Emails_Validar(AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (string.IsNullOrEmpty(dto.Email_1) || !AF_Persona_Email_Validar(dto.Email_1))
            {
                errores.Add("Email principal no es válido.");
            }

            if (!string.IsNullOrEmpty(dto.Email_2) && !AF_Persona_Email_Validar(dto.Email_2))
            {
                errores.Add("Email secundario no es válido.");
            }
        }

        /// <summary>Valida los campos de la persona según las reglas vigentes del formulario.</summary>
        private static void AF_Persona_Basicos_Validar(AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(dto.Apellido_1)) errores.Add("Falta el Apellido 1.");
            if (string.IsNullOrWhiteSpace(dto.Apellido_2)) errores.Add("Falta el Apellido 2.");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) errores.Add("Falta el Nombre.");
            if (string.IsNullOrWhiteSpace(dto.Genero)) errores.Add("No se especificó el Sexo.");
            if (string.IsNullOrWhiteSpace(dto.EstadoCivil)) errores.Add("No se especificó el Estado Civil.");
        }

        /// <summary>Valida los campos de la persona según las reglas vigentes del formulario.</summary>
        private static void AF_Persona_Salario_Validar(AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (dto.SalarioDivisa == "COL")
            {
                if (dto.SalarioDevengado < 100000 || dto.SalarioDevengado > 10000000)
                {
                    errores.Add("Salario Devengado no es válido");
                }

                return;
            }

            if (dto.SalarioDevengado < 200 || dto.SalarioDevengado > 20000)
            {
                errores.Add("Salario Devengado no es válido");
            }
        }

        /// <summary>Valida los campos de la persona según las reglas vigentes del formulario.</summary>
        private static void AF_Persona_Ubicacion_Validar(AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(dto.Provincia)) errores.Add("No se especificó la Provincia.");
            if (string.IsNullOrWhiteSpace(dto.Canton)) errores.Add("No se especificó el Cantón.");
            if (string.IsNullOrWhiteSpace(dto.Distrito)) errores.Add("No se especificó el Distrito.");
            if (!AF_Persona_Direccion_Validar(dto.Direccion)) errores.Add("No se especificó la Dirección.");

            AF_Persona_UbicacionTrabajo_Validar(dto, errores);

            if (!dto.C_Actividad.HasValue) errores.Add("No se especificó Actividad (Oficina Cumplimiento).");

            if (string.IsNullOrWhiteSpace(dto.Departamento) && string.IsNullOrWhiteSpace(dto.UP))
            {
                errores.Add("No se especificó el Departamento o la Unidad Programática.");
            }
        }

        /// <summary>La dirección de trabajo es opcional, pero si se indica se revisa completa.</summary>
        private static void AF_Persona_UbicacionTrabajo_Validar(AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (string.IsNullOrEmpty(dto.TraDireccion))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(dto.TraProvincia)) errores.Add("No se especificó la Provincia de la Dirección de Trabajo.");
            if (string.IsNullOrWhiteSpace(dto.TraCanton)) errores.Add("No se especificó el Cantón de la Dirección de Trabajo.");
            if (string.IsNullOrWhiteSpace(dto.TraDistrito)) errores.Add("No se especificó el Distrito en la Dirección de Trabajo.");
            if (!AF_Persona_Direccion_Validar(dto.TraDireccion)) errores.Add("La Dirección Exacta de Trabajo no fue suministrada correctamente.");
        }

        /// <summary>Valida los campos de la persona según las reglas vigentes del formulario.</summary>
        private static void AF_Persona_PromotorFechas_Validar(SqlConnection connection, AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (dto.Mov != 'A')
            {
                return;
            }

            var estadoPromotor = connection.QueryFirstOrDefault<int>(
                "SELECT ISNULL(estado,0) FROM promotores WHERE id_promotor = @PromotorId",
                new { dto.PromotorId });

            if (estadoPromotor == 0)
            {
                errores.Add("El promotor indicado está inactivo o no existe.");
            }

            if (dto.fNacimiento > DateTime.Now.AddYears(-17))
            {
                errores.Add("La persona es menor de edad.");
            }

            if (dto.fCedulaVence <= DateTime.Now.AddDays(20))
            {
                errores.Add("La cédula está próxima a vencer.");
            }
        }

        /// <summary>Valida los campos de la persona según las reglas vigentes del formulario.</summary>
        private static void AF_Persona_Laborales_Validar(AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (dto.Profesion <= 0)
            {
                errores.Add("Profesión no es válida.");
            }

            if (dto.Sector <= 0)
            {
                errores.Add("Sector no es válido.");
            }

            if (string.IsNullOrWhiteSpace(dto.EstadoLaboral))
            {
                errores.Add("No se especificó el Estado Laboral.");
            }

            if (dto.NivelAcademico <= 0)
            {
                errores.Add("No se especificó el nivel académico.");
            }

            if (string.IsNullOrWhiteSpace(dto.CargoDesc))
            {
                errores.Add("Tienen que indicar el Puesto que desempeña.");
            }
        }

        /// <summary>
        /// Valida beneficiarios, albacea y teléfonos registrados (paridad con fxValida de VB6).
        /// Solo aplica a personas existentes: en un alta aún no hay datos relacionados.
        /// </summary>
        private static void AF_Persona_Relacionados_Validar(SqlConnection connection, AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (dto.Mov != 'E' || string.IsNullOrWhiteSpace(dto.Cedula))
            {
                return;
            }

            var estado = connection.QueryFirst<AfPersonaRelacionadosEstado>(
                @"SELECT CAST(dbo.fxAFI_Afiliacion_Valida_Beneficiarios(@Cedula) AS int) AS Beneficiarios,
                         CAST(dbo.fxAFI_Afiliacion_Valida_Telefonos(@Cedula) AS int) AS Telefonos,
                         CAST(dbo.fxAFI_Afiliacion_Valida_Beneficiarios_MenoresSinAlbacea(@Cedula) AS int) AS MenoresSinAlbacea",
                new { dto.Cedula });

            if (estado.Beneficiarios == 0 && dto.I_Beneficiario)
            {
                errores.Add("No se han registrado los Beneficiarios o están incompletos.");
            }

            if (estado.MenoresSinAlbacea > 0 && dto.I_Beneficiario && (dto.AlbaceaCedula?.Trim().Length ?? 0) <= 5)
            {
                errores.Add("Existen Beneficiarios menores de edad y no se han indicado los Albaceas.");
            }

            if (estado.Telefonos == 0)
            {
                errores.Add("No se han registrado los teléfonos de contacto.");
            }
        }

        private sealed class AfPersonaRelacionadosEstado
        {
            public int Beneficiarios { get; set; }
            public int Telefonos { get; set; }
            public int MenoresSinAlbacea { get; set; }
        }

        /// <summary>
        /// Paridad con fxDireccion_Valida de VB6: sin los caracteres de relleno "-", "#" y "*",
        /// la dirección debe tener al menos 3 palabras y 20 caracteres.
        /// </summary>
        private static bool AF_Persona_Direccion_Validar(string? direccion)
        {
            var limpia = (direccion ?? string.Empty).Replace("-", "").Replace("#", "").Replace("*", "");
            var palabras = limpia.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            return palabras >= 3 && limpia.Length >= 20;
        }

        /// <summary>Valida los campos de la persona según las reglas vigentes del formulario.</summary>
        private static bool AF_Persona_Email_Validar(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.None, RegexTimeout);
        }
    }
}
