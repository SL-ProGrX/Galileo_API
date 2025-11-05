using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Credito;

namespace PgxAPI.DataBaseTier.ProGrX.Credito
{
    public class frmCR_SolCreacionAgendaDBs
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 3; // Modulo de Créditos
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmCR_SolCreacionAgendaDBs(IConfiguration? config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Método para obtener los comités activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> CR_SolCreacionAgenda_Comites_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select id_comite as 'item',descripcion from comites where estado = 1";
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
        /// Método para generar el acta de la agenda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="acta"></param>
        /// <returns></returns>
        public ErrorDTO<CrSolCreacionAgendaReporteData> CR_SolCreacionAgenda_Acta_Generar(int CodEmpresa, CrSolCreacionAgendaActaData acta )
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<CrSolCreacionAgendaReporteData>
            {
                Code = 0,
                Description = "Ok",
                Result = new CrSolCreacionAgendaReporteData()
            };
            try
            {
                if (!fxValida(acta.acta, acta.id_comite ?? 0))
                {
                    response.Code = -1;
                    response.Description = "Faltan datos obligatorios.";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    if (acta.validaActa == 3 || acta.validaActa == 0) //'número de txtActa > COMITES.Acta
                    {
                        response.Code = -1;
                        response.Description = "Numero de Acta no puede ser mayor a numero sugerido.";
                        return response;
                    }

                    if (acta.validaActa == 2) //'Crear Acta
                    {
                        var query = "";

                        //actualiza comite
                        query = $@"UPDATE COMITES SET ACTA = @acta WHERE ID_COMITE = @id_comite";
                        connection.Execute(query, new { acta = acta.acta, id_comite = acta.id_comite });

                        string fechaInicioStr = _AuxiliarDB.validaFechaGlobal(acta.fechaInicio);
                        string fechaCorteStr = _AuxiliarDB.validaFechaGlobal(acta.fechaCorte);


                        //consulto registro de creditos
                        query = $@" select id_solicitud from reg_creditos where acta is null and estadosol ='R'
                                     and fechasol between @fechaInicio
                                     and @fechaCorte and id_comite = @comite";

                        var listaSolicitudes = connection.Query<int>(query, new
                        {
                            fechaInicio = fechaInicioStr,
                            fechaCorte = fechaCorteStr,
                            comite = acta.id_comite
                        }).ToList();

                        //actualizo acta en registros de creditos
                        foreach (var solicitud in listaSolicitudes)
                        {
                            query = $@" UPDATE REG_CREDITOS SET ACTA = @acta WHERE ID_SOLICITUD = @solicitud ";
                            connection.Execute(query, new { acta = acta.acta, solicitud = solicitud });
                        }

                        //NOTA: Este codigo esta comentado en v6 pero sin comentario
                       // query = $@"exc sp_crActualizaActa {acta.id_comite} {acta.acta}, '{fechaInicioStr}', '{fechaCorteStr}' ";

                        response.Result.reporte = "AGENDA";
                        response.Result.reg_credito = "REG_CREDITOS.FECHASOL";
                    }

                    if(acta.validaActa == 1){
                        //' impresion de acta anterior, número en txtActa < COMITES.Acta
                        response.Result.reporte = "REIMPRESION DE AGENDA";
                        response.Result.reg_credito = "REG_CREDITOS.FECHASOL";
                    }
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
        /// Método para validar los datos de entrada
        /// </summary>
        /// <param name="acta"></param>
        /// <param name="comite"></param>
        /// <returns></returns>
        private bool fxValida(int? acta, int comite)
        {
            var response = true;

            if(acta == null || acta.ToString().Trim() == "")
            {
                response = false;
            }

            if (comite == null || comite == 0)
            {
                response = false;
            }

            return response;

        }

        /// <summary>
        /// Método para consultar el acta actual del comité
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <returns></returns>
        public ErrorDTO<int> CR_SolCreacionAgenda_Acta_Consulta(int CodEmpresa, int id_comite)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(acta,0) as 'Acta' from comites where id_comite= @comites";
                    response.Result = connection.Query<int>(query, new { comites = id_comite }).FirstOrDefault();

                    if(response.Result != null || response.Result >= 0)
                    {
                        response.Result = response.Result + 1;
                    }
                    else
                    {
                        response.Result = 1;
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

    }
}
