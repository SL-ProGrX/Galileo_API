using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Fondos;

namespace PgxAPI.DataBaseTier.ProGrX.Fondos
{
    public class frmFNDOperadorasDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 18; // Modulo de Fondo de Inversion
        private readonly mSecurityMainDb _Security_MainDB;
        private readonly mFNDFuncionesDB _mFNDFunciones;

        public frmFNDOperadorasDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
            _mFNDFunciones = new mFNDFuncionesDB(_config);
        }

        /// <summary>
        /// Obtiene la operadora
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_operadora"></param>
        /// <returns></returns>
        public ErrorDto<FndOperadoraDTO> AF_Operadora_Obtener(int CodEmpresa, int cod_operadora)
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<FndOperadoraDTO>
            {
                Code = 0,
                Result = new FndOperadoraDTO()
            };

            try
            {
                using var connection = new SqlConnection(connString);

                string sql = @"
                        SELECT *
                        FROM vFnd_Operadoras
                        WHERE cod_Operadora = @CodOperadora";

                response.Result = connection.QueryFirstOrDefault<FndOperadoraDTO>(sql, new { CodOperadora = cod_operadora });
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
        ///  Obtiene las operadoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Operadoras_Obtener(int CodEmpresa)
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
                    var query = @"SELECT cod_operadora AS item, descripcion AS descripcion 
                                FROM vFnd_Operadoras
								order by item desc";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Guardar o actualizar la operadora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Operadora_Guardar(int codEmpresa, FndOperadoraDTO request)
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto { Code = 0, Description = "OK" };

            try
            {
                using var connection = new SqlConnection(connString);

                if (request.cod_operadora == null)
                {
                    string insertSql = @"
                INSERT INTO Fnd_Operadoras 
                        (Descripcion, Activa, Notas, 
                         Cta_Fondo, Cta_Retiros, Cta_Ingresos, MULTA_MNT_TOPE)
                    VALUES 
                        (@Descripcion, @Activa, @Notas,
                         @ctaplan, @ctaret, @ctaing, @multa_mnt_tope);

                    SELECT CAST(SCOPE_IDENTITY() as int); ";

                    int nuevoId = connection.ExecuteScalar<int>(insertSql, request);
                    request.cod_operadora = nuevoId;

                    response.Description = "Operadora registrada correctamente.";
                    response.Code = request.cod_operadora;
                }
                else
                {
                    string updateSql = @"
                UPDATE FND_Operadoras
                SET 
                    Descripcion     = @Descripcion,
                    Cta_Fondo       = @ctaplan,
                    Cta_Retiros     = @ctaret,
                    Cta_Ingresos    = @ctaing,
                    Notas           = @Notas,
                    Activa          = @Activa,
                    MULTA_MNT_TOPE  = @multa_mnt_tope
                WHERE cod_operadora = @cod_operadora;";

                    int rows = connection.Execute(updateSql, request);


                    response.Description = "Operadora actualizada correctamente.";
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
        /// Obtiene planes por operadora
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_operadora"></param>
        /// <returns></returns>
        public ErrorDto<List<OperadoraPlanDTO>> FND_OperadoraPlanes_Obtener(int CodEmpresa, int cod_operadora)
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<OperadoraPlanDTO>>
            {
                Code = 0,
                Result = new List<OperadoraPlanDTO>()
            };

            try
            {
                using var connection = new SqlConnection(connString);

                string sql = @"
                    SELECT 
                        Cod_Plan,
                        Plan_Desc,
                        Cod_Divisa,
                        Contratos,
                        Total * dbo.fxSys_Tipo_Cambio_Apl(Tipo_Cambio) AS TotalLocal,
                        Total AS TotalDivisa
                    FROM vFnd_Operadoras_Rsm
                    WHERE Cod_Operadora = @CodOperadora
                    ORDER BY Cod_Plan;";

                response.Result = connection.Query<OperadoraPlanDTO>(sql, new { CodOperadora = cod_operadora }).ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Elimina la operadora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_operadora"></param>
        /// <returns></returns>
        public ErrorDto AF_Operadora_Eliminar(int codEmpresa, int cod_operadora)
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto { Code = 0, Description = "OK" };

            try
            {
                using var connection = new SqlConnection(connString);

                string sql = @"DELETE FROM FND_Operadoras WHERE cod_operadora = @cod_operadora";

                connection.Execute(sql, new { cod_operadora = cod_operadora });

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Scroll para la busqueda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="scrollCode"></param>
        /// <returns></returns>
        public ErrorDto<FndOperadoraDTO> AF_Operadora_Scroll_Obtener(int CodEmpresa, int operadora, int scrollCode)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FndOperadoraDTO>
            {
                Code = 0,
                Result = new FndOperadoraDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Top 1 cod_operadora from FND_OPERADORAS";

                    if (scrollCode == 1)
                    {
                        query += " where cod_operadora > @operadora order by cod_operadora asc";
                    }
                    else
                    {
                        query += " where cod_operadora < @operadora order by cod_operadora desc";
                    }
                    var cod_operadora = connection.Query<int>(query, new { operadora = operadora }).FirstOrDefault();
                    response = AF_Operadora_Obtener(CodEmpresa, cod_operadora);
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
    }
}
