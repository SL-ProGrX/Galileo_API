using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_Externo.Models.NewFolder;
using System.Data;
using System.Globalization;
using System.Text.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class ZohoDB
    {
        /// <summary>
        /// Guarda el expediente de un ticket de tipo Sepelios.
        /// </summary>
        private ErrorDto Sepelios_Guardar(int CodEmpresa, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            return AF_Beneficios_Zoho_Expediente_Procesar(new ZohoExpedienteProcesoRequest
            {
                CodEmpresa = CodEmpresa,
                Datos = datos,
                Usuario = usuario,
                Solicitud = jsonZoho,
                RegistrarUsuarioEnError = true,
                Preparar = AF_Beneficios_Zoho_Sepelios_Preparar
            });
        }

        private ZohoExpedientePreparacion AF_Beneficios_Zoho_Sepelios_Preparar(
            IDbConnection connection,
            ZohoExpedienteProcesoRequest request)
        {
            var cedula = CfStr(request.Datos, CampoCedulaZoho);
            if (string.IsNullOrEmpty(cedula))
            {
                return new ZohoExpedientePreparacion
                {
                    MensajeError = MensajeCedulaRequerida,
                    RegistrarError = false
                };
            }

            var estadoSocio = _mBeneficiosDB.ValidaEstadoSocio(request.CodEmpresa, cedula.Trim());
            if (estadoSocio.Code == -1)
            {
                return new ZohoExpedientePreparacion
                {
                    MensajeError = estadoSocio.Description + "..."
                };
            }

            var parentesco = (CfStr(request.Datos, "cf_parentesco_de_la_persona_fallecida") ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
            var codigoBeneficio = Sepelios_CodigoBeneficio_Obtener(parentesco);
            var beneficio = AF_Beneficios_Zoho_BeneficioBase_Crear(
                request,
                cedula,
                codigoBeneficio,
                string.Empty);
            beneficio.sepelio_identificacion = CfStr(
                request.Datos,
                "cf_numero_de_identificacion_de_persona_fallecida")?.Trim();
            beneficio.sepelio_nombre = CfStr(
                request.Datos,
                "cf_nombre_completo_de_persona_fallecida")?.Trim();

            var fechaDefuncion = CfStr(request.Datos, "cf_fecha_de_la_defuncion");
            if (fechaDefuncion != null && DateTime.TryParse(
                fechaDefuncion,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var fecha))
            {
                beneficio.sepelio_fecha_fallecimiento = fecha;
            }

            beneficio.monto = AF_Beneficios_Zoho_Monto_Obtener(connection, "B_SEPE", codigoBeneficio);
            beneficio.monto_aplicado = beneficio.monto;
            beneficio.tipo = new AfBeneficioIntegralDropsLista { item = "M" };

            return new ZohoExpedientePreparacion
            {
                Beneficio = beneficio,
                CodigoBeneficio = codigoBeneficio,
                CodigoFormulario = codigoBeneficio.Trim()
            };
        }

        /// <summary>
        /// Guarda el expediente de un ticket de tipo Reconocimientos e inserta el registro de reconocimiento asociado.
        /// </summary>
        private ErrorDto Reconocimientos_Guardar(int CodEmpresa, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            return AF_Beneficios_Zoho_Expediente_Procesar(new ZohoExpedienteProcesoRequest
            {
                CodEmpresa = CodEmpresa,
                Datos = datos,
                Usuario = usuario,
                Solicitud = jsonZoho,
                Preparar = AF_Beneficios_Zoho_Reconocimientos_Preparar
            });
        }

        private ZohoExpedientePreparacion AF_Beneficios_Zoho_Reconocimientos_Preparar(
            IDbConnection connection,
            ZohoExpedienteProcesoRequest request)
        {
            var cedula = CfStr(request.Datos, CampoCedulaZoho);
            var mensajeError = string.IsNullOrEmpty(cedula) ? MensajeCedulaRequerida : string.Empty;
            var estadoSocio = _mBeneficiosDB.ValidaEstadoSocio(
                request.CodEmpresa,
                (cedula ?? string.Empty).Trim());

            if (estadoSocio.Code == -1)
            {
                mensajeError += estadoSocio.Description + "...";
            }

            if (!string.IsNullOrWhiteSpace(mensajeError))
            {
                return new ZohoExpedientePreparacion { MensajeError = mensajeError };
            }

            var reconocimiento = (CfStr(request.Datos, "cf_tipo_de_reconocimiento") ?? string.Empty).Trim();
            var codigoBeneficio = reconocimiento switch
            {
                "Académico" => "MEAC",
                "Científico" => "MERC",
                "Artístico" => "MERA",
                "Deportivo" => "MERD",
                _ => string.Empty
            };
            var beneficio = AF_Beneficios_Zoho_BeneficioBase_Crear(
                request,
                cedula ?? string.Empty,
                codigoBeneficio,
                string.Empty);
            beneficio.monto = AF_Beneficios_Zoho_Monto_Obtener(connection, "B_RECO", codigoBeneficio);
            beneficio.monto_aplicado = beneficio.monto;
            beneficio.tipo = new AfBeneficioIntegralDropsLista { item = "M" };

            return new ZohoExpedientePreparacion
            {
                Beneficio = beneficio,
                CodigoBeneficio = codigoBeneficio,
                CodigoFormulario = codigoBeneficio.Trim(),
                GuardarReconocimiento = true
            };
        }

        private void Reconocimientos_Detalle_Guardar(ReconocimientoGuardarRequest request)
        {
            var fechaNacimiento = Reconocimientos_FechaNacimiento_Obtener(request.Datos);
            var reconocimientoDatos = new AfiBeneReconocimientos
            {
                id_beneficio = Convert.ToInt32(request.Expediente[0], CultureInfo.InvariantCulture),
                consec = Convert.ToInt32(request.Expediente[1], CultureInfo.InvariantCulture),
                cod_beneficio = request.CodigoBeneficio,
                cedula_estudiante = (CfStr(request.Datos, "cf_identificacion_de_estudiante") ?? string.Empty).Trim(),
                fecha_nacimiento = fechaNacimiento,
                edad = DateTime.Now.Year - fechaNacimiento.Year,
                genero = Reconocimientos_Genero_Obtener(request.Datos),
                tipo_centro = Reconocimientos_TipoCentro_Obtener(request.Datos),
                nivel_academico = new AfBeneficioIntegralDropsLista { item = (CfStr(request.Datos, "cf_grado_cursado_en_el_presente_ano") ?? string.Empty).Trim() },
                grado = new AfBeneficioIntegralDropsLista { item = (CfStr(request.Datos, "cf_grado_cursado_el_ano_anterior") ?? string.Empty).Trim() },
                tipo_reconocimiento = new AfBeneficioIntegralDropsLista { item = Reconocimientos_Tipo_Obtener(request.CodigoBeneficio) },
                matematicas = ParseIntCf(request.Datos, "cf_promedio_matematica"),
                ciencias = ParseIntCf(request.Datos, "cf_promedio_ciencia_as"),
                estudios_sociales = ParseIntCf(request.Datos, "cf_promedio_estudios_sociales"),
                espanol = ParseIntCf(request.Datos, "cf_promedio_espanol"),
                idioma = ParseIntCf(request.Datos, "cf_promedio_un_idioma_secundaria"),
                centro_educativo = (CfStr(request.Datos, "cf_nombre_del_centro_educativo") ?? string.Empty).Trim(),
                registro_usuario = request.Usuario
            };

            Reconocimientos_Nombre_Asignar(reconocimientoDatos, request.Datos);
            var afiReconocimientos = new FrmAfBeneficiosIntegralRecDB(_config);
            afiReconocimientos.BeneReconocimiento_Ingresar(request.CodEmpresa, reconocimientoDatos);
        }

        private static void Reconocimientos_Nombre_Asignar(AfiBeneReconocimientos reconocimiento, Dictionary<string, JsonElement> datos)
        {
            var nombreCompleto = CfStr(datos, "cf_nombre_de_estudiantes")?.Trim()
                ?? CfStr(datos, "cf_nombre_y_apellidos_de_estudiante")?.Trim()
                ?? string.Empty;
            var nombres = nombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            reconocimiento.nombre = nombres.Length > 0 ? nombres[0] : nombreCompleto;
            reconocimiento.primer_apellido = nombres.Length > 1 ? nombres[1] : null;
            reconocimiento.segundo_apellido = nombres.Length > 2 ? nombres[2] : null;
        }

        private static DateTime Reconocimientos_FechaNacimiento_Obtener(Dictionary<string, JsonElement> datos)
        {
            var valor = CfStr(datos, "cf_fecha_nacimiento_del_estudiante");
            return valor != null && DateTime.TryParse(valor.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var fecha)
                ? fecha
                : DateTime.Now;
        }

        private static AfBeneficioIntegralDropsLista Reconocimientos_Genero_Obtener(Dictionary<string, JsonElement> datos)
        {
            return (CfStr(datos, "cf_genero") ?? string.Empty).Trim() switch
            {
                "Masculino" => new AfBeneficioIntegralDropsLista { item = "M", descripcion = "Masculino" },
                "Femenino" => new AfBeneficioIntegralDropsLista { item = "F", descripcion = "Femenino" },
                _ => new AfBeneficioIntegralDropsLista { item = "O", descripcion = "Otro" }
            };
        }

        private static AfBeneficioIntegralDropsLista Reconocimientos_TipoCentro_Obtener(Dictionary<string, JsonElement> datos)
        {
            return (CfStr(datos, "cf_tipo_de_centro_educativo") ?? string.Empty).Trim() switch
            {
                "Privado" => new AfBeneficioIntegralDropsLista { item = "PR", descripcion = "Privado" },
                "Público" => new AfBeneficioIntegralDropsLista { item = "PU", descripcion = "Público" },
                _ => new AfBeneficioIntegralDropsLista()
            };
        }

        private static string Reconocimientos_Tipo_Obtener(string codigoBeneficio)
        {
            return codigoBeneficio switch
            {
                "MEAC" => "AC",
                "MERC" => "CI",
                "MERA" => "CUA",
                "MERD" => "DE",
                _ => string.Empty
            };
        }

        private static string Sepelios_CodigoBeneficio_Obtener(string parentesco)
        {
            return parentesco switch
            {
                _ when parentesco.Contains("PADRE", StringComparison.Ordinal) => "MPAD",
                _ when parentesco.Contains("MADRE", StringComparison.Ordinal) => "MMADRE",
                _ when parentesco.Contains("HIJO", StringComparison.Ordinal) => "MHIJO",
                _ when parentesco.Contains("CONYUGUE", StringComparison.Ordinal) => "MCON",
                _ => string.Empty
            };
        }

        private sealed class ReconocimientoGuardarRequest
        {
            public int CodEmpresa { get; init; }
            public Dictionary<string, JsonElement> Datos { get; init; } = [];
            public string Usuario { get; init; } = string.Empty;
            public string CodigoBeneficio { get; init; } = string.Empty;
            public string[] Expediente { get; init; } = [];
        }
    }
}
