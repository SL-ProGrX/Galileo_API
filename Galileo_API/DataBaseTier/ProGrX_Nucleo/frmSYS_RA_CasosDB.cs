using Dapper;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_RA_CasosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmSYS_RA_CasosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);

        }

        /// <summary>
        /// Metodo para buscar listado de casos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SysRaCasosData>> SYS_RA_Casos_Buscar(int CodEmpresa, SysCasosFiltroData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysRaCasosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysRaCasosData>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    DateTimeOffset fecha_inicio = DateTimeOffset.Parse(filtros.inicioVenc);
                    DateTimeOffset fecha_fin = DateTimeOffset.Parse(filtros.finVenc);


                    var query = $@"select Persona_Id, Cedula, Nombre, EstadoDesc, TipoDesc, Fecha_Vence, Registro_Fecha, Registro_Usuario from vSYS_RA_Casos 
                                        where cedula like @ced and nombre like @nombre ";

                    if (filtros.persona_id > 0)
                    {
                        query += " AND Persona_Id = @personaId";
                    }
                    if (filtros.estado != "T")
                    {
                        query += " AND EstadoDesc = @estado ";
                    }
                    if (filtros.tipo != "T")
                    {
                        query += " AND Tipo_Id = @tipoId ";
                    }
                    if (filtros.vence)
                    {
                        query += " AND Fecha_Vence  between  @fechaInicio and @fechaFin";
                    }
                    query += " order by Persona_Id";

                    result.Result = connection.Query<SysRaCasosData>(query,
                        new
                        {
                            ced = $"%{filtros.cedula?.Trim()}%",
                            nombre = $"%{filtros.nombre?.Trim()}%",
                            personaId = filtros.persona_id,
                            estado = filtros.estado,
                            tipoId = filtros.tipo,
                            fechaInicio = fecha_inicio.Date,
                            fechaFin = fecha_fin.Date.AddDays(1).AddTicks(-1)

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

        /// <summary>
        /// Consulta las autorizaciones por id de persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="persona_id"></param>
        /// <returns></returns>
        public ErrorDto<List<SysCasosAutorizacionesData>> SYS_RA_CasosAutorizaciones_Obtener(int CodEmpresa, int persona_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysCasosAutorizacionesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysCasosAutorizacionesData>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {


                    var query = $@"select * from vSYS_RA_Autorizaciones  
                                   where Persona_Id = @personaId order by Autorizacion_Id desc";

                    result.Result = connection.Query<SysCasosAutorizacionesData>(query,
                        new
                        {
                            personaId = persona_id
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

        /// <summary>
        /// Consulta de expedientes por autorizacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="autorizacionId"></param>
        /// <returns></returns>
        public ErrorDto<List<SysCasosAccesosData>> SYS_RA_CasosAccesos_Obtener(int CodEmpresa, int autorizacionId)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysCasosAccesosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysCasosAccesosData>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {


                    var query = $@"select * from vSYS_RA_Accesos  
                                   where Autorizacion_Id = @autoId  order by registro_fecha desc";

                    result.Result = connection.Query<SysCasosAccesosData>(query,
                        new
                        {
                            autoId = autorizacionId
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
