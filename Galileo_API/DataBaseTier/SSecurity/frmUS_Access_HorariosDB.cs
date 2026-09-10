using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmUsAccessHorariosDb
    {
        private readonly IConfiguration _config;

        public FrmUsAccessHorariosDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<HorarioDto>> ObtenerHorariosPorEmpresa(int empresaId)
        {
            var response = DbHelper.CreateOkResponse(new List<HorarioDto>());
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_Horarios_Empresa_Consultar]";
                    var values = new
                    {
                        EmpresaId = empresaId,
                    };
                    var horarios = connection.Query<HorarioDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                    response.Result = horarios;

                    foreach (HorarioDto dt in horarios)
                    {
                        dt.Estado = (dt.Activo ?? false) ? "ACTIVO" : "INACTIVO";
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

        public ErrorDto HorarioRegistrar(HorarioDto horarioDto)
        {
            ErrorDto resp = DbHelper.CreateOkResponse();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_Cliente_Horarios_Registra]";
                    var values = new
                    {
                        Cliente = horarioDto.IdEmpresa,
                        Horario = horarioDto.CodHorario,
                        Descripcion = horarioDto.Descripcion,
                        Activo = horarioDto.Activo,
                        L_Inicio = horarioDto.LunInicio,
                        L_Corte = horarioDto.LunCorte,
                        K_Inicio = horarioDto.MarInicio,
                        K_Corte = horarioDto.MarCorte,
                        M_Inicio = horarioDto.MieInicio,
                        M_Corte = horarioDto.MieCorte,
                        J_Inicio = horarioDto.JueInicio,
                        J_Corte = horarioDto.JueCorte,
                        V_Inicio = horarioDto.VieInicio,
                        V_Corte = horarioDto.VieCorte,
                        S_Inicio = horarioDto.SabInicio,
                        S_Corte = horarioDto.SabCorte,
                        D_Inicio = horarioDto.DomInicio,
                        D_Corte = horarioDto.DomCorte,
                        Usuario = horarioDto.UsuarioRegistro
                    };

                    connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto HorarioEliminar(HorarioDto request)
        {
            ErrorDto resp = DbHelper.CreateOkResponse();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_Horario_Eliminar]";
                    var values = new
                    {
                        EmpresaId = request.IdEmpresa,
                        CodHorario = request.CodHorario
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
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
    }
}
