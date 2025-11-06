using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_Integral_SanDB
    {
        private readonly IConfiguration _config;
        private readonly mBeneficiosDB _mBeneficiosDB;

        public frmAF_Beneficios_Integral_SanDB(IConfiguration config)
        {
            _config = config;
            _mBeneficiosDB = new mBeneficiosDB(_config);
        }

        /// <summary>
        /// Cargo la lista disponibles a escojer en el formulario de sanciones
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public List<BeneficiosSancionesLista> BeneSancionMotivoLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            List<BeneficiosSancionesLista> info = new List<BeneficiosSancionesLista>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT [TIPO_SANCION] AS item
                                      ,[DESCRIPCION] AS descripcion
                                      ,[PLAZO_MAXIMO] AS plazo
                                      , CODIGO_COBRO 
                                  FROM [AFI_BENE_SANCIONES_TIPOS]
                                  WHERE ACTIVO = 1";
                    info = connection.Query<BeneficiosSancionesLista>(query).ToList();

                }
            }
            catch (Exception)
            {
                info = null;
            }

            return info;
        }

        /// <summary>
        /// Obtengo las sanciones del socio para mostrar en el formulario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiBeneSancionesDto>> BeneSacionesSocio_Obtener(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneSancionesDto>>();
            try
            {
                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"exec spAFI_Bene_Socio_Sanciones '{cedula}' ";

                    response.Result = db.Query<AfiBeneSancionesDto>(query).ToList();

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
        /// Guardo o actualizo la sancion del socio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="sancion"></param>
        /// <returns></returns>
        public ErrorDto BeneSancionesSocio_Guardar(int CodCliente, AfiBeneSancionesDto sancion)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {

                //Si el id es 0 es un insert, si no es un update
                if (sancion.sancion_id == 0)
                {

                    if (!BeneSancion_Insertar(CodCliente, sancion))
                    {
                        info.Code = -1;
                        info.Description = "Error al actualizar el dato";
                    }
                    else
                    {

                        info.Description = sancion.sancion_id.ToString();

                    }
                }
                else
                {
                    info = BeneSancion_Actualizar(CodCliente, sancion);
                }
            }

            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;


        }

        /// <summary>
        /// Inserto la sancion del socio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="sancion"></param>
        /// <returns></returns>
        private bool BeneSancion_Insertar(int CodCliente, AfiBeneSancionesDto sancion)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = sancion.activo ? 1 : 0;

                    var procedure = "[spAFI_Bene_Sancion_Registro]";
                    var values = new
                    {
                        Cedula = sancion.cedula,
                        TipoSancion = sancion.tipo_sancion,
                        Activo = activo,
                        Notas = sancion.notas,
                        FechaInicio = sancion.fecha_inicio,
                        FechaCorte = sancion.fecha_corte,
                        Monto = sancion.monto,
                        CodigoCobro = sancion.codigo_cobro,
                        Plazo = sancion.plazo,
                        NOperacion = sancion.n_operacion,
                        RegistroUsuario = sancion.registro_usuario

                    };

                    connection.Query<AfiBeneSancionesDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                    var query = $@"SELECT TOP 1 SANCION_ID FROM AFI_BENE_SANCIONES WHERE CEDULA = '{sancion.cedula}' ORDER BY SANCION_ID DESC";
                    sancion.sancion_id = connection.Query<int>(query).FirstOrDefault();

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = sancion.cod_beneficio,
                        consec = sancion.consec,
                        movimiento = "Inserta",
                        detalle = $@"Inserta datos Sanción COD: [{sancion.sancion_id}]",
                        registro_usuario = sancion.registro_usuario
                    });

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
     
            return true;
        }

        /// <summary>
        /// Actualizo la sancion del socio este metodo tambien funciona para activar o desactivar la sancion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="sancion"></param>
        /// <returns></returns>
        private ErrorDto BeneSancion_Actualizar(int CodCliente, AfiBeneSancionesDto sancion)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = sancion.activo ? 1 : 0;
                    int plazo = (sancion.plazo == null) ? 0 : (int)sancion.plazo ;

                    var query = $@"UPDATE [dbo].[AFI_BENE_SANCIONES]
                                   SET [TIPO_SANCION] = '{sancion.tipo_sancion}'
                                      ,[ACTIVO] = {activo}
                                      ,[NOTAS] = '{sancion.notas}'
                                      ,[MONTO] = '{sancion.monto}'
                                      ,[CODIGO_COBRO] = '{sancion.codigo_cobro}'
                                      ,[PLAZO] = {plazo}
                                      ,[MODIFICA_FECHA] = getDate()
                                      ,[MODIFICA_USUARIO] = '{sancion.registro_usuario}'
                                 WHERE SANCION_ID = {sancion.sancion_id} ";

                    var query1 = $@"UPDATE [dbo].[REG_CREDITOS]
                                   SET PLAZO = '{sancion.plazocredito}'
                                 WHERE id_solicitud = '{sancion.n_operacion}' ";

                    connection.Query<BeneficiosSancionesLista>(query).ToList();
                    connection.Query(query1);

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = sancion.cod_beneficio,
                        consec = sancion.consec,
                        movimiento = "Actualiza",
                        detalle = $@"Actualiza datos Sanción COD: [{sancion.sancion_id}]",
                        registro_usuario = sancion.registro_usuario
                    });

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