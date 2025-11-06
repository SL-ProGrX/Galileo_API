using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_CRParametrosDB
    {
        private readonly IConfiguration? _config;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 1;

        public frmAF_CRParametrosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de parámetros de control de renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCrParametrosData>> AF_CRParametros_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<AfCrParametrosData>>()
            {
                Code = 0,
                Description = "OK",
                Result = new List<AfCrParametrosData>()
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                string query = "SELECT * FROM afi_cr_parametros";
                result.Result = connection.Query<AfCrParametrosData>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Guarda (inserta o actualiza) los parámetros de control de renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto AF_CRParametros_Guardar(int CodEmpresa, string usuario, AfCrParametrosData parametros)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Validar existencia
                var queryExiste = @"SELECT TOP 1 id FROM afi_cr_parametros ORDER BY id";
                var existe = connection.QueryFirstOrDefault<int?>(queryExiste);

                if (parametros.isNew)
                {
                    if (existe.HasValue)
                    {
                        result.Code = -2;
                        result.Description = "Ya existe un registro de parámetros.";
                    }
                    else
                    {
                        result = AF_CRParametros_Insertar(CodEmpresa, usuario, parametros);
                    }
                }
                else
                {
                    if (!existe.HasValue)
                    {
                        result.Code = -2;
                        result.Description = "No existe ningún registro de parámetros para actualizar.";
                    }
                    else
                    {
                        parametros.id = existe.Value;
                        result = AF_CRParametros_Actualizar(CodEmpresa, usuario, parametros);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Inserta un nuevo registro de parámetros.
        /// </summary>
        private ErrorDto AF_CRParametros_Insertar(int CodEmpresa, string usuario, AfCrParametrosData parametros)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Insertado correctamente"
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryInsert = @"INSERT INTO afi_cr_parametros
                    (dias_vence, liq_pat_control, fecha_limite, tipo_vencimiento, utiliza_zonas, activar_control)
                    VALUES (@dias_vence, @liq_pat_control, @fecha_limite, @tipo_vencimiento, @utiliza_zonas, @activar_control)";
                connection.Execute(queryInsert, new
                {
                    parametros.dias_vence,
                    liq_pat_control = parametros.liq_pat_control ? 1 : 0,
                    parametros.fecha_limite,
                    parametros.tipo_vencimiento,
                    utiliza_zonas = parametros.utiliza_zonas ? 1 : 0,
                    activar_control = parametros.activar_control ? 1 : 0
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Parámetros Renuncia: {parametros.dias_vence}, {parametros.liq_pat_control}, {parametros.fecha_limite}, {parametros.tipo_vencimiento}, {parametros.utiliza_zonas}, {parametros.activar_control}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Actualiza un registro de parámetros existente.
        /// </summary>
        private ErrorDto AF_CRParametros_Actualizar(int CodEmpresa, string usuario, AfCrParametrosData parametros)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Actualizado correctamente"
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryUpdate = @"UPDATE afi_cr_parametros
                    SET dias_vence = @dias_vence,
                        liq_pat_control = @liq_pat_control,
                        fecha_limite = @fecha_limite,
                        tipo_vencimiento = @tipo_vencimiento,
                        utiliza_zonas = @utiliza_zonas,
                        activar_control = @activar_control
                    WHERE id = @id";
                connection.Execute(queryUpdate, new
                {
                    parametros.id,
                    parametros.dias_vence,
                    liq_pat_control = parametros.liq_pat_control ? 1 : 0,
                    parametros.fecha_limite,
                    parametros.tipo_vencimiento,
                    utiliza_zonas = parametros.utiliza_zonas ? 1 : 0,
                    activar_control = parametros.activar_control ? 1 : 0
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Parámetros Renuncia: {parametros.dias_vence}, {parametros.liq_pat_control}, {parametros.fecha_limite}, {parametros.tipo_vencimiento}, {parametros.utiliza_zonas}, {parametros.activar_control}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}
