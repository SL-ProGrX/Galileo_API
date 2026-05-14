using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndOperadorasDB
    {
        private readonly IConfiguration _config;

        private const string SqlOperadoraObtener = @"
                    SELECT
                        cod_operadora,
                        Descripcion,
                        Activa,
                        Notas,
                        Cta_Fondo,
                        Cta_Retiros,
                        Cta_Ingresos,
                        MULTA_MNT_TOPE
                    FROM dbo.vFnd_Operadoras
                    WHERE cod_Operadora = @CodOperadora;";

        private const string SqlOperadorasDropdown = @"
                    SELECT
                        cod_operadora AS item,
                        descripcion AS descripcion
                    FROM dbo.vFnd_Operadoras
                    ORDER BY item DESC;";

        private const string SqlInsertOperadora = @"
                    INSERT INTO dbo.Fnd_Operadoras
                    (
                        Descripcion,
                        Activa,
                        Notas,
                        Cta_Fondo,
                        Cta_Retiros,
                        Cta_Ingresos,
                        MULTA_MNT_TOPE
                    )
                    VALUES
                    (
                        @Descripcion,
                        @Activa,
                        @Notas,
                        @ctaplan,
                        @ctaret,
                        @ctaing,
                        @multa_mnt_tope
                    );

                    SELECT CAST(SCOPE_IDENTITY() AS int);";

        private const string SqlUpdateOperadora = @"
                    UPDATE dbo.FND_Operadoras
                    SET Descripcion = @Descripcion,
                        Cta_Fondo = @ctaplan,
                        Cta_Retiros = @ctaret,
                        Cta_Ingresos = @ctaing,
                        Notas = @Notas,
                        Activa = @Activa,
                        MULTA_MNT_TOPE = @multa_mnt_tope
                    WHERE cod_operadora = @cod_operadora;";

        private const string SqlOperadoraPlanes = @"
                    SELECT
                        Cod_Plan,
                        Plan_Desc,
                        Cod_Divisa,
                        Contratos,
                        Total * dbo.fxSys_Tipo_Cambio_Apl(Tipo_Cambio) AS TotalLocal,
                        Total AS TotalDivisa
                    FROM dbo.vFnd_Operadoras_Rsm
                    WHERE Cod_Operadora = @CodOperadora
                    ORDER BY Cod_Plan;";

        private const string SqlDeleteOperadora = @"
                    DELETE FROM dbo.FND_Operadoras
                    WHERE cod_operadora = @cod_operadora;";

        private const string SqlScrollSiguiente = @"
                    SELECT TOP 1 cod_operadora
                    FROM dbo.FND_OPERADORAS
                    WHERE cod_operadora > @operadora
                    ORDER BY cod_operadora ASC;";

        private const string SqlScrollAnterior = @"
                    SELECT TOP 1 cod_operadora
                    FROM dbo.FND_OPERADORAS
                    WHERE cod_operadora < @operadora
                    ORDER BY cod_operadora DESC;";

        public FrmFndOperadorasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la operadora
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_operadora"></param>
        /// <returns></returns>
        public ErrorDto<FndOperadoraDto> AF_Operadora_Obtener(int CodEmpresa, int cod_operadora)
        {
            var result = DbHelper.ExecuteSingleQuery<FndOperadoraDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlOperadoraObtener,
                default,
                new { CodOperadora = cod_operadora });

            return new ErrorDto<FndOperadoraDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndOperadoraDto()
            };
        }


        /// <summary>
        ///  Obtiene las operadoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Operadoras_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlOperadorasDropdown);
        }


        /// <summary>
        /// Guardar o actualizar la operadora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Operadora_Guardar(int codEmpresa, FndOperadoraDto request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de la operadora son requeridos.", -2);
            }

            if (request.cod_operadora == null)
            {
                var insertResult = DbHelper.ExecuteSingleQuery(
                    new PortalDB(_config),
                    codEmpresa,
                    SqlInsertOperadora,
                    0,
                    request);

                if (insertResult.Code != 0)
                {
                    return DbHelper.ErrorResponse(insertResult.Description ?? "Error al registrar operadora.", insertResult.Code.GetValueOrDefault(-1));
                }

                request.cod_operadora = insertResult.Result;
                return new ErrorDto
                {
                    Code = request.cod_operadora.GetValueOrDefault(),
                    Description = "Operadora registrada correctamente."
                };
            }

            var updateResult = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                codEmpresa,
                SqlUpdateOperadora,
                request);

            return updateResult.Code == 0
                ? DbHelper.OkResponse("Operadora actualizada correctamente.")
                : updateResult;
        }


        /// <summary>
        /// Obtiene planes por operadora
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_operadora"></param>
        /// <returns></returns>
        public ErrorDto<List<OperadoraPlanDto>> FND_OperadoraPlanes_Obtener(int CodEmpresa, int cod_operadora)
        {
            return DbHelper.ExecuteListQuery<OperadoraPlanDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlOperadoraPlanes,
                new { CodOperadora = cod_operadora });
        }

        /// <summary>
        /// Elimina la operadora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_operadora"></param>
        /// <returns></returns>
        public ErrorDto AF_Operadora_Eliminar(int codEmpresa, int cod_operadora)
        {
            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                codEmpresa,
                SqlDeleteOperadora,
                new { cod_operadora });
        }


        /// <summary>
        /// Scroll para la busqueda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="scrollCode"></param>
        /// <returns></returns>
        public ErrorDto<FndOperadoraDto> AF_Operadora_Scroll_Obtener(int CodEmpresa, int operadora, int scrollCode)
        {
            var codResult = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                scrollCode == 1 ? SqlScrollSiguiente : SqlScrollAnterior,
                0,
                new { operadora });

            if (codResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    codResult.Description ?? "Error al consultar la operadora.",
                    codResult.Code.GetValueOrDefault(-1),
                    new FndOperadoraDto());
            }

            if (codResult.Result == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró una operadora siguiente o anterior.",
                    -2,
                    new FndOperadoraDto());
            }

            return AF_Operadora_Obtener(CodEmpresa, codResult.Result);
        }
    }
}
