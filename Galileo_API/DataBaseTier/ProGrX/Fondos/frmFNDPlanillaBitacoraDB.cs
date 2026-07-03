using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo_API.DataBaseTier;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndPlanillaBitacoraDb
    {
        private readonly IConfiguration _config;

        private const string SqlBitacora = @"
                    SELECT
                        id_seq,
                        cod_institucion,
                        proceso,
                        cod_plan,
                        gestion,
                        transaccion,
                        documento,
                        usuario,
                        fecha,
                        casos,
                        monto
                    FROM dbo.fnd_prm_bitacora
                    WHERE cod_institucion = @CodInstitucion
                      AND proceso = @Proceso
                    ORDER BY id_seq;";

        private const string SqlInstituciones = @"
                    SELECT
                        cod_institucion AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM dbo.instituciones
                    ORDER BY descripcion;";

        private const string SqlOperadoras = @"
                    SELECT
                        cod_operadora AS item,
                        descripcion
                    FROM dbo.fnd_Operadoras
                    ORDER BY descripcion;";

        private const string SqlPlanes = @"
                    SELECT
                        cod_plan AS item,
                        descripcion
                    FROM dbo.fnd_planes
                    ORDER BY descripcion;";

        public FrmFndPlanillaBitacoraDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtener bitacora de planilla
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="Proceso"></param>
        /// <returns></returns>
        public ErrorDto<List<FndPrmBitacoraDto>> FND_PlanillaBitacora_Obtener(int CodEmpresa, int CodInstitucion, int Proceso)
        {
            var response = DbHelper.ExecuteListQuery<FndPrmBitacoraDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlBitacora,
                new
                {
                    CodInstitucion,
                    Proceso
                });

            if (response.Code == 0 && response.Result is not null)
            {
                NormalizarBitacora(response.Result);
            }

            return response;
        }

        /// <summary>
        /// Obtener instituciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaBitacora_Instituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlInstituciones);
        }

        /// <summary>
        /// Obtener operadoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaBitacora_Operadoras_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlOperadoras);
        }

        /// <summary>
        /// Obtener planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaBitacora_Planes_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanes);
        }

        /// <summary>
        /// Obtener proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Proceso"></param>
        /// <param name="Direccion"></param>
        /// <returns></returns>
        public ErrorDto<int> FND_PlanillaBitacora_Proceso_Obtener(int CodEmpresa, int Proceso, int Direccion)
        {
            decimal result;
            try
            {
                var mCobroDb = new MCobroDb(_config);
                result = Direccion == 1
                    ? mCobroDb.fxFechaProcesoSiguiente(CodEmpresa, Proceso)
                    : mCobroDb.fxFechaProcesoAnterior(CodEmpresa, Proceso);
                return new ErrorDto<int>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = (int)result
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<int>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = 0
                };
            }
        }

        private static void NormalizarBitacora(List<FndPrmBitacoraDto> bitacora)
        {
            foreach (var item in bitacora)
            {
                item.gestion = item.gestion == "R" ? "Recepción" : "Envio";
                item.transaccion = FxSIFPlanillaTipoTransac(item.transaccion ?? string.Empty);
            }
        }

        /// <summary>
        /// Obtener descripcion de la transaccion
        /// </summary>
        /// <param name="pTransaccion"></param>
        /// <returns></returns>
        public static string FxSIFPlanillaTipoTransac(string pTransaccion)
        {
            return (pTransaccion ?? string.Empty).Trim() switch
            {
                "01" => "Cambia Fecha de Proceso",
                "02" => "Genera deducciones",
                "03" => "Carga deducciones",
                "04" => "Desglosa deducciones",
                _ => "No.Identificado"
            };
        }

    }
}
