using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasTiposCambiosDB
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 5;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCajasTiposCambiosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
            _Security_MainDB = new MSecurityMainDb(config);
        }
        /// <summary>
        /// Lista de tipos de cambios de una contabilidad.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="cod_divisa"></param>"
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<CajasTiposCambiosData>> Cajas_TiposCambios_Obtener(int CodEmpresa,int codContabilidad,string cod_divisa,FiltrosLazyLoadData filtros)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var result = new ErrorDto<List<CajasTiposCambiosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasTiposCambiosData>()
            };

            try
            {
                if (filtros == null)
                    filtros = new FiltrosLazyLoadData();

                var parameters = new DynamicParameters();
                parameters.Add("@cod_divisa", cod_divisa);
                parameters.Add("@cod_contabilidad", codContabilidad);

                var where = @"
                    WHERE COD_CONTABILIDAD = @cod_contabilidad
                      AND cod_divisa       = @cod_divisa";

                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    where += @"
                      AND (
                             CAST(ID_Cambio AS varchar(50)) LIKE @filtro
                          OR CAST(TC_Compra AS varchar(50)) LIKE @filtro
                          OR CAST(TC_Venta  AS varchar(50)) LIKE @filtro
                          OR CAST(Variacion AS varchar(50)) LIKE @filtro
                          )";

                    parameters.Add("@filtro", "%" + filtros.filtro + "%");
                }
                string requestedSort = (filtros.sortField ?? string.Empty).Trim().ToUpperInvariant();
                string sortField;

                if (requestedSort == "ID_CAMBIO")
                    sortField = "ID_Cambio";
                else if (requestedSort == "TC_COMPRA")
                    sortField = "TC_Compra";
                else if (requestedSort == "TC_VENTA")
                    sortField = "TC_Venta";
                else if (requestedSort == "INICIO")
                    sortField = "Inicio";
                else if (requestedSort == "CORTE")
                    sortField = "Corte";
                else if (requestedSort == "VARIACION")
                    sortField = "Variacion";
                else
                    sortField = "ID_Cambio";

                string sortDirection = filtros.sortOrder == 0 ? "DESC" : "ASC";

                string query = @"
                SELECT
                      ID_Cambio         AS id_cambio,
                      COD_CONTABILIDAD  AS cod_contabilidad,
                      RTRIM(cod_divisa) AS cod_divisa,
                      TC_Compra         AS tc_compra,
                      TC_Venta          AS tc_venta,
                      Inicio            AS inicio,
                      Corte             AS corte,
                      Variacion         AS variacion,
                      RTRIM(Usuario)    AS usuario,
                      Fecha             AS fecha
                FROM CAJAS_DIVISAS_TIPO_CAMBIO
                " + where + @"
                ORDER BY " + sortField + " " + sortDirection + ";";

                var enumerable = cn.Query<CajasTiposCambiosData>(query, parameters);
                var lista = new List<CajasTiposCambiosData>();
                foreach (var item in enumerable)
                {
                    lista.Add(item);
                }

                result.Result = lista;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<CajasTiposCambiosData>();
            }

            return result;
        }
        /// <summary>
        /// Inserta o actualiza un tipo de cambio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cambio"></param>
        /// <returns></returns>
        public ErrorDto Cajas_TiposCambios_Guardar(int CodEmpresa,string usuario,CajasTiposCambiosData cambio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                if (cambio.cod_contabilidad <= 0)
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar la contabilidad.";
                    return resp;
                }

                if (string.IsNullOrWhiteSpace(cambio.cod_divisa))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar la divisa.";
                    return resp;
                }

                if (cambio.tc_compra <= 0 || cambio.tc_venta <= 0)
                {
                    resp.Code = -2;
                    resp.Description = "Los tipos de cambio deben ser mayores a cero.";
                    return resp;
                }

                const string qExiste = @"
                SELECT ISNULL(COUNT(*),0)
                FROM CAJAS_DIVISAS_TIPO_CAMBIO
                WHERE COD_CONTABILIDAD = @cod_contabilidad
                  AND cod_divisa       = @cod_divisa
                  AND ID_Cambio        = @id_cambio;";

                int existe = cn.ExecuteScalar<int>(qExiste, new
                {
                    cod_contabilidad = cambio.cod_contabilidad,
                    cod_divisa = cambio.cod_divisa,
                    id_cambio = cambio.id_cambio
                });

                if (cambio.isNew)
                {
                    if (existe > 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"El ID de cambio {cambio.id_cambio} ya existe para la divisa {cambio.cod_divisa}.";
                        return resp;
                    }

                    resp = Cajas_TiposCambios_Insertar(CodEmpresa, usuario, cambio);
                }
                else
                {
                    if (existe == 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"El ID de cambio {cambio.id_cambio} no existe para la divisa {cambio.cod_divisa}.";
                        return resp;
                    }

                    resp = Cajas_TiposCambios_Actualizar(CodEmpresa, usuario, cambio);
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
        /// Inserta un nuevo tipo de cambio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cambio"></param>
        /// <returns>.</returns>
        private ErrorDto Cajas_TiposCambios_Insertar(int CodEmpresa,string usuario,CajasTiposCambiosData cambio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                if (cambio.id_cambio <= 0)
                {
                    const string qNextId = @"
                    SELECT ISNULL(MAX(ID_Cambio),0) + 1
                    FROM CAJAS_DIVISAS_TIPO_CAMBIO
                    WHERE COD_CONTABILIDAD = @cod_contabilidad
                      AND cod_divisa       = @cod_divisa;";

                    cambio.id_cambio = cn.ExecuteScalar<int>(qNextId, new
                    {
                        cod_contabilidad = cambio.cod_contabilidad,
                        cod_divisa = cambio.cod_divisa
                    });
                }

                const string qInsert = @"
                INSERT INTO CAJAS_DIVISAS_TIPO_CAMBIO
                    (ID_Cambio, COD_CONTABILIDAD, cod_divisa, Usuario, Fecha,
                     TC_Compra, TC_Venta, Inicio, Corte, Variacion)
                VALUES
                    (@id_cambio, @cod_contabilidad, @cod_divisa, @usuario, dbo.MyGetdate(),
                     @tc_compra, @tc_venta, @inicio, @corte, @variacion);";

                cn.Execute(qInsert, new
                {
                    id_cambio = cambio.id_cambio,
                    cod_contabilidad = cambio.cod_contabilidad,
                    cod_divisa = cambio.cod_divisa,
                    usuario = usuario ?? string.Empty,
                    tc_compra = cambio.tc_compra,
                    tc_venta = cambio.tc_venta,
                    inicio = cambio.inicio ?? DateTime.Today,
                    corte = cambio.corte ?? DateTime.Today,
                    variacion = cambio.variacion
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? string.Empty,
                    Modulo = vModulo,
                    Movimiento = "Registra - WEB",
                    DetalleMovimiento = $"Tipo Cambio ID: {cambio.id_cambio} Divisa: {cambio.cod_divisa} Conta: {cambio.cod_contabilidad}"
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
        /// Actualiza un tipo de cambio existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cambio"></param>
        /// <returns></returns>
        private ErrorDto Cajas_TiposCambios_Actualizar(int CodEmpresa,string usuario,CajasTiposCambiosData cambio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                const string qUpdate = @"
                UPDATE CAJAS_DIVISAS_TIPO_CAMBIO
                SET TC_Compra = @tc_compra,
                    TC_Venta  = @tc_venta,
                    Inicio    = @inicio,
                    Corte     = @corte,
                    Variacion = @variacion
                WHERE COD_CONTABILIDAD = @cod_contabilidad
                  AND cod_divisa       = @cod_divisa
                  AND ID_Cambio        = @id_cambio;";

                cn.Execute(qUpdate, new
                {
                    tc_compra = cambio.tc_compra,
                    tc_venta = cambio.tc_venta,
                    inicio = cambio.inicio ?? DateTime.Today,
                    corte = cambio.corte ?? DateTime.Today,
                    variacion = cambio.variacion,
                    cod_contabilidad = cambio.cod_contabilidad,
                    cod_divisa = cambio.cod_divisa,
                    id_cambio = cambio.id_cambio
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? string.Empty,
                    Modulo = vModulo,
                    Movimiento = "Modifica - WEB",
                    DetalleMovimiento = $"Tipo Cambio ID: {cambio.id_cambio} Divisa: {cambio.cod_divisa} Conta: {cambio.cod_contabilidad}"
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
        /// <param name="codContabilidad"></param>
        /// <param name="id_cambio"></param>
        /// <param name="cod_divisa"></param>
        /// <returns></returns>
        public ErrorDto Cajas_TiposCambios_Eliminar(int CodEmpresa,string usuario,int codContabilidad,string cod_divisa,int id_cambio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                const string qDelete = @"
                    DELETE FROM CAJAS_DIVISAS_TIPO_CAMBIO
                    WHERE COD_CONTABILIDAD = @cod_contabilidad
                      AND cod_divisa       = @cod_divisa
                      AND ID_Cambio        = @id_cambio;";

                int rows = cn.Execute(qDelete, new
                {
                    cod_contabilidad = codContabilidad,
                    cod_divisa,
                    id_cambio
                });

                if (rows == 0)
                {
                    resp.Code = -2;
                    resp.Description = $"El tipo de cambio ID {id_cambio} para la divisa {cod_divisa} no existe.";
                    return resp;
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? string.Empty,
                    Modulo = vModulo,
                    Movimiento = "Elimina - WEB",
                    DetalleMovimiento = $"Tipo Cambio ID: {id_cambio} Divisa: {cod_divisa} Conta: {codContabilidad}"
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
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TiposCambios_Divisas_Obtener(int CodEmpresa,int codContabilidad)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                const string q = @"
                SELECT
                      RTRIM(cod_divisa)  AS item,
                      RTRIM(descripcion) AS descripcion
                FROM CntX_Divisas
                WHERE COD_CONTABILIDAD = @contabilidad
                  AND divisa_local      = 0
                ORDER BY cod_divisa;";

                var enumerable = cn.Query<DropDownListaGenericaModel>(
                    q,
                    new { contabilidad = codContabilidad });

                var lista = new List<DropDownListaGenericaModel>();
                foreach (var item in enumerable)
                {
                    lista.Add(item);
                }

                result.Result = lista;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<DropDownListaGenericaModel>();
            }

            return result;
        }
    }
}
