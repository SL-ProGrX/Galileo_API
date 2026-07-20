using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXEsquemasDB
    {
        private readonly PortalDB _portalDB;

        public FrmCntXEsquemasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las contabilidades disponibles con los niveles que definen su estructura.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa cuya base de datos se consulta.</param>
        /// <returns>Lista de contabilidades o el detalle del error producido.</returns>
        public ErrorDto<List<ContabilidadDto>> ObtenerContabilidades(int codEmpresa)
        {
            const string sql = """
                SELECT
                    COD_CONTABILIDAD AS cod_contabilidad,
                    NOMBRE AS nombre,
                    Nivel1,
                    Nivel2,
                    Nivel3,
                    Nivel4,
                    Nivel5,
                    Nivel6,
                    Nivel7,
                    Nivel8
                FROM CntX_Contabilidades
                """;

            return DbHelper.ExecuteListQuery<ContabilidadDto>(_portalDB, codEmpresa, sql);
        }

        /// <summary>
        /// Valida que fuente y destino tengan la misma estructura y copia el esquema contable.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa donde se ejecuta el proceso.</param>
        /// <param name="codFuente">Código de la contabilidad fuente.</param>
        /// <param name="codDestino">Código de la contabilidad destino.</param>
        /// <param name="inicializa">Indica si se inicializa la configuración del destino.</param>
        /// <param name="usuario">Usuario que solicita la copia.</param>
        /// <returns>Resultado de la copia o el detalle de la validación o error.</returns>
        public ErrorDto Copiar(
            int codEmpresa,
            int codFuente,
            int codDestino,
            bool inicializa,
            string usuario
        )
        {
            if (codFuente == codDestino)
            {
                return DbHelper.ErrorResponse("El origen y destino son el mismo.", -2);
            }

            const string estructuraSql = """
                SELECT
                    COD_CONTABILIDAD AS cod_contabilidad,
                    NOMBRE AS nombre,
                    Nivel1,
                    Nivel2,
                    Nivel3,
                    Nivel4,
                    Nivel5,
                    Nivel6,
                    Nivel7,
                    Nivel8
                FROM CntX_Contabilidades
                WHERE COD_CONTABILIDAD IN @Codigos
                """;

            var estructuras = DbHelper.ExecuteListQuery<ContabilidadDto>(
                _portalDB,
                codEmpresa,
                estructuraSql,
                new { Codigos = new[] { codFuente, codDestino } }
            );

            if (estructuras.Code < 0)
            {
                return DbHelper.ErrorResponse(
                    estructuras.Description ?? "No fue posible validar las contabilidades."
                );
            }

            var fuente = estructuras.Result?.FirstOrDefault(x => x.cod_contabilidad == codFuente);
            var destino = estructuras.Result?.FirstOrDefault(x => x.cod_contabilidad == codDestino);

            if (fuente is null || destino is null)
            {
                return DbHelper.ErrorResponse(
                    "No se encontró la contabilidad fuente o destino.",
                    -2
                );
            }

            if (!TieneMismaEstructura(fuente, destino))
            {
                return DbHelper.ErrorResponse(
                    "La estructura de la cuenta contable no es la misma en el Fuente-Destino.",
                    -2
                );
            }

            const string copiarSql = """
                EXEC spCntX_Util_Contabilidad_Copia
                    @CodFuente,
                    @CodDestino,
                    @Inicializa,
                    @Usuario,
                    @Token
                """;

            return DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                copiarSql,
                new
                {
                    CodFuente = codFuente,
                    CodDestino = codDestino,
                    Inicializa = inicializa ? 1 : 0,
                    Usuario = usuario,
                    Token = "*xHM1tOk3n$"
                }
            );
        }

        /// <summary>
        /// Compara los ocho niveles que componen la estructura contable.
        /// </summary>
        /// <param name="fuente">Contabilidad utilizada como fuente.</param>
        /// <param name="destino">Contabilidad que recibirá el esquema.</param>
        /// <returns><see langword="true"/> cuando todos los niveles coinciden.</returns>
        private static bool TieneMismaEstructura(ContabilidadDto fuente, ContabilidadDto destino)
        {
            return fuente.nivel1 == destino.nivel1
                && fuente.nivel2 == destino.nivel2
                && fuente.nivel3 == destino.nivel3
                && fuente.nivel4 == destino.nivel4
                && fuente.nivel5 == destino.nivel5
                && fuente.nivel6 == destino.nivel6
                && fuente.nivel7 == destino.nivel7
                && fuente.nivel8 == destino.nivel8;
        }
    }
}
