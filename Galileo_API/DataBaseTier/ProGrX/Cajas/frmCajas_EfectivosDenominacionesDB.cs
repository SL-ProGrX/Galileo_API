using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasEfectivosDenominacionesDB
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 5;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCajasEfectivosDenominacionesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
            _Security_MainDB = new MSecurityMainDb(config);
        }
        /// <summary>
        /// Obtiene la lista de denominaciones de efectivo por divisa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_divisa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasEfectivosDenominacionesData>> Cajas_EfectivosDenominaciones_Obtener(int CodEmpresa,string cod_divisa,FiltrosLazyLoadData filtros)
        {
            var result = DbHelper.CreateOkResponse(new List<CajasEfectivosDenominacionesData>());

            try
            {
                filtros ??= new FiltrosLazyLoadData();
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var parameters = new DynamicParameters();
                parameters.Add("@cod_divisa", cod_divisa);

                var where = " WHERE cod_divisa = @cod_divisa ";

                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    where += @"
                AND (
                       CAST(Denominacion AS varchar(50)) LIKE @filtro
                    OR descripcion LIKE @filtro
                    OR Tipo LIKE @filtro
                )";

                    parameters.Add("@filtro", $"%{filtros.filtro}%");
                }

                var allowedSortFields = new[] { "Denominacion", "descripcion", "Tipo" };

                var sortField = allowedSortFields
                    .FirstOrDefault(f => f.Equals(filtros.sortField, StringComparison.OrdinalIgnoreCase))
                    ?? "Denominacion";

                var sortDirection = filtros.sortOrder == 0 ? "DESC" : "ASC";

                var query = $@"
            SELECT 
                  cod_divisa
                , Denominacion  AS denominacion
                , Tipo          AS tipo
                , descripcion
                , Activa        AS activa
                , Registro_Usuario AS registro_usuario
                , Registro_Fecha   AS registro_fecha
            FROM CAJAS_EFECTIVO_DENOMINACIONES
            {where}
            ORDER BY {sortField} {sortDirection};";

                result.Result = connection
                    .Query<CajasEfectivosDenominacionesData>(query, parameters)
                    .ToList();
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
        /// Inserta o actualiza una denominación de efectivo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="denominacion"></param>
        /// <returns></returns>
        public ErrorDto Cajas_EfectivosDenominaciones_Guardar(int CodEmpresa, string usuario, CajasEfectivosDenominacionesData denominacion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                // Validación básica
                if (denominacion.denominacion <= 0)
                {
                    resp.Code = -2;
                    resp.Description = "La denominación debe ser mayor a cero.";
                    return resp;
                }

                if (string.IsNullOrWhiteSpace(denominacion.cod_divisa))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar la divisa.";
                    return resp;
                }
                var qExiste = @"
            SELECT ISNULL(COUNT(*),0) 
            FROM CAJAS_EFECTIVO_DENOMINACIONES
            WHERE cod_divisa = @cod_divisa
              AND Denominacion = @denominacion";

                var existe = connection.QueryFirstOrDefault<int>(qExiste, new
                {
                    cod_divisa = denominacion.cod_divisa,
                    denominacion = denominacion.denominacion
                });

                if (denominacion.isNew)
                {
                    if (existe > 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"La denominación {denominacion.denominacion} ya existe para la divisa {denominacion.cod_divisa}.";
                        return resp;
                    }
                    resp = Cajas_EfectivosDenominaciones_Insertar(CodEmpresa, usuario, denominacion);
                }
                else
                {
                    if (existe == 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"La denominación {denominacion.denominacion} no existe para la divisa {denominacion.cod_divisa}.";
                        return resp;
                    }

                    // Igual que Ubicaciones: el actualizar abre su propia conexión
                    resp = Cajas_EfectivosDenominaciones_Actualizar(CodEmpresa, usuario, denominacion);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }


        /// <summary>
        /// Inserta una nueva denominación de efectivo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="denominacion"></param>
        /// <returns>.</returns>
        private ErrorDto Cajas_EfectivosDenominaciones_Insertar(int CodEmpresa, string usuario, CajasEfectivosDenominacionesData denominacion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                var q = @"
            INSERT INTO CAJAS_EFECTIVO_DENOMINACIONES
                (cod_divisa, Denominacion, Tipo, descripcion, Activa, Registro_Usuario, Registro_Fecha)
            VALUES
                (@cod_divisa, @denominacion, @tipo, @descripcion, @activa, @registro_usuario, dbo.MyGetdate())";

                connection.Execute(q, new
                {
                    cod_divisa = denominacion.cod_divisa,
                    denominacion = denominacion.denominacion,
                    tipo = (denominacion.tipo ?? "B").ToUpper().Substring(0, 1),
                    descripcion = (denominacion.descripcion ?? string.Empty).ToUpper(),
                    activa = denominacion.activa ? 1 : 0,
                    registro_usuario = usuario ?? string.Empty
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? string.Empty,
                    DetalleMovimiento = $"Denominación del Efectivo: {denominacion.denominacion} Divisa: {denominacion.cod_divisa}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }


        /// <summary>
        /// Actualiza una denominación de efectivo existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="denominacion"></param>
        /// <returns></returns>
        private ErrorDto Cajas_EfectivosDenominaciones_Actualizar(int CodEmpresa, string usuario, CajasEfectivosDenominacionesData denominacion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                var q = @"
            UPDATE CAJAS_EFECTIVO_DENOMINACIONES
            SET descripcion = @descripcion,
                Tipo = @tipo,
                Activa = @activa
            WHERE cod_divisa = @cod_divisa
              AND Denominacion = @denominacion";

                connection.Execute(q, new
                {
                    cod_divisa = denominacion.cod_divisa,
                    denominacion = denominacion.denominacion,
                    tipo = (denominacion.tipo ?? "B").ToUpper().Substring(0, 1),
                    descripcion = (denominacion.descripcion ?? string.Empty).ToUpper(),
                    activa = denominacion.activa ? 1 : 0
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? string.Empty,
                    DetalleMovimiento = $"Denominación del Efectivo: {denominacion.denominacion} Divisa: {denominacion.cod_divisa}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }


        /// <summary>
        /// Elimina una denominación de efectivo por divisa y monto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_divisa"></param>
        /// <param name="denominacion"></param>
        /// <returns></returns>
        public ErrorDto Cajas_EfectivosDenominaciones_Eliminar(int CodEmpresa, string usuario, string cod_divisa, decimal denominacion)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                var q = @"
                    DELETE FROM CAJAS_EFECTIVO_DENOMINACIONES
                    WHERE cod_divisa = @cod_divisa
                      AND Denominacion = @denominacion";

                var rows = cn.Execute(q, new { cod_divisa, denominacion });

                if (rows == 0)
                {
                    resp.Code = -2;
                    resp.Description = $"La denominación {denominacion} para la divisa {cod_divisa} no existe.";
                    return resp;
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? string.Empty,
                    DetalleMovimiento = $"Denominación del Efectivo: {denominacion} Divisa: {cod_divisa}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }
        /// <summary>
        /// Devuelve una lista con las divisas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_EfectivosDenominaciones_Divisas_Obtener(int CodEmpresa,int codContabilidad)
        {
            const string sql = @"
            SELECT 
                  RTRIM(cod_divisa)    AS item
                , RTRIM(descripcion)  AS descripcion
            FROM CntX_Divisas
            WHERE COD_CONTABILIDAD = @contabilidad
            ORDER BY divisa_local, cod_divisa";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql,
                new { contabilidad = codContabilidad });
        }
    }
}