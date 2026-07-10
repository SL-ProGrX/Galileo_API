using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using System.Data;
using System.Text.RegularExpressions;

namespace Galileo.DataBaseTier
{
    public partial class FrmAFPrincipalDB
    {
        
        #region Campos y constructor
        private readonly IConfiguration _config;
        private const string CONSULTA_REALIZADA_CORRECTAMENTE = "Consulta realizada correctamente";
        private const string OPERACION_REALIZADA_CORRECTAMENTE = "Operación realizada correctamente";
        private const string CAMPO_COD_INSTITUCION = "cod_institucion";
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);

        public FrmAFPrincipalDB(IConfiguration config)
        {
            _config = config;
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene datos de catalogo generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodCatalogo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CatalogosGenerales_Obtener(int CodEmpresa, string CodCatalogo)
        {
            const string query = @"select Catalogo_Id as item, Descripcion from AFI_CATALOGOS Where Tipo_Id = @CodCatalogo";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                new { CodCatalogo });
        }

        /// <summary>
        /// Obtiene datos de los catalogo generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_institucion"></param>
        /// <returns></returns>
        public ErrorDto<AfCatalogosGeneralesDto> AF_Catalogos_Obtener(int CodEmpresa, string? cod_institucion)
        {

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parameters = new DynamicParameters();
                if (string.IsNullOrWhiteSpace(cod_institucion) || cod_institucion == "undefined")
                {
                    parameters.Add("@cod_institucion", null, DbType.String);
                }
                else
                {
                    parameters.Add("@cod_institucion", cod_institucion, DbType.String);
                }

                using var multi = connection.QueryMultiple(
                    "spAF_Catalogos_Consulta",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                return new AfCatalogosGeneralesDto
                {
                    EstadoCivil = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Divisas = multi.Read<DropDownListaGenericaModel>().ToList(),
                    TiposIdentificacion = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Profesiones = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Sectores = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Sociedades = multi.Read<DropDownListaGenericaModel>().ToList(),
                    ActividadesEconomicas = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Paises = multi.Read<DropDownListaGenericaModel>().ToList(),
                    EstadosPersonaIngreso = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Nacionalidades = multi.Read<DropDownListaGenericaModel>().ToList(),
                    NivelAcademico = multi.Read<DropDownListaGenericaModel>().ToList(),
                    EstadoLaboral = multi.Read<DropDownListaGenericaModel>().ToList(),
                    ActividadLaboral = multi.Read<DropDownListaGenericaModel>().ToList(),
                    RelacionParentesco = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Promotores = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Instituciones = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Deductoras = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Departamentos = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Secciones = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Unidad = multi.Read<DropDownListaGenericaModel>().ToList()
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new AfCatalogosGeneralesDto())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar catálogos.", result.Code.GetValueOrDefault(-1), new AfCatalogosGeneralesDto());
        }

        /// <summary>
        /// Obtiene persona por cédula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns>Datos de la persona</returns>
        public ErrorDto<AfPersonaDto> AF_Persona_Obtener(int CodEmpresa, string cedula)
        {
            var result = DbHelper.ExecuteSingleQuery<AfPersonaDto>(
                CreatePortalDb(),
                CodEmpresa,
                "spAFI_Persona_Consulta",
                null,
                new { Cedula = cedula });

            if (result.Code == 0 && result.Result is not null && string.IsNullOrWhiteSpace(result.Result.prideduc))
            {
                result.Result.prideduc = ObtenerFechaProcesoCredito(CodEmpresa);
            }

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new AfPersonaDto())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener persona.", result.Code.GetValueOrDefault(-1), new AfPersonaDto());
        }

        private string ObtenerFechaProcesoCredito(int CodEmpresa)
        {
            const string query = @"
                SELECT FORMAT(dbo.MyGetdate(), 'yyyyMM')
                FROM CntX_Contabilidades
                WHERE cod_contabilidad = @CodEmpresa";

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<string>(query, new { CodEmpresa }));

            return result.Code == 0 && !string.IsNullOrWhiteSpace(result.Result)
                ? result.Result
                : DateTime.Now.ToString("yyyyMM");
        }

        #endregion

        #region Registros y actualizaciones

        /// <summary>
        /// Guarda los datos de una persona en el sistema.
        /// </summary>
        /// <param name = "CodEmpresa" ></param>
        /// <param name="request"></param>
        /// <param name="mov"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Guardar(int CodEmpresa, string request, string mov)
        {
            var req = JsonConvert.DeserializeObject<AfPersonaAddRequestDto>(request) ?? new AfPersonaAddRequestDto();

            var validar = AF_Persona_Validar(CodEmpresa, request);
            if (validar.Code != 0)
            {
                return validar;
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var p = BuildPersonaParameters(req, mov);
                var spResult = connection.QuerySingle<AfPersonaAddResultDto>(
                    "dbo.spAFI_Persona_Add",
                    p,
                    commandType: CommandType.StoredProcedure);

                if (spResult.Pass == 0)
                {
                    throw new InvalidOperationException(spResult.Error_Msj ?? "No se pudo procesar el registro.");
                }

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse(OPERACION_REALIZADA_CORRECTAMENTE)
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar persona.", result.Code.GetValueOrDefault(-1));
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
            var request = JsonConvert.DeserializeObject<AfPersonaNombramientoDto>(req) ?? new AfPersonaNombramientoDto();
            return ExecuteStoredProcedure(
                CodEmpresa,
                "spAFI_Persona_Nombramientos_Add",
                new
                {
                    Cedula = request.cedula,
                    EstadoLaboral = request.estado_laboral,
                    Fecha = request.registro_fecha,
                    Usuario = request.registro_usuario,
                    Mov = mov
                });
        }

        /// <summary>
        /// Agrega o actualiza una relación de una persona en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Relacion_Add(int CodEmpresa, string request, string mov)
        {
            var req = JsonConvert.DeserializeObject<AfPersonaRelacionDtoAdd>(request) ?? new AfPersonaRelacionDtoAdd();
            return ExecuteStoredProcedure(
                CodEmpresa,
                "spAFI_Persona_Relacion_Add",
                new
                {
                    IdCedPrincipal = req.cedulasocio,
                    TipoId = req.cod_tipo_id,
                    Cedula = req.cedula,
                    Nombre_Completo = req.nombre,
                    Apellido_1 = req.apellido1,
                    Apellido_2 = req.apellido2,
                    Nombre = req.nombre,
                    TipoRelacion = req.cod_tipo_vinculo,
                    Empleado = req.empleado,
                    TelTrab = req.teltra,
                    TelTrabExt = req.teltraext,
                    TelCell = req.telcell,
                    Usuario = req.registro_usuario,
                    Mov = mov
                },
                "Registro realizado correctamente");
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
            return ExecuteStoredProcedure(
                CodEmpresa,
                "spAFI_Persona_Relacion_Del",
                new
                {
                    Id = idRelacion,
                    Usuario = usuario
                },
                "Registro eliminado correctamente");
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
            var request = JsonConvert.DeserializeObject<AfPersonaSalarioAddDto>(req) ?? new AfPersonaSalarioAddDto();
            return ExecuteStoredProcedure(
                CodEmpresa,
                "spAFI_Persona_Salarios_Add",
                new
                {
                    Cedula = request.Cedula,
                    Tipo = request.TipoSalario,
                    Divisa = request.Divisa,
                    Fecha = request.Fecha,
                    SalarioDevengado = request.Devengado,
                    Rebajos = request.Rebajos,
                    SalarioNeto = request.Neto,
                    Embargo = request.Embargos,
                    Usuario = request.Usuario,
                    mov = "A"
                });
        }

        /// <summary>
        /// Agrega un ingreso económico para una persona en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Ingresos_Economicos_Add(int CodEmpresa, AfPersonaIngresoEconomicoAddDto request)
        {
            return ExecuteStoredProcedure(
                CodEmpresa,
                "spAFI_Persona_Ingresos_Economicos_Add",
                new
                {
                    Cedula = request.Cedula,
                    Ingreso = request.Ingreso,
                    Usuario = request.Usuario,
                    Tipo = request.Tipo
                },
                "Ingreso económico registrado correctamente");
        }

        /// <summary>
        /// Insetar una dirección para una persona en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Direccion_Add(int CodEmpresa, string request, string mov)
        {
            var req = JsonConvert.DeserializeObject<AfPersonaDireccionDto>(request) ?? new AfPersonaDireccionDto();
            return ExecuteStoredProcedure(
                CodEmpresa,
                "dbo.spAFI_Persona_Direccion_Add",
                new
                {
                    Cedula = req.CEDULA,
                    Provincia = req.PROVINCIA,
                    Canton = req.CANTON,
                    Distrito = req.DISTRITO,
                    Direccion = req.DIRECCION,
                    Email01 = string.IsNullOrWhiteSpace(req.EMAIL_01) ? null : req.EMAIL_01,
                    Email02 = string.IsNullOrWhiteSpace(req.EMAIL_02) ? null : req.EMAIL_02,
                    Telefono01 = string.IsNullOrWhiteSpace(req.TELEFONO_01) ? null : req.TELEFONO_01,
                    Telefono02 = string.IsNullOrWhiteSpace(req.TELEFONO_02) ? null : req.TELEFONO_02,
                    Usuario = req.REGISTRO_USUARIO,
                    Mov = mov,
                    CodApp = req.COD_APP,
                    Tipo = req.Tipo
                },
                "Dirección registrada correctamente");
        }

        /// <summary>
        /// Registra escolaridad de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Escolaridad_Registra(int CodEmpresa, string request)
        {
            var req = JsonConvert.DeserializeObject<AfPersonaEscolaridadRegistraDto>(request) ?? new AfPersonaEscolaridadRegistraDto();
            var codEscolaridad = FormatearCodigoJerarquico(req.CodEscolaridad);

            return ExecuteStoredProcedure(
                CodEmpresa,
                "dbo.spAFI_Persona_Escolaridad_Registra",
                new
                {
                    Cedula = req.Cedula,
                    Codigo = codEscolaridad,
                    TipoMov = req.Asignado ? "A" : "E",
                    Usuario = req.Usuario
                });
        }

        /// <summary>
        /// Registra preferencias de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Preferencias_Registra(int CodEmpresa, string request)
        {
            var req = JsonConvert.DeserializeObject<AfPreferenciaDto>(request) ?? new AfPreferenciaDto();
            var codPreferencia = FormatearCodigoJerarquico(req.cod_preferencia);

            return ExecuteStoredProcedure(
                CodEmpresa,
                "dbo.spAFI_Persona_Preferencias_Registra",
                new
                {
                    Cedula = req.Cedula,
                    Codigo = codPreferencia,
                    TipoMov = req.asignado ? "A" : "E",
                    Usuario = req.registro_usuario
                });
        }

        /// <summary>
        /// Registra canales de preferencia para la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Canales_Registra(int CodEmpresa, string req)
        {
            var request = JsonConvert.DeserializeObject<AfCanalesDto>(req) ?? new AfCanalesDto();
            return ExecuteStoredProcedure(
                CodEmpresa,
                "dbo.spAFI_Persona_Canales_Registra",
                new
                {
                    Cedula = request.cedula,
                    Canal = request.canal_tipo.ToString("D2"),
                    TipoMov = request.asignado ? "A" : "E",
                    Usuario = request.registro_usuario
                });
        }

        /// <summary>
        /// Vincula persona a patrimonio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Patrimonio_Vincula(int CodEmpresa, AfPersonaPatrimonioVinculaDto req)
        {
            return ExecuteStoredProcedure(
                CodEmpresa,
                "dbo.spAFI_PERSONA_PATRIMONIO_Vincula",
                new { Cedula = req.Cedula });
        }

        /// <summary>
        /// Registra bienes de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Bienes_Registra(int CodEmpresa, string req)
        {
            var request = JsonConvert.DeserializeObject<AfPersonaBienesRegistraDto>(req) ?? new AfPersonaBienesRegistraDto();
            var codBien = FormatearCodigoJerarquico(request.CodBien);

            return ExecuteStoredProcedure(
                CodEmpresa,
                "dbo.spAFI_Persona_Bienes_Registra",
                new
                {
                    Cedula = request.Cedula,
                    Codigo = codBien,
                    TipoMov = request.Asignado ? "A" : "E",
                    Usuario = request.Usuario
                });
        }

        /// <summary>
        /// Registra productos de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Productos_Registra(int CodEmpresa, string req)
        {
            var request = JsonConvert.DeserializeObject<AfPersonaProductosRegistraDto>(req) ?? new AfPersonaProductosRegistraDto();
            return ExecuteStoredProcedure(
                CodEmpresa,
                "dbo.spAFI_Persona_Productos_Registra",
                new
                {
                    Cedula = request.cedula,
                    Codigo = request.codproducto,
                    TipoMov = request.asignado ? "A" : "E",
                    Usuario = request.usuario
                });
        }

        /// <summary>
        /// Registro default
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_RegistroDefault(int CodEmpresa, AfRegistroDefaultDto req)
        {
            return ExecuteStoredProcedure(
                CodEmpresa,
                "dbo.spAFI_RegistroDefault",
                new
                {
                    Cedula = req.Cedula,
                    Usuario = req.Usuario
                });
        }

        /// <summary>
        /// Obtiene Provincias
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Provincias_Obtener(int CodEmpresa)
        {
            const string query = @"select Provincia as item, rtrim(Descripcion) as descripcion from Provincias";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene Cantones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Provincia"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Cantones_Obtener(int CodEmpresa, string Provincia)
        {
            const string query = @"select Canton as item, rtrim(Descripcion) as descripcion from Cantones
                        where provincia = @Provincia order by descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                new { Provincia });
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
            const string query = @"select Distrito as item, rtrim(Descripcion) as descripcion from Distritos
                        where provincia = @Provincia and canton = @Canton order by descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                new { Provincia, Canton });
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
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var defaultCedula = scrollCode == 0 ? "0" : " ";
                var currentCedula = string.IsNullOrWhiteSpace(cedula)
                    ? defaultCedula
                    : cedula;

                var query = @"select Top 1 cedula from socios";
                query += scrollCode switch
                {
                    0 => @" where cedula < @cedula order by cedula desc",
                    1 => @" where cedula > @cedula order by cedula asc",
                    _ => string.Empty
                };

                return connection.Query<string>(query, new { cedula = currentCedula }).FirstOrDefault() ?? "0";
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? "0")
                : DbHelper.CreateErrorResponse(result.Description ?? "Error en el scroll de persona.", result.Code.GetValueOrDefault(-1), "0");
        }

        /// <summary>
        /// Obtiene los productos que tiene una empresa en la organizacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCumplimientoDto>> AF_PersonaProductos_Consulta(int CodEmpresa, string Cedula)
        {
            return DbHelper.ExecuteListQuery<AfCumplimientoDto>(
                CreatePortalDb(),
                CodEmpresa,
                "exec spAFI_Persona_Productos_Consulta @Cedula",
                new { Cedula });
        }

        /// <summary>
        /// Obtiene datos de los catalogo generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfConsultasGeneralesDto> AF_Persona_Consulta_Obtener(
            int CodEmpresa,
            string cedula,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la cedula.",
                    -1,
                    new AfConsultasGeneralesDto());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                using var multi = connection.QueryMultiple(
                    "spAF_Persona_Consultas",
                    new
                    {
                        Cedula = cedula.Trim(),
                        Usuario = (usuario ?? string.Empty).Trim()
                    },
                    commandType: CommandType.StoredProcedure);

                return new AfConsultasGeneralesDto
                {
                    Telefonos = multi.Read<AfTelefonosDto>().ToList(),
                    CuentasBancarias = multi.Read<AfCuentaBancariaDto>().ToList(),
                    Beneficiarios = multi.Read<AfPersonaBeneficiarioDto>().ToList(),
                    Tarjetas = multi.Read<AfTarjetaDto>().ToList(),
                    Localizaciones = multi.Read<AfDireccionDto>().ToList(),
                    Ingresos = multi.Read<AfPersonaIngresoDto>().ToList(),
                    Renuncias = multi.Read<AfPersonaRenunciaDto>().ToList(),
                    Liquidaciones = multi.Read<AfPersonaLiquidacionDto>().ToList(),
                    Nombramientos = multi.Read<AfPersonaNombramientoDto>().ToList(),
                    Salarios = multi.Read<AfPersonaSalarioDto>().ToList(),
                    Emails = multi.Read<AfPersonaEmailDto>().ToList(),
                    Motivos = multi.Read<AfMotivosDto>().ToList(),
                    Canales = multi.Read<AfCanalesDto>().ToList(),
                    Preferencias = multi.Read<AfPreferenciaDto>().ToList(),
                    Bienes = multi.Read<AfBienDto>().ToList(),
                    Escolaridad = multi.Read<AfEscolaridadDto>().ToList(),
                    Relaciones = multi.Read<AfPersonaRelacionDto>().ToList()
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new AfConsultasGeneralesDto())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al consultar información general de la persona.",
                    result.Code.GetValueOrDefault(-1),
                    new AfConsultasGeneralesDto());
        }

        /// <summary>
        /// Obtiene las relaciones de una persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfPersonaRelacionDto>> AF_Persona_Relacion_List(int CodEmpresa, string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la cedula.",
                    -1,
                    new List<AfPersonaRelacionDto>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfPersonaRelacionDto>(
                    "dbo.spAFI_Persona_Relacion_List",
                    new
                    {
                        IdCedPrincipal = cedula.Trim(),
                        Activo = 1
                    },
                    commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfPersonaRelacionDto>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al consultar las relaciones de la persona.",
                    result.Code.GetValueOrDefault(-1),
                    new List<AfPersonaRelacionDto>());
        }

        /// <summary>
        /// Registra Indicadores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Indicadores_Registra(int CodEmpresa, string req)
        {
            var request = JsonConvert.DeserializeObject<AfPersonaIndicadoresDto>(req) ?? new AfPersonaIndicadoresDto();
            return ExecuteStoredProcedure(
                CodEmpresa,
                "dbo.spAFI_Persona_Indicadores",
                new
                {
                    Cedula = request.cedula,
                    Indicador = request.indicador,
                    Valor = request.valor,
                    Usuario = request.usuario,
                    Nota = request.nota ?? string.Empty
                });
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
            const string query = @"UPDATE socios SET Prideduc = @prideduc WHERE cedula = @cedula";
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                query,
                new { prideduc, cedula });

            return result.Code == 0
                ? DbHelper.OkResponse(OPERACION_REALIZADA_CORRECTAMENTE)
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar primera deducción.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Elimina(int CodEmpresa, string cedula)
        {
            const string query = @"delete from socios WHERE cedula = @cedula";
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                query,
                new { cedula });

            return result.Code == 0
                ? DbHelper.OkResponse(OPERACION_REALIZADA_CORRECTAMENTE)
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar persona.", result.Code.GetValueOrDefault(-1));
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
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var tablas = new Dictionary<int, (string Tabla, string Campo, string[] Filtros)>
                {
                    { 1, ("PROMOTORES", "id_promotor", Array.Empty<string>()) },
                    { 2, ("INSTITUCIONES", "cod_institucion", Array.Empty<string>()) },
                    { 3, ("vAFI_Deductoras", "cod_deductora", new[] { CAMPO_COD_INSTITUCION }) },
                    { 4, ("AFDepartamentos", "cod_departamento", new[] { CAMPO_COD_INSTITUCION }) },
                    { 5, ("AFSecciones", "cod_seccion", new[] { CAMPO_COD_INSTITUCION, "cod_departamento" }) },
                    { 6, ("AFI_Profesiones", "cod_profesion", Array.Empty<string>()) },
                    { 7, ("AFI_Sectores", "cod_sector", Array.Empty<string>()) },
                    { 8, ("AFI_SOCIEDADES_TIPOS", "cod_sociedad", Array.Empty<string>()) },
                    { 9, ("Unidad", "cod_unidad", Array.Empty<string>()) }
                };

                if (!tablas.ContainsKey(tipoScroll))
                {
                    throw new InvalidOperationException("Tipo de scroll no soportado");
                }

                var (tabla, campo, filtros) = tablas[tipoScroll];
                var valorId = string.IsNullOrEmpty(id) ? "0" : id;
                var order = scrollCode == 0 ? "DESC" : "ASC";
                var operador = scrollCode == 0 ? "<" : ">";

                var query = $@"
                        SELECT TOP 1 {campo} 
                        FROM {tabla} 
                        WHERE {campo} {operador} @id";

                if (filtros.Contains(CAMPO_COD_INSTITUCION))
                {
                    query += " AND cod_institucion = @cod_Institucion";
                }

                if (filtros.Contains("cod_departamento") && !string.IsNullOrEmpty(cod_Departamento))
                {
                    query += " AND cod_departamento = @cod_Departamento";
                }

                query += $" ORDER BY {campo} {order}";

                return connection.Query<string>(query, new { id = valorId, cod_Institucion, cod_Departamento }).FirstOrDefault() ?? "0";
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? "0")
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al ejecutar scroll general.", result.Code.GetValueOrDefault(-1), "0");
        }

        /// <summary>
        /// Valida Persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Validar(int CodEmpresa, string req)
        {
            var dto = JsonConvert.DeserializeObject<AfPersonaAddRequestDto>(req) ?? new AfPersonaAddRequestDto();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => ValidarPersona(connection, dto));

            return result.Code == 0
                ? result.Result ?? DbHelper.OkResponse("Validación correcta")
                : DbHelper.ErrorResponse(result.Description ?? "Error al validar persona.", result.Code.GetValueOrDefault(-1));
        }
        /// <summary>
        /// Obtiene Persona patron nacional
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfPadronPersonaDto> AF_PersonaPadron_Obtener(int codEmpresa, string cedula)
        {
            string stringConn = _config.GetConnectionString("BaseConnString")
                ?? throw new InvalidOperationException("Connection string 'BaseConnString' not found.");
            var response = new ErrorDto<AfPadronPersonaDto>
            {
                Code = 0,
                Description = CONSULTA_REALIZADA_CORRECTAMENTE,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.QueryFirstOrDefault<AfPadronPersonaDto>(
                    "spSYS_Consulta_Padron",
                    new { Identificacion = cedula, Pais = "CRI" },
                    commandType: CommandType.StoredProcedure);
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
            var request = JsonConvert.DeserializeObject<AfPersonaDimexAddDto>(req) ?? new AfPersonaDimexAddDto();
            return ExecuteStoredProcedure(
                codEmpresa,
                "dbo.spAFI_Persona_Dimex_Add",
                new
                {
                    Cedula = request.cedula,
                    Dimex = request.dimex,
                    Activo = request.activo,
                    Usuario = request.usuario
                });
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
            return ExecuteStoredProcedure(
                codEmpresa,
                "dbo.spAFI_Persona_Direccion_Elimina",
                new
                {
                    Cedula = cedula,
                    Linea = linea,
                    Usuario = usuario
                });
        }

        /// <summary>
        /// Registra motivos de afiliacion de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Motivos_Registra(int CodEmpresa, string request)
        {
            var req = JsonConvert.DeserializeObject<AfMotivosDto>(request) ?? new AfMotivosDto();
            var codMotivo = FormatearCodigoJerarquico(req.cod_motivo);

            return ExecuteStoredProcedure(
                CodEmpresa,
                "dbo.spAFI_Persona_Motivos_Registra",
                new
                {
                    Cedula = req.cedula,
                    Motivo = codMotivo,
                    TipoMov = req.asignado ? "A" : "E",
                    Usuario = req.registro_usuario
                });
        }

        /// <summary>
        /// Obtiene la fecha del servidor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery<DateTime>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT dbo.MyGetdate() AS Fecha",
                default);

            return result.Code == 0
                ? DbHelper.OkResponse(result.Result.ToString("yyyy-MM-dd HH:mm:ss"))
                : DbHelper.ErrorResponse(result.Description ?? "Error al obtener fecha del servidor.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region Helpers privados

        private PortalDB CreatePortalDb() => new(_config);

        private ErrorDto ExecuteStoredProcedure(int codEmpresa, string storedProcedure, object parameters, string successMessage = OPERACION_REALIZADA_CORRECTAMENTE)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse(successMessage)
                : DbHelper.ErrorResponse(result.Description ?? $"Error al ejecutar {storedProcedure}.", result.Code.GetValueOrDefault(-1));
        }

        private static string FormatearCodigoJerarquico(string? codigo)
        {
            var valor = codigo ?? string.Empty;
            if (valor.Contains("."))
            {
                var partes = valor.Split('.');
                var entero = partes[0].PadLeft(2, '0');
                return partes.Length > 1 ? $"{entero}.{partes[1]}" : entero;
            }

            return valor.PadLeft(2, '0');
        }
        #endregion
    }
}

