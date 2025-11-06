using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_RemesasIngresosDB
    {
        private readonly IConfiguration? _config;

        public frmAF_RemesasIngresosDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtener listado de remesas (últimas 150)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        /// 
        public ErrorDto<List<AdiRemesaIngDto>> AFI_Remesas_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AdiRemesaIngDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AdiRemesaIngDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                            SELECT TOP 150 
                                COD_REMESA,
                                FECHA,
                                USUARIO,
                                FECHA_INICIO,
                                FECHA_CORTE,
                                NOTAS,
                                ESTADO,
                                CASE 
                                    WHEN ESTADO = 'A' THEN 'Remesa Abierta'
                                    ELSE 'Remesa Cerrada'
                                END AS ESTADO,
                                MICROFILM_FECHA,
                                MICROFILM_USUARIO
                            FROM AFI_REMESAS_ING
                            ORDER BY FECHA DESC";

                    var result = connection.Query<AdiRemesaIngDto>(query).ToList();
                    response.Result = result;
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
        /// Elimina la remesa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codRemesa"></param>
        /// <returns></returns>
        public ErrorDto AFI_Remesa_Eliminar(int codEmpresa, string codRemesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                string sql = @"DELETE FROM AFI_REMESAS_ING WHERE COD_REMESA = @CodRemesa";

                connection.Execute(sql, new { CodRemesa = codRemesa });

                response.Description = "Remesa eliminada correctamente";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Registra la remesa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AFI_Remesa_Registrar(int codEmpresa, AdiRemesaIngRequestDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                if (request.CodRemesa == 0) // ➡ Insert
                {
                    string sqlUltimo = "SELECT ISNULL(MAX(COD_REMESA),0) + 1 FROM AFI_REMESAS_ING";
                    int nuevoCod = connection.ExecuteScalar<int>(sqlUltimo);

                    string sqlInsert = @"
                INSERT INTO AFI_REMESAS_ING (cod_remesa, usuario, fecha, estado, fecha_inicio, fecha_corte, notas)
                VALUES (@CodRemesa, @Usuario, dbo.MyGetdate(), 'A', @FechaInicio, @FechaCorte, @Notas)";

                    connection.Execute(sqlInsert, new
                    {
                        CodRemesa = nuevoCod,
                        request.Usuario,
                        request.FechaInicio,
                        request.FechaCorte,
                        request.Notas
                    });

                    response.Code = 1;
                    response.Description = $"Remesa registrada correctamente. Código: {nuevoCod}";
                }
                else // ➡ Update
                {
                    if (request.Estado != "Remesa Cerrada")
                    {
                        string sqlUpdate = @"
                    UPDATE AFI_REMESAS_ING
                    SET usuario = @Usuario,
                        fecha_inicio = @FechaInicio,
                        fecha_corte = @FechaCorte,
                        notas = @Notas
                    WHERE cod_remesa = @CodRemesa";

                        connection.Execute(sqlUpdate, new
                        {
                            request.Usuario,
                            request.FechaInicio,
                            request.FechaCorte,
                            request.Notas,
                            request.CodRemesa
                        });

                        response.Code = 1;
                        response.Description = $"Remesa actualizada correctamente. Código: {request.CodRemesa}";
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description = "No se puede modificar la remesa porque ya fue cerrada.";
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


        /// <summary>
        /// Obtiene la remesa abierta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_RemesaAbiertas_Obtener(int CodEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };
            try
            {
                var conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string query = @"
            SELECT 
                COD_REMESA AS item,
                RIGHT('0000' + CAST(COD_REMESA AS VARCHAR(4)), 4) 
                    + '...' + RTRIM(USUARIO) 
                    + '...' + CONVERT(VARCHAR(19), FECHA, 120)
                    + ' I:' + FORMAT(FECHA_INICIO, 'dd/MM/yyyy')
                    + ' C:' + FORMAT(FECHA_CORTE, 'dd/MM/yyyy') AS descripcion
            FROM AFI_REMESAS_ING
            WHERE ESTADO = 'A'
            ORDER BY FECHA DESC;";

                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene los ingresos pendients
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codRemesa"></param>
        /// <param name="oficina"></param>
        /// <returns></returns>
        public ErrorDto<List<IngresosPendientesDto>> AFI_IngresosPendientes_Obtener(int codEmpresa, string codRemesa, string oficina = "")
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<IngresosPendientesDto>>
            {
                Code = 0,
                Result = new List<IngresosPendientesDto>()
            };

            try
            {
                using var connection = new SqlConnection(connString);

                // 🔹 1. Obtener fechas de la remesa
                string sqlFechas = @"SELECT fecha_inicio, fecha_corte 
                             FROM AFI_REMESAS_ING 
                             WHERE cod_remesa = @CodRemesa";

                var fechas = connection.QueryFirstOrDefault<(DateTime FechaInicio, DateTime FechaCorte)>(sqlFechas, new { CodRemesa = codRemesa });

                if (fechas == default)
                {
                    response.Code = -1;
                    response.Description = "No se encontraron fechas para la remesa.";
                    return response;
                }

                // 🔹 2. Query de ingresos pendientes
                string sqlIngresos = @"
            SELECT 
                A.Consec,
                A.Cedula,
                S.Nombre,
                A.Fecha_Ingreso
            FROM AFI_INGRESOS A
            INNER JOIN Socios S ON A.Cedula = S.Cedula AND S.EstadoActual = 'S'
            WHERE 
                A.Fecha_Ingreso BETWEEN @FechaInicio AND @FechaCorte
                AND A.cod_remesa IS NULL
                AND dbo.fxSIFTagCierre(A.Cedula, A.Consec,'AFI') = 1
                /**filtroOficina**/
            ORDER BY A.Consec";

                // 🔹 Si la oficina no es "TODOS", agrega filtro dinámico
                if (!string.IsNullOrEmpty(oficina) && oficina.ToUpper() != "TODOS")
                {
                    sqlIngresos = sqlIngresos.Replace("/**filtroOficina**/", "AND A.Cod_Oficina = @Oficina");
                }
                else
                {
                    sqlIngresos = sqlIngresos.Replace("/**filtroOficina**/", "");
                }

                var lista = connection.Query<IngresosPendientesDto>(sqlIngresos, new
                {
                    FechaInicio = fechas.FechaInicio.Date,
                    FechaCorte = fechas.FechaCorte.Date.AddDays(1).AddSeconds(-1), // hasta las 23:59:59
                    Oficina = oficina
                }).ToList();

                response.Result = lista;
                response.Description = "Consulta realizada correctamente";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Cierra la remesa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codRemesa"></param>
        /// <returns></returns>
        public ErrorDto AFI_Remesa_Cerrar(int codEmpresa, int codRemesa)
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(connString);

                // Validar si la remesa está abierta
                string sqlExiste = @"SELECT COUNT(*) 
                             FROM AFI_REMESAS_ING 
                             WHERE cod_remesa = @CodRemesa AND estado = 'A'";

                int existe = connection.ExecuteScalar<int>(sqlExiste, new { CodRemesa = codRemesa });
                if (existe == 0)
                {
                    response.Code = -1;
                    response.Description = "La remesa ya se encuentra cerrada.";
                    return response;
                }

                // Actualizar estado
                string sqlUpdate = @"UPDATE AFI_REMESAS_ING 
                             SET estado = 'C' 
                             WHERE cod_remesa = @CodRemesa";

                connection.Execute(sqlUpdate, new { CodRemesa = codRemesa });

                response.Description = $"Remesa {codRemesa} cerrada correctamente.";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Carga la remesa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codRemesa"></param>
        /// <param name="ingresosSeleccionados"></param>
        /// <returns></returns>
        public ErrorDto AFI_Remesa_Cargar(int codEmpresa, int codRemesa, List<int> ingresosSeleccionados)
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(connString);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
     
                    string sqlExiste = @"SELECT COUNT(*) 
                                 FROM AFI_REMESAS_ING 
                                 WHERE cod_remesa = @CodRemesa AND estado = 'A'";
                    int existe = connection.ExecuteScalar<int>(sqlExiste, new { CodRemesa = codRemesa }, transaction);
                    if (existe == 0)
                    {
                        response.Code = -1;
                        response.Description = "La remesa ya está cerrada, no se puede cargar.";
                        return response;
                    }

                    string sqlUpdate = @"UPDATE AFI_INGRESOS 
                                 SET cod_remesa = @CodRemesa 
                                 WHERE Consec = @Consec";

                    foreach (var consec in ingresosSeleccionados)
                    {
                        connection.Execute(sqlUpdate, new { CodRemesa = codRemesa, Consec = consec }, transaction);
                    }

                    transaction.Commit();
                    response.Description = $"Se cargaron {ingresosSeleccionados.Count} ingresos a la remesa {codRemesa}.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    response.Code = -1;
                    response.Description = ex.Message;
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
        /// Obtiene remesas por cedula
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<RemesaConsultaDto>> AFI_RemesaPorCedula_Obtener(int codEmpresa, string cedula)
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<RemesaConsultaDto>>
            {
                Code = 0,
                Result = new List<RemesaConsultaDto>()
            };

            try
            {
                using var connection = new SqlConnection(connString);

                string sql = @"
            SELECT A.cod_remesa, A.fecha, A.usuario
            FROM AFI_REMESAS_ING A
            INNER JOIN AFI_INGRESOS X ON A.cod_remesa = X.cod_remesa
            WHERE X.cedula = @Cedula";

                var lista = connection.Query<RemesaConsultaDto>(sql, new { Cedula = cedula }).ToList();

                    response.Result = lista;
                    response.Description = "Consulta realizada correctamente";

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
