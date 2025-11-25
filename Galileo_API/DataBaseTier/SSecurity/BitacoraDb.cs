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

        private static (DateTime ini, DateTime fin) GetDateRange(BitacoraRequestDto dto)
        {
            DateTime ini = DateTime.MinValue;
            DateTime fin = DateTime.MaxValue;

            // Normalizamos horas usando ?? en lugar de operador ternario
            var horaInicio = dto.HoraInicio ?? DateTime.MinValue;
            var horaInicioLocal = TimeZoneInfo.ConvertTimeFromUtc(horaInicio.ToUniversalTime(), TimeZoneInfo.Local);

            var horaCorte = dto.HoraCorte ?? DateTime.MinValue;
            var horaCorteLocal = TimeZoneInfo.ConvertTimeFromUtc(horaCorte.ToUniversalTime(), TimeZoneInfo.Local);

            // Caso: todas las fechas -> salimos temprano
            if (dto.todas == true)
            {
                ini = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Local);
                fin = new DateTime(2100, 12, 30, 23, 59, 59, DateTimeKind.Local);
                return (ini, fin);
            }

            // Caso: rango específico (si no es todas ni todos)
            if (dto.todas != true && dto.todos != true)
            {
                var fechaInicio = dto.FechaInicio ?? new DateTime(1900, 1, 1);
                var fechaCorte = dto.FechaCorte ?? new DateTime(2100, 12, 30);

                ini = new DateTime(
                    fechaInicio.Year,
                    fechaInicio.Month,
                    fechaInicio.Day,
                    horaInicioLocal.Hour,
                    horaInicioLocal.Minute,
                    horaInicioLocal.Second,
                    DateTimeKind.Local);

                fin = new DateTime(
                    fechaCorte.Year,
                    fechaCorte.Month,
                    fechaCorte.Day,
                    horaCorteLocal.Hour,
                    horaCorteLocal.Minute,
                    horaCorteLocal.Second,
                    DateTimeKind.Local);
            }

            return (ini, fin);
        }


    }
}