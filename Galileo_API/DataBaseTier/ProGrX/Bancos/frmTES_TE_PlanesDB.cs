using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using PgxAPI.Models.TES;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_TE_PlanesDB
    {
        private readonly IConfiguration? _config;
        MSecurityMainDb DBBitacora;

        public frmTES_TE_PlanesDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Hacer scroll entre los planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="codPlan"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<TesBancoPlanesData> TES_Planes_Scroll(int CodEmpresa, int scrollCode, string codPlan, int banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesBancoPlanesData>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Top 1 * from TES_BANCO_PLANES_TE";
                    if (scrollCode == 1)
                    {
                        query += " WHERE id_banco = @banco AND cod_Plan > @codPlan ORDER BY cod_Plan ASC";
                    }
                    else
                    {
                        query += " WHERE id_banco = @banco AND cod_Plan < @codPlan ORDER BY cod_Plan DESC";
                    }

                    response.Result = connection.Query<TesBancoPlanesData>(query,
                        new { banco = banco, codPlan = codPlan }).FirstOrDefault();
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
        /// Obtener información de planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <param name="codPlan"></param>
        /// <returns></returns>
		public ErrorDto<TesBancoPlanesData> TES_PlanesConsulta_Obtener(int CodEmpresa, int banco, string codPlan)
		{
			string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
			var response = new ErrorDto<TesBancoPlanesData>
			{
				Code = 0,
				Result = new TesBancoPlanesData()
			};
			try
			{
				using var connection = new SqlConnection(stringConn);
				{
					var query = @"exec spTes_Planes_Consulta @banco, @codPlan";
					response.Result = connection.QueryFirstOrDefault<TesBancoPlanesData>(query,
						new { banco = banco, codPlan = codPlan });
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
        /// Obtener información de grupos bancarios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
		public ErrorDto<TesBancosGruposData> TES_Planes_BancosGrupos_Obtener(int CodEmpresa, int banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesBancosGruposData>
            {
                Code = 0,
                Result = new TesBancosGruposData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select B.ID_BANCO, B.COD_GRUPO, B.DESCRIPCION , B.DESC_CORTA
                        , Bg.DESCRIPCION as 'Banco_Desc', Bg.DESC_CORTA as 'Banco_Desc_Corta'
                        from TES_BANCOS B inner join TES_BANCOS_GRUPOS Bg on B.COD_GRUPO = Bg.COD_GRUPO 
                        Where B.ID_Banco = @banco";
                    response.Result = connection.QueryFirstOrDefault<TesBancosGruposData>(query,
                        new { banco = banco });
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
        /// Agregar o actualizar un plan
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="infoPlan"></param>
        /// <returns></returns>
        public ErrorDto TES_Planes_Guardar(int CodEmpresa, string infoPlan)
        {
            TesBancoPlanesData request = JsonConvert.DeserializeObject<TesBancoPlanesData>(infoPlan) ?? new TesBancoPlanesData();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"exec spTes_Planes_Registro @banco, @codPlan, @consecId, 
                            @consecInt, @usuario, 'A'";
                    response.Code = connection.Execute(query,
                        new { 
                            banco = request.id_banco, 
                            codPlan = request.cod_plan,  
                            consecId = request.numero_te,
                            consecInt = request.numero_interno,
                            usuario = request.registro_usuario.ToUpper()
                        });

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = request.registro_usuario.ToUpper(),
                        DetalleMovimiento = "Cta Id: "+ request.id_banco + ", Plan: "+ request.cod_plan +
                        ", Consec Id: "+ request.numero_te + ", Consec Interno: "+ request.numero_interno,
                        Movimiento = "REGISTRA - WEB",
                        Modulo = 9
                    });

                    response.Description = "Plan Registrado Satisfactoriamente!";
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
        /// Borrar un plan
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="infoPlan"></param>
        /// <returns></returns>
        public ErrorDto TES_Planes_Borrar(int CodEmpresa, string infoPlan)
        {
            TesBancoPlanesData request = JsonConvert.DeserializeObject<TesBancoPlanesData>(infoPlan) ?? new TesBancoPlanesData();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"exec spTes_Planes_Registro @banco, @codPlan, @consecId, 
                            @consecInt, @usuario, 'E'";
                    response.Code = connection.Execute(query,
                        new
                        {
                            banco = request.id_banco,
                            codPlan = request.cod_plan,
                            consecId = request.numero_te,
                            consecInt = request.numero_interno,
                            usuario = request.registro_usuario.ToUpper()
                        });

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = request.registro_usuario.ToUpper(),
                        DetalleMovimiento = "Cta Id: " + request.id_banco + ", Plan: " + request.cod_plan +
                        ", Consec Id: " + request.numero_te + ", Consec Interno: " + request.numero_interno,
                        Movimiento = "ELIMINA - WEB",
                        Modulo = 9
                    });

                    response.Description = "Plan Eliminado Satisfactoriamente!";
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
