using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysEducacionDB
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 10; // Modulo de Tesorería
        private readonly MSecurityMainDb _Security_MainDB;


        public FrmSysEducacionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Consulta la lista de centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysEducacionLista> Sys_EducacionlLista_Obtener(int CodEmpresa, string tipo, FiltrosLazyLoadData filtros)
        {
            var db = DbHelper.WithConn(_portalDb, Emp(CodEmpresa), connection =>
            {
                var search = filtros?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 0; // 0=DESC, 1=ASC

                var offset = filtros?.pagina ?? 0;
                var fetch = filtros?.paginacion ?? 0;
                if (fetch <= 0)
                    fetch = int.MaxValue;

                const string sql = @"
                    SELECT cod_Educ, descripcion, Activa, 0 as btn
                    FROM SYS_EDUCACION_CFG
                    WHERE Tipo = @tipo
                      AND (@search IS NULL
                           OR cod_Educ LIKE @search
                           OR descripcion LIKE @search)
                    ORDER BY
                        -- ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_educ' THEN cod_Educ END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'descripcion' THEN descripcion END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'activa' THEN CONVERT(int, Activa) END ASC,

                        -- DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_educ' THEN cod_Educ END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'descripcion' THEN descripcion END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'activa' THEN CONVERT(int, Activa) END DESC,

                        -- Fallback
                        cod_Educ ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                var lista = connection.Query<SysEducacionData>(sql, new
                {
                    tipo,
                    search = searchLike,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();

                return new SysEducacionLista
                {
                    total = lista.Count,
                    lista = lista
                };
            });

            if (db.Code != 0)
            {
                db.Result = new SysEducacionLista
                {
                    total = 0,
                    lista = new List<SysEducacionData>()
                };
            }

            return db;
        }


        /// <summary>
        /// Actualiza o inserta datos de centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto Sys_Educacion_Guardar(int CodEmpresa, string usuario, SysEducacionData datos)
        {
            var result = DbHelper.CreateOkResponse();

            try
            {
                //verifico si existe dato (parametrizado)
                const string query = @"select isnull(count(*),0) as Existe
                                       from SYS_EDUCACION_CFG
                                       where cod_Educ = @cod_educ and Tipo = @tipo";

                var existeDb = DbHelper.ExecuteSingleQuery<int>(
                    _portalDb,
                    Emp(CodEmpresa),
                    query,
                    0,
                    new { cod_educ = datos.cod_educ, tipo = datos.tipo });

                if (existeDb.Code != 0)
                    return DbHelper.ErrorResponse(existeDb.Description ?? "Error al consultar existencia.", existeDb.Code ?? -1);

                var existe = existeDb.Result;

                if (datos.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code = -2;
                        result.Description = $"El código no puede ser utilizado! Ya existe un item diferente con su uso!";
                    }
                    else
                    {
                        result = Sys_Educacion_Insertar(CodEmpresa, usuario, datos);
                    }
                }
                else if (existe == 0)
                {
                    result.Code = -2;
                    result.Description = $"El código {datos.cod_educ} no existe.";
                }
                else
                {
                    result = Sys_Educacion_Actualizar(CodEmpresa, usuario, datos);
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
        /// Actualiza centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dato"></param>
        /// <returns></returns>
        private ErrorDto Sys_Educacion_Actualizar(int CodEmpresa, string usuario, SysEducacionData dato)
        {
            var query = @"UPDATE SYS_EDUCACION_CFG
                                    SET descripcion = @descripcion,
                                        Activa = @estado
                                    WHERE  cod_Educ = @cod_dato";

            var db = DbHelper.ExecuteNonQuery(_portalDb, Emp(CodEmpresa), query, new
            {
                cod_dato = dato.cod_educ,
                descripcion = dato.descripcion,
                estado = dato.activa
            });

            if (db.Code == 0)
            {
                LogEducacionBitacora(CodEmpresa, usuario, dato.cod_educ, dato.descripcion, "Modifica - WEB");
            }

            return db;
        }


        /// <summary>
        /// Inserta centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dato"></param>
        /// <returns></returns>
        private ErrorDto Sys_Educacion_Insertar(int CodEmpresa, string usuario, SysEducacionData dato)
        {
            var query = @"INSERT SYS_EDUCACION_CFG(cod_Educ, Tipo, descripcion, Activa, Registro_Usuario, Registro_Fecha) 
                                    VALUES (@cod_dato,@tipo, @descripcion, @estado, @usuario, dbo.MyGetdate() )";

            var db = DbHelper.ExecuteNonQuery(_portalDb, Emp(CodEmpresa), query, new
            {
                cod_dato = dato.cod_educ,
                tipo = dato.tipo,
                descripcion = dato.descripcion,
                estado = dato.activa,
                usuario = usuario
            });

            if (db.Code == 0)
            {
                LogEducacionBitacora(CodEmpresa, usuario, dato.cod_educ, dato.descripcion, "Registra - WEB");
            }

            return db;
        }


        /// <summary>
        /// Elimina centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_Educ"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto Sys_Educacion_Eliminar(int CodEmpresa, string usuario, string cod_Educ, string tipo)
        {
            var query = @"DELETE SYS_EDUCACION_CFG where cod_Educ =  @cod_Educ and Tipo = @tipo";

            var db = DbHelper.ExecuteNonQuery(_portalDb, Emp(CodEmpresa), query, new { cod_Educ = cod_Educ, tipo = tipo });

            if (db.Code == 0)
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = Emp(CodEmpresa),
                    Usuario = usuario,
                    DetalleMovimiento = $"Educacion Doc. : {cod_Educ}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }

            return db;
        }


        /// <summary>
        /// Consulta el detalle de centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDetalleEduc"></param>
        /// <param name="cod_Educ"></param>
        /// <returns></returns>
        public ErrorDto<List<SysEducacionDetalleData>> Sys_EducacionDetalle_Consulta(int CodEmpresa, string tipoDetalleEduc, string cod_Educ)
        {
            var result = DbHelper.WithConn(_portalDb, Emp(CodEmpresa), connection =>
            {
                const string sp = "spSys_Educacion_Asigna_Consulta";
                return connection.Query<SysEducacionDetalleData>(sp, new
                {
                    cod_Educ = (cod_Educ ?? string.Empty).Trim(),
                    tipoDetalleEduc
                }, commandType: CommandType.StoredProcedure).ToList();
            });

            if (result.Code != 0)
                return new ErrorDto<List<SysEducacionDetalleData>>
                {
                    Code = -1,
                    Description = result.Description,
                    Result = new List<SysEducacionDetalleData>()
                };

            return new ErrorDto<List<SysEducacionDetalleData>>
            {
                Code = 0,
                Description = "Ok",
                Result = result.Result ?? new List<SysEducacionDetalleData>()
            };
        }


        /// <summary>
        /// Asigna o des asigna detalle de centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_Educ"></param>
        /// <param name="cod_DetalleEduc"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public ErrorDto Sys_EducacionDetalle_Asignar(int CodEmpresa, string usuario, string cod_Educ, string cod_DetalleEduc, bool accion)
        {
            string check = accion ? "A" : "E";

            var db = DbHelper.WithConn(_portalDb, Emp(CodEmpresa), connection =>
            {
                const string query = @"exec spSys_Educacion_Asigna @cod_Educ,@cod_DetalleEduc,@usuario,@check";
                connection.Execute(query, new
                {
                    cod_Educ = cod_Educ.Trim(),
                    cod_DetalleEduc = cod_DetalleEduc,
                    usuario = usuario,
                    check = check
                });
                return 1;
            });

            return db.Code == 0 ? DbHelper.CreateOkResponse() : DbHelper.ErrorResponse(db.Description ?? "Error al asignar detalle de educación.", db.Code ?? -1);
        }

        private static int Emp(int? codEmpresa) => codEmpresa ?? 0;

        private void LogEducacionBitacora(int? CodEmpresa, string usuario, string? codEduc, string? descripcion, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = Emp(CodEmpresa),
                Usuario = usuario,
                DetalleMovimiento = $"Educacion Doc. : {codEduc} - {descripcion}",
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}