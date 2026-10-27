using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;
using System.Data;

namespace Galileo_API.DataBaseTier
{
    public class ClsAgregarBD
    {
        private readonly PortalDB _portalDB;

        public ClsAgregarBD(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Guarda el registro de avalúo definitivo de una garantía.
        /// Replica ObjAgregar.fxRegistroAvaluo de VB6 usando spCRDVivGarantiaAvaluo_A.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa sobre la que se registra el avalúo.</param>
        /// <param name="request">Datos definitivos del avalúo y del ingeniero responsable.</param>
        /// <returns>Resultado de la ejecución del procedimiento de registro.</returns>
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

            const string sql = """
                EXEC dbo.spCRDVivGarantiaAvaluo_A
                    @IdGarantia,
                    @IdContacto,
                    @FechaInspeccion,
                    @ValorTerreno,
                    @ValorConstruccion,
                    @ObservacionesAvaluo,
                    @RegistroUsuario,
                    @RegistroFecha,
                    @Viaticos,
                    @Tipo_Poliza;
                """;

            return DbHelper.ExecuteNonQuery(_portalDB, codEmpresa, sql, parametros);
        }
    }
}
