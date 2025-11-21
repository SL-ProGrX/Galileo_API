using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class BitacoraDb
    {
        private readonly IConfiguration _config;

        public BitacoraDb(IConfiguration config)
        {
            _config = config;
        }

        public List<BitacoraResultDto> BitacoraObtener(BitacoraRequestDto bitacoraRequestDto)
        {
            List<BitacoraResultDto> resp = new List<BitacoraResultDto>();
            try
            {
                (DateTime ini, DateTime fin) = GetDateRange(bitacoraRequestDto);

                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spSEG_Bitacora_Consulta]";

                    string? detalleValue = null;
                    if (bitacoraRequestDto.Detalle != null)
                    {
                        var detalleTrimmed = bitacoraRequestDto.Detalle.Trim();
                        detalleValue = string.IsNullOrEmpty(detalleTrimmed) ? null : detalleTrimmed;
                    }

                    var values = new
                    {
                        Cliente = bitacoraRequestDto.Cliente,
                        FechaInicio = ini,
                        FechaCorte = fin,
                        Usuario = string.IsNullOrEmpty(bitacoraRequestDto.Usuario) ? null : bitacoraRequestDto.Usuario,
                        Modulo = bitacoraRequestDto.Modulo == 0 ? null : bitacoraRequestDto.Modulo,
                        Movimiento = string.IsNullOrEmpty(bitacoraRequestDto.Movimiento) || bitacoraRequestDto?.Movimiento?.Trim() == "TODOS" ? null : bitacoraRequestDto?.Movimiento,
                        Detalle = detalleValue,
                        AppName = string.IsNullOrEmpty(bitacoraRequestDto?.AppName) ? null : bitacoraRequestDto.AppName,
                        AppVersion = string.IsNullOrEmpty(bitacoraRequestDto?.AppVersion) ? null : bitacoraRequestDto.AppVersion,
                        LogEquipo = string.IsNullOrEmpty(bitacoraRequestDto?.LogEquipo) ? null : bitacoraRequestDto.LogEquipo,
                        LogIP = string.IsNullOrEmpty(bitacoraRequestDto?.LogIP) ? null : bitacoraRequestDto.LogIP,
                        EquipoMAC = string.IsNullOrEmpty(bitacoraRequestDto?.EquipoMAC) ? null : bitacoraRequestDto.EquipoMAC,
                    };
                    resp = connection.Query<BitacoraResultDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception)
            {
                // Handle error
            }
            return resp;
        }

        private static (DateTime ini, DateTime fin) GetDateRange(BitacoraRequestDto bitacoraRequestDto)
        {
            DateTime ini = DateTime.MinValue;
            DateTime fin = DateTime.MaxValue;

            DateTime horaInicioUtc = bitacoraRequestDto.HoraInicio.ToUniversalTime();
            DateTime horaInicioLocal = TimeZoneInfo.ConvertTimeFromUtc(horaInicioUtc, TimeZoneInfo.Local);

            DateTime horaCorteUtc = bitacoraRequestDto.HoraCorte.ToUniversalTime();
            DateTime horaCorteLocal = TimeZoneInfo.ConvertTimeFromUtc(horaCorteUtc, TimeZoneInfo.Local);

            if (!bitacoraRequestDto.todas && !bitacoraRequestDto.todos)
            {
                ini = new DateTime(
                    bitacoraRequestDto.FechaInicio.Year,
                    bitacoraRequestDto.FechaInicio.Month,
                    bitacoraRequestDto.FechaInicio.Day,
                    horaInicioLocal.Hour,
                    horaInicioLocal.Minute,
                    horaInicioLocal.Second,
                    DateTimeKind.Local
                );

                fin = new DateTime(
                    bitacoraRequestDto.FechaCorte.Year,
                    bitacoraRequestDto.FechaCorte.Month,
                    bitacoraRequestDto.FechaCorte.Day,
                    horaCorteLocal.Hour,
                    horaCorteLocal.Minute,
                    horaCorteLocal.Second,
                    DateTimeKind.Local
                );
            }

            if (bitacoraRequestDto.todas)
            {
                ini = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Local);
                fin = new DateTime(2100, 12, 30, 23, 59, 59, DateTimeKind.Local);
            }

            return (ini, fin);
        }
    }
}