using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_CxC
{
    public class FrmCxCCuentasSgtIngresosDb
    {
        private readonly PortalDB _portalDb;

        public FrmCxCCuentasSgtIngresosDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCxCCuentasSgtIngresosDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Lista registros de ingresos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCIngresoDto>> ListarRegistrosIngresos(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<List<CxCIngresoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
                    SELECT 
                        Reb.Operacion     AS operacion,
                        Reb.Linea         AS linea,
                        Reb.cod_cargo     AS cod_cargo,
                        Car.Descripcion   AS descripcion,
                        Reb.Monto         AS monto,
                        Reb.Tipo          AS tipo,
                        Reb.Valor         AS valor,
                        Reb.Modifica      AS modifica,
                        Reb.Detalle       AS detalle,
                        Reb.Registro_Usuario AS usuario,
                        Reb.Registro_Fecha  AS fecha_registro
                    FROM CxC_Cuentas_Ingresos Reb
                    INNER JOIN CxC_Cargos Car 
                        ON Car.cod_cargo = Reb.cod_cargo
                    WHERE Reb.Operacion = @operacion
                    ORDER BY Reb.Linea
                ";

                response.Result = cn.Query<CxCIngresoDto>(sql, new { operacion }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene el centro de costo y la unidad asignada
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static (string codUnidad, string codCentroCosto) ObtenerOficina(SqlConnection cn, string usuario)
        {
            var result = cn.QueryFirstOrDefault<dynamic>(
                "sbSIFOficinasUsuario",
                new { usuario },
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == null)
                throw new InvalidOperationException("No se pudo obtener oficina del usuario");

            if (result.Inconsistencia == 1)
                throw new InvalidOperationException("No existen oficinas creadas");

            if (result.Inconsistencia == 2)
                throw new InvalidOperationException("Usuario no asignado a oficina");

            return (result.Cod_Unidad, result.Cod_Centro_Costo);
        }

        /// <summary>
        /// Guarda y actualiza el registro
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<bool> GuardarRegistrosIngresos(int codEmpresa, CxCIngresoGuardarDto dto)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Open();

                // 🔥 obtener oficina desde backend
                var (codUnidad, codCentroCosto) = ObtenerOficina(cn, dto.usuario);

                if (dto.linea == null)
                {
                    const string sql = @"
                        INSERT INTO CxC_Cuentas_Ingresos
                        (
                            Linea,
                            cod_cargo,
                            Operacion,
                            tipo,
                            monto,
                            valor,
                            modifica,
                            detalle,
                            registro_usuario,
                            registro_fecha,
                            cod_unidad,
                            cod_centro_costo
                        )
                        VALUES
                        (
                            ISNULL(
                                (SELECT MAX(Linea) + 1 
                                 FROM CxC_Cuentas_Ingresos 
                                 WHERE Operacion = @operacion),
                                1
                            ),
                            @cod_cargo,
                            @operacion,
                            @tipo,
                            @monto,
                            @valor,
                            1,
                            @detalle,
                            @usuario,
                            dbo.MyGetdate(),
                            @cod_unidad,
                            @cod_centro_costo
                        )
                    ";

                    cn.Execute(sql, new
                    {
                        dto.operacion,
                        dto.cod_cargo,
                        dto.tipo,
                        dto.monto,
                        dto.valor,
                        dto.detalle,
                        dto.usuario,
                        cod_unidad = codUnidad,
                        cod_centro_costo = codCentroCosto
                    });
                }
                else
                {
                    const string sql = @"
                        UPDATE CxC_Cuentas_Ingresos
                        SET 
                            tipo = @tipo,
                            valor = @valor,
                            monto = @monto,
                            detalle = @detalle,
                            registro_usuario = @usuario,
                            registro_fecha = dbo.MyGetdate(),
                            cod_cargo = @cod_cargo
                        WHERE Operacion = @operacion
                        AND Linea = @linea
                    ";

                    cn.Execute(sql, dto);
                }

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Elimina el registro de ingresos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="linea"></param>
        /// <param name="codCargo"></param>
        /// <returns></returns>
        public ErrorDto<bool> EliminarRegistroIngresos(int codEmpresa, int operacion, int linea, string codCargo)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
                    DELETE FROM CxC_Cuentas_Ingresos
                    WHERE Operacion = @operacion
                    AND Linea = @linea
                    AND cod_cargo = @codCargo
                ";

                cn.Execute(sql, new { operacion, linea, codCargo });

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Actualiza el registro de ingresos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> ActualizarRegistroingreso(int codEmpresa, int operacion, string usuario)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Execute(
                    "spCxC_CuentaIngresoReposicion",
                    new { operacion, usuario },
                    commandType: System.Data.CommandType.StoredProcedure
                );

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Scroll
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="codCargo"></param>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public ErrorDto<CxCIngresoDto> Scroll(int codEmpresa, int operacion, string? codCargo, string direccion)
        {
            var response = new ErrorDto<CxCIngresoDto>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                string sql = direccion == "SIGUIENTE"
                    ? @"
                SELECT TOP 1 cod_cargo, descripcion
                FROM CxC_Cargos
                WHERE cod_cargo > @codCargo
                AND Activo = 1
                AND Tipo = 'I'
                AND cod_cargo NOT IN (
                    SELECT cod_cargo 
                    FROM CxC_Cuentas_Ingresos 
                    WHERE Operacion = @operacion
                )
                ORDER BY cod_cargo ASC
              "
                    : @"
                SELECT TOP 1 cod_cargo, descripcion
                FROM CxC_Cargos
                WHERE cod_cargo < @codCargo
                AND Activo = 1
                AND Tipo = 'I'
                AND cod_cargo NOT IN (
                    SELECT cod_cargo 
                    FROM CxC_Cuentas_Ingresos 
                    WHERE Operacion = @operacion
                )
                ORDER BY cod_cargo DESC
              ";

                response.Result = cn.QueryFirstOrDefault<CxCIngresoDto>(
                    sql,
                    new { codCargo, operacion }
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Lista los ingresos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCIngresoDto>> IngresosListar(int codEmpresa)
        {
            var response = new ErrorDto<List<CxCIngresoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT cod_cargo, descripcion
            FROM CxC_Cargos
            WHERE Tipo = 'I'
            AND Activo = 1
            ORDER BY cod_cargo
        ";

                response.Result = cn.Query<CxCIngresoDto>(sql).ToList();
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