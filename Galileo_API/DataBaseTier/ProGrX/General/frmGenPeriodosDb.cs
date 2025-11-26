using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmGenPeriodosDb
    {

        private readonly IConfiguration _config;

        public FrmGenPeriodosDb(IConfiguration config)
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
                resp = Periodos_ObtenerQuery(connection, estado);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        private static ErrorDto<List<PeriodoDto>> Periodos_ObtenerQuery(SqlConnection connection, string estado)
        {
            var query = string.Empty;
            var resp = new ErrorDto<List<PeriodoDto>>();

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
                string descripcionEstado = string.Empty;
                if (dt.Estado == "P")
                {
                    descripcionEstado = "PENDIENTE";
                }
                else if (dt.Estado == "C")
                {
                    descripcionEstado = "CERRADO";
                }
                dt.DescripcionEstado = descripcionEstado;
                dt.Activo = (dt.Estado == "P") ? 1 : 0;
            }
            return resp;
        }

        public ErrorDto Periodo_Cerrar(int CodEmpresa, PeriodoDto periodoDto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resultado = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(stringConn);

                if (!PeriodoEstaPendiente(connection, periodoDto))
                {
                    resultado.Code = -1;
                    resultado.Description = "El periodo ya se encuentra cerrado, verifique...";
                    return resultado;
                }

                if (ExistenPeriodosPosterioresCerrados(connection, periodoDto))
                {
                    resultado.Code = -1;
                    resultado.Description = "Existen periodos posteriores ya cerrados, verifique...";
                    return resultado;
                }

                if (!PeriodoAnteriorCerrado(connection, periodoDto, out string mensajeAnterior))
                {
                    resultado.Code = -1;
                    resultado.Description = mensajeAnterior;
                    return resultado;
                }

                int lngAnioX, iMesX;
                ObtenerPeriodoAnterior(periodoDto.Anio, periodoDto.Mes, out lngAnioX, out iMesX);

                var procedure = "[spINVCierrePeriodo]";
                var values = new
                {
                    AnioI = periodoDto.Anio,
                    MesI = periodoDto.Mes,
                    AnioC = lngAnioX,
                    MesC = iMesX
                };
                int res = connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);

                resultado.Code = res;
                resultado.Description = "El Cierre del Periodo se Realizó Satisfactoriamente...";
            }
            catch (Exception ex)
            {
                resultado.Code = -1;
                resultado.Description = ex.Message;
            }
            return resultado;
        }

        private static bool PeriodoEstaPendiente(SqlConnection connection, PeriodoDto periodoDto)
        {
            var query = "select isnull(count(*),0) as Existe from pv_periodos where estado = 'P' and anio = @Anio and Mes = @Mes";
            var parameters = new DynamicParameters();
            parameters.Add("Anio", periodoDto.Anio, DbType.Int32);
            parameters.Add("Mes", periodoDto.Mes, DbType.Int32);
            int res = connection.Query<int>(query, parameters).FirstOrDefault();
            return res > 0;
        }

        private static bool ExistenPeriodosPosterioresCerrados(SqlConnection connection, PeriodoDto periodoDto)
        {
            var query = "select isnull(count(*),0) as Existe from pv_periodos where mes > @Mes and anio = @Anio and estado = 'C'";
            var parameters = new DynamicParameters();
            parameters.Add("Anio", periodoDto.Anio, DbType.Int32);
            parameters.Add("Mes", periodoDto.Mes, DbType.Int32);
            int res = connection.Query<int>(query, parameters).FirstOrDefault();
            if (res > 0)
                return true;

            query = "select isnull(count(*),0) as Existe from pv_periodos where anio > @Anio  and estado = 'C'";
            res = connection.Query<int>(query, parameters).FirstOrDefault();
            return res > 0;
        }

        private static bool PeriodoAnteriorCerrado(SqlConnection connection, PeriodoDto periodoDto, out string mensaje)
        {
            mensaje = string.Empty;
            int lngAnioX, iMesX;
            ObtenerPeriodoAnterior(periodoDto.Anio, periodoDto.Mes, out lngAnioX, out iMesX);

            var parameters2 = new DynamicParameters();
            parameters2.Add("Anio", lngAnioX, DbType.Int32);
            parameters2.Add("Mes", iMesX, DbType.Int32);

            var query = "select Estado from pv_periodos where anio = @Anio and Mes = @Mes";
            string? resultadoEstado = connection.Query<string>(query, parameters2).FirstOrDefault();

            if (resultadoEstado == "P")
            {
                mensaje = "El periodo Anterior no se ha cerrado, proceda en el mismo orden...";
                return false;
            }
            return true;
        }

        private static void ObtenerPeriodoAnterior(int anio, int mes, out int anioAnterior, out int mesAnterior)
        {
            if (mes == 1)
            {
                mesAnterior = 12;
                anioAnterior = anio - 1;
            }
            else
            {
                mesAnterior = mes - 1;
                anioAnterior = anio;
            }
        }
    }
}
