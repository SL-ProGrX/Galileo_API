using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaEstadoPreanalisisDB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaEstadoPreanalisisDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Carga el estado actual, linea, validaciones de autorizadores y causas del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstadoPreanalisisCargarResponse> Prea_frmPreaEstadoPreanalisis_Cargar(
            int codEmpresa,
            string usuario,
            string cod_preanalisis,
            string tipo)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var response = ObtenerEncabezado(connection, cod_preanalisis);
                if (string.IsNullOrWhiteSpace(response.cod_preanalisis))
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontro el expediente indicado.",
                        -1,
                        new FrmPreaEstadoPreanalisisCargarResponse());
                }

                var tipoCausas = string.IsNullOrWhiteSpace(tipo) ? response.estado : tipo.Trim().ToUpperInvariant();
                response.causas = ObtenerCausas(connection, response.cod_preanalisis, tipoCausas, response.cod_linea);

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new FrmPreaEstadoPreanalisisCargarResponse());
            }
        }

        /// <summary>
        /// Actualiza el estado del preanalisis, aplica la validacion de resolucion e inserta el tag del movimiento.
        /// </summary>
        public ErrorDto<FrmPreaEstadoPreanalisisGuardarResponse> Prea_frmPreaEstadoPreanalisis_Guardar(
            int codEmpresa,
            FrmPreaEstadoPreanalisisGuardarRequest request)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                var expediente = request.cod_preanalisis.Trim();
                var estado = request.estado.Trim().ToUpperInvariant();
                var estadoV2 = ObtenerEstadoV2(estado);

                if (estado is "A" or "D")
                {
                    var mensajeValidacion = ValidarResolucion(connection, transaction, expediente, request.usuario);
                    if (!string.IsNullOrWhiteSpace(mensajeValidacion))
                    {
                        transaction.Rollback();
                        return DbHelper.CreateErrorResponse(
                            mensajeValidacion,
                            -1,
                            new FrmPreaEstadoPreanalisisGuardarResponse());
                    }
                }

                const string updateSql = @"
UPDATE CRD_PREA_PREANALISIS
SET ESTADO = @estado,
    COD_ESTADO_V2 = @estado_v2,
    USUARIO_GESTION = @usuario,
    FECHA_GESTION = dbo.MyGetdate()
WHERE COD_PREANALISIS = @cod_preanalisis
   OR COD_PREANALISIS_REF = @cod_preanalisis;";

                connection.Execute(
                    updateSql,
                    new
                    {
                        estado,
                        estado_v2 = estadoV2,
                        usuario = request.usuario.Trim(),
                        cod_preanalisis = expediente
                    },
                    transaction,
                    commandType: CommandType.Text);

                InsertarTag(connection, transaction, expediente, estado, request.usuario);
                transaction.Commit();

                return DbHelper.CreateOkResponse(new FrmPreaEstadoPreanalisisGuardarResponse
                {
                    cod_preanalisis = expediente,
                    estado = estado,
                    estado_desc = ObtenerEstadoDescripcion(estado),
                    mensaje = "La informacion fue actualizada correctamente."
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new FrmPreaEstadoPreanalisisGuardarResponse());
            }
        }

        /// <summary>
        /// Registra o elimina una causa de gestion para el estado seleccionado.
        /// </summary>
        public ErrorDto<FrmPreaEstadoPreanalisisCausaRegistrarResponse> Prea_frmPreaEstadoPreanalisis_Causa_Registrar(
            int codEmpresa,
            FrmPreaEstadoPreanalisisCausaRegistrarRequest request)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                if (request.activo == true)
                {
                    const string insertSql = @"
IF NOT EXISTS (
    SELECT 1
    FROM CRD_PREA_GESTION
    WHERE COD_CAUSAS = @cod_causas
      AND TIPO = @tipo
      AND COD_PREANALISIS = @cod_preanalisis
      AND CODIGO = @codigo
)
BEGIN
    INSERT INTO CRD_PREA_GESTION
    (
        COD_CAUSAS,
        TIPO,
        COD_PREANALISIS,
        CODIGO,
        REGISTRO_FECHA,
        REGISTRO_USUARIO
    )
    VALUES
    (
        @cod_causas,
        @tipo,
        @cod_preanalisis,
        @codigo,
        dbo.MyGetdate(),
        @usuario
    );
END;";

                    connection.Execute(insertSql, CrearParametrosCausa(request), commandType: CommandType.Text);
                }
                else
                {
                    const string deleteSql = @"
DELETE FROM CRD_PREA_GESTION
WHERE COD_CAUSAS = @cod_causas
  AND TIPO = @tipo
  AND COD_PREANALISIS = @cod_preanalisis
  AND CODIGO = @codigo;";

                    connection.Execute(deleteSql, CrearParametrosCausa(request), commandType: CommandType.Text);
                }

                return DbHelper.CreateOkResponse(new FrmPreaEstadoPreanalisisCausaRegistrarResponse
                {
                    cod_causas = request.cod_causas.Trim(),
                    activo = request.activo == true,
                    mensaje = request.activo == true ? "Causa registrada correctamente." : "Causa eliminada correctamente."
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new FrmPreaEstadoPreanalisisCausaRegistrarResponse());
            }
        }

        private static FrmPreaEstadoPreanalisisCargarResponse ObtenerEncabezado(IDbConnection connection, string codPreanalisis)
        {
            const string sql = @"
SELECT TOP 1
    P.COD_PREANALISIS AS cod_preanalisis,
    ISNULL(P.COD_PREANALISIS_REF, '') AS cod_preanalisis_ref,
    ISNULL(P.ESTADO, 'R') AS estado,
    CASE ISNULL(P.ESTADO, 'R')
        WHEN 'P' THEN 'Pendiente'
        WHEN 'A' THEN 'Aprobado'
        WHEN 'D' THEN 'Denegado'
        WHEN 'B' THEN 'Abandonado'
        ELSE 'Recibido'
    END AS estado_desc,
    ISNULL(P.COD_ESTADO_V2, '') AS cod_estado_v2,
    ISNULL(P.COD_LINEA, '') AS cod_linea,
    CASE WHEN ISNULL(A.cantidad, 0) > 0 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS requiere_autorizadores,
    CASE WHEN ISNULL(PA.cantidad, 0) > 0 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS autorizadores_marcados
FROM CRD_PREA_PREANALISIS P
OUTER APPLY (
    SELECT COUNT(*) AS cantidad
    FROM CRD_COMITES_AUTORIZADORES CA
    WHERE CA.ID_COMITE = ISNULL(P.ID_COMITE, 0)
) A
OUTER APPLY (
    SELECT COUNT(*) AS cantidad
    FROM CRD_PREA_AUTORIZADORES PRA
    WHERE PRA.COD_PREANALISIS = P.COD_PREANALISIS
) PA
WHERE P.COD_PREANALISIS = @cod_preanalisis
   OR P.COD_PREANALISIS_REF = @cod_preanalisis;";

            return connection.QueryFirstOrDefault<FrmPreaEstadoPreanalisisCargarResponse>(
                sql,
                new { cod_preanalisis = codPreanalisis.Trim() },
                commandType: CommandType.Text) ?? new FrmPreaEstadoPreanalisisCargarResponse();
        }

        private static List<FrmPreaEstadoPreanalisisCausaDto> ObtenerCausas(
            IDbConnection connection,
            string codPreanalisis,
            string estado,
            string codigo)
        {
            if (estado is not ("P" or "D"))
            {
                return [];
            }

            const string sql = @"
SELECT
    Cg.COD_CAUSAS AS cod_causas,
    Cg.DESCRIPCION AS descripcion,
    CASE WHEN ISNULL(Pa.COD_CAUSAS, '') = '' THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS activo,
    ISNULL(CONVERT(varchar(19), Pa.REGISTRO_FECHA, 120), '') AS registro_fecha,
    ISNULL(Pa.REGISTRO_USUARIO, '') AS registro_usuario
FROM OPERACION_CAUSAS Cg
LEFT JOIN CRD_PREA_GESTION Pa
    ON Cg.COD_CAUSAS = Pa.COD_CAUSAS
    AND Cg.TIPO = Pa.TIPO
    AND Pa.COD_PREANALISIS = @cod_preanalisis
    AND Pa.CODIGO = @codigo
WHERE Cg.TIPO = @tipo
ORDER BY ISNULL(Pa.REGISTRO_FECHA, GETDATE()) ASC, Cg.COD_CAUSAS;";

            return connection.Query<FrmPreaEstadoPreanalisisCausaDto>(
                sql,
                new
                {
                    cod_preanalisis = codPreanalisis.Trim(),
                    tipo = estado.Trim(),
                    codigo = codigo.Trim()
                },
                commandType: CommandType.Text).ToList();
        }

        private static string ValidarResolucion(
            IDbConnection connection,
            IDbTransaction transaction,
            string codPreanalisis,
            string usuario)
        {
            const string sql = @"
SELECT dbo.fxCrd_Comites_Valida_Resolucion(ID_COMITE, COD_LINEA, GARANTIA, MONTO, @usuario) AS mensaje
FROM CRD_PREA_PREANALISIS
WHERE COD_PREANALISIS = @cod_preanalisis;";

            return connection.QueryFirstOrDefault<string>(
                sql,
                new
                {
                    cod_preanalisis = codPreanalisis.Trim(),
                    usuario = usuario.Trim()
                },
                transaction,
                commandType: CommandType.Text) ?? string.Empty;
        }

        private static void InsertarTag(
            IDbConnection connection,
            IDbTransaction transaction,
            string codPreanalisis,
            string estado,
            string usuario)
        {
            var tag = ObtenerTag(connection, transaction, estado);
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            const string existeTagSql = "SELECT COUNT(*) FROM CRD_TAGS WHERE TAG_CODIGO = @tag;";
            var existeTag = connection.QueryFirstOrDefault<int>(
                existeTagSql,
                new { tag },
                transaction,
                commandType: CommandType.Text);

            if (existeTag <= 0)
            {
                return;
            }

            const string insertSql = @"
DECLARE @lineaTag int;
DECLARE @codLinea varchar(50);

SELECT @lineaTag = ISNULL(MAX(LINEA), 0) + 1
FROM CRD_PREA_TAGS
WHERE COD_PREANALISIS = @cod_preanalisis;

SELECT @codLinea = ISNULL(COD_LINEA, '')
FROM CRD_PREA_PREANALISIS
WHERE COD_PREANALISIS = @cod_preanalisis;

INSERT INTO CRD_PREA_TAGS
(
    LINEA,
    CODIGO,
    COD_PREANALISIS,
    TAG_CODIGO,
    ASIGNADO_A,
    REGISTRO_FECHA,
    REGISTRO_USUARIO,
    NOTAS
)
VALUES
(
    @lineaTag,
    @codLinea,
    @cod_preanalisis,
    @tag,
    '',
    dbo.MyGetdate(),
    @usuario,
    @nota
);";

            connection.Execute(
                insertSql,
                new
                {
                    cod_preanalisis = codPreanalisis.Trim(),
                    tag,
                    usuario = usuario.Trim(),
                    nota = ObtenerNotaTag(estado, usuario)
                },
                transaction,
                commandType: CommandType.Text);
        }

        private static string ObtenerTag(IDbConnection connection, IDbTransaction transaction, string estado)
        {
            var codParametro = estado switch
            {
                "A" => "01",
                "D" => "02",
                "P" or "R" => string.Empty,
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(codParametro))
            {
                return "S16";
            }

            const string sql = @"
SELECT ISNULL(VALOR, '') AS valor
FROM CRD_COMITES_PARAMETROS
WHERE COD_PARAMETRO = @cod_parametro;";

            return connection.QueryFirstOrDefault<string>(
                sql,
                new { cod_parametro = codParametro },
                transaction,
                commandType: CommandType.Text) ?? string.Empty;
        }

        private static object CrearParametrosCausa(FrmPreaEstadoPreanalisisCausaRegistrarRequest request)
            => new
            {
                usuario = request.usuario.Trim(),
                cod_preanalisis = request.cod_preanalisis.Trim(),
                tipo = request.tipo.Trim().ToUpperInvariant(),
                codigo = request.codigo.Trim(),
                cod_causas = request.cod_causas.Trim()
            };

        private static string ObtenerEstadoV2(string estado)
            => estado switch
            {
                "P" => "PEND",
                "A" => "APRO",
                "D" => "DESC",
                _ => "RECI"
            };

        private static string ObtenerEstadoDescripcion(string estado)
            => estado switch
            {
                "P" => "Pendiente",
                "A" => "Aprobado",
                "D" => "Denegado",
                _ => "Recibido"
            };

        private static string ObtenerNotaTag(string estado, string usuario)
            => estado switch
            {
                "P" => $"{usuario.Trim()} cambio de estado del estudio crediticio a pendiente",
                "R" => $"{usuario.Trim()} cambio de estado del estudio crediticio a recibido",
                _ => $"{usuario.Trim()} operacion realizada de estudio crediticio"
            };
    }
}
