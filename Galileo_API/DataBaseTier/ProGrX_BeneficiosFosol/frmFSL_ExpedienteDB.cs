using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmFSL_ExpedienteDB
    {
        private readonly IConfiguration _config;

        public frmFSL_ExpedienteDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obteniene la lista de planes
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<FslMenusData>> FslPlanLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslMenusData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select  COD_PLAN as item , Rtrim(COD_PLAN) + ' - ' + rtrim(descripcion) as descripcion
		                                     from  FSL_PLANES where activo = 1";
                    response.Result = connection.Query<FslMenusData>(query).ToList();
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
        /// Obteniene la lista de comites
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<FslMenusData>> FslComiteLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslMenusData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select COD_COMITE as item, Rtrim(COD_COMITE) + ' - ' + rtrim(descripcion) as descripcion
			                             from  FSL_COMITES where activo = 1";
                    response.Result = connection.Query<FslMenusData>(query).ToList();
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
        /// Obteniene la lista de enfermedades
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<FslMenusData>> FslEnfermedadesLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslMenusData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select COD_ENFERMEDAD as item, Rtrim(COD_ENFERMEDAD) + ' - ' + rtrim(descripcion) as descripcion
			                             from  FSL_TIPOS_ENFERMEDADES where ACTIVA = 1";
                    response.Result = connection.Query<FslMenusData>(query).ToList();
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
        /// Obteniene la lista de causas
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_plan"></param>
        /// <returns></returns>
        public ErrorDto<List<FslMenusData>> FslCausasLista_Obtener(int CodCliente, string cod_plan)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslMenusData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select COD_CAUSA as item, Rtrim(COD_CAUSA) + ' - ' + rtrim(descripcion) as descripcion
                                         from  FSL_PLANES_CAUSAS where COD_PLAN = '{cod_plan}'  order by COD_CAUSA";
                    response.Result = connection.Query<FslMenusData>(query).ToList();
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
        /// Consulta los expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_expediente"></param>
        /// <returns></returns>
        public ErrorDto<FslExpedienteDatos> FslExpediente_Obtener(int CodCliente, int cod_expediente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<FslExpedienteDatos>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Ex.*, Soc.NOMBRE
			                    , Pl.COD_PLAN + ' - ' + rtrim(Pl.DESCRIPCION) as 'Plan'
			                    , Pc.COD_CAUSA + ' - ' + rtrim(Pc.DESCRIPCION) as 'Causa'
			                    , Te.COD_ENFERMEDAD + ' - ' + rtrim(Te.DESCRIPCION) as 'Enfermedad'
			                    , Co.COD_COMITE + ' - ' + rtrim(Co.DESCRIPCION) as 'Comite'
			                     from FSL_EXPEDIENTES Ex
			                     inner join SOCIOS Soc on Ex.Cedula = Soc.Cedula
			                     inner join FSL_PLANES Pl on Ex.COD_PLAN = Pl.COD_PLAN
			                     inner join FSL_PLANES_CAUSAS Pc on Ex.COD_PLAN = Pc.COD_PLAN and Ex.COD_CAUSA = Pc.COD_CAUSA
			                     inner join FSL_TIPOS_ENFERMEDADES Te on Ex.COD_ENFERMEDAD = Te.COD_ENFERMEDAD
			                     inner join FSL_COMITES Co on Ex.COD_COMITE = Co.COD_COMITE 
                                    Where Ex.COD_EXPEDIENTE = {cod_expediente} ";

                    response.Result = connection.Query<FslExpedienteDatos>(query).FirstOrDefault();

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
        /// Inserta un expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="jsonExp"></param>
        /// <returns></returns>
        public ErrorDto FslExpediente_Insertar(int CodCliente, string jsonExp)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            FslExpedienteDatos expediente = new FslExpedienteDatos();
            expediente = JsonConvert.DeserializeObject<FslExpedienteDatos>(jsonExp);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                if (expediente.notas == null)
                {
                    resp.Code = -1;
                    resp.Description = "- Indique una Nota v�lida!";
                    return resp;
                }
                else
                {
                    if (expediente.notas.Length <= 10)
                    {
                        resp.Code = -1;
                        resp.Description = "- Notas del expediente debe tener m�s de 10 letras!";
                        return resp;
                    }
                }

                if (fxValida(CodCliente, expediente.cedula, expediente.cod_plan, expediente.cod_causa, ref resp))
                {
                    string vTipoDesembolso = fxPlanTipoDesembolso(CodCliente, expediente.cod_plan);

                    using var connection = new SqlConnection(clienteConnString);
                    {
                        var query = $@"insert FSL_EXPEDIENTES(
											COD_EXPEDIENTE,
											CEDULA, 
											COD_PLAN, 
											COD_CAUSA,
											COD_COMITE,
											COD_ENFERMEDAD,
											ESTADO,
											RESOLUCION_ESTADO, 
											PRESENTA_CEDULA, 
											PRESENTA_NOMBRE, 
											PRESENTA_NOTAS, 
											REFERENCIA_DOCUMENTO, 
											REFERENCIA_NUMERO, 
											ENFERMEDAD_FECHA,
											ENFERMEDAD_USUARIO, 
											ENFERMEDAD_NOTAS, 
											FECHA_ESTABLECE_CAUSA, 
											NOTAS, 
											TOTAL_DISPONIBLE, 
											TOTAL_APLICADO, 
											TOTAL_SOBRANTE, 
											REGISTRO_FECHA, 
											REGISTRO_USUARIO, 
											TIPO_DESEMBOLSO)
												   VALUES(
											dbo.fxFSL_ExpedienteConsecutivo(),
											'{expediente.cedula}',
											'{expediente.cod_plan}',
											'{expediente.cod_causa}',
											'{expediente.cod_comite}',
											'{expediente.cod_enfermedad}',
											'P',
											'P',
                                            '{expediente.presenta_cedula}',
											'{expediente.presenta_nombre}',
											'{expediente.presenta_notas}',
											'{expediente.referencia_documento}',
											'{expediente.referencia_numero}',
											'{expediente.enfermedad_fecha}',
											'{expediente.enfermedad_usuario}',
											'{expediente.enfermedad_notas}',
											'{expediente.fecha_establece_causa}',
											'{expediente.notas}',
											0,
											0,
											0,
											getdate(),
											'{expediente.registro_usuario}',
											'{vTipoDesembolso}')";

                        var result = connection.Execute(query);

                        long vCodExpediente = fxExpedienteConsecutivo(CodCliente, expediente.cedula);


                        //Actualiza Requisitos
                        spFSL_ExpedienteRequisitos(CodCliente, vCodExpediente, expediente.registro_usuario, resp);
                        if (resp.Code != 0)
                        {
                            return resp;
                        }
                        //Actualiza Calculos de Creditos (FOSOL)
                        spFSL_ExpedienteOperaciones(CodCliente, vCodExpediente, expediente.registro_usuario, resp);
                        if (resp.Code != 0)
                        {
                            return resp;
                        }

                        resp.Description = vCodExpediente.ToString();

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
        /// Actualiza los datos del expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="jsonExp"></param>
        /// <returns></returns>
		public ErrorDto FslExpediente_Actualizar(int CodCliente, string jsonExp)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            FslExpedienteDatos expediente = new FslExpedienteDatos();
            expediente = JsonConvert.DeserializeObject<FslExpedienteDatos>(jsonExp) ?? new FslExpedienteDatos();
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {

                if (expediente.estado != "P")
                {
                    resp.Code = -1;
                    resp.Description = "No se puede modificar este tr�mite porque no se encuentra pendiente";
                }

                string vTipoDesembolso = fxPlanTipoDesembolso(CodCliente, expediente.cod_plan);

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"update FSL_EXPEDIENTES set 
	                                COD_PLAN = '{expediente.cod_plan}',
	                                COD_CAUSA = '{expediente.cod_causa}',
	                                COD_COMITE ='{expediente.cod_comite}',
	                                COD_ENFERMEDAD = '{expediente.cod_enfermedad}',
	                                notas = '{expediente.notas}',
	                                PRESENTA_CEDULA = '{expediente.presenta_cedula}',
	                                PRESENTA_NOMBRE = '{expediente.presenta_nombre}',
	                                REFERENCIA_DOCUMENTO = '{expediente.referencia_documento}', 
	                                REFERENCIA_NUMERO = '{expediente.referencia_numero}',
	                                PRESENTA_NOTAS = '{expediente.presenta_notas}',
	                                FECHA_ESTABLECE_CAUSA = '{expediente.fecha_establece_causa}',
	                                ENFERMEDAD_FECHA = '{expediente.enfermedad_fecha}',
	                                ENFERMEDAD_NOTAS = '{expediente.enfermedad_notas}',
	                                MODIFICA_USUARIO = '{expediente.modifica_usuario}',
	                                MODIFICA_FECHA = GETDATE(), 
	                                TIPO_DESEMBOLSO = '{vTipoDesembolso}'
	                                where COD_EXPEDIENTE = {expediente.cod_expediente}";

                    var result = connection.Execute(query);

                }

                //Actualiza Requisitos
                spFSL_ExpedienteRequisitos(CodCliente, expediente.cod_expediente, expediente.registro_usuario, resp);
                if (resp.Code != 0)
                {
                    return resp;
                }
                //Actualiza Calculos de Creditos (FOSOL)
                spFSL_ExpedienteOperaciones(CodCliente, expediente.cod_expediente, expediente.registro_usuario, resp);
                if (resp.Code != 0)
                {
                    return resp;
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
        /// Obtiene el tipo de desembolso del plan
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_plan"></param>
        /// <returns></returns>
        private string fxPlanTipoDesembolso(int CodCliente, string cod_plan)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            string respuesta = "";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select TIPO_DESEMBOLSO
                                      from FSL_PLANES where cod_plan = '{cod_plan}'";

                    respuesta = connection.Query<string>(query).FirstOrDefault();

                }
            }
            catch (Exception)
            {
                respuesta = null;
            }
            return respuesta;
        }

        /// <summary>
        /// Obteniene el consecutivo del expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private long fxExpedienteConsecutivo(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            long respuesta = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select isnull(max(cod_Expediente),0) as 'Ultimo'
                                      from FSL_Expedientes where Cedula = '{cedula}'";

                    respuesta = connection.Query<long>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                respuesta = 0;
                _ = ex.Message;
            }
            return respuesta;
        }
        /// <summary>
        /// Actualiza requisitos del expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Expediente"></param>
        /// <param name="Usuario"></param>
        /// <param name="info"></param>
        private void spFSL_ExpedienteRequisitos(int CodCliente, long Expediente, string Usuario, ErrorDto info)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "[spFSL_ExpedienteRequisitos]";
                    var values = new
                    {
                        Expediente = Expediente,
                        Usuario = Usuario,
                    };

                    connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
        }

        /// <summary>
        /// Actualiza Calculos de Creditos (FOSOL)
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Expediente"></param>
        /// <param name="Usuario"></param>
        /// <param name="info"></param>
        private void spFSL_ExpedienteOperaciones(int CodCliente, long Expediente, string Usuario, ErrorDto info)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "[spFSL_ExpedienteOperaciones]";
                    var values = new
                    {
                        Expediente = Expediente,
                        Usuario = Usuario,
                    };

                    connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
        }

        /// <summary>
        /// Valida si el expediente ya fue presentado
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <param name="tipo"></param>
        /// <param name="causa"></param>
        /// <returns></returns>
        public ErrorDto FslExpediente_Valida(int CodCliente, string cedula, string tipo, string causa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select dbo.fxFSL_ExpedienteValidaRegistro(
                                        '{cedula}',
                                        '{tipo}',
                                        '{causa}',0) as 'Cumple'";

                    long info = connection.Query<long>(query).FirstOrDefault();

                    if (info == 0)
                    {
                        resp.Code = -1;
                        resp.Description = "- El caso ya fue presentado anteriormente...verifique!";
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
        /// Obtiene los requisitos del expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_expediente"></param>
        /// <returns></returns>
        public ErrorDto<List<FslRequisitosExp>> FslRequisitos_Obtener(int CodCliente, int cod_expediente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslRequisitosExp>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select Ex.COD_REQUISITO,Rq.DESCRIPCION, EX.Estado, Ex.Opcional 
                                       from FSL_EXPEDIENTES_REQUISITOS Ex 
                                        inner join FSL_REQUISITOS Rq on Ex.cod_requisito = Rq.cod_requisito
                                       where Ex.cod_Expediente = {cod_expediente}";
                    response.Result = connection.Query<FslRequisitosExp>(query).ToList();
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
        /// Obtiene las operaciones del expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_expediente"></param>
        /// <returns></returns>
        public ErrorDto<List<FslOperacionesDatos>> FslOperaciones_Obtener(int CodCliente, int cod_expediente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslOperacionesDatos>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select E.ID_SOLICITUD, E.REFERENCIA, Gar.DESCRIPCION, R.PRIDEDUC, R.MONTOAPR 
                                      , E.SALDO_CORTE , E.MONTO_BASE, E.PORC_RELACION , E.TIPO_TABLA , E.PORCENTAJE
                                      , E.MONTO_RECONOCIMIENTO, E.TIEMPO_TRANS, case when E.Tipo_Base = 'S' then 'Saldo' else 'Mnt.Form.' end as '_BASE'
                                    from FSL_EXPEDIENTES_DETALLE  E inner join REG_CREDITOS R on E.ID_SOLICITUD = R.ID_SOLICITUD
                                    inner join CRD_GARANTIA_TIPOS Gar on R.GARANTIA = Gar.GARANTIA
                                    Where E.COD_EXPEDIENTE = {cod_expediente} Order by isnull(E.referencia,E.id_Solicitud) desc";
                    response.Result = connection.Query<FslOperacionesDatos>(query).ToList();
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
        /// Obtener la resolucion del expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_expediente"></param>
        /// <returns></returns>
        public ErrorDto<List<FslResolucionDatos>> FslResolucion_Obtener(int CodCliente, int cod_expediente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslResolucionDatos>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select Cm.Cedula,Cm.Nombre,isnull(Ec.Asigna_Usuario,'No!') as 'ASIGNADO'
                                       from FSL_EXPEDIENTES Ex
                                        inner join FSL_COMITES_MIEMBROS Cm on Ex.COD_COMITE = Cm.COD_COMITE
                                         left join FSL_EXPEDIENTE_COMITE Ec on Ex.COD_EXPEDIENTE = Ec.COD_EXPEDIENTE and Ex.COD_COMITE = Ec.COD_COMITE
                                               and Cm.Cedula = Ex.Cedula
                                       where Ex.cod_Expediente = {cod_expediente} and Cm.Activo = 1";
                    response.Result = connection.Query<FslResolucionDatos>(query).ToList();
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
        /// Obtiene las validaciones de la resolucion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_expediente"></param>
        /// <returns></returns>
        public ErrorDto<List<FslResolucionValidacionesDatos>> FslResolucionlVal_Obtener(int CodCliente, int cod_expediente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslResolucionValidacionesDatos>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@" select dbo.fxFSL_ExpedienteValidaRequisitos(Ex.Cod_Expediente) as 'CumpleRequisitos'
                                        , dbo.fxFSL_ExpedienteValidaTiempoPresentacion(Ex.Cod_Expediente) as 'CumpleTiempo'
                                        , dbo.fxFSL_ExpedienteValidaRegistro(Ex.Cedula, Ex.Cod_Plan, Ex.Cod_Causa,Ex.Cod_Expediente) as 'CumpleRegistro'
                                         from FSL_EXPEDIENTES Ex
                                         Where Ex.COD_EXPEDIENTE = {cod_expediente}";
                    response.Result = connection.Query<FslResolucionValidacionesDatos>(query).ToList();
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
        /// Obtiene las gestiones del expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_expediente"></param>
        /// <returns></returns>
        public ErrorDto<List<FslExpGestiones>> FslExpGestiones_Obtener(int CodCliente, int cod_expediente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslExpGestiones>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Tg.Descripcion, Eg.*
                                    from FSL_EXPEDIENTE_GESTIONES Eg inner join FSL_TIPOS_GESTIONES Tg on Eg.COD_GESTION = Tg.COD_GESTION
                                    Where Eg.cod_Expediente = {cod_expediente} order by registro_fecha desc";
                    response.Result = connection.Query<FslExpGestiones>(query).ToList();
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
        /// Obtiene las apelaciones del expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_expediente"></param>
        /// <returns></returns>
        public ErrorDto<List<FslApelacionDatos>> FslApelaciones_Obtener(int CodCliente, int cod_expediente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslApelacionDatos>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Ta.Descripcion, Ea.*
                                    from FSL_EXPEDIENTES_APELACIONES Ea inner join FSL_TIPOS_APELACIONES Ta on Ea.COD_APELACION = Ta.COD_APELACION
                                    Where Ea.cod_Expediente = {cod_expediente} order by registra_fecha desc";
                    response.Result = connection.Query<FslApelacionDatos>(query).ToList();
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
        /// Obtiene la lista de expedientes
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<FslExpedienteListaData> FslExpedientesLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<FslExpedienteListaData>();
            response.Result = new FslExpedienteListaData();
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "Select count(COD_EXPEDIENTE) From FSL_EXPEDIENTES";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE  COD_EXPEDIENTE LIKE '%" + filtro + "%' " +
                            " OR CEDULA LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"select COD_EXPEDIENTE,CEDULA from FSL_EXPEDIENTES
                                         {filtro} 
                                       ORDER BY COD_EXPEDIENTE
                                        {paginaActual}
                                        {paginacionActual} ";
                    response.Result.expediente = connection.Query<FslExpedienteData>(query).ToList();
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
        /// Actualiza el estado de un requisito
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="requisito"></param>
        /// <returns></returns>
        public ErrorDto FslExpRequisto_Actualizar(int CodCliente, FslExpedienteUpdate requisito)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int activo = requisito.estado ? 1 : 0;
                    var query = $@"update FSL_EXPEDIENTES_REQUISITOS set Estado = '{activo}', registro_fecha = getdate(), registro_usuario = '{requisito.registro_usuario}'
		                                            where cod_expediente = {requisito.cod_expediente} and Cod_Requisito = '{requisito.cod_Requisito}' ";
                    var result = connection.Execute(query);
                    resp.Description = "Requisito actualizado satisfactoriamente!";
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
        /// Valida el usuario del comite
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto FslMiembroValida(int CodCliente, FslMiembroValida usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                //  using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "[spSEG_Logon]";
                    //    var procedure = "[spSEGLogon]";
                    var values = new
                    {
                        Usuario = usuario.usuario,
                        Clave = usuario.clave
                    };

                    int res = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    if (res == 0)
                    {
                        info.Code = -1;
                        info.Description = "No fue posible validar al usuario.: Verifique su contrase�a!";
                    }
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        public ErrorDto FslUsuarioVinculado_Obtener(int CodCliente, string cedula, string cod_comite)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select usuario_Vinculado from FSL_Comites_Miembros 
                                    where cedula = '{cedula}'  and cod_comite = '{cod_comite}' ";
                    resp.Description = connection.Query<string>(query).LastOrDefault();
                }
            }
            catch (Exception)
            {
                resp.Code = -1;
                resp.Description = "No fue posible obtener el usuario vinculado";
            }
            return resp;
        }

        public ErrorDto FslResolucion_Guardar(int CodCliente, FslResolucionGuardar resolucion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                string query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select NUMERO_RESOLUTORES from FSL_Comites
		                            where cod_Comite = '{resolucion.cod_comite}'";

                    int vNumResolutores = connection.Query<int>(query).FirstOrDefault();

                    if (resolucion.miembros.Count < vNumResolutores)
                    {
                        info.Code = -1;
                        info.Description = $"Debe de indicar al menos ({vNumResolutores}) miembros del comit� VALIDADOS! que den la resoluci�n!";
                        return info;
                    }

                    query = $@"update FSL_EXPEDIENTES set RESOLUCION_NOTAS = '{resolucion.resolucion_notas}',RESOLUCION_ESTADO = '{resolucion.resolucion_estado}', RESOLUCION_FECHA = getdate()
                                           ,RESOLUCION_USUARIO = '{resolucion.resolucion_usuario}',ESTADO = '{resolucion.resolucion_estado}' where COD_EXPEDIENTE = {resolucion.cod_expediente}";

                    var result = connection.Execute(query);

                    query = $@"delete FSL_EXPEDIENTE_COMITE WHERE COD_EXPEDIENTE = {resolucion.cod_expediente} ";
                    result = connection.Execute(query);

                    foreach (var item in resolucion.miembros)
                    {
                        query = $@"INSERT FSL_EXPEDIENTE_COMITE(COD_EXPEDIENTE,COD_COMITE,CEDULA,ASIGNA_FECHA,ASIGNA_USUARIO,RESOLUCION_ESTADO)
					   values({resolucion.cod_expediente},'{resolucion.cod_comite}','{item.cedula}',getdate(),'{resolucion.resolucion_usuario}','{resolucion.estado}')";

                        result = connection.Execute(query);

                    }

                    info.Description = "Expediente actualizado satisfactoriamente...";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        private bool fxValida(int CodCliente, string cedula, string cod_plan, string cod_comite, ref ErrorDto error)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool info = false;
            error = new ErrorDto();
            error.Code = 0;
            try
            {
                if (cod_plan == null || cod_comite == null)
                {
                    error.Code = -1;
                    error.Description = "\n-Faltan datos por llenar.";
                    return false;
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select dbo.fxFSL_ExpedienteValidaRegistro('{cedula}','{cod_plan}','{cod_comite}',0) as 'Cumple'";
                    string resp = connection.Query<string>(query).FirstOrDefault();
                    if (resp == "0")
                    {
                        error.Code = -1;
                        error.Description = "\n- El caso ya fue presentado anteriormente...verifique!";
                    }
                    else
                    {
                        info = true;
                    }
                }
            }
            catch (Exception ex)
            {
                info = false;
                error.Code = -1;
                error.Description = ex.Message;

            }
            return info;
        }

        public ErrorDto FslExpediente_Aplicar(int CodCliente, long cod_expediente, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "[spFSL_AplicacionFosol]";
                    var values = new
                    {
                        Expediente = cod_expediente,
                        Usuario = usuario
                    };

                    var res = connection.Query<dynamic>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    if (res == null)
                    {
                        info.Code = -1;
                        info.Description = "No fue posible aplicar la operaci�n";
                    }
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }



    }
}