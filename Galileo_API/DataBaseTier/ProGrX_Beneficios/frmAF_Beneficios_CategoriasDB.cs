using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.SYS;


namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_CategoriasDB
    {
        private readonly IConfiguration _config;
        private readonly mBeneficiosDB _mBeneficiosDB;

        public frmAF_Beneficios_CategoriasDB(IConfiguration config)
        {
            _config = config;
            _mBeneficiosDB = new mBeneficiosDB(_config);
        }


        /// <summary>
        /// Obtiene la lista lazy  
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<BENE_CATEGORIADataLista> BeneficiosCategorias_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<BENE_CATEGORIADataLista>();
            response.Result = new BENE_CATEGORIADataLista();
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) FROM AFI_BENE_CATEGORIAS";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE COD_CATEGORIA LIKE '%" + filtro + "%' OR DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT * FROM AFI_BENE_CATEGORIAS
                                         {filtro} 
                                        ORDER BY COD_CATEGORIA
                                        {paginaActual}
                                        {paginacionActual} ";

                    response.Result.Lista = connection.Query<BENE_CATEGORIA>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Total = 0;
            }
            return response;
        }


        /// <summary>
        /// Actualiza el detalle de una categoria
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO BeneficiosCategorias_Actualizar(int CodEmpresa, BENE_CATEGORIA request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int activo = request.activo ? 1 : 0;
                    int apre = request.i_apremiante ? 1 : 0;
                    int reco = request.i_reconocimientos ? 1 : 0;
                    int crece = request.i_crece ? 1 : 0;
                    int fena = request.i_fena ? 1 : 0;
                    int sepe = request.i_sepelio ? 1 : 0;
                    int desa = request.i_desastres ? 1 : 0;

                    var query = $@"UPDATE AFI_BENE_CATEGORIAS 
                                SET descripcion = '{request.descripcion}', activo = {activo}, 
                                i_apremiante = '{apre}', i_reconocimientos = {reco}, i_crece = {crece},
                                i_fena = '{fena}', i_sepelio = {sepe}, i_desastres = {desa},
                                modifica_fecha = GETDATE(), modifica_usuario = '{request.modifica_usuario}'
                                WHERE cod_categoria = '{request.cod_categoria}'";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Agrega categoria
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO BeneficiosCategorias_Agregar(int CodEmpresa, BENE_CATEGORIA request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"select COUNT(*) FROM AFI_BENE_CATEGORIAS WHERE COD_CATEGORIA = '{request.cod_categoria}'";
                    int existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe > 0)
                    {
                        resp.Code = -1;
                        resp.Description = "Ya existe una categoría con el código: " + request.cod_categoria + ", por favor verifique";
                    }
                    else
                    {
                        int activo = request.activo ? 1 : 0;
                        int apre = request.i_apremiante ? 1 : 0;
                        int reco = request.i_reconocimientos ? 1 : 0;
                        int crece = request.i_crece ? 1 : 0;
                        int fena = request.i_fena ? 1 : 0;
                        int sepe = request.i_sepelio ? 1 : 0;
                        int desa = request.i_desastres ? 1 : 0;

                        query = $@"INSERT INTO AFI_BENE_CATEGORIAS(cod_categoria,descripcion,activo, i_apremiante, i_reconocimientos, i_crece, i_fena,i_sepelio, i_desastres,registro_fecha, registro_usuario)
                        values('{request.cod_categoria}','{request.descripcion}', {activo}, '{apre}', {reco},{crece}, {fena},{sepe}, {desa},getdate(),'{request.registro_usuario}')";

                        resp.Code = connection.ExecuteAsync(query).Result;
                        resp.Description = "Ok";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Elimina una categoria
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDTO BeneficiosCategorias_Eliminar(int CodEmpresa, string id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"DELETE AFI_BENE_CATEGORIAS where COD_CATEGORIA = '{id}'";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene los permisos de una categoria
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_categoria"></param>
        /// <returns></returns>
        public ErrorDTO<List<Bene_CategoriaPermisos>> BeneficiosCategorias_ObtenerPermisos(int CodCliente, string cod_categoria, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<Bene_CategoriaPermisos>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec spAFI_Bene_CategoriaPermisos '{cod_categoria}', '{filtro}'";
                    response.Result = connection.Query<Bene_CategoriaPermisos>(query).ToList();
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
        /// Registra los permisos de una categoria
        /// </summary>
        /// <param name="Cod_Categoria"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO registroPermisosCategoria(int CodCliente, string Cod_Categoria, Bene_CategoriaPermisos request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query1 = $@" SELECT I_CAMBIAR_ESTADO,  I_MODIFICA_EXPEDIENTE, I_TRASLADO_TESORERIA, I_PAGO_PROGRAMAR, 
                     I_PAGO_APROBAR_M, I_PAGO_REALIZAR, I_INGRESAR_SOLICITUD, I_PERIODO, I_PAGO_CONSULTA,  I_APROBAR, I_RECHAZAR, I_ANULAR, I_DEVOLVER_RESOLUCION
                     FROM AFI_BENE_GRUPOS_ROLES  WHERE COD_CATEGORIA = '{Cod_Categoria}' and usuario = '{request.nombre}' ";

                    var result = connection.QueryFirstOrDefault(query1, new { Cod_Categoria, request.nombre});


                    int I_CAMBIAR_ESTADO_ANTERIOR = result?.I_CAMBIAR_ESTADO ?? 0;
                    int I_MODIFICA_EXPEDIENTE_ANTERIOR = result?.I_MODIFICA_EXPEDIENTE ?? 0;
                    int I_TRASLADO_TESORERIA_ANTERIOR = result?.I_TRASLADO_TESORERIA ?? 0;
                    int I_PAGO_PROGRAMAR_ANTERIOR = result?.I_PAGO_PROGRAMAR ?? 0;
                    int I_PAGO_APROBAR_M_ANTERIOR = result?.I_PAGO_APROBAR_M ?? 0;
                    int I_PAGO_REALIZAR_ANTERIOR = result?.I_PAGO_REALIZAR ?? 0;
                    int I_INGRESAR_SOLICITUD_ANTERIOR = result?.I_INGRESAR_SOLICITUD ?? 0;
                    int I_PERIODO_ANTERIOR = result?.I_PERIODO ?? 0;
                    int I_PAGO_CONSULTA_ANTERIOR = result?.I_PAGO_CONSULTA ?? 0;
                    int I_APROBAR_ANTERIOR = result?.I_APROBAR ?? 0;
                    int I_RECHAZAR_ANTERIOR = result?.I_RECHAZAR ?? 0;
                    int I_ANULAR_ANTERIOR = result?.I_ANULAR ?? 0;
                    int I_DEVOLVER_RESOLUCION_ANTERIOR = result?.I_DEVOLVER_RESOLUCION ?? 0;



                    var query = @$"exec spAFI_Bene_CategoriaPermisoRegistro '{Cod_Categoria}', '{request.nombre}',
                        {Convert.ToInt32(request.i_cambiar_estado)}, {Convert.ToInt32(request.i_modifica_expediente)},
                        {Convert.ToInt32(request.i_traslado_tesoreria)},{Convert.ToInt32(request.i_pago_programar)},
                        {Convert.ToInt32(request.i_pago_aprobar_m)}, {Convert.ToInt32(request.i_pago_realizar)},
                        {Convert.ToInt32(request.i_ingresar_solicitud)},{Convert.ToInt32(request.i_periodo)},
                        {Convert.ToInt32(request.i_pago_consulta)}, {Convert.ToInt32(request.i_aprobar)},
                        {Convert.ToInt32(request.i_rechazar)},{Convert.ToInt32(request.i_anular)},
                        {Convert.ToInt32(request.i_devolver_resolucion)},'{request.registro_usuario}' ,
                         {request.cod_rol}";
                    connection.Execute(query);

                    resp.Description = "Registro actualizado satisfactoriamente";
                    // Actualizar el estado a su representación textual de cada campo

                    // Cambiar Estado

                    if (I_CAMBIAR_ESTADO_ANTERIOR != (request.i_cambiar_estado ? 1 : 0))
                    {
                        if (request.i_cambiar_estado == true)
                        {
                            string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Cambiar Estado de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                            RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                        }
                        else
                        {
                            string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Cambiar Estado de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                            RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                        }
                    }

                    if (I_MODIFICA_EXPEDIENTE_ANTERIOR != (request.i_modifica_expediente ? 1 : 0))
                    {
                        // Modificar Expediente
                        if (request.i_modifica_expediente == true)
                        {
                            string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Modificar Expediente de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                            RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                        }
                        else
                        {
                            string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Modificar Expediente de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                            RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                        }
                    }

                    // Traslado Tesorería
                    if (I_TRASLADO_TESORERIA_ANTERIOR != (request.i_traslado_tesoreria ? 1 : 0))
                    {
                        if (request.i_traslado_tesoreria == true)
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Traslado Tesorería de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    else
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Traslado Tesorería de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    }

                    // Programar Pago
                    if (I_PAGO_PROGRAMAR_ANTERIOR != (request.i_pago_programar ? 1 : 0))
                    {
                        if (request.i_pago_programar == true)
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Programar Pago de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    else
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Programar Pago de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    }

                    // Aprobar Monto
                    if (I_PAGO_APROBAR_M_ANTERIOR != (request.i_pago_aprobar_m ? 1 : 0))
                    {
                        if (request.i_pago_aprobar_m == true)
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Aprobar Monto de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    else
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Aprobar Monto de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    }


                    // Realizar Pago
                    if (I_PAGO_REALIZAR_ANTERIOR != (request.i_pago_realizar ? 1 : 0))
                    {
                        if (request.i_pago_realizar == true)
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Realizar Pago de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    else
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Realizar Pago de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    }

                    // Ingresar Solicitud
                    if (I_INGRESAR_SOLICITUD_ANTERIOR != (request.i_ingresar_solicitud ? 1 : 0))
                    {
                        if (request.i_ingresar_solicitud == true)
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Ingresar Solicitud de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    else
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Ingresar Solicitud de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    }

                    // Consultar Pago
                    if (I_PAGO_CONSULTA_ANTERIOR != (request.i_pago_consulta ? 1 : 0))
                    {
                        if (request.i_pago_consulta == true)
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Consultar Pago de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    else
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Consultar Pago de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    }

                    // Aprobar
                    if (I_APROBAR_ANTERIOR != (request.i_aprobar ? 1 : 0))
                    {
                        if (request.i_aprobar == true)
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Aprobar de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    else
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Aprobar de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    }

                    // Rechazar
                    if (I_RECHAZAR_ANTERIOR != (request.i_rechazar ? 1 : 0))
                    {
                        if (request.i_rechazar == true)
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Rechazar de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    else
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Rechazar de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    }

                    // Anular
                    if (I_ANULAR_ANTERIOR != (request.i_anular ? 1 : 0))
                    {
                        if (request.i_anular == true)
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Anular de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    else
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Anular de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    }

                    // Devolver Resolución
                    if (I_DEVOLVER_RESOLUCION_ANTERIOR != (request.i_devolver_resolucion ? 1 : 0))
                    {
                        if (request.i_devolver_resolucion == true)
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Devolver Resolución de [Inactivo] por [Activo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    else
                    {
                        string detalle = $@"El usuario [{request.registro_usuario}] actualiza permiso de Devolver Resolución de [Activo] por [Inactivo] del usuario [{request.nombre}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, Cod_Categoria, request.registro_usuario);
                    }
                    }

                }
                }

                
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene la lista de validaciones
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<BeneValidaLista>> BeneValidacionesLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<BeneValidaLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT COD_VAL AS ITEM, DESCRIPCION FROM AFI_BENE_VALIDACIONES WHERE ESTADO = 1 ";
                    response.Result = connection.Query<BeneValidaLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Obtiene la lista de validaciones por categoria
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_categoria"></param>
        /// <returns></returns>
        public ErrorDTO<List<BeneCategoriaValidaLista>> BeneCategoriaValida_Obtener(int CodCliente, string cod_categoria)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<BeneCategoriaValidaLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM AFI_BENE_VALIDA_CATEGORIA WHERE cod_categoria = '{cod_categoria}' ";
                    response.Result = connection.Query<BeneCategoriaValidaLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Guarda la validacion de la categoria
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="valida"></param>
        /// <returns></returns>
        public ErrorDTO BeneCategoriaValida_Guardar(int CodCliente, BeneCategoriaValidaLista valida)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //busco si exite la validacion
                    var query = $@"SELECT COUNT(*) FROM AFI_BENE_VALIDA_CATEGORIA WHERE cod_categoria = '{valida.cod_categoria}' AND cod_val = {valida.cod_val} ";
                    int existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe > 0)
                    {
                        query = $@"UPDATE AFI_BENE_VALIDA_CATEGORIA SET registro = {Convert.ToInt32(valida.registro)}, registro_justifica = {Convert.ToInt32(valida.registro_justifica)},registro_info = {Convert.ToInt32(valida.registro_info)},
                        pago = {Convert.ToInt32(valida.pago)}, pago_justifica = {Convert.ToInt32(valida.pago_justifica)},pago_info = {Convert.ToInt32(valida.pago_info)}, estado = {Convert.ToInt32(valida.estado)}, 
                        modifica_usuario = '{valida.modifica_usuario}', modifica_fecha = GETDATE() WHERE cod_categoria = '{valida.cod_categoria}' AND cod_val = {valida.cod_val} ";
                    }
                    else
                    {
                        query = $@"INSERT INTO AFI_BENE_VALIDA_CATEGORIA(cod_categoria, cod_val, registro, registro_justifica,registro_info, pago, pago_justifica,pago_info, estado, registro_usuario, registro_fecha)
                        VALUES('{valida.cod_categoria}', {valida.cod_val}, {Convert.ToInt32(valida.registro)}, {Convert.ToInt32(valida.registro_justifica)}, {Convert.ToInt32(valida.registro_info)}, {Convert.ToInt32(valida.pago)}, {Convert.ToInt32(valida.pago_justifica)}, {Convert.ToInt32(valida.pago_info)},{Convert.ToInt32(valida.estado)}, '{valida.registro_usuario}', GETDATE())";
                    }

                    response.Code = connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Registra la bitacora de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        /// <param name="codBeneficio"></param>
        /// <param name="registraUser"></param>
        private void RegistrarBitacora(int CodCliente, string movimiento, string detalle, string codBeneficio, string registraUser)
        {
            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
            {
                EmpresaId = CodCliente,
                cod_beneficio = codBeneficio,
                consec = -2,
                movimiento = movimiento,
                detalle = detalle,
                registro_usuario = registraUser
            });
        }






    }



}