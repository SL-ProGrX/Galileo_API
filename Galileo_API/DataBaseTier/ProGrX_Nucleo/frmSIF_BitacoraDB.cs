using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.SIF;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmSIF_BitacoraDB
    {

        private readonly IConfiguration _config;

        public frmSIF_BitacoraDB(IConfiguration config)
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
        public ErrorDTO<sifBitacoraLista> Bitacora_Obtener(int codEmpresa, string filtros)
        {
            var response = new ErrorDTO<sifBitacoraLista>
            {
                Code = 0,
                Description = "OK",
                Result = new sifBitacoraLista
                {
                    total = 0,
                    lista = new List<BitacoraResultadoDto>()
                }
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                var bitacora = JsonConvert.DeserializeObject<BitacoraDTO>(filtros);

                DateTime fechaInicio, fechaCorte;

                if (bitacora.todasFechas)
                {
                    fechaInicio = new DateTime(1900, 1, 1, 0, 0, 0);
                    fechaCorte = new DateTime(2100, 12, 30, 23, 59, 59);
                }
                else
                {
                    var fi = bitacora.fechainicio ?? DateTime.Today;
                    var fc = bitacora.fechacorte ?? fi;

                    if (bitacora.todasHoras)
                    {
                        fechaInicio = fi.Date; // 00:00:00
                        fechaCorte = fc.Date.AddDays(1).AddTicks(-1); // 23:59:59.9999999
                    }
                    else
                    {
                        fechaInicio = fi;
                        fechaCorte = fc;
                    }
                }

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
        /// Obtiene la lista de módulos del sistema
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorDTO<List<DropDownListaGenericaModel>> BitacoraModulos_Obtener(int codEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");


                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));
                connection.Open();

                const string procedure = "[spSEG_Modulos_Consulta]";

                var rows = connection.Query<BitacoraModuloDTO>(procedure, commandType: CommandType.StoredProcedure).ToList();

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
        public ErrorDTO<List<DropDownListaGenericaModel>> BitacoraUsuarios_Obtener(int CodEmpresa)
        {
            var result = new ErrorDTO<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Nombre as 'item',descripcion from usuarios where estado = 'A' ";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
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