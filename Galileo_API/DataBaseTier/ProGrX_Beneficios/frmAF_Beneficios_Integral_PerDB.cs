using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_Integral_PerDB
    {
        private readonly IConfiguration _config;
        private mProGrX_AuxiliarDB mAuxiliarDB;

        public frmAF_Beneficios_Integral_PerDB(IConfiguration config)
        {
            _config = config;
            mAuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Obtengo lista de Estado Civil para Tab Datos Persona de Beneficio Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> EstadoCivilLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralDropsLista>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select Estado_Civil as 'item', Descripcion as 'descripcion' from SYS_ESTADO_CIVIL " +
                        " where Activo = 1  order by Descripcion asc ";

                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "EstadoCivilLista_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo lista de Nivel Academico para Tab Datos Persona de Beneficio Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> NivelAcademicoLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralDropsLista>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = " select Catalogo_Id as 'item', Descripcion as 'descripcion' " +
                        " from AFI_CATALOGOS Where Tipo_Id = 3 order by Descripcion ";

                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "NivelAcademicoLista_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo lista de Nacionalidad para Tab Datos Persona de Beneficio Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> NacionalidadLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralDropsLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select cod_nacionalidad as 'item', Descripcion as 'descripcion' from Sys_nacionalidades " +
                        " where Activo = 1  order by Omision desc, Descripcion asc ";

                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "NacionalidadLista_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo lista de Paises para Tab Datos Persona de Beneficio Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> PaisLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralDropsLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select cod_Pais as 'item', Descripcion as 'descripcion' from Paises " +
                        " where Activo = 1   order by Omision desc, Descripcion asc";
                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "PaisLista_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo lista de Provincias para Tab Datos Persona de Beneficio Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> ProvinciaLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralDropsLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select Provincia as item, rtrim(Descripcion) as descripcion from Provincias ";
                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ProvinciaLista_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo los datos de la persona para el formulario de Beneficio Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfiBeneficioIntegralPersonaData> DatosPersona_Obtener(int CodCliente, string? cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<AfiBeneficioIntegralPersonaData>();
            response.Code = 0;

            if (cedula == null)
            {
                return null;
            }

            if (cedula.Length < 2)
            {
                return null;
            }

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    


                    var query = $@"SELECT
                                        -- Primer apellido
                                        SUBSTRING(s.NOMBRE, 1, CHARINDEX(' ', s.NOMBRE) - 1) AS Apellido1,
                                        -- Segundo apellido
                                        SUBSTRING(s.NOMBRE,
                                                  CHARINDEX(' ', s.NOMBRE) + 1,
                                                  CHARINDEX(' ', s.NOMBRE, CHARINDEX(' ', s.NOMBRE) + 1) - CHARINDEX(' ', s.NOMBRE) - 1) AS Apellido2,
                                        -- Nombre
                                        SUBSTRING(s.NOMBRE,
                                                  CHARINDEX(' ', s.NOMBRE, CHARINDEX(' ', s.NOMBRE) + 1) + 1,
                                                  LEN(s.NOMBRE)) AS Nombrev2,
                                        S.NOMBRE,
                                        s.ESTADOCIVIL,
                                        s.SEXO,
                                        s.FECHA_NAC,
                                        s.FECHAINGRESO,
                                        s.ct AS LUGAR_TRABAJO,
                                        s.NIVEL_ACADEMICO,
                                        s.PROFESION, 
                                        s.COD_NACIONALIDAD,
                                        s.COD_PAIS_NAC,
                                        s.AF_EMAIL,
                                        s.EMAIL_02,
                                        s.APTO,
                                        s.PROVINCIA,
                                        s.CANTON,
                                        s.DISTRITO,
                                        s.DIRECCION,
                                        s.ESTADOACTUAL,
                                        s.PROFESION,
	                                    m.MEMBRESIA,
                                        s.estadolaboral
                                    FROM 
                                        SOCIOS s
                                    LEFT JOIN 
                                                [dbo].[vAFI_Membresias] m ON s.CEDULA = m.CEDULA  
                                        WHERE s.CEDULA = '{cedula}'";


                    response.Result = connection.Query<AfiBeneficioIntegralPersonaData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "DatosPersona_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;

        }

        /// <summary>
        /// Obtengo las cuentas bancarias asociadas al socio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>

        public ErrorDto<List<CuentaListaData>> Cuentas_Obtener(int CodCliente, string Usuario)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<CuentaListaData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = "[spCrd_SGT_Bancos]";
                    var values = new
                    {
                        usuario = Usuario,
                    };

                    response.Result = connection.Query<CuentaListaData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Cuentas_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }
        /// <summary>
        /// Valida si el socio existe o esta activo
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto validaSocioExiste(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT CEDULA, NOMBRE FROM SOCIOS WHERE CEDULA = '{cedula}'";
                    var resp = connection.Query(query).ToList();

                    if (resp.Count == 0)
                    {
                        info.Code = -1;
                        info.Description = "No se encontro Socio";
                    }
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = "validaSocioExiste" + ex.Message; ;
            }

            return info;

        }

        /// <summary>
        /// Guardo los datos de la persona para el formulario de Beneficio Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public ErrorDto Persona_Actualizar(int CodCliente, string cedula, string persona)
        {
            BeneficioPersona personainsertar = JsonConvert.DeserializeObject<BeneficioPersona>(persona) ?? new BeneficioPersona();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string FechaNacimiento = mAuxiliarDB.validaFechaGlobal(personainsertar.FechaNacimiento);
                    string FechaIngreso = mAuxiliarDB.validaFechaGlobal(personainsertar.FechaIngreso);

                    var query = $@"UPDATE SOCIOS SET 
                                    ESTADOCIVIL = '{personainsertar.EstadoCivil}'
                                    ,AF_EMAIL = '{personainsertar.Email1}'
                                    ,EMAIL_02 = '{personainsertar.Email2}'
                                    ,APTO = '{personainsertar.AptoPostal}'
                                    ,DIRECCION = '{personainsertar.Direccion}'
                                    ,COD_NACIONALIDAD = '{personainsertar.Nacionalidad}'
                                    ,PROVINCIA = {personainsertar.Provincia}
                                    ,CANTON = '{personainsertar.Canton}'
                                    ,DISTRITO = '{personainsertar.Distrito}'
                                    ,NIVEL_ACADEMICO = '{personainsertar.NivelAcademico}'
                                    ,COD_PAIS_NAC = '{personainsertar.PaisNacimiento.Trim()}'
                                    ,PROFESION = '{personainsertar.Ocupacion}'
                                    ,FECHA_NAC = '{FechaNacimiento}' 
                                    ,FECHAINGRESO = '{FechaIngreso}'
                                    ,ESTADOLABORAL = '{personainsertar.estadolaboral}'
                                     WHERE cedula = '{cedula}'";

                    info.Code = connection.Execute(query);



                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        /// <summary>
        /// Guardo los datos de contactos telefonicos de la persona para el formulario de Beneficio Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="telefono"></param>
        /// <returns></returns>
        public ErrorDto Telefono_Guardar(int CodCliente, AFIBeneTelefonoGuardar telefono)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //valido si el miembro ya existe

                    if (telefono.id_telefono != 0)
                    {
                        if (!Telefono_Actualizar(CodCliente, telefono))
                        {

                            response.Description = "Error al actualizar el Asociado";
                        }
                        else
                        {
                            response.Description = telefono.id_telefono.ToString();
                            response.Code = 0;
                        }
                    }
                    else
                    {
                        response = Telefono_Agregar(CodCliente, telefono);
                    }
                }



            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Telefono_Guardar - " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Agrega un nuevo telefono a la persona
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="telefono"></param>
        /// <returns></returns>
        private ErrorDto Telefono_Agregar(int CodCliente, AFIBeneTelefonoGuardar telefono)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = @$"INSERT INTO [dbo].[AFI_BENE_REGISTRO_TELEFONOS]
                                           ([COD_BENEFICIO]
                                           ,[CONSEC]
                                           ,[TIPO]
                                           ,[CONTACTO]
                                           ,[TELEFONO]
                                           ,[EXT]
                                           ,[CEDULA]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ('{telefono.cod_beneficio}'
                                           ,{telefono.consec}
                                           ,{telefono.tipo.item}
                                           ,'{telefono.contacto}'
                                           ,'{telefono.telefono}'
                                           ,'{telefono.ext}'
                                           ,'{telefono.cedula}'
                                            ,getDate() 
                                            ,'{telefono.user_registra}')";

                    response.Code = connection.Execute(query);

                    query = "SELECT IDENT_CURRENT('AFI_BENE_REGISTRO_TELEFONOS') as 'id'";
                    telefono.id_telefono = connection.Query<int>(query).FirstOrDefault();

                    response.Description = telefono.id_telefono.ToString();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Telefono_Agregar - " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Actualiza un telefono de la persona
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="telefono"></param>
        /// <returns></returns>
        private bool Telefono_Actualizar(int CodCliente, AFIBeneTelefonoGuardar telefono)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<AFIBeneTelefonoGuardar>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = @$"UPDATE [dbo].[AFI_BENE_REGISTRO_TELEFONOS]
                               SET

                                           [TIPO] = '{telefono.tipo.item}'
                                           ,[TELEFONO] = '{telefono.telefono}'
                                           ,[CONTACTO] = '{telefono.contacto}'
                                           ,[EXT] = '{telefono.ext}'


                             WHERE cedula = {telefono.cedula} and id_telefono = {telefono.id_telefono}  ";

                    connection.Execute(query);

                }
            }

            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Telefono_Actualizar - " + ex.Message;
                response.Result = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Obtengo la lista de telefonos del socio para el formulario de Beneficio Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AFIBeneTelefono>> Telefonos_Obtener(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AFIBeneTelefono>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Actualiza Telefonos del SIF
                    var update = $@"INSERT INTO AFI_BENE_REGISTRO_TELEFONOS (
                                            COD_BENEFICIO, 
                                            CONSEC, 
                                            TIPO, 
                                            TELEFONO, 
                                            EXT, 
                                            CONTACTO, 
                                            REGISTRO_FECHA, 
                                            REGISTRO_USUARIO, 
                                            CEDULA
                                        )
                                        SELECT   
                                            '1' AS COD_BENEFICIO, 
                                            TELEFONO AS CONSEC, 
                                            TIPO, 
                                            NUMERO AS TELEFONO, 
                                            EXT, 
                                            CONTACTO, 
                                            FECHA AS REGISTRO_FECHA, 
                                            USUARIO AS REGISTRO_USUARIO, 
                                            CEDULA
                                        FROM TELEFONOS T
                                        WHERE
										NUMERO IS NOT NULL AND 
										NUMERO NOT IN (
                                            SELECT TELEFONO 
                                            FROM AFI_BENE_REGISTRO_TELEFONOS A
											WHERE T.NUMERO = A.TELEFONO
                                        )";
                    connection.Execute(update);

                    var query = $@"select * from AFI_BENE_REGISTRO_TELEFONOS WHERE CEDULA = '{cedula}'";

                    response.Result = connection.Query<AFIBeneTelefono>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "DatosPersona_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Elimina un telefono de la persona
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Telefono_Eliminar(int CodCliente, int id, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"DELETE AFI_BENE_REGISTRO_TELEFONOS WHERE id_telefono = {id} ";

                    info.Code = connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = "Telefono_Eliminar" + ex;
            }
            return info;
        }

        /// <summary>
        /// Valida estados de la persona, si esta activo o inactivo, ai tiene sanciones y otros temas.
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto ValidarPersona(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using (var connection = new SqlConnection(clienteConnString))
                {
                    var query = "SELECT * FROM AFI_BENE_VALIDACIONES WHERE ESTADO = 1 AND TIPO = 'P'" +
                        "AND COD_VAL IN (" +
                        "SELECT COD_VAL FROM AFI_BENE_VALIDA_CATEGORIA WHERE ESTADO = 1" +
                        ") ORDER BY PRIORIDAD ASC";
                    var validaciones = connection.Query<afiBeneCalidaciones>(query).ToList();

                    foreach (var validacion in validaciones)
                    {
                        query = validacion.query_val.Replace("@cedula", cedula);
                        var result = connection.Query<int>(query).FirstOrDefault();

                        if (result == validacion.resultado_val)
                        {
                            info.Code = 0;
                            info.Description += validacion.msj_val + "...\n";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }


        /// <summary>
        /// Obtengo lista de Estado Laboral para Tab Datos Persona de Beneficio Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> EstadoLaboral_Obtener (int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralDropsLista>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select ESTADO_LABORAL as 'item', Descripcion as 'descripcion' from AFI_ESTADO_LABORAL " +
                        " where Activo = 1  order by Descripcion asc ";

                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "EstadoLaboral_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }


    }
}