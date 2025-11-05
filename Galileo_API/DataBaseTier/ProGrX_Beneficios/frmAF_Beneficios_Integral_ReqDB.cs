using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.GA;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class AF_Beneficios_Integral_ReqDB
    {
        private readonly IConfiguration _config;
        private readonly mBeneficiosDB _mBeneficiosDB;

        public AF_Beneficios_Integral_ReqDB(IConfiguration config)
        {
            _config = config;
            _mBeneficiosDB = new mBeneficiosDB(config);
        }

        /// <summary>
        /// Obtengo la lista de requisitos para el formulario de registro de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="consec"></param>
        /// <returns></returns>
        public ErrorDTO<List<BENE_REG_REQUISITO>> Bene_Registro_Requisitos_Obtener(int CodCliente, int consec)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<BENE_REG_REQUISITO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = $"[spAFI_Bene_Registro_Requisitos_List]";
                    var values = new
                    {
                        Consec_Bene = consec,
                    };

                    response.Result = connection.Query<BENE_REG_REQUISITO>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Bene_Registro_Requisitos_Obtener: " + ex.Message;
                response.Result = null!;
            }
            return response;
        }

        /// <summary>
        /// Guardo el registro de los requisitos del beneficio NOTA: Los documentos se envian a GA.
        /// </summary>
        /// <param name="requisito"></param>
        /// <returns></returns>
        public ErrorDTO BeneRegistroRequisitos_Guardar(BeneRequisitosGuardar requisito)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(requisito.codCliente);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var Query = $@"INSERT INTO AFI_BENE_REGISTRO_REQUISITOS
                                       ([COD_BENEFICIO]
                                       ,[CONSEC]
                                       ,[COD_REQUISITO]
                                       ,[REGISTRO_FECHA]
                                       ,[REGISTRO_USUARIO])
                                 VALUES
                                       ('{requisito.cod_beneficio}'
                                       ,{requisito.consec}
                                       ,'{requisito.cod_requisito}'
                                       ,getDate()
                                       ,'{requisito.usuario}')";

                    resp.Code = connection.Execute(Query);

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                    {
                        EmpresaId = requisito.codCliente,
                        cod_beneficio = requisito.cod_beneficio,
                        consec = requisito.consec,
                        movimiento = "Actualiza",
                        detalle = $@"Se cargo Requisito COD: [{requisito.cod_requisito}]",
                        registro_usuario = requisito.usuario
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "BeneRegistroRequisitos_Guardar: " + ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Elimino el registro de los requisitos del beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_beneficio"></param>
        /// <param name="consec"></param>
        /// <param name="cod_requisito"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO BeneRegistroRequisitos_Eliminar(int CodCliente,string cod_beneficio, int consec, string cod_requisito, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO resp = new ErrorDTO();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    if(cod_requisito != null)
                    {
                        var query = @$"DELETE [dbo].[AFI_BENE_REGISTRO_REQUISITOS] 
                         WHERE COD_REQUISITO = '{cod_requisito}' AND CONSEC = '{consec}' AND COD_BENEFICIO = '{cod_beneficio}'";

                        resp.Code = connection.Execute(query);

                        _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            cod_beneficio = cod_beneficio,
                            consec = consec,
                            movimiento = "Actualiza",
                            detalle = $@"Se elimina Requisito COD: [{cod_requisito}]",
                            registro_usuario = usuario
                        });
                    }
                    
                   

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "BeneRegistroRequisitos_Eliminar: " + ex.Message;
            }

            return resp;

        }

        /// <summary>
        /// Asocio el archivo a un requisito del beneficio
        /// </summary>
        /// <param name="modulo"></param>
        /// <param name="TypeId"></param>
        /// <param name="requisito"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDTO BeneRegistroRequisito_Asociar(
            string modulo, 
            string TypeId, string requisito, DocumentosArchivoDTO data)
        {
            ErrorDTO resp = new();
            try
            {
                BeneRequisitosGuardar beneRequisitos = Newtonsoft.Json.JsonConvert.DeserializeObject<BeneRequisitosGuardar>(requisito);
                using (var connection = new SqlConnection(_config.GetConnectionString("GAConnString")))
                {
                    //valido si existen registros con la misma llave
                    var queryValida = $@"SELECT Count(*) FROM GA_Files WHERE Llave_01 = '{data.llave_01}' AND Llave_02 = '{data.llave_02}' AND Llave_03 = '{beneRequisitos.cod_requisito}'";
                    var result = connection.Query<int>(queryValida).FirstOrDefault();
                    //si no existe registro con la misma llave

                    if (result > 0)
                    {
                        string queryChange = $@"UPDATE GA_Files SET 
                                                TypeId = '999' ,
                                                ModuloId = 'CL_01',
                                                Llave_03 = '{data.llave_03}'
                                            WHERE Llave_01 = '{data.llave_01}' 
                                              AND Llave_02 = '{data.llave_02}' 
                                              AND Llave_03 = '{beneRequisitos.cod_requisito}'";
                        resp.Code = connection.Execute(queryChange);

                        BeneRegistroRequisitos_Eliminar(beneRequisitos.codCliente, beneRequisitos.cod_beneficio, beneRequisitos.consec, beneRequisitos.cod_requisito, beneRequisitos.usuario);
                    }
                    

                    string query = $@"UPDATE GA_Files SET 
                                                TypeId = '{TypeId}' ,
                                                ModuloId = '{modulo}',
                                                Llave_03 = '{beneRequisitos.cod_requisito}'
                                            WHERE Llave_01 = '{data.llave_01}' 
                                              AND Llave_02 = '{data.llave_02}' 
                                              AND Llave_03 = '{data.llave_03}'";
                    resp.Code = connection.Execute(query);

                    BeneRegistroRequisitos_Guardar(beneRequisitos);

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "BeneRegistroRequisito_Asociar: " + ex.Message;
            }
            return resp;
        }

    }
}