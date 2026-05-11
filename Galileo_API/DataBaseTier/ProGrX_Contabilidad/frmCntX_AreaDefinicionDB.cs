using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXAreaDefinicionDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly int vModulo = 20; // Módulo de Contabilidad

        public FrmCntXAreaDefinicionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mSecurityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de áreas de definición para una contabilidad, ordenada por el campo especificado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <param name="codigoConta">Código de contabilidad.</param>
        /// <param name="order">Campo de ordenamiento: 'cod_area' o 'descripcion'.</param>
        /// <returns>Lista de áreas de definición.</returns>
        public ErrorDto<List<AreaDefinicionDto>> AreaDefinicion_Lista(int codEmpresa, int codigoConta, string order)
        {
            var orderBy = (order?.ToLower() == "descripcion") ? "descripcion" : "cod_area";
            var sql = $@"select cod_area, descripcion from CntX_Area_Definicion where cod_contabilidad = @codigoConta order by {orderBy}";
            return DbHelper.ExecuteListQuery<AreaDefinicionDto>(_portalDb, codEmpresa, sql, new { codigoConta });
        }

        /// <summary>
        /// Obtiene la lista de tipos de cuentas para una contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <param name="codigoConta">Código de contabilidad.</param>
        /// <returns>Lista de tipos de cuentas.</returns>
        public ErrorDto<List<TipoCuentaDto>> TiposCuentas_Lista(int codEmpresa, int codigoConta)
        {
            var sql = @"select tipo_cuenta, Descripcion from CntX_Tipos_Cuentas where cod_contabilidad = @codigoConta";
            return DbHelper.ExecuteListQuery<TipoCuentaDto>(_portalDb, codEmpresa, sql, new { codigoConta });
        }

        /// <summary>
        /// Obtiene la lista de cuentas según el nodo y tipo/cuenta actual.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <param name="codigoConta">Código de contabilidad.</param>
        /// <param name="tipoCuenta">Tipo de cuenta seleccionado.</param>
        /// <param name="cuentaActual">Cuenta madre actual.</param>
        /// <param name="nodo">Nodo actual ('T' para raíz).</param>
        /// <returns>Lista de cuentas.</returns>
        public ErrorDto<List<CuentaNodoDto>> Cuentas_ListaNodo(int codEmpresa, int codigoConta, string tipoCuenta, string cuentaActual, string nodo)
        {
            string sql;
            object parametros;
            if (nodo == "T")
            {
                sql = @"select cod_cuenta, descripcion, acepta_movimientos from CntX_Cuentas where cuenta_madre = '' and cod_contabilidad = @codigoConta and tipo_cuenta = @tipoCuenta";
                parametros = new { codigoConta, tipoCuenta };
            }
            else
            {
                sql = @"select cod_cuenta, descripcion, acepta_movimientos from CntX_Cuentas where cuenta_madre = @cuentaActual and cod_contabilidad = @codigoConta";
                parametros = new { cuentaActual, codigoConta };
            }
            return DbHelper.ExecuteListQuery<CuentaNodoDto>(_portalDb, codEmpresa, sql, parametros);
        }

        /// <summary>
        /// Valida si existe la relación área-cuenta en CntX_Area_Cuentas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <param name="codigoConta">Código de contabilidad.</param>
        /// <param name="cuentaNodo">Cuenta del nodo.</param>
        /// <param name="areaActual">Área actual.</param>
        /// <returns>DTO con el campo Existe.</returns>
        public ErrorDto<ExisteDto?> AreaCuenta_Existe(int codEmpresa, int codigoConta, string cuentaNodo, int areaActual)
        {
            var sql = @"select isnull(count(*), 0) as Existe from CntX_Area_Cuentas where cod_contabilidad = @codigoConta and cod_cuenta = @cuentaNodo and cod_area = @areaActual";
            var parametros = new { codigoConta, cuentaNodo, areaActual };
            return DbHelper.ExecuteSingleQuery<ExisteDto>(_portalDb, codEmpresa, sql, default, parametros);
        }

        /// <summary>
        /// Elimina registros de área en CntX_Area_Cuentas y CntX_Area_Reportes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <param name="codigoConta">Código de contabilidad.</param>
        /// <param name="areaActual">Área actual.</param>
        /// <returns>True si se eliminó algún registro.</returns>
        public ErrorDto<bool> Area_Eliminar(int codEmpresa, int codigoConta, int areaActual)
        {
            var sqlCuentas = @"delete CntX_Area_Cuentas where cod_contabilidad = @codigoConta and cod_area = @areaActual";
            var sqlReportes = @"delete CntX_Area_Reportes where cod_contabilidad = @codigoConta and cod_area = @areaActual";
            var parametros = new { codigoConta, areaActual };
            var rowsCuentas = DbHelper.ExecuteNonQueryWithResult(_portalDb, codEmpresa, sqlCuentas, parametros).Result;
            var rowsReportes = DbHelper.ExecuteNonQueryWithResult(_portalDb, codEmpresa, sqlReportes, parametros).Result;
            bool ok = rowsCuentas > 0 || rowsReportes > 0;
            return new ErrorDto<bool> { Result = ok, Code = ok ? 0 : -2, Description = ok ? "Ok" : "No se eliminó ningún registro" };
        }

        /// <summary>
        /// Inserta un registro en CntX_Area_Cuentas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <param name="codigoConta">Código de contabilidad.</param>
        /// <param name="areaActual">Área actual.</param>
        /// <param name="cuentaMarcada">Cuenta marcada.</param>
        /// <returns>True si se insertó el registro.</returns>
        public ErrorDto<bool> AreaCuenta_Insertar(int codEmpresa, int codigoConta, int areaActual, string cuentaMarcada)
        {
            var sql = @"insert CntX_Area_Cuentas (cod_contabilidad, cod_area, cod_cuenta) values (@codigoConta, @areaActual, @cuentaMarcada)";
            var parametros = new { codigoConta, areaActual, cuentaMarcada };
            var rows = DbHelper.ExecuteNonQueryWithResult(_portalDb, codEmpresa, sql, parametros).Result;
            bool ok = rows > 0;
            return new ErrorDto<bool> { Result = ok, Code = ok ? 0 : -2, Description = ok ? "Ok" : "No se insertó el registro" };
        }

        /// <summary>
        /// Obtiene el detalle de cuentas asociadas a un área.
        /// </summary>
        public ErrorDto<List<AreaCuentaDetalleDto>> AreaCuenta_DetalleLista(int codEmpresa, int codigoConta, int areaActual)
        {
            var sql = @"select
  A.cod_cuenta,
  C.acepta_movimientos,
  C.cuenta_madre,
  C.nivel
from CntX_Area_Cuentas A
inner join CntX_Cuentas C
  on A.cod_cuenta = C.cod_cuenta
 and A.cod_contabilidad = C.cod_contabilidad
where A.cod_area = @areaActual
  and A.cod_contabilidad = @codigoConta
order by A.cod_cuenta desc";
            var parametros = new { areaActual, codigoConta };
            return DbHelper.ExecuteListQuery<AreaCuentaDetalleDto>(_portalDb, codEmpresa, sql, parametros);
        }

        /// <summary>
        /// Valida si existe una relación área-cuenta específica.
        /// </summary>
        public ErrorDto<ExisteDto?> AreaCuenta_ExistePorCuenta(int codEmpresa, int codigoConta, string codCuenta, int areaActual)
        {
            var sql = @"select isnull(count(*), 0) as Existe from CntX_Area_Cuentas where cod_contabilidad = @codigoConta and cod_cuenta = @codCuenta and cod_area = @areaActual";
            var parametros = new { codigoConta, codCuenta, areaActual };
            return DbHelper.ExecuteSingleQuery<ExisteDto>(_portalDb, codEmpresa, sql, default, parametros);
        }

        /// <summary>
        /// Inserta una relación área-cuenta para una cuenta madre.
        /// </summary>
        public ErrorDto<bool> AreaCuenta_InsertarMadre(int codEmpresa, int codigoConta, int areaActual, string cuentaMadre)
        {
            var sql = @"insert CntX_Area_Cuentas(cod_contabilidad,cod_area,cod_cuenta) values(@codigoConta, @areaActual, @cuentaMadre)";
            var parametros = new { codigoConta, areaActual, cuentaMadre };
            var rows = DbHelper.ExecuteNonQueryWithResult(_portalDb, codEmpresa, sql, parametros).Result;
            bool ok = rows > 0;
            return new ErrorDto<bool> { Result = ok, Code = ok ? 0 : -2, Description = ok ? "Ok" : "No se insertó el registro" };
        }

        /// <summary>
        /// Inserta un área de definición y registra en bitácora (módulo 20).
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <param name="codigoConta">Código de contabilidad.</param>
        /// <param name="nombreArea">Nombre del área (se guarda en mayúscula).</param>
        /// <param name="chkActiva">Valor de activa.</param>
        /// <param name="usuario">Usuario que realiza la acción.</param>
        /// <returns>Nuevo id de área.</returns>
        public ErrorDto<int> AreaDefinicion_Insertar(int codEmpresa, int codigoConta, string nombreArea, bool chkActiva, string usuario)
        {
            var sqlUltimo = "select isnull(max(cod_area), 0) + 1 as nuevo from CntX_Area_Definicion where cod_contabilidad = @codigoConta";
            int nuevoId = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QuerySingle<int>(sqlUltimo, new { codigoConta })
            ).Result;
            var sqlInsert = @"insert into CntX_Area_Definicion (cod_contabilidad, cod_area, descripcion, activa) values (@codigoConta, @nuevoId, @nombreArea, @chkActiva)";
            var parametros = new { codigoConta, nuevoId, nombreArea = nombreArea.ToUpper(), chkActiva = chkActiva ? 1 : 0 };
            var rows = DbHelper.ExecuteNonQueryWithResult(_portalDb, codEmpresa, sqlInsert, parametros).Result;
            if (rows > 0)
            {
                _mSecurityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Area Trabajo: {nuevoId} Conta.{codigoConta}",
                    Movimiento = "Registra - Web",
                    Modulo = vModulo
                });
            }
            return new ErrorDto<int> { Result = nuevoId, Code = rows > 0 ? 0 : -2, Description = rows > 0 ? "Ok" : "No se insertó el área" };
        }

        /// <summary>
        /// Obtiene la lista de unidades activas para una contabilidad.
        /// </summary>
        public ErrorDto<List<UnidadDto>> Unidades_Lista(int codEmpresa, int codigoConta)
        {
            var sql = @"select cod_unidad, Descripcion from CntX_Unidades where cod_contabilidad = @codigoConta and activa = 1";
            return DbHelper.ExecuteListQuery<UnidadDto>(_portalDb, codEmpresa, sql, new { codigoConta });
        }

        /// <summary>
        /// Obtiene la lista de centros de costo asociados a una unidad para una contabilidad.
        /// </summary>
        public ErrorDto<List<CentroCostoDto>> CentroCostos_ListaPorUnidad(int codEmpresa, int codigoConta, string unidadActual)
        {
            var sql = @"select cod_centro_costo, descripcion from CntX_Centro_Costos where cod_contabilidad = @codigoConta and cod_centro_costo in (select cod_centro_costo from cntX_unidades_cc where cod_contabilidad = @codigoConta and cod_unidad = @unidadActual)";
            var parametros = new { codigoConta, unidadActual };
            return DbHelper.ExecuteListQuery<CentroCostoDto>(_portalDb, codEmpresa, sql, parametros);
        }

        /// <summary>
        /// Elimina un área de definición por contabilidad y área.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <param name="codigoConta">Código de contabilidad.</param>
        /// <param name="areaActual">Área actual.</param>
        /// <returns>True si se eliminó el registro.</returns>
        public ErrorDto<bool> AreaDefinicion_Eliminar(int codEmpresa, int codigoConta, int areaActual)
        {
            var sql = @"delete CntX_Area_Definicion where cod_contabilidad = @codigoConta and cod_area = @areaActual";
            var parametros = new { codigoConta, areaActual };
            var rows = DbHelper.ExecuteNonQueryWithResult(_portalDb, codEmpresa, sql, parametros).Result;
            bool ok = rows > 0;
            return new ErrorDto<bool> { Result = ok, Code = ok ? 0 : -2, Description = ok ? "Ok" : "No se eliminó el registro" };
        }
    }
}
