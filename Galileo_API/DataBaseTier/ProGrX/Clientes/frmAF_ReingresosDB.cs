using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using PgxAPI.Models.ProGrX.Clientes.TuProyecto.Core.Models;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_ReingresosDB
    {
        private readonly IConfiguration _config;

        public frmAF_ReingresosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtener tipos de causa para congelar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PromotoresReingreso_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"SELECT id_promotor AS item, nombre AS descripcion 
                                    FROM PROMOTORES 
                                    WHERE estado = 1;";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Activa un socio, registra ingreso y vincula patrimonio en una sola transacción.
        /// </summary>
        public ErrorDto AF_Persona_ActivarYVincular(int CodEmpresa, string request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            AF_Prsona_ActivacionDTO req = JsonConvert.DeserializeObject<AF_Prsona_ActivacionDTO>(request) ?? new AF_Prsona_ActivacionDTO();

            var response = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(stringConn);
                connection.Open();

                using var transaction = connection.BeginTransaction();
                try
                {
                   
                    var queryUpdate = @"
                        UPDATE socios
                        SET estadoactual = 'S',
                            FechaIngreso = @FechaIngreso,
                            priDeduc = @PriDeduc,
                            reg_user = @Usuario,
                            reg_fecha = dbo.MyGetdate(),
                            Fecha_Comision = NULL,
                            id_promotor = @IdPromotor,
                            cod_oficina = @CodOficina
                        WHERE cedula = @Cedula;";

                    connection.Execute(queryUpdate, new
                    {
                        Cedula = req.cedula,
                        FechaIngreso = DateTime.Now, // fxFechaServidor
                        PriDeduc = req.pri_deduc,
                        Usuario = req.usuario,
                        IdPromotor = req.id_promotor,
                        CodOficina = req.cod_oficina
                    }, transaction);

                    
                    var queryInsert = @"
                        INSERT INTO afi_ingresos
                            (Cedula, fecha_ingreso, id_promotor, Boleta, Usuario, Fecha, cod_oficina)
                        VALUES
                            (@Cedula, dbo.MyGetdate(), @IdPromotor, @Boleta, @Usuario, dbo.MyGetdate(), @CodOficina);";

                    connection.Execute(queryInsert, new
                    {
                        Cedula = req.cedula,
                        IdPromotor = req.id_promotor,
                        Boleta = req.boleta,
                        Usuario = req.usuario,
                        CodOficina = req.cod_oficina
                    }, transaction);

                    
                    var querySP = "EXEC spAFI_PERSONA_PATRIMONIO_Vincula @Cedula;";
                    connection.Execute(querySP, new { Cedula = req.cedula }, transaction);

                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }


}
