using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndExclusionMultaDb
    {
        private readonly IConfiguration _config;

        private const string SpExclusionMultasList = "spFnd_Exclusion_Multas_List";
        private const string SpExclusionMultasAdd = "spFnd_Exclusion_Multas_Add";

        private const string SqlOperadoras = @"
                    SELECT
                        RTRIM(descripcion) AS descripcion,
                        cod_operadora AS item
                    FROM dbo.FND_Operadoras
                    ORDER BY descripcion;";

        private const string SqlPlanes = @"
                    SELECT
                        cod_plan AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM dbo.fnd_planes
                    WHERE cod_operadora = @cod_operadora
                    ORDER BY descripcion;";

        private const string SqlContratos = @"
                    SELECT
                        cod_contrato,
                        cedula,
                        nombre
                    FROM dbo.vFnd_Contratos
                    WHERE Cod_Operadora = @cod_operadora
                      AND Cod_Plan = @cod_plan
                      AND Estado = 'A'
                    ORDER BY NOMBRE;";

        public FrmFndExclusionMultaDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene las operadoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Operadoras_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlOperadoras);
        }

        /// <summary>
        /// Obtiene los planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Planes_Obtener(int CodEmpresa, string cod_operadora)
        {
            if (string.IsNullOrWhiteSpace(cod_operadora))
            {
                return DbHelper.CreateErrorResponse(
                    "La operadora es requerida para buscar planes.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanes,
                new { cod_operadora = NormalizarTexto(cod_operadora) });
        }

        /// <summary>
        /// Busca contratos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_operadora"></param>
        /// <param name="cod_plan"></param>
        /// <returns></returns>
        public ErrorDto<List<FndContratoDto>> FND_Contratos_Obtener(int CodEmpresa, string cod_operadora, string cod_plan)
        {
            return DbHelper.ExecuteListQuery<FndContratoDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlContratos,
                new
                {
                    cod_operadora = NormalizarTexto(cod_operadora),
                    cod_plan = NormalizarTexto(cod_plan)
                });
        }

        /// <summary>
        /// Busca las multas excluidas segun los filtros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<FndExclusionMultaDto>> FND_Exclusion_Multas_List(int CodEmpresa, FiltrosBuscarExclusionDto filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de búsqueda son requeridos.",
                    -2,
                    new List<FndExclusionMultaDto>());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Query<FndExclusionMultaDto>(
                    SpExclusionMultasList,
                    CrearParametrosBusqueda(filtros),
                    commandType: CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<FndExclusionMultaDto>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<FndExclusionMultaDto>()
            };
        }


        /// <summary>
        /// Registra una nueva exclusion de multa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto FND_Exclusion_Multas_Add(int CodEmpresa, RegistrarExclusionDto request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de exclusión son requeridos.", -2);
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    SpExclusionMultasAdd,
                    CrearParametrosRegistro(request),
                    commandType: CommandType.StoredProcedure));

            return result.Code == 0
                ? DbHelper.OkResponse("Operación realizada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar exclusión de multa.", result.Code ?? -1);
        }


        private static DynamicParameters CrearParametrosBusqueda(FiltrosBuscarExclusionDto filtros)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Operadora", NormalizarTexto(filtros.cod_operadora), DbType.String);
            parameters.Add("@Plan", NormalizarTexto(filtros.cod_plan), DbType.String);
            parameters.Add("@Contrato", filtros.contrato, DbType.Int64);
            parameters.Add("@Cedula", NormalizarTexto(filtros.cedula), DbType.String);
            parameters.Add("@Nombre", NormalizarTexto(filtros.nombre), DbType.String);

            var rango = CrearRangoFechas(filtros);
            parameters.Add("@Inicio", rango.Inicio, DbType.DateTime);
            parameters.Add("@Corte", rango.Corte, DbType.DateTime);

            return parameters;
        }

        private static DynamicParameters CrearParametrosRegistro(RegistrarExclusionDto request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Operadora", NormalizarTexto(request.cod_operadora), DbType.String);
            parameters.Add("@Plan", NormalizarTexto(request.cod_plan), DbType.String);
            parameters.Add("@Contrato", request.cod_contrato, DbType.Int64);
            parameters.Add("@Cedula", NormalizarTexto(request.cedula), DbType.String);
            parameters.Add("@Excluye", request.excluye ? 1 : 0, DbType.Int32);
            parameters.Add("@Usuario", NormalizarTexto(request.usuario), DbType.String);
            return parameters;
        }

        private static RangoFechas CrearRangoFechas(FiltrosBuscarExclusionDto filtros)
        {
            return filtros.todas_fechas
                ? new RangoFechas(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTime.Now)
                : new RangoFechas(
                    (filtros.fecha_inicio ?? DateTime.MinValue).Date,
                    ((filtros.fecha_corte ?? DateTime.Now).Date.AddDays(1).AddTicks(-1))
                );
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private sealed record RangoFechas(DateTime Inicio, DateTime Corte);
    }

}
