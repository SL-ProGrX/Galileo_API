using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmGenPeriodosDB
    {
        private readonly IConfiguration _config;

        public frmGenPeriodosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<PeriodoDto>> Periodos_ObtenerTodos(int CodEmpresa, string estado)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto<List<PeriodoDto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = string.Empty;

                    if (estado == "T")
                    {
                        query = "SELECT Anio, Mes, Proceso, Estado  FROM pv_periodos order by proceso desc";
                        resp.Result = connection.Query<PeriodoDto>(query).ToList();
                    }
                    else
                    {
                        query = "SELECT Anio, Mes, Proceso, Estado  FROM pv_periodos where Estado = @Estado order by proceso desc";

                        var parameters = new DynamicParameters();
                        parameters.Add("Estado", estado, DbType.String);

                        resp.Result = connection.Query<PeriodoDto>(query, parameters).ToList();
                    }

                    foreach (PeriodoDto dt in resp.Result)
                    {
                        dt.DescripcionEstado = dt.Estado == "P" ? "PENDIENTE" : (dt.Estado == "C" ? "CERRADO" : string.Empty);
                        dt.Activo = (dt.Estado == "P") ? 1 : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto Periodo_Cerrar(int CodEmpresa, PeriodoDto periodoDto)
        {
            int res = 0;
            string resultadoMensaje = string.Empty;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resultado = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select isnull(count(*),0) as Existe from pv_periodos where estado = 'P' and anio = @Anio and Mes = @Mes";

                    var parameters = new DynamicParameters();
                    parameters.Add("Anio", periodoDto.Anio, DbType.Int32);
                    parameters.Add("Mes", periodoDto.Mes, DbType.Int32);

                    res = connection.Query<int>(query, parameters).FirstOrDefault();

                    if (res > 0)
                    {
                        query = "select isnull(count(*),0) as Existe from pv_periodos where mes > @Mes and anio = @Anio and estado = 'C'";

                        res = connection.Query<int>(query, parameters).FirstOrDefault();

                        if (res == 0)
                        {
                            query = "select isnull(count(*),0) as Existe from pv_periodos where anio > @Anio  and estado = 'C'";
                            res = connection.Query<int>(query, parameters).FirstOrDefault();

                            if (res == 0)
                            {
                                //Verifica que el periodo anterior este cerrado, por cuestiones de orden. Si no existe no hay problema.
                                int lngAnioX = periodoDto.Anio;
                                int iMesX = periodoDto.Mes;

                                if (iMesX == 1)
                                {
                                    iMesX = 12;
                                    lngAnioX = lngAnioX - 1;
                                }
                                else
                                {
                                    iMesX = iMesX - 1;
                                }

                                var parameters2 = new DynamicParameters();
                                parameters2.Add("Anio", lngAnioX, DbType.Int32);
                                parameters2.Add("Mes", iMesX, DbType.Int32);

                                query = "select Estado from pv_periodos where anio = @Anio and Mes = @Mes";
                                string? resultadoEstado = connection.Query<string>(query, parameters2).FirstOrDefault();

                                if (resultadoEstado == "P")
                                {
                                    res = -1;
                                    resultadoMensaje = "El periodo Anterior no se ha cerrado, proceda en el mismo orden...";
                                }
                                else
                                {
                                    //Verificar si existe el Periodo Siguiente, de lo contrario crearlo
                                    lngAnioX = periodoDto.Anio;
                                    iMesX = periodoDto.Mes;

                                    if (iMesX == 1)
                                    {
                                        iMesX = 12;
                                        lngAnioX = lngAnioX - 1;
                                    }
                                    else
                                    {
                                        iMesX = iMesX - 1;
                                    }

                                    /* desde aqui lo realiza el Sp */
                                    var procedure = "[spINVCierrePeriodo]";
                                    var values = new
                                    {
                                        AnioI = periodoDto.Anio,
                                        MesI = periodoDto.Mes,
                                        AnioC = lngAnioX,
                                        MesC = iMesX
                                    };
                                    res = connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
                                    resultadoMensaje = "El Cierre del Periodo se Realizó Satisfactoriamente...";

                                }
                            }
                            else
                            {
                                res = -1;
                                resultadoMensaje = "Existen periodos posteriores ya cerrados, verifique...";
                            }
                        }
                        else
                        {
                            res = -1;
                            resultadoMensaje = "Existen periodos posteriores ya cerrados, verifique...";
                        }
                    }
                    else
                    {
                        res = -1;
                        resultadoMensaje = "El periodo ya se encuentra cerrado, verifique...";
                    }
                }
                resultado.Code = res;
                resultado.Description = resultadoMensaje;

            }
            catch (Exception ex)
            {
                _ = ex.Message;
                resultado.Code = -1;
                resultado.Description = ex.Message;
            }
            return resultado;
        }
    }
}
