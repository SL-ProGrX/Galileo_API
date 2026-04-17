
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
        /// Ejecución de carga masiva de cobro judicial.
        /// </summary>
        private static void EjecutarCargaMasivaCobroJudicial(IDbConnection connection, IEnumerable<string> operaciones, string usuario)
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
                var operacionSegura = SanitizarValorSql(operacion);
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
                parameters.Add($"ProcesoId{i}", ModuloCobroJudicialMasivo);
                parameters.Add($"Usuario{i}", usuarioSeguro);
                parameters.Add($"Llave01{i}", operacionSegura);
                parameters.Add($"Llave02{i}", string.Empty);
                parameters.Add($"Clean{i}", clean);
                i++;

                if (numeroLinea == 1)
                {
                    connection.Execute(sqlActual,parameters);
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
