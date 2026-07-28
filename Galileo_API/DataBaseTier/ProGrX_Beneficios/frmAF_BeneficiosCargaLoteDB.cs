using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de la Carga por Lote de Beneficios (frmAF_BeneficiosCargaLote).
    /// </summary>
    public partial class FrmAfBeneficiosCargaLoteDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosCargaLoteDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Inserta un lote de beneficios ejecutando el SP por cada registro válido.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="beneficio">JSON con la lista de registros a cargar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Beneficio_Lote_Carga_Insertar(int CodEmpresa, string beneficio)
        {
            var lote = JsonConvert.DeserializeObject<List<BeneficioExcelData>>(beneficio) ?? new List<BeneficioExcelData>();

            var validacion = ValidarLote(lote);
            if (validacion != null)
            {
                return validacion;
            }

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                foreach (var item in lote)
                {
                    connection.Execute("[spBeneficio_W_Lote_Carga]", new
                    {
                        Codigo = item.cod_beneficio,
                        Cedula = item.cedula,
                        Nombre = item.nombre,
                        Monto = item.monto,
                        Usuario = item.usuario,
                        Beneficiario_Id = item.beneficiario_id,
                        Beneficiario_Nombre = item.beneficiario_nombre,
                        Beneficiario_IBAN = item.beneficiario_iban,
                        Inicializa = item.inicializa
                    }, commandType: CommandType.StoredProcedure);
                }

                return DbHelper.OkResponse("Lote cargado exitosamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Valida que los registros del lote tengan código y cédula.
        /// </summary>
        /// <param name="lote">Lista de registros a validar.</param>
        /// <returns>Error si hay inconsistencia; null si es válido.</returns>
        private static ErrorDto? ValidarLote(List<BeneficioExcelData> lote)
        {
            if (lote.Any(item => string.IsNullOrEmpty(item.cod_beneficio)))
            {
                return DbHelper.ErrorResponse("El campo Codigo no puede estar vacio");
            }

            if (lote.Any(item => string.IsNullOrEmpty(item.cedula)))
            {
                return DbHelper.ErrorResponse("El campo Cedula no puede estar vacio");
            }

            return null;
        }

        /// <summary>
        /// Obtiene la revisión del lote cargado desde el SP.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <param name="usuario">Usuario que realizó la carga.</param>
        /// <returns>Lista de registros del lote.</returns>
        public ErrorDto<List<AfiBeneCargaLoteData>> Beneficio_Lote_Revisa_Obtener(int CodEmpresa, string cod_beneficio, string usuario)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfiBeneCargaLoteData>("[spBeneficio_Lote_Revisa]",
                    new { Codigo = cod_beneficio, usuario },
                    commandType: CommandType.StoredProcedure).ToList());
        }

        /// <summary>
        /// Procesa el lote de beneficios mediante el SP con el formato indicado.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <param name="usuario">Usuario que procesa el lote.</param>
        /// <param name="Formato">Formato de procesamiento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Beneficio_Lote_Procesa(int CodEmpresa, string cod_beneficio, string usuario, string Formato)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                var code = connection.Query<int>("[spBeneficio_Lote_Procesa]",
                    new { Codigo = cod_beneficio, usuario, Formato },
                    commandType: CommandType.StoredProcedure).FirstOrDefault();

                return new ErrorDto { Code = code, Description = "Lote procesado exitosamente" };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
