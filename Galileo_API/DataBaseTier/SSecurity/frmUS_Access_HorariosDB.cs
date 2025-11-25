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

        public List<HorarioDto> ObtenerHorariosPorEmpresa(int empresaId)
        {
            List<HorarioDto> result = new List<HorarioDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_Horarios_Empresa_Consultar]";
                    var values = new
                    {
                        EmpresaId = empresaId,
                    };
                    result = connection.Query<HorarioDto>(procedure, values, commandType: CommandType.StoredProcedure)!.ToList();

                    foreach (HorarioDto dt in result)
                    {
                        dt.Estado = (dt.Activo ?? false) ? "ACTIVO" : "INACTIVO";
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public ErrorDto HorarioRegistrar(HorarioDto horarioDto)
        {
            ErrorDto resp = new ErrorDto();
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

                    resp.Code = connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
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

        public ErrorDto HorarioEliminar(HorarioDto request)
        {
            ErrorDto resp = new ErrorDto();
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
