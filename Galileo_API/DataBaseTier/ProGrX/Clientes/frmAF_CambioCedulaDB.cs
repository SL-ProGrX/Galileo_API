using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_CambioCedulaDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;

        public frmAF_CambioCedulaDB(IConfiguration config)
        {
            _config = config;
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener tipos de cedulas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposCedulas_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select TIPO_ID as IDX, rtrim(Descripcion) as ItmX from AFI_TIPOS_IDS order by Tipo_Id";
                    response.Result = connection.Query(query)
                        .Select(row => new DropDownListaGenericaModel
                        {
                            item = row.IDX,
                            descripcion = row.ItmX
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        /// Actualiza el cambio de Cedula 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cambioData"></param>
        /// <returns></returns>
        public ErrorDto AF_CambioCedula_Aplicar(int CodEmpresa, string usuario, string cambioData)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AFCambioCedulaDto cambioCedula = JsonConvert.DeserializeObject<AFCambioCedulaDto>(cambioData);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Guardado correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Actualiza el Parametro de Validacion y Luego lo Aplica
                    var query = "select LARGO_MINIMO from AFI_TIPOS_IDS Where TIPO_ID = @tipoId";
                    int largo_Minimo = connection.QueryFirstOrDefault<int>(query, new
                    {
                        tipoId = cambioCedula.tipo
                    });
                    if (cambioCedula.cedulaNueva == null || cambioCedula.cedulaNueva.Length != largo_Minimo)
                    {
                        response.Code = -2;
                        response.Description = "El n&uacute;mero de identificaci&oacute;n nuevo no cumplen con los caracteres requeridos " + largo_Minimo + ", verifique!";
                        return response;
                    }

                    var sql = @" EXEC spAFI_Identificacion_Cambio @CedulaNueva, @CedulaAnterior, @Usuario, @TipoId";
                    connection.Execute(sql, new
                    {
                        CedulaNueva = cambioCedula.cedulaNueva,
                        CedulaAnterior = cambioCedula.cedulaAnterior,
                        Usuario = usuario,
                        TipoId = cambioCedula.tipo
                    });

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario.ToUpper(),
                        DetalleMovimiento = "Cambio de Cedula : " + cambioCedula.cedulaAnterior + " a " + cambioCedula.cedulaNueva + " : " + cambioCedula.nombre,
                        Movimiento = "APLICA - WEB",
                        Modulo = 9
                    });
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
        /// Obtener cedulas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<AFCedulaCambioDto> AF_Cedula_Obtener(int CodEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AFCedulaCambioDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new AFCedulaCambioDto()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT 
                                    S.Cedula AS cedulaActual, 
                                    S.NOMBRE AS nombre, 
                                    S.TIPO_ID AS tipoid, 
                                    Tip.DESCRIPCION AS TipoId_Desc,
                                    CASE 
                                        WHEN Ep.COD_ESTADO = 'N' THEN 'No Asociado'
                                        ELSE 'Asociado'
                                    END AS estado,
                                    Ep.DESCRIPCION AS Estado_Persona
                                FROM 
                                    socios S
                                INNER JOIN 
                                    AFI_TIPOS_IDS Tip ON S.TIPO_ID = Tip.TIPO_ID
                                INNER JOIN 
                                    AFI_ESTADOS_PERSONA Ep ON S.ESTADOACTUAL = Ep.COD_ESTADO
                                WHERE 
                                    TRIM(S.cedula) = '{cedula.Trim()}'";
                    response.Result = connection.QueryFirstOrDefault<AFCedulaCambioDto>(query);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Validar usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto AF_Usuario_Validar(int CodEmpresa, string parametros)
        {
            AFUsuarioLogonDto param = JsonConvert.DeserializeObject<AFUsuarioLogonDto>(parametros);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            var paramAccess = new ParametrosAccesoDto();
            paramAccess.EmpresaId = CodEmpresa;
            paramAccess.Usuario = param.usuario;
            paramAccess.Modulo = 1;
            paramAccess.FormName = "frmAF_CambioCedula";
            paramAccess.Boton = "cmdAplicar";
            try
            {
                //Verifica que el usuario Autorizador tambien tenga acceso al cambio de Identificaci�n
                if (_mSecurity.Derecho(paramAccess) != 1)
                {
                    response.Code = -2;
                    response.Description = "El Usuario: " + param.usuario + ", no es tiene permisos de cambio de Identificaci&oacute;n de Personas!";
                    return response;
                }
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                {
                    //Verifica Usuario / Cifrado Actual
                    var sql = @"exec spSEG_Logon @usuario, @clave";
                    int Existe = connection.QueryFirstOrDefault<int>(sql, new
                    {
                        usuario = param.usuario,
                        clave = param.clave
                    });

                    if (Existe == 0)
                    {
                        response.Code = -2;
                        response.Description = "Clave de Usuario incorrecta, intente de nuevo";
                    }
                }

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