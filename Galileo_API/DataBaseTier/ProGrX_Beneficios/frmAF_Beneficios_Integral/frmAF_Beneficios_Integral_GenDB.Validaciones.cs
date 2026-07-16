using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralGenDB
    {
        /// <summary>
        /// Valida si el socio ya está registrado en el programa Crece dentro de la vigencia.
        /// </summary>
        public ErrorDto ValidaProgramaCrece(int CodCliente, string cedula)
        {
            const string sql = @"
                SELECT COUNT(*) FROM AFI_BENE_OTORGA
                WHERE CEDULA = @cedula
                  AND COD_BENEFICIO IN (SELECT B.COD_BENEFICIO FROM AFI_BENEFICIOS B WHERE COD_CATEGORIA = 'B_CRECE')
                  AND DATEDIFF(MONTH, REGISTRA_FECHA, GETDATE()) < (SELECT MAX(A.VIGENCIA_MESES) FROM AFI_BENEFICIOS A WHERE COD_CATEGORIA = 'B_CRECE')";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<int>(sql, new { cedula }));

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = "Error al validar el programa" };
            }

            return result.Result > 0
                ? new ErrorDto { Code = -1, Description = "El socio ya se encuentra registrado en el programa Crece, ¿Desea registrarlo nuevamente?" }
                : new ErrorDto { Code = 0, Description = string.Empty };
        }

        /// <summary>
        /// Valida si el estado seleccionado es de resolución del expediente.
        /// </summary>
        public ErrorDto ValidaEstadoExpediente(int CodCliente, string estado, string categoria)
        {
            const string sql = @"
                SELECT COUNT(*) FROM [AFI_BENE_ESTADOS]
                WHERE COD_ESTADO IN (SELECT COD_ESTADO FROM [AFI_BENE_GRUPO_ESTADOS] WHERE COD_GRUPO IN (
                    SELECT COD_GRUPO FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = @categoria))
                  AND P_FINALIZA = 1 AND COD_ESTADO = @estado AND PROCESO IN ('A', 'D')";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<int>(sql, new { categoria, estado }));

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            return result.Result == 0
                ? new ErrorDto { Code = -1, Description = "El estado seleccionado no es de resolución del expediente" }
                : new ErrorDto { Code = 0, Description = string.Empty };
        }

        /// <summary>
        /// Valida si el beneficio requiere justificación ejecutando las validaciones configuradas.
        /// </summary>
        public ErrorDto ValidaRequiereJustificacion(int CodCliente, string cedula, string beneficio)
        {
            const string sqlValidaciones = @"
                SELECT * FROM AFI_BENE_VALIDACIONES abv WHERE COD_VAL IN (
                    SELECT COD_VAL FROM AFI_BENE_VALIDA_CATEGORIA
                    WHERE COD_CATEGORIA = (SELECT ab.COD_CATEGORIA FROM AFI_BENEFICIOS ab WHERE ab.COD_BENEFICIO = @beneficio)
                      AND ESTADO = 1 AND REGISTRO_JUSTIFICA = 1)";

            var cedulaSegura = NormalizarSeguro(cedula);
            var beneficioSeguro = NormalizarSeguro(beneficio);

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var validaciones = connection.Query<ValidacionRow>(sqlValidaciones, new { beneficio }).ToList();
                var mensajes = string.Empty;

                foreach (var validacion in validaciones)
                {
                    if (string.IsNullOrWhiteSpace(validacion.query_val))
                    {
                        continue;
                    }

                    var sql = validacion.query_val.Replace("@cedula", cedulaSegura).Replace("@cod_beneficio", beneficioSeguro);
                    var valor = connection.QueryFirstOrDefault<int>(sql);

                    if (valor == validacion.resultado_val)
                    {
                        mensajes += validacion.msj_val + "...\n";
                    }
                }

                return mensajes;
            });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            return new ErrorDto
            {
                Code = string.IsNullOrEmpty(result.Result) ? 0 : -1,
                Description = result.Result ?? string.Empty
            };
        }

        /// <summary>
        /// Obtiene el tipo de beneficio ('M' por defecto si no existe o es nulo).
        /// </summary>
        public ErrorDto<string> ValidaTipoBeneficio(int CodCliente, string? cod_beneficio)
        {
            if (cod_beneficio == null)
            {
                return new ErrorDto<string> { Code = 0, Description = "El código de beneficio no puede ser nulo", Result = "M" };
            }

            const string sql = "SELECT TIPO FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<string>(sql, new { codBeneficio = cod_beneficio }));

            return new ErrorDto<string>
            {
                Code = result.Code,
                Description = result.Description,
                Result = string.IsNullOrEmpty(result.Result) ? "M" : result.Result
            };
        }

        /// <summary>
        /// Valida si la persona indicada figura como fallecida (delegado al validador compartido).
        /// </summary>
        public ErrorDto ValidaFallecido(int CodCliente, string cedulafallecido)
            => _mBeneficiosDB.ValidaFallecido(CodCliente, cedulafallecido);
    }
}
