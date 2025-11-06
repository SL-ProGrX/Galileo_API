using Dapper; 
using PgxAPI.Models;
using PgxAPI.Models.ERROR; 
using PgxAPI.Models.ProGrX_Nucleo; 
using System.Data;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_RA_PersonasDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;

        public frmSYS_RA_PersonasDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Busca los expedientes restringidos de personas según los filtros proporcionados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SysRaExpedientesData>> SYS_RA_Personas_Buscar(int CodEmpresa, SysExpedienteFiltroData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysRaExpedientesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysRaExpedientesData>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    if (!filtros.vence)
                    {
                        DateTimeOffset fecha_inicio = DateTimeOffset.Parse(filtros.inicioVenc);
                        string? fechaIni = fecha_inicio.ToString("yyyy-MM-dd");
                        DateTimeOffset fecha_fin = DateTimeOffset.Parse(filtros.finVenc);
                        string? fechaCorte = fecha_fin.ToString("yyyy-MM-dd");



                        var query = $@"select *, isnull(Fecha_Vence, '2300/01/01') as 'Vence_Fix' from vSYS_RA_Casos where cedula like @ced and nombre like @nombre and Estado = @estado
                            and isnull(Fecha_Vence, '2300/01/01')  between '{fechaIni} 00:00:00' and '{fechaCorte} 23:59:59'";

                        result.Result = connection.Query<SysRaExpedientesData>(query,
                            new
                            {
                                ced = $"%{filtros.cedula?.Trim()}%",
                                nombre = $"%{filtros.nombre?.Trim()}%",
                                estado = filtros.estado,

                            }).ToList();


                    }
                    else
                    {
                        var query = $@"select *, isnull(Fecha_Vence, '2300/01/01') as 'Vence_Fix' from vSYS_RA_Casos where cedula like @ced and nombre like @nombre and Estado = @estado";

                        result.Result = connection.Query<SysRaExpedientesData>(query,
                            new
                            {
                                ced = $"%{filtros.cedula?.Trim()}%",
                                nombre = $"%{filtros.nombre?.Trim()}%",
                                estado = filtros.estado,

                            }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Guarda o actualiza los datos en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="personaId"></param>
        /// <param name="datos"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto SYS_RA_Personas_Guardar(int CodEmpresa, int personaId, SysRaExpedientesData datos, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
              
                using var connection = new SqlConnection(stringConn);
                {

                    int PERSONA_ID = connection.QuerySingle<int>(
                                "spSYS_RA_Persona_Add",
                                new
                                {
                                    PersonaId = datos.persona_id,
                                    Cedula = datos.cedula.Trim(),
                                    Estado = datos.estado,
                                    TipoId = datos.tipo_id,
                                    Vence = datos.vence ? datos.vencimiento.Value.Date : (DateTime?)null,
                                    Notas = datos.notas,
                                    Usuario = usuario
                                },
                            commandType: CommandType.StoredProcedure
                            );
                    result.Code = PERSONA_ID;

                    if (datos.persona_id == 0)
                    {
                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Expediente Restringido: {datos.persona_id} Cedula = {datos.cedula}",
                            Movimiento = "Registra - WEB",
                            Modulo = vModulo
                        });
                    }
                    else
                    {
                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Expediente Restringido: {datos.persona_id} Cedula = {datos.cedula}",
                            Movimiento = "Modifica - WEB",
                            Modulo = vModulo
                        });
                    }


                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de usuarios según el código de empresa 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SYS_Usuarios_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select CEDULA as 'item',NOMBRE as 'descripcion' from SOCIOS ";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de tipos  
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SYS_RaTipos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    query = $@"select rtrim(TIPO_ID) as 'item',rtrim(descripcion) as 'descripcion' from SYS_EXP_TIPOS  where Activo = 1 order by TIPO_ID";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;

        }
        
        /// <summary>
        /// Obtiene los casos restringidos por cédula.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<SysAutorizacionesData>> SYS_RA_CasosPorCedula_Obtener(int CodEmpresa, string filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
             filtro = (filtro== "undefined" ? "" : filtro);
            var result = new ErrorDto<List<SysAutorizacionesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysAutorizacionesData>()
            };
            try
            {
            
                using var connection = new SqlConnection(stringConn);
                {                  
                    var query = $@"SELECT Persona_Id, Cedula, NOMBRE, Estado  FROM vSYS_RA_Casos where cedula like @ced ";
                    result.Result = connection.Query<SysAutorizacionesData>(query,
                        new
                        {
                            ced = $"%{filtro.Trim()}%",
                       

                        }).ToList();

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
    }
}
