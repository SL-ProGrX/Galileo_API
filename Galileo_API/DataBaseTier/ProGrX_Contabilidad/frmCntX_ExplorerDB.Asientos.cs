using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public partial class FrmCntXExploradorContableDb
    {
        /// <summary>
        /// Mayoriza un asiento contable pendiente y balanceado.
        /// </summary>
        public ErrorDto<bool> Asientos_Mayorizar(CntxMayorizarRequest dto)
        {
            var response = new ErrorDto<bool>();

            if (!dto.cod_empresa.HasValue || dto.cod_contabilidad <= 0 ||
                string.IsNullOrWhiteSpace(dto.tipo_asiento) ||
                string.IsNullOrWhiteSpace(dto.num_asiento) ||
                string.IsNullOrWhiteSpace(dto.usuario))
            {
                response.Code = -1;
                response.Description = "Los datos del asiento son requeridos";
                return response;
            }

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(dto.cod_empresa.Value));

                var asiento = cn.QuerySingleOrDefault<(byte[] ts, DateTime? fecha_aplicado, string balanceado)>(
                    @"SELECT ts, fecha_aplicado, ISNULL(balanceado, 'N') AS balanceado
                      FROM CntX_Asientos
                      WHERE cod_contabilidad = @cod_contabilidad
                        AND tipo_asiento = @tipo_asiento
                        AND num_asiento = @num_asiento",
                    dto);

                if (asiento.ts == null)
                {
                    response.Code = -1;
                    response.Description = "El asiento seleccionado ya no existe";
                    return response;
                }

                if (dto.ts != null && !asiento.ts.SequenceEqual(dto.ts))
                {
                    response.Code = -2;
                    response.Description = "El asiento fue modificado por otro usuario";
                    return response;
                }

                if (asiento.fecha_aplicado.HasValue)
                {
                    response.Code = -1;
                    response.Description = "El asiento ya se encuentra mayorizado";
                    return response;
                }

                if (!string.Equals(asiento.balanceado, "S", StringComparison.OrdinalIgnoreCase))
                {
                    response.Code = -1;
                    response.Description = "No se puede mayorizar un asiento desbalanceado";
                    return response;
                }

                var periodoAbierto = cn.ExecuteScalar<int>(
                    @"SELECT COUNT(1)
                      FROM CntX_Periodos
                      WHERE cod_contabilidad = @cod_contabilidad
                        AND anio = @anio
                        AND mes = @mes
                        AND estado = 'P'",
                    dto) > 0;

                if (!periodoAbierto)
                {
                    response.Code = -2;
                    response.Description = "No se puede mayorizar este asiento porque el periodo se encuentra cerrado";
                    return response;
                }

                cn.Execute(
                    @"exec spCntX_AsientoMayoriza
                        @cod_contabilidad,
                        @usuario,
                        @tipo_asiento,
                        @num_asiento",
                    dto);

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
        /// Elimina un asiento respetando concurrencia, período y reglas de autorización.
        /// </summary>
        public ErrorDto<bool> Asiento_Borrar(CntxBorrarAsientoRequest dto)
        {
            var response = new ErrorDto<bool>();

            if (!dto.cod_empresa.HasValue || dto.cod_contabilidad <= 0 ||
                string.IsNullOrWhiteSpace(dto.tipo_asiento) ||
                string.IsNullOrWhiteSpace(dto.num_asiento) ||
                string.IsNullOrWhiteSpace(dto.usuario))
            {
                response.Code = -1;
                response.Description = "Los datos del asiento son requeridos";
                return response;
            }

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(dto.cod_empresa.Value));
                cn.Open();
                using var transaction = cn.BeginTransaction();

                try
                {
                    var asiento = cn.QuerySingleOrDefault<AsientoBorrarInfo>(
                        @"SELECT
                            ts,
                            fecha_aplicado,
                            CONVERT(varchar(10), modulo) AS modulo,
                            fecha_autoriza,
                            anio,
                            mes
                          FROM CntX_Asientos WITH (UPDLOCK, HOLDLOCK)
                          WHERE cod_contabilidad = @cod_contabilidad
                            AND tipo_asiento = @tipo_asiento
                            AND num_asiento = @num_asiento",
                        dto,
                        transaction);

                    if (asiento == null)
                    {
                        response.Code = -1;
                        response.Description = "El asiento seleccionado ya no existe";
                        transaction.Rollback();
                        return response;
                    }

                    if (dto.ts != null && asiento.ts != null && !asiento.ts.SequenceEqual(dto.ts))
                    {
                        response.Code = -2;
                        response.Description = "El asiento fue modificado por otro usuario";
                        transaction.Rollback();
                        return response;
                    }

                    var periodoAbierto = cn.ExecuteScalar<int>(
                        @"SELECT COUNT(1)
                          FROM CntX_Periodos
                          WHERE cod_contabilidad = @cod_contabilidad
                            AND anio = @anio
                            AND mes = @mes
                            AND estado = 'P'",
                        new
                        {
                            dto.cod_contabilidad,
                            asiento.anio,
                            asiento.mes
                        },
                        transaction) > 0;

                    if (!periodoAbierto)
                    {
                        response.Code = -2;
                        response.Description = "No se puede eliminar este asiento porque el periodo se encuentra cerrado";
                        transaction.Rollback();
                        return response;
                    }

                    if (!string.Equals(asiento.modulo, "20", StringComparison.OrdinalIgnoreCase) &&
                        !asiento.fecha_autoriza.HasValue)
                    {
                        response.Code = -1;
                        response.Description = "El asiento debe estar autorizado antes de eliminarse";
                        transaction.Rollback();
                        return response;
                    }

                    if (asiento.fecha_aplicado.HasValue)
                    {
                        cn.Execute(
                            @"exec spCntX_AsientoReversa
                                @cod_contabilidad,
                                @usuario,
                                @tipo_asiento,
                                @num_asiento",
                            dto,
                            transaction);
                    }

                    cn.Execute(
                        @"DELETE FROM CntX_Asientos_Detalle
                          WHERE cod_contabilidad = @cod_contabilidad
                            AND tipo_asiento = @tipo_asiento
                            AND num_asiento = @num_asiento;

                          DELETE FROM CntX_Asientos
                          WHERE cod_contabilidad = @cod_contabilidad
                            AND tipo_asiento = @tipo_asiento
                            AND num_asiento = @num_asiento;",
                        dto,
                        transaction);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                _securityDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = dto.cod_empresa.Value,
                    Usuario = dto.usuario!,
                    Modulo = 20,
                    Movimiento = "Elimina Asiento - WEB",
                    DetalleMovimiento = $"Asiento: {dto.tipo_asiento}-{dto.num_asiento} Conta.{dto.cod_contabilidad}"
                });

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
        /// Obtiene las notas del asiento seleccionado.
        /// </summary>
        public ErrorDto<string?> NotasAsiento(
            int codEmpresa,
            int cod_contabilidad,
            string tipo_asiento,
            string num_asiento)
        {
            var response = new ErrorDto<string?>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.QueryFirstOrDefault<string?>(
                    @"SELECT notas
                      FROM CntX_Asientos
                      WHERE cod_contabilidad = @cod_contabilidad
                        AND tipo_asiento = @tipo_asiento
                        AND num_asiento = @num_asiento",
                    new
                    {
                        cod_contabilidad,
                        tipo_asiento,
                        num_asiento
                    });
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
