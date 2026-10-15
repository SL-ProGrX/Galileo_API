using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public sealed class FrmPreaEstudiov2ScoreCrediticioDB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaEstudiov2ScoreCrediticioDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<PreaScoreCrediticioHistorialDto> Consultar(
            int codEmpresa, string codPreanalisis, string cedula)
        {
            var respuesta = new ErrorDto<PreaScoreCrediticioHistorialDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new PreaScoreCrediticioHistorialDto()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                var parametros = new DynamicParameters();
                parametros.Add("Expediente", codPreanalisis, DbType.AnsiString, size: 20);
                parametros.Add("Cedula", cedula, DbType.AnsiString);

                var existe = connection.ExecuteScalar<int>(
                    """
                    SELECT COUNT(1) FROM CRD_PREA_PREANALISIS
                    WHERE COD_PREANALISIS = @Expediente AND CEDULA = @Cedula
                    """, parametros);
                if (existe == 0)
                {
                    respuesta.Code = -1;
                    respuesta.Description = "La identificación no corresponde al expediente seleccionado.";
                    return respuesta;
                }

                const string sql = """
                    SELECT m.Id AS id, m.CodPreanalisis AS cod_preanalisis,
                           m.TipoScore AS tipo_score, m.FechaRegistra AS fecha_registra,
                           m.UsuarioRegistra AS usuario_registra
                    FROM CrdPreaScoresCrediticiosMaestro m
                    INNER JOIN CRD_PREA_PREANALISIS p ON p.COD_PREANALISIS = m.CodPreanalisis
                    WHERE m.CodPreanalisis = @Expediente AND p.CEDULA = @Cedula AND m.TipoScore = 1
                    ORDER BY m.FechaRegistra DESC, m.Id DESC;

                    SELECT d.Id AS id, d.IdMaestro AS id_maestro, d.Modelo AS modelo,
                           d.Objetivo AS objetivo, d.ProbabilidadDefecto AS probabilidad_defecto,
                           d.DecisionBinaria AS decision_binaria, d.Veredicto AS veredicto,
                           d.CategoriaRiesgo AS categoria_riesgo, d.UmbralAplicado AS umbral_aplicado
                    FROM CrdPreaScoresCrediticiosDetalle d
                    INNER JOIN CrdPreaScoresCrediticiosMaestro m ON m.Id = d.IdMaestro
                    INNER JOIN CRD_PREA_PREANALISIS p ON p.COD_PREANALISIS = m.CodPreanalisis
                    WHERE m.CodPreanalisis = @Expediente AND p.CEDULA = @Cedula AND m.TipoScore = 1
                    ORDER BY d.Id;
                    """;
                using var resultados = connection.QueryMultiple(sql, parametros);
                var maestros = resultados.Read<PreaScoreCrediticioRegistroDto>().ToList();
                var detalles = resultados.Read<ScoreDetalleFila>().ToLookup(fila => fila.id_maestro);
                foreach (var maestro in maestros)
                {
                    maestro.detalles = detalles[maestro.id].Select(fila => new PreaScoreCrediticioDetalleDto
                    {
                        id = fila.id,
                        modelo = fila.modelo,
                        objetivo = fila.objetivo,
                        probabilidad_defecto = fila.probabilidad_defecto,
                        decision_binaria = fila.decision_binaria,
                        veredicto = fila.veredicto,
                        categoria_riesgo = fila.categoria_riesgo,
                        umbral_aplicado = fila.umbral_aplicado
                    }).ToList();
                }
                respuesta.Result.historial = maestros;
            }
            catch (Exception ex)
            {
                respuesta.Code = -1;
                respuesta.Description = ex.Message;
            }

            return respuesta;
        }

        public bool PerteneceExpediente(int codEmpresa, string expediente, string cedula)
        {
            using var connection = _portalDb.CreateConnection(codEmpresa);
            return connection.ExecuteScalar<int>(
                """
                SELECT COUNT(1) FROM CRD_PREA_PREANALISIS
                WHERE COD_PREANALISIS = @Expediente AND CEDULA = @Cedula
                """, new { Expediente = expediente, Cedula = cedula }) > 0;
        }

        public PreaScoreCrediticioRegistroDto Guardar(
            int codEmpresa, string expediente, string cedula, string usuario,
            PreaScoreCrediticioDetalleDto detalle)
        {
            using var connection = _portalDb.CreateConnection(codEmpresa);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var parametros = new DynamicParameters();
            parametros.Add("Expediente", expediente, DbType.AnsiString, size: 20);
            parametros.Add("Cedula", cedula, DbType.AnsiString);
            parametros.Add("Usuario", usuario, DbType.String, size: 30);
            var existe = connection.ExecuteScalar<int>(
                """
                SELECT COUNT(1) FROM CRD_PREA_PREANALISIS WITH (UPDLOCK, HOLDLOCK)
                WHERE COD_PREANALISIS = @Expediente AND CEDULA = @Cedula
                """, parametros, transaction);
            if (existe == 0)
                throw new InvalidOperationException("La identificación del expediente cambió durante la consulta del score.");

            var maestro = connection.QuerySingle<PreaScoreCrediticioRegistroDto>(
                """
                INSERT INTO CrdPreaScoresCrediticiosMaestro
                    (CodPreanalisis, TipoScore, FechaRegistra, UsuarioRegistra)
                OUTPUT INSERTED.Id AS id, INSERTED.CodPreanalisis AS cod_preanalisis,
                       INSERTED.TipoScore AS tipo_score, INSERTED.FechaRegistra AS fecha_registra,
                       INSERTED.UsuarioRegistra AS usuario_registra
                VALUES (@Expediente, 1, SYSDATETIME(), @Usuario);
                """, parametros, transaction);

            var datos = new DynamicParameters();
            datos.Add("IdMaestro", maestro.id, DbType.Int32);
            datos.Add("Modelo", detalle.modelo, DbType.AnsiString, size: 25);
            datos.Add("Objetivo", detalle.objetivo, DbType.AnsiString, size: 25);
            datos.Add("ProbabilidadDefecto", detalle.probabilidad_defecto, DbType.Decimal, precision: 18, scale: 6);
            datos.Add("DecisionBinaria", detalle.decision_binaria, DbType.Boolean);
            datos.Add("Veredicto", detalle.veredicto, DbType.AnsiString, size: 100);
            datos.Add("CategoriaRiesgo", detalle.categoria_riesgo, DbType.String, size: 30);
            datos.Add("UmbralAplicado", detalle.umbral_aplicado, DbType.Decimal, precision: 18, scale: 6);
            detalle.id = connection.QuerySingle<int>(
                """
                INSERT INTO CrdPreaScoresCrediticiosDetalle
                    (IdMaestro, Modelo, Objetivo, ProbabilidadDefecto, DecisionBinaria,
                     Veredicto, CategoriaRiesgo, UmbralAplicado)
                OUTPUT INSERTED.Id
                VALUES (@IdMaestro, @Modelo, @Objetivo, @ProbabilidadDefecto, @DecisionBinaria,
                        @Veredicto, @CategoriaRiesgo, @UmbralAplicado);
                """, datos, transaction);
            maestro.detalles.Add(detalle);
            transaction.Commit();
            return maestro;
        }

        private sealed class ScoreDetalleFila
        {
            public int id { get; set; }
            public int id_maestro { get; set; }
            public string modelo { get; set; } = string.Empty;
            public string? objetivo { get; set; }
            public decimal probabilidad_defecto { get; set; }
            public bool decision_binaria { get; set; }
            public string veredicto { get; set; } = string.Empty;
            public string categoria_riesgo { get; set; } = string.Empty;
            public decimal umbral_aplicado { get; set; }
        }
    }
}
