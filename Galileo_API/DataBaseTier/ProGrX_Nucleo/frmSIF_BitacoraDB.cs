using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmSifBitacoraDB
    {

        private readonly IConfiguration _config;

        public FrmSifBitacoraDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la bitácora del sistema
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorDto<SifBitacoraLista> Bitacora_Obtener(int codEmpresa, string filtros)
        {
            var response = new ErrorDto<SifBitacoraLista>
            {
                Code = 0,
                Description = "OK",
                Result = new SifBitacoraLista
                {
                    total = 0,
                    lista = new List<BitacoraResultadoDto>()
                }
            };

            try
            {
                var bitacora = JsonConvert.DeserializeObject<BitacoraDto>(filtros);

                if (bitacora == null)
                    throw new ArgumentNullException("filtros", "Los filtros de bitácora no pueden ser nulos.");

                var (fechaInicio, fechaCorte) = CalcularFechas(bitacora);

                var procedure = "[spSEG_Bitacora_Consulta]";
                var values = new
                {
                    Cliente = bitacora.cliente,
                    FechaInicio = fechaInicio,
                    FechaCorte = fechaCorte,
                    Usuario = string.IsNullOrWhiteSpace(bitacora.usuario) ? null : bitacora.usuario,
                    Modulo = bitacora.modulo == 0 ? null : bitacora.modulo,
                    Movimiento = (string.IsNullOrWhiteSpace(bitacora.movimiento) || bitacora.movimiento.Trim() == "TODOS") ? null : bitacora.movimiento,
                    Detalle = string.IsNullOrWhiteSpace(bitacora.detalle) ? null : bitacora.detalle,
                    AppName = string.IsNullOrWhiteSpace(bitacora.appname) ? null : bitacora.appname,
                    AppVersion = string.IsNullOrWhiteSpace(bitacora.appversion) ? null : bitacora.appversion,
                    LogEquipo = string.IsNullOrWhiteSpace(bitacora.logequipo) ? null : bitacora.logequipo,
                    LogIP = string.IsNullOrWhiteSpace(bitacora.logip) ? null : bitacora.logip,
                    EquipoMAC = string.IsNullOrWhiteSpace(bitacora.equipomac) ? null : bitacora.equipomac,
                };

                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                var lista = connection.Query<BitacoraResultadoDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                response.Result.lista = lista;
                response.Result.total = lista.Count; // si el SP no devuelve un total, contamos la lista
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener bitácora: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Calcula las fechas de inicio y corte para la bitácora según los filtros.
        /// </summary>
        /// <param name="bitacora"></param>
        /// <returns>Tuple con fechaInicio y fechaCorte</returns>
        private static (DateTime fechaInicio, DateTime fechaCorte) CalcularFechas(BitacoraDto bitacora)
        {
            if (bitacora.todasFechas)
            {
                return (
                    new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                    new DateTime(2100, 12, 30, 23, 59, 59, DateTimeKind.Unspecified)
                );
            }
            var fi = bitacora.fechainicio ?? DateTime.Today;
            var fc = bitacora.fechacorte ?? fi;

            if (bitacora.todasHoras)
            {
                return (
                    fi.Date, // 00:00:00
                    fc.Date.AddDays(1).AddTicks(-1) // 23:59:59.9999999
                );
            }
            return (fi, fc);
        }


        /// <summary>
        /// Obtiene la lista de módulos del sistema
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorDto<List<DropDownListaGenericaModel>> BitacoraModulos_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                const string procedure = "[spSEG_Modulos_Consulta]";

                var rows = connection.Query<BitacoraModuloDto>(procedure, commandType: CommandType.StoredProcedure).ToList();

                response.Result = rows.Select(r => new DropDownListaGenericaModel
                {
                    item = r.Modulo,
                    descripcion = r.Descripcion,
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener los módulos del sistema: {ex.Message}";
                response.Result = new List<DropDownListaGenericaModel>();
            }

            return response;
        }


        /// <summary>
        /// Obtiene la lista de usuarios del sistema
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> BitacoraUsuarios_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                    var query = $@"select Nombre as 'item',descripcion from usuarios where estado = 'A' ";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
    }
}