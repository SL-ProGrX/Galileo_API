using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;
using System.Data;

namespace Galileo_API.DataBaseTier
{
    public class clsAgregarBD
    {
        private readonly PortalDB _portalDB;

        public clsAgregarBD(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Guarda el registro de avalúo definitivo de una garantía.
        /// Replica ObjAgregar.fxRegistroAvaluo de VB6 usando spCRDVivGarantiaAvaluo_A.
        /// </summary>
        public ErrorDto fxRegistroAvaluo(
            int codEmpresa,
            FrmVivGarantiaAvaluoPosteriorRequest request)
        {
            var parametros = new DynamicParameters();

            parametros.Add("@IdGarantia", request.id_garantia, DbType.Int64);
            parametros.Add("@IdContacto", request.id_ingeniero, DbType.Int64);
            parametros.Add(
                "@FechaInspeccion",
                request.fecha_inspeccion?.ToString("yyyy-MM-dd"),
                DbType.String
            );
            parametros.Add("@ValorTerreno", request.valor_terreno, DbType.Decimal);
            parametros.Add("@ValorConstruccion", request.valor_construccion, DbType.Decimal);
            parametros.Add(
                "@ObservacionesAvaluo",
                string.IsNullOrWhiteSpace(request.observaciones_avaluo) ? null : request.observaciones_avaluo.Trim(),
                DbType.String
            );
            parametros.Add("@RegistroUsuario", request.registro_usuario?.Trim() ?? string.Empty, DbType.String);
            parametros.Add(
                "@RegistroFecha",
                string.IsNullOrWhiteSpace(request.registro_fecha.ToString()) || request.registro_fecha?.ToString("yyyy-MM-dd") == "1900/01/01"
                    ? null
                    : request.registro_fecha?.ToString("yyyy-MM-dd"),
                DbType.String
            );
            parametros.Add("@Viaticos", request.viaticos, DbType.Decimal);
            parametros.Add("@Tipo_Poliza", request.tipo_poliza.Trim(), DbType.String);
            var connectionString = _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);
            return DbHelper.ExecuteStoredProcedureSingle<ErrorDto>(
              connectionString,
              "dbo.spCRDVivGarantiaAvaluo_A",
              default,
              parametros
          ).Result ?? new ErrorDto { Code = -1, Description = "Error desconocido." };

        }
    }
}
