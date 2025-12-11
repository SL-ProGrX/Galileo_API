using System.Globalization;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class MBeneficiosDB
    {
        private readonly IConfiguration _config;
        private const string CodCategoriaPlaceholder = "@cod_categoria";
        private const string CodBeneficioPlaceholder = "@cod_beneficio";
        private const string CedulaPlaceholder = "@cedula";
        private const string UsuarioPlaceholder = "@usuario";
        private const string IdBeneficioPlaceholder = "@id_beneficio";
        private const string MontoUsuarioPlaceholder = "@monto_usuario";
        private const string SepelioIdentificacionPlaceholder = "@sepelio_identificacion";

        public MBeneficiosDB(IConfiguration config)
        {
            _config = config;
        }

        #region Helpers comunes

        private SqlConnection CreateEmpresaConnection(int codEmpresa)
        {
            var connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            if (string.IsNullOrWhiteSpace(connString))
                throw new InvalidOperationException("Cadena de conexión de empresa no configurada.");

            return new SqlConnection(connString);
        }

        #endregion

        public ErrorDto fxNombre(int CodEmpresa, string cedula)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                using var connection = CreateEmpresaConnection(CodEmpresa);
                const string query = "select nombre from socios where cedula = @cedula";
                info.Description = connection.Query<string>(query, new { cedula = cedula.Trim() }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto fxDescribeBanco(int CodEmpresa, int codBanco)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                using var connection = CreateEmpresaConnection(CodEmpresa);
                const string query = "select descripcion from Tes_Bancos where id_banco = @codBanco";
                info.Description = connection.Query<string>(query, new { codBanco }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public static string fxEstadoBeneficio(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return "DESCONOCIDO";

            return estado.ToUpper() switch
            {
                "A" => "APROBADO",
                "S" => "SOLICITADO",
                "R" => "RECHAZADO",
                "E" => "EJECUTADO",
                "P" => "PENDIENTE",
                "APROBADO" => "A",
                "SOLICITADO" => "S",
                "RECHAZADO" => "R",
                "EJECUTADO" => "E",
                "PENDIENTE" => "P",
                _ => "DESCONOCIDO"
            };
        }

        public string fxSIFParametros(int CodEmpresa, string cod_parametro)
        {
            string resp;
            try
            {
                using var connection = CreateEmpresaConnection(CodEmpresa);
                const string query = "Select valor from SIF_parametros where cod_parametro = @cod_parametro";
                resp = connection.Query<string>(query, new { cod_parametro }).FirstOrDefault() ?? string.Empty;
            }
            catch (Exception ex)
            {
                resp = "N";
                _ = ex.Message;
            }
            return resp;
        }

        public string fxFSL_Parametros(int CodEmpresa, string cod_parametro)
        {
            string resp;
            try
            {
                using var connection = CreateEmpresaConnection(CodEmpresa);
                const string query = "select valor from fsl_parametros where cod_parametro = @cod_parametro";
                resp = connection.Query<string>(query, new { cod_parametro }).FirstOrDefault() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                resp = "N";
            }
            return resp;
        }

        /// <summary>
        /// Registra en bitácora los movimientos del beneficio Integral
        /// </summary>
        public ErrorDto BitacoraBeneficios(BitacoraBeneInsertarDto req)
        {
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = CreateEmpresaConnection(req.EmpresaId);

                var strSQL = @"
                    INSERT INTO [dbo].[AFI_BENE_REGISTRO_BITACORA]
                               ([COD_BENEFICIO]
                               ,[CONSEC]
                               ,[MOVIMIENTO]
                               ,[DETALLE]
                               ,[REGISTRO_FECHA]
                               ,[REGISTRO_USUARIO])
                         VALUES
                               (@cod_beneficio
                               ,@consec
                               ,@movimiento
                               ,@detalle
                               ,getdate()
                               ,@registro_usuario)";

                resp.Code = connection.Execute(strSQL, new
                {
                    req.cod_beneficio,
                    req.consec,
                    req.movimiento,
                    req.detalle,
                    req.registro_usuario
                });
                resp.Description = "Ok";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Busca el ultimo consecutivo de un beneficio
        /// </summary>
        public long fxConsec(int CodCliente, string cod_beneficio)
        {
            long vBeneConsec;
            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);
                const string query = @"Select isnull(Max(consec),0) as consecutivo 
                                       from afi_bene_otorga 
                                       where cod_beneficio = @cod_beneficio";
                vBeneConsec = connection.Query<long>(query, new { cod_beneficio }).FirstOrDefault() + 1;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                vBeneConsec = 0;
            }
            return vBeneConsec;
        }

        /// <summary>
        /// Valida si es socio esta activo o inactivo.
        /// </summary>
        public ErrorDto<BeneficioGeneralDatos> ValidaEstadoSocio(int CodCliente, string cedula)
        {
            var response = new ErrorDto<BeneficioGeneralDatos>();

            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);
                const string query = @"SELECT ESTADOACTUAL FROM SOCIOS WHERE CEDULA = @cedula";
                string? estado = connection.Query<string>(query, new { cedula }).FirstOrDefault();

                if (estado != "S")
                {
                    response.Code = -1;
                    response.Description = "El asociado se encuentra inactivo";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ValidaEstadoSocio - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto ValidarPersona(int CodCliente, string cedula, string? cod_beneficio)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);

                string query;
                if (cod_beneficio == null)
                {
                    query = @"SELECT * 
                              FROM AFI_BENE_VALIDACIONES 
                              WHERE ESTADO = 1 AND TIPO = 'P' AND REGISTRO = 1 
                              ORDER BY PRIORIDAD ASC";
                }
                else
                {
                    query = @"
                        select abv.* 
                        FROM AFI_BENE_VALIDA_CATEGORIA c 
                        left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                        WHERE COD_CATEGORIA = 
                        (
	                        SELECT ab.COD_CATEGORIA 
                            FROM AFI_BENEFICIOS ab 
	                        WHERE ab.COD_BENEFICIO = @cod_beneficio
                        ) 
                        AND c.ESTADO = 1 
                        AND TIPO = 'P' 
                        AND REGISTRO = 1 
                        order by abv.PRIORIDAD asc";
                }

                var validaciones = connection.Query<AfiBeneCalidaciones>(query, new { cod_beneficio }).ToList();

                foreach (var validacion in validaciones)
                {
                    var sql = validacion.query_val
                        .Replace("CedulaPlaceholder", cedula)
                        .Replace("CodBeneficioPlaceholder", cod_beneficio);

                    var result = connection.Query<int>(sql).FirstOrDefault();

                    if (result == validacion.resultado_val)
                    {
                        info.Code = 0;
                        info.Description += validacion.msj_val + "...\n";
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }

        public ErrorDto ValidarPersonaPago(int CodCliente, string cedula, string? cod_beneficio)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);

                string query;
                if (cod_beneficio == null)
                {
                    query = @"SELECT * 
                              FROM AFI_BENE_VALIDACIONES 
                              WHERE ESTADO = 1 
                                AND PAGO = 1 
                                AND TIPO = 'P' 
                              ORDER BY PRIORIDAD ASC";
                }
                else
                {
                    query = @"
                        select abv.* 
                        FROM AFI_BENE_VALIDA_CATEGORIA c 
                        left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                        WHERE COD_CATEGORIA = 
                        (
	                        SELECT ab.COD_CATEGORIA 
                            FROM AFI_BENEFICIOS ab 
	                        WHERE ab.COD_BENEFICIO = @cod_beneficio
                        ) 
                        AND c.ESTADO = 1 
                        AND TIPO = 'P' 
                        AND PAGO = 1 
                        order by abv.PRIORIDAD asc";
                }

                var validaciones = connection.Query<AfiBeneCalidaciones>(query, new { cod_beneficio }).ToList();

                foreach (var validacion in validaciones)
                {
                    var sql = validacion.query_val
                        .Replace(CedulaPlaceholder, cedula)
                        .Replace(CodBeneficioPlaceholder, cod_beneficio);

                    var result = connection.Query<int>(sql).FirstOrDefault();

                    if (result == validacion.resultado_val)
                    {
                        info.Code = 0;
                        info.Description += validacion.msj_val + "...\n";
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }

        public ErrorDto ValidaRequisitos(int CodCliente, string estado, string cod_beneficio, int consec)
        {
            var response = new ErrorDto();

            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);

                const string dtEstado = @"
                    SELECT COD_ESTADO
                    FROM [dbo].[AFI_BENE_ESTADOS]
                    WHERE COD_ESTADO = @estado 
                      AND P_FINALIZA = 1 
                      AND PROCESO = 'A'";

                string? finaliza = connection.Query<string>(dtEstado, new { estado }).FirstOrDefault();

                if (finaliza != null)
                {
                    var query = @"
                        SELECT 
                            CASE 
                                WHEN COUNT(CASE WHEN R.REQUERIDO = 1 AND RR.COD_BENEFICIO IS NOT NULL THEN 1 END) 
                                     = COUNT(CASE WHEN R.REQUERIDO = 1 THEN 1 END)
                                THEN 0
                                ELSE 1
                            END AS CumplenRequisito
                        FROM [AFI_BENE_GRUPO_REQUISITOS] GR
                        LEFT JOIN AFI_BENE_REQUISITOS R 
                            ON R.COD_REQUISITO = GR.COD_REQUISITO
                        LEFT JOIN AFI_BENE_REGISTRO_REQUISITOS RR 
                            ON RR.COD_REQUISITO = GR.COD_REQUISITO
                           AND RR.COD_BENEFICIO = @cod_beneficio
                           AND RR.CONSEC = @consec
                        WHERE GR.COD_GRUPO = 
                              (SELECT COD_GRUPO 
                               FROM AFI_BENEFICIOS 
                               WHERE COD_BENEFICIO = @cod_beneficio)";

                    var cumpleRequisito = connection.Query<int>(query, new { cod_beneficio, consec }).FirstOrDefault();
                    if (cumpleRequisito == 1)
                    {
                        response.Code = -1;
                        response.Description = "No cumple con los requisitos del beneficio";
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ValidaBeneficio - " + ex.Message;
            }
            return response;
        }

        public ErrorDto ValidaFallecido(int CodCliente, string cedulafallecido)
        {
            var response = new ErrorDto { Code = 0 };

            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);
                const string query = @"
                    SELECT CONCAT(O.ID_BENEFICIO, TRIM(O.COD_BENEFICIO), FORMAT(O.CONSEC,'00000'), '- cédula: ', O.CEDULA) as Texto
                    FROM AFI_BENE_OTORGA O 
                    WHERE SEPELIO_IDENTIFICACION = @cedulafallecido";

                var fallecido = connection.Query<string>(query, new { cedulafallecido }).ToList();

                if (fallecido.Count > 0)
                {
                    var otrosRegistros = new StringBuilder();
                    foreach (var item in fallecido)
                    {
                        otrosRegistros.Append(item + " - ");
                    }

                    response.Code = -1;
                    response.Description = "La cédula del fallecido se encuentra en los siguientes expedientes: " +
                                           otrosRegistros.ToString();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ValidaFallecido - " + ex.Message;
            }
            return response;
        }

        public ErrorDto ValidarBeneficioDato(int CodCliente, BeneficioGeneralDatos beneficio)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);

                const string queryValidaciones = @"
                    select abv.* 
                    FROM AFI_BENE_VALIDA_CATEGORIA c 
                    left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                    WHERE COD_CATEGORIA = 
                    (
	                    SELECT ab.COD_CATEGORIA 
                        FROM AFI_BENEFICIOS ab 
                        WHERE ab.COD_BENEFICIO = @CodBeneficio
                    ) 
                    AND c.ESTADO = 1 
                    AND TIPO = 'G' 
                    AND REGISTRO = 1 
                    order by abv.PRIORIDAD asc";

                var validaciones = connection
                    .Query<AfiBeneCalidaciones>(queryValidaciones, new { CodBeneficio = beneficio.cod_beneficio.item })
                    .ToList();

                foreach (var validacion in validaciones)
                {
                    var sql = (validacion.query_val ?? string.Empty)
                        .Replace(CedulaPlaceholder, beneficio.cedula)
                        .Replace(UsuarioPlaceholder, beneficio.registra_user)
                        .Replace(CodBeneficioPlaceholder, beneficio.id_beneficio.ToString())
                        .Replace(CodCategoriaPlaceholder, beneficio.cod_categoria)
                        .Replace(MontoUsuarioPlaceholder, Convert.ToDecimal(beneficio.monto_aplicado).ToString(CultureInfo.InvariantCulture))
                        .Replace(SepelioIdentificacionPlaceholder, beneficio.sepelio_identificacion);

                    var result = connection.Query<int>(sql).FirstOrDefault();

                    if (result == validacion.resultado_val)
                    {
                        info.Code = -1;
                        info.Description += validacion.msj_val + "...\n";
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }

        public ErrorDto ValidarBeneficioPagoDato(int CodCliente, BeneficioGeneralDatos beneficio)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);

                const string queryValidaciones = @"
                    select abv.* 
                    FROM AFI_BENE_VALIDA_CATEGORIA c 
                    left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                    WHERE COD_CATEGORIA = 
                    (
	                    SELECT ab.COD_CATEGORIA 
                        FROM AFI_BENEFICIOS ab 
                        WHERE ab.COD_BENEFICIO = @CodBeneficio
                    ) 
                    AND c.ESTADO = 1 
                    AND TIPO = 'G' 
                    AND PAGO = 1 
                    order by abv.PRIORIDAD asc";

                var validaciones = connection
                    .Query<AfiBeneCalidaciones>(queryValidaciones, new { CodBeneficio = beneficio.cod_beneficio.item })
                    .ToList();

                foreach (var validacion in validaciones)
                {
                    var sql = (validacion.query_val ?? string.Empty)
                        .Replace(CedulaPlaceholder, beneficio.cedula)
                        .Replace(UsuarioPlaceholder, beneficio.registra_user)
                        .Replace(CodBeneficioPlaceholder, beneficio.cod_beneficio.item.ToString())
                        .Replace(CodCategoriaPlaceholder, beneficio.cod_categoria)
                        .Replace(MontoUsuarioPlaceholder, Convert.ToDecimal(beneficio.monto_aplicado).ToString(CultureInfo.InvariantCulture))
                        .Replace(SepelioIdentificacionPlaceholder, beneficio.sepelio_identificacion);

                    var result = connection.Query<int>(sql).FirstOrDefault();

                    if (result == validacion.resultado_val)
                    {
                        info.Code = -1;
                        info.Description += validacion.msj_val + "...\n";
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }

        public ErrorDto ValidarBeneficioJustificaDato(int CodCliente, BeneficioGeneralDatos beneficio, bool justifica)
        {
            var info = new ErrorDto
            {
                Code = 0,
                Description = string.Empty
            };

            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);

                const string queryValidaciones = @"
                    SELECT abv.*, c.registro_justifica
                    FROM AFI_BENE_VALIDA_CATEGORIA c
                    LEFT JOIN AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                    WHERE c.COD_CATEGORIA = (
                        SELECT ab.COD_CATEGORIA
                        FROM AFI_BENEFICIOS ab
                        WHERE ab.COD_BENEFICIO = @CodBeneficio
                    )
                      AND c.ESTADO = 1
                      AND c.REGISTRO = 1
                      AND c.TIPO <> 'G'
                    ORDER BY abv.PRIORIDAD ASC;";

                var validaciones = connection
                    .Query<AfiBeneCalidaciones>(queryValidaciones, new { CodBeneficio = beneficio.cod_beneficio.item })
                    .ToList();

                int justificadas = 0;
                int obligatorias = 0;

                foreach (var v in validaciones)
                {
                    var sql = BuildValidationSql(v.query_val, beneficio);
                    var result = connection.QueryFirstOrDefault<int>(sql);

                    if (result != v.resultado_val) continue;

                    obligatorias++;
                    if (v.registro_justifica) justificadas++;

                    if (!string.IsNullOrEmpty(v.msj_val))
                        info.Description += v.msj_val + "...\n";
                }

                if (justificadas > 0)
                    info.Code = justifica ? 0 : -1;

                int activas = obligatorias - justificadas;
                if (activas > 0 && !string.IsNullOrEmpty(info.Description))
                    info.Code = -1;
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;

            static string BuildValidationSql(string? template, BeneficioGeneralDatos b)
            {
                return (template ?? string.Empty)
                    .Replace(CedulaPlaceholder, b.cedula)
                    .Replace(UsuarioPlaceholder, b.registra_user)
                    .Replace(CodBeneficioPlaceholder, b.cod_beneficio.item.ToString(CultureInfo.InvariantCulture))
                    .Replace(CodCategoriaPlaceholder, b.cod_categoria)
                    .Replace(MontoUsuarioPlaceholder, Convert.ToDecimal(b.monto_aplicado).ToString(CultureInfo.InvariantCulture))
                    .Replace(SepelioIdentificacionPlaceholder, b.sepelio_identificacion);
            }
        }

        public ErrorDto ValidarBeneficioPagoJustificaDato(int CodCliente, BeneficioGeneralDatos beneficio, bool justifica)
        {
            var info = new ErrorDto { Code = 0, Description = string.Empty };

            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);

                const string queryValidaciones = @"
                    SELECT abv.*, c.pago_justifica
                    FROM AFI_BENE_VALIDA_CATEGORIA c
                    LEFT JOIN AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                    WHERE c.COD_CATEGORIA = (
                        SELECT ab.COD_CATEGORIA
                        FROM AFI_BENEFICIOS ab
                        WHERE ab.COD_BENEFICIO = @CodBeneficio
                    )
                    AND c.ESTADO = 1
                    AND c.PAGO = 1
                    AND c.TIPO <> 'G'
                    ORDER BY abv.PRIORIDAD ASC";

                var validaciones = connection
                    .Query<AfiBeneCalidaciones>(queryValidaciones, new { CodBeneficio = beneficio.cod_beneficio.item })
                    .ToList();

                int justificadas = 0, obligatorias = 0;
                var desc = new StringBuilder();

                foreach (var v in validaciones)
                {
                    var sql = BuildValidationSql(v.query_val, beneficio);
                    var result = connection.QueryFirstOrDefault<int>(sql);
                    if (result != v.resultado_val) continue;

                    obligatorias++;
                    if (v.pago_justifica) justificadas++;
                    if (!string.IsNullOrEmpty(v.msj_val)) desc.Append(v.msj_val).Append("...\n");
                }

                info.Description = desc.ToString();
                info.Code = DecideCode(justificadas, obligatorias, justifica, !string.IsNullOrEmpty(info.Description));
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;

            static string BuildValidationSql(string? template, BeneficioGeneralDatos b) =>
                (template ?? string.Empty)
                .Replace(CedulaPlaceholder, b.cedula)
                .Replace(UsuarioPlaceholder, b.registra_user)
                .Replace(IdBeneficioPlaceholder, b.id_beneficio.ToString())
                .Replace(CodBeneficioPlaceholder, b.cod_beneficio.item.ToString())
                .Replace(CodCategoriaPlaceholder, b.cod_categoria)
                .Replace(MontoUsuarioPlaceholder, Convert.ToDecimal(b.monto_aplicado).ToString(CultureInfo.InvariantCulture))
                .Replace(SepelioIdentificacionPlaceholder, b.sepelio_identificacion);

            static int DecideCode(int justificadas, int obligatorias, bool justifica, bool hasDesc)
            {
                if (justificadas > 0 && !justifica) return -1;
                var activas = obligatorias - justificadas;
                if (activas > 0 && hasDesc) return -1;
                return 0;
            }
        }

        public ErrorDto ValidaCargaPagos(int CodCliente, BeneficioGeneralDatos beneficio)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);

                const string queryValidaciones = @"
                    select abv.*, c.pago_justifica 
                    FROM AFI_BENE_VALIDA_CATEGORIA c 
                    left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                    WHERE COD_CATEGORIA = 
                        (
	                        SELECT ab.COD_CATEGORIA 
                            FROM AFI_BENEFICIOS ab 
                            WHERE ab.COD_BENEFICIO = @CodBeneficio
                        ) 
                      AND c.ESTADO = 1 
                      AND PAGO = 1 
                      AND TIPO != 'G' 
                    order by abv.PRIORIDAD asc";

                var validaciones = connection
                    .Query<AfiBeneCalidaciones>(queryValidaciones, new { CodBeneficio = beneficio.cod_beneficio.item })
                    .ToList();

                foreach (var validacion in validaciones)
                {
                    var sql = (validacion.query_val ?? string.Empty)
                        .Replace(CedulaPlaceholder, beneficio.cedula)
                        .Replace(UsuarioPlaceholder, beneficio.registra_user)
                        .Replace(IdBeneficioPlaceholder, beneficio.id_beneficio.ToString())
                        .Replace(CodBeneficioPlaceholder, beneficio.cod_beneficio.item.ToString())
                        .Replace(CodCategoriaPlaceholder, beneficio.cod_categoria)
                        .Replace(MontoUsuarioPlaceholder, Convert.ToDecimal(beneficio.monto_aplicado).ToString(CultureInfo.InvariantCulture))
                        .Replace(SepelioIdentificacionPlaceholder, beneficio.sepelio_identificacion);

                    var result = connection.Query<int>(sql).FirstOrDefault();

                    if (result == validacion.resultado_val)
                    {
                        if (validacion.pago_justifica)
                        {
                            info.Description += " ** " + validacion.msj_val + " ** ...\n";
                        }
                        else
                        {
                            info.Description += validacion.msj_val + "...\n";
                        }

                        info.Code = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }
    }
}