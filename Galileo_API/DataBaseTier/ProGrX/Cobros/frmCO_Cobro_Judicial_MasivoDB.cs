
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using System.Data;
using System.Text;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoCobroJudicialMasivoModels;


namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoCobroJudicialMasivoDB
    {
        private readonly PortalDB _portalDB;
        private const string ModuloCobroJudicialMasivo = "CBR-CJE";
        private const string AccionCarga = "C";
        private const int LongitudMaximaBloqueSql = 40000;
        // Módulo para bitácora
        private const int ModuloBitacora = 4;

        public FrmCoCobroJudicialMasivoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }


        /// <summary>
        /// Carga masivamente las operaciones de cobro judicial.
        /// Migra la lógica de frmCO_Cobro_Judicial_Masivo.
        /// </summary>
        public ErrorDto<CoCobroJudicialMasivoCargaResponse> Co_CobroJudicialMasivo_CargarOperaciones(int CodEmpresa, List<string> operaciones, string usuario)
        {

            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                EjecutarCargaMasivaCobroJudicial(
                    connection,
                    operaciones!,
                    usuario);

                var casosValidos = ObtenerRevisionCobroJudicial(connection, usuario, "R");
                var casosInconsistentes = ObtenerRevisionCobroJudicial(connection, usuario, "I");

                var response = new CoCobroJudicialMasivoCargaResponse
                {
                    CasosValidos = casosValidos,
                    CasosInconsistentes = casosInconsistentes,
                    Resumen = new CoCobroJudicialMasivoResumenModel
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
                return DbHelper.CreateErrorResponse<CoCobroJudicialMasivoCargaResponse>(
           "Ocurrió un error al cargar y revisar el archivo de cobro judicial.",
           -1,
           new CoCobroJudicialMasivoCargaResponse());
            }
        }

        /// <summary>
        /// Ejecuta la carga masiva replicando la lógica VB6:
        /// primer registro con bandera 1, siguientes con 0 y ejecución por bloques.
        /// </summary>
        private static void EjecutarCargaMasivaCobroJudicial(IDbConnection connection, IEnumerable<string> operaciones, string usuario)
        {
            var numeroLinea = 0;
            var bloqueSql = new StringBuilder();
            var usuarioSeguro = SanitizarValorSql(usuario);

            foreach (var operacion in operaciones)
            {
                if (!EsOperacionValida(operacion))
                {
                    continue;
                }

                numeroLinea++;
                var operacionSegura = SanitizarValorSql(operacion);

                var sqlActual = ConstruirSentenciaCargaMasiva(
                    usuarioSeguro,
                    operacionSegura,
                    numeroLinea == 1 ? 1 : 0);

                if (numeroLinea == 1)
                {
                    connection.Execute(sqlActual);
                    continue;
                }

                bloqueSql.Append(sqlActual);

                if (bloqueSql.Length > LongitudMaximaBloqueSql)
                {
                    EjecutarBloqueSql(connection, bloqueSql);
                }
            }

            EjecutarBloqueSql(connection, bloqueSql);
        }

        /// <summary>
        /// Construye la sentencia EXEC para la carga masiva.
        /// </summary>
        private static string ConstruirSentenciaCargaMasiva(string usuario,string operacion,int esInicio)
        {
            return            
            $"{new string(' ', 10)}exec spSys_Carga_Masiva '{AccionCarga}', '{ModuloCobroJudicialMasivo}', '{usuario}', '{operacion}', '', {esInicio}";
        }

        /// <summary>
        /// Ejecuta el bloque acumulado y lo limpia.
        /// </summary>
        private static void EjecutarBloqueSql(IDbConnection connection, StringBuilder bloqueSql)
        {
            if (bloqueSql.Length == 0)
            {
                return;
            }

            connection.Execute(bloqueSql.ToString());
            bloqueSql.Clear();
        }

        /// <summary>
        /// Indica si la lista contiene elementos para procesar.
        /// </summary>
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
        /// Sanitiza valores para construcción de SQL dinámico.
        /// </summary>
        private static string SanitizarValorSql(string? valor)
        {
            return (valor ?? string.Empty).Trim().Replace("'", "''");
        }

        /// <summary>
        /// Recupera la revisión de los casos de cobro judicial, diferenciando entre válidos e inconsistentes según el tipo de revisión solicitado.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoRevision"></param>
        /// <returns></returns>
        private static List<CoCobroJudicialMasivoRegistroModel> ObtenerRevisionCobroJudicial(IDbConnection connection,string usuario,string tipoRevision)
        {
            const string storedProcedure = "spCBR_Cobro_Judicial_Masivo_Revisa";

            var resultado = connection.Query<CoCobroJudicialMasivoRegistroModel>(
                storedProcedure,
                new
                {
                    Tipo = AccionCarga,
                    ProcesoId = ModuloCobroJudicialMasivo,
                    Usuario = usuario,
                    Lista = tipoRevision
                },
                commandType: CommandType.StoredProcedure);

            return resultado.AsList();
        }
    
        /// <summary>
        ///  Procesa los casos de cobro judicial masivo previamente cargados y revisados, registrando una nota explicativa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> Co_CobroJudicialMasivo_Procesar(int CodEmpresa, string nota, string usuario)
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
                const string storedProcedure = "spCBR_Cobro_Judicial_Masivo_Procesa";

                var parameters = new DynamicParameters();
                parameters.Add("@Tipo", "C");
                parameters.Add("@ProcesoId", "CBR-CJE");
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
                    "Error al procesar los cobros judiciales masivos.",
                    -1,
                    false);
            }
        }
    }
}
