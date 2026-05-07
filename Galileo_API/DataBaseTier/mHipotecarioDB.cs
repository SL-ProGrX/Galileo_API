using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier
{
    public class MHipotecarioDB
    {
        private readonly PortalDB _portalDb;

        public MHipotecarioDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de fincas asociadas a una operación o expediente.
        /// Replica sbFincas_Asociadas del VB6 usando spCrd_Fincas_Asociadas.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Operación o expediente.</param>
        /// <returns>Lista de fincas asociadas.</returns>
        public ErrorDto<List<FrmVivGarantiaFincaAsociadaItem>> sbFincas_Asociadas(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
            const string query = @"
                    CREATE TABLE #FincasAsociadas (
                        IdGarantia BIGINT NULL,
                        NumeroOperacion BIGINT NULL,
                        NumeroFinca VARCHAR(100) NULL,
                        NumPlanoCatastro VARCHAR(100) NULL,
                        Estado VARCHAR(20) NULL,
                        ValorTerreno DECIMAL(18, 2) NULL,
                        ValorConstruccion DECIMAL(18, 2) NULL,
                        AreaFinca DECIMAL(18, 2) NULL,
                        GradoHipoteca VARCHAR(50) NULL,
                        TipoPoliza VARCHAR(50) NULL,
                        Cedula VARCHAR(50) NULL,
                        Nombre VARCHAR(300) NULL,
                        Linea_Estado VARCHAR(50) NULL,
                        Saldo DECIMAL(18, 2) NULL,
                        Codigo VARCHAR(50) NULL,
                        Linea_Desc VARCHAR(300) NULL,
                        Poliza_Id BIGINT NULL,
                        Poliza_Cuota DECIMAL(18, 2) NULL,
                        Poliza_Estado VARCHAR(50) NULL,
                        Poliza_Codigo VARCHAR(50) NULL,
                        Poliza_Desc VARCHAR(300) NULL,
                        Tipo_Aplicacion VARCHAR(50) NULL
                    );

                    INSERT INTO #FincasAsociadas
                    EXEC spCrd_Fincas_Asociadas @operacion, @expediente;

                    SELECT
                        ISNULL(IdGarantia, 0) AS id_garantia,
                        ISNULL(NumeroOperacion, 0) AS numero_operacion,
                        RTRIM(ISNULL(NumeroFinca, '')) AS numero_finca,
                        RTRIM(ISNULL(NumPlanoCatastro, '')) AS num_plano_catastro,
                        RTRIM(ISNULL(Estado, '')) AS estado,
                        ISNULL(ValorTerreno, 0) AS valor_terreno,
                        ISNULL(ValorConstruccion, 0) AS valor_construccion,
                        ISNULL(AreaFinca, 0) AS area_finca,
                        RTRIM(ISNULL(GradoHipoteca, '')) AS grado_hipoteca,
                        RTRIM(ISNULL(TipoPoliza, '')) AS tipo_poliza,
                        RTRIM(ISNULL(Cedula, '')) AS cedula,
                        RTRIM(ISNULL(Nombre, '')) AS nombre,
                        RTRIM(ISNULL(Linea_Estado, '')) AS linea_estado,
                        ISNULL(Saldo, 0) AS saldo,
                        RTRIM(ISNULL(Codigo, '')) AS codigo,
                        RTRIM(ISNULL(Linea_Desc, '')) AS linea_desc,
                        ISNULL(Poliza_Id, 0) AS poliza_id,
                        ISNULL(Poliza_Cuota, 0) AS poliza_cuota,
                        RTRIM(ISNULL(Poliza_Estado, '')) AS poliza_estado,
                        RTRIM(ISNULL(Poliza_Codigo, '')) AS poliza_codigo,
                        RTRIM(ISNULL(Poliza_Desc, '')) AS poliza_desc,
                        RTRIM(ISNULL(Tipo_Aplicacion, '')) AS tipo_aplicacion
                    FROM #FincasAsociadas;

                    DROP TABLE #FincasAsociadas;";

            return DbHelper.ExecuteListQuery<FrmVivGarantiaFincaAsociadaItem>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    operacion = request.operacion,
                    expediente = request.expediente.Trim()
                }
            );
        }
    }
}
