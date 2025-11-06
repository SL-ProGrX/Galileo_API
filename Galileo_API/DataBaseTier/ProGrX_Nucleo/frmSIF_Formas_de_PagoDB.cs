using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using System.Data;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSIF_Formas_de_PagoDB
    {
        private readonly IConfiguration? _config;

        public frmSIF_Formas_de_PagoDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la forma de pago por código
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codFormaPago"></param>
        /// <returns></returns>
        public ErrorDto<SifFormasPago> SIF_Formas_Pago_Obtener(int codEmpresa, string codFormaPago)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var result = new ErrorDto<SifFormasPago>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT 
                                *
                            FROM vSys_Formas_Pago
                            WHERE COD_FORMA_PAGO = @codFormaPago";

                result.Result = connection.QueryFirstOrDefault<SifFormasPago>(
                    query,
                    new { codFormaPago }
                );
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
        /// Obtiene el siguiente o anterior código de forma de pago según el orden.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codFormaPagoActual"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        public ErrorDto<string> SIF_Formas_Pago_Obtener_SigAnt(int codEmpresa, string? codFormaPagoActual, string orden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                string query;

                if (orden?.ToLower() == "asc")
                {
                    if (string.IsNullOrEmpty(codFormaPagoActual))
                    {
                        // Obtener el primer código
                        query = @"SELECT TOP 1 COD_FORMA_PAGO FROM sif_formas_pago ORDER BY COD_FORMA_PAGO ASC";
                        result.Result = connection.QueryFirstOrDefault<string>(query);
                    }
                    else
                    {
                        query = @"SELECT TOP 1 COD_FORMA_PAGO 
                                  FROM sif_formas_pago 
                                  WHERE COD_FORMA_PAGO > @codFormaPagoActual 
                                  ORDER BY COD_FORMA_PAGO ASC";
                        result.Result = connection.QueryFirstOrDefault<string>(query, new { codFormaPagoActual });
                    }
                }
                else if (orden?.ToLower() == "desc")
                {
                    if (string.IsNullOrEmpty(codFormaPagoActual))
                    {
                        // Obtener el último código
                        query = @"SELECT TOP 1 COD_FORMA_PAGO FROM sif_formas_pago ORDER BY COD_FORMA_PAGO DESC";
                        result.Result = connection.QueryFirstOrDefault<string>(query);
                    }
                    else
                    {
                        query = @"SELECT TOP 1 COD_FORMA_PAGO 
                                  FROM sif_formas_pago 
                                  WHERE COD_FORMA_PAGO < @codFormaPagoActual 
                                  ORDER BY COD_FORMA_PAGO DESC";
                        result.Result = connection.QueryFirstOrDefault<string>(query, new { codFormaPagoActual });
                    }
                }
                else
                {
                    result.Code = -1;
                    result.Description = "Parámetro 'orden' inválido. Debe ser 'asc' o 'desc'.";
                    return result;
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

        private bool FormaPagoExiste(SqlConnection connection, string codFormaPago)
        {
            var query = @"SELECT ISNULL(COUNT(*),0) FROM sif_formas_pago WHERE COD_FORMA_PAGO = @codFormaPago";
            int existe = connection.QueryFirstOrDefault<int>(query, new { codFormaPago });
            return existe > 0;
        }

        /// <summary>
        /// Inserta o actualiza una forma de pago.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="forma_pago"></param>
        /// <returns></returns>
        public ErrorDto SIF_Formas_Pago_Guardar(int codEmpresa, SifFormasPago forma_pago)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                connection.Open();

                // Validar si existe la forma de pago usando la función privada
                bool existe = FormaPagoExiste(connection, forma_pago.cod_forma_pago);

                if (!existe)
                {
                    result = SIF_Formas_Pago_Insertar(connection, codEmpresa, forma_pago);
                }
                else
                {
                    result = SIF_Formas_Pago_Actualizar(connection, codEmpresa, forma_pago);
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        private ErrorDto SIF_Formas_Pago_Insertar(SqlConnection connection, int codEmpresa, SifFormasPago forma_pago)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Forma de pago registrada correctamente."
            };
            try
            {
                var insertSql = @"
            INSERT INTO sif_formas_pago (
                COD_FORMA_PAGO, DESCRIPCION, ACTIVA, EFECTIVO, APLICA_SALDOS_FAVOR, COD_CUENTA, TIPO, APLICA_PARA_DEPOSITO,
                MAXIMO_APL, MAXIMO_MONTO, OR_APLICA, OR_DIARIO_APL, OR_DIARIO_MONTO, OR_MENSUAL_APL, OR_MENSUAL_MONTO,
                CODIGO_FE, RECIBO_DIGITAL, REGISTRO_USUARIO, REGISTRO_FECHA
            ) VALUES (
                @cod_forma_pago, UPPER(LTRIM(RTRIM(@descripcion))), @activa, @efectivo, @aplica_saldos_favor, @cod_cuenta, @tipo, @aplica_para_deposito,
                @maximo_apl, @maximo_monto, @or_aplica, @or_diario_apl, @or_diario_monto, @or_mensual_apl, @or_mensual_monto,
                @codigo_fe, @recibo_digital, @registro_usuario, GETDATE()
            )";
                connection.Execute(insertSql, new
                {
                    cod_forma_pago = forma_pago.cod_forma_pago.ToUpper(),
                    forma_pago.descripcion,
                    forma_pago.activa,
                    forma_pago.efectivo,
                    forma_pago.aplica_saldos_favor,
                    forma_pago.cod_cuenta,
                    forma_pago.tipo,
                    forma_pago.aplica_para_deposito,
                    forma_pago.maximo_apl,
                    forma_pago.maximo_monto,
                    forma_pago.or_aplica,
                    forma_pago.or_diario_apl,
                    forma_pago.or_diario_monto,
                    forma_pago.or_mensual_apl,
                    forma_pago.or_mensual_monto,
                    forma_pago.codigo_fe,
                    forma_pago.recibo_digital,
                    forma_pago.registro_usuario
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        private ErrorDto SIF_Formas_Pago_Actualizar(SqlConnection connection, int codEmpresa, SifFormasPago forma_pago)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Forma de pago actualizada correctamente."
            };
            try
            {
                var updateSql = @"
            UPDATE sif_formas_pago SET
                DESCRIPCION = UPPER(LTRIM(RTRIM(@descripcion))),
                ACTIVA = @activa,
                EFECTIVO = @efectivo,
                APLICA_SALDOS_FAVOR = @aplica_saldos_favor,
                COD_CUENTA = @cod_cuenta,
                TIPO = @tipo,
                APLICA_PARA_DEPOSITO = @aplica_para_deposito,
                MAXIMO_APL = @maximo_apl,
                MAXIMO_MONTO = @maximo_monto,
                OR_APLICA = @or_aplica,
                OR_DIARIO_APL = @or_diario_apl,
                OR_DIARIO_MONTO = @or_diario_monto,
                OR_MENSUAL_APL = @or_mensual_apl,
                OR_MENSUAL_MONTO = @or_mensual_monto,
                CODIGO_FE = @codigo_fe,
                RECIBO_DIGITAL = @recibo_digital,
                REGISTRO_USUARIO = @registro_usuario,
                REGISTRO_FECHA = GETDATE()
            WHERE UPPER(COD_FORMA_PAGO) = @cod_forma_pago";
                connection.Execute(updateSql, new
                {
                    cod_forma_pago = forma_pago.cod_forma_pago.ToUpper(),
                    forma_pago.descripcion,
                    forma_pago.activa,
                    forma_pago.efectivo,
                    forma_pago.aplica_saldos_favor,
                    forma_pago.cod_cuenta,
                    forma_pago.tipo,
                    forma_pago.aplica_para_deposito,
                    forma_pago.maximo_apl,
                    forma_pago.maximo_monto,
                    forma_pago.or_aplica,
                    forma_pago.or_diario_apl,
                    forma_pago.or_diario_monto,
                    forma_pago.or_mensual_apl,
                    forma_pago.or_mensual_monto,
                    forma_pago.codigo_fe,
                    forma_pago.recibo_digital,
                    forma_pago.registro_usuario
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Obtiene formas de pago con base en filtros y paginación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<SifFormasPagoList>> SIF_Formas_Pago_Obtener_Lista(int codEmpresa, string? filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var result = new ErrorDto<List<SifFormasPagoList>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifFormasPagoList>()
            };

            try
            {
                var where = "";
                if (!string.IsNullOrEmpty(filtro))
                {
                    where = " WHERE (COD_FORMA_PAGO LIKE '%" + filtro + "%' OR DESCRIPCION LIKE '%" + filtro + "%') ";
                }

                var query = $@"SELECT COD_FORMA_PAGO, DESCRIPCION FROM vSys_Formas_Pago {where}";

                using var connection = new SqlConnection(stringConn);
                result.Result = connection.Query<SifFormasPagoList>(query).ToList();
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
        /// Obtiene la lista de cuentas bancarias segun la forma de pago
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codFormaPago"></param>
        /// <returns></returns>
        public List<SysCuentasBancariasList> CuentasBancarias_Obtener_Lista(int CodEmpresa, string codFormaPago)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<SysCuentasBancariasList> info = new List<SysCuentasBancariasList>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var query = @"
                    select 
                      Ban.ID_BANCO,
                      Ban.DESCRIPCION,
                      Ban.Cta,
                      isnull(Fp.Id_Banco,0)      as Idx,
                      Ban.Cod_Divisa,
                      isnull(Eb.DESCRIPCION,'')  as Entidad_Desc
                    from TES_BANCOS Ban
                    left join SIF_FORMAS_PAGO_BANCOS_ASG Fp
                      on Ban.ID_BANCO = Fp.id_banco
                     and Fp.cod_forma_pago = @codFormaPago
                    left join TES_BANCOS_GRUPOS Eb
                      on Ban.Cod_Grupo = Eb.Cod_Grupo
                    where Ban.ESTADO = 'A'
                    order by Fp.id_Banco desc, Ban.ID_BANCO asc;";

                info = connection.Query<SysCuentasBancariasList>(query, new { codFormaPago }).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        /// <summary>
        /// Asigna o elimina cuentas bancarias segun la forma de pago
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CuentasBancarias_Asignar(int codEmpresa, SifFormasPagoBancoAsgDto data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var result = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(stringConn);

                // Verifica si existe
                var queryExiste = @"SELECT COUNT(*) FROM SIF_FORMAS_PAGO_BANCOS_ASG WHERE id_banco = @IdBanco AND cod_forma_pago = @CodFormaPago";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { data.IdBanco, data.CodFormaPago });

                if (existe > 0)
                {
                    // Elimina
                    var queryDelete = @"DELETE SIF_FORMAS_PAGO_BANCOS_ASG WHERE id_banco = @IdBanco AND cod_forma_pago = @CodFormaPago";
                    connection.Execute(queryDelete, new { data.IdBanco, data.CodFormaPago });
                    result.Description = "Eliminado correctamente.";
                }
                else
                {
                    // Inserta
                    var queryInsert = @"INSERT SIF_FORMAS_PAGO_BANCOS_ASG (id_banco, cod_forma_pago, registro_fecha, registro_usuario)
                                VALUES (@IdBanco, @CodFormaPago, dbo.MyGetdate(), @RegistroUsuario)";
                    connection.Execute(queryInsert, new { data.IdBanco, data.CodFormaPago, data.RegistroUsuario });
                    result.Description = "Insertado correctamente.";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

    }
}
