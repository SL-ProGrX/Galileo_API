using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using System.Data;

namespace PgxAPI.DataBaseTier
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
                DateTime ini = DateTime.MinValue;
                DateTime fin = DateTime.MaxValue;

                // Assuming bitacoraRequestDto.HoraInicio is a DateTime object
                DateTime horaInicioUtc = bitacoraRequestDto.HoraInicio.ToUniversalTime();  // Convert to UTC
                DateTime horaInicioLocal = TimeZoneInfo.ConvertTimeFromUtc(horaInicioUtc, TimeZoneInfo.Local);  // Convert to server's local time

                // Do the same for HoraCorte
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
                        horaInicioLocal.Second
                    );

                    fin = new DateTime(
                        bitacoraRequestDto.FechaCorte.Year,
                        bitacoraRequestDto.FechaCorte.Month,
                        bitacoraRequestDto.FechaCorte.Day,
                        horaCorteLocal.Hour,
                        horaCorteLocal.Minute,
                        horaCorteLocal.Second
                    );
                }

                // If "todas" is true, use default full range of dates
                if (bitacoraRequestDto.todas)
                {
                    ini = new DateTime(1900, 1, 1, 0, 0, 0);
                    fin = new DateTime(2100, 12, 30, 23, 59, 59);
                }

                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spSEG_Bitacora_Consulta]";
                    var values = new
                    {
                        Cliente = bitacoraRequestDto.Cliente,
                        FechaInicio = ini,
                        FechaCorte = fin,
                        Usuario = string.IsNullOrEmpty(bitacoraRequestDto.Usuario) ? null : bitacoraRequestDto.Usuario,
                        Modulo = bitacoraRequestDto.Modulo == 0 ? null : bitacoraRequestDto.Modulo,
                        Movimiento = string.IsNullOrEmpty(bitacoraRequestDto.Movimiento) || bitacoraRequestDto?.Movimiento?.Trim() == "TODOS" ? null : bitacoraRequestDto?.Movimiento,
                        Detalle = string.IsNullOrEmpty(bitacoraRequestDto.Detalle) ? null : bitacoraRequestDto.Detalle,
                        AppName = string.IsNullOrEmpty(bitacoraRequestDto.AppName) ? null : bitacoraRequestDto.AppName,
                        AppVersion = string.IsNullOrEmpty(bitacoraRequestDto.AppVersion) ? null : bitacoraRequestDto.AppVersion,
                        LogEquipo = string.IsNullOrEmpty(bitacoraRequestDto.LogEquipo) ? null : bitacoraRequestDto.LogEquipo,
                        LogIP = string.IsNullOrEmpty(bitacoraRequestDto.LogIP) ? null : bitacoraRequestDto.LogIP,
                        EquipoMAC = string.IsNullOrEmpty(bitacoraRequestDto.EquipoMAC) ? null : bitacoraRequestDto.EquipoMAC,
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


        //public List<BitacoraResultDto> BitacoraObtener(BitacoraRequestDto bitacoraRequestDto)
        //{
        //    List<BitacoraResultDto> resp = new List<BitacoraResultDto>();
        //    try
        //    {
        //        DateTime ini;
        //        DateTime fin;


        //        if(!bitacoraRequestDto.todas && !bitacoraRequestDto.todos)
        //        {
        //             ini = new DateTime(
        //                                            bitacoraRequestDto.FechaInicio.Year,
        //                                            bitacoraRequestDto.FechaInicio.Month,
        //                                            bitacoraRequestDto.FechaInicio.Day,
        //                                            bitacoraRequestDto.HoraInicio.Hour,
        //                                            bitacoraRequestDto.HoraInicio.Minute,
        //                                            bitacoraRequestDto.HoraInicio.Second
        //                                        );

        //             fin = new DateTime(
        //                                            bitacoraRequestDto.FechaCorte.Year,
        //                                            bitacoraRequestDto.FechaCorte.Month,
        //                                            bitacoraRequestDto.FechaCorte.Day,
        //                                            bitacoraRequestDto.HoraCorte.Hour,
        //                                            bitacoraRequestDto.HoraCorte.Minute,
        //                                            bitacoraRequestDto.HoraCorte.Second
        //                                        );
        //        }

        //        if(bitacoraRequestDto.todas)
        //        {
        //            ini = new DateTime(1900, 1, 1, 0, 0, 0);
        //            fin = new DateTime(2100, 12, 30, 23, 59, 59);
        //        }

        //        using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
        //        {
        //            var procedure = "[spSEG_Bitacora_Consulta]";
        //            var values = new
        //            {
        //                Cliente  = bitacoraRequestDto.Cliente,
        //                FechaInicio = ini,
        //                FechaCorte = fin,
        //                Usuario = bitacoraRequestDto.Usuario.IsNullOrEmpty() ? null : bitacoraRequestDto.Usuario,
        //                Modulo = bitacoraRequestDto.Modulo == 0 ? null : bitacoraRequestDto.Modulo,
        //                Movimiento = ( bitacoraRequestDto.Movimiento.IsNullOrEmpty() || bitacoraRequestDto?.Movimiento?.Trim() == "TODOS") ? null : bitacoraRequestDto?.Movimiento,
        //                Detalle = bitacoraRequestDto.Detalle.IsNullOrEmpty() ? null : bitacoraRequestDto.Detalle,
        //                AppName = bitacoraRequestDto.AppName.IsNullOrEmpty() ? null : bitacoraRequestDto.AppName,
        //                AppVersion = bitacoraRequestDto.AppVersion.IsNullOrEmpty() ? null : bitacoraRequestDto.AppVersion,
        //                LogEquipo = bitacoraRequestDto.LogEquipo.IsNullOrEmpty() ? null : bitacoraRequestDto.LogEquipo,
        //                LogIP = bitacoraRequestDto.LogIP.IsNullOrEmpty() ? null : bitacoraRequestDto.LogIP,
        //                EquipoMAC = bitacoraRequestDto.EquipoMAC.IsNullOrEmpty() ? null : bitacoraRequestDto.EquipoMAC,
        //            };
        //            resp = connection.Query<BitacoraResultDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //         _ = ex.Message;
        //    }
        //    return resp;
        //}
    }
}
