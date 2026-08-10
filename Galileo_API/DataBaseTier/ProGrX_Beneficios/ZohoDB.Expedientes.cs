using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_Externo.Models.NewFolder;
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
            var response = new ErrorDto { Code = 0 };
            var msjError = string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                var cedula = CfStr(datos, CampoCedulaZoho);
                if (string.IsNullOrEmpty(cedula))
                {
                    response.Code = -1;
                    response.Description = MensajeCedulaRequerida;
                    return response;
                }

                var estadoSocio = _mBeneficiosDB.ValidaEstadoSocio(CodEmpresa, cedula.Trim());
                if (estadoSocio.Code == -1)
                {
                    response.Code = -1;
                    msjError += estadoSocio.Description + "...";
                }

                if (response.Code != -1)
                {
                    var parentesco = (CfStr(datos, "cf_parentesco_de_la_persona_fallecida") ?? string.Empty).Trim().ToUpperInvariant();
                    var codBeneficio = Sepelios_CodigoBeneficio_Obtener(parentesco);

                    var beneficio = new BeneficioGeneralDatos
                    {
                        cod_beneficio = new AfBeneficioIntegralDropsLista { item = codBeneficio },
                        id_beneficio = 0,
                        cedula = cedula.Trim(),
                        monto_aplicado = 0,
                        registra_user = usuario,
                        modifica_usuario = usuario,
                        sepelio_identificacion = CfStr(datos, "cf_numero_de_identificacion_de_persona_fallecida")?.Trim(),
                        sepelio_nombre = CfStr(datos, "cf_nombre_completo_de_persona_fallecida")?.Trim(),
                        estado = new AfBeneficioIntegralDropsLista { item = string.Empty },
                        consec = 0,
                        requiere_justificacion = jsonZoho.justificacion != null,
                        notas = jsonZoho.justificacion ?? string.Empty
                    };

                    var fechaDefuncion = CfStr(datos, "cf_fecha_de_la_defuncion");
                    if (fechaDefuncion != null && DateTime.TryParse(fechaDefuncion, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var fDefuncion))
                    {
                        beneficio.sepelio_fecha_fallecimiento = fDefuncion;
                    }

                    beneficio.monto = connection.QueryFirstOrDefault<float>(@"SELECT [MONTO]
                              FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = 'B_SEPE'
                              AND COD_GRUPO in (
                                  SELECT COD_GRUPO
                                  FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = 'B_SEPE'
                                  AND COD_BENEFICIO = @codBeneficio
                              )", new { codBeneficio });
                    beneficio.monto_aplicado = beneficio.monto;

                    beneficio.tipo = new AfBeneficioIntegralDropsLista { item = "M" };

                    var respBeneficio = _beneIntegral.BeneficioIntegralGeneral_Guardar(CodEmpresa, "API", beneficio).Result;

                    if (respBeneficio.Code == -1)
                    {
                        response.Code = -1;
                        msjError += respBeneficio.Description + "...";
                    }
                    else
                    {
                        var expediente = (respBeneficio.Description ?? "0@0").Split('@');
                        var nExpediente = expediente[0].PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].PadLeft(6, '0');

                        connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                   SET [N_EXPEDIENTE] = @nExpediente, [CONSEC] = @consec, COD_BENEFICIO = @codBeneficio,
                                       ID_BENEFICIO = @idBeneficio, [ESTADO] = 'S', INCLUIDO_POR = @usuario, INCLUIDO_FECHA = getdate()
                                 WHERE ID_ZOHO = @idZoho",
                            new { nExpediente, consec = expediente[1], codBeneficio = codBeneficio.Trim(), idBeneficio = expediente[0], usuario, idZoho = jsonZoho.ticket });

                        // Los adjuntos no se transfieren porque esta versión aún no dispone de la
                        // infraestructura HTTP de threads y attachments de Zoho Desk.

                        if (expediente[0] != "0")
                        {
                            var filtros = new FrmFiltros
                            {
                                codCliente = CodEmpresa,
                                cod_beneficio = codBeneficio.Trim(),
                                id_beneficio = beneficio.id_beneficio,
                                socio = beneficio.cedula,
                                usuario = usuario
                            };

                            IncluirRespuestasFormularios(filtros, datos);
                        }
                    }
                }

                if (msjError.Trim() != string.Empty)
                {
                    response.Code = -1;
                    response.Description = msjError;

                    connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                               SET [MSJ_INTERFACE] = @msjError, [ESTADO] = 'E', VISTO_POR = @usuario, VISTO_FECHA = getdate()
                             WHERE ID_ZOHO = @idZoho", new { msjError, usuario, idZoho = jsonZoho.ticket });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda el expediente de un ticket de tipo Reconocimientos e inserta el registro de reconocimiento asociado.
        /// </summary>
        private ErrorDto Reconocimientos_Guardar(int CodEmpresa, Dictionary<string, JsonElement> datos, string usuario, ZohoTicketAdd jsonZoho)
        {
            var response = new ErrorDto { Code = 0 };
            var msjError = string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                var cedula = CfStr(datos, CampoCedulaZoho);
                if (string.IsNullOrEmpty(cedula))
                {
                    response.Code = -1;
                    msjError += MensajeCedulaRequerida;
                }

                var estadoSocio = _mBeneficiosDB.ValidaEstadoSocio(CodEmpresa, (cedula ?? string.Empty).Trim());
                if (estadoSocio.Code == -1)
                {
                    response.Code = -1;
                    msjError += estadoSocio.Description + "...";
                }

                if (response.Code != -1)
                {
                    var reconocimiento = (CfStr(datos, "cf_tipo_de_reconocimiento") ?? string.Empty).Trim();
                    var codBeneficio = reconocimiento switch
                    {
                        "Académico" => "MEAC",
                        "Científico" => "MERC",
                        "Artístico" => "MERA",
                        "Deportivo" => "MERD",
                        _ => string.Empty
                    };

                    var beneficio = new BeneficioGeneralDatos
                    {
                        cod_beneficio = new AfBeneficioIntegralDropsLista { item = codBeneficio },
                        id_beneficio = 0,
                        cedula = (cedula ?? string.Empty).Trim(),
                        monto_aplicado = 0,
                        registra_user = usuario,
                        modifica_usuario = usuario,
                        sepelio_identificacion = null,
                        estado = new AfBeneficioIntegralDropsLista { item = string.Empty },
                        consec = 0,
                        requiere_justificacion = jsonZoho.justificacion != null,
                        notas = jsonZoho.justificacion ?? string.Empty
                    };

                    beneficio.monto = connection.QueryFirstOrDefault<float>(@"SELECT [MONTO]
                                  FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = 'B_RECO'
                                  AND COD_GRUPO in (
                                      SELECT COD_GRUPO
                                      FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = 'B_RECO'
                                      AND COD_BENEFICIO = @codBeneficio
                                  )", new { codBeneficio });
                    beneficio.monto_aplicado = beneficio.monto;
                    beneficio.tipo = new AfBeneficioIntegralDropsLista { item = "M" };

                    var respBeneficio = _beneIntegral.BeneficioIntegralGeneral_Guardar(CodEmpresa, "API", beneficio).Result;

                    if (respBeneficio.Code == -1)
                    {
                        response.Code = -1;
                        msjError += respBeneficio.Description + "...";
                    }
                    else
                    {
                        var expediente = (respBeneficio.Description ?? "0@0").Split('@');
                        var nExpediente = expediente[0].PadLeft(6, '0') + codBeneficio.Trim() + expediente[1].PadLeft(6, '0');

                        connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                                   SET [N_EXPEDIENTE] = @nExpediente, [CONSEC] = @consec, COD_BENEFICIO = @codBeneficio,
                                       ID_BENEFICIO = @idBeneficio, [ESTADO] = 'S', INCLUIDO_POR = @usuario, INCLUIDO_FECHA = getdate()
                                 WHERE ID_ZOHO = @idZoho",
                            new { nExpediente, consec = expediente[1], codBeneficio = codBeneficio.Trim(), idBeneficio = expediente[0], usuario, idZoho = jsonZoho.ticket });

                        if (expediente[0] != "0")
                        {
                            Reconocimientos_Detalle_Guardar(new ReconocimientoGuardarRequest
                            {
                                CodEmpresa = CodEmpresa,
                                Datos = datos,
                                Usuario = usuario,
                                CodigoBeneficio = codBeneficio,
                                Expediente = expediente
                            });
                            var filtros = new FrmFiltros
                            {
                                codCliente = CodEmpresa,
                                cod_beneficio = codBeneficio.Trim(),
                                id_beneficio = beneficio.id_beneficio,
                                socio = beneficio.cedula,
                                usuario = usuario
                            };

                            IncluirRespuestasFormularios(filtros, datos);
                        }

                        // Los adjuntos no se transfieren porque esta versión aún no dispone de la
                        // infraestructura HTTP de threads y attachments de Zoho Desk.
                    }
                }

                if (msjError.Trim() != string.Empty)
                {
                    response.Code = -1;
                    response.Description = msjError;

                    connection.Execute(@"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                               SET [MSJ_INTERFACE] = @msjError, [ESTADO] = 'E'
                             WHERE ID_ZOHO = @idZoho", new { msjError, idZoho = jsonZoho.ticket });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
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
