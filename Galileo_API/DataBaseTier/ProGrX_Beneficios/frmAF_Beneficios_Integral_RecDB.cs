using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class AF_Beneficios_Integral_RecDB
    {
        private readonly IConfiguration _config;
        private mProGrX_AuxiliarDB mAuxiliarDB;

        public AF_Beneficios_Integral_RecDB(IConfiguration config)
        {
            _config = config;
            mAuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Obtengo los datos del reconocimiento de un beneficio
        /// Beneficio Integral / DropDown Reconocimientos / Tab Reconocimientos
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_beneficio"></param>
        /// <returns></returns>
        public ErrorDTO<AfiBeneReconocimientosDatos> BeneReconocimiento_Obtener(int CodCliente, int id_beneficio)
        {
            
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AfiBeneReconocimientosDatos>();
            try
            {
                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"exec spAFI_Bene_Socio_Reconocimiento_Consultar {id_beneficio} ";
                    response.Result = db.Query<AfiBeneReconocimientosDatos>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneReconocimiento_Obtener: " + ex.Message;
                response.Result = null;
            }
            return response;

        }

        /// <summary>
        /// Guardo el reconocimiento de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="reconocimiento"></param>
        /// <returns></returns>
        public ErrorDTO BeneReconocimiento_Guardar(int CodCliente, AfiBeneReconocimientos reconocimiento)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO error = new ErrorDTO();
            int existeReco = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                { 
                    var query = $@"SELECT COUNT(*)
                    FROM AFI_BENE_REGISTRO_RECONOCIMIENTOS R
                    LEFT JOIN AFI_BENE_OTORGA O ON O.ID_BENEFICIO = R.ID_BENEFICIO 
                    WHERE R.CEDULA_ESTUDIANTE = '{reconocimiento.cedula_estudiante}' 
                    AND YEAR(R.RECONOCIMIENTO_FECHA) = YEAR(GETDATE()) 
                    AND O.ESTADO IN (SELECT COD_ESTADO FROM AFI_BENE_ESTADOS WHERE P_FINALIZA = 1 AND PROCESO = 'A')
                    AND O.ID_BENEFICIO != '{reconocimiento.id_beneficio}'";
                    existeReco = connection.Query<int>(query).FirstOrDefault();
                }
                if (existeReco > 0)
                {
                    error.Code = -1;
                    error.Description = "El estudiante con la c�dula "+reconocimiento.cedula_estudiante+" ya tiene un reconomiento asignado.";
                    return error;
                }
                //Si el id es 0 es un insert, si no es un update
                if (reconocimiento.id_reconocimiento != 0)
                {
                    error = BeneReconocimiento_Actualizar(CodCliente, reconocimiento);
                }
                else
                {
                    error = BeneReconocimiento_Ingresar(CodCliente, reconocimiento);
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = "BeneReconocimiento_Guardar : " + ex.Message;
            }
            return error;
        }

        /// <summary>
        /// Creo un nuevo reconocimiento de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="reconocimiento"></param>
        /// <returns></returns>
        public ErrorDTO BeneReconocimiento_Ingresar(int CodCliente, AfiBeneReconocimientos reconocimiento)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO error = new ErrorDTO();

            string fecha_nacimiento = mAuxiliarDB.validaFechaGlobal(reconocimiento.fecha_nacimiento); 

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string genero = reconocimiento.genero == null ? "" : reconocimiento.genero.item;
                    string tipo_centro = reconocimiento.tipo_centro == null ? "" : reconocimiento.tipo_centro.item;
                    string nivel_academico = reconocimiento.nivel_academico == null ? "" : reconocimiento.nivel_academico.item;
                    string grado = reconocimiento.grado == null ? "" : reconocimiento.grado.item;
                    string tipo_reconocimiento = reconocimiento.tipo_reconocimiento == null ? "" : reconocimiento.tipo_reconocimiento.item;
                    string rango = reconocimiento.rango == null ? "" : reconocimiento.rango.item;
                    string reconocimiento_etapa = reconocimiento.reconocimiento_etapa == null ? "" : reconocimiento.reconocimiento_etapa.item;
                    string reconocimiento_nivel = reconocimiento.reconocimiento_nivel == null ? "" : reconocimiento.reconocimiento_nivel.item;

                    var Query = $@"INSERT INTO [dbo].[AFI_BENE_REGISTRO_RECONOCIMIENTOS]
                                       ([COD_BENEFICIO]
                                       ,[CONSEC]
                                       ,[ID_BENEFICIO]
                                       ,[CEDULA_ESTUDIANTE]
                                       ,[FECHA_NACIMIENTO]
                                       ,[EDAD]
                                       ,[GENERO]
                                       ,[PRIMER_APELLIDO]
                                       ,[SEGUNDO_APELLIDO]
                                       ,[NOMBRE]
                                       ,[TIPO_CENTRO]
                                       ,[CENTRO_EDUCATIVO]
                                       ,[NIVEL_ACADEMICO]
                                       ,[GRADO]
                                       ,[OBSERVACIONES]
                                       ,[TIPO_RECONOCIMIENTO]
                                       ,[MATEMATICAS]
                                       ,[CIENCIAS]
                                       ,[ESTUDIOS_SOCIALES]
                                       ,[ESPANOL]
                                       ,[IDIOMA]
                                       ,[RANGO]
                                       ,[RECONOCIMIENTO_ETAPA]
                                       ,[RECONOCIMIENTO_FECHA]
                                       ,[RECONOCIMIENTO_NIVEL]
                                       ,[REGISTRO_FECHA]
                                       ,[REGISTRO_USUARIO] )
                                 VALUES
                                       ('{reconocimiento.cod_beneficio}'
                                       ,{reconocimiento.consec}
                                       ,{reconocimiento.id_beneficio}
                                       ,'{reconocimiento.cedula_estudiante}'
                                       ,'{fecha_nacimiento}'
                                       ,{reconocimiento.edad}
                                       ,'{genero}'
                                       ,'{reconocimiento.primer_apellido}'
                                       ,'{reconocimiento.segundo_apellido}'
                                       ,'{reconocimiento.nombre}'
                                       ,'{tipo_centro}'
                                       ,'{reconocimiento.centro_educativo}'
                                       ,'{nivel_academico}'
                                       ,'{grado}'
                                       ,'{reconocimiento.observaciones}'
                                       ,'{tipo_reconocimiento}'
                                       ,{reconocimiento.matematicas}
                                       ,{reconocimiento.ciencias}
                                       ,{reconocimiento.estudios_sociales}
                                       ,{reconocimiento.espanol}
                                       ,{reconocimiento.idioma}
                                       ,'{rango}'
                                       ,'{reconocimiento_etapa}'
                                       ,'{reconocimiento.reconocimiento_fecha}'
                                       ,'{reconocimiento_nivel}'
                                       ,getDate()
                                       ,'{reconocimiento.registro_usuario}'
                                    )";

                    error.Code = connection.Execute(Query);

                    Query = "SELECT IDENT_CURRENT('AFI_BENE_REGISTRO_RECONOCIMIENTOS') as 'id'";
                    reconocimiento.id_reconocimiento = connection.Query<int>(Query).FirstOrDefault();

                    error.Description = reconocimiento.id_reconocimiento.ToString();
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }

        /// <summary>
        /// Actualizo el reconocimiento de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="reconocimiento"></param>
        /// <returns></returns>
        private ErrorDTO BeneReconocimiento_Actualizar(int CodCliente, AfiBeneReconocimientos reconocimiento)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO error = new ErrorDTO();


            string fecha_nacimiento = mAuxiliarDB.validaFechaGlobal(reconocimiento.fecha_nacimiento);

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string genero = reconocimiento.genero == null ? "" : reconocimiento.genero.item;
                    string tipo_centro = reconocimiento.tipo_centro == null ? "" : reconocimiento.tipo_centro.item;
                    string nivel_academico = reconocimiento.nivel_academico == null ? "" : reconocimiento.nivel_academico.item;
                    string grado = reconocimiento.grado == null ? "" : reconocimiento.grado.item;
                    string tipo_reconocimiento = reconocimiento.tipo_reconocimiento == null ? "" : reconocimiento.tipo_reconocimiento.item;
                    string rango = reconocimiento.rango == null ? "" : reconocimiento.rango.item;
                    string reconocimiento_etapa = reconocimiento.reconocimiento_etapa == null ? "" : reconocimiento.reconocimiento_etapa.item;
                    string reconocimiento_nivel = reconocimiento.reconocimiento_nivel == null ? "" : reconocimiento.reconocimiento_nivel.item;


                    var Query = $@"UPDATE [dbo].[AFI_BENE_REGISTRO_RECONOCIMIENTOS]
                                   SET [COD_BENEFICIO] = '{reconocimiento.cod_beneficio}'
                                      ,[CEDULA_ESTUDIANTE] = '{reconocimiento.cedula_estudiante}'
                                      ,[FECHA_NACIMIENTO] = '{fecha_nacimiento}' 
                                      ,[EDAD] = {reconocimiento.edad}
                                      ,[GENERO] = '{genero}'
                                      ,[PRIMER_APELLIDO] = '{reconocimiento.primer_apellido}'
                                      ,[SEGUNDO_APELLIDO] = '{reconocimiento.segundo_apellido}'
                                      ,[NOMBRE] = '{reconocimiento.nombre}'
                                      ,[TIPO_CENTRO] = '{tipo_centro}'
                                      ,[CENTRO_EDUCATIVO] = '{reconocimiento.centro_educativo}'
                                      ,[NIVEL_ACADEMICO] = '{nivel_academico}'
                                      ,[GRADO] = '{grado}'
                                      ,[OBSERVACIONES] = '{reconocimiento.observaciones}'
                                      ,[TIPO_RECONOCIMIENTO] = '{tipo_reconocimiento}'
                                      ,[MATEMATICAS] = {reconocimiento.matematicas}
                                      ,[CIENCIAS] = {reconocimiento.ciencias}
                                      ,[ESTUDIOS_SOCIALES] = {reconocimiento.estudios_sociales}
                                      ,[ESPANOL] = {reconocimiento.espanol}
                                      ,[IDIOMA] = {reconocimiento.idioma}
                                      ,[RANGO] = '{rango}'
                                      ,[RECONOCIMIENTO_ETAPA] = '{reconocimiento_etapa}'
                                      ,[RECONOCIMIENTO_FECHA] = '{reconocimiento.reconocimiento_fecha}'
                                      ,[RECONOCIMIENTO_NIVEL] = '{reconocimiento_nivel}'
                                      ,[MODIFICA_FECHA] = getDate()
                                      ,[MODIFICA_USUARIO] = '{reconocimiento.modifica_usuario}'
                                 WHERE  id_reconocimiento = '{reconocimiento.id_reconocimiento}' ";

                    error.Code = connection.Execute(Query);
                    error.Description = reconocimiento.id_reconocimiento.ToString();
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }

        /// <summary>
        /// Rechazo el reconocimiento de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_beneficio"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO BeneReconocimiento_Rechazar(int CodCliente, int id_beneficio, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"UPDATE AFI_BENE_OTORGA
                    SET ESTADO = 'R', MODIFICA_USUARIO = '{usuario}', MODIFICA_FECHA = GETDATE() 
                    WHERE id_beneficio = {id_beneficio}";

                    response.Code = connection.Execute(query);
                    response.Description = "Expediente rechazado correctamente";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneReconocimiento_Rechazar: " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Valido si el estudiante ya tiene un reconocimiento de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <param name="id_beneficio"></param>
        /// <returns></returns>
        public ErrorDTO ValidaEstudiante_Obtener(int CodCliente, string cedula, string id_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT CONCAT(O.ID_BENEFICIO, TRIM(O.COD_BENEFICIO) ,FORMAT(O.CONSEC, '00000'), '- C�dula: ', O.CEDULA )
                                            FROM AFI_BENE_REGISTRO_RECONOCIMIENTOS R
                                            LEFT JOIN AFI_BENE_OTORGA O ON O.ID_BENEFICIO = R.ID_BENEFICIO 
                                            WHERE R.CEDULA_ESTUDIANTE = '{cedula}' 
                                            AND YEAR(O.REGISTRA_FECHA) = YEAR(GETDATE()) 
                                            AND DATEDIFF(YEAR, O.REGISTRA_FECHA, GETDATE()) <= 1
                                            AND O.ID_BENEFICIO != '{id_beneficio}' ";

                    var lista = connection.Query<string>(query).ToList();

                    if(lista.Count > 0)
                    {
                        response.Code = -1;
                        response.Description = "El estudiante ya esta registrado en ";
                        foreach (var item in lista)
                        {
                            response.Description += item + " ";
                        }   
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ValidaEstudiante_Obtener: " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Valido la nota minima para el beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO ValidaNotaMinima(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var  resp = new ErrorDTO();
            try
            {
                string notaMinima = _config.GetSection("AFI_Beneficios").GetSection("NotaMinima").Value.ToString();
                using var connection = new SqlConnection(clienteConnString);
                {
                    
                    var query = $@"SELECT VALOR FROM [SIF_PARAMETROS] WHERE COD_PARAMETRO = '{notaMinima}' ";
                    var nota = connection.Query<float>(query).FirstOrDefault();

                    if(nota != null)
                    {
                        resp.Description = nota.ToString();
                    }
                    else
                    {
                        resp.Description = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "ValidaNotaMinima: " + ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Valido la nota minima para pasar la materia
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO ValidaNotaPasaMateria(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDTO();
            try
            {
                string notaMinima = _config.GetSection("AFI_Beneficios").GetSection("NotaPasaAnho").Value.ToString();
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT VALOR FROM [SIF_PARAMETROS] WHERE COD_PARAMETRO = '{notaMinima}' ";
                    var nota = connection.Query<float>(query).FirstOrDefault();

                    if (nota != null)
                    {
                        resp.Description = nota.ToString();
                    }
                    else
                    {
                        resp.Description = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "ValidaNotaPasaMateria: " + ex.Message;
            }
            return resp;
        }

    }
}