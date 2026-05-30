using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier
{
    public class FrmAFPlanillaEnviaDB
    {
        private readonly IConfiguration _config;

        private const string SqlInstituciones = @"
                    SELECT cod_institucion AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM dbo.instituciones
                    WHERE activa = 1
                    ORDER BY descripcion;";

        private const string SqlPeriodosProceso = @"
                    ;WITH Periodos AS (
                        SELECT dbo.fxSIFPrmProcesoAnt(
                                   dbo.fxSIFPrmProcesoAnt(YEAR(dbo.MyGetdate()) * 100 + MONTH(dbo.MyGetdate()))
                               ) AS item,
                               0 AS Orden
                        UNION ALL
                        SELECT dbo.fxSIFPrmProcesoSig(item), Orden + 1
                        FROM Periodos
                        WHERE Orden < 6
                    )
                    SELECT item
                    FROM Periodos
                    ORDER BY item;";

        private const string SpArchivoObtener = "spPrm_Formato_PG_Soc";

        public FrmAFPlanillaEnviaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene las instituciones activas para envío de planillas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de instituciones activas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstituciones);
        }


        /// <summary>
        /// Obtiene los períodos de proceso disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de períodos de proceso.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PeriodosProceso_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPeriodosProceso);
        }


        /// <summary>
        /// Obtiene el archivo de planilla generado para una institución y período.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="codinstitucion">Código de institución.</param>
        /// <param name="fechaproceso">Fecha de proceso.</param>
        /// <returns>Resultado del archivo generado.</returns>
        public ErrorDto<List<AfArchivoResultadoDto>> AF_Archivo_Obtener(int CodEmpresa, string codinstitucion, string fechaproceso)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfArchivoResultadoDto>(
                    SpArchivoObtener,
                    new
                    {
                        CodInstitucion = NormalizarTexto(codinstitucion),
                        FechaProceso = NormalizarTexto(fechaproceso)
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());
        }
        

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}