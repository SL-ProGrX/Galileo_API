using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXUnidadesDb
    {
        private readonly PortalDB _portalDB;

        public FrmCntXUnidadesDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }


        /// <summary>
        /// Lista las unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXUnidadDto>> CntX_Unidades_Listar(int codEmpresa)
        {
            var response = new ErrorDto<List<CntXUnidadDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
                    SELECT
                        RTRIM(cod_unidad) AS cod_unidad,
                        RTRIM(descripcion) AS descripcion,
                        Nivel AS nivel,
                        unidad_omision,
                        reporta_renta,
                        activa,
                        RTRIM(Cta_Renta) AS cta_renta,
                        RTRIM(Cta_Renta_Gasto) AS cta_renta_gasto
                    FROM CntX_Unidades
                    WHERE COD_CONTABILIDAD = 2
                    ORDER BY cod_unidad;
                ";

                response.Result = cn.Query<CntXUnidadDto>(sql, new { codEmpresa }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda las unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<bool> CntX_Unidades_Guardar(int codEmpresa, string usuario, CntXUnidadGuardarDto dto)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                // Existe?
                var existe = cn.ExecuteScalar<int>(@"
                    SELECT ISNULL(COUNT(*),0)
                    FROM CntX_Unidades
                    WHERE COD_CONTABILIDAD = @codEmpresa
                      AND cod_unidad = @cod_unidad;
                ", new
                {
                    codEmpresa,
                    cod_unidad = dto.cod_unidad.Trim()
                });

                if (existe == 0)
                {
                    // INSERT (mantengo mismas columnas que VB6, excepto cuentas que vos no estás editando aún)
                    cn.Execute(@"
                        INSERT INTO CntX_Unidades
                        (
                            cod_unidad,
                            COD_CONTABILIDAD,
                            descripcion,
                            nivel,
                            unidad_omision,
                            reporta_renta,
                            activa
                        )
                        VALUES
                        (
                            @cod_unidad,
                            @codEmpresa,
                            @descripcion,
                            @nivel,
                            @unidad_omision,
                            @reporta_renta,
                            @activa
                        );
                    ", new
                    {
                        codEmpresa,
                        cod_unidad = dto.cod_unidad.Trim().ToUpper(),
                        descripcion = dto.descripcion.Trim(),
                        nivel = dto.nivel,
                        unidad_omision = dto.unidad_omision,
                        reporta_renta = dto.reporta_renta,
                        activa = dto.activa
                    });
                }
                else
                {
                    // UPDATE
                    cn.Execute(@"
                        UPDATE CntX_Unidades
                           SET descripcion = @descripcion,
                               nivel = @nivel,
                               unidad_omision = @unidad_omision,
                               reporta_renta = @reporta_renta,
                               activa = @activa
                         WHERE COD_CONTABILIDAD = @codEmpresa
                           AND cod_unidad = @cod_unidad;
                    ", new
                    {
                        codEmpresa,
                        cod_unidad = dto.cod_unidad.Trim(),
                        descripcion = dto.descripcion.Trim(),
                        nivel = dto.nivel,
                        unidad_omision = dto.unidad_omision,
                        reporta_renta = dto.reporta_renta,
                        activa = dto.activa
                    });
                }

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }

        /// <summary>
        /// Elimina las unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<bool> CntX_Unidades_Eliminar(int codEmpresa, string usuario, string codUnidad)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Execute(@"
                    DELETE CntX_Unidades
                     WHERE COD_CONTABILIDAD = @codEmpresa
                       AND cod_unidad = @codUnidad;
                ", new { codEmpresa, codUnidad });

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }


        /// <summary>
        /// Lista las unidades activas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXUnidadActivaDto>> CntX_Unidades_Activas_Listar(int codEmpresa)
        {
            var response = new ErrorDto<List<CntXUnidadActivaDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
                    SELECT
                        RTRIM(cod_unidad) AS cod_unidad,
                        RTRIM(descripcion) AS descripcion
                    FROM CntX_Unidades
                    WHERE activa = 1
                      AND cod_contabilidad = @codEmpresa
                    ORDER BY cod_unidad;
                ";

                response.Result = cn.Query<CntXUnidadActivaDto>(sql, new { codEmpresa }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Trae centros de costo por unidad 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXCentroCostoDto>> CntX_CentrosCosto_PorUnidad(int codEmpresa, string codUnidad)
        {
            var response = new ErrorDto<List<CntXCentroCostoDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
                    SELECT
                        RTRIM(C.cod_centro_costo) AS cod_centro_costo,
                        RTRIM(C.descripcion) AS descripcion,
                        CASE WHEN A.cod_centro_costo IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS asignado
                    FROM CntX_Centro_costos C
                    LEFT JOIN CntX_Unidades_CC A
                           ON C.cod_centro_costo = A.cod_centro_costo
                          AND C.cod_contabilidad = A.cod_contabilidad
                          AND A.cod_unidad = @codUnidad
                          AND A.cod_contabilidad = @codEmpresa
                    WHERE C.cod_contabilidad = @codEmpresa
                    ORDER BY asignado DESC, C.cod_centro_costo;
                ";

                response.Result = cn.Query<CntXCentroCostoDto>(sql, new { codEmpresa, codUnidad }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda las unidades CC
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<bool> CntX_Unidades_CC_Guardar(int codEmpresa, string usuario, CntXUnidadCCGuardarDto dto)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                if (dto.asociado == 1)
                {
                    // INSERT si no existe
                    cn.Execute(@"
                        IF NOT EXISTS (
                            SELECT 1
                              FROM CntX_Unidades_CC
                             WHERE cod_unidad = @cod_unidad
                               AND cod_centro_costo = @cod_centro_costo
                               AND cod_contabilidad = @codEmpresa
                        )
                        BEGIN
                            INSERT INTO CntX_Unidades_CC(cod_unidad, cod_centro_costo, cod_contabilidad)
                            VALUES(@cod_unidad, @cod_centro_costo, @codEmpresa);
                        END
                    ", new
                    {
                        codEmpresa,
                        cod_unidad = dto.cod_unidad.Trim(),
                        cod_centro_costo = dto.cod_centro_costo.Trim()
                    });
                }
                else
                {
                    // DELETE
                    cn.Execute(@"
                        DELETE CntX_Unidades_CC
                         WHERE cod_unidad = @cod_unidad
                           AND cod_centro_costo = @cod_centro_costo
                           AND cod_contabilidad = @codEmpresa;
                    ", new
                    {
                        codEmpresa,
                        cod_unidad = dto.cod_unidad.Trim(),
                        cod_centro_costo = dto.cod_centro_costo.Trim()
                    });
                }

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }


        /// <summary>
        /// Consulta las unidades CC
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXCentroCostoDto>> CntX_Unidades_CC_Consulta(int codEmpresa,string codUnidad
        )
        {
            var response = new ErrorDto<List<CntXCentroCostoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var sql = @"
                   SELECT
                      C.cod_centro_costo,
                      C.descripcion,
                      CASE 
                        WHEN A.cod_centro_costo IS NULL THEN 0
                        ELSE 1
                      END AS asignado
                    FROM CntX_Centro_costos C
                    LEFT JOIN CntX_Unidades_CC A
                      ON C.cod_centro_costo = A.cod_centro_costo
                     AND A.cod_unidad = @codUnidad
                     AND A.cod_contabilidad = 2
                    WHERE C.cod_contabilidad = 2
                    ORDER BY asignado DESC, C.cod_centro_costo
                ";

                response.Result = cn
                    .Query<CntXCentroCostoDto>(
                        sql,
                        new
                        {
                            codEmpresa,
                            codUnidad
                        }
                    )
                    .ToList();
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
