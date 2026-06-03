using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using System.Text.RegularExpressions;

namespace Galileo.DataBaseTier
{
    public partial class FrmAFPrincipalDB
    {
        private static DynamicParameters BuildPersonaParameters(AfPersonaAddRequestDto req, string mov)
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
            p.Add("@SalarioEmbargo", req.SalarioEmbargo == "1" ? "S" : "N");
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

            return p;
        }

        /// <summary>
        /// Valida los datos principales de una persona antes de guardar.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        private static ErrorDto ValidarPersona(SqlConnection connection, AfPersonaAddRequestDto dto)
        {
            var errores = new List<string>();
            var esAlta = char.ToUpperInvariant(dto.Mov) != 'E';

            ValidarCedula(connection, dto, errores, esAlta);
            ValidarEmails(dto, errores);
            ValidarDatosBasicos(dto, errores);
            ValidarSalario(dto, errores);
            ValidarUbicacionYActividad(dto, errores);
            ValidarPromotorYFechas(connection, dto, errores, esAlta);
            ValidarDatosLaborales(dto, errores);

            return errores.Count > 0
                ? DbHelper.ErrorResponse(string.Join(Environment.NewLine, errores), -2)
                : DbHelper.OkResponse("Validación correcta");
        }

        private static void ValidarCedula(SqlConnection connection, AfPersonaAddRequestDto dto, List<string> errores, bool esAlta)
        {
            var largo = connection.QueryFirstOrDefault<int>(
                "SELECT LARGO_MINIMO FROM AFI_TIPOS_IDS WHERE TIPO_ID = @TipoId",
                new { dto.TipoId });

            if (esAlta && !string.IsNullOrEmpty(dto.Cedula) && dto.Cedula.Length != largo)
            {
                errores.Add($"Número de Identidad inválido, se espera {largo} caracteres.");
            }

            if (!string.IsNullOrEmpty(dto.Cedula) && dto.Cedula.Length > 20)
            {
                errores.Add("Número de Identidad no puede superar 20 caracteres.");
            }
        }

        private static void ValidarEmails(AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (string.IsNullOrEmpty(dto.Email_1) || !EsEmailValido(dto.Email_1))
            {
                errores.Add("Email principal no es válido.");
            }

            if (!string.IsNullOrEmpty(dto.Email_2) && !EsEmailValido(dto.Email_2))
            {
                errores.Add("Email secundario no es válido.");
            }
        }

        private static void ValidarDatosBasicos(AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(dto.Apellido_1)) errores.Add("Falta el Apellido 1.");
            if (string.IsNullOrWhiteSpace(dto.Apellido_2)) errores.Add("Falta el Apellido 2.");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) errores.Add("Falta el Nombre.");
            if (string.IsNullOrWhiteSpace(dto.Genero)) errores.Add("No se especificó el Sexo.");
            if (string.IsNullOrWhiteSpace(dto.EstadoCivil)) errores.Add("No se especificó el Estado Civil.");
        }

        private static void ValidarSalario(AfPersonaAddRequestDto dto, List<string> errores)
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

        private static void ValidarUbicacionYActividad(AfPersonaAddRequestDto dto, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(dto.Provincia)) errores.Add("No se especificó la Provincia.");
            if (string.IsNullOrWhiteSpace(dto.Canton)) errores.Add("No se especificó el Cantón.");
            if (string.IsNullOrWhiteSpace(dto.Distrito)) errores.Add("No se especificó el Distrito.");
            if (string.IsNullOrWhiteSpace(dto.Direccion)) errores.Add("No se especificó la Dirección.");
            if (string.IsNullOrWhiteSpace(dto.Actividad)) errores.Add("No se especificó Actividad (Oficina Cumplimiento).");

            if (string.IsNullOrWhiteSpace(dto.Departamento) || string.IsNullOrWhiteSpace(dto.UP))
            {
                errores.Add("No se especificó el Departamento o la Unidad Programática.");
            }
        }

        private static void ValidarPromotorYFechas(SqlConnection connection, AfPersonaAddRequestDto dto, List<string> errores, bool esAlta)
        {
            if (!esAlta)
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

        private static void ValidarDatosLaborales(AfPersonaAddRequestDto dto, List<string> errores)
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

        private static bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.None, RegexTimeout);
        }
    }
}
