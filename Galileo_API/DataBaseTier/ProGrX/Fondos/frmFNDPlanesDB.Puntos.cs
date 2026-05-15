using System.Data;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndPlanesDb
    {


        private const string SpReglasTasasList = "spFnd_Reglas_Tasas_List";
        private const string SpReglasTasas = "spFnd_Reglas_Tasas";

        private const string SqlReglasTasasDetalle = @"
                    SELECT
                        COD_TABLA_AUM,
                        Tipo_Tasa,
                        desde,
                        hasta,
                        plus
                    FROM dbo.FND_TABLA_AUMENTOS
                    WHERE cod_operadora = @codOperadora
                      AND cod_plan = @codPlan
                      AND ID_PER_TASA = @id_per_tasa
                    ORDER BY COD_TABLA_AUM;";

        private const string SqlDeletePunto = @"
                    DELETE FROM dbo.FND_PLANES_PUNTOS
                    WHERE id = @Id;";

        private const string SqlInsertPuntoDetalle = @"
                    INSERT INTO dbo.FND_TABLA_AUMENTOS
                    (
                        cod_operadora,
                        cod_plan,
                        Tipo_Tasa,
                        desde,
                        hasta,
                        plus,
                        registro_usuario,
                        registro_fecha,
                        ID_PER_TASA
                    )
                    VALUES
                    (
                        @cod_operadora,
                        @cod_plan,
                        @tipo_tasa,
                        @desde,
                        @hasta,
                        @plus,
                        @usuario,
                        dbo.MyGetDate(),
                        @id_per_tasa
                    );

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

        private const string SqlUpdatePuntoDetalle = @"
                    UPDATE dbo.FND_TABLA_AUMENTOS
                    SET Tipo_Tasa = @tipo_tasa,
                        desde = @desde,
                        hasta = @hasta,
                        plus = @plus,
                        actualiza_usuario = @usuario,
                        actualiza_fecha = dbo.MyGetDate()
                    WHERE COD_TABLA_AUM = @id;";

        private const string SqlDeletePuntoDetalle = @"
                    DELETE FROM dbo.FND_TABLA_AUMENTOS
                    WHERE COD_TABLA_AUM = @id;";

        #region Puntos

        /// <summary>
        /// Obtiene las reglas tasa list
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlan"></param>
        /// <returns></returns>
        public ErrorDto<List<FndReglaTasaDto>> Fnd_ReglasTasas_List(int CodEmpresa, int codOperadora, string codPlan)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<FndReglaTasaDto>(
                    SpReglasTasasList,
                    new
                    {
                        CodOperadora = codOperadora,
                        CodPlan = NormalizarTexto(codPlan)
                    },
                    commandType: CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<FndReglaTasaDto>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<FndReglaTasaDto>()
            };
        }

        /// <summary>
        /// Obtiene las reglas detalle
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlan"></param>
        /// <param name="id_per_tasa"></param>
        /// <returns></returns>
        public ErrorDto<List<FndReglaTasaDetalleDto>> Fnd_ReglasTasas_Detalle_Obtener(int CodEmpresa, int codOperadora, string codPlan, int id_per_tasa)
        {
            return DbHelper.ExecuteListQuery<FndReglaTasaDetalleDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlReglasTasasDetalle,
                new
                {
                    codOperadora,
                    codPlan = NormalizarTexto(codPlan),
                    id_per_tasa
                });
        }

        /// <summary>
        /// Guarda los planes de retiros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<FndPlanRetiroDto> Fnd_Planes_Retiros_Guardar(int CodEmpresa, string usuario, FndPlanRetiroDto dto)
        {
            var resp = new ErrorDto<FndPlanRetiroDto> { Code = 0 };

            try
            {
                string connStr = new PortalDB(_config)
                    .ObtenerDbConnStringEmpresa(CodEmpresa);

                using var connection = new SqlConnection(connStr);

                if (dto.id == 0)
                {

                    string sql = @"
                INSERT INTO fnd_tabla_retiros
                (
                    cod_operadora,
                    cod_plan,
                    desde,
                    hasta,
                    porcentaje,
                    aplicar_a,
                    registro_usuario,
                    registro_fecha
                )
                VALUES
                (
                    @cod_operadora,
                    @cod_plan,
                    @desde,
                    @hasta,
                    @porcentaje,
                    @aplicar,
                    @usuario,
                    dbo.MyGetDate()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);
                ";

                    dto.id = connection.QuerySingle<int>(sql, new
                    {
                        dto.cod_operadora,
                        dto.cod_plan,
                        dto.desde,
                        dto.hasta,
                        dto.porcentaje,
                        aplicar = dto.aplicar,
                        usuario
                    });

                    dto.registro_usuario = usuario;
                    dto.registro_fecha = DateTime.Now;
                }
                else
                {

                    string sql = @"
                        UPDATE fnd_tabla_retiros SET
                            desde = @desde,
                            hasta = @hasta,
                            porcentaje = @porcentaje,
                            aplicar_a = @aplicar,
                            actualiza_usuario = @usuario,
                            actualiza_fecha = dbo.MyGetDate()
                        WHERE cod_fnd_tabla_ret = @id
                          AND cod_operadora = @cod_operadora
                          AND cod_plan = @cod_plan;
";

                    connection.Execute(sql, new
                    {
                        dto.desde,
                        dto.hasta,
                        dto.porcentaje,
                        aplicar = dto.aplicar,
                        usuario,
                        dto.id,
                        dto.cod_operadora,
                        dto.cod_plan
                    });

                    dto.actualiza_usuario = usuario;
                    dto.actualiza_fecha = DateTime.Now;
                }

                resp.Result = dto;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Elimina los retiros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDto<string> Fnd_Planes_Retiros_Eliminar(int CodEmpresa, int id)
        {
            var resp = new ErrorDto<string> { Code = 0 };

            try
            {
                string connStr = new PortalDB(_config)
                    .ObtenerDbConnStringEmpresa(CodEmpresa);

                using var connection = new SqlConnection(connStr);

                string sql = "DELETE FROM fnd_tabla_retiros WHERE cod_fnd_tabla_ret = @id";

                connection.Execute(sql, new { id });

                resp.Result = "OK";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Elimina los puntos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDto<string> Fnd_Planes_Puntos_Eliminar(int codEmpresa, int id)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                codEmpresa,
                SqlDeletePunto,
                new { Id = id });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al eliminar punto.", result.Code.GetValueOrDefault(-1), string.Empty);
            }

            return result.Result == 0
                ? DbHelper.CreateErrorResponse("No se encontró el punto para eliminar.", -1, string.Empty)
                : DbHelper.CreateOkResponse("OK");
        }

        /// <summary>
        /// Guarda los puntos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<FndPlanPuntoDto> Fnd_Planes_Puntos_Guardar(int CodEmpresa, string Usuario, FndPlanPuntoDto dto)
        {
            var response = DbHelper.CreateOkResponse(dto);

            try
            {
                if (dto is null)
                {
                    return DbHelper.CreateErrorResponse<FndPlanPuntoDto>("Los datos del punto son requeridos.", -2, null!);
                }

                var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                    connection.QueryFirstOrDefault<dynamic>(
                        SpReglasTasas,
                        CrearParametrosPunto(Usuario, dto),
                        commandType: CommandType.StoredProcedure));

                if (result.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(result.Description ?? "Error al guardar punto.", result.Code.GetValueOrDefault(-1), dto);
                }

                if (result.Result is null)
                {
                    return DbHelper.CreateErrorResponse("El SP no devolvió resultado.", -1, dto);
                }

                if (result.Result.IdRegla != null)
                {
                    dto.id = Convert.ToInt32(result.Result.IdRegla);
                }

                response.Result = dto;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda los puntos de detalle
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<FndPlanPuntoDetalleDto> Fnd_Planes_Puntos_Detalle_Guardar(int CodEmpresa, string Usuario, FndPlanPuntoDetalleDto dto)
        {
            var response = DbHelper.CreateOkResponse(dto);

            try
            {
                if (dto is null)
                {
                    return DbHelper.CreateErrorResponse<FndPlanPuntoDetalleDto>("Los datos del detalle son requeridos.", -2, null!);
                }

                var parametros = CrearParametrosPuntoDetalle(Usuario, dto);

                if (dto.id == 0)
                {
                    var insert = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                        connection.ExecuteScalar<int>(SqlInsertPuntoDetalle, parametros));

                    if (insert.Code != 0)
                    {
                        return DbHelper.CreateErrorResponse(insert.Description ?? "Error al insertar detalle.", insert.Code.GetValueOrDefault(-1), dto);
                    }

                    dto.id = insert.Result;
                }
                else
                {
                    var update = DbHelper.ExecuteNonQuery(
                        CreatePortalDb(),
                        CodEmpresa,
                        SqlUpdatePuntoDetalle,
                        parametros);

                    if (update.Code != 0)
                    {
                        return DbHelper.CreateErrorResponse(update.Description ?? "Error al actualizar detalle.", update.Code.GetValueOrDefault(-1), dto);
                    }
                }

                response.Result = dto;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Elimina los puntos de planes detalle
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDto<string> Fnd_Planes_Puntos_Detalle_Eliminar(int CodEmpresa, int id)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                SqlDeletePuntoDetalle,
                new { id });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al eliminar detalle.", result.Code.GetValueOrDefault(-1), string.Empty);
            }

            return result.Result == 0
                ? DbHelper.CreateErrorResponse("No se encontró el detalle para eliminar.", -1, string.Empty)
                : DbHelper.CreateOkResponse("OK");
        }
        /// <summary>
        /// Crea parámetros seguros para guardar una regla de tasa.
        /// </summary>
        private static object CrearParametrosPunto(string usuario, FndPlanPuntoDto dto)
        {
            return new
            {
                Id = dto.id,
                Operadora = dto.cod_operadora,
                Plan = NormalizarTexto(dto.cod_plan),
                Inicio = dto.fecha_referencia,
                Tipo = NormalizarTexto(dto.tipo),
                Justificacion = NormalizarTexto(dto.justificacion),
                Usuario = NormalizarTexto(usuario)
            };
        }

        /// <summary>
        /// Crea parámetros seguros para guardar el detalle de una regla de tasa.
        /// </summary>
        private static object CrearParametrosPuntoDetalle(string usuario, FndPlanPuntoDetalleDto dto)
        {
            return new
            {
                dto.id,
                dto.cod_operadora,
                cod_plan = NormalizarTexto(dto.cod_plan),
                tipo_tasa = NormalizarTexto(dto.tipo_tasa),
                dto.desde,
                dto.hasta,
                dto.plus,
                usuario = NormalizarTexto(usuario),
                dto.id_per_tasa
            };
        }



        /// <summary>
        /// Guarda los destinos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto Planes_Destinos_Guardar(int CodEmpresa, FndPlanDestinoGuardarDto dto)
        {
            var result = new ErrorDto { Code = 0, Description = "OK" };

            try
            {
                string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using var conn = new SqlConnection(connString);

                var parameters = new DynamicParameters();
                parameters.Add("@Id", dto.id);
                parameters.Add("@Plan", dto.cod_plan);
                parameters.Add("@Descripcion", dto.descripcion);
                parameters.Add("@Activo", dto.activo ? 1 : 0);
                parameters.Add("@Usuario", dto.usuario);

                conn.QueryFirst<int>("spFnd_Planes_Destinos_Ahorros_Add", parameters, commandType: CommandType.StoredProcedure);

            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }

            return result;
        }

        /// <summary>
        /// Elimina los planes de destinos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> Planes_Destinos_Eliminar(int CodEmpresa, int id, string usuario)
        {
            var result = new ErrorDto<bool> { Code = 0, Description = "OK" };

            try
            {
                string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using var conn = new SqlConnection(connString);

                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                parameters.Add("@Usuario", usuario);

                conn.Execute("spFnd_Planes_Destinos_Ahorros_Delete",
                             parameters,
                             commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                return new ErrorDto<bool>
                {
                    Code = -1,
                    Description = ex.Message
                };
            }

            return result;
        }

        /// <summary>
        /// Guarda los planes de destinos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<bool> Fnd_Planes_Destinos_Asociados_Guardar(int CodEmpresa, string usuario, FndPlanDestinoAsociadoDto dto)
        {
            var resp = new ErrorDto<bool> { Code = 0 };

            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var db = new SqlConnection(conn);

                if (dto.asociado == 1)
                {
                    string insert = @"
                INSERT INTO FND_PLANES_DESTINOS
                (cod_plan, cod_operadora, cod_destino, registro_usuario, registro_fecha)
                VALUES (@cod_plan, @cod_operadora, @id_destino, @usuario, dbo.MyGetDate())";

                    db.Execute(insert, new
                    {
                        dto.cod_plan,
                        dto.cod_operadora,
                        dto.id_destino,
                        usuario
                    });
                }
                else
                {
                    string delete = @"
                DELETE FROM FND_PLANES_DESTINOS
                WHERE cod_plan = @cod_plan
                AND cod_operadora = @cod_operadora
                AND cod_destino = @id_destino";

                    db.Execute(delete, new
                    {
                        dto.cod_plan,
                        dto.cod_operadora,
                        dto.id_destino
                    });
                }

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        #endregion

    }
}