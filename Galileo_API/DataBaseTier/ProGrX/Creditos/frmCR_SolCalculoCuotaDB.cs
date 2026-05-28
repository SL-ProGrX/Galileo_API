using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSolCalculoCuotaDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrSolCalculoCuotaDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la fecha del servidor y el catalogo de factores de calculo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CrSolCalculoCuotaPantallaData> CrSolCalculoCuota_Pantalla_Obtener(int codEmpresa)
        {
            const string sql = @"select getdate() as fecha_servidor;";

            var fechaResp = DbHelper.ExecuteSingleQuery<CrSolCalculoCuotaPantallaData>(
                _portalDb,
                codEmpresa,
                sql);

            if (fechaResp.Code < 0)
            {
                return DbHelper.CreateErrorResponse<CrSolCalculoCuotaPantallaData>(
                    $"Error al obtener la fecha del servidor. {fechaResp.Description}");
            }

            if (fechaResp.Result == null)
            {
                return DbHelper.CreateErrorResponse<CrSolCalculoCuotaPantallaData>(
                    "No se pudo obtener la fecha del servidor.");
            }

            fechaResp.Result.factor_default = "01";
            fechaResp.Result.factores = ObtenerFactoresCalculo();

            return new ErrorDto<CrSolCalculoCuotaPantallaData>
            {
                Code = 0,
                Description = "Ok",
                Result = fechaResp.Result
            };
        }

        /// <summary>
        /// Obtiene el codigo o descripcion equivalente del factor de calculo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSolCalculoCuotaFactorData> CrSolCalculoCuota_Factor_Obtener(
            int codEmpresa,
            CrSolCalculoCuotaFactorRequest request)
        {
            var valor = ObtenerFactorCalculo(request?.dato ?? string.Empty);

            return new ErrorDto<CrSolCalculoCuotaFactorData>
            {
                Code = 0,
                Description = "Ok",
                Result = new CrSolCalculoCuotaFactorData
                {
                    valor = valor
                }
            };
        }

        /// <summary>
        /// Calcula una cuota segun monto, plazo, interes y frecuencia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSolCalculoCuotaCalcularCuotaData> CrSolCalculoCuota_Cuota_Calcular(
            int codEmpresa,
            CrSolCalculoCuotaCalcularCuotaRequest request)
        {
            var cuota = CalcularCuota(
                request?.monto ?? 0,
                request?.plazo ?? 0,
                request?.interes ?? 0,
                string.IsNullOrWhiteSpace(request?.frecuencia) ? "M" : request.frecuencia);

            return new ErrorDto<CrSolCalculoCuotaCalcularCuotaData>
            {
                Code = 0,
                Description = "Ok",
                Result = new CrSolCalculoCuotaCalcularCuotaData
                {
                    cuota = cuota
                }
            };
        }

        /// <summary>
        /// Calcula la cuota para el esquema actual nivelada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSolCalculoCuotaNiveladaData> CrSolCalculoCuota_Nivelada_Calcular(
            int codEmpresa,
            CrSolCalculoCuotaNiveladaRequest request)
        {
            var cuota = CalcularCuotaNivelada(
                request?.saldo ?? 0,
                request?.plazo ?? 0,
                request?.tasa ?? 0,
                request?.fecha_inicio ?? DateTime.Now);

            return new ErrorDto<CrSolCalculoCuotaNiveladaData>
            {
                Code = 0,
                Description = "Ok",
                Result = new CrSolCalculoCuotaNiveladaData
                {
                    cuota = cuota
                }
            };
        }

        /// <summary>
        /// Obtiene los dias del mes y año indicados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSolCalculoCuotaDiasMesData> CrSolCalculoCuota_DiasMes_Obtener(
            int codEmpresa,
            CrSolCalculoCuotaDiasMesRequest request)
        {
            var dias = ObtenerDiasMes(
                request?.mes ?? 0,
                request?.anio ?? 0);

            return new ErrorDto<CrSolCalculoCuotaDiasMesData>
            {
                Code = 0,
                Description = "Ok",
                Result = new CrSolCalculoCuotaDiasMesData
                {
                    dias = dias
                }
            };
        }

        private List<DropDownListaGenericaModel> ObtenerFactoresCalculo()
            => MCredito.SbCrdFactorCalculo();

        private string ObtenerFactorCalculo(string dato)
            => MCredito.FxCrdFactorCalculo(dato);

        private decimal CalcularCuota(decimal monto, int plazo, decimal interes, string frecuencia = "M")
            => MCobroDb.fxCalcula_Cuota(monto, plazo, interes, frecuencia);

        private decimal CalcularCuotaNivelada(decimal saldo, int plazo, decimal tasa, DateTime fechaInicio)
            => MCredito.FxCrdCuotaNivelada(saldo, plazo, tasa, fechaInicio);

        private int ObtenerDiasMes(int mes, int anio)
            => MCredito.fxMesDias(mes, anio);
    }
}