using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_SectoresDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public frmAF_SectoresDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de sectores.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <returns></returns>
        public ErrorDto<SectoresLista> AF_Sectores_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<SectoresLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SectoresLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryTotal = @"SELECT COUNT(*) FROM afi_sectores";
                result.Result.Total = connection.Query<int>(queryTotal).FirstOrDefault();

                var query = @"SELECT cod_sector, descripcion FROM afi_sectores ORDER BY cod_sector";
                result.Result.Lista = connection.Query<SectoresData>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.Lista = null;
            }
            return result;
        }

        /// <summary>
        /// Inserta o actualiza un sector.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="sector">Datos del sector</param>
        /// <returns></returns>
        public ErrorDto AF_Sectores_Guardar(int CodEmpresa, string usuario, SectoresData sector)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryExiste = @"SELECT COUNT(*) FROM afi_sectores WHERE cod_sector = @cod_sector";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { cod_sector = sector.Cod_Sector });

                if (existe == 0)
                {
                    result = AF_Sectores_Insertar(CodEmpresa, usuario, sector);
                }
                else
                {
                    result = AF_Sectores_Actualizar(CodEmpresa, usuario, sector);
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
        /// Inserta un nuevo sector y registra en bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="sector">Datos del sector a insertar</param>
        /// <returns></returns>
        private ErrorDto AF_Sectores_Insertar(int CodEmpresa, string usuario, SectoresData sector)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO afi_sectores (descripcion) VALUES (@descripcion)";
                connection.Execute(query, new { descripcion = sector.Descripcion });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Sector: {sector.Descripcion}",
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
        /// Actualiza un sector existente y registra en bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="sector">Datos del sector a actualizar</param>
        /// <returns></returns>
        private ErrorDto AF_Sectores_Actualizar(int CodEmpresa, string usuario, SectoresData sector)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"UPDATE afi_sectores SET descripcion = @descripcion WHERE cod_sector = @cod_sector";
                connection.Execute(query, new { cod_sector = sector.Cod_Sector, descripcion = sector.Descripcion });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Sector: {sector.Cod_Sector} - {sector.Descripcion}",
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

        /// <summary>
        /// Elimina un sector por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="cod_sector">Código del sector a eliminar</param>
        /// <returns></returns>
        public ErrorDto AF_Sectores_Eliminar(int CodEmpresa, string usuario, int cod_sector)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM afi_sectores WHERE cod_sector = @cod_sector";
                connection.Execute(query, new { cod_sector });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Sector: {cod_sector}",
                    Movimiento = "Elimina - WEB",
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
