using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCategoriasCreditoDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrCategoriasCreditoDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el catálogo de probabilidades
        /// de incumplimiento o default.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<
            CrCategoriasCreditoProbabilidadDefaultData>>
            CR_frmCR_Categorias_Credito_ProbabilidadDefault_Obtener(
                int codEmpresa)
        {
            const string sql = """
                select
                    ID_PROBABILIDAD_DEF
                        as id_probabilidad_def,
                    rtrim(DESCRIPCION)
                        as descripcion,
                    rtrim(CATEGORIA)
                        as categoria,
                    VALOR_INICIAL
                        as valor_inicial,
                    VALOR_FINAL
                        as valor_final,
                    rtrim(
                        isnull(
                            USUARIO_REGISTRA,
                            ''
                        )
                    ) as usuario_registra,
                    FEC_REGISTRA
                        as fec_registra,
                    rtrim(
                        isnull(
                            USUARIO_MODIFICA,
                            ''
                        )
                    ) as usuario_modifica,
                    FEC_MODIFICA
                        as fec_modifica
                from CRD_CATALOGO_PROBABILIDAD_DEF
                order by
                    ID_PROBABILIDAD_DEF
                """;

            return DbHelper.ExecuteListQuery<
                CrCategoriasCreditoProbabilidadDefaultData>(
                    _portalDb,
                    codEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene el catálogo de probabilidades de mora.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<
            CrCategoriasCreditoProbabilidadMoraData>>
            CR_frmCR_Categorias_Credito_ProbabilidadMora_Obtener(
                int codEmpresa)
        {
            const string sql = """
                select
                    ID_PROBABILIDAD_MORA
                        as id_probabilidad_mora,
                    rtrim(DESCRIPCION)
                        as descripcion,
                    rtrim(TIPO_MORA)
                        as tipo_mora,
                    PORC_PROBABILIDAD
                        as porc_probabilidad,
                    rtrim(
                        isnull(
                            USUARIO_REGISTRA,
                            ''
                        )
                    ) as usuario_registra,
                    FEC_REGISTRA
                        as fec_registra,
                    rtrim(
                        isnull(
                            USUARIO_MODIFICA,
                            ''
                        )
                    ) as usuario_modifica,
                    FEC_MODIFICA
                        as fec_modifica
                from CRD_CATALOGO_PROBABILIDAD_MORA
                order by
                    ID_PROBABILIDAD_MORA
                """;

            return DbHelper.ExecuteListQuery<
                CrCategoriasCreditoProbabilidadMoraData>(
                    _portalDb,
                    codEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene los segmentos configurados
        /// para probabilidad de crédito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<
            CrCategoriasCreditoSegmentoData>>
            CR_frmCR_Categorias_Credito_Segmentos_Obtener(
                int codEmpresa)
        {
            const string sql = """
                select
                    ID_SEGMENTO
                        as id_segmento,
                    rtrim(COD_SEGMENTO)
                        as cod_segmento,
                    rtrim(DESCRIPCION)
                        as descripcion,
                    PORC_SEGMENTO
                        as porc_segmento,
                    rtrim(
                        isnull(
                            USUARIO_REGISTRA,
                            ''
                        )
                    ) as usuario_registra,
                    FEC_REGISTRA
                        as fec_registra,
                    rtrim(
                        isnull(
                            USUARIO_MODIFICA,
                            ''
                        )
                    ) as usuario_modifica,
                    FEC_MODIFICA
                        as fec_modifica
                from CRD_SEGMENTOS_PROBABILIDAD
                order by
                    ID_SEGMENTO
                """;

            return DbHelper.ExecuteListQuery<
                CrCategoriasCreditoSegmentoData>(
                    _portalDb,
                    codEmpresa,
                    sql);
        }
    }
}