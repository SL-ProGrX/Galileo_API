using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using System.Data;
using System.Text.RegularExpressions;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_PrincipalDB
    {
        private readonly IConfiguration _config;

        public frmAF_PrincipalDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene datos de catalogo generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodCatalogo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CatalogosGenerales_Obtener(int CodEmpresa, string CodCatalogo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Catalogo_Id as item, Descripcion from AFI_CATALOGOS Where Tipo_Id = {CodCatalogo}";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene datos de los catalogo generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_institucion"></param>
        /// <returns></returns>
        public ErrorDto<AfCatalogosGeneralesDto> AF_Catalogos_Obtener(int CodEmpresa, string? cod_institucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<AfCatalogosGeneralesDto>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new AfCatalogosGeneralesDto()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var parameters = new DynamicParameters();
                if (string.IsNullOrWhiteSpace(cod_institucion) || cod_institucion == "undefined")
                    parameters.Add("@cod_institucion", null, DbType.String);
                else
                    parameters.Add("@cod_institucion", cod_institucion, DbType.String);

                using var multi = connection.QueryMultiple(
                    "spAF_Catalogos_Consulta",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                response.Result.EstadoCivil = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Divisas = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.TiposIdentificacion = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Profesiones = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Sectores = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Sociedades = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.ActividadesEconomicas = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Paises = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.EstadosPersonaIngreso = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Nacionalidades = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.NivelAcademico = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.EstadoLaboral = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.ActividadLaboral = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.RelacionParentesco = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Promotores = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Instituciones = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Deductoras = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Departamentos = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Secciones = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Unidad = multi.Read<DropDownListaGenericaModel>().ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene persona por cédula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns>Datos de la persona</returns>
        public ErrorDto<AfPersonaDto> AF_Persona_Obtener(int CodEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AfPersonaDto>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = null
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var parameters = new DynamicParameters();
                parameters.Add("@Cedula", cedula);

                var result = connection.QueryFirstOrDefault<AfPersonaDto>("spAFI_Persona_Consulta", parameters, commandType: CommandType.StoredProcedure);

                response.Result = result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Guarda los datos de una persona en el sistema.
        /// </summary>
        /// <param name = "CodEmpresa" ></param>
        /// <param name="request"></param>
        /// <param name="mov"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Guardar(int CodEmpresa, string request, string mov)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            AfPersonaAddRequestDto req = JsonConvert.DeserializeObject<AfPersonaAddRequestDto>(request) ?? new AfPersonaAddRequestDto();

            var response = new ErrorDto
            {
                Code = 0,
            };

            try
            {

                var validar = AF_Persona_Validar(CodEmpresa, request);
                if (validar.Code != 0)
                {
                    return validar;
                }

                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                // Identificación / Estado
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

                // Ingreso / Laboral
                p.Add("@PromotorId", req.PromotorId);
                p.Add("@Boleta", req.Boleta);
                p.Add("@fIngreso", req.fIngreso);
                p.Add("@EstadoLaboral", req.EstadoLaboral);

                // Nacimiento / Nacionalidad
                p.Add("@PaisNac", req.PaisNac);
                p.Add("@Nacionalidad", req.Nacionalidad);

                // Contacto
                p.Add("@Email_1", req.Email_1);
                p.Add("@Email_2", req.Email_2);

                // Dirección principal
                p.Add("@Provincia", req.Provincia);
                p.Add("@Canton", req.Canton);
                p.Add("@Distrito", req.Distrito);
                p.Add("@Direccion", req.Direccion);
                p.Add("@AptoPostal", req.AptoPostal);
                p.Add("@Notificacion", req.Notificacion);

                // Institución / Org
                p.Add("@Institucion", req.Institucion);
                p.Add("@Departamento", req.Departamento);
                p.Add("@Seccion", req.Seccion);
                p.Add("@UP", req.UP);
                p.Add("@UT", req.UT);
                p.Add("@CT", req.CT);
                p.Add("@Deductora", req.Deductora);

                // Perfil
                p.Add("@Profesion", req.Profesion);
                p.Add("@Sector", req.Sector);
                p.Add("@NPagos", req.NPagos);
                p.Add("@NHijos", req.NHijos);
                p.Add("@PriDeduc", req.PriDeduc);
                p.Add("@fNombramiento", req.fNombramiento);
                p.Add("@NivelAcademico", req.NivelAcademico);

                // Jurídico / Económico
                p.Add("@Sociedad", req.Sociedad);
                p.Add("@Actividad", req.Actividad);
                p.Add("@Propiedades", req.Propiedades);
                p.Add("@Oficina", req.Oficina);

                // Redes
                p.Add("@facebook", req.Facebook);
                p.Add("@Twitter", req.Twitter);
                p.Add("@LinkedIn", req.LinkedIn);
                p.Add("@Instagram", req.Instagram);
                p.Add("@Blog", req.Blog);

                // Conyuge
                p.Add("@ConyugeCedula", req.ConyugeCedula);
                p.Add("@ConyugeNombre", req.ConyugeNombre);
                p.Add("@ConyugeTelCel", req.ConyugeTelCel);
                p.Add("@ConyugeTelTra", req.ConyugeTelTra);
                p.Add("@ConyugeTelTraExt", req.ConyugeTelTraExt);

                // Albacea
                p.Add("@AlbaceaCedula", req.AlbaceaCedula);
                p.Add("@AlbaceaNombre", req.AlbaceaNombre);
                p.Add("@AlbaceaTelCel", req.AlbaceaTelCel);
                p.Add("@AlbaceaTelTra", req.AlbaceaTelTra);
                p.Add("@AlbaceaTelTraExt", req.AlbaceaTelTraExt);

                // Salario
                p.Add("@SalarioTipo", req.SalarioTipo);
                p.Add("@SalarioDivisa", req.SalarioDivisa);
                p.Add("@SalarioFecha", req.SalarioFecha);
                p.Add("@SalarioDevengado", req.SalarioDevengado);
                p.Add("@SalarioRebajos", req.SalarioRebajos);
                p.Add("@SalarioNeto", req.SalarioNeto);
                p.Add("@SalarioEmbargo", req.SalarioEmbargo == "1" ? "S" : "N");

                // Flags / Otros
                p.Add("@AdminitraAportePatronal", req.AdministraAportePatronal);
                p.Add("@Sugef", req.Sugef);
                p.Add("@I_Beneficiario", req.I_Beneficiario);
                p.Add("@I_TrabajoPropio", req.I_TrabajoPropio);
                p.Add("@Tipo_Patron", req.Tipo_Patron);
                p.Add("@CargoDesc", req.CargoDesc);

                // PEP
                p.Add("@PEP_Ind", req.PEP_Ind);
                p.Add("@PEP_Inicio", req.PEP_Inicio);
                p.Add("@PEP_Corte", req.PEP_Corte);
                p.Add("@PEP_Cargo", req.PEP_Cargo);
                p.Add("@TipoCES", req.TipoCES);
                p.Add("@C_Actividad", req.C_Actividad);

                // Auditoría / Movimiento
                p.Add("@Usuario", req.Usuario);
                p.Add("@Mov", mov);

                // Dirección de Trabajo (opcionales)
                p.Add("@TraProvincia", req.TraProvincia);
                p.Add("@TraCanton", req.TraCanton);
                p.Add("@TraDistrito", req.TraDistrito);
                p.Add("@TraDireccion", req.TraDireccion);

                foreach (var name in p.ParameterNames)
                {
                    var val = p.Get<dynamic>(name);
                    Console.WriteLine($"{name}: {val ?? "NULL"} ({val?.GetType().Name})");
                }


                var result = connection.QuerySingle<AfPersonaAddResultDto>("dbo.spAFI_Persona_Add", p, commandType: CommandType.StoredProcedure
                );

                if (result.Pass == 0)
                {
                    response.Code = -1;
                    response.Description = result.Error_Msj ?? "No se pudo procesar el registro.";
                }
                else
                {
                    response.Description = "Operación realizada correctamente";
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
        /// Registra un nombramiento para una persona en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <param name="mov"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Nombramientos_Add(int CodEmpresa, string req, string mov)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            AfPersonaNombramientoDto request = JsonConvert.DeserializeObject<AfPersonaNombramientoDto>(req) ?? new AfPersonaNombramientoDto();

            var response = new ErrorDto
            {
                Code = 0,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var parameters = new DynamicParameters();
                parameters.Add("@Cedula", request.cedula);
                parameters.Add("@EstadoLaboral", request.estado_laboral);
                parameters.Add("@Fecha", request.registro_fecha);
                parameters.Add("@Usuario", request.registro_usuario);
                parameters.Add("@Mov", mov);

                connection.Execute("spAFI_Persona_Nombramientos_Add", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Agrega o actualiza una relación de una persona en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Relacion_Add(int CodEmpresa, string request, string mov)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            AfPersonaRelacionDtoAdd req = JsonConvert.DeserializeObject<AfPersonaRelacionDtoAdd>(request) ?? new AfPersonaRelacionDtoAdd();

            var response = new ErrorDto { Code = 0, Description = "Registro realizado correctamente" };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var parameters = new DynamicParameters();
                parameters.Add("@IdCedPrincipal", req.cedulasocio);
                parameters.Add("@TipoId", req.cod_tipo_id);
                parameters.Add("@Cedula", req.cedula);
                parameters.Add("@Nombre_Completo", req.nombre);
                parameters.Add("@Apellido_1", req.apellido1);
                parameters.Add("@Apellido_2", req.apellido2);
                parameters.Add("@Nombre", req.nombre);
                parameters.Add("@TipoRelacion", req.cod_tipo_vinculo);
                parameters.Add("@Empleado", req.empleado);
                parameters.Add("@TelTrab", req.teltra);
                parameters.Add("@TelTrabExt", req.teltraext);
                parameters.Add("@TelCell", req.telcell);
                parameters.Add("@Usuario", req.registro_usuario);
                parameters.Add("@Mov", mov);

                connection.Execute("spAFI_Persona_Relacion_Add", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Elimina una relación de una persona en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idRelacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Relacion_Del(int CodEmpresa, int idRelacion, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0, Description = "Registro eliminado correctamente" };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var parameters = new DynamicParameters();
                parameters.Add("@Id", idRelacion);
                parameters.Add("@Usuario", usuario);

                connection.Execute("spAFI_Persona_Relacion_Del", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Agrega un salario para una persona en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <param name="mov"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Salarios_Add(int CodEmpresa, string req, string mov)
        {
            string cn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            AfPersonaSalarioAddDto request = JsonConvert.DeserializeObject<AfPersonaSalarioAddDto>(req) ?? new AfPersonaSalarioAddDto();

            var response = new ErrorDto { Code = 0 };
            try
            {
                using var connection = new SqlConnection(cn);
                var p = new DynamicParameters();
                p.Add("@Cedula", request.Cedula);
                p.Add("@Tipo", request.TipoSalario);
                p.Add("@Divisa", request.Divisa);
                p.Add("@Fecha", request.Fecha);
                p.Add("@SalarioDevengado", request.Devengado);
                p.Add("@Rebajos", request.Rebajos);
                p.Add("@SalarioNeto", request.Neto);
                p.Add("@Embargo", request.Embargos);
                p.Add("@Usuario", request.Usuario);
                p.Add("@mov", "A");

                connection.Execute("spAFI_Persona_Salarios_Add", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Agrega un ingreso económico para una persona en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Ingresos_Economicos_Add(int CodEmpresa, AfPersonaIngresoEconomicoAddDto request)
        {
            string cn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0, Description = "Ingreso económico registrado correctamente" };
            try
            {
                using var connection = new SqlConnection(cn);
                var p = new DynamicParameters();
                p.Add("@Cedula", request.Cedula);
                p.Add("@Ingreso", request.Ingreso);
                p.Add("@Usuario", request.Usuario);
                p.Add("@Tipo", request.Tipo); // en VB6 usaban 1

                connection.Execute("spAFI_Persona_Ingresos_Economicos_Add", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;


        }


        /// <summary>
        /// Insetar una dirección para una persona en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Direccion_Add(int CodEmpresa, string request, string mov)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            AfPersonaDireccionDto req = JsonConvert.DeserializeObject<AfPersonaDireccionDto>(request) ?? new AfPersonaDireccionDto();


            var response = new ErrorDto
            {
                Code = 0,
                Description = "Dirección registrada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                p.Add("@Cedula", req.CEDULA);
                p.Add("@Provincia", req.PROVINCIA);
                p.Add("@Canton", req.CANTON);
                p.Add("@Distrito", req.DISTRITO);
                p.Add("@Direccion", req.DIRECCION);
                p.Add("@Email01", string.IsNullOrWhiteSpace(req.EMAIL_01) ? null : req.EMAIL_01);
                p.Add("@Email02", string.IsNullOrWhiteSpace(req.EMAIL_02) ? null : req.EMAIL_02);
                p.Add("@Telefono01", string.IsNullOrWhiteSpace(req.TELEFONO_01) ? null : req.TELEFONO_01);
                p.Add("@Telefono02", string.IsNullOrWhiteSpace(req.TELEFONO_02) ? null : req.TELEFONO_02);
                p.Add("@Usuario", req.REGISTRO_USUARIO);
                p.Add("@Mov", mov);
                p.Add("@CodApp", req.COD_APP);
                p.Add("@Tipo", req.Tipo);

                connection.Execute("dbo.spAFI_Persona_Direccion_Add", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Registra escolaridad de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Escolaridad_Registra(int CodEmpresa, string request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AfPersonaEscolaridadRegistraDto req = JsonConvert.DeserializeObject<AfPersonaEscolaridadRegistraDto>(request) ?? new AfPersonaEscolaridadRegistraDto();

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                string codEscolaridad;
                if (req.CodEscolaridad.Contains("."))
                {
                    var partes = req.CodEscolaridad.Split('.');
                    var entero = partes[0].PadLeft(2, '0');
                    codEscolaridad = $"{entero}.{partes[1]}";
                }
                else
                {
                    codEscolaridad = req.CodEscolaridad.PadLeft(2, '0');
                }

                p.Add("@Cedula", req.Cedula);
                p.Add("@Codigo", codEscolaridad);
                p.Add("@TipoMov", req.Asignado ? "A" : "E");
                p.Add("@Usuario", req.Usuario);

                connection.Execute("dbo.spAFI_Persona_Escolaridad_Registra", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Registra preferencias de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Preferencias_Registra(int CodEmpresa, string request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AfPreferenciaDto req = JsonConvert.DeserializeObject<AfPreferenciaDto>(request) ?? new AfPreferenciaDto();
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                string codPreferencia;
                if (req.cod_preferencia.Contains("."))
                {
                    var partes = req.cod_preferencia.Split('.');
                    var entero = partes[0].PadLeft(2, '0');
                    codPreferencia = $"{entero}.{partes[1]}";
                }
                else
                {
                    codPreferencia = req.cod_preferencia.PadLeft(2, '0');
                }

                p.Add("@Cedula", req.Cedula);
                p.Add("@Codigo", codPreferencia);
                p.Add("@TipoMov", req.asignado ? "A" : "E");
                p.Add("@Usuario", req.registro_usuario);

                connection.Execute("dbo.spAFI_Persona_Preferencias_Registra", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Registra canales de preferencia para la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Canales_Registra(int CodEmpresa, string req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AfCanalesDto request = JsonConvert.DeserializeObject<AfCanalesDto>(req) ?? new AfCanalesDto();
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                p.Add("@Cedula", request.cedula);
                p.Add("@Canal", request.canal_tipo.ToString("D2"));
                p.Add("@TipoMov", request.asignado ? "A" : "E");
                p.Add("@Usuario", request.registro_usuario);

                connection.Execute("dbo.spAFI_Persona_Canales_Registra", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Vincula persona a patrimonio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Patrimonio_Vincula(int CodEmpresa, AfPersonaPatrimonioVinculaDto req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();
                p.Add("@Cedula", req.Cedula);

                // Si tu SP está con dbo:
                connection.Execute("dbo.spAFI_PERSONA_PATRIMONIO_Vincula", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Registra bienes de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Bienes_Registra(int CodEmpresa, string req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AfPersonaBienesRegistraDto request = JsonConvert.DeserializeObject<AfPersonaBienesRegistraDto>(req) ?? new AfPersonaBienesRegistraDto();

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                string codBien;
                if (request.CodBien.Contains("."))
                {
                    var partes = request.CodBien.Split('.');
                    var entero = partes[0].PadLeft(2, '0');
                    codBien = $"{entero}.{partes[1]}";
                }
                else
                {
                    codBien = request.CodBien.PadLeft(2, '0');
                }

                p.Add("@Cedula", request.Cedula);
                p.Add("@Codigo", codBien);
                p.Add("@TipoMov", request.Asignado ? "A" : "E");
                p.Add("@Usuario", request.Usuario);

                connection.Execute("dbo.spAFI_Persona_Bienes_Registra", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Registra productos de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Productos_Registra(int CodEmpresa, string req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            AfPersonaProductosRegistraDto request = JsonConvert.DeserializeObject<AfPersonaProductosRegistraDto>(req) ?? new AfPersonaProductosRegistraDto();
            var response = new ErrorDto
            {
                Code = 0,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                p.Add("@Cedula", request.cedula);
                p.Add("@Codigo", request.codproducto);
                p.Add("@TipoMov", request.asignado ? "A" : "E");
                p.Add("@Usuario", request.usuario);

                connection.Execute("dbo.spAFI_Persona_Productos_Registra", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Registro default
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_RegistroDefault(int CodEmpresa, AfRegistroDefaultDto req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();
                p.Add("@Cedula", req.Cedula);
                p.Add("@Usuario", req.Usuario);

                connection.Execute("dbo.spAFI_RegistroDefault", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene Provincias
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Provincias_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Provincia as item, rtrim(Descripcion) as descripcion from Provincias";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene Cantones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Provincia"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Cantones_Obtener(int CodEmpresa, string Provincia)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Canton as item, rtrim(Descripcion) as descripcion from Cantones
                        where provincia = @Provincia order by descripcion";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { Provincia }).ToList();
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
        /// Obtiene Distritos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Provincia"></param>
        /// <param name="Canton"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Distritos_Obtener(int CodEmpresa, string Provincia, string Canton)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Distrito as item, rtrim(Descripcion) as descripcion from Distritos
                        where provincia = @Provincia and canton = @Canton order by descripcion";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { Provincia, Canton }).ToList();
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
        /// Scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<string> TES_Persona_Scroll(int CodEmpresa, int scrollCode, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = "0"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Top 1 cedula from socios";

                    switch (scrollCode)
                    {
                        case 0:
                            if (cedula == "")
                            {
                                cedula = "0";
                            }
                            query += $@" where cedula < @cedula order by cedula desc";

                            break;
                        case 1:
                            if (cedula == "0")
                            {
                                cedula = " ";
                            }

                            query += $@" where cedula > @cedula order by cedula asc";

                            break;
                        default:
                            break;
                    }

                    response.Result = connection.Query<string>(query, new { cedula }).FirstOrDefault();
                    if (response.Result == "0")
                    {
                        TES_Persona_Scroll(CodEmpresa, scrollCode, response.Result.ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = "0";
            }

            return response;
        }


        /// <summary>
        /// Obtiene los productos que tiene una empresa en la organizacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCumplimientoDto>> AF_PersonaProductos_Consulta(int CodEmpresa, string Cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<AfCumplimientoDto>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<AfCumplimientoDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spAFI_Persona_Productos_Consulta @Cedula";
                    response.Result = connection.Query<AfCumplimientoDto>(query, new { Cedula }).ToList();
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
        /// Obtiene datos de los catalogo generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfConsultasGeneralesDto> AF_Persona_Consulta_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<AfConsultasGeneralesDto>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new AfConsultasGeneralesDto()
            };

            try
            {
                using var connection = new SqlConnection(stringConn); connection.Open();


                var parametros = new { Cedula = cedula, Usuario = usuario };

                using var multi = connection.QueryMultiple("spAF_Persona_Consultas", param: parametros, commandType: CommandType.StoredProcedure);

                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                response.Result.Telefonos = multi.Read<AfTelefonosDto>().ToList();
                response.Result.CuentasBancarias = multi.Read<AfCuentaBancariaDto>().ToList();
                response.Result.Beneficiarios = multi.Read<AfPersonaBeneficiarioDto>().ToList();
                response.Result.Tarjetas = multi.Read<AfTarjetaDto>().ToList();
                response.Result.Localizaciones = multi.Read<AfDireccionDto>().ToList();
                response.Result.Ingresos = multi.Read<AfPersonaIngresoDto>().ToList();
                response.Result.Renuncias = multi.Read<AfPersonaRenunciaDto>().ToList();
                response.Result.Liquidaciones = multi.Read<AfPersonaLiquidacionDto>().ToList();
                response.Result.Nombramientos = multi.Read<AfPersonaNombramientoDto>().ToList();
                response.Result.Salarios = multi.Read<AfPersonaSalarioDto>().ToList();
                response.Result.Emails = multi.Read<AfPersonaEmailDto>().ToList();
                response.Result.Motivos = multi.Read<AfMotivosDto>().ToList();
                response.Result.Canales = multi.Read<AfCanalesDto>().ToList();
                response.Result.Preferencias = multi.Read<AfPreferenciaDto>().ToList();
                response.Result.Bienes = multi.Read<AfBienDto>().ToList();
                response.Result.Escolaridad = multi.Read<AfEscolaridadDto>().ToList();
                response.Result.Relaciones = multi.Read<AfPersonaRelacionDto>().ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Registra Indicadores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Indicadores_Registra(int CodEmpresa, string req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            AfPersonaIndicadoresDto request = JsonConvert.DeserializeObject<AfPersonaIndicadoresDto>(req) ?? new AfPersonaIndicadoresDto();
            var response = new ErrorDto
            {
                Code = 0,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                p.Add("@Cedula", request.cedula);
                p.Add("@Indicador", request.indicador);
                p.Add("@Valor", request.valor);
                p.Add("@Usuario", request.usuario);
                p.Add("@Nota", request.nota ?? string.Empty);

                connection.Execute("dbo.spAFI_Persona_Indicadores", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Primera deduccion registra
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="prideduc"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_PrimeraDeduccion_Registra(int CodEmpresa, string cedula, string prideduc)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = $@"UPDATE socios SET Prideduc = @prideduc WHERE cedula = @cedula';";
                connection.Execute(query, new { prideduc, cedula });

                response.Code = 0;

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Elimina persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Elimina(int CodEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = $@"delete socios WHERE cedula = @cedula';";
                connection.Execute(query, new { cedula });

                response.Code = 0;

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<string> AF_Scroll_General(int CodEmpresa, int scrollCode, string id, int tipoScroll, string cod_Institucion, string cod_Departamento = "")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = "0"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var tablas = new Dictionary<int, (string Tabla, string Campo, string[] Filtros)>
                    {
                        { 1, ("PROMOTORES", "id_promotor", new string[] { }) },
                        { 2, ("INSTITUCIONES", "cod_institucion", new string[] { }) },
                        { 3, ("vAFI_Deductoras", "cod_deductora", new string[] { "cod_institucion" }) },
                        { 4, ("AFDepartamentos", "cod_departamento", new string[] { "cod_institucion" }) },
                        { 5, ("AFSecciones", "cod_seccion", new string[] { "cod_institucion", "cod_departamento" }) },
                        { 6, ("AFI_Profesiones", "cod_profesion", new string[] { }) },
                        { 7, ("AFI_Sectores", "cod_sector", new string[] { }) },
                        { 8, ("AFI_SOCIEDADES_TIPOS", "cod_sociedad", new string[] { }) },
                        { 9, ("Unidad", "cod_unidad", new string[] { }) }

                    };

                if (!tablas.ContainsKey(tipoScroll))
                {
                    response.Description = "Tipo de scroll no soportado";
                    return response;
                }

                var (tabla, campo, filtros) = tablas[tipoScroll];

                if (string.IsNullOrEmpty(id))
                    id = "0";

                var order = scrollCode == 0 ? "DESC" : "ASC";
                var operador = scrollCode == 0 ? "<" : ">";

                var query = $@"
                        SELECT TOP 1 {campo} 
                        FROM {tabla} 
                        WHERE {campo} {operador} @id";

                // Aplica filtros
                if (filtros.Contains("cod_institucion"))
                    query += $" AND cod_institucion = @cod_Institucion";

                if (filtros.Contains("cod_departamento") && !string.IsNullOrEmpty(cod_Departamento))
                    query += $" AND cod_departamento = @cod_Departamento";

                query += $" ORDER BY {campo} {order}";

                response.Result = connection.Query<string>(query, new { id, cod_Institucion, cod_Departamento }).FirstOrDefault();

                if (response.Result == "0")
                {
                    return AF_Scroll_General(CodEmpresa, scrollCode, response.Result.ToString(), tipoScroll, cod_Institucion, cod_Departamento);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = "0";
            }

            return response;
        }

        /// <summary>
        /// Valida Persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Validar(int CodEmpresa, string req)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AfPersonaAddRequestDto dto = JsonConvert.DeserializeObject<AfPersonaAddRequestDto>(req) ?? new AfPersonaAddRequestDto();
            var response = new ErrorDto { Code = 0, Description = "Validación correcta" };

            try
            {
                using var connection = new SqlConnection(connStr);
                var errores = new List<string>();

                // Validar largo de cédula
                var largo = connection.QueryFirstOrDefault<int>(
                    "SELECT LARGO_MINIMO FROM AFI_TIPOS_IDS WHERE TIPO_ID = @TipoId",
                    new { dto.TipoId });

                if (!string.IsNullOrEmpty(dto.Cedula) && dto.Cedula.Length != largo)
                    errores.Add($"Número de Identidad inválido, se espera {largo} caracteres.");

                if (dto.Cedula.Length > 20)
                    errores.Add("Número de Identidad no puede superar 20 caracteres.");


                if (!EsEmailValido(dto.Email_1))
                    errores.Add("Email principal no es válido.");
                if (!string.IsNullOrEmpty(dto.Email_2) && !EsEmailValido(dto.Email_2))
                    errores.Add("Email secundario no es válido.");

                if (string.IsNullOrWhiteSpace(dto.Apellido_1)) errores.Add("Falta el Apellido 1.");
                if (string.IsNullOrWhiteSpace(dto.Apellido_2)) errores.Add("Falta el Apellido 2.");
                if (string.IsNullOrWhiteSpace(dto.Nombre)) errores.Add("Falta el Nombre.");
                if (string.IsNullOrWhiteSpace(dto.Genero)) errores.Add("No se especificó el Sexo.");
                if (string.IsNullOrWhiteSpace(dto.EstadoCivil)) errores.Add("No se especificó el Estado Civil.");



                if (dto.SalarioDivisa == "COL")
                {
                    if (dto.SalarioDevengado < 100000 || dto.SalarioDevengado > 10000000)
                        errores.Add("Salario devengado fuera de rango (COL).");
                }
                else
                {
                    if (dto.SalarioDevengado < 200 || dto.SalarioDevengado > 20000)
                        errores.Add("Salario devengado fuera de rango (USD).");
                }


                if (string.IsNullOrWhiteSpace(dto.Provincia)) errores.Add("No se especificó la Provincia.");
                if (string.IsNullOrWhiteSpace(dto.Canton)) errores.Add("No se especificó el Cantón.");
                if (string.IsNullOrWhiteSpace(dto.Distrito)) errores.Add("No se especificó el Distrito.");
                if (string.IsNullOrWhiteSpace(dto.Direccion)) errores.Add("No se especificó la Dirección.");


                //if (string.IsNullOrWhiteSpace(dto.TraProvincia))
                //    errores.Add("No se especificó la Provincia de la Dirección de Trabajo.");
                //if (string.IsNullOrWhiteSpace(dto.TraCanton))
                //    errores.Add("No se especificó el Cantón de la Dirección de Trabajo.");
                //if (string.IsNullOrWhiteSpace(dto.TraDistrito))
                //    errores.Add("No se especificó el Distrito en la Dirección de Trabajo.");


                //if (string.IsNullOrWhiteSpace(dto.TraDireccion) || dto.TraDireccion.Length < 5)
                //    errores.Add("La Dirección Exacta de Trabajo no fue suministrada correctamente.");


                var estadoPromotor = connection.QueryFirstOrDefault<int>(
                    "SELECT ISNULL(estado,0) FROM promotores WHERE id_promotor = @PromotorId",
                    new { dto.PromotorId });

                if (estadoPromotor == 0)
                    errores.Add("El promotor indicado está inactivo o no existe.");


                if (dto.fNacimiento > DateTime.Now.AddYears(-17))
                    errores.Add("La persona es menor de edad.");
                if (dto.fCedulaVence <= DateTime.Now.AddDays(20))
                    errores.Add("La cédula está próxima a vencer.");

                if (dto.Profesion <= 0)
                    errores.Add("Profesión no es válida.");
                if (dto.Sector <= 0)
                    errores.Add("Sector no es válido.");

                if (string.IsNullOrWhiteSpace(dto.EstadoLaboral))
                    errores.Add("No se especificó el Estado Laboral.");
                if (string.IsNullOrWhiteSpace(dto.NivelAcademico.ToString()))
                    errores.Add("No se especificó el nivel académico.");
                //if (string.IsNullOrWhiteSpace(dto.C_Actividad))
                //    errores.Add("No se especificó Actividad (Oficina Cumplimiento).");

                // Departamento / Unidad Programática
                if (dto.Departamento == "U.Programatica" && string.IsNullOrWhiteSpace(dto.UP))
                {
                    errores.Add("No se especificó el Departamento o Unidad Programática.");
                }

                // Puesto
                if (string.IsNullOrWhiteSpace(dto.CargoDesc))
                {
                    errores.Add("Tienen que indicar el Puesto que desempeña.");
                }

                // Estado de la Persona autorizado en la institución
                var existeEstado = connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*) 
                      FROM AFI_ESTADOS_INSTITUCIONES 
                      WHERE cod_estado = @Estado 
                        AND cod_institucion = @Institucion",
                    new { Estado = dto.Estado, Institucion = dto.Institucion });

                if (existeEstado == 0)
                {
                    errores.Add($"El ESTADO de la Persona a modificar o incluir no está autorizado en esta institución: {dto.Institucion}");
                }

                // Resultado
                if (errores.Count > 0)
                {
                    response.Code = -2;
                    response.Description = string.Join(Environment.NewLine, errores);
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
        /// Email valido
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        /// <summary>
        /// Obtiene Persona patron nacional
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfPadronPersonaDto> AF_PersonaPadron_Obtener(int codEmpresa, string cedula)
        {
            string stringConn = _config.GetConnectionString("BaseConnString");
            var response = new ErrorDto<AfPadronPersonaDto>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var parameters = new DynamicParameters();
                parameters.Add("@Identificacion", cedula);
                parameters.Add("@Pais", "CRI");

                response.Result = connection.QueryFirstOrDefault<AfPadronPersonaDto>(
                    "spSYS_Consulta_Padron",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Agrega el Dimex a la persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Dimex_Add(int codEmpresa, string req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            AfPersonaDimexAddDto request = JsonConvert.DeserializeObject<AfPersonaDimexAddDto>(req) ?? new AfPersonaDimexAddDto();

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                p.Add("@Cedula", request.cedula);
                p.Add("@Dimex", request.dimex);
                p.Add("@Activo", request.activo);
                p.Add("@Usuario", request.usuario);

                connection.Execute("dbo.spAFI_Persona_Dimex_Add", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Elimina una direccion de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="linea"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Direccion_Elimina(int codEmpresa, string cedula, string linea, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);


            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                p.Add("@Cedula", cedula);
                p.Add("@Linea", linea);
                p.Add("@Usuario", usuario);

                connection.Execute("dbo.spAFI_Persona_Direccion_Elimina", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }



        /// <summary>
        /// Registra motivos de afiliacion de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Motivos_Registra(int CodEmpresa, string request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AfMotivosDto req = JsonConvert.DeserializeObject<AfMotivosDto>(request) ?? new AfMotivosDto();
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                string codMotivo;
                if (req.cod_motivo.Contains("."))
                {
                    var partes = req.cod_motivo.Split('.');
                    var entero = partes[0].PadLeft(2, '0');
                    codMotivo = $"{entero}.{partes[1]}";
                }
                else
                {
                    codMotivo = req.cod_motivo.PadLeft(2, '0');
                }

                p.Add("@Cedula", req.cedula);
                p.Add("@Motivo", codMotivo);
                p.Add("@TipoMov", req.asignado ? "A" : "E");
                p.Add("@Usuario", req.registro_usuario);

                connection.Execute("dbo.spAFI_Persona_Motivos_Registra", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene la fecha del servidor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                string sql = "SELECT dbo.MyGetdate() AS Fecha";

                var fechaServidor = connection.QuerySingle<DateTime>(sql);

                response.Description = fechaServidor.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


    }
}