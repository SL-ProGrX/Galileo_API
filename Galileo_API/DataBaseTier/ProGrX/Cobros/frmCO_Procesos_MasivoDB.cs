
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using System.Data;
using System.Text;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoProcesosMasivoModels;


namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoProcesosMasivoDB
    {
        private readonly PortalDB _portalDB;
        private const string ModuloIncobrablesMasivo = "CBR-INC";
        private const string ModuloCobroJudicialMasivo = "CBR-CJE";
        private const string ModuloCobroJudicialReversaMasivo = "CBR-re";


        private const string AccionCarga = "C";
        private const int LongitudMaximaBloqueSql = 40000;
        // Módulo para bitácora
        private const int ModuloBitacora = 4;

        public FrmCoProcesosMasivoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }


        /// <summary>
        /// Carga masivamente las operaciones de proceso indicado. 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operaciones"></param>
        /// <param name="usuario"></param>
        ///  <param name="modulo"></param>
        /// <returns></returns>
        public ErrorDto<CoProcesosMasivoCargaResponse> Co_ProcesosMasivo_CargarArchivo(int CodEmpresa, List<string> operaciones, string usuario, string modulo)
        {

            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {

                if (!TieneOperacionesValidas(operaciones))
                {
                    return DbHelper.CreateErrorResponse<CoProcesosMasivoCargaResponse>(
                                  "No existen operaciones válidas para procesar.",
                                  -1,
                                  new CoProcesosMasivoCargaResponse());
                }



                EjecutarCargaMasivaProcesos(
                    connection,
                    operaciones!,
                    usuario,
                    modulo);

                var casosValidos = ObtenerRevisionProcesos(connection, usuario, "R", modulo);
                var casosInconsistentes = ObtenerRevisionProcesos(connection, usuario, "I", modulo);

                var response = new CoProcesosMasivoCargaResponse
                {
                    CasosValidos = casosValidos,
                    CasosInconsistentes = casosInconsistentes,
                    Resumen = new CoProcesosMasivoResumenModel
                    {
                        CantidadCasosValidos = casosValidos.Count,
                        TotalMoraFinanciera = casosValidos.Sum(x => x.Mora_Financiera),
                        TotalMoraLegal = casosValidos.Sum(x => x.Mora_Legal)
                    }
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CoProcesosMasivoCargaResponse>(
           "Ocurrió un error al cargar y revisar el archivo de incobrables.",
           -1,
           new CoProcesosMasivoCargaResponse());
            }
        }

        /// <summary>
        /// Ejecución de carga masiva delos registros de proceso correspondiente.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="operaciones"></param>
        /// <param name="usuario"></param>
        private static void EjecutarCargaMasivaProcesos(IDbConnection connection, IEnumerable<string> operaciones, string usuario,string modulo)
        {
            var numeroLinea = 0;
            var i = 0;
            var bloqueSql = new StringBuilder();
            var parameters = new DynamicParameters();
            var usuarioSeguro = (usuario ?? string.Empty).Trim();


            foreach (var operacion in operaciones)
            {
                if (!EsOperacionValida(operacion))
                {
                    continue;
                }

                numeroLinea++;
                var operacionSegura = (operacion ?? string.Empty).Trim();
                var clean = numeroLinea == 1 ? 1 : 0;


                var sqlActual = $@"
               exec spSys_Carga_Masiva
                   @Tipo{i},
                   @ProcesoId{i},
                   @Usuario{i},
                   @Llave01{i},
                   @Llave02{i},
                   @Clean{i};";

                parameters.Add($"Tipo{i}", AccionCarga);
                parameters.Add($"ProcesoId{i}", modulo);
                parameters.Add($"Usuario{i}", usuarioSeguro);
                parameters.Add($"Llave01{i}", operacionSegura);
                parameters.Add($"Llave02{i}", string.Empty);
                parameters.Add($"Clean{i}", clean);
                i++;

                if (numeroLinea == 1)
                {
                    connection.Execute(sqlActual, parameters);
                    continue;
                }

                bloqueSql.Append(sqlActual);

                if (bloqueSql.Length > LongitudMaximaBloqueSql)
                {
                    EjecutarBloqueSql(connection, bloqueSql, parameters);
                }
            }

            EjecutarBloqueSql(connection, bloqueSql, parameters);
        }

        /// <summary>
        /// Ejecuta el bloque acumulado y lo limpia.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="bloqueSql"></param>
        /// <param name="parameters"></param>
        private static void EjecutarBloqueSql(IDbConnection connection, StringBuilder bloqueSql, DynamicParameters parameters)
        {
            if (bloqueSql.Length == 0)
            {
                return;
            }

            connection.Execute(bloqueSql.ToString(), parameters);
            bloqueSql.Clear();
        }

        /// <summary>
        /// Indica si la lista contiene elementos para procesar.
        /// </summary>
        /// <param name="operaciones"></param>
        /// <returns></returns>
        private static bool TieneOperacionesValidas(List<string>? operaciones)
        {
            return operaciones != null && operaciones.Count > 0;
        }

        /// <summary>
        /// Valida que la operación tenga contenido y sea numérica.
        /// </summary>
        private static bool EsOperacionValida(string? operacion)
        {
            if (string.IsNullOrWhiteSpace(operacion))
            {
                return false;
            }

            return decimal.TryParse(operacion.Trim(), out _);
        }


        /// <summary>
        /// Recupera la revisión de los casos del procesos de carga masiva, diferenciando entre válidos e inconsistentes según el tipo de revisión solicitado.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoRevision"></param>
        /// <param name="modulo"></param>
        /// <returns></returns>
        private static List<CoProcesosMasivoRegistroModel> ObtenerRevisionProcesos(IDbConnection connection, string usuario, string tipoRevision,string modulo)
        {
            string  storedProcedure = ObtenerStoredProcedureRevision(modulo);

             
            if (string.IsNullOrWhiteSpace(storedProcedure))
            {
                throw new ArgumentException("Módulo no válido.");
            }

            var resultado = connection.Query<CoProcesosMasivoRegistroModel>(
                storedProcedure,
                new
                {
                    Tipo = AccionCarga,
                    ProcesoId = modulo,
                    Usuario = usuario,
                    Lista = tipoRevision
                },
                commandType: CommandType.StoredProcedure);

            return resultado.AsList();
        }

        /// <summary>
        ///  Procesa los casos del proceso masivo previamente cargados y revisados, registrando una nota explicativa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <param name="modulo"></param>
        /// <returns></returns>
        public ErrorDto<bool> Co_ProcesosMasivo_Procesar(int CodEmpresa, string nota, string usuario,string modulo)
        {


            var notas = nota?.Trim() ?? string.Empty;

            if (notas.Length < 30)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Debe especificar una nota válida de al menos 30 caracteres.",
                    -1,
                    false);
            }

            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                 string storedProcedure = ObtenerStoredProcedureProceso(modulo);
               

                var parameters = new DynamicParameters();
                parameters.Add("@Tipo", AccionCarga);
                parameters.Add("@ProcesoId", modulo);
                parameters.Add("@Usuario", usuario);
                parameters.Add("@Notas", notas);

                connection.Execute(
                    storedProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Error al procesar los  registros del proceso.",
                    -1,
                    false);
            }
        }
        private static string ObtenerStoredProcedureRevision(string modulo) => modulo switch
        {
            ModuloCobroJudicialMasivo => "spCBR_Cobro_Judicial_Masivo_Revisa",
            ModuloIncobrablesMasivo => "spCBR_Incobrables_Masivo_Revisa",
            ModuloCobroJudicialReversaMasivo => "spCBR_Cobro_Judicial_Masivo_Revisa",
            _ => throw new ArgumentException("Módulo no válido.")
        };

        private static string ObtenerStoredProcedureProceso(string modulo) => modulo switch
        {
            ModuloCobroJudicialMasivo => "spCBR_Cobro_Judicial_Masivo_Procesa",
            ModuloIncobrablesMasivo => "spCBR_Incobrables_Masivo_Procesa",
            ModuloCobroJudicialReversaMasivo => "spCBR_Cobro_Judicial_Masivo_Procesa",
            _ => throw new ArgumentException("Módulo no válido.")
        };
    }
}
