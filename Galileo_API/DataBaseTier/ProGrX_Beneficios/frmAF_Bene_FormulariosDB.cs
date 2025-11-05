using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmAF_Bene_FormulariosDB
    {
        private readonly IConfiguration _config;
        private readonly mBeneficiosDB _mBeneficiosDB;

        public frmAF_Bene_FormulariosDB(IConfiguration config)
        {
            _config = config;
            _mBeneficiosDB = new mBeneficiosDB(config);
        }

        #region Crear Formulario
        /// <summary>
        /// Obtengo lista de Formularios por beneficio para el Mantenimiento
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        public ErrorDTO<List<Formulario>> AfBeneFormulario_Obtener(int CodCliente, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<Formulario>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT F.ID_FORM, F.COD_FORMULARIO,F.COD_BENEFICIO, F.FRM_TITULO,  
                                          F.REGISTRO_USUARIO,	F.REGISTRO_FECHA,	F.MODIFICA_USUARIO,
                                          F.MODIFICA_FECHA,	F.ACTIVO, 
										  (SELECT COUNT(P.ID_FRM_PREGUNTA) FROM AFI_BENE_FORM_PREGUNTAS_W P
                                          WHERE P.ID_FORM = F.ID_FORM AND P.BORRADO = 0) AS TOTAL_PREGUNTAS
										  FROM AFI_BENE_FORM_MAIN_W F
                                       WHERE F.COD_BENEFICIO = '{cod_beneficio}' AND F.BORRADO = 0 ORDER BY F.ID_FORM DESC ";
                    response.Result = connection.Query<Formulario>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo las preguntas del formulario por ID_FORM
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_form"></param>
        /// <returns></returns>
        public ErrorDTO<Form> AfBeneFormularioPregunta_Obtener(int CodCliente, int id_form)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<Form>();
            response.Result = new Form();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query2 = $@"SELECT P.ID_FRM_PREGUNTA,P.PREGUNTA_ORDEN,P.PREGUNTA_TITULO,P.PREGUNTA_TIPO,	
                                           P.REQUERIDO,campo_homologado,
										   (SELECT COUNT(ID_OPCIONES) FROM AFI_BENE_FORM_OPCIONES_W
                                             WHERE ID_FRM_PREGUNTA = P.ID_FRM_PREGUNTA  AND BORRADO = 0) AS total_opciones
										   FROM AFI_BENE_FORM_PREGUNTAS_W P WHERE ID_FORM = '{id_form}' AND BORRADO = 0 ORDER BY PREGUNTA_ORDEN ASC";
                    response.Result.id = id_form;
                    response.Result.questions = connection.Query<FormQuestion>(query2).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo las opciones de las preguntas por ID_FRM_PREGUNTA
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_frm_pregunta"></param>
        /// <returns></returns>
        public ErrorDTO<List<OptionabledQuestion>> AfBeneFormularioOpciones_Obtener(int CodCliente, int id_frm_pregunta)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<OptionabledQuestion>>();
            response.Result = new List<OptionabledQuestion>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query3 = $@"SELECT ID_OPCIONES, ITEM,DESCRIPCION,SELECCION,	ID_FRM_PREGUNTA	 FROM AFI_BENE_FORM_OPCIONES_W
                                           WHERE ID_FRM_PREGUNTA = '{id_frm_pregunta}'  AND BORRADO = 0 ORDER BY ITEM DESC";
                    response.Result = connection.Query<OptionabledQuestion>(query3).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Agrega un Formulario nuevo "formulario" es el JSON completo del formulario creado
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="formulario"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneFormularios_Agregar(int CodCliente, string formulario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            Formulario frm = JsonConvert.DeserializeObject<Formulario>(formulario);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //valido el ultimo ID
                    var query = $@"SELECT ISNULL(MAX(ID_FORM),0) + 1 FROM AFI_BENE_FORM_MAIN_W";
                    frm.id_form = connection.Query<int>(query).FirstOrDefault();
                    info.Description = frm.id_form.ToString();
                    //cambio el formulario activo
                    //query = $@"UPDATE AFI_BENE_FORM_MAIN_W SET ACTIVO = 0 WHERE COD_BENEFICIO = '{frm.cod_beneficio}'";
                    //var update = connection.Execute(query);


                    //Insertar Formulario Encabezado
                    query = $@"INSERT INTO [dbo].[AFI_BENE_FORM_MAIN_W]
                                               ([ID_FORM]
                                               ,[COD_FORMULARIO]
                                               ,[COD_BENEFICIO]
                                               ,[FRM_TITULO]
                                               ,[REGISTRO_USUARIO]
                                               ,[REGISTRO_FECHA]
                                               ,[ACTIVO],
                                                BORRADO)
                                         VALUES
                                               ({frm.id_form} 
                                               ,{frm.id_form}
                                               ,'{frm.cod_beneficio}'
                                               ,'{frm.frm_titulo}'
                                               ,'{frm.registro_usuario}'
                                               ,getdate()
                                               , 1, 0 )";
                    var encabezado = connection.Execute(query);

                    //Insertar Preguntas
                    foreach (var question in frm.formulario.questions)
                    {
                        //valido el ultimo ID pregunta
                        query = $@"SELECT ISNULL(MAX(ID_FRM_PREGUNTA),0) + 1 FROM AFI_BENE_FORM_PREGUNTAS_W";
                        question.id_frm_pregunta = connection.Query<int>(query).FirstOrDefault();

                        int requerido = (question.requerido == true) ? 1 : 0;

                        query = $@"INSERT INTO [dbo].[AFI_BENE_FORM_PREGUNTAS_W]
                                               ([ID_FORM]
                                               ,[ID_FRM_PREGUNTA]
                                               ,[PREGUNTA_ORDEN]
                                               ,[PREGUNTA_TITULO]
                                               ,[PREGUNTA_TIPO]
                                              ,[REGISTRO_FECHA]
                                              ,[REGISTRO_USUARIO]
                                               ,[REQUERIDO], BORRADO)
                                         VALUES
                                               ({frm.id_form}
                                               ,{question.id_frm_pregunta}
                                               ,{question.pregunta_orden}
                                               ,'{question.pregunta_titulo}'
                                               ,'{question.pregunta_tipo}'
                                               ,getdate()    
                                               ,'{frm.registro_usuario}'
                                               ,{requerido}, 0)";
                        var pregunta = connection.Execute(query);

                        //Insertar Opciones
                        if (question.opciones != null)
                        {
                            foreach (OptionabledQuestion option in question.opciones)
                            {
                                //ultimo ID opcion
                                query = $@"SELECT ISNULL(MAX(ID_OPCIONES),0) + 1 FROM AFI_BENE_FORM_OPCIONES_W";
                                int id_opciones = connection.Query<int>(query).FirstOrDefault();

                                int selected = (option.selected == true) ? 1 : 0;
                                query = $@"INSERT INTO [dbo].[AFI_BENE_FORM_OPCIONES_W]
                                               ([ID_OPCIONES]
                                               ,[ID_FRM_PREGUNTA]
                                               ,[ITEM]
                                               ,[DESCRIPCION]
                                               ,[SELECCION]
                                              ,[REGISTRO_FECHA]
                                              ,[REGISTRO_USUARIO], BORRADO)
                                         VALUES
                                               ({id_opciones}
                                               ,{question.id_frm_pregunta}
                                               ,'{option.item}'
                                               ,'{option.descripcion}'
                                               ,{selected}
                                                , getdate(), '{frm.registro_usuario}', 0 )";
                                var opcion = connection.Execute(query);
                            }
                        }
                    }

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = frm.cod_beneficio,
                        consec = 0,
                        movimiento = "Creacion Form-WEB",
                        detalle = $"Creacion de Formulario [{frm.frm_titulo}] con codigo [{frm.id_form}] para Beneficio [{frm.cod_beneficio}]",
                        registro_usuario = frm.registro_usuario
                    });

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        #endregion

        #region Formulario Socio

        /// <summary>
        /// Obtiene los formularios por socio y beneficio
        /// </summary>
        /// <param name="Jformulario"></param>
        /// <returns></returns>
        public ErrorDTO<List<Formulario>> AfBeneFormSocios_Obtener(string Jformulario)
        {
            FrmFiltros formulario = JsonConvert.DeserializeObject<FrmFiltros>(Jformulario) ?? new FrmFiltros();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(formulario.codCliente);
            var response = new ErrorDTO<List<Formulario>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT ID_FORM, COD_FORMULARIO,COD_BENEFICIO, FRM_TITULO,  
                                          REGISTRO_USUARIO,	REGISTRO_FECHA,	MODIFICA_USUARIO,
                                          MODIFICA_FECHA,	ACTIVO FROM AFI_BENE_FORM_MAIN_W
                                       WHERE COD_BENEFICIO = '{formulario.cod_beneficio}' and ACTIVO = 1 AND BORRADO = 0";
                    response.Result = connection.Query<Formulario>(query).ToList();

                    foreach (var item in response.Result)
                    {
                        item.formulario = new Form();
                        item.formulario.id = item.id_form;

                        var query2 = $@"SELECT ID_FRM_PREGUNTA,	PREGUNTA_ORDEN,	PREGUNTA_TITULO,PREGUNTA_TIPO,CAMPO_HOMOLOGADO,
                                                (SELECT
                                                R.RESPUESTA_VALOR
                                                FROM AFI_BENE_FORM_RESPUESTAS_W R
                                                WHERE CEDULA = '{formulario.socio}' AND ID_BENEFICIO = '{formulario.id_beneficio}' AND R.ID_OPCIONES_RSP = 0 AND R.ID_FRM_PREGUNTA = PR.ID_FRM_PREGUNTA ) AS
                                                RESPUESTA, REQUERIDO FROM AFI_BENE_FORM_PREGUNTAS_W PR
                                                WHERE ID_FORM = '{item.id_form}'  AND BORRADO = 0 ORDER BY PREGUNTA_ORDEN ASC";

                        item.formulario.questions = connection.Query<FormQuestion>(query2).ToList();

                        foreach (var question in item.formulario.questions)
                        {
                            question.opciones = new List<OptionabledQuestion>();
                            switch (question.pregunta_tipo)
                            {
                                case "select":
                                case "multiSelect":
                                case "checkbox":
                                    List<OptionabledQuestion> respuestas = new List<OptionabledQuestion>();
                                    var query3 = $@"SELECT
                                                R.ID_OPCIONES_RSP
                                                FROM [dbo].[AFI_BENE_FORM_RESPUESTAS_W] R
                                                WHERE CEDULA = '{formulario.socio}' AND ID_BENEFICIO = '{formulario.id_beneficio}' AND R.ID_OPCIONES_RSP != 0 AND 
                                               R.ID_FRM_PREGUNTA = {question.id_frm_pregunta}";
                                    int id_opcion = connection.Query<int>(query3).FirstOrDefault();

                                    query3 = $@"SELECT ITEM,DESCRIPCION,	
                                        ISNULL((SELECT SELECCION FROM AFI_BENE_FORM_OPRESP_W WHERE  ID_OPCIONES = {id_opcion} AND ITEM = OP.ITEM ),0)
                                        selected,	ID_FRM_PREGUNTA	 FROM AFI_BENE_FORM_OPCIONES_W OP
                                            WHERE ID_FRM_PREGUNTA = '{question.id_frm_pregunta}'  AND BORRADO = 0 ORDER BY ITEM DESC ";
                                    question.opciones = connection.Query<OptionabledQuestion>(query3).ToList();

                                    //filtro las respuestas que selected = 1
                                    respuestas = question.opciones.Where(x => x.selected == true).ToList();
                                    question.respuesta = respuestas;
                                    break;

                                case "radio":

                                    OptionabledQuestion respuestaRadio = new OptionabledQuestion();
                                    query3 = $@"SELECT
                                                R.ID_OPCIONES_RSP
                                                FROM [dbo].[AFI_BENE_FORM_RESPUESTAS_W] R
                                                WHERE CEDULA = '{formulario.socio}' AND R.ID_OPCIONES_RSP != 0 AND 
                                               R.ID_FRM_PREGUNTA = {question.id_frm_pregunta}";
                                    id_opcion = connection.Query<int>(query3).FirstOrDefault();

                                    query3 = $@"SELECT ITEM,DESCRIPCION,	
                                        ISNULL((SELECT SELECCION FROM AFI_BENE_FORM_OPRESP_W WHERE  ID_OPCIONES = {id_opcion} AND ITEM = OP.ITEM ),0)
                                        selected,	ID_FRM_PREGUNTA	 FROM AFI_BENE_FORM_OPCIONES_W OP
                                            WHERE ID_FRM_PREGUNTA = '{question.id_frm_pregunta}'  AND BORRADO = 0 ORDER BY ITEM DESC ";
                                    question.opciones = connection.Query<OptionabledQuestion>(query3).ToList();

                                    //filtro las respuestas que selected = 1
                                    if (id_opcion > 0)
                                    {
                                        try
                                        {
                                            respuestaRadio = question.opciones.Where(x => x.selected == true).FirstOrDefault();
                                            question.respuesta = respuestaRadio.item;
                                        }
                                        catch (Exception)
                                        {
                                            question.respuesta = null;
                                        }

                                    }


                                    break;
                                case "date":
                                    if(question.respuesta != null)
                                    {
                                        question.respuestaFecha = DateTime.Parse(question.respuesta.ToString());
                                    }
                                    else
                                    {
                                        question.respuestaFecha = DateTime.Now;
                                    }
                                     
                                    break;
                                default:

                                    break;
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        ///  Agrega las respuestas del formulario por socio y beneficio
        /// </summary>
        /// <param name="Jdatos"></param>
        /// <param name="frm"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneFrmRespuesta_Agregar(string Jdatos, Form frm)
        {
            FrmFiltros datos = JsonConvert.DeserializeObject<FrmFiltros>(Jdatos) ?? new FrmFiltros();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(datos.codCliente);
            // Form frm = JsonConvert.DeserializeObject<Form>(formulario);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                //valido si alguna pregunta es requerida
                foreach (FormQuestion item in frm.questions)
                {
                    if (item.requerido == true && item.respuesta == null)
                    {
                        info.Code = -1;
                        info.Description = "La pregunta " + item.pregunta_titulo + " es requerida";
                        return info;
                    }
                    item.respuesta = (item.respuesta == null) ? null : item.respuesta.ToString();

                    //if (item.respuesta){
                    //    item.respuesta = item.respuesta.ToString().Replace("ValueKind = String :", "").Trim();
                    //}
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    //valido si el socio ya tiene un formulario
                    var query = $@"SELECT COUNT(*) FROM AFI_BENE_FORM_RESPUESTAS_W WHERE COD_BENEFICIO = '{datos.cod_beneficio}' 
                                 AND ID_BENEFICIO = '{datos.id_beneficio}' AND CEDULA = '{datos.socio}'";
                    int count = connection.Query<int>(query).FirstOrDefault();
                    if (count > 0)
                    {
                        try
                        {

                            //actualizar formulario
                            ErrorDTO<bool> resp = LimpiaRespuestasSocio(datos, frm.id);
                        }
                        catch (Exception ex)
                        {
                            info.Code = -1;
                            info.Description = ex.Message;
                            return info;
                        }
                    }

                    //valido ultimo ID 
                    query = $@"SELECT ISNULL(MAX(ID_RESPUESTA),0) + 1 FROM AFI_BENE_FORM_RESPUESTAS_W";
                    int id_respuesta = connection.Query<int>(query).FirstOrDefault();

                    foreach (FormQuestion item in frm.questions)
                    {
                        if (item.respuesta != null)
                        {

                            string respuesta = string.Empty;
                            ErrorDTO<int> id_opciones = new ErrorDTO<int>();
                            id_opciones.Result = 0;
                            //Valido por tipo de pregunta
                            switch (item.pregunta_tipo)
                            {
                                case "text":
                                case "textarea":
                                case "date":
                                case "number":
                                case "email":
                                    respuesta = item.respuesta.ToString();
                                    break;
                                case "select":
                                case "multiSelect":
                                case "checkbox":

                                    id_opciones = GuardarOpcionesRespuesta
                                        (datos.codCliente, item.id_frm_pregunta, id_respuesta, datos.usuario, item.respuesta);

                                    if (id_opciones.Code == -1)
                                    {
                                        info.Code = -1;
                                        info.Description = id_opciones.Description;
                                        return info;
                                    }
                                    break;
                                case "radio":
                                    id_opciones = GuardaOpcionSelectRespuesta
                                     (datos.codCliente, item.id_frm_pregunta, id_respuesta, datos.usuario, item.respuesta);

                                    if(id_opciones.Code == -1)
                                    {
                                        info.Code = -1;
                                        info.Description = id_opciones.Description;
                                        return info;
                                    }

                                    break;
                                default:
                                    break;
                            }
                            //Insertar Formulario Encabezado
                            query = $@"INSERT INTO [dbo].[AFI_BENE_FORM_RESPUESTAS_W]
                                               ([ID_RESPUESTA]
                                               ,[ID_FRM]
                                               ,[ID_FRM_PREGUNTA]
                                               ,[COD_BENEFICIO]
                                               ,[ID_BENEFICIO]
                                               ,[CEDULA]
                                               ,[PREGUNTA_TIPO]
                                               ,[ID_OPCIONES_RSP]
                                               ,[RESPUESTA_VALOR]
                                               ,[REGISTRO_FECHA]
                                               ,[REGISTRO_USUARIO]
                                               )
                                         VALUES
                                               ({id_respuesta}
                                                ,{frm.id}
                                                ,{item.id_frm_pregunta}
                                                ,'{datos.cod_beneficio}'
                                                ,{datos.id_beneficio}
                                                ,'{datos.socio}'
                                                ,'{item.pregunta_tipo}'
                                                ,{id_opciones.Result}
                                               ,'{respuesta}'
                                               , getdate(), '{datos.usuario}')";

                            var encabezado = connection.Execute(query);

                            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                            {
                                EmpresaId = datos.codCliente,
                                cod_beneficio = datos.cod_beneficio,
                                consec = 0,
                                movimiento = "Ingresa FrmSocio-Web",
                                detalle = $"Ingresa ID_FRM {frm.id} " +
                                $"PREGUNTA {item.id_frm_pregunta} BENEFICIO:{datos.cod_beneficio} " +
                                $"CEDULA:{datos.socio}",
                                registro_usuario = datos.usuario
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Guarda las respuestas de tipo objeto en listas de modelo OptionabledQuestion.
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_pregunta"></param>
        /// <param name="id_respuesta"></param>
        /// <param name="usuario"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        private ErrorDTO<int> GuardarOpcionesRespuesta(int CodCliente, int id_pregunta, int id_respuesta, string usuario, object question)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            string objJSon = question.ToString();
            var response = new ErrorDTO<int>();
            List<OptionabledQuestion> qst = new List<OptionabledQuestion>();
            OptionabledQuestion qst2 = new OptionabledQuestion();
            try
            {
                //convierto question a OptionabledQuestion
                qst = JsonConvert.DeserializeObject<List<OptionabledQuestion>>(objJSon);
            }
            catch (Exception)
            {
                //convierto question a OptionabledQuestion
                qst2 = JsonConvert.DeserializeObject<OptionabledQuestion>(objJSon) ?? new OptionabledQuestion();
            }


            response.Result = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT ISNULL(MAX(ID_OPCIONES),0) + 1 FROM AFI_BENE_FORM_OPRESP_W";
                    response.Result = connection.Query<int>(query).FirstOrDefault();
                    //ultimo consecutivo de opciones
                    if (qst.Count != 0)
                    {
                        foreach (var q in qst)
                        {

                            //Insertar Opciones
                            query = $@"INSERT INTO [dbo].[AFI_BENE_FORM_OPRESP_W]
                                           ([ID_OPCIONES]
                                           ,[ITEM]
                                           ,[DESCRIPCION]
                                           ,[SELECCION]
                                           ,[ID_FRM_PREGUNTA]
                                           ,[ID_FRM_RESPUESTA]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ({response.Result}
                                           ,'{q.item}'
                                           ,'{q.descripcion}'
                                           , 1
                                           ,{id_pregunta}
                                           ,{id_respuesta}
                                           ,getdate()
                                           ,'{usuario}'
                                           )";
                            response.Code = connection.Execute(query);
                        }
                    }
                    else
                    {

                        //Insertar Opciones
                        query = $@"INSERT INTO [dbo].[AFI_BENE_FORM_OPRESP_W]
                                           ([ID_OPCIONES]
                                           ,[ITEM]
                                           ,[DESCRIPCION]
                                           ,[SELECCION]
                                           ,[ID_FRM_PREGUNTA]
                                           ,[ID_FRM_RESPUESTA]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ({response.Result}
                                           ,'{qst2.item}'
                                           ,'{qst2.descripcion}'
                                           , 1
                                           ,{id_pregunta}
                                           ,{id_respuesta}
                                           ,getdate()
                                           ,'{usuario}'
                                           )";
                        response.Code = connection.Execute(query);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }

        /// <summary>
        /// Guarda las respuestas de tipo objeto en listas de string.
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_pregunta"></param>
        /// <param name="id_respuesta"></param>
        /// <param name="usuario"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        private ErrorDTO<int> GuardaOpcionSelectRespuesta(int CodCliente, int id_pregunta, int id_respuesta, string usuario, object question)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            string objJSon = JsonConvert.SerializeObject(question);

            List<string> qst = new List<string>();
            string qst2 = "";

            try
            {
                qst2 = JsonConvert.DeserializeObject<string>(objJSon);
            }
            catch (Exception)
            {
                qst = JsonConvert.DeserializeObject<List<string>>(objJSon);
            }

            ErrorDTO<int> id_opciones = new ErrorDTO<int>();
            id_opciones.Result = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //ultimo consecutivo de opciones
                    if (qst.Count != 0)
                    {
                        foreach (var q in qst)
                        {
                            var query = $@"SELECT ISNULL(MAX(ID_OPCIONES),0) + 1 FROM AFI_BENE_FORM_OPRESP_W";
                            id_opciones.Result = connection.Query<int>(query).FirstOrDefault();
                            //Insertar Opciones
                            query = $@"INSERT INTO [dbo].[AFI_BENE_FORM_OPRESP_W]
                                           ([ID_OPCIONES]
                                           ,[ITEM]
                                           ,[DESCRIPCION]
                                           ,[SELECCION]
                                           ,[ID_FRM_PREGUNTA]
                                           ,[ID_FRM_RESPUESTA]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ({id_opciones.Result}
                                           ,'{q}'
                                           ,'{q}'
                                           , 1
                                           ,{id_pregunta}
                                           ,{id_respuesta}
                                           ,getdate()
                                           ,'{usuario}'
                                           )";
                            id_opciones.Code = connection.Execute(query);
                        }
                    }
                    else
                    {
                        var query = $@"SELECT ISNULL(MAX(ID_OPCIONES),0) + 1 FROM AFI_BENE_FORM_OPRESP_W";
                        id_opciones.Result = connection.Query<int>(query).FirstOrDefault();
                        //Insertar Opciones
                        query = $@"INSERT INTO [dbo].[AFI_BENE_FORM_OPRESP_W]
                                           ([ID_OPCIONES]
                                           ,[ITEM]
                                           ,[DESCRIPCION]
                                           ,[SELECCION]
                                           ,[ID_FRM_PREGUNTA]
                                           ,[ID_FRM_RESPUESTA]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ({id_opciones.Result}
                                           ,'{qst2}'
                                           ,'{qst2}'
                                           , 1
                                           ,{id_pregunta}
                                           ,{id_respuesta}
                                           ,getdate()
                                           ,'{usuario}'
                                           )";
                        id_opciones.Code = connection.Execute(query);
                    }
                }
            }
            catch (Exception ex)
            {
                id_opciones.Code = -1;
                id_opciones.Description = ex.Message;
                id_opciones.Result = 0;
            }
            return id_opciones;

        }

        /// <summary>
        /// Limpia las respuestas del formulario por socio y beneficio
        /// </summary>
        /// <param name="datos"></param>
        /// <param name="id_frm"></param>
        /// <returns></returns>
        public ErrorDTO<bool> LimpiaRespuestasSocio(FrmFiltros datos, int id_frm)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(datos.codCliente);
            var info = new ErrorDTO<bool>();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT ID_OPCIONES_RSP FROM AFI_BENE_FORM_RESPUESTAS_W WHERE 
                                 COD_BENEFICIO = '{datos.cod_beneficio}' AND CEDULA = '{datos.socio}' AND ID_FRM = '{id_frm}' ";
                    List<int> respOp = connection.Query<int>(query).ToList();

                    foreach (var item in respOp)
                    {
                        query = $@"DELETE FROM AFI_BENE_FORM_OPRESP_W WHERE ID_OPCIONES = {item}";
                        connection.Execute(query);
                    }

                    query = $@"DELETE FROM AFI_BENE_FORM_RESPUESTAS_W WHERE COD_BENEFICIO = '{datos.cod_beneficio}'
                              AND ID_BENEFICIO = '{datos.id_beneficio}'  AND CEDULA = '{datos.socio}' AND ID_FRM = '{id_frm}' ";
                    info.Code = connection.Execute(query);

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                    {
                        EmpresaId = datos.codCliente,
                        cod_beneficio = datos.cod_beneficio,
                        consec = 0,
                        movimiento = "Edita Frm Socio-Web",
                        detalle = $@"Edita Respuestas Frm Socio {datos.socio} de beneficio {datos.cod_beneficio} ",
                        registro_usuario = datos.usuario
                    });
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = false;
            }
            return info;
        }

        #endregion

        #region Edita Encabezado
        /// <summary>
        /// Actualiza el Titulo del Formulario y si esta activo o inactivo. 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="formulario"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneFrmEncabezado_Actualizar(int CodCliente, string formulario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            Formulario frm = JsonConvert.DeserializeObject<Formulario>(formulario);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    int activo = (frm.activo == true) ? 1 : 0;

                    //Insertar Formulario Encabezado
                    var query = $@"";
                    query = $@"UPDATE AFI_BENE_FORM_MAIN_W
                               SET FRM_TITULO = '{frm.frm_titulo}'
                                  ,MODIFICA_USUARIO = '{frm.modifica_usuario}'
                                  ,MODIFICA_FECHA = getdate()
                                  , ACTIVO = {activo}
                             WHERE COD_BENEFICIO = '{frm.cod_beneficio}' AND ID_FORM = '{frm.id_form}' ";
                    info.Code = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Eliminel formulario y sus preguntas y opciones
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="if_frm"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneFormulario_Eliminar(int CodCliente, int if_frm)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Borro las opciones de las preguntas segun formulario
                    var Query = $@"UPDATE AFI_BENE_FORM_OPCIONES_W SET BORRADO = 1 WHERE ID_FRM_PREGUNTA IN (SELECT ID_FRM_PREGUNTA FROM AFI_BENE_FORM_PREGUNTAS_W WHERE ID_FORM = '{if_frm}') ";
                    info.Code = connection.Execute(Query);

                    //Borro las preguntas segun formulario
                    Query = $@"UPDATE AFI_BENE_FORM_PREGUNTAS_W SET BORRADO = 1 WHERE ID_FORM = '{if_frm}'";
                    info.Code = connection.Execute(Query);

                    //Borro el formulario
                    Query = $@"UPDATE AFI_BENE_FORM_MAIN_W SET BORRADO = 1 WHERE ID_FORM = '{if_frm}'";
                    info.Code = connection.Execute(Query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;

        }
        #endregion

        #region Edita Detalle Formulario

        /// <summary>
        /// Actualiza las preguntas y opciones del formulario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_from"></param>
        /// <param name="usuario"></param>
        /// <param name="formulario"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneFrmDetalle_Actualizar(int CodCliente, int id_from, string usuario, string formulario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            FormQuestion frm = JsonConvert.DeserializeObject<FormQuestion>(formulario);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int requerido = (frm.requerido == true) ? 1 : 0;

                    //Agrego Pregunta si es = 0
                    if (frm.id_frm_pregunta == 0)
                    {
                        var qInsert = $@"SELECT ISNULL(MAX(ID_FRM_PREGUNTA),0) + 1 FROM AFI_BENE_FORM_PREGUNTAS_W";
                        frm.id_frm_pregunta = connection.Query<int>(qInsert).FirstOrDefault();

                        var query = $@"INSERT INTO [dbo].[AFI_BENE_FORM_PREGUNTAS_W]
                                               ([ID_FORM]
                                               ,[ID_FRM_PREGUNTA]
                                               ,[PREGUNTA_ORDEN]
                                               ,[PREGUNTA_TITULO]
                                               ,[PREGUNTA_TIPO]
                                               ,[REGISTRO_FECHA]
                                               ,[REGISTRO_USUARIO]
                                               ,[REQUERIDO], [BORRADO], [CAMPO_HOMOLOGADO])
                                         VALUES
                                               ({id_from}
                                               ,{frm.id_frm_pregunta}
                                               ,{frm.pregunta_orden}
                                               ,'{frm.pregunta_titulo}'
                                               ,'{frm.pregunta_tipo}'
                                               ,getdate()    
                                               ,'{usuario}'
                                               ,{requerido}, 0, '{frm.campo_homologado}')";
                        info.Code = connection.Execute(query);

                    }
                    else
                    {
                        //Actualizar Preguntas
                        var query = $@"UPDATE AFI_BENE_FORM_PREGUNTAS_W
                                       SET PREGUNTA_ORDEN = {frm.pregunta_orden}
                                          ,PREGUNTA_TITULO = '{frm.pregunta_titulo}'
                                          ,PREGUNTA_TIPO = '{frm.pregunta_tipo}'
                                          ,REQUERIDO = {requerido}, [MODIFICA_FECHA] = getdate(), [MODIFICA_USUARIO] = '{usuario}', [CAMPO_HOMOLOGADO] = '{frm.campo_homologado}'
                                     WHERE ID_FORM = '{id_from}' AND ID_FRM_PREGUNTA = '{frm.id_frm_pregunta}' ";

                        info.Code = connection.Execute(query);
                    }

                    info.Description = frm.id_frm_pregunta.ToString();

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;

        }

        /// <summary>
        /// Metodo para eliminar las preguntas y opciones del formulario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="frm_pregunta"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneFrmDetalle_Elimina(int CodCliente, int frm_pregunta, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Borro las opciones de las preguntas segun formulario
                    var Query = $@"UPDATE AFI_BENE_FORM_OPCIONES_W SET BORRADO = 1 
                                       , MODIFICA_USUARIO = '{usuario}', MODIFICA_FECHA = getdate()  
                                            WHERE ID_FRM_PREGUNTA = {frm_pregunta} ";
                    connection.Execute(Query);

                    //Borro las preguntas segun formulario
                    Query = $@"UPDATE AFI_BENE_FORM_PREGUNTAS_W SET BORRADO = 1 
                               , MODIFICA_USUARIO = '{usuario}', MODIFICA_FECHA = getdate()  WHERE ID_FRM_PREGUNTA = '{frm_pregunta}'";
                    info.Code = connection.Execute(Query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        #endregion

        #region Edita Opciones Pregunta

        /// <summary>
        /// Actualiza las opciones de las preguntas del formulario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="frm_pregunta"></param>
        /// <param name="usuario"></param>
        /// <param name="opcion"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneFrmOpciones_Actualizar(int CodCliente, int frm_pregunta, string usuario, string opcion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            OptionabledQuestion frm = JsonConvert.DeserializeObject<OptionabledQuestion>(opcion);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Agrego Pregunta si es = 0
                    if (frm.id_opciones == 0)
                    {
                        var qInsert = $@"SELECT ISNULL(MAX(ID_OPCIONES),0) + 1 FROM AFI_BENE_FORM_OPCIONES_W";
                        frm.id_opciones = connection.Query<int>(qInsert).FirstOrDefault();

                        qInsert = $@"INSERT INTO [dbo].[AFI_BENE_FORM_OPCIONES_W]
                                               ([ID_OPCIONES]
                                               ,[ID_FRM_PREGUNTA]
                                               ,[ITEM]
                                               ,[DESCRIPCION]
                                               ,[SELECCION]
                                               ,[REGISTRO_FECHA]
                                               ,[REGISTRO_USUARIO], [BORRADO])
                                         VALUES
                                               ({frm.id_opciones}
                                               ,{frm_pregunta}
                                               ,'{frm.item}'
                                               ,'{frm.descripcion}'
                                               ,0
                                               ,getdate()    
                                               ,'{usuario}', 0
                                               )";

                        info.Code = connection.Execute(qInsert);
                    }
                    else
                    {
                        //Actualizar Preguntas
                        var qUpdate = $@"UPDATE AFI_BENE_FORM_OPCIONES_W
                                       SET ITEM = '{frm.item}'
                                          ,DESCRIPCION = '{frm.descripcion}'
                                          ,[MODIFICA_FECHA] = getdate()
                                          ,[MODIFICA_USUARIO] = '{usuario}'
                                     WHERE ID_FRM_PREGUNTA = '{frm_pregunta}' AND ID_OPCIONES = '{frm.id_opciones}' ";

                        info.Code = connection.Execute(qUpdate);
                    }

                    info.Description = frm.id_opciones.ToString();

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Elimina las opciones de las preguntas del formulario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="frm_pregunta"></param>
        /// <param name="id_opciones"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneFrmOpciones_Eliminar(int CodCliente, int frm_pregunta, int id_opciones, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Borro las opciones de las preguntas segun formulario
                    var Query = $@"UPDATE AFI_BENE_FORM_OPCIONES_W SET BORRADO = 1 ,
                                            MODIFICA_USUARIO = '{usuario}', MODIFICA_FECHA = getdate()  
                                            WHERE ID_FRM_PREGUNTA = {frm_pregunta} AND ID_OPCIONES = {id_opciones} ";
                    info.Code = connection.Execute(Query);

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        #endregion

        #region Reportes
        /// <summary>
        /// Metodo para obtener el reporte de los formularios
        /// </summary>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDTO<object> AfBeneficiosReporte_Obtener(FrmReporteDatos datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(datos.codCliente);
            var info = new ErrorDTO<object>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spBene_W_FormRespuestasPivot]";
                    var values = new
                    {
                        ID_FORM = datos.id_frm,
                        FechaInicio = datos.fechaInicio,
                        FechaFin = datos.fechaFin
                    };

                    info.Result = connection.Query(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = null;
            }
            return info;
        }

        /// <summary>
        /// Metodo para obtener el reporte de los formularios por socio
        /// </summary>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDTO<List<ReporteFormularioDatos>> AfBeneficiosReporteSocio_Obtener(FrmReporteDatos datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(datos.codCliente);
            var info = new ErrorDTO<List<ReporteFormularioDatos>>();
            try
            {

                using (IDbConnection db = new SqlConnection(stringConn))
                {
                    var query = $@"exec spAFI_Bene_FormularioRepSocio {datos.id_frm}, '{datos.cedula.Trim()}' ";
                    info.Result = db.Query<ReporteFormularioDatos>(query).ToList();

                    string beneficio = info.Result.FirstOrDefault().cod_beneficio;
                    string cedula = info.Result.FirstOrDefault().cedula;
                    DateTime dateTime = info.Result.FirstOrDefault().registro_fecha;

                    foreach (var item in info.Result)
                    {
                        if (item.cod_beneficio == null)
                        {
                            info.Result[info.Result.IndexOf(item)].cod_beneficio = beneficio;
                            info.Result[info.Result.IndexOf(item)].cedula = cedula;
                            info.Result[info.Result.IndexOf(item)].registro_fecha = dateTime;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = null;
            }
            return info;
        }

        #endregion





    }
}