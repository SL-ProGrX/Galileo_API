using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del Constructor de Formularios de Beneficios (frmAF_Bene_Formularios).
    /// Consultas aquí; guardado en .Guardar, respuestas de socio en .Respuestas, reportes en .Reportes.
    /// </summary>
    public partial class FrmAfBeneFormulariosDB
    {
        private readonly IConfiguration _config;
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Inicializa el acceso a datos y la bitácora de beneficios con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneFormulariosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mBeneficiosDB = new MBeneficiosDB(_config);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de formularios de un beneficio para el mantenimiento.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <returns>Lista de formularios.</returns>
        public ErrorDto<List<Formulario>> AfBeneFormulario_Obtener(int CodCliente, string cod_beneficio)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT F.ID_FORM, F.COD_FORMULARIO, F.COD_BENEFICIO, F.FRM_TITULO,
                                            F.REGISTRO_USUARIO, F.REGISTRO_FECHA, F.MODIFICA_USUARIO, F.MODIFICA_FECHA, F.ACTIVO,
                                            (SELECT COUNT(P.ID_FRM_PREGUNTA) FROM AFI_BENE_FORM_PREGUNTAS_W P
                                             WHERE P.ID_FORM = F.ID_FORM AND P.BORRADO = 0) AS TOTAL_PREGUNTAS
                                     FROM AFI_BENE_FORM_MAIN_W F
                                     WHERE F.COD_BENEFICIO = @cod_beneficio AND F.BORRADO = 0
                                     ORDER BY F.ID_FORM DESC";
                return connection.Query<Formulario>(sql, new { cod_beneficio }).ToList();
            });
        }

        /// <summary>
        /// Obtiene las preguntas de un formulario por su ID.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="id_form">Identificador del formulario.</param>
        /// <returns>Formulario con sus preguntas.</returns>
        public ErrorDto<Form> AfBeneFormularioPregunta_Obtener(int CodCliente, int id_form)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT P.ID_FRM_PREGUNTA, P.PREGUNTA_ORDEN, P.PREGUNTA_TITULO, P.PREGUNTA_TIPO,
                                            P.REQUERIDO, campo_homologado,
                                            (SELECT COUNT(ID_OPCIONES) FROM AFI_BENE_FORM_OPCIONES_W
                                             WHERE ID_FRM_PREGUNTA = P.ID_FRM_PREGUNTA AND BORRADO = 0) AS total_opciones
                                     FROM AFI_BENE_FORM_PREGUNTAS_W P
                                     WHERE ID_FORM = @id_form AND BORRADO = 0
                                     ORDER BY PREGUNTA_ORDEN ASC";
                return new Form
                {
                    id = id_form,
                    questions = connection.Query<FormQuestion>(sql, new { id_form }).ToList()
                };
            });
        }

        /// <summary>
        /// Obtiene las opciones de una pregunta por su ID.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="id_frm_pregunta">Identificador de la pregunta.</param>
        /// <returns>Lista de opciones.</returns>
        public ErrorDto<List<OptionabledQuestion>> AfBeneFormularioOpciones_Obtener(int CodCliente, int id_frm_pregunta)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT ID_OPCIONES, ITEM, DESCRIPCION, SELECCION, ID_FRM_PREGUNTA
                                     FROM AFI_BENE_FORM_OPCIONES_W
                                     WHERE ID_FRM_PREGUNTA = @id_frm_pregunta AND BORRADO = 0
                                     ORDER BY ITEM DESC";
                return connection.Query<OptionabledQuestion>(sql, new { id_frm_pregunta }).ToList();
            });
        }
    }
}
