using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntxRolesAccesoDB
    {

        private readonly PortalDB _portalDb;

        public FrmCntxRolesAccesoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de roles de acceso por contabilidad usando el SP spCntX_AC_Rol_List.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de roles de acceso.</returns>
        public ErrorDto<List<CntXAcRolDto>> CntXAcRol_Lista(int codEmpresa, int codContabilidad, string usuario)
        {
            var sql = "spCntX_AC_Rol_List";
            var parameters = new { Contabilidad = codContabilidad, Usuario = usuario };
            return DbHelper.ExecuteListQuery<CntXAcRolDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Consulta el catálogo de cuentas para un rol de acceso usando el SP spCntX_AC_Cuentas_Consulta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="rol">Rol de acceso.</param>
        /// <param name="ctaInicio">Cuenta inicial.</param>
        /// <param name="ctaCorte">Cuenta final.</param>
        /// <param name="filtro">Filtro de descripción.</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de cuentas disponibles para el rol.</returns>
        public ErrorDto<List<CntXAcCuentaDto>> CntXAcCuentas_Consulta(int codEmpresa, int codContabilidad, string rol, string ctaInicio, string ctaCorte, string filtro, string usuario)
        {
            var sql = "spCntX_AC_Cuentas_Consulta";
            var parameters = new
            {
                Contabilidad = codContabilidad,
                Rol = rol,
                CtaInicio = ctaInicio,
                CtaCorte = ctaCorte,
                Filtro = filtro,
                Usuario = usuario
            };
            return DbHelper.ExecuteListQuery<CntXAcCuentaDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Consulta las cuentas asignadas a un rol de acceso usando el SP spCntX_AC_Cuentas_Consulta_Asignadas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="rol">Rol de acceso.</param>
        /// <param name="filtro">Filtro de descripción.</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de cuentas asignadas al rol.</returns>
        public ErrorDto<List<CntXAcCuentaDto>> CntXAcCuentas_Consulta_Asignadas(int codEmpresa, int codContabilidad, string rol, string filtro, string usuario)
        {
            var sql = "spCntX_AC_Cuentas_Consulta_Asignadas";
            var parameters = new
            {
                Contabilidad = codContabilidad,
                Rol = rol,
                Filtro = filtro,
                Usuario = usuario
            };
            return DbHelper.ExecuteListQuery<CntXAcCuentaDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Consulta las unidades de negocio disponibles para el rol usando el SP spCntX_AC_Unidades_Consulta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="rol">Rol de acceso.</param>
        /// <param name="filtro">Filtro de descripción (opcional).</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de unidades de negocio.</returns>
        public ErrorDto<List<CntXAcUnidadDto>> CntXAcUnidades_Consulta(int codEmpresa, int codContabilidad, string rol, string filtro, string usuario)
        {
            var sql = "spCntX_AC_Unidades_Consulta";
            var parameters = new
            {
                Contabilidad = codContabilidad,
                Rol = rol,
                Filtro = filtro ?? "",
                Usuario = usuario
            };
            return DbHelper.ExecuteListQuery<CntXAcUnidadDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Consulta los centros de costo de la unidad de negocio disponibles para el rol usando el SP spCntX_AC_Centro_Costo_Consulta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="rol">Rol de acceso.</param>
        /// <param name="unidad">Unidad de negocio.</param>
        /// <param name="filtro">Filtro de descripción (opcional).</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de centros de costo.</returns>
        public ErrorDto<List<CntXAcCentroCostoDto>> CntXAcCentroCosto_Consulta(int codEmpresa, int codContabilidad, string rol, string unidad, string filtro, string usuario)
        {
            var sql = "spCntX_AC_Centro_Costo_Consulta";
            var parameters = new
            {
                Contabilidad = codContabilidad,
                Rol = rol,
                Unidad = unidad,
                Filtro = filtro ?? "",
                Usuario = usuario
            };
            return DbHelper.ExecuteListQuery<CntXAcCentroCostoDto>(_portalDb, codEmpresa, sql, parameters);
        }
    }
}
