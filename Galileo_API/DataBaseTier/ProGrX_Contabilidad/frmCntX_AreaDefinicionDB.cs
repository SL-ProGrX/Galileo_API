using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXAreaDefinicionDB
    {
        private readonly PortalDB _portalDb;

        public FrmCntXAreaDefinicionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
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
        public ErrorDto<ExisteDto> AreaCuenta_Existe(int codEmpresa, int codigoConta, string cuentaNodo, int areaActual)
        {
            var sql = @"select isnull(count(*), 0) as Existe from CntX_Area_Cuentas where cod_contabilidad = @codigoConta and cod_cuenta = @cuentaNodo and cod_area = @areaActual";
            var parametros = new { codigoConta, cuentaNodo, areaActual };
            return DbHelper.ExecuteSingleQuery<ExisteDto>(_portalDb, codEmpresa, sql, default, parametros);
        }
    }
}
