using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndConsultaDb
    {
        private readonly IConfiguration _config;
        private const string SqlConsultaContratos = @"
                    SELECT TOP (@Lineas)
                        O.Descripcion AS Operadora,
                        F.Cod_Operadora,
                        F.Cod_plan,
                        P.Descripcion,
                        F.Cod_Contrato,
                        F.Cedula,
                        S.Nombre
                    FROM dbo.Fnd_Contratos F
                    INNER JOIN dbo.Fnd_Operadoras O
                        ON F.Cod_operadora = O.Cod_operadora
                    INNER JOIN dbo.Fnd_planes P
                        ON F.Cod_plan = P.Cod_plan
                    INNER JOIN dbo.Socios S
                        ON F.Cedula = S.Cedula
                    WHERE F.Estado <> 'L'
                      AND dbo.fxFndColaboradorVisualiza(F.COD_OPERADORA, F.COD_PLAN, F.cedula, S.ESTADOACTUAL, @Usuario) = 1
                      AND (@CodOperadora IS NULL OR F.Cod_operadora = @CodOperadora)
                      AND (@CodPlan IS NULL OR F.Cod_Plan LIKE @CodPlan)
                      AND (@CodContrato IS NULL OR F.Cod_Contrato = @CodContrato)
                      AND (@Cedula IS NULL OR F.Cedula LIKE @Cedula)
                      AND (@Nombre IS NULL OR S.Nombre LIKE @Nombre)
                    ORDER BY F.Cod_Operadora, F.Cod_plan, F.Cod_Contrato;";

        public FrmFndConsultaDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtner consulta de movimientos a contratos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<FndConsultaDto>> FND_Consulta_Obtener(int CodEmpresa, FndConsultaFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de consulta son requeridos.",
                    -2,
                    new List<FndConsultaDto>());
            }

            return DbHelper.ExecuteListQuery<FndConsultaDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlConsultaContratos,
                CrearParametrosConsulta(filtros));
        }

        /// <summary>
        /// Obtener lista de operadoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Consulta_Operadora_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        cod_operadora AS item,
                        descripcion
                    FROM dbo.fnd_Operadoras
                    ORDER BY descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, query);
        }

        /// <summary>
        /// Obtener lista de planes 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Operadora"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Consulta_Planes_Obtener(int CodEmpresa, int? Operadora)
        {
            const string query = @"
                    SELECT
                        cod_plan AS item,
                        descripcion
                    FROM dbo.fnd_Planes
                    WHERE @Operadora IS NULL OR Cod_operadora = @Operadora
                    ORDER BY cod_plan;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new { Operadora = NormalizarOperadora(Operadora) });
        }


        private static object CrearParametrosConsulta(FndConsultaFiltros filtros)
        {
            return new
            {
                Lineas = NormalizarLineas(filtros.lineas),
                Usuario = NormalizarTexto(filtros.usuario),
                CodOperadora = NormalizarEntero(filtros.cod_operadora),
                CodPlan = CrearFiltroPrefijo(filtros.cod_plan),
                CodContrato = NormalizarLong(filtros.cod_contrato),
                Cedula = CrearFiltroPrefijo(filtros.cedula),
                Nombre = CrearFiltroPrefijo(filtros.nombre)
            };
        }

        private static int NormalizarLineas(int? lineas)
        {
            var valor = lineas.GetValueOrDefault(100);
            return Math.Clamp(valor, 1, 1000);
        }

        private static int? NormalizarOperadora(int? operadora) => operadora.GetValueOrDefault() == 0 ? null : operadora;

        private static int? NormalizarEntero(int? valor) => valor.GetValueOrDefault() == 0 ? null : valor;

        private static long? NormalizarLong(long? valor) => valor.GetValueOrDefault() == 0 ? null : valor;

        private static string? CrearFiltroPrefijo(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return string.IsNullOrWhiteSpace(texto) ? null : $"{texto}%";
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
