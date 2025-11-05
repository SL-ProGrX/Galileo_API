using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_Motivos_IngresoDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1; // Modulo de Tesorería
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_Motivos_IngresoDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de motivos de ingreso con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDTO<MotivoIngresoLista> AF_MotivosIngreso_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<MotivoIngresoLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new MotivoIngresoLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Total
                var queryTotal = @"select COUNT(COD_MOTIVO) from AFI_MOTIVOS_INGRESOS";
                result.Result.Total = connection.Query<int>(queryTotal).FirstOrDefault();

                // Filtros
                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( COD_MOTIVO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                // Orden
                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "COD_MOTIVO" : filtros.sortField;
                string sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";

                // Paginación
                int pagina = filtros.pagina ?? 0;
                int paginacion = filtros.paginacion ?? 10;

                var query = $@"select COD_MOTIVO, descripcion, ACTIVO, Registro_Fecha, Registro_Usuario
                               from AFI_MOTIVOS_INGRESOS
                               {where}
                               order by {sortField} {sortOrder}
                               OFFSET {pagina} ROWS 
                               FETCH NEXT {paginacion} ROWS ONLY";

                result.Result.Lista = connection.Query<MotivoIngresoData>(query).ToList();
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
        /// Inserta o actualiza un motivo de ingreso.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="motivoIngreso">Datos del motivo de ingreso</param>
        /// <returns></returns>
        public ErrorDTO AF_MotivosIngreso_Guardar(int CodEmpresa, string usuario, MotivoIngresoData motivoIngreso)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Verifico si existe el motivo
                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_MOTIVOS_INGRESOS WHERE COD_MOTIVO = @COD_MOTIVO";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { COD_MOTIVO = motivoIngreso.Cod_Motivo.ToUpper() });

                if (existe == 0)
                {
                    // Insertar
                    result = AF_MotivosIngreso_Insertar(CodEmpresa, usuario, motivoIngreso);
                }
                else
                {
                    // Actualizar
                    result = AF_MotivosIngreso_Actualizar(CodEmpresa, usuario, motivoIngreso);
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
        /// Inserta un nuevo motivo de ingreso.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="motivoIngreso">Datos del motivo de ingreso a insertar</param>
        /// <returns></returns>
        private ErrorDTO AF_MotivosIngreso_Insertar(int CodEmpresa, string usuario, MotivoIngresoData motivoIngreso)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_MOTIVOS_INGRESOS (COD_MOTIVO, Descripcion, ACTIVO, registro_fecha, registro_usuario)
                              VALUES (@COD_MOTIVO, @Descripcion, @ACTIVO, dbo.myGetdate(), @Usuario)";
                connection.Execute(query, new
                {
                    COD_MOTIVO = motivoIngreso.Cod_Motivo.ToUpper(),
                    Descripcion = motivoIngreso.Descripcion,
                    ACTIVO = motivoIngreso.Activo,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Motivo de Ingreso : {motivoIngreso.Cod_Motivo} - {motivoIngreso.Descripcion}",
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
        /// Actualiza un motivo de ingreso existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="motivoIngreso">Datos del motivo de ingreso a actualizar</param>
        /// <returns></returns>
        private ErrorDTO AF_MotivosIngreso_Actualizar(int CodEmpresa, string usuario, MotivoIngresoData motivoIngreso)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"UPDATE AFI_MOTIVOS_INGRESOS
                              SET Descripcion = @Descripcion,
                                  ACTIVO = @ACTIVO
                              WHERE COD_MOTIVO = @COD_MOTIVO";
                connection.Execute(query, new
                {
                    COD_MOTIVO = motivoIngreso.Cod_Motivo.ToUpper(),
                    Descripcion = motivoIngreso.Descripcion,
                    ACTIVO = motivoIngreso.Activo
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Motivo de Ingreso : {motivoIngreso.Cod_Motivo} - {motivoIngreso.Descripcion}",
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
        /// Elimina un motivo de ingreso por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="codMotivo">Código del motivo de ingreso a eliminar</param>
        /// <returns></returns>
        public ErrorDTO AF_MotivosIngreso_Eliminar(int CodEmpresa, string usuario, string codMotivo)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM AFI_MOTIVOS_INGRESOS WHERE COD_MOTIVO = @COD_MOTIVO";
                connection.Execute(query, new { COD_MOTIVO = codMotivo.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Motivo de Ingreso : {codMotivo}",
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

        /// <summary>
        /// Valida si un motivo de ingreso ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="codMotivo">Código del motivo de ingreso a validar</param>
        /// <returns></returns>
        public ErrorDTO AF_MotivosIngreso_Valida(int CodEmpresa, string codMotivo)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_MOTIVOS_INGRESOS WHERE COD_MOTIVO = @COD_MOTIVO";
                var existe = connection.QueryFirstOrDefault<int>(query, new { COD_MOTIVO = codMotivo.ToUpper() });
                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "El motivo de ingreso ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El motivo de ingreso es válido.";
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
        /// Obtiene la lista de motivos de ingreso sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns></returns>
        public ErrorDTO<List<MotivoIngresoData>> AF_MotivosIngreso_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<List<MotivoIngresoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<MotivoIngresoData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( COD_MOTIVO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                var query = $@"select COD_MOTIVO, descripcion, ACTIVO, Registro_Fecha, Registro_Usuario
                               from AFI_MOTIVOS_INGRESOS
                               {where}
                               order by COD_MOTIVO";

                result.Result = connection.Query<MotivoIngresoData>(query).ToList();
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
